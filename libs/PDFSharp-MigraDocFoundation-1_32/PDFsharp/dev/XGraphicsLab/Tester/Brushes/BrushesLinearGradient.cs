using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class BrushesLinearGradient : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XLinearGradientBrush.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      XRect rect;
      XLinearGradientBrush brush;
      Graphics grfx = gfx.Internals.Graphics;

      XLinearGradientBrush brush2 = 
        new XLinearGradientBrush(
        new XPoint(100, 100), 
        new XPoint(300, 300), 
        XColors.DarkRed, XColors.Yellow);

      //gfx.FillRectangle(brush, 0, 0, 600, 600);
      //gfx.TranslateTransform(35, 200);
      //gfx.RotateTransform(17);

      rect = new XRect(20, 50, 100, 200);
      brush = new XLinearGradientBrush(rect, 
        XColors.DarkRed, XColors.Yellow, XLinearGradientMode.Horizontal);
      gfx.DrawRectangle(XPens.Red, brush, rect);

      rect = new XRect(140, 50, 100, 200);
      brush = new XLinearGradientBrush(rect, 
        XColors.DarkRed, XColors.Yellow, XLinearGradientMode.Vertical);
        gfx.DrawRectangle(XPens.Red, brush, rect);

      rect = new XRect(260, 50, 100, 200);
      brush = new XLinearGradientBrush(rect, 
        XColors.DarkRed, XColors.Yellow, XLinearGradientMode.ForwardDiagonal);
        gfx.DrawRectangle(XPens.Red, brush, rect);

      rect = new XRect(380, 50, 100, 200);
      brush = new XLinearGradientBrush(rect, 
        XColors.DarkRed, XColors.Yellow, XLinearGradientMode.BackwardDiagonal);
        gfx.DrawRectangle(XPens.Red, brush, rect);


      gfx.TranslateTransform(80, 250);
      gfx.ScaleTransform(1.1);
      gfx.RotateTransform(20);

      rect = new XRect(20, 50, 100, 200);
      brush = new XLinearGradientBrush(rect, 
        XColors.Orange, XColors.DarkBlue, XLinearGradientMode.Horizontal);
      gfx.DrawRectangle(XPens.Red, brush, rect);

      rect = new XRect(140, 50, 100, 200);
      brush = new XLinearGradientBrush(rect, 
        XColors.Orange, XColors.DarkBlue, XLinearGradientMode.Vertical);
      gfx.DrawRectangle(XPens.Red, brush, rect);

      rect = new XRect(260, 50, 100, 200);
      brush = new XLinearGradientBrush(rect, 
        XColors.Orange, XColors.DarkBlue, XLinearGradientMode.ForwardDiagonal);
      gfx.DrawRectangle(XPens.Red, brush, rect);

      rect = new XRect(380, 50, 100, 200);
      brush = new XLinearGradientBrush(rect, 
        XColors.Orange, XColors.DarkBlue, XLinearGradientMode.BackwardDiagonal);
      gfx.DrawRectangle(XPens.Red, brush, rect);
    }

    public override string Description
    {
      get {return "XLinearGradientBrush";}
    }
  }
}
