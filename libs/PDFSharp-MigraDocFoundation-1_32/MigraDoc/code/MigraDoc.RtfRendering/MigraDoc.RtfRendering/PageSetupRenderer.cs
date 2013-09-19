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
using System.Collections;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Class to render a PageSetup to RTF.
  /// </summary>
  internal class PageSetupRenderer : RendererBase
  {
    internal PageSetupRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.pageSetup = domObj as PageSetup;
    }

    /// <summary>
    /// Render a PageSetup object to RTF.
    /// </summary>
    internal override void Render()
    {
      useEffectiveValue = true;
      object obj = GetValueAsIntended("DifferentFirstPageHeaderFooter");
      if (obj != null && (bool)obj)
        this.rtfWriter.WriteControl("titlepg");

      obj = GetValueAsIntended("Orientation");
      if (obj != null && (Orientation)obj == Orientation.Landscape)
        this.rtfWriter.WriteControl("lndscpsxn");
      RenderPageSize(obj == null ? Orientation.Portrait : (Orientation)obj);
      RenderPageDistances();
      RenderSectionStart();
    }

    /// <summary>
    /// Renders attributes related to the page margins and header footer distances.
    /// </summary>
    private void RenderPageDistances()
    {
      Translate("LeftMargin", "marglsxn");
      Translate("RightMargin", "margrsxn");
      Translate("TopMargin", "margtsxn");
      Translate("BottomMargin", "margbsxn");

      Translate("MirrorMargins", "margmirsxn");
      Translate("HeaderDistance", "headery");
      Translate("FooterDistance", "footery");
    }

    /// <summary>
    /// Renders attributes related to the section start behavior.
    /// </summary>
    private void RenderSectionStart()
    {
      Translate("SectionStart", "sbk");

      object obj = GetValueAsIntended("StartingNumber");
      if (obj != null && (int)obj >= 0)
      {
        //"pgnrestart" needs to be written here so that the starting page number is used.
        this.rtfWriter.WriteControl("pgnrestart");
        this.rtfWriter.WriteControl("pgnstarts", (int)obj);
      }
    }
    /// <summary>
    /// Renders the page size, taking into account Orientation, PageFormat and PageWidth / PageHeight.
    /// </summary>
    private void RenderPageSize(Orientation orient)
    {
      if (orient == Orientation.Landscape)
      {
        RenderUnit("pghsxn", this.pageSetup.PageWidth);
        RenderUnit("pgwsxn", this.pageSetup.PageHeight);
      }
      else
      {
        RenderUnit("pghsxn", this.pageSetup.PageHeight);
        RenderUnit("pgwsxn", this.pageSetup.PageWidth);
      }
    }

    PageSetup pageSetup;
  }
}
