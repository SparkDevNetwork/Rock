using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class ShapesRoundedRectangle : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawRoundedRectangle.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

#if DEBUG
      gfx.WriteComment("begin DrawRoundedRectangle");
#endif
      // Stroke rounded rectangle
      gfx.DrawRoundedRectangle(properties.Pen1.Pen, 50, 100, 450, 150, 50, 30);
#if DEBUG
      gfx.WriteComment("end DrawRoundedRectangle");
#endif

      // Fill rounded rectangle
      gfx.DrawRoundedRectangle(properties.Brush2.Brush, new Rectangle(50, 300, 450, 150), new SizeF(30, 20));

      // Stroke and fill rounded rectangle
      gfx.DrawRoundedRectangle(properties.Pen2.Pen, properties.Brush2.Brush, 
        new XRect(50, 500, 450, 150),  new XSize(75, 75));
    }

    public override string Description
    {
      get {return "DrawRoundedRectangle";}
    }
  }
}
