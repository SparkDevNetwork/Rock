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
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.RtfRendering;
using PdfSharp.Pdf;

namespace HelloWorld
{
  /// <summary>
  /// This sample is the obligatory Hello World program for MigraDoc documents.
  /// </summary>
  class Programm
  {
    static void Main()
    {
      // Create a MigraDoc document
      Document document = CreateDocument();
      document.UseCmykColor = true;

     // string ddl = MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToString(document);

#if true_
      RtfDocumentRenderer renderer = new RtfDocumentRenderer();
      renderer.Render(document, "HelloWorld.rtf", null);
#endif
      
      // ----- Unicode encoding and font program embedding in MigraDoc is demonstrated here -----

      // A flag indicating whether to create a Unicode PDF or a WinAnsi PDF file.
      // This setting applies to all fonts used in the PDF document.
      // This setting has no effect on the RTF renderer.
      const bool unicode = false;

      // An enum indicating whether to embed fonts or not.
      // This setting applies to all font programs used in the document.
      // This setting has no effect on the RTF renderer.
      // (The term 'font program' is used by Adobe for a file containing a font. Technically a 'font file'
      // is a collection of small programs and each program renders the glyph of a character when executed.
      // Using a font in PDFsharp may lead to the embedding of one or more font programms, because each outline
      // (regular, bold, italic, bold+italic, ...) has its own fontprogram)
      const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

      // ----------------------------------------------------------------------------------------

      // Create a renderer for the MigraDoc document.
      PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);

      // Associate the MigraDoc document with a renderer
      pdfRenderer.Document = document;

      // Layout and render document to PDF
      pdfRenderer.RenderDocument();

      // Save the document...
      const string filename = "HelloWorld.pdf";
      pdfRenderer.PdfDocument.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }

    /// <summary>
    /// Creates an absolutely minimalistic document.
    /// </summary>
    static Document CreateDocument()
    {
      // Create a new MigraDoc document
      Document document = new Document();

      // Add a section to the document
      Section section = document.AddSection();

      // Add a paragraph to the section
      Paragraph paragraph = section.AddParagraph();

      paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);

      // Add some text to the paragraph
      //paragraph.AddFormattedText("Hello, World!  öäüÖÄÜß~§≤≥≈≠", TextFormat.Italic);
      paragraph.AddFormattedText("Hello, World!", TextFormat.Bold);

      return document;
    }
  }
}
