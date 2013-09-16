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
  /// Test curves.
  /// </summary>
  [TestClass]
  public class ClipPaths : TestBase
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
    public void TestClipPath()
    {
      Render("ClipPath", RenderClipPath);
    }

    void RenderClipPath(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XGraphicsPath path = new XGraphicsPath();
      path.AddString("Clip!", new XFontFamily("Verdana"), XFontStyle.Bold, 90, new XRect(0, 0, 250, 140),
        XStringFormats.Center);

      gfx.IntersectClip(path);

      gfx.DrawRectangle(XBrushes.LightSalmon, new XRect(0, 0, 10000, 10000));
      // Draw a beam of dotted lines
      XPen pen = XPens.DarkRed.Clone();
      pen.DashStyle = XDashStyle.Dot;
      for (double r = 0; r <= 90; r += 0.5)
        gfx.DrawLine(pen, 0, 0, 1000 * Math.Cos(r / 90 * Math.PI), 1000 * Math.Sin(r / 90 * Math.PI));
    }
  }
}