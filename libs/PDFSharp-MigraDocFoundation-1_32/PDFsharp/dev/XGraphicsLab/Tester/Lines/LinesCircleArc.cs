using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class LinesCircleArc : TesterBase
  {
    /// <summary>
    /// Test internal algorithm.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      float width = 200;
      float height = 200;
      //height = 150;

      RectangleF rect = new RectangleF(200, 300, width, height);
      Arc(gfx, rect,  20,  50);
    }

    public void Arc(XGraphics gfx, RectangleF rect, float startAngle, float sweepAngle)
    {
      RectangleF rect2 = RectangleF.Inflate(rect, rect.Width, 0);
      Box2(gfx, rect2, startAngle, sweepAngle);
      Box1(gfx, rect, startAngle, sweepAngle);
      //Box(gfx, rect, startAngle, sweepAngle);
      gfx.DrawArc(properties.Pen2.Pen, rect, startAngle, sweepAngle);
      gfx.DrawArc(properties.Pen3.Pen, rect2, 0, 90);
      gfx.DrawArc(properties.Pen2.Pen, rect2, startAngle, sweepAngle);

      float delta = (float)(Math.PI / 360);
      float rx = rect2.Width / 2;
      float ry = rect2.Height / 2;
      float dx = rect2.X + rx;
      float dy = rect2.Y + ry;
      for (float rad = 0; rad < Math.PI * 2; rad += delta)
      {
        gfx.DrawLine(XPens.LawnGreen, (float)(dx + rx * Math.Cos(rad)), (float)(dy + ry * Math.Sin(rad)), 
          (float)(dx + rx * Math.Cos(rad + delta)), (float)(dy + ry * Math.Sin(rad + delta)));
      }

      double gamma1 = (ry * Math.Cos(20 * 0.017453292519943295)) / (rx * Math.Sin(20 * 0.017453292519943295));
      gamma1 = Math.Atan(gamma1);
      gamma1 = 90 - gamma1 / 0.017453292519943295;
      double gamma2 = (ry * Math.Cos(70 * 0.017453292519943295)) / (rx * Math.Sin(70 * 0.017453292519943295));
      gamma2 = Math.Atan(gamma2);
      gamma2 = 90 - gamma2 / 0.017453292519943295 - gamma1;
      gfx.DrawArc(XPens.Black, rect2, (float)gamma1, (float)gamma2);
    }

    void Box1(XGraphics gfx, RectangleF rect, float startAngle, float sweepAngle)
    {
      float xc = rect.X + rect.Width / 2;
      float yc = rect.Y + rect.Height / 2;
      double a = startAngle * 0.017453292519943295;
      double b = (startAngle + sweepAngle) * 0.017453292519943295;

      gfx.DrawRectangle(XPens.Black, rect);

      //      for (float deg = 0; deg < 360; deg += 10)
      //        gfx.DrawLine(XPens.Yellow, xc, yc, 
      //          (float)(xc + rect.Width * Math.Cos(deg * 0.017453292519943295)), 
      //          (float)(yc + rect.Height * Math.Sin(deg * 0.017453292519943295)));

      float f = Math.Max(rect.Width, rect.Height) * 3 / 2;
      for (float deg = 0; deg < 360; deg += 10)
        gfx.DrawLine(XPens.Goldenrod, xc, yc, 
          (float)(xc + f * Math.Cos(deg * 0.017453292519943295)), 
          (float)(yc + f * Math.Sin(deg * 0.017453292519943295)));

      gfx.DrawLine(XPens.PaleGreen, xc, rect.Y, xc, rect.Y + rect.Height);
      gfx.DrawLine(XPens.PaleGreen, rect.X, yc, rect.X + rect.Width, yc);
      //gfx.DrawLine(XPens.DarkGray, xc, yc, (float)(xc + rect.Width / 2 * Math.Cos(a)), (float)(yc + rect.Height / 2 * Math.Sin(a)));
      //gfx.DrawLine(XPens.DarkGray, xc, yc, (float)(xc + rect.Width / 2 * Math.Cos(b)), (float)(yc + rect.Height / 2 * Math.Sin(b)));
    }

    void Box2(XGraphics gfx, RectangleF rect, float startAngle, float sweepAngle)
    {
      float xc = rect.X + rect.Width / 2;
      float yc = rect.Y + rect.Height / 2;
      double a = startAngle * 0.017453292519943295;
      double b = (startAngle + sweepAngle) * 0.017453292519943295;

      gfx.DrawRectangle(XPens.Black, rect);

      for (float deg = 0; deg < 360; deg += 10)
        gfx.DrawLine(XPens.Yellow, xc, yc, 
          (float)(xc + rect.Width * Math.Cos(deg * 0.017453292519943295)), 
          (float)(yc + rect.Height * Math.Sin(deg * 0.017453292519943295)));

      //if (rect.Width == rect.Height)
      //{
      //  float f = Math.Max(rect.Width / 2, rect.Height / 2);
      //  for (float deg = 0; deg < 360; deg += 10)
      //    gfx.DrawLine(XPens.Goldenrod, xc, yc, 
      //      (float)(xc + f * Math.Cos(deg * 0.017453292519943295)), 
      //      (float)(yc + f * Math.Sin(deg * 0.017453292519943295)));
      //}
      //gfx.DrawLine(XPens.PaleGreen, xc, rect.Y, xc, rect.Y + rect.Height);
      //gfx.DrawLine(XPens.PaleGreen, rect.X, yc, rect.X + rect.Width, yc);
      gfx.DrawLine(XPens.DarkGray, xc, yc, (float)(xc + rect.Width / 2 * Math.Cos(a)), (float)(yc + rect.Height / 2 * Math.Sin(a)));
      gfx.DrawLine(XPens.DarkGray, xc, yc, (float)(xc + rect.Width / 2 * Math.Cos(b)), (float)(yc + rect.Height / 2 * Math.Sin(b)));
    }

    public override string Description
    {
      get {return "DrawArc";}
    }
  }
}
