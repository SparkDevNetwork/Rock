using System;
using System.Collections.Generic;
using System.Text;

namespace XDrawing
{
  static class TestBezier
  {
    public static void Test()
    {
      const double κ = 0.552284749f;  // := 4/3 * (1 - cos(-π/4)) / sin(π/4)) <=> 4/3 * sqrt(2) - 1
      //XRect rect = new XRect(x, y, width, height);
      double δx = 1;
      double δy = 1;
      double fx = δx * κ;
      double fy = δy * κ;
      //double x0 = rect.X + δx;
      //double y0 = rect.Y + δy;

      double start = 0;
      double stop = Math.PI / 2;
      for (double α = start; α <= stop; α += (Math.PI / 3600))
      {
        double x = Math.Cos(α);
        double y = Math.Sin(α);
      }
    }
  }
}