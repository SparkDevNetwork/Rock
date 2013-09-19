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
using MigraDoc.DocumentObjectModel.IO;
using MigraDoc.DocumentObjectModel.Internals;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Render the format information of a cell.
  /// </summary>
  internal class CellFormatRenderer : RendererBase
  {
    internal CellFormatRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.cell = domObj as Cell;
    }

    /// <summary>
    /// Renders the cell's shading, borders and so on (used by the RowRenderer).
    /// </summary>
    internal override void Render()
    {
      useEffectiveValue = true;
      this.coveringCell = this.cellList.GetCoveringCell(this.cell);
      Borders borders = this.cellList.GetEffectiveBorders(this.coveringCell);
      if (this.cell.Column.Index != this.coveringCell.Column.Index)
        return;

      if (borders != null)
      {
        BordersRenderer brdrsRenderer = new BordersRenderer(borders, this.docRenderer);
        brdrsRenderer.leaveAwayLeft = this.cell.Column.Index != this.coveringCell.Column.Index;
        brdrsRenderer.leaveAwayTop = this.cell.Row.Index != this.coveringCell.Row.Index;
        brdrsRenderer.leaveAwayBottom = this.cell.Row.Index != this.coveringCell.Row.Index + this.coveringCell.MergeDown;
        brdrsRenderer.leaveAwayRight = false;
        brdrsRenderer.ParentCell = this.cell;
        brdrsRenderer.Render();
      }
      if (cell == this.coveringCell)
      {
        RenderLeftRightPadding();
        Translate("VerticalAlignment", "clvertal");
      }
      object obj = this.coveringCell.GetValue("Shading", GV.GetNull);
      if (obj != null)
        new ShadingRenderer((DocumentObject)obj, this.docRenderer).Render();

      //Note that vertical and horizontal merging are not symmetrical.
      //Horizontally merged cells are simply rendered as bigger cells.
      if (this.cell.Row.Index == this.coveringCell.Row.Index && this.coveringCell.MergeDown > 0)
        this.rtfWriter.WriteControl("clvmgf");

      if (this.cell.Row.Index > this.coveringCell.Row.Index)
        this.rtfWriter.WriteControl("clvmrg");

      this.rtfWriter.WriteControl("cellx", GetRightCellBoundary());
    }

    private void RenderLeftRightPadding()
    {
      string clPadCtrl = "clpad";
      string cellPadUnit = "clpadf";
      object cellPdgVal = this.cell.Column.GetValue("LeftPadding", GV.GetNull);
      if (cellPdgVal == null)
        cellPdgVal = Unit.FromCentimeter(0.12);

      //Top and left padding are mixed up in word:
      this.rtfWriter.WriteControl(clPadCtrl + "t", ToRtfUnit((Unit)cellPdgVal, RtfUnit.Twips));
      //Tells the RTF reader to take it as twips:
      this.rtfWriter.WriteControl(cellPadUnit + "t", 3);
      cellPdgVal = this.cell.Column.GetValue("RightPadding", GV.GetNull);
      if (cellPdgVal == null)
        cellPdgVal = Unit.FromCentimeter(0.12);

      this.rtfWriter.WriteControl(clPadCtrl + "r", ToRtfUnit((Unit)cellPdgVal, RtfUnit.Twips));
      //Tells the RTF reader to take it as Twips:
      this.rtfWriter.WriteControl(cellPadUnit + "r", 3);
    }

    /// <summary>
    /// Gets the right boundary of the cell which is currently rendered.
    /// </summary>
    private int GetRightCellBoundary()
    {
      int rightClmIdx = this.coveringCell.Column.Index + this.coveringCell.MergeRight;
      double width = RowsRenderer.CalculateLeftIndent(this.cell.Table.Rows).Point;
      for (int idx = 0; idx <= rightClmIdx; ++idx)
      {
        object obj = this.cell.Table.Columns[idx].GetValue("Width", GV.GetNull);
        if (obj != null)
          width += ((Unit)obj).Point;
        else
          width += ((Unit)"2.5cm").Point;
      }
      return ToRtfUnit(new Unit((double)width), RtfUnit.Twips);
    }

    /// <summary>
    /// Sets the MergedCellList received from the DOM table. This property is set by the RowRenderer.
    /// </summary>
    internal MergedCellList CellList
    {
      set
      {
        this.cellList = value;
      }
    }
    MergedCellList cellList = null;
    Cell coveringCell;
    Cell cell;
  }
}
