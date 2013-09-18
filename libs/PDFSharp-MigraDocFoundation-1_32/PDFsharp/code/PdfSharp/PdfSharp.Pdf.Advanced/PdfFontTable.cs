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
using System.Globalization;
using System.Text;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Fonts.OpenType;

namespace PdfSharp.Pdf.Advanced
{
  internal enum FontType
  {
    /// <summary>
    /// TrueType with WinAnsi encoding.
    /// </summary>
    TrueType = 1,

    /// <summary>
    /// TrueType with Identity-H or Identity-V encoding (unicode).
    /// </summary>
    Type0 = 2,
  }

  /// <summary>
  /// Contains all used fonts of a document.
  /// </summary>
  internal sealed class PdfFontTable : PdfResourceTable
  {
    /// <summary>
    /// Initializes a new instance of this class, which is a singleton for each document.
    /// </summary>
    public PdfFontTable(PdfDocument document)
      : base(document)
    {
    }

    /// <summary>
    /// Gets a PdfFont from an XFont. If no PdfFont already exists, a new one is created.
    /// </summary>
    public PdfFont GetFont(XFont font)
    {
      string fontName = font.Name;

      PdfFontTable.FontSelector selector = font.selector;
      if (selector == null)
      {
        selector = new FontSelector(font);
        font.selector = selector;
      }
      PdfFont pdfFont;
      if (!this.fonts.TryGetValue(selector, out pdfFont))
      {
        if (font.Unicode)
          pdfFont = new PdfType0Font(this.owner, font, font.IsVertical);
        else
          pdfFont = new PdfTrueTypeFont(this.owner, font);
        //pdfFont.Document = this.document;
        Debug.Assert(pdfFont.Owner == this.owner);
        this.fonts[selector] = pdfFont;
        //if (this.owner.EarlyWrite)
        //{
        //  //pdfFont.Close(); delete 
        //  //pdfFont.AssignObjID(ref this.document.ObjectID); // BUG: just test code!!!!
        //  //pdfFont.WriteObject(null);
        //}
      }
      return pdfFont;

#if false
      goto TrueTypeFont;
      switch (font.Name)
      {
        case "Times":
        case "Times New Roman":
          std = 0;
          break;

        case "Helvetica":
          std = 1;
          break;

        case "Courier":
          std = 2;
          break;

        case "Symbol":
          std = 3;
          break;

        case "ZapfDingbats":
          std = 4;
          break;
      }
      if (std != -1)
      {
        int idx = (int)font.Style & 0x3;
        string name = pdfStandardFonts[std][idx];
        PdfFont pdfFont = GetFont(name);
        if (pdfFont == null)
        {
          pdfFont = new PdfFont();
          pdfFont.SubType = "/Type1";
          pdfFont.BaseFont = name;
          pdfFont.DefaultName = string.Format("F{0}", PdfFontTable.fontNumber++);
        }
        return pdfFont;
      }
      else
      {
      TrueTypeFont:
        // TrueType font
        PdfFont pdfFont = new PdfFont();
        pdfFont.SubType = "/TrueType";
        pdfFont.FirstChar = 0;
        pdfFont.LastChar = 255;
        pdfFont.BaseFont = font.Name;
        pdfFont.DefaultName = string.Format("F{0}", PdfFontTable.fontNumber++);
      } 
#endif
      // TrueType font
      //      PdfFont pdfFont = new PdfFont();
      //      pdfFont.descriptor = new PdfFontDescriptor((TrueTypeDescriptor)FontDescriptorStock.Global.CreateDescriptor(font));
      //      pdfFont.SubType = "/TrueType";
      //      pdfFont.FirstChar = 0;
      //      pdfFont.LastChar = 255;
      //      pdfFont.BaseFont = font.Name;
      //      pdfFont.BaseFont = pdfFont.BaseFont.Replace(" ", "");
      //      switch (font.Style & (XFontStyle.Bold | XFontStyle.Italic))
      //      {
      //        case XFontStyle.Bold:
      //          pdfFont.BaseFont += ",Bold";
      //          break;
      //
      //        case XFontStyle.Italic:
      //          pdfFont.BaseFont += ",Italic";
      //          break;
      //        
      //        case XFontStyle.Bold | XFontStyle.Italic:
      //          pdfFont.BaseFont += ",BoldItalic";
      //          break;
      //      }
      //      pdfFont.descriptor.FontName = pdfFont.BaseFont;
      //      pdfFont.DefaultName = string.Format("F{0}", PdfFontTable.fontNumber++);
    }

    //string[][] pdfStandardFonts =
    //{
    //  new string[]{"Times-Roman", "Times-Bold", "Times-Italic", "Times-BoldItalic"},
    //  new string[]{"Helvetica", "Helvetica-Bold", "Helvetica-Oblique", "Helvetica-BoldOblique"},
    //  new string[]{"Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique"},
    //  new string[]{"Symbol", "Symbol", "Symbol", "Symbol"},
    //  new string[]{"ZapfDingbats", "ZapfDingbats", "ZapfDingbats", "ZapfDingbats"},
    //};

#if true
    /// <summary>
    /// Gets a PdfFont from a font program. If no PdfFont already exists, a new one is created.
    /// </summary>
    public PdfFont GetFont(string idName, byte[] fontData)
    {
      PdfFontTable.FontSelector selector = new FontSelector(idName);
      PdfFont pdfFont;
      if (!this.fonts.TryGetValue(selector, out pdfFont))
      {
        //if (font.Unicode)
        pdfFont = new PdfType0Font(this.owner, idName, fontData, false);
        //else
        //  pdfFont = new PdfTrueTypeFont(this.owner, font);
        //pdfFont.Document = this.document;
        Debug.Assert(pdfFont.Owner == this.owner);
        this.fonts[selector] = pdfFont;
      }
      return pdfFont;
    }
#endif

    /// <summary>
    /// Tries to gets a PdfFont from the font dictionary.
    /// Returns null if no such PdfFont exists.
    /// </summary>
    public PdfFont TryGetFont(string idName)
    {
      FontSelector selector = new FontSelector(idName);
      PdfFont pdfFont;
      this.fonts.TryGetValue(selector, out pdfFont);
      return pdfFont;
    }

    /// <summary>
    /// Map from PdfFontSelector to PdfFont.
    /// </summary>
    readonly Dictionary<FontSelector, PdfFont> fonts = new Dictionary<FontSelector, PdfFont>();

    public void PrepareForSave()
    {
      foreach (PdfFont font in this.fonts.Values)
        font.PrepareForSave();
    }

    /// <summary>
    /// A collection of information that uniquely identifies a particular PDF font.
    /// ... more docu... TODO
    /// Two PDF fonts are equal if and only if their font selector objects are equal.
    /// </summary>
    public class FontSelector
    {
      /// <summary>
      /// Initializes a new instance of PdfFontSelector from an XFont.
      /// </summary>
      public FontSelector(XFont font)
      {
        this.name = font.Name;
        // Ignore Strikeout and Underline
        this.style = font.Style & (XFontStyle.Bold | XFontStyle.Italic);
        // Clear styles that are not available as a separate type face to prevent embedding of identical font files
#if GDI && !WPF
        if ((this.style & XFontStyle.Bold) == XFontStyle.Bold && !font.FontFamily.IsStyleAvailable(XFontStyle.Bold))
          this.style &= ~XFontStyle.Bold;
        if ((this.style & XFontStyle.Italic) == XFontStyle.Italic && !font.FontFamily.IsStyleAvailable(XFontStyle.Italic))
          this.style &= ~XFontStyle.Italic;
#endif
#if WPF && !GDI
#if !SILVERLIGHT
        Debug.Assert(font.typeface != null);
        if ((this.style & XFontStyle.Bold) == XFontStyle.Bold && font.typeface.IsBoldSimulated)
          this.style &= ~XFontStyle.Bold;
        if ((this.style & XFontStyle.Italic) == XFontStyle.Italic && font.typeface.IsObliqueSimulated)
          this.style &= ~XFontStyle.Italic;
#else
        // AGHACK
#endif
#endif
#if WPF && GDI
        Debug.Assert(font.typeface != null);
        if ((this.style & XFontStyle.Bold) == XFontStyle.Bold && font.typeface.IsBoldSimulated)
          this.style &= ~XFontStyle.Bold;
        if ((this.style & XFontStyle.Italic) == XFontStyle.Italic && font.typeface.IsObliqueSimulated)
          this.style &= ~XFontStyle.Italic;
#endif
        this.fontType = font.Unicode ? FontType.Type0 : FontType.TrueType;
      }

      /// <summary>
      /// Initializes a new instance of PdfFontSelector from a unique name.
      /// </summary>
      public FontSelector(string name)
      {
        this.name = name;
        //// Ignore Strikeout and Underline
        //this.style = font.Style & (XFontStyle.Bold | XFontStyle.Italic);
        //// Clear styles that are not available to prevent embedding of identical font files
        //if ((this.style & XFontStyle.Bold) == XFontStyle.Bold && !font.FontFamily.IsStyleAvailable(XFontStyle.Bold))
        //  this.style &= ~XFontStyle.Bold;
        //if ((this.style & XFontStyle.Italic) == XFontStyle.Italic && !font.FontFamily.IsStyleAvailable(XFontStyle.Italic))
        //  this.style &= ~XFontStyle.Italic;
        this.fontType = FontType.Type0;
      }

      public FontSelector(XFontFamily family, XFontStyle style)
      {
        throw new NotImplementedException("PdfFontSelector(XFontFamily family, XFontStyle style)");
      }

      /// <summary>
      /// Gets the (generated) resource name of the font. In our own PDF files equal fonts share the
      /// same resource name in all contents streams.
      /// </summary>
      public string Name
      {
        get { return this.name; }
      }
      string name;

      /// <summary>
      /// Gets the style. Contains only flags that effects the font face and are available for the specified font.
      /// </summary>
      /// <value>The style.</value>
      public XFontStyle Style
      {
        get { return this.style; }
      }
      XFontStyle style;

      /// <summary>
      /// Gets the type of the font (TrueType with Ansi Encoding or CID font).
      /// </summary>
      public FontType FontType
      {
        get { return this.fontType; }
      }
      FontType fontType;

      public static bool operator ==(FontSelector selector1, FontSelector selector2)
      {
        if (!Object.ReferenceEquals(selector1, null))
          selector1.Equals(selector2);
        return Object.ReferenceEquals(selector2, null);
      }

      public static bool operator !=(FontSelector selector1, FontSelector selector2)
      {
        return !(selector1 == selector2);
      }

      public override bool Equals(object obj)
      {
        FontSelector selector = obj as FontSelector;
        if (obj != null && this.name == selector.name && this.style == selector.style)
          return this.fontType == selector.fontType;
        return false;
      }

      public override int GetHashCode()
      {
        return this.name.GetHashCode() ^ this.style.GetHashCode() ^ this.fontType.GetHashCode();
      }

      /// <summary>
      /// Returns a string for diagnostic purposes only.
      /// </summary>
      public override string ToString()
      {
        string variation = "";
        switch (this.style)
        {
          case XFontStyle.Regular:
            variation = "(Regular)";
            break;

          case XFontStyle.Bold:
            variation = "(Bold)";
            break;

          case XFontStyle.Italic:
            variation = "(Italic)";
            break;

          case XFontStyle.Bold | XFontStyle.Italic:
            variation = "(BoldItalic)";
            break;
        }
        return this.name + variation;
      }
    }
  }
}
