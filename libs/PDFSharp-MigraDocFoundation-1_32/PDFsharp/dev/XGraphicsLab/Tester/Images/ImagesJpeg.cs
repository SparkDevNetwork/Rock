using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of XGraphics.DrawImage.
  /// </summary>
  public class ImagesJpeg : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      XImage image = XImage.FromFile(@"..\..\images\Z3.jpg");
      //XImage image = XImage.FromFile(@"..\..\images\CityLogo.jpeg");
      double width = image.PixelWidth;
      double height = image.PixelHeight;
      //double resolution = image.Format
      width.GetType();
      height.GetType();
      //gfx.DrawImage(image, 100, 100, 200, 200);
      gfx.DrawImage(image, 100, 100);
    }

    public override string Description
    {
      get {return "DrawImage";}
    }
  }
}
