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
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Represents a PDF object identifier, a pair of object and generation number.
  /// </summary>
  [DebuggerDisplay("({ObjectNumber}, {GenerationNumber})")]
  public struct PdfObjectID : IComparable
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfObjectID"/> class.
    /// </summary>
    /// <param name="objectNumber">The object number.</param>
    public PdfObjectID(int objectNumber)
    {
      Debug.Assert(objectNumber >= 1, "Object number out of range.");
      this.objectNumber = objectNumber;
      this.generationNumber = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfObjectID"/> class.
    /// </summary>
    /// <param name="objectNumber">The object number.</param>
    /// <param name="generationNumber">The generation number.</param>
    public PdfObjectID(int objectNumber, int generationNumber)
    {
      Debug.Assert(objectNumber >= 1, "Object number out of range.");
      //Debug.Assert(generationNumber >= 0 && generationNumber <= 65535, "Generation number out of range.");
#if DEBUG
      // iText creates generation numbers with a value of 65536... 
      if (generationNumber > 65535)
        Debug.WriteLine(String.Format("Generation number: {0}", generationNumber));
#endif
      this.objectNumber = objectNumber;
      this.generationNumber = (ushort)generationNumber;
    }

    /// <summary>
    /// Gets or sets the object number.
    /// </summary>
    public int ObjectNumber
    {
      get { return this.objectNumber; }
      set { this.objectNumber = value; }
    }
    int objectNumber;

    /// <summary>
    /// Gets or sets the generation number.
    /// </summary>
    public int GenerationNumber
    {
      get { return this.generationNumber; }
      set { this.generationNumber = (ushort)value; }
    }
    ushort generationNumber;

    /// <summary>
    /// Indicates whether this object is an empty object identifier.
    /// </summary>
    public bool IsEmpty
    {
      get { return this.objectNumber == 0; }
    }

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    public override bool Equals(object obj)
    {
      if (obj is PdfObjectID)
      {
        PdfObjectID id = (PdfObjectID)obj;
        if (this.objectNumber == id.objectNumber)
          return this.generationNumber == id.generationNumber;
      }
      return false;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return this.objectNumber ^ this.generationNumber;
    }

    /// <summary>
    /// Determines whether the tow objects are equal.
    /// </summary>
    public static bool operator ==(PdfObjectID left, PdfObjectID right)
    {
      return left.Equals(right);
    }

    /// <summary>
    /// Determines whether the tow objects not are equal.
    /// </summary>
    public static bool operator !=(PdfObjectID left, PdfObjectID right)
    {
      return !left.Equals(right);
    }

    /// <summary>
    /// Returns the object and generation numbers as a string.
    /// </summary>
    public override string ToString()
    {
      return objectNumber.ToString(CultureInfo.InvariantCulture) + " " + generationNumber.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Creates an empty object identifier.
    /// </summary>
    public static PdfObjectID Empty
    {
      get { return new PdfObjectID(); }
    }

    /// <summary>
    /// Compares the current object id with another object.
    /// </summary>
    public int CompareTo(object obj)
    {
      if (obj is PdfObjectID)
      {
        PdfObjectID id = (PdfObjectID)obj;
        if (this.objectNumber == id.objectNumber)
          return this.generationNumber - id.generationNumber;
        return this.objectNumber - id.objectNumber;
      }
      return 1;
    }
  }
}
