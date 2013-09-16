using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class ImagesPng : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawImage.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      XImage image = XImage.FromFile(@"..\..\images\test.png");  
      gfx.DrawImage(image, 100, 100, 400, 400);

      XImage image2 = XImage.FromFile(@"..\..\images\empira.png");  
      gfx.DrawImage(image2, 100, 500, 200, 200);

      XImage image3 = XImage.FromFile(@"..\..\images\reddot.png");  
      gfx.DrawImage(image3, 300, 500, 200, 200);
    }

    public override string Description
    {
      get {return "DrawImage";}
    }
  }
}
