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
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;

namespace Graphics
{
  /// <summary>
  /// The base class with some helper functions.
  /// </summary>
  public class Base
  {
    protected XColor backColor;
    protected XColor backColor2;
    protected XColor shadowColor;
    protected double borderWidth;
    protected XPen borderPen;

    protected Base()
    {
      this.backColor = XColors.Ivory;
      this.backColor2 = XColors.WhiteSmoke;

      this.backColor = XColor.FromArgb(212, 224, 240);
      this.backColor2 = XColor.FromArgb(253, 254, 254);

      this.shadowColor = XColors.Gainsboro;
      this.borderWidth = 4.5;
      this.borderPen = new XPen(XColor.FromArgb(94, 118, 151), this.borderWidth);//new XPen(XColors.SteelBlue, this.borderWidth);
    }

    /// <summary>
    /// Draws the page title and footer.
    /// </summary>
    public void DrawTitle(PdfPage page, XGraphics gfx, string title)
    {
      XRect rect = new XRect(new XPoint(), gfx.PageSize);
      rect.Inflate(-10, -15);
      XFont font = new XFont("Verdana", 14, XFontStyle.Bold);
      gfx.DrawString(title, font, XBrushes.MidnightBlue, rect, XStringFormats.TopCenter);

      rect.Offset(0, 5);
      font = new XFont("Verdana", 8, XFontStyle.Italic);
      XStringFormat format = new XStringFormat();
      format.Alignment = XStringAlignment.Near;
      format.LineAlignment = XLineAlignment.Far;
      gfx.DrawString("Created with " + PdfSharp.ProductVersionInfo.Producer, font, XBrushes.DarkOrchid, rect, format);

      font = new XFont("Verdana", 8);
      format.Alignment = XStringAlignment.Center;
      gfx.DrawString(Program.s_document.PageCount.ToString(), font, XBrushes.DarkOrchid, rect, format);

      Program.s_document.Outlines.Add(title, page, true);
    }

    /// <summary>
    /// Draws a sample box.
    /// </summary>
    public void BeginBox(XGraphics gfx, int number, string title)
    {
      const int dEllipse = 15;
      XRect rect = new XRect(0, 20, 300, 200);
      if (number % 2 == 0)
        rect.X = 300 - 5;
      rect.Y = 40 + ((number - 1) / 2) * (200 - 5);
      rect.Inflate(-10, -10);
      XRect rect2 = rect;
      rect2.Offset(this.borderWidth, this.borderWidth);
      gfx.DrawRoundedRectangle(new XSolidBrush(this.shadowColor), rect2, new XSize(dEllipse + 8, dEllipse + 8));
      XLinearGradientBrush brush = new XLinearGradientBrush(rect, this.backColor, this.backColor2, XLinearGradientMode.Vertical);
      gfx.DrawRoundedRectangle(this.borderPen, brush, rect, new XSize(dEllipse, dEllipse));
      rect.Inflate(-5, -5);

      XFont font = new XFont("Verdana", 12, XFontStyle.Regular);
      gfx.DrawString(title, font, XBrushes.Navy, rect, XStringFormats.TopCenter);

      rect.Inflate(-10, -5);
      rect.Y += 20;
      rect.Height -= 20;
      //gfx.DrawRectangle(XPens.Red, rect);

      this.state = gfx.Save();
      gfx.TranslateTransform(rect.X, rect.Y);
    }

    public void EndBox(XGraphics gfx)
    {
      gfx.Restore(this.state);
    }

    /// <summary>
    /// Gets a five-pointed star with the specified size and center.
    /// </summary>
    protected static XPoint[] GetPentagram(double size, XPoint center)
    {
      XPoint[] points = Pentagram.Clone() as XPoint[];
      for (int idx = 0; idx < 5; idx++)
      {
        points[idx].X = points[idx].X * size + center.X;
        points[idx].Y = points[idx].Y * size + center.Y;
      }
      return points;
    }

    /// <summary>
    /// Gets a normalized five-pointed star.
    /// </summary>
    static XPoint[] Pentagram
    {
      get
      {
        if (pentagram == null)
        {
          int[] order = new int[] { 0, 3, 1, 4, 2 };
          pentagram = new XPoint[5];
          for (int idx = 0; idx < 5; idx++)
          {
            double rad = order[idx] * 2 * Math.PI / 5 - Math.PI / 10;
            pentagram[idx].X = Math.Cos(rad);
            pentagram[idx].Y = Math.Sin(rad);
          }
        }
        return pentagram;
      }
    }
    static XPoint[] pentagram;

    XGraphicsState state;
  }
}
