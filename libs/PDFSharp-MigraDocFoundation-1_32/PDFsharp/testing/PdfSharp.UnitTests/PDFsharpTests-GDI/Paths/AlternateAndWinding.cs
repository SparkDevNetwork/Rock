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
  public class AlternateAndWinding : TestBase
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
    public void TestAlternatePath()
    {
      Render("AlternatePath", RenderAlternatePath);
    }

    [TestMethod]
    public void TestWindingPath()
    {
      Render("WindingPath", RenderWindingPath);
    }

    void RenderAlternatePath(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XPen pen = new XPen(XColors.Navy, 2.5);

      // Alternate fill mode
      XGraphicsPath path = new XGraphicsPath();
      path.FillMode = XFillMode.Alternate;
      path.AddLine(10, 130, 10, 40);
      path.AddBeziers(new XPoint[]{new XPoint(10, 40), new XPoint(30, 0), new XPoint(40, 20), new XPoint(60, 40), 
                                   new XPoint(80, 60), new XPoint(100, 60), new XPoint(120, 40)});
      path.AddLine(120, 40, 120, 130);
      path.CloseFigure();
      path.AddEllipse(40, 80, 50, 40);
      gfx.DrawPath(pen, XBrushes.DarkOrange, path);
    }

    void RenderWindingPath(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 150);

      XPen pen = new XPen(XColors.Navy, 2.5);

      // Winding fill mode
      XGraphicsPath path = new XGraphicsPath();
      path = new XGraphicsPath();
      path.FillMode = XFillMode.Winding;
      path.AddLine(130, 130, 130, 40);
      path.AddBeziers(new XPoint[]{new XPoint(130, 40), new XPoint(150, 0), new XPoint(160, 20), new XPoint(180, 40), 
                                   new XPoint(200, 60), new XPoint(220, 60), new XPoint(240, 40)});
      path.AddLine(240, 40, 240, 130);
      path.CloseFigure();
      path.AddEllipse(160, 80, 50, 40);
      gfx.DrawPath(pen, XBrushes.DarkOrange, path);
    }
  }
}