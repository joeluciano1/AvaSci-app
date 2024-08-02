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
        // Initialize DataTable to assign as DateSource to the light table
        DataTable table = new DataTable();
        pdfLightTable.Style.BorderPen = new PdfPen(new PdfColor(0, 0, 0), 0.5F);

        //Include columns to the DataTable
        table.Columns.Add("Subject");
        table.Columns.Add("Foot Strike at (time)");
        table.Columns.Add("Angle Difference");
        table.Columns.Add("mm Distance");
        table.Columns.Add("Max Angle Difference");
        table.Columns.Add("Max mm Distance");

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
        //Applying cell padding to table
        pdfLightTable.Style.CellPadding = 3;
        pdfLightTable.ApplyBuiltinStyle(PdfLightTableBuiltinStyle.GridTable3Accent3);
        //Assign data source
        pdfLightTable.DataSource = table;
        //Setting this property to true to show the header of table
        pdfLightTable.Style.ShowHeader = true;
        //Draw PdfLightTable
        pdfLightTable.Draw(page, new PointF(0, 0));
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