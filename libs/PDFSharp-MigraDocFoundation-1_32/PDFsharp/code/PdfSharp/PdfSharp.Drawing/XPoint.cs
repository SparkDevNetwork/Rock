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
#endif
#if WPF
using System.Windows;
#endif
using PdfSharp.Internal;

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Represents a pair of floating point x- and y-coordinates that defines a point
  /// in a two-dimensional plane.
  /// </summary>
  [DebuggerDisplay("X={X.ToString(\"0.####\",System.Globalization.CultureInfo.InvariantCulture)}, Y={Y.ToString(\"0.####\",System.Globalization.CultureInfo.InvariantCulture)}")]
  [Serializable]
  [StructLayout(LayoutKind.Sequential)] // TypeConverter(typeof(PointConverter)), ValueSerializer(typeof(PointValueSerializer))]
  public struct XPoint : IFormattable
  {
    /// <summary>
    /// Initializes a new instance of the XPoint class with the specified coordinates.
    /// </summary>
    public XPoint(double x, double y)
    {
      this.x = x;
      this.y = y;
    }

#if GDI
    /// <summary>
    /// Initializes a new instance of the XPoint class with the specified point.
    /// </summary>
    public XPoint(System.Drawing.Point point)
    {
      this.x = point.X;
      this.y = point.Y;
    }
#endif

#if WPF
    /// <summary>
    /// Initializes a new instance of the XPoint class with the specified point.
    /// </summary>
    public XPoint(System.Windows.Point point)
    {
      this.x = point.X;
      this.y = point.Y;
    }
#endif

#if GDI
    /// <summary>
    /// Initializes a new instance of the XPoint class with the specified point.
    /// </summary>
    public XPoint(PointF point)
    {
      this.x = point.X;
      this.y = point.Y;
    }
#endif

    /// <summary>
    /// Determines whether two points are equal.
    /// </summary>
    public static bool operator ==(XPoint point1, XPoint point2)
    {
      return point1.x == point2.x && point1.y == point2.y;
    }

    /// <summary>
    /// Determines whether two points are not equal.
    /// </summary>
    public static bool operator !=(XPoint point1, XPoint point2)
    {
      return !(point1 == point2);
    }

    /// <summary>
    /// Indicates whether the specified points are equal.
    /// </summary>
    public static bool Equals(XPoint point1, XPoint point2)
    {
      return point1.X.Equals(point2.X) && point1.Y.Equals(point2.Y);
    }

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    public override bool Equals(object o)
    {
      if ((o == null) || !(o is XPoint))
        return false;
      XPoint point = (XPoint)o;
      return Equals(this, point);
    }

    /// <summary>
    /// Indicates whether this instance and a specified point are equal.
    /// </summary>
    public bool Equals(XPoint value)
    {
      return Equals(this, value);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return X.GetHashCode() ^ Y.GetHashCode();
    }

    /// <summary>
    /// Parses the point from a string.
    /// </summary>
    public static XPoint Parse(string source)
    {
      CultureInfo cultureInfo = CultureInfo.InvariantCulture;
      TokenizerHelper helper = new TokenizerHelper(source, cultureInfo);
      string str = helper.NextTokenRequired();
      XPoint point = new XPoint(Convert.ToDouble(str, cultureInfo), Convert.ToDouble(helper.NextTokenRequired(), cultureInfo));
      helper.LastTokenRequired();
      return point;
    }

    /// <summary>
    /// Parses an array of points from a string.
    /// </summary>
    public static XPoint[] ParsePoints(string value)
    {
      if (value == null)
        throw new ArgumentNullException("value");
      // TODO: Reflect reliabel implementation from Avalon
      // TODOWPF
      string[] values = value.Split(' ');
      int count = values.Length;
      XPoint[] points = new XPoint[count];
      for (int idx = 0; idx < count; idx++)
        points[idx] = Parse(values[idx]);
      return points;
    }

    /// <summary>
    /// Gets the x-coordinate of this XPoint.
    /// </summary>
    public double X
    {
      get { return this.x; }
      set { this.x = value; }
    }

    /// <summary>
    /// Gets the x-coordinate of this XPoint.
    /// </summary>
    public double Y
    {
      get { return this.y; }
      set { this.y = value; }
    }

#if GDI
    /// <summary>
    /// Converts this XPoint to a System.Drawing.Point.
    /// </summary>
    public PointF ToPointF()
    {
      return new PointF((float)this.x, (float)this.y);
    }
#endif

#if WPF
    /// <summary>
    /// Converts this XPoint to a System.Windows.Point.
    /// </summary>
    public System.Windows.Point ToPoint()
    {
      return new System.Windows.Point(this.x, this.y);
    }
#endif

    /// <summary>
    /// Converts this XPoint to a human readable string.
    /// </summary>
    public override string ToString()
    {
      return this.ConvertToString(null, null);
    }

    /// <summary>
    /// Converts this XPoint to a human readable string.
    /// </summary>
    public string ToString(IFormatProvider provider)
    {
      return this.ConvertToString(null, provider);
    }

    /// <summary>
    /// Converts this XPoint to a human readable string.
    /// </summary>
    string IFormattable.ToString(string format, IFormatProvider provider)
    {
      return this.ConvertToString(format, provider);
    }

    /// <summary>
    /// Implements ToString.
    /// </summary>
    internal string ConvertToString(string format, IFormatProvider provider)
    {
      char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
      return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}", new object[] { numericListSeparator, this.x, this.y });
    }

    /// <summary>
    /// Offsets the x and y value of this point.
    /// </summary>
    public void Offset(double offsetX, double offsetY)
    {
      this.x += offsetX;
      this.y += offsetY;
    }

    /// <summary>
    /// Indicates whether this XPoint is empty.
    /// </summary>
    [Browsable(false)]
    [Obsolete("Use '== new XPoint()'")]
    public bool IsEmpty // DELETE: 09-12-31
    {
      get { return this.x == 0 && this.y == 0; }
    }

    /// <summary>
    /// Adds a point and a vector.
    /// </summary>
    public static XPoint operator +(XPoint point, XVector vector)
    {
      return new XPoint(point.x + vector.x, point.y + vector.y);
    }

    /// <summary>
    /// Adds a point and a size.
    /// </summary>
    public static XPoint operator +(XPoint point, XSize size) // TODO: make obsolete
    {
      return new XPoint(point.x + size.width, point.y + size.height);
    }

    /// <summary>
    /// Adds a point and a vector.
    /// </summary>
    public static XPoint Add(XPoint point, XVector vector)
    {
      return new XPoint(point.x + vector.x, point.y + vector.y);
    }

    /// <summary>
    /// Subtracts a vector from a point.
    /// </summary>
    public static XPoint operator -(XPoint point, XVector vector)
    {
      return new XPoint(point.x - vector.x, point.y - vector.y);
    }

    /// <summary>
    /// Subtracts a vector from a point.
    /// </summary>
    public static XPoint Subtract(XPoint point, XVector vector)
    {
      return new XPoint(point.x - vector.x, point.y - vector.y);
    }

    /// <summary>
    /// Subtracts a point from a point.
    /// </summary>
    public static XVector operator -(XPoint point1, XPoint point2)
    {
      return new XVector(point1.x - point2.x, point1.y - point2.y);
    }

    /// <summary>
    /// Subtracts a size from a point.
    /// </summary>
    [Obsolete("Use XVector instead of XSize as second parameter.")]
    public static XPoint operator -(XPoint point, XSize size) // TODO: make obsolete
    {
      return new XPoint(point.x - size.width, point.y - size.height);
    }

    /// <summary>
    /// Subtracts a point from a point.
    /// </summary>
    public static XVector Subtract(XPoint point1, XPoint point2)
    {
      return new XVector(point1.x - point2.x, point1.y - point2.y);
    }

    /// <summary>
    /// Multiplies a point with a matrix.
    /// </summary>
    public static XPoint operator *(XPoint point, XMatrix matrix)
    {
      return matrix.Transform(point);
    }

    /// <summary>
    /// Multiplies a point with a matrix.
    /// </summary>
    public static XPoint Multiply(XPoint point, XMatrix matrix)
    {
      return matrix.Transform(point);
    }

    /// <summary>
    /// Multiplies a point with a scalar value.
    /// </summary>
    public static XPoint operator *(XPoint point, double value)
    {
      return new XPoint(point.x * value, point.y * value);
    }

    /// <summary>
    /// Multiplies a point with a scalar value.
    /// </summary>
    public static XPoint operator *(double value, XPoint point)
    {
      return new XPoint(value * point.x, value * point.y);
    }

    /// <summary>
    /// Divides a point by a scalar value.
    /// </summary>
    [Obsolete("Avoid using this operator.")]
    public static XPoint operator /(XPoint point, double value)  // DELETE: 09-12-31
    {
      if (value == 0)
        throw new DivideByZeroException("Divisor is zero.");

      return new XPoint(point.x / value, point.y / value);
    }

    /// <summary>
    /// Performs an explicit conversion from XPoint to XSize.
    /// </summary>
    public static explicit operator XSize(XPoint point)
    {
      return new XSize(Math.Abs(point.x), Math.Abs(point.y));
    }

    /// <summary>
    /// Performs an explicit conversion from XPoint to XVector.
    /// </summary>
    public static explicit operator XVector(XPoint point)
    {
      return new XVector(point.x, point.y);
    }

#if WPF
    /// <summary>
    /// Performs an implicit conversion from XPoint to Point.
    /// </summary>
    public static implicit operator System.Windows.Point(XPoint point)
    {
      return new System.Windows.Point(point.x, point.y);
    }

    /// <summary>
    /// Performs an implicit conversion from Point to XPoint.
    /// </summary>
    public static implicit operator XPoint(System.Windows.Point point)
    {
      return new XPoint(point.X, point.Y);
    }
#endif

    /// <summary>
    /// For convergence with WPF use new XPoint(), not XPoint.Empty
    /// </summary>
    [Obsolete("For convergence with WPF use new XPoint(), not XPoint.Empty")]
    public static readonly XPoint Empty = new XPoint();  // DELETE: 09-12-31

    internal double x;
    internal double y;
  }
}
