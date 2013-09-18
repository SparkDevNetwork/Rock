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

namespace HelloWorld
{
  /// <summary>
  /// This sample shows a document with different page sizes.
  /// Note: You can set the size of a page to any size using the Width and
  /// Height properties. This sample just shows the predefined sizes.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // Create a new PDF document
      PdfDocument document = new PdfDocument();

      // Create a font
      XFont font = new XFont("Times", 25, XFontStyle.Bold);

      PageSize[] pageSizes = (PageSize[])Enum.GetValues(typeof(PageSize));
      foreach (PageSize pageSize in pageSizes)
      {
        if (pageSize == PageSize.Undefined)
          continue;

        // One page in Portrait...
        PdfPage page = document.AddPage();
        page.Size = pageSize;
        XGraphics gfx = XGraphics.FromPdfPage(page);
        gfx.DrawString(pageSize.ToString(), font, XBrushes.DarkRed,
          new XRect(0, 0, page.Width, page.Height),
          XStringFormats.Center);

        // ... and one in Landscape orientation.
        page = document.AddPage();
        page.Size = pageSize;
        page.Orientation = PageOrientation.Landscape;
        gfx = XGraphics.FromPdfPage(page);
        gfx.DrawString(pageSize + " (landscape)", font,
          XBrushes.DarkRed, new XRect(0, 0, page.Width, page.Height),
          XStringFormats.Center);
      }

      // Save the document...
      const string filename = "PageSizes_tempfile.pdf";
      document.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }
  }
}