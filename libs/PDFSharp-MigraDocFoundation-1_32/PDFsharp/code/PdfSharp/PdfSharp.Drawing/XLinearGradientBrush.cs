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
using System.ComponentModel;
using System.IO;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp;
using PdfSharp.Internal;

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Defines a Brush with a linear gradient.
  /// </summary>
  public sealed class XLinearGradientBrush : XBrush
  {
    //internal XLinearGradientBrush();

#if GDI
    /// <summary>
    /// Initializes a new instance of the <see cref="XLinearGradientBrush"/> class.
    /// </summary>
    public XLinearGradientBrush(System.Drawing.Point point1, System.Drawing.Point point2, XColor color1, XColor color2)
      : this(new XPoint(point1), new XPoint(point2), color1, color2)
    {
    }
#endif

#if WPF
    /// <summary>
    /// Initializes a new instance of the <see cref="XLinearGradientBrush"/> class.
    /// </summary>
    public XLinearGradientBrush(System.Windows.Point point1, System.Windows.Point point2, XColor color1, XColor color2)
      : this(new XPoint(point1), new XPoint(point2), color1, color2)
    {
    }
#endif

#if GDI
    /// <summary>
    /// Initializes a new instance of the <see cref="XLinearGradientBrush"/> class.
    /// </summary>
    public XLinearGradientBrush(PointF point1, PointF point2, XColor color1, XColor color2)
      : this(new XPoint(point1), new XPoint(point2), color1, color2)
    {
    }
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="XLinearGradientBrush"/> class.
    /// </summary>
    public XLinearGradientBrush(XPoint point1, XPoint point2, XColor color1, XColor color2)
    {
      this.point1 = point1;
      this.point2 = point2;
      this.color1 = color1;
      this.color2 = color2;
    }

#if GDI
    /// <summary>
    /// Initializes a new instance of the <see cref="XLinearGradientBrush"/> class.
    /// </summary>
    public XLinearGradientBrush(Rectangle rect, XColor color1, XColor color2, XLinearGradientMode linearGradientMode)
      : this(new XRect(rect), color1, color2, linearGradientMode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XLinearGradientBrush"/> class.
    /// </summary>
    public XLinearGradientBrush(RectangleF rect, XColor color1, XColor color2, XLinearGradientMode linearGradientMode)
      : this(new XRect(rect), color1, color2, linearGradientMode)
    {
    }
#endif

#if WPF
    /// <summary>
    /// Initializes a new instance of the <see cref="XLinearGradientBrush"/> class.
    /// </summary>
    public XLinearGradientBrush(Rect rect, XColor color1, XColor color2, XLinearGradientMode linearGradientMode)
      : this(new XRect(rect), color1, color2, linearGradientMode)
    {
    }
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="XLinearGradientBrush"/> class.
    /// </summary>
    public XLinearGradientBrush(XRect rect, XColor color1, XColor color2, XLinearGradientMode linearGradientMode)
    {
      if (!Enum.IsDefined(typeof(XLinearGradientMode), linearGradientMode))
        throw new InvalidEnumArgumentException("linearGradientMode", (int)linearGradientMode, typeof(XLinearGradientMode));

      if (rect.Width == 0 || rect.Height == 0)
        throw new ArgumentException("Invalid rectangle.", "rect");

      this.useRect = true;
      this.color1 = color1;
      this.color2 = color2;
      this.rect = rect;
      this.linearGradientMode = linearGradientMode;
    }

    // TODO: 
    //public XLinearGradientBrush(Rectangle rect, XColor color1, XColor color2, double angle);
    //public XLinearGradientBrush(RectangleF rect, XColor color1, XColor color2, double angle);
    //public XLinearGradientBrush(Rectangle rect, XColor color1, XColor color2, double angle, bool isAngleScaleable);
    //public XLinearGradientBrush(RectangleF rect, XColor color1, XColor color2, double angle, bool isAngleScaleable);
    //public XLinearGradientBrush(RectangleF rect, XColor color1, XColor color2, double angle, bool isAngleScaleable);

    //private Blend _GetBlend();
    //private ColorBlend _GetInterpolationColors();
    //private XColor[] _GetLinearColors();
    //private RectangleF _GetRectangle();
    //private Matrix _GetTransform();
    //private WrapMode _GetWrapMode();
    //private void _SetBlend(Blend blend);
    //private void _SetInterpolationColors(ColorBlend blend);
    //private void _SetLinearColors(XColor color1, XColor color2);
    //private void _SetTransform(Matrix matrix);
    //private void _SetWrapMode(WrapMode wrapMode);

    //public override object Clone();

    /// <summary>
    /// Gets or sets an XMatrix that defines a local geometric transform for this LinearGradientBrush.
    /// </summary>
    public XMatrix Transform
    {
      get { return this.matrix; }
      set { this.matrix = value; }
    }

    /// <summary>
    /// Translates the brush with the specified offset.
    /// </summary>
    public void TranslateTransform(double dx, double dy)
    {
      this.matrix.TranslatePrepend(dx, dy);
    }

    /// <summary>
    /// Translates the brush with the specified offset.
    /// </summary>
    public void TranslateTransform(double dx, double dy, XMatrixOrder order)
    {
      this.matrix.Translate(dx, dy, order);
    }

    /// <summary>
    /// Scales the brush with the specified scalars.
    /// </summary>
    public void ScaleTransform(double sx, double sy)
    {
      this.matrix.ScalePrepend(sx, sy);
    }

    /// <summary>
    /// Scales the brush with the specified scalars.
    /// </summary>
    public void ScaleTransform(double sx, double sy, XMatrixOrder order)
    {
      this.matrix.Scale(sx, sy, order);
    }

    /// <summary>
    /// Rotates the brush with the specified angle.
    /// </summary>
    public void RotateTransform(double angle)
    {
      this.matrix.RotatePrepend(angle);
    }

    /// <summary>
    /// Rotates the brush with the specified angle.
    /// </summary>
    public void RotateTransform(double angle, XMatrixOrder order)
    {
      this.matrix.Rotate(angle, order);
    }

    /// <summary>
    /// Multiply the brush transformation matrix with the specified matrix.
    /// </summary>
    public void MultiplyTransform(XMatrix matrix)
    {
      this.matrix.Prepend(matrix);
    }

    /// <summary>
    /// Multiply the brush transformation matrix with the specified matrix.
    /// </summary>
    public void MultiplyTransform(XMatrix matrix, XMatrixOrder order)
    {
      this.matrix.Multiply(matrix, order);
    }

    /// <summary>
    /// Resets the brush transformation matrix with identity matrix.
    /// </summary>
    public void ResetTransform()
    {
      this.matrix = new XMatrix();  //XMatrix.Identity;
    }

    //public void SetBlendTriangularShape(double focus);
    //public void SetBlendTriangularShape(double focus, double scale);
    //public void SetSigmaBellShape(double focus);
    //public void SetSigmaBellShape(double focus, double scale);

#if GDI
    internal override System.Drawing.Brush RealizeGdiBrush()
    {
      //if (this.dirty)
      //{
      //  if (this.brush == null)
      //    this.brush = new SolidBrush(this.color.ToGdiColor());
      //  else
      //  {
      //    this.brush.Color = this.color.ToGdiColor();
      //  }
      //  this.dirty = false;
      //}

      // TODO: use this.dirty to optimize code
      System.Drawing.Drawing2D.LinearGradientBrush brush;
      if (this.useRect)
      {
        brush = new System.Drawing.Drawing2D.LinearGradientBrush(this.rect.ToRectangleF(),
          this.color1.ToGdiColor(), this.color2.ToGdiColor(), (LinearGradientMode)this.linearGradientMode);
      }
      else
      {
        brush = new System.Drawing.Drawing2D.LinearGradientBrush(
          this.point1.ToPointF(), this.point2.ToPointF(),
          this.color1.ToGdiColor(), this.color2.ToGdiColor());
      }
      if (!this.matrix.IsIdentity)
        brush.Transform = this.matrix.ToGdiMatrix();
      //brush.WrapMode = WrapMode.Clamp;
      return brush;  //this.brush;
    }
#endif

#if WPF
    internal override System.Windows.Media.Brush RealizeWpfBrush()
    {
      //if (this.dirty)
      //{
      //  if (this.brush == null)
      //    this.brush = new SolidBrush(this.color.ToGdiColor());
      //  else
      //  {
      //    this.brush.Color = this.color.ToGdiColor();
      //  }
      //  this.dirty = false;
      //}

      System.Windows.Media.LinearGradientBrush brush;
      if (this.useRect)
      {
#if !SILVERLIGHT
        brush = new System.Windows.Media.LinearGradientBrush(this.color1.ToWpfColor(), this.color2.ToWpfColor(), new System.Windows.Point(0, 0), new System.Windows.Point(1,1));// this.rect.TopLeft, this.rect.BottomRight);
        //brush = new System.Drawing.Drawing2D.LinearGradientBrush(this.rect.ToRectangleF(),
        //  this.color1.ToGdiColor(), this.color2.ToGdiColor(), (LinearGradientMode)this.linearGradientMode);
#else
        GradientStop gs1 = new GradientStop();
        gs1.Color = this.color1.ToWpfColor();
        gs1.Offset = 0;

        GradientStop gs2 = new GradientStop();
        gs2.Color = this.color2.ToWpfColor();
        gs2.Offset = 1;

        GradientStopCollection gsc = new GradientStopCollection();
        gsc.Add(gs1);
        gsc.Add(gs2);

        brush = new LinearGradientBrush(gsc, 0);
        brush.StartPoint = new Point(0, 0);
        brush.EndPoint = new Point(1, 1);
#endif
      }
      else
      {
#if !SILVERLIGHT
        brush = new System.Windows.Media.LinearGradientBrush(this.color1.ToWpfColor(), this.color2.ToWpfColor(), this.point1, this.point2);
        //brush = new System.Drawing.Drawing2D.LinearGradientBrush(
        //  this.point1.ToPointF(), this.point2.ToPointF(),
        //  this.color1.ToGdiColor(), this.color2.ToGdiColor());
#else
        GradientStop gs1 = new GradientStop();
        gs1.Color = this.color1.ToWpfColor();
        gs1.Offset = 0;

        GradientStop gs2 = new GradientStop();
        gs2.Color = this.color2.ToWpfColor();
        gs2.Offset = 1;

        GradientStopCollection gsc = new GradientStopCollection();
        gsc.Add(gs1);
        gsc.Add(gs2);

        brush = new LinearGradientBrush(gsc, 0);
        brush.StartPoint = this.point1;
        brush.EndPoint = this.point2;
#endif
      }
      if (!this.matrix.IsIdentity)
      {
#if !SILVERLIGHT
        brush.Transform = new MatrixTransform(this.matrix.ToWpfMatrix());
#else
        MatrixTransform transform = new MatrixTransform();
        transform.Matrix = this.matrix.ToWpfMatrix();
        brush.Transform = transform;
#endif
      }
      return brush;  //this.brush;
    }
#endif

    //public Blend Blend { get; set; }
    //public bool GammaCorrection { get; set; }
    //public ColorBlend InterpolationColors { get; set; }
    //public XColor[] LinearColors { get; set; }
    //public RectangleF Rectangle { get; }
    //public WrapMode WrapMode { get; set; }
    //private bool interpolationColorsWasSet;

    internal bool useRect;
    internal XPoint point1, point2;
    internal XColor color1, color2;
    internal XRect rect;
    internal XLinearGradientMode linearGradientMode;

    internal XMatrix matrix;
    //bool dirty = true;
    //LinearGradientBrush brush;
  }
}