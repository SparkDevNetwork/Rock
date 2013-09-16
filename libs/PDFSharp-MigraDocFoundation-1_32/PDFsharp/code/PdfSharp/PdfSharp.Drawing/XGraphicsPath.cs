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
using System.Collections;
using System.Globalization;
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

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Represents a series of connected lines and curves.
  /// </summary>
  public sealed class XGraphicsPath
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="XGraphicsPath"/> class.
    /// </summary>
    public XGraphicsPath()
    {
#if GDI
      this.gdipPath = new GraphicsPath();
#endif
#if WPF
      this.pathGeometry = new PathGeometry();
#endif
    }

#if GDI
    /// <summary>
    /// Initializes a new instance of the <see cref="XGraphicsPath"/> class.
    /// </summary>
    public XGraphicsPath(PointF[] points, byte[] types, XFillMode fillMode)
    {
#if GDI
      this.gdipPath = new GraphicsPath(points, types, (FillMode)fillMode);
#endif
#if WPF
      this.pathGeometry = new PathGeometry();
      this.pathGeometry.FillRule = FillRule.EvenOdd;
#endif
    }
#endif

#if GDI
    /// <summary>
    /// Gets access to underlying GDI+ path.
    /// </summary>
    internal GraphicsPath gdipPath;
#endif

#if WPF
    /// <summary>
    /// Gets access to underlying WPF path geometry.
    /// </summary>
    internal PathGeometry pathGeometry;

    ///// <summary>
    ///// The current path figure;
    ///// </summary>
    //PathFigure figure;

    /// <summary>
    /// Gets the current path figure.
    /// </summary>
    PathFigure CurrentPathFigure
    {
      get
      {
        int count = this.pathGeometry.Figures.Count;
        if (count == 0)
        {
          this.pathGeometry.Figures.Add(new PathFigure());
          count++;
        }
        else if (this.startNewFigure && this.pathGeometry.Figures[count - 1].Segments.Count == 0)
        {
          this.pathGeometry.Figures.Add(new PathFigure());
          count++;
        }
        return this.pathGeometry.Figures[count - 1];

        //if (this.figure == null)
        //{
        //  this.figure = new PathFigure();
        //  this.pathGeometry.Figures.Add(this.figure);
        //}
        //return this.figure;
      }
    }
    bool startNewFigure;
#endif

    /// <summary>
    /// Clones this instance.
    /// </summary>
    public XGraphicsPath Clone()
    {
      XGraphicsPath path = (XGraphicsPath)MemberwiseClone();
#if GDI
      path.gdipPath = this.gdipPath.Clone() as GraphicsPath;
#endif
#if WPF
#if !SILVERLIGHT
      path.pathGeometry = this.pathGeometry.Clone();
#else
      // AGHACK
#endif
#endif
      return path;
    }

    ///// <summary>
    ///// For internal use only.
    ///// </summary>
    //internal XGraphicsPathItem[] GetPathData()
    //{
    //  int count = this.items.Count;
    //  XGraphicsPathItem[] data = new XGraphicsPathItem[count];
    //  for (int idx = 0; idx < count; idx++)
    //    data[idx] = ((XGraphicsPathItem)this.items[idx]).Clone() as XGraphicsPathItem;
    //  return data;
    //}

    // ----- AddLine ------------------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds a line segment to current figure.
    /// </summary>
    public void AddLine(System.Drawing.Point pt1, System.Drawing.Point pt2)
    {
      AddLine((double)pt1.X, (double)pt1.Y, (double)pt2.X, (double)pt2.Y);
    }
#endif

#if WPF
    /// <summary>
    /// Adds a line segment to current figure.
    /// </summary>
    public void AddLine(System.Windows.Point pt1, System.Windows.Point pt2)
    {
      AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
    }
#endif

#if GDI
    /// <summary>
    /// Adds  a line segment to current figure.
    /// </summary>
    public void AddLine(PointF pt1, PointF pt2)
    {
      AddLine((double)pt1.X, (double)pt1.Y, (double)pt2.X, (double)pt2.Y);
    }
#endif

    /// <summary>
    /// Adds  a line segment to current figure.
    /// </summary>
    public void AddLine(XPoint pt1, XPoint pt2)
    {
      AddLine(pt1.X, pt1.Y, pt2.X, pt2.Y);
    }

    /// <summary>
    /// Adds  a line segment to current figure.
    /// </summary>
    public void AddLine(int x1, int y1, int x2, int y2)
    {
      AddLine((double)x1, (double)y1, (double)x2, (double)y2);
    }

    /// <summary>
    /// Adds  a line segment to current figure.
    /// </summary>
    public void AddLine(double x1, double y1, double x2, double y2)
    {
#if GDI
      this.gdipPath.AddLine((float)x1, (float)y1, (float)x2, (float)y2);
#endif
#if WPF
      PathFigure figure = CurrentPathFigure;
      if (figure.Segments.Count == 0)
      {
        figure.StartPoint = new System.Windows.Point(x1, y1);
#if !SILVERLIGHT
        LineSegment lineSegment = new LineSegment(new System.Windows.Point(x2, y2), true);
#else
        LineSegment lineSegment = new LineSegment();
        lineSegment.Point = new Point(x2, y2);
        // AGHACK: ,true??
#endif
        figure.Segments.Add(lineSegment);
      }
      else
      {
#if !SILVERLIGHT
        LineSegment lineSegment1 = new LineSegment(new System.Windows.Point(x1, y1), true);
        LineSegment lineSegment2 = new LineSegment(new System.Windows.Point(x2, y2), true);
#else
        LineSegment lineSegment1 = new LineSegment();
        lineSegment1.Point = new Point(x1, y1);
        // AGHACK: ,true??
        LineSegment lineSegment2 = new LineSegment();
        lineSegment2.Point = new Point(x2, y2);
        // AGHACK: ,true??
#endif
        figure.Segments.Add(lineSegment1);
        figure.Segments.Add(lineSegment2);
      }
#endif
    }

    // ----- AddLines -----------------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds a series of connected line segments to current figure.
    /// </summary>
    public void AddLines(System.Drawing.Point[] points)
    {
      AddLines(XGraphics.MakeXPointArray(points));
    }
#endif

#if WPF
    /// <summary>
    /// Adds a series of connected line segments to current figure.
    /// </summary>
    public void AddLines(System.Windows.Point[] points)
    {
      AddLines(XGraphics.MakeXPointArray(points));
    }
#endif

#if GDI
    /// <summary>
    /// Adds a series of connected line segments to current figure.
    /// </summary>
    public void AddLines(PointF[] points)
    {
      AddLines(XGraphics.MakeXPointArray(points));
    }
#endif

    /// <summary>
    /// Adds a series of connected line segments to current figure.
    /// </summary>
    public void AddLines(XPoint[] points)
    {
      if (points == null)
        throw new ArgumentNullException("points");

      int count = points.Length;
      if (count == 0)
        return;

#if GDI
      this.gdipPath.AddLines(XGraphics.MakePointFArray(points));
#endif
#if WPF
      PathFigure figure = CurrentPathFigure;
      if (figure.Segments.Count == 0)
      {
        figure.StartPoint = new System.Windows.Point(points[0].x, points[0].y);
        for (int idx = 1; idx < count; idx++)
        {
#if !SILVERLIGHT
          LineSegment lineSegment = new LineSegment(new System.Windows.Point(points[idx].x, points[idx].y), true);
#else
          LineSegment lineSegment = new LineSegment();
          lineSegment.Point = new Point(points[idx].x, points[idx].y); // ,true?
#endif
          figure.Segments.Add(lineSegment);
        }
      }
      else
      {
        for (int idx = 0; idx < count; idx++)
        {
          // figure.Segments.Add(new LineSegment(new System.Windows.Point(points[idx].x, points[idx].y), true));
#if !SILVERLIGHT
          LineSegment lineSegment = new LineSegment(new System.Windows.Point(points[idx].x, points[idx].y), true);
#else
          LineSegment lineSegment = new LineSegment();
          lineSegment.Point = new Point(points[idx].x, points[idx].y); // ,true?
#endif
          figure.Segments.Add(lineSegment);
        }
      }
#endif
    }

    // ----- AddBezier ----------------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds a cubic Bézier curve to the current figure.
    /// </summary>
    public void AddBezier(System.Drawing.Point pt1, System.Drawing.Point pt2, System.Drawing.Point pt3, System.Drawing.Point pt4)
    {
      AddBezier((double)pt1.X, (double)pt1.Y, (double)pt2.X, (double)pt2.Y, (double)pt3.X, (double)pt3.Y, (double)pt4.X, (double)pt4.Y);
    }
#endif

#if WPF
    /// <summary>
    /// Adds a cubic Bézier curve to the current figure.
    /// </summary>
    public void AddBezier(System.Windows.Point pt1, System.Windows.Point pt2, System.Windows.Point pt3, System.Windows.Point pt4)
    {
      AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
    }
#endif

#if GDI
    /// <summary>
    /// Adds a cubic Bézier curve to the current figure.
    /// </summary>
    public void AddBezier(PointF pt1, PointF pt2, PointF pt3, PointF pt4)
    {
      AddBezier((double)pt1.X, (double)pt1.Y, (double)pt2.X, (double)pt2.Y, (double)pt3.X, (double)pt3.Y, (double)pt4.X, (double)pt4.Y);
    }
#endif

    /// <summary>
    /// Adds a cubic Bézier curve to the current figure.
    /// </summary>
    public void AddBezier(XPoint pt1, XPoint pt2, XPoint pt3, XPoint pt4)
    {
      AddBezier(pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
    }

    /// <summary>
    /// Adds a cubic Bézier curve to the current figure.
    /// </summary>
    public void AddBezier(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
    {
      AddBezier((double)x1, (double)y1, (double)x2, (double)y2, (double)x3, (double)y3, (double)x4, (double)y4);
    }

    /// <summary>
    /// Adds a cubic Bézier curve to the current figure.
    /// </summary>
    public void AddBezier(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
    {
#if GDI
      this.gdipPath.AddBezier((float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3, (float)x4, (float)y4);
#endif
#if WPF
      PathFigure figure = CurrentPathFigure;
      if (figure.Segments.Count == 0)
        figure.StartPoint = new System.Windows.Point(x1, y1);
      else
      {
        // figure.Segments.Add(new LineSegment(new System.Windows.Point(x1, y1), true));
#if !SILVERLIGHT
        LineSegment lineSegment = new LineSegment(new System.Windows.Point(x1, y1), true);
#else
        LineSegment lineSegment = new LineSegment();
        lineSegment.Point = new Point(x1, y1);
#endif
        figure.Segments.Add(lineSegment);
      }
      //figure.Segments.Add(new BezierSegment(
      //  new System.Windows.Point(x2, y2),
      //  new System.Windows.Point(x3, y3),
      //  new System.Windows.Point(x4, y4), true));
#if !SILVERLIGHT
      BezierSegment bezierSegment = new BezierSegment(
        new System.Windows.Point(x2, y2),
        new System.Windows.Point(x3, y3),
        new System.Windows.Point(x4, y4), true);
#else
      BezierSegment bezierSegment = new BezierSegment();
      bezierSegment.Point1 = new Point(x2, y2);
      bezierSegment.Point2 = new Point(x3, y3);
      bezierSegment.Point3 = new Point(x4, y4);
#endif
      figure.Segments.Add(bezierSegment);
#endif
    }

    // ----- AddBeziers ---------------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds a sequence of connected cubic Bézier curves to the current figure.
    /// </summary>
    public void AddBeziers(System.Drawing.Point[] points)
    {
      AddBeziers(XGraphics.MakeXPointArray(points));
    }
#endif

#if WPF
    /// <summary>
    /// Adds a sequence of connected cubic Bézier curves to the current figure.
    /// </summary>
    public void AddBeziers(System.Windows.Point[] points)
    {
      AddBeziers(XGraphics.MakeXPointArray(points));
    }
#endif

#if GDI
    /// <summary>
    /// Adds a sequence of connected cubic Bézier curves to the current figure.
    /// </summary>
    public void AddBeziers(PointF[] points)
    {
      AddBeziers(XGraphics.MakeXPointArray(points));
    }
#endif

    /// <summary>
    /// Adds a sequence of connected cubic Bézier curves to the current figure.
    /// </summary>
    public void AddBeziers(XPoint[] points)
    {
      if (points == null)
        new ArgumentNullException("points");

      int count = points.Length;
      if (points.Length < 4)
        throw new ArgumentException("At least four points required for bezier curve.", "points");

      if ((points.Length - 1) % 3 != 0)
        throw new ArgumentException("Invalid number of points for bezier curve. Number must fulfil 4+3n.", "points");

#if GDI
      this.gdipPath.AddBeziers(XGraphics.MakePointFArray(points));
#endif
#if WPF
      PathFigure figure = CurrentPathFigure;
      if (figure.Segments.Count == 0)
        figure.StartPoint = new System.Windows.Point(points[0].x, points[0].y);
      else
      {
        // figure.Segments.Add(new LineSegment(new System.Windows.Point(points[0].x, points[0].y), true));
#if !SILVERLIGHT
        LineSegment lineSegment = new LineSegment(new System.Windows.Point(points[0].x, points[0].y), true);
#else
        LineSegment lineSegment = new LineSegment();
        lineSegment.Point = new Point(points[0].x, points[0].y);
#endif
        figure.Segments.Add(lineSegment);
      }
      for (int idx = 1; idx < count; idx += 3)
      {
        //figure.Segments.Add(new BezierSegment(
        //                      new System.Windows.Point(points[idx].x, points[idx].y),
        //                      new System.Windows.Point(points[idx + 1].x, points[idx + 1].y),
        //                      new System.Windows.Point(points[idx + 2].x, points[idx + 2].y), true));
#if !SILVERLIGHT
        BezierSegment bezierSegment = new BezierSegment(
                              new System.Windows.Point(points[idx].x, points[idx].y),
                              new System.Windows.Point(points[idx + 1].x, points[idx + 1].y),
                              new System.Windows.Point(points[idx + 2].x, points[idx + 2].y), true);
#else
        BezierSegment bezierSegment = new BezierSegment();
        bezierSegment.Point1 = new Point(points[idx].x, points[idx].y);
        bezierSegment.Point2 = new Point(points[idx + 1].x, points[idx + 1].y);
        bezierSegment.Point3 = new Point(points[idx + 2].x, points[idx + 2].y);
#endif
        figure.Segments.Add(bezierSegment);
      }
#endif
    }

    // ----- AddCurve -----------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds a spline curve to the current figure.
    /// </summary>
    public void AddCurve(System.Drawing.Point[] points)
    {
      AddCurve(XGraphics.MakeXPointArray(points));
    }
#endif

#if WPF
    /// <summary>
    /// Adds a spline curve to the current figure.
    /// </summary>
    public void AddCurve(System.Windows.Point[] points)
    {
      AddCurve(XGraphics.MakeXPointArray(points));
    }
#endif

#if GDI
    /// <summary>
    /// Adds a spline curve to the current figure.
    /// </summary>
    public void AddCurve(PointF[] points)
    {
      AddCurve(XGraphics.MakeXPointArray(points));
    }
#endif

    /// <summary>
    /// Adds a spline curve to the current figure.
    /// </summary>
    public void AddCurve(XPoint[] points)
    {
      AddCurve(points, 0.5);
    }

#if GDI
    /// <summary>
    /// Adds a spline curve to the current figure.
    /// </summary>
    public void AddCurve(System.Drawing.Point[] points, double tension)
    {
      AddCurve(XGraphics.MakeXPointArray(points), tension);
    }
#endif

#if WPF
    /// <summary>
    /// Adds a spline curve to the current figure.
    /// </summary>
    public void AddCurve(System.Windows.Point[] points, double tension)
    {
      AddCurve(XGraphics.MakeXPointArray(points), tension);
    }
#endif

#if GDI
    /// <summary>
    /// Adds a spline curve to the current figure.
    /// </summary>
    public void AddCurve(PointF[] points, double tension)
    {
      AddCurve(XGraphics.MakeXPointArray(points), tension);
    }
#endif

    /// <summary>
    /// Adds a spline curve to the current figure.
    /// </summary>
    public void AddCurve(XPoint[] points, double tension)
    {
      int count = points.Length;
      if (count < 2)
        throw new ArgumentException("AddCurve requires two or more points.", "points");
#if GDI
      this.gdipPath.AddCurve(XGraphics.MakePointFArray(points), (float)tension);
#endif
#if WPF
      tension /= 3;

      PathFigure figure = CurrentPathFigure;
      if (figure.Segments.Count == 0)
        figure.StartPoint = new System.Windows.Point(points[0].x, points[0].y);
      else
      {
        // figure.Segments.Add(new LineSegment(new System.Windows.Point(points[0].x, points[0].y), true));
#if !SILVERLIGHT
        LineSegment lineSegment = new LineSegment(new System.Windows.Point(points[0].x, points[0].y), true);
#else
        LineSegment lineSegment = new LineSegment();
        lineSegment.Point = new Point(points[0].x, points[0].y);
#endif
        figure.Segments.Add(lineSegment);
      }

      if (count == 2)
      {
        figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[0], points[0], points[1], points[1], tension));
      }
      else
      {
        figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[0], points[0], points[1], points[2], tension));
        for (int idx = 1; idx < count - 2; idx++)
          figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[idx - 1], points[idx], points[idx + 1], points[idx + 2], tension));
        figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[count - 3], points[count - 2], points[count - 1], points[count - 1], tension));
      }
#endif
    }

#if GDI
    /// <summary>
    /// Adds a spline curve to the current figure.
    /// </summary>
    public void AddCurve(System.Drawing.Point[] points, int offset, int numberOfSegments, float tension)
    {
      AddCurve(XGraphics.MakeXPointArray(points), offset, numberOfSegments, tension);
    }
#endif

#if WPF
    /// <summary>
    /// Adds a spline curve to the current figure.
    /// </summary>
    public void AddCurve(System.Windows.Point[] points, int offset, int numberOfSegments, float tension)
    {
      AddCurve(XGraphics.MakeXPointArray(points), offset, numberOfSegments, tension);
    }
#endif

#if GDI
    /// <summary>
    /// Adds a spline curve to the current figure.
    /// </summary>
    public void AddCurve(PointF[] points, int offset, int numberOfSegments, float tension)
    {
      AddCurve(XGraphics.MakeXPointArray(points), offset, numberOfSegments, tension);
    }
#endif

    /// <summary>
    /// Adds a spline curve to the current figure.
    /// </summary>
    public void AddCurve(XPoint[] points, int offset, int numberOfSegments, double tension)
    {
#if GDI
      this.gdipPath.AddCurve(XGraphics.MakePointFArray(points), offset, numberOfSegments, (float)tension);
#endif
#if WPF
      throw new NotImplementedException("AddCurve not yet implemented.");
#endif
    }

    // ----- AddArc -------------------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds an elliptical arc to the current figure.
    /// </summary>
    public void AddArc(Rectangle rect, double startAngle, double sweepAngle)
    {
      AddArc((double)rect.X, (double)rect.Y, (double)rect.Width, (double)rect.Height, startAngle, sweepAngle);
    }
#endif

#if GDI
    /// <summary>
    /// Adds an elliptical arc to the current figure.
    /// </summary>
    public void AddArc(RectangleF rect, double startAngle, double sweepAngle)
    {
      AddArc((double)rect.X, (double)rect.Y, (double)rect.Width, (double)rect.Height, startAngle, sweepAngle);
    }
#endif

    /// <summary>
    /// Adds an elliptical arc to the current figure.
    /// </summary>
    public void AddArc(XRect rect, double startAngle, double sweepAngle)
    {
      AddArc((double)rect.X, (double)rect.Y, (double)rect.Width, (double)rect.Height, startAngle, sweepAngle);
    }

    /// <summary>
    /// Adds an elliptical arc to the current figure.
    /// </summary>
    public void AddArc(int x, int y, int width, int height, int startAngle, int sweepAngle)
    {
      AddArc((double)x, (double)y, (double)width, (double)height, (double)startAngle, (double)sweepAngle);
    }

    /// <summary>
    /// Adds an elliptical arc to the current figure.
    /// </summary>
    public void AddArc(double x, double y, double width, double height, double startAngle, double sweepAngle)
    {
#if GDI
      this.gdipPath.AddArc((float)x, (float)y, (float)width, (float)height, (float)startAngle, (float)sweepAngle);
#endif
#if WPF
      PathFigure figure = CurrentPathFigure;
      System.Windows.Point startPoint;
      ArcSegment seg = GeometryHelper.CreateArcSegment(x, y, width, height, startAngle, sweepAngle, out startPoint);
      if (figure.Segments.Count == 0)
        figure.StartPoint = startPoint;
      figure.Segments.Add(seg);

      //figure.Segments.Add(
      //if (figure.Segments.Count == 0)
      //  figure.StartPoint = new System.Windows.Point(points[0].x, points[0].y);
      //else
      //  figure.Segments.Add(new LineSegment(new System.Windows.Point(points[0].x, points[0].y), true));

      //for (int idx = 1; idx < 5555; idx += 3)
      //  figure.Segments.Add(new BezierSegment(
      //    new System.Windows.Point(points[idx].x, points[idx].y),
      //    new System.Windows.Point(points[idx + 1].x, points[idx + 1].y),
      //    new System.Windows.Point(points[idx + 2].x, points[idx + 2].y), true));
#endif
    }

#if WPF
    /// <summary>
    /// Adds an elliptical arc to the current figure. The arc is specified WPF like.
    /// </summary>
    public void AddArc(XPoint point1, XPoint point2, XSize size, double rotationAngle, bool isLargeArg, SweepDirection sweepDirection)
    {
      PathFigure figure = CurrentPathFigure;
      if (figure.Segments.Count == 0)
        figure.StartPoint = point1.ToPoint();
      else
      {
        // figure.Segments.Add(new LineSegment(point1.ToPoint(), true));
#if !SILVERLIGHT
        LineSegment lineSegment = new LineSegment(point1.ToPoint(), true);
#else
        LineSegment lineSegment = new LineSegment();
        lineSegment.Point = point1.ToPoint();
#endif
        figure.Segments.Add(lineSegment);
      }

      // figure.Segments.Add(new ArcSegment(point2.ToPoint(), size.ToSize(), rotationAngle, isLargeArg, sweepDirection, true));
#if !SILVERLIGHT
      ArcSegment arcSegment = new ArcSegment(point2.ToPoint(), size.ToSize(), rotationAngle, isLargeArg, sweepDirection, true);
#else
      ArcSegment arcSegment = new ArcSegment();
      arcSegment.Point = point2.ToPoint();
      arcSegment.Size = size.ToSize();
      arcSegment.RotationAngle = rotationAngle;
      arcSegment.IsLargeArc = isLargeArg;
      arcSegment.SweepDirection = sweepDirection;
#endif
      figure.Segments.Add(arcSegment);
    }
#endif

    // ----- AddRectangle -------------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds a rectangle to this path.
    /// </summary>
    public void AddRectangle(Rectangle rect)
    {
      AddRectangle(new XRect(rect));
    }
#endif

#if GDI
    /// <summary>
    /// Adds a rectangle to this path.
    /// </summary>
    public void AddRectangle(RectangleF rect)
    {
      AddRectangle(new XRect(rect));
    }
#endif

    /// <summary>
    /// Adds a rectangle to this path.
    /// </summary>
    public void AddRectangle(XRect rect)
    {
#if GDI
      this.gdipPath.AddRectangle(rect.ToRectangleF());
#endif
#if WPF
      StartFigure();
      PathFigure figure = CurrentPathFigure;
      figure.StartPoint = new System.Windows.Point(rect.x, rect.y);

      // figure.Segments.Add(new LineSegment(new System.Windows.Point(rect.x + rect.width, rect.y), true));
      // figure.Segments.Add(new LineSegment(new System.Windows.Point(rect.x + rect.width, rect.y + rect.height), true));
      // figure.Segments.Add(new LineSegment(new System.Windows.Point(rect.x, rect.y + rect.height), true));
#if !SILVERLIGHT
      LineSegment lineSegment1 = new LineSegment(new System.Windows.Point(rect.x + rect.width, rect.y), true);
      LineSegment lineSegment2 = new LineSegment(new System.Windows.Point(rect.x + rect.width, rect.y + rect.height), true);
      LineSegment lineSegment3 = new LineSegment(new System.Windows.Point(rect.x, rect.y + rect.height), true);
#else
      LineSegment lineSegment1 = new LineSegment();
      lineSegment1.Point = new Point(rect.x + rect.width, rect.y);
      LineSegment lineSegment2 = new LineSegment();
      lineSegment2.Point = new Point(rect.x + rect.width, rect.y + rect.height);
      LineSegment lineSegment3 = new LineSegment();
      lineSegment3.Point = new Point(rect.x, rect.y + rect.height);
#endif
      figure.Segments.Add(lineSegment1);
      figure.Segments.Add(lineSegment2);
      figure.Segments.Add(lineSegment3);
      CloseFigure();
#endif
    }

    /// <summary>
    /// Adds a rectangle to this path.
    /// </summary>
    public void AddRectangle(int x, int y, int width, int height)
    {
      AddRectangle(new XRect(x, y, width, height));
    }

    /// <summary>
    /// Adds a rectangle to this path.
    /// </summary>
    public void AddRectangle(double x, double y, double width, double height)
    {
      AddRectangle(new XRect(x, y, width, height));
    }

    // ----- AddRectangles ------------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds a series of rectangles to this path.
    /// </summary>
    public void AddRectangles(Rectangle[] rects)
    {
      int count = rects.Length;
      for (int idx = 0; idx < count; idx++)
        AddRectangle(rects[idx]);
      this.gdipPath.AddRectangles(rects);
    }
#endif

#if GDI
    /// <summary>
    /// Adds a series of rectangles to this path.
    /// </summary>
    public void AddRectangles(RectangleF[] rects)
    {
      int count = rects.Length;
      for (int idx = 0; idx < count; idx++)
        AddRectangle(rects[idx]);
      this.gdipPath.AddRectangles(rects);
    }
#endif

    /// <summary>
    /// Adds a series of rectangles to this path.
    /// </summary>
    public void AddRectangles(XRect[] rects)
    {
      int count = rects.Length;
      for (int idx = 0; idx < count; idx++)
      {
#if GDI
        this.gdipPath.AddRectangle(rects[idx].ToRectangleF());
#endif
#if WPF
        StartFigure();
        PathFigure figure = CurrentPathFigure;
        XRect rect = rects[idx];
        figure.StartPoint = new System.Windows.Point(rect.x, rect.y);

        // figure.Segments.Add(new LineSegment(new System.Windows.Point(rect.x + rect.width, rect.y), true));
        // figure.Segments.Add(new LineSegment(new System.Windows.Point(rect.x + rect.width, rect.y + rect.height), true));
        // figure.Segments.Add(new LineSegment(new System.Windows.Point(rect.x, rect.y + rect.height), true));
#if !SILVERLIGHT
        LineSegment lineSegment1 = new LineSegment(new System.Windows.Point(rect.x + rect.width, rect.y), true);
        LineSegment lineSegment2 = new LineSegment(new System.Windows.Point(rect.x + rect.width, rect.y + rect.height), true);
        LineSegment lineSegment3 = new LineSegment(new System.Windows.Point(rect.x, rect.y + rect.height), true);
#else
        LineSegment lineSegment1 = new LineSegment();
        lineSegment1.Point = new Point(rect.x + rect.width, rect.y);
        LineSegment lineSegment2 = new LineSegment();
        lineSegment2.Point = new Point(rect.x + rect.width, rect.y + rect.height);
        LineSegment lineSegment3 = new LineSegment();
        lineSegment3.Point = new Point(rect.x, rect.y + rect.height);
#endif
        figure.Segments.Add(lineSegment1);
        figure.Segments.Add(lineSegment2);
        figure.Segments.Add(lineSegment3);
        CloseFigure();
#endif
      }
    }

    // ----- AddRoundedRectangle ------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds a rectangle with rounded corners to this path.
    /// </summary>
    public void AddRoundedRectangle(Rectangle rect, System.Drawing.Size ellipseSize)
    {
      AddRoundedRectangle((double)rect.X, (double)rect.Y, (double)rect.Width, (double)rect.Height,
        (double)ellipseSize.Width, (double)ellipseSize.Height);
    }
#endif

#if WPF
    /// <summary>
    /// Adds a rectangle with rounded corners to this path.
    /// </summary>
    public void AddRoundedRectangle(Rect rect, System.Windows.Size ellipseSize)
    {
      AddRoundedRectangle((double)rect.X, (double)rect.Y, (double)rect.Width, (double)rect.Height,
        (double)ellipseSize.Width, (double)ellipseSize.Height);
    }
#endif

#if GDI
    /// <summary>
    /// Adds a rectangle with rounded corners to this path.
    /// </summary>
    public void AddRoundedRectangle(RectangleF rect, SizeF ellipseSize)
    {
      AddRoundedRectangle((double)rect.X, (double)rect.Y, (double)rect.Width, (double)rect.Height,
        (double)ellipseSize.Width, (double)ellipseSize.Height);
    }
#endif

#if GDI
    /// <summary>
    /// Adds a rectangle with rounded corners to this path.
    /// </summary>
    public void AddRoundedRectangle(XRect rect, SizeF ellipseSize)
    {
      AddRoundedRectangle((double)rect.X, (double)rect.Y, (double)rect.Width, (double)rect.Height,
        (double)ellipseSize.Width, (double)ellipseSize.Height);
    }
#endif

    /// <summary>
    /// Adds a rectangle with rounded corners to this path.
    /// </summary>
    public void AddRoundedRectangle(int x, int y, int width, int height, int ellipseWidth, int ellipseHeight)
    {
      AddRoundedRectangle((double)x, (double)y, (double)width, (double)height, (double)ellipseWidth, (double)ellipseHeight);
    }

    /// <summary>
    /// Adds a rectangle with rounded corners to this path.
    /// </summary>
    public void AddRoundedRectangle(double x, double y, double width, double height, double ellipseWidth, double ellipseHeight)
    {
#if GDI
      this.gdipPath.AddArc((float)(x + width - ellipseWidth), (float)y, (float)ellipseWidth, (float)ellipseHeight, -90, 90);
      this.gdipPath.AddArc((float)(x + width - ellipseWidth), (float)(y + height - ellipseHeight), (float)ellipseWidth, (float)ellipseHeight, 0, 90);
      this.gdipPath.AddArc((float)x, (float)(y + height - ellipseHeight), (float)ellipseWidth, (float)ellipseHeight, 90, 90);
      this.gdipPath.AddArc((float)x, (float)y, (float)ellipseWidth, (float)ellipseHeight, 180, 90);
      this.gdipPath.CloseFigure();
#endif
#if WPF
      double ex = ellipseWidth / 2;
      double ey = ellipseHeight / 2;
      StartFigure();
      PathFigure figure = CurrentPathFigure;
      figure.StartPoint = new System.Windows.Point(x + ex, y);

#if !SILVERLIGHT
      figure.Segments.Add(new LineSegment(new System.Windows.Point(x + width - ex, y), true));
      // TODOWPF XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXx
      figure.Segments.Add(new ArcSegment(new System.Windows.Point(x + width, y + ey), new System.Windows.Size(ex, ey), 0, false, SweepDirection.Clockwise, true));
      //figure.Segments.Add(new LineSegment(new System.Windows.Point(x + width, y + ey), true));

      figure.Segments.Add(new LineSegment(new System.Windows.Point(x + width, y + height - ey), true));
      // TODOWPF
      figure.Segments.Add(new ArcSegment(new System.Windows.Point(x + width - ex, y + height), new System.Windows.Size(ex, ey), 0, false, SweepDirection.Clockwise, true));
      //figure.Segments.Add(new LineSegment(new System.Windows.Point(x + width - ex, y + height), true));

      figure.Segments.Add(new LineSegment(new System.Windows.Point(x + ex, y + height), true));
      // TODOWPF
      figure.Segments.Add(new ArcSegment(new System.Windows.Point(x, y + height - ey), new System.Windows.Size(ex, ey), 0, false, SweepDirection.Clockwise, true));
      //figure.Segments.Add(new LineSegment(new System.Windows.Point(x, y + height - ey), true));

      figure.Segments.Add(new LineSegment(new System.Windows.Point(x, y + ey), true));
      // TODOWPF
      figure.Segments.Add(new ArcSegment(new System.Windows.Point(x + ex, y), new System.Windows.Size(ex, ey), 0, false, SweepDirection.Clockwise, true));
      //figure.Segments.Add(new LineSegment(new System.Windows.Point(x + ex, y), true));
#else
      // AGHACK
#endif
      CloseFigure();
#endif
    }

    // ----- AddEllipse ---------------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds an ellipse to the current path.
    /// </summary>
    public void AddEllipse(Rectangle rect)
    {
      AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
    }
#endif

#if GDI
    /// <summary>
    /// Adds an ellipse to the current path.
    /// </summary>
    public void AddEllipse(RectangleF rect)
    {
      AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);
    }
#endif

    /// <summary>
    /// Adds an ellipse to the current path.
    /// </summary>
    public void AddEllipse(XRect rect)
    {
      AddEllipse(rect.x, rect.y, rect.width, rect.height);
    }

    /// <summary>
    /// Adds an ellipse to the current path.
    /// </summary>
    public void AddEllipse(int x, int y, int width, int height)
    {
      AddEllipse((double)x, (double)y, (double)width, (double)height);
    }

    /// <summary>
    /// Adds an ellipse to the current path.
    /// </summary>
    public void AddEllipse(double x, double y, double width, double height)
    {
#if GDI
      this.gdipPath.AddEllipse((float)x, (float)y, (float)width, (float)height);
#endif
#if WPF
      StartFigure();
      //this.pathGeometry.AddGeometry(new EllipseGeometry(new Rect(x, y, width, height)));
#if !SILVERLIGHT
      this.pathGeometry.AddGeometry(new EllipseGeometry(new Rect(x, y, width, height)));
      //EllipseGeometry ellipseGeometry = new EllipseGeometry(new Rect(x, y, width, height));
      //this.pa thGeometry..AddGeometry(ellipseGeometry);
#else
      // AGHACK: No AddGeometry in Silverlight version of PathGeometry
      //EllipseGeometry ellipseGeometry = new EllipseGeometry();
      //ellipseGeometry.Center = new Point((x + width) / 2, (y + height) / 2);
      //ellipseGeometry.RadiusX = width / 2;
      //ellipseGeometry.RadiusY = height / 2;
#endif
#endif
    }

    // ----- AddPolygon ---------------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds a polygon to this path.
    /// </summary>
    public void AddPolygon(System.Drawing.Point[] points)
    {
      this.gdipPath.AddPolygon(points);
    }
#endif

#if WPF
    /// <summary>
    /// Adds a polygon to this path.
    /// </summary>
    public void AddPolygon(System.Windows.Point[] points)
    {
      // TODO: fill mode unclear here
      StartFigure();
#if !SILVERLIGHT
      this.pathGeometry.AddGeometry(GeometryHelper.CreatePolygonGeometry(points, XFillMode.Alternate, true));
#else
      // AGHACK: No AddGeometry in Silverlight version of PathGeometry
#endif
    }
#endif

#if GDI
    /// <summary>
    /// Adds a polygon to this path.
    /// </summary>
    public void AddPolygon(PointF[] points)
    {
      this.gdipPath.AddPolygon(points);
    }
#endif

    /// <summary>
    /// Adds a polygon to this path.
    /// </summary>
    public void AddPolygon(XPoint[] points)
    {
#if GDI
      this.gdipPath.AddPolygon(XGraphics.MakePointFArray(points));
#endif
#if WPF
      StartFigure();
#if !SILVERLIGHT
      this.pathGeometry.AddGeometry(GeometryHelper.CreatePolygonGeometry(XGraphics.MakePointArray(points), XFillMode.Alternate, true));
#else
      // AGHACK: No AddGeometry in Silverlight version of PathGeometry
#endif
#endif
    }

    // ----- AddPie -------------------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds the outline of a pie shape to this path.
    /// </summary>
    public void AddPie(Rectangle rect, double startAngle, double sweepAngle)
    {
      this.gdipPath.AddPie(rect, (float)startAngle, (float)sweepAngle);
    }
#endif

#if GDI
    /// <summary>
    /// Adds the outline of a pie shape to this path.
    /// </summary>
    public void AddPie(RectangleF rect, double startAngle, double sweepAngle)
    {
      AddPie((double)rect.X, (double)rect.Y, (double)rect.Width, (double)rect.Height, startAngle, sweepAngle);
    }
#endif

    /// <summary>
    /// Adds the outline of a pie shape to this path.
    /// </summary>
    public void AddPie(XRect rect, double startAngle, double sweepAngle)
    {
      AddPie(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
    }

    /// <summary>
    /// Adds the outline of a pie shape to this path.
    /// </summary>
    public void AddPie(int x, int y, int width, int height, double startAngle, double sweepAngle)
    {
      AddPie((double)x, (double)y, (double)width, (double)height, startAngle, sweepAngle);
    }

    /// <summary>
    /// Adds the outline of a pie shape to this path.
    /// </summary>
    public void AddPie(double x, double y, double width, double height, double startAngle, double sweepAngle)
    {
#if GDI
      this.gdipPath.AddPie((float)x, (float)y, (float)width, (float)height, (float)startAngle, (float)sweepAngle);
#endif
#if WPF
#endif
    }

    // ----- AddClosedCurve ------------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds a closed curve to this path.
    /// </summary>
    public void AddClosedCurve(System.Drawing.Point[] points)
    {
      AddClosedCurve(XGraphics.MakeXPointArray(points), 0.5);
    }
#endif

#if WPF
    /// <summary>
    /// Adds a closed curve to this path.
    /// </summary>
    public void AddClosedCurve(System.Windows.Point[] points)
    {
      AddClosedCurve(XGraphics.MakeXPointArray(points), 0.5);
    }
#endif

#if GDI
    /// <summary>
    /// Adds a closed curve to this path.
    /// </summary>
    public void AddClosedCurve(PointF[] points)
    {
      AddClosedCurve(XGraphics.MakeXPointArray(points), 0.5);
    }
#endif

    /// <summary>
    /// Adds a closed curve to this path.
    /// </summary>
    public void AddClosedCurve(XPoint[] points)
    {
      AddClosedCurve(points, 0.5);
    }

#if GDI
    /// <summary>
    /// Adds a closed curve to this path.
    /// </summary>
    public void AddClosedCurve(System.Drawing.Point[] points, double tension)
    {
      AddClosedCurve(XGraphics.MakeXPointArray(points), tension);
    }
#endif

#if WPF
    /// <summary>
    /// Adds a closed curve to this path.
    /// </summary>
    public void AddClosedCurve(System.Windows.Point[] points, double tension)
    {
      AddClosedCurve(XGraphics.MakeXPointArray(points), tension);
    }
#endif

#if GDI
    /// <summary>
    /// Adds a closed curve to this path.
    /// </summary>
    public void AddClosedCurve(PointF[] points, double tension)
    {
      AddClosedCurve(XGraphics.MakeXPointArray(points), tension);
    }
#endif

    /// <summary>
    /// Adds a closed curve to this path.
    /// </summary>
    public void AddClosedCurve(XPoint[] points, double tension)
    {
      if (points == null)
        throw new ArgumentNullException("points");
      int count = points.Length;
      if (count == 0)
        return;
      if (count < 2)
        throw new ArgumentException("Not enough points.", "points");
#if GDI
      this.gdipPath.AddClosedCurve(XGraphics.MakePointFArray(points), (float)tension);
#endif
#if WPF
      tension /= 3;

      StartFigure();
      PathFigure figure = CurrentPathFigure;
      figure.StartPoint = new System.Windows.Point(points[0].x, points[0].y);

      if (count == 2)
      {
        figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[0], points[0], points[1], points[1], tension));
      }
      else
      {
        figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[count - 1], points[0], points[1], points[2], tension));
        for (int idx = 1; idx < count - 2; idx++)
          figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[idx - 1], points[idx], points[idx + 1], points[idx + 2], tension));
        figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[count - 3], points[count - 2], points[count - 1], points[0], tension));
        figure.Segments.Add(GeometryHelper.CreateCurveSegment(points[count - 2], points[count - 1], points[0], points[1], tension));
      }
#endif
    }

    // ----- AddPath ------------------------------------------------------------------------------

    /// <summary>
    /// Adds the specified path to this path.
    /// </summary>
    public void AddPath(XGraphicsPath path, bool connect)
    {
#if GDI
      this.gdipPath.AddPath(path.gdipPath, connect);
#endif
#if WPF
#if !SILVERLIGHT
      this.pathGeometry.AddGeometry(path.pathGeometry);
#else
      // AGHACK: No AddGeometry in Silverlight version of PathGeometry
#endif
#endif
    }

    // ----- AddString ----------------------------------------------------------------------------

#if GDI
    /// <summary>
    /// Adds a text string to this path.
    /// </summary>
    public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, System.Drawing.Point origin, XStringFormat format)
    {
      AddString(s, family, style, emSize, new XRect(origin.X, origin.Y, 0, 0), format);
    }
#endif

#if WPF
    /// <summary>
    /// Adds a text string to this path.
    /// </summary>
    public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, System.Windows.Point origin, XStringFormat format)
    {
      AddString(s, family, style, emSize, new XPoint(origin), format);
    }
#endif

#if GDI
    /// <summary>
    /// Adds a text string to this path.
    /// </summary>
    public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, PointF origin, XStringFormat format)
    {
      AddString(s, family, style, emSize, new XRect(origin.X, origin.Y, 0, 0), format);
    }
#endif

    /// <summary>
    /// Adds a text string to this path.
    /// </summary>
    public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, XPoint origin, XStringFormat format)
    {
      try
      {
#if GDI
        this.gdipPath.AddString(s, family.gdiFamily, (int)style, (float)emSize, origin.ToPointF(), format.RealizeGdiStringFormat());
#endif
#if WPF
#if !SILVERLIGHT
        Typeface typeface = FontHelper.CreateTypeface(family.wpfFamily, style);
        //FormattedText ft = new FormattedText(s, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, emSize, System.Windows.Media.Brushes.Black);
        FormattedText formattedText = FontHelper.CreateFormattedText(s, typeface, emSize, System.Windows.Media.Brushes.Black);
        Geometry geo = formattedText.BuildGeometry(origin);
        this.pathGeometry.AddGeometry(geo);
#else
        // AGHACK: 
#endif
#endif
      }
      catch { }
    }

#if GDI
    /// <summary>
    /// Adds a text string to this path.
    /// </summary>
    public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, Rectangle layoutRect, XStringFormat format)
    {
      this.gdipPath.AddString(s, family.gdiFamily, (int)style, (float)emSize, layoutRect, format.RealizeGdiStringFormat());
    }
#endif

#if GDI
    /// <summary>
    /// Adds a text string to this path.
    /// </summary>
    public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, RectangleF layoutRect, XStringFormat format)
    {
      this.gdipPath.AddString(s, family.gdiFamily, (int)style, (float)emSize, layoutRect, format.RealizeGdiStringFormat());
    }
#endif

#if WPF
    /// <summary>
    /// Adds a text string to this path.
    /// </summary>
    public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, Rect rect, XStringFormat format)
    {
      //this.gdip Path.AddString(s, family.gdiFamily, (int)style, (float)emSize, layoutRect, format.RealizeGdiStringFormat());
      AddString(s, family, style, emSize, new XRect(rect), format);
    }
#endif

    /// <summary>
    /// Adds a text string to this path.
    /// </summary>
    public void AddString(string s, XFontFamily family, XFontStyle style, double emSize, XRect layoutRect, XStringFormat format)
    {
      if (s == null)
        throw new ArgumentNullException("s");
      if (family == null)
        throw new ArgumentNullException("family");

      if (format.LineAlignment == XLineAlignment.BaseLine && layoutRect.Height != 0)
        throw new InvalidOperationException("DrawString: With XLineAlignment.BaseLine the height of the layout rectangle must be 0.");

      if (s.Length == 0)
        return;

      if (format == null)
        format = XStringFormats.Default;

      XFont font = new XFont(family.Name, emSize, style);
#if GDI && !WPF
          RectangleF rc = layoutRect.ToRectangleF();
          if (format.LineAlignment == XLineAlignment.BaseLine)
          {
            double lineSpace = font.GetHeight();
            int cellSpace = font.FontFamily.GetLineSpacing(font.Style);
            int cellAscent = font.FontFamily.GetCellAscent(font.Style);
            int cellDescent = font.FontFamily.GetCellDescent(font.Style);
            double cyAscent = lineSpace * cellAscent / cellSpace;
            cyAscent = lineSpace * font.cellAscent / font.cellSpace;
            rc.Offset(0, (float)-cyAscent);
          }
          //this.gfx.DrawString(text, font.RealizeGdiFont(), brush.RealizeGdiBrush(), rect,
          //  format != null ? format.RealizeGdiStringFormat() : null);
      this.gdipPath.AddString(s, family.gdiFamily, (int)style, (float)emSize, rc, format.RealizeGdiStringFormat());
#endif
#if WPF && !GDI
#if !SILVERLIGHT
      // Just a first sketch, but currently we do not need it and there is enough to do...
      double x = layoutRect.X;
      double y = layoutRect.Y;

      //double lineSpace = font.GetHeight(this);
      //double cyAscent = lineSpace * font.cellAscent / font.cellSpace;
      //double cyDescent = lineSpace * font.cellDescent / font.cellSpace;

      //double cyAscent = family.GetCellAscent(style) * family.GetLineSpacing(style) / family.getl; //fontlineSpace * font.cellAscent / font.cellSpace;
      //double cyDescent =family.GetCellDescent(style); // lineSpace * font.cellDescent / font.cellSpace;
      double lineSpace = font.GetHeight();
      double cyAscent = lineSpace * font.cellAscent / font.cellSpace;
      double cyDescent = lineSpace * font.cellDescent / font.cellSpace;

      bool bold = (style & XFontStyle.Bold) != 0;
      bool italic = (style & XFontStyle.Italic) != 0;
      bool strikeout = (style & XFontStyle.Strikeout) != 0;
      bool underline = (style & XFontStyle.Underline) != 0;

      Typeface typeface = FontHelper.CreateTypeface(family.wpfFamily, style);
      //FormattedText formattedText = new FormattedText(s, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, emSize, System.Windows.Media.Brushes.Black);
      FormattedText formattedText = FontHelper.CreateFormattedText(s, typeface, emSize, System.Windows.Media.Brushes.Black);

      switch (format.Alignment)
      {
        case XStringAlignment.Near:
          // nothing to do, this is the default
          //formattedText.TextAlignment = TextAlignment.Left;
          break;

        case XStringAlignment.Center:
          x += layoutRect.Width / 2;
          formattedText.TextAlignment = TextAlignment.Center;
          break;

        case XStringAlignment.Far:
          x += layoutRect.Width;
          formattedText.TextAlignment = TextAlignment.Right;
          break;
      }
      //if (PageDirection == XPageDirection.Downwards)
      //{
      switch (format.LineAlignment)
      {
        case XLineAlignment.Near:
          //y += cyAscent;
          break;

        case XLineAlignment.Center:
          // TODO use CapHeight. PDFlib also uses 3/4 of ascent
          y += -formattedText.Baseline + (cyAscent * 2 / 4) + layoutRect.Height / 2;
          break;

        case XLineAlignment.Far:
          y += -formattedText.Baseline - cyDescent + layoutRect.Height;
          break;

        case XLineAlignment.BaseLine:
          y -= formattedText.Baseline;
          break;
      }
      //}
      //else
      //{
      //  // TODOWPF
      //  switch (format.LineAlignment)
      //  {
      //    case XLineAlignment.Near:
      //      //y += cyDescent;
      //      break;

      //    case XLineAlignment.Center:
      //      // TODO use CapHeight. PDFlib also uses 3/4 of ascent
      //      //y += -(cyAscent * 3 / 4) / 2 + rect.Height / 2;
      //      break;

      //    case XLineAlignment.Far:
      //      //y += -cyAscent + rect.Height;
      //      break;

      //    case XLineAlignment.BaseLine:
      //      // nothing to do
      //      break;
      //  }
      //}

      //if (bold && !descriptor.IsBoldFace)
      //{
      //  // TODO: emulate bold by thicker outline
      //}

      //if (italic && !descriptor.IsBoldFace)
      //{
      //  // TODO: emulate italic by shearing transformation
      //}

      if (underline)
      {
        //double underlinePosition = lineSpace * realizedFont.FontDescriptor.descriptor.UnderlinePosition / font.cellSpace;
        //double underlineThickness = lineSpace * realizedFont.FontDescriptor.descriptor.UnderlineThickness / font.cellSpace;
        //DrawRectangle(null, brush, x, y - underlinePosition, width, underlineThickness);
      }

      if (strikeout)
      {
        //double strikeoutPosition = lineSpace * realizedFont.FontDescriptor.descriptor.StrikeoutPosition / font.cellSpace;
        //double strikeoutSize = lineSpace * realizedFont.FontDescriptor.descriptor.StrikeoutSize / font.cellSpace;
        //DrawRectangle(null, brush, x, y - strikeoutPosition - strikeoutSize, width, strikeoutSize);
      }

      //this.dc.DrawText(formattedText, layoutRectangle.Location.ToPoint());
      //this.dc.DrawText(formattedText, new System.Windows.Point(x, y));

      Geometry geo = formattedText.BuildGeometry(new Point(x, y));
      this.pathGeometry.AddGeometry(geo);
#else
      // AGHACK
#endif
#endif
    }

    // ----- CloseAllFigures ----------------------------------------------------------------------

    // TODO? CloseAllFigures
    //public void CloseAllFigures();

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Closes the current figure and starts a new figure.
    /// </summary>
    public void CloseFigure()
    {
#if GDI
      this.gdipPath.CloseFigure();
#endif
#if WPF
      PathFigure figure = CurrentPathFigure;
      if (figure.Segments.Count != 0)
      {
        figure.IsClosed = true;
        //this.figure = null; // force start of new figure
        this.startNewFigure = true;
      }
#endif
    }

    /// <summary>
    /// Starts a new figure without closing the current figure.
    /// </summary>
    public void StartFigure()
    {
#if GDI
      this.gdipPath.StartFigure();
#endif
#if WPF
      PathFigure figure = CurrentPathFigure;
      if (figure.Segments.Count != 0)
      {
        figure = new PathFigure();
        this.pathGeometry.Figures.Add(figure);
      }
#endif
    }

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Gets or sets an XFillMode that determines how the interiors of shapes are filled.
    /// </summary>
    public XFillMode FillMode
    {
      get { return this.fillMode; }
      set
      {
        this.fillMode = value;
#if GDI
        this.gdipPath.FillMode = (FillMode)value;
#endif
#if WPF
        this.pathGeometry.FillRule = value == XFillMode.Winding ? FillRule.Nonzero : FillRule.EvenOdd;
#endif
      }
    }
    XFillMode fillMode;

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Converts each curve in this XGraphicsPath into a sequence of connected line segments. 
    /// </summary>
    public void Flatten()
    {
#if GDI
      this.gdipPath.Flatten();
#endif
#if WPF
#if !SILVERLIGHT
      this.pathGeometry = this.pathGeometry.GetFlattenedPathGeometry();
#else
      // AGHACK
#endif
#endif
    }

    /// <summary>
    /// Converts each curve in this XGraphicsPath into a sequence of connected line segments. 
    /// </summary>
    public void Flatten(XMatrix matrix)
    {
#if GDI
      this.gdipPath.Flatten(matrix.ToGdiMatrix());
#endif
#if WPF
#if !SILVERLIGHT
      this.pathGeometry = this.pathGeometry.GetFlattenedPathGeometry();
      this.pathGeometry.Transform = new MatrixTransform(matrix.ToWpfMatrix());
#else
      // AGHACK
#endif
#endif
    }

    /// <summary>
    /// Converts each curve in this XGraphicsPath into a sequence of connected line segments. 
    /// </summary>
    public void Flatten(XMatrix matrix, double flatness)
    {
#if GDI
      this.gdipPath.Flatten(matrix.ToGdiMatrix(), (float)flatness);
#endif
#if WPF
#if !SILVERLIGHT
      this.pathGeometry = this.pathGeometry.GetFlattenedPathGeometry();
      // TODO: matrix handling not yet tested
      if (!matrix.IsIdentity)
        this.pathGeometry.Transform = new MatrixTransform(matrix.ToWpfMatrix());
#else
      // AGHACK
#endif
#endif
    }

    // --------------------------------------------------------------------------------------------

    /// <summary>
    /// Replaces this path with curves that enclose the area that is filled when this path is drawn 
    /// by the specified pen.
    /// </summary>
    public void Widen(XPen pen)
    {
#if GDI
      this.gdipPath.Widen(pen.RealizeGdiPen());
#endif
#if WPF
#if !SILVERLIGHT
      this.pathGeometry = this.pathGeometry.GetWidenedPathGeometry(pen.RealizeWpfPen());
#else
      // AGHACK
#endif
#endif
    }

    /// <summary>
    /// Replaces this path with curves that enclose the area that is filled when this path is drawn 
    /// by the specified pen.
    /// </summary>
    public void Widen(XPen pen, XMatrix matrix)
    {
#if GDI
      this.gdipPath.Widen(pen.RealizeGdiPen(), matrix.ToGdiMatrix());
#endif
#if WPF
#if !SILVERLIGHT
      this.pathGeometry = this.pathGeometry.GetWidenedPathGeometry(pen.RealizeWpfPen());
#else
      // AGHACK
#endif
#endif
    }

    /// <summary>
    /// Replaces this path with curves that enclose the area that is filled when this path is drawn 
    /// by the specified pen.
    /// </summary>
    public void Widen(XPen pen, XMatrix matrix, double flatness)
    {
#if GDI
      this.gdipPath.Widen(pen.RealizeGdiPen(), matrix.ToGdiMatrix(), (float)flatness);
#endif
#if WPF
#if !SILVERLIGHT
      this.pathGeometry = this.pathGeometry.GetWidenedPathGeometry(pen.RealizeWpfPen());
#else
      // AGHACK
#endif
#endif
    }

    /// <summary>
    /// Grants access to internal objects of this class.
    /// </summary>
    public XGraphicsPathInternals Internals
    {
      get { return new XGraphicsPathInternals(this); }
    }
  }
}