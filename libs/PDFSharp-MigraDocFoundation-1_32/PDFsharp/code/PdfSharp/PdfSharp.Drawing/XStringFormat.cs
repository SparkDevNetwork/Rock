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
using System.IO;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows.Media;
#endif
using PdfSharp.Internal;

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Not used in this implementation.
  /// </summary>
  [Flags]
  public enum XStringFormatFlags
  {
    //DirectionRightToLeft  = 0x0001,
    //DirectionVertical     = 0x0002,
    //FitBlackBox           = 0x0004,
    //DisplayFormatControl  = 0x0020,
    //NoFontFallback        = 0x0400,
    /// <summary>
    /// The default value.
    /// </summary>
    MeasureTrailingSpaces = 0x0800,
    //NoWrap                = 0x1000,
    //LineLimit             = 0x2000,
    //NoClip                = 0x4000,
  }

  ////public enum StringTrimming
  ////{
  ////  None              = 0,
  ////  Character         = 1,
  ////  Word              = 2,
  ////  EllipsisCharacter = 3,
  ////  EllipsisWord      = 4,
  ////  EllipsisPath      = 5,
  ////}

  /// <summary>
  /// Represents the text layout information.
  /// </summary>
  public class XStringFormat
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="XStringFormat"/> class.
    /// </summary>
    public XStringFormat()
    {
#if GDI
      // We must clone GenericTypographic, otherwise we change the original!
      this.stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
#endif
    }

    //TODO public StringFormat(StringFormat format);
    //public StringFormat(StringFormatFlags options);
    //public StringFormat(StringFormatFlags options, int language);
    //public object Clone();
    //public void Dispose();
    //private void Dispose(bool disposing);
    //protected override void Finalize();
    //public float[] GetTabStops(out float firstTabOffset);
    //public void SetDigitSubstitution(int language, StringDigitSubstitute substitute);
    //public void SetMeasurableCharacterRanges(CharacterRange[] ranges);
    //public void SetTabStops(float firstTabOffset, float[] tabStops);
    //public override string ToString();

    /// <summary>
    /// Gets or sets horizontal text alignment information.
    /// </summary>
    public XStringAlignment Alignment
    { 
      get {return this.alignment;}
      set
      {
        this.alignment = value;
#if GDI
        this.stringFormat.Alignment = (StringAlignment)value;
#endif
      }
    }
    XStringAlignment alignment;

    //public int DigitSubstitutionLanguage { get; }
    //public StringDigitSubstitute DigitSubstitutionMethod { get; }
    //public StringFormatFlags FormatFlags { get; set; }
    //public static StringFormat GenericDefault { get; }
    //public static StringFormat GenericTypographic { get; }
    //public HotkeyPrefix HotkeyPrefix { get; set; }

    /// <summary>
    /// Gets or sets the line alignment.
    /// </summary>
    public XLineAlignment LineAlignment
    { 
      get {return this.lineAlignment;}
      set
      {
        this.lineAlignment = value;
#if GDI
        if (value == XLineAlignment.BaseLine)
          this.stringFormat.LineAlignment = StringAlignment.Near;
        else
          this.stringFormat.LineAlignment = (StringAlignment)value;
#endif
      }
    }
    XLineAlignment lineAlignment;

    //public StringTrimming Trimming { get; set; }

    /// <summary>
    /// Gets a new XStringFormat object that aligns the text left on the base line.
    /// </summary>
    [Obsolete("Use XStringFormats.Default. (Note plural in class name.)")]
    public static XStringFormat Default
    {
      get
      {
        XStringFormat format = new XStringFormat();
        format.LineAlignment = XLineAlignment.BaseLine;
        return format;
      }
    }

    /// <summary>
    /// Gets a new XStringFormat object that aligns the text top left of the layout rectangle.
    /// </summary>
    [Obsolete("Use XStringFormats.Default. (Note plural in class name.)")]
    public static XStringFormat TopLeft
    {
      get
      {
        XStringFormat format = new XStringFormat();
        format.Alignment = XStringAlignment.Near;
        format.LineAlignment = XLineAlignment.Near;
        return format;
      }
    }

    /// <summary>
    /// Gets a new XStringFormat object that centers the text in the middle of the layout rectangle.
    /// </summary>
    [Obsolete("Use XStringFormats.Center. (Note plural in class name.)")]
    public static XStringFormat Center
    {
      get
      {
        XStringFormat format = new XStringFormat();
        format.Alignment = XStringAlignment.Center;
        format.LineAlignment = XLineAlignment.Center;
        return format;
      }
    }

    /// <summary>
    /// Gets a new XStringFormat object that centers the text at the top of the layout rectangle.
    /// </summary>
    [Obsolete("Use XStringFormats.TopCenter. (Note plural in class name.)")]
    public static XStringFormat TopCenter
    {
      get
      {
        XStringFormat format = new XStringFormat();
        format.Alignment = XStringAlignment.Center;
        format.LineAlignment = XLineAlignment.Near;
        return format;
      }
    }

    /// <summary>
    /// Gets a new XStringFormat object that centers the text at the bottom of the layout rectangle.
    /// </summary>
    [Obsolete("Use XStringFormats.BottomCenter. (Note plural in class name.)")]
    public static XStringFormat BottomCenter
    {
      get
      {
        XStringFormat format = new XStringFormat();
        format.Alignment = XStringAlignment.Center;
        format.LineAlignment = XLineAlignment.Far;
        return format;
      }
    }

    /// <summary>
    /// Gets or sets flags with format information.
    /// </summary>
    public XStringFormatFlags FormatFlags
    {
      get {return this.formatFlags;}
      set
      {
        this.formatFlags = value;
#if GDI
        this.stringFormat.FormatFlags = (StringFormatFlags)value;
#endif
      }
    }
    private XStringFormatFlags formatFlags;

#if GDI
    internal StringFormat RealizeGdiStringFormat()
    {
      if (this.stringFormat == null)
      {
        this.stringFormat = StringFormat.GenericTypographic; //.GenericDefault;
        this.stringFormat.Alignment = (StringAlignment)this.alignment;
        this.stringFormat.LineAlignment = (StringAlignment)this.lineAlignment;
        this.stringFormat.FormatFlags = (StringFormatFlags)this.formatFlags;
      }
      return this.stringFormat;
    }
    StringFormat stringFormat;
#endif
  }
}
