using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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
    public RectTransform rectTransform1;
    public void GeneratePDFTest()
    {
        
        GraphData graphData = new GraphData()
        {
            Graph1Name = "Left",
            Graph2Name = "Right",
            JointThatIsReported = ReferenceManager.instance.graphManagers.FirstOrDefault(x=>x.gameObject.activeSelf).name,
            MaxGraph1Value = "56",
            MaxGraph2Value = "60",
            MinGraph1Value = "20",
            MinGraph2Value = "34",
            RangeGraph1Value = "12",
            RangeGraph2Value = "35",
            Username = GeneralStaticManager.GlobalVar["UserName"]
        };

        TestButton(graphData);
    }

    public RectTransform rectT; // Assign the UI element which you wanna capture
    public int width; // width of the object to capture
    public int height; // height of the object to capture
    
    [ContextMenu("TakeScreenShot")]    
    
    public void CaptureRectTransform()
    {
       StartCoroutine(takeScreenShot());
    }
    public IEnumerator takeScreenShot()
    {
        yield return new WaitForEndOfFrame(); // it must be a coroutine 

        yield return new WaitForEndOfFrame();

        //Get the corners of RectTransform rect and store it in a array vector
        Vector3[] corners = new Vector3[4];
        rectT.GetWorldCorners(corners);

        //Remove 100 and you will get error
        int width = ((int)corners[3].x - (int)corners[0].x) - 100;
        int height = (int)corners[1].y - (int)corners[0].y;
        var startX = corners[0].x;
        var startY = corners[0].y;

        //Make a temporary texture and read pixels from it
        Texture2D ss = new Texture2D(width, height, TextureFormat.ARGB32, false);
        ss.ReadPixels(new Rect(startX, startY, width, height), 0, 0);
        ss.Apply();
        
        ss = Resize(ss, 446, 288);
        
        
        GraphImage = ss;
        GeneratePDFTest();

    }
    public async void TestButton(GraphData graphData)
    {
        //Create a new PDF document.

        PdfDocument document = new PdfDocument();
        PdfPage page = document.Pages.Add();
        PdfGraphics graphics = page.Graphics;
        

        byte[] bytesHeader = headerImage.EncodeToPNG();
        Stream stream1 = new MemoryStream(bytesHeader);
        PdfBitmap pdfBitmapHeader = new PdfBitmap(stream1);

        byte[] graphImageBytes = GraphImage.EncodeToPNG();
        Stream graphImageStream = new MemoryStream(graphImageBytes);
        PdfBitmap graphImageBitMap = new PdfBitmap(graphImageStream);
        

        PdfStringFormat format = new PdfStringFormat();
        format.Alignment = PdfTextAlignment.Justify;

        
        graphics.DrawImage(pdfBitmapHeader, new PointF(0, 0), new SizeF(page.Size.Width - 100, page.Size.Height));
        graphics.DrawString(graphData.Username, new PdfStandardFont(PdfFontFamily.Helvetica,12,PdfFontStyle.Bold), PdfBrushes.Black, UsernamePosition.ToPointF());
        graphics.DrawString($"{System.DateTime.Now.ToString("MM/dd/yyyy")}", new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Regular), PdfBrushes.Black, DatePosition.ToPointF());
        graphics.DrawString(graphData.JointThatIsReported, new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold|PdfFontStyle.Bold), PdfBrushes.Black, JointNamePosition.ToPointF());
        graphics.DrawString("Min", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Black, MinHeadingPosition.ToPointF());
        graphics.DrawString("Max", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Black, MaxHeadingPosition.ToPointF());
        graphics.DrawString("Range", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Black, RangeHeadingPosition.ToPointF());

        graphics.DrawString(graphData.Graph1Name, new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Red, HeadingGOnePosition.ToPointF());
        graphics.DrawString(graphData.Graph2Name, new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Blue, HeadingGTwoPosition.ToPointF());

        graphics.DrawString(graphData.MinGraph1Value+ "º", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Red, MinValueOnePosition.ToPointF() ,format);
        graphics.DrawString(graphData.MinGraph2Value + "º", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.BlueViolet, MinValueTwoPosition.ToPointF(), format);

        graphics.DrawString(graphData.MaxGraph1Value + "º", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Red, MaxValueOnePosition.ToPointF(), format);
        graphics.DrawString(graphData.MaxGraph2Value + "º", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.BlueViolet, MaxValueTwoPosition.ToPointF(), format);

        graphics.DrawString(graphData.RangeGraph1Value + "º", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.Red, RangeValueOnePosition.ToPointF(), format);
        graphics.DrawString(graphData.RangeGraph2Value + "º", new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Bold), PdfBrushes.BlueViolet, RangeValueTwoPosition.ToPointF(), format);

        graphics.DrawImage(graphImageBitMap, GraphImagePosition.ToPointF());

        MemoryStream stream = new MemoryStream();

        

        document.Save(stream);

        

        stream.Position = 0;
        string path = Path.Combine( Application.persistentDataPath , "Sample.pdf");


        File.WriteAllBytes(path, stream.ToArray());
        await Task.Delay(1000);
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

    Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24,RenderTextureFormat.ARGB32);
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