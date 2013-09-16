using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Base class for all Tester classes.
  /// </summary>
  public abstract class TesterBase
  {
    public TesterBase()
    {
      this.properties = XGraphicsLab.properties;
    }

    public virtual void RenderPage(XGraphics gfx)
    {
      //Debug.WriteLine("RenderPage");
      //gfx.Clear(this.properties.General.BackColor.Color);
      //DrawGridlines(gfx);
      gfx.SmoothingMode = XSmoothingMode.AntiAlias;
    }

    protected XFont CreateFont(string familyName, double emSize)
    {
      XFont font;
      try
      {
        font = new XFont(familyName, emSize);
      }
      catch
      {
        font = new XFont("Courier", emSize);
      }
      return font;
    }

    protected XFont CreateFont(string familyName, double emSize, XFontStyle style)
    {
      XFont font;
      try
      {
        font = new XFont(familyName, emSize, style);
      }
      catch
      {
        font = new XFont("Courier", emSize, style);
      }
      return font;
    }

    protected XFont CreateFont(string familyName, double emSize, XFontStyle style, XPdfFontOptions options)
    {
      XFont font;
      try
      {
        font = new XFont(familyName, emSize, style, options);
      }
      catch
      {
        font = new XFont("Courier", emSize, style, options);
      }
      return font;
    }

    protected void DrawGridlines(XGraphics gfx)
    {
      XPen majorpen = XPens.DarkGray.Clone();
      majorpen.Width = 1;
      XPen minorpen = XPens.LightGray.Clone();
      minorpen.Width = 0.1f;
      gfx.SmoothingMode = XSmoothingMode.HighSpeed;
      DrawGridlines(gfx, new XPoint(100, 100), majorpen, 100, minorpen, 10);

      string text = this.Description;
      XFont font = new XFont("Verdana", 14, XFontStyle.Bold);
      XSize size = gfx.MeasureString(text, font);
      gfx.DrawString(text, font, XBrushes.Black, (600 - size.Width) / 2, 30);
    }

    public abstract string Description {get;}

    protected GraphicsProperties properties;

    protected static PointF[] Pentagram
    {
      get
      {
        int[] order = new int[] { 0, 3, 1, 4, 2 };
        if (pentagram == null)
        {
          pentagram = new PointF[5];
          for (int idx = 0; idx < 5; idx++)
          {
            double rad = order[idx] * 2 * Math.PI / 5 - Math.PI / 10;
            pentagram[idx].X = (float)Math.Cos(rad);
            pentagram[idx].Y = (float)Math.Sin(rad);
          }
        }
        return pentagram;
      }
    }
    static PointF[] pentagram;

    protected static PointF[] GetPentagram(float size, PointF center)
    {
      PointF[] points = Pentagram.Clone() as PointF[];
      for (int idx = 0; idx < 5; idx++)
      {
        points[idx].X = points[idx].X * size + center.X;
        points[idx].Y = points[idx].Y * size + center.Y;
      }
      return points;
    }

    protected const double Deg2Rad = Math.PI / 180;

    public void DrawGridlines(XGraphics gfx, XPoint origin, XPen majorpen, double majordelta, XPen minorpen, double minordelta)
    {
      RectangleF box = new RectangleF(0, 0, 600, 850);
      DrawGridline(gfx, origin, minorpen, minordelta, box);
      DrawGridline(gfx, origin, majorpen, majordelta, box);
      /*
            float xmin = -10000f, ymin = -10000f, xmax = 10000f, ymax = 10000f;
            float x, y;
            x = origin.X;
            while (x < xmax)
            {
              DrawLine(majorpen, x, ymin, x, ymax);
              x += majordelta;
            }
            x = origin.X - majordelta;
            while (x > xmin)
            {
              DrawLine(majorpen, x, ymin, x, ymax);
              x -= majordelta;
            }
            y = origin.Y;
            while (y < ymax)
            {
              DrawLine(majorpen, xmin, y, xmax, y);
              y += majordelta;
            }
            y = origin.Y - majordelta;
            while (y > ymin)
            {
              DrawLine(majorpen, xmin, y, xmax, y);
              y -= majordelta;
            }
       */
    }

    [Conditional("DEBUG")]
    void DrawGridline(XGraphics gfx, XPoint origin, XPen pen, double delta, XRect box)
    {
      double xmin = box.X, ymin = box.Y, xmax = box.X + box.Width, ymax = box.Y + box.Height;
      double x, y;
      y = origin.Y;
      while (y < ymax)
      {
        gfx.DrawLine(pen, xmin, y, xmax, y);
        y += delta;
      }
      y = origin.Y - delta;
      while (y > ymin)
      {
        gfx.DrawLine(pen, xmin, y, xmax, y);
        y -= delta;
      }
      x = origin.X;
      while (x < xmax)
      {
        gfx.DrawLine(pen, x, ymin, x, ymax);
        x += delta;
      }
      x = origin.X - delta;
      while (x > xmin)
      {
        gfx.DrawLine(pen, x, ymin, x, ymax);
        x -= delta;
      }
    }
  }
}
