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
using MigraDoc.DocumentObjectModel.Visitors;
using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Class to render a Row to RTF.
  /// </summary>
  internal class RowRenderer : RendererBase
  {
    internal RowRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.row = domObj as Row;
    }

    /// <summary>
    /// Render a Row to RTF.
    /// </summary>
    internal override void Render()
    {
      this.useEffectiveValue = true;
      this.rtfWriter.WriteControl("trowd");
      new RowsRenderer(DocumentRelations.GetParent(this.row) as Rows, this.docRenderer).Render();
      RenderRowHeight();
      //MigraDoc always keeps together table rows.
      this.rtfWriter.WriteControl("trkeep");
      Translate("HeadingFormat", "trhdr");

      // trkeepfollow is intended to keep table rows together.
      // Unfortunalte, this does not work in word.
      int thisRowIdx = this.row.Index;
      for (int rowIdx = 0; rowIdx <= this.row.Index; ++rowIdx)
      {
        object keepWith = this.row.Table.Rows[rowIdx].GetValue("KeepWith");
        if (keepWith != null && (int)keepWith + rowIdx > thisRowIdx)
          this.rtfWriter.WriteControl("trkeepfollow");
      }
      RenderTopBottomPadding();

      //Cell borders etc. are written before the contents.
      for (int idx = 0; idx < this.row.Table.Columns.Count; ++idx)
      {
        Cell cell = this.row.Cells[idx];
        CellFormatRenderer cellFrmtRenderer =
        new CellFormatRenderer(cell, this.docRenderer);
        cellFrmtRenderer.CellList = this.cellList;
        cellFrmtRenderer.Render();
      }
      foreach (Cell cell in this.row.Cells)
      {
        CellRenderer cellRndrr = new CellRenderer(cell, this.docRenderer);
        cellRndrr.CellList = this.cellList;
        cellRndrr.Render();
      }

      this.rtfWriter.WriteControl("row");
    }

    private void RenderTopBottomPadding()
    {
      string rwPadCtrl = "trpadd";
      string rwPadUnit = "trpaddf";
      object rwPdgVal = this.row.GetValue("TopPadding", GV.GetNull);
      if (rwPdgVal == null)
        rwPdgVal = Unit.FromCentimeter(0);

      //Word bug: Top and leftpadding are being confused in word.
      this.rtfWriter.WriteControl(rwPadCtrl + "t", ToRtfUnit((Unit)rwPdgVal, RtfUnit.Twips));
      //Tells the RTF reader to take it as Twips:
      this.rtfWriter.WriteControl(rwPadUnit + "t", 3);
      rwPdgVal = this.row.GetValue("BottomPadding", GV.GetNull);
      if (rwPdgVal == null)
        rwPdgVal = Unit.FromCentimeter(0);

      this.rtfWriter.WriteControl(rwPadCtrl + "b", ToRtfUnit((Unit)rwPdgVal, RtfUnit.Twips));
      this.rtfWriter.WriteControl(rwPadUnit + "b", 3);
    }
    private void RenderRowHeight()
    {
      object heightObj = GetValueAsIntended("Height");
      object heightRlObj = GetValueAsIntended("HeightRule");
      if (heightRlObj != null)
      {
        switch ((RowHeightRule)heightRlObj)
        {
          case RowHeightRule.AtLeast:
            Translate("Height", "trrh", RtfUnit.Twips, "0", false);
            break;
          case RowHeightRule.Auto:
            this.rtfWriter.WriteControl("trrh", 0);
            break;

          case RowHeightRule.Exactly:
            if (heightObj != null)
              RenderUnit("trrh", -((Unit)heightObj).Point);
            break;
        }
      }
      else
        Translate("Height", "trrh", RtfUnit.Twips, "0", false); //treat it like "AtLeast".
    }

    /// <summary>
    /// Sets the merged cell list. This property is set by the table renderer.
    /// </summary>
    internal MergedCellList CellList
    {
      set
      {
        this.cellList = value;
      }
    }
    MergedCellList cellList = null;
    Row row;
  }
}
