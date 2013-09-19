using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of XGraphics.DrawLine.
  /// </summary>
  public class LinesStraightLines : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      gfx.DrawLine(properties.Pen1.Pen, 50, 100, 550, 100);
      gfx.DrawLine(properties.Pen2.Pen, 50, 200, 550, 200);
      gfx.DrawLine(properties.Pen3.Pen, 50, 300, 550, 300);

      XPen pen = properties.Pen2.Pen.Clone();

      pen.DashStyle = XDashStyle.Dash;
      gfx.DrawLine(pen, 50, 500, 550, 500);

      pen.DashStyle = XDashStyle.Dot;
      gfx.DrawLine(pen, 50, 550, 550, 550);

      pen.DashStyle = XDashStyle.DashDot;
      gfx.DrawLine(pen, 50, 600, 550, 600);

      pen.DashStyle = XDashStyle.DashDotDot;
      gfx.DrawLine(pen, 50, 650, 550, 650);

      // Custom pattern
      //pen.DashStyle = XDashStyle.Custom;
      pen.DashPattern = new double[] { 3, 1, 2.5, 1.5 };
      pen.Width = 7;

      pen.DashOffset = 1;
      gfx.DrawLine(pen, 50, 700, 550, 700);

      pen.DashOffset = 2;
      gfx.DrawLine(pen, 50, 720, 550, 720);

      pen.DashOffset = 4;
      gfx.DrawLine(pen, 50, 740, 550, 740);

    }

    //public override string Description
    //{
    //  get { return "DrawLine"; }
    //}
  }
}
