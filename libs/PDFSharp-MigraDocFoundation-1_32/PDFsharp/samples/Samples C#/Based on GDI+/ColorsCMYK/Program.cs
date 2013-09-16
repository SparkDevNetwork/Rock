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

namespace ColorsCMYK
{
  /// <summary>
  /// This sample shows how to use CMYK colors.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // Get a fresh copy of the sample PDF file
      const string filenameSource = "SomeLayout.pdf";
      const string filenameDest = "ColorsCMYK_tempfile.pdf";
      File.Copy(Path.Combine("../../../../../PDFs/", filenameSource),
        Path.Combine(Directory.GetCurrentDirectory(), filenameDest), true);

      // Create the font for drawing the watermark
      //XFont font = new XFont("Times", emSize, XFontStyle.BoldItalic);

      // Open an existing document for editing and draw on first page
      PdfDocument document = PdfReader.Open(filenameDest);
      document.Options.ColorMode = PdfColorMode.Cmyk;

      // Set version to PDF 1.4 (Acrobat 5) because we use transparency.
      if (document.Version < 14)
        document.Version = 14;

      PdfPage page = document.Pages[0];

      // Get an XGraphics object for drawing beneath the existing content

      XGraphics gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);

      gfx.DrawRectangle(new XSolidBrush(XColor.FromCmyk(1, 0.68, 0, 0.12)), new XRect(30, 60, 50, 50));
      gfx.DrawRectangle(new XSolidBrush(XColor.FromCmyk(0, 0.70, 1, 0)), new XRect(550, 60, 50, 50));

      gfx.DrawRectangle(new XSolidBrush(XColor.FromCmyk(0, 0, 0, 0)), new XRect(90, 100, 50, 50));
      gfx.DrawRectangle(new XSolidBrush(XColor.FromCmyk(0, 0, 0, 0)), new XRect(150, 100, 50, 50));

      gfx.DrawRectangle(new XSolidBrush(XColor.FromCmyk(0.7, 0, 0.70, 1, 0)), new XRect(90, 100, 50, 50));
      gfx.DrawRectangle(new XSolidBrush(XColor.FromCmyk(0.5, 0, 0.70, 1, 0)), new XRect(150, 100, 50, 50));

      gfx.DrawRectangle(new XSolidBrush(XColor.FromCmyk(0.35, 0.15, 0, 0.08)), new XRect(50, 360, 50, 50));
      gfx.DrawRectangle(new XSolidBrush(XColor.FromCmyk(0.25, 0.10, 0, 0.05)), new XRect(150, 360, 50, 50));
      gfx.DrawRectangle(new XSolidBrush(XColor.FromCmyk(0.15, 0.05, 0, 0)), new XRect(250, 360, 50, 50));

      // Save the document...
      document.Save(filenameDest);
      // ...and start a viewer
      Process.Start(filenameDest);
    }
  }
}