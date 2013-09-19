using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;
using PdfSharp.Drawing.BarCodes;

namespace XDrawing.TestLab.Tester
{
  public class BarCodesTypes : TesterBase
  {
    /// <summary>
    /// Demonstrates serveral bar code types.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      XRect rc;
      base.RenderPage(gfx);

      Graphics grfx = gfx.Internals.Graphics;

      Code2of5Interleaved bc25 = new Code2of5Interleaved();
      bc25.Text = "123456";
      bc25.Size = new XSize(90, 30);
      //bc25.Direction = BarCodeDirection.RightToLeft;
      bc25.TextLocation = TextLocation.Above;
      gfx.DrawBarCode(bc25, XBrushes.DarkBlue, new XPoint(100, 100));

      CodeDataMatrix dm = new CodeDataMatrix("test", 26);
      dm.Size = new XSize(XUnit.FromMillimeter(15), XUnit.FromMillimeter(15));
      gfx.DrawMatrixCode(dm, XBrushes.DarkBlue, new XPoint(300, 100));

      rc = new XRect(30, 200, XUnit.FromCentimeter(9.3) + XUnit.FromMillimeter(0.5), XUnit.FromMillimeter(6));
      gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(128, XColors.LightSeaGreen)), rc);

      CodeOmr omr = new CodeOmr(0xF8F5FF3F.ToString(), rc.Size, CodeDirection.LeftToRight);
      omr.MakerDistance = XUnit.FromMillimeter(3);
      omr.MakerThickness = XUnit.FromMillimeter(0.5);
      gfx.DrawBarCode(omr, XBrushes.Black, rc.Center);

      omr.Direction = CodeDirection.RightToLeft;
      gfx.DrawBarCode(omr, XBrushes.Black, rc.Center + new XSize(0, 50));

      omr.Direction = CodeDirection.RightToLeft;
      gfx.DrawBarCode(omr, XBrushes.Black, rc.Center + new XSize(0, 50));

      omr.Direction = CodeDirection.TopToBottom;
      gfx.DrawBarCode(omr, XBrushes.Black, rc.Center + new XSize(300, 25));
    }

    public override string Description
    {
      get {return "Bar Code Types";}
    }
  }
}
