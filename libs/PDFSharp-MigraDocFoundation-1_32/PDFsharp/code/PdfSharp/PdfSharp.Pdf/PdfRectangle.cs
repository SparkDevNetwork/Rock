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
using System.Collections;
using System.Text;
#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows.Media;
#endif
using PdfSharp.Internal;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Represents a PDF rectangle value, that is internally an array with 4 real values.
  /// </summary>
  // TODO: Can the format be expressed less verbose?
  [DebuggerDisplay("X1={X1.ToString(\"0.####\",System.Globalization.CultureInfo.InvariantCulture)}, X2={X2.ToString(\"0.####\",System.Globalization.CultureInfo.InvariantCulture)}, Y1={Y1.ToString(\"0.####\",System.Globalization.CultureInfo.InvariantCulture)}, Y2={Y2.ToString(\"0.####\",System.Globalization.CultureInfo.InvariantCulture)}")]
  public sealed class PdfRectangle : PdfItem
  {
    // This class must behave like a value type. Therefore it cannot be changed (like System.String).

    /// <summary>
    /// Initializes a new instance of the PdfRectangle class.
    /// </summary>
    public PdfRectangle()
    {
    }

    /// <summary>
    /// Initializes a new instance of the PdfRectangle class with two points specifying
    /// two diagonally opposite corners. Notice that in contrast to GDI+ convention the 
    /// 3rd and the 4th parameter specify a point and not a width. This is so much confusing
    /// that this function is for internal use only.
    /// </summary>
    internal PdfRectangle(double x1, double y1, double x2, double y2)
    {
      this.x1 = x1;
      this.y1 = y1;
      this.x2 = x2;
      this.y2 = y2;
    }

#if GDI
    /// <summary>
    /// Initializes a new instance of the PdfRectangle class with two points specifying
    /// two diagonally opposite corners.
    /// </summary>
    public PdfRectangle(PointF pt1, PointF pt2)
    {
      this.x1 = pt1.X;
      this.y1 = pt1.Y;
      this.x2 = pt2.X;
      this.y2 = pt2.Y;
    }
#endif

    /// <summary>
    /// Initializes a new instance of the PdfRectangle class with two points specifying
    /// two diagonally opposite corners.
    /// </summary>
    public PdfRectangle(XPoint pt1, XPoint pt2)
    {
      this.x1 = pt1.X;
      this.y1 = pt1.Y;
      this.x2 = pt2.X;
      this.y2 = pt2.Y;
    }

#if GDI
    /// <summary>
    /// Initializes a new instance of the PdfRectangle class with the specified location and size.
    /// </summary>
    public PdfRectangle(PointF pt, SizeF size)
    {
      this.x1 = pt.X;
      this.y1 = pt.Y;
      this.x2 = pt.X + size.Width;
      this.y2 = pt.Y + size.Height;
    }
#endif

    /// <summary>
    /// Initializes a new instance of the PdfRectangle class with the specified location and size.
    /// </summary>
    public PdfRectangle(XPoint pt, XSize size)
    {
      this.x1 = pt.X;
      this.y1 = pt.Y;
      this.x2 = pt.X + size.Width;
      this.y2 = pt.Y + size.Height;
    }

    /// <summary>
    /// Initializes a new instance of the PdfRectangle class with the specified XRect.
    /// </summary>
    public PdfRectangle(XRect rect)
    {
      this.x1 = rect.x;
      this.y1 = rect.y;
      this.x2 = rect.x + rect.width;
      this.y2 = rect.y + rect.height;
    }

    /// <summary>
    /// Initializes a new instance of the PdfRectangle class with the specified PdfArray.
    /// </summary>
    internal PdfRectangle(PdfItem item)
    {
      if (item == null || item is PdfNull)
        return;

      if (item is PdfReference)
        item = ((PdfReference)item).Value;

      PdfArray array = item as PdfArray;
      if (array == null)
        throw new InvalidOperationException(PSSR.UnexpectedTokenInPdfFile);

      this.x1 = array.Elements.GetReal(0);
      this.y1 = array.Elements.GetReal(1);
      this.x2 = array.Elements.GetReal(2);
      this.y2 = array.Elements.GetReal(3);
    }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    public new PdfRectangle Clone()
    {
      return (PdfRectangle)Copy();
    }

    /// <summary>
    /// Implements cloning this instance.
    /// </summary>
    protected override object Copy()
    {
      PdfRectangle rect = (PdfRectangle)base.Copy();
      return rect;
    }

    /// <summary>
    /// Tests whether all coordinate are zero.
    /// </summary>
    public bool IsEmpty
    {
      get { return this.x1 == 0 && this.y1 == 0 && this.x2 == 0 && this.y2 == 0; }
    }

    /// <summary>
    /// Tests whether the specified object is a PdfRectangle and has equal coordinates.
    /// </summary>
    public override bool Equals(object obj)
    {
      if (obj is PdfRectangle)
      {
        PdfRectangle rect = (PdfRectangle)obj;
        return rect.x1 == this.x1 && rect.y1 == this.y1 && rect.x2 == this.x2 && rect.y2 == this.y2;
      }
      return false;
    }

    /// <summary>
    /// Serves as a hash function for a particular type.
    /// </summary>
    public override int GetHashCode()
    {
      // This code is from System.Drawing...
      return (int)(((((uint)this.x1) ^ ((((uint)this.y1) << 13) |
        (((uint)this.y1) >> 0x13))) ^ ((((uint)this.x2) << 0x1a) |
        (((uint)this.x2) >> 6))) ^ ((((uint)this.y2) << 7) |
        (((uint)this.y2) >> 0x19)));
    }

    /// <summary>
    /// Tests whether two structures have equal coordinates.
    /// </summary>
    public static bool operator ==(PdfRectangle left, PdfRectangle right)
    {
      if ((object)left != null)
      {
        if ((object)right != null)
          return left.x1 == right.x1 && left.y1 == right.y1 && left.x2 == right.x2 && left.y2 == right.y2;
        else
          return false;
      }
      else
        return (object)right == null;
    }

    /// <summary>
    /// Tests whether two structures differ in one or more coordinates.
    /// </summary>
    public static bool operator !=(PdfRectangle left, PdfRectangle right)
    {
      return !(left == right);
    }

    /// <summary>
    /// Gets or sets the x-coordinate of the first corner of this PdfRectangle.
    /// </summary>
    public double X1
    {
      get { return this.x1; }
    }
    double x1;

    /// <summary>
    /// Gets or sets the y-coordinate of the first corner of this PdfRectangle.
    /// </summary>
    public double Y1
    {
      get { return this.y1; }
    }
    double y1;

    /// <summary>
    /// Gets or sets the x-coordinate of the second corner of this PdfRectangle.
    /// </summary>
    public double X2
    {
      get { return this.x2; }
    }
    double x2;

    /// <summary>
    /// Gets or sets the y-coordinate of the second corner of this PdfRectangle.
    /// </summary>
    public double Y2
    {
      get { return this.y2; }
    }
    double y2;

    /// <summary>
    /// Gets X2 - X1.
    /// </summary>
    public double Width
    {
      get { return this.x2 - this.x1; }
    }

    /// <summary>
    /// Gets Y2 - Y1.
    /// </summary>
    public double Height
    {
      get { return this.y2 - this.y1; }
    }

    /// <summary>
    /// Gets or sets the coordinates of the first point of this PdfRectangle.
    /// </summary>
    public XPoint Location
    {
      get { return new XPoint(this.x1, this.y1); }
    }

    /// <summary>
    /// Gets or sets the size of this PdfRectangle.
    /// </summary>
    public XSize Size
    {
      get { return new XSize(this.x2 - this.x1, this.y2 - this.y1); }
    }

#if GDI
    /// <summary>
    /// Determines if the specified point is contained within this PdfRectangle.
    /// </summary>
    public bool Contains(PointF pt)
    {
      return Contains(pt.X, pt.Y);
    }
#endif

    /// <summary>
    /// Determines if the specified point is contained within this PdfRectangle.
    /// </summary>
    public bool Contains(XPoint pt)
    {
      return Contains(pt.X, pt.Y);
    }

    /// <summary>
    /// Determines if the specified point is contained within this PdfRectangle.
    /// </summary>
    public bool Contains(double x, double y)
    {
      // Treat rectangle inclusive/inclusive.
      return this.x1 <= x && x <= this.x2 && this.y1 <= y && y <= this.y2;
    }

#if GDI
    /// <summary>
    /// Determines if the rectangular region represented by rect is entirely contained within this PdfRectangle.
    /// </summary>
    public bool Contains(RectangleF rect)
    {
      return this.x1 <= rect.X && (rect.X + rect.Width) <= this.x2 &&
        this.y1 <= rect.Y && (rect.Y + rect.Height) <= this.y2;
    }
#endif

    /// <summary>
    /// Determines if the rectangular region represented by rect is entirely contained within this PdfRectangle.
    /// </summary>
    public bool Contains(XRect rect)
    {
      return this.x1 <= rect.X && (rect.X + rect.Width) <= this.x2 &&
        this.y1 <= rect.Y && (rect.Y + rect.Height) <= this.y2;
    }

    /// <summary>
    /// Determines if the rectangular region represented by rect is entirely contained within this PdfRectangle.
    /// </summary>
    public bool Contains(PdfRectangle rect)
    {
      return this.x1 <= rect.x1 && rect.x2 <= this.x2 &&
        this.y1 <= rect.y1 && rect.y2 <= this.y2;
    }

    /// <summary>
    /// Returns the rectangle as an XRect object.
    /// </summary>
    public XRect ToXRect()
    {
      return new XRect(this.x1, this.y1, this.Width, this.Height);
    }

    /// <summary>
    /// Returns the rectangle as a string in the form «[x1 y1 x2 y2]».
    /// </summary>
    public override string ToString()
    {
      return PdfEncoders.Format("[{0:0.###} {1:0.###} {2:0.###} {3:0.###}]", this.x1, this.y1, this.x2, this.y2);
    }

    /// <summary>
    /// Writes the rectangle.
    /// </summary>
    internal override void WriteObject(PdfWriter writer)
    {
      writer.Write(this);
    }

    //    /// <summary>
    //    /// Adjusts the location of this PdfRectangle by the specified amount.
    //    /// </summary>
    //    public void Offset(PointF pos)
    //    {
    //      Offset(pos.X, pos.Y);
    //    }
    //
    //    /// <summary>
    //    /// Adjusts the location of this PdfRectangle by the specified amount.
    //    /// </summary>
    //    public void Offset(double x, double y)
    //    {
    //      this.x1 += x;
    //      this.y1 += y;
    //      this.x2 += x;
    //      this.y2 += y;
    //    }
    //
    //    /// <summary>
    //    /// Inflates this PdfRectangle by the specified amount.
    //    /// </summary>
    //    public void Inflate(double x, double y)
    //    {
    //      this.x1 -= x;
    //      this.y1 -= y;
    //      this.x2 += x;
    //      this.y2 += y;
    //    }
    //
    //    /// <summary>
    //    /// Inflates this PdfRectangle by the specified amount.
    //    /// </summary>
    //    public void Inflate(SizeF size)
    //    {
    //      Inflate(size.Width, size.Height);
    //    }
    //
    //    /// <summary>
    //    /// Creates and returns an inflated copy of the specified PdfRectangle.
    //    /// </summary>
    //    public static PdfRectangle Inflate(PdfRectangle rect, double x, double y)
    //    {
    //      rect.Inflate(x, y);
    //      return rect;
    //    }
    //
    //    /// <summary>
    //    /// Replaces this PdfRectangle with the intersection of itself and the specified PdfRectangle.
    //    /// </summary>
    //    public void Intersect(PdfRectangle rect)
    //    {
    //      PdfRectangle rect2 = PdfRectangle.Intersect(rect, this);
    //      this.x1 = rect2.x1;
    //      this.y1 = rect2.y1;
    //      this.x2 = rect2.x2;
    //      this.y2 = rect2.y2;
    //    }
    //
    //    /// <summary>
    //    /// Returns a PdfRectangle that represents the intersection of two rectangles. If there is no intersection,
    //    /// an empty PdfRectangle is returned.
    //    /// </summary>
    //    public static PdfRectangle Intersect(PdfRectangle rect1, PdfRectangle rect2)
    //    {
    //      double xx1 = Math.Max(rect1.x1, rect2.x1);
    //      double xx2 = Math.Min(rect1.x2, rect2.x2);
    //      double yy1 = Math.Max(rect1.y1, rect2.y1);
    //      double yy2 = Math.Min(rect1.y2, rect2.y2);
    //      if (xx2 >= xx1 && yy2 >= yy1)
    //        return new PdfRectangle(xx1, yy1, xx2, yy2);
    //      return PdfRectangle.Empty;
    //    }
    //
    //    /// <summary>
    //    /// Determines if this rectangle intersects with the specified PdfRectangle.
    //    /// </summary>
    //    public bool IntersectsWith(PdfRectangle rect)
    //    {
    //      return rect.x1 < this.x2 && this.x1 < rect.x2 && rect.y1 < this.y2 && this.y1 < rect.y2;
    //    }
    //
    //    /// <summary>
    //    /// Creates the smallest rectangle that can contain both of two specified rectangles.
    //    /// </summary>
    //    public static PdfRectangle Union(PdfRectangle rect1, PdfRectangle rect2)
    //    {
    //      return new PdfRectangle(
    //        Math.Min(rect1.x1, rect2.x1), Math.Max(rect1.x2, rect2.x2),
    //        Math.Min(rect1.y1, rect2.y1), Math.Max(rect1.y2, rect2.y2));
    //    }

    /// <summary>
    /// Represents an empty PdfRectangle.
    /// </summary>
    public static readonly PdfRectangle Empty = new PdfRectangle();
  }
}
