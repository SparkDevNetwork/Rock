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

namespace PdfSharp.UnitTests.Images
{
  /// <summary>
  /// 
  /// </summary>
  [TestClass]
  public class JPEGs : TestBase
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
    public void TestJPEGs()
    {
      Render("JPEGs", RenderJPEGs);
    }

    [TestMethod]
    public void TestGIFs()
    {
      Render("GIFs", RenderGIFs);
    }

    void RenderJPEGs(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XImage image = XImage.FromFile(jpegSamplePath);

      // Left position in point
      double x = (250 - image.PixelWidth * 72 / image.HorizontalResolution) / 2;
      gfx.DrawImage(image, x, 0);
    }

    void RenderGIFs(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XImage image = XImage.FromFile(gifSamplePath);

      // Left position in point
      double x = (250 - image.PixelWidth * 72 / image.HorizontalResolution) / 2;
      gfx.DrawImage(image, x, 0);
    }

    string jpegSamplePath = "../../../../../../dev/XGraphicsLab/images/Z3.jpg";
    string gifSamplePath = "../../../../../../dev/XGraphicsLab/images/Test.gif";
    //string pngSamplePath = "../../../../../../XGraphicsLab/images/Test.png";
    //string tiffSamplePath = "../../../../../../XGraphicsLab/images/Rose (RGB 8).tif";
    //string pdfSamplePath = "../../../../../../PDFs/SomeLayout.pdf";

  }
}