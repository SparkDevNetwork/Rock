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
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.IO;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Drawing;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Internal;

namespace PdfSharp.Pdf.Advanced
{
  /// <summary>
  /// Represents the imported objects of an external document. Used to cache objects that are
  /// already imported when a PdfFormXObject is added to a page.
  /// </summary>
  internal sealed class PdfImportedObjectTable
  {
    /// <summary>
    /// Initializes a new instance of this class with the document the objects are imported from.
    /// </summary>
    public PdfImportedObjectTable(PdfDocument owner, PdfDocument externalDocument)
    {
      if (owner == null)
        throw new ArgumentNullException("owner");
      if (externalDocument == null)
        throw new ArgumentNullException("externalDocument");
      this.owner = owner;
      this.externalDocumentHandle = externalDocument.Handle;
      this.xObjects = new PdfFormXObject[externalDocument.PageCount];
    }
    PdfFormXObject[] xObjects;

    /// <summary>
    /// Gets the document this table belongs to.
    /// </summary>
    public PdfDocument Owner
    {
      get { return this.owner; }
    }
    PdfDocument owner;

    /// <summary>
    /// Gets the external document, or null, if the external document is garbage collected.
    /// </summary>
    public PdfDocument ExternalDocument
    {
      get 
      {
        if (this.externalDocumentHandle.IsAlive)
          return this.externalDocumentHandle.Target;
        return null; 
      }
    }
    PdfDocument.DocumentHandle externalDocumentHandle;

    public PdfFormXObject GetXObject(int pageNumber)
    {
      return this.xObjects[pageNumber - 1];
    }

    public void SetXObject(int pageNumber, PdfFormXObject xObject)
    {
      this.xObjects[pageNumber - 1] = xObject;
    }

    /// <summary>
    /// Indicates whether the specified object is already imported.
    /// </summary>
    public bool Contains(PdfObjectID externalID)
    {
      return this.externalIDs.ContainsKey(externalID.ToString());
    }

    /// <summary>
    /// Adds a cloned object to this table.
    /// </summary>
    /// <param name="externalID">The object identifier in the foreign object.</param>
    /// <param name="iref">The cross reference to the clone of the foreign object, which belongs to
    /// this document. In general the clone has a different object identifier.</param>
    public void Add(PdfObjectID externalID, PdfReference iref)
    {
      this.externalIDs[externalID.ToString()] = iref;
    }

    /// <summary>
    /// Gets the cloned object that corresponds to the specified external identifier.
    /// </summary>
    public PdfReference this[PdfObjectID externalID]
    {
      get { return (PdfReference)this.externalIDs[externalID.ToString()]; }
    }

    /// <summary>
    /// Maps external object identifiers to cross reference entries of the importing document
    /// {PdfObjectID -> PdfReference}.
    /// </summary>
    Dictionary<string, PdfReference> externalIDs = new Dictionary<string, PdfReference>();
  }
}
