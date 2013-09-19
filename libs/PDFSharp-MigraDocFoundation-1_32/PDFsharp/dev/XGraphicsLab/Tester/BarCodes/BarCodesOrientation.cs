using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;
using PdfSharp.Drawing.BarCodes;

namespace XDrawing.TestLab.Tester
{
  public class BarCodesOrientation : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of barcodes.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      Graphics grfx = gfx.Internals.Graphics;

      Code2of5Interleaved bc25 = new Code2of5Interleaved();
      bc25.Text = "123456";
      bc25.Size = new XSize(90, 30);
      //bc25.Direction = CodeDirection.RightToLeft;
      bc25.TextLocation = TextLocation.Above;
      gfx.DrawBarCode(bc25, XBrushes.DarkBlue, new XPoint(100, 100));

      bc25.Direction = CodeDirection.RightToLeft;
      gfx.DrawBarCode(bc25, XBrushes.DarkBlue, new XPoint(300, 100));

      bc25.Direction = CodeDirection.TopToBottom;
      gfx.DrawBarCode(bc25, XBrushes.DarkBlue, new XPoint(100, 300));

      bc25.Direction = CodeDirection.BottomToTop;
      gfx.DrawBarCode(bc25, XBrushes.Red, new XPoint(300, 300));

      Code3of9Standard bc39 = new Code3of9Standard("ISABEL123", new XSize(90, 40));
      
      bc39.TextLocation = TextLocation.AboveEmbedded;
      gfx.DrawBarCode(bc39, XBrushes.DarkBlue, new XPoint(100, 500));

      bc39.Direction = CodeDirection.RightToLeft;
      gfx.DrawBarCode(bc39, XBrushes.DarkBlue, new XPoint(300, 500));

      bc39.Text = "TITUS";
      bc39.Direction = CodeDirection.TopToBottom;
      gfx.DrawBarCode(bc39, XBrushes.DarkBlue, new XPoint(100, 700));

      bc39.Direction = CodeDirection.BottomToTop;
      gfx.DrawBarCode(bc39, XBrushes.Red, new XPoint(300, 700));

    }

    public override string Description
    {
      get {return "Bar Codes";}
    }
  }
}
