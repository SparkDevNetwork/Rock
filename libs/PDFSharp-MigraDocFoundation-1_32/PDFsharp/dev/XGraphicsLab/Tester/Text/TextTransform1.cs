using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of font transform.
  /// </summary>
  public class TextTransform1 : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      //base.RenderPage(gfx);

      XPoint[] origins = new XPoint[] 
      { 
        new XPoint(100, 200), new XPoint(300, 200), 
        new XPoint(100, 400), new XPoint(300, 400),
        new XPoint(100, 600), new XPoint(350, 600),
      };

      XPoint origin;
      XGraphicsState state;
      float length = 100;

      // Not transformed
      origin = origins[0];
      DrawAxes(gfx, XPens.Black, origin, length);
      gfx.DrawString(this.properties.Font1.Text, this.properties.Font1.Font, this.properties.Font1.Brush, origin);

      // Translation
      state = gfx.Save();
      origin = origins[1];
      DrawAxes(gfx, XPens.Black, origin, length);
      gfx.TranslateTransform(20, -30);
      DrawAxes(gfx, XPens.DarkGray, origin, length);
      gfx.DrawString(this.properties.Font1.Text, this.properties.Font1.Font, this.properties.Font1.Brush, origin);
      gfx.Restore(state);

#if true
      // Scaling
      state = gfx.Save();
      origin = origins[2];
      DrawAxes(gfx, XPens.Black, origin, length);
      gfx.TranslateTransform(origin.X, origin.Y);
      gfx.ScaleTransform(1.3, 1.5);
      DrawAxes(gfx, XPens.DarkGray, new XPoint(), length);
      gfx.DrawString(this.properties.Font1.Text, this.properties.Font1.Font, this.properties.Font1.Brush, 0, 0);
      gfx.Restore(state);

      // Rotation
      state = gfx.Save();
      origin = origins[3];
      DrawAxes(gfx, XPens.Black, origin, length);
      gfx.TranslateTransform(origin.X, origin.Y);
      gfx.RotateTransform(-45);
      DrawAxes(gfx, XPens.DarkGray, new XPoint(), length);
      gfx.DrawString(this.properties.Font1.Text, this.properties.Font1.Font, this.properties.Font1.Brush, 0, 0);
      gfx.Restore(state);

      // Skewing (or shearing)
      state = gfx.Save();
      origin = origins[4];
      DrawAxes(gfx, XPens.Black, origin, length);
      gfx.TranslateTransform(origin.X, origin.Y);
      gfx.MultiplyTransform(new Matrix(1, -0.3f, -0.4f, 1, 0, 0));
      DrawAxes(gfx, XPens.DarkGray, new XPoint(), length);
      gfx.DrawString(this.properties.Font1.Text, this.properties.Font1.Font, this.properties.Font1.Brush, 0, 0);
      gfx.Restore(state);

      // Reflection
      state = gfx.Save();
      origin = origins[5];
      DrawAxes(gfx, XPens.Black, origin, length);
      gfx.TranslateTransform(origin.X, origin.Y);
      gfx.MultiplyTransform(new Matrix(-1, 0, 0, -1, 0, 0));
      DrawAxes(gfx, XPens.DarkGray, new XPoint(), length);
      gfx.DrawString(this.properties.Font1.Text, this.properties.Font1.Font, this.properties.Font1.Brush, 0, 0);
      gfx.Restore(state);
#endif
    }

    void DrawAxes(XGraphics gfx, XPen pen, XPoint origin, float length)
    {
      gfx.DrawLines(pen, origin.X, origin.Y - length, origin.X, origin.Y, origin.X + length, origin.Y);
    }

    public override string Description
    {
      get { return "DrawString transformation"; }
    }
  }
}