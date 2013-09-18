using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class ShapesClipTest1 : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.Transform.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      gfx.Save();

      gfx.IntersectClip(new XRect(20, 20, 300, 500));
      gfx.DrawRectangle(XBrushes.Yellow, 0, 0, gfx.PageSize.Width, gfx.PageSize.Height);

      gfx.IntersectClip(new XRect(100, 200, 300, 500));
      gfx.DrawRectangle(XBrushes.LightBlue, 0, 0, gfx.PageSize.Width, gfx.PageSize.Height);

      gfx.DrawLine(XPens.MediumSlateBlue, 0, 0, 150, 200);
      gfx.DrawPolygon(properties.Pen1.Pen, GetPentagram(75, new PointF(150, 200)));


      Matrix matrix = new Matrix();
      //matrix.Scale(2f, 1.5f);
      //matrix.Translate(-200, -400);
      //matrix.Rotate(45);
      //matrix.Translate(200, 400);
      //gfx.Transform = matrix;
      //gfx.TranslateTransform(50, 30);

#if true
      gfx.TranslateTransform(30, 40, XMatrixOrder.Prepend);
      gfx.ScaleTransform(2.0f, 2.0f, XMatrixOrder.Prepend);
      gfx.RotateTransform(15, XMatrixOrder.Prepend);
#else
      gfx.TranslateTransform(30, 40, XMatrixOrder.Append);
      gfx.ScaleTransform(2.0f, 2.0f, XMatrixOrder.Append);
      gfx.RotateTransform(15, XMatrixOrder.Append);
#endif
      bool id = matrix.IsIdentity;
      matrix.Scale(2.0f, 2.0f, MatrixOrder.Prepend);
      //matrix.Translate(30, -50);
      matrix.Rotate(15, MatrixOrder.Prepend);
      //Matrix mtx = gfx.Transform.ToGdipMatrix();
      //gfx.Transform = matrix;

      gfx.DrawLine(XPens.MediumSlateBlue, 0, 0, 150, 200);
      gfx.DrawPolygon(properties.Pen2.Pen, GetPentagram(75, new PointF(150, 200)));

      //gfx.ResetClip();
      gfx.Restore();

      gfx.DrawLine(XPens.Red, 0, 0, 1000, 1000);

      gfx.DrawPolygon(XPens.SandyBrown, GetPentagram(75, new PointF(150, 200)));

    }

    public override string Description
    {
      get {return "Transform";}
    }
  }
}
