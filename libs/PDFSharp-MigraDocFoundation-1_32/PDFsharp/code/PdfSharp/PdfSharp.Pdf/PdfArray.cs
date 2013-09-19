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
using PdfSharp.Internal;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Represents a PDF array object.
  /// </summary>
  [DebuggerDisplay("(elements={Elements.Count})")]
  public class PdfArray : PdfObject, IEnumerable<PdfItem>
  {
    ArrayElements elements;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfArray"/> class.
    /// </summary>
    public PdfArray()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfArray"/> class.
    /// </summary>
    /// <param name="document">The document.</param>
    public PdfArray(PdfDocument document)
      : base(document)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfArray"/> class.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="items">The items.</param>
    public PdfArray(PdfDocument document, params PdfItem[] items)
      : base(document)
    {
      foreach (PdfItem item in items)
        Elements.Add(item);
    }

    /// <summary>
    /// Initializes a new instance from an existing dictionary. Used for object type transformation.
    /// </summary>
    /// <param name="array">The array.</param>
    protected PdfArray(PdfArray array)
      : base(array)
    {
      if (array.elements != null)
        array.elements.SetOwner(this);
    }

    /// <summary>
    /// Creates a copy of this array. Direct elements are deep copied. Indirect references are not
    /// modified.
    /// </summary>
    public new PdfArray Clone()
    {
      return (PdfArray)Copy();
    }

    /// <summary>
    /// Implements the copy mechanism.
    /// </summary>
    protected override object Copy()
    {
      PdfArray array = (PdfArray)base.Copy();
      if (array.elements != null)
      {
        array.elements = array.elements.Clone();
        int count = array.elements.Count;
        for (int idx = 0; idx < count; idx++)
        {
          PdfItem item = array.elements[idx];
          if (item is PdfObject)
            array.elements[idx] = item.Clone();
        }
      }
      return array;
    }

    /// <summary>
    /// Gets the collection containing the elements of this object.
    /// </summary>
    public ArrayElements Elements
    {
      get
      {
        if (this.elements == null)
          this.elements = new ArrayElements(this);
        return this.elements;
      }
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    public virtual IEnumerator<PdfItem> GetEnumerator()
    {
      return Elements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Returns a string with the content of this object in a readable form. Useful for debugging purposes only.
    /// </summary>
    public override string ToString()
    {
      StringBuilder pdf = new StringBuilder();
      pdf.Append("[ ");
      int count = Elements.Count;
      for (int idx = 0; idx < count; idx++)
        pdf.Append(Elements[idx].ToString() + " ");
      pdf.Append("]");
      return pdf.ToString();
    }

    internal override void WriteObject(PdfWriter writer)
    {
      writer.WriteBeginObject(this);
      int count = Elements.Count;
      for (int idx = 0; idx < count; idx++)
      {
        PdfItem value = Elements[idx];
        value.WriteObject(writer);
      }
      writer.WriteEndObject();
    }

    /// <summary>
    /// Represents the elements of an PdfArray.
    /// </summary>
    public sealed class ArrayElements : IList<PdfItem>, ICloneable
    {
      List<PdfItem> elements;
      PdfArray owner;

      internal ArrayElements(PdfArray array)
      {
        this.elements = new List<PdfItem>();
        this.owner = array;
      }

      object ICloneable.Clone()
      {
        ArrayElements elements = (ArrayElements)MemberwiseClone();
        elements.elements = new List<PdfItem>(elements.elements);
        elements.owner = null;
        return elements;
      }

      /// <summary>
      /// Creates a shallow copy of this object.
      /// </summary>
      public ArrayElements Clone()
      {
        return (ArrayElements)((ICloneable)this).Clone();
      }

      /// <summary>
      /// Moves this instance to another dictionary during object type transformation.
      /// </summary>
      internal void SetOwner(PdfArray array)
      {
        this.owner = array;
        array.elements = this;
      }

      /// <summary>
      /// Converts the specified value to boolean.
      /// If the value not exists, the function returns false.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// If the index is out of range, the function throws an ArgumentOutOfRangeException.
      /// </summary>
      public bool GetBoolean(int index)
      {
        if (index < 0 || index >= Count)
          throw new ArgumentOutOfRangeException("index", index, PSSR.IndexOutOfRange);

        object obj = this[index];
        if (obj == null)
          return false;
        if (obj is PdfBoolean)
          return ((PdfBoolean)obj).Value;
        else if (obj is PdfBooleanObject)
          return ((PdfBooleanObject)obj).Value;
        throw new InvalidCastException("GetBoolean: Object is not a boolean.");
      }

      /// <summary>
      /// Converts the specified value to integer.
      /// If the value not exists, the function returns 0.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// If the index is out of range, the function throws an ArgumentOutOfRangeException.
      /// </summary>
      public int GetInteger(int index)
      {
        if (index < 0 || index >= Count)
          throw new ArgumentOutOfRangeException("index", index, PSSR.IndexOutOfRange);

        object obj = this[index];
        if (obj == null)
          return 0;
        if (obj is PdfInteger)
          return ((PdfInteger)obj).Value;
        if (obj is PdfIntegerObject)
          return ((PdfIntegerObject)obj).Value;
        throw new InvalidCastException("GetInteger: Object is not an integer.");
      }

      /// <summary>
      /// Converts the specified value to double.
      /// If the value not exists, the function returns 0.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// If the index is out of range, the function throws an ArgumentOutOfRangeException.
      /// </summary>
      public double GetReal(int index)
      {
        if (index < 0 || index >= Count)
          throw new ArgumentOutOfRangeException("index", index, PSSR.IndexOutOfRange);

        object obj = this[index];
        if (obj == null)
          return 0;
        if (obj is PdfReal)
          return ((PdfReal)obj).Value;
        else if (obj is PdfRealObject)
          return ((PdfRealObject)obj).Value;
        else if (obj is PdfInteger)
          return ((PdfInteger)obj).Value;
        else if (obj is PdfIntegerObject)
          return ((PdfIntegerObject)obj).Value;
        throw new InvalidCastException("GetReal: Object is not a number.");
      }

      /// <summary>
      /// Converts the specified value to string.
      /// If the value not exists, the function returns the empty string.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// If the index is out of range, the function throws an ArgumentOutOfRangeException.
      /// </summary>
      public string GetString(int index)
      {
        if (index < 0 || index >= Count)
          throw new ArgumentOutOfRangeException("index", index, PSSR.IndexOutOfRange);

        object obj = this[index];
        if (obj == null)
          return "";
        if (obj is PdfString)
          return ((PdfString)obj).Value;
        if (obj is PdfStringObject)
          return ((PdfStringObject)obj).Value;
        throw new InvalidCastException("GetString: Object is not an integer.");
      }

      /// <summary>
      /// Converts the specified value to a name.
      /// If the value not exists, the function returns the empty string.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// If the index is out of range, the function throws an ArgumentOutOfRangeException.
      /// </summary>
      public string GetName(int index)
      {
        if (index < 0 || index >= Count)
          throw new ArgumentOutOfRangeException("index", index, PSSR.IndexOutOfRange);

        object obj = this[index];
        if (obj == null)
          return "";
        if (obj is PdfName)
          return ((PdfName)obj).Value;
        if (obj is PdfNameObject)
          return ((PdfNameObject)obj).Value;
        throw new InvalidCastException("GetName: Object is not an integer.");
      }

      /// <summary>
      /// Returns the indirect object if the value at the specified index is a PdfReference.
      /// </summary>
      [Obsolete("Use GetObject, GetDictionary, GetArray, or GetReference")]
      public PdfObject GetIndirectObject(int index)
      {
        if (index < 0 || index >= Count)
          throw new ArgumentOutOfRangeException("index", index, PSSR.IndexOutOfRange);

        PdfItem item = this[index];
        if (item is PdfReference)
          return ((PdfReference)item).Value;
        return null;
      }

      /// <summary>
      /// Gets the PdfObject with the specified index, or null, if no such object exists. If the index refers to
      /// a reference, the referenced PdfObject is returned.
      /// </summary>
      public PdfObject GetObject(int index)
      {
        if (index < 0 || index >= Count)
          throw new ArgumentOutOfRangeException("index", index, PSSR.IndexOutOfRange);

        PdfItem item = this[index];
        if (item is PdfReference)
          return ((PdfReference)item).Value;
        return item as PdfObject;
      }

      /// <summary>
      /// Gets the PdfArray with the specified index, or null, if no such object exists. If the index refers to
      /// a reference, the referenced PdfArray is returned.
      /// </summary>
      public PdfDictionary GetDictionary(int index)
      {
        return GetObject(index) as PdfDictionary;
      }

      /// <summary>
      /// Gets the PdfArray with the specified index, or null, if no such object exists. If the index refers to
      /// a reference, the referenced PdfArray is returned.
      /// </summary>
      public PdfArray GetArray(int index)
      {
        return GetObject(index) as PdfArray;
      }

      /// <summary>
      /// Gets the PdfReference with the specified index, or null, if no such object exists.
      /// </summary>
      public PdfReference GetReference(int index)
      {
        PdfItem item = this[index];
        return item as PdfReference;
      }

      /// <summary>
      /// Gets all items of this array.
      /// </summary>
      public PdfItem[] Items
      {
        get { return this.elements.ToArray(); }
      }

      ///// <summary>
      ///// INTERNAL USE ONLY.
      ///// </summary>
      //public List<PdfItem> GetArrayList_()
      //{
      //  // I use this hack to order the pages by ZIP code (MigraDoc ControlCode-Generator)
      //  // TODO: implement a clean solution
      //  return this.elements;
      //}

      #region IList Members

      /// <summary>
      /// Returns false.
      /// </summary>
      public bool IsReadOnly
      {
        get { return false; }
      }

      //object IList.this[int index]
      //{
      //  get { return this.elements[index]; }
      //  set { this.elements[index] = value as PdfItem; }
      //}

      /// <summary>
      /// Gets or sets an item at the specified index.
      /// </summary>
      /// <value></value>
      public PdfItem this[int index]
      {
        get { return this.elements[index]; }
        set
        {
          if (value == null)
            throw new ArgumentNullException("value");
          this.elements[index] = value;
        }
      }

      /// <summary>
      /// Removes the item at the specified index.
      /// </summary>
      public void RemoveAt(int index)
      {
        this.elements.RemoveAt(index);
      }

      /// <summary>
      /// Removes the first occurrence of a specific object from the array/>.
      /// </summary>
      public bool Remove(PdfItem item)
      {
        return this.elements.Remove(item);
      }

      /// <summary>
      /// Inserts the item the specified index.
      /// </summary>
      public void Insert(int index, PdfItem value)
      {
        this.elements.Insert(index, value);
      }

      /// <summary>
      /// Determines whether the specified value is in the array.
      /// </summary>
      public bool Contains(PdfItem value)
      {
        return this.elements.Contains(value);
      }

      /// <summary>
      /// Removes all items from the array.
      /// </summary>
      public void Clear()
      {
        this.elements.Clear();
      }

      /// <summary>
      /// Gets the index of the specified item.
      /// </summary>
      public int IndexOf(PdfItem value)
      {
        return this.elements.IndexOf(value);
      }

      /// <summary>
      /// Appends the specified object to the array.
      /// </summary>
      public void Add(PdfItem value)
      {
        // TODO: ??? 
        //Debug.Assert((value is PdfObject && ((PdfObject)value).Reference == null) | !(value is PdfObject),
        //  "You try to set an indirect object directly into an array.");

        PdfObject obj = value as PdfObject;
        if (obj != null && obj.IsIndirect)
          this.elements.Add(obj.Reference);
        else
          this.elements.Add(value);
      }

      /// <summary>
      /// Returns false.
      /// </summary>
      public bool IsFixedSize
      {
        get { return false; }
      }

      #endregion

      #region ICollection Members

      /// <summary>
      /// Returns false.
      /// </summary>
      public bool IsSynchronized
      {
        get { return false; }
      }

      /// <summary>
      /// Gets the number of elements in the array.
      /// </summary>
      public int Count
      {
        get { return this.elements.Count; }
      }

      /// <summary>
      /// Copies the elements of the array to the specified array.
      /// </summary>
      public void CopyTo(PdfItem[] array, int index)
      {
        this.elements.CopyTo(array, index);
      }

      /// <summary>
      /// The current implementation return null.
      /// </summary>
      public object SyncRoot
      {
        get { return null; }
      }

      #endregion

      /// <summary>
      /// Returns an enumerator that iterates through the array.
      /// </summary>
      public IEnumerator<PdfItem> GetEnumerator()
      {
        return this.elements.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return this.elements.GetEnumerator();
      }
    }
  }
}
