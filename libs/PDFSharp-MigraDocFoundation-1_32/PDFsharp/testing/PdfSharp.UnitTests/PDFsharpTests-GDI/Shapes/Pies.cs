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
  public class Pies : TestBase
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
    public void TestPies()
    {
      Render("Pies", RenderPies);
    }

    void RenderPies(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XPen pen = new XPen(XColors.DarkBlue, 2.5);

      gfx.DrawPie(pen, 10, 0, 100, 90, -120, 75);
      gfx.DrawPie(XBrushes.Gold, 130, 0, 100, 90, -160, 150);
      gfx.DrawPie(pen, XBrushes.Gold, 10, 50, 100, 90, 80, 70);
      gfx.DrawPie(pen, XBrushes.Gold, 150, 80, 60, 60, 35, 290);
    }
  }
}