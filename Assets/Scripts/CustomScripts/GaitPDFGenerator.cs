using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LightBuzz.BodyTracking;
using Newtonsoft.Json;
using Nrjwolf.Tools;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Tables;
using UnityEngine;

public class GaitPDFGenerator : MonoBehaviour
{
    public async void GeneratePDF()
    {
        //Create a new PDF document
        PdfDocument doc = new PdfDocument();
        //Add a page
        PdfPage page = doc.Pages.Add();
        // Create a PdfLightTable
        PdfLightTable pdfLightTable = new PdfLightTable();
        
        DataTable table = new DataTable();
        
        pdfLightTable.Style.BorderPen = new PdfPen(new PdfColor(0, 0, 0), 0.5F);
        
        
        table.Columns.Add("Subject");
        table.Columns.Add("Standing at (time)");
        table.Columns.Add("Foot Strike at (time)");
        table.Columns.Add("Foot Passing at (time)");
        table.Columns.Add("Hip/Knee ABD Angle Difference");
        table.Columns.Add("Var/Val Distance");
        table.Columns.Add("Leg in Question");
        //Include rows to the DataTable
        for (int i = 0; i < ReferenceManager.instance.AngleAtFootStrikingTime.Count; i++)
        {
            var item1 = ReferenceManager.instance.standingDetectionBodies.FirstOrDefault(x=>x.angleDifferenceValue !=0 && x.distanceValue!=0&&!x.added);
            if(item1 != null)
            {
                item1.added = true;
                 table.Rows.Add(
                new string[]
                {
                    GeneralStaticManager.GlobalVar["Subject"],
                    item1.TimeofStanding,
                    "",
                    "",
                    item1.angleDifferenceValue.ToString("0.00"),
                    item1.distanceValue.ToString("0.00"),
                    item1.nameOfTheFoot,
                }
            );
            }
            table.Rows.Add(
                new string[]
                {
                    GeneralStaticManager.GlobalVar["Subject"],
                    "",
                    ReferenceManager
                        .instance.AngleAtFootStrikingTime.ElementAt(i)
                        .Key,
                        "",
                    ReferenceManager
                        .instance.AngleAtFootStrikingTime.ElementAt(i)
                        .Value.ToString("0.00") + "º",
                    ReferenceManager
                        .instance.DistanceAtFootStrikingTime.ElementAt(i)
                        .Value.ToString("0.00"),
                    ResearchMeasurementManager.instance.leftLeg ? "Left Leg":"Right Leg",    
                }
            );
            var item = ReferenceManager.instance.heelPressDetectionBodies.FirstOrDefault(x=>TimeSpan.ParseExact(x.TimeOfHeelPressed,@"mm\:ss\:fff", CultureInfo.InvariantCulture) > TimeSpan.ParseExact(ReferenceManager.instance.AngleAtFootStrikingTime.ElementAt(i).Key,@"mm\:ss\:fff", CultureInfo.InvariantCulture)&&!x.added);
            if(item == null)
            {
                continue;
            }
            item.added = true;
             table.Rows.Add(
                new string[]
                {
                    GeneralStaticManager.GlobalVar["Subject"],
                    "",
                    "",
                    item.TimeOfHeelPressed,
                    item.angleDifferenceValue.ToString("0.00"),
                    item.distanceValue.ToString("0.00"),
                    item.nameOfTheFoot,
                }
            );
           
        }
        //Applying cell padding to table
        pdfLightTable.Style.CellPadding = 3;
        pdfLightTable.ApplyBuiltinStyle(PdfLightTableBuiltinStyle.GridTable3Accent3);
        
        //Assign data source
        pdfLightTable.DataSource = table;
        
        //Setting this property to true to show the header of table
        pdfLightTable.Style.ShowHeader = true;
        
        //Draw PdfLightTable
        var result =pdfLightTable.Draw(page, new PointF(0, 0));
        
        //Save the document
        MemoryStream stream = new MemoryStream();

        doc.Save(stream);

        stream.Position = 0;
        string path = Path.Combine(Application.persistentDataPath, "Gait.pdf");

        ReferenceManager.instance.heelPressDetectionBodies.ForEach(x => x.added = false);
        File.WriteAllBytes(path, stream.ToArray());
        await Task.Delay(1000);

#if UNITY_EDITOR
        System.Diagnostics.Process.Start(path);
        Debug.Log("Is Editor");

#else

        string url = "file://" + path.Replace(" ", "%20");
        Debug.Log("URL = " + url);
        Debug.Log("Persistance = " + path);
        GeneralStaticManager.OpenFile(path);
#endif
    }

    public void UploadResults()
    {
        for (int i = 0; i < ReferenceManager.instance.AngleAtFootStrikingTime.Count; i++)
        {
            CreateGaitReportBody body = new CreateGaitReportBody()
            {
                ReportsRecordId = ReferenceManager.instance.SelectedVideoID,
                CreatedBy = GeneralStaticManager.GlobalVar["UserName"],
                Subject = GeneralStaticManager.GlobalVar["Subject"],
                FootStrikeAtTime = (float)TimeSpan.ParseExact(ReferenceManager
                    .instance.AngleAtFootStrikingTime.ElementAt(i)
                    .Key,@"mm\:ss\:fff", CultureInfo.InvariantCulture).TotalSeconds,
                MaxAngleDifference = ReferenceManager
                    .instance.maxAngleAtFootStrikingTime.ElementAt(i)
                    .Value,
                MaxmmDistance = ReferenceManager
                    .instance.maxDistanceAtFootStrikingTime.ElementAt(i)
                    .Value,
                AngleDifferenceAtTime = ReferenceManager
                    .instance.AngleAtFootStrikingTime.ElementAt(i)
                    .Value,
                MMDistaceAtTime = ReferenceManager
                    .instance.DistanceAtFootStrikingTime.ElementAt(i)
                    .Value,
                SelectedLeg = ResearchMeasurementManager.instance.leftLeg ? "Left Leg" : "Right Leg"
            };
            UploadGaitJson(i, body);
            var item = ReferenceManager.instance.heelPressDetectionBodies.FirstOrDefault(x => TimeSpan.ParseExact(x.TimeOfHeelPressed, @"mm\:ss\:fff", CultureInfo.InvariantCulture) > TimeSpan.ParseExact(ReferenceManager.instance.AngleAtFootStrikingTime.ElementAt(i).Key, @"mm\:ss\:fff", CultureInfo.InvariantCulture) && !x.added);
            
            if (item == null)
            {
                continue;
            }
            item.added = true;
            CreateGaitReportBody bodyHeel = new CreateGaitReportBody()
            {
                ReportsRecordId = ReferenceManager.instance.SelectedVideoID,
                CreatedBy = GeneralStaticManager.GlobalVar["UserName"],
                Subject = GeneralStaticManager.GlobalVar["Subject"],
                HeelPassingAtTime = (float)TimeSpan.ParseExact(item.TimeOfHeelPressed,@"mm\:ss\:fff", CultureInfo.InvariantCulture).TotalSeconds,
                AngleDifferenceAtTime =item.angleDifferenceValue,
                MMDistaceAtTime = item.distanceValue,
                SelectedLeg = ResearchMeasurementManager.instance.leftLeg ? "Left Leg" : "Right Leg"
            };
            UploadGaitJson(i, bodyHeel);
        }
        ReferenceManager.instance.heelPressDetectionBodies.ForEach(x => x.added = false);
    }

    private static void UploadGaitJson(int i, CreateGaitReportBody body)
    {
        string json = JsonConvert.SerializeObject(body);
        APIHandler.instance.Post(
            "UserReport/PostGaitReport",
            json,
            onSuccess: (response) =>
            {
                ResponseWithNoObject responseWithNoObject =
                    JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
                if (responseWithNoObject.isSuccess && i == ReferenceManager.instance.AngleAtFootStrikingTime.Count - 1)
                {
                    ReferenceManager.instance.PopupManager.Show(
                        "Success",
                        "Your Report has been uploaded successfully"
                    );
                }
                if (responseWithNoObject.isError)
                {
                    string reasons = "";
                    foreach (var item in responseWithNoObject.serviceErrors)
                    {
                        reasons += $"\n {item.code} {item.description}";
                    }
                    IOSNativeAlert.ShowAlertMessage(
                        "Signin Failed!",
                        $"Reasons are: {reasons}"
                    );
                    Debug.Log($"{responseWithNoObject.serviceErrors}");
                }
            },
            onError: (error) =>
            {
                IOSNativeAlert.ShowAlertMessage("Signin Failed!", $"Reasons are: {error}");
                Debug.LogError($"Error: {error} value was: {body.HeelPassingAtTime}");
            }
        );
    }
}
