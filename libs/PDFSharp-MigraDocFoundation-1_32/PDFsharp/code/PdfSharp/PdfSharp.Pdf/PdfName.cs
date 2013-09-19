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
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Represents a PDF name value.
  /// </summary>
  [DebuggerDisplay("({Value})")]
  public sealed class PdfName : PdfItem
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfName"/> class.
    /// </summary>
    public PdfName()
    {
      this.value = "/";  // Empty name.
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfName"/> class.
    /// </summary>
    public PdfName(string value)
    {
      if (value == null)
        throw new ArgumentNullException("value");
      if (value.Length == 0 || value[0] != '/')
        throw new ArgumentException(PSSR.NameMustStartWithSlash);

      this.value = value;
    }

    /// <summary>
    /// Determines whether the specified object is equal to this name.
    /// </summary>
    public override bool Equals(object obj)
    {
      return this.value.Equals(obj);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return this.value.GetHashCode();
    }

    /// <summary>
    /// Gets the name as a string.
    /// </summary>
    public string Value
    {
      // This class must behave like a value type. Therefore it cannot be changed (like System.String).
      get { return this.value; }
    }
    string value;

    /// <summary>
    /// Returns the name. The string always begins with a slash.
    /// </summary>
    public override string ToString()
    {
      return this.value;
    }

    /// <summary>
    /// Determines whether the specified name and string are equal.
    /// </summary>
    public static bool operator ==(PdfName name, string str)
    {
      return name.value == str;
    }

    /// <summary>
    /// Determines whether the specified name and string are not equal.
    /// </summary>
    public static bool operator !=(PdfName name, string str)
    {
      return name.value != str;
    }

#if leads_to_ambiguity
    public static bool operator ==(string str, PdfName name)
    {
      return str == name.value;
    }

    public static bool operator !=(string str, PdfName name)
    {
      return str == name.value;
    }

    public static bool operator ==(PdfName name1, PdfName name2)
    {
      return name1.value == name2.value;
    }

    public static bool operator !=(PdfName name1, PdfName name2)
    {
      return name1.value != name2.value;
    }
#endif

    /// <summary>
    /// Represents the empty name.
    /// </summary>
    public static readonly PdfName Empty = new PdfName("/");

    /// <summary>
    /// Writes the name including the leading slash.
    /// </summary>
    internal override void WriteObject(PdfWriter writer)
    {
      // TODO: what if unicode character are part of the name? 
      writer.Write(this);
    }

    /// <summary>
    /// Gets the comparer for this type.
    /// </summary>
    public static PdfXNameComparer Comparer
    {
      get { return new PdfXNameComparer(); }
    }

    /// <summary>
    /// Implements a comparer that compares PdfName objects.
    /// </summary>
    public class PdfXNameComparer : IComparer<PdfName>
    {
      /// <summary>
      /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
      /// </summary>
      /// <param name="x">The first object to compare.</param>
      /// <param name="y">The second object to compare.</param>
      public int Compare(PdfName x, PdfName y)
      {
        PdfName l = x; // as PdfName;
        PdfName r = y; // as PdfName;
        if (l != null)
        {
          if (r != null)
            return l.value.CompareTo(r.value);
          else
            return -1;
        }
        else if (r != null)
          return 1;
        else
          return 0;
      }
    }
  }
}
