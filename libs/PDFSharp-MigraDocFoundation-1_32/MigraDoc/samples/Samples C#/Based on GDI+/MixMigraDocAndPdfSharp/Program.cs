#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   PDFsharp Team (mailto:PDFsharpSupport@pdfsharp.de)
//
// Copyright (c) 2001-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://www.migradoc.com
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
using System.Text;
#if !WPF
using System.Drawing;
#endif
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace MixMigraDocAndPdfSharp
{
  /// <summary>
  /// This sample demonstrates how to mix MigraDoc and PDFsharp.
  /// </summary>
  class Program
  {
    static void Main()
    {
      DateTime now = DateTime.Now;
      string filename = "MixMigraDocAndPdfSharp.pdf";
      filename = Guid.NewGuid().ToString("D").ToUpper() + ".pdf";
      PdfDocument document = new PdfDocument();
      document.Info.Title = "PDFsharp XGraphic Sample";
      document.Info.Author = "Stefan Lange";
      document.Info.Subject = "Created with code snippets that show the use of graphical functions";
      document.Info.Keywords = "PDFsharp, XGraphics";

      SamplePage1(document);

      SamplePage2(document);

      Debug.WriteLine("seconds=" + (DateTime.Now - now).TotalSeconds.ToString());

      // Save the document...
      document.Save(filename);
      // ...and start a viewer
      Process.Start(filename);
    }

    /// <summary>
    /// Renders a single paragraph.
    /// </summary>
    static void SamplePage1(PdfDocument document)
    {
      PdfPage page = document.AddPage();
      XGraphics gfx = XGraphics.FromPdfPage(page);
      // HACK²
      gfx.MUH = PdfFontEncoding.Unicode;
      gfx.MFEH = PdfFontEmbedding.Default;

      XFont font = new XFont("Verdana", 13, XFontStyle.Bold);

      gfx.DrawString("The following paragraph was rendered using MigraDoc:", font, XBrushes.Black,
        new XRect(100, 100, page.Width - 200, 300), XStringFormats.Center);

      // You always need a MigraDoc document for rendering.
      Document doc = new Document();
      Section sec = doc.AddSection();
      // Add a single paragraph with some text and format information.
      Paragraph para = sec.AddParagraph();
      para.Format.Alignment = ParagraphAlignment.Justify;
      para.Format.Font.Name = "Times New Roman";
      para.Format.Font.Size = 12;
      para.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.DarkGray;
      para.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.DarkGray;
      para.AddText("Duisism odigna acipsum delesenisl ");
      para.AddFormattedText("ullum in velenit", TextFormat.Bold);
      para.AddText(" ipit iurero dolum zzriliquisis nit wis dolore vel et nonsequipit, velendigna "+
        "auguercilit lor se dipisl duismod tatem zzrit at laore magna feummod oloborting ea con vel "+
        "essit augiati onsequat luptat nos diatum vel ullum illummy nonsent nit ipis et nonsequis "+
        "niation utpat. Odolobor augait et non etueril landre min ut ulla feugiam commodo lortie ex "+
        "essent augait el ing eumsan hendre feugait prat augiatem amconul laoreet. ≤≥≈≠");
      para.Format.Borders.Distance = "5pt";
      para.Format.Borders.Color = Colors.Gold;

      // Create a renderer and prepare (=layout) the document
      MigraDoc.Rendering.DocumentRenderer docRenderer = new DocumentRenderer(doc);
      docRenderer.PrepareDocument();

      // Render the paragraph. You can render tables or shapes the same way.
      docRenderer.RenderObject(gfx, XUnit.FromCentimeter(5), XUnit.FromCentimeter(10), "12cm", para);
    }

    /// <summary>
    /// Renders a whole MigraDoc document scaled to a single PDF page.
    /// </summary>
    static void SamplePage2(PdfDocument document)
    {
      PdfPage page = document.AddPage();
      XGraphics gfx = XGraphics.FromPdfPage(page);
      // HACK²
      gfx.MUH = PdfFontEncoding.Unicode;
      gfx.MFEH = PdfFontEmbedding.Default;
      
      // Create document from HalloMigraDoc sample
      Document doc = HelloMigraDoc.Documents.CreateDocument();

      // Create a renderer and prepare (=layout) the document
      MigraDoc.Rendering.DocumentRenderer docRenderer = new DocumentRenderer(doc);
      docRenderer.PrepareDocument();

      // For clarity we use point as unit of measure in this sample.
      // A4 is the standard letter size in Germany (21cm x 29.7cm).
      XRect A4Rect = new XRect(0, 0, A4Width, A4Height);

      int pageCount = docRenderer.FormattedDocument.PageCount;
      for (int idx = 0; idx < pageCount; idx++)
      {
        XRect rect = GetRect(idx);

        // Use BeginContainer / EndContainer for simplicity only. You can naturaly use you own transformations.
        XGraphicsContainer container = gfx.BeginContainer(rect, A4Rect, XGraphicsUnit.Point);
        
        // Draw page border for better visual representation
        gfx.DrawRectangle(XPens.LightGray, A4Rect);

        // Render the page. Note that page numbers start with 1.
        docRenderer.RenderPage(gfx, idx + 1);

        // Note: The outline and the hyperlinks (table of content) does not work in the produced PDF document.

        // Pop the previous graphical state
        gfx.EndContainer(container);
      }
    }

    /// <summary>
    /// Calculates thumb rectangle.
    /// </summary>
    static XRect GetRect(int index)
    {
      XRect rect = new XRect(0, 0, A4Width / 3 * 0.9, A4Height / 3 * 0.9);
      rect.X = (index % 3) * A4Width / 3 + A4Width * 0.05 / 3;
      rect.Y = (index / 3) * A4Height / 3 + A4Height * 0.05 / 3;
      return rect;
    }
    static double A4Width = XUnit.FromCentimeter(21).Point;
    static double A4Height = XUnit.FromCentimeter(29.7).Point;
  }
}
