using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class ImagesTiff : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawImage.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);
      gfx.SmoothingMode = XSmoothingMode.None;

      //XImage image = XImage.FromFile(@"..\..\images\PNG (200x100).png");
      //XImage image = XImage.FromFile(@"..\..\images\CMYK balloons.tif");  
      //XImage image = XImage.FromFile(@"..\..\images\Dune.tif");  
      //XImage image = XImage.FromFile(@"..\..\images\Fern.tif");  
      XImage image = XImage.FromFile(@"../../images/Rose (RGB 8).tif");  

      gfx.DrawImage(image, 100, 100, 400, 400);
    }

    public override string Description
    {
      get {return "DrawImage";}
    }
  }
}
