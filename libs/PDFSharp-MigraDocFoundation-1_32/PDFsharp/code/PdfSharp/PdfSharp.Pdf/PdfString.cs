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
using System.Text;
using System.IO;
using PdfSharp.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Determines the encoding of a PdfString or PdfStringObject.
  /// </summary>
  [Flags]
  public enum PdfStringEncoding
  {
    /// <summary>
    /// The characters of the string are actually bytes with an unknown or context specific meaning or encoding.
    /// With this encoding the 8 high bits of each character is zero.
    /// </summary>
    RawEncoding = 0x00,

    /// <summary>
    /// Not yet used by PDFsharp.
    /// </summary>
    StandardEncoding = 0x01,

    /// <summary>
    /// The characters of the string are actually bytes with PDF document encoding.
    /// With this encoding the 8 high bits of each character is zero.
    /// </summary>
    PDFDocEncoding = 0x02,

    /// <summary>
    /// The characters of the string are actually bytes with Windows ANSI encoding.
    /// With this encoding the 8 high bits of each character is zero.
    /// </summary>
    WinAnsiEncoding = 0x03,

    /// <summary>
    /// Not yet used by PDFsharp.
    /// </summary>
    MacRomanEncoding = 0x04,  // not used by PDFsharp

    /// <summary>
    /// Not yet used by PDFsharp.
    /// </summary>
    MacExpertEncoding = 0x05,  // not used by PDFsharp

    /// <summary>
    /// The characters of the string are Unicode characters.
    /// </summary>
    Unicode = 0x06,
  }

  /// <summary>
  /// Internal wrapper for PdfStringEncoding.
  /// </summary>
  [Flags]
  enum PdfStringFlags
  {
    RawEncoding = 0x00,
    StandardEncoding = 0x01,  // not used by PDFsharp
    PDFDocEncoding = 0x02,
    WinAnsiEncoding = 0x03,
    MacRomanEncoding = 0x04,  // not used by PDFsharp
    MacExpertEncoding = 0x05,  // not used by PDFsharp
    Unicode = 0x06,
    EncodingMask = 0x0F,

    HexLiteral = 0x80,
  }

  /// <summary>
  /// Represents a direct text string value.
  /// </summary>
  [DebuggerDisplay("({Value})")]
  public sealed class PdfString : PdfItem
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfString"/> class.
    /// </summary>
    public PdfString()
    {
      this.flags = PdfStringFlags.RawEncoding;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfString"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    public PdfString(string value)
    {
      this.value = value;
      this.flags = PdfStringFlags.RawEncoding;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfString"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="encoding">The encoding.</param>
    public PdfString(string value, PdfStringEncoding encoding)
    {
      this.value = value;
      //if ((flags & PdfStringFlags.EncodingMask) == 0)
      //  flags |= PdfStringFlags.PDFDocEncoding;
      this.flags = (PdfStringFlags)encoding;
    }

    internal PdfString(string value, PdfStringFlags flags)
    {
      this.value = value;
      //if ((flags & PdfStringFlags.EncodingMask) == 0)
      //  flags |= PdfStringFlags.PDFDocEncoding;
      this.flags = flags;
    }

    /// <summary>
    /// Gets the number of characters in this string.
    /// </summary>
    public int Length
    {
      get { return this.value == null ? 0 : this.value.Length; }
    }

    /// <summary>
    /// Gets the encoding.
    /// </summary>
    public PdfStringEncoding Encoding
    {
      get { return (PdfStringEncoding)(this.flags & PdfStringFlags.EncodingMask); }
      //set { this.flags = (this.flags & ~PdfStringFlags.EncodingMask) | ((PdfStringFlags)value & PdfStringFlags.EncodingMask);}
    }

    /// <summary>
    /// Gets a value indicating whether the string is a hexadecimal literal.
    /// </summary>
    public bool HexLiteral
    {
      get { return (this.flags & PdfStringFlags.HexLiteral) != 0; }
      //set { this.flags = value ? this.flags | PdfStringFlags.HexLiteral : this.flags & ~PdfStringFlags.HexLiteral;}
    }

    internal PdfStringFlags Flags
    {
      get { return this.flags; }
      //set { this.flags = value; }
    }
    PdfStringFlags flags;

    /// <summary>
    /// Gets the string value.
    /// </summary>
    public string Value
    {
      // This class must behave like a value type. Therefore it cannot be changed (like System.String).
      get { return this.value == null ? "" : this.value; }
    }
    string value;

    /// <summary>
    /// Gets or sets the string value for encryption purposes.
    /// </summary>
    internal byte[] EncryptionValue
    {
      // TODO: Unicode case is not handled!
      get { return this.value == null ? new byte[0] : PdfEncoders.RawEncoding.GetBytes(this.value); }
      // BUG: May lead to trouble with the value semantics of PdfString
      set { this.value = PdfEncoders.RawEncoding.GetString(value, 0, value.Length); }
    }

    /// <summary>
    /// Returns the string.
    /// </summary>
    public override string ToString()
    {
      return this.value;
    }

    /// <summary>
    /// Hack for document encoded bookmarks.
    /// </summary>
    public string ToStringFromPdfDocEncoded()
    {
      int length = this.value.Length;
      char[] bytes = new char[length];
      for (int idx = 0; idx < length; idx++)
      {
        char ch = this.value[idx];
        if (ch <= 255)
        {
          bytes[idx] = Encode[ch];
        }
        else
        {
          Debugger.Break();
        }
      }
      StringBuilder sb = new StringBuilder(length);
      for (int idx = 0; idx < length; idx++)
        sb.Append((char)bytes[idx]);
      return sb.ToString();
    }
    static char[] Encode = new char[]
    {
      '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07', '\x08', '\x09', '\x0A', '\x0B', '\x0C', '\x0D', '\x0E', '\x0F',
      '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17', '\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', '\x1E', '\x1F',
      '\x20', '\x21', '\x22', '\x23', '\x24', '\x25', '\x26', '\x27', '\x28', '\x29', '\x2A', '\x2B', '\x2C', '\x2D', '\x2E', '\x2F',
      '\x30', '\x31', '\x32', '\x33', '\x34', '\x35', '\x36', '\x37', '\x38', '\x39', '\x3A', '\x3B', '\x3C', '\x3D', '\x3E', '\x3F',
      '\x40', '\x41', '\x42', '\x43', '\x44', '\x45', '\x46', '\x47', '\x48', '\x49', '\x4A', '\x4B', '\x4C', '\x4D', '\x4E', '\x4F',
      '\x50', '\x51', '\x52', '\x53', '\x54', '\x55', '\x56', '\x57', '\x58', '\x59', '\x5A', '\x5B', '\x5C', '\x5D', '\x5E', '\x5F',
      '\x60', '\x61', '\x62', '\x63', '\x64', '\x65', '\x66', '\x67', '\x68', '\x69', '\x6A', '\x6B', '\x6C', '\x6D', '\x6E', '\x6F',
      '\x70', '\x71', '\x72', '\x73', '\x74', '\x75', '\x76', '\x77', '\x78', '\x79', '\x7A', '\x7B', '\x7C', '\x7D', '\x7E', '\x7F',
      '\x2022', '\x2020', '\x2021', '\x2026', '\x2014', '\x2013', '\x0192', '\x2044', '\x2039', '\x203A', '\x2212', '\x2030', '\x201E', '\x201C', '\x201D', '\x2018',
      '\x2019', '\x201A', '\x2122', '\xFB01', '\xFB02', '\x0141', '\x0152', '\x0160', '\x0178', '\x017D', '\x0131', '\x0142', '\x0153', '\x0161', '\x017E', '\xFFFD',
      '\x20AC', '\xA1', '\xA2', '\xA3', '\xA4', '\xA5', '\xA6', '\xA7', '\xA8', '\xA9', '\xAA', '\xAB', '\xAC', '\xAD', '\xAE', '\xAF',
      '\xB0', '\xB1', '\xB2', '\xB3', '\xB4', '\xB5', '\xB6', '\xB7', '\xB8', '\xB9', '\xBA', '\xBB', '\xBC', '\xBD', '\xBE', '\xBF',
      '\xC0', '\xC1', '\xC2', '\xC3', '\xC4', '\xC5', '\xC6', '\xC7', '\xC8', '\xC9', '\xCA', '\xCB', '\xCC', '\xCD', '\xCE', '\xCF',
      '\xD0', '\xD1', '\xD2', '\xD3', '\xD4', '\xD5', '\xD6', '\xD7', '\xD8', '\xD9', '\xDA', '\xDB', '\xDC', '\xDD', '\xDE', '\xDF',
      '\xE0', '\xE1', '\xE2', '\xE3', '\xE4', '\xE5', '\xE6', '\xE7', '\xE8', '\xE9', '\xEA', '\xEB', '\xEC', '\xED', '\xEE', '\xEF',
      '\xF0', '\xF1', '\xF2', '\xF3', '\xF4', '\xF5', '\xF6', '\xF7', '\xF8', '\xF9', '\xFA', '\xFB', '\xFC', '\xFD', '\xFE', '\xFF',
    };

    /// <summary>
    /// Writes the string DocEncoded.
    /// </summary>
    internal override void WriteObject(PdfWriter writer)
    {
      writer.Write(this);
    }
  }
}
