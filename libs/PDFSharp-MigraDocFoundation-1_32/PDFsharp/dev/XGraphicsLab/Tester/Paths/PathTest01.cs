using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of XGraphics.DrawPath.
  /// </summary>
  public class PathTest01 : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      XGraphicsPath path = new XGraphicsPath();

      path.AddLine(50, 150, 50, 100);
      path.AddArc(50, 50, 100, 100, -180, 180);
      path.AddLine(150, 70, 200, 70);
      path.AddLine(200, 70, 200, 150);
      path.CloseFigure();
      gfx.DrawPath(properties.Pen2.Pen, properties.Brush2.Brush, path);
    }

    public override string Description
    {
      get {return "DrawPath";}
    }
  }
}
