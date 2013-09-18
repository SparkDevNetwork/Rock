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
using MigraDoc.DocumentObjectModel.Fields;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Renders an information field to RTF.
  /// </summary>
  internal class InfoFieldRenderer : FieldRenderer
  {
    internal InfoFieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.infoField = domObj as InfoField;
    }

    /// <summary>
    /// Renders an InfoField to RTF.
    /// </summary>
    internal override void Render()
    {
      StartField();
      this.rtfWriter.WriteText("INFO ");
      switch (this.infoField.Name.ToLower())
      {
        case "author":
          this.rtfWriter.WriteText("Author");
          break;

        case "title":
          this.rtfWriter.WriteText("Title");
          break;

        case "keywords":
          this.rtfWriter.WriteText("Keywords");
          break;

        case "subject":
          this.rtfWriter.WriteText("Subject");
          break;
      }
      EndField();
    }

    /// <summary>
    /// Gets the requested document info if available.
    /// </summary>
    protected override string GetFieldResult()
    {
      Document doc = this.infoField.Document;
      if (!doc.IsNull("Info." + this.infoField.Name))
        return doc.Info.GetValue(this.infoField.Name) as string;

      return "";
    }
    InfoField infoField;
  }
}
