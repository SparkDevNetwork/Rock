using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of font transform.
  /// </summary>
  public class TextTransform2 : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      PointF[] origins = new PointF[] 
      { 
        new PointF(100, 200), new PointF(300, 200), 
        new PointF(100, 400), new PointF(300, 400),
        new PointF(100, 600), new PointF(350, 600),
      };
      PointF origin;
      XGraphicsContainer container;
      float length = 100;

      // Not transformed
      origin = origins[0];
      DrawAxes(gfx, XPens.Black, origin, length);
      gfx.DrawString(this.properties.Font2.Text, this.properties.Font2.Font, this.properties.Font2.Brush, origin);

      // Translation
      container = gfx.BeginContainer(new RectangleF(10, 10, 1, 1), new RectangleF(0, 0, 1, 1), XGraphicsUnit.Point);
      origin = origins[1];
      DrawAxes(gfx, XPens.Black, origin, length);
      gfx.TranslateTransform(20, -30);
      DrawAxes(gfx, XPens.DarkGray, origin, length);
      gfx.DrawString(this.properties.Font2.Text, this.properties.Font2.Font, this.properties.Font2.Brush, origin);
      gfx.EndContainer(container);

      // Scaling
      container = gfx.BeginContainer(new RectangleF(0, 0, 1, 1), new RectangleF(0, 0, 1, 1), XGraphicsUnit.Point);
      origin = origins[2];
      DrawAxes(gfx, XPens.Black, origin, length);
      gfx.TranslateTransform(origin.X, origin.Y);
      gfx.ScaleTransform(1.3f, 1.5f);
      DrawAxes(gfx, XPens.DarkGray, new PointF(), length);
      gfx.DrawString(this.properties.Font2.Text, this.properties.Font2.Font, this.properties.Font2.Brush, 0, 0);
      gfx.EndContainer(container);

      // Rotation
      container = gfx.BeginContainer(new RectangleF(0, 0, 1, 1), new RectangleF(0, 0, 1, 1), XGraphicsUnit.Point);
      origin = origins[3];  
      DrawAxes(gfx, XPens.Black, origin, length);
      gfx.TranslateTransform(origin.X, origin.Y);
      gfx.RotateTransform(-45);
      DrawAxes(gfx, XPens.DarkGray, new PointF(), length);
      gfx.DrawString(this.properties.Font2.Text, this.properties.Font2.Font, this.properties.Font2.Brush, 0 , 0);
      gfx.EndContainer(container);

      // Skewing (or shearing)
      container = gfx.BeginContainer(new RectangleF(0, 0, 1, 1), new RectangleF(0, 0, 1, 1), XGraphicsUnit.Point);
      origin = origins[4];  
      DrawAxes(gfx, XPens.Black, origin, length);
      gfx.TranslateTransform(origin.X, origin.Y);
      gfx.MultiplyTransform(new Matrix(1, -0.3f, -0.4f, 1, 0, 0));
      DrawAxes(gfx, XPens.DarkGray, new PointF(), length);
      gfx.DrawString(this.properties.Font2.Text, this.properties.Font2.Font, this.properties.Font2.Brush, 0 , 0);
      gfx.EndContainer(container);

      // Reflection
      container = gfx.BeginContainer(new RectangleF(0, 0, 1, 1), new RectangleF(0, 0, 1, 1), XGraphicsUnit.Point);
      origin = origins[5];  
      DrawAxes(gfx, XPens.Black, origin, length);
      gfx.TranslateTransform(origin.X, origin.Y);
      gfx.MultiplyTransform(new Matrix(-1, 0, 0, -1, 0, 0));
      DrawAxes(gfx, XPens.DarkGray, new PointF(), length);
      gfx.DrawString(this.properties.Font2.Text, this.properties.Font2.Font, this.properties.Font2.Brush, 0 , 0);
      gfx.EndContainer(container);
    }

    void DrawAxes(XGraphics gfx, XPen pen, PointF origin, float length)
    {
      gfx.DrawLines(pen, origin.X, origin.Y - length, origin.X, origin.Y, origin.X + length, origin.Y);
    }

    public override string Description
    {
      get {return "DrawString";}
    }
  }
}
