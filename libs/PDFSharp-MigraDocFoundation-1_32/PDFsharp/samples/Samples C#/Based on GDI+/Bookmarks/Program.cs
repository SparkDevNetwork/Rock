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
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Bookmarks
{
  /// <summary>
  /// This sample shows how to create bookmarks. Bookmarks are called outlines
  /// in the PDF reference manual, that's why you deal with the class PdfOutline.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // Create a new PDF document
      PdfDocument document = new PdfDocument();

      // Create a font
      XFont font = new XFont("Verdana", 16);

      // Create first page
      PdfPage page = document.AddPage();
      XGraphics gfx = XGraphics.FromPdfPage(page);
      gfx.DrawString("Page 1", font, XBrushes.Black, 20, 50, XStringFormats.Default);

      // Create the root bookmark. You can set the style and the color.
      PdfOutline outline = document.Outlines.Add("Root", page, true, PdfOutlineStyle.Bold, XColors.Red);

      // Create some more pages
      for (int idx = 2; idx <= 5; idx++)
      {
        page = document.AddPage();
        gfx = XGraphics.FromPdfPage(page);

        string text = "Page " + idx;
        gfx.DrawString(text, font, XBrushes.Black, 20, 50, XStringFormats.Default);

        // Create a sub bookmark
        outline.Outlines.Add(text, page, true);
      }

      // Save the document...
      const string filename = "Bookmarks_tempfile.pdf";
      document.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }
  }
}