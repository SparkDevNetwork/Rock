using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Drawing;

namespace PdfSharp.UnitTests.Helpers
{
  static class GeometryObjects
  {
    /// <summary>
    /// Gets a five-pointed star with the specified size and center.
    /// </summary>
    public static XPoint[] GetPentagram(double size, XPoint center)
    {
      XPoint[] points = Pentagram.Clone() as XPoint[];
      for (int idx = 0; idx < 5; idx++)
      {
        points[idx].X = points[idx].X * size + center.X;
        points[idx].Y = points[idx].Y * size + center.Y;
      }
      return points;
    }

    /// <summary>
    /// Gets a normalized five-pointed star.
    /// </summary>
    static XPoint[] Pentagram
    {
      get
      {
        if (pentagram == null)
        {
          int[] order = { 0, 3, 1, 4, 2 };
          pentagram = new XPoint[5];
          for (int idx = 0; idx < 5; idx++)
          {
            double rad = order[idx] * 2 * Math.PI / 5 - Math.PI / 10;
            pentagram[idx].X = Math.Cos(rad);
            pentagram[idx].Y = Math.Sin(rad);
          }
        }
        return pentagram;
      }
    }
    static XPoint[] pentagram;
  }
}