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
using PdfSharp.Pdf.Printing;

namespace PrintPdfFile
{
  /// <summary>
  /// This sample shows how to print a PDF file automatically using Adobe Reader or Adobe Acrobat.
  /// PDFsharp cannot print PDF files, but you can use Adobe Reader with a command line switch to do
  /// the job.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // Set Acrobat Reader EXE, e.g.:
      //PdfPrinter.AdobeReaderPath = @"C:\Program Files\Adobe\Adobe Acrobat 7.0\Acrobat\Acrobat.exe";
      // -or-
      //PdfPrinter.AdobeReaderPath = @"C:\Program Files\Adobe\[...]\AcroRd32.exe";

      // On my computer (running Windows Vista 64) it is here:
      PdfFilePrinter.AdobeReaderPath = @"C:\Program Files (x86)\Adobe\Acrobat 8.0\Acrobat\Acrobat.exe";

      // Set the file to print and the Windows name of the printer.
      // At my home office I have an old Laserjet 6L under my desk.
      PdfFilePrinter printer = new PdfFilePrinter(@"..\..\..\..\..\PDFS\HelloWorld.pdf", "HP LaserJet 6L");

      try
      {
        printer.Print();
      }
      catch (Exception ex)
      {
        Console.WriteLine("Error: " + ex.Message);
      }
    }
  }
}