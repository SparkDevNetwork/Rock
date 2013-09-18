using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class ImagesGif : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.DrawImage.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      XImage image = XImage.FromFile(@"..\..\images\Test.gif");  

      gfx.DrawImage(image, 100, 100, 400, 400);

      XImage image2 = XImage.FromFile(@"..\..\images\image009.gif");  

      gfx.DrawImage(image2, 100, 500, 400, 200);
    }

    public override string Description
    {
      get {return "DrawImage";}
    }
  }
}
