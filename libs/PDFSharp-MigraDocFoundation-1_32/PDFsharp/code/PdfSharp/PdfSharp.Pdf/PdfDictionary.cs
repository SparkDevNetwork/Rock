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
using System.Reflection;
using System.Text;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Value creation flags. Specifies whether and how a value that not exists is created.
  /// </summary>
  public enum VCF
  {
    /// <summary>
    /// Don't create the value.
    /// </summary>
    None,

    /// <summary>
    /// Create the value as direct object.
    /// </summary>
    Create,

    /// <summary>
    /// Create the value as indirect object.
    /// </summary>
    CreateIndirect,
  }

  /// <summary>
  /// Represents a PDF dictionary object.
  /// </summary>
  [DebuggerDisplay("(pairs={Elements.Count})")]
  public class PdfDictionary : PdfObject, IEnumerable
  {
    /// <summary>
    /// The elements of the dictionary.
    /// </summary>
    protected DictionaryElements elements;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfDictionary"/> class.
    /// </summary>
    public PdfDictionary()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfDictionary"/> class.
    /// </summary>
    /// <param name="document">The document.</param>
    public PdfDictionary(PdfDocument document)
      : base(document)
    {
    }

    /// <summary>
    /// Initializes a new instance from an existing dictionary. Used for object type transformation.
    /// </summary>
    protected PdfDictionary(PdfDictionary dict)
      : base(dict)
    {
      if (dict.elements != null)
        dict.elements.ChangeOwner(this);
      if (dict.stream != null)
        dict.stream.SetOwner(this);
    }

    /// <summary>
    /// Creates a copy of this dictionary. Direct values are deep copied. Indirect references are not
    /// modified.
    /// </summary>
    public new PdfDictionary Clone()
    {
      return (PdfDictionary)Copy();
    }

    /// <summary>
    /// This function is useful for importing objects from external documents. The returned object is not
    /// yet complete. irefs refer to external objects and directed objects are cloned but their document
    /// property is null. A cloned dictionary or array needs a 'fix-up' to be a valid object.
    /// </summary>
    protected override object Copy()
    {
      PdfDictionary dict = (PdfDictionary)base.Copy();
      if (dict.elements != null)
      {
        dict.elements = dict.elements.Clone();
        dict.elements.ChangeOwner(dict);
        PdfName[] names = dict.elements.KeyNames;
        foreach (PdfName name in names)
        {
          PdfObject obj = dict.elements[name] as PdfObject;
          if (obj != null)
          {
            obj = obj.Clone();
            // Recall that obj.Document is now null
            dict.elements[name] = obj;
          }
        }
      }
      if (dict.stream != null)
      {
        dict.stream = dict.stream.Clone();
        dict.stream.SetOwner(dict);
      }
      return dict;
    }

    /// <summary>
    /// Gets the hashtable containing the elements of this dictionary.
    /// </summary>
    public DictionaryElements Elements
    {
      get
      {
        if (this.elements == null)
          this.elements = new DictionaryElements(this);
        return this.elements;
      }
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    public IEnumerator GetEnumerator()
    {
      return Elements.GetEnumerator();
    }

    /// <summary>
    /// Returns a string with the content of this object in a readable form. Useful for debugging purposes only.
    /// </summary>
    public override string ToString()
    {
      // Get keys and sort
      PdfName[] keys = Elements.KeyNames;
      List<PdfName> list = new List<PdfName>(keys);
      list.Sort((IComparer<PdfName>)PdfName.Comparer);
      list.CopyTo(keys, 0);

      StringBuilder pdf = new StringBuilder();
      pdf.Append("<< ");
      foreach (PdfName key in keys)
        pdf.Append(key + " " + this.Elements[key] + " ");
      pdf.Append(">>");

      return pdf.ToString();
    }

    internal override void WriteObject(PdfWriter writer)
    {
      writer.WriteBeginObject(this);
      //int count = Elements.Count;
      PdfName[] keys = Elements.KeyNames;

#if DEBUG
      // TODO: automatically set length
      if (this.stream != null)
        Debug.Assert(Elements.ContainsKey(PdfStream.Keys.Length), "Dictionary has a stream but no length is set.");
#endif

#if DEBUG
      // Sort keys for debugging purposes. Comparing PDF files with for example programms like
      // Araxis Merge is easier with sorted keys.
      if (writer.Layout == PdfWriterLayout.Verbose)
      {
        List<PdfName> list = new List<PdfName>(keys);
        list.Sort(PdfName.Comparer);
        list.CopyTo(keys, 0);
      }
#endif

      foreach (PdfName key in keys)
        WriteDictionaryElement(writer, key);
      if (Stream != null)
        WriteDictionaryStream(writer);
      writer.WriteEndObject();
    }

    /// <summary>
    /// Writes a key/value pair of this dictionary. This function is intended to be overridden
    /// in derived classes.
    /// </summary>
    internal virtual void WriteDictionaryElement(PdfWriter writer, PdfName key)
    {
      if (key == null)
        throw new ArgumentNullException("key");
      PdfItem item = Elements[key];
#if DEBUG
      // TODO: simplify PDFsharp
      if (item is PdfObject && ((PdfObject)item).IsIndirect)
      {
        // Replace an indirect object by its Reference
        item = ((PdfObject)item).Reference;
        Debug.Assert(false, "Check when we come here.");
      }
#endif
      key.WriteObject(writer);
      item.WriteObject(writer);
      writer.NewLine();
    }

    /// <summary>
    /// Writes the stream of this dictionary. This function is intended to be overridden
    /// in a derived class.
    /// </summary>
    internal virtual void WriteDictionaryStream(PdfWriter writer)
    {
      writer.WriteStream(this, (writer.Options & PdfWriterOptions.OmitStream) == PdfWriterOptions.OmitStream);
    }

    /// <summary>
    /// Gets or sets the PDF stream belonging to this dictionary. Returns null if the dictionary has
    /// no stream. To create the stream, call the CreateStream function.
    /// </summary>
    public PdfStream Stream
    {
      get { return this.stream; }
      set { this.stream = value; }
    }
    PdfStream stream;

    /// <summary>
    /// Creates the stream of this dictionary and initializes it with the specified byte array.
    /// The function must not be called if the dictionary already has a stream.
    /// </summary>
    public PdfStream CreateStream(byte[] value)
    {
      if (this.stream != null)
        throw new InvalidOperationException("The dictionary already has a stream.");

      this.stream = new PdfStream(value, this);
      // Always set the length
      Elements[PdfStream.Keys.Length] = new PdfInteger(this.stream.Length);
      return this.stream;
    }

    /// <summary>
    /// When overridden in a derived class, gets the KeysMeta of this dictionary type.
    /// </summary>
    internal virtual DictionaryMeta Meta
    {
      get { return null; }
    }

    /// <summary>
    /// Represents the interface to the elements of a PDF dictionary.
    /// </summary>
    public sealed class DictionaryElements : IDictionary<string, PdfItem>, ICloneable
    {
      Dictionary<string, PdfItem> elements;
      PdfDictionary owner;

      internal DictionaryElements(PdfDictionary dict)
      {
        this.elements = new Dictionary<string, PdfItem>();
        this.owner = dict;
      }

      object ICloneable.Clone()
      {
        DictionaryElements dictionaryElements = (DictionaryElements)MemberwiseClone();
        dictionaryElements.elements = new Dictionary<string, PdfItem>(dictionaryElements.elements);
        dictionaryElements.owner = null;
        return dictionaryElements;
      }

      /// <summary>
      /// Creates a shallow copy of this object. The clone is not owned by a dictionary anymore.
      /// </summary>
      public DictionaryElements Clone()
      {
        return (DictionaryElements)((ICloneable)this).Clone();
      }

      /// <summary>
      /// Moves this instance to another dictionary during object type transformation.
      /// </summary>
      internal void ChangeOwner(PdfDictionary dict)
      {
        this.owner = dict;
        //???
        //if (dict.elements != null)
        //  Debug.Assert(dict.elements == this);
        dict.elements = this;
      }

      [Obsolete("Renamed to ChangeOwner for consistency.")]
      internal void SetOwner(PdfDictionary dict)
      {
        ChangeOwner(dict);
      }

      /// <summary>
      /// Gets the dictionary that this elements object belongs to.
      /// </summary>
      internal PdfDictionary Owner
      {
        get { return this.owner; }
      }

      /// <summary>
      /// Converts the specified value to boolean.
      /// If the value not exists, the function returns false.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// </summary>
      public bool GetBoolean(string key, bool create)
      {
        object obj = this[key];
        if (obj == null)
        {
          if (create)
            this[key] = new PdfBoolean();
          return false;
        }
        if (obj is PdfReference)
          obj = ((PdfReference)obj).Value;

        if (obj is PdfBoolean)
          return ((PdfBoolean)obj).Value;
        else if (obj is PdfBooleanObject)
          return ((PdfBooleanObject)obj).Value;
        throw new InvalidCastException("GetBoolean: Object is not a boolean.");
      }

      /// <summary>
      /// Converts the specified value to boolean.
      /// If the value not exists, the function returns false.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// </summary>
      public bool GetBoolean(string key)
      {
        return GetBoolean(key, false);
      }

      /// <summary>
      /// Sets the entry to a direct boolean value.
      /// </summary>
      public void SetBoolean(string key, bool value)
      {
        this[key] = new PdfBoolean(value);
      }

      /// <summary>
      /// Converts the specified value to integer.
      /// If the value not exists, the function returns 0.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// </summary>
      public int GetInteger(string key, bool create)
      {
        object obj = this[key];
        if (obj == null)
        {
          if (create)
            this[key] = new PdfInteger();
          return 0;
        }
        if (obj is PdfReference)
          obj = ((PdfReference)obj).Value;

        if (obj is PdfInteger)
          return ((PdfInteger)obj).Value;
        if (obj is PdfIntegerObject)
          return ((PdfIntegerObject)obj).Value;
        throw new InvalidCastException("GetInteger: Object is not an integer.");
      }

      /// <summary>
      /// Converts the specified value to integer.
      /// If the value not exists, the function returns 0.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// </summary>
      public int GetInteger(string key)
      {
        return GetInteger(key, false);
      }

      /// <summary>
      /// Sets the entry to a direct integer value.
      /// </summary>
      public void SetInteger(string key, int value)
      {
        this[key] = new PdfInteger(value);
      }

      /// <summary>
      /// Converts the specified value to double.
      /// If the value not exists, the function returns 0.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// </summary>
      public double GetReal(string key, bool create)
      {
        object obj = this[key];
        if (obj == null)
        {
          if (create)
            this[key] = new PdfReal();
          return 0;
        }
        if (obj is PdfReference)
          obj = ((PdfReference)obj).Value;

        if (obj is PdfReal)
          return ((PdfReal)obj).Value;
        
        if (obj is PdfRealObject)
          return ((PdfRealObject)obj).Value;
        
        if (obj is PdfInteger)
          return ((PdfInteger)obj).Value;
        
        if (obj is PdfIntegerObject)
          return ((PdfIntegerObject)obj).Value;
        
        throw new InvalidCastException("GetReal: Object is not a number.");
      }

      /// <summary>
      /// Converts the specified value to double.
      /// If the value not exists, the function returns 0.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// </summary>
      public double GetReal(string key)
      {
        return GetReal(key, false);
      }

      /// <summary>
      /// Sets the entry to a direct double value.
      /// </summary>
      public void SetReal(string key, double value)
      {
        this[key] = new PdfReal(value);
      }

      /// <summary>
      /// Converts the specified value to String.
      /// If the value not exists, the function returns the empty string.
      /// </summary>
      public string GetString(string key, bool create)
      {
        object obj = this[key];
        if (obj == null)
        {
          if (create)
            this[key] = new PdfString();
          return "";
        }
        if (obj is PdfReference)
          obj = ((PdfReference)obj).Value;

        if (obj is PdfString)
          return ((PdfString)obj).Value;
        
        if (obj is PdfStringObject)
          return ((PdfStringObject)obj).Value;
        
        if (obj is PdfName)
          return ((PdfName)obj).Value;
        
        if (obj is PdfNameObject)
          return ((PdfNameObject)obj).Value;
        
        throw new InvalidCastException("GetString: Object is not a string.");
      }

      /// <summary>
      /// Converts the specified value to String.
      /// If the value not exists, the function returns the empty string.
      /// </summary>
      public string GetString(string key)
      {
        return GetString(key, false);
      }

      /// <summary>
      /// Sets the entry to a direct string value.
      /// </summary>
      public void SetString(string key, string value)
      {
        this[key] = new PdfString(value);
      }

      /// <summary>
      /// Converts the specified value to a name.
      /// If the value not exists, the function returns the empty string.
      /// </summary>
      public string GetName(string key)
      {
        object obj = this[key];
        if (obj == null)
        {
          //if (create)
          //  this[key] = new Pdf();
          return String.Empty;
        }
        if (obj is PdfReference)
          obj = ((PdfReference)obj).Value;

        if (obj is PdfName)
          return ((PdfName)obj).Value;
        
        if (obj is PdfNameObject)
          return ((PdfNameObject)obj).Value;

        throw new InvalidCastException("GetName: Object is not a name.");
      }

      /// <summary>
      /// Sets the specified name value.
      /// If the value doesn't start with a slash, it is added automatically.
      /// </summary>
      public void SetName(string key, string value)
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (value.Length == 0 || value[0] != '/')
          value = "/" + value;

        this[key] = new PdfName(value);
      }

      /// <summary>
      /// Converts the specified value to PdfRectangle.
      /// If the value not exists, the function returns an empty rectangle.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// </summary>
      public PdfRectangle GetRectangle(string key, bool create)
      {
        PdfRectangle value = new PdfRectangle();
        object obj = this[key];
        if (obj == null)
        {
          if (create)
            this[key] = value = new PdfRectangle();
          return value;
        }
        if (obj is PdfReference)
          obj = ((PdfReference)obj).Value;

        PdfArray array = obj as PdfArray;
        if (array != null && array.Elements.Count == 4)
        {
          value = new PdfRectangle(array.Elements.GetReal(0), array.Elements.GetReal(1),
            array.Elements.GetReal(2), array.Elements.GetReal(3));
          this[key] = value;
        }
        else
          value = (PdfRectangle)obj;
        return value;
      }

      /// <summary>
      /// Converts the specified value to PdfRectangle.
      /// If the value not exists, the function returns an empty rectangle.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// </summary>
      public PdfRectangle GetRectangle(string key)
      {
        return GetRectangle(key, false);
      }

      /// <summary>
      /// Sets the entry to a direct rectangle value, represented by an array with four values.
      /// </summary>
      public void SetRectangle(string key, PdfRectangle rect)
      {
        this.elements[key] = rect;
      }

      /// Converts the specified value to XMatrix.
      /// If the value not exists, the function returns an identity matrix.
      /// If the value is not convertible, the function throws an InvalidCastException.
      public XMatrix GetMatrix(string key, bool create)
      {
        XMatrix value = new XMatrix();
        object obj = this[key];
        if (obj == null)
        {
          if (create)
            this[key] = new PdfLiteral("[1 0 0 1 0 0]");  // cannot be parsed, implement a PdfMatrix...
          return value;
        }
        if (obj is PdfReference)
          obj = ((PdfReference)obj).Value;

        PdfArray array = obj as PdfArray;
        if (array != null && array.Elements.Count == 6)
        {
          value = new XMatrix(array.Elements.GetReal(0), array.Elements.GetReal(1), array.Elements.GetReal(2),
            array.Elements.GetReal(3), array.Elements.GetReal(4), array.Elements.GetReal(5));
        }
        else if (obj is PdfLiteral)
        {
          throw new NotImplementedException("Parsing matrix from literal.");
        }
        else
          throw new InvalidCastException("Element is not an array with 6 values.");
        return value;
      }

      /// Converts the specified value to XMatrix.
      /// If the value not exists, the function returns an identity matrix.
      /// If the value is not convertible, the function throws an InvalidCastException.
      public XMatrix GetMatrix(string key)
      {
        return GetMatrix(key, false);
      }

      /// <summary>
      /// Sets the entry to a direct matrix value, represented by an array with six values.
      /// </summary>
      public void SetMatrix(string key, XMatrix matrix)
      {
        this.elements[key] = PdfLiteral.FromMatrix(matrix);
      }

      /// <summary>
      /// Converts the specified value to DateTime.
      /// If the value not exists, the function returns the specified default value.
      /// If the value is not convertible, the function throws an InvalidCastException.
      /// </summary>
      public DateTime GetDateTime(string key, DateTime defaultValue)
      {
        object obj = this[key];
        if (obj == null)
        {
          //if (create)
          //  this[key] = new Pdf();
          return defaultValue;
        }
        if (obj is PdfReference)
          obj = ((PdfReference)obj).Value;

        if (obj is PdfDate)
          return ((PdfDate)obj).Value;

        string date;
        if (obj is PdfString)
          date = ((PdfString)obj).Value;
        else if (obj is PdfStringObject)
          date = ((PdfStringObject)obj).Value;
        else
          throw new InvalidCastException("GetName: Object is not a name.");

        if (date != "")
        {
          try
          {
            defaultValue = Parser.ParseDateTime(date, defaultValue);
          }
          catch { }
        }
        return defaultValue;
      }

      /// <summary>
      /// Sets the entry to a direct datetime value.
      /// </summary>
      public void SetDateTime(string key, DateTime value)
      {
        this.elements[key] = new PdfDate(value);
      }

      internal int GetEnumFromName(string key, object defaultValue, bool create)
      {
        if (!(defaultValue is Enum))
          throw new ArgumentException("defaultValue");

        object obj = this[key];
        if (obj == null)
        {
          if (create)
            this[key] = new PdfName(defaultValue.ToString());
          return (int)defaultValue;
        }
        Debug.Assert(obj is Enum);
        return (int)Enum.Parse(defaultValue.GetType(), obj.ToString().Substring(1), false);
      }

      internal int GetEnumFromName(string key, object defaultValue)
      {
        return GetEnumFromName(key, defaultValue, false);
      }

      internal void SetEnumAsName(string key, object value)
      {
        if (!(value is Enum))
          throw new ArgumentException("value");
        this.elements[key] = new PdfName("/" + value);
      }

      /// <summary>
      /// Gets the value for the specified key. If the value does not exist, it is optionally created.
      /// </summary>
      public PdfItem GetValue(string key, VCF options)
      {
        PdfObject obj;
        PdfDictionary dict;
        PdfArray array;
        PdfReference iref;
        PdfItem value = this[key];
        if (value == null)
        {
          if (options != VCF.None)
          {
            Type type = GetValueType(key);
            if (type != null)
            {
              Debug.Assert(typeof(PdfItem).IsAssignableFrom(type), "Type not allowed.");
              if (typeof(PdfDictionary).IsAssignableFrom(type))
              {
                value = obj = CreateDictionary(type, null);
              }
              else if (typeof(PdfArray).IsAssignableFrom(type))
              {
                value = obj = CreateArray(type, null);
              }
              else
                throw new NotImplementedException("Type other than array or dictionary.");

              if (options == VCF.CreateIndirect)
              {
                this.owner.Owner.irefTable.Add(obj);
                this[key] = obj.Reference;
              }
              else
                this[key] = obj;
            }
            else
              throw new NotImplementedException("Cannot create value for key: " + key);
          }
        }
        else
        {
          // The value exists and can returned. But for imported documents check for necessary
          // object type transformation.
          if ((iref = value as PdfReference) != null)
          {
            // Case: value is an indirect reference
            value = iref.Value;
            if (value == null)
            {
              // If we come here PDF file is currupt
              throw new InvalidOperationException("Indirect reference without value.");
            }
            else
            {
              if (true) // || this.owner.Document.IsImported)
              {
                Type type = GetValueType(key);
                Debug.Assert(type != null, "No value type specified in meta information. Please send this file to PDFsharp support.");

                if (type != null && type != value.GetType())
                {
                  if (typeof(PdfDictionary).IsAssignableFrom(type))
                  {
                    Debug.Assert(value is PdfDictionary, "Bug in PDFsharp. Please send this file to PDFsharp support.");
                    value = CreateDictionary(type, (PdfDictionary)value);
                  }
                  else if (typeof(PdfArray).IsAssignableFrom(type))
                  {
                    Debug.Assert(value is PdfArray, "Bug in PDFsharp. Please send this file to PDFsharp support.");
                    value = CreateArray(type, (PdfArray)value);
                  }
                  else
                    throw new NotImplementedException("Type other than array or dictionary.");
                }
              }
            }
            return value;
          }
          // Transformation is only possible after PDF import.
          if (true) // || this.owner.Document.IsImported)
          {
            // Case: value is a direct object
            if ((dict = value as PdfDictionary) != null)
            {
              Type type = GetValueType(key);
              Debug.Assert(type != null, "No value type specified in meta information. Please send this file to PDFsharp support.");
              if (dict.GetType() != type)
                dict = CreateDictionary(type, dict);
              return dict;
            }
            
            if ((array = value as PdfArray) != null)
            {
              Type type = GetValueType(key);
              Debug.Assert(type != null, "No value type specified in meta information. Please send this file to PDFsharp support.");
              if (array.GetType() != type)
                array = CreateArray(type, array);
              return array;
            }
          }
        }
        return value;
      }

      /// <summary>
      /// Short cut for GetValue(key, VCF.None).
      /// </summary>
      public PdfItem GetValue(string key)
      {
        return GetValue(key, VCF.None);
      }

      /// <summary>
      /// Returns the type of the object to be created as value of the specified key.
      /// </summary>
      Type GetValueType(string key)  // TODO: move to PdfObject
      {
        Type type = null;
        DictionaryMeta meta = this.owner.Meta;
        if (meta != null)
        {
          KeyDescriptor kd = meta[key];
          if (kd != null)
            type = kd.GetValueType();
          else
            Debug.WriteLine("Warning: Key not desciptor table: " + key);  // TODO: check what this means...
        }
        else
          Debug.WriteLine("Warning: No meta provided for type: " + this.owner.GetType().Name);  // TODO: check what this means...
        return type;
      }

      PdfArray CreateArray(Type type, PdfArray oldArray)
      {
        ConstructorInfo ctorInfo;
        PdfArray array;
        if (oldArray == null)
        {
          // Use contstructor with signature 'Ctor(PdfDocument owner)'.
          ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new Type[] { typeof(PdfDocument) }, null);
          Debug.Assert(ctorInfo != null, "No appropriate constructor found for type: " + type.Name);
          array = ctorInfo.Invoke(new object[] { this.owner.Owner }) as PdfArray;
        }
        else
        {
          // Use contstructor with signature 'Ctor(PdfDictionary dict)'.
          ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new Type[] { typeof(PdfArray) }, null);
          Debug.Assert(ctorInfo != null, "No appropriate constructor found for type: " + type.Name);
          array = ctorInfo.Invoke(new object[] { oldArray }) as PdfArray;
        }
        return array;
      }

      PdfDictionary CreateDictionary(Type type, PdfDictionary oldDictionary)
      {
        ConstructorInfo ctorInfo;
        PdfDictionary dict;
        if (oldDictionary == null)
        {
          // Use contstructor with signature 'Ctor(PdfDocument owner)'.
          ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new Type[] { typeof(PdfDocument) }, null);
          Debug.Assert(ctorInfo != null, "No appropriate constructor found for type: " + type.Name);
          dict = ctorInfo.Invoke(new object[] { this.owner.Owner }) as PdfDictionary;
        }
        else
        {
          // Use contstructor with signature 'Ctor(PdfDictionary dict)'.
          ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, new Type[] { typeof(PdfDictionary) }, null);
          Debug.Assert(ctorInfo != null, "No appropriate constructor found for type: " + type.Name);
          dict = ctorInfo.Invoke(new object[] { oldDictionary }) as PdfDictionary;
        }
        return dict;
      }

      PdfItem CreateValue(Type type, PdfDictionary oldValue)
      {
        ConstructorInfo ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
          null, new Type[] { typeof(PdfDocument) }, null);
        PdfObject obj = ctorInfo.Invoke(new object[] { this.owner.Owner }) as PdfObject;
        if (oldValue != null)
        {
          obj.Reference = oldValue.Reference;
          obj.Reference.Value = obj;
          if (obj is PdfDictionary)
          {
            PdfDictionary dict = (PdfDictionary)obj;
            dict.elements = oldValue.elements;
          }
        }
        return obj;
      }

      /// <summary>
      /// Sets the entry with the specified value. DON'T USE THIS FUNCTION - IT MAY BE REMOVED.
      /// </summary>
      public void SetValue(string key, PdfItem value)
      {
        Debug.Assert((value is PdfObject && ((PdfObject)value).Reference == null) | !(value is PdfObject),
          "You try to set an indirect object directly into a dictionary.");

        // HACK?
        this.elements[key] = value;
      }

      /// <summary>
      /// Returns the indirect object if the value of the specified key is a PdfReference.
      /// </summary>
      [Obsolete("Use GetObject, GetDictionary, GetArray, or GetReference")]
      public PdfObject GetIndirectObject(string key)
      {
        PdfItem item = this[key];
        if (item is PdfReference)
          return ((PdfReference)item).Value;
        return null;
      }

      /// <summary>
      /// Gets the PdfObject with the specified key, or null, if no such object exists. If the key refers to
      /// a reference, the referenced PdfObject is returned.
      /// </summary>
      public PdfObject GetObject(string key)
      {
        PdfItem item = this[key];
        if (item is PdfReference)
          return ((PdfReference)item).Value;
        return item as PdfObject;
      }

      /// <summary>
      /// Gets the PdfArray with the specified key, or null, if no such object exists. If the key refers to
      /// a reference, the referenced PdfDictionary is returned.
      /// </summary>
      public PdfDictionary GetDictionary(string key)
      {
        return GetObject(key) as PdfDictionary;
      }

      /// <summary>
      /// Gets the PdfArray with the specified key, or null, if no such object exists. If the key refers to
      /// a reference, the referenced PdfArray is returned.
      /// </summary>
      public PdfArray GetArray(string key)
      {
        return GetObject(key) as PdfArray;
      }

      /// <summary>
      /// Gets the PdfReference with the specified key, or null, if no such object exists.
      /// </summary>
      public PdfReference GetReference(string key)
      {
        PdfItem item = this[key];
        return item as PdfReference;
      }

      /// <summary>
      /// Sets the entry to the specified object. The object must not be an indirect object,
      /// otherwise an exception is raised.
      /// </summary>
      public void SetObject(string key, PdfObject obj)
      {
        if (obj.Reference != null)
          throw new ArgumentException("PdfObject must not be an indirect object.", "obj");
        this[key] = obj;
      }

      /// <summary>
      /// Sets the entry as a reference to the specified object. The object must be an indirect object,
      /// otherwise an exception is raised.
      /// </summary>
      public void SetReference(string key, PdfObject obj)
      {
        if (obj.Reference == null)
          throw new ArgumentException("PdfObject must be an indirect object.", "obj");
        this[key] = obj.Reference;
      }

      #region IDictionary Members

      /// <summary>
      /// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object is read-only.
      /// </summary>
      public bool IsReadOnly
      {
        get { return false; }
      }

      /// <summary>
      /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
      /// </summary>
      public IEnumerator<KeyValuePair<string, PdfItem>> GetEnumerator()
      {
        return this.elements.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return ((ICollection)this.elements).GetEnumerator();
      }

      //object IDictionary.this[object key]
      //{
      //  get { return this.elements[key]; }
      //  set
      //  {
      //    if (key == null)
      //      throw new ArgumentNullException("key");
      //    if (!(key is string))
      //      throw new ArgumentException("Key must be of type System.String.");
      //    if (((string)key) == "")
      //      throw new ArgumentException(PSSR.NameMustStartWithSlash, "key");
      //    if (((string)key)[0] != '/')
      //      throw new ArgumentException(PSSR.NameMustStartWithSlash, "key");

      //    this.elements[key] = value;
      //  }
      //}

      /// <summary>
      /// Gets or sets an entry in the dictionary. The specified key must be a valid PDF name
      /// starting with a slash '/'. This property provides full access to the elements of the
      /// PDF dictionary. Wrong use can lead to errors or corrupt PDF files.
      /// </summary>
      public PdfItem this[string key]
      {
        get
        {
          PdfItem item;
          this.elements.TryGetValue(key, out item);
          return item;
        }
        set
        {
          if (value == null)
            throw new ArgumentNullException("value");
#if DEBUG
          if (key == "/MediaBox")
            key.GetType();

          //if (value is PdfObject)
          //{
          //  PdfObject obj = (PdfObject)value;
          //  if (obj.Reference != null)
          //    throw new ArgumentException("An object with an indirect reference cannot be a direct value. Try to set an indirect refernece.");
          //}
          if (value is PdfDictionary)
          {
            PdfDictionary dict = (PdfDictionary)value;
            if (dict.stream != null)
              throw new ArgumentException("A dictionary with stream cannot be a direct value.");
          }
#endif
          PdfObject obj = value as PdfObject;
          if (obj != null && obj.IsIndirect)
            value = obj.Reference;
          this.elements[key] = value;
        }
      }

      /// <summary>
      /// Gets or sets an entry in the dictionary identified by a PdfName object.
      /// </summary>
      public PdfItem this[PdfName key]
      {
        get { return this[key.Value]; }
        set
        {
          if (value == null)
            throw new ArgumentNullException("value");

#if DEBUG
          if (value is PdfDictionary)
          {
            PdfDictionary dict = (PdfDictionary)value;
            if (dict.stream != null)
              throw new ArgumentException("A dictionary with stream cannot be a direct value.");
          }
#endif

          PdfObject obj = value as PdfObject;
          if (obj != null && obj.IsIndirect)
            value = obj.Reference;
          this.elements[key.Value] = value;
        }
      }

      /// <summary>
      /// Removes the value with the specified key.
      /// </summary>
      public bool Remove(string key)
      {
        return this.elements.Remove(key);
      }

      /// <summary>
      /// Removes the value with the specified key.
      /// </summary>
      public bool Remove(KeyValuePair<string, PdfItem> item)
      {
        throw new NotImplementedException();
      }

      /// <summary>
      /// Determines whether the dictionary contains the specified name.
      /// </summary>
      [Obsolete("Use ContainsKey.")]
      public bool Contains(string key)
      {
        return this.elements.ContainsKey(key);
      }

      /// <summary>
      /// Determines whether the dictionary contains the specified name.
      /// </summary>
      public bool ContainsKey(string key)
      {
        return this.elements.ContainsKey(key);
      }

      /// <summary>
      /// Determines whether the dictionary contains a specific value.
      /// </summary>
      public bool Contains(KeyValuePair<string, PdfItem> item)
      {
        throw new NotImplementedException();
      }

      /// <summary>
      /// Removes all elements from the dictionary.
      /// </summary>
      public void Clear()
      {
        this.elements.Clear();
      }

      //void IDictionary.Add(object key, object value)
      //{
      //  if (key == null)
      //    throw new ArgumentNullException("key");
      //  if (key is PdfName)
      //    key = (key as PdfName).Value;
      //  if (!(key is string))
      //    throw new ArgumentException("key must be of type System.String.");
      //  if (((string)key) == "")
      //    throw new ArgumentException("key");
      //  if (((string)key)[0] != '/')
      //    throw new ArgumentException("The key must start with a slash '/'.");

      //  // If object is indirect automatically convert value to reference.
      //  PdfObject obj = value as PdfObject;
      //  if (obj != null && obj.IsIndirect)
      //    value = obj.Reference;

      //  this.elements.Add(key, value);
      //}

      /// <summary>
      /// Adds the specified value to the dictionary.
      /// </summary>
      public void Add(string key, PdfItem value)
      {
        if (String.IsNullOrEmpty(key))
          throw new ArgumentNullException("key");

        if (key[0] != '/')
          throw new ArgumentException("The key must start with a slash '/'.");

        // If object is indirect automatically convert value to reference.
        PdfObject obj = value as PdfObject;
        if (obj != null && obj.IsIndirect)
          value = obj.Reference;

        this.elements.Add(key, value);
      }

      /// <summary>
      /// Adds an item to the dictionary.
      /// </summary>
      public void Add(KeyValuePair<string, PdfItem> item)
      {
        Add(item.Key, item.Value);
      }

      /// <summary>
      /// Gets all keys currently in use in this dictionary as an array of PdfName objects.
      /// </summary>
      public PdfName[] KeyNames
      {
        get
        {
          ICollection values = this.elements.Keys;
          int count = values.Count;
          string[] strings = new string[count];
          values.CopyTo(strings, 0);
          PdfName[] names = new PdfName[count];
          for (int idx = 0; idx < count; idx++)
            names[idx] = new PdfName(strings[idx]);
          return names;
        }
      }

      /// <summary>
      /// Get all keys currently in use in this dictionary as an array of string objects.
      /// </summary>
      public ICollection<string> Keys
      {
        get
        {
          ICollection values = this.elements.Keys;
          int count = values.Count;
          string[] keys = new string[count];
          values.CopyTo(keys, 0);
          return keys;
        }
      }

      /// <summary>
      /// Gets the value associated with the specified key.
      /// </summary>
      public bool TryGetValue(string key, out PdfItem value)
      {
        return this.elements.TryGetValue(key, out value);
      }

      /// <summary>
      /// Gets all values currently in use in this dictionary as an array of PdfItem objects.
      /// </summary>
      public ICollection<PdfItem> Values
      {
        get
        {
          ICollection values = this.elements.Values;
          PdfItem[] items = new PdfItem[values.Count];
          values.CopyTo(items, 0);
          return items;
        }
      }

      /// <summary>
      /// Return false.
      /// </summary>
      public bool IsFixedSize
      {
        get { return false; }
      }

      #endregion

      #region ICollection Members

      /// <summary>
      /// Return false.
      /// </summary>
      public bool IsSynchronized
      {
        get { return false; }
      }

      /// <summary>
      /// Gets the number of elements contained in the dictionary.
      /// </summary>
      public int Count
      {
        get { return this.elements.Count; }
      }

      ///// <param name="array">The one-dimensional array that is the destination of the elements copied from.</param>
      ///// <param name="index">The zero-based index in array at which copying begins.</param>
      //public void CopyTo(Array array, int index)
      //{
      //  //this.elements.CopyTo(array, index);
      //  throw new NotImplementedException();
      //}

      /// <summary>
      /// Copies the elements of the dictionary to an array, starting at a particular index.
      /// </summary>
      public void CopyTo(KeyValuePair<string, PdfItem>[] array, int arrayIndex)
      {
        throw new NotImplementedException();
      }

      /// <summary>
      /// The current implementation returns null.
      /// </summary>
      public object SyncRoot
      {
        get { return null; }
      }

      #endregion
    }

    /// <summary>
    /// The PDF stream objects.
    /// </summary>
    public sealed class PdfStream
    {
      /// <summary>
      /// The dictionary the stream belongs to.
      /// </summary>
      PdfDictionary owner;

      internal PdfStream(PdfDictionary owner)
      {
        if (owner == null)
          throw new ArgumentNullException("owner");
        this.owner = owner;
      }

      /// <summary>
      /// A .NET string can contain char(0) as a valid character.
      /// </summary>
      internal PdfStream(byte[] value, PdfDictionary owner)
        : this(owner)
      {
        this.value = value;
      }

      /// <summary>
      /// Clones this stream by creating a deep copy.
      /// </summary>
      public PdfStream Clone()
      {
        PdfStream stream = (PdfStream)MemberwiseClone();
        stream.owner = null;
        if (stream.value != null)
        {
          stream.value = new byte[stream.value.Length];
          this.value.CopyTo(stream.value, 0);
        }
        return stream;
      }

      /// <summary>
      /// Moves this instance to another dictionary during object type transformation.
      /// </summary>
      internal void SetOwner(PdfDictionary dict)
      {
        this.owner = dict;
        owner.stream = this;
      }

      /// <summary>
      /// Gets the length of the stream, i.e. the actual number of bytes in the stream.
      /// </summary>
      public int Length
      {
        get { return this.value != null ? value.Length : 0; }
      }

      //    /// <summary>
      //    /// Gets the native length of the stream, i.e. the number of bytes when they are neither
      //    /// compressed nor encrypted.
      //    /// </summary>
      //    public int Length
      //    {
      //      get {return this.length;}
      //    }

      /// <summary>
      /// Get or sets the bytes of the stream as they are, i.e. if one or more filters exists the bytes are
      /// not unfiltered.
      /// </summary>
      public byte[] Value
      {
        get { return this.value; }
        set
        {
          if (value == null)
            throw new ArgumentNullException("value");
          this.value = value;
          this.owner.Elements.SetInteger(Keys.Length, value.Length);
        }
      }
      byte[] value;

      /// <summary>
      /// Gets the value of the stream unfiltered. The stream content is not modified by this operation.
      /// </summary>
      public byte[] UnfilteredValue
      {
        get
        {
          byte[] bytes = null;
          if (this.value != null)
          {
            PdfItem filter = this.owner.Elements["/Filter"];
            if (filter != null)
            {
              bytes = Filtering.Decode(this.value, filter);
              if (bytes == null)
              {
                string message = String.Format("«Cannot decode filter '{0}'»", filter);
                bytes = PdfEncoders.RawEncoding.GetBytes(message);
              }
            }
            else
            {
              bytes = new byte[this.value.Length];
              this.value.CopyTo(bytes, 0);
            }
          }
          return bytes ?? new byte[0];
        }
      }

      /// <summary>
      /// Tries to unfilter the bytes of the stream. If the stream is filtered and PDFsharp knows the filter
      /// algorithm, the stream content is replaced by its unfiltered value and the function returns true.
      /// Otherwise the content remains untouched and the function returns false.
      /// The function is useful for analyzing existing PDF files.
      /// </summary>
      public bool TryUnfilter()
      {
        if (this.value != null)
        {
          PdfItem filter = this.owner.Elements["/Filter"];
          if (filter != null)
          {
            // PDFsharp can only uncompress streams that are compressed with
            // the ZIP or LHZ algorithm.
            byte[] bytes = Filtering.Decode(this.value, filter);
            if (bytes != null)
            {
              this.owner.Elements.Remove(Keys.Filter);
              Value = bytes;
            }
            else
              return false;
          }
        }
        return true;
      }

      /// <summary>
      /// Compresses the stream with the FlateDecode filter.
      /// If a filter is already defined, the function has no effect.
      /// </summary>
      public void Zip()
      {
        if (this.value == null)
          return;

        if (!this.owner.Elements.ContainsKey("/Filter"))
        {
          this.value = Filtering.FlateDecode.Encode(this.value);
          this.owner.Elements["/Filter"] = new PdfName("/FlateDecode");
          this.owner.Elements["/Length"] = new PdfInteger(this.value.Length);
        }
      }

      /// <summary>
      /// Returns the stream content as a raw string.
      /// </summary>
      public override string ToString()
      {
        if (this.value == null)
          return "«null»";

        string stream;
        PdfItem filter = this.owner.Elements["/Filter"];
        if (filter != null)
        {
#if true
          byte[] bytes = Filtering.Decode(this.value, filter);
          if (bytes != null)
            stream = PdfEncoders.RawEncoding.GetString(bytes, 0, bytes.Length);
#else

          if (this.owner.Elements.GetString("/Filter") == "/FlateDecode")
          {
            stream = Filtering.FlateDecode.DecodeToString(this.value);
          }
#endif
          else
            throw new NotImplementedException("Unknown filter");
        }
        else
          stream = PdfEncoders.RawEncoding.GetString(this.value, 0, this.value.Length);

        return stream;
      }

      //internal void WriteObject_(Stream stream)
      //{
      //  if (this.value != null)
      //    stream.Write(this.value, 0, this.value.Length);
      //}

      /// <summary>
      /// Converts a raw encoded string into a byte array.
      /// </summary>
      public static byte[] RawEncode(string content)
      {
        return PdfEncoders.RawEncoding.GetBytes(content);
      }

      /// <summary>
      /// Common keys for all streams.
      /// </summary>
      public class Keys : KeysBase
      {
        /// <summary>
        /// (Required) The number of bytes from the beginning of the line following the keyword
        /// stream to the last byte just before the keyword endstream. (There may be an additional
        /// EOL marker, preceding endstream, that is not included in the count and is not logically
        /// part of the stream data.)
        /// </summary>
        [KeyInfo(KeyType.Integer | KeyType.Required)]
        public const string Length = "/Length";

        /// <summary>
        /// (Optional) The name of a filter to be applied in processing the stream data found between
        /// the keywords stream and endstream, or an array of such names. Multiple filters should be
        /// specified in the order in which they are to be applied.
        /// </summary>
        [KeyInfo(KeyType.NameOrArray | KeyType.Optional)]
        public const string Filter = "/Filter";

        /// <summary>
        /// (Optional) A parameter dictionary or an array of such dictionaries, used by the filters
        /// specified by Filter. If there is only one filter and that filter has parameters, DecodeParms
        /// must be set to the filter’s parameter dictionary unless all the filter’s parameters have
        /// their default values, in which case the DecodeParms entry may be omitted. If there are 
        /// multiple filters and any of the filters has parameters set to nondefault values, DecodeParms
        /// must be an array with one entry for each filter: either the parameter dictionary for that
        /// filter, or the null object if that filter has no parameters (or if all of its parameters have
        /// their default values). If none of the filters have parameters, or if all their parameters
        /// have default values, the DecodeParms entry may be omitted.
        /// </summary>
        [KeyInfo(KeyType.ArrayOrDictionary | KeyType.Optional)]
        public const string DecodeParms = "/DecodeParms";

        /// <summary>
        /// (Optional; PDF 1.2) The file containing the stream data. If this entry is present, the bytes
        /// between stream and endstream are ignored, the filters are specified by FFilter rather than
        /// Filter, and the filter parameters are specified by FDecodeParms rather than DecodeParms.
        /// However, the Length entry should still specify the number of those bytes. (Usually, there are
        /// no bytes and Length is 0.)
        /// </summary>
        [KeyInfo("1.2", KeyType.String | KeyType.Optional)]
        public const string F = "/F";

        /// <summary>
        /// (Optional; PDF 1.2) The name of a filter to be applied in processing the data found in the
        /// stream’s external file, or an array of such names. The same rules apply as for Filter.
        /// </summary>
        [KeyInfo("1.2", KeyType.NameOrArray | KeyType.Optional)]
        public const string FFilter = "/FFilter";

        /// <summary>
        /// (Optional; PDF 1.2) A parameter dictionary, or an array of such dictionaries, used by the
        /// filters specified by FFilter. The same rules apply as for DecodeParms.
        /// </summary>
        [KeyInfo("1.2", KeyType.ArrayOrDictionary | KeyType.Optional)]
        public const string FDecodeParms = "/FDecodeParms";

        /// <summary>
        /// Optional; PDF 1.5) A non-negative integer representing the number of bytes in the decoded
        /// (defiltered) stream. It can be used to determine, for example, whether enough disk space is
        /// available to write a stream to a file.
        /// This value should be considered a hint only; for some stream filters, it may not be possible
        /// to determine this value precisely.
        /// </summary>
        [KeyInfo("1.5", KeyType.Integer | KeyType.Optional)]
        public const string DL = "/DL";
      }
    }
  }
}