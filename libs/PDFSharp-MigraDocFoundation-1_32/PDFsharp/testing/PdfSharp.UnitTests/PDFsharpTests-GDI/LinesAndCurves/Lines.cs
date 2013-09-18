using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#if GDI
using System.Drawing;
using System.Drawing.Imaging;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.UnitTests.Helpers;

namespace PdfSharp.UnitTests.LinesAndCurves
{
  /// <summary>
  /// Tests lines and polylines.
  /// </summary>
  [TestClass]
  public class Lines : TestBase
  {
    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext { get; set; }

    [ClassInitialize()]
    public static void ClassInitialize(TestContext testContext)
    {
    }

    [ClassCleanup()]
    public static void MyClassCleanup()
    {
    }

    [TestInitialize()]
    public void TestInitialize()
    {
      BeginPdf();
      BeginImage();
    }

    [TestCleanup()]
    public void TestCleanup() 
    {
      EndPdf();
      EndImage();
    }

    [TestMethod]
    public void TestDrawLine()
    {
      Render("Lines", RenderLines);
    }

    [TestMethod]
    public void TestDrawLines()
    {
      Render("PolyLines", RenderPolyLines);
    }

    /// <summary>
    /// Draws simple lines.
    /// </summary>
    void RenderLines(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      gfx.DrawLine(XPens.Red, new XPoint(10, 10), new XPoint(300, 300));

      gfx.DrawLine(XPens.DarkGreen, 0, 0, 250, 0);

      gfx.DrawLine(XPens.Gold, 15, 7, 230, 15);

      XPen pen = new XPen(XColors.Navy, 4);
      gfx.DrawLine(pen, 0, 20, 250, 20);

      pen = new XPen(XColors.Firebrick, 6);
      pen.DashStyle = XDashStyle.Dash;
      gfx.DrawLine(pen, 0, 40, 250, 40);
      pen.Width = 7.3;
      pen.DashStyle = XDashStyle.DashDotDot;
      gfx.DrawLine(pen, 0, 60, 250, 60);

      pen = new XPen(XColors.Goldenrod, 10);
      pen.LineCap = XLineCap.Flat;
      gfx.DrawLine(pen, 10, 90, 240, 90);
      gfx.DrawLine(XPens.Black, 10, 90, 240, 90);

      pen = new XPen(XColors.Goldenrod, 10);
      pen.LineCap = XLineCap.Square;
      gfx.DrawLine(pen, 10, 110, 240, 110);
      gfx.DrawLine(XPens.Black, 10, 110, 240, 110);

      pen = new XPen(XColors.Goldenrod, 10);
      pen.LineCap = XLineCap.Round;
      gfx.DrawLine(pen, 10, 130, 240, 130);
      gfx.DrawLine(XPens.Black, 10, 130, 240, 130);
    }

    /// <summary>
    /// Draws simple lines.
    /// </summary>
    void RenderPolyLines(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XPen pen = new XPen(XColors.DarkSeaGreen, 6);
      pen.LineCap = XLineCap.Round;
      pen.LineJoin = XLineJoin.Bevel;
      XPoint[] points =
        new XPoint[] { new XPoint(20, 30), new XPoint(60, 120), new XPoint(90, 20), new XPoint(170, 90), new XPoint(230, 40) };
      gfx.DrawLines(pen, points);
    }
  }
}