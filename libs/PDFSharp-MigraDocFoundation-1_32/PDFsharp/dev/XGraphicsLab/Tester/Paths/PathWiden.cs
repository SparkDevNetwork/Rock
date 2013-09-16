using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of XGraphics.DrawPath.
  /// </summary>
  public class PathWiden : TesterBase
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
      XPen pen = new XPen(XColors.Red, 50);

      path.Widen(pen, new XMatrix(), 3);
      path.FillMode = this.properties.General.FillMode;
      gfx.DrawPath(properties.Pen2.Pen, properties.Brush2.Brush, path);
    }

    public override string Description
    {
      get { return "Widen Path"; }
    }
  }
}
