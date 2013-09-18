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
  public class MeasureText : TestBase
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
    public void TestMeasureText()
    {
      Render("MeasureText", RenderMeasureText);
    }

    void RenderMeasureText(XGraphics gfx)
    {
      gfx.TranslateTransform(15, 20);

      XFontStyle style = XFontStyle.Regular;
      XFont font = new XFont("Times New Roman", 95, style);

      string text = "Hello";
      double x = 20, y = 100;
      XSize size = gfx.MeasureString(text, font);

      double lineSpace = font.GetHeight(gfx);
      int cellSpace = font.FontFamily.GetLineSpacing(style);
      int cellAscent = font.FontFamily.GetCellAscent(style);
      int cellDescent = font.FontFamily.GetCellDescent(style);
      int cellLeading = cellSpace - cellAscent - cellDescent;

      // Get effective ascent
      double ascent = lineSpace * cellAscent / cellSpace;
      gfx.DrawRectangle(XBrushes.Bisque, x, y - ascent, size.Width, ascent);

      // Get effective descent
      double descent = lineSpace * cellDescent / cellSpace;
      gfx.DrawRectangle(XBrushes.LightGreen, x, y, size.Width, descent);

      // Get effective leading
      double leading = lineSpace * cellLeading / cellSpace;
      gfx.DrawRectangle(XBrushes.Yellow, x, y + descent, size.Width, leading);

      // Draw text half transparent
      XColor color = XColors.DarkSlateBlue;
      color.A = 0.6;
      gfx.DrawString(text, font, new XSolidBrush(color), x, y);
    }
  }
}