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
  /// Pens for all the pre-defined colors.
  /// </summary>
  public static class XPens
  {
    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen AliceBlue
    {
      get
      {
        if (XPens.aliceBlue == null)
          XPens.aliceBlue = new XPen(XColors.AliceBlue, 1, true);
        return XPens.aliceBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen AntiqueWhite
    {
      get
      {
        if (XPens.antiqueWhite == null)
          XPens.antiqueWhite = new XPen(XColors.AntiqueWhite, 1, true);
        return XPens.antiqueWhite;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Aqua
    {
      get
      {
        if (XPens.aqua == null)
          XPens.aqua = new XPen(XColors.Aqua, 1, true);
        return XPens.aqua;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Aquamarine
    {
      get
      {
        if (XPens.aquamarine == null)
          XPens.aquamarine = new XPen(XColors.Aquamarine, 1, true);
        return XPens.aquamarine;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Azure
    {
      get
      {
        if (XPens.azure == null)
          XPens.azure = new XPen(XColors.Azure, 1, true);
        return XPens.azure;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Beige
    {
      get
      {
        if (XPens.beige == null)
          XPens.beige = new XPen(XColors.Beige, 1, true);
        return XPens.beige;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Bisque
    {
      get
      {
        if (XPens.bisque == null)
          XPens.bisque = new XPen(XColors.Bisque, 1, true);
        return XPens.bisque;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Black
    {
      get
      {
        if (XPens.black == null)
          XPens.black = new XPen(XColors.Black, 1, true);
        return XPens.black;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen BlanchedAlmond
    {
      get
      {
        if (XPens.blanchedAlmond == null)
          XPens.blanchedAlmond = new XPen(XColors.BlanchedAlmond, 1, true);
        return XPens.blanchedAlmond;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Blue
    {
      get
      {
        if (XPens.blue == null)
          XPens.blue = new XPen(XColors.Blue, 1, true);
        return XPens.blue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen BlueViolet
    {
      get
      {
        if (XPens.blueViolet == null)
          XPens.blueViolet = new XPen(XColors.BlueViolet, 1, true);
        return XPens.blueViolet;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Brown
    {
      get
      {
        if (XPens.brown == null)
          XPens.brown = new XPen(XColors.Brown, 1, true);
        return XPens.brown;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen BurlyWood
    {
      get
      {
        if (XPens.burlyWood == null)
          XPens.burlyWood = new XPen(XColors.BurlyWood, 1, true);
        return XPens.burlyWood;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen CadetBlue
    {
      get
      {
        if (XPens.cadetBlue == null)
          XPens.cadetBlue = new XPen(XColors.CadetBlue, 1, true);
        return XPens.cadetBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Chartreuse
    {
      get
      {
        if (XPens.chartreuse == null)
          XPens.chartreuse = new XPen(XColors.Chartreuse, 1, true);
        return XPens.chartreuse;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Chocolate
    {
      get
      {
        if (XPens.chocolate == null)
          XPens.chocolate = new XPen(XColors.Chocolate, 1, true);
        return XPens.chocolate;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Coral
    {
      get
      {
        if (XPens.coral == null)
          XPens.coral = new XPen(XColors.Coral, 1, true);
        return XPens.coral;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen CornflowerBlue
    {
      get
      {
        if (XPens.cornflowerBlue == null)
          XPens.cornflowerBlue = new XPen(XColors.CornflowerBlue, 1, true);
        return XPens.cornflowerBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Cornsilk
    {
      get
      {
        if (XPens.cornsilk == null)
          XPens.cornsilk = new XPen(XColors.Cornsilk, 1, true);
        return XPens.cornsilk;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Crimson
    {
      get
      {
        if (XPens.crimson == null)
          XPens.crimson = new XPen(XColors.Crimson, 1, true);
        return XPens.crimson;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Cyan
    {
      get
      {
        if (XPens.cyan == null)
          XPens.cyan = new XPen(XColors.Cyan, 1, true);
        return XPens.cyan;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkBlue
    {
      get
      {
        if (XPens.darkBlue == null)
          XPens.darkBlue = new XPen(XColors.DarkBlue, 1, true);
        return XPens.darkBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkCyan
    {
      get
      {
        if (XPens.darkCyan == null)
          XPens.darkCyan = new XPen(XColors.DarkCyan, 1, true);
        return XPens.darkCyan;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkGoldenrod
    {
      get
      {
        if (XPens.darkGoldenrod == null)
          XPens.darkGoldenrod = new XPen(XColors.DarkGoldenrod, 1, true);
        return XPens.darkGoldenrod;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkGray
    {
      get
      {
        if (XPens.darkGray == null)
          XPens.darkGray = new XPen(XColors.DarkGray, 1, true);
        return XPens.darkGray;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkGreen
    {
      get
      {
        if (XPens.darkGreen == null)
          XPens.darkGreen = new XPen(XColors.DarkGreen, 1, true);
        return XPens.darkGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkKhaki
    {
      get
      {
        if (XPens.darkKhaki == null)
          XPens.darkKhaki = new XPen(XColors.DarkKhaki, 1, true);
        return XPens.darkKhaki;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkMagenta
    {
      get
      {
        if (XPens.darkMagenta == null)
          XPens.darkMagenta = new XPen(XColors.DarkMagenta, 1, true);
        return XPens.darkMagenta;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkOliveGreen
    {
      get
      {
        if (XPens.darkOliveGreen == null)
          XPens.darkOliveGreen = new XPen(XColors.DarkOliveGreen, 1, true);
        return XPens.darkOliveGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkOrange
    {
      get
      {
        if (XPens.darkOrange == null)
          XPens.darkOrange = new XPen(XColors.DarkOrange, 1, true);
        return XPens.darkOrange;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkOrchid
    {
      get
      {
        if (XPens.darkOrchid == null)
          XPens.darkOrchid = new XPen(XColors.DarkOrchid, 1, true);
        return XPens.darkOrchid;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkRed
    {
      get
      {
        if (XPens.darkRed == null)
          XPens.darkRed = new XPen(XColors.DarkRed, 1, true);
        return XPens.darkRed;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkSalmon
    {
      get
      {
        if (XPens.darkSalmon == null)
          XPens.darkSalmon = new XPen(XColors.DarkSalmon, 1, true);
        return XPens.darkSalmon;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkSeaGreen
    {
      get
      {
        if (XPens.darkSeaGreen == null)
          XPens.darkSeaGreen = new XPen(XColors.DarkSeaGreen, 1, true);
        return XPens.darkSeaGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkSlateBlue
    {
      get
      {
        if (XPens.darkSlateBlue == null)
          XPens.darkSlateBlue = new XPen(XColors.DarkSlateBlue, 1, true);
        return XPens.darkSlateBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkSlateGray
    {
      get
      {
        if (XPens.darkSlateGray == null)
          XPens.darkSlateGray = new XPen(XColors.DarkSlateGray, 1, true);
        return XPens.darkSlateGray;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkTurquoise
    {
      get
      {
        if (XPens.darkTurquoise == null)
          XPens.darkTurquoise = new XPen(XColors.DarkTurquoise, 1, true);
        return XPens.darkTurquoise;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DarkViolet
    {
      get
      {
        if (XPens.darkViolet == null)
          XPens.darkViolet = new XPen(XColors.DarkViolet, 1, true);
        return XPens.darkViolet;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DeepPink
    {
      get
      {
        if (XPens.deepPink == null)
          XPens.deepPink = new XPen(XColors.DeepPink, 1, true);
        return XPens.deepPink;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DeepSkyBlue
    {
      get
      {
        if (XPens.deepSkyBlue == null)
          XPens.deepSkyBlue = new XPen(XColors.DeepSkyBlue, 1, true);
        return XPens.deepSkyBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DimGray
    {
      get
      {
        if (XPens.dimGray == null)
          XPens.dimGray = new XPen(XColors.DimGray, 1, true);
        return XPens.dimGray;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen DodgerBlue
    {
      get
      {
        if (XPens.dodgerBlue == null)
          XPens.dodgerBlue = new XPen(XColors.DodgerBlue, 1, true);
        return XPens.dodgerBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Firebrick
    {
      get
      {
        if (XPens.firebrick == null)
          XPens.firebrick = new XPen(XColors.Firebrick, 1, true);
        return XPens.firebrick;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen FloralWhite
    {
      get
      {
        if (XPens.floralWhite == null)
          XPens.floralWhite = new XPen(XColors.FloralWhite, 1, true);
        return XPens.floralWhite;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen ForestGreen
    {
      get
      {
        if (XPens.forestGreen == null)
          XPens.forestGreen = new XPen(XColors.ForestGreen, 1, true);
        return XPens.forestGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Fuchsia
    {
      get
      {
        if (XPens.fuchsia == null)
          XPens.fuchsia = new XPen(XColors.Fuchsia, 1, true);
        return XPens.fuchsia;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Gainsboro
    {
      get
      {
        if (XPens.gainsboro == null)
          XPens.gainsboro = new XPen(XColors.Gainsboro, 1, true);
        return XPens.gainsboro;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen GhostWhite
    {
      get
      {
        if (XPens.ghostWhite == null)
          XPens.ghostWhite = new XPen(XColors.GhostWhite, 1, true);
        return XPens.ghostWhite;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Gold
    {
      get
      {
        if (XPens.gold == null)
          XPens.gold = new XPen(XColors.Gold, 1, true);
        return XPens.gold;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Goldenrod
    {
      get
      {
        if (XPens.goldenrod == null)
          XPens.goldenrod = new XPen(XColors.Goldenrod, 1, true);
        return XPens.goldenrod;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Gray
    {
      get
      {
        if (XPens.gray == null)
          XPens.gray = new XPen(XColors.Gray, 1, true);
        return XPens.gray;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Green
    {
      get
      {
        if (XPens.green == null)
          XPens.green = new XPen(XColors.Green, 1, true);
        return XPens.green;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen GreenYellow
    {
      get
      {
        if (XPens.greenYellow == null)
          XPens.greenYellow = new XPen(XColors.GreenYellow, 1, true);
        return XPens.greenYellow;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Honeydew
    {
      get
      {
        if (XPens.honeydew == null)
          XPens.honeydew = new XPen(XColors.Honeydew, 1, true);
        return XPens.honeydew;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen HotPink
    {
      get
      {
        if (XPens.hotPink == null)
          XPens.hotPink = new XPen(XColors.HotPink, 1, true);
        return XPens.hotPink;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen IndianRed
    {
      get
      {
        if (XPens.indianRed == null)
          XPens.indianRed = new XPen(XColors.IndianRed, 1, true);
        return XPens.indianRed;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Indigo
    {
      get
      {
        if (XPens.indigo == null)
          XPens.indigo = new XPen(XColors.Indigo, 1, true);
        return XPens.indigo;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Ivory
    {
      get
      {
        if (XPens.ivory == null)
          XPens.ivory = new XPen(XColors.Ivory, 1, true);
        return XPens.ivory;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Khaki
    {
      get
      {
        if (XPens.khaki == null)
          XPens.khaki = new XPen(XColors.Khaki, 1, true);
        return XPens.khaki;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Lavender
    {
      get
      {
        if (XPens.lavender == null)
          XPens.lavender = new XPen(XColors.Lavender, 1, true);
        return XPens.lavender;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LavenderBlush
    {
      get
      {
        if (XPens.lavenderBlush == null)
          XPens.lavenderBlush = new XPen(XColors.LavenderBlush, 1, true);
        return XPens.lavenderBlush;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LawnGreen
    {
      get
      {
        if (XPens.lawnGreen == null)
          XPens.lawnGreen = new XPen(XColors.LawnGreen, 1, true);
        return XPens.lawnGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LemonChiffon
    {
      get
      {
        if (XPens.lemonChiffon == null)
          XPens.lemonChiffon = new XPen(XColors.LemonChiffon, 1, true);
        return XPens.lemonChiffon;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightBlue
    {
      get
      {
        if (XPens.lightBlue == null)
          XPens.lightBlue = new XPen(XColors.LightBlue, 1, true);
        return XPens.lightBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightCoral
    {
      get
      {
        if (XPens.lightCoral == null)
          XPens.lightCoral = new XPen(XColors.LightCoral, 1, true);
        return XPens.lightCoral;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightCyan
    {
      get
      {
        if (XPens.lightCyan == null)
          XPens.lightCyan = new XPen(XColors.LightCyan, 1, true);
        return XPens.lightCyan;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightGoldenrodYellow
    {
      get
      {
        if (XPens.lightGoldenrodYellow == null)
          XPens.lightGoldenrodYellow = new XPen(XColors.LightGoldenrodYellow, 1, true);
        return XPens.lightGoldenrodYellow;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightGray
    {
      get
      {
        if (XPens.lightGray == null)
          XPens.lightGray = new XPen(XColors.LightGray, 1, true);
        return XPens.lightGray;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightGreen
    {
      get
      {
        if (XPens.lightGreen == null)
          XPens.lightGreen = new XPen(XColors.LightGreen, 1, true);
        return XPens.lightGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightPink
    {
      get
      {
        if (XPens.lightPink == null)
          XPens.lightPink = new XPen(XColors.LightPink, 1, true);
        return XPens.lightPink;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightSalmon
    {
      get
      {
        if (XPens.lightSalmon == null)
          XPens.lightSalmon = new XPen(XColors.LightSalmon, 1, true);
        return XPens.lightSalmon;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightSeaGreen
    {
      get
      {
        if (XPens.lightSeaGreen == null)
          XPens.lightSeaGreen = new XPen(XColors.LightSeaGreen, 1, true);
        return XPens.lightSeaGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightSkyBlue
    {
      get
      {
        if (XPens.lightSkyBlue == null)
          XPens.lightSkyBlue = new XPen(XColors.LightSkyBlue, 1, true);
        return XPens.lightSkyBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightSlateGray
    {
      get
      {
        if (XPens.lightSlateGray == null)
          XPens.lightSlateGray = new XPen(XColors.LightSlateGray, 1, true);
        return XPens.lightSlateGray;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightSteelBlue
    {
      get
      {
        if (XPens.lightSteelBlue == null)
          XPens.lightSteelBlue = new XPen(XColors.LightSteelBlue, 1, true);
        return XPens.lightSteelBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LightYellow
    {
      get
      {
        if (XPens.lightYellow == null)
          XPens.lightYellow = new XPen(XColors.LightYellow, 1, true);
        return XPens.lightYellow;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Lime
    {
      get
      {
        if (XPens.lime == null)
          XPens.lime = new XPen(XColors.Lime, 1, true);
        return XPens.lime;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen LimeGreen
    {
      get
      {
        if (XPens.limeGreen == null)
          XPens.limeGreen = new XPen(XColors.LimeGreen, 1, true);
        return XPens.limeGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Linen
    {
      get
      {
        if (XPens.linen == null)
          XPens.linen = new XPen(XColors.Linen, 1, true);
        return XPens.linen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Magenta
    {
      get
      {
        if (XPens.magenta == null)
          XPens.magenta = new XPen(XColors.Magenta, 1, true);
        return XPens.magenta;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Maroon
    {
      get
      {
        if (XPens.maroon == null)
          XPens.maroon = new XPen(XColors.Maroon, 1, true);
        return XPens.maroon;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen MediumAquamarine
    {
      get
      {
        if (XPens.mediumAquamarine == null)
          XPens.mediumAquamarine = new XPen(XColors.MediumAquamarine, 1, true);
        return XPens.mediumAquamarine;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen MediumBlue
    {
      get
      {
        if (XPens.mediumBlue == null)
          XPens.mediumBlue = new XPen(XColors.MediumBlue, 1, true);
        return XPens.mediumBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen MediumOrchid
    {
      get
      {
        if (XPens.mediumOrchid == null)
          XPens.mediumOrchid = new XPen(XColors.MediumOrchid, 1, true);
        return XPens.mediumOrchid;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen MediumPurple
    {
      get
      {
        if (XPens.mediumPurple == null)
          XPens.mediumPurple = new XPen(XColors.MediumPurple, 1, true);
        return XPens.mediumPurple;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen MediumSeaGreen
    {
      get
      {
        if (XPens.mediumSeaGreen == null)
          XPens.mediumSeaGreen = new XPen(XColors.MediumSeaGreen, 1, true);
        return XPens.mediumSeaGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen MediumSlateBlue
    {
      get
      {
        if (XPens.mediumSlateBlue == null)
          XPens.mediumSlateBlue = new XPen(XColors.MediumSlateBlue, 1, true);
        return XPens.mediumSlateBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen MediumSpringGreen
    {
      get
      {
        if (XPens.mediumSpringGreen == null)
          XPens.mediumSpringGreen = new XPen(XColors.MediumSpringGreen, 1, true);
        return XPens.mediumSpringGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen MediumTurquoise
    {
      get
      {
        if (XPens.mediumTurquoise == null)
          XPens.mediumTurquoise = new XPen(XColors.MediumTurquoise, 1, true);
        return XPens.mediumTurquoise;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen MediumVioletRed
    {
      get
      {
        if (XPens.mediumVioletRed == null)
          XPens.mediumVioletRed = new XPen(XColors.MediumVioletRed, 1, true);
        return XPens.mediumVioletRed;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen MidnightBlue
    {
      get
      {
        if (XPens.midnightBlue == null)
          XPens.midnightBlue = new XPen(XColors.MidnightBlue, 1, true);
        return XPens.midnightBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen MintCream
    {
      get
      {
        if (XPens.mintCream == null)
          XPens.mintCream = new XPen(XColors.MintCream, 1, true);
        return XPens.mintCream;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen MistyRose
    {
      get
      {
        if (XPens.mistyRose == null)
          XPens.mistyRose = new XPen(XColors.MistyRose, 1, true);
        return XPens.mistyRose;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Moccasin
    {
      get
      {
        if (XPens.moccasin == null)
          XPens.moccasin = new XPen(XColors.Moccasin, 1, true);
        return XPens.moccasin;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen NavajoWhite
    {
      get
      {
        if (XPens.navajoWhite == null)
          XPens.navajoWhite = new XPen(XColors.NavajoWhite, 1, true);
        return XPens.navajoWhite;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Navy
    {
      get
      {
        if (XPens.navy == null)
          XPens.navy = new XPen(XColors.Navy, 1, true);
        return XPens.navy;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen OldLace
    {
      get
      {
        if (XPens.oldLace == null)
          XPens.oldLace = new XPen(XColors.OldLace, 1, true);
        return XPens.oldLace;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Olive
    {
      get
      {
        if (XPens.olive == null)
          XPens.olive = new XPen(XColors.Olive, 1, true);
        return XPens.olive;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen OliveDrab
    {
      get
      {
        if (XPens.oliveDrab == null)
          XPens.oliveDrab = new XPen(XColors.OliveDrab, 1, true);
        return XPens.oliveDrab;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Orange
    {
      get
      {
        if (XPens.orange == null)
          XPens.orange = new XPen(XColors.Orange, 1, true);
        return XPens.orange;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen OrangeRed
    {
      get
      {
        if (XPens.orangeRed == null)
          XPens.orangeRed = new XPen(XColors.OrangeRed, 1, true);
        return XPens.orangeRed;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Orchid
    {
      get
      {
        if (XPens.orchid == null)
          XPens.orchid = new XPen(XColors.Orchid, 1, true);
        return XPens.orchid;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen PaleGoldenrod
    {
      get
      {
        if (XPens.paleGoldenrod == null)
          XPens.paleGoldenrod = new XPen(XColors.PaleGoldenrod, 1, true);
        return XPens.paleGoldenrod;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen PaleGreen
    {
      get
      {
        if (XPens.paleGreen == null)
          XPens.paleGreen = new XPen(XColors.PaleGreen, 1, true);
        return XPens.paleGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen PaleTurquoise
    {
      get
      {
        if (XPens.paleTurquoise == null)
          XPens.paleTurquoise = new XPen(XColors.PaleTurquoise, 1, true);
        return XPens.paleTurquoise;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen PaleVioletRed
    {
      get
      {
        if (XPens.paleVioletRed == null)
          XPens.paleVioletRed = new XPen(XColors.PaleVioletRed, 1, true);
        return XPens.paleVioletRed;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen PapayaWhip
    {
      get
      {
        if (XPens.papayaWhip == null)
          XPens.papayaWhip = new XPen(XColors.PapayaWhip, 1, true);
        return XPens.papayaWhip;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen PeachPuff
    {
      get
      {
        if (XPens.peachPuff == null)
          XPens.peachPuff = new XPen(XColors.PeachPuff, 1, true);
        return XPens.peachPuff;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Peru
    {
      get
      {
        if (XPens.peru == null)
          XPens.peru = new XPen(XColors.Peru, 1, true);
        return XPens.peru;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Pink
    {
      get
      {
        if (XPens.pink == null)
          XPens.pink = new XPen(XColors.Pink, 1, true);
        return XPens.pink;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Plum
    {
      get
      {
        if (XPens.plum == null)
          XPens.plum = new XPen(XColors.Plum, 1, true);
        return XPens.plum;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen PowderBlue
    {
      get
      {
        if (XPens.powderBlue == null)
          XPens.powderBlue = new XPen(XColors.PowderBlue, 1, true);
        return XPens.powderBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Purple
    {
      get
      {
        if (XPens.purple == null)
          XPens.purple = new XPen(XColors.Purple, 1, true);
        return XPens.purple;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Red
    {
      get
      {
        if (XPens.red == null)
          XPens.red = new XPen(XColors.Red, 1, true);
        return XPens.red;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen RosyBrown
    {
      get
      {
        if (XPens.rosyBrown == null)
          XPens.rosyBrown = new XPen(XColors.RosyBrown, 1, true);
        return XPens.rosyBrown;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen RoyalBlue
    {
      get
      {
        if (XPens.royalBlue == null)
          XPens.royalBlue = new XPen(XColors.RoyalBlue, 1, true);
        return XPens.royalBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen SaddleBrown
    {
      get
      {
        if (XPens.saddleBrown == null)
          XPens.saddleBrown = new XPen(XColors.SaddleBrown, 1, true);
        return XPens.saddleBrown;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Salmon
    {
      get
      {
        if (XPens.salmon == null)
          XPens.salmon = new XPen(XColors.Salmon, 1, true);
        return XPens.salmon;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen SandyBrown
    {
      get
      {
        if (XPens.sandyBrown == null)
          XPens.sandyBrown = new XPen(XColors.SandyBrown, 1, true);
        return XPens.sandyBrown;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen SeaGreen
    {
      get
      {
        if (XPens.seaGreen == null)
          XPens.seaGreen = new XPen(XColors.SeaGreen, 1, true);
        return XPens.seaGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen SeaShell
    {
      get
      {
        if (XPens.seaShell == null)
          XPens.seaShell = new XPen(XColors.SeaShell, 1, true);
        return XPens.seaShell;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Sienna
    {
      get
      {
        if (XPens.sienna == null)
          XPens.sienna = new XPen(XColors.Sienna, 1, true);
        return XPens.sienna;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Silver
    {
      get
      {
        if (XPens.silver == null)
          XPens.silver = new XPen(XColors.Silver, 1, true);
        return XPens.silver;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen SkyBlue
    {
      get
      {
        if (XPens.skyBlue == null)
          XPens.skyBlue = new XPen(XColors.SkyBlue, 1, true);
        return XPens.skyBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen SlateBlue
    {
      get
      {
        if (XPens.slateBlue == null)
          XPens.slateBlue = new XPen(XColors.SlateBlue, 1, true);
        return XPens.slateBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen SlateGray
    {
      get
      {
        if (XPens.slateGray == null)
          XPens.slateGray = new XPen(XColors.SlateGray, 1, true);
        return XPens.slateGray;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Snow
    {
      get
      {
        if (XPens.snow == null)
          XPens.snow = new XPen(XColors.Snow, 1, true);
        return XPens.snow;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen SpringGreen
    {
      get
      {
        if (XPens.springGreen == null)
          XPens.springGreen = new XPen(XColors.SpringGreen, 1, true);
        return XPens.springGreen;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen SteelBlue
    {
      get
      {
        if (XPens.steelBlue == null)
          XPens.steelBlue = new XPen(XColors.SteelBlue, 1, true);
        return XPens.steelBlue;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Tan
    {
      get
      {
        if (XPens.tan == null)
          XPens.tan = new XPen(XColors.Tan, 1, true);
        return XPens.tan;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Teal
    {
      get
      {
        if (XPens.teal == null)
          XPens.teal = new XPen(XColors.Teal, 1, true);
        return XPens.teal;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Thistle
    {
      get
      {
        if (XPens.thistle == null)
          XPens.thistle = new XPen(XColors.Thistle, 1, true);
        return XPens.thistle;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Tomato
    {
      get
      {
        if (XPens.tomato == null)
          XPens.tomato = new XPen(XColors.Tomato, 1, true);
        return XPens.tomato;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Transparent
    {
      get
      {
        if (XPens.transparent == null)
          XPens.transparent = new XPen(XColors.Transparent, 1, true);
        return XPens.transparent;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Turquoise
    {
      get
      {
        if (XPens.turquoise == null)
          XPens.turquoise = new XPen(XColors.Turquoise, 1, true);
        return XPens.turquoise;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Violet
    {
      get
      {
        if (XPens.violet == null)
          XPens.violet = new XPen(XColors.Violet, 1, true);
        return XPens.violet;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Wheat
    {
      get
      {
        if (XPens.wheat == null)
          XPens.wheat = new XPen(XColors.Wheat, 1, true);
        return XPens.wheat;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen White
    {
      get
      {
        if (XPens.white == null)
          XPens.white = new XPen(XColors.White, 1, true);
        return XPens.white;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen WhiteSmoke
    {
      get
      {
        if (XPens.whiteSmoke == null)
          XPens.whiteSmoke = new XPen(XColors.WhiteSmoke, 1, true);
        return XPens.whiteSmoke;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen Yellow
    {
      get
      {
        if (XPens.yellow == null)
          XPens.yellow = new XPen(XColors.Yellow, 1, true);
        return XPens.yellow;
      }
    }

    /// <summary>Gets a pre-defined XPen object.</summary>
    public static XPen YellowGreen
    {
      get
      {
        if (XPens.yellowGreen == null)
          XPens.yellowGreen = new XPen(XColors.YellowGreen, 1, true);
        return XPens.yellowGreen;
      }
    }


    static XPen aliceBlue;
    static XPen antiqueWhite;
    static XPen aqua;
    static XPen aquamarine;
    static XPen azure;
    static XPen beige;
    static XPen bisque;
    static XPen black;
    static XPen blanchedAlmond;
    static XPen blue;
    static XPen blueViolet;
    static XPen brown;
    static XPen burlyWood;
    static XPen cadetBlue;
    static XPen chartreuse;
    static XPen chocolate;
    static XPen coral;
    static XPen cornflowerBlue;
    static XPen cornsilk;
    static XPen crimson;
    static XPen cyan;
    static XPen darkBlue;
    static XPen darkCyan;
    static XPen darkGoldenrod;
    static XPen darkGray;
    static XPen darkGreen;
    static XPen darkKhaki;
    static XPen darkMagenta;
    static XPen darkOliveGreen;
    static XPen darkOrange;
    static XPen darkOrchid;
    static XPen darkRed;
    static XPen darkSalmon;
    static XPen darkSeaGreen;
    static XPen darkSlateBlue;
    static XPen darkSlateGray;
    static XPen darkTurquoise;
    static XPen darkViolet;
    static XPen deepPink;
    static XPen deepSkyBlue;
    static XPen dimGray;
    static XPen dodgerBlue;
    static XPen firebrick;
    static XPen floralWhite;
    static XPen forestGreen;
    static XPen fuchsia;
    static XPen gainsboro;
    static XPen ghostWhite;
    static XPen gold;
    static XPen goldenrod;
    static XPen gray;
    static XPen green;
    static XPen greenYellow;
    static XPen honeydew;
    static XPen hotPink;
    static XPen indianRed;
    static XPen indigo;
    static XPen ivory;
    static XPen khaki;
    static XPen lavender;
    static XPen lavenderBlush;
    static XPen lawnGreen;
    static XPen lemonChiffon;
    static XPen lightBlue;
    static XPen lightCoral;
    static XPen lightCyan;
    static XPen lightGoldenrodYellow;
    static XPen lightGray;
    static XPen lightGreen;
    static XPen lightPink;
    static XPen lightSalmon;
    static XPen lightSeaGreen;
    static XPen lightSkyBlue;
    static XPen lightSlateGray;
    static XPen lightSteelBlue;
    static XPen lightYellow;
    static XPen lime;
    static XPen limeGreen;
    static XPen linen;
    static XPen magenta;
    static XPen maroon;
    static XPen mediumAquamarine;
    static XPen mediumBlue;
    static XPen mediumOrchid;
    static XPen mediumPurple;
    static XPen mediumSeaGreen;
    static XPen mediumSlateBlue;
    static XPen mediumSpringGreen;
    static XPen mediumTurquoise;
    static XPen mediumVioletRed;
    static XPen midnightBlue;
    static XPen mintCream;
    static XPen mistyRose;
    static XPen moccasin;
    static XPen navajoWhite;
    static XPen navy;
    static XPen oldLace;
    static XPen olive;
    static XPen oliveDrab;
    static XPen orange;
    static XPen orangeRed;
    static XPen orchid;
    static XPen paleGoldenrod;
    static XPen paleGreen;
    static XPen paleTurquoise;
    static XPen paleVioletRed;
    static XPen papayaWhip;
    static XPen peachPuff;
    static XPen peru;
    static XPen pink;
    static XPen plum;
    static XPen powderBlue;
    static XPen purple;
    static XPen red;
    static XPen rosyBrown;
    static XPen royalBlue;
    static XPen saddleBrown;
    static XPen salmon;
    static XPen sandyBrown;
    static XPen seaGreen;
    static XPen seaShell;
    static XPen sienna;
    static XPen silver;
    static XPen skyBlue;
    static XPen slateBlue;
    static XPen slateGray;
    static XPen snow;
    static XPen springGreen;
    static XPen steelBlue;
    static XPen tan;
    static XPen teal;
    static XPen thistle;
    static XPen tomato;
    static XPen transparent;
    static XPen turquoise;
    static XPen violet;
    static XPen wheat;
    static XPen white;
    static XPen whiteSmoke;
    static XPen yellow;
    static XPen yellowGreen;
  }
}
