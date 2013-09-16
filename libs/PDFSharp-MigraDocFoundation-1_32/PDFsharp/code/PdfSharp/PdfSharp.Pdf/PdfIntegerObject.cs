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
using System.Globalization;
using System.Collections;
using System.Text;
using System.IO;
using PdfSharp.Internal;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Represents an indirect integer value. This type is not used by PDFsharp. If it is imported from
  /// an external PDF file, the value is converted into a direct object.
  /// </summary>
  [DebuggerDisplay("({Value})")]
  public sealed class PdfIntegerObject : PdfNumberObject
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfIntegerObject"/> class.
    /// </summary>
    public PdfIntegerObject()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfIntegerObject"/> class.
    /// </summary>
    public PdfIntegerObject(int value)
    {
      this.value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfIntegerObject"/> class.
    /// </summary>
    public PdfIntegerObject(PdfDocument document, int value)
      : base(document)
    {
      this.value = value;
    }

    /// <summary>
    /// Gets the value as integer.
    /// </summary>
    public int Value
    {
      get { return this.value; }
      //set {this.value = value;}
    }
    int value;

    /// <summary>
    /// Returns the integer as string.
    /// </summary>
    public override string ToString()
    {
      return value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Writes the integer literal.
    /// </summary>
    internal override void WriteObject(PdfWriter writer)
    {
      writer.WriteBeginObject(this);
      writer.Write(this.value);
      writer.WriteEndObject();
    }
  }
}
