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
using System.Collections;
using System.Reflection;
using System.Text;
using System.IO;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf.Advanced
{
  /// <summary>
  /// Provides access to the internal document data structures. This class prevents the public
  /// interfaces from pollution with to much internal functions.
  /// </summary>
  public class PdfInternals  // TODO: PdfDocumentInternals... PdfPageInterals etc.
  {
    internal PdfInternals(PdfDocument document)
    {
      this.document = document;
    }
    PdfDocument document;

    /// <summary>
    /// Gets or sets the first document identifier.
    /// </summary>
    public string FirstDocumentID
    {
      get { return this.document.trailer.GetDocumentID(0); }
      set { this.document.trailer.SetDocumentID(0, value); }
    }

    /// <summary>
    /// Gets the first document identifier as GUID.
    /// </summary>
    public Guid FirstDocumentGuid
    {
      get { return GuidFromString(this.document.trailer.GetDocumentID(0)); }
    }

    /// <summary>
    /// Gets or sets the second document identifier.
    /// </summary>
    public string SecondDocumentID
    {
      get { return this.document.trailer.GetDocumentID(1); }
      set { this.document.trailer.SetDocumentID(1, value); }
    }

    /// <summary>
    /// Gets the first document identifier as GUID.
    /// </summary>
    public Guid SecondDocumentGuid
    {
      get { return GuidFromString(this.document.trailer.GetDocumentID(0)); }
    }

    Guid GuidFromString(string id)
    {
      if (id == null || id.Length != 16)
        return Guid.Empty;

      StringBuilder guid = new StringBuilder();
      for (int idx = 0; idx < 16; idx++)
        guid.AppendFormat("{0:X2}", (byte)id[idx]);

      return new Guid(guid.ToString());
    }

    /// <summary>
    /// Gets the catalog dictionary.
    /// </summary>
    public PdfCatalog Catalog
    {
      get { return this.document.Catalog; }
    }

    /// <summary>
    /// Returns the object with the specified Identifier, or null, if no such object exists.
    /// </summary>
    public PdfObject GetObject(PdfObjectID objectID)
    {
      return document.irefTable[objectID].Value;
    }

    /// <summary>
    /// Returns the PdfReference of the specified object, or null, if the object is not in the
    /// document's object table.
    /// </summary>
    public static PdfReference GetReference(PdfObject obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      return obj.Reference;
    }

    /// <summary>
    /// Gets the object identifier of the specified object.
    /// </summary>
    public static PdfObjectID GetObjectID(PdfObject obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      return obj.ObjectID;
    }

    /// <summary>
    /// Gets the object number of the specified object.
    /// </summary>
    public static int GetObjectNumber(PdfObject obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      return obj.ObjectNumber;
    }

    /// <summary>
    /// Gets the generation number of the specified object.
    /// </summary>
    public static int GenerationNumber(PdfObject obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      return obj.GenerationNumber;
    }

    /// <summary>
    /// Gets all indirect objects ordered by their object identifier.
    /// </summary>
    public PdfObject[] GetAllObjects()
    {
      PdfReference[] irefs = this.document.irefTable.AllReferences;
      int count = irefs.Length;
      PdfObject[] objects = new PdfObject[count];
      for (int idx = 0; idx < count; idx++)
        objects[idx] = irefs[idx].Value;
      return objects;
    }

    /// <summary>
    /// Gets all indirect objects ordered by their object identifier.
    /// </summary>
    [Obsolete("Use GetAllObjects.")]  // Properties should not return arrays
    public PdfObject[] AllObjects
    {
      get { return GetAllObjects(); }
    }

    /// <summary>
    /// Creates the indirect object of the specified type, adds it to the document, and
    /// returns the object.
    /// </summary>
    public T CreateIndirectObject<T>() where T : PdfObject
    {
      T result = null;
      ConstructorInfo ctorInfo = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.ExactBinding,
        null, new Type[] { typeof(PdfDocument) }, null);
      if (ctorInfo != null)
      {
        result = (T)ctorInfo.Invoke(new object[] { this.document });
        Debug.Assert(result != null);
        AddObject(result);
      }
      Debug.Assert(result != null, "CreateIndirectObject failed with type " + typeof(T).FullName);
      return result;
    }

    /// <summary>
    /// Adds an object to the PDF document. This operation and only this operation makes the object 
    /// an indirect object owned by this document.
    /// </summary>
    public void AddObject(PdfObject obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      if (obj.Owner == null)
        obj.Document = this.document;
      else if (obj.Owner != this.document)
        throw new InvalidOperationException("Object does not belong to this document.");
      this.document.irefTable.Add(obj);
    }

    /// <summary>
    /// Removes an object from the PDF document.
    /// </summary>
    public void RemoveObject(PdfObject obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      if (obj.Reference == null)
        throw new InvalidOperationException("Only indirect objects can be removed.");
      if (obj.Owner != this.document)
        throw new InvalidOperationException("Object does not belong to this document.");

      this.document.irefTable.Remove(obj.Reference);
    }

    /// <summary>
    /// Returns an array containing the specified object as first element follows by its transitive
    /// closure. The closure of an object are all objects that can be reached by indirect references. 
    /// The transitive closure is the result of applying the calculation of the closure to a closure
    /// as long as no new objects came along. This is e.g. useful for getting all objects belonging 
    /// to the resources of a page.
    /// </summary>
    public PdfObject[] GetClosure(PdfObject obj)
    {
      return GetClosure(obj, Int32.MaxValue);
    }

    /// <summary>
    /// Returns an array containing the specified object as first element follows by its transitive
    /// closure limited by the specified number of iterations.
    /// </summary>
    public PdfObject[] GetClosure(PdfObject obj, int depth)
    {
      PdfReference[] references = this.document.irefTable.TransitiveClosure(obj, depth);
      int count = references.Length + 1;
      PdfObject[] objects = new PdfObject[count];
      objects[0] = obj;
      for (int idx = 1; idx < count; idx++)
        objects[idx] = references[idx - 1].Value;
      return objects;
    }

    /// <summary>
    /// Writes a PdfItem into the specified stream.
    /// </summary>
    // This function exists to keep PdfWriter and PdfItem.WriteObject internal.
    public void WriteObject(Stream stream, PdfItem item)
    {
      // Never write an encrypted object
      PdfWriter writer = new PdfWriter(stream, null);
      writer.Options = PdfWriterOptions.OmitStream;
      item.WriteObject(writer);
    }

    /// <summary>
    /// The name of the custom value key.
    /// </summary>
    public string CustomValueKey = "/PdfSharp.CustomValue";
  }
}
