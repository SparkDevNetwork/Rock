#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   PDFsharp Team (mailto:PDFsharpSupport@pdfsharp.de)
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Watermark
{
  /// <summary>
  /// This sample shows three variations how to add a watermark text to an
  /// existing PDF file.
  /// </summary>
  class Program
  {
    static void Main()
    {
      const string watermark = "PDFsharp";
      const int emSize = 150;

      // Get a fresh copy of the sample PDF file
      const string filename = "Portable Document Format.pdf";
      File.Copy(Path.Combine("../../../../../PDFs/", filename),
        Path.Combine(Directory.GetCurrentDirectory(), filename), true);

      // Create the font for drawing the watermark
      XFont font = new XFont("Times New Roman", emSize, XFontStyle.BoldItalic);

      // Open an existing document for editing and loop through its pages
      PdfDocument document = PdfReader.Open(filename);

      // Set version to PDF 1.4 (Acrobat 5) because we use transparency.
      if (document.Version < 14)
        document.Version = 14;

      for (int idx = 0; idx < document.Pages.Count; idx++)
      {
        //if (idx == 1) break;
        PdfPage page = document.Pages[idx];

        switch (idx % 3)
        {
          case 0:
            {
              // Variation 1: Draw watermark as text string

              // Get an XGraphics object for drawing beneath the existing content
              XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend);

#if true_
            // Fill background with linear gradient color
            XRect rect = page.MediaBox.ToXRect();
            XLinearGradientBrush gbrush = new XLinearGradientBrush(rect,
              XColors.LightSalmon, XColors.WhiteSmoke, XLinearGradientMode.Vertical);
            gfx.DrawRectangle(gbrush, rect);
#endif

              // Get the size (in point) of the text
              XSize size = gfx.MeasureString(watermark, font);

              // Define a rotation transformation at the center of the page
              gfx.TranslateTransform(page.Width / 2, page.Height / 2);
              gfx.RotateTransform(-Math.Atan(page.Height / page.Width) * 180 / Math.PI);
              gfx.TranslateTransform(-page.Width / 2, -page.Height / 2);

              // Create a string format
              XStringFormat format = new XStringFormat();
              format.Alignment = XStringAlignment.Near;
              format.LineAlignment = XLineAlignment.Near;

              // Create a dimmed red brush
              XBrush brush = new XSolidBrush(XColor.FromArgb(128, 255, 0, 0));

              // Draw the string
              gfx.DrawString(watermark, font, brush,
                new XPoint((page.Width - size.Width) / 2, (page.Height - size.Height) / 2),
                format);
            }
            break;

          case 1:
            {
              // Variation 2: Draw watermark as outlined graphical path

              // Get an XGraphics object for drawing beneath the existing content
              XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Prepend);

              // Get the size (in point) of the text
              XSize size = gfx.MeasureString(watermark, font);

              // Define a rotation transformation at the center of the page
              gfx.TranslateTransform(page.Width / 2, page.Height / 2);
              gfx.RotateTransform(-Math.Atan(page.Height / page.Width) * 180 / Math.PI);
              gfx.TranslateTransform(-page.Width / 2, -page.Height / 2);

              // Create a graphical path
              XGraphicsPath path = new XGraphicsPath();

              // Add the text to the path
              path.AddString(watermark, font.FontFamily, XFontStyle.BoldItalic, 150,
                new XPoint((page.Width - size.Width) / 2, (page.Height - size.Height) / 2),
                XStringFormats.Default);

              // Create a dimmed red pen
              XPen pen = new XPen(XColor.FromArgb(128, 255, 0, 0), 2);

              // Stroke the outline of the path
              gfx.DrawPath(pen, path);
            }
            break;

          case 2:
            {
              // Variation 3: Draw watermark as transparent graphical path above text

              // Get an XGraphics object for drawing above the existing content
              XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

              // Get the size (in point) of the text
              XSize size = gfx.MeasureString(watermark, font);

              // Define a rotation transformation at the center of the page
              gfx.TranslateTransform(page.Width / 2, page.Height / 2);
              gfx.RotateTransform(-Math.Atan(page.Height / page.Width) * 180 / Math.PI);
              gfx.TranslateTransform(-page.Width / 2, -page.Height / 2);

              // Create a graphical path
              XGraphicsPath path = new XGraphicsPath();

              // Add the text to the path
              path.AddString(watermark, font.FontFamily, XFontStyle.BoldItalic, 150,
                new XPoint((page.Width - size.Width) / 2, (page.Height - size.Height) / 2),
                XStringFormats.Default);

              // Create a dimmed red pen and brush
              XPen pen = new XPen(XColor.FromArgb(50, 75, 0, 130), 3);
              XBrush brush = new XSolidBrush(XColor.FromArgb(50, 106, 90, 205));

              // Stroke the outline of the path
              gfx.DrawPath(pen, brush, path);
            }
            break;
        }
      }
      // Save the document...
      document.Save(filename);
      // ...and start a viewer
      Process.Start(filename);
    }
  }
}