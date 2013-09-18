using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;

namespace XDrawing.TestLab.Tester
{
  /// <summary>
  /// Draws a Spirograph.
  /// </summary>
  public class MiscSpiroGraph : TesterBase
  {
    /// <summary>
    /// Source and more infos: http://www.math.dartmouth.edu/~dlittle/java/SpiroGraph/
    /// </summary>
    public override void RenderPage(XGraphics gfx)
    {
      base.RenderPage(gfx);

      //int R =  60, r =  60, p =   60, N = 270; // Cardioid
      //int R =  60, r = -45, p = -101, N = 270; // Rounded Square
      //int R =  75, r = -25, p =   85, N = 270; // Gold fish
      //int R =  75, r = -30, p =   60, N = 270; // Star fish
      //int R = 100, r =  49, p =   66, N =   7; // String of Pearls
      //int R =  90, r =   1, p =  105, N = 105; // Rotating Triangle

      //int R =  90, r =   1, p =  105, N = 105;
      int R =  60, r =   2, p =  122, N = 490;

      int revs = Math.Abs(r) / Gcd(R, Math.Abs(r));
      XPoint[] points = new XPoint[revs * N + 1];
      for (int i = 0; i <= revs * N; i++)
      {
        double t = 4 * i * Math.PI / N;
        points[i].X = ((R+r)*Math.Cos(t) - p * Math.Cos((R+r)* t / r));
        points[i].Y = ((R+r)*Math.Sin(t) - p * Math.Sin((R+r)* t / r));
      }

#if true
      // Draw as lines
      gfx.TranslateTransform(300, 250);
      gfx.DrawLines(properties.Pen2.Pen, points);

      //gfx.DrawPolygon(properties.Pen2.Pen, properties.Brush2.Brush, points, properties.General.FillMode);

      // Draw as closed curve
      gfx.TranslateTransform(0, 400);
      gfx.DrawClosedCurve(properties.Pen2.Pen, properties.Brush2.Brush, points, properties.General.FillMode,
        properties.General.Tension);
#else
      gfx.TranslateTransform(300, 400);
      XSolidBrush dotBrush = new XSolidBrush(properties.Pen2.Pen.Color);
      float width = properties.Pen2.Width;
      for (int i = 0; i < revs * N; i++)
        gfx.DrawEllipse(dotBrush,points[i].X, points[i].Y, width, width);
#endif
    }

    public override string Description
    {
      get {return "Spirograph (DrawLines & DrawClosedCurve)";}
    }
  
    /// <summary>
    /// Calculates greatest common divisor.
    /// </summary>
    int Gcd(int x, int y)
    {
      return x % y == 0 ? y : Gcd(y, x % y);
    }
  }
}
