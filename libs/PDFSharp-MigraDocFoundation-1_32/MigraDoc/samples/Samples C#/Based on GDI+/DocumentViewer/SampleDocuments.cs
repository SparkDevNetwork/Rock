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
using MigraDoc.DocumentObjectModel;
using HelloMigraDoc;

namespace DocumentViewer
{
  sealed class SampleDocuments
  {
    /// <summary>
    /// Creates the initial sample document.
    /// </summary>
    public static Document CreateSample1()
    {
      // Create a new MigraDoc document
      Document document = new Document();

      // Add a section to the document
      Section section = document.AddSection();

      // Add a paragraph to the section
      Paragraph paragraph = section.AddParagraph();

      // Add some text to the paragraph
      paragraph.AddFormattedText("Hi!");

      paragraph = section.AddParagraph();
      paragraph.AddText("This is a MigraDoc document created for the DocumentViewer sample application.");
      paragraph.AddText("The DocumentViewer demonstrates all techniques you need to preview and print a MigraDoc document, and convert it to a PDF, RTF, or image file.");

      section.AddParagraph();
      section.AddParagraph();
      paragraph = section.AddParagraph("A4 portrait");
      paragraph.Format.Font.Size = "1.5cm";

      section.AddPageBreak();
      section.AddParagraph().AddText("Page 2");

      section = document.AddSection();
      section.PageSetup.Orientation = Orientation.Landscape;

      paragraph = section.AddParagraph("A4 landscape");
      paragraph.Format.Font.Size = "1.5cm";

      section.AddPageBreak();
      section.AddParagraph().AddText("Page 4");

      section = document.AddSection();
      section.PageSetup.Orientation = Orientation.Portrait;
      section.PageSetup.PageFormat = PageFormat.A5;

      paragraph = section.AddParagraph("A5 portrait");
      paragraph.Format.Font.Size = "1.5cm";

      section.AddPageBreak();
      section.AddParagraph().AddText("Page 6");

      return document;
    }

    /// <summary>
    /// Creates the document from sample HelloMigraDoc.
    /// </summary>
    public static Document CreateSample2()
    {
      return Documents.CreateDocument();
    }
  }
}
