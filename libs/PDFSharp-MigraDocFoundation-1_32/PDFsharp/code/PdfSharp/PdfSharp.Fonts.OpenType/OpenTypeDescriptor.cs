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
using System.Runtime.InteropServices;
using System.Text;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp.Pdf.Internal;
using PdfSharp.Drawing;

namespace PdfSharp.Fonts.OpenType
{
  /// <summary>
  /// The OpenType font descriptor.
  /// </summary>
  internal sealed class OpenTypeDescriptor : FontDescriptor
  {
    public OpenTypeDescriptor(XFont font, XPdfFontOptions options)
    {
      try
      {
        this.fontData = new FontData(font, options);
        this.fontName = font.Name;
        Initialize();
      }
      catch
      {
        throw;
      }
    }

    //#if WPF
    //    public TrueTypeDescriptor(XFont font, XPdfFontOptions options)
    //    {
    //      try
    //      {
    //        this.fontData = new FontData(font, options);
    //        this.fontName = font.Name;
    //        Initialize();
    //      }
    //      catch (Exception ex)
    //      {
    //        throw ex;
    //      }
    //    }
    //#endif

    //internal TrueTypeDescriptor(FontSelector selector)
    //{
    //  throw new NotImplementedException("TrueTypeDescriptor(FontSelector selector)");
    //}

    internal OpenTypeDescriptor(XFont font)
      : this(font, font.PdfOptions)
    { }

    internal OpenTypeDescriptor(string idName, byte[] fontData)
    {
      try
      {
        this.fontData = new FontData(fontData);
        // Try to get real name form name table
        if (idName.Contains("XPS-Font-") && this.fontData.name != null && this.fontData.name.Name.Length != 0)
        {
          string tag = String.Empty;
          if (idName.IndexOf('+') == 6)
            tag = idName.Substring(0, 6);
          idName = tag + "+" + this.fontData.name.Name;
          if (this.fontData.name.Style.Length != 0)
            idName += "," + this.fontData.name.Style;
          idName = idName.Replace(" ", "");
        }
        this.fontName = idName;
        Initialize();
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    internal OpenTypeDescriptor(byte[] fontData)
    {
      try
      {
        this.fontData = new FontData(fontData);
        // Try to get real name form name table
        string name = this.fontData.name.Name;
        if (this.fontData.name.Style.Length != 0)
          name += "," + this.fontData.name.Style;
        name = name.Replace(" ", "");
        this.fontName = name;
        Initialize();
      }
      catch
      {
        throw;
      }
    }

    internal FontData fontData;

    void Initialize()
    {
      //bool embeddingRestricted = this.fontData.os2.fsType == 0x0002;

      //this.fontName = image.n
      this.italicAngle = this.fontData.post.italicAngle;

      this.xMin = this.fontData.head.xMin;
      this.yMin = this.fontData.head.yMin;
      this.xMax = this.fontData.head.xMax;
      this.yMax = this.fontData.head.yMax;

      this.underlinePosition = this.fontData.post.underlinePosition;
      this.underlineThickness = this.fontData.post.underlineThickness;
      this.strikeoutPosition = this.fontData.os2.yStrikeoutPosition;
      this.strikeoutSize = this.fontData.os2.yStrikeoutSize;

      // No documetation found how to get the set vertical stems width from the
      // TrueType tables.
      // The following formula comes from PDFlib Lite source code. Acrobat 5.0 sets
      // /StemV to 0 always. I think the value doesn't matter.
      //float weight = (float)(this.image.os2.usWeightClass / 65.0f);
      //this.stemV = (int)(50 + weight * weight);  // MAGIC
      this.stemV = 0;

      // PDFlib states that some Apple fonts miss the OS/2 table.
      Debug.Assert(fontData.os2 != null, "TrueType font has no OS/2 table.");

      this.unitsPerEm = fontData.head.unitsPerEm;

      // PDFlib takes sTypoAscender and sTypoDescender from OS/2 tabel, but GDI+ uses usWinAscent and usWinDescent
      if (fontData.os2.sTypoAscender != 0)
        this.ascender = fontData.os2.usWinAscent;
      else
        this.ascender = fontData.hhea.ascender;
      Debug.Assert(this.ascender > 0, "PDFsharp internal: Ascender should be greater than 0.");

      if (fontData.os2.sTypoDescender != 0)
      {
        this.descender = fontData.os2.usWinDescent;
        Debug.Assert(this.descender > 0, "PDFsharp internal: Font with non positive ascender value found.");
#if true_
        Debug.WriteLine(String.Format(CultureInfo.InvariantCulture,
          "os2.usWinDescent={0}, hhea.descender={1}, os2.sTypoDescender={2}", fontData.os2.usWinDescent, fontData.hhea.descender, fontData.os2.sTypoDescender));
#endif
        // Force sign from hhea.descender
        // TODO:
        this.descender = Math.Abs(this.descender) * Math.Sign(fontData.hhea.descender);
      }
      else
        this.descender = fontData.hhea.descender;
      Debug.Assert(this.descender < 0, "PDFsharp internal: Ascender should be less than 0.");

      this.leading = fontData.hhea.lineGap;

      // sCapHeight and sxHeight are only valid if version >= 2
      if (fontData.os2.version >= 2 && fontData.os2.sCapHeight != 0)
        this.capHeight = fontData.os2.sCapHeight;
      else
        this.capHeight = fontData.hhea.ascender;

      if (fontData.os2.version >= 2 && fontData.os2.sxHeight != 0)
        this.xHeight = fontData.os2.sxHeight;
      else
        this.xHeight = (int)(0.66f * this.ascender);

      //this.flags = this.image.

      Encoding ansi = PdfEncoders.WinAnsiEncoding; // System.Text.Encoding.Default;
      Encoding unicode = Encoding.Unicode;
      byte[] bytes = new byte[256];

      bool symbol = this.fontData.cmap.symbol;
      this.widths = new int[256];
      for (int idx = 0; idx < 256; idx++)
      {
        bytes[idx] = (byte)idx;
        // PDFlib handles some font flaws here...
        // We wait for bug reports.

        char ch = (char)idx;
        string s = ansi.GetString(bytes, idx, 1);
        if (s.Length != 0)
        {
          if (s[0] != ch)
            ch = s[0];
        }
#if DEBUG
        if (idx == 'S')
          GetType();
#endif
        int glyphIndex;
        if (symbol)
        {
          glyphIndex = idx + (this.fontData.os2.usFirstCharIndex & 0xFF00);
          glyphIndex = CharCodeToGlyphIndex((char)glyphIndex);
        }
        else
        {
          //Debug.Assert(idx + (this.fontData.os2.usFirstCharIndex & 0xFF00) == idx);
          //glyphIndex = CharCodeToGlyphIndex((char)idx);
          glyphIndex = CharCodeToGlyphIndex(ch);
        }
        this.widths[idx] = GlyphIndexToPdfWidth(glyphIndex);
      }
    }
    public int[] widths;

    /// <summary>
    /// Gets a value indicating whether this instance belongs to a bold font.
    /// </summary>
    public override bool IsBoldFace
    {
      get
      {
        // usWeightClass 700 is Bold
        //Debug.Assert((this.fontData.os2.usWeightClass >= 700) == ((this.fontData.os2.fsSelection & (ushort)OS2Table.FontSelectionFlags.Bold) != 0));
        return (this.fontData.os2.fsSelection & (ushort)OS2Table.FontSelectionFlags.Bold) != 0;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance belongs to an italic font.
    /// </summary>
    public override bool IsItalicFace
    {
      get { return (this.fontData.os2.fsSelection & (ushort)OS2Table.FontSelectionFlags.Italic) != 0; }
    }

    internal int DesignUnitsToPdf(double value)
    {
      return (int)Math.Round(value * 1000.0 / this.fontData.head.unitsPerEm);
    }

    /// <summary>
    /// Maps a unicode to the index of the corresponding glyph.
    /// See OpenType spec "cmap - Character To Glyph Index Mapping Table / Format 4: Segment mapping to delta values"
    /// for details about this a little bit strange looking algorithm.
    /// </summary>
    public int CharCodeToGlyphIndex(char value)
    {
      try
      {
        CMap4 cmap = this.fontData.cmap.cmap4;
        int segCount = cmap.segCountX2 / 2;
        int seg;
        for (seg = 0; seg < segCount; seg++)
        {
          if (value <= cmap.endCount[seg])
            break;
        }
        Debug.Assert(seg < segCount);

        if (value < cmap.startCount[seg])
          return 0;

        if (cmap.idRangeOffs[seg] == 0)
          return (value + cmap.idDelta[seg]) & 0xFFFF;

        int idx = cmap.idRangeOffs[seg] / 2 + (value - cmap.startCount[seg]) - (segCount - seg);
        Debug.Assert(idx >= 0 && idx < cmap.glyphCount);

        if (cmap.glyphIdArray[idx] == 0)
          return 0;
        
        return (cmap.glyphIdArray[idx] + cmap.idDelta[seg]) & 0xFFFF;
      }
      catch
      {
        throw;
      }
    }

    /// <summary>
    /// Converts the width of a glyph identified by its index to PDF design units.
    /// </summary>
    public int GlyphIndexToPdfWidth(int glyphIndex)
    {
      try
      {
        int numberOfHMetrics = this.fontData.hhea.numberOfHMetrics;
        int unitsPerEm = this.fontData.head.unitsPerEm;

        // glyphIndex >= numberOfHMetrics means the font is mono-spaced and all glyphs have the same width
        if (glyphIndex >= numberOfHMetrics)
          glyphIndex = numberOfHMetrics - 1;

        int width = this.fontData.hmtx.metrics[glyphIndex].advanceWidth;

        // Sometimes the unitsPerEm is 1000, sometimes a power of 2.
        if (unitsPerEm == 1000)
          return width;
        return width * 1000 / unitsPerEm; // normalize
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    /// <summary>
    ///   //Converts the width of a glyph identified by its index to PDF design units.
    /// </summary>
    public int GlyphIndexToWidth(int glyphIndex)
    {
      try
      {
        int numberOfHMetrics = fontData.hhea.numberOfHMetrics;

        // glyphIndex >= numberOfHMetrics means the font is mono-spaced and all glyphs have the same width
        if (glyphIndex >= numberOfHMetrics)
          glyphIndex = numberOfHMetrics - 1;

        int width = fontData.hmtx.metrics[glyphIndex].advanceWidth;

        return width;
      }
      catch (Exception)
      {
        GetType();
        throw;
      }
    }


    public int PdfWidthFromCharCode(char ch)
    {
      int idx = CharCodeToGlyphIndex(ch);
      int width = GlyphIndexToPdfWidth(idx);
      return width;
    }

#if DEBUG_
    public static void Test()
    {
      Font font = new Font("Times New Roman", 10);
      FontData image = new FontData(font);

//      Font font = new Font("Isabelle", 12);
//      LOGFONT logFont = new LOGFONT();
//      font.ToLogFont(logFont);
//
//      IntPtr hfont = CreateFontIndirect(logFont);
////      IntPtr hfont2 = font.ToHfont();
////      System.Windows.Forms.MessageBox.Show(hfont2.ToString());
//
//      Graphics gfx = Graphics.FromHwnd(IntPtr.Zero);
//      IntPtr hdc = gfx.GetHdc();
//      IntPtr oldFont =  SelectObject(hdc, hfont);
//      int size = GetFontData(hdc, 0, 0, null, 0);
//
//      byte[] fontbits = new byte[size];
//      int xx = GetFontData(hdc, 0, 0, fontbits, size);
//      SelectObject(hdc, oldFont);
//      DeleteObject(hfont);
//      gfx.ReleaseHdc(hdc);
//
//      FontData image = new FontData(fontbits);
//      //image.Read();
//
//
//      //HandleRef
//
//      font.GetType();
    }
#endif
  }
}