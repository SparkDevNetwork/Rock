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
//using System.Drawing;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Graphics
{
  /// <summary>
  /// This sample shows some of the capabilities of the XGraphcis class.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // Create a temporary file
      string filename = String.Format("{0}_tempfile.pdf", Guid.NewGuid().ToString("D").ToUpper());
      s_document = new PdfDocument();
      s_document.Info.Title = "PDFsharp XGraphic Sample";
      s_document.Info.Author = "Stefan Lange";
      s_document.Info.Subject = "Created with code snippets that show the use of graphical functions";
      s_document.Info.Keywords = "PDFsharp, XGraphics";

      // Create demonstration pages
      new LinesAndCurves().DrawPage(s_document.AddPage());
      new Shapes().DrawPage(s_document.AddPage());
      new Paths().DrawPage(s_document.AddPage());
      new Text().DrawPage(s_document.AddPage());
      new Images().DrawPage(s_document.AddPage());

      // Save the s_document...
      s_document.Save(filename);
      // ...and start a viewer
      Process.Start(filename);
    }

    internal static PdfDocument s_document;
  }
}
