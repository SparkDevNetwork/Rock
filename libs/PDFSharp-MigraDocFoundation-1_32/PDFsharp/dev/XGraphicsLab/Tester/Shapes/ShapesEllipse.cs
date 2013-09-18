using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class ShapesEllipse : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawRoundedRectangle.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      // Stroke ellipse
      gfx.DrawEllipse(properties.Pen1.Pen, 50, 100, 450, 150);

      // Fill ellipse
      gfx.DrawEllipse(properties.Brush2.Brush, new Rectangle(50, 300, 450, 150));

      // Stroke and fill ellipse
      gfx.DrawEllipse(properties.Pen2.Pen, properties.Brush2.Brush, new RectangleF(50, 500, 450, 150));

      // Stroke circle
      gfx.DrawEllipse(properties.Pen2.Pen, new XRect(100, 200, 400, 400));

#if DEBUG
      int count = 360;
      XPoint[] circle = new XPoint[count];
      for (int idx = 0; idx < count; idx++)
      {
        double rad = idx * 2 * Math.PI / count;
        circle[idx].X = Math.Cos(rad) * 200 + 300;
        circle[idx].Y = Math.Sin(rad) * 200 + 400;
      }
      gfx.DrawPolygon(properties.Pen3.Pen, circle);
#endif
    }

    public override string Description
    {
      get {return "DrawEllipse";}
    }
  }
}
