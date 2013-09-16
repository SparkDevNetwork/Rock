#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   David Stephensen (mailto:David.Stephensen@pdfsharp.com)
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
using System.IO;

namespace PdfSharp.Pdf.Filters
{
  /// <summary>
  /// Implements the LzwDecode filter.
  /// </summary>
  public class LzwDecode : Filter
  {
    /// <summary>
    /// Throws a NotImplementedException because the obsolete LZW encoding is not supported by PDFsharp.
    /// </summary>
    public override byte[] Encode(byte[] data)
    {
      throw new NotImplementedException("PDFsharp does not support LZW encoding.");
    }

    /// <summary>
    /// Decodes the specified data.
    /// </summary>
    public override byte[] Decode(byte[] data, FilterParms parms)
    {
      if (data[0] == 0x00 && data[1] == 0x01)
        throw new Exception("LZW flavour not supported.");

      MemoryStream outputStream = new MemoryStream();

      InitializeDictionary();

      this.data = data;
      bytePointer = 0;
      nextData = 0;
      nextBits = 0;
      int code, oldCode = 0;
      byte[] str;

      while ((code = NextCode) != 257)
      {
        if (code == 256)
        {
          InitializeDictionary();
          code = NextCode;
          if (code == 257)
          {
            break;
          }
          outputStream.Write(stringTable[code], 0, stringTable[code].Length);
          oldCode = code;

        }
        else
        {
          if (code < tableIndex)
          {
            str = stringTable[code];
            outputStream.Write(str, 0, str.Length);
            AddEntry(stringTable[oldCode], str[0]);
            oldCode = code;
          }
          else
          {
            str = stringTable[oldCode];
            outputStream.Write(str, 0, str.Length);
            AddEntry(str, str[0]);
            oldCode = code;
          }
        }
      }

      if (outputStream.Length >= 0)
      {
        outputStream.Capacity = (int)outputStream.Length;
        return outputStream.GetBuffer();
      }
      return null;
    }

    /// <summary>
    /// Initialize the dictionary.
    /// </summary>
    void InitializeDictionary()
    {
      stringTable = new byte[8192][];

      for (int i = 0; i < 256; i++)
      {
        stringTable[i] = new byte[1];
        stringTable[i][0] = (byte)i;
      }

      tableIndex = 258;
      bitsToGet = 9;
    }

    /// <summary>
    /// Add a new entry to the Dictionary.
    /// </summary>
    void AddEntry(byte[] oldstring, byte newstring)
    {
      int length = oldstring.Length;
      byte[] str = new byte[length + 1];
      Array.Copy(oldstring, 0, str, 0, length);
      str[length] = newstring;

      stringTable[tableIndex++] = str;

      if (tableIndex == 511)
        bitsToGet = 10;
      else if (tableIndex == 1023)
        bitsToGet = 11;
      else if (tableIndex == 2047)
        bitsToGet = 12;
    }

    /// <summary>
    /// Returns the next set of bits.
    /// </summary>
    int NextCode
    {
      get
      {
        try
        {
          nextData = (nextData << 8) | (data[bytePointer++] & 0xff);
          nextBits += 8;

          if (nextBits < bitsToGet)
          {
            nextData = (nextData << 8) | (data[bytePointer++] & 0xff);
            nextBits += 8;
          }

          int code = (nextData >> (nextBits - bitsToGet)) & andTable[bitsToGet - 9];
          nextBits -= bitsToGet;

          return code;
        }
        catch
        {
          return 257;
        }
      }
    }

    readonly int[] andTable = { 511, 1023, 2047, 4095 };
    byte[][] stringTable;
    byte[] data;
    int tableIndex, bitsToGet = 9;
    int bytePointer;
    int nextData = 0;
    int nextBits = 0;
  }
}
