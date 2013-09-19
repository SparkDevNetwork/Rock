using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class LinesBézierCurve : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawBezier.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      gfx.DrawEllipse(XBrushes.Red, MakeRect(50, 100));
      gfx.DrawEllipse(XBrushes.Red, MakeRect(450, 100));
      gfx.DrawEllipse(XBrushes.Red, MakeRect(550, 190));
      gfx.DrawEllipse(XBrushes.Red, MakeRect(150, 300));

      gfx.DrawLine(XPens.Red, 50, 100, 450, 100);
      gfx.DrawLine(XPens.Red, 550, 190, 150, 300);

      gfx.DrawBezier(properties.Pen2.Pen, 50, 100, 450, 100, 550, 190, 150, 300);

      //XPoint[] points = new XPoint[1 + 3 * 3];
      //Random rnd = new Random();
      //for (int idx = 0; idx < points.Length; idx++)
      //{
      //  points[idx].X = 100 + rnd.Next(400);
      //  points[idx].Y = 200 + rnd.Next(700);
      //}
      //gfx.DrawBeziers(properties.Pen1.Pen, points);
    }

    XRect MakeRect(double x, double y)
    {
      int d = 5;
      return new XRect(x - d, y - d, 2 * d, 2 * d);
    }

    public override string Description
    {
      get {return "DrawBezier";}
    }
  }
}
