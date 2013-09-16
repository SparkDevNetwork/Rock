using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class ShapesPie : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawPie.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      double width = 230;
      double height = 150;
      Pie1(gfx, new XRect(50, 50, width, height),    15,  75);
      Pie1(gfx, new XRect(320, 50, width, height),  95, -330);
      Pie1(gfx, new XRect(50, 250, width, height),  190, 45);
      Pie2(gfx, new XRect(320, 250, width, height), 280,  70);
      Pie1(gfx, new XRect(50, 450, width, height), -30, 1310);
      Pie2(gfx, new XRect(320, 450, width, height), 15,  160);
      Pie1(gfx, new XRect(50, 650, width, height),  20,  -150);
      Pie1(gfx, new XRect(320, 650, width, height),  90, -100);
    }

    public override string Description
    {
      get {return "DrawPie";}
    }

    public void Pie1(XGraphics gfx, XRect rect, double startAngle, double sweepAngle)
    {
      Box(gfx, rect, startAngle, sweepAngle);
      gfx.DrawPie(properties.Pen2.Pen, rect, startAngle, sweepAngle);
    }

    public void Pie2(XGraphics gfx, XRect rect, double startAngle, double sweepAngle)
    {
      Box(gfx, rect, startAngle, sweepAngle);
      gfx.DrawPie(properties.Pen2.Pen, properties.Brush2.Brush, rect, startAngle, sweepAngle);
    }

    void Box(XGraphics gfx, XRect rect, double startAngle, double sweepAngle)
    {
      double xc = rect.X + rect.Width / 2;
      double yc = rect.Y + rect.Height / 2;
      double a = startAngle * Deg2Rad;
      double b = (startAngle + sweepAngle) * Deg2Rad;

      XGraphicsState state = gfx.Save();
      gfx.IntersectClip(rect);

      double f = Math.Max(rect.Width / 2, rect.Height / 2);
      for (double deg = 0; deg < 360; deg += 10)
        gfx.DrawLine(XPens.Goldenrod, xc, yc, 
          (xc + f * Math.Cos(deg * 0.017453292519943295)), 
          (yc + f * Math.Sin(deg * 0.017453292519943295)));

      gfx.DrawLine(XPens.PaleGreen, xc, rect.Y, xc, rect.Y + rect.Height);
      gfx.DrawLine(XPens.PaleGreen, rect.X, yc, rect.X + rect.Width, yc);
      //gfx.DrawLine(XPens.DarkGray, xc, yc, (double)(xc + rect.Width / 2 * Math.Cos(a)), (double)(yc + rect.Height / 2 * Math.Sin(a)));
      //gfx.DrawLine(XPens.DarkGray, xc, yc, (double)(xc + rect.Width / 2 * Math.Cos(b)), (double)(yc + rect.Height / 2 * Math.Sin(b)));

      gfx.Restore(state);
      gfx.DrawRectangle(properties.Pen1.Pen, rect);
    }

  }
}
