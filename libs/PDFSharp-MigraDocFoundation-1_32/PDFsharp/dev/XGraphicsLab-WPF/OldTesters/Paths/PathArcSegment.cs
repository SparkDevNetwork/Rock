using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of XGraphics.SetClip.
  /// </summary>
  public class PathArcSegment : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);
      DrawGridlines(gfx);

      // Create a new graphical path
      XGraphicsPath path = new XGraphicsPath();

      XSize size = new XSize(90, 140);
      double rotationAngle = 130;

      path.AddArc(new XPoint(100, 100), new XPoint(200, 200), size, rotationAngle, false, System.Windows.Media.SweepDirection.Clockwise);
      path.StartFigure();
      path.AddArc(new XPoint(400, 100), new XPoint(500, 200), size, rotationAngle, false, System.Windows.Media.SweepDirection.Counterclockwise);
      path.StartFigure();
      path.AddArc(new XPoint(100, 300), new XPoint(200, 400), size, rotationAngle, true, System.Windows.Media.SweepDirection.Clockwise);
      path.StartFigure();
      path.AddArc(new XPoint(400, 300), new XPoint(500, 400), size, rotationAngle, true, System.Windows.Media.SweepDirection.Counterclockwise);
      path.StartFigure();

#if DEBUG_
      gfx.WriteComment("PathArcSegment");
#endif
      gfx.DrawPath(XPens.Red, path);
    }

    public override string Description
    {
      get { return "PathArcSegment"; }
    }
  }
}
