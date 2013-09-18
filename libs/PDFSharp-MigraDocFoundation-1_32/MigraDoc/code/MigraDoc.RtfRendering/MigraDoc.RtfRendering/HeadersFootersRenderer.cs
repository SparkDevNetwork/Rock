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
  /// Renders headers and footers to RTF.
  /// </summary>
  internal class HeadersFootersRenderer : RendererBase
  {
    internal HeadersFootersRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.headersFooters = domObj as HeadersFooters;
    }


    /// <summary>
    /// Renders a section's headers and footers to RTF.
    /// </summary>
    /* The MigraDoc DOM page setup properties, the MigraDoc DOM headers and the
     * RTF Controls For left, right and first header/footer 
     * and the RTF flag titlepg are related as follows.
     * 
     * |  First | OddEvn | | Left  | Right | First |TitlePg |
     * ------------------------------------------------------
     * |  True  | True   | | EvnPg | Prim  | First |  True  |
     * |  False | False  | | Prim  | Prim  |   -   |  False |
     * |  True  | False  | | Prim  | Prim  | First |  True  |
     * |  False | True   | | EvnPg | Prim  |   -   |  False |
    */
    internal override void Render()
    {
      useEffectiveValue = true;
      object obj = GetValueAsIntended("Primary");
      if (obj != null)
        RenderHeaderFooter((HeaderFooter)obj, HeaderFooterIndex.Primary);

      obj = GetValueAsIntended("FirstPage");
      if (obj != null)
        RenderHeaderFooter((HeaderFooter)obj, HeaderFooterIndex.FirstPage);

      obj = GetValueAsIntended("EvenPage");
      if (obj != null)
        RenderHeaderFooter((HeaderFooter)obj, HeaderFooterIndex.EvenPage);
    }

    /// <summary>
    /// Renders a single header footer.
    /// </summary>
    private void RenderHeaderFooter(HeaderFooter hdrFtr, HeaderFooterIndex renderAs)
    {
      HeaderFooterRenderer hfr = new HeaderFooterRenderer(hdrFtr, this.docRenderer);
      hfr.PageSetup = this.pageSetup;
      hfr.RenderAs = renderAs;
      hfr.Render();
    }

    /// <summary>
    /// Sets the PageSetup (It stems from the section the HeadersFooters are used in).
    /// Caution: This PageSetup might differ from the one the "parent" section's got
    /// for inheritance reasons.
    /// </summary>
    internal PageSetup PageSetup
    {
      set
      {
        this.pageSetup = value;
      }
    }
    private HeadersFooters headersFooters;
    private PageSetup pageSetup;
  }
}
