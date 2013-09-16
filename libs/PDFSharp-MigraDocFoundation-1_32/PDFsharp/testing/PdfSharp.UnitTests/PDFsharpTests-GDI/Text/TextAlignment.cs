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

namespace PdfSharp.UnitTests.Text
{
  /// <summary>
  /// 
  /// </summary>
  [TestClass]
  public class TextAlignment : TestBase
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
    public void TestTextAlignment()
    {
      Render("TextAlignment", RenderTextAlignment);
    }

    void RenderTextAlignment(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XRect rect = new XRect(0, 0, 250, 140);

      XFont font = new XFont("Verdana", 10);
      XBrush brush = XBrushes.Purple;
      XStringFormat format = new XStringFormat();

      gfx.DrawRectangle(XPens.YellowGreen, rect);
      gfx.DrawLine(XPens.YellowGreen, rect.Width / 2, 0, rect.Width / 2, rect.Height);
      gfx.DrawLine(XPens.YellowGreen, 0, rect.Height / 2, rect.Width, rect.Height / 2);

#if true
      gfx.DrawString("TopLeft", font, brush, rect, format);

      format.Alignment = XStringAlignment.Center;
      gfx.DrawString("TopCenter", font, brush, rect, format);

      format.Alignment = XStringAlignment.Far;
      gfx.DrawString("TopRight", font, brush, rect, format);

      format.LineAlignment = XLineAlignment.Center;
      format.Alignment = XStringAlignment.Near;
      gfx.DrawString("CenterLeft", font, brush, rect, format);

      format.Alignment = XStringAlignment.Center;
      gfx.DrawString("Center", font, brush, rect, format);

      format.Alignment = XStringAlignment.Far;
      gfx.DrawString("CenterRight", font, brush, rect, format);

      format.LineAlignment = XLineAlignment.Far;
      format.Alignment = XStringAlignment.Near;
      gfx.DrawString("BottomLeft", font, brush, rect, format);

      format.Alignment = XStringAlignment.Center;
      gfx.DrawString("BottomCenter", font, brush, rect, format);

      format.Alignment = XStringAlignment.Far;
      gfx.DrawString("BottomRight", font, brush, rect, format);
#else
      format.Alignment = XStringAlignment.Far;
      gfx.DrawString("TopRight", font, brush, rect, format);
#endif
    }
  }
}