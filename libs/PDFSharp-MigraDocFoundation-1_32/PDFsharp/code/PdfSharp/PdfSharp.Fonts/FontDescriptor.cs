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

namespace PdfSharp.Fonts
{
  /// <summary>
  /// Base class for all font descriptors.
  /// </summary>
  internal class FontDescriptor
  {
    /// <summary>
    /// 
    /// </summary>
    public string FontFile
    {
      get { return this.fontFile; }
    }
    protected string fontFile;

    /// <summary>
    /// 
    /// </summary>
    public string FontType
    {
      get { return this.fontType; }
    }
    protected string fontType;

    /// <summary>
    /// 
    /// </summary>
    public string FontName
    {
      get { return this.fontName; }
    }
    protected string fontName;

    /// <summary>
    /// 
    /// </summary>
    public string FullName
    {
      get { return this.fullName; }
    }
    protected string fullName;

    /// <summary>
    /// 
    /// </summary>
    public string FamilyName
    {
      get { return this.familyName; }
    }
    protected string familyName;

    /// <summary>
    /// 
    /// </summary>
    public string Weight
    {
      get { return this.weight; }
    }
    protected string weight;

    /// <summary>
    /// Gets a value indicating whether this instance belongs to a bold font.
    /// </summary>
    public virtual bool IsBoldFace
    {
      get { return false; }
    }

    /// <summary>
    /// 
    /// </summary>
    public float ItalicAngle
    {
      get { return this.italicAngle; }
    }
    protected float italicAngle;

    /// <summary>
    /// Gets a value indicating whether this instance belongs to an italic font.
    /// </summary>
    public virtual bool IsItalicFace
    {
      get { return false; }
    }

    /// <summary>
    /// 
    /// </summary>
    public int XMin
    {
      get { return this.xMin; }
    }
    protected int xMin;

    /// <summary>
    /// 
    /// </summary>
    public int YMin
    {
      get { return this.yMin; }
    }
    protected int yMin;

    /// <summary>
    /// 
    /// </summary>
    public int XMax
    {
      get { return this.xMax; }
    }
    protected int xMax;

    /// <summary>
    /// 
    /// </summary>
    public int YMax
    {
      get { return this.yMax; }
    }
    protected int yMax;

    /// <summary>
    /// 
    /// </summary>
    public bool IsFixedPitch
    {
      get { return this.isFixedPitch; }
    }
    protected bool isFixedPitch;

    //Rect FontBBox;

    /// <summary>
    /// 
    /// </summary>
    public int UnderlinePosition
    {
      get { return this.underlinePosition; }
    }
    protected int underlinePosition;

    /// <summary>
    /// 
    /// </summary>
    public int UnderlineThickness
    {
      get { return this.underlineThickness; }
    }
    protected int underlineThickness;

    /// <summary>
    /// 
    /// </summary>
    public int StrikeoutPosition
    {
      get { return this.strikeoutPosition; }
    }
    protected int strikeoutPosition;

    /// <summary>
    /// 
    /// </summary>
    public int StrikeoutSize
    {
      get { return this.strikeoutSize; }
    }
    protected int strikeoutSize;

    /// <summary>
    /// 
    /// </summary>
    public string Version
    {
      get { return this.version; }
    }
    protected string version;

    ///// <summary>
    ///// 
    ///// </summary>
    //public string Notice
    //{
    //  get { return this.Notice; }
    //}
    //protected string notice;

    /// <summary>
    /// 
    /// </summary>
    public string EncodingScheme
    {
      get { return this.encodingScheme; }
    }
    protected string encodingScheme;

    /// <summary>
    /// 
    /// </summary>
    public int UnitsPerEm
    {
      get { return this.unitsPerEm; }
    }
    protected int unitsPerEm;

    /// <summary>
    /// 
    /// </summary>
    public int CapHeight
    {
      get { return this.capHeight; }
    }
    protected int capHeight;

    /// <summary>
    /// 
    /// </summary>
    public int XHeight
    {
      get { return this.xHeight; }
    }
    protected int xHeight;

    /// <summary>
    /// 
    /// </summary>
    public int Ascender
    {
      get { return this.ascender; }
    }
    protected int ascender;

    /// <summary>
    /// 
    /// </summary>
    public int Descender
    {
      get { return this.descender; }
    }
    protected int descender;

    /// <summary>
    /// 
    /// </summary>
    public int Leading
    {
      get { return this.leading; }
    }
    protected int leading;

    /// <summary>
    /// 
    /// </summary>
    public int Flags
    {
      get { return this.flags; }
    }
    protected int flags;

    /// <summary>
    /// 
    /// </summary>
    public int StemV
    {
      get { return this.stemV; }
    }
    protected int stemV;

    /// <summary>
    /// Under Construction
    /// </summary>
    public XFontMetrics FontMetrics
    {
      get
      {
        if (this.fontMetrics == null)
        {
          this.fontMetrics = new XFontMetrics(this.fontName, this.unitsPerEm, this.ascender, this.descender, this.leading, this.capHeight,
            this.xHeight, this.stemV, 0, 0, 0);
        }
        return this.fontMetrics;
      }
    }
    XFontMetrics fontMetrics;
  }
}