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

// By courtesy of Peter Berndts 

namespace Booklet
{
  /// <summary>
  /// This sample shows how to produce a booklet by placing
  /// two pages of an existing document on
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
      // (Note: one page contains two pages from the source document.
      //  If the number of pages of the source document can not be
      //  divided by 4, the first pages of the output document will
      //  each contain only one page from the source document.)
      outputDocument.PageLayout = PdfPageLayout.SinglePage;

      XGraphics gfx;

      // Open the external document as XPdfForm object
      XPdfForm form = XPdfForm.FromFile(filename);
      // Determine width and height
      double extWidth = form.PixelWidth;
      double extHeight = form.PixelHeight;

      int inputPages = form.PageCount;
      int sheets = inputPages / 4;
      if (sheets * 4 < inputPages)
        sheets += 1;
      int allpages = 4 * sheets;
      int vacats = allpages - inputPages;


      for (int idx = 1; idx <= sheets; idx += 1)
      {
        // Front page of a sheet:
        // Add a new page to the output document
        PdfPage page = outputDocument.AddPage();
        page.Orientation = PageOrientation.Landscape;
        page.Width = 2 * extWidth;
        page.Height = extHeight;
        double width = page.Width;
        double height = page.Height;

        gfx = XGraphics.FromPdfPage(page);

        // Skip if left side has to remain blank
        XRect box;
        if (vacats > 0)
          vacats -= 1;
        else
        {
          // Set page number (which is one-based) for left side
          form.PageNumber = allpages + 2 * (1 - idx);
          box = new XRect(0, 0, width / 2, height);
          // Draw the page identified by the page number like an image
          gfx.DrawImage(form, box);
        }

        // Set page number (which is one-based) for right side
        form.PageNumber = 2 * idx - 1;
        box = new XRect(width / 2, 0, width / 2, height);
        // Draw the page identified by the page number like an image
        gfx.DrawImage(form, box);

        // Back page of a sheet
        page = outputDocument.AddPage();
        page.Orientation = PageOrientation.Landscape;
        page.Width = 2 * extWidth;
        page.Height = extHeight;

        gfx = XGraphics.FromPdfPage(page);

        // Set page number (which is one-based) for left side
        form.PageNumber = 2 * idx;
        box = new XRect(0, 0, width / 2, height);
        // Draw the page identified by the page number like an image
        gfx.DrawImage(form, box);

        // Skip if right side has to remain blank
        if (vacats > 0)
          vacats -= 1;
        else
        {
          // Set page number (which is one-based) for right side
          form.PageNumber = allpages + 1 - 2 * idx;
          box = new XRect(width / 2, 0, width / 2, height);
          // Draw the page identified by the page number like an image
          gfx.DrawImage(form, box);
        }
      }

      // Save the document...
      filename = "Booklet.pdf";
      outputDocument.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }
  }
}
