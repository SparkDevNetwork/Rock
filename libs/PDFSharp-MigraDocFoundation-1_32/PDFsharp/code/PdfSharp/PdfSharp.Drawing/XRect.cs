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
using System.Windows.Media;
#endif
using PdfSharp.Internal;

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Stores a set of four floating-point numbers that represent the location and size of a rectangle.
  /// </summary>
  [DebuggerDisplay("({X}, {Y}, {Width}, {Height})")]
  [Serializable, StructLayout(LayoutKind.Sequential)] // , ValueSerializer(typeof(RectValueSerializer)), TypeConverter(typeof(RectConverter))]
  public struct XRect : IFormattable
  {
    /// <summary>
    /// Initializes a new instance of the XRect class.
    /// </summary>
    public XRect(double x, double y, double width, double height)
    {
      if (width < 0 || height < 0)
        throw new ArgumentException("WidthAndHeightCannotBeNegative"); //SR.Get(SRID.Size_WidthAndHeightCannotBeNegative, new object[0]));
      this.x = x;
      this.y = y;
      this.width = width;
      this.height = height;
    }

    /// <summary>
    /// Initializes a new instance of the XRect class.
    /// </summary>
    public XRect(XPoint point1, XPoint point2)
    {
      this.x = Math.Min(point1.x, point2.x);
      this.y = Math.Min(point1.y, point2.y);
      this.width = Math.Max(Math.Max(point1.x, point2.x) - this.x, 0);
      this.height = Math.Max(Math.Max(point1.y, point2.y) - this.y, 0);
    }

    /// <summary>
    /// Initializes a new instance of the XRect class.
    /// </summary>
    public XRect(XPoint point, XVector vector)
      : this(point, point + vector)
    { }

    /// <summary>
    /// Initializes a new instance of the XRect class.
    /// </summary>
    public XRect(XPoint location, XSize size)
    {
      if (size.IsEmpty)
        this = s_empty;
      else
      {
        this.x = location.x;
        this.y = location.y;
        this.width = size.width;
        this.height = size.height;
      }
    }

    /// <summary>
    /// Initializes a new instance of the XRect class.
    /// </summary>
    public XRect(XSize size)
    {
      if (size.IsEmpty)
        this = s_empty;
      else
      {
        this.x = this.y = 0;
        this.width = size.Width;
        this.height = size.Height;
      }
    }

#if GDI
    /// <summary>
    /// Initializes a new instance of the XRect class.
    /// </summary>
    public XRect(PointF location, SizeF size)
    {
      this.x = location.X;
      this.y = location.Y;
      this.width = size.Width;
      this.height = size.Height;
    }
#endif

#if GDI
    /// <summary>
    /// Initializes a new instance of the XRect class.
    /// </summary>
    public XRect(RectangleF rect)
    {
      this.x = rect.X;
      this.y = rect.Y;
      this.width = rect.Width;
      this.height = rect.Height;
    }
#endif

#if WPF
    /// <summary>
    /// Initializes a new instance of the XRect class.
    /// </summary>
    public XRect(Rect rect)
    {
      this.x = rect.X;
      this.y = rect.Y;
      this.width = rect.Width;
      this.height = rect.Height;
    }
#endif

    /// <summary>
    /// Creates a rectangle from for straight lines.
    /// </summary>
    public static XRect FromLTRB(double left, double top, double right, double bottom)
    {
      return new XRect(left, top, right - left, bottom - top);
    }

    /// <summary>
    /// Determines whether the two rectangles are equal.
    /// </summary>
    public static bool operator ==(XRect rect1, XRect rect2)
    {
      return rect1.X == rect2.X && rect1.Y == rect2.Y && rect1.Width == rect2.Width && rect1.Height == rect2.Height;
    }

    /// <summary>
    /// Determines whether the two rectangles are not equal.
    /// </summary>
    public static bool operator !=(XRect rect1, XRect rect2)
    {
      return !(rect1 == rect2);
    }

    /// <summary>
    /// Determines whether the two rectangles are equal.
    /// </summary>
    public static bool Equals(XRect rect1, XRect rect2)
    {
      if (rect1.IsEmpty)
        return rect2.IsEmpty;
      return rect1.X.Equals(rect2.X) && rect1.Y.Equals(rect2.Y) && rect1.Width.Equals(rect2.Width) && rect1.Height.Equals(rect2.Height);
    }

    /// <summary>
    /// Determines whether this instance and the specified object are equal.
    /// </summary>
    public override bool Equals(object o)
    {
      if ((o == null) || !(o is XRect))
        return false;
      XRect rect = (XRect)o;
      return Equals(this, rect);
    }

    /// <summary>
    /// Determines whether this instance and the specified rect are equal.
    /// </summary>
    public bool Equals(XRect value)
    {
      return Equals(this, value);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      if (IsEmpty)
        return 0;
      return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
    }

    /// <summary>
    /// Parses the rectangle from a string.
    /// </summary>
    public static XRect Parse(string source)
    {
      XRect empty;
      CultureInfo cultureInfo = CultureInfo.InvariantCulture;
      TokenizerHelper helper = new TokenizerHelper(source, cultureInfo);
      string str = helper.NextTokenRequired();
      if (str == "Empty")
        empty = Empty;
      else
        empty = new XRect(Convert.ToDouble(str, cultureInfo), Convert.ToDouble(helper.NextTokenRequired(), cultureInfo), Convert.ToDouble(helper.NextTokenRequired(), cultureInfo), Convert.ToDouble(helper.NextTokenRequired(), cultureInfo));
      helper.LastTokenRequired();
      return empty;
    }

    /// <summary>
    /// Converts this XRect to a human readable string.
    /// </summary>
    public override string ToString()
    {
      return ConvertToString(null, null);
    }

    /// <summary>
    /// Converts this XRect to a human readable string.
    /// </summary>
    public string ToString(IFormatProvider provider)
    {
      return ConvertToString(null, provider);
    }

    /// <summary>
    /// Converts this XRect to a human readable string.
    /// </summary>
    string IFormattable.ToString(string format, IFormatProvider provider)
    {
      return ConvertToString(format, provider);
    }

    internal string ConvertToString(string format, IFormatProvider provider)
    {
      if (IsEmpty)
        return "Empty";
      char numericListSeparator = TokenizerHelper.GetNumericListSeparator(provider);
      return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}", new object[] { numericListSeparator, this.x, this.y, this.width, this.height });
    }

    /// <summary>
    /// Gets the empty rectangle.
    /// </summary>
    public static XRect Empty
    {
      get { return s_empty; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    public bool IsEmpty
    {
      get { return this.width < 0; }
    }

    /// <summary>
    /// Gets or sets the location of the rectangle.
    /// </summary>
    public XPoint Location
    {
      get { return new XPoint(this.x, this.y); }
      set
      {
        if (IsEmpty)
          throw new InvalidOperationException("CannotModifyEmptyRect"); //SR.Get(SRID.Rect_CannotModifyEmptyRect, new object[0]));
        this.x = value.x;
        this.y = value.y;
      }
    }

    /// <summary>
    /// Gets or sets the size of the rectangle.
    /// </summary>
    //[Browsable(false)]
    public XSize Size
    {
      get
      {
        if (IsEmpty)
          return XSize.Empty;
        return new XSize(this.width, this.height);
      }
      set
      {
        if (value.IsEmpty)
          this = s_empty;
        else
        {
          if (IsEmpty)
            throw new InvalidOperationException("CannotModifyEmptyRect"); //SR.Get(SRID.Rect_CannotModifyEmptyRect, new object[0]));
          this.width = value.width;
          this.height = value.height;
        }
      }
    }

    /// <summary>
    /// Gets or sets the X value of the rectangle.
    /// </summary>
    public double X
    {
      get { return this.x; }
      set
      {
        if (IsEmpty)
          throw new InvalidOperationException("CannotModifyEmptyRect"); //SR.Get(SRID.Rect_CannotModifyEmptyRect, new object[0]));
        this.x = value;
      }
    }

    /// <summary>
    /// Gets or sets the Y value of the rectangle.
    /// </summary>
    public double Y
    {
      get { return this.y; }
      set
      {
        if (IsEmpty)
          throw new InvalidOperationException("CannotModifyEmptyRect"); //SR.Get(SRID.Rect_CannotModifyEmptyRect, new object[0]));
        this.y = value;
      }
    }

    /// <summary>
    /// Gets or sets the width of the rectangle.
    /// </summary>
    public double Width
    {
      get { return this.width; }
      set
      {
        if (IsEmpty)
          throw new InvalidOperationException("CannotModifyEmptyRect"); //SR.Get(SRID.Rect_CannotModifyEmptyRect, new object[0]));
        if (value < 0)
          throw new ArgumentException("WidthCannotBeNegative"); //SR.Get(SRID.Size_WidthCannotBeNegative, new object[0]));

        this.width = value;
      }
    }

    /// <summary>
    /// Gets or sets the height of the rectangle.
    /// </summary>
    public double Height
    {
      get { return this.height; }
      set
      {
        if (IsEmpty)
          throw new InvalidOperationException("CannotModifyEmptyRect"); //SR.Get(SRID.Rect_CannotModifyEmptyRect, new object[0]));
        if (value < 0)
          throw new ArgumentException("WidthCannotBeNegative"); //SR.Get(SRID.Size_WidthCannotBeNegative, new object[0]));
        this.height = value;
      }
    }

    /// <summary>
    /// Gets the x-axis value of the left side of the rectangle. 
    /// </summary>
    public double Left
    {
      get { return this.x; }
    }

    /// <summary>
    /// Gets the y-axis value of the top side of the rectangle. 
    /// </summary>
    public double Top
    {
      get { return this.y; }
    }

    /// <summary>
    /// Gets the x-axis value of the right side of the rectangle. 
    /// </summary>
    public double Right
    {
      get
      {
        if (IsEmpty)
          return double.NegativeInfinity;
        return this.x + this.width;
      }
    }

    /// <summary>
    /// Gets the y-axis value of the bottom side of the rectangle. 
    /// </summary>
    public double Bottom
    {
      get
      {
        if (IsEmpty)
          return double.NegativeInfinity;
        return this.y + this.height;
      }
    }

    /// <summary>
    /// Gets the position of the top-left corner of the rectangle. 
    /// </summary>
    public XPoint TopLeft
    {
      get { return new XPoint(Left, Top); }
    }

    /// <summary>
    /// Gets the position of the top-right corner of the rectangle. 
    /// </summary>
    public XPoint TopRight
    {
      get { return new XPoint(Right, Top); }
    }

    /// <summary>
    /// Gets the position of the bottom-left corner of the rectangle. 
    /// </summary>
    public XPoint BottomLeft
    {
      get { return new XPoint(Left, Bottom); }
    }

    /// <summary>
    /// Gets the position of the bottom-right corner of the rectangle. 
    /// </summary>
    public XPoint BottomRight
    {
      get { return new XPoint(Right, Bottom); }
    }

    /// <summary>
    /// Gets the center of the rectangle.
    /// </summary>
    //[Browsable(false)]
    public XPoint Center
    {
      get { return new XPoint(this.x + this.width / 2, this.y + this.height / 2); }
    }

    /// <summary>
    /// Indicates whether the rectangle contains the specified point. 
    /// </summary>
    public bool Contains(XPoint point)
    {
      return Contains(point.x, point.y);
    }

    /// <summary>
    /// Indicates whether the rectangle contains the specified point. 
    /// </summary>
    public bool Contains(double x, double y)
    {
      if (IsEmpty)
        return false;
      return ContainsInternal(x, y);
    }

    /// <summary>
    /// Indicates whether the rectangle contains the specified rectangle. 
    /// </summary>
    public bool Contains(XRect rect)
    {
      return !IsEmpty && !rect.IsEmpty &&
        this.x <= rect.x && this.y <= rect.y &&
        this.x + this.width >= rect.x + rect.width && this.y + this.height >= rect.y + rect.height;
    }

    /// <summary>
    /// Indicates whether the specified rectangle intersects with the current rectangle.
    /// </summary>
    public bool IntersectsWith(XRect rect)
    {
      return !IsEmpty && !rect.IsEmpty &&
          rect.Left <= Right && rect.Right >= Left &&
          rect.Top <= Bottom && rect.Bottom >= Top;
    }

    /// <summary>
    /// Sets current rectangle to the intersection of the current rectangle and the specified rectangle.
    /// </summary>
    public void Intersect(XRect rect)
    {
      if (!IntersectsWith(rect))
        this = Empty;
      else
      {
        double left = Math.Max(Left, rect.Left);
        double top = Math.Max(Top, rect.Top);
        this.width = Math.Max(Math.Min(Right, rect.Right) - left, 0.0);
        this.height = Math.Max(Math.Min(Bottom, rect.Bottom) - top, 0.0);
        this.x = left;
        this.y = top;
      }
    }

    /// <summary>
    /// Returns the intersection of two rectangles.
    /// </summary>
    public static XRect Intersect(XRect rect1, XRect rect2)
    {
      rect1.Intersect(rect2);
      return rect1;
    }

    /// <summary>
    /// Sets current rectangle to the union of the current rectangle and the specified rectangle.
    /// </summary>
    public void Union(XRect rect)
    {
      if (IsEmpty)
        this = rect;
      else if (!rect.IsEmpty)
      {
        double left = Math.Min(Left, rect.Left);
        double top = Math.Min(Top, rect.Top);
        if (rect.Width == Double.PositiveInfinity || Width == Double.PositiveInfinity)
          this.width = Double.PositiveInfinity;
        else
        {
          double right = Math.Max(Right, rect.Right);
          this.width = Math.Max(right - left, 0.0);
        }

        if (rect.Height == Double.PositiveInfinity || this.height == Double.PositiveInfinity)
          this.height = Double.PositiveInfinity;
        else
        {
          double bottom = Math.Max(Bottom, rect.Bottom);
          this.height = Math.Max(bottom - top, 0.0);
        }
        this.x = left;
        this.y = top;
      }
    }

    /// <summary>
    /// Returns the union of two rectangles.
    /// </summary>
    public static XRect Union(XRect rect1, XRect rect2)
    {
      rect1.Union(rect2);
      return rect1;
    }

    /// <summary>
    /// Sets current rectangle to the union of the current rectangle and the specified point.
    /// </summary>
    public void Union(XPoint point)
    {
      Union(new XRect(point, point));
    }

    /// <summary>
    /// Returns the intersection of a rectangle and a point.
    /// </summary>
    public static XRect Union(XRect rect, XPoint point)
    {
      rect.Union(new XRect(point, point));
      return rect;
    }

    /// <summary>
    /// Moves a rectangle by the specified amount.
    /// </summary>
    public void Offset(XVector offsetVector)
    {
      if (IsEmpty)
        throw new InvalidOperationException("CannotCallMethod"); //SR.Get(SRID.Rect_CannotCallMethod, new object[0]));
      this.x += offsetVector.x;
      this.y += offsetVector.y;
    }

    /// <summary>
    /// Moves a rectangle by the specified amount.
    /// </summary>
    public void Offset(double offsetX, double offsetY)
    {
      if (IsEmpty)
        throw new InvalidOperationException("CannotCallMethod"); //SR.Get(SRID.Rect_CannotCallMethod, new object[0]));
      this.x += offsetX;
      this.y += offsetY;
    }

    /// <summary>
    /// Returns a rectangle that is offset from the specified rectangle by using the specified vector. 
    /// </summary>
    public static XRect Offset(XRect rect, XVector offsetVector)
    {
      rect.Offset(offsetVector.X, offsetVector.Y);
      return rect;
    }

    /// <summary>
    /// Returns a rectangle that is offset from the specified rectangle by using specified horizontal and vertical amounts. 
    /// </summary>
    public static XRect Offset(XRect rect, double offsetX, double offsetY)
    {
      rect.Offset(offsetX, offsetY);
      return rect;
    }

    /// <summary>
    /// Translates the rectangle by adding the specified point.
    /// </summary>
    //[Obsolete("Use Offset.")]
    public static XRect operator +(XRect rect, XPoint point)
    {
      return new XRect(rect.x + point.x, rect.Y + point.y, rect.width, rect.height);
    }

    /// <summary>
    /// Translates the rectangle by subtracting the specified point.
    /// </summary>
    //[Obsolete("Use Offset.")]
    public static XRect operator -(XRect rect, XPoint point)
    {
      return new XRect(rect.x - point.x, rect.Y - point.y, rect.width, rect.height);
    }

    /// <summary>
    /// Expands the rectangle by using the specified Size, in all directions.
    /// </summary>
    public void Inflate(XSize size)
    {
      Inflate(size.width, size.height);
    }

    /// <summary>
    /// Expands or shrinks the rectangle by using the specified width and height amounts, in all directions.
    /// </summary>
    public void Inflate(double width, double height)
    {
      if (IsEmpty)
        throw new InvalidOperationException("CannotCallMethod"); //SR.Get(SRID.Rect_CannotCallMethod, new object[0]));
      this.x -= width;
      this.y -= height;
      this.width += width;
      this.width += width;
      this.height += height;
      this.height += height;
      if (this.width < 0 || this.height < 0)
        this = s_empty;
    }

    /// <summary>
    /// Returns the rectangle that results from expanding the specified rectangle by the specified Size, in all directions.
    /// </summary>
    public static XRect Inflate(XRect rect, XSize size)
    {
      rect.Inflate(size.width, size.height);
      return rect;
    }

    /// <summary>
    /// Creates a rectangle that results from expanding or shrinking the specified rectangle by the specified width and height amounts, in all directions.
    /// </summary>
    public static XRect Inflate(XRect rect, double width, double height)
    {
      rect.Inflate(width, height);
      return rect;
    }

    /// <summary>
    /// Returns the rectangle that results from applying the specified matrix to the specified rectangle.
    /// </summary>
    public static XRect Transform(XRect rect, XMatrix matrix)
    {
      XMatrix.MatrixHelper.TransformRect(ref rect, ref matrix);
      return rect;
    }

    /// <summary>
    /// Transforms the rectangle by applying the specified matrix.
    /// </summary>
    public void Transform(XMatrix matrix)
    {
      XMatrix.MatrixHelper.TransformRect(ref this, ref matrix);
    }

    /// <summary>
    /// Multiplies the size of the current rectangle by the specified x and y values.
    /// </summary>
    public void Scale(double scaleX, double scaleY)
    {
      if (!IsEmpty)
      {
        this.x *= scaleX;
        this.y *= scaleY;
        this.width *= scaleX;
        this.height *= scaleY;
        if (scaleX < 0)
        {
          this.x += this.width;
          this.width *= -1.0;
        }
        if (scaleY < 0)
        {
          this.y += this.height;
          this.height *= -1.0;
        }
      }
    }

#if GDI
    /// <summary>
    /// Converts this instance to a System.Drawing.RectangleF.
    /// </summary>
    public RectangleF ToRectangleF()
    {
      return new RectangleF((float)this.x, (float)this.y, (float)this.width, (float)this.height);
    }
#endif

#if GDI
    /// <summary>
    /// Performs an implicit  conversion from a System.Drawing.Rectangle to an XRect.
    /// </summary>
    public static implicit operator XRect(Rectangle rect)
    {
      return new XRect(rect.X, rect.Y, rect.Width, rect.Height);
    }

    /// <summary>
    /// Performs an implicit  conversion from a System.Drawing.RectangleF to an XRect.
    /// </summary>
    public static implicit operator XRect(RectangleF rect)
    {
      return new XRect(rect.X, rect.Y, rect.Width, rect.Height);
    }
#endif

#if WPF
    /// <summary>
    /// Performs an implicit conversion from System.Windows.Rect to XRect.
    /// </summary>
    public static implicit operator XRect(Rect rect)
    {
      return new XRect(rect.X, rect.Y, rect.Width, rect.Height);
    }
#endif

    bool ContainsInternal(double x, double y)
    {
      return x >= this.x && x - this.width <= this.x && y >= this.y && y - this.height <= this.y;
    }

    static XRect CreateEmptyRect()
    {
      XRect rect = new XRect();
      rect.x = double.PositiveInfinity;
      rect.y = double.PositiveInfinity;
      rect.width = double.NegativeInfinity;
      rect.height = double.NegativeInfinity;
      return rect;
    }

    static XRect()
    {
      s_empty = CreateEmptyRect();
    }

    internal double x;
    internal double y;
    internal double width;
    internal double height;
    private static readonly XRect s_empty;
  }
}
