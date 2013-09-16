using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class LinesBézierCurves : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawBeziers.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      int n = 2;
      int count = 1 + 3 * n;
      XPoint[] points = new XPoint[count];
      Random rnd = new Random(42);
      for (int idx = 0; idx < count; idx++)
      {
        points[idx].X = 20 + rnd.Next(600);
        points[idx].Y = 50 + rnd.Next(800);
      }

      // Draw the points
      XPen pen = new XPen(XColors.Red, 0.5);
      pen.DashStyle = XDashStyle.Dash;
      for (int idx = 0; idx + 3 < count; idx += 3)
      {
        gfx.DrawEllipse(XBrushes.Red, MakeRect(points[idx]));
        gfx.DrawEllipse(XBrushes.Red, MakeRect(points[idx + 1]));
        gfx.DrawEllipse(XBrushes.Red, MakeRect(points[idx + 2]));
        gfx.DrawEllipse(XBrushes.Red, MakeRect(points[idx + 3]));
        gfx.DrawLine(pen, points[idx], points[idx + 1]);
        gfx.DrawLine(pen, points[idx + 2], points[idx + 3]);
      }

      // Draw the curve
      gfx.DrawBeziers(properties.Pen2.Pen, points);
    }

    XRect MakeRect(XPoint pt)
    {
      int d = 5;
      return new XRect(pt.X - d, pt.Y - d, 2 * d, 2 * d);
    }

    public override string Description
    {
      get {return "DrawBeziers";}
    }
  }
}
