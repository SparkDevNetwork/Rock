using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of XGraphics.DrawPath.
  /// </summary>
  public class PathGlyph : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

#if true__
      XPen pen = new XPen(XColors.DarkGreen, 20);
      gfx.DrawLine(pen, 0, 0, 1000, 1000);
#endif

      XGraphicsPath path = new XGraphicsPath();

      path.AddString("@", new XFontFamily("Times New Roman"), XFontStyle.BoldItalic, 500, new XPoint(90, 60), XStringFormats.Default);
      gfx.DrawPath(properties.Pen2.Pen, properties.Brush2.Brush, path);
    }

    public override string Description
    {
      get { return "AddString & DrawPath"; }
    }
  }
}
