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
  /// Represents a direct unsigned integer value.
  /// </summary>
  [DebuggerDisplay("({Value})")]
  public sealed class PdfUInteger : PdfNumber, IConvertible
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfUInteger"/> class.
    /// </summary>
    public PdfUInteger()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfUInteger"/> class.
    /// </summary>
    public PdfUInteger(uint value)
    {
      this.value = value;
    }

    /// <summary>
    /// Gets the value as integer.
    /// </summary>
    public uint Value
    {
      // This class must behave like a value type. Therefore it cannot be changed (like System.String).
      get { return this.value; }
    }
    uint value;

    /// <summary>
    /// Returns the unsigned integer as string.
    /// </summary>
    public override string ToString()
    {
      return value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Writes the integer as string.
    /// </summary>
    internal override void WriteObject(PdfWriter writer)
    {
      writer.Write(this);
    }

    #region IConvertible Members

    /// <summary>
    /// Converts the value of this instance to an equivalent 64-bit unsigned integer.
    /// </summary>
    public ulong ToUInt64(IFormatProvider provider)
    {
      return Convert.ToUInt64(this.value);
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent 8-bit signed integer.
    /// </summary>
    public sbyte ToSByte(IFormatProvider provider)
    {
      throw new InvalidCastException();
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent double-precision floating-point number.
    /// </summary>
    public double ToDouble(IFormatProvider provider)
    {
      return value;
    }

    /// <summary>
    /// Returns an undefined DateTime structure.
    /// </summary>
    public DateTime ToDateTime(IFormatProvider provider)
    {
      // TODO:  Add PdfUInteger.ToDateTime implementation
      return new DateTime();
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent single-precision floating-point number.
    /// </summary>
    public float ToSingle(IFormatProvider provider)
    {
      return value;
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent Boolean value.
    /// </summary>
    public bool ToBoolean(IFormatProvider provider)
    {
      return Convert.ToBoolean(this.value);
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent 32-bit signed integer.
    /// </summary>
    public int ToInt32(IFormatProvider provider)
    {
      return Convert.ToInt32(this.value);
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent 16-bit unsigned integer.
    /// </summary>
    public ushort ToUInt16(IFormatProvider provider)
    {
      return Convert.ToUInt16(this.value);
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent 16-bit signed integer.
    /// </summary>
    public short ToInt16(IFormatProvider provider)
    {
      return Convert.ToInt16(this.value);
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent <see cref="T:System.String"></see>.
    /// </summary>
    string System.IConvertible.ToString(IFormatProvider provider)
    {
      return value.ToString(provider);
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent 8-bit unsigned integer.
    /// </summary>
    public byte ToByte(IFormatProvider provider)
    {
      return Convert.ToByte(this.value);
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent Unicode character.
    /// </summary>
    public char ToChar(IFormatProvider provider)
    {
      return Convert.ToChar(this.value);
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent 64-bit signed integer.
    /// </summary>
    public long ToInt64(IFormatProvider provider)
    {
      return value;
    }

    /// <summary>
    /// Returns type code for 32-bit integers.
    /// </summary>
    public System.TypeCode GetTypeCode()
    {
      return TypeCode.Int32;
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent <see cref="T:System.Decimal"></see> number.
    /// </summary>
    public decimal ToDecimal(IFormatProvider provider)
    {
      return value;
    }

    /// <summary>
    /// Returns null.
    /// </summary>
    public object ToType(Type conversionType, IFormatProvider provider)
    {
      // TODO:  Add PdfUInteger.ToType implementation
      return null;
    }

    /// <summary>
    /// Converts the value of this instance to an equivalent 32-bit unsigned integer.
    /// </summary>
    public uint ToUInt32(IFormatProvider provider)
    {
      return Convert.ToUInt32(this.value);
    }

    #endregion
  }
}