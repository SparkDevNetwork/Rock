using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfSharp.UnitTests.Helpers
{
  public delegate void RenderMethod(XGraphics gfx);

  public class TestBase
  {
    public static readonly double WidthInMillimeter = 135;
    public static readonly double HeightInMillimeter = 180;
    public static readonly double WidthInPoint = XUnit.FromMillimeter(WidthInMillimeter);
    public static readonly double HeightInPoint = XUnit.FromMillimeter(HeightInMillimeter);

    /// <summary>
    /// Prepares new PDF page for drawing.
    /// </summary>
    public void BeginPdf()
    {
      document = new PdfDocument();
      document.PageLayout = PdfPageLayout.SinglePage;
      PdfPage page = document.AddPage();
      page.Width = WidthInPoint;
      page.Height = HeightInPoint;
      this.pdfGfx = XGraphics.FromPdfPage(page);

      // Draw a bounding box
      XRect rect = new XRect(0.5, 0.5, WidthInPoint - 1, HeightInPoint - 1);
      this.pdfGfx.DrawRectangle(XBrushes.WhiteSmoke, rect);
    }

    /// <summary>
    /// Ends current PDF page.
    /// </summary>
    public void EndPdf()
    {
      this.pdfGfx = null;
    }

    /// <summary>
    /// Prepares new bitmap image for drawing.
    /// </summary>
    public void BeginImage()
    {
#if GDI
      int factor = 2;
      int width = (int)(WidthInPoint * factor);
      int height = (int)(HeightInPoint * factor);
      this.image = new Bitmap(width, height);
      this.image.SetResolution(72 * factor, 72 * factor);
      this.gdiGfx = Graphics.FromImage(this.image);
      this.gdiGfx.SmoothingMode = SmoothingMode.HighQuality;

      // Draw a bounding box
      Rectangle rect = new Rectangle(0, 0, width - 1, height - 1);
      this.gdiGfx.FillRectangle(Brushes.WhiteSmoke, rect);
      //this.gdiGfx.DrawRectangle(new Pen(Brushes.LightGray, factor), rect);

      this.gdiGfx.ScaleTransform(factor, factor);
      this.imgGfx = XGraphics.FromGraphics(this.gdiGfx, new XSize(WidthInPoint, HeightInPoint), XGraphicsUnit.Point);
#endif
#if WPF
      int factor = 4;
      int width = (int)(WidthInPoint * factor);
      int height = (int)(HeightInPoint * factor);
      this.image = new RenderTargetBitmap(width, height, 72 * factor, 72 * factor, PixelFormats.Default);
      this.dv = new DrawingVisual();
      this.dc = this.dv.RenderOpen();

      // Draw a bounding box
      //Rect rect = new Rect(0, 0, width - 1, height - 1);
      Rect rect = new Rect(0, 0, WidthInPoint * 4 / 3 - 1, HeightInPoint * 4 / 3 - 1);
      //this.dc.DrawRectangle(Brushes.WhiteSmoke, new Pen(Brushes.LightGray, 1), rect);
      this.dc.DrawRectangle(Brushes.WhiteSmoke, null, rect);

      //double d = 10;
      //Pen pen = new Pen(Brushes.Red, 5);
      //this.dc.DrawLine(pen, rect.TopLeft, rect.TopLeft + new Vector(d, d));
      //this.dc.DrawLine(pen, rect.TopRight, rect.TopRight + new Vector(-d, d));
      //this.dc.DrawLine(pen, rect.BottomLeft, rect.BottomLeft + new Vector(d, -d));
      //this.dc.DrawLine(pen, rect.BottomRight, rect.BottomRight + new Vector(-d, -d));

      //this.dc.PushTransform(new ScaleTransform(factor, factor));
      this.imgGfx = XGraphics.FromDrawingContext(this.dc, new XSize(WidthInPoint, HeightInPoint), XGraphicsUnit.Point);
#endif
    }

    /// <summary>
    /// Ends current GDI+ image.
    /// </summary>
    public void EndImage()
    {
      //this.gdiGfx.Dispose();
      //this.image.Dispose();
      //this.image.Dispose();

      //this.image = null;
      //this.gdiGfx = null;
      //this.imgGfx = null;
    }

    public void Render(string name, RenderMethod renderMethod)
    {
      Name = name;

      renderMethod(this.pdfGfx);
      SavePdf();

      renderMethod(this.imgGfx);
      SaveImage();

      AppendToResultPdf();
    }

    /// <summary>
    /// Saves the PDF file of the current test.
    /// </summary>
    public void SavePdf()
    {
      this.document.Save(Path.Combine(OutputDirectory, Name + ".pdf"));
    }

    /// <summary>
    /// Saves the bitmap image file of the current test.
    /// </summary>
    public void SaveImage()
    {
#if GDI
      this.image.Save(Path.Combine(OutputDirectory, Name + ".png"), ImageFormat.Png);
#endif
#if WPF
      this.dc.Close();
      this.image.Render(this.dv);

      FileStream stream = new FileStream(Path.Combine(OutputDirectory, Name + ".png"), FileMode.Create);
      PngBitmapEncoder encoder = new PngBitmapEncoder();
      string author = encoder.CodecInfo.Author.ToString();
      encoder.Frames.Add(BitmapFrame.Create(this.image));
      encoder.Save(stream);
      stream.Close();
#endif
    }

    /// <summary>
    /// Append PDF and bitmap image to result PDF file.
    /// </summary>
    public void AppendToResultPdf()
    {
      string resultFileName = Path.Combine(OutputDirectory, "~TestResult.pdf");
      PdfDocument pdfResultDocument = null;
      if (File.Exists(resultFileName))
        pdfResultDocument = PdfReader.Open(resultFileName, PdfDocumentOpenMode.Modify);
      else
      {
        pdfResultDocument = new PdfDocument();
        pdfResultDocument.PageLayout = PdfPageLayout.SinglePage;

#if GDI
        pdfResultDocument.Info.Title = "PDFsharp Unit Tests based on GDI+";
#endif
#if WPF
        pdfResultDocument.Info.Title = "PDFsharp Unit Tests based on WPF";
#endif
        pdfResultDocument.Info.Author = "Stefan Lange";
      }

      PdfPage page = pdfResultDocument.AddPage();
      page.Orientation = PageOrientation.Landscape;
      XGraphics gfx = XGraphics.FromPdfPage(page);
      gfx.DrawRectangle(XBrushes.GhostWhite, new XRect(0, 0, 1000, 1000));

      double x1 = XUnit.FromMillimeter((297 - 2 * WidthInMillimeter) / 3);
      double x2 = XUnit.FromMillimeter((297 - 2 * WidthInMillimeter) / 3 * 2 + WidthInMillimeter);
      double y = XUnit.FromMillimeter((210 - HeightInMillimeter) / 2);
      double yt = XUnit.FromMillimeter(HeightInMillimeter) + y + 20;
      gfx.DrawString(String.Format("PDFsharp Unit Test '{0}'", this.Name), new XFont("Arial", 9, XFontStyle.Bold),
        XBrushes.DarkRed, new XPoint(x1, 30));

      // Draw the PDF result
#if GDI
      //gfx.DrawString("What you see: A PDF form created with PDFsharp based on GDI+", new XFont("Verdana", 9), XBrushes.DarkBlue, new XPoint(x1, yt));
      string subtitle = "This is a PDF form created with PDFsharp based on GDI+";
#endif
#if WPF
      //gfx.DrawString("What you see: A PDF form created with PDFsharp based on WPF", new XFont("Verdana", 9), XBrushes.DarkBlue, new XPoint(x1, yt));
      string subtitle = "This is a PDF form created with PDFsharp based on WPF";
#endif
      gfx.DrawString(subtitle, new XFont("Arial", 8),        XBrushes.DarkBlue, new XRect(x1, yt, WidthInPoint, 0), XStringFormats.Default);
      XPdfForm form = XPdfForm.FromFile(Path.Combine(OutputDirectory, Name + ".pdf"));
      gfx.DrawImage(form, new XPoint(x1, y));

      // Draw the result bitmap
#if GDI
      //gfx.DrawString("What you see: A bitmap image created with PDFsharp based on GDI+", new XFont("Verdana", 9), XBrushes.DarkBlue, new XPoint(x2, yt));
      subtitle = "As a reference, this is a bitmap image created with PDFsharp based on GDI+";
#endif
#if WPF
      //gfx.DrawString("What you see: A bitmap image created with PDFsharp based on WPF", new XFont("Verdana", 9), XBrushes.DarkBlue, new XPoint(x2, yt));
      subtitle = "As a reference, this is a bitmap image created with PDFsharp based on WPF";
#endif
      gfx.DrawString(subtitle, new XFont("Arial", 8), XBrushes.DarkBlue, new XRect(x2, yt, WidthInPoint, 0), XStringFormats.Default);
      XImage image = XImage.FromFile(Path.Combine(OutputDirectory, Name + ".png"));
      image.Interpolate = false;
      gfx.DrawImage(image, new XPoint(x2, y));
      pdfResultDocument.Save(resultFileName);
    }

    public string Name;

#if GDI
    protected XGraphics pdfGfx;
    protected Bitmap image;
    protected Graphics gdiGfx;
    protected XGraphics imgGfx;
    protected PdfDocument document;
#endif
#if WPF
    protected XGraphics pdfGfx;
    DrawingVisual dv;
    //protected BitmapImage image;
    protected RenderTargetBitmap image;
    protected DrawingContext dc;
    protected XGraphics imgGfx;
    protected PdfDocument document;
#endif

    public static string OutputDirectory = ".";
  }
}