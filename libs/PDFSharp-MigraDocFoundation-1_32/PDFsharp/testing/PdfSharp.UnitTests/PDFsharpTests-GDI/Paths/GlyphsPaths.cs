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

namespace PdfSharp.UnitTests.Paths
{
  /// <summary>
  /// 
  /// </summary>
  [TestClass]
  public class GlyphsPaths : TestBase
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
    public void TestGlyphsPath()
    {
      Render("GlyphsPath", RenderGlyphsPath);
    }

    void RenderGlyphsPath(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XGraphicsPath path = new XGraphicsPath();
      //path.AddString("Hello!", new XFontFamily("Times New Roman"), XFontStyle.BoldItalic, 100, new XRect(0, 0, 250, 140),
      //        XStringFormat.Center);
      path.AddString("Hello!", new XFontFamily("Times New Roman"), XFontStyle.BoldItalic, 100, new XRect(0, 0, 250, 140), XStringFormats.Center);
      gfx.DrawPath(new XPen(XColors.Purple, 2.3), XBrushes.DarkOrchid, path);
    }
  }
}