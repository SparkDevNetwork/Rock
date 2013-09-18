using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;
using PdfSharp.Drawing.BarCodes;

namespace XDrawing.TestLab.Tester
{
  public class BarCodesDataMatrix : TesterBase
  {
    /// <summary>
    /// Demonstrates serveral bar code types.
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      XFont font = new XFont("Verdana", 14, XFontStyle.Bold);
      string info = "DataMatrix is a fake in the Open Source version!";
      XSize size = gfx.MeasureString(info, font);
      gfx.DrawString(info, font, XBrushes.Firebrick, (600 - size.Width) / 2, 50);


      //Graphics grfx = gfx.Internals.Graphics;

      CodeDataMatrix dm = new CodeDataMatrix("test", 26);
      dm.Size = new XSize(XUnit.FromMillimeter(15), XUnit.FromMillimeter(15));
      gfx.DrawMatrixCode(dm, XBrushes.DarkBlue, new XPoint(100, 100));

      dm = new CodeDataMatrix("test", 12, 12);
      dm.Size = new XSize(XUnit.FromMillimeter(15), XUnit.FromMillimeter(15));
      gfx.DrawMatrixCode(dm, XBrushes.DarkBlue, new XPoint(300, 100));

      dm = new CodeDataMatrix("test", 16, 48);
      dm.Size = new XSize(XUnit.FromMillimeter(50), XUnit.FromMillimeter(18));
      gfx.DrawMatrixCode(dm, XBrushes.DarkBlue, new XPoint(500, 100));

      dm = new CodeDataMatrix("0123456789", 52);
      dm.Size = new XSize(XUnit.FromMillimeter(30), XUnit.FromMillimeter(30));
      gfx.DrawMatrixCode(dm, XBrushes.DarkBlue, new XPoint(100, 300));

      dm = new CodeDataMatrix("0123456789", 12, 26);
      dm.Direction = CodeDirection.TopToBottom;
      dm.Size = new XSize(XUnit.FromMillimeter(14), XUnit.FromMillimeter(7));
      gfx.DrawMatrixCode(dm, XBrushes.DarkBlue, new XPoint(300, 300));

      dm = new CodeDataMatrix("0123456789", 96);
      dm.Size = new XSize(XUnit.FromMillimeter(30), XUnit.FromMillimeter(30));
      gfx.DrawMatrixCode(dm, XBrushes.DarkBlue, new XPoint(500, 300));

      dm = new CodeDataMatrix("www.empira.de", 20);
      dm.Direction = CodeDirection.BottomToTop;
      dm.Size = new XSize(XUnit.FromMillimeter(7), XUnit.FromMillimeter(7));
      gfx.DrawMatrixCode(dm, XBrushes.DarkBlue, new XPoint(100, 500));

      dm = new CodeDataMatrix("www.empira.de", 144, 144, 2);
      dm.Size = new XSize(XUnit.FromMillimeter(50), XUnit.FromMillimeter(50));
      gfx.DrawMatrixCode(dm, XBrushes.DarkBlue, new XPoint(300, 500));

      dm = new CodeDataMatrix("www.empira.de", 88);
      dm.Direction = CodeDirection.RightToLeft;
      dm.Size = new XSize(XUnit.FromMillimeter(15), XUnit.FromMillimeter(15));
      gfx.DrawMatrixCode(dm, XBrushes.DarkBlue, new XPoint(500, 500));
    }

    public override string Description
    {
      get { return "Bar Code Data Matrix"; }
    }
  }
}
