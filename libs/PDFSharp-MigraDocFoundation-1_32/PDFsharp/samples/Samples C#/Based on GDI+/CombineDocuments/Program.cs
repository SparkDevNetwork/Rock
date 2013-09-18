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

namespace CombineDocuments
{
  /// <summary>
  /// This sample shows how to create a new document from two existing PDF files.
  /// The pages are inserted alternately from two external documents. This may be
  /// useful for visual comparison.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // NOTE: This samples opens the same document ('Portable Document Format.pdf') twice,
      // which leads to the same page on the left and the right. This is only because I
      // don't want to add too many sample PDF files to the 'PDFs' folder and keep the 
      // download of PDFsharp small. You should replace the two files with your own stuff.

      Variant1();
      Variant2();
    }

    /// <summary>
    /// Imports pages from an external document.
    /// Note that this technique imports the whole page including the hyperlinks.
    /// </summary>
    static void Variant1()
    {
      // Get two fresh copies of the sample PDF files
      // (Note: The input files are not modified in this sample.)
      const string filename1 = "Portable Document Format.pdf";
      File.Copy(Path.Combine("../../../../../PDFs/", filename1),
        Path.Combine(Directory.GetCurrentDirectory(), filename1), true);
      const string filename2 = "Portable Document Format.pdf";
      File.Copy(Path.Combine("../../../../../PDFs/", filename2),
        Path.Combine(Directory.GetCurrentDirectory(), filename2), true);

      // Open the input files
      PdfDocument inputDocument1 = PdfReader.Open(filename1, PdfDocumentOpenMode.Import);
      PdfDocument inputDocument2 = PdfReader.Open(filename2, PdfDocumentOpenMode.Import);

      // Create the output document
      PdfDocument outputDocument = new PdfDocument();

      // Show consecutive pages facing. Requires Acrobat 5 or higher.
      outputDocument.PageLayout = PdfPageLayout.TwoColumnLeft;

      XFont font = new XFont("Verdana", 10, XFontStyle.Bold);
      XStringFormat format = new XStringFormat();
      format.Alignment = XStringAlignment.Center;
      format.LineAlignment = XLineAlignment.Far;
      XGraphics gfx;
      XRect box;
      int count = Math.Max(inputDocument1.PageCount, inputDocument2.PageCount);
      for (int idx = 0; idx < count; idx++)
      {
        // Get page from 1st document
        PdfPage page1 = inputDocument1.PageCount > idx ?
          inputDocument1.Pages[idx] : new PdfPage();

        // Get page from 2nd document
        PdfPage page2 = inputDocument2.PageCount > idx ?
          inputDocument2.Pages[idx] : new PdfPage();

        // Add both pages to the output document
        page1 = outputDocument.AddPage(page1);
        page2 = outputDocument.AddPage(page2);

        // Write document file name and page number on each page
        gfx = XGraphics.FromPdfPage(page1);
        box = page1.MediaBox.ToXRect();
        box.Inflate(0, -10);
        gfx.DrawString(String.Format("{0} • {1}", filename1, idx + 1),
          font, XBrushes.Red, box, format);

        gfx = XGraphics.FromPdfPage(page2);
        box = page2.MediaBox.ToXRect();
        box.Inflate(0, -10);
        gfx.DrawString(String.Format("{0} • {1}", filename2, idx + 1),
          font, XBrushes.Red, box, format);
      }

      // Save the document...
      const string filename = "CompareDocument1_tempfile.pdf";
      outputDocument.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }

    /// <summary>
    /// Imports the pages as form X objects.
    /// Note that this technique copies only the visual content and the
    /// hyperlinks do not work.
    /// </summary>
    static void Variant2()
    {
      // Get fresh copies of the sample PDF files
      const string filename1 = "Portable Document Format.pdf";
      File.Copy(Path.Combine("../../../../../PDFs/", filename1),
        Path.Combine(Directory.GetCurrentDirectory(), filename1), true);
      const string filename2 = "Portable Document Format.pdf";
      File.Copy(Path.Combine("../../../../../PDFs/", filename2),
        Path.Combine(Directory.GetCurrentDirectory(), filename2), true);

      // Create the output document
      PdfDocument outputDocument = new PdfDocument();

      // Show consecutive pages facing
      outputDocument.PageLayout = PdfPageLayout.TwoPageLeft;

      XFont font = new XFont("Verdana", 10, XFontStyle.Bold);
      XStringFormat format = new XStringFormat();
      format.Alignment = XStringAlignment.Center;
      format.LineAlignment = XLineAlignment.Far;
      XGraphics gfx;
      XRect box;

      // Open the external documents as XPdfForm objects. Such objects are
      // treated like images. By default the first page of the document is
      // referenced by a new XPdfForm.
      XPdfForm form1 = XPdfForm.FromFile(filename1);
      XPdfForm form2 = XPdfForm.FromFile(filename2);

      int count = Math.Max(form1.PageCount, form2.PageCount);
      for (int idx = 0; idx < count; idx++)
      {
        // Add two new pages to the output document
        PdfPage page1 = outputDocument.AddPage();
        PdfPage page2 = outputDocument.AddPage();

        if (form1.PageCount > idx)
        {
          // Get a graphics object for page1
          gfx = XGraphics.FromPdfPage(page1);

          // Set page number (which is one-based)
          form1.PageNumber = idx + 1;

          // Draw the page identified by the page number like an image
          gfx.DrawImage(form1, new XRect(0, 0, form1.PointWidth, form1.PointHeight));

          // Write document file name and page number on each page
          box = page1.MediaBox.ToXRect();
          box.Inflate(0, -10);
          gfx.DrawString(String.Format("{0} • {1}", filename1, idx + 1),
            font, XBrushes.Red, box, format);
        }

        // Same as above for second page
        if (form2.PageCount > idx)
        {
          gfx = XGraphics.FromPdfPage(page2);

          form2.PageNumber = idx + 1;
          gfx.DrawImage(form2, new XRect(0, 0, form2.PointWidth, form2.PointHeight));

          box = page2.MediaBox.ToXRect();
          box.Inflate(0, -10);
          gfx.DrawString(String.Format("{0} • {1}", filename2, idx + 1),
            font, XBrushes.Red, box, format);
        }
      }

      // Save the document...
      const string filename = "CompareDocument2_tempfile.pdf";
      outputDocument.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }
  }
}
