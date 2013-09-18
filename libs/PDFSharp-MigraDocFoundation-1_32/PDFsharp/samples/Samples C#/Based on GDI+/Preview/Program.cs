#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   PDFsharp Team (mailto:PDFsharpSupport@pdfsharp.de)
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using PdfSharp.Drawing;
using PdfSharp.Forms;

namespace Preview
{
  /// <summary>
  /// This sample shows how to render graphics in both a preview and a PDF document.
  /// </summary>
  class Program
  {
    static void Main() 
    {
      Renderer renderer = new Renderer();
      PreviewForm form = new PreviewForm();
      form.RenderEvent = new PagePreview.RenderEvent(renderer.Render);
      Application.Run(form);
    }
  }

  class Renderer
  {
    /// <summary>
    /// Renders the content of the page.
    /// </summary>
    public void Render(XGraphics gfx)
    {
      XRect rect;
      XPen pen;
      double x = 50, y = 100;
      XFont fontH1 = new XFont("Times", 18, XFontStyle.Bold);
      XFont font = new XFont("Times", 12);
      XFont fontItalic = new XFont("Times", 12, XFontStyle.BoldItalic);
      double ls = font.GetHeight(gfx);

      // Draw some text
      gfx.DrawString("Create PDF on the fly with PDFsharp",
          fontH1, XBrushes.Black, x, x);
      gfx.DrawString("With PDFsharp you can use the same code to draw graphic, " +
          "text and images on different targets.", font, XBrushes.Black, x, y);
      y += ls;
      gfx.DrawString("The object used for drawing is the XGraphics object.", 
          font, XBrushes.Black, x, y);
      y += 2 * ls;

      // Draw an arc
      pen = new XPen(XColors.Red, 4);
      pen.DashStyle = XDashStyle.Dash;
      gfx.DrawArc(pen, x + 20, y, 100, 60, 150, 120);

      // Draw a star
      XGraphicsState gs = gfx.Save();
      gfx.TranslateTransform(x + 140, y + 30);
      for (int idx = 0; idx < 360; idx += 10)
      {
        gfx.RotateTransform(10);
        gfx.DrawLine(XPens.DarkGreen, 0, 0, 30, 0);
      }
      gfx.Restore(gs);

      // Draw a rounded rectangle
      rect = new XRect(x + 230, y, 100, 60);
      pen = new XPen(XColors.DarkBlue, 2.5);
      XColor color1 = XColor.FromKnownColor(KnownColor.DarkBlue);
      XColor color2 = XColors.Red;
      XLinearGradientBrush lbrush = new XLinearGradientBrush(rect, color1, color2, 
        XLinearGradientMode.Vertical);
      gfx.DrawRoundedRectangle(pen, lbrush, rect, new XSize(10, 10));
      
      // Draw a pie
      pen = new XPen(XColors.DarkOrange, 1.5);
      pen.DashStyle = XDashStyle.Dot;
      gfx.DrawPie(pen, XBrushes.Blue, x + 360, y, 100, 60, -130, 135);
      
      // Draw some more text
      y += 60 + 2 * ls;
      gfx.DrawString("With XGraphics you can draw on a PDF page as well as " +
          "on any System.Drawing.Graphics object.", font, XBrushes.Black, x, y);
      y += ls * 1.1;
      gfx.DrawString("Use the same code to", font, XBrushes.Black, x, y);
      x += 10;
      y += ls * 1.1;
      gfx.DrawString("• draw on a newly created PDF page", font, XBrushes.Black, x, y);
      y += ls;
      gfx.DrawString("• draw above or beneath of the content of an existing PDF page", 
          font, XBrushes.Black, x, y);
      y += ls;
      gfx.DrawString("• draw in a window", font, XBrushes.Black, x, y);
      y += ls;
      gfx.DrawString("• draw on a printer", font, XBrushes.Black, x, y);
      y += ls;
      gfx.DrawString("• draw in a bitmap image", font, XBrushes.Black, x, y);
      x -= 10;
      y += ls * 1.1;
      gfx.DrawString("You can also import an existing PDF page and use it like " +
          "an image, e.g. draw it on another PDF page.", font, XBrushes.Black, x, y);
      y += ls * 1.1 * 2;
      gfx.DrawString("Imported PDF pages are neither drawn nor printed; create a " + 
          "PDF file to see or print them!", fontItalic, XBrushes.Firebrick, x, y);
      y += ls * 1.1;
      gfx.DrawString("Below this text is a PDF form that will be visible when " + 
          "viewed or printed with a PDF viewer.", fontItalic, XBrushes.Firebrick, x, y);
      y += ls * 1.1;
      XGraphicsState state = gfx.Save();
      XRect rcImage = new XRect(100, y, 100, 100 * Math.Sqrt(2));
      gfx.DrawRectangle(XBrushes.Snow, rcImage);
      gfx.DrawImage(XPdfForm.FromFile("../../../../../PDFs/SomeLayout.pdf"), rcImage);
      gfx.Restore(state);
    }
  }
}
