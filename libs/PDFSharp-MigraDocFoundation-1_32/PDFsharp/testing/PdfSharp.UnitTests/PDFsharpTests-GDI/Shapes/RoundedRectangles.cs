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

namespace PdfSharp.UnitTests.Shapes
{
  /// <summary>
  /// 
  /// </summary>
  [TestClass]
  public class RoundedRectangles : TestBase
  {
    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext { get; set; }

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
    public void TestRoundedRectangles()
    {
      Render("RoundedRectangles", RenderRoundedRectangles);
    }

    void RenderRoundedRectangles(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XPen pen = new XPen(XColors.RoyalBlue, Math.PI);

      gfx.DrawRoundedRectangle(pen, 10, 0, 100, 60, 30, 20);
      gfx.DrawRoundedRectangle(XBrushes.Orange, 130, 0, 100, 60, 30, 20);
      gfx.DrawRoundedRectangle(pen, XBrushes.Orange, 10, 80, 100, 60, 30, 20);
      gfx.DrawRoundedRectangle(pen, XBrushes.Orange, 150, 80, 60, 60, 20, 20);
    }
  }
}