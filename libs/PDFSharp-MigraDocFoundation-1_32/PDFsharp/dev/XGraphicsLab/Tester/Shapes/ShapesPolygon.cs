using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class ShapesPolygon : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawPolygon.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      gfx.DrawPolygon(properties.Pen2.Pen, GetPentagram(75, new PointF(150, 200)));

      gfx.DrawPolygon(properties.Brush2.Brush, GetPentagram(75, new PointF(150, 400)), XFillMode.Alternate);

      gfx.DrawPolygon(properties.Brush2.Brush, GetPentagram(75, new PointF(350, 400)), XFillMode.Winding);

      gfx.DrawPolygon(properties.Pen2.Pen, properties.Brush2.Brush, GetPentagram(75, new PointF(150, 600)), XFillMode.Alternate);

      gfx.DrawPolygon(properties.Pen2.Pen, properties.Brush2.Brush, GetPentagram(75, new PointF(350, 600)), XFillMode.Winding);
    }

    public override string Description
    {
      get {return "DrawPolygon";}
    }
  }
}
