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
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.IO;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp.Internal;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Drawing.Pdf
{
  /// <summary>
  /// Represents a drawing surface for PdfPages.
  /// </summary>
  class XGraphicsPdfRenderer : IXGraphicsRenderer
  {
    public XGraphicsPdfRenderer(PdfPage page, XGraphics gfx, XGraphicsPdfPageOptions options)
    {
      this.page = page;
      this.colorMode = page.document.Options.ColorMode;
      this.options = options;
#if MIGRADOC
      this.options = options & ~XGraphicsPdfPageOptions.PDFlibHack;
      pdflibHack = (options & XGraphicsPdfPageOptions.PDFlibHack) != 0;
#endif
      this.gfx = gfx;
      this.content = new StringBuilder();
      page.RenderContent.pdfRenderer = this;
      this.gfxState = new PdfGraphicsState(this);
    }
#if MIGRADOC
    bool pdflibHack;
#endif

    public XGraphicsPdfRenderer(XForm form, XGraphics gfx)
    {
      this.form = form;
      this.colorMode = form.Owner.Options.ColorMode;
      this.gfx = gfx;
      this.content = new StringBuilder();
      form.pdfRenderer = this;
      this.gfxState = new PdfGraphicsState(this);
    }

    /// <summary>
    /// Gets the content created by this renderer.
    /// </summary>
    string GetContent()
    {
      EndPage();
      return this.content.ToString();
    }

    public XGraphicsPdfPageOptions PageOptions
    {
      get { return this.options; }
    }

    public void Close()
    {
      if (this.page != null)
      {
        PdfContent content = page.RenderContent;
        content.CreateStream(PdfEncoders.RawEncoding.GetBytes(GetContent()));

        this.gfx = null;
        this.page.RenderContent.pdfRenderer = null;
        this.page.RenderContent = null;
        this.page = null;
      }
      else if (this.form != null)
      {
        this.form.pdfForm.CreateStream(PdfEncoders.RawEncoding.GetBytes(GetContent()));
        this.gfx = null;
        this.form.pdfRenderer = null;
        this.form = null;
      }
    }

    // --------------------------------------------------------------------------------------------

    #region  Drawing

    //void SetPageLayout(down, point(0, 0), unit

    // ----- Clear --------------------------------------------------------------------------------

    public void Clear(XColor color)
    {
      if (!this.gfx.transform.IsIdentity)
        throw new NotImplementedException("Transform must be identity to clear the canvas.");

      // TODO: this is implementation is bogus. Reset transformation to identity an then fill
      XBrush brush = new XSolidBrush(color);
      DrawRectangle(null, brush, 0, 0, Size.Width, Size.Height);
    }

    // ----- DrawLine -----------------------------------------------------------------------------

    /// <summary>
    /// Strokes a single connection of two points.
    /// </summary>
    public void DrawLine(XPen pen, double x1, double y1, double x2, double y2)
    {
      DrawLines(pen, new XPoint[2] { new XPoint(x1, y1), new XPoint(x2, y2) });
    }

    // ----- DrawLines ----------------------------------------------------------------------------

    /// <summary>
    /// Strokes a series of connected points.
    /// </summary>
    public void DrawLines(XPen pen, XPoint[] points)
    {
      if (pen == null)
        throw new ArgumentNullException("pen");
      if (points == null)
        throw new ArgumentNullException("points");

      int count = points.Length;
      if (count == 0)
        return;

      Realize(pen);

      AppendFormat("{0:0.###} {1:0.###} m\n", points[0].X, points[0].Y);
      for (int idx = 1; idx < count; idx++)
        AppendFormat("{0:0.###} {1:0.###} l\n", points[idx].X, points[idx].Y);
      this.content.Append("S\n");
    }

    // ----- DrawBezier ---------------------------------------------------------------------------

    public void DrawBezier(XPen pen, double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
    {
      DrawBeziers(pen, new XPoint[4] { new XPoint(x1, y1), new XPoint(x2, y2), new XPoint(x3, y3), new XPoint(x4, y4) });
    }

    // ----- DrawBeziers --------------------------------------------------------------------------

    public void DrawBeziers(XPen pen, XPoint[] points)
    {
      if (pen == null)
        throw new ArgumentNullException("pen");
      if (points == null)
        throw new ArgumentNullException("points");

      int count = points.Length;
      if (count == 0)
        return;

      if ((count - 1) % 3 != 0)
        throw new ArgumentException("Invalid number of points for bezier curves. Number must fulfil 4+3n.", "points");

      Realize(pen);

      AppendFormat("{0:0.####} {1:0.####} m\n", points[0].X, points[0].Y);
      for (int idx = 1; idx < count; idx += 3)
        AppendFormat("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n",
          points[idx].X, points[idx].Y,
          points[idx + 1].X, points[idx + 1].Y,
          points[idx + 2].X, points[idx + 2].Y);

      AppendStrokeFill(pen, null, XFillMode.Alternate, false);
    }

    // ----- DrawCurve ----------------------------------------------------------------------------

    public void DrawCurve(XPen pen, XPoint[] points, double tension)
    {
      if (pen == null)
        throw new ArgumentNullException("pen");
      if (points == null)
        throw new ArgumentNullException("points");

      int count = points.Length;
      if (count == 0)
        return;
      if (count < 2)
        throw new ArgumentException("Not enough points", "points");

      // See http://pubpages.unh.edu/~cs770/a5/cardinal.html
      tension /= 3;

      Realize(pen);

      AppendFormat("{0:0.###} {1:0.###} m\n", points[0].X, points[0].Y);
      if (count == 2)
      {
        // Just draws a line...
        AppendCurveSegment(points[0], points[0], points[1], points[1], tension);
      }
      else
      {
        AppendCurveSegment(points[0], points[0], points[1], points[2], tension);
        for (int idx = 1; idx < count - 2; idx++)
          AppendCurveSegment(points[idx - 1], points[idx], points[idx + 1], points[idx + 2], tension);
        AppendCurveSegment(points[count - 3], points[count - 2], points[count - 1], points[count - 1], tension);
      }
      AppendStrokeFill(pen, null, XFillMode.Alternate, false);
    }

    // ----- DrawArc ------------------------------------------------------------------------------

    public void DrawArc(XPen pen, double x, double y, double width, double height, double startAngle, double sweepAngle)
    {
      if (pen == null)
        throw new ArgumentNullException("pen");

      Realize(pen);

      AppendPartialArc(x, y, width, height, startAngle, sweepAngle, PathStart.MoveTo1st, new XMatrix());
      AppendStrokeFill(pen, null, XFillMode.Alternate, false);
    }

    // ----- DrawRectangle ------------------------------------------------------------------------

    public void DrawRectangle(XPen pen, XBrush brush, double x, double y, double width, double height)
    {
      if (pen == null && brush == null)
        throw new ArgumentNullException("pen and brush");

      Realize(pen, brush);
      AppendFormat("{0:0.###} {1:0.###} {2:0.###} {3:0.###} re\n", x, y, width, height);

      if (pen != null && brush != null)
        this.content.Append("B\n");
      else if (pen != null)
        this.content.Append("S\n");
      else
        this.content.Append("f\n");
    }

    // ----- DrawRectangles -----------------------------------------------------------------------

    public void DrawRectangles(XPen pen, XBrush brush, XRect[] rects)
    {
      int count = rects.Length;
      for (int idx = 0; idx < count; idx++)
      {
        XRect rect = rects[idx];
        DrawRectangle(pen, brush, rect.X, rect.Y, rect.Width, rect.Height);
      }
    }

    // ----- DrawRoundedRectangle -----------------------------------------------------------------

    public void DrawRoundedRectangle(XPen pen, XBrush brush, double x, double y, double width, double height, double ellipseWidth, double ellipseHeight)
    {
      XGraphicsPath path = new XGraphicsPath();
      path.AddRoundedRectangle(x, y, width, height, ellipseWidth, ellipseHeight);
      DrawPath(pen, brush, path);
    }

    // ----- DrawEllipse --------------------------------------------------------------------------

    public void DrawEllipse(XPen pen, XBrush brush, double x, double y, double width, double height)
    {
      Realize(pen, brush);

      // Useful information are here http://home.t-online.de/home/Robert.Rossmair/ellipse.htm
      // or here http://www.whizkidtech.redprince.net/bezier/circle/
      // Deeper but more difficult: http://www.tinaja.com/cubic01.asp
      // Petzold: 4/3 * tan(α / 4)
      const double κ = 0.5522847498;  // := 4/3 * (1 - cos(-π/4)) / sin(π/4)) <=> 4/3 * sqrt(2) - 1
      XRect rect = new XRect(x, y, width, height);
      double δx = rect.Width / 2;
      double δy = rect.Height / 2;
      double fx = δx * κ;
      double fy = δy * κ;
      double x0 = rect.X + δx;
      double y0 = rect.Y + δy;

      AppendFormat("{0:0.####} {1:0.####} m\n", x0 + δx, y0);
      AppendFormat("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n",
        x0 + δx, y0 + fy, x0 + fx, y0 + δy, x0, y0 + δy);
      AppendFormat("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n",
        x0 - fx, y0 + δy, x0 - δx, y0 + fy, x0 - δx, y0);
      AppendFormat("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n",
        x0 - δx, y0 - fy, x0 - fx, y0 - δy, x0, y0 - δy);
      AppendFormat("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n",
        x0 + fx, y0 - δy, x0 + δx, y0 - fy, x0 + δx, y0);
      AppendStrokeFill(pen, brush, XFillMode.Winding, true);
    }

    // ----- DrawPolygon --------------------------------------------------------------------------

    public void DrawPolygon(XPen pen, XBrush brush, XPoint[] points, XFillMode fillmode)
    {
      Realize(pen, brush);

      int count = points.Length;
      if (points.Length < 2)
        throw new ArgumentException("points", PSSR.PointArrayAtLeast(2));

      AppendFormat("{0:0.####} {1:0.####} m\n", points[0].X, points[0].Y);
      for (int idx = 1; idx < count; idx++)
        AppendFormat("{0:0.####} {1:0.####} l\n", points[idx].X, points[idx].Y);

      AppendStrokeFill(pen, brush, fillmode, true);
    }

    // ----- DrawPie ------------------------------------------------------------------------------

    public void DrawPie(XPen pen, XBrush brush, double x, double y, double width, double height,
      double startAngle, double sweepAngle)
    {
      Realize(pen, brush);

      AppendFormat("{0:0.####} {1:0.####} m\n", x + width / 2, y + height / 2);
      AppendPartialArc(x, y, width, height, startAngle, sweepAngle, PathStart.LineTo1st, new XMatrix());
      AppendStrokeFill(pen, brush, XFillMode.Alternate, true);
    }

    // ----- DrawClosedCurve ----------------------------------------------------------------------

    public void DrawClosedCurve(XPen pen, XBrush brush, XPoint[] points, double tension, XFillMode fillmode)
    {
      int count = points.Length;
      if (count == 0)
        return;
      if (count < 2)
        throw new ArgumentException("Not enough points", "points");

      // Simply tried out. Not proofed why it is correct.
      tension /= 3;

      Realize(pen, brush);

      AppendFormat("{0:0.####} {1:0.####} m\n", points[0].X, points[0].Y);
      if (count == 2)
      {
        // Just draws a line...
        AppendCurveSegment(points[0], points[0], points[1], points[1], tension);
      }
      else
      {
        AppendCurveSegment(points[count - 1], points[0], points[1], points[2], tension);
        for (int idx = 1; idx < count - 2; idx++)
          AppendCurveSegment(points[idx - 1], points[idx], points[idx + 1], points[idx + 2], tension);
        AppendCurveSegment(points[count - 3], points[count - 2], points[count - 1], points[0], tension);
        AppendCurveSegment(points[count - 2], points[count - 1], points[0], points[1], tension);
      }
      AppendStrokeFill(pen, brush, fillmode, true);
    }

    // ----- DrawPath -----------------------------------------------------------------------------

    public void DrawPath(XPen pen, XBrush brush, XGraphicsPath path)
    {
      if (pen == null && brush == null)
        throw new ArgumentNullException("pen");

#if GDI && !WPF
      Realize(pen, brush);
      AppendPath(path.gdipPath);
      AppendStrokeFill(pen, brush, path.FillMode, false);
#endif
#if WPF && !GDI
      Realize(pen, brush);
      AppendPath(path.pathGeometry);
      AppendStrokeFill(pen, brush, path.FillMode, false);
#endif
#if WPF && GDI
      Realize(pen, brush);
      if (this.gfx.targetContext == XGraphicTargetContext.GDI)
        AppendPath(path.gdipPath);
      else
        AppendPath(path.pathGeometry);
      AppendStrokeFill(pen, brush, path.FillMode, false);
#endif
    }

    // ----- DrawString ---------------------------------------------------------------------------

    public void DrawString(string s, XFont font, XBrush brush, XRect rect, XStringFormat format)
    {
      Realize(font, brush, 0);

      double x = rect.X;
      double y = rect.Y;

      double lineSpace = font.GetHeight(this.gfx);
      //int cellSpace = font.cellSpace; // font.FontFamily.GetLineSpacing(font.Style);
      //int cellAscent = font.cellAscent; // font.FontFamily.GetCellAscent(font.Style);
      //int cellDescent = font.cellDescent; // font.FontFamily.GetCellDescent(font.Style);
      //double cyAscent = lineSpace * cellAscent / cellSpace;
      //double cyDescent = lineSpace * cellDescent / cellSpace;
      double cyAscent = lineSpace * font.cellAscent / font.cellSpace;
      double cyDescent = lineSpace * font.cellDescent / font.cellSpace;
      double width = this.gfx.MeasureString(s, font).Width;

      bool bold = (font.Style & XFontStyle.Bold) != 0;
      bool italic = (font.Style & XFontStyle.Italic) != 0;
      bool strikeout = (font.Style & XFontStyle.Strikeout) != 0;
      bool underline = (font.Style & XFontStyle.Underline) != 0;

      switch (format.Alignment)
      {
        case XStringAlignment.Near:
          // nothing to do
          break;

        case XStringAlignment.Center:
          x += (rect.Width - width) / 2;
          break;

        case XStringAlignment.Far:
          x += rect.Width - width;
          break;
      }
      if (Gfx.PageDirection == XPageDirection.Downwards)
      {
        switch (format.LineAlignment)
        {
          case XLineAlignment.Near:
            y += cyAscent;
            break;

          case XLineAlignment.Center:
            // TODO use CapHeight. PDFlib also uses 3/4 of ascent
            y += (cyAscent * 3 / 4) / 2 + rect.Height / 2;
            break;

          case XLineAlignment.Far:
            y += -cyDescent + rect.Height;
            break;

          case XLineAlignment.BaseLine:
            // nothing to do
            break;
        }
      }
      else
      {
        switch (format.LineAlignment)
        {
          case XLineAlignment.Near:
            y += cyDescent;
            break;

          case XLineAlignment.Center:
            // TODO use CapHeight. PDFlib also uses 3/4 of ascent
            y += -(cyAscent * 3 / 4) / 2 + rect.Height / 2;
            break;

          case XLineAlignment.Far:
            y += -cyAscent + rect.Height;
            break;

          case XLineAlignment.BaseLine:
            // nothing to do
            break;
        }
      }

      PdfFont realizedFont = this.gfxState.realizedFont;
      Debug.Assert(realizedFont != null);
      realizedFont.AddChars(s);

      OpenTypeDescriptor descriptor = realizedFont.FontDescriptor.descriptor;

      if (bold && !descriptor.IsBoldFace)
      {
        // TODO: emulate bold by thicker outline
      }

      if (italic && !descriptor.IsBoldFace)
      {
        // TODO: emulate italic by shearing transformation
      }

      if (font.Unicode)
      {
        string s2 = "";
        for (int idx = 0; idx < s.Length; idx++)
        {
          char ch = s[idx];
          int glyphID = 0;
          if (descriptor.fontData.cmap.symbol)
          {
            glyphID = (int)ch + (descriptor.fontData.os2.usFirstCharIndex & 0xFF00);
            glyphID = descriptor.CharCodeToGlyphIndex((char)glyphID);
          }
          else
            glyphID = descriptor.CharCodeToGlyphIndex(ch);
          s2 += (char)glyphID;
        }
        s = s2;

        byte[] bytes = PdfEncoders.RawUnicodeEncoding.GetBytes(s);
        bytes = PdfEncoders.FormatStringLiteral(bytes, true, false, true, null);
        string text = PdfEncoders.RawEncoding.GetString(bytes, 0, bytes.Length);
        XPoint pos = new XPoint(x, y);
        AdjustTextMatrix(ref pos);
        AppendFormat(
          "{0:0.####} {1:0.####} Td {2} Tj\n", pos.x, pos.y, text);
        //PdfEncoders.ToStringLiteral(s, PdfStringEncoding.RawEncoding, null));
      }
      else
      {
        byte[] bytes = PdfEncoders.WinAnsiEncoding.GetBytes(s);
        XPoint pos = new XPoint(x, y);
        AdjustTextMatrix(ref pos);
        AppendFormat(
          "{0:0.####} {1:0.####} Td {2} Tj\n", pos.x, pos.y,
          PdfEncoders.ToStringLiteral(bytes, false, null));
      }

      if (underline)
      {
        double underlinePosition = lineSpace * realizedFont.FontDescriptor.descriptor.UnderlinePosition / font.cellSpace;
        double underlineThickness = lineSpace * realizedFont.FontDescriptor.descriptor.UnderlineThickness / font.cellSpace;
        DrawRectangle(null, brush, x, y - underlinePosition, width, underlineThickness);
      }

      if (strikeout)
      {
        double strikeoutPosition = lineSpace * realizedFont.FontDescriptor.descriptor.StrikeoutPosition / font.cellSpace;
        double strikeoutSize = lineSpace * realizedFont.FontDescriptor.descriptor.StrikeoutSize / font.cellSpace;
        DrawRectangle(null, brush, x, y - strikeoutPosition - strikeoutSize, width, strikeoutSize);
      }
    }

    // ----- DrawImage ----------------------------------------------------------------------------

    //public void DrawImage(Image image, Point point);
    //public void DrawImage(Image image, PointF point);
    //public void DrawImage(Image image, Point[] destPoints);
    //public void DrawImage(Image image, PointF[] destPoints);
    //public void DrawImage(Image image, Rectangle rect);
    //public void DrawImage(Image image, RectangleF rect);
    //public void DrawImage(Image image, int x, int y);
    //public void DrawImage(Image image, float x, float y);
    //public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit);
    //public void DrawImage(Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit);
    //public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit);
    //public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit);
    //public void DrawImage(Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit);
    //public void DrawImage(Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit);
    //public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr);
    //public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr);
    //public void DrawImage(Image image, int x, int y, int width, int height);
    //public void DrawImage(Image image, float x, float y, float width, float height);
    //public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback);
    //public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback);
    //public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit);
    //public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit);
    //public void DrawImage(Image image, Point[] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData);
    //public void DrawImage(Image image, PointF[] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData);
    //public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr);
    //public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs);
    //public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback);
    //public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback);
    //public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback, IntPtr callbackData);
    //public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes

    public void DrawImage(XImage image, double x, double y, double width, double height)
    {
      string name = Realize(image);
      if (!(image is XForm))
      {
        if (this.gfx.PageDirection == XPageDirection.Downwards)
        {
          AppendFormat("q {2:0.####} 0 0 -{3:0.####} {0:0.####} {4:0.####} cm {5} Do Q\n",
            x, y, width, height, y + height, name);
        }
        else
        {
          AppendFormat("q {2:0.####} 0 0 {3:0.####} {0:0.####} {1:0.####} cm {4} Do Q\n",
            x, y, width, height, name);
        }
      }
      else
      {
        BeginPage();

        XForm form = (XForm)image;
        form.Finish();

        PdfFormXObject pdfForm = Owner.FormTable.GetForm(form);

        double cx = width / image.PointWidth;
        double cy = height / image.PointHeight;

        if (cx != 0 && cy != 0)
        {
          if (this.gfx.PageDirection == XPageDirection.Downwards)
          {
            AppendFormat("q {2:0.####} 0 0 -{3:0.####} {0:0.####} {4:0.####} cm 100 Tz {5} Do Q\n",
              x, y, cx, cy, y + height, name);
          }
          else
          {
            AppendFormat("q {2:0.####} 0 0 {3:0.####} {0:0.####} {1:0.####} cm {4} Do Q\n",
              x, y, cx, cy, name);
          }
        }
      }
    }

    // TODO: incomplete - srcRect not used
    public void DrawImage(XImage image, XRect destRect, XRect srcRect, XGraphicsUnit srcUnit)
    {
      double x = destRect.X;
      double y = destRect.Y;
      double width = destRect.Width;
      double height = destRect.Height;

      string name = Realize(image);
      if (!(image is XForm))
      {
        if (this.gfx.PageDirection == XPageDirection.Downwards)
        {
          AppendFormat("q {2:0.####} 0 0 -{3:0.####} {0:0.####} {4:0.####} cm {5} Do\nQ\n",
            x, y, width, height, y + height, name);
        }
        else
        {
          AppendFormat("q {2:0.####} 0 0 {3:0.####} {0:0.####} {1:0.####} cm {4} Do Q\n",
            x, y, width, height, name);
        }
      }
      else
      {
        BeginPage();

        XForm xForm = (XForm)image;
        xForm.Finish();

        PdfFormXObject pdfForm = Owner.FormTable.GetForm(xForm);

        double cx = width / image.PointWidth;
        double cy = height / image.PointHeight;

        if (cx != 0 && cy != 0)
        {
          if (this.gfx.PageDirection == XPageDirection.Downwards)
          {
            AppendFormat("q {2:0.####} 0 0 -{3:0.####} {0:0.####} {4:0.####} cm 100 Tz {5} Do Q\n",
              x, y, cx, cy, y + height, name);
          }
          else
          {
            AppendFormat("q {2:0.####} 0 0 {3:0.####} {0:0.####} {1:0.####} cm {4} Do Q\n",
              x, y, cx, cy, name);
          }
        }
      }
    }

    #endregion

    // --------------------------------------------------------------------------------------------

    #region Save and Restore

    /// <summary>
    /// Clones the current graphics state and push it on a stack.
    /// </summary>
    public void Save(XGraphicsState state)
    {
      // Before saving, the current transformation matrix must be completely realized.
      BeginGraphic();
      RealizeTransform();
      // Associate the XGraphicsState with the current PdgGraphicsState.
      this.gfxState.InternalState = state.InternalState;
      SaveState();
    }

    public void Restore(XGraphicsState state)
    {
      BeginGraphic();
      RestoreState(state.InternalState);
    }

    public void BeginContainer(XGraphicsContainer container, XRect dstrect, XRect srcrect, XGraphicsUnit unit)
    {
      // Before saving, the current transformation matrix must be completely realized.
      BeginGraphic();
      RealizeTransform();
      this.gfxState.InternalState = container.InternalState;
      SaveState();
      //throw new NotImplementedException("BeginContainer");
      //      PdfGraphicsState pdfstate = (PdfGraphicsState)this.gfxState.Clone();
      //      this.gfxStateStack.Push(pdfstate);
      //      container.Handle = pdfstate;
      //      container.Handle = this.gfxState.Clone();

      //      Matrix matrix = new Matrix();
      //      matrix.Translate(srcrect.X, srcrect.Y);
      //      matrix.Scale(dstrect.Width / srcrect.Width, dstrect.Height / srcrect.Height);
      //      matrix.Translate(dstrect.X, dstrect.Y);
      //      Transform = matrix;
    }

    public void EndContainer(XGraphicsContainer container)
    {
      BeginGraphic();
      RestoreState(container.InternalState);
      //      PdfGraphicsState pdfstate = (PdfGraphicsState)container.Handle;
      //      this.dirty |= DirtyFlags.Ctm;
      //      this.gfxState = pdfstate;
    }

    #endregion

    // --------------------------------------------------------------------------------------------

    #region Transformation

    public void SetPageTransform(XPageDirection direction, XPoint origion, XGraphicsUnit unit)
    {
      if (this.gfxStateStack.Count > 0)
        throw new InvalidOperationException("PageTransformation can be modified only when the graphics stack is empty.");

      throw new NotImplementedException("SetPageTransform");
    }

    //public void TranslateTransform(double dx, double dy, XMatrixOrder order)
    //{
    //}
    //
    //public void ScaleTransform(double scaleX, double scaleY, XMatrixOrder order)
    //{
    //}
    //
    //public void ScaleTransform(double scaleXY, XMatrixOrder order)
    //{
    //}
    //
    //public void RotateTransform(double angle, XMatrixOrder order)
    //{
    //}
    //
    //public void MultiplyTransform(XMatrix matrix, XMatrixOrder order)
    //{
    //}

    public XMatrix Transform
    {
      //get {return this.gfxState.Ctm;}
      set { this.gfxState.Transform = value; }
    }

    #endregion

    // --------------------------------------------------------------------------------------------

    #region Clipping

    public void SetClip(XGraphicsPath path, XCombineMode combineMode)
    {
      if (path == null)
        throw new NotImplementedException("SetClip with no path.");

      // Ensure that the graphics state stack level is at least 2, because otherwise an error
      // occurs when someone set the clip region before something was drawn.
      if (this.gfxState.Level < GraphicsStackLevelWorldSpace)
        RealizeTransform();  // TODO: refactor this function

      if (combineMode == XCombineMode.Replace)
      {
        if (this.clipLevel != 0)
        {
          if (this.clipLevel != this.gfxState.Level)
            throw new NotImplementedException("Cannot set new clip region in an inner graphic state level.");
          else
            ResetClip();
        }
        this.clipLevel = this.gfxState.Level;
      }
      else if (combineMode == XCombineMode.Intersect)
      {
        if (this.clipLevel == 0)
          this.clipLevel = this.gfxState.Level;
      }
      else
      {
        Debug.Assert(false, "Invalid XCombineMode in internal function.");
      }
      this.gfxState.SetAndRealizeClipPath(path);
    }

    /// <summary>
    /// Sets the clip path empty. Only possible if graphic state level has the same value as it has when
    /// the first time SetClip was invoked.
    /// </summary>
    public void ResetClip()
    {
      // No clip level means no clipping occurs and nothing is to do.
      if (this.clipLevel == 0)
        return;

      // Only at the clipLevel the clipping can be reset.
      if (this.clipLevel != this.gfxState.Level)
        throw new NotImplementedException("Cannot reset clip region in an inner graphic state level.");

      // Must be in graphical mode before popping the graphics state.
      BeginGraphic();

      // Save InternalGraphicsState and transformation of the current graphical state.
      InternalGraphicsState state = this.gfxState.InternalState;
      XMatrix ctm = this.gfxState.Transform;
      // Empty clip path by switching back to the previous state.
      RestoreState();
      SaveState();
      // Save internal state
      this.gfxState.InternalState = state;
      // Restore CTM
      this.gfxState.Transform = ctm;
    }

    /// <summary>
    /// The nesting level of the PDF graphics state stack when the clip region was set to non empty.
    /// Because of the way PDF is made the clip region can only be reset at this level.
    /// </summary>
    int clipLevel;

    #endregion

    // --------------------------------------------------------------------------------------------

    #region Miscellaneous

    /// <summary>
    /// Writes a comment to the PDF content stream. May be useful for debugging purposes.
    /// </summary>
    public void WriteComment(string comment)
    {
      comment.Replace("\n", "\n% ");
      // TODO: Some more checks necessary?
      Append("% " + comment + "\n");
    }

    #endregion

    // --------------------------------------------------------------------------------------------

    #region Append to PDF stream

    /// <summary>
    /// Appends one or up to five Bézier curves that interpolate the arc.
    /// </summary>
    void AppendPartialArc(double x, double y, double width, double height, double startAngle, double sweepAngle, PathStart pathStart, XMatrix matrix)
    {
      // Normalize the angles
      double α = startAngle;
      if (α < 0)
        α = α + (1 + Math.Floor((Math.Abs(α) / 360))) * 360;
      else if (α > 360)
        α = α - Math.Floor(α / 360) * 360;
      Debug.Assert(α >= 0 && α <= 360);

      double β = sweepAngle;
      if (β < -360)
        β = -360;
      else if (β > 360)
        β = 360;

      if (α == 0 && β < 0)
        α = 360;
      else if (α == 360 && β > 0)
        α = 0;

      // Is it possible that the arc is small starts and ends in same quadrant?
      bool smallAngle = Math.Abs(β) <= 90;

      β = α + β;
      if (β < 0)
        β = β + (1 + Math.Floor((Math.Abs(β) / 360))) * 360;

      bool clockwise = sweepAngle > 0;
      int startQuadrant = Quatrant(α, true, clockwise);
      int endQuadrant = Quatrant(β, false, clockwise);

      if (startQuadrant == endQuadrant && smallAngle)
        AppendPartialArcQuadrant(x, y, width, height, α, β, pathStart, matrix);
      else
      {
        int currentQuadrant = startQuadrant;
        bool firstLoop = true;
        do
        {
          if (currentQuadrant == startQuadrant && firstLoop)
          {
            double ξ = currentQuadrant * 90 + (clockwise ? 90 : 0);
            AppendPartialArcQuadrant(x, y, width, height, α, ξ, pathStart, matrix);
          }
          else if (currentQuadrant == endQuadrant)
          {
            double ξ = currentQuadrant * 90 + (clockwise ? 0 : 90);
            AppendPartialArcQuadrant(x, y, width, height, ξ, β, PathStart.Ignore1st, matrix);
          }
          else
          {
            double ξ1 = currentQuadrant * 90 + (clockwise ? 0 : 90);
            double ξ2 = currentQuadrant * 90 + (clockwise ? 90 : 0);
            AppendPartialArcQuadrant(x, y, width, height, ξ1, ξ2, PathStart.Ignore1st, matrix);
          }

          // Don't stop immediately if arc is greater than 270 degrees
          if (currentQuadrant == endQuadrant && smallAngle)
            break;
          else
            smallAngle = true;

          if (clockwise)
            currentQuadrant = currentQuadrant == 3 ? 0 : currentQuadrant + 1;
          else
            currentQuadrant = currentQuadrant == 0 ? 3 : currentQuadrant - 1;

          firstLoop = false;
        } while (true);
      }
    }

    /// <summary>
    /// Gets the quadrant (0 through 3) of the specified angle. If the angle lies on an edge
    /// (0, 90, 180, etc.) the result depends on the details how the angle is used.
    /// </summary>
    int Quatrant(double φ, bool start, bool clockwise)
    {
      Debug.Assert(φ >= 0);
      if (φ > 360)
        φ = φ - Math.Floor(φ / 360) * 360;

      int quadrant = (int)(φ / 90);
      if (quadrant * 90 == φ)
      {
        if ((start && !clockwise) || (!start && clockwise))
          quadrant = quadrant == 0 ? 3 : quadrant - 1;
      }
      else
        quadrant = clockwise ? ((int)Math.Floor(φ / 90)) % 4 : (int)Math.Floor(φ / 90);
      return quadrant;
    }

    /// <summary>
    /// Appends a Bézier curve for an arc within a quadrant.
    /// </summary>
    void AppendPartialArcQuadrant(double x, double y, double width, double height, double α, double β, PathStart pathStart, XMatrix matrix)
    {
      Debug.Assert(α >= 0 && α <= 360);
      Debug.Assert(β >= 0);
      if (β > 360)
        β = β - Math.Floor(β / 360) * 360;
      Debug.Assert(Math.Abs(α - β) <= 90);

      // Scanling factor
      double δx = width / 2;
      double δy = height / 2;

      // Center of ellipse
      double x0 = x + δx;
      double y0 = y + δy;

      // We have the following quarters:
      //     |
      //   2 | 3
      // ----+-----
      //   1 | 0
      //     |
      // If the angles lie in quarter 2 or 3, their values are subtracted by 180 and the
      // resulting curve is reflected at the center. This algorithm works as expected (simply tried out).
      // There may be a mathematically more elegant solution...
      bool reflect = false;
      if (α >= 180 && β >= 180)
      {
        α -= 180;
        β -= 180;
        reflect = true;
      }

      double cosα, cosβ, sinα, sinβ;
      if (width == height)
      {
        // Circular arc needs no correction.
        α = α * Calc.Deg2Rad;
        β = β * Calc.Deg2Rad;
      }
      else
      {
        // Elliptic arc needs the angles to be adjusted such that the scaling transformation is compensated.
        α = α * Calc.Deg2Rad;
        sinα = Math.Sin(α);
        if (Math.Abs(sinα) > 1E-10)
          α = Calc.πHalf - Math.Atan(δy * Math.Cos(α) / (δx * sinα));
        β = β * Calc.Deg2Rad;
        sinβ = Math.Sin(β);
        if (Math.Abs(sinβ) > 1E-10)
          β = Calc.πHalf - Math.Atan(δy * Math.Cos(β) / (δx * sinβ));
      }

      double κ = 4 * (1 - Math.Cos((α - β) / 2)) / (3 * Math.Sin((β - α) / 2));
      sinα = Math.Sin(α);
      cosα = Math.Cos(α);
      sinβ = Math.Sin(β);
      cosβ = Math.Cos(β);

      XPoint pt1, pt2, pt3;
      if (!reflect)
      {
        // Calculation for quarter 0 and 1
        switch (pathStart)
        {
          case PathStart.MoveTo1st:
            pt1 = matrix.Transform(new XPoint(x0 + δx * cosα, y0 + δy * sinα));
            AppendFormat("{0:0.###} {1:0.###} m\n", pt1.x, pt1.y);
            break;

          case PathStart.LineTo1st:
            pt1 = matrix.Transform(new XPoint(x0 + δx * cosα, y0 + δy * sinα));
            AppendFormat("{0:0.###} {1:0.###} l\n", pt1.x, pt1.y);
            break;

          case PathStart.Ignore1st:
            break;
        }
        pt1 = matrix.Transform(new XPoint(x0 + δx * (cosα - κ * sinα), y0 + δy * (sinα + κ * cosα)));
        pt2 = matrix.Transform(new XPoint(x0 + δx * (cosβ + κ * sinβ), y0 + δy * (sinβ - κ * cosβ)));
        pt3 = matrix.Transform(new XPoint(x0 + δx * cosβ, y0 + δy * sinβ));
        AppendFormat("{0:0.###} {1:0.###} {2:0.###} {3:0.###} {4:0.###} {5:0.###} c\n",
          pt1.x, pt1.y, pt2.x, pt2.y, pt3.x, pt3.y);
      }
      else
      {
        // Calculation for quarter 2 and 3
        switch (pathStart)
        {
          case PathStart.MoveTo1st:
            pt1 = matrix.Transform(new XPoint(x0 - δx * cosα, y0 - δy * sinα));
            AppendFormat("{0:0.###} {1:0.###} m\n", pt1.x, pt1.y);
            break;

          case PathStart.LineTo1st:
            pt1 = matrix.Transform(new XPoint(x0 - δx * cosα, y0 - δy * sinα));
            AppendFormat("{0:0.###} {1:0.###} l\n", pt1.x, pt1.y);
            break;

          case PathStart.Ignore1st:
            break;
        }
        pt1 = matrix.Transform(new XPoint(x0 - δx * (cosα - κ * sinα), y0 - δy * (sinα + κ * cosα)));
        pt2 = matrix.Transform(new XPoint(x0 - δx * (cosβ + κ * sinβ), y0 - δy * (sinβ - κ * cosβ)));
        pt3 = matrix.Transform(new XPoint(x0 - δx * cosβ, y0 - δy * sinβ));
        AppendFormat("{0:0.###} {1:0.###} {2:0.###} {3:0.###} {4:0.###} {5:0.###} c\n",
          pt1.x, pt1.y, pt2.x, pt2.y, pt3.x, pt3.y);
      }
    }

#if WPF
    void AppendPartialArc(System.Windows.Point point1, System.Windows.Point point2, double rotationAngle,
      System.Windows.Size size, bool isLargeArc, System.Windows.Media.SweepDirection sweepDirection, PathStart pathStart)
    {
#if true
      //AppendPartialArc(currentPoint, seg.Point, seg.RotationAngle, seg.Size, seg.IsLargeArc, seg.SweepDirection, PathStart.Ignore1st);

      int pieces;
      PointCollection points = GeometryHelper.ArcToBezier(point1.X, point1.Y, size.Width, size.Height, rotationAngle, isLargeArc, 
        sweepDirection == SweepDirection.Clockwise, point2.X, point2.Y, out pieces);

      int count = points.Count;
      int start = count % 3 == 1 ? 1 : 0;
      if (start == 1)
        AppendFormat("{0:0.####} {1:0.####} m\n", points[0].X, points[0].Y);
      for (int idx = start; idx < count; idx += 3)
        AppendFormat("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n",
          points[idx].X, points[idx].Y,
          points[idx + 1].X, points[idx + 1].Y,
          points[idx + 2].X, points[idx + 2].Y);

#else
      List<XPoint> points = GeometryHelper.BezierCurveFromArc((XPoint)point1, (XPoint)point2, rotationAngle, (XSize)size,
        isLargeArc, sweepDirection == SweepDirection.Clockwise, pathStart);
      int count = points.Count;
      int start = count % 3 == 1 ? 1 : 0;
      if (start == 1)
        AppendFormat("{0:0.####} {1:0.####} m\n", points[0].X, points[0].Y);
      for (int idx = start; idx < count; idx += 3)
        AppendFormat("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n",
          points[idx].X, points[idx].Y,
          points[idx + 1].X, points[idx + 1].Y,
          points[idx + 2].X, points[idx + 2].Y);
#endif
    }
#endif

    /// <summary>
    /// Appends a Bézier curve for a cardinal spline through pt1 and pt2.
    /// </summary>
    void AppendCurveSegment(XPoint pt0, XPoint pt1, XPoint pt2, XPoint pt3, double tension3)
    {
      AppendFormat("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n",
        pt1.X + tension3 * (pt2.X - pt0.X), pt1.Y + tension3 * (pt2.Y - pt0.Y),
        pt2.X - tension3 * (pt3.X - pt1.X), pt2.Y - tension3 * (pt3.Y - pt1.Y),
        pt2.X, pt2.Y);
    }

#if GDI
    /// <summary>
    /// Appends the content of a GraphicsPath object.
    /// </summary>
    internal void AppendPath(GraphicsPath path)
    {
      int count = path.PointCount;
      if (count == 0)
        return;
      PointF[] points = path.PathPoints;
      Byte[] types = path.PathTypes;

      for (int idx = 0; idx < count; idx++)
      {
        // From GDI+ documentation:
        const byte PathPointTypeStart = 0; // move
        const byte PathPointTypeLine = 1; // line
        const byte PathPointTypeBezier = 3; // default Bezier (= cubic Bezier)
        const byte PathPointTypePathTypeMask = 0x07; // type mask (lowest 3 bits).
        //const byte PathPointTypeDashMode = 0x10; // currently in dash mode.
        //const byte PathPointTypePathMarker = 0x20; // a marker for the path.
        const byte PathPointTypeCloseSubpath = 0x80; // closed flag

        byte type = types[idx];
        switch (type & PathPointTypePathTypeMask)
        {
          case PathPointTypeStart:
            //PDF_moveto(pdf, points[idx].X, points[idx].Y);
            AppendFormat("{0:0.####} {1:0.####} m\n", points[idx].X, points[idx].Y);
            break;

          case PathPointTypeLine:
            //PDF_lineto(pdf, points[idx].X, points[idx].Y);
            AppendFormat("{0:0.####} {1:0.####} l\n", points[idx].X, points[idx].Y);
            if ((type & PathPointTypeCloseSubpath) != 0)
              Append("h\n");
            break;

          case PathPointTypeBezier:
            Debug.Assert(idx + 2 < count);
            //PDF_curveto(pdf, points[idx].X, points[idx].Y, 
            //                 points[idx + 1].X, points[idx + 1].Y, 
            //                 points[idx + 2].X, points[idx + 2].Y);
            AppendFormat("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n", points[idx].X, points[idx].Y,
              points[++idx].X, points[idx].Y, points[++idx].X, points[idx].Y);
            if ((types[idx] & PathPointTypeCloseSubpath) != 0)
              Append("h\n");
            break;
        }
      }
    }
#endif

#if WPF
    /// <summary>
    /// Appends the content of a PathGeometry object.
    /// </summary>
    internal void AppendPath(PathGeometry geometry)
    {
#if true_
      // sissy version
      geometry = geometry.GetFlattenedPathGeometry();
#endif
      foreach (PathFigure figure in geometry.Figures)
      {
        System.Windows.Point currentPoint = new System.Windows.Point();

        // Move to start point
        currentPoint = figure.StartPoint;
        AppendFormat("{0:0.####} {1:0.####} m\n", currentPoint.X, currentPoint.Y);

        foreach (PathSegment segment in figure.Segments)
        {
          Type type = segment.GetType();
          if (type == typeof(LineSegment))
          {
            // Draw a single line
            System.Windows.Point point = ((LineSegment)segment).Point;
            currentPoint = point;
            AppendFormat("{0:0.####} {1:0.####} l\n", point.X, point.Y);
          }
          else if (type == typeof(PolyLineSegment))
          {
            // Draw connected lines
            PointCollection points = ((PolyLineSegment)segment).Points;
            foreach (System.Windows.Point point in points)
            {
              currentPoint = point; // I forced myself not to optimize this assignment
              AppendFormat("{0:0.####} {1:0.####} l\n", point.X, point.Y);
            }
          }
          else if (type == typeof(BezierSegment))
          {
            // Draw Bézier curve
            BezierSegment seg = (BezierSegment)segment;
            System.Windows.Point point1 = seg.Point1;
            System.Windows.Point point2 = seg.Point2;
            System.Windows.Point point3 = seg.Point3;
            AppendFormat("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n",
              point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y);
            currentPoint = point3;
          }
          else if (type == typeof(PolyBezierSegment))
          {
            // Draw connected Bézier curves
            PointCollection points = ((PolyBezierSegment)segment).Points;
            int count = points.Count;
            if (count > 0)
            {
              Debug.Assert(count % 3 == 0, "Number of Points in PolyBezierSegment are not a multiple of 3.");
              for (int idx = 0; idx < count - 2; idx += 3)
              {
                System.Windows.Point point1 = points[idx];
                System.Windows.Point point2 = points[idx + 1];
                System.Windows.Point point3 = points[idx + 2];
                AppendFormat("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n",
                  point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y);
              }
              currentPoint = points[count - 1];
            }
          }
          else if (type == typeof(ArcSegment))
          {
            // Draw arc
            ArcSegment seg = (ArcSegment)segment;
            AppendPartialArc(currentPoint, seg.Point, seg.RotationAngle, seg.Size, seg.IsLargeArc, seg.SweepDirection, PathStart.Ignore1st);
            currentPoint = seg.Point;
          }
          else if (type == typeof(QuadraticBezierSegment))
          {
            QuadraticBezierSegment seg = (QuadraticBezierSegment)segment;
            currentPoint = seg.Point2;
            // TODOWPF: Undone because XGraphics has no such curve type
            throw new NotImplementedException("AppendPath with QuadraticBezierSegment.");
          }
          else if (type == typeof(PolyQuadraticBezierSegment))
          {
            PolyQuadraticBezierSegment seg = (PolyQuadraticBezierSegment)segment;
            currentPoint = seg.Points[seg.Points.Count - 1];
            // TODOWPF: Undone because XGraphics has no such curve type
            throw new NotImplementedException("AppendPath with PolyQuadraticBezierSegment.");
          }
        }
        if (figure.IsClosed)
          Append("h\n");
      }
    }
#endif

    internal void Append(string value)
    {
      this.content.Append(value);
    }

    internal void AppendFormat(string format, params object[] args)
    {
      this.content.AppendFormat(CultureInfo.InvariantCulture, format, args);
    }

    void AppendStrokeFill(XPen pen, XBrush brush, XFillMode fillMode, bool closePath)
    {
      if (closePath)
        this.content.Append("h ");

      if (fillMode == XFillMode.Winding)
      {
        if (pen != null && brush != null)
          this.content.Append("B\n");
        else if (pen != null)
          this.content.Append("S\n");
        else
          this.content.Append("f\n");
      }
      else
      {
        if (pen != null && brush != null)
          this.content.Append("B*\n");
        else if (pen != null)
          this.content.Append("S\n");
        else
          this.content.Append("f*\n");
      }
    }
    #endregion

    // --------------------------------------------------------------------------------------------

    #region Realizing graphical state

    /// <summary>
    /// Initializes the default view transformation, i.e. the transformation from the user page
    /// space to the PDF page space.
    /// </summary>
    void BeginPage()
    {
      if (this.gfxState.Level == GraphicsStackLevelInitial)
      {
        // Flip page horizontaly and mirror text.
        // TODO: Is PageOriging and PageScale (== Viewport) useful? Or just public DefaultViewMatrix (like Presentation Manager has had)
        this.defaultViewMatrix = new XMatrix();  //XMatrix.Identity;
        if (this.gfx.PageDirection == XPageDirection.Downwards)
        {
#if MIGRADOC
          if (this.pdflibHack)
          {
            // MigraDoc 1.1 (written in C++) based on PDFlib and our RenderContext for GDI+. To keep it running
            // on PDFlib (and PGL) I need this hack here in the MigraDoc build of PDFsharp.
            // The new and more flexible MigraDoc Rendering (written in C#) doesn't need it anymore.
            SaveState();
            defaultViewMatrix.Translate(0, this.page.Height);
            defaultViewMatrix.Scale(1, -1);
          }
          else
#endif
          {
            // Take TrimBox into account
            double pageHeight = Size.Height;
            XPoint trimOffset = new XPoint();
            if (this.page != null && this.page.TrimMargins.AreSet)
            {
              pageHeight += this.page.TrimMargins.Top.Point + this.page.TrimMargins.Bottom.Point;
              trimOffset = new XPoint(this.page.TrimMargins.Left.Point, this.page.TrimMargins.Top.Point);
            }

            if (this.page != null && this.page.Elements.GetInteger("/Rotate") == 90)  // HACK for InDesign flyer
            {
              defaultViewMatrix.RotatePrepend(90);
              defaultViewMatrix.ScalePrepend(1, -1);
            }
            else
            {
              // Recall that the value of Height depends on Orientation.
              defaultViewMatrix.TranslatePrepend(0, pageHeight);
              defaultViewMatrix.Scale(1, -1, XMatrixOrder.Prepend);
            }

            // Scale with page units
            switch (this.gfx.PageUnit)
            {
              case XGraphicsUnit.Inch:
                defaultViewMatrix.ScalePrepend(XUnit.InchFactor);
                break;

              case XGraphicsUnit.Millimeter:
                defaultViewMatrix.ScalePrepend(XUnit.MillimeterFactor);
                break;

              case XGraphicsUnit.Centimeter:
                defaultViewMatrix.ScalePrepend(XUnit.CentimeterFactor);
                break;
            }

            if (trimOffset != new XPoint())
            {
              Debug.Assert(this.gfx.PageUnit == XGraphicsUnit.Point, "With TrimMargins set the page units must be Point. Ohter cases nyi.");
              defaultViewMatrix.TranslatePrepend(trimOffset.x, trimOffset.y);
            }

            // Save initial graphic state
            SaveState();
            // Set page transformation
            double[] cm = defaultViewMatrix.GetElements();
            AppendFormat("{0:0.###} {1:0.###} {2:0.###} {3:0.###} {4:0.###} {5:0.###} cm ",
              cm[0], cm[1], cm[2], cm[3], cm[4], cm[5]);
            AppendFormat("-100 Tz\n");
          }
        }
        else
        {
          // Scale with page units
          switch (this.gfx.PageUnit)
          {
            case XGraphicsUnit.Inch:
              defaultViewMatrix.ScalePrepend(XUnit.InchFactor);
              break;

            case XGraphicsUnit.Millimeter:
              defaultViewMatrix.ScalePrepend(XUnit.MillimeterFactor);
              break;

            case XGraphicsUnit.Centimeter:
              defaultViewMatrix.ScalePrepend(XUnit.CentimeterFactor);
              break;
          }

          // Save initial graphic state
          SaveState();
          // Set page transformation
          double[] cm = defaultViewMatrix.GetElements();
          AppendFormat("{0:0.###} {1:0.###} {2:0.###} {3:0.###} {4:0.###} {5:0.###} cm ",
            cm[0], cm[1], cm[2], cm[3], cm[4], cm[5]);
        }
      }
    }

    /// <summary>
    /// Ends the content stream, i.e. ends the text mode and balances the graphic state stack.
    /// </summary>
    void EndPage()
    {
      if (this.streamMode == StreamMode.Text)
      {
        this.content.Append("ET\n");
        this.streamMode = StreamMode.Graphic;
      }

      while (this.gfxStateStack.Count != 0)
        RestoreState();
    }

    /// <summary>
    /// Begins the graphic mode (i.e. ends the text mode).
    /// </summary>
    internal void BeginGraphic()
    {
      if (this.streamMode != StreamMode.Graphic)
      {
        if (this.streamMode == StreamMode.Text)
          this.content.Append("ET\n");

        this.streamMode = StreamMode.Graphic;
      }
    }
    StreamMode streamMode;

    /// <summary>
    /// Makes the specified pen and brush to the current graphics objects.
    /// </summary>
    void Realize(XPen pen, XBrush brush)
    {
      BeginPage();
      BeginGraphic();
      RealizeTransform();

      if (pen != null)
        this.gfxState.RealizePen(pen, this.colorMode); // this.page.document.Options.ColorMode);

      if (brush != null)
        this.gfxState.RealizeBrush(brush, this.colorMode); // this.page.document.Options.ColorMode);
    }

    /// <summary>
    /// Makes the specified pen to the current graphics object.
    /// </summary>
    void Realize(XPen pen)
    {
      Realize(pen, null);
    }

    /// <summary>
    /// Makes the specified brush to the current graphics object.
    /// </summary>
    void Realize(XBrush brush)
    {
      Realize(null, brush);
    }

    /// <summary>
    /// Makes the specified font and brush to the current graphics objects.
    /// </summary>
    void Realize(XFont font, XBrush brush, int renderMode)
    {
      BeginPage();
      RealizeTransform();

      if (this.streamMode != StreamMode.Text)
      {
        this.streamMode = StreamMode.Text;
        this.content.Append("BT\n");
        // Text matrix is empty after BT
        this.gfxState.realizedTextPosition = new XPoint();
      }
      this.gfxState.RealizeFont(font, brush, renderMode);
    }

    void AdjustTextMatrix(ref XPoint pos)
    {
      XPoint posSave = pos;
      pos = pos - new XVector(this.gfxState.realizedTextPosition.x, this.gfxState.realizedTextPosition.y);
      this.gfxState.realizedTextPosition = posSave;
    }

    /// <summary>
    /// Makes the specified image to the current graphics object.
    /// </summary>
    string Realize(XImage image)
    {
      BeginPage();
      BeginGraphic();
      RealizeTransform();

      string imageName;
      if (image is XForm)
        imageName = GetFormName(image as XForm);
      else
        imageName = GetImageName(image);
      return imageName;
    }

    /// <summary>
    /// Realizes the current transformation matrix, if necessary.
    /// </summary>
    void RealizeTransform()
    {
      BeginPage();

      if (this.gfxState.Level == GraphicsStackLevelPageSpace)
      {
        BeginGraphic();
        SaveState();
      }

      if (gfxState.MustRealizeCtm)
      {
        BeginGraphic();
        this.gfxState.RealizeCtm();
      }
    }

    #endregion

#if GDI
    [Conditional("DEBUG")]
    void DumpPathData(PathData pathData)
    {
      int count = pathData.Points.Length;
      for (int idx = 0; idx < count; idx++)
      {
        string info = PdfEncoders.Format("{0:X}   {1:####0.000} {2:####0.000}", pathData.Types[idx], pathData.Points[idx].X, pathData.Points[idx].Y);
        Debug.WriteLine(info, "PathData");
      }
    }
#endif

    /// <summary>
    /// Gets the owning PdfDocument of this page or form.
    /// </summary>
    internal PdfDocument Owner
    {
      get
      {
        if (this.page != null)
          return this.page.Owner;
        else
          return this.form.Owner;
      }
    }

    internal XGraphics Gfx
    {
      get { return this.gfx; }
    }

    /// <summary>
    /// Gets the PdfResources of this page or form.
    /// </summary>
    internal PdfResources Resources
    {
      get
      {
        if (this.page != null)
          return this.page.Resources;
        else
          return this.form.Resources;
      }
    }

    /// <summary>
    /// Gets the size of this page or form.
    /// </summary>
    internal XSize Size
    {
      get
      {
        if (this.page != null)
          return new XSize(this.page.Width, this.page.Height);
        else
          return this.form.Size;
      }
    }

    /// <summary>
    /// Gets the resource name of the specified font within this page or form.
    /// </summary>
    internal string GetFontName(XFont font, out PdfFont pdfFont)
    {
      if (this.page != null)
        return this.page.GetFontName(font, out pdfFont);
      else
        return this.form.GetFontName(font, out pdfFont);
    }

    /// <summary>
    /// Gets the resource name of the specified image within this page or form.
    /// </summary>
    internal string GetImageName(XImage image)
    {
      if (this.page != null)
        return this.page.GetImageName(image);
      else
        return this.form.GetImageName(image);
    }

    /// <summary>
    /// Gets the resource name of the specified form within this page or form.
    /// </summary>
    internal string GetFormName(XForm form)
    {
      if (this.page != null)
        return this.page.GetFormName(form);
      else
        return this.form.GetFormName(form);
    }

    internal PdfPage page;
    internal XForm form;
    internal PdfColorMode colorMode;
    XGraphicsPdfPageOptions options;
    XGraphics gfx;
    StringBuilder content;

    /// <summary>
    /// The q/Q nesting level is 0.
    /// </summary>
    const int GraphicsStackLevelInitial = 0;

    /// <summary>
    /// The q/Q nesting level is 1.
    /// </summary>
    const int GraphicsStackLevelPageSpace = 1;

    /// <summary>
    /// The q/Q nesting level is 2.
    /// </summary>
    const int GraphicsStackLevelWorldSpace = 2;

    #region PDF Graphics State

    /// <summary>
    /// Saves the current graphical state.
    /// </summary>
    void SaveState()
    {
      Debug.Assert(this.streamMode == StreamMode.Graphic, "Cannot save state in text mode.");

      this.gfxStateStack.Push(this.gfxState);
      this.gfxState = this.gfxState.Clone();
      this.gfxState.Level = this.gfxStateStack.Count;
      Append("q\n");
    }

    /// <summary>
    /// Restores the previous graphical state.
    /// </summary>
    void RestoreState()
    {
      Debug.Assert(this.streamMode == StreamMode.Graphic, "Cannot restore state in text mode.");

      this.gfxState = (PdfGraphicsState)this.gfxStateStack.Pop();
      Append("Q\n");
    }

    PdfGraphicsState RestoreState(InternalGraphicsState state)
    {
      int count = 1;
      PdfGraphicsState top = (PdfGraphicsState)this.gfxStateStack.Pop();
      while (top.InternalState != state)
      {
        Append("Q\n");
        count++;
        top = (PdfGraphicsState)this.gfxStateStack.Pop();
      }
      Append("Q\n");
      this.gfxState = top;
      return top;
    }

    /// <summary>
    /// The current graphical state.
    /// </summary>
    PdfGraphicsState gfxState;

    /// <summary>
    /// The graphical state stack.
    /// </summary>
    Stack<PdfGraphicsState> gfxStateStack = new Stack<PdfGraphicsState>();

    #endregion

    /// <summary>
    /// The final transformation from the world space to the default page space.
    /// </summary>
    internal XMatrix defaultViewMatrix;
  }
}