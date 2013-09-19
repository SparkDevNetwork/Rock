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
  /// Test curves.
  /// </summary>
  [TestClass]
  public class Curves : TestBase
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
    public void TestCurves()
    {
      Render("Curves", RenderCurves);
    }

    /// <summary>
    /// Draws curves.
    /// </summary>
    void RenderCurves(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XPoint[] points = { new XPoint(20, 30), new XPoint(60, 120), new XPoint(90, 20), new XPoint(170, 90), new XPoint(230, 40) };
      XPen pen = new XPen(XColors.RoyalBlue, 3.5);
      gfx.DrawCurve(pen, points, 1);
    }
  }
}