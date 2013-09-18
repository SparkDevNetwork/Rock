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
using MigraDoc.DocumentObjectModel.Internals;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Renders a font to RTF.
  /// </summary>
  internal class FontRenderer : RendererBase
  {
    internal FontRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.font = domObj as Font;
    }

    /// <summary>
    /// Renders a font to RTF.
    /// </summary>
    internal override void Render()
    {
      this.useEffectiveValue = true;

      string name = (string)GetValueAsIntended("Name");
      Debug.Assert(name != "");

      if (name != null)
        this.rtfWriter.WriteControl("f", this.docRenderer.GetFontIndex(name));

      Translate("Size", "fs", RtfUnit.HalfPts, null, false);
      TranslateBool("Bold", "b", "b0", false);
      Translate("Underline", "ul");
      TranslateBool("Italic", "i", "i0", false);
      Translate("Color", "cf");

      object objectSub = this.font.GetValue("Subscript", GV.GetNull);
      if (objectSub != null && (bool)(objectSub))
        this.rtfWriter.WriteControl("sub");

      object objectSuper = this.font.GetValue("Superscript", GV.GetNull);
      if (objectSuper != null && (bool)(objectSuper))
        this.rtfWriter.WriteControl("super");
    }
    Font font;
  }
}