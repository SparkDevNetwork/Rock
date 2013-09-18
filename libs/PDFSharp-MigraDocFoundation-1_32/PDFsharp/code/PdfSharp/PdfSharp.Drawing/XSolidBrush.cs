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
using System.IO;
#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp.Internal;

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Defines a single color object used to fill shapes and draw text.
  /// </summary>
  public class XSolidBrush : XBrush
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="XSolidBrush"/> class.
    /// </summary>
    public XSolidBrush()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XSolidBrush"/> class.
    /// </summary>
    public XSolidBrush(XColor color)
      : this(color, false)
    {
    }

    internal XSolidBrush(XColor color, bool immutable)
    {
      this.color = color;
      this.immutable = immutable;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XSolidBrush"/> class.
    /// </summary>
    public XSolidBrush(XSolidBrush brush)
    {
      this.color = brush.Color;
    }

    /// <summary>
    /// Gets or sets the color of this brush.
    /// </summary>
    public XColor Color
    {
      get { return this.color; }
      set
      {
        if (this.immutable)
          throw new ArgumentException(PSSR.CannotChangeImmutableObject("XSolidBrush"));
        this.color = value;
#if GDI
        this.gdiDirty = this.gdiDirty || this.color != value;
#endif
#if WPF
        this.wpfDirty = this.wpfDirty || this.color != value;
#endif
#if GDI && WPF
        this.gdiDirty = this.wpfDirty = true;
#endif
      }
    }
    internal XColor color;

#if GDI
    internal override System.Drawing.Brush RealizeGdiBrush()
    {
      if (this.gdiDirty)
      {
        if (this.gdiBrush == null)
          this.gdiBrush = new SolidBrush(this.color.ToGdiColor());
        else
          this.gdiBrush.Color = this.color.ToGdiColor();
        this.gdiDirty = false;
      }

#if DEBUG
      System.Drawing.Color clr = this.color.ToGdiColor();
      SolidBrush brush1 = new SolidBrush(clr);
      Debug.Assert(this.gdiBrush.Color == brush1.Color);
#endif
      return this.gdiBrush;
    }
#endif

#if WPF
    internal override System.Windows.Media.Brush RealizeWpfBrush()
    {
      if (this.wpfDirty)
      {
        if (this.wpfBrush == null)
          this.wpfBrush = new SolidColorBrush(this.color.ToWpfColor());
        else
          this.wpfBrush.Color = this.color.ToWpfColor();
        this.wpfDirty = false;
      }

#if DEBUG_
      System.Windows.Media.Color clr = this.color.ToWpfColor();
      System.Windows.Media.SolidColorBrush brush1 = new System.Windows.Media.SolidColorBrush(clr); //System.Drawing.Color.FromArgb(128, 128, 0, 0));
      Debug.Assert(this.wpfBrush.Color == brush1.Color);  // Crashes during unit testing
#endif
      return this.wpfBrush;
    }
#endif

#if GDI
    bool gdiDirty = true;
    SolidBrush gdiBrush;
#endif
#if WPF
    bool wpfDirty = true;
    SolidColorBrush wpfBrush;
#endif
    bool immutable;
  }
}