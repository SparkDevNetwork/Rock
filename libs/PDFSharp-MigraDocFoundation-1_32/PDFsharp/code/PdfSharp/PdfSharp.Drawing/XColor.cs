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
#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows.Media;
#endif
using PdfSharp.Internal;

namespace PdfSharp.Drawing
{
  ///<summary>
  /// Represents a RGB, CMYK, or gray scale color.
  /// </summary>
  [DebuggerDisplay("(A={A}, R={R}, G={G}, B={B} C={C}, M={M}, Y={Y}, K={K})")]
  public struct XColor
  {
    XColor(uint argb)
    {
      this.cs = XColorSpace.Rgb;
      this.a = (byte)((argb >> 24) & 0xff) / 255f;
      this.r = (byte)((argb >> 16) & 0xff);
      this.g = (byte)((argb >> 8) & 0xff);
      this.b = (byte)(argb & 0xff);
      this.c = 0;
      this.m = 0;
      this.y = 0;
      this.k = 0;
      this.gs = 0;
      RgbChanged();
      cs.GetType(); // Suppress warning
    }

    XColor(byte alpha, byte red, byte green, byte blue)
    {
      this.cs = XColorSpace.Rgb;
      this.a = alpha / 255f;
      this.r = red;
      this.g = green;
      this.b = blue;
      this.c = 0;
      this.m = 0;
      this.y = 0;
      this.k = 0;
      this.gs = 0;
      RgbChanged();
      cs.GetType(); // Suppress warning
    }

    XColor(double alpha, double cyan, double magenta, double yellow, double black)
    {
      this.cs = XColorSpace.Cmyk;
      this.a = (float)(alpha > 1 ? 1 : (alpha < 0 ? 0 : alpha));
      this.c = (float)(cyan > 1 ? 1 : (cyan < 0 ? 0 : cyan));
      this.m = (float)(magenta > 1 ? 1 : (magenta < 0 ? 0 : magenta));
      this.y = (float)(yellow > 1 ? 1 : (yellow < 0 ? 0 : yellow));
      this.k = (float)(black > 1 ? 1 : (black < 0 ? 0 : black));
      this.r = 0;
      this.g = 0;
      this.b = 0;
      this.gs = 0f;
      CmykChanged();
    }

    XColor(double cyan, double magenta, double yellow, double black)
      : this(1.0, cyan, magenta, yellow, black)
    { }

    XColor(double gray)
    {
      this.cs = XColorSpace.GrayScale;
      if (gray < 0)
        this.gs = 0;
      else if (gray > 1)
        this.gs = 1;
      this.gs = (float)gray;

      this.a = 1;
      this.r = 0;
      this.g = 0;
      this.b = 0;
      this.c = 0;
      this.m = 0;
      this.y = 0;
      this.k = 0;
      GrayChanged();
    }

#if GDI
    XColor(System.Drawing.Color color)
      : this(color.A, color.R, color.G, color.B)
    { }
#endif

#if WPF
    XColor(System.Windows.Media.Color color)
      : this(color.A, color.R, color.G, color.B)
    { }
#endif

#if GDI
    XColor(KnownColor knownColor)
      : this(System.Drawing.Color.FromKnownColor((System.Drawing.KnownColor)knownColor))
    { }
#endif

    internal XColor(XKnownColor knownColor)
      : this(XKnownColorTable.KnownColorToArgb(knownColor))
    { }

    /// <summary>
    /// Creates an XColor structure from a 32-bit ARGB value.
    /// </summary>
    public static XColor FromArgb(int argb)
    {
      return new XColor((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)(argb));
    }

    /// <summary>
    /// Creates an XColor structure from a 32-bit ARGB value.
    /// </summary>
    public static XColor FromArgb(uint argb)
    {
      return new XColor((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)(argb));
    }

    // from System.Drawing.Color
    //public static XColor FromArgb(int alpha, Color baseColor);
    //public static XColor FromArgb(int red, int green, int blue);
    //public static XColor FromArgb(int alpha, int red, int green, int blue);
    //public static XColor FromKnownColor(KnownColor color);
    //public static XColor FromName(string name);

    /// <summary>
    /// Creates an XColor structure from the specified 8-bit color values (red, green, and blue).
    /// The alpha value is implicitly 255 (fully opaque).
    /// </summary>
    public static XColor FromArgb(int red, int green, int blue)
    {
      CheckByte(red, "red");
      CheckByte(green, "green");
      CheckByte(blue, "blue");
      return new XColor(255, (byte)red, (byte)green, (byte)blue);
    }

    /// <summary>
    /// Creates an XColor structure from the four ARGB component (alpha, red, green, and blue) values.
    /// </summary>
    public static XColor FromArgb(int alpha, int red, int green, int blue)
    {
      CheckByte(alpha, "alpha");
      CheckByte(red, "red");
      CheckByte(green, "green");
      CheckByte(blue, "blue");
      return new XColor((byte)alpha, (byte)red, (byte)green, (byte)blue);
    }

#if GDI
    /// <summary>
    /// Creates an XColor structure from the specified System.Drawing.Color.
    /// </summary>
    public static XColor FromArgb(System.Drawing.Color color)
    {
      return new XColor(color);
    }
#endif

#if WPF
    /// <summary>
    /// Creates an XColor structure from the specified System.Drawing.Color.
    /// </summary>
    public static XColor FromArgb(System.Windows.Media.Color color)
    {
      return new XColor(color);
    }
#endif

    /// <summary>
    /// Creates an XColor structure from the specified alpha value and color.
    /// </summary>
    public static XColor FromArgb(int alpha, XColor color)
    {
      color.A = ((byte)alpha) / 255.0;
      return color;
    }

#if GDI
    /// <summary>
    /// Creates an XColor structure from the specified alpha value and color.
    /// </summary>
    public static XColor FromArgb(int alpha, System.Drawing.Color color)
    {
      return new XColor(alpha, color.R, color.G, color.B);
    }
#endif

#if WPF
    /// <summary>
    /// Creates an XColor structure from the specified alpha value and color.
    /// </summary>
    public static XColor FromArgb(int alpha, System.Windows.Media.Color color)
    {
      return new XColor(alpha, color.R, color.G, color.B);
    }
#endif

    /// <summary>
    /// Creates an XColor structure from the specified CMYK values.
    /// </summary>
    public static XColor FromCmyk(double cyan, double magenta, double yellow, double black)
    {
      return new XColor(cyan, magenta, yellow, black);
    }

    /// <summary>
    /// Creates an XColor structure from the specified CMYK values.
    /// </summary>
    public static XColor FromCmyk(double alpha, double cyan, double magenta, double yellow, double black)
    {
      return new XColor(alpha, cyan, magenta, yellow, black);
    }

    /// <summary>
    /// Creates an XColor structure from the specified gray value.
    /// </summary>
    public static XColor FromGrayScale(double grayScale)
    {
      return new XColor(grayScale);
    }

    /// <summary>
    /// Creates an XColor from the specified pre-defined color.
    /// </summary>
    public static XColor FromKnownColor(XKnownColor color)
    {
      return new XColor(color);
    }

#if GDI
    /// <summary>
    /// Creates an XColor from the specified pre-defined color.
    /// </summary>
    public static XColor FromKnownColor(KnownColor color)
    {
      return new XColor(color);
    }
#endif

    /// <summary>
    /// Creates an XColor from the specified name of a pre-defined color.
    /// </summary>
    public static XColor FromName(string name)
    {
#if GDI
      // The implementation in System.Drawing.dll is interesting. It uses a ColorConverter
      // with hash tables, locking mechanisms etc. I'm not sure what problems that solves.
      // So I don't use the source, but the reflection.
      try
      {
        return new XColor((KnownColor)Enum.Parse(typeof(KnownColor), name, true));
      }
      catch { }
#endif
      return XColor.Empty;
    }

    /// <summary>
    /// Gets or sets the color space to be used for PDF generation.
    /// </summary>
    public XColorSpace ColorSpace
    {
      get { return this.cs; }
      set
      {
        if (!Enum.IsDefined(typeof(XColorSpace), value))
          throw new InvalidEnumArgumentException("value", (int)value, typeof(XColorSpace));
        this.cs = value;
      }
    }

    /// <summary>
    /// Indicates whether this XColor structure is uninitialized.
    /// </summary>
    public bool IsEmpty
    {
      get { return this == XColor.Empty; }
    }

#if GDI
#if UseGdiObjects
    /// <summary>
    /// Implicit conversion from Color to XColor
    /// </summary>
    public static implicit operator XColor(Color color)
    {
      return new XColor(color);
    }
#endif

    ///<summary>
    /// Creates a System.Drawing.Color object from this color.
    /// </summary>
    public System.Drawing.Color ToGdiColor()
    {
      return System.Drawing.Color.FromArgb((int)(this.a * 255), this.r, this.g, this.b);
    }
#endif

#if WPF
    ///<summary>
    /// Creates a System.Windows.Media.Color object from this color.
    /// </summary>
    public System.Windows.Media.Color ToWpfColor()
    {
      return System.Windows.Media.Color.FromArgb((byte)(this.a * 255), this.r, this.g, this.b);
    }
#endif

    /// <summary>
    /// Determines whether the specified object is a Color structure and is equivalent to this 
    /// Color structure.
    /// </summary>
    public override bool Equals(object obj)
    {
      if (obj is XColor)
      {
        XColor color = (XColor)obj;
        if (this.r == color.r && this.g == color.g && this.b == color.b &&
          this.c == color.c && this.m == color.m && this.y == color.y && this.k == color.k &&
          this.gs == color.gs)
        {
          return this.a == color.a;
        }
      }
      return false;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
      return ((byte)(this.a * 255)) ^ this.r ^ this.g ^ this.b;
      // ^ *(int*)&this.c ^ *(int*)&this.m ^ *(int*)&this.y ^ *(int*)&this.k;
    }

    /// <summary>
    /// Determines whether two colors are equal.
    /// </summary>
    public static bool operator ==(XColor left, XColor right)
    {
      if (left.r == right.r && left.g == right.g && left.b == right.b &&
          left.c == right.c && left.m == right.m && left.y == right.y && left.k == right.k &&
          left.gs == right.gs)
      {
        return left.a == right.a;
      }
      return false;
    }

    /// <summary>
    /// Determines whether two colors are not equal.
    /// </summary>
    public static bool operator !=(XColor left, XColor right)
    {
      return !(left == right);
    }

    /// <summary>
    /// Gets a value indicating whether this color is a known color.
    /// </summary>
    public bool IsKnownColor
    {
      get { return XKnownColorTable.IsKnownColor(Argb); }
    }

    //public bool IsNamedColor { get; }
    //public bool IsSystemColor { get; }

    /// <summary>
    /// Gets the hue-saturation-brightness (HSB) hue value, in degrees, for this color.
    /// </summary>
    /// <returns>The hue, in degrees, of this color. The hue is measured in degrees, ranging from 0 through 360, in HSB color space.</returns>
    public double GetHue()
    {
      if ((this.r == this.g) && (this.g == this.b))
        return 0;

      double value1 = this.r / 255.0;
      double value2 = this.g / 255.0;
      double value3 = this.b / 255.0;
      double value7 = 0;
      double value4 = value1;
      double value5 = value1;
      if (value2 > value4)
        value4 = value2;

      if (value3 > value4)
        value4 = value3;

      if (value2 < value5)
        value5 = value2;

      if (value3 < value5)
        value5 = value3;

      double value6 = value4 - value5;
      if (value1 == value4)
        value7 = (value2 - value3) / value6;
      else if (value2 == value4)
        value7 = 2f + ((value3 - value1) / value6);
      else if (value3 == value4)
        value7 = 4f + ((value1 - value2) / value6);

      value7 *= 60;
      if (value7 < 0)
        value7 += 360;
      return value7;
    }

    /// <summary>
    /// Gets the hue-saturation-brightness (HSB) saturation value for this color.
    /// </summary>
    /// <returns>The saturation of this color. The saturation ranges from 0 through 1, where 0 is grayscale and 1 is the most saturated.</returns>
    public double GetSaturation()
    {
      double value1 = this.r / 255.0;
      double value2 = this.g / 255.0;
      double value3 = this.b / 255.0;
      double value7 = 0;
      double value4 = value1;
      double value5 = value1;
      if (value2 > value4)
        value4 = value2;

      if (value3 > value4)
        value4 = value3;

      if (value2 < value5)
        value5 = value2;

      if (value3 < value5)
        value5 = value3;

      if (value4 == value5)
        return value7;

      double value6 = (value4 + value5) / 2;
      if (value6 <= 0.5)
        return (value4 - value5) / (value4 + value5);
      return (value4 - value5) / ((2f - value4) - value5);
    }

    /// <summary>
    /// Gets the hue-saturation-brightness (HSB) brightness value for this color.
    /// </summary>
    /// <returns>The brightness of this color. The brightness ranges from 0 through 1, where 0 represents black and 1 represents white.</returns>
    public double GetBrightness()
    {
      double value1 = this.r / 255.0;
      double value2 = this.g / 255.0;
      double value3 = this.b / 255.0;
      double value4 = value1;
      double value5 = value1;
      if (value2 > value4)
        value4 = value2;

      if (value3 > value4)
        value4 = value3;

      if (value2 < value5)
        value5 = value2;

      if (value3 < value5)
        value5 = value3;

      return (value4 + value5) / 2;
    }

    ///<summary>
    /// One of the RGB values changed; recalculate other color representations.
    /// </summary>
    void RgbChanged()
    {
      // ReSharper disable LocalVariableHidesMember
      this.cs = XColorSpace.Rgb;
      int c = 255 - this.r;
      int m = 255 - this.g;
      int y = 255 - this.b;
      int k = Math.Min(c, Math.Min(m, y));
      if (k == 255)
        this.c = this.m = this.y = 0;
      else
      {
        float black = 255f - k;
        this.c = (c - k) / black;
        this.m = (m - k) / black;
        this.y = (y - k) / black;
      }
      this.k = this.gs = k / 255f;
      // ReSharper restore LocalVariableHidesMember
    }

    ///<summary>
    /// One of the CMYK values changed; recalculate other color representations.
    /// </summary>
    void CmykChanged()
    {
      this.cs = XColorSpace.Cmyk;
      float black = this.k * 255;
      float factor = 255f - black;
      this.r = (byte)(255 - Math.Min(255f, this.c * factor + black));
      this.g = (byte)(255 - Math.Min(255f, this.m * factor + black));
      this.b = (byte)(255 - Math.Min(255f, this.y * factor + black));
      this.gs = (float)(1 - Math.Min(1.0, 0.3f * this.c + 0.59f * this.m + 0.11 * this.y + this.k));
    }

    ///<summary>
    /// The gray scale value changed; recalculate other color representations.
    /// </summary>
    void GrayChanged()
    {
      this.cs = XColorSpace.GrayScale;
      this.r = (byte)(this.gs * 255);
      this.g = (byte)(this.gs * 255);
      this.b = (byte)(this.gs * 255);
      this.c = 0;
      this.m = 0;
      this.y = 0;
      this.k = 1 - this.gs;
    }

    // Properties

    /// <summary>
    /// Gets or sets the alpha value the specifies the transparency. 
    /// The value is in the range from 1 (opaque) to 0 (completely transparent).
    /// </summary>
    public double A
    {
      get { return this.a; }
      set
      {
        if (value < 0)
          this.a = 0;
        else if (value > 1)
          this.a = 1;
        else
          this.a = (float)value;
      }
    }

    /// <summary>
    /// Gets or sets the red value.
    /// </summary>
    public byte R
    {
      get { return this.r; }
      set { this.r = value; RgbChanged(); }
    }

    /// <summary>
    /// Gets or sets the green value.
    /// </summary>
    public byte G
    {
      get { return this.g; }
      set { this.g = value; RgbChanged(); }
    }

    /// <summary>
    /// Gets or sets the blue value.
    /// </summary>
    public byte B
    {
      get { return this.b; }
      set { this.b = value; RgbChanged(); }
    }

    /// <summary>
    /// Gets the RGB part value of the color. Internal helper function.
    /// </summary>
    internal uint Rgb
    {
      get { return ((uint)this.r << 16) | ((uint)this.g << 8) | this.b; }
    }

    /// <summary>
    /// Gets the ARGB part value of the color. Internal helper function.
    /// </summary>
    internal uint Argb
    {
      get { return ((uint)(this.a * 255) << 24) | ((uint)this.r << 16) | ((uint)this.g << 8) | this.b; }
    }

    /// <summary>
    /// Gets or sets the cyan value.
    /// </summary>
    public double C
    {
      get { return this.c; }
      set
      {
        if (value < 0)
          this.c = 0;
        else if (value > 1)
          this.c = 1;
        else
          this.c = (float)value;
        CmykChanged();
      }
    }

    /// <summary>
    /// Gets or sets the magenta value.
    /// </summary>
    public double M
    {
      get { return this.m; }
      set
      {
        if (value < 0)
          this.m = 0;
        else if (value > 1)
          this.m = 1;
        else
          this.m = (float)value;
        CmykChanged();
      }
    }

    /// <summary>
    /// Gets or sets the yellow value.
    /// </summary>
    public double Y
    {
      get { return this.y; }
      set
      {
        if (value < 0)
          this.y = 0;
        else if (value > 1)
          this.y = 1;
        else
          this.y = (float)value;
        CmykChanged();
      }
    }

    /// <summary>
    /// Gets or sets the black (or key) value.
    /// </summary>
    public double K
    {
      get { return this.k; }
      set
      {
        if (value < 0)
          this.k = 0;
        else if (value > 1)
          this.k = 1;
        else
          this.k = (float)value;
        CmykChanged();
      }
    }

    /// <summary>
    /// Gets or sets the gray scale value.
    /// </summary>
    public double GS
    {
      get { return this.gs; }
      set
      {
        if (value < 0)
          this.gs = 0;
        else if (value > 1)
          this.gs = 1;
        else
          this.gs = (float)value;
        GrayChanged();
      }
    }

    /// <summary>
    /// Represents the null color.
    /// </summary>
    public static XColor Empty;

    ///<summary>
    /// Special property for XmlSerializer only.
    /// </summary>
    public string RgbCmykG
    {
      get
      {
        return String.Format(CultureInfo.InvariantCulture,
          "{0};{1};{2};{3};{4};{5};{6};{7};{8}", this.r, this.g, this.b, this.c, this.m, this.y, this.k, this.gs, this.a);
      }
      set
      {
        string[] values = value.Split(';');
        this.r = byte.Parse(values[0], CultureInfo.InvariantCulture);
        this.g = byte.Parse(values[1], CultureInfo.InvariantCulture);
        this.b = byte.Parse(values[2], CultureInfo.InvariantCulture);
        this.c = float.Parse(values[3], CultureInfo.InvariantCulture);
        this.m = float.Parse(values[4], CultureInfo.InvariantCulture);
        this.y = float.Parse(values[5], CultureInfo.InvariantCulture);
        this.k = float.Parse(values[6], CultureInfo.InvariantCulture);
        this.gs = float.Parse(values[7], CultureInfo.InvariantCulture);
        this.a = float.Parse(values[8], CultureInfo.InvariantCulture);
      }
    }

    static void CheckByte(int val, string name)
    {
      if (val < 0 || val > 0xFF)
        throw new ArgumentException(PSSR.InvalidValue(val, name, 0, 255));
    }

    private XColorSpace cs;

    private float a;  // alpha

    private byte r;   // \
    private byte g;   // |--- RGB
    private byte b;   // /

    private float c;  // \
    private float m;  // |--- CMYK
    private float y;  // |
    private float k;  // /

    private float gs; // >--- gray scale
  }
}