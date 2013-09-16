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
using System.Text;
using System.IO;

namespace PdfSharp.Fonts
{
  /// <summary>
  /// Represents a writer for generation of font file streams. 
  /// </summary>
  internal class FontWriter
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="FontWriter"/> class.
    /// </summary>
    public FontWriter(Stream stream)
    {
      this.stream = stream;
    }

    /// <summary>
    /// Closes the writer and, if specified, the underlying stream.
    /// </summary>
    public void Close(bool closeUnderlyingStream)
    {
      if (this.stream != null && closeUnderlyingStream)
        this.stream.Close();
      //this.stream = null;
    }

    /// <summary>
    /// Closes the writer and the underlying stream.
    /// </summary>
    public void Close()
    {
      Close(true);
    }

    /// <summary>
    /// Gets or sets the position within the stream.
    /// </summary>
    public int Position
    {
      get { return (int)this.stream.Position; }
      set { this.stream.Position = value; }
    }

    /// <summary>
    /// Writes the specified value to the font stream.
    /// </summary>
    public void WriteByte(byte value)
    {
      this.stream.WriteByte(value);
    }

    /// <summary>
    /// Writes the specified value to the font stream.
    /// </summary>
    public void WriteByte(int value)
    {
      this.stream.WriteByte((byte)value);
    }

    /// <summary>
    /// Writes the specified value to the font stream using big-endian.
    /// </summary>
    public void WriteShort(short value)
    {
      this.stream.WriteByte((byte)(value >> 8));
      this.stream.WriteByte((byte)value);
    }

    /// <summary>
    /// Writes the specified value to the font stream using big-endian.
    /// </summary>
    public void WriteShort(int value)
    {
      WriteShort((short)value);
    }

    /// <summary>
    /// Writes the specified value to the font stream using big-endian.
    /// </summary>
    public void WriteUShort(ushort value)
    {
      this.stream.WriteByte((byte)(value >> 8));
      this.stream.WriteByte((byte)value);
    }

    /// <summary>
    /// Writes the specified value to the font stream using big-endian.
    /// </summary>
    public void WriteUShort(int value)
    {
      WriteUShort((ushort)value);
    }

    /// <summary>
    /// Writes the specified value to the font stream using big-endian.
    /// </summary>
    public void WriteInt(int value)
    {
      this.stream.WriteByte((byte)(value >> 24));
      this.stream.WriteByte((byte)(value >> 16));
      this.stream.WriteByte((byte)(value >> 8));
      this.stream.WriteByte((byte)value);
    }

    /// <summary>
    /// Writes the specified value to the font stream using big-endian.
    /// </summary>
    public void WriteUInt(uint value)
    {
      this.stream.WriteByte((byte)(value >> 24));
      this.stream.WriteByte((byte)(value >> 16));
      this.stream.WriteByte((byte)(value >> 8));
      this.stream.WriteByte((byte)value);
    }

    //public short ReadFWord()
    //public ushort ReadUFWord()
    //public long ReadLongDate()
    //public string ReadString(int size)

    public void Write(byte[] buffer)
    {
      this.stream.Write(buffer, 0, buffer.Length);
    }

    public void Write(byte[] buffer, int offset, int count)
    {
      this.stream.Write(buffer, offset, count);
    }

    /// <summary>
    /// Gets the underlying stream.
    /// </summary>
    internal Stream Stream
    {
      get { return this.stream; }
    }
    
    readonly Stream stream;
  }
}
