#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Stefan Lange (mailto:Stefan.Lange@pdfsharp.com)
//
// Copyright (c) 2005-2011 empira Software GmbH, Cologne (Germany)
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
#endif
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif
using PdfSharp.Fonts;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Drawing.Pdf;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Internal
{
  /// <summary>
  /// Helper class for fonts in PDFsharp 1.4.
  /// </summary>
  public static class FontHelper14
  {
#if WPF_
    /// <summary>
    /// </summary>
    public static XSize MeasureStringWpf(string text, XFont font, XStringFormat stringFormat)
    {
      FormattedText formattedText = FontHelper.CreateFormattedText(text, font.typeface, font.Size, System.Windows.Media.Brushes.Black);
      XSize wpfSize = new XSize(formattedText.WidthIncludingTrailingWhitespace, formattedText.Height);
      return wpfSize;
    }
#endif

    // ...took a look at the source code of WPF. About 50 classes and several 10,000 lines of code
    // deal with that what colloquial is called 'fonts'.
    // So let's start simple.
    /// <summary>
    /// Simple measure string function.
    /// </summary>
    public static XSize MeasureString(string text, XFont font, XStringFormat stringFormat)
    {
      XSize size = new XSize();

      OpenTypeDescriptor descriptor = FontDescriptorStock.Global.CreateDescriptor(font) as OpenTypeDescriptor;
      if (descriptor != null)
      {
        size.Height = (descriptor.Ascender + Math.Abs(descriptor.Descender)) * font.Size / font.unitsPerEm;
        Debug.Assert(descriptor.Ascender > 0);

        bool symbol = descriptor.fontData.cmap.symbol;
        int length = text.Length;
        int width = 0;
        for (int idx = 0; idx < length; idx++)
        {
          char ch = text[idx];
          int glyphIndex = 0;
          if (symbol)
          {
            glyphIndex = ch + (descriptor.fontData.os2.usFirstCharIndex & 0xFF00); // @@@
            glyphIndex = descriptor.CharCodeToGlyphIndex((char)glyphIndex);
          }
          else
            glyphIndex = descriptor.CharCodeToGlyphIndex(ch);

          //double width = descriptor.GlyphIndexToEmfWidth(glyphIndex, font.Size);
          //size.Width += width;
          width += descriptor.GlyphIndexToWidth(glyphIndex);
        }
        size.Width = width * font.Size * (font.Italic ? 1 : 1) / descriptor.UnitsPerEm;
      }
      Debug.Assert(descriptor != null, "No OpenTypeDescriptor.");
      return size;
    }
  }
}