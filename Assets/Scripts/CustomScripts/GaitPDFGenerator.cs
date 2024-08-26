using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        PdfLightTable pdfLightTable2 = new PdfLightTable();
        // Initialize DataTable to assign as DateSource to the light table
        DataTable table = new DataTable();
        DataTable table2 = new DataTable();
        pdfLightTable.Style.BorderPen = new PdfPen(new PdfColor(0, 0, 0), 0.5F);
        pdfLightTable2.Style.BorderPen = new PdfPen(new PdfColor(0, 0, 0), 0.5F);

        //Include columns to the DataTable
        table.Columns.Add("Subject");
        table.Columns.Add("Foot Strike at (time)");
        table.Columns.Add("Angle Difference");
        table.Columns.Add("mm Distance");
        table.Columns.Add("Max Angle Difference");
        table.Columns.Add("Max mm Distance");

        table2.Columns.Add("Subject");
        table2.Columns.Add("Heal Passing (Time)");
        table2.Columns.Add("ABD Angle Difference");
        table2.Columns.Add("Hip/Knee Distance");
        table2.Columns.Add("Heel Detected");


        //Include rows to the DataTable
        for (int i = 0; i < ReferenceManager.instance.maxAngleAtFootStrikingTime.Count; i++)
        {
            table.Rows.Add(
                new string[]
                {
                    GeneralStaticManager.GlobalVar["Subject"],
                    ReferenceManager
                        .instance.maxAngleAtFootStrikingTime.ElementAt(i)
                        .Key.ToString("0.0"),
                    ReferenceManager
                        .instance.AngleAtFootStrikingTime.ElementAt(i)
                        .Value.ToString("0.00") + "º",
                    ReferenceManager
                        .instance.DistanceAtFootStrikingTime.ElementAt(i)
                        .Value.ToString("0.00") + "mm",
                    ReferenceManager
                        .instance.maxAngleAtFootStrikingTime.ElementAt(i)
                        .Value.ToString("0.00") + "º",
                    ReferenceManager
                        .instance.maxDistanceAtFootStrikingTime.ElementAt(i)
                        .Value.ToString("0.00") + "mm"
                }
            );
        }
        for(int i = 0; i < ReferenceManager.instance.heelPressDetectionBodies.Count; i++)
        {
            table2.Rows.Add(
                new string[]
                {
                    GeneralStaticManager.GlobalVar["Subject"],
                    TimeSpan.FromSeconds(float.Parse(ReferenceManager
                        .instance.heelPressDetectionBodies[i].TimeOfHeelPressed)).ToString(@"mm\:ss\:fff"),
                    ReferenceManager
                        .instance.heelPressDetectionBodies[i].angleDifferenceValue.ToString("0.00"),
                    ReferenceManager
                        .instance.heelPressDetectionBodies[i].distanceValue.ToString("0.00"),
                    ReferenceManager
                        .instance.heelPressDetectionBodies[i].nameOfTheFoot,
                }
            );
        }
        //Applying cell padding to table
        pdfLightTable.Style.CellPadding = 3;
        pdfLightTable2.Style.CellPadding = 3;
        pdfLightTable.ApplyBuiltinStyle(PdfLightTableBuiltinStyle.GridTable3Accent3);
        pdfLightTable2.ApplyBuiltinStyle(PdfLightTableBuiltinStyle.GridTable3Accent3);
        //Assign data source
        pdfLightTable.DataSource = table;
        pdfLightTable2.DataSource = table2;
        //Setting this property to true to show the header of table
        pdfLightTable.Style.ShowHeader = true;
        pdfLightTable2.Style.ShowHeader = true;
        //Draw PdfLightTable
        var result =pdfLightTable.Draw(page, new PointF(0, 0));
        pdfLightTable2.Draw(page, new PointF(0, result.Bounds.Height+10));
        //Save the document
        MemoryStream stream = new MemoryStream();

        doc.Save(stream);

        stream.Position = 0;
        string path = Path.Combine(Application.persistentDataPath, "Gait.pdf");

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
        for (int i = 0; i < ReferenceManager.instance.maxAngleAtFootStrikingTime.Count; i++)
        {
            CreateGaitReportBody body = new CreateGaitReportBody()
            {
                ReportsRecordId = ReferenceManager.instance.SelectedVideoID,
                CreatedBy = GeneralStaticManager.GlobalVar["UserName"],
                Subject = GeneralStaticManager.GlobalVar["Subject"],
                FootStrikeAtTime = ReferenceManager
                    .instance.maxAngleAtFootStrikingTime.ElementAt(i)
                    .Key,
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
            string json = JsonConvert.SerializeObject(body);
            APIHandler.instance.Post(
                "UserReport/PostGaitReport",
                json,
                onSuccess: (response) =>
                {
                    ResponseWithNoObject responseWithNoObject =
                        JsonConvert.DeserializeObject<ResponseWithNoObject>(response);
                    if (responseWithNoObject.isSuccess)
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
                    Debug.LogError($"Error: {error}");
                }
            );
        }
    }
}
