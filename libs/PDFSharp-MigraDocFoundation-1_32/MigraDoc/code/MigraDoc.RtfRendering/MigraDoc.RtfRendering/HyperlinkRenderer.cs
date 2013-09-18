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
using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.IO;
using MigraDoc.DocumentObjectModel.Internals;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Renders a Hyperlink to RTF.
  /// </summary>
  internal class HyperlinkRenderer : RendererBase
  {
    internal HyperlinkRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.hyperlink = domObj as Hyperlink;
    }

    /// <summary>
    /// Renders a hyperlink to RTF.
    /// </summary>
    internal override void Render()
    {
      useEffectiveValue = true;
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("field");
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("fldinst", true);
      this.rtfWriter.WriteText("HYPERLINK ");
      string name = this.hyperlink.Name;
      if (this.hyperlink.IsNull("Type") || this.hyperlink.Type == HyperlinkType.Local)
      {
        name = BookmarkFieldRenderer.MakeValidBookmarkName(this.hyperlink.Name);
        this.rtfWriter.WriteText(@"\l ");
      }
      else if (this.hyperlink.Type == HyperlinkType.File)
      {
        string workingDirectory = this.docRenderer.WorkingDirectory;
        if (workingDirectory != null)
          name = Path.Combine(this.docRenderer.WorkingDirectory, name);

        name = name.Replace(@"\", @"\\");
      }

      this.rtfWriter.WriteText("\"" + name + "\"");
      this.rtfWriter.EndContent();
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("fldrslt");
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("cs", this.docRenderer.GetStyleIndex("Hyperlink"));

      FontRenderer fontRenderer = new FontRenderer(this.hyperlink.Font, this.docRenderer);
      fontRenderer.Render();

      if (!this.hyperlink.IsNull("Elements"))
      {
        foreach (DocumentObject domObj in hyperlink.Elements)
          RendererFactory.CreateRenderer(domObj, this.docRenderer).Render();
      }
      this.rtfWriter.EndContent();
      this.rtfWriter.EndContent();
      this.rtfWriter.EndContent();
    }
    Hyperlink hyperlink;
  }
}
