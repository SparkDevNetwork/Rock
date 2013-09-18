#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Stefan Lange (mailto:Stefan.Lange@pdfsharp.com)
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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Fonts.OpenType;

namespace PdfSharp.Pdf.Advanced
{
  /// <summary>
  /// Contains all external PDF files from which PdfFormXObjects are imported into the current document.
  /// </summary>
  internal sealed class PdfFormXObjectTable : PdfResourceTable
  {
    // The name PdfFormXObjectTable is technically not correct, because in contrast to PdfFontTable
    // or PdfImageTable this class holds no PdfFormXObject objects. Actually it holds instances of
    // the class ImportedObjectTable, one for each external document. The PdfFormXObject instances
    // are not cached, because they hold a transformation matrix that make them unique. If the user
    // wants to use a particual page of a PdfFormXObject more than once, he must reuse the object
    // before he changes the PageNumber or the transformation matrix. In other words this class
    // caches the indirect objects of an external form, not the form itself.

    /// <summary>
    /// Initializes a new instance of this class, which is a singleton for each document.
    /// </summary>
    public PdfFormXObjectTable(PdfDocument document)
      : base(document)
    { }

    /// <summary>
    /// Gets a PdfFormXObject from an XPdfForm. Because the returned objects must be unique, always
    /// a new instance of PdfFormXObject is created if none exists for the specified form. 
    /// </summary>
    public PdfFormXObject GetForm(XForm form)
    {
      // If the form already has a PdfFormXObject, return it.
      if (form.pdfForm != null)
      {
        Debug.Assert(form.IsTemplate, "An XPdfForm must not have a PdfFormXObject.");
        if (object.ReferenceEquals(form.pdfForm.Owner, this.owner))
          return form.pdfForm;
        //throw new InvalidOperationException("Because of a current limitation of PDFsharp an XPdfForm object can be used only within one single PdfDocument.");

        // Dispose PdfFromXObject when document has changed
        form.pdfForm = null;
      }

      XPdfForm pdfForm = form as XPdfForm;
      if (pdfForm != null)
      {
        // Is the external PDF file from which is imported already known for the current document?
        Selector selector = new Selector(form);
        PdfImportedObjectTable importedObjectTable;
        if (!this.forms.TryGetValue(selector, out importedObjectTable))
        {
          // No: Get the external document from the form and create ImportedObjectTable.
          PdfDocument doc = pdfForm.ExternalDocument;
          importedObjectTable = new PdfImportedObjectTable(this.owner, doc);
          this.forms[selector] = importedObjectTable;
        }

        PdfFormXObject xObject = importedObjectTable.GetXObject(pdfForm.PageNumber);
        if (xObject == null)
        {
          xObject = new PdfFormXObject(this.owner, importedObjectTable, pdfForm);
          importedObjectTable.SetXObject(pdfForm.PageNumber, xObject);
        }
        return xObject;
      }
      Debug.Assert(form.GetType() == typeof(XForm));
      form.pdfForm = new PdfFormXObject(this.owner, form);
      return form.pdfForm;
    }

    /// <summary>
    /// Gets the imported object table.
    /// </summary>
    public PdfImportedObjectTable GetImportedObjectTable(PdfPage page)
    {
      // Is the external PDF file from which is imported already known for the current document?
      Selector selector = new Selector(page);
      PdfImportedObjectTable importedObjectTable;
      if (!this.forms.TryGetValue(selector, out importedObjectTable))
      {
        importedObjectTable = new PdfImportedObjectTable(this.owner, page.Owner);
        this.forms[selector] = importedObjectTable;
      }
      return importedObjectTable;
    }

    public void DetachDocument(PdfDocument.DocumentHandle handle)
    {
      if (handle.IsAlive)
      {
        foreach (Selector selector in this.forms.Keys)
        {
          PdfImportedObjectTable table = (PdfImportedObjectTable)this.forms[selector];
          if (table.ExternalDocument != null && table.ExternalDocument.Handle == handle)
          {
            this.forms.Remove(selector);
            break;
          }
        }
      }

      // Clean table
      bool itemRemoved = true;
      while (itemRemoved)
      {
        itemRemoved = false;
        foreach (Selector selector in this.forms.Keys)
        {
          PdfImportedObjectTable table = this.forms[selector];
          if (table.ExternalDocument == null)
          {
            this.forms.Remove(selector);
            itemRemoved = true;
            break;
          }
        }
      }
    }

    /// <summary>
    /// Map from Selector to PdfImportedObjectTable.
    /// </summary>
    readonly Dictionary<Selector, PdfImportedObjectTable> forms = new Dictionary<Selector, PdfImportedObjectTable>();

    /// <summary>
    /// A collection of information that uniquely identifies a particular ImportedObjectTable.
    /// </summary>
    public class Selector
    {
      /// <summary>
      /// Initializes a new instance of FormSelector from an XPdfForm.
      /// </summary>
      public Selector(XForm form)
      {
        // HACK: just use full path to identify
        this.path = form.path.ToLower(CultureInfo.InvariantCulture);
      }

      /// <summary>
      /// Initializes a new instance of FormSelector from a PdfPage.
      /// </summary>
      public Selector(PdfPage page)
      {
        PdfDocument owner = page.Owner;
        //string path = owner.FullPath;
        //if (path.Length == 0)
        path = "*" + owner.Guid.ToString("B");

        this.path = path.ToLower(CultureInfo.InvariantCulture);
      }

      public string Path
      {
        get { return this.path; }
        set { this.path = value; }
      }
      string path;

      public override bool Equals(object obj)
      {
        Selector selector = obj as Selector;
        if (obj == null)
          return false;
        return this.path == selector.path; ;
      }

      public override int GetHashCode()
      {
        return this.path.GetHashCode();
      }
    }
  }
}
