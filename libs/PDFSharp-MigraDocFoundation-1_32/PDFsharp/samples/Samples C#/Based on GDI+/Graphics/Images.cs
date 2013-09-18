#region PDFsharp - A .NET library for processing PDF
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
//
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT OF THIRD PARTY RIGHTS.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
// USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Graphics
{
  /// <summary>
  /// Shows how to draw images.
  /// </summary>
  public class Images : Base
  {
    const string jpegSamplePath = "../../../../../../dev/XGraphicsLab/images/Z3.jpg";
    const string gifSamplePath = "../../../../../../dev/XGraphicsLab/images/Test.gif";
    const string pngSamplePath = "../../../../../../dev/XGraphicsLab/images/Test.png";
    const string tiffSamplePath = "../../../../../../dev/XGraphicsLab/images/Rose (RGB 8).tif";
    const string pdfSamplePath = "../../../../../PDFs/SomeLayout.pdf";

    public void DrawPage(PdfPage page)
    {
      XGraphics gfx = XGraphics.FromPdfPage(page);

      DrawTitle(page, gfx, "Images");

      DrawImage(gfx, 1);
      DrawImageScaled(gfx, 2);
      DrawImageRotated(gfx, 3);
      DrawImageSheared(gfx, 4);
      DrawGif(gfx, 5);
      DrawPng(gfx, 6);
      DrawTiff(gfx, 7);
      DrawFormXObject(gfx, 8);
    }

    /// <summary>
    /// Draws an image in original size.
    /// </summary>
    void DrawImage(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "DrawImage (original)");

      XImage image = XImage.FromFile(jpegSamplePath);

      // Left position in point
      double x = (250 - image.PixelWidth * 72 / image.HorizontalResolution) / 2;
      gfx.DrawImage(image, x, 0);

      EndBox(gfx);
    }

    /// <summary>
    /// Draws an image scaled.
    /// </summary>
    void DrawImageScaled(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "DrawImage (scaled)");

      XImage image = XImage.FromFile(jpegSamplePath);
      gfx.DrawImage(image, 0, 0, 250, 140);

      EndBox(gfx);
    }

    /// <summary>
    /// Draws an image transformed.
    /// </summary>
    void DrawImageRotated(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "DrawImage (rotated)");

      XImage image = XImage.FromFile(jpegSamplePath);

      const double dx = 250, dy = 140;

      gfx.TranslateTransform(dx / 2, dy / 2);
      gfx.ScaleTransform(0.7);
      gfx.RotateTransform(-25);
      gfx.TranslateTransform(-dx / 2, -dy / 2);

      //XMatrix matrix = new XMatrix();  //XMatrix.Identity;

      double width = image.PixelWidth * 72 / image.HorizontalResolution;
      double height = image.PixelHeight * 72 / image.HorizontalResolution;

      gfx.DrawImage(image, (dx - width) / 2, 0, width, height);

      EndBox(gfx);
    }

    /// <summary>
    /// Draws an image transformed.
    /// </summary>
    void DrawImageSheared(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "DrawImage (sheared)");

      XImage image = XImage.FromFile(jpegSamplePath);

      const double dx = 250, dy = 140;

      //XMatrix matrix = gfx.Transform;
      //matrix.TranslatePrepend(dx / 2, dy / 2);
      //matrix.ScalePrepend(-0.7, 0.7);
      //matrix.ShearPrepend(-0.4, -0.3);
      //matrix.TranslatePrepend(-dx / 2, -dy / 2);
      //gfx.Transform = matrix;

      gfx.TranslateTransform(dx / 2, dy / 2);
      gfx.ScaleTransform(-0.7, 0.7);
      gfx.ShearTransform(-0.4, -0.3);
      gfx.TranslateTransform(-dx / 2, -dy / 2);

      double width = image.PixelWidth * 72 / image.HorizontalResolution;
      double height = image.PixelHeight * 72 / image.HorizontalResolution;

      gfx.DrawImage(image, (dx - width) / 2, 0, width, height);

      EndBox(gfx);
    }

    /// <summary>
    /// Draws a GIF image with transparency.
    /// </summary>
    void DrawGif(XGraphics gfx, int number)
    {
      this.backColor = XColors.LightGoldenrodYellow;
      this.borderPen = new XPen(XColor.FromArgb(202, 121, 74), this.borderWidth);
      BeginBox(gfx, number, "DrawImage (GIF)");

      XImage image = XImage.FromFile(gifSamplePath);

      const double dx = 250, dy = 140;

      double width = image.PixelWidth * 72 / image.HorizontalResolution;
      double height = image.PixelHeight * 72 / image.HorizontalResolution;

      gfx.DrawImage(image, (dx - width) / 2, (dy - height) / 2, width, height);

      EndBox(gfx);
    }

    /// <summary>
    /// Draws a PNG image with transparency.
    /// </summary>
    void DrawPng(XGraphics gfx, int number)
    {
      BeginBox(gfx, number, "DrawImage (PNG)");

      XImage image = XImage.FromFile(pngSamplePath);

      const double dx = 250, dy = 140;

      double width = image.PixelWidth * 72 / image.HorizontalResolution;
      double height = image.PixelHeight * 72 / image.HorizontalResolution;

      gfx.DrawImage(image, (dx - width) / 2, (dy - height) / 2, width, height);

      EndBox(gfx);
    }

    /// <summary>
    /// Draws a TIFF image with transparency.
    /// </summary>
    void DrawTiff(XGraphics gfx, int number)
    {
      XColor oldBackColor = this.backColor;
      this.backColor = XColors.LightGoldenrodYellow;
      BeginBox(gfx, number, "DrawImage (TIFF)");

      XImage image = XImage.FromFile(tiffSamplePath);

      const double dx = 250, dy = 140;

      double width = image.PixelWidth * 72 / image.HorizontalResolution;
      double height = image.PixelHeight * 72 / image.HorizontalResolution;

      gfx.DrawImage(image, (dx - width) / 2, (dy - height) / 2, width, height);

      EndBox(gfx);
      this.backColor = oldBackColor;
    }

    /// <summary>
    /// Draws a form XObject (a page from an external PDF file).
    /// </summary>
    void DrawFormXObject(XGraphics gfx, int number)
    {
      //this.backColor = XColors.LightSalmon;
      BeginBox(gfx, number, "DrawImage (Form XObject)");

      XImage image = XImage.FromFile(pdfSamplePath);

      const double dx = 250, dy = 140;

      gfx.TranslateTransform(dx / 2, dy / 2);
      gfx.ScaleTransform(0.35);
      gfx.TranslateTransform(-dx / 2, -dy / 2);

      double width = image.PixelWidth * 72 / image.HorizontalResolution;
      double height = image.PixelHeight * 72 / image.HorizontalResolution;

      gfx.DrawImage(image, (dx - width) / 2, (dy - height) / 2, width, height);

      EndBox(gfx);
    }
  }
}
