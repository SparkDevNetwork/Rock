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
  /// Shows how to handle text.
  /// </summary>
  public class Text : Base
  {
    public void DrawPage(PdfPage page)
    {
      XGraphics gfx = XGraphics.FromPdfPage(page);

      DrawTitle(page, gfx, "Text");

      DrawText(gfx, 1);
      DrawTextAlignment(gfx, 2);
      MeasureText(gfx, 3);
    }

    /// <summary>
    /// Draws text in different styles.
    /// </summary>
    void DrawText(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "Text Styles");

      const string facename = "Times New Roman";

      //XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
      XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.WinAnsi, PdfFontEmbedding.Default);

      XFont fontRegular = new XFont(facename, 20, XFontStyle.Regular, options);
      XFont fontBold = new XFont(facename, 20, XFontStyle.Bold, options);
      XFont fontItalic = new XFont(facename, 20, XFontStyle.Italic, options);
      XFont fontBoldItalic = new XFont(facename, 20, XFontStyle.BoldItalic, options);

      // The default alignment is baseline left (that differs from GDI+)
      gfx.DrawString("Times (regular)", fontRegular, XBrushes.DarkSlateGray, 0, 30);
      gfx.DrawString("Times (bold)", fontBold, XBrushes.DarkSlateGray, 0, 65);
      gfx.DrawString("Times (italic)", fontItalic, XBrushes.DarkSlateGray, 0, 100);
      gfx.DrawString("Times (bold italic)", fontBoldItalic, XBrushes.DarkSlateGray, 0, 135);

      EndBox(gfx);
    }

    /// <summary>
    /// Shows how to align text in the layout rectangle.
    /// </summary>
    void DrawTextAlignment(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "Text Alignment");
      XRect rect = new XRect(0, 0, 250, 140);

      XFont font = new XFont("Verdana", 10);
      XBrush brush = XBrushes.Purple;
      XStringFormat format = new XStringFormat();

      gfx.DrawRectangle(XPens.YellowGreen, rect);
      gfx.DrawLine(XPens.YellowGreen, rect.Width / 2, 0, rect.Width / 2, rect.Height);
      gfx.DrawLine(XPens.YellowGreen, 0, rect.Height / 2, rect.Width, rect.Height / 2);

      gfx.DrawString("TopLeft", font, brush, rect, format);

      format.Alignment = XStringAlignment.Center;
      gfx.DrawString("TopCenter", font, brush, rect, format);

      format.Alignment = XStringAlignment.Far;
      gfx.DrawString("TopRight", font, brush, rect, format);

      format.LineAlignment = XLineAlignment.Center;
      format.Alignment = XStringAlignment.Near;
      gfx.DrawString("CenterLeft", font, brush, rect, format);

      format.Alignment = XStringAlignment.Center;
      gfx.DrawString("Center", font, brush, rect, format);

      format.Alignment = XStringAlignment.Far;
      gfx.DrawString("CenterRight", font, brush, rect, format);

      format.LineAlignment = XLineAlignment.Far;
      format.Alignment = XStringAlignment.Near;
      gfx.DrawString("BottomLeft", font, brush, rect, format);

      format.Alignment = XStringAlignment.Center;
      gfx.DrawString("BottomCenter", font, brush, rect, format);

      format.Alignment = XStringAlignment.Far;
      gfx.DrawString("BottomRight", font, brush, rect, format);

      EndBox(gfx);
    }

    /// <summary>
    /// Shows how to get text metric information.
    /// </summary>
    void MeasureText(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "Measure Text");

      const XFontStyle style = XFontStyle.Regular;
      XFont font = new XFont("Times New Roman", 95, style);

      const string text = "Hallo";
      const double x = 20, y = 100;
      XSize size = gfx.MeasureString(text, font);

      double lineSpace = font.GetHeight(gfx);
      int cellSpace = font.FontFamily.GetLineSpacing(style);
      int cellAscent = font.FontFamily.GetCellAscent(style);
      int cellDescent = font.FontFamily.GetCellDescent(style);
      int cellLeading = cellSpace - cellAscent - cellDescent;

      // Get effective ascent
      double ascent = lineSpace * cellAscent / cellSpace;
      gfx.DrawRectangle(XBrushes.Bisque, x, y - ascent, size.Width, ascent);

      // Get effective descent
      double descent = lineSpace * cellDescent / cellSpace;
      gfx.DrawRectangle(XBrushes.LightGreen, x, y, size.Width, descent);

      // Get effective leading
      double leading = lineSpace * cellLeading / cellSpace;
      gfx.DrawRectangle(XBrushes.Yellow, x, y + descent, size.Width, leading);

      // Draw text half transparent
      XColor color = XColors.DarkSlateBlue;
      color.A = 0.6;
      gfx.DrawString(text, font, new XSolidBrush(color), x, y);

      EndBox(gfx);
    }
  }
}
