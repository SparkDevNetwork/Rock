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
using PdfSharp.Pdf.Security;

namespace UnprotectDocument
{
  /// <summary>
  /// This sample shows how to unprotect a document (if you know the password).
  /// Note that we will not explain nor give any tips how to crack a protected document with PDFsharp.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // Get a fresh copy of the sample PDF file.
      // The passwords are 'user' and 'owner' in this sample.
      const string filenameSource = "HelloWorld (protected).pdf";
      const string filenameDest = "HelloWorld_tempfile.pdf";
      File.Copy(Path.Combine("../../../../../PDFs/", filenameSource),
        Path.Combine(Directory.GetCurrentDirectory(), filenameDest), true);

      PdfDocument document;

      // Opening a document will fail with an invalid password.
      try
      {
        document = PdfReader.Open(filenameDest, "invalid password");
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
      }

      // You can specifiy a delegate, which is called if the document needs a
      // password. If you want to modify the document, you must provide the
      // owner password.
      document = PdfReader.Open(filenameDest, PdfDocumentOpenMode.Modify, PasswordProvider);

      // Open the document with the user password.
      document = PdfReader.Open(filenameDest, "user", PdfDocumentOpenMode.ReadOnly);

      // Use the property HasOwnerPermissions to decide whether the used password
      // was the user or the owner password. In both cases PDFsharp provides full
      // access to the PDF document. It is up to the programmer who uses PDFsharp
      // to honor the access rights. PDFsharp doesn't try to protect the document
      // because this make little sence for an open source library.
      bool hasOwnerAccess = document.SecuritySettings.HasOwnerPermissions;

      // Open the document with the owner password.
      document = PdfReader.Open(filenameDest, "owner");
      hasOwnerAccess = document.SecuritySettings.HasOwnerPermissions;

      // A document opened with the owner password is completely unprotected
      // and can be modified.
      XGraphics gfx = XGraphics.FromPdfPage(document.Pages[0]);
      gfx.DrawString("Some text...",
        new XFont("Times New Roman", 12), XBrushes.Firebrick, 50, 100);

      // The modified document is saved without any protection applied.
      PdfDocumentSecurityLevel level = document.SecuritySettings.DocumentSecurityLevel;

      // If you want to save it protected, you must set the DocumentSecurityLevel
      // or apply new passwords.
      // In the current implementation the old passwords are not automatically
      // reused. See 'ProtectDocument' sample for further information.

      // Save the document...
      document.Save(filenameDest);
      // ...and start a viewer.
      Process.Start(filenameDest);
    }

    /// <summary>
    /// The 'get the password' call back function.
    /// </summary>
    static void PasswordProvider(PdfPasswordProviderArgs args)
    {
      // Show a dialog here in a real application
      args.Password = "owner";
    }
  }
}