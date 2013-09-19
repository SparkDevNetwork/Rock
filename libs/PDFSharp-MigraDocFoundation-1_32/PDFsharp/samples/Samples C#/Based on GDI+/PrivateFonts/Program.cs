using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace PrivateFonts
{
  /// <summary>
  /// This sample shows how to use fonts that are not installed but comes as resources of the application.
  /// </summary>
  class Program
  {
    static void Main(string[] args)
    {
      // First of all initialize the global XPrivateFontCollection.
      XPrivateFontCollection globalFontCollection = XPrivateFontCollection.Global;

      // Due to technical limitations of the current implementation of PDFsharp the global font collection
      // must be initialized when your applications starts and belongs implicitely to all PDF documents.


      // Adding private fonts differs between GDI+ and WPF

#if GDI
#endif

#if WPF
      // Without the following line of code the Uri constructor (see below) fails...
      new System.Windows.Application();

      // Add the 3 type faces of 'FrutigerLight' from the resources
      Uri uri = new Uri("pack://application:,,,/");
      //const string name = "./FrutigerFonts/#FrutigerLight";
      //const string name = "./Fonts/#Early Tickertape";
      const string name = "./#Early Tickertape";
      globalFontCollection.Add(uri, name);

      // Add 2 type faces of 'Frutiger' from the resources
      //globalFontCollection.Add(uri, "./FrutigerFonts/#Frutiger");
      //globalFontCollection.Add(uri, "./Fonts/#Oblivious font");
      globalFontCollection.Add(uri, "./#Oblivious font");
#endif

      // Create new document
      PdfDocument document = new PdfDocument();

      // Set font embedding to Always
      XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);

      PdfPage page = document.AddPage();
      XGraphics gfx = XGraphics.FromPdfPage(page);

      const string familyName1 = "Early Tickertape";

      XFont font1 = new XFont(familyName1, 24, XFontStyle.Regular, options);
      gfx.DrawString("Early Tickertape", font1, XBrushes.Black, 100, 100);

      XFont font2 = new XFont(familyName1, 24, XFontStyle.Bold, options);
      gfx.DrawString("Early Tickertape bold (fontface n.a.)", font2, XBrushes.Black, 100, 150);

      XFont font3 = new XFont(familyName1, 24, XFontStyle.Italic, options);
      gfx.DrawString("Early Tickertape italic (fontface n.a.)", font3, XBrushes.Black, 100, 200);

      XFont font4 = new XFont(familyName1, 24, XFontStyle.BoldItalic, options);
      gfx.DrawString("Early Tickertape bold italic (fontface n.a.)", font4, XBrushes.Black, 100, 250);

      const string familyName2 = "Oblivious font";

      XFont font5 = new XFont(familyName2, 24, XFontStyle.Regular, options);
      gfx.DrawString("Oblivious regular", font5, XBrushes.Black, 100, 400);

      XFont font6 = new XFont(familyName2, 24, XFontStyle.Bold, options);
      gfx.DrawString("Oblivious bold (fontface n.a.)", font6, XBrushes.Black, 100, 450);

      XFont font7 = new XFont(familyName2, 24, XFontStyle.Italic, options);
      gfx.DrawString("Oblivious italic (fontface n.a.)", font7, XBrushes.Black, 100, 500);

      XFont font8 = new XFont(familyName2, 24, XFontStyle.BoldItalic, options);
      gfx.DrawString("Oblivious bold italic (fontface n.a.)", font8, XBrushes.Black, 100, 550);

#if false
      const string familyName1 = "FrutigerLight";

      XFont font1 = new XFont(familyName1, 24, XFontStyle.Regular, options);
      gfx.DrawString("Frutiger Light regular", font1, XBrushes.Black, 100, 100);

      XFont font2 = new XFont(familyName1, 24, XFontStyle.Bold, options);
      gfx.DrawString("Frutiger Light bold", font2, XBrushes.Black, 100, 150);

      XFont font3 = new XFont(familyName1, 24, XFontStyle.Italic, options);
      gfx.DrawString("Frutiger Light italic", font3, XBrushes.Black, 100, 200);

      XFont font4 = new XFont(familyName1, 24, XFontStyle.BoldItalic, options);
      gfx.DrawString("Frutiger Light bold italic (fontface n.a.)", font4, XBrushes.Black, 100, 250);

      const string familyName2 = "Frutiger";

      XFont font5 = new XFont(familyName2, 24, XFontStyle.Regular, options);
      gfx.DrawString("Frutiger regular", font5, XBrushes.Black, 100, 400);

      XFont font6 = new XFont(familyName2, 24, XFontStyle.Bold, options);
      gfx.DrawString("Frutiger bold", font6, XBrushes.Black, 100, 450);

      XFont font7 = new XFont(familyName2, 24, XFontStyle.Italic, options);
      gfx.DrawString("Frutiger italic (fontface n.a.)", font7, XBrushes.Black, 100, 500);

      XFont font8 = new XFont(familyName2, 24, XFontStyle.BoldItalic, options);
      gfx.DrawString("Frutiger bold italic (fontface n.a.)", font8, XBrushes.Black, 100, 550);
#endif

      string filename = Guid.NewGuid().ToString("N") + "_tempfile.pdf";
      // Save the document...
      document.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }
  }
}
