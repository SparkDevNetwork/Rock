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
using System.Text;

namespace PdfSharp.Pdf.Internal
{
  /// <summary>
  /// An encoder for raw strings. The raw encoding is simply the identity relation between
  /// charachters and bytes. PDFsharp internally works with raw encoded strings instead of
  /// byte arrays because strings are much more handy than byte arrays.
  /// </summary>
  internal sealed class RawEncoding : Encoding
  {
    public RawEncoding()
    {
    }

    public override int GetByteCount(char[] chars, int index, int count)
    {
      return count;
    }

    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
      for (int count = charCount; count > 0; charIndex++, byteIndex++, count--)
      {
        Debug.Assert((uint)chars[charIndex] < 256, "Raw string contains invalid character with a value > 255.");
        bytes[byteIndex] = (byte)chars[charIndex];
      }
      return charCount;
    }

    public override int GetCharCount(byte[] bytes, int index, int count)
    {
      return count;
    }

    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
      for (int count = byteCount; count > 0; byteIndex++, charIndex++, count--)
        chars[charIndex] = (char)bytes[byteIndex];
      return byteCount;
    }

    public override int GetMaxByteCount(int charCount)
    {
      return charCount;
    }

    public override int GetMaxCharCount(int byteCount)
    {
      return byteCount;
    }
  }
}
