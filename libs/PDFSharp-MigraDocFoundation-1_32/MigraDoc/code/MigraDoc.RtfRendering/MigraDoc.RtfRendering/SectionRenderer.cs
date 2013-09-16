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

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Class to render a Section to RTF.
  /// </summary>
  internal class SectionRenderer : RendererBase
  {
    internal SectionRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.section = domObj as Section;
    }

    /// <summary>
    /// Renders a section to RTF
    /// </summary>
    internal override void Render()
    {
      this.useEffectiveValue = true;

      Sections secs = DocumentRelations.GetParent(this.section) as Sections;
      if (this.section != secs.First)
      {
        this.rtfWriter.WriteControl("pard");
        this.rtfWriter.WriteControl("sect");
      }
      this.rtfWriter.WriteControl("sectd");

      //Rendering some footnote attributes:
      this.docRenderer.RenderSectionProperties();

      object pageStp = this.section.PageSetup;
      if (pageStp != null)
        RendererFactory.CreateRenderer((PageSetup)pageStp, this.docRenderer).Render();

      object hdrs = GetValueAsIntended("Headers");
      if (hdrs != null)
      {
        HeadersFootersRenderer hfr = new HeadersFootersRenderer(hdrs as HeadersFooters, this.docRenderer);
        //PageSetup muss hier gesetzt werden, da die HeaderFooter anderem Abschnitt gehören können als das PageSetup
        hfr.PageSetup = (PageSetup)pageStp;
        hfr.Render();
      }

      object ftrs = GetValueAsIntended("Footers");
      if (ftrs != null)
      {
        HeadersFootersRenderer hfr = new HeadersFootersRenderer(ftrs as HeadersFooters, this.docRenderer);
        hfr.PageSetup = (PageSetup)pageStp;
        hfr.Render();
      }

      if (!section.IsNull("Elements"))
      {
        foreach (DocumentObject docObj in this.section.Elements)
        {
          RendererBase rndrr = RendererFactory.CreateRenderer(docObj, this.docRenderer);
          if (rndrr != null)
            rndrr.Render();
        }
      }
    }
    Section section;
  }
}
