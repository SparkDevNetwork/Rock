#region PDFsharp - A .NET library for processing PDF
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
//
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT OF THIRD PARTY RIGHTS.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
// USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
//using System.Drawing;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Graphics
{
  /// <summary>
  /// Shows how to draw graphical shapes.
  /// </summary>
  public class Shapes : Base
  {
    public void DrawPage(PdfPage page)
    {
      XGraphics gfx = XGraphics.FromPdfPage(page);

      DrawTitle(page, gfx, "Shapes");

      DrawRectangle(gfx, 1);
      DrawRoundedRectangle(gfx, 2);
      DrawEllipse(gfx, 3);
      DrawPolygon(gfx, 4);
      DrawPie(gfx, 5);
      DrawClosedCurve(gfx, 6);
    }

    /// <summary>
    /// Draws rectangles.
    /// </summary>
    void DrawRectangle(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "DrawRectangle");

      XPen pen = new XPen(XColors.Navy, Math.PI);

      gfx.DrawRectangle(pen, 10, 0, 100, 60);
      gfx.DrawRectangle(XBrushes.DarkOrange, 130, 0, 100, 60);
      gfx.DrawRectangle(pen, XBrushes.DarkOrange, 10, 80, 100, 60);
      gfx.DrawRectangle(pen, XBrushes.DarkOrange, 150, 80, 60, 60);

      EndBox(gfx);
    }

    /// <summary>
    /// Draws rounded rectangles.
    /// </summary>
    void DrawRoundedRectangle(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "DrawRoundedRectangle");

      XPen pen = new XPen(XColors.RoyalBlue, Math.PI);

      gfx.DrawRoundedRectangle(pen, 10, 0, 100, 60, 30, 20);
      gfx.DrawRoundedRectangle(XBrushes.Orange, 130, 0, 100, 60, 30, 20);
      gfx.DrawRoundedRectangle(pen, XBrushes.Orange, 10, 80, 100, 60, 30, 20);
      gfx.DrawRoundedRectangle(pen, XBrushes.Orange, 150, 80, 60, 60, 20, 20);

      EndBox(gfx);
    }

    /// <summary>
    /// Draws ellipses.
    /// </summary>
    void DrawEllipse(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "DrawEllipse");

      XPen pen = new XPen(XColors.DarkBlue, 2.5);

      gfx.DrawEllipse(pen, 10, 0, 100, 60);
      gfx.DrawEllipse(XBrushes.Goldenrod, 130, 0, 100, 60);
      gfx.DrawEllipse(pen, XBrushes.Goldenrod, 10, 80, 100, 60);
      gfx.DrawEllipse(pen, XBrushes.Goldenrod, 150, 80, 60, 60);

      EndBox(gfx);
    }

    /// <summary>
    /// Draws polygons.
    /// </summary>
    void DrawPolygon(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "DrawPolygon");

      XPen pen = new XPen(XColors.DarkBlue, 2.5);

      gfx.DrawPolygon(pen, XBrushes.LightCoral, GetPentagram(50, new XPoint(60, 70)), XFillMode.Winding);
      gfx.DrawPolygon(pen, XBrushes.LightCoral, GetPentagram(50, new XPoint(180, 70)), XFillMode.Alternate);

      EndBox(gfx);
    }

    /// <summary>
    /// Draws pies.
    /// </summary>
    void DrawPie(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "DrawPie");

      XPen pen = new XPen(XColors.DarkBlue, 2.5);

      gfx.DrawPie(pen, 10, 0, 100, 90, -120, 75);
      gfx.DrawPie(XBrushes.Gold, 130, 0, 100, 90, -160, 150);
      gfx.DrawPie(pen, XBrushes.Gold, 10, 50, 100, 90, 80, 70);
      gfx.DrawPie(pen, XBrushes.Gold, 150, 80, 60, 60, 35, 290);

      EndBox(gfx);
    }

    /// <summary>
    /// Draws a closed cardinal spline.
    /// </summary>
    void DrawClosedCurve(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "DrawClosedCurve");

      XPen pen = new XPen(XColors.DarkBlue, 2.5);
      gfx.DrawClosedCurve(pen, XBrushes.SkyBlue,
        new XPoint[] { new XPoint(10, 120), new XPoint(80, 30), new XPoint(220, 20), new XPoint(170, 110), new XPoint(100, 90) },
        XFillMode.Winding, 0.7);

      EndBox(gfx);
    }
  }
}
