using System;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Tests...
  /// </summary>
  public class TextCodePage : TesterBase
  {
    public override void RenderPage(XGraphics gfx)
    {
      //gfx.Clear(this.properties.General.BackColor.Color);

      DrawCodePage(gfx, new XPoint(100, 100));
    }

    void DrawCodePage(XGraphics gfx, XPoint origin)
    {
      const double dx = 25;
      const double dy = 25;
      XFont labelFont = new XFont("Verdana", 10, XFontStyle.Bold);
      //XFont font = new XFont("Bauhaus", 16);
      XFont font = this.properties.Font1.Font;
      //XFont labelFont = font;
      //font = new XFont("Symbol", 16);
      Encoding encoding = Encoding.GetEncoding(1252);
      double asdf = XColors.LightGray.GS;
      //XBrush lighter = new XSolidBrush(XColor.FromGrayScale(XColor.LightGray.GS * 1.1));
      XBrush lighter = new XSolidBrush(XColor.FromGrayScale(0.9));

      XFontStyle style = font.Style;
      double lineSpace = font.GetHeight(gfx);
      int cellSpace   = font.FontFamily.GetLineSpacing(style);
      int cellAscent  = font.FontFamily.GetCellAscent(style);
      int cellDescent = font.FontFamily.GetCellDescent(style);
      int cellLeading = cellSpace - cellAscent - cellDescent;

      double ascent  = lineSpace * cellAscent / cellSpace;
      double descent = lineSpace * cellDescent / cellSpace;
      double leading = lineSpace * cellLeading / cellSpace;

      double x = origin.X + dx;
      double y = origin.Y;
      //for (int idx = 0; idx < 16; idx++)
      //  gfx.DrawString("x" + idx.ToString("X"), labelFont, XBrushes.DarkGray, x + idx * dx, y);
      for (int row = 0; row < 16; row++)
      {
        x = origin.X;
        y += dy;
        //gfx.DrawString(row.ToString("X") + "x", labelFont, XBrushes.DarkGray, x, y);
        for (int clm = 0; clm < 16; clm++)
        {
          x += dx;
          string glyph = encoding.GetString(new byte[1]{Convert.ToByte(row * 16 + clm)});
          glyph += "!";
          XSize size = gfx.MeasureString(glyph, font);
          gfx.DrawRectangle(XBrushes.LightGray, x, y - size.Height + descent, size.Width, size.Height);
          gfx.DrawRectangle(lighter, x, y - size.Height + descent, size.Width, leading);
          gfx.DrawRectangle(lighter, x, y, size.Width, descent);
          gfx.DrawString(glyph, font, XBrushes.Black, x, y);
        }
      }
    }

    public override string Description
    {
      get {return "DrawString";}
    }
  }
}
