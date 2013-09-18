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
using MigraDoc.RtfRendering.Resources;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Base class for all classes that render fields to RTF.
  /// </summary>
  internal abstract class FieldRenderer : RendererBase
  {
    internal FieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
    }

    /// <summary>
    /// Starts an RTF field with appropriate control words.
    /// </summary>
    protected void StartField()
    {
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("field");
      this.rtfWriter.WriteControl("flddirty");
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("fldinst", true);
    }

    /// <summary>
    /// Ends an RTF field with appropriate control words.
    /// </summary>
    protected void EndField()
    {
      this.rtfWriter.EndContent();
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("fldrslt");
      this.rtfWriter.WriteText(GetFieldResult());
      this.rtfWriter.EndContent();
      this.rtfWriter.EndContent();
    }

    /// <summary>
    /// Gets the field result if possible.
    /// </summary>
    protected virtual string GetFieldResult()
    {
      return Messages.UpdateField;
    }
  }
}
