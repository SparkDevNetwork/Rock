#region PDFsharp - A .NET library for processing PDF
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
//
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT OF THIRD PARTY RIGHTS.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
// USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;

namespace ReadFormFields
{
  // Note: AcroForms are still under construction in this version and some functionality may not work
  // as expected.

  /// <summary>
  /// This sample shows how to read values from a filled interactive form (AcroForm).
  /// </summary>
  class Program
  {
    [STAThread]
    static void Main(string[] args)
    {
      // Set the name of the filled file
      string filename;
      //filename = "fw4 (Acrobat 4).pdf";
      //filename = "fw4 (Acrobat 5).pdf";
      //filename = "fw4 (Acrobat 6).pdf";
      filename = "fw4 (Acrobat 7).pdf";
      filename = Path.Combine("../../FilledForms", filename);

      // Open the file
      PdfDocument document = PdfReader.Open(filename, PdfDocumentOpenMode.ReadOnly);

      // Get the root object of all interactive form fields
      PdfAcroForm form = document.AcroForm;

      // Get all form fields of the whole document
      PdfAcroField.PdfAcroFieldCollection fields = form.Fields;

      // Get all form fields of the whole document
      string[] names = fields.Names;
      names = fields.DescendantNames;

      // Read the value of each field
      for (int idx = 0; idx < names.Length; idx++)
      {
        string fqName = names[idx];
        PdfAcroField field = fields[fqName];

        PdfTextField txtField;
        PdfRadioButtonField radField;
        PdfCheckBoxField chkField;
        PdfListBoxField lbxField;
        PdfComboBoxField cbxField;
        PdfGenericField genField;

        if ((txtField = field as PdfTextField) != null)
        {
          Console.WriteLine(String.Format("text field '{0}': '{1}'", txtField.Name, txtField.Text));
        }
        else if ((radField = field as PdfRadioButtonField) != null)
        {
          Console.WriteLine(String.Format("radio button '{0}': '{1}'", radField.Name, radField.Value));
        }
        else if ((chkField = field as PdfCheckBoxField) != null)
        {
          Console.WriteLine(String.Format("radio button '{0}': '{1}'", chkField.Name, chkField.Value));
        }
        else if ((lbxField = field as PdfListBoxField) != null)
        {
          Console.WriteLine(String.Format("radio button '{0}': '{1}'", lbxField.Name, lbxField.Value));
        }
        else if ((cbxField = field as PdfComboBoxField) != null)
        {
          Console.WriteLine(String.Format("radio button '{0}': '{1}'", cbxField.Name, cbxField.Value));
        }
        else if ((genField = field as PdfGenericField) != null)
        {
          Console.WriteLine(String.Format("radio button '{0}': '{1}'", genField.Name, genField.Value));
        }
      }

      // Wait for Return
      Console.ReadLine();
    }
  }
}
