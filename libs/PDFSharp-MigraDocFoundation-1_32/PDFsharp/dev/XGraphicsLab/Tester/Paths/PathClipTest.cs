using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  public class PathClipTest : TesterBase
  {
    /// <summary>
    /// Demonstrates the use of XGraphics.SetClip.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      gfx.Save();
      gfx.TranslateTransform(50, 50);
      gfx.IntersectClip(new Rectangle(0, 0, 400, 250));
      gfx.TranslateTransform(50, 50);
      //gfx.Clear(XColor.GhostWhite);
      gfx.DrawEllipse(XPens.Green, XBrushes.Yellow, 40, 40, 500, 500);
      gfx.Restore();

      gfx.Save();
      //gfx.Transform = new XMatrix();  //XMatrix.Identity;
      gfx.TranslateTransform(200, 200);
      gfx.IntersectClip(new Rectangle(0, 0, 400, 250));
      gfx.DrawEllipse(XPens.Green, XBrushes.Yellow, 40, 40, 500, 500);
      gfx.Restore();
    }

    public override string Description
    {
      get { return "SetClip"; }
    }
  }
}
