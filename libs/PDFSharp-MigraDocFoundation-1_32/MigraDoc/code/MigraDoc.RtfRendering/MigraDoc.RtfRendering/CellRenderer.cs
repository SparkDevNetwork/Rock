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
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Renders a cell to RTF.
  /// </summary>
  internal class CellRenderer : StyleAndFormatRenderer
  {
    internal CellRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.cell = domObj as Cell;
    }

    internal override void Render()
    {
      useEffectiveValue = true;
      Cell cvrgCell = this.cellList.GetCoveringCell(this.cell);
      if (this.cell.Column.Index != cvrgCell.Column.Index)
        return;

      bool writtenAnyContent = false;
      if (!this.cell.IsNull("Elements"))
      {
        if (this.cell == cvrgCell)
        {
          foreach (DocumentObject docObj in this.cell.Elements)
          {
            RendererBase rndrr = RendererFactory.CreateRenderer(docObj, this.docRenderer);
            if (rndrr != null)
            {
              rndrr.Render();
              writtenAnyContent = true;
            }
          }
        }
      }
      if (!writtenAnyContent)
      {
        //Format attributes need to be set here to satisfy Word 2000.
        this.rtfWriter.WriteControl("pard");
        RenderStyleAndFormat();
        this.rtfWriter.WriteControl("intbl");
        EndStyleAndFormatAfterContent();
      }
      this.rtfWriter.WriteControl("cell");
    }

    internal MergedCellList CellList
    {
      set
      {
        this.cellList = value;
      }
    }
    MergedCellList cellList = null;
    Cell cell;
  }
}
