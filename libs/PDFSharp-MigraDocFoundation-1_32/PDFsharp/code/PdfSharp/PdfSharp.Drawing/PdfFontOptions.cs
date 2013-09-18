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
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows.Media;
#endif
using PdfSharp.Internal;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Specifies details about how the font is used in PDF creation.
  /// </summary>
  public class XPdfFontOptions
  {
    internal XPdfFontOptions() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="XPdfFontOptions"/> class.
    /// </summary>
    public XPdfFontOptions(PdfFontEncoding encoding,  PdfFontEmbedding embedding)
    {
      this.fontEncoding = encoding;
      this.fontEmbedding = embedding;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XPdfFontOptions"/> class.
    /// </summary>
    public XPdfFontOptions(PdfFontEncoding encoding)
    {
      this.fontEncoding = encoding;
      this.fontEmbedding = PdfFontEmbedding.None;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XPdfFontOptions"/> class.
    /// </summary>
    public XPdfFontOptions(PdfFontEmbedding embedding)
    {
      this.fontEncoding = PdfFontEncoding.WinAnsi;
      this.fontEmbedding = embedding;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XPdfFontOptions"/> class.
    /// </summary>
    /// <param name="embed">If set to <c>true</c> the font is embedded.</param>
    /// <param name="unicode">If set to <c>true</c> Unicode encoding is used.</param>
    /// <param name="baseFont">Not yet implemented. Should be "".</param>
    /// <param name="fontFile">Not yet implemented. Should be "".</param>
    [Obsolete("Use other constructor")]
    XPdfFontOptions(bool embed, bool unicode, string baseFont, string fontFile)
    {
      this.fontEmbedding = embed ? PdfFontEmbedding.Always : PdfFontEmbedding.None;
      this.fontEncoding = unicode ? PdfFontEncoding.Unicode : PdfFontEncoding.WinAnsi;
      //this.baseFont = baseFont == null ? "" : baseFont;
      //this.fontFile = fontFile == null ? "" : fontFile;

      this.fontEmbedding = PdfFontEmbedding.Default;
      this.fontEncoding = PdfFontEncoding.WinAnsi;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XPdfFontOptions"/> class.
    /// </summary>
    /// <param name="unicode">If set to <c>true</c> Unicode encoding is used.</param>
    /// <param name="fontData">User supplied font data.</param>
    [Obsolete("Use other constructor")]
    public XPdfFontOptions(bool unicode, byte[] fontData)
    {
      this.fontEmbedding = PdfFontEmbedding.None;
      this.fontEncoding = unicode ? PdfFontEncoding.Unicode : PdfFontEncoding.WinAnsi;
      //this.baseFont = "";
      //this.fontFile = "";
      //this.fontData = fontData;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XPdfFontOptions"/> class.
    /// </summary>
    /// <param name="embed">If set to <c>true</c> the font is embedded.</param>
    /// <param name="baseFont">Not yet implemented. Should be "".</param>
    [Obsolete("Use other constructor")]
    public XPdfFontOptions(bool embed, string baseFont)
      : this(embed, false, baseFont, "")
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="XPdfFontOptions"/> class.
    /// </summary>
    /// <param name="embed">If set to <c>true</c> the font is embedded.</param>
    /// <param name="unicode">If set to <c>true</c> Unicode encoding is used.</param>
    [Obsolete("Use other constructor")]
    public XPdfFontOptions(bool embed, bool unicode)
      : this(embed, unicode, "", "")
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="XPdfFontOptions"/> class.
    /// </summary>
    /// <param name="embed">If set to <c>true</c> the font is embedded.</param>
    [Obsolete("Use other constructor")]
    public XPdfFontOptions(bool embed)
      : this(embed, false, "", "")
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="XPdfFontOptions"/> class.
    /// </summary>
    /// <param name="fontEmbedding">Indicates how font is embedded.</param>
    /// <param name="unicode">If set to <c>true</c> Unicode encoding is used.</param>
    [Obsolete("Use other constructor")]
    public XPdfFontOptions(PdfFontEmbedding fontEmbedding, bool unicode)
      : this(fontEmbedding == PdfFontEmbedding.Always || fontEmbedding == PdfFontEmbedding.Automatic,
      unicode, "", "")
    { }

    /// <summary>
    /// Gets a value indicating whether the gets embedded in the PDF file.
    /// </summary>
    [Obsolete("Use FontEmbedding")]
    public bool Embed
    {
      get { return this.fontEmbedding != PdfFontEmbedding.None; }
    }
    //bool embed;

    /// <summary>
    /// Gets a value indicating the font embedding.
    /// </summary>
    public PdfFontEmbedding FontEmbedding
    {
      get { return this.fontEmbedding; }
    }
    PdfFontEmbedding fontEmbedding;

    //public bool Subset
    //{
    //  get {return this.subset;}
    //}
    //bool subset;

    /// <summary>
    /// Gets a value indicating whether the font is encoded as Unicode.
    /// </summary>
    [Obsolete("Use FontEncoding")]
    public bool Unicode
    {
      get { return this.fontEncoding == PdfFontEncoding.Unicode; }
    }
    //bool unicode;


    /// <summary>
    /// Gets a value indicating how the font is encoded.
    /// </summary>
    public PdfFontEncoding FontEncoding
    {
      get { return this.fontEncoding; }
    }
    PdfFontEncoding fontEncoding;

    /// <summary>
    /// Not yet implemented.
    /// </summary>
    [Obsolete("Not yet implemented")]
    public string BaseFont
    {
      //get { return this.baseFont; }
      get { return ""; }
    }
    //string baseFont = "";

    /// <summary>
    /// Not yet implemented.
    /// </summary>
    [Obsolete("Not yet implemented")]
    public string FontFile
    {
      //get { return this.fontFile; }
      get { return ""; }
    }
    //string fontFile = "";

    /// <summary>
    /// Gets the font image.
    /// </summary>
    [Obsolete("Not yet implemented")]
    public byte[] FontData
    {
      //get { return this.fontData; }
      get { return null; }
    }
    //byte[] fontData;

    // this is part of XGraphics
    //    public double CharacterSpacing;
    //    public double WordSpacing;
    //    public double HorizontalScaling;
    //    public double Leading;
    //    public double TextRise;
    //    Kerning
  }
}
