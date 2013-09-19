using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class LinesArc : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawArc.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      double width = 230;
      double height = width / 2;
      //width  = 150;
      //height = 150;
#if true
      Arc(gfx, new XRect(50, 50, width, height),   10,  90);
      Arc(gfx, new XRect(320, 50, width, height),  10, -90);
      Arc(gfx, new XRect(50, 250, width, height),  90, 170);
      Arc(gfx, new XRect(320, 250, width, height), 80,  90);
      Arc(gfx, new XRect(50, 450, width, height), -30, 110);
      Arc(gfx, new XRect(320, 450, width, height), 15,  260);
      Arc(gfx, new XRect(50, 650, width, height),  20, -150);
      Arc(gfx, new XRect(320, 650, width, height), 110,-360);
#else
      int d = -320;
      Arc(gfx, new XRect(50, 50, width, height),    30, d);
      Arc(gfx, new XRect(320, 50, width, height),  120, d);
      Arc(gfx, new XRect(50, 250, width, height),  220, d);
      Arc(gfx, new XRect(320, 250, width, height), 310, d);

//      Arc(gfx, new XRect(50, 50, width, height),    90,  -90);
//      Arc(gfx, new XRect(320, 50, width, height),  180, -90);
//      Arc(gfx, new XRect(50, 250, width, height),  270, -90);
//      Arc(gfx, new XRect(320, 250, width, height), 360,  -90);

      Arc(gfx, new XRect(50, 450, width, height), -30, 30);
      Arc(gfx, new XRect(320, 450, width, height), 120, -25);
      Arc(gfx, new XRect(50, 650, width, height),  -140,  15);
      Arc(gfx, new XRect(320, 650, width, height),  140,  15);
#endif
    }

    public void Arc(XGraphics gfx, XRect rect, double startAngle, double sweepAngle)
    {
      Box(gfx, rect, startAngle, sweepAngle);

      gfx.DrawArc(properties.Pen3.Pen, rect, startAngle, sweepAngle);

      DrawHandMadeArc(gfx, XPens.Red, rect, startAngle, sweepAngle);
    }

    void Box(XGraphics gfx, XRect rect, double startAngle, double sweepAngle)
    {
      double xc = rect.X + rect.Width / 2;
      double yc = rect.Y + rect.Height / 2;
      double a = startAngle * 0.017453292519943295;
      double b = (startAngle + sweepAngle) * 0.017453292519943295;

      XGraphicsState state = gfx.Save();
      gfx.IntersectClip(rect);
#if true
#if true_
      for (double deg = 0; deg < 360; deg += 10)
        gfx.DrawLine(XPens.Yellow, xc, yc, 
          (xc + rect.Width / 2 * Math.Cos(deg * 0.017453292519943295)), 
          (yc + rect.Height / 2 * Math.Sin(deg * 0.017453292519943295)));
#endif
      double f = Math.Max(rect.Width / 2, rect.Height / 2);
      for (double deg = 0; deg < 360; deg += 10)
        gfx.DrawLine(XPens.Goldenrod, xc, yc, 
          (xc + f * Math.Cos(deg * 0.017453292519943295)), 
          (yc + f * Math.Sin(deg * 0.017453292519943295)));

      gfx.DrawLine(XPens.PaleGreen, xc, rect.Y, xc, rect.Y + rect.Height);
      gfx.DrawLine(XPens.PaleGreen, rect.X, yc, rect.X + rect.Width, yc);
      //gfx.DrawLine(XPens.DarkGray, xc, yc, (xc + rect.Width / 2 * Math.Cos(a)), (yc + rect.Height / 2 * Math.Sin(a)));
      //gfx.DrawLine(XPens.DarkGray, xc, yc, (xc + rect.Width / 2 * Math.Cos(b)), (yc + rect.Height / 2 * Math.Sin(b)));
#endif
      gfx.Restore(state);
      gfx.DrawRectangle(properties.Pen1.Pen, rect);
    }

    /// <summary>
    /// Draws an arc using trigonometry functions to proof visually that the underlying Bézier curves
    /// are correctly calculated.
    /// </summary>
    [Conditional("DEBUG")]
    public void DrawHandMadeArc(XGraphics gfx, XPen pen, XRect rect, double startAngle, double sweepAngle)
    {
      const double deg2rad = Math.PI / 180;
      double dx = rect.Width / 2;
      double dy = rect.Height / 2;
      double x0 = rect.X + dx;
      double y0 = rect.Y + dy;

      double a = startAngle;
      if (a < 0)
        a =  a + (1 + Math.Floor((Math.Abs(a) / 360))) * 360;
      else if (a > 360)
        a =  a - Math.Floor(a / 360) * 360;

      double b = startAngle + sweepAngle;
      if (b < 0)
        b =  b + (1 + Math.Floor((Math.Abs(b) / 360))) * 360;
      else if (b > 360)
        b =  b - Math.Floor(b / 360) * 360;

      a *= deg2rad;
      b *= deg2rad;

      if (rect.Width != rect.Height)
      {
        double sina = Math.Sin(a);
        if (Math.Abs(sina) > 1E-10)
        {
          if (a < Math.PI)
            a = Math.PI / 2 - Math.Atan(dy * Math.Cos(a) / (dx * sina));
          else
            a = 3 * Math.PI / 2 - Math.Atan(dy * Math.Cos(a) / (dx * sina));
        }
        double sinb = Math.Sin(b);
        if (Math.Abs(sinb) > 1E-10)
        {
          if (b < Math.PI)
            b = Math.PI / 2 - Math.Atan(dy * Math.Cos(b) / (dx * sinb));
          else
            b = 3 * Math.PI / 2 - Math.Atan(dy * Math.Cos(b) / (dx * sinb));
        }
      }

      int count;
      double adeg = a / deg2rad;
      double bdeg = b / deg2rad;
      if (adeg < bdeg)
      {
        if (sweepAngle > 0)
          count = (int)(bdeg - adeg) + 1;
        else
          count = 361 - (int)(bdeg - adeg);
      }
      else
      {
        if (sweepAngle > 0)
          count = 361 - (int)(adeg - bdeg);
        else
          count = (int)(adeg - bdeg) + 1;
      }
      if (count < 2)
        count = 2;
      XPoint[] points = new XPoint[count];
      double delta = sweepAngle > 0 ? deg2rad : -deg2rad;
#if true_
      gfx.DrawLine(XPens.Green, x0, y0, x0 + dx * Math.Cos(a), y0 + dy * Math.Sin(a));
      gfx.DrawLine(XPens.Green, x0, y0, x0 + dx * Math.Cos(b), y0 + dy * Math.Sin(b));
#endif
      for (int idx = 0; idx < count; idx++)
      {
        double angle;
        if (idx + 1 != count)
          angle = a + idx * delta;
        else
          angle = b;
        points[idx].X = x0 + dx * Math.Cos(angle);
        points[idx].Y = y0 + dy * Math.Sin(angle);
      }
      gfx.DrawLines(pen, points);
    }

    public override string Description
    {
      get {return "DrawArc";}
    }
  }
}
