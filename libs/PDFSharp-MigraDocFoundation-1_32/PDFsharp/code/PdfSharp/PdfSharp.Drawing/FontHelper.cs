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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
//#if GDI
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.Drawing.Imaging;
//#endif
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Drawing.Pdf;
using PdfSharp.Fonts;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Drawing
{
#if WPF
  /// <summary>
  /// The Get WPF Value flags.
  /// </summary>
  enum GWV
  {
    GetCellAscent,
    GetCellDescent,
    GetEmHeight,
    GetLineSpacing,
    //IsStyleAvailable
  }

  /// <summary>
  /// Helper class for fonts.
  /// </summary>
  static class FontHelper
  {
    //private const string testFontName = "Times New Roman";
    //const string testFontName = "Segoe Condensed";
    //const string testFontName = "Frutiger LT 45 Light";

    //static FontHelper()
    //{
    //  FontFamily fontFamily = new FontFamily(testFontName);
    //  s_typefaces = new List<Typeface>(fontFamily.GetTypefaces());
    //}

    //private static List<Typeface> s_typefaces;

    /// <summary>
    /// Creates a typeface.
    /// </summary>
    public static Typeface CreateTypeface(FontFamily family, XFontStyle style)
    {
      // BUG: does not work with fonts that have others than the four default styles
      FontStyle fontStyle = FontStyleFromStyle(style);
      FontWeight fontWeight = FontWeightFromStyle(style);
      Typeface typeface = new Typeface(family, fontStyle, fontWeight, FontStretches.Normal);

      //List<Typeface> typefaces = new List<Typeface>(fontFamily.GetTypefaces());
      //typefaces.GetType();
      //Typeface typeface = typefaces[3];

      return typeface;
    }

#if !SILVERLIGHT
    /// <summary>
    /// Creates the formatted text.
    /// </summary>
    public static FormattedText CreateFormattedText(string text, Typeface typeface, double emSize, Brush brush)
    {
      //FontFamily fontFamily = new FontFamily(testFontName);
      //typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Bold, FontStretches.Condensed);
      //List<Typeface> typefaces = new List<Typeface>(fontFamily.GetTypefaces());
      //typefaces.GetType();
      //typeface = s_typefaces[0];

      // BUG: does not work with fonts that have others than the four default styles
      FormattedText formattedText = new FormattedText(text, new CultureInfo("en-us"), FlowDirection.LeftToRight, typeface, emSize, brush);
      //formattedText.SetFontWeight(FontWeights.Bold);
      //formattedText.SetFontStyle(FontStyles.Oblique);
      //formattedText.SetFontStretch(FontStretches.Condensed);
      return formattedText;
    }
#endif

#if SILVERLIGHT
    /// <summary>
    /// Creates the TextBlock.
    /// </summary>
    public static TextBlock CreateTextBlock(string text, Typeface typeface, double emSize, Brush brush)
    {
      TextBlock textBlock = new TextBlock();
      textBlock.FontFamily = new FontFamily("Verdana");
      textBlock.FontSize = emSize;
      textBlock.Foreground = brush;
      textBlock.Text = text;

      return textBlock;
    }
#endif

    /// <summary>
    /// Simple hack to make it work...
    /// </summary>
    public static FontStyle FontStyleFromStyle(XFontStyle style)
    {
      switch (style & XFontStyle.BoldItalic)  // mask out Underline and Strikeout
      {
        case XFontStyle.Regular:
          return FontStyles.Normal;

        case XFontStyle.Bold:
          return FontStyles.Normal;

        case XFontStyle.Italic:
          return FontStyles.Italic;

        case XFontStyle.BoldItalic:
          return FontStyles.Italic;
      }
      return FontStyles.Normal;
    }

    /// <summary>
    /// Simple hack to make it work...
    /// </summary>
    public static FontWeight FontWeightFromStyle(XFontStyle style)
    {
      switch (style)
      {
        case XFontStyle.Regular:
          return FontWeights.Normal;

        case XFontStyle.Bold:
          return FontWeights.Bold;

        case XFontStyle.Italic:
          return FontWeights.Normal;

        case XFontStyle.BoldItalic:
          return FontWeights.Bold;
      }
      return FontWeights.Normal;
    }

    public static int GetWpfValue(XFontFamily family, XFontStyle style, GWV value)
    {
      FontDescriptor descriptor = FontDescriptorStock.Global.CreateDescriptor(family, style);
      XFontMetrics metrics = descriptor.FontMetrics;

      switch (value)
      {
        case GWV.GetCellAscent:
          return metrics.Ascent;

        case GWV.GetCellDescent:
          return Math.Abs(metrics.Descent);

        case GWV.GetEmHeight:
          //return (int)metrics.CapHeight;
          return metrics.UnitsPerEm;

        case GWV.GetLineSpacing:
          return metrics.Ascent + Math.Abs(metrics.Descent) + metrics.Leading;

        default:
          throw new InvalidOperationException("Unknown GWV value.");
        // DELETE
        //case GWV.IsStyleAvailable:
        //  style &= XFontStyle.Regular | XFontStyle.Bold | XFontStyle.Italic | XFontStyle.BoldItalic; // same as XFontStyle.BoldItalic
        //  List<Typeface> s_typefaces = new List<Typeface>(family.wpfFamily.GetTypefaces());
        //  foreach (Typeface typeface in s_typefaces)
        //  {
        //  }
        //  Debugger.Break();
        ////typeface.Style = FontStyles.
      }
    }

    /// <summary>
    /// Determines whether the style is available as a glyph type face in the specified font family, i.e. the specified style is not simulated.
    /// </summary>
    public static bool IsStyleAvailable(XFontFamily family, XFontStyle style)
    {
#if !SILVERLIGHT
      // TODOWPF: check for correctness
      FontDescriptor descriptor = FontDescriptorStock.Global.CreateDescriptor(family, style);
      XFontMetrics metrics = descriptor.FontMetrics;

      style &= XFontStyle.Regular | XFontStyle.Bold | XFontStyle.Italic | XFontStyle.BoldItalic; // same as XFontStyle.BoldItalic
      List<Typeface> typefaces = new List<Typeface>(family.wpfFamily.GetTypefaces());
      foreach (Typeface typeface in typefaces)
      {
        bool available = false;
        GlyphTypeface glyphTypeface;
        if (typeface.TryGetGlyphTypeface(out glyphTypeface))
        {
#if DEBUG
          glyphTypeface.GetType();
#endif
          available = true;
        }

#if DEBUG_ // 
        int weightClass = typeface.Weight.ToOpenTypeWeight();
        switch (style)
        {
          case XFontStyle.Regular:
            //if (typeface.TryGetGlyphTypeface(.Style == FontStyles.Normal && typeface.Weight== FontWeights.Normal.)
            break;

          case XFontStyle.Bold:
            break;

          case XFontStyle.Italic:
            break;

          case XFontStyle.BoldItalic:
            break;
        }
#endif
        if (available)
          return true;
      }
      return false;
#else
      return true; // AGHACK
#endif
    }
  }
#endif
}