using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Checks some internal optimizations in PDF creation.
  /// </summary>
  public class RealizeTest : TesterBase
  {
    public RealizeTest()
    {
    }

    public override void RenderPage(XGraphics gfx)
    {
      //base.RenderPage(gfx);

      XFont font1 = new XFont("Times New Roman", 12);
      XFont font2 = new XFont("Courier New", 10, XFontStyle.Bold);

      gfx.WriteComment("Word 11");
      gfx.DrawString("Word 11", font1, XBrushes.Black, new XPoint(50, 100));

      gfx.WriteComment("Word 12");
      gfx.DrawString("Word 12", font1, XBrushes.Black, new XPoint(100, 100));


      gfx.WriteComment("Word 21");
      gfx.DrawString("Word 21", font2, XBrushes.Black, new XPoint(50, 200));

      gfx.WriteComment("Word 22");
      gfx.DrawString("Word 22", font2, XBrushes.Black, new XPoint(100, 200));

      gfx.WriteComment("Word 23");
      gfx.DrawString("Word 22", font2, XBrushes.Red, new XPoint(150, 200));

      gfx.WriteComment("Word 24");
      gfx.DrawString("Word 24", font2, XBrushes.Red, new XPoint(200, 200));
    }

    public override string Description
    {
      get {return "DrawString";}
    }
  }
}
