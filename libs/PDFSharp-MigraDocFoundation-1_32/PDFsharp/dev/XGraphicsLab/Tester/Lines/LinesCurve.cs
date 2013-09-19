using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of XGraphics.DrawCurve.
  /// </summary>
  public class LinesCurve : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      XPoint[] points = new XPoint[]
      {
        new XPoint(50, 100),
        new XPoint(450, 120),
        new XPoint(550, 300),
        new XPoint(150, 380),
      };
      gfx.DrawCurve(properties.Pen2.Pen, points, properties.General.Tension);
    }

    public override string Description
    {
      get {return "DrawCurve";}
    }
  }
}
