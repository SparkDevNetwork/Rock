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
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.AcroForms;
using PdfSharp.Pdf.IO;

namespace FillFormFields
{
  // Note: AcroForms are still under construction in this version. Filling form fields is more obscure than
  // I expected. The implementation is partially undone.

  /// <summary>
  /// This sample shows how to fill an interactive form (AcroForm).
  /// </summary>
  class Program
  {
    [STAThread]
    static void Main(string[] args)
    {
      // Get a fresh copy of a sample PDF form file
      string filename;
      filename = "fw4.pdf";
      //filename = "fss4.pdf";
      //filename = "fw9.pdf";
      //filename = "f8822.pdf";
      
      while (!File.Exists(Path.Combine("../../../../PDFs", filename)))
      {
        if (DialogResult.Yes ==
          MessageBox.Show("This sample needs a PDF form that is not included with this release.\n" +
          "Please download the Form W-4 from http://www.irs.gov" +
          "\nSelect Yes to open this URL in your browser.\n" +
          "Select No to exit this sample now.", 
          "PDFsharp FillFormFields sample",
          MessageBoxButtons.YesNo,
          MessageBoxIcon.Question))
        {
          Process.Start("../../SampleForms/ReadMe.txt");
          Process.Start("http://www.irs.gov/");
          MessageBox.Show("Close this dialog to continue the sample after downloading the W-4 form to the appropriate folder.",
            "PDFsharp FillFormFields sample");
        }
        else
          return;
      }
 
      File.Copy(Path.Combine("../../../../PDFs", filename), 
        Path.Combine(Directory.GetCurrentDirectory(), filename), true);

      // Open the file
      PdfDocument document = PdfReader.Open(filename, PdfDocumentOpenMode.Modify);

      // Get the root object of all interactive form fields
      PdfAcroForm form = document.AcroForm;

      // Get all form fields of the whole document
      PdfAcroField.PdfAcroFieldCollection fields = form.Fields;

      // Get all form fields of the whole document
      string[] names = fields.Names;
      names = fields.DescendantNames;

      // Fill some value in each field
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
          // 
          txtField.Text = "Hello";
        }
        else if ((radField = field as PdfRadioButtonField) != null)
        {
          radField.SelectedIndex = 0;
        }
        else if ((chkField = field as PdfCheckBoxField) != null)
        {
          chkField.Checked = idx % 2 == 0;
        }
        else if ((lbxField = field as PdfListBoxField) != null)
        {
          lbxField.SelectedIndex = 0;
        }
        else if ((cbxField = field as PdfComboBoxField) != null)
        {
          cbxField.SelectedIndex = 0;
        }
        else if ((genField = field as PdfGenericField) != null)
        {
        }
      }

      // Save the document...
      document.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }
  }
}
