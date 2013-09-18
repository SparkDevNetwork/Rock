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
using System.Runtime.InteropServices;
using PdfSharp.Drawing;
using PdfSharp.Internal;

using Fixed = System.Int32;
using FWord = System.Int16;
using UFWord = System.UInt16;

namespace PdfSharp.Fonts.OpenType
{
  /// <summary>
  /// Represents an indirect reference to an existing font table in a font image.
  /// Used to create binary copies of an existing font table that is not modified.
  /// </summary>
  internal class IRefFontTable : OpenTypeFontTable
  {
    public IRefFontTable(FontData fontData, OpenTypeFontTable fontTable)
      : base(null, fontTable.DirectoryEntry.Tag)
    {
      this.fontData = fontData;
      this.irefDirectoryEntry = fontTable.DirectoryEntry;
    }

    TableDirectoryEntry irefDirectoryEntry;

    /// <summary>
    /// Prepares the font table to be compiled into its binary representation.
    /// </summary>
    public override void PrepareForCompilation()
    {
      base.PrepareForCompilation();
      DirectoryEntry.Length = this.irefDirectoryEntry.Length;
      DirectoryEntry.CheckSum = this.irefDirectoryEntry.CheckSum;
#if DEBUG
      // Check the checksum algorithm
      if (DirectoryEntry.Tag != TableTagNames.Head)
      {
        byte[] bytes = new byte[DirectoryEntry.PaddedLength];
        Buffer.BlockCopy(this.irefDirectoryEntry.FontTable.fontData.Data, this.irefDirectoryEntry.Offset, bytes, 0, DirectoryEntry.PaddedLength);
        uint checkSum1 = DirectoryEntry.CheckSum;
        uint checkSum2 = CalcChecksum(bytes);
        // TODO: Sometimes this Assert fails,
        //Debug.Assert(checkSum1 == checkSum2, "Bug in checksum algorithm.");
      }
#endif
    }

    /// <summary>
    /// Converts the font into its binary representation.
    /// </summary>
    public override void Write(OpenTypeFontWriter writer)
    {
      writer.Write(this.irefDirectoryEntry.FontTable.fontData.Data, this.irefDirectoryEntry.Offset, this.irefDirectoryEntry.PaddedLength);
    }
  }
}
