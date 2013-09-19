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
  /// Test arces.
  /// </summary>
  [TestClass]
  public class Arcs : TestBase
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
    public void TestArcs()
    {
      Render("Arcs", RenderArcs);
    }

    /// <summary>
    /// Draws Arcs.
    /// </summary>
    void RenderArcs(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XPen pen = new XPen(XColors.Plum, 4.7);
      gfx.DrawArc(pen, 0, 0, 250, 140, 190, 200);
    }
  }
}