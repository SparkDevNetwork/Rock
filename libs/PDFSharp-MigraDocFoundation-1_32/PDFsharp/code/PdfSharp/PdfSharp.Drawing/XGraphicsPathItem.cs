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
using System.Text;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
#if true
  /// <summary>
  /// Represents a segment of a path defined by a type and a set of points.
  /// </summary>
  internal sealed class XGraphicsPathItem
  {
    public XGraphicsPathItem(XGraphicsPathItemType type)
    {
      this.type   = type;
      this.points = null;
    }

#if GDI
    public XGraphicsPathItem(XGraphicsPathItemType type, params PointF[] points)
    {
      this.type   = type;
      this.points = XGraphics.MakeXPointArray(points);
    }
#endif
    
    public XGraphicsPathItem(XGraphicsPathItemType type, params XPoint[] points)
    {
      this.type   = type;
      this.points = (XPoint[])points.Clone();
    }

    public XGraphicsPathItem Clone()
    {
      XGraphicsPathItem item = MemberwiseClone() as XGraphicsPathItem;
      item.points = this.points.Clone() as XPoint[];
      return item;
    }

    public XGraphicsPathItemType type;
    public XPoint[] points;
  }
#endif
}