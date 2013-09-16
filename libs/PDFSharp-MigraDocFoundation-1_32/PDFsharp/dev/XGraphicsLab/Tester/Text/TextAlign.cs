using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Demonstrates the use of font alingment.
  /// </summary>
  public class TextAlign : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      string facename = "Times";
      float size = 24;
      XFont fontR = new XFont(facename, size, this.properties.Font1.Style);
      fontR = this.properties.Font1.Font;
      //XFont fontB = new XFont(facename, size, XFontStyle.Bold);
      //XFont fontI = new XFont(facename, size, XFontStyle.Italic);
      //XFont fontBI = new XFont(facename, size, XFontStyle.Bold | XFontStyle.Italic);
      //gfx.DrawString("Hello", this.properties.Font1.Font, this.properties.Font1.Brush, 200, 200);
      float x0 = 80;
      float x1 = 520;
      float y0 = 80;
      float y1 = 760 / 2;
      RectangleF rect = new RectangleF(x0, y0, x1 - x0, y1 - y0);
      XPen pen = XPens.SlateBlue;
      gfx.DrawRectangle(pen, rect);
      gfx.DrawLine(pen, (x0 + x1) / 2, y0, (x0 + x1) / 2, y1);
      gfx.DrawLine(pen, x0, (y0 + y1) / 2, x1, (y0 + y1) / 2);

      XSolidBrush brush = this.properties.Font1.Brush;
      XStringFormat format = new XStringFormat();

      double lineSpace = fontR.GetHeight(gfx);
      int cellSpace = fontR.FontFamily.GetLineSpacing(fontR.Style);
      int cellAscent = fontR.FontFamily.GetCellAscent(fontR.Style);
      int cellDescent = fontR.FontFamily.GetCellDescent(fontR.Style);
      double cyAscent = lineSpace * cellAscent / cellSpace;

      gfx.DrawString("TopLeft", fontR, brush, rect, format);

      format.Alignment = XStringAlignment.Center;
      gfx.DrawString("TopCenter", fontR, brush, rect, format);

      format.Alignment = XStringAlignment.Far;
      gfx.DrawString("TopRight", fontR, brush, rect, format);

      format.LineAlignment= XLineAlignment.Center;
      format.Alignment = XStringAlignment.Near;
      gfx.DrawString("CenterLeft", fontR, brush, rect, format);

      format.Alignment = XStringAlignment.Center;
      gfx.DrawString("Center", fontR, brush, rect, format);

      format.Alignment = XStringAlignment.Far;
      gfx.DrawString("CenterRight", fontR, brush, rect, format);


      format.LineAlignment= XLineAlignment.Far;
      format.Alignment = XStringAlignment.Near;
      gfx.DrawString("BottomLeft", fontR, brush, rect, format);

      format.Alignment = XStringAlignment.Center;
      gfx.DrawString("BottomCenter", fontR, brush, rect, format);

      format.Alignment = XStringAlignment.Far;
      gfx.DrawString("BottomRight", fontR, brush, rect, format);


//      format.LineAlignment= XLineAlignment.Center;
//      format.Alignment = XStringAlignment.Center;
//      gfx.DrawString("CenterLeft", fontR, brush, rect, format);

//      gfx.DrawString("Times 40", fontR, this.properties.Font1.Brush, x, 200);
//      gfx.DrawString("Times bold 40", fontB, this.properties.Font1.Brush, x, 300);
//      gfx.DrawString("Times italic 40", fontI, this.properties.Font1.Brush, x, 400);
//      gfx.DrawString("Times bold italic 40", fontBI, this.properties.Font1.Brush, x, 500);
    }

    public override string Description
    {
      get {return "Alignment";}
    }
  }
}
