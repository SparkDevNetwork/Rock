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
using System.Globalization;
using System.Text;
using System.IO;
using PdfSharp.Internal;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Base class of all composite PDF objects.
  /// </summary>
  public abstract class PdfObject : PdfItem
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfObject"/> class.
    /// </summary>
    protected PdfObject()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfObject"/> class.
    /// </summary>
    protected PdfObject(PdfDocument document)
    {
      Document = document;
    }

    /// <summary>
    /// Initializes a new instance from an existing object. Used for object type transformation.
    /// </summary>
    protected PdfObject(PdfObject obj)
    {
      Document = obj.Owner;
      // If the object that was transformed to an instance of a derived class was an indirect object
      // set the value of the reference to this.
      if (obj.iref != null)
        obj.iref.Value = this;
#if DEBUG_
      else
      {
        // If this occurs it is an internal error
        Debug.Assert(false, "Object type transformation must not be done with direct objects");
      }
#endif
    }

    /// <summary>
    /// Creates a copy of this object. The clone does not belong to a document, i.e. its owner and its iref are null.
    /// </summary>
    public new PdfObject Clone()
    {
      return (PdfObject)Copy();
    }

    /// <summary>
    /// Implements the copy mechanism. Must be overridden in derived classes.
    /// </summary>
    protected override object Copy()
    {
      PdfObject obj = (PdfObject)base.Copy();
      obj.document = null;
      obj.iref = null;
      return obj;
    }

#if true_  // works, but may lead to other problems that I cannot assess
    /// <summary>
    /// Determines whether the specified object is equal to the current PdfObject.
    /// </summary>
    public override bool Equals(object obj)
    {
      if (obj is PdfObject)
      {
        PdfObject other = (PdfObject)obj;
        // Take object type transformation into account
        if (this.iref != null && other.iref != null)
        {
          Debug.Assert(this.iref.Value != null, "iref without value.");
          Debug.Assert(other.iref.Value != null, "iref without value.");
          return Object.ReferenceEquals(this.iref.Value, other.iref.Value);
        }
      }
      return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      if (this.iref != null)
      {
        Debug.Assert(this.iref.Value != null, "iref without value.");
        return this.iref.GetHashCode();
      }
      return base.GetHashCode();
    }
#endif

    /// <summary>
    /// Sets the object and generation number
    /// Setting the object identifier makes this object an indirect object, i.e. the object gets
    /// a PdfReference entry in the PdfReferenceTable.
    /// </summary>
    internal void SetObjectID(int objectNumber, int generationNumber)
    {
      PdfObjectID objectID = new PdfObjectID(objectNumber, generationNumber);

      // TODO: check imported
      if (this.iref == null)
        this.iref = this.document.irefTable[objectID];
      if (this.iref == null)
      {
        this.iref = new PdfReference(this);
        this.iref.ObjectID = objectID;
      }
      this.iref.Value = this;
      this.iref.Document = this.document;
    }

    //internal void SetObjectID2(PdfObjectID objectID)
    //{
    //  if (this.iref == null)
    //    this.iref = this.document.irefTable[objectID];
    //  //this.iref = new PdfReference(this);
    //  //this.iref.ObjectID = objectID;
    //  this.iref.Value = this;
    //}

    /// <summary>
    /// Gets the PdfDocument this object belongs to.
    /// </summary>
    public virtual PdfDocument Owner
    {
      get { return this.document; }
    }

    /// <summary>
    /// Gets or sets the PdfDocument this object belongs to.
    /// </summary>
    internal virtual PdfDocument Document
    {
      set
      {
        if (!ReferenceEquals(this.document, value))
        {
          if (this.document != null)
            throw new InvalidOperationException("Cannot change document.");
          this.document = value;
          if (this.iref != null)
            this.iref.Document = value;
        }
      }
    }
    internal PdfDocument document;

    /// <summary>
    /// Indicates whether the object is an indirect object.
    /// </summary>
    public bool IsIndirect
    {
      // An object is an indirect object if and only if is has an indirect reference value.
      get { return this.iref != null; }
    }

    /// <summary>
    /// Gets the PdfInternals object of this document, that grants access to some internal structures
    /// which are not part of the public interface of PdfDocument.
    /// </summary>
    public PdfObjectInternals Internals
    {
      get
      {
        if (this.internals == null)
          this.internals = new PdfObjectInternals(this);
        return this.internals;
      }
    }
    PdfObjectInternals internals;

    /// <summary>
    /// When overridden in a derived class, prepares the object to get saved.
    /// </summary>
    internal virtual void PrepareForSave()
    {
    }

    /// <summary>
    /// Saves the stream position. 2nd Edition.
    /// </summary>
    internal override void WriteObject(PdfWriter writer)
    {
      Debug.Assert(false, "Must not come here!");
      //      Debug.Assert(this.inStreamOffset <= 0);
      //      if (this.inStreamOffset == 0)
      //      {
      //        //this.InStreamOffset = stream.Position;
      //        this.document.xrefTable.AddObject(this);
      //        return Format("{0} {1} obj\n", this.objectID, this.generation);
      //      }
      //      else if (this.inStreamOffset == -1)
      //      {
      //
      //      }
      //      return null;
      //
    }

    /// <summary>
    /// Gets the object identifier. Returns PdfObjectID.Empty for direct objects.
    /// </summary>
    internal PdfObjectID ObjectID
    {
      get { return this.iref != null ? this.iref.ObjectID : PdfObjectID.Empty; }
    }

    /// <summary>
    /// Gets the object number.
    /// </summary>
    internal int ObjectNumber
    {
      get { return ObjectID.ObjectNumber; }
    }

    /// <summary>
    /// Gets the generation number.
    /// </summary>
    internal int GenerationNumber
    {
      get { return ObjectID.GenerationNumber; }
    }

    ///// <summary>
    ///// Creates a deep copy of the specified value and its transitive closure and adds the
    ///// new objects to the specified owner document.
    ///// </summary>
    /// <param name="owner">The document that owns the cloned objects.</param>
    /// <param name="externalObject">The root object to be cloned.</param>
    /// <returns>The clone of the root object</returns>
    internal static PdfObject DeepCopyClosure(PdfDocument owner, PdfObject externalObject)
    {
      // Get transitive closure
      PdfObject[] elements = externalObject.Owner.Internals.GetClosure(externalObject);
      int count = elements.Length;
#if DEBUG_
        for (int idx = 0; idx < count; idx++)
        {
          Debug.Assert(elements[idx].XRef != null);
          Debug.Assert(elements[idx].XRef.Document != null);
          Debug.Assert(elements[idx].Document != null);
          if (elements[idx].ObjectID.ObjectNumber == 12)
            GetType();
        }
#endif
      // 1st loop. Replace all objects by their clones.
      PdfImportedObjectTable iot = new PdfImportedObjectTable(owner, externalObject.Owner);
      for (int idx = 0; idx < count; idx++)
      {
        PdfObject obj = elements[idx];
        PdfObject clone = obj.Clone();
        Debug.Assert(clone.Reference == null);
        clone.Document = owner;
        if (obj.Reference != null)
        {
          // Case: The cloned object was an indirect object.
          // add clone to new owner document
          owner.irefTable.Add(clone);
          // the clone gets an iref by adding it to its new owner
          Debug.Assert(clone.Reference != null);
          // save an association from old object identifier to new iref
          iot.Add(obj.ObjectID, clone.Reference);
        }
        else
        {
          // Case: The cloned object was an direct object.
          // only the root object can be a direct object
          Debug.Assert(idx == 0);
        }
        // replace external object by its clone
        elements[idx] = clone;
      }
#if DEBUG_
        for (int idx = 0; idx < count; idx++)
        {
          Debug.Assert(elements[idx].XRef != null);
          Debug.Assert(elements[idx].XRef.Document != null);
          Debug.Assert(resources[idx].Document != null);
          if (elements[idx].ObjectID.ObjectNumber == 12)
            GetType();
        }
#endif

      // 2nd loop. Fix up all indirect references that still refers to the import document.
      for (int idx = 0; idx < count; idx++)
      {
        PdfObject obj = elements[idx];
        Debug.Assert(obj.Owner == owner);
        FixUpObject(iot, owner, obj);
      }

      // return the clone of the former root object
      return elements[0];
    }

    ///// <summary>
    ///// Imports an object and its transitive closure to the specified document.
    ///// </summary>
    /// <param name="importedObjectTable">The imported object table of the owner for the external document.</param>
    /// <param name="owner">The document that owns the cloned objects.</param>
    /// <param name="externalObject">The root object to be cloned.</param>
    /// <returns>The clone of the root object</returns>
    internal static PdfObject ImportClosure(PdfImportedObjectTable importedObjectTable, PdfDocument owner, PdfObject externalObject)
    {
      Debug.Assert(ReferenceEquals(importedObjectTable.Owner, owner), "importedObjectTable does not belong to the owner.");
      Debug.Assert(ReferenceEquals(importedObjectTable.ExternalDocument, externalObject.Owner),
        "The ExternalDocument of the importedObjectTable does not belong to the owner of object to be imported.");

      // Get transitive closure of external object
      PdfObject[] elements = externalObject.Owner.Internals.GetClosure(externalObject);
      int count = elements.Length;
#if DEBUG_
        for (int idx = 0; idx < count; idx++)
        {
          Debug.Assert(elements[idx].XRef != null);
          Debug.Assert(elements[idx].XRef.Document != null);
          Debug.Assert(elements[idx].Document != null);
          if (elements[idx].ObjectID.ObjectNumber == 12)
            GetType();
        }
#endif
      // 1st loop. Already imported objects are reused and new ones are cloned.
      for (int idx = 0; idx < count; idx++)
      {
        PdfObject obj = elements[idx];
        Debug.Assert(!ReferenceEquals(obj.Owner, owner));

        if (importedObjectTable.Contains(obj.ObjectID))
        {
          // External object was already imported
          PdfReference iref = importedObjectTable[obj.ObjectID];
          Debug.Assert(iref != null);
          Debug.Assert(iref.Value != null);
          Debug.Assert(iref.Document == owner);
          // replace external object by the already cloned counterpart
          elements[idx] = iref.Value;
        }
        else
        {
          // External object was not imported ealier and must be cloned
          PdfObject clone = obj.Clone();
          Debug.Assert(clone.Reference == null);
          clone.Document = owner;
          if (obj.Reference != null)
          {
            // Case: The cloned object was an indirect object.
            // add clone to new owner document
            owner.irefTable.Add(clone);
            Debug.Assert(clone.Reference != null);
            // save an association from old object identifier to new iref
            importedObjectTable.Add(obj.ObjectID, clone.Reference);
          }
          else
          {
            // Case: The cloned object was a direct object.
            // only the root object can be a direct object
            Debug.Assert(idx == 0);
            //// add it to this (the importer) document
            //owner.irefTable.Add(clone);
            //Debug.Assert(clone.Reference != null);
          }
          // replace external object by its clone
          elements[idx] = clone;
        }
      }
#if DEBUG_
      for (int idx = 0; idx < count; idx++)
      {
        Debug.Assert(elements[idx].XRef != null);
        Debug.Assert(elements[idx].XRef.Document != null);
        Debug.Assert(elements[idx].Document != null);
        if (resources[idx].ObjectID.ObjectNumber == 12)
          GetType();
      }
#endif

      // 2nd loop. Fix up indirect references that still refers to the external document.
      for (int idx = 0; idx < count; idx++)
      {
        PdfObject obj = elements[idx];
        Debug.Assert(owner != null);
        FixUpObject(importedObjectTable, importedObjectTable.Owner, obj);
      }

      // return the imported root object
      return elements[0];
    }

    /// <summary>
    /// Replace all indirect references to external objects by their cloned counterparts
    /// owned by the importer document.
    /// </summary>
    internal static void FixUpObject(PdfImportedObjectTable iot, PdfDocument owner, PdfObject value)
    {
      Debug.Assert(ReferenceEquals(iot.Owner, owner));

      PdfDictionary dict;
      PdfArray array;
      if ((dict = value as PdfDictionary) != null)
      {
        // Set document for cloned direct objects
        if (dict.Owner == null)
          dict.Document = owner;
        else
          Debug.Assert(dict.Owner == owner);

        // Search for indirect references in all keys
        PdfName[] names = dict.Elements.KeyNames;
        foreach (PdfName name in names)
        {
          PdfItem item = dict.Elements[name];
          // Is item an iref?
          PdfReference iref = item as PdfReference;
          if (iref != null)
          {
            // Does the iref already belongs to the owner?
            if (iref.Document == owner)
            {
              // Yes: fine. Happens when an already cloned object is reused.
              continue;
            }
            else
            {
              //Debug.Assert(iref.Document == iot.Document);
              // No: replace with iref of cloned object
              PdfReference newXRef = iot[iref.ObjectID];
              Debug.Assert(newXRef != null);
              Debug.Assert(newXRef.Document == owner);
              dict.Elements[name] = newXRef;
            }
          }
          else if (item is PdfObject)
          {
            // Fix up inner objects
            FixUpObject(iot, owner, (PdfObject)item);
          }
        }
      }
      else if ((array = value as PdfArray) != null)
      {
        // Set document for cloned direct objects
        if (array.Owner == null)
          array.Document = owner;
        else
          Debug.Assert(array.Owner == owner);

        // Search for indirect references in all array elements
        int count = array.Elements.Count;
        for (int idx = 0; idx < count; idx++)
        {
          PdfItem item = array.Elements[idx];
          // Is item an iref?
          PdfReference iref = item as PdfReference;
          if (iref != null)
          {
            // Does the iref already belongs to the owner?
            if (iref.Document == owner)
            {
              // Yes: fine. Happens when an already cloned object is reused.
              continue;
            }
            else
            {
              Debug.Assert(iref.Document == iot.ExternalDocument);
              // No: replace with iref of cloned object
              PdfReference newXRef = iot[iref.ObjectID];
              Debug.Assert(newXRef != null);
              Debug.Assert(newXRef.Document == owner);
              array.Elements[idx] = newXRef;
            }
          }
          else if (item is PdfObject)
          {
            // Fix up inner objects
            FixUpObject(iot, owner, (PdfObject)item);
          }
        }
      }
    }

    /// <summary>
    /// Gets the indirect reference of this object. If the value is null, this object is a direct object.
    /// </summary>
    public PdfReference Reference
    {
      get { return this.iref; }
      set
      {
        //Debug.Assert(value.Value == null);
        this.iref = value;
      }
    }
    internal PdfReference iref;
  }
}