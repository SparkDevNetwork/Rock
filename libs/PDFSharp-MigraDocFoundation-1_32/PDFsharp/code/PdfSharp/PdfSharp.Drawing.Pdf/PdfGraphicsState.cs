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
using System.Text;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
#endif
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Drawing.Pdf
{
  // TODO: update the following text
  //
  // In PDF the current transformation matrix (CTM) can only be modified, but not set. The XGraphics
  // object allows to set the transformation matrix, which leads to a problem. In PDF the only way
  // to reset the CTM to its original value is to save and restore the PDF graphics state. Don't try
  // to keep track of every modification and then reset the CTM by multiplying with the inverse matrix
  // of the product of all modifications. PDFlib uses this 'trick', but it does not work. Because of
  // rounding errors everything on the PDF page looks sloping after some resets. Saving and restoring
  // the graphics state is the only possible way to reset the CTM, but because the PDF restore operator
  // 'Q' resets not only the CTM but all other graphics state values, we have to implement our own 
  // graphics state management. This is apparently the only safe way to give the XGrahics users the 
  // illusion that they can arbitrarily set the transformation matrix.
  // 
  // The current implementation is just a draft. Save/Restore works only once and clipping is not
  // correctly restored in some cases.

  /// <summary>
  /// Represents the current PDF graphics state.
  /// </summary>
  internal sealed class PdfGraphicsState : ICloneable
  {
    public PdfGraphicsState(XGraphicsPdfRenderer renderer)
    {
      this.renderer = renderer;
    }
    readonly XGraphicsPdfRenderer renderer;

    public PdfGraphicsState Clone()
    {
      PdfGraphicsState state = (PdfGraphicsState)MemberwiseClone();
      return state;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    internal int Level;

    internal InternalGraphicsState InternalState;

    public void PushState()
    {
      //BeginGraphic
      this.renderer.Append("q/n");
    }

    public void PopState()
    {
      //BeginGraphic
      this.renderer.Append("Q/n");
    }

    #region Stroke

    double realizedLineWith = -1;
    int realizedLineCap = -1;
    int realizedLineJoin = -1;
    double realizedMiterLimit = -1;
    XDashStyle realizedDashStyle = (XDashStyle)(-1);
    string realizedDashPattern;
    XColor realizedStrokeColor = XColor.Empty;

    public void RealizePen(XPen pen, PdfColorMode colorMode)
    {
      XColor color = pen.Color;
      color = ColorSpaceHelper.EnsureColorMode(colorMode, color);

      if (this.realizedLineWith != pen.width)
      {
        this.renderer.AppendFormat("{0:0.###} w\n", pen.width);
        this.realizedLineWith = pen.width;
      }

      if (this.realizedLineCap != (int)pen.lineCap)
      {
        this.renderer.AppendFormat("{0} J\n", (int)pen.lineCap);
        this.realizedLineCap = (int)pen.lineCap;
      }

      if (this.realizedLineJoin != (int)pen.lineJoin)
      {
        this.renderer.AppendFormat("{0} j\n", (int)pen.lineJoin);
        this.realizedLineJoin = (int)pen.lineJoin;
      }

      if (this.realizedLineCap == (int)XLineJoin.Miter)
      {
        if (this.realizedMiterLimit != (int)pen.miterLimit && (int)pen.miterLimit != 0)
        {
          this.renderer.AppendFormat("{0} M\n", (int)pen.miterLimit);
          this.realizedMiterLimit = (int)pen.miterLimit;
        }
      }

      if (this.realizedDashStyle != pen.dashStyle || pen.dashStyle == XDashStyle.Custom)
      {
        double dot = pen.Width;
        double dash = 3 * dot;

        // Line width 0 is not recommended but valid
        XDashStyle dashStyle = pen.DashStyle;
        if (dot == 0)
          dashStyle = XDashStyle.Solid;

        switch (dashStyle)
        {
          case XDashStyle.Solid:
            this.renderer.Append("[]0 d\n");
            break;

          case XDashStyle.Dash:
            this.renderer.AppendFormat("[{0:0.##} {1:0.##}]0 d\n", dash, dot);
            break;

          case XDashStyle.Dot:
            this.renderer.AppendFormat("[{0:0.##}]0 d\n", dot);
            break;

          case XDashStyle.DashDot:
            this.renderer.AppendFormat("[{0:0.##} {1:0.##} {1:0.##} {1:0.##}]0 d\n", dash, dot);
            break;

          case XDashStyle.DashDotDot:
            this.renderer.AppendFormat("[{0:0.##} {1:0.##} {1:0.##} {1:0.##} {1:0.##} {1:0.##}]0 d\n", dash, dot);
            break;

          case XDashStyle.Custom:
            {
              StringBuilder pdf = new StringBuilder("[", 256);
              int len = pen.dashPattern == null ? 0 : pen.dashPattern.Length;
              for (int idx = 0; idx < len; idx++)
              {
                if (idx > 0)
                  pdf.Append(' ');
                pdf.Append(PdfEncoders.ToString(pen.dashPattern[idx] * pen.width));
              }
              // Make an even number of values look like in GDI+
              if (len > 0 && len % 2 == 1)
              {
                pdf.Append(' ');
                pdf.Append(PdfEncoders.ToString(0.2 * pen.width));
              }
              pdf.AppendFormat(CultureInfo.InvariantCulture, "]{0:0.###} d\n", pen.dashOffset * pen.width);
              string pattern = pdf.ToString();

              // BUG: drice2@ageone.de reported a realizing problem
              // HACK: I romove the if clause
              //if (this.realizedDashPattern != pattern)
              {
                this.realizedDashPattern = pattern;
                this.renderer.Append(pattern);
              }
            }
            break;
        }
        this.realizedDashStyle = dashStyle;
      }

      if (colorMode != PdfColorMode.Cmyk)
      {
        if (this.realizedStrokeColor.Rgb != color.Rgb)
        {
          this.renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Rgb));
          this.renderer.Append(" RG\n");
        }
      }
      else
      {
        if (!ColorSpaceHelper.IsEqualCmyk(this.realizedStrokeColor, color))
        {
          this.renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Cmyk));
          this.renderer.Append(" K\n");
        }
      }

      if (this.renderer.Owner.Version >= 14 && this.realizedStrokeColor.A != color.A)
      {
        PdfExtGState extGState = this.renderer.Owner.ExtGStateTable.GetExtGStateStroke(color.A);
        string gs = this.renderer.Resources.AddExtGState(extGState);
        this.renderer.AppendFormat("{0} gs\n", gs);

        // Must create transparany group
        if (this.renderer.page != null && color.A < 1)
          this.renderer.page.transparencyUsed = true;
      }
      this.realizedStrokeColor = color;
    }

    #endregion

    #region Fill

    XColor realizedFillColor = XColor.Empty;

    public void RealizeBrush(XBrush brush, PdfColorMode colorMode)
    {
      if (brush is XSolidBrush)
      {
        XColor color = ((XSolidBrush)brush).Color;
        color = ColorSpaceHelper.EnsureColorMode(colorMode, color);

        if (colorMode != PdfColorMode.Cmyk)
        {
          if (this.realizedFillColor.Rgb != color.Rgb)
          {
            this.renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Rgb));
            this.renderer.Append(" rg\n");
          }
        }
        else
        {
          if (!ColorSpaceHelper.IsEqualCmyk(this.realizedFillColor, color))
          {
            this.renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Cmyk));
            this.renderer.Append(" k\n");
          }
        }

        if (this.renderer.Owner.Version >= 14 && this.realizedFillColor.A != color.A)
        {
          PdfExtGState extGState = this.renderer.Owner.ExtGStateTable.GetExtGStateNonStroke(color.A);
          string gs = this.renderer.Resources.AddExtGState(extGState);
          this.renderer.AppendFormat("{0} gs\n", gs);

          // Must create transparany group
          if (this.renderer.page != null && color.A < 1)
            this.renderer.page.transparencyUsed = true;
        }
        this.realizedFillColor = color;
      }
      else if (brush is XLinearGradientBrush)
      {
        XMatrix matrix = this.renderer.defaultViewMatrix;
        matrix.Prepend(this.Transform);
        PdfShadingPattern pattern = new PdfShadingPattern(this.renderer.Owner);
        pattern.SetupFromBrush((XLinearGradientBrush)brush, matrix);
        string name = this.renderer.Resources.AddPattern(pattern);
        this.renderer.AppendFormat("/Pattern cs\n", name);
        this.renderer.AppendFormat("{0} scn\n", name);

        // Invalidate fill color
        this.realizedFillColor = XColor.Empty;
      }
    }
    #endregion

    #region Text

    internal PdfFont realizedFont;
    string realizedFontName = String.Empty;
    double realizedFontSize;

    public void RealizeFont(XFont font, XBrush brush, int renderMode)
    {
      // So far rendering mode 0 only
      RealizeBrush(brush, this.renderer.colorMode); // this.renderer.page.document.Options.ColorMode);

      this.realizedFont = null;
      string fontName = this.renderer.GetFontName(font, out this.realizedFont);
      if (fontName != this.realizedFontName || this.realizedFontSize != font.Size)
      {
        if (this.renderer.Gfx.PageDirection == XPageDirection.Downwards)
          this.renderer.AppendFormat("{0} {1:0.###} Tf\n", fontName, -font.Size);
        else
          this.renderer.AppendFormat("{0} {1:0.###} Tf\n", fontName, font.Size);

        this.realizedFontName = fontName;
        this.realizedFontSize = font.Size;
      }
    }

    public XPoint realizedTextPosition;

    #endregion

    #region Transformation

    /// <summary>
    /// The realized current transformation matrix.
    /// </summary>
    private XMatrix realizedCtm;

    /// <summary>
    /// The unrealized current transformation matrix.
    /// </summary>
    XMatrix unrealizedCtm;

    /// <summary>
    /// A flag indicating whether the CTM must be realized.
    /// </summary>
    public bool MustRealizeCtm;

    public XMatrix Transform
    {
      get
      {
        if (this.MustRealizeCtm)
        {
          XMatrix matrix = this.realizedCtm;
          matrix.Prepend(this.unrealizedCtm);
          return matrix;
        }
        return this.realizedCtm;
      }
      set
      {
        XMatrix matrix = this.realizedCtm;
        matrix.Invert();
        matrix.Prepend(value);
        this.unrealizedCtm = matrix;
        this.MustRealizeCtm = !this.unrealizedCtm.IsIdentity;
      }
    }

    /// <summary>
    /// Modifies the current transformation matrix.
    /// </summary>
    public void MultiplyTransform(XMatrix matrix, XMatrixOrder order)
    {
      if (!matrix.IsIdentity)
      {
        this.MustRealizeCtm = true;
        this.unrealizedCtm.Multiply(matrix, order);
      }
    }

    /// <summary>
    /// Realizes the CTM.
    /// </summary>
    public void RealizeCtm()
    {
      if (this.MustRealizeCtm)
      {
        Debug.Assert(!this.unrealizedCtm.IsIdentity, "mrCtm is unnecessarily set.");

        double[] matrix = this.unrealizedCtm.GetElements();
        // Up to six decimal digits to prevent round up problems
        this.renderer.AppendFormat("{0:0.######} {1:0.######} {2:0.######} {3:0.######} {4:0.######} {5:0.######} cm\n",
          matrix[0], matrix[1], matrix[2], matrix[3], matrix[4], matrix[5]);

        this.realizedCtm.Prepend(this.unrealizedCtm);
        this.unrealizedCtm = new XMatrix();  //XMatrix.Identity;
        this.MustRealizeCtm = false;
      }
    }

    #endregion

    #region Clip Path

    public void SetAndRealizeClipRect(XRect clipRect)
    {
      XGraphicsPath clipPath = new XGraphicsPath();
      clipPath.AddRectangle(clipRect);
      RealizeClipPath(clipPath);
    }

    public void SetAndRealizeClipPath(XGraphicsPath clipPath)
    {
      RealizeClipPath(clipPath);
    }

    void RealizeClipPath(XGraphicsPath clipPath)
    {
      this.renderer.BeginGraphic();
      RealizeCtm();
#if GDI && !WPF
      this.renderer.AppendPath(clipPath.gdipPath);
#endif
#if WPF &&!GDI
      this.renderer.AppendPath(clipPath.pathGeometry);
#endif
#if WPF && GDI
      if (this.renderer.Gfx.targetContext == XGraphicTargetContext.GDI)
      {
        this.renderer.AppendPath(clipPath.gdipPath);
      }
      else
      {
        this.renderer.AppendPath(clipPath.pathGeometry);
      }
#endif
      if (clipPath.FillMode == XFillMode.Winding)
        this.renderer.Append("W n\n");
      else
        this.renderer.Append("W* n\n");
    }

    #endregion
  }
}
