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
using PdfSharp.Pdf.Annotations;

namespace Annotations
{
  /// <summary>
  /// This sample shows how to create annotations.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // Create a new PDF document
      PdfDocument document = new PdfDocument();

      // Create a font
      XFont font = new XFont("Verdana", 14);

      // Create a page
      PdfPage page = document.AddPage();
      XGraphics gfx = XGraphics.FromPdfPage(page);

      // Create a PDF text annotation
      PdfTextAnnotation textAnnot = new PdfTextAnnotation();
      textAnnot.Title = "This is the title";
      textAnnot.Subject = "This is the subject";
      textAnnot.Contents = "This is the contents of the annotation.\rThis is the 2nd line.";
      textAnnot.Icon = PdfTextAnnotationIcon.Note;

      gfx.DrawString("The first text annotation", font, XBrushes.Black, 30, 50, XStringFormats.Default);

      // Convert rectangle form world space to page space. This is necessary because the annotation is
      // placed relative to the bottom left corner of the page with units measured in point.
      XRect rect = gfx.Transformer.WorldToDefaultPage(new XRect(new XPoint(30, 60), new XSize(30, 30)));
      textAnnot.Rectangle = new PdfRectangle(rect);

      // Add the annotation to the page
      page.Annotations.Add(textAnnot);

      // Create another PDF text annotation which is open and transparent
      textAnnot = new PdfTextAnnotation();
      textAnnot.Title = "Annotation 2 (title)";
      textAnnot.Subject = "Annotation 2 (subject)";
      textAnnot.Contents = "This is the contents of the 2nd annotation.";
      textAnnot.Icon = PdfTextAnnotationIcon.Help;
      textAnnot.Color = XColors.LimeGreen;
      textAnnot.Opacity = 0.5;
      textAnnot.Open = true;

      gfx.DrawString("The second text annotation (opened)", font, XBrushes.Black, 30, 140, XStringFormats.Default);

      rect = gfx.Transformer.WorldToDefaultPage(new XRect(new XPoint(30, 150), new XSize(30, 30)));
      textAnnot.Rectangle = new PdfRectangle(rect);

      // Add the 2nd annotation to the page
      page.Annotations.Add(textAnnot);


      // Create a so called rubber stamp annotion. I'm not sure if it is useful, but at least
      // it looks impressive...
      PdfRubberStampAnnotation rsAnnot = new PdfRubberStampAnnotation();
      rsAnnot.Icon = PdfRubberStampAnnotationIcon.TopSecret;
      rsAnnot.Flags = PdfAnnotationFlags.ReadOnly;

      rect = gfx.Transformer.WorldToDefaultPage(new XRect(new XPoint(100, 400), new XSize(350, 150)));
      rsAnnot.Rectangle = new PdfRectangle(rect);

      // Add the rubber stamp annotation to the page
      page.Annotations.Add(rsAnnot);

      // PDF supports some more pretty types of annotations like PdfLineAnnotation, PdfSquareAnnotation,
      // PdfCircleAnnotation, PdfMarkupAnnotation (with the subtypes PdfHighlightAnnotation, PdfUnderlineAnnotation,
      // PdfStrikeOutAnnotation, and PdfSquigglyAnnotation), PdfSoundAnnotation, or PdfMovieAnnotation.
      // If you need one of them, feel encouraged to implement it. It is quite easy.

      // Save the document...
      const string filename = "Annotations_tempfile.pdf";
      document.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }
  }
}
