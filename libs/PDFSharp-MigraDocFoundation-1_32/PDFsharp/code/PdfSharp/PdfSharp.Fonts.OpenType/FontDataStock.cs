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
using System.Collections.Generic;
using PdfSharp.Drawing;
using PdfSharp.Fonts.OpenType;

namespace PdfSharp.Fonts.OpenType
{
  /// <summary>
  /// Global table of TrueType font faces.
  /// </summary>
  class FontDataStock  // TODO: rename
  {
    FontDataStock()
    {
      this.fontDataTable = new Dictionary<string, FontData>();
    }

    public FontData RegisterFontData(byte[] data)
    {
      uint checksum = CalcChecksum(data);
      string key = String.Format("??{0:X}", checksum);

      FontData fontData;
      if (!this.fontDataTable.TryGetValue(key, out fontData))
      {
        lock (typeof(FontDataStock))
        {
          // may be created by other thread meanwhile
          if (!this.fontDataTable.TryGetValue(key, out fontData))
          {
            fontData = new FontData(data);
            this.fontDataTable.Add(key, fontData);
            this.lastEntry = fontData;
          }
        }
      }
      return fontData;
    }
    private FontData lastEntry;

    public bool UnregisterFontData(FontData fontData)
    {
      Debug.Assert(false);
      return false;
    }

    internal FontData[] GetFontDataList()
    {
      int count = fontDataTable.Values.Count;
      FontData[] fontDataArray = new FontData[count];
      fontDataTable.Values.CopyTo(fontDataArray, 0);
      return fontDataArray;
    }

    //internal FontData FindFont(XTypefaceHack typeface)
    //{
    //  // HACK: 
    //  if (this.fontDataTable.Count > 1)
    //    return this.lastEntry;
    //  return null;
    //}

    /// <summary>
    /// Calculates an Adler32 checksum.
    /// </summary>
    static uint CalcChecksum(byte[] buffer)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");

      const uint BASE = 65521; // largest prime smaller than 65536
      uint s1 = 0;
      uint s2 = 0;
      int length = buffer.Length;
      int offset = 0;
      while (length > 0)
      {
        int n = 3800;
        if (n > length)
          n = length;
        length -= n;
        while (--n >= 0)
        {
          s1 = s1 + (uint)(buffer[offset++] & 0xFF);
          s2 = s2 + s1;
        }
        s1 %= BASE;
        s2 %= BASE;
      }
      return (s2 << 16) | s1;
    }

    /// <summary>
    /// Gets the singleton.
    /// </summary>
    public static FontDataStock Global
    {
      get
      {
        if (global == null)
        {
          lock (typeof(FontDataStock))
          {
            if (global == null)
              global = new FontDataStock();
          }
        }
        return global;
      }
    }
    static FontDataStock global;

    Dictionary<string, FontData> fontDataTable;
  }
}
