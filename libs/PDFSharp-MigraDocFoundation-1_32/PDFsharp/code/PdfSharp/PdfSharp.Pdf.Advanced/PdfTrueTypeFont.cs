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
  /// Represents a TrueType font.
  /// </summary>
  internal class PdfTrueTypeFont : PdfFont
  {
    public PdfTrueTypeFont(PdfDocument document)
      : base(document)
    {
    }

    /// <summary>
    /// Initializes a new instance of PdfTrueTypeFont from an XFont.
    /// </summary>
    public PdfTrueTypeFont(PdfDocument document, XFont font)
      : base(document)
    {
      Elements.SetName(Keys.Type, "/Font");
      Elements.SetName(Keys.Subtype, "/TrueType");

      // TrueType with WinAnsiEncoding only
      OpenTypeDescriptor ttDescriptor = (OpenTypeDescriptor)FontDescriptorStock.Global.CreateDescriptor(font);
      this.fontDescriptor = new PdfFontDescriptor(document, ttDescriptor);
      this.fontOptions = font.PdfOptions;
      Debug.Assert(this.fontOptions != null);

      //this.cmapInfo = new CMapInfo(null/*ttDescriptor*/);
      this.cmapInfo = new CMapInfo(ttDescriptor);

      BaseFont = font.Name.Replace(" ", "");
      switch (font.Style & (XFontStyle.Bold | XFontStyle.Italic))
      {
        case XFontStyle.Bold:
          BaseFont += ",Bold";
          break;

        case XFontStyle.Italic:
          BaseFont += ",Italic";
          break;

        case XFontStyle.Bold | XFontStyle.Italic:
          BaseFont += ",BoldItalic";
          break;
      }
      if (this.fontOptions.FontEmbedding == PdfFontEmbedding.Always)
        BaseFont = PdfFont.CreateEmbeddedFontSubsetName(BaseFont);
      this.fontDescriptor.FontName = BaseFont;

      Debug.Assert(this.fontOptions.FontEncoding == PdfFontEncoding.WinAnsi);
      if (!IsSymbolFont)
        Encoding = "/WinAnsiEncoding";

      //        {
      //#if true
      //          throw new NotImplementedException("Specifying a font file is not yet supported.");
      //#else
      //          // Testcode
      //          FileStream stream = new FileStream("WAL____I.AFM", FileAccess.Read);
      //          int length = stream.Length;
      //          byte[] fontProgram = new byte[length];
      //          PdfDictionary fontStream = new PdfDictionary(this.Document);
      //          this.Document.xrefTable.Add(fontStream);
      //          this.fontDescriptor.Elements[PdfFontDescriptor.Keys.FontFile] = fontStream.XRef;

      //          fontStream.Elements["/Length1"] = new PdfInteger(fontProgram.Length);
      //          if (!this.Document.Options.NoCompression)
      //          {
      //            fontProgram = Filtering.FlateDecode.Encode(fontProgram);
      //            fontStream.Elements["/Filter"] = new PdfName("/FlateDecode");
      //          }
      //          fontStream.Elements["/Length"] = new PdfInteger(fontProgram.Length);
      //          fontStream.CreateStream(fontProgram);
      //#endif
      //        }

      Owner.irefTable.Add(this.fontDescriptor);
      Elements[Keys.FontDescriptor] = this.fontDescriptor.Reference;

      FontEncoding = font.PdfOptions.FontEncoding;
      FontEmbedding = font.PdfOptions.FontEmbedding;
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

    public int FirstChar
    {
      get { return Elements.GetInteger(Keys.FirstChar); }
      set { Elements.SetInteger(Keys.FirstChar, value); }
    }

    public int LastChar
    {
      get { return Elements.GetInteger(Keys.LastChar); }
      set { Elements.SetInteger(Keys.LastChar, value); }
    }

    public PdfArray Widths
    {
      get { return (PdfArray)Elements.GetValue(Keys.Widths, VCF.Create); }
    }

    public string Encoding
    {
      get { return Elements.GetName(Keys.Encoding); }
      set { Elements.SetName(Keys.Encoding, value); }
    }

    /// <summary>
    /// Prepares the object to get saved.
    /// </summary>
    internal override void PrepareForSave()
    {
      base.PrepareForSave();

      if (FontEmbedding == PdfFontEmbedding.Always || FontEmbedding == PdfFontEmbedding.Automatic)
      {
        FontData subSet = this.fontDescriptor.descriptor.fontData.CreateFontSubSet(this.cmapInfo.GlyphIndices, false);
        byte[] fontData = subSet.Data;

#if DEBUG_
        TrueTypeFontSubSet fss = new TrueTypeFontSubSet("", this.cmapInfo.descriptor.fontData, this.cmapInfo.GlyphIndices, 0, true, false);
        byte[] fontSubSet = fss.Process();
        fss.CompareBytes(fontSubSet, fontProgram);
#endif
        PdfDictionary fontStream = new PdfDictionary(this.Owner);
        this.Owner.Internals.AddObject(fontStream);
        this.fontDescriptor.Elements[PdfFontDescriptor.Keys.FontFile2] = fontStream.Reference;

        fontStream.Elements["/Length1"] = new PdfInteger(fontData.Length);
        if (!this.Owner.Options.NoCompression)
        {
          fontData = Filtering.FlateDecode.Encode(fontData);
          fontStream.Elements["/Filter"] = new PdfName("/FlateDecode");
        }
        fontStream.Elements["/Length"] = new PdfInteger(fontData.Length);
        fontStream.CreateStream(fontData);
      }

      //if (this.cmapInfo == null)
      //{
        FirstChar = 0;
        LastChar = 255;
        PdfArray width = Widths;
        //width.Elements.Clear();
        for (int idx = 0; idx < 256; idx++)
          width.Elements.Add(new PdfInteger(this.fontDescriptor.descriptor.widths[idx]));
      //}
      //else
      //{
      //  FirstChar = (char)Math.Min(this.cmapInfo.MinChar, 255u);
      //  LastChar = (char)Math.Min(this.cmapInfo.MaxChar, 255u);

      //  PdfArray width = Widths;
      //  Debug.Assert(width.Elements.Count == 0);
      //  //width.Elements.Clear();
      //  for (int idx = FirstChar; idx <= LastChar; idx++)
      //  {
      //    int charWidth = 0;
      //    if (this.cmapInfo.Contains((char)idx))
      //      charWidth = this.fontDescriptor.descriptor.widths[idx];
      //    width.Elements.Add(new PdfInteger(charWidth));
      //  }
      //}
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
      /// (Required) The type of font; must be TrueType for a TrueType font.
      /// </summary>
      [KeyInfo(KeyType.Name | KeyType.Required)]
      public new const string Subtype = "/Subtype";

      /// <summary>
      /// (Required in PDF 1.0; optional otherwise) The name by which this font is 
      /// referenced in the Font subdictionary of the current resource dictionary.
      /// </summary>
      [KeyInfo(KeyType.Name | KeyType.Optional)]
      public const string Name = "/Name";

      /// <summary>
      /// (Required) The PostScript name of the font. For Type 1 fonts, this is usually
      /// the value of the FontName entry in the font program; for more information.
      /// The Post-Script name of the font can be used to find the font’s definition in 
      /// the consumer application or its environment. It is also the name that is used when
      /// printing to a PostScript output device.
      /// </summary>
      [KeyInfo(KeyType.Name | KeyType.Required)]
      public new const string BaseFont = "/BaseFont";

      /// <summary>
      /// (Required except for the standard 14 fonts) The first character code defined 
      /// in the font’s Widths array.
      /// </summary>
      [KeyInfo(KeyType.Integer)]
      public const string FirstChar = "/FirstChar";

      /// <summary>
      /// (Required except for the standard 14 fonts) The last character code defined
      /// in the font’s Widths array.
      /// </summary>
      [KeyInfo(KeyType.Integer)]
      public const string LastChar = "/LastChar";

      /// <summary>
      /// (Required except for the standard 14 fonts; indirect reference preferred)
      /// An array of (LastChar - FirstChar + 1) widths, each element being the glyph width
      /// for the character code that equals FirstChar plus the array index. For character
      /// codes outside the range FirstChar to LastChar, the value of MissingWidth from the 
      /// FontDescriptor entry for this font is used. The glyph widths are measured in units 
      /// in which 1000 units corresponds to 1 unit in text space. These widths must be 
      /// consistent with the actual widths given in the font program. 
      /// </summary>
      [KeyInfo(KeyType.Array, typeof(PdfArray))]
      public const string Widths = "/Widths";

      /// <summary>
      /// (Required except for the standard 14 fonts; must be an indirect reference)
      /// A font descriptor describing the font’s metrics other than its glyph widths.
      /// Note: For the standard 14 fonts, the entries FirstChar, LastChar, Widths, and 
      /// FontDescriptor must either all be present or all be absent. Ordinarily, they are
      /// absent; specifying them enables a standard font to be overridden.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.MustBeIndirect, typeof(PdfFontDescriptor))]
      public new const string FontDescriptor = "/FontDescriptor";

      /// <summary>
      /// (Optional) A specification of the font’s character encoding if different from its
      /// built-in encoding. The value of Encoding is either the name of a predefined
      /// encoding (MacRomanEncoding, MacExpertEncoding, or WinAnsiEncoding, as described in 
      /// Appendix D) or an encoding dictionary that specifies differences from the font’s
      /// built-in encoding or from a specified predefined encoding.
      /// </summary>
      [KeyInfo(KeyType.Name | KeyType.Dictionary)]
      public const string Encoding = "/Encoding";

      /// <summary>
      /// (Optional; PDF 1.2) A stream containing a CMap file that maps character
      /// codes to Unicode values.
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
