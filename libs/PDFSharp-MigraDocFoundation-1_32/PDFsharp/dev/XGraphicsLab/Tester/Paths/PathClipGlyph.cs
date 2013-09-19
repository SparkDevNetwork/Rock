using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of XGraphics.SetClip.
  /// </summary>
  public class PathClipGlyph : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      // Create a new graphical path
      XGraphicsPath path = new XGraphicsPath();

      // Add the outline of the glyphs of the word 'Clip' to the path
      path.AddString("Clip!", new XFontFamily("Times New Roman"), XFontStyle.BoldItalic, 250, new XPoint(30, 100), XStringFormats.Default);

#if DEBUG_
      gfx.WriteComment("SetClip");
#endif
      // Set the path as clip path
      gfx.IntersectClip(path);
#if DEBUG_
      gfx.WriteComment("Random lines");
#endif
      // Draw some random lines to show that clipping happens
      Random rnd = new Random(42);
      for (int idx = 0; idx < 300; idx++)
        gfx.DrawLine(properties.Pen2.Pen, rnd.Next(600), rnd.Next(500), rnd.Next(600), rnd.Next(500));
    }

    public override string Description
    {
      get {return "SetClip(path)";}
    }
  }
}
