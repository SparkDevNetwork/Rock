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
using System.Diagnostics;
using System.IO;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace XForms
{
  /// <summary>
  /// This sample shows how to create an XForm object from scratch. You can think of such an
  /// object as a template, that, once created, can be drawn frequently anywhere in your PDF document.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // Create a new PDF document
      PdfDocument document = new PdfDocument();

      // Create a font
      XFont font = new XFont("Verdana", 16);

      // Create a new page
      PdfPage page = document.AddPage();
      XGraphics gfx = XGraphics.FromPdfPage(page, XPageDirection.Downwards);
      gfx.DrawString("XPdfForm Sample", font, XBrushes.DarkGray, 15, 25, XStringFormats.Default);

      // Step 1: Create an XForm and draw some graphics on it

      // Create an empty XForm object with the specified width and height
      // A form is bound to its target document when it is created. The reason is that the form can 
      // share fonts and other objects with its target document.
      XForm form = new XForm(document, XUnit.FromMillimeter(70), XUnit.FromMillimeter(55));

      // Create an XGraphics object for drawing the contents of the form.
      XGraphics formGfx = XGraphics.FromForm(form);

      // Draw a large transparent rectangle to visualize the area the form occupies
      XColor back = XColors.Orange;
      back.A = 0.2;
      XSolidBrush brush = new XSolidBrush(back);
      formGfx.DrawRectangle(brush, -10000, -10000, 20000, 20000);
      
      // On a form you can draw...

      // ... text
      formGfx.DrawString("Text, Graphics, Images, and Forms", new XFont("Verdana", 10, XFontStyle.Regular), XBrushes.Navy, 3, 0, XStringFormats.TopLeft);
      XPen pen = XPens.LightBlue.Clone();
      pen.Width = 2.5;

      // ... graphics like Bézier curves
      formGfx.DrawBeziers(pen, XPoint.ParsePoints("30,120 80,20 100,140 175,33.3"));

      // ... raster images like GIF files
      XGraphicsState state = formGfx.Save();
      formGfx.RotateAtTransform(17, new XPoint(30, 30));
      formGfx.DrawImage(XImage.FromFile("../../../../../../dev/XGraphicsLab/images/Test.gif"), 20, 20);
      formGfx.Restore(state);

      // ... and forms like XPdfForm objects
      state = formGfx.Save();
      formGfx.RotateAtTransform(-8, new XPoint(165, 115));
      formGfx.DrawImage(XPdfForm.FromFile("../../../../../PDFs/SomeLayout.pdf"), new XRect(140, 80, 50, 50 * Math.Sqrt(2)));
      formGfx.Restore(state);

      // When you finished drawing on the form, dispose the XGraphic object.
      formGfx.Dispose();


      // Step 2: Draw the XPdfForm on your PDF page like an image

      // Draw the form on the page of the document in its original size
      gfx.DrawImage(form, 20, 50);

#if true
      // Draw it stretched
      gfx.DrawImage(form, 300, 100, 250, 40);

      // Draw and rotate it
      const int d = 25;
      for (int idx = 0; idx < 360; idx += d)
      {
        gfx.DrawImage(form, 300, 480, 200, 200);
        gfx.RotateAtTransform(d, new XPoint(300, 480));
      }
#endif

      // Save the document...
      const string filename = "XForms_tempfile.pdf";
      document.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }
  }
}
