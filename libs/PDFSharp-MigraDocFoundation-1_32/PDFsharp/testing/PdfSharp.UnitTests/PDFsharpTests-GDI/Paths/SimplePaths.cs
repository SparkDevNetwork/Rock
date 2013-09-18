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
  public class SimplePaths : TestBase
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
    public void TestOpenPath()
    {
      Render("OpenPath", RenderOpenPath);
    }

    [TestMethod]
    public void TestClosedPath()
    {
      Render("ClosedPath", RenderClosedPath);
    }

    void RenderOpenPath(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XPen pen = new XPen(XColors.Navy, Math.PI);
      pen.DashStyle = XDashStyle.Dash;

      XGraphicsPath path = new XGraphicsPath();
      path.AddLine(10, 120, 50, 60);
      path.AddArc(50, 20, 110, 80, 180, 180);
      path.AddLine(160, 60, 220, 100);
      gfx.DrawPath(pen, path);
    }

    void RenderClosedPath(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 150);

      XPen pen = new XPen(XColors.Navy, Math.PI);
      pen.DashStyle = XDashStyle.Dash;

      XGraphicsPath path = new XGraphicsPath();
      path.AddLine(10, 120, 50, 60);
      path.AddArc(50, 20, 110, 80, 180, 180);
      path.AddLine(160, 60, 220, 100);
      path.CloseFigure();
      gfx.DrawPath(pen, path);
    }
  }
}