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

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Brushes for all the pre-defined colors.
  /// </summary>
  public static class XBrushes
  {
    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush AliceBlue
    {
      get
      {
        if (XBrushes.aliceBlue == null)
          XBrushes.aliceBlue = new XSolidBrush(XColors.AliceBlue, true);
        return XBrushes.aliceBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush AntiqueWhite
    {
      get
      {
        if (XBrushes.antiqueWhite == null)
          XBrushes.antiqueWhite = new XSolidBrush(XColors.AntiqueWhite, true);
        return XBrushes.antiqueWhite;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Aqua
    {
      get
      {
        if (XBrushes.aqua == null)
          XBrushes.aqua = new XSolidBrush(XColors.Aqua, true);
        return XBrushes.aqua;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Aquamarine
    {
      get
      {
        if (XBrushes.aquamarine == null)
          XBrushes.aquamarine = new XSolidBrush(XColors.Aquamarine, true);
        return XBrushes.aquamarine;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Azure
    {
      get
      {
        if (XBrushes.azure == null)
          XBrushes.azure = new XSolidBrush(XColors.Azure, true);
        return XBrushes.azure;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Beige
    {
      get
      {
        if (XBrushes.beige == null)
          XBrushes.beige = new XSolidBrush(XColors.Beige, true);
        return XBrushes.beige;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Bisque
    {
      get
      {
        if (XBrushes.bisque == null)
          XBrushes.bisque = new XSolidBrush(XColors.Bisque, true);
        return XBrushes.bisque;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Black
    {
      get
      {
        if (XBrushes.black == null)
          XBrushes.black = new XSolidBrush(XColors.Black, true);
        return XBrushes.black;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush BlanchedAlmond
    {
      get
      {
        if (XBrushes.blanchedAlmond == null)
          XBrushes.blanchedAlmond = new XSolidBrush(XColors.BlanchedAlmond, true);
        return XBrushes.blanchedAlmond;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Blue
    {
      get
      {
        if (XBrushes.blue == null)
          XBrushes.blue = new XSolidBrush(XColors.Blue, true);
        return XBrushes.blue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush BlueViolet
    {
      get
      {
        if (XBrushes.blueViolet == null)
          XBrushes.blueViolet = new XSolidBrush(XColors.BlueViolet, true);
        return XBrushes.blueViolet;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Brown
    {
      get
      {
        if (XBrushes.brown == null)
          XBrushes.brown = new XSolidBrush(XColors.Brown, true);
        return XBrushes.brown;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush BurlyWood
    {
      get
      {
        if (XBrushes.burlyWood == null)
          XBrushes.burlyWood = new XSolidBrush(XColors.BurlyWood, true);
        return XBrushes.burlyWood;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush CadetBlue
    {
      get
      {
        if (XBrushes.cadetBlue == null)
          XBrushes.cadetBlue = new XSolidBrush(XColors.CadetBlue, true);
        return XBrushes.cadetBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Chartreuse
    {
      get
      {
        if (XBrushes.chartreuse == null)
          XBrushes.chartreuse = new XSolidBrush(XColors.Chartreuse, true);
        return XBrushes.chartreuse;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Chocolate
    {
      get
      {
        if (XBrushes.chocolate == null)
          XBrushes.chocolate = new XSolidBrush(XColors.Chocolate, true);
        return XBrushes.chocolate;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Coral
    {
      get
      {
        if (XBrushes.coral == null)
          XBrushes.coral = new XSolidBrush(XColors.Coral, true);
        return XBrushes.coral;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush CornflowerBlue
    {
      get
      {
        if (XBrushes.cornflowerBlue == null)
          XBrushes.cornflowerBlue = new XSolidBrush(XColors.CornflowerBlue, true);
        return XBrushes.cornflowerBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Cornsilk
    {
      get
      {
        if (XBrushes.cornsilk == null)
          XBrushes.cornsilk = new XSolidBrush(XColors.Cornsilk, true);
        return XBrushes.cornsilk;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Crimson
    {
      get
      {
        if (XBrushes.crimson == null)
          XBrushes.crimson = new XSolidBrush(XColors.Crimson, true);
        return XBrushes.crimson;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Cyan
    {
      get
      {
        if (XBrushes.cyan == null)
          XBrushes.cyan = new XSolidBrush(XColors.Cyan, true);
        return XBrushes.cyan;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkBlue
    {
      get
      {
        if (XBrushes.darkBlue == null)
          XBrushes.darkBlue = new XSolidBrush(XColors.DarkBlue, true);
        return XBrushes.darkBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkCyan
    {
      get
      {
        if (XBrushes.darkCyan == null)
          XBrushes.darkCyan = new XSolidBrush(XColors.DarkCyan, true);
        return XBrushes.darkCyan;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkGoldenrod
    {
      get
      {
        if (XBrushes.darkGoldenrod == null)
          XBrushes.darkGoldenrod = new XSolidBrush(XColors.DarkGoldenrod, true);
        return XBrushes.darkGoldenrod;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkGray
    {
      get
      {
        if (XBrushes.darkGray == null)
          XBrushes.darkGray = new XSolidBrush(XColors.DarkGray, true);
        return XBrushes.darkGray;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkGreen
    {
      get
      {
        if (XBrushes.darkGreen == null)
          XBrushes.darkGreen = new XSolidBrush(XColors.DarkGreen, true);
        return XBrushes.darkGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkKhaki
    {
      get
      {
        if (XBrushes.darkKhaki == null)
          XBrushes.darkKhaki = new XSolidBrush(XColors.DarkKhaki, true);
        return XBrushes.darkKhaki;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkMagenta
    {
      get
      {
        if (XBrushes.darkMagenta == null)
          XBrushes.darkMagenta = new XSolidBrush(XColors.DarkMagenta, true);
        return XBrushes.darkMagenta;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkOliveGreen
    {
      get
      {
        if (XBrushes.darkOliveGreen == null)
          XBrushes.darkOliveGreen = new XSolidBrush(XColors.DarkOliveGreen, true);
        return XBrushes.darkOliveGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkOrange
    {
      get
      {
        if (XBrushes.darkOrange == null)
          XBrushes.darkOrange = new XSolidBrush(XColors.DarkOrange, true);
        return XBrushes.darkOrange;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkOrchid
    {
      get
      {
        if (XBrushes.darkOrchid == null)
          XBrushes.darkOrchid = new XSolidBrush(XColors.DarkOrchid, true);
        return XBrushes.darkOrchid;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkRed
    {
      get
      {
        if (XBrushes.darkRed == null)
          XBrushes.darkRed = new XSolidBrush(XColors.DarkRed, true);
        return XBrushes.darkRed;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkSalmon
    {
      get
      {
        if (XBrushes.darkSalmon == null)
          XBrushes.darkSalmon = new XSolidBrush(XColors.DarkSalmon, true);
        return XBrushes.darkSalmon;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkSeaGreen
    {
      get
      {
        if (XBrushes.darkSeaGreen == null)
          XBrushes.darkSeaGreen = new XSolidBrush(XColors.DarkSeaGreen, true);
        return XBrushes.darkSeaGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkSlateBlue
    {
      get
      {
        if (XBrushes.darkSlateBlue == null)
          XBrushes.darkSlateBlue = new XSolidBrush(XColors.DarkSlateBlue, true);
        return XBrushes.darkSlateBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkSlateGray
    {
      get
      {
        if (XBrushes.darkSlateGray == null)
          XBrushes.darkSlateGray = new XSolidBrush(XColors.DarkSlateGray, true);
        return XBrushes.darkSlateGray;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkTurquoise
    {
      get
      {
        if (XBrushes.darkTurquoise == null)
          XBrushes.darkTurquoise = new XSolidBrush(XColors.DarkTurquoise, true);
        return XBrushes.darkTurquoise;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DarkViolet
    {
      get
      {
        if (XBrushes.darkViolet == null)
          XBrushes.darkViolet = new XSolidBrush(XColors.DarkViolet, true);
        return XBrushes.darkViolet;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DeepPink
    {
      get
      {
        if (XBrushes.deepPink == null)
          XBrushes.deepPink = new XSolidBrush(XColors.DeepPink, true);
        return XBrushes.deepPink;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DeepSkyBlue
    {
      get
      {
        if (XBrushes.deepSkyBlue == null)
          XBrushes.deepSkyBlue = new XSolidBrush(XColors.DeepSkyBlue, true);
        return XBrushes.deepSkyBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DimGray
    {
      get
      {
        if (XBrushes.dimGray == null)
          XBrushes.dimGray = new XSolidBrush(XColors.DimGray, true);
        return XBrushes.dimGray;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush DodgerBlue
    {
      get
      {
        if (XBrushes.dodgerBlue == null)
          XBrushes.dodgerBlue = new XSolidBrush(XColors.DodgerBlue, true);
        return XBrushes.dodgerBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Firebrick
    {
      get
      {
        if (XBrushes.firebrick == null)
          XBrushes.firebrick = new XSolidBrush(XColors.Firebrick, true);
        return XBrushes.firebrick;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush FloralWhite
    {
      get
      {
        if (XBrushes.floralWhite == null)
          XBrushes.floralWhite = new XSolidBrush(XColors.FloralWhite, true);
        return XBrushes.floralWhite;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush ForestGreen
    {
      get
      {
        if (XBrushes.forestGreen == null)
          XBrushes.forestGreen = new XSolidBrush(XColors.ForestGreen, true);
        return XBrushes.forestGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Fuchsia
    {
      get
      {
        if (XBrushes.fuchsia == null)
          XBrushes.fuchsia = new XSolidBrush(XColors.Fuchsia, true);
        return XBrushes.fuchsia;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Gainsboro
    {
      get
      {
        if (XBrushes.gainsboro == null)
          XBrushes.gainsboro = new XSolidBrush(XColors.Gainsboro, true);
        return XBrushes.gainsboro;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush GhostWhite
    {
      get
      {
        if (XBrushes.ghostWhite == null)
          XBrushes.ghostWhite = new XSolidBrush(XColors.GhostWhite, true);
        return XBrushes.ghostWhite;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Gold
    {
      get
      {
        if (XBrushes.gold == null)
          XBrushes.gold = new XSolidBrush(XColors.Gold, true);
        return XBrushes.gold;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Goldenrod
    {
      get
      {
        if (XBrushes.goldenrod == null)
          XBrushes.goldenrod = new XSolidBrush(XColors.Goldenrod, true);
        return XBrushes.goldenrod;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Gray
    {
      get
      {
        if (XBrushes.gray == null)
          XBrushes.gray = new XSolidBrush(XColors.Gray, true);
        return XBrushes.gray;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Green
    {
      get
      {
        if (XBrushes.green == null)
          XBrushes.green = new XSolidBrush(XColors.Green, true);
        return XBrushes.green;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush GreenYellow
    {
      get
      {
        if (XBrushes.greenYellow == null)
          XBrushes.greenYellow = new XSolidBrush(XColors.GreenYellow, true);
        return XBrushes.greenYellow;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Honeydew
    {
      get
      {
        if (XBrushes.honeydew == null)
          XBrushes.honeydew = new XSolidBrush(XColors.Honeydew, true);
        return XBrushes.honeydew;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush HotPink
    {
      get
      {
        if (XBrushes.hotPink == null)
          XBrushes.hotPink = new XSolidBrush(XColors.HotPink, true);
        return XBrushes.hotPink;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush IndianRed
    {
      get
      {
        if (XBrushes.indianRed == null)
          XBrushes.indianRed = new XSolidBrush(XColors.IndianRed, true);
        return XBrushes.indianRed;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Indigo
    {
      get
      {
        if (XBrushes.indigo == null)
          XBrushes.indigo = new XSolidBrush(XColors.Indigo, true);
        return XBrushes.indigo;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Ivory
    {
      get
      {
        if (XBrushes.ivory == null)
          XBrushes.ivory = new XSolidBrush(XColors.Ivory, true);
        return XBrushes.ivory;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Khaki
    {
      get
      {
        if (XBrushes.khaki == null)
          XBrushes.khaki = new XSolidBrush(XColors.Khaki, true);
        return XBrushes.khaki;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Lavender
    {
      get
      {
        if (XBrushes.lavender == null)
          XBrushes.lavender = new XSolidBrush(XColors.Lavender, true);
        return XBrushes.lavender;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LavenderBlush
    {
      get
      {
        if (XBrushes.lavenderBlush == null)
          XBrushes.lavenderBlush = new XSolidBrush(XColors.LavenderBlush, true);
        return XBrushes.lavenderBlush;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LawnGreen
    {
      get
      {
        if (XBrushes.lawnGreen == null)
          XBrushes.lawnGreen = new XSolidBrush(XColors.LawnGreen, true);
        return XBrushes.lawnGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LemonChiffon
    {
      get
      {
        if (XBrushes.lemonChiffon == null)
          XBrushes.lemonChiffon = new XSolidBrush(XColors.LemonChiffon, true);
        return XBrushes.lemonChiffon;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightBlue
    {
      get
      {
        if (XBrushes.lightBlue == null)
          XBrushes.lightBlue = new XSolidBrush(XColors.LightBlue, true);
        return XBrushes.lightBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightCoral
    {
      get
      {
        if (XBrushes.lightCoral == null)
          XBrushes.lightCoral = new XSolidBrush(XColors.LightCoral, true);
        return XBrushes.lightCoral;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightCyan
    {
      get
      {
        if (XBrushes.lightCyan == null)
          XBrushes.lightCyan = new XSolidBrush(XColors.LightCyan, true);
        return XBrushes.lightCyan;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightGoldenrodYellow
    {
      get
      {
        if (XBrushes.lightGoldenrodYellow == null)
          XBrushes.lightGoldenrodYellow = new XSolidBrush(XColors.LightGoldenrodYellow, true);
        return XBrushes.lightGoldenrodYellow;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightGray
    {
      get
      {
        if (XBrushes.lightGray == null)
          XBrushes.lightGray = new XSolidBrush(XColors.LightGray, true);
        return XBrushes.lightGray;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightGreen
    {
      get
      {
        if (XBrushes.lightGreen == null)
          XBrushes.lightGreen = new XSolidBrush(XColors.LightGreen, true);
        return XBrushes.lightGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightPink
    {
      get
      {
        if (XBrushes.lightPink == null)
          XBrushes.lightPink = new XSolidBrush(XColors.LightPink, true);
        return XBrushes.lightPink;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightSalmon
    {
      get
      {
        if (XBrushes.lightSalmon == null)
          XBrushes.lightSalmon = new XSolidBrush(XColors.LightSalmon, true);
        return XBrushes.lightSalmon;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightSeaGreen
    {
      get
      {
        if (XBrushes.lightSeaGreen == null)
          XBrushes.lightSeaGreen = new XSolidBrush(XColors.LightSeaGreen, true);
        return XBrushes.lightSeaGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightSkyBlue
    {
      get
      {
        if (XBrushes.lightSkyBlue == null)
          XBrushes.lightSkyBlue = new XSolidBrush(XColors.LightSkyBlue, true);
        return XBrushes.lightSkyBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightSlateGray
    {
      get
      {
        if (XBrushes.lightSlateGray == null)
          XBrushes.lightSlateGray = new XSolidBrush(XColors.LightSlateGray, true);
        return XBrushes.lightSlateGray;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightSteelBlue
    {
      get
      {
        if (XBrushes.lightSteelBlue == null)
          XBrushes.lightSteelBlue = new XSolidBrush(XColors.LightSteelBlue, true);
        return XBrushes.lightSteelBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LightYellow
    {
      get
      {
        if (XBrushes.lightYellow == null)
          XBrushes.lightYellow = new XSolidBrush(XColors.LightYellow, true);
        return XBrushes.lightYellow;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Lime
    {
      get
      {
        if (XBrushes.lime == null)
          XBrushes.lime = new XSolidBrush(XColors.Lime, true);
        return XBrushes.lime;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush LimeGreen
    {
      get
      {
        if (XBrushes.limeGreen == null)
          XBrushes.limeGreen = new XSolidBrush(XColors.LimeGreen, true);
        return XBrushes.limeGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Linen
    {
      get
      {
        if (XBrushes.linen == null)
          XBrushes.linen = new XSolidBrush(XColors.Linen, true);
        return XBrushes.linen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Magenta
    {
      get
      {
        if (XBrushes.magenta == null)
          XBrushes.magenta = new XSolidBrush(XColors.Magenta, true);
        return XBrushes.magenta;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Maroon
    {
      get
      {
        if (XBrushes.maroon == null)
          XBrushes.maroon = new XSolidBrush(XColors.Maroon, true);
        return XBrushes.maroon;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush MediumAquamarine
    {
      get
      {
        if (XBrushes.mediumAquamarine == null)
          XBrushes.mediumAquamarine = new XSolidBrush(XColors.MediumAquamarine, true);
        return XBrushes.mediumAquamarine;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush MediumBlue
    {
      get
      {
        if (XBrushes.mediumBlue == null)
          XBrushes.mediumBlue = new XSolidBrush(XColors.MediumBlue, true);
        return XBrushes.mediumBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush MediumOrchid
    {
      get
      {
        if (XBrushes.mediumOrchid == null)
          XBrushes.mediumOrchid = new XSolidBrush(XColors.MediumOrchid, true);
        return XBrushes.mediumOrchid;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush MediumPurple
    {
      get
      {
        if (XBrushes.mediumPurple == null)
          XBrushes.mediumPurple = new XSolidBrush(XColors.MediumPurple, true);
        return XBrushes.mediumPurple;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush MediumSeaGreen
    {
      get
      {
        if (XBrushes.mediumSeaGreen == null)
          XBrushes.mediumSeaGreen = new XSolidBrush(XColors.MediumSeaGreen, true);
        return XBrushes.mediumSeaGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush MediumSlateBlue
    {
      get
      {
        if (XBrushes.mediumSlateBlue == null)
          XBrushes.mediumSlateBlue = new XSolidBrush(XColors.MediumSlateBlue, true);
        return XBrushes.mediumSlateBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush MediumSpringGreen
    {
      get
      {
        if (XBrushes.mediumSpringGreen == null)
          XBrushes.mediumSpringGreen = new XSolidBrush(XColors.MediumSpringGreen, true);
        return XBrushes.mediumSpringGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush MediumTurquoise
    {
      get
      {
        if (XBrushes.mediumTurquoise == null)
          XBrushes.mediumTurquoise = new XSolidBrush(XColors.MediumTurquoise, true);
        return XBrushes.mediumTurquoise;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush MediumVioletRed
    {
      get
      {
        if (XBrushes.mediumVioletRed == null)
          XBrushes.mediumVioletRed = new XSolidBrush(XColors.MediumVioletRed, true);
        return XBrushes.mediumVioletRed;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush MidnightBlue
    {
      get
      {
        if (XBrushes.midnightBlue == null)
          XBrushes.midnightBlue = new XSolidBrush(XColors.MidnightBlue, true);
        return XBrushes.midnightBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush MintCream
    {
      get
      {
        if (XBrushes.mintCream == null)
          XBrushes.mintCream = new XSolidBrush(XColors.MintCream, true);
        return XBrushes.mintCream;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush MistyRose
    {
      get
      {
        if (XBrushes.mistyRose == null)
          XBrushes.mistyRose = new XSolidBrush(XColors.MistyRose, true);
        return XBrushes.mistyRose;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Moccasin
    {
      get
      {
        if (XBrushes.moccasin == null)
          XBrushes.moccasin = new XSolidBrush(XColors.Moccasin, true);
        return XBrushes.moccasin;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush NavajoWhite
    {
      get
      {
        if (XBrushes.navajoWhite == null)
          XBrushes.navajoWhite = new XSolidBrush(XColors.NavajoWhite, true);
        return XBrushes.navajoWhite;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Navy
    {
      get
      {
        if (XBrushes.navy == null)
          XBrushes.navy = new XSolidBrush(XColors.Navy, true);
        return XBrushes.navy;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush OldLace
    {
      get
      {
        if (XBrushes.oldLace == null)
          XBrushes.oldLace = new XSolidBrush(XColors.OldLace, true);
        return XBrushes.oldLace;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Olive
    {
      get
      {
        if (XBrushes.olive == null)
          XBrushes.olive = new XSolidBrush(XColors.Olive, true);
        return XBrushes.olive;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush OliveDrab
    {
      get
      {
        if (XBrushes.oliveDrab == null)
          XBrushes.oliveDrab = new XSolidBrush(XColors.OliveDrab, true);
        return XBrushes.oliveDrab;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Orange
    {
      get
      {
        if (XBrushes.orange == null)
          XBrushes.orange = new XSolidBrush(XColors.Orange, true);
        return XBrushes.orange;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush OrangeRed
    {
      get
      {
        if (XBrushes.orangeRed == null)
          XBrushes.orangeRed = new XSolidBrush(XColors.OrangeRed, true);
        return XBrushes.orangeRed;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Orchid
    {
      get
      {
        if (XBrushes.orchid == null)
          XBrushes.orchid = new XSolidBrush(XColors.Orchid, true);
        return XBrushes.orchid;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush PaleGoldenrod
    {
      get
      {
        if (XBrushes.paleGoldenrod == null)
          XBrushes.paleGoldenrod = new XSolidBrush(XColors.PaleGoldenrod, true);
        return XBrushes.paleGoldenrod;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush PaleGreen
    {
      get
      {
        if (XBrushes.paleGreen == null)
          XBrushes.paleGreen = new XSolidBrush(XColors.PaleGreen, true);
        return XBrushes.paleGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush PaleTurquoise
    {
      get
      {
        if (XBrushes.paleTurquoise == null)
          XBrushes.paleTurquoise = new XSolidBrush(XColors.PaleTurquoise, true);
        return XBrushes.paleTurquoise;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush PaleVioletRed
    {
      get
      {
        if (XBrushes.paleVioletRed == null)
          XBrushes.paleVioletRed = new XSolidBrush(XColors.PaleVioletRed, true);
        return XBrushes.paleVioletRed;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush PapayaWhip
    {
      get
      {
        if (XBrushes.papayaWhip == null)
          XBrushes.papayaWhip = new XSolidBrush(XColors.PapayaWhip, true);
        return XBrushes.papayaWhip;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush PeachPuff
    {
      get
      {
        if (XBrushes.peachPuff == null)
          XBrushes.peachPuff = new XSolidBrush(XColors.PeachPuff, true);
        return XBrushes.peachPuff;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Peru
    {
      get
      {
        if (XBrushes.peru == null)
          XBrushes.peru = new XSolidBrush(XColors.Peru, true);
        return XBrushes.peru;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Pink
    {
      get
      {
        if (XBrushes.pink == null)
          XBrushes.pink = new XSolidBrush(XColors.Pink, true);
        return XBrushes.pink;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Plum
    {
      get
      {
        if (XBrushes.plum == null)
          XBrushes.plum = new XSolidBrush(XColors.Plum, true);
        return XBrushes.plum;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush PowderBlue
    {
      get
      {
        if (XBrushes.powderBlue == null)
          XBrushes.powderBlue = new XSolidBrush(XColors.PowderBlue, true);
        return XBrushes.powderBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Purple
    {
      get
      {
        if (XBrushes.purple == null)
          XBrushes.purple = new XSolidBrush(XColors.Purple, true);
        return XBrushes.purple;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Red
    {
      get
      {
        if (XBrushes.red == null)
          XBrushes.red = new XSolidBrush(XColors.Red, true);
        return XBrushes.red;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush RosyBrown
    {
      get
      {
        if (XBrushes.rosyBrown == null)
          XBrushes.rosyBrown = new XSolidBrush(XColors.RosyBrown, true);
        return XBrushes.rosyBrown;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush RoyalBlue
    {
      get
      {
        if (XBrushes.royalBlue == null)
          XBrushes.royalBlue = new XSolidBrush(XColors.RoyalBlue, true);
        return XBrushes.royalBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush SaddleBrown
    {
      get
      {
        if (XBrushes.saddleBrown == null)
          XBrushes.saddleBrown = new XSolidBrush(XColors.SaddleBrown, true);
        return XBrushes.saddleBrown;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Salmon
    {
      get
      {
        if (XBrushes.salmon == null)
          XBrushes.salmon = new XSolidBrush(XColors.Salmon, true);
        return XBrushes.salmon;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush SandyBrown
    {
      get
      {
        if (XBrushes.sandyBrown == null)
          XBrushes.sandyBrown = new XSolidBrush(XColors.SandyBrown, true);
        return XBrushes.sandyBrown;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush SeaGreen
    {
      get
      {
        if (XBrushes.seaGreen == null)
          XBrushes.seaGreen = new XSolidBrush(XColors.SeaGreen, true);
        return XBrushes.seaGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush SeaShell
    {
      get
      {
        if (XBrushes.seaShell == null)
          XBrushes.seaShell = new XSolidBrush(XColors.SeaShell, true);
        return XBrushes.seaShell;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Sienna
    {
      get
      {
        if (XBrushes.sienna == null)
          XBrushes.sienna = new XSolidBrush(XColors.Sienna, true);
        return XBrushes.sienna;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Silver
    {
      get
      {
        if (XBrushes.silver == null)
          XBrushes.silver = new XSolidBrush(XColors.Silver, true);
        return XBrushes.silver;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush SkyBlue
    {
      get
      {
        if (XBrushes.skyBlue == null)
          XBrushes.skyBlue = new XSolidBrush(XColors.SkyBlue, true);
        return XBrushes.skyBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush SlateBlue
    {
      get
      {
        if (XBrushes.slateBlue == null)
          XBrushes.slateBlue = new XSolidBrush(XColors.SlateBlue, true);
        return XBrushes.slateBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush SlateGray
    {
      get
      {
        if (XBrushes.slateGray == null)
          XBrushes.slateGray = new XSolidBrush(XColors.SlateGray, true);
        return XBrushes.slateGray;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Snow
    {
      get
      {
        if (XBrushes.snow == null)
          XBrushes.snow = new XSolidBrush(XColors.Snow, true);
        return XBrushes.snow;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush SpringGreen
    {
      get
      {
        if (XBrushes.springGreen == null)
          XBrushes.springGreen = new XSolidBrush(XColors.SpringGreen, true);
        return XBrushes.springGreen;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush SteelBlue
    {
      get
      {
        if (XBrushes.steelBlue == null)
          XBrushes.steelBlue = new XSolidBrush(XColors.SteelBlue, true);
        return XBrushes.steelBlue;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Tan
    {
      get
      {
        if (XBrushes.tan == null)
          XBrushes.tan = new XSolidBrush(XColors.Tan, true);
        return XBrushes.tan;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Teal
    {
      get
      {
        if (XBrushes.teal == null)
          XBrushes.teal = new XSolidBrush(XColors.Teal, true);
        return XBrushes.teal;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Thistle
    {
      get
      {
        if (XBrushes.thistle == null)
          XBrushes.thistle = new XSolidBrush(XColors.Thistle, true);
        return XBrushes.thistle;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Tomato
    {
      get
      {
        if (XBrushes.tomato == null)
          XBrushes.tomato = new XSolidBrush(XColors.Tomato, true);
        return XBrushes.tomato;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Transparent
    {
      get
      {
        if (XBrushes.transparent == null)
          XBrushes.transparent = new XSolidBrush(XColors.Transparent, true);
        return XBrushes.transparent;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Turquoise
    {
      get
      {
        if (XBrushes.turquoise == null)
          XBrushes.turquoise = new XSolidBrush(XColors.Turquoise, true);
        return XBrushes.turquoise;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Violet
    {
      get
      {
        if (XBrushes.violet == null)
          XBrushes.violet = new XSolidBrush(XColors.Violet, true);
        return XBrushes.violet;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Wheat
    {
      get
      {
        if (XBrushes.wheat == null)
          XBrushes.wheat = new XSolidBrush(XColors.Wheat, true);
        return XBrushes.wheat;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush White
    {
      get
      {
        if (XBrushes.white == null)
          XBrushes.white = new XSolidBrush(XColors.White, true);
        return XBrushes.white;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush WhiteSmoke
    {
      get
      {
        if (XBrushes.whiteSmoke == null)
          XBrushes.whiteSmoke = new XSolidBrush(XColors.WhiteSmoke, true);
        return XBrushes.whiteSmoke;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush Yellow
    {
      get
      {
        if (XBrushes.yellow == null)
          XBrushes.yellow = new XSolidBrush(XColors.Yellow, true);
        return XBrushes.yellow;
      }
    }

    /// <summary>Gets a pre-defined XBrush object.</summary>
    public static XSolidBrush YellowGreen
    {
      get
      {
        if (XBrushes.yellowGreen == null)
          XBrushes.yellowGreen = new XSolidBrush(XColors.YellowGreen, true);
        return XBrushes.yellowGreen;
      }
    }

    static XSolidBrush aliceBlue;
    static XSolidBrush antiqueWhite;
    static XSolidBrush aqua;
    static XSolidBrush aquamarine;
    static XSolidBrush azure;
    static XSolidBrush beige;
    static XSolidBrush bisque;
    static XSolidBrush black;
    static XSolidBrush blanchedAlmond;
    static XSolidBrush blue;
    static XSolidBrush blueViolet;
    static XSolidBrush brown;
    static XSolidBrush burlyWood;
    static XSolidBrush cadetBlue;
    static XSolidBrush chartreuse;
    static XSolidBrush chocolate;
    static XSolidBrush coral;
    static XSolidBrush cornflowerBlue;
    static XSolidBrush cornsilk;
    static XSolidBrush crimson;
    static XSolidBrush cyan;
    static XSolidBrush darkBlue;
    static XSolidBrush darkCyan;
    static XSolidBrush darkGoldenrod;
    static XSolidBrush darkGray;
    static XSolidBrush darkGreen;
    static XSolidBrush darkKhaki;
    static XSolidBrush darkMagenta;
    static XSolidBrush darkOliveGreen;
    static XSolidBrush darkOrange;
    static XSolidBrush darkOrchid;
    static XSolidBrush darkRed;
    static XSolidBrush darkSalmon;
    static XSolidBrush darkSeaGreen;
    static XSolidBrush darkSlateBlue;
    static XSolidBrush darkSlateGray;
    static XSolidBrush darkTurquoise;
    static XSolidBrush darkViolet;
    static XSolidBrush deepPink;
    static XSolidBrush deepSkyBlue;
    static XSolidBrush dimGray;
    static XSolidBrush dodgerBlue;
    static XSolidBrush firebrick;
    static XSolidBrush floralWhite;
    static XSolidBrush forestGreen;
    static XSolidBrush fuchsia;
    static XSolidBrush gainsboro;
    static XSolidBrush ghostWhite;
    static XSolidBrush gold;
    static XSolidBrush goldenrod;
    static XSolidBrush gray;
    static XSolidBrush green;
    static XSolidBrush greenYellow;
    static XSolidBrush honeydew;
    static XSolidBrush hotPink;
    static XSolidBrush indianRed;
    static XSolidBrush indigo;
    static XSolidBrush ivory;
    static XSolidBrush khaki;
    static XSolidBrush lavender;
    static XSolidBrush lavenderBlush;
    static XSolidBrush lawnGreen;
    static XSolidBrush lemonChiffon;
    static XSolidBrush lightBlue;
    static XSolidBrush lightCoral;
    static XSolidBrush lightCyan;
    static XSolidBrush lightGoldenrodYellow;
    static XSolidBrush lightGray;
    static XSolidBrush lightGreen;
    static XSolidBrush lightPink;
    static XSolidBrush lightSalmon;
    static XSolidBrush lightSeaGreen;
    static XSolidBrush lightSkyBlue;
    static XSolidBrush lightSlateGray;
    static XSolidBrush lightSteelBlue;
    static XSolidBrush lightYellow;
    static XSolidBrush lime;
    static XSolidBrush limeGreen;
    static XSolidBrush linen;
    static XSolidBrush magenta;
    static XSolidBrush maroon;
    static XSolidBrush mediumAquamarine;
    static XSolidBrush mediumBlue;
    static XSolidBrush mediumOrchid;
    static XSolidBrush mediumPurple;
    static XSolidBrush mediumSeaGreen;
    static XSolidBrush mediumSlateBlue;
    static XSolidBrush mediumSpringGreen;
    static XSolidBrush mediumTurquoise;
    static XSolidBrush mediumVioletRed;
    static XSolidBrush midnightBlue;
    static XSolidBrush mintCream;
    static XSolidBrush mistyRose;
    static XSolidBrush moccasin;
    static XSolidBrush navajoWhite;
    static XSolidBrush navy;
    static XSolidBrush oldLace;
    static XSolidBrush olive;
    static XSolidBrush oliveDrab;
    static XSolidBrush orange;
    static XSolidBrush orangeRed;
    static XSolidBrush orchid;
    static XSolidBrush paleGoldenrod;
    static XSolidBrush paleGreen;
    static XSolidBrush paleTurquoise;
    static XSolidBrush paleVioletRed;
    static XSolidBrush papayaWhip;
    static XSolidBrush peachPuff;
    static XSolidBrush peru;
    static XSolidBrush pink;
    static XSolidBrush plum;
    static XSolidBrush powderBlue;
    static XSolidBrush purple;
    static XSolidBrush red;
    static XSolidBrush rosyBrown;
    static XSolidBrush royalBlue;
    static XSolidBrush saddleBrown;
    static XSolidBrush salmon;
    static XSolidBrush sandyBrown;
    static XSolidBrush seaGreen;
    static XSolidBrush seaShell;
    static XSolidBrush sienna;
    static XSolidBrush silver;
    static XSolidBrush skyBlue;
    static XSolidBrush slateBlue;
    static XSolidBrush slateGray;
    static XSolidBrush snow;
    static XSolidBrush springGreen;
    static XSolidBrush steelBlue;
    static XSolidBrush tan;
    static XSolidBrush teal;
    static XSolidBrush thistle;
    static XSolidBrush tomato;
    static XSolidBrush transparent;
    static XSolidBrush turquoise;
    static XSolidBrush violet;
    static XSolidBrush wheat;
    static XSolidBrush white;
    static XSolidBrush whiteSmoke;
    static XSolidBrush yellow;
    static XSolidBrush yellowGreen;
  }
}
