using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Draws an analog clock.
  /// </summary>
  public class MiscClock : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      this.dt = DateTime.Now;

      XPen   pen   = new XPen(XColors.DarkBlue, 5);
      XBrush brush = new XSolidBrush(properties.Brush2.Brush);

      InitializeCoordinates(gfx);
      DrawFace(gfx, pen, brush);
      DrawHourHand(gfx, pen, brush);
      DrawMinuteHand(gfx, pen, brush);
      DrawSecondHand(gfx, new XPen(XColors.Red, 7));
    }

    public override string Description
    {
      get {return "Clock";}
    }
  
    DateTime dt;

    /// <summary>
    /// Gets or sets the time to display.
    /// </summary>
    public DateTime Time
    {
      get {return this.dt;}
//      set 
//      { 
//        using (Graphics gfx = CreateGraphics())
//        {
//          InitializeCoordinates(gfx);
//
//          XPen pen = new XPen(BackColor);
//
//          if (this.dt.Hour != value.Hour)
//          {
//            DrawHourHand(gfx, pen);
//          }
//          if (this.dt.Minute != value.Minute)
//          {
//            DrawHourHand(gfx, pen);
//            DrawMinuteHand(gfx, pen);
//          }
//          if (this.dt.Second != value.Second)
//          {
//            DrawMinuteHand(gfx, pen);
//            DrawSecondHand(gfx, pen);
//          }
//          if (this.dt.Millisecond != value.Millisecond)
//          {
//            DrawSecondHand(gfx, pen);
//          }          
//          this.dt  = value;
//          pen = new XPen(ForeColor);
//
//          DrawHourHand(gfx, pen);
//          DrawMinuteHand(gfx, pen);
//          DrawSecondHand(gfx, pen);
//        }
//      }
    }

    void InitializeCoordinates(XGraphics gfx)
    {
      double width  = 600;
      double height = 800;

      gfx.TranslateTransform(width / 2, height / 2);

      //float fInches = Math.Min(width / gfx.DpiX, height / gfx.DpiY);
      double fInches = Math.Min(width, height);

      gfx.ScaleTransform(fInches * 1 / 2000);
    }

    void DrawFace(XGraphics gfx, XPen pen, XBrush brush)
    {
      for (int i = 0; i < 60; i++)
      {
        int iSize = i % 5 == 0 ? 100 : 30;

        gfx.DrawEllipse(pen, brush, 0 - iSize / 2, -900 - iSize / 2, iSize, iSize);
        gfx.RotateTransform(6);
      }          
    }

    protected void DrawHourHand(XGraphics gfx, XPen pen, XBrush brush)
    {
      XGraphicsState gs = gfx.Save();
      gfx.RotateTransform(360 * Time.Hour / 12 + 30 * Time.Minute / 60);

      gfx.DrawPolygon(pen, brush,
        new XPoint[]{new XPoint(0,  150), new XPoint(100, 0), new XPoint(0, -600), new XPoint(-100, 0)},
        XFillMode.Winding);
      gfx.Restore(gs);
    }

    protected void DrawMinuteHand(XGraphics gfx, XPen pen, XBrush brush)
    {
      XGraphicsState gs = gfx.Save();
      gfx.RotateTransform(360 * Time.Minute / 60 + 6 * Time.Second / 60);

      gfx.DrawPolygon(pen, brush, 
        new XPoint[]{new XPoint(0,  200), new XPoint(50, 0), new XPoint(0, -800), new XPoint(-50, 0)},
        XFillMode.Winding);
      gfx.Restore(gs);
    }

    protected void DrawSecondHand(XGraphics gfx, XPen pen)
    {
      XGraphicsState gs = gfx.Save();
      gfx.RotateTransform(360 * Time.Second / 60 + 6 * Time.Millisecond / 1000);

      gfx.DrawEllipse(new XSolidBrush(pen.Color), -15, -15, 30, 30);
      gfx.DrawLine(pen, 0, 40, 0, -800);
      gfx.Restore(gs);          
    }
  }
}
