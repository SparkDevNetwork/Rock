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
using System.IO;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows.Media;
#endif
using PdfSharp.Internal;

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Represents an abstract drawing surface for PdfPages.
  /// </summary>
  internal interface IXGraphicsRenderer
  {
    void Close();

    #region Drawing

    /// <summary>
    /// Fills the entire drawing surface with the specified color.
    /// </summary>
    void Clear(XColor color);

    /// <summary>
    /// Draws a straight line.
    /// </summary>
    void DrawLine(XPen pen, double x1, double y1, double x2, double y2);

    /// <summary>
    /// Draws a series of straight lines.
    /// </summary>
    void DrawLines(XPen pen, XPoint[] points);

    /// <summary>
    /// Draws a Bézier spline.
    /// </summary>
    void DrawBezier(XPen pen, double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4);

    /// <summary>
    /// Draws a series of Bézier splines.
    /// </summary>
    void DrawBeziers(XPen pen, XPoint[] points);

    /// <summary>
    /// Draws a cardinal spline.
    /// </summary>
    void DrawCurve(XPen pen, XPoint[] points, double tension);

    /// <summary>
    /// Draws an arc.
    /// </summary>
    void DrawArc(XPen pen, double x, double y, double width, double height, double startAngle, double sweepAngle);

    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    void DrawRectangle(XPen pen, XBrush brush, double x, double y, double width, double height);

    /// <summary>
    /// Draws a series of rectangles.
    /// </summary>
    void DrawRectangles(XPen pen, XBrush brush, XRect[] rects);

    /// <summary>
    /// Draws a rectangle with rounded corners.
    /// </summary>
    void DrawRoundedRectangle(XPen pen, XBrush brush, double x, double y, double width, double height, double ellipseWidth, double ellipseHeight);

    /// <summary>
    /// Draws an ellipse.
    /// </summary>
    void DrawEllipse(XPen pen, XBrush brush, double x, double y, double width, double height);

    /// <summary>
    /// Draws a polygon.
    /// </summary>
    void DrawPolygon(XPen pen, XBrush brush, XPoint[] points, XFillMode fillmode);

    /// <summary>
    /// Draws a pie.
    /// </summary>
    void DrawPie(XPen pen, XBrush brush, double x, double y, double width, double height, double startAngle, double sweepAngle);

    /// <summary>
    /// Draws a cardinal spline.
    /// </summary>
    void DrawClosedCurve(XPen pen, XBrush brush, XPoint[] points, double tension, XFillMode fillmode);

    /// <summary>
    /// Draws a graphical path.
    /// </summary>
    void DrawPath(XPen pen, XBrush brush, XGraphicsPath path);

    /// <summary>
    /// Draws a series of glyphs identified by the specified text and font.
    /// </summary>
    void DrawString(string s, XFont font, XBrush brush, XRect layoutRectangle, XStringFormat format);

    /// <summary>
    /// Draws an image.
    /// </summary>
    void DrawImage(XImage image, double x, double y, double width, double height);
    void DrawImage(XImage image, XRect destRect, XRect srcRect, XGraphicsUnit srcUnit);

    #endregion

    #region Save and Restore

    /// <summary>
    /// Saves the current graphics state without changing it.
    /// </summary>
    void Save(XGraphicsState state);

    /// <summary>
    /// Restores the specified graphics state.
    /// </summary>
    void Restore(XGraphicsState state);

    /// <summary>
    /// 
    /// </summary>
    void BeginContainer(XGraphicsContainer container, XRect dstrect, XRect srcrect, XGraphicsUnit unit);

    /// <summary>
    /// 
    /// </summary>
    void EndContainer(XGraphicsContainer container);

    #endregion

    #region Transformation

    //void TranslateTransform(double dx, double dy, XMatrixOrder order);
    //void ScaleTransform(double scaleX, double scaleY, XMatrixOrder order);
    //void ScaleTransform(double scaleXY, XMatrixOrder order);
    //void RotateTransform(double angle, XMatrixOrder order);
    //void MultiplyTransform(XMatrix matrix, XMatrixOrder order);

    /// <summary>
    /// Sets all values that influence the page transformation.
    /// </summary>
    void SetPageTransform(XPageDirection direction, XPoint origion, XGraphicsUnit unit);

    /// <summary>
    /// Gets or sets the transformation matrix.
    /// </summary>
    XMatrix Transform {/*get;*/ set;}

    #endregion

    #region Clipping

    void SetClip(XGraphicsPath path, XCombineMode combineMode);
    
    void ResetClip();

    //public void SetClip(GraphicsPath path);
    //public void SetClip(Graphics g);
    //public void SetClip(Rectangle rect);
    //public void SetClip(XRect rect);
    //public void SetClip(GraphicsPath path, CombineMode combineMode);
    //public void SetClip(Graphics g, CombineMode combineMode);
    //public void SetClip(Rectangle rect, CombineMode combineMode);
    //public void SetClip(XRect rect, CombineMode combineMode);
    //public void SetClip(Region region, CombineMode combineMode);
    //public void ExcludeClip(Rectangle rect);
    //public void ExcludeClip(Region region);
    //public void IntersectClip(Rectangle rect);
    //public void IntersectClip(XRect rect);
    //public void IntersectClip(Region region);

    #endregion

    #region Miscellaneous
    /// <summary>
    /// Writes a comment to the output stream. Comments have no effect on the rendering of the output.
    /// </summary>
    void WriteComment(string comment);
    #endregion
  }
}
