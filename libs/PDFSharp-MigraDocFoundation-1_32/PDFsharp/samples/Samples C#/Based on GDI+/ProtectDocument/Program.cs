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
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Security;

namespace ProtectDocument
{
  /// <summary>
  /// This sample shows how to protect a document with a password.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // Get a fresh copy of the sample PDF file
      const string filenameSource = "HelloWorld.pdf";
      const string filenameDest = "HelloWorld_tempfile.pdf";
      File.Copy(Path.Combine("../../../../../PDFs/", filenameSource), 
        Path.Combine(Directory.GetCurrentDirectory(), filenameDest), true);

      // Open an existing document. Providing an unrequired password is ignored.
      PdfDocument document = PdfReader.Open(filenameDest, "some text");

      PdfSecuritySettings securitySettings = document.SecuritySettings;

      // Setting one of the passwords automatically sets the security level to 
      // PdfDocumentSecurityLevel.Encrypted128Bit.
      securitySettings.UserPassword  = "user";
      securitySettings.OwnerPassword = "owner";

      // Don't use 40 bit encryption unless needed for compatibility
      //securitySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted40Bit;

      // Restrict some rights.
      securitySettings.PermitAccessibilityExtractContent = false;
      securitySettings.PermitAnnotations = false;
      securitySettings.PermitAssembleDocument = false;
      securitySettings.PermitExtractContent = false;
      securitySettings.PermitFormsFill = true;
      securitySettings.PermitFullQualityPrint = false;
      securitySettings.PermitModifyDocument = true;
      securitySettings.PermitPrint = false;

      // Save the document...
      document.Save(filenameDest);
      // ...and start a viewer.
      Process.Start(filenameDest);
    }
  }
}