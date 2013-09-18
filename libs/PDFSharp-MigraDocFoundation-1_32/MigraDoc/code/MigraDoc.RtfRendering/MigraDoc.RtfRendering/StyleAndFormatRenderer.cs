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
using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.IO;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Base class to render objects that have a style and a format attribute (currently cells, paragraphs).
  /// </summary>
  internal abstract class StyleAndFormatRenderer : RendererBase
  {
    internal StyleAndFormatRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
    }

    /// <summary>
    /// Renders format and style. Always call EndFormatAndStyleAfterContent() after the content was written.
    /// </summary>
    protected void RenderStyleAndFormat()
    {
      object styleName = GetValueAsIntended("Style");
      object parStyleName = styleName;
      Style style = this.docRenderer.Document.Styles[(string)styleName];
      this.hasCharacterStyle = false;
      if (style != null)
      {
        if (((Style)style).Type == StyleType.Character)
        {
          this.hasCharacterStyle = true;
          parStyleName = "Normal";
        }
      }
      else
        parStyleName = null;

      if (parStyleName != null)
        this.rtfWriter.WriteControl("s", this.docRenderer.GetStyleIndex((string)parStyleName));

      ParagraphFormat frmt = GetValueAsIntended("Format") as ParagraphFormat;
      RendererFactory.CreateRenderer(frmt, this.docRenderer).Render();
      this.rtfWriter.WriteControl("brdrbtw");//Sollte Border trennen, funktioniert aber nicht.
      if (this.hasCharacterStyle)
      {
        this.rtfWriter.StartContent();
        this.rtfWriter.WriteControlWithStar("cs", this.docRenderer.GetStyleIndex((string)styleName));
        object font = GetValueAsIntended("Format.Font");
        if (font != null)
          new FontRenderer(((Font)font), this.docRenderer).Render();
      }
    }


    /// <summary>
    /// Ends the format and style rendering. Always paired with RenderStyleAndFormat().
    /// </summary>
    protected void EndStyleAndFormatAfterContent()
    {
      if (this.hasCharacterStyle)
      {
        this.rtfWriter.EndContent();
      }
    }
    private bool hasCharacterStyle = false;
  }
}
