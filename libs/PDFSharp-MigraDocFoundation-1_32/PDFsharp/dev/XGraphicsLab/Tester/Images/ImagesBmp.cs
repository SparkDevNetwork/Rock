using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class ImagesBmp : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawImage.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      XImage image = XImage.FromFile(@"..\..\images\Test.bmp");  

      double dx = gfx.PageSize.Width;
      double dy = gfx.PageSize.Height;

#if true
      gfx.TranslateTransform(dx / 2, dy / 2);
      gfx.RotateTransform(-60);
      gfx.TranslateTransform(-dx / 2, -dy / 2);
#endif

      gfx.DrawImage(image, (dx - image.PixelWidth) / 2, (dy - image.PixelHeight) / 2, image.PixelWidth, image.PixelHeight);
    }

    public override string Description
    {
      get {return "DrawImage";}
    }
  }
}
