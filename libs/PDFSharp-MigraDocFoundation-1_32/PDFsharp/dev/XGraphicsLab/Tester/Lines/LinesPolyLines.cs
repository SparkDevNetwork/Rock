using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class LinesPolyLines : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawLines.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      //base.RenderPage(gfx);

      XPoint[] points = new XPoint[]
      {
        new XPoint(50, 100),
        new XPoint(450, 120),
        new XPoint(550, 300),
        new XPoint(150, 380),
        new XPoint(120, 190),
      };

      gfx.DrawLines(properties.Pen2.Pen, points);
    }

    public override string Description
    {
      get {return "DrawLine";}
    }
  }
}
