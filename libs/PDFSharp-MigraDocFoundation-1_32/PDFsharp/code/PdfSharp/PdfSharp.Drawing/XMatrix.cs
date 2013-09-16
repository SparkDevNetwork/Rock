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
using System.ComponentModel;
using System.Runtime.InteropServices;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp.Internal;

// ReSharper disable RedundantNameQualifier
namespace PdfSharp.Drawing
{
  /// <summary>
  /// Represents a 3-by-3 matrix that represents an affine 2D transformation.
  /// </summary>
  [DebuggerDisplay("({M11}, {M12}, {M21}, {M22}, {OffsetX}, {OffsetY})")]
  [Serializable, StructLayout(LayoutKind.Sequential)] //, TypeConverter(typeof(MatrixConverter)), ValueSerializer(typeof(MatrixValueSerializer))]
  public struct XMatrix : IFormattable
  {
    [Flags]
    internal enum XMatrixTypes
    {
      Identity = 0,
      Translation = 1,
      Scaling = 2,
      Unknown = 4
    }

    /// <summary>
    /// Initializes a new instance of the XMatrix struct.
    /// </summary>
    public XMatrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY)
    {
      this.m11 = m11;
      this.m12 = m12;
      this.m21 = m21;
      this.m22 = m22;
      this.offsetX = offsetX;
      this.offsetY = offsetY;
      this.type = XMatrixTypes.Unknown;
      this.padding = 0;
      DeriveMatrixType();
    }

    /// <summary>
    /// Gets a value that indicates whether this matrix is an identity matrix. 
    /// </summary>
    public static XMatrix Identity
    {
      get { return s_identity; }
    }

    /// <summary>
    /// Sets this matrix into an identity matrix.
    /// </summary>
    public void SetIdentity()
    {
      this.type = XMatrixTypes.Identity;
    }

    /// <summary>
    /// Fixes a bug that XMatrixTypes.Identity is not handled correctly in some cases.
    /// </summary>
    // HACK: Fixes a bug that XMatrixTypes.Identity is not handled correctly in some cases.
    // TODO: Eliminate this function.
    void InitIdentity()
    {
      Debug.Assert(this.type == XMatrixTypes.Identity);
      this.m11 = 1;
      this.m22 = 1;
      Debug.Assert(this.m12 == 0);
      Debug.Assert(this.m21 == 0);
      Debug.Assert(this.offsetX == 0);
      Debug.Assert(this.offsetY == 0);
    }

    /// <summary>
    /// Gets a value indicating whether this matrix instance is the identity matrix.
    /// </summary>
    public bool IsIdentity
    {
      get
      {
        if (this.type == XMatrixTypes.Identity)
          return true;
        if (this.m11 == 1.0 && this.m12 == 0 && this.m21 == 0 && this.m22 == 1.0 && this.offsetX == 0 && this.offsetY == 0)
        {
          this.type = XMatrixTypes.Identity;
          return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Gets an array of double values that represents the elements of this matrix.
    /// </summary>
    [Obsolete("Use GetElements().")]
    public double[] Elements
    {
      get { return GetElements(); }
    }

    /// <summary>
    /// Gets an array of double values that represents the elements of this matrix.
    /// </summary>
    public double[] GetElements()
    {
      if (this.type == XMatrixTypes.Identity)
        return new double[] { 1, 0, 0, 1, 0, 0 };
      return new double[] { this.m11, this.m12, this.m21, this.m22, this.offsetX, this.offsetY };
    }

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    public static XMatrix operator *(XMatrix trans1, XMatrix trans2)
    {
      MatrixHelper.MultiplyMatrix(ref trans1, ref trans2);
      return trans1;
    }

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    public static XMatrix Multiply(XMatrix trans1, XMatrix trans2)
    {
      MatrixHelper.MultiplyMatrix(ref trans1, ref trans2);
      return trans1;
    }

    /// <summary>
    /// Appends the specified matrix to this matrix. 
    /// </summary>
    public void Append(XMatrix matrix)
    {
      this *= matrix;
    }

    /// <summary>
    /// Prepends the specified matrix to this matrix. 
    /// </summary>
    public void Prepend(XMatrix matrix)
    {
      this = matrix * this;
    }

    /// <summary>
    /// Appends the specified matrix to this matrix. 
    /// </summary>
    [Obsolete("Use Append.")]
    public void Multiply(XMatrix matrix)
    {
      Append(matrix);
    }

    /// <summary>
    /// Prepends the specified matrix to this matrix. 
    /// </summary>
    [Obsolete("Use Prepend.")]
    public void MultiplyPrepend(XMatrix matrix)
    {
      Prepend(matrix);
    }

    /// <summary>
    /// Multiplies this matrix with the specified matrix.
    /// </summary>
    public void Multiply(XMatrix matrix, XMatrixOrder order)
    {
      // HACK in Multiply
      if (this.type == XMatrixTypes.Identity)
        InitIdentity();

      // Must use properties, the fields can be invalid if the matrix is identity matrix.
      double t11 = M11;
      double t12 = M12;
      double t21 = M21;
      double t22 = M22;
      double tdx = OffsetX;
      double tdy = OffsetY;

      if (order == XMatrixOrder.Append)
      {
        this.m11 = t11 * matrix.M11 + t12 * matrix.M21;
        this.m12 = t11 * matrix.M12 + t12 * matrix.M22;
        this.m21 = t21 * matrix.M11 + t22 * matrix.M21;
        this.m22 = t21 * matrix.M12 + t22 * matrix.M22;
        this.offsetX = tdx * matrix.M11 + tdy * matrix.M21 + matrix.OffsetX;
        this.offsetY = tdx * matrix.M12 + tdy * matrix.M22 + matrix.OffsetY;
      }
      else
      {
        this.m11 = t11 * matrix.M11 + t21 * matrix.M12;
        this.m12 = t12 * matrix.M11 + t22 * matrix.M12;
        this.m21 = t11 * matrix.M21 + t21 * matrix.M22;
        this.m22 = t12 * matrix.M21 + t22 * matrix.M22;
        this.offsetX = t11 * matrix.OffsetX + t21 * matrix.OffsetY + tdx;
        this.offsetY = t12 * matrix.OffsetX + t22 * matrix.OffsetY + tdy;
      }
      DeriveMatrixType();
    }

    /// <summary>
    /// Appends a translation of the specified offsets to this matrix.
    /// </summary>
    [Obsolete("Use TranslateAppend or TranslatePrepend explicitly, because in GDI+ and WPF the defaults are contrary.", true)]
    public void Translate(double offsetX, double offsetY)
    {
      throw new InvalidOperationException("Temporarily out of order.");
      //if (this.type == XMatrixTypes.Identity)
      //{
      //  this.SetMatrix(1.0, 0, 0, 1.0, offsetX, offsetY, XMatrixTypes.Translation);
      //}
      //else if (this.type == XMatrixTypes.Unknown)
      //{
      //  this.offsetX += offsetX;
      //  this.offsetY += offsetY;
      //}
      //else
      //{
      //  this.offsetX += offsetX;
      //  this.offsetY += offsetY;
      //  this.type |= XMatrixTypes.Translation;
      //}
    }

    /// <summary>
    /// Appends a translation of the specified offsets to this matrix.
    /// </summary>
    public void TranslateAppend(double offsetX, double offsetY) // TODO: will become default
    {
      if (this.type == XMatrixTypes.Identity)
      {
        SetMatrix(1, 0, 0, 1, offsetX, offsetY, XMatrixTypes.Translation);
      }
      else if (this.type == XMatrixTypes.Unknown)
      {
        this.offsetX += offsetX;
        this.offsetY += offsetY;
      }
      else
      {
        this.offsetX += offsetX;
        this.offsetY += offsetY;
        this.type |= XMatrixTypes.Translation;
      }
    }

    /// <summary>
    /// Prepends a translation of the specified offsets to this matrix.
    /// </summary>
    public void TranslatePrepend(double offsetX, double offsetY)
    {
      this = CreateTranslation(offsetX, offsetY) * this;
    }

    /// <summary>
    /// Translates the matrix with the specified offsets.
    /// </summary>
    public void Translate(double offsetX, double offsetY, XMatrixOrder order)
    {
      // HACK in Translate
      if (this.type == XMatrixTypes.Identity)
        InitIdentity();

      if (order == XMatrixOrder.Append)
      {
        this.offsetX += offsetX;
        this.offsetY += offsetY;
      }
      else
      {
        this.offsetX += offsetX * this.m11 + offsetY * this.m21;
        this.offsetY += offsetX * this.m12 + offsetY * this.m22;
      }
      DeriveMatrixType();
    }

    /// <summary>
    /// Appends the specified scale vector to this matrix.
    /// </summary>
    [Obsolete("Use ScaleAppend or ScalePrepend explicitly, because in GDI+ and WPF the defaults are contrary.", true)]
    public void Scale(double scaleX, double scaleY)
    {
      throw new InvalidOperationException("Temporarily out of order.");
      //this *= CreateScaling(scaleX, scaleY);
    }

    /// <summary>
    /// Appends the specified scale vector to this matrix.
    /// </summary>
    public void ScaleAppend(double scaleX, double scaleY)  // TODO: will become default
    {
      this *= CreateScaling(scaleX, scaleY);
    }

    /// <summary>
    /// Prepends the specified scale vector to this matrix.
    /// </summary>
    public void ScalePrepend(double scaleX, double scaleY)
    {
      this = CreateScaling(scaleX, scaleY) * this;
    }

    /// <summary>
    /// Scales the matrix with the specified scalars.
    /// </summary>
    public void Scale(double scaleX, double scaleY, XMatrixOrder order)
    {
      // HACK in Scale
      if (this.type == XMatrixTypes.Identity)
        InitIdentity();

      if (order == XMatrixOrder.Append)
      {
        this.m11 *= scaleX;
        this.m12 *= scaleY;
        this.m21 *= scaleX;
        this.m22 *= scaleY;
        this.offsetX *= scaleX;
        this.offsetY *= scaleY;
      }
      else
      {
        this.m11 *= scaleX;
        this.m12 *= scaleX;
        this.m21 *= scaleY;
        this.m22 *= scaleY;
      }
      DeriveMatrixType();
    }

    /// <summary>
    /// Scales the matrix with the specified scalar.
    /// </summary>
    [Obsolete("Use ScaleAppend or ScalePrepend explicitly, because in GDI+ and WPF the defaults are contrary.", true)]
    public void Scale(double scaleXY)
    {
      throw new InvalidOperationException("Temporarily out of order.");
      //Scale(scaleXY, scaleXY, XMatrixOrder.Prepend);
    }

    /// <summary>
    /// Appends the specified scale vector to this matrix.
    /// </summary>
    public void ScaleAppend(double scaleXY)
    {
      Scale(scaleXY, scaleXY, XMatrixOrder.Append);
    }

    /// <summary>
    /// Prepends the specified scale vector to this matrix.
    /// </summary>
    public void ScalePrepend(double scaleXY)
    {
      Scale(scaleXY, scaleXY, XMatrixOrder.Prepend);
    }

    /// <summary>
    /// Scales the matrix with the specified scalar.
    /// </summary>
    public void Scale(double scaleXY, XMatrixOrder order)
    {
      Scale(scaleXY, scaleXY, order);
    }

    /// <summary>
    /// Function is obsolete.
    /// </summary>
    [Obsolete("Use ScaleAtAppend or ScaleAtPrepend explicitly, because in GDI+ and WPF the defaults are contrary.", true)]
    public void ScaleAt(double scaleX, double scaleY, double centerX, double centerY)
    {
      throw new InvalidOperationException("Temporarily out of order.");
      //this *= CreateScaling(scaleX, scaleY, centerX, centerY);
    }

    /// <summary>
    /// Apppends the specified scale about the specified point of this matrix.
    /// </summary>
    public void ScaleAtAppend(double scaleX, double scaleY, double centerX, double centerY) // TODO: will become default
    {
      this *= CreateScaling(scaleX, scaleY, centerX, centerY);
    }

    /// <summary>
    /// Prepends the specified scale about the specified point of this matrix.
    /// </summary>
    public void ScaleAtPrepend(double scaleX, double scaleY, double centerX, double centerY)
    {
      this = CreateScaling(scaleX, scaleY, centerX, centerY) * this;
    }

    /// <summary>
    /// Function is obsolete.
    /// </summary>
    [Obsolete("Use RotateAppend or RotatePrepend explicitly, because in GDI+ and WPF the defaults are contrary.", true)]
    public void Rotate(double angle)
    {
      throw new InvalidOperationException("Temporarily out of order.");
      //angle = angle % 360.0;
      //this *= CreateRotationRadians(angle * 0.017453292519943295);
    }

    /// <summary>
    /// Appends a rotation of the specified angle to this matrix.
    /// </summary>
    public void RotateAppend(double angle) // TODO: will become default Rotate
    {
      angle = angle % 360.0;
      this *= CreateRotationRadians(angle * 0.017453292519943295);
    }

    /// <summary>
    /// Prepends a rotation of the specified angle to this matrix.
    /// </summary>
    public void RotatePrepend(double angle)
    {
      angle = angle % 360.0;
      this = CreateRotationRadians(angle * 0.017453292519943295) * this;
    }

    /// <summary>
    /// Rotates the matrix with the specified angle.
    /// </summary>
    public void Rotate(double angle, XMatrixOrder order)
    {
      // HACK in Rotate
      if (this.type == XMatrixTypes.Identity)
        InitIdentity();

      angle = angle * Calc.Deg2Rad;
      double cos = Math.Cos(angle);
      double sin = Math.Sin(angle);
      if (order == XMatrixOrder.Append)
      {
        double t11 = this.m11;
        double t12 = this.m12;
        double t21 = this.m21;
        double t22 = this.m22;
        double tdx = this.offsetX;
        double tdy = this.offsetY;
        this.m11 = t11 * cos - t12 * sin;
        this.m12 = t11 * sin + t12 * cos;
        this.m21 = t21 * cos - t22 * sin;
        this.m22 = t21 * sin + t22 * cos;
        this.offsetX = tdx * cos - tdy * sin;
        this.offsetY = tdx * sin + tdy * cos;
      }
      else
      {
        double t11 = this.m11;
        double t12 = this.m12;
        double t21 = this.m21;
        double t22 = this.m22;
        this.m11 = t11 * cos + t21 * sin;
        this.m12 = t12 * cos + t22 * sin;
        this.m21 = -t11 * sin + t21 * cos;
        this.m22 = -t12 * sin + t22 * cos;
      }
      DeriveMatrixType();
    }

    /// <summary>
    /// Function is obsolete.
    /// </summary>
    [Obsolete("Use RotateAtAppend or RotateAtPrepend explicitly, because in GDI+ and WPF the defaults are contrary.", true)]
    public void RotateAt(double angle, double centerX, double centerY)
    {
      throw new InvalidOperationException("Temporarily out of order.");
      //angle = angle % 360.0;
      //this *= CreateRotationRadians(angle * 0.017453292519943295, centerX, centerY);
    }

    /// <summary>
    /// Appends a rotation of the specified angle at the specified point to this matrix.
    /// </summary>
    public void RotateAtAppend(double angle, double centerX, double centerY)  // TODO: will become default
    {
      angle = angle % 360.0;
      this *= CreateRotationRadians(angle * 0.017453292519943295, centerX, centerY);
    }

    /// <summary>
    /// Prepends a rotation of the specified angle at the specified point to this matrix.
    /// </summary>
    public void RotateAtPrepend(double angle, double centerX, double centerY)
    {
      angle = angle % 360.0;
      this = CreateRotationRadians(angle * 0.017453292519943295, centerX, centerY) * this;
    }

    /// <summary>
    /// Rotates the matrix with the specified angle at the specified point.
    /// </summary>
    [Obsolete("Use RotateAtAppend or RotateAtPrepend explicitly, because in GDI+ and WPF the defaults are contrary.", true)]
    public void RotateAt(double angle, XPoint point)
    {
      throw new InvalidOperationException("Temporarily out of order.");
      //RotateAt(angle, point, XMatrixOrder.Prepend);
    }

    /// <summary>
    /// Appends a rotation of the specified angle at the specified point to this matrix.
    /// </summary>
    public void RotateAtAppend(double angle, XPoint point)
    {
      RotateAt(angle, point, XMatrixOrder.Append);
    }

    /// <summary>
    /// Prepends a rotation of the specified angle at the specified point to this matrix.
    /// </summary>
    public void RotateAtPrepend(double angle, XPoint point)
    {
      RotateAt(angle, point, XMatrixOrder.Prepend);
    }

    /// <summary>
    /// Rotates the matrix with the specified angle at the specified point.
    /// </summary>
    public void RotateAt(double angle, XPoint point, XMatrixOrder order)
    {
      if (order == XMatrixOrder.Append)
      {
        angle = angle % 360.0;
        this *= CreateRotationRadians(angle * 0.017453292519943295, point.x, point.y);

        //this.Translate(point.X, point.Y, order);
        //this.Rotate(angle, order);
        //this.Translate(-point.X, -point.Y, order);
      }
      else
      {
        angle = angle % 360.0;
        this = CreateRotationRadians(angle * 0.017453292519943295, point.x, point.y) * this;
      }
      DeriveMatrixType();
    }

    /// <summary>
    /// Function is obsolete.
    /// </summary>
    [Obsolete("Use ShearAppend or ShearPrepend explicitly, because in GDI+ and WPF the defaults are contrary.", true)]
    public void Shear(double shearX, double shearY)
    {
      throw new InvalidOperationException("Temporarily out of order.");
      //Shear(shearX, shearY, XMatrixOrder.Prepend);
    }

    /// <summary>
    /// Appends a skew of the specified degrees in the x and y dimensions to this matrix.
    /// </summary>
    public void ShearAppend(double shearX, double shearY) // TODO: will become default
    {
      Shear(shearX, shearY, XMatrixOrder.Append);
    }

    /// <summary>
    /// Prepends a skew of the specified degrees in the x and y dimensions to this matrix.
    /// </summary>
    public void ShearPrepend(double shearX, double shearY)
    {
      Shear(shearX, shearY, XMatrixOrder.Prepend);
    }

    /// <summary>
    /// Shears the matrix with the specified scalars.
    /// </summary>
    public void Shear(double shearX, double shearY, XMatrixOrder order)
    {
      // HACK in Shear
      if (this.type == XMatrixTypes.Identity)
        InitIdentity();

      double t11 = this.m11;
      double t12 = this.m12;
      double t21 = this.m21;
      double t22 = this.m22;
      double tdx = this.offsetX;
      double tdy = this.offsetY;
      if (order == XMatrixOrder.Append)
      {
        this.m11 += shearX * t12;
        this.m12 += shearY * t11;
        this.m21 += shearX * t22;
        this.m22 += shearY * t21;
        this.offsetX += shearX * tdy;
        this.offsetY += shearY * tdx;
      }
      else
      {
        this.m11 += shearY * t21;
        this.m12 += shearY * t22;
        this.m21 += shearX * t11;
        this.m22 += shearX * t12;
      }
      DeriveMatrixType();
    }

    /// <summary>
    /// Function is obsolete.
    /// </summary>
    [Obsolete("Use SkewAppend or SkewPrepend explicitly, because in GDI+ and WPF the defaults are contrary.", true)]
    public void Skew(double skewX, double skewY)
    {
      throw new InvalidOperationException("Temporarily out of order.");
      //skewX = skewX % 360.0;
      //skewY = skewY % 360.0;
      //this *= CreateSkewRadians(skewX * 0.017453292519943295, skewY * 0.017453292519943295);
    }

    /// <summary>
    /// Appends a skew of the specified degrees in the x and y dimensions to this matrix.
    /// </summary>
    public void SkewAppend(double skewX, double skewY)
    {
      skewX = skewX % 360.0;
      skewY = skewY % 360.0;
      this *= CreateSkewRadians(skewX * 0.017453292519943295, skewY * 0.017453292519943295);
    }

    /// <summary>
    /// Prepends a skew of the specified degrees in the x and y dimensions to this matrix.
    /// </summary>
    public void SkewPrepend(double skewX, double skewY)
    {
      skewX = skewX % 360.0;
      skewY = skewY % 360.0;
      this = CreateSkewRadians(skewX * 0.017453292519943295, skewY * 0.017453292519943295) * this;
    }

    /// <summary>
    /// Transforms the specified point by this matrix and returns the result.
    /// </summary>
    public XPoint Transform(XPoint point)
    {
      XPoint point2 = point;
      MultiplyPoint(ref point2.x, ref point2.y);
      return point2;
    }

    /// <summary>
    /// Transforms the specified points by this matrix. 
    /// </summary>
    public void Transform(XPoint[] points)
    {
      if (points != null)
      {
        int count = points.Length;
        for (int idx = 0; idx < count; idx++)
          MultiplyPoint(ref points[idx].x, ref points[idx].y);
      }
    }

    /// <summary>
    /// Multiplies all points of the specified array with the this matrix.
    /// </summary>
    public void TransformPoints(XPoint[] points)
    {
      if (points == null)
        throw new ArgumentNullException("points");

      if (IsIdentity)
        return;

      int count = points.Length;
      for (int idx = 0; idx < count; idx++)
      {
        double x = points[idx].X;
        double y = points[idx].Y;
        points[idx].X = x * this.m11 + y * this.m21 + this.offsetX;
        points[idx].Y = x * this.m12 + y * this.m22 + this.offsetY;
      }
    }

#if GDI
    /// <summary>
    /// Multiplies all points of the specified array with the this matrix.
    /// </summary>
    public void TransformPoints(System.Drawing.Point[] points)
    {
      if (points == null)
        throw new ArgumentNullException("points");

      if (IsIdentity)
        return;

      int count = points.Length;
      for (int idx = 0; idx < count; idx++)
      {
        double x = points[idx].X;
        double y = points[idx].Y;
        points[idx].X = (int)(x * this.m11 + y * this.m21 + this.offsetX);
        points[idx].Y = (int)(x * this.m12 + y * this.m22 + this.offsetY);
      }
    }
#endif

#if WPF
    /// <summary>
    /// Multiplies all points of the specified array with the this matrix.
    /// </summary>
    public void TransformPoints(System.Windows.Point[] points)
    {
      if (points == null)
        throw new ArgumentNullException("points");

      if (IsIdentity)
        return;

      int count = points.Length;
      for (int idx = 0; idx < count; idx++)
      {
        double x = points[idx].X;
        double y = points[idx].Y;
        points[idx].X = (int)(x * this.m11 + y * this.m21 + this.offsetX);
        points[idx].Y = (int)(x * this.m12 + y * this.m22 + this.offsetY);
      }
    }
#endif

    /// <summary>
    /// Transforms the specified vector by this Matrix and returns the result.
    /// </summary>
    public XVector Transform(XVector vector)
    {
      XVector vector2 = vector;
      MultiplyVector(ref vector2.x, ref vector2.y);
      return vector2;
    }

    /// <summary>
    /// Transforms the specified vectors by this matrix.
    /// </summary>
    public void Transform(XVector[] vectors)
    {
      if (vectors != null)
      {
        int count = vectors.Length;
        for (int idx = 0; idx < count; idx++)
          MultiplyVector(ref vectors[idx].x, ref vectors[idx].y);
      }
    }

#if GDI
    /// <summary>
    /// Multiplies all vectors of the specified array with the this matrix. The translation elements 
    /// of this matrix (third row) are ignored.
    /// </summary>
    public void TransformVectors(PointF[] points)
    {
      if (points == null)
        throw new ArgumentNullException("points");

      if (IsIdentity)
        return;

      int count = points.Length;
      for (int idx = 0; idx < count; idx++)
      {
        double x = points[idx].X;
        double y = points[idx].Y;
        points[idx].X = (float)(x * this.m11 + y * this.m21 + this.offsetX);
        points[idx].Y = (float)(x * this.m12 + y * this.m22 + this.offsetY);
      }
    }
#endif

    /// <summary>
    /// Gets the determinant of this matrix.
    /// </summary>
    public double Determinant
    {
      get
      {
        switch (this.type)
        {
          case XMatrixTypes.Identity:
          case XMatrixTypes.Translation:
            return 1.0;

          case XMatrixTypes.Scaling:
          case XMatrixTypes.Scaling | XMatrixTypes.Translation:
            return this.m11 * this.m22;
        }
        return (this.m11 * this.m22) - (this.m12 * this.m21);
      }
    }

    /// <summary>
    /// Gets a value that indicates whether this matrix is invertible.
    /// </summary>
    public bool HasInverse
    {
      get { return !DoubleUtil.IsZero(Determinant); }
    }

    /// <summary>
    /// Inverts the matrix.
    /// </summary>
    public void Invert()
    {
      double determinant = Determinant;
      if (DoubleUtil.IsZero(determinant))
        throw new InvalidOperationException("NotInvertible"); //SR.Get(SRID.Transform_NotInvertible, new object[0]));

      switch (this.type)
      {
        case XMatrixTypes.Identity:
          break;

        case XMatrixTypes.Translation:
          this.offsetX = -this.offsetX;
          this.offsetY = -this.offsetY;
          return;

        case XMatrixTypes.Scaling:
          this.m11 = 1.0 / this.m11;
          this.m22 = 1.0 / this.m22;
          return;

        case XMatrixTypes.Scaling | XMatrixTypes.Translation:
          this.m11 = 1.0 / this.m11;
          this.m22 = 1.0 / this.m22;
          this.offsetX = -this.offsetX * this.m11;
          this.offsetY = -this.offsetY * this.m22;
          return;

        default:
          {
            double detInvers = 1.0 / determinant;
            SetMatrix(this.m22 * detInvers, -this.m12 * detInvers, -this.m21 * detInvers, this.m11 * detInvers, (this.m21 * this.offsetY - this.offsetX * this.m22) * detInvers, (this.offsetX * this.m12 - this.m11 * this.offsetY) * detInvers, XMatrixTypes.Unknown);
            break;
          }
      }
    }

    /// <summary>
    /// Gets or sets the value of the first row and first column of this matrix.
    /// </summary>
    public double M11
    {
      get
      {
        if (this.type == XMatrixTypes.Identity)
          return 1.0;
        return this.m11;
      }
      set
      {
        if (this.type == XMatrixTypes.Identity)
          SetMatrix(value, 0, 0, 1, 0, 0, XMatrixTypes.Scaling);
        else
        {
          this.m11 = value;
          if (this.type != XMatrixTypes.Unknown)
            this.type |= XMatrixTypes.Scaling;
        }
      }
    }

    /// <summary>
    /// Gets or sets the value of the first row and second column of this matrix.
    /// </summary>
    public double M12
    {
      get
      {
        if (this.type == XMatrixTypes.Identity)
          return 0;
        return this.m12;
      }
      set
      {
        if (this.type == XMatrixTypes.Identity)
          SetMatrix(1, value, 0, 1, 0, 0, XMatrixTypes.Unknown);
        else
        {
          this.m12 = value;
          this.type = XMatrixTypes.Unknown;
        }
      }
    }

    /// <summary>
    /// Gets or sets the value of the second row and first column of this matrix.
    /// </summary>
    public double M21
    {
      get
      {
        if (this.type == XMatrixTypes.Identity)
          return 0;
        return this.m21;
      }
      set
      {
        if (this.type == XMatrixTypes.Identity)
          SetMatrix(1, 0, value, 1, 0, 0, XMatrixTypes.Unknown);
        else
        {
          this.m21 = value;
          this.type = XMatrixTypes.Unknown;
        }
      }
    }

    /// <summary>
    /// Gets or sets the value of the second row and second column of this matrix.
    /// </summary>
    public double M22
    {
      get
      {
        if (this.type == XMatrixTypes.Identity)
          return 1.0;
        return this.m22;
      }
      set
      {
        if (this.type == XMatrixTypes.Identity)
          SetMatrix(1, 0, 0, value, 0, 0, XMatrixTypes.Scaling);
        else
        {
          this.m22 = value;
          if (this.type != XMatrixTypes.Unknown)
          {
            this.type |= XMatrixTypes.Scaling;
          }
        }
      }
    }

    /// <summary>
    /// Gets or sets the value of the third row and first column of this matrix.
    /// </summary>
    public double OffsetX
    {
      get
      {
        if (this.type == XMatrixTypes.Identity)
          return 0;
        return this.offsetX;
      }
      set
      {
        if (this.type == XMatrixTypes.Identity)
          SetMatrix(1, 0, 0, 1, value, 0, XMatrixTypes.Translation);
        else
        {
          this.offsetX = value;
          if (this.type != XMatrixTypes.Unknown)
            this.type |= XMatrixTypes.Translation;
        }
      }
    }

    /// <summary>
    /// Gets or sets the value of the third row and second  column of this matrix.
    /// </summary>
    public double OffsetY
    {
      get
      {
        if (this.type == XMatrixTypes.Identity)
          return 0;
        return this.offsetY;
      }
      set
      {
        if (this.type == XMatrixTypes.Identity)
          SetMatrix(1, 0, 0, 1, 0, value, XMatrixTypes.Translation);
        else
        {
          this.offsetY = value;
          if (this.type != XMatrixTypes.Unknown)
            this.type |= XMatrixTypes.Translation;
        }
      }
    }

#if GDI
    /// <summary>
    /// Converts this matrix to a System.Drawing.Drawing2D.Matrix object.
    /// </summary>
    public System.Drawing.Drawing2D.Matrix ToGdiMatrix()
    {
      return new System.Drawing.Drawing2D.Matrix((float)this.m11, (float)this.m12, (float)this.m21, (float)this.m22,
        (float)this.offsetX, (float)this.offsetY);
    }

    /// <summary>
    /// Obsolete, will be deleted.
    /// </summary>
    [Obsolete("Use ToGdiMatrix.")]
    public System.Drawing.Drawing2D.Matrix ToGdipMatrix()
    {
      return new System.Drawing.Drawing2D.Matrix((float)this.m11, (float)this.m12, (float)this.m21, (float)this.m22,
        (float)this.offsetX, (float)this.offsetY);
    }
#endif

#if WPF
    /// <summary>
    /// Converts this matrix to a System.Drawing.Drawing2D.Matrix object.
    /// </summary>
    public System.Windows.Media.Matrix ToWpfMatrix()
    {
      return new System.Windows.Media.Matrix(this.m11, this.m12, this.m21, this.m22, this.offsetX, this.offsetY);
    }
#endif

#if GDI
    /// <summary>
    /// Explicitly converts a XMatrix to a Matrix.
    /// </summary>
    public static explicit operator System.Drawing.Drawing2D.Matrix(XMatrix matrix)
    {
      if (matrix.IsIdentity)
        return new System.Drawing.Drawing2D.Matrix();

      return new System.Drawing.Drawing2D.Matrix(
        (float)matrix.m11, (float)matrix.m12,
        (float)matrix.m21, (float)matrix.m22,
        (float)matrix.offsetX, (float)matrix.offsetY);
    }
#endif

#if WPF
    /// <summary>
    /// Explicitly converts an XMatrix to a Matrix.
    /// </summary>
    public static explicit operator System.Windows.Media.Matrix(XMatrix matrix)
    {
      if (matrix.IsIdentity)
        return new System.Windows.Media.Matrix();

      return new System.Windows.Media.Matrix(
        matrix.m11, matrix.m12,
        matrix.m21, matrix.m22,
        matrix.offsetX, matrix.offsetY);
    }
#endif

#if GDI
    /// <summary>
    /// Implicitly converts a Matrix to an XMatrix.
    /// </summary>
    public static implicit operator XMatrix(System.Drawing.Drawing2D.Matrix matrix)
    {
      float[] elements = matrix.Elements;
      return new XMatrix(elements[0], elements[1], elements[2], elements[3], elements[4], elements[5]);
    }
#endif

#if WPF
    /// <summary>
    /// Implicitly converts a Matrix to an XMatrix.
    /// </summary>
    public static implicit operator XMatrix(System.Windows.Media.Matrix matrix)
    {
      return new XMatrix(matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.OffsetX, matrix.OffsetY);
    }
#endif

    /// <summary>
    /// Determines whether the two matrices are equal.
    /// </summary>
    public static bool operator ==(XMatrix matrix1, XMatrix matrix2)
    {
      if (matrix1.IsDistinguishedIdentity || matrix2.IsDistinguishedIdentity)
        return (matrix1.IsIdentity == matrix2.IsIdentity);

      return matrix1.M11 == matrix2.M11 && matrix1.M12 == matrix2.M12 && matrix1.M21 == matrix2.M21 && matrix1.M22 == matrix2.M22 &&
        matrix1.OffsetX == matrix2.OffsetX && matrix1.OffsetY == matrix2.OffsetY;
    }

    /// <summary>
    /// Determines whether the two matrices are not equal.
    /// </summary>
    public static bool operator !=(XMatrix matrix1, XMatrix matrix2)
    {
      return !(matrix1 == matrix2);
    }

    /// <summary>
    /// Determines whether the two matrices are equal.
    /// </summary>
    public static bool Equals(XMatrix matrix1, XMatrix matrix2)
    {
      if (matrix1.IsDistinguishedIdentity || matrix2.IsDistinguishedIdentity)
        return matrix1.IsIdentity == matrix2.IsIdentity;

      return matrix1.M11.Equals(matrix2.M11) && matrix1.M12.Equals(matrix2.M12) &&
        matrix1.M21.Equals(matrix2.M21) && matrix1.M22.Equals(matrix2.M22) &&
        matrix1.OffsetX.Equals(matrix2.OffsetX) && matrix1.OffsetY.Equals(matrix2.OffsetY);
    }

    /// <summary>
    /// Determines whether this matrix is equal to the specified object.
    /// </summary>
    public override bool Equals(object o)
    {
      if (o == null || !(o is XMatrix))
        return false;
      XMatrix matrix = (XMatrix)o;
      return Equals(this, matrix);
    }

    /// <summary>
    /// Determines whether this matrix is equal to the specified matrix.
    /// </summary>
    public bool Equals(XMatrix value)
    {
      return Equals(this, value);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      if (IsDistinguishedIdentity)
        return 0;
      return M11.GetHashCode() ^ M12.GetHashCode() ^ M21.GetHashCode() ^ M22.GetHashCode() ^ OffsetX.GetHashCode() ^ OffsetY.GetHashCode();
    }

    /// <summary>
    /// Parses a matrix from a string.
    /// </summary>
    public static XMatrix Parse(string source)
    {
      XMatrix identity;
      IFormatProvider cultureInfo = CultureInfo.InvariantCulture; //.GetCultureInfo("en-us");
      TokenizerHelper helper = new TokenizerHelper(source, cultureInfo);
      string str = helper.NextTokenRequired();
      identity = str == "Identity" ? Identity : 
        new XMatrix(Convert.ToDouble(str, cultureInfo), Convert.ToDouble(helper.NextTokenRequired(), cultureInfo), Convert.ToDouble(helper.NextTokenRequired(), cultureInfo), Convert.ToDouble(helper.NextTokenRequired(), cultureInfo), Convert.ToDouble(helper.NextTokenRequired(), cultureInfo), Convert.ToDouble(helper.NextTokenRequired(), cultureInfo));
      helper.LastTokenRequired();
      return identity;
    }

    /// <summary>
    /// Converts this XMatrix to a human readable string.
    /// </summary>
    public override string ToString()
    {
      return ConvertToString(null, null);
    }

    /// <summary>
    /// Converts this XMatrix to a human readable string.
    /// </summary>
    public string ToString(IFormatProvider provider)
    {
      return ConvertToString(null, provider);
    }

    /// <summary>
    /// Converts this XMatrix to a human readable string.
    /// </summary>
    string IFormattable.ToString(string format, IFormatProvider provider)
    {
      return ConvertToString(format, provider);
    }

    internal string ConvertToString(string format, IFormatProvider provider)
    {
      if (IsIdentity)
        return "Identity";

      char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
      return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}{0}{5:" + format + "}{0}{6:" + format + "}", new object[] { numericListSeparator, this.m11, this.m12, this.m21, this.m22, this.offsetX, this.offsetY });
    }

    internal void MultiplyVector(ref double x, ref double y)
    {
      switch (this.type)
      {
        case XMatrixTypes.Identity:
        case XMatrixTypes.Translation:
          return;

        case XMatrixTypes.Scaling:
        case XMatrixTypes.Scaling | XMatrixTypes.Translation:
          x *= this.m11;
          y *= this.m22;
          return;
      }
      double d1 = y * this.m21;
      double d2 = x * this.m12;
      x *= this.m11;
      x += d1;
      y *= this.m22;
      y += d2;
    }

    internal void MultiplyPoint(ref double x, ref double y)
    {
      switch (this.type)
      {
        case XMatrixTypes.Identity:
          return;

        case XMatrixTypes.Translation:
          x += this.offsetX;
          y += this.offsetY;
          return;

        case XMatrixTypes.Scaling:
          x *= this.m11;
          y *= this.m22;
          return;

        case (XMatrixTypes.Scaling | XMatrixTypes.Translation):
          x *= this.m11;
          x += this.offsetX;
          y *= this.m22;
          y += this.offsetY;
          return;
      }
      double d1 = (y * this.m21) + this.offsetX;
      double d2 = (x * this.m12) + this.offsetY;
      x *= this.m11;
      x += d1;
      y *= this.m22;
      y += d2;
    }

    internal static XMatrix CreateRotationRadians(double angle)
    {
      return CreateRotationRadians(angle, 0, 0);
    }

    internal static XMatrix CreateRotationRadians(double angle, double centerX, double centerY)
    {
      XMatrix matrix = new XMatrix();
      double sin = Math.Sin(angle);
      double cos = Math.Cos(angle);
      double offsetX = (centerX * (1.0 - cos)) + (centerY * sin);
      double offsetY = (centerY * (1.0 - cos)) - (centerX * sin);
      matrix.SetMatrix(cos, sin, -sin, cos, offsetX, offsetY, XMatrixTypes.Unknown);
      return matrix;
    }

    internal static XMatrix CreateScaling(double scaleX, double scaleY, double centerX, double centerY)
    {
      XMatrix matrix = new XMatrix();
      matrix.SetMatrix(scaleX, 0, 0, scaleY, centerX - scaleX * centerX, centerY - scaleY * centerY, XMatrixTypes.Scaling | XMatrixTypes.Translation);
      return matrix;
    }

    internal static XMatrix CreateScaling(double scaleX, double scaleY)
    {
      XMatrix matrix = new XMatrix();
      matrix.SetMatrix(scaleX, 0, 0, scaleY, 0, 0, XMatrixTypes.Scaling);
      return matrix;
    }

    internal static XMatrix CreateSkewRadians(double skewX, double skewY)
    {
      XMatrix matrix = new XMatrix();
      matrix.SetMatrix(1, Math.Tan(skewY), Math.Tan(skewX), 1, 0, 0, XMatrixTypes.Unknown);
      return matrix;
    }

    internal static XMatrix CreateTranslation(double offsetX, double offsetY)
    {
      XMatrix matrix = new XMatrix();
      matrix.SetMatrix(1, 0, 0, 1, offsetX, offsetY, XMatrixTypes.Translation);
      return matrix;
    }

    static XMatrix CreateIdentity()
    {
      XMatrix matrix = new XMatrix();
      matrix.SetMatrix(1, 0, 0, 1.0, 0, 0, XMatrixTypes.Identity);
      return matrix;
    }

    /// <summary>
    /// Sets the matrix.
    /// </summary>
    // ReSharper disable ParameterHidesMember
    void SetMatrix(double m11, double m12, double m21, double m22, double offsetX, double offsetY, XMatrixTypes type)
    // ReSharper restore ParameterHidesMember
    {
      this.m11 = m11;
      this.m12 = m12;
      this.m21 = m21;
      this.m22 = m22;
      this.offsetX = offsetX;
      this.offsetY = offsetY;
      this.type = type;
    }

    void DeriveMatrixType()
    {
      this.type = XMatrixTypes.Identity;
      if (this.m21 != 0 || this.m12 != 0)
      {
        this.type = XMatrixTypes.Unknown;
      }
      else
      {
        if (this.m11 != 1 || this.m22 != 1)
          this.type = XMatrixTypes.Scaling;

        if (this.offsetX != 0 || this.offsetY != 0)
          this.type |= XMatrixTypes.Translation;

        if ((this.type & (XMatrixTypes.Scaling | XMatrixTypes.Translation)) == XMatrixTypes.Identity)
          this.type = XMatrixTypes.Identity;
      }
    }

    private bool IsDistinguishedIdentity
    {
      get { return (this.type == XMatrixTypes.Identity); }
    }

    static XMatrix()
    {
      s_identity = CreateIdentity();
    }

    // Keep the fields private and force using the properties.
    // This prevents using m11 and m22 by mistake when the matrix is identity.
    double m11;
    double m12;
    double m21;
    double m22;
    double offsetX;
    double offsetY;
    XMatrixTypes type;
    int padding;
    static readonly XMatrix s_identity;

    /// <summary>
    /// Internal matrix helper.
    /// </summary>
    internal static class MatrixHelper
    {
      // Fast multiplication taking matrix type into account. Reflectored from WPF.
      internal static void MultiplyMatrix(ref XMatrix matrix1, ref XMatrix matrix2)
      {
        XMatrixTypes type1 = matrix1.type;
        XMatrixTypes type2 = matrix2.type;
        if (type2 != XMatrixTypes.Identity)
        {
          if (type1 == XMatrixTypes.Identity)
            matrix1 = matrix2;
          else if (type2 == XMatrixTypes.Translation)
          {
            matrix1.offsetX += matrix2.offsetX;
            matrix1.offsetY += matrix2.offsetY;
            if (type1 != XMatrixTypes.Unknown)
              matrix1.type |= XMatrixTypes.Translation;
          }
          else if (type1 == XMatrixTypes.Translation)
          {
            double num = matrix1.offsetX;
            double num2 = matrix1.offsetY;
            matrix1 = matrix2;
            matrix1.offsetX = num * matrix2.m11 + num2 * matrix2.m21 + matrix2.offsetX;
            matrix1.offsetY = num * matrix2.m12 + num2 * matrix2.m22 + matrix2.offsetY;
            if (type2 == XMatrixTypes.Unknown)
              matrix1.type = XMatrixTypes.Unknown;
            else
              matrix1.type = XMatrixTypes.Scaling | XMatrixTypes.Translation;
          }
          else
          {
            switch ((((int)type1) << 4) | (int)type2)
            {
              case 0x22:
                matrix1.m11 *= matrix2.m11;
                matrix1.m22 *= matrix2.m22;
                return;

              case 0x23:
                matrix1.m11 *= matrix2.m11;
                matrix1.m22 *= matrix2.m22;
                matrix1.offsetX = matrix2.offsetX;
                matrix1.offsetY = matrix2.offsetY;
                matrix1.type = XMatrixTypes.Scaling | XMatrixTypes.Translation;
                return;

              case 0x24:
              case 0x34:
              case 0x42:
              case 0x43:
              case 0x44:
                matrix1 = new XMatrix(
                  matrix1.m11 * matrix2.m11 + matrix1.m12 * matrix2.m21,
                  matrix1.m11 * matrix2.m12 + matrix1.m12 * matrix2.m22,
                  matrix1.m21 * matrix2.m11 + matrix1.m22 * matrix2.m21,
                  matrix1.m21 * matrix2.m12 + matrix1.m22 * matrix2.m22,
                  matrix1.offsetX * matrix2.m11 + matrix1.offsetY * matrix2.m21 + matrix2.offsetX,
                  matrix1.offsetX * matrix2.m12 + matrix1.offsetY * matrix2.m22 + matrix2.offsetY);
                return;

              case 50:
                matrix1.m11 *= matrix2.m11;
                matrix1.m22 *= matrix2.m22;
                matrix1.offsetX *= matrix2.m11;
                matrix1.offsetY *= matrix2.m22;
                return;

              case 0x33:
                matrix1.m11 *= matrix2.m11;
                matrix1.m22 *= matrix2.m22;
                matrix1.offsetX = matrix2.m11 * matrix1.offsetX + matrix2.offsetX;
                matrix1.offsetY = matrix2.m22 * matrix1.offsetY + matrix2.offsetY;
                return;
            }
          }
        }
      }

      internal static void PrependOffset(ref XMatrix matrix, double offsetX, double offsetY)
      {
        if (matrix.type == XMatrixTypes.Identity)
        {
          matrix = new XMatrix(1, 0, 0, 1, offsetX, offsetY);
          matrix.type = XMatrixTypes.Translation;
        }
        else
        {
          matrix.offsetX += (matrix.m11 * offsetX) + (matrix.m21 * offsetY);
          matrix.offsetY += (matrix.m12 * offsetX) + (matrix.m22 * offsetY);
          if (matrix.type != XMatrixTypes.Unknown)
            matrix.type |= XMatrixTypes.Translation;
        }
      }

      internal static void TransformRect(ref XRect rect, ref XMatrix matrix)
      {
        if (!rect.IsEmpty)
        {
          XMatrixTypes types = matrix.type;
          if (types != XMatrixTypes.Identity)
          {
            if ((types & XMatrixTypes.Scaling) != XMatrixTypes.Identity)
            {
              rect.x *= matrix.m11;
              rect.y *= matrix.m22;
              rect.width *= matrix.m11;
              rect.height *= matrix.m22;
              if (rect.width < 0)
              {
                rect.x += rect.width;
                rect.width = -rect.width;
              }
              if (rect.height < 0)
              {
                rect.y += rect.height;
                rect.height = -rect.height;
              }
            }
            if ((types & XMatrixTypes.Translation) != XMatrixTypes.Identity)
            {
              rect.x += matrix.offsetX;
              rect.y += matrix.offsetY;
            }
            if (types == XMatrixTypes.Unknown)
            {
              XPoint point = matrix.Transform(rect.TopLeft);
              XPoint point2 = matrix.Transform(rect.TopRight);
              XPoint point3 = matrix.Transform(rect.BottomRight);
              XPoint point4 = matrix.Transform(rect.BottomLeft);
              rect.x = Math.Min(Math.Min(point.X, point2.X), Math.Min(point3.X, point4.X));
              rect.y = Math.Min(Math.Min(point.Y, point2.Y), Math.Min(point3.Y, point4.Y));
              rect.width = Math.Max(Math.Max(point.X, point2.X), Math.Max(point3.X, point4.X)) - rect.x;
              rect.height = Math.Max(Math.Max(point.Y, point2.Y), Math.Max(point3.Y, point4.Y)) - rect.y;
            }
          }
        }
      }
    }
  }
}
