#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   Klaus Potzesny (mailto:Klaus.Potzesny@pdfsharp.com)
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
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Class to dynamically create renderers.
  /// </summary>
  internal sealed class RendererFactory
  {
    /// <summary>
    /// Dynamically creates a renderer for the given document object.
    /// </summary>
    internal static RendererBase CreateRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
    {
      switch (domObj.GetType().Name)
      {
        case "Style":
          return new StyleRenderer(domObj, docRenderer);

        case "ParagraphFormat":
          return new ParagraphFormatRenderer(domObj, docRenderer);

        case "Font":
          return new FontRenderer(domObj, docRenderer);

        case "Borders":
          return new BordersRenderer(domObj, docRenderer);

        case "Border":
          return new BorderRenderer(domObj, docRenderer);

        case "TabStops":
          return new TabStopsRenderer(domObj, docRenderer);

        case "TabStop":
          return new TabStopRenderer(domObj, docRenderer);

        case "Section":
          return new SectionRenderer(domObj, docRenderer);

        case "PageSetup":
          return new PageSetupRenderer(domObj, docRenderer);

        case "Paragraph":
          return new ParagraphRenderer(domObj, docRenderer);

        case "Text":
          return new TextRenderer(domObj, docRenderer);

        case "FormattedText":
          return new FormattedTextRenderer(domObj, docRenderer);

        case "Character":
          return new CharacterRenderer(domObj, docRenderer);
        //Fields start
        case "BookmarkField":
          return new BookmarkFieldRenderer(domObj, docRenderer);

        case "PageField":
          return new PageFieldRenderer(domObj, docRenderer);

        case "PageRefField":
          return new PageRefFieldRenderer(domObj, docRenderer);

        case "NumPagesField":
          return new NumPagesFieldRenderer(domObj, docRenderer);

        case "SectionField":
          return new SectionFieldRenderer(domObj, docRenderer);

        case "SectionPagesField":
          return new SectionPagesFieldRenderer(domObj, docRenderer);

        case "InfoField":
          return new InfoFieldRenderer(domObj, docRenderer);

        case "DateField":
          return new DateFieldRenderer(domObj, docRenderer);
        //Fields end
        case "Hyperlink":
          return new HyperlinkRenderer(domObj, docRenderer);

        case "Footnote":
          return new FootnoteRenderer(domObj, docRenderer);

        case "ListInfo":
          return new ListInfoRenderer(domObj, docRenderer);

        case "Image":
          return new ImageRenderer(domObj, docRenderer);

        case "TextFrame":
          return new TextFrameRenderer(domObj, docRenderer);

        case "Chart":
          return new ChartRenderer(domObj, docRenderer);

        case "HeadersFooters":
          return new HeadersFootersRenderer(domObj, docRenderer);

        case "HeaderFooter":
          return new HeaderFooterRenderer(domObj, docRenderer);

        case "PageBreak":
          return new PageBreakRenderer(domObj, docRenderer);
        //Table
        case "Table":
          return new TableRenderer(domObj, docRenderer);

        case "Row":
          return new RowRenderer(domObj, docRenderer);

        case "Cell":
          return new CellRenderer(domObj, docRenderer);
        //Table end
      }
      return null;
    }
  }
}
