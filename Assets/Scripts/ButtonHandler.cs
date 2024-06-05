using ChartAndGraph;
using LightBuzz.AvaSci.Measurements;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class ButtonHandler : MonoBehaviour
{
    public Texture2D headerImage;
    public Texture2D GraphImage;
    [Space(10)]
    public Vector2 UsernamePosition;
    public Vector2 DatePosition;
    public Vector2 JointNamePosition;
    public Vector2 HeadingGOnePosition;
    public Vector2 HeadingGTwoPosition;
    public Vector2 MinHeadingPosition;
    public Vector2 MaxHeadingPosition;
    public Vector2 RangeHeadingPosition;

    public Vector2 MinValueOnePosition;
    public Vector2 MinValueTwoPosition;
    public Vector2 MaxValueOnePosition;
    public Vector2 MaxValueTwoPosition;
    public Vector2 RangeValueOnePosition;
    public Vector2 RangeValueTwoPosition;
    public Vector2 GraphImagePosition;

    public Canvas MainCanvas;
    public ScrollRect scrollView;
    public bool dontHidePDF;
    public Toggle GraphPanelButton;
    private void Start()
    {
        ScrollViewFocusFunctions.FocusOnItem(scrollView, ReferenceManager.instance.graphManagers[0].GetComponent<RectTransform>());
    }
    public void GeneratePDFTest()
    {
        List<GraphData> graphDatas = new List<GraphData>();
        foreach (GraphManager MyGraphManager in ReferenceManager.instance.graphManagers)
        {
            string jointOne = Enum.GetName(typeof(MeasurementType), MyGraphManager.JointType);
            string jointTwo = MyGraphManager.SecondJointType == MeasurementType.None ? "" : Enum.GetName(typeof(MeasurementType), MyGraphManager.SecondJointType);
            string jointsinReport = jointOne + ", " + jointTwo;

            string maxJointOne;
            string minJointOne;
            string rangeJointOne;

            string maxJointTwo = "";
            string minJointTwo = "";
            string rangeJointTwo = "";

            //////////////// First Joint /////////////////

            double jointOneMaxValue = Math.Round(GeneralStaticManager.GraphsReadings[jointOne].Max(), 2);
            double jointOneMinValue = Math.Round(GeneralStaticManager.GraphsReadings[jointOne].Min(), 2);
            maxJointOne = jointOneMaxValue.ToString();

            minJointOne = jointOneMinValue.ToString();
            rangeJointOne = Math.Abs(jointOneMaxValue - jointOneMinValue).ToString();

            //////////////// Second Joint /////////////////

            if (MyGraphManager.SecondJointType != MeasurementType.None)
            {
                double jointTwoMaxValue = Math.Round(GeneralStaticManager.GraphsReadings[jointTwo].Max(), 2);
                double jointTwoMinValue = Math.Round(GeneralStaticManager.GraphsReadings[jointTwo].Min(), 2);
                maxJointTwo = jointTwoMaxValue.ToString();

                minJointTwo = jointTwoMinValue.ToString();
                rangeJointTwo = Math.Abs(jointTwoMaxValue - jointTwoMinValue).ToString();
            }

            GraphData graphData = new GraphData()
            {
                Graph1Name = jointOne,
                Graph2Name = jointTwo,
                JointThatIsReported = jointsinReport,
                MaxGraph1Value = maxJointOne,
                MaxGraph2Value = maxJointTwo,
                MinGraph1Value = minJointOne,
                MinGraph2Value = minJointTwo,
                RangeGraph1Value = rangeJointOne,
                RangeGraph2Value = rangeJointTwo,
                Username = GeneralStaticManager.GlobalVar["UserName"]
            };

            graphDatas.Add(graphData);
        }
        TestButton(graphDatas);
    }



    
    public async void CaptureRectTransform()
    {
        if (ReferenceManager.instance.GraphMinimizer.GraphToResize.preferredHeight == 0)
        {
            GraphPanelButton.onValueChanged.Invoke(true);
            await Task.Delay(1000);
            //ReferenceManager.instance.PopupManager.Show("Not Allowed", "Please maximize the graph first", true);
            //return;
        }



        StartCoroutine(takeScreenShot());
    }

    public IEnumerator takeScreenShot()
    {
        MainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Canvas.ForceUpdateCanvases();
        foreach (GraphManager graphManager in ReferenceManager.instance.graphManagers)
        {
            //Code will throw error at runtime if this is removed
            ScrollViewFocusFunctions.FocusOnItem(scrollView, graphManager.GetComponent<RectTransform>());

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            //Get the corners of RectTransform rect and store it in a array vector
            Vector3[] corners = new Vector3[4];
            graphManager.MySSRef.GetWorldCorners(corners);

            //Remove 100 and you will get error
            int width = ((int)corners[3].x - (int)corners[0].x) - 100;
            int height = (int)corners[1].y - (int)corners[0].y;
            var startX = corners[0].x;
            var startY = corners[0].y;

            //Make a temporary texture and read pixels from it
            Texture2D ss = new Texture2D(width, height, TextureFormat.RGB24, false);

            ss.ReadPixels(new Rect(startX, startY, width, height), 0, 0);

            ss.Apply();

            GraphImage = ss;
            graphManager.GraphImage = Resize(GraphImage, 446, 288);

        }
        ReferenceManager.instance.LoadingManager.Show("Generating PDF Report for you");
        GeneratePDFTest();
        MainCanvas.renderMode = RenderMode.ScreenSpaceCamera;

    }
    public async void TestButton(List<GraphData> graphDatas)
    {
        //Create a new PDF document.
        PdfDocument document = new PdfDocument();
        foreach (GraphData graphData in graphDatas)
        {
            PdfPage page = document.Pages.Add();
            PdfGraphics graphics = page.Graphics;


            byte[] bytesHeader = headerImage.EncodeToPNG();
            Stream stream1 = new MemoryStream(bytesHeader);
            PdfBitmap pdfBitmapHeader = new PdfBitmap(stream1);

            GraphImage = ReferenceManager.instance.graphManagers.FirstOrDefault(x => graphData.JointThatIsReported.Contains(Enum.GetName(typeof(MeasurementType), x.JointType))).GraphImage;

            byte[] graphImageBytes = GraphImage.EncodeToPNG();
            Stream graphImageStream = new MemoryStream(graphImageBytes);
            PdfBitmap graphImageBitMap = new PdfBitmap(graphImageStream);


            PdfStringFormat format = new PdfStringFormat();
            format.Alignment = PdfTextAlignment.Justify;


            graphics.DrawImage(pdfBitmapHeader, new PointF(0, 0), new SizeF(page.Size.Width - 100, page.Size.Height));
            graphics.DrawString(graphData.Username, new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold), PdfBrushes.Black, UsernamePosition.ToPointF());
            graphics.DrawString($"{System.DateTime.Now.ToString("MM/dd/yyyy")}", new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Regular), PdfBrushes.Black, DatePosition.ToPointF());
            graphics.DrawString(graphData.JointThatIsReported, new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold | PdfFontStyle.Bold), PdfBrushes.Black, JointNamePosition.ToPointF());



            if (!string.IsNullOrWhiteSpace(graphData.MinGraph1Value))
                graphics.DrawString("Min", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Black, MinHeadingPosition.ToPointF());
            if (!string.IsNullOrWhiteSpace(graphData.MaxGraph1Value))
                graphics.DrawString("Max", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Black, MaxHeadingPosition.ToPointF());
            if (!string.IsNullOrWhiteSpace(graphData.RangeGraph1Value))
                graphics.DrawString("Range", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Black, RangeHeadingPosition.ToPointF());

            if (!string.IsNullOrWhiteSpace(graphData.MinGraph1Value))
            {
                if (graphData.Graph1Name.Contains("Left"))
                    graphics.DrawString("Left", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Red, HeadingGOnePosition.ToPointF());
                else if (graphData.Graph1Name.Contains("Right"))
                    graphics.DrawString("Right", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Red, HeadingGOnePosition.ToPointF());
                else
                    graphics.DrawString("", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Red, HeadingGOnePosition.ToPointF());
            }
            if (!string.IsNullOrWhiteSpace(graphData.MinGraph2Value))
            {
                if (graphData.Graph2Name.Contains("Left"))
                    graphics.DrawString("Left", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Blue, HeadingGTwoPosition.ToPointF());
                else if (graphData.Graph2Name.Contains("Right"))
                    graphics.DrawString("Right", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Blue, HeadingGTwoPosition.ToPointF());
                else
                    graphics.DrawString("", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Blue, HeadingGTwoPosition.ToPointF());
            }

            if (!string.IsNullOrWhiteSpace(graphData.MinGraph1Value))
                graphics.DrawString(graphData.MinGraph1Value + (graphData.Graph1Name.Contains("Distance") ? "m" : "º"), new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Red, MinValueOnePosition.ToPointF(), format);
            if (!string.IsNullOrWhiteSpace(graphData.MinGraph2Value))
                graphics.DrawString(graphData.MinGraph2Value + (graphData.Graph2Name.Contains("Distance") ? "m" : "º"), new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.BlueViolet, MinValueTwoPosition.ToPointF(), format);

            if (!string.IsNullOrWhiteSpace(graphData.MaxGraph1Value))
                graphics.DrawString(graphData.MaxGraph1Value + (graphData.Graph1Name.Contains("Distance") ? "m" : "º"), new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Red, MaxValueOnePosition.ToPointF(), format);
            if (!string.IsNullOrWhiteSpace(graphData.MaxGraph2Value))
                graphics.DrawString(graphData.MaxGraph2Value + (graphData.Graph2Name.Contains("Distance") ? "m" : "º"), new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.BlueViolet, MaxValueTwoPosition.ToPointF(), format);

            if (!string.IsNullOrWhiteSpace(graphData.RangeGraph1Value))
                graphics.DrawString(graphData.RangeGraph1Value + (graphData.Graph1Name.Contains("Distance") ? "m" : "º"), new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Red, RangeValueOnePosition.ToPointF(), format);
            if (!string.IsNullOrWhiteSpace(graphData.RangeGraph2Value))
                graphics.DrawString(graphData.RangeGraph2Value + (graphData.Graph2Name.Contains("Distance") ? "m" : "º"), new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.BlueViolet, RangeValueTwoPosition.ToPointF(), format);

            graphics.DrawImage(graphImageBitMap, GraphImagePosition.ToPointF());
        }
        MemoryStream stream = new MemoryStream();



        document.Save(stream);



        stream.Position = 0;
        string path = Path.Combine(Application.persistentDataPath, "Sample.pdf");


        File.WriteAllBytes(path, stream.ToArray());
        await Task.Delay(1000);
        if (!dontHidePDF)
        {
#if UNITY_EDITOR
            System.Diagnostics.Process.Start(path);
            Debug.Log("Is Editor");

#else

        string url = "file://" + path.Replace(" ", "%20");
        Debug.Log("URL = "+ url);
        Debug.Log("Persistance = " + path);
        GeneralStaticManager.OpenFile(path);
#endif
        }
        dontHidePDF = false;
        ReferenceManager.instance.LoadingManager.Hide();
    }
    Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24, RenderTextureFormat.ARGB32);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

}
public class GraphData
{
    public string Username;
    public string JointThatIsReported;
    public string Graph1Name;
    public string Graph2Name;
    public string MinGraph1Value;
    public string MinGraph2Value;
    public string MaxGraph1Value;
    public string MaxGraph2Value;
    public string RangeGraph1Value;
    public string RangeGraph2Value;
}