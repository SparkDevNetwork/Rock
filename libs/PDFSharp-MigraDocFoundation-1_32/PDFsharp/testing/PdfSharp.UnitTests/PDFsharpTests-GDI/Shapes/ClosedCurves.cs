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
  public class ClosedCurves : TestBase
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
    public void TestClosedCurves()
    {
      Render("ClosedCurves", RenderClosedCurves);
    }

    void RenderClosedCurves(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XPen pen = new XPen(XColors.DarkBlue, 2.5);
      gfx.DrawClosedCurve(pen, XBrushes.SkyBlue,
        new XPoint[] { new XPoint(10, 120), new XPoint(80, 30), new XPoint(220, 20), new XPoint(170, 110), new XPoint(100, 90) },
        XFillMode.Winding, 0.7);
    }
  }
}