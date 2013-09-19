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
using MigraDoc.DocumentObjectModel.Internals;
using MigraDoc.DocumentObjectModel.IO;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Class to render a Style to RTF.
  /// </summary>
  internal class StyleRenderer : RendererBase
  {
    internal StyleRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.style = domObj as Style;
      this.styles = DocumentRelations.GetParent(this.style) as Styles;
    }

    internal override void Render()
    {
      this.rtfWriter.StartContent();
      int currIdx = this.styles.GetIndex(this.style.Name);
      RendererBase rndrr = null;
      if (this.style.Type == StyleType.Character)
      {
        this.rtfWriter.WriteControlWithStar("cs", currIdx);
        this.rtfWriter.WriteControl("additive");
        rndrr = RendererFactory.CreateRenderer(this.style.Font, this.docRenderer);
      }
      else
      {
        this.rtfWriter.WriteControl("s", currIdx);
        rndrr = RendererFactory.CreateRenderer(this.style.ParagraphFormat, this.docRenderer);
      }
      if (this.style.BaseStyle != "")
      {
        int bsIdx = this.styles.GetIndex(this.style.BaseStyle);
        this.rtfWriter.WriteControl("sbasedon", bsIdx);
      }
      rndrr.Render();
      this.rtfWriter.WriteText(this.style.Name);
      this.rtfWriter.WriteSeparator();
      this.rtfWriter.EndContent();
    }
    private Style style;
    private Styles styles;
  }
}
