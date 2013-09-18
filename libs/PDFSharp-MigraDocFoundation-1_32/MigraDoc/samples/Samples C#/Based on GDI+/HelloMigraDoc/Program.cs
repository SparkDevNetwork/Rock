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

namespace HelloMigraDoc
{
  /// <summary>
  /// This sample shows some features of MigraDoc.
  /// </summary>
  class Program
  {
    static void Main()
    {
      // Create a MigraDoc document
      Document document = Documents.CreateDocument();

      //string ddl = MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToString(document);
      MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToFile(document, "MigraDoc.mdddl");

      PdfDocumentRenderer renderer = new PdfDocumentRenderer(true, PdfSharp.Pdf.PdfFontEmbedding.Always);
      renderer.Document = document;

      renderer.RenderDocument();

      // Save the document...
#if DEBUG
      string filename = Guid.NewGuid().ToString("N").ToUpper() + ".pdf";
#else
      string filename = "HelloMigraDoc.pdf";
#endif
      renderer.PdfDocument.Save(filename);
      // ...and start a viewer.
      Process.Start(filename);
    }

    ///// <summary>
    ///// Creates the sample document.
    ///// </summary>
    //static Document CreateDocument()
    //{
    //  // Create a new MigraDoc document
    //  Document document = new Document();
    //  document.Info.Title = "Hello, MigraDoc";
    //  document.Info.Subject = "Demonstrates an excerpt of the capabilities of MigraDoc.";
    //  document.Info.Author = "Stefan Lange";

    //  Styles.DefineStyles(document);

    //  Cover.DefineCover(document);
    //  TableOfContents.DefineTableOfContents(document);

    //  DefineContentSection(document);

    //  Paragraphs.DefineParagraphs(document);
    //  Tables.DefineTables(document);
    //  Charts.DefineCharts(document);

    //  return document;
    //}

    ///// <summary>
    ///// Creates an absolutely minimalistic document.
    ///// </summary>
    //static void DefineContentSection(Document document)
    //{
    //  Section section = document.AddSection();
    //  section.PageSetup.OddAndEvenPagesHeaderFooter = true;
    //  section.PageSetup.StartingNumber = 1;

    //  HeaderFooter header = section.Headers.Primary;
    //  header.AddParagraph("\tOdd Page Header");
      
    //  header = section.Headers.EvenPage;
    //  header.AddParagraph("Even Page Header");

    //  // Create a paragraph with centered page number. See definition of style "Footer".
    //  Paragraph paragraph = new Paragraph();
    //  paragraph.AddTab();
    //  paragraph.AddPageField();

    //  // Add paragraph to footer for odd pages.
    //  section.Footers.Primary.Add(paragraph);
    //  // Add clone of paragraph to footer for odd pages. Cloning is necessary because an object must
    //  // not belong to more than one other object. If you forget cloning an exception is thrown.
    //  section.Footers.EvenPage.Add(paragraph.Clone());
    //}
  }
}
