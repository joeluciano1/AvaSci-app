using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PDFExportHandler : MonoBehaviour
{
    // Start is called before the first frame update
    //void Start()
    //{
    //    //Create a new PDF document.

    //    PdfDocument document = new PdfDocument();

    //    //Add a page to the document.

    //    PdfPage page = document.Pages.Add();

    //    //Create PDF graphics for the page.

    //    PdfGraphics graphics = page.Graphics;

    //    //Set the standard font.

    //    PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 20);

    //    //Draw the text.

    //    graphics.DrawString("Hello World!!!", font, PdfBrushes.Black, new Syncfusion.Drawing.PointF(0, 0));

    //    //Create the stream object.

    //    MemoryStream stream = new MemoryStream();

    //    //Save the document into memory stream.

    //    document.Save(stream);

    //    //If the position is not set to '0' then the PDF will be empty.

    //    stream.Position = 0;

    //    //Close the document.

    //    File.WriteAllBytes("Sample.pdf", stream.ToArray());

    //    System.Diagnostics.Process.Start("Sample.pdf", @"C:\Program Files (x86)\Adobe\Acrobat DC\Acrobat\AcroRd32.exe");
    //}

    // Update is called once per frame
    void Update()
    {
        
    }
}
