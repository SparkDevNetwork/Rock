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
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;

namespace TwoPagesOnOne
{
  /// <summary>
  /// This sample shows how to place two pages of an existing document on
  /// one landscape orientated page of a new document.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // Get a fresh copy of the sample PDF file
      string filename = "Portable Document Format.pdf";
      File.Copy(Path.Combine("../../../../../PDFs/", filename), 
        Path.Combine(Directory.GetCurrentDirectory(), filename), true);

      // Create the output document
      PdfDocument outputDocument = new PdfDocument();

      // Show single pages
      // (Note: one page contains two pages from the source document)
      outputDocument.PageLayout = PdfPageLayout.SinglePage;

      XFont font = new XFont("Verdana", 8, XFontStyle.Bold);
      XStringFormat format = new XStringFormat();
      format.Alignment = XStringAlignment.Center;
      format.LineAlignment = XLineAlignment.Far;
      XGraphics gfx;
      XRect box;

      // Open the external document as XPdfForm object
      XPdfForm form = XPdfForm.FromFile(filename);

      for (int idx = 0; idx < form.PageCount; idx += 2)
      {
        // Add a new page to the output document
        PdfPage page = outputDocument.AddPage();
        page.Orientation = PageOrientation.Landscape;
        double width  = page.Width;
        double height = page.Height;

        int rotate = page.Elements.GetInteger("/Rotate");

        gfx = XGraphics.FromPdfPage(page);

        // Set page number (which is one-based)
        form.PageNumber = idx + 1;

        box = new XRect(0, 0, width / 2, height);
        // Draw the page identified by the page number like an image
        gfx.DrawImage(form, box);

        // Write document file name and page number on each page
        box.Inflate(0, -10);
        gfx.DrawString(String.Format("- {1} -", filename, idx + 1),
          font, XBrushes.Red, box, format);

        if (idx + 1 < form.PageCount)
        {
          // Set page number (which is one-based)
          form.PageNumber = idx + 2;

          box = new XRect(width / 2, 0, width / 2, height);
          // Draw the page identified by the page number like an image
          gfx.DrawImage(form, box);

          // Write document file name and page number on each page
          box.Inflate(0, -10);
          gfx.DrawString(String.Format("- {1} -", filename, idx + 2),
            font, XBrushes.Red, box, format);
        }
      }

      // Save the document...
      filename = "TwoPagesOnOne_tempfile.pdf";
      outputDocument.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }
  }
}
