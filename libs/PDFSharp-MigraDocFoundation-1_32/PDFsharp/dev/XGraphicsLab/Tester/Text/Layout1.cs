using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Checks some internal optimizations in PDF creation.
  /// </summary>
  public class Layout1 : TesterBase
  {
    public Layout1()
    {
    }

    public override void RenderPage(XGraphics gfx)
    {
      //base.RenderPage(gfx);

      XTextFormatter tf = new XTextFormatter(gfx);
      XRect rect;
      string text = this.properties.Font1.Text;
      //text = "First\nSecond Line\nlaksdjf 234 234";

      rect = new XRect(40, 100, 250, 200);
      gfx.DrawRectangle(XBrushes.SeaShell, rect);
      //tf.Alignment = ParagraphAlignment.Left;
      tf.DrawString(text, this.properties.Font1.Font, this.properties.Font1.Brush, 
        rect, XStringFormats.TopLeft);

      rect = new XRect(310, 100, 250, 200);
      gfx.DrawRectangle(XBrushes.SeaShell, rect);
      tf.Alignment = XParagraphAlignment.Right;
      tf.DrawString(text, this.properties.Font1.Font, this.properties.Font1.Brush,
        rect, XStringFormats.TopLeft);

      rect = new XRect(40, 400, 250, 200);
      gfx.DrawRectangle(XBrushes.SeaShell, rect);
      tf.Alignment = XParagraphAlignment.Center;
      tf.DrawString(text, this.properties.Font1.Font, this.properties.Font1.Brush,
        rect, XStringFormats.TopLeft);

      rect = new XRect(310, 400, 250, 200);
      gfx.DrawRectangle(XBrushes.SeaShell, rect);
      tf.Alignment = XParagraphAlignment.Justify;
      tf.DrawString(text, this.properties.Font1.Font, this.properties.Font1.Brush,
        rect, XStringFormats.TopLeft);
    }

    public override string Description
    {
      get {return "Layout 1";}
    }
  }
}
