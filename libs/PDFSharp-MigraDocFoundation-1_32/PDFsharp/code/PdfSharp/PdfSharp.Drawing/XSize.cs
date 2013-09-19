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
  /// Represents a pair of floating-point numbers, typically the width and height of a
  /// graphical object.
  /// </summary>
  //[DebuggerDisplay("({Width}, {Height})")]
  [DebuggerDisplay("Width={Width.ToString(\"0.####\",System.Globalization.CultureInfo.InvariantCulture)}, Height={Height.ToString(\"0.####\",System.Globalization.CultureInfo.InvariantCulture)}")]
  [Serializable, StructLayout(LayoutKind.Sequential)] //, ValueSerializer(typeof(SizeValueSerializer)), TypeConverter(typeof(SizeConverter))]
  public struct XSize : IFormattable
  {
    /// <summary>
    /// Initializes a new instance of the XPoint class with the specified values.
    /// </summary>
    public XSize(double width, double height)
    {
      if (width < 0 || height < 0)
        throw new ArgumentException("WidthAndHeightCannotBeNegative"); //SR.Get(SRID.Size_WidthAndHeightCannotBeNegative, new object[0]));

      this.width = width;
      this.height = height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XSize"/> class.
    /// </summary>
    [Obsolete("Use explicit conversion to make your code more readable.")]
    public XSize(XPoint pt)  // DELETE: 08-12-31
    {
      this.width = pt.X;
      this.height = pt.Y;
    }

    /// <summary>
    /// Determines whether two size objects are equal.
    /// </summary>
    public static bool operator ==(XSize size1, XSize size2)
    {
      return size1.Width == size2.Width && size1.Height == size2.Height;
    }

    /// <summary>
    /// Determines whether two size objects are not equal.
    /// </summary>
    public static bool operator !=(XSize size1, XSize size2)
    {
      return !(size1 == size2);
    }

    /// <summary>
    /// Indicates whether this tow instance are equal.
    /// </summary>
    public static bool Equals(XSize size1, XSize size2)
    {
      if (size1.IsEmpty)
        return size2.IsEmpty;
      return size1.Width.Equals(size2.Width) && size1.Height.Equals(size2.Height);
    }

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    public override bool Equals(object o)
    {
      if (o == null || !(o is XSize))
        return false;
      XSize size = (XSize)o;
      return Equals(this, size);
    }

    /// <summary>
    /// Indicates whether this instance and a specified size are equal.
    /// </summary>
    public bool Equals(XSize value)
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
      return Width.GetHashCode() ^ Height.GetHashCode();
    }

    /// <summary>
    /// Parses the size from a string.
    /// </summary>
    public static XSize Parse(string source)
    {
      XSize empty;
      CultureInfo cultureInfo = CultureInfo.InvariantCulture;
      TokenizerHelper helper = new TokenizerHelper(source, cultureInfo);
      string str = helper.NextTokenRequired();
      if (str == "Empty")
        empty = Empty;
      else
        empty = new XSize(Convert.ToDouble(str, cultureInfo), Convert.ToDouble(helper.NextTokenRequired(), cultureInfo));
      helper.LastTokenRequired();
      return empty;
    }

#if GDI
    /// <summary>
    /// Converts this XSize to a PointF.
    /// </summary>
    public PointF ToPointF()
    {
      return new PointF((float)this.width, (float)this.height);
    }
#endif

    /// <summary>
    /// Converts this XSize to an XPoint.
    /// </summary>
    public XPoint ToXPoint()
    {
      return new XPoint(this.width, this.height);
    }

    /// <summary>
    /// Converts this XSize to an XVector.
    /// </summary>
    public XVector ToXVector()
    {
      return new XVector(this.width, this.height);
    }

#if GDI
    /// <summary>
    /// Converts this XSize to a SizeF.
    /// </summary>
    public SizeF ToSizeF()
    {
      return new SizeF((float)this.width, (float)this.height);
    }
#endif

#if WPF
    /// <summary>
    /// Converts this XSize to a System.Windows.Size.
    /// </summary>
    public System.Windows.Size ToSize()
    {
      return new System.Windows.Size(this.width, this.height);
    }
#endif

#if GDI
    /// <summary>
    /// Creates an XSize from a System.Drawing.Size.
    /// </summary>
    public static XSize FromSize(System.Drawing.Size size)
    {
      return new XSize(size.Width, size.Height);
    }

    /// <summary>
    /// Implicit conversion from XSize to System.Drawing.Size. The conversion must be implicit because the
    /// WinForms designer uses it.
    /// </summary>
    public static implicit operator XSize(System.Drawing.Size size)
    {
      return new XSize(size.Width, size.Height);
    }
#endif

#if WPF
    /// <summary>
    /// Creates an XSize from a System.Drawing.Size.
    /// </summary>
    public static XSize FromSize(System.Windows.Size size)
    {
      return new XSize(size.Width, size.Height);
    }
#endif

#if GDI
    /// <summary>
    /// Creates an XSize from a System.Drawing.Size.
    /// </summary>
    public static XSize FromSizeF(SizeF size)
    {
      return new XSize(size.Width, size.Height);
    }
#endif

    /// <summary>
    /// Converts this XSize to a human readable string.
    /// </summary>
    public override string ToString()
    {
      return ConvertToString(null, null);
    }

    /// <summary>
    /// Converts this XSize to a human readable string.
    /// </summary>
    public string ToString(IFormatProvider provider)
    {
      return ConvertToString(null, provider);
    }

    /// <summary>
    /// Converts this XSize to a human readable string.
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
      return string.Format(provider, "{1:" + format + "}{0}{2:" + format + "}", new object[] { numericListSeparator, this.width, this.height });
    }

    /// <summary>
    /// Returns an empty size, i.e. a size with a width or height less than 0.
    /// </summary>
    public static XSize Empty
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
    /// Gets or sets the width.
    /// </summary>
    public double Width
    {
      get { return this.width; }
      set
      {
        if (IsEmpty)
          throw new InvalidOperationException("CannotModifyEmptySize"); //SR.Get(SRID.Size_CannotModifyEmptySize, new object[0]));
        if (value < 0)
          throw new ArgumentException("WidthCannotBeNegative"); //SR.Get(SRID.Size_WidthCannotBeNegative, new object[0]));
        this.width = value;
      }
    }

    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    public double Height
    {
      get { return this.height; }
      set
      {
        if (IsEmpty)
          throw new InvalidOperationException("CannotModifyEmptySize"); // SR.Get(SRID.Size_CannotModifyEmptySize, new object[0]));
        if (value < 0)
          throw new ArgumentException("HeightCannotBeNegative"); //SR.Get(SRID.Size_HeightCannotBeNegative, new object[0]));
        this.height = value;
      }
    }

    /// <summary>
    /// Performs an explicit conversion from XSize to XVector.
    /// </summary>
    public static explicit operator XVector(XSize size)
    {
      return new XVector(size.width, size.height);
    }

    /// <summary>
    /// Performs an explicit conversion from XSize to XPoint.
    /// </summary>
    public static explicit operator XPoint(XSize size)
    {
      return new XPoint(size.width, size.height);
    }

#if WPF
    /// <summary>
    /// Performs an explicit conversion from Size to XSize.
    /// </summary>
    public static explicit operator XSize(System.Windows.Size size)
    {
      return new XSize(size.Width, size.Height);
    }
#endif

    private static XSize CreateEmptySize()
    {
      XSize size = new XSize();
      size.width = double.NegativeInfinity;
      size.height = double.NegativeInfinity;
      return size;
    }

    static XSize()
    {
      s_empty = CreateEmptySize();
    }

    internal double width;
    internal double height;
    private static readonly XSize s_empty;
  }
}