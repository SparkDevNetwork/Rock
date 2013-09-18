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

namespace SplitDocument
{
  /// <summary>
  /// This sample shows how to convert a PDF document with n pages into
  /// n documents with one page each.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // Get a fresh copy of the sample PDF file
      const string filename = "Portable Document Format.pdf";
      File.Copy(Path.Combine("../../../../../PDFs/", filename), 
        Path.Combine(Directory.GetCurrentDirectory(), filename), true);

      // Open the file
      PdfDocument inputDocument = PdfReader.Open(filename, PdfDocumentOpenMode.Import);

      string name = Path.GetFileNameWithoutExtension(filename);
      for (int idx = 0; idx < inputDocument.PageCount; idx++)
      {
        // Create new document
        PdfDocument outputDocument = new PdfDocument();
        outputDocument.Version = inputDocument.Version;
        outputDocument.Info.Title =
          String.Format("Page {0} of {1}", idx + 1, inputDocument.Info.Title);
        outputDocument.Info.Creator = inputDocument.Info.Creator;

        // Add the page and save it
        outputDocument.AddPage(inputDocument.Pages[idx]);
        outputDocument.Save(String.Format("{0} - Page {1}_tempfile.pdf", name, idx + 1));
      }
    }
  }
}
