using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of XGraphics.DrawImage in connection with XPdfForm.
  /// </summary>
  public class ImagesFormXObjectTemplate : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
#if true_
      // Create a new PDF document
      //PdfDocument document = new PdfDocument();

      // Create a font
      XFont font = new XFont("Verdana", 16);

      // Create a new page
      //PdfPage page = document.AddPage();
      //XGraphics gfx = XGraphics.FromPdfPage(page);
      //gfx.DrawString("XPdfForm Sample", font, XBrushes.DarkGray, 15, 25, XStringFormat.Default);

      // Step 1: Create an XForm and draw some graphics on it

      // Create an empty XForm object with the specified width and height
      // A form is bound to its target document when it is created. The reason is that the form can 
      // share fonts and other objects with its target document.
      XForm form = new XForm(gfx, XUnit.FromMillimeter(70), XUnit.FromMillimeter(55));

      // Create an XGraphics object for drawing the contents of the form.
      XGraphics formGfx = XGraphics.FromForm(form);

      // Draw a large transparent rectangle to visualize the area the form occupies
      XColor back = XColors.Orange;
      back.A = 0.2;
      XSolidBrush brush = new XSolidBrush(back);
      formGfx.DrawRectangle(brush, -10000, -10000, 20000, 20000);


      // On a form you can draw...

      //// ... text
      //formGfx.DrawString("Text, Graphics, Images, and Forms", new XFont("Verdana", 10, XFontStyle.Regular), XBrushes.Navy, 3, 0, XStringFormat.TopLeft);
      //XPen pen = XPens.LightBlue.Clone();
      //pen.Width = 2.5;

      // ... graphics like Bézier curves
      //formGfx.DrawBeziers(pen, XPoint.ParsePoints("30,120 80,20, 100,140 175,33.3"));

      //// ... raster images like GIF files
      //XGraphicsState state = formGfx.Save();
      //formGfx.RotateAtTransform(17, new XPoint(30, 30));
      //formGfx.DrawImage(XImage.FromFile("../../../../XGraphicsLab/images/Test.gif"), 20, 20);
      //formGfx.Restore(state);

      //// ... and forms like XPdfForm objects
      //state = formGfx.Save();
      //formGfx.RotateAtTransform(-8, new XPoint(165, 115));
      //formGfx.DrawImage(XPdfForm.FromFile("../../../../PDFs/SomeLayout.pdf"), new XRect(140, 80, 50, 50 * Math.Sqrt(2)));
      //formGfx.Restore(state);

      // When you finished drawing on the form, dispose the XGraphic object.
      formGfx.Dispose();


      // Step 2: Draw the XPdfForm on your PDF page like an image

      // Draw the form on the page of the document in its original size
      gfx.DrawImage(form, 20, 50);

#if true_
      // Draw it stretched
      gfx.DrawImage(form, 300, 100, 250, 40);

      // Draw and rotate it
      int d = 25;
      for (int idx = 0; idx < 360; idx += d)
      {
        gfx.DrawImage(form, 300, 480, 200, 200);
        gfx.RotateAtTransform(d, new XPoint(300, 480));
      }
#endif

      //// Save the document...
      //string filename = "XForms.pdf";
      //document.Save(filename);
      //// ...and start a viewer.
      //Process.Start(filename);
#else
      //base.RenderPage(gfx);

      int cx = 300;
      int cy = 240;
      XForm form;
      //if (gfx.PdfPage == null)
        form = new XForm(gfx, cx, cy);
      //else
      //  form = new XForm(gfx.PdfPage.Owner, cx, cy);

      double dx = gfx.PageSize.Width;
      double dy = gfx.PageSize.Height;

      XGraphics formgfx = XGraphics.FromForm(form);
      XSolidBrush brush = new XSolidBrush(XColor.FromArgb(128, 0, 255, 255));
      formgfx.DrawRectangle(brush, -1000, -1000, 2000, 2000);
      formgfx.DrawLine(XPens.Red, 0, 0, cx, cy);
      formgfx.DrawLine(XPens.Red, cx, 0, 0, cy);

      XFont font = new XFont("Times", 16, XFontStyle.BoldItalic);
      formgfx.DrawString("Text", font, XBrushes.DarkOrange, 0, 0, XStringFormats.TopLeft);
      formgfx.DrawString("Text", font, XBrushes.DarkOrange, new XRect(0, 0, cx, cy), XStringFormats.Center);

      // Required to finish drawing the form. Both cases are correct.
#if true
      formgfx.Dispose();
#else
      form.DrawingFinished();
#endif
      gfx.DrawImage(form, 50, 50);

#if true_
      gfx.TranslateTransform(dx / 2, dy / 2);
      gfx.RotateTransform(-25);
      gfx.TranslateTransform(-dx / 2, -dy / 2);

      gfx.DrawImage(form, (dx - form.Width) / 2, (dy - form.Height) / 2, form.Width, form.Height);
#endif
#endif
    }

    public override string Description
    {
      get { return "DrawImage"; }
    }
  }
}
