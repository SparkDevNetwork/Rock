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
using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Fonts;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Filters;

namespace PdfSharp.Pdf.Advanced
{
  /// <summary>
  /// Represents a composite font. Used for Unicode encoding.
  /// </summary>
  internal sealed class PdfType0Font : PdfFont
  {
    public PdfType0Font(PdfDocument document)
      : base(document)
    {
    }

    public PdfType0Font(PdfDocument document, XFont font, bool vertical)
      : base(document)
    {
      Elements.SetName(Keys.Type, "/Font");
      Elements.SetName(Keys.Subtype, "/Type0");
      Elements.SetName(Keys.Encoding, vertical ? "/Identity-V" : "/Identity-H");

      OpenTypeDescriptor ttDescriptor = (OpenTypeDescriptor)FontDescriptorStock.Global.CreateDescriptor(font);
      this.fontDescriptor = new PdfFontDescriptor(document, ttDescriptor);
      this.fontOptions = font.PdfOptions;
      Debug.Assert(this.fontOptions != null);

      this.cmapInfo = new CMapInfo(ttDescriptor);
      this.descendantFont = new PdfCIDFont(document, this.fontDescriptor, font);
      this.descendantFont.CMapInfo = this.cmapInfo;

      // Create ToUnicode map
      this.toUnicode = new PdfToUnicodeMap(document, this.cmapInfo);
      document.Internals.AddObject(toUnicode);
      Elements.Add(Keys.ToUnicode, toUnicode);

      //if (this.fontOptions.BaseFont != "")
      //{
      //  BaseFont = this.fontOptions.BaseFont;
      //}
      //else
      {
        BaseFont = font.Name.Replace(" ", "");
        switch (font.Style & (XFontStyle.Bold | XFontStyle.Italic))
        {
          case XFontStyle.Bold:
            this.BaseFont += ",Bold";
            break;

          case XFontStyle.Italic:
            this.BaseFont += ",Italic";
            break;

          case XFontStyle.Bold | XFontStyle.Italic:
            this.BaseFont += ",BoldItalic";
            break;
        }
      }
      // CID fonts are always embedded
      BaseFont = PdfFont.CreateEmbeddedFontSubsetName(BaseFont);

      this.fontDescriptor.FontName = BaseFont;
      this.descendantFont.BaseFont = BaseFont;

      PdfArray descendantFonts = new PdfArray(document);
      Owner.irefTable.Add(descendantFont);
      descendantFonts.Elements.Add(descendantFont.Reference);
      Elements[Keys.DescendantFonts] = descendantFonts;
    }

    public PdfType0Font(PdfDocument document, string idName, byte[] fontData, bool vertical)
      : base(document)
    {
      Elements.SetName(Keys.Type, "/Font");
      Elements.SetName(Keys.Subtype, "/Type0");
      Elements.SetName(Keys.Encoding, vertical ? "/Identity-V" : "/Identity-H");

      OpenTypeDescriptor ttDescriptor = (OpenTypeDescriptor)FontDescriptorStock.Global.CreateDescriptor(idName, fontData);
      this.fontDescriptor = new PdfFontDescriptor(document, ttDescriptor);
      this.fontOptions = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
      Debug.Assert(this.fontOptions != null);

      this.cmapInfo = new CMapInfo(ttDescriptor);
      this.descendantFont = new PdfCIDFont(document, this.fontDescriptor, fontData);
      this.descendantFont.CMapInfo = this.cmapInfo;

      // Create ToUnicode map
      this.toUnicode = new PdfToUnicodeMap(document, this.cmapInfo);
      document.Internals.AddObject(toUnicode);
      Elements.Add(Keys.ToUnicode, toUnicode);

      BaseFont = ttDescriptor.FontName.Replace(" ", "");
        //switch (font.Style & (XFontStyle.Bold | XFontStyle.Italic))
        //{
        //  case XFontStyle.Bold:
        //    this.BaseFont += ",Bold";
        //    break;

        //  case XFontStyle.Italic:
        //    this.BaseFont += ",Italic";
        //    break;

        //  case XFontStyle.Bold | XFontStyle.Italic:
        //    this.BaseFont += ",BoldItalic";
        //    break;
        //}

      // CID fonts are always embedded
      if (!BaseFont.Contains("+"))  // HACK in PdfType0Font
        BaseFont = PdfFont.CreateEmbeddedFontSubsetName(BaseFont);

      this.fontDescriptor.FontName = BaseFont;
      this.descendantFont.BaseFont = BaseFont;

      PdfArray descendantFonts = new PdfArray(document);
      Owner.irefTable.Add(descendantFont);
      descendantFonts.Elements.Add(descendantFont.Reference);
      Elements[Keys.DescendantFonts] = descendantFonts;
    }

    XPdfFontOptions FontOptions
    {
      get { return this.fontOptions; }
    }
    XPdfFontOptions fontOptions;

    public string BaseFont
    {
      get { return Elements.GetName(Keys.BaseFont); }
      set { Elements.SetName(Keys.BaseFont, value); }
    }

    internal PdfCIDFont DescendantFont
    {
      get { return this.descendantFont; }
    }
    PdfCIDFont descendantFont;

    internal override void PrepareForSave()
    {
      base.PrepareForSave();

#if true
      // use GetGlyphIndices to create the widths array
      OpenTypeDescriptor descriptor = (OpenTypeDescriptor)this.fontDescriptor.descriptor;
      StringBuilder w = new StringBuilder("[");
      if (this.cmapInfo != null)
      {
        int[] glyphIndices = this.cmapInfo.GetGlyphIndices();
        int count = glyphIndices.Length;
        int[] glyphWidths = new int[count];

        for (int idx = 0; idx < count; idx++)
          glyphWidths[idx] = descriptor.GlyphIndexToPdfWidth(glyphIndices[idx]);

        //TODO: optimize order of indices

        for (int idx = 0; idx < count; idx++)
          w.AppendFormat("{0}[{1}]", glyphIndices[idx], glyphWidths[idx]);
        w.Append("]");
        this.descendantFont.Elements.SetValue(PdfCIDFont.Keys.W, new PdfLiteral(w.ToString()));
#else
      TrueTypeDescriptor descriptor = (TrueTypeDescriptor)this.fontDescriptor.descriptor;
      bool symbol = descriptor.fontData.cmap.symbol;
      StringBuilder w = new StringBuilder("[");
      if (this.cmapInfo != null)
      {
        char[] chars = this.cmapInfo.Chars;
        int count = chars.Length;
        // We don't care about char that share the same glyph
        int[] glyphIndices = new int[count];
        int[] glyphWidths = new int[count];

        for (int idx = 0; idx < count; idx++)
        {
          char ch = chars[idx];
          int glyphIndex;
          if (symbol)
          {
            glyphIndex = (int)ch + (descriptor.fontData.os2.usFirstCharIndex & 0xFF00);
            glyphIndex = descriptor.CharCodeToGlyphIndex((char)glyphIndex);
          }
          else
            glyphIndex = descriptor.CharCodeToGlyphIndex(ch);

          descriptor.CharCodeToGlyphIndex(chars[idx]);
          glyphIndices[idx] = glyphIndex;
          glyphWidths[idx] = descriptor.GlyphIndexToPdfWidth(glyphIndex);
        }

        //TODO: optimize order of indices

        for (int idx = 0; idx < count; idx++)
          w.AppendFormat("{0}[{1}]", glyphIndices[idx], glyphWidths[idx]);
        w.Append("]");
        this.descendantFont.Elements.SetValue(PdfCIDFont.Keys.W, new PdfLiteral(w.ToString()));
#endif
      }

      this.descendantFont.PrepareForSave();
      this.toUnicode.PrepareForSave();
    }

    /// <summary>
    /// Predefined keys of this dictionary.
    /// </summary>
    public new sealed class Keys : PdfFont.Keys
    {
      /// <summary>
      /// (Required) The type of PDF object that this dictionary describes;
      /// must be Font for a font dictionary.
      /// </summary>
      [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "Font")]
      public new const string Type = "/Type";

      /// <summary>
      /// (Required) The type of font; must be Type0 for a Type 0 font.
      /// </summary>
      [KeyInfo(KeyType.Name | KeyType.Required)]
      public new const string Subtype = "/Subtype";

      /// <summary>
      /// (Required) The PostScript name of the font. In principle, this is an arbitrary
      /// name, since there is no font program associated directly with a Type 0 font
      /// dictionary. The conventions described here ensure maximum compatibility
      /// with existing Acrobat products.
      /// If the descendant is a Type 0 CIDFont, this name should be the concatenation
      /// of the CIDFont’s BaseFont name, a hyphen, and the CMap name given in the
      /// Encoding entry (or the CMapName entry in the CMap). If the descendant is a
      /// Type 2 CIDFont, this name should be the same as the CIDFont’s BaseFont name.
      /// </summary>
      [KeyInfo(KeyType.Name | KeyType.Required)]
      public new const string BaseFont = "/BaseFont";

      /// <summary>
      /// (Required) The name of a predefined CMap, or a stream containing a CMap
      /// that maps character codes to font numbers and CIDs. If the descendant is a
      /// Type 2 CIDFont whose associated TrueType font program is not embedded
      /// in the PDF file, the Encoding entry must be a predefined CMap name.
      /// </summary>
      [KeyInfo(KeyType.StreamOrName | KeyType.Required)]
      public const string Encoding = "/Encoding";

      /// <summary>
      /// (Required) A one-element array specifying the CIDFont dictionary that is the
      /// descendant of this Type 0 font.
      /// </summary>
      [KeyInfo(KeyType.Array | KeyType.Required)]
      public const string DescendantFonts = "/DescendantFonts";

      /// <summary>
      /// ((Optional) A stream containing a CMap file that maps character codes to
      /// Unicode values.
      /// </summary>
      [KeyInfo(KeyType.Stream | KeyType.Optional)]
      public const string ToUnicode = "/ToUnicode";

      /// <summary>
      /// Gets the KeysMeta for these keys.
      /// </summary>
      internal static DictionaryMeta Meta
      {
        get
        {
          if (Keys.meta == null)
            Keys.meta = CreateMeta(typeof(Keys));
          return Keys.meta;
        }
      }
      static DictionaryMeta meta;
    }

    /// <summary>
    /// Gets the KeysMeta of this dictionary type.
    /// </summary>
    internal override DictionaryMeta Meta
    {
      get { return Keys.Meta; }
    }
  }
}
