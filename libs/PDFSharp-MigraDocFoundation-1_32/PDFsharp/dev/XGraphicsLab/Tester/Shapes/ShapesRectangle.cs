using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class ShapesRectangle : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawRectangle.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      // Stroke rectangle
      gfx.DrawRectangle(properties.Pen1.Pen, 50, 100, 450, 150);

      // Fill rectangle
      gfx.DrawRectangle(properties.Brush2.Brush, new Rectangle(50, 300, 450, 150));

      // Stroke and fill rectangle
      gfx.DrawRectangle(properties.Pen1.Pen, properties.Brush2.Brush, new RectangleF(50, 500, 450, 150));
    }

    public override string Description
    {
      get {return "DrawRectangle";}
    }
  }
}
