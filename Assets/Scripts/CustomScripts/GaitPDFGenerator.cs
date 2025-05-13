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
        table.Columns.Add("Initial Pose at (time)");
        table.Columns.Add("Foot Strike at (time)");
        table.Columns.Add("Foot Passing at (time)");
        table.Columns.Add("Hip/Knee ABD Angle Difference");
        table.Columns.Add("Var/Val Distance");
        table.Columns.Add("Condition");
        table.Columns.Add("Hip ABD at (time)");
        table.Columns.Add("Stride Length at (time)");
        // table.Columns.Add("Ankle ABD at (time)");
        table.Columns.Add("Pelvis Angle at (time)");
        table.Columns.Add("Leg in Question");
        //Include rows to the DataTable
        
        StandingDetectionBody item1 = ReferenceManager.instance.standingDetectionBodies[1];
        // TimeSpan initialTime =new TimeSpan();
        if(item1 != null)
        {
            
            item1.added = true;
            string condition = "";
            if (item1.angleDifferenceValue < -1)
            {
                condition = "Valgus";
            }
            else if (item1.angleDifferenceValue > 1)
            {
                condition = "Varus";
            }
            else
            {
                condition = "Normal";
            }
            table.Rows.Add(
                new string[]
                {
                    GeneralStaticManager.GlobalVar["Subject"],
                    TimeSpan.FromSeconds(float.Parse(item1.TimeofStanding)).ToString(@"mm\:ss\:fff"),
                    "",
                    "",
                    item1.angleDifferenceValue.ToString("00.00"),
                    item1.distanceValue.ToString("00.00"),
                    condition,
                    item1.kneeAbductionValue.ToString("00.00"), // here knee is actually hip todo: rename the variable to hipAbductionvalue
                    "Not Calculated Yet",
                    // item1.ankleAbductionValue.ToString("00.00"),
                    item1.pelvisAngleValue.ToString("00.00"),
                    item1.nameOfTheFoot,
                }
            );
        }
        for (int i = 0; i < ReferenceManager.instance.AngleAtFootStrikingTime.Count; i++)
        {
            string condition1 = "";
            if (ReferenceManager
                    .instance.AngleAtFootStrikingTime.ElementAt(i)
                    .Value < -1)
            {
                condition1 = "Valgus";
            }
            else if (ReferenceManager
                         .instance.AngleAtFootStrikingTime.ElementAt(i)
                         .Value > 1)
            {
                condition1 = "Varus";
            }
            else
            {
                condition1 = "Normal";
            }
            table.Rows.Add(
                new string[]
                {
                    GeneralStaticManager.GlobalVar["Subject"],
                    "",
                    TimeSpan.FromSeconds(float.Parse(ReferenceManager
                        .instance.AngleAtFootStrikingTime.ElementAt(i)
                        .Key)).ToString(@"mm\:ss\:fff"),
                        "",
                    ReferenceManager
                        .instance.AngleAtFootStrikingTime.ElementAt(i)
                        .Value.ToString("00.00") + "º",
                    ReferenceManager
                        .instance.DistanceAtFootStrikingTime.ElementAt(i)
                        .Value.ToString("00.00"),
                    condition1,
                        ReferenceManager
                        .instance.KneeAbductionAtFootStrikingTime.ElementAt(i)  // here knee is actually hip todo: rename the variable to hipAbductionvalue
                        .Value.ToString("00.00") + "º",
                        i > ReferenceManager.instance.StrideLengthAtFootStrikingTime.Count-1 ? "Not Calculated Here": ReferenceManager.instance.StrideLengthAtFootStrikingTime.ElementAt(i).Value.ToString(),
                        // ReferenceManager
                        // .instance.AnkleAbductionAtFootStrikingTime.ElementAt(i)
                        // .Value.ToString("00.00") + "º",
                        ReferenceManager
                        .instance.PelvisAngleAtFootStrikingTime.ElementAt(i)
                        .Value.ToString("00.00") + "º",
                    ResearchMeasurementManager.instance.leftLeg ? "Left Leg":"Right Leg",    
                }
            );
            var item = ReferenceManager.instance.heelPressDetectionBodies.FirstOrDefault(x=>float.Parse(x.TimeOfHeelPressed) > float.Parse(ReferenceManager.instance.AngleAtFootStrikingTime.ElementAt(i).Key) &&!x.added);
          
            
            if(item == null || i == ReferenceManager.instance.AngleAtFootStrikingTime.Count-1)
            {
                continue;
            }
            // item.added = true;
            string condition2 = "";
            
                if (item.angleDifferenceValue < -1)
                {
                    condition2 = "Valgus";
                }
                else if (item.angleDifferenceValue > 1)
                {
                    condition2 = "Varus";
                }
                else
                {
                    condition2 = "Normal";
                }
           

            table.Rows.Add(
                new string[]
                {
                    GeneralStaticManager.GlobalVar["Subject"],
                    "",
                    "",
                    TimeSpan.FromSeconds(float.Parse(item.TimeOfHeelPressed)).ToString(@"mm\:ss\:fff"),
                    item.angleDifferenceValue.ToString(),
                    item.distanceValue.ToString(),
                    condition2,
                    item.angleDifferenceValue.ToString(),
                    "Not calculated here",
                    // item.ankleAbductionValue.ToString("00.00"),
                    ((float)item.pelvisAngleValue).ToString("00.00"),
                    ResearchMeasurementManager.instance.leftLeg ? "Left Leg":"Right Leg",
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
        ReferenceManager.instance.standingDetectionBodies.ForEach(x => x.added = false);
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
        if(ReferenceManager.instance.AngleAtFootStrikingTime.Count == 0){
            ReferenceManager.instance.PopupManager.Show("No Detection!", "There were no steps taken by user");
            return;
        }
        for (int i = 0; i < ReferenceManager.instance.AngleAtFootStrikingTime.Count; i++)
        {
            StandingDetectionBody item2 = ReferenceManager.instance.standingDetectionBodies[1];
            if (item2 != null)
            {
                item2.added = true;
                 CreateGaitReportBody bodyStand = new CreateGaitReportBody()
            {
                ReportsRecordId = ReferenceManager.instance.SelectedVideoID,
                CreatedBy = GeneralStaticManager.GlobalVar["UserName"],
                Subject = GeneralStaticManager.GlobalVar["Subject"],
                SubjectStandingAtTime = float.Parse(item2.TimeofStanding),
                AngleDifferenceAtTime =item2.angleDifferenceValue,
                // MMDistaceAtTime = item2.distanceValue,
                HipAbductionAtTime = item2.kneeAbductionValue,
                AnkleAbductionAtTime = item2.ankleAbductionValue,
                PelvisAngleAtTime = item2.pelvisAngleValue,
                VarusValgusAtTime = item2.distanceValue,
                SelectedLeg = ResearchMeasurementManager.instance.leftLeg ? "Left Leg" : "Right Leg",
                Condition = item2.angleDifferenceValue < -1 ? "Valgus" : item2.angleDifferenceValue > 1 ? "Varus" : "Normal",
                StrideLenghtAtTime = null

            };
            UploadGaitJson(i, bodyStand);
            }
            CreateGaitReportBody body = new CreateGaitReportBody()
            {
                ReportsRecordId = ReferenceManager.instance.SelectedVideoID,
                CreatedBy = GeneralStaticManager.GlobalVar["UserName"],
                Subject = GeneralStaticManager.GlobalVar["Subject"],
                FootStrikeAtTime = float.Parse(ReferenceManager
                    .instance.AngleAtFootStrikingTime.ElementAt(i)
                    .Key),
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
                HipAbductionAtTime = ReferenceManager
                    .instance.KneeAbductionAtFootStrikingTime.ElementAt(i)
                    .Value,
                StrideLenghtAtTime = i > ReferenceManager.instance.StrideLengthAtFootStrikingTime.Count-1 ? null: ReferenceManager.instance.StrideLengthAtFootStrikingTime.ElementAt(i).Value,
                // AnkleAbductionAtTime = ReferenceManager
                //     .instance.AnkleAbductionAtFootStrikingTime.ElementAt(i)
                //     .Value,
                PelvisAngleAtTime = ReferenceManager
                    .instance.PelvisAngleAtFootStrikingTime.ElementAt(i)
                    .Value,
                SelectedLeg = ResearchMeasurementManager.instance.leftLeg ? "Left Leg" : "Right Leg",
                VarusValgusAtTime = ReferenceManager.instance.DistanceAtFootStrikingTime.ElementAt(i).Value,
                Condition = ReferenceManager.instance.AngleAtFootStrikingTime.ElementAt(i).Value < -1 ? "Valgus" :ReferenceManager.instance.AngleAtFootStrikingTime.ElementAt(i).Value > 1 ? "Varus" : "Normal"
            };
            UploadGaitJson(i, body);
            var item = ReferenceManager.instance.heelPressDetectionBodies.FirstOrDefault(x=>float.Parse(x.TimeOfHeelPressed) > float.Parse(ReferenceManager.instance.AngleAtFootStrikingTime.ElementAt(i).Key) &&!x.added);
            
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
                HeelPassingAtTime = float.Parse(item.TimeOfHeelPressed),
                AngleDifferenceAtTime =item.angleDifferenceValue,
                MMDistaceAtTime = item.distanceValue,
                HipAbductionAtTime = item.kneeAbductionValue,
                AnkleAbductionAtTime = item.ankleAbductionValue,
                PelvisAngleAtTime = item.pelvisAngleValue,
                SelectedLeg = ResearchMeasurementManager.instance.leftLeg ? "Left Leg" : "Right Leg",
                VarusValgusAtTime = item.distanceValue,
                Condition = item.angleDifferenceValue < -1 ? "Valgus" : item.angleDifferenceValue > 1 ? "Varus" : "Normal",
                StrideLenghtAtTime = null
            };
            UploadGaitJson(i, bodyHeel);
        }
        ReferenceManager.instance.heelPressDetectionBodies.ForEach(x => x.added = false);
        ReferenceManager.instance.standingDetectionBodies.ForEach(x => x.added = false);
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
                    ReferenceManager.instance.PopupManager.Show(
                        "Gait Report Upload Failed!",
                        $"Reasons are: {reasons}"
                    );
                    Debug.Log($"{responseWithNoObject.serviceErrors}");
                }
            },
            onError: (error) =>
            {
                ReferenceManager.instance.PopupManager.Show("Gait Report Upload Failed!", $"Reasons are: {error}");
                Debug.LogError($"Error: {error} value was: {body.HeelPassingAtTime}");
            }
        );
    }
}
