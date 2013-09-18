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
using System.Collections.Specialized;
using MigraDoc.DocumentObjectModel.Internals;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Renders a header or footer to RTF.
  /// </summary>
  internal class HeaderFooterRenderer : RendererBase
  {
    internal HeaderFooterRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.headerFooter = domObj as HeaderFooter;
    }

    /// <summary>
    /// Renders a single Header or Footer.
    /// </summary>
    internal override void Render()
    {
      StringCollection hdrFtrCtrls = GetHeaderFooterControls();
      foreach (string hdrFtrCtrl in hdrFtrCtrls)
      {
        this.rtfWriter.StartContent();
        this.rtfWriter.WriteControl(hdrFtrCtrl);
        //REM: Could be optimized by storing rendering results in a strings.
        foreach (DocumentObject docObj in this.headerFooter.Elements)
        {
          RendererBase rndrr = RendererFactory.CreateRenderer(docObj, this.docRenderer);
          if (rndrr != null)
            rndrr.Render();
        }
        this.rtfWriter.EndContent();
      }
    }

    /// <summary>
    /// Gets a collection of RTF header/footer control words the HeaderFooter is rendered in.
    /// (e.g. the primary header might be rendered into the rtf controls headerl and headerr for left and right pages.)
    /// </summary>
    private StringCollection GetHeaderFooterControls()
    {
      StringCollection retColl = new StringCollection();
      bool differentFirst = (bool)this.pageSetup.GetValue("DifferentFirstPageHeaderFooter", GV.GetNull);
      bool oddEven = (bool)this.pageSetup.GetValue("OddAndEvenPagesHeaderFooter", GV.GetNull);
      string ctrlBase = this.headerFooter.IsHeader ? "header" : "footer";
      if (renderAs == HeaderFooterIndex.FirstPage)
      {
        if (differentFirst)
          retColl.Add(ctrlBase + "f");
      }
      else if (renderAs == HeaderFooterIndex.EvenPage)
      {
        if (oddEven)
          retColl.Add(ctrlBase + "l");
      }
      else if (renderAs == HeaderFooterIndex.Primary)//clearly
      {
        retColl.Add(ctrlBase + "r");
        if (!oddEven)
          retColl.Add(ctrlBase + "l");
      }
      return retColl;
    }

    /// <summary>
    /// Sets the PageSetup (It stems from the section the currently HeaderFooter is used in).
    /// Caution: This PageSetup might differ from the one the "parent" section's got
    /// for inheritance reasons. This value is set by the HeadersFootersRenderer.
    /// </summary>
    internal PageSetup PageSetup
    {
      set
      {
        this.pageSetup = value;
      }
    }

    /// <summary>
    /// Sets the HeaderFooterIndex (Primary, Even FirstPage) the rendered HeaderFooer
    /// shall represent. This value is set by the HeadersFootersRenderer.
    /// </summary>
    internal HeaderFooterIndex RenderAs
    {
      set
      {
        this.renderAs = value;
      }
    }
    private HeaderFooterIndex renderAs;
    private PageSetup pageSetup;
    private HeaderFooter headerFooter;
  }
}
