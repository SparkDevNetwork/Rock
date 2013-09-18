using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of XGraphics.DrawImage.
  /// </summary>
  public class ImagesFormXObject : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      XImage image = XImage.FromFile(@"..\..\..\..\samples\PDFs\SomeLayout.pdf");

      double dx = gfx.PageSize.Width;
      double dy = gfx.PageSize.Height;

#if true
      gfx.TranslateTransform(dx / 2, dy / 2);
      gfx.RotateTransform(-25);
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
