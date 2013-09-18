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
using System.Collections.Generic;
using System.Text;
using System.IO;
using PdfSharp.Internal;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Represents the cross reference table of a PDF document. It contains all indirect objects of
  /// a document.
  /// </summary>
  internal sealed class PdfReferenceTable // do not derive from PdfObject
  {
    public PdfReferenceTable(PdfDocument document)
    {
      this.document = document;
    }
    PdfDocument document;

    /// <summary>
    /// Represents the relation between PdfObjectID and PdfReference for a PdfDocument.
    /// </summary>
    public Dictionary<PdfObjectID, PdfReference> objectTable = new Dictionary<PdfObjectID, PdfReference>();

    internal bool IsUnderConstruction
    {
      get { return this.isUnderConstruction; }
      set { this.isUnderConstruction = value; }
    }
    bool isUnderConstruction;

    /// <summary>
    /// Adds a cross reference entry to the table. Used when parsing the trailer.
    /// </summary>
    public void Add(PdfReference iref)
    {
      if (iref.ObjectID.IsEmpty)
        iref.ObjectID = new PdfObjectID(GetNewObjectNumber());

      if (this.objectTable.ContainsKey(iref.ObjectID))
        throw new InvalidOperationException("Object already in table.");

      this.objectTable.Add(iref.ObjectID, iref);
    }

    /// <summary>
    /// Adds a PdfObject to the table.
    /// </summary>
    public void Add(PdfObject value)
    {
      if (value.Owner == null)
        value.Document = this.document;
      else
        Debug.Assert(value.Owner == this.document);

      if (value.ObjectID.IsEmpty)
        value.SetObjectID(GetNewObjectNumber(), 0);

      if (this.objectTable.ContainsKey(value.ObjectID))
        throw new InvalidOperationException("Object already in table.");

      this.objectTable.Add(value.ObjectID, value.Reference);
    }

    public void Remove(PdfReference iref)
    {
      this.objectTable.Remove(iref.ObjectID);
    }

    /// <summary>
    /// Gets a cross reference entry from an object identifier.
    /// Returns null if no object with the specified ID exists in the object table.
    /// </summary>
    public PdfReference this[PdfObjectID objectID]
    {
      get
      {
        PdfReference iref;
        this.objectTable.TryGetValue(objectID, out iref);
        return iref;
      }
    }

    /// <summary>
    /// Indicates whether the specified object identifier is in the table.
    /// </summary>
    public bool Contains(PdfObjectID objectID)
    {
      return this.objectTable.ContainsKey(objectID);
    }

    //public PdfObject GetObject(PdfObjectID objectID)
    //{
    //  return this[objectID].Value;
    //}

    //    /// <summary>
    //    /// Gets the entry for the specified object, or null, if the object is not in
    //    /// this XRef table.
    //    /// </summary>
    //    internal PdfReference GetEntry(PdfObjectID objectID)
    //    {
    //      return this[objectID];
    //    }

    /// <summary>
    /// Returns the next free object number.
    /// </summary>
    public int GetNewObjectNumber()
    {
      // New objects are numbered consecutively. If a document is imported, maxObjectNumber is
      // set to the highest object number used in the document.
      return ++this.maxObjectNumber;
    }
    internal int maxObjectNumber;

    /// <summary>
    /// Writes the iref section in pdf stream.
    /// </summary>
    internal void WriteObject(PdfWriter writer)
    {
      writer.WriteRaw("xref\n");

      //      PdfObjectID[] objectIDs2 = AllObjectIDs;
      PdfReference[] irefs = AllReferences;
      //      PdfObjectID id1 = objectIDs2[0];
      //      PdfReference     xref1 = irefs[0];
      //      id1.GetType();
      //      xref1.GetType();
      //ArrayList list = new ArrayList(AllObjectIDs);
      //list.Sort();
      //list.CopyTo(objectIDs);

      int count = irefs.Length;
      writer.WriteRaw(String.Format("0 {0}\n", count + 1));
      writer.WriteRaw(String.Format("{0:0000000000} {1:00000} {2} \n", 0, 65535, "f"));
      //PdfEncoders.WriteAnsi(stream, text);

      for (int idx = 0; idx < count; idx++)
      {
        PdfReference iref = irefs[idx];

        //text = String.Format("{0} {1}\n", iref.ObjectID.ObjectNumber, 1);
        //PdfEncoders.WriteAnsi(stream, text);

        // Acrobat is very pedantic; it must be exactly 20 bytes per line.
        writer.WriteRaw(
          String.Format("{0:0000000000} {1:00000} {2} \n", iref.Position, iref.GenerationNumber, "n"));
        //PdfEncoders.WriteAnsi(stream, text);
      }
    }

    /// <summary>
    /// Gets an array of all object identifier. For debugging purposes only.
    /// </summary>
    internal PdfObjectID[] AllObjectIDs
    {
      get
      {
        ICollection collection = this.objectTable.Keys;
        PdfObjectID[] objectIDs = new PdfObjectID[collection.Count];
        collection.CopyTo(objectIDs, 0);
        return objectIDs;
      }
    }

    /// <summary>
    /// Gets an array of all cross references ordered increasing by their object identifier.
    /// </summary>
    internal PdfReference[] AllReferences
    {
      get
      {
        Dictionary<PdfObjectID, PdfReference>.ValueCollection collection = this.objectTable.Values;
        List<PdfReference> list = new List<PdfReference>(collection);
        list.Sort(PdfReference.Comparer);
        PdfReference[] irefs = new PdfReference[collection.Count];
        list.CopyTo(irefs, 0);
        return irefs;
      }
    }

    internal void HandleOrphanedReferences()
    {
    }

    /// <summary>
    /// Removes all objects that cannot be reached from the trailer. Returns the number of removed objects.
    /// </summary>
    internal int Compact()
    {
      // TODO: remove PdfBooleanObject, PdfIntegerObject etc.
      int removed = this.objectTable.Count;
      //CheckConsistence();
      // TODO: Is this really so easy?
      PdfReference[] irefs = TransitiveClosure(this.document.trailer);

#if DEBUG_
      foreach (PdfReference iref in this.objectTable.Values)
      {
        if (iref.Value == null)
          this.GetType();
        Debug.Assert(iref.Value != null);
      }

      foreach (PdfReference iref in irefs)
      {
        if (!this.objectTable.Contains(iref.ObjectID))
          this.GetType();
        Debug.Assert(this.objectTable.Contains(iref.ObjectID));

        if (iref.Value == null)
          this.GetType();
        Debug.Assert(iref.Value != null);
      }
#endif

      this.maxObjectNumber = 0;
      this.objectTable.Clear();
      foreach (PdfReference iref in irefs)
      {
        this.objectTable.Add(iref.ObjectID, iref);
        this.maxObjectNumber = Math.Max(this.maxObjectNumber, iref.ObjectNumber);
      }
      //CheckConsistence();
      removed -= this.objectTable.Count;
      return removed;
    }

    /// <summary>
    /// Renumbers the objects starting at 1.
    /// </summary>
    internal void Renumber()
    {
      //CheckConsistence();
      PdfReference[] irefs = AllReferences;
      this.objectTable.Clear();
      // Give all objects a new number
      int count = irefs.Length;
      for (int idx = 0; idx < count; idx++)
      {
        PdfReference iref = irefs[idx];
#if DEBUG_
        if (iref.ObjectNumber == 1108)
          GetType();
#endif
        iref.ObjectID = new PdfObjectID(idx + 1);
        // Rehash with new number
        this.objectTable.Add(iref.ObjectID, iref);
      }
      this.maxObjectNumber = count;
      //CheckConsistence();
    }

    /// <summary>
    /// Checks the logical consistence for debugging purposes (useful after reconstruction work).
    /// </summary>
    [Conditional("DEBUG_")]
    public void CheckConsistence()
    {
      Dictionary<PdfReference, object> ht1 = new Dictionary<PdfReference, object>();
      foreach (PdfReference iref in this.objectTable.Values)
      {
        Debug.Assert(!ht1.ContainsKey(iref), "Duplicate iref.");
        Debug.Assert(iref.Value != null);
        ht1.Add(iref, null);
      }

      Dictionary<PdfObjectID, object> ht2 = new Dictionary<PdfObjectID, object>();
      foreach (PdfReference iref in this.objectTable.Values)
      {
        Debug.Assert(!ht2.ContainsKey(iref.ObjectID), "Duplicate iref.");
        ht2.Add(iref.ObjectID, null);
      }

      ICollection collection = this.objectTable.Values;
      int count = collection.Count;
      PdfReference[] irefs = new PdfReference[count];
      collection.CopyTo(irefs, 0);
#if true_
      for (int i = 0; i < count; i++)
        for (int j = 0; j < count; j++)
          if (i != j)
          {
            Debug.Assert(Object.ReferenceEquals(irefs[i].Document, this.document));
            Debug.Assert(irefs[i] != irefs[j]);
            Debug.Assert(!Object.ReferenceEquals(irefs[i], irefs[j]));
            Debug.Assert(!Object.ReferenceEquals(irefs[i].Value, irefs[j].Value));
            Debug.Assert(!Object.ReferenceEquals(irefs[i].ObjectID, irefs[j].Value.ObjectID));
            Debug.Assert(irefs[i].ObjectNumber != irefs[j].Value.ObjectNumber);
            Debug.Assert(Object.ReferenceEquals(irefs[i].Document, irefs[j].Document));
            GetType();
          }
#endif
    }

    ///// <summary>
    ///// The garbage collector for PDF objects.
    ///// </summary>
    //public sealed class GC
    //{
    //  PdfXRefTable xrefTable;
    //
    //  internal GC(PdfXRefTable xrefTable)
    //  {
    //    this.xrefTable = xrefTable;
    //  }
    //
    //  public void Collect()
    //  {
    //  }
    //
    //  public PdfReference[] ReachableObjects()
    //  {
    //    Hashtable objects = new Hashtable();
    //    TransitiveClosure(objects, this.xrefTable.document.trailer);
    //  }

    /// <summary>
    /// Calculates the transitive closure of the specified PdfObject, i.e. all indirect objects
    /// recursively reachable from the specified object.
    /// </summary>
    public PdfReference[] TransitiveClosure(PdfObject pdfObject)
    {
      return TransitiveClosure(pdfObject, Int16.MaxValue);
    }

    /// <summary>
    /// Calculates the transitive closure of the specified PdfObject with the specified depth, i.e. all indirect objects
    /// recursively reachable from the specified object in up to maximally depth steps.
    /// </summary>
    public PdfReference[] TransitiveClosure(PdfObject pdfObject, int depth)
    {
      CheckConsistence();
      Dictionary<PdfItem, object> objects = new Dictionary<PdfItem, object>();
      this.overflow = new Dictionary<PdfItem, object>();
      TransitiveClosureImplementation(objects, pdfObject, ref depth);
    TryAgain:
      if (this.overflow.Count > 0)
      {
        PdfObject[] array = new PdfObject[this.overflow.Count];
        this.overflow.Keys.CopyTo(array, 0);
        this.overflow = new Dictionary<PdfItem, object>();
        for (int idx = 0; idx < array.Length; idx++)
        {
          //PdfObject o = array[idx];
          //o.GetType();
          PdfObject obj = array[idx];
          //if (!objects.Contains(obj))
          //  objects.Add(obj, null);
          TransitiveClosureImplementation(objects, obj, ref depth);
        }
        goto TryAgain;
      }

      CheckConsistence();

      ICollection collection = objects.Keys;
      int count = collection.Count;
      PdfReference[] irefs = new PdfReference[count];
      collection.CopyTo(irefs, 0);

#if true_
      for (int i = 0; i < count; i++)
        for (int j = 0; j < count; j++)
          if (i != j)
          {
            Debug.Assert(Object.ReferenceEquals(irefs[i].Document, this.document));
            Debug.Assert(irefs[i] != irefs[j]);
            Debug.Assert(!Object.ReferenceEquals(irefs[i], irefs[j]));
            Debug.Assert(!Object.ReferenceEquals(irefs[i].Value, irefs[j].Value));
            Debug.Assert(!Object.ReferenceEquals(irefs[i].ObjectID, irefs[j].Value.ObjectID));
            Debug.Assert(irefs[i].ObjectNumber != irefs[j].Value.ObjectNumber);
            Debug.Assert(Object.ReferenceEquals(irefs[i].Document, irefs[j].Document));
            GetType();
          }
#endif
      return irefs;
    }

    static int nestingLevel;
    Dictionary<PdfItem, object> overflow = new Dictionary<PdfItem, object>();
    void TransitiveClosureImplementation(Dictionary<PdfItem, object> objects, PdfObject pdfObject, ref int depth)
    {
      if (depth-- == 0)
        return;
      try
      {
        nestingLevel++;
        if (nestingLevel >= 1000)
        {
          //Debug.WriteLine(String.Format("Nestinglevel={0}", nestingLevel));
          //GetType();
          if (!this.overflow.ContainsKey(pdfObject))
            this.overflow.Add(pdfObject, null);
          return;
        }
#if DEBUG_
        //enterCount++;
        if (enterCount == 5400)
          GetType();
        //if (!Object.ReferenceEquals(pdfObject.Owner, this.document))
        //  GetType();
        //////Debug.Assert(Object.ReferenceEquals(pdfObject27.Document, this.document));
        //      if (item is PdfObject && ((PdfObject)item).ObjectID.ObjectNumber == 5)
        //        Debug.WriteLine("items: " + ((PdfObject)item).ObjectID.ToString());
        //if (pdfObject.ObjectNumber == 5)
        //  GetType();
#endif

        IEnumerable enumerable = null; //(IEnumerator)pdfObject;
        if (pdfObject is PdfDictionary)
          enumerable = ((PdfDictionary)pdfObject).Elements.Values;
        else if (pdfObject is PdfArray)
          enumerable = ((PdfArray)pdfObject).Elements;
        if (enumerable != null)
        {
          foreach (PdfItem item in enumerable)
          {
            PdfReference iref = item as PdfReference;
            if (iref != null)
            {
              // Is this an indirect reference to an object that not exists?
              //if (iref.Document == null)
              //{
              //  Debug.WriteLine("Dead object dedected: " + iref.ObjectID.ToString());
              //  PdfReference dead = DeadObject;
              //  iref.ObjectID = dead.ObjectID;
              //  iref.Document = this.document;
              //  iref.SetObject(dead.Value);
              //  PdfDictionary dict = (PdfDictionary)dead.Value;
              //
              //  dict.Elements["/DeadObjectCount"] = 
              //    new PdfInteger(dict.Elements.GetInteger("/DeadObjectCount") + 1);
              //
              //  iref = dead;
              //}

              if (!Object.ReferenceEquals(iref.Document, this.document))
              {
                GetType();
                Debug.WriteLine(String.Format("Bad iref: {0}", iref.ObjectID.ToString()));
              }
              Debug.Assert(Object.ReferenceEquals(iref.Document, this.document) || iref.Document == null, "External object detected!");
#if DEBUG
              if (iref.ObjectID.ObjectNumber == 23)
                GetType();
#endif
              if (!objects.ContainsKey(iref))
              {
                PdfObject value = iref.Value;

                // Ignore unreachable objets
                if (iref.Document != null)
                {
                  // ... from trailer hack
                  if (value == null)
                  {
                    iref = this.objectTable[iref.ObjectID];
                    Debug.Assert(iref.Value != null);
                    value = iref.Value;
                  }
                  Debug.Assert(Object.ReferenceEquals(iref.Document, this.document));
                  objects.Add(iref, null);
                  //Debug.WriteLine(String.Format("objects.Add('{0}', null);", iref.ObjectID.ToString()));
                  if (value is PdfArray || value is PdfDictionary)
                    TransitiveClosureImplementation(objects, value, ref depth);
                }
                //else
                //{
                //  objects2.Add(this[iref.ObjectID], null);
                //}
              }
            }
            else
            {
              PdfObject pdfObject28 = item as PdfObject;
              //if (pdfObject28 != null)
              //  Debug.Assert(Object.ReferenceEquals(pdfObject28.Document, this.document));
              if (pdfObject28 != null && (pdfObject28 is PdfDictionary || pdfObject28 is PdfArray))
                TransitiveClosureImplementation(objects, pdfObject28, ref depth);
            }
          }
        }
      }
      finally
      {
        nestingLevel--;
      }
    }

    /// <summary>
    /// Gets the cross reference to an objects used for undefined indirect references.
    /// </summary>
    public PdfReference DeadObject
    {
      get
      {
        if (false || this.deadObject == null)
        {
          this.deadObject = new PdfDictionary(this.document);
          Add(this.deadObject);
          this.deadObject.Elements.Add("/DeadObjectCount", new PdfInteger());
        }
        return this.deadObject.Reference;
      }
    }
    PdfDictionary deadObject;
  }
}
