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
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Internals;
using MigraDoc.DocumentObjectModel.IO;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Class to render a Row to RTF..
  /// </summary>
  internal class RowsRenderer : RendererBase
  {
    internal RowsRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.rows = domObj as Rows;
    }

    internal override void Render()
    {
      Translate("Alignment", "trq");
      this.rtfWriter.WriteControl("trleft", ToTwips(CalculateLeftIndent(this.rows)));
    }

    internal static Unit CalculateLeftIndent(Rows rows)
    {
      object leftInd = rows.GetValue("LeftIndent", GV.GetNull);
      if (leftInd == null)
      {
        leftInd = rows.Table.Columns[0].GetValue("LeftPadding", GV.GetNull);
        if (leftInd == null)
          leftInd = Unit.FromCentimeter(-0.12);
        else
          leftInd = Unit.FromPoint(-((Unit)leftInd));

        Cell cell = rows[0].Cells[0];

        object visible = cell.GetValue("Borders.Left.Visible", GV.GetNull);
        object lineWidth = cell.GetValue("Borders.Left.Width", GV.GetNull);

        object style = cell.GetValue("Borders.Left.Style", GV.GetNull);
        object color = cell.GetValue("Borders.Left.Color", GV.GetNull);

        if (visible == null || (bool)visible)
        {
          if (lineWidth != null || style != null || color != null)
          {
            if (style != null && (BorderStyle)style != BorderStyle.None)
            {
              if (lineWidth != null)
                leftInd = Unit.FromPoint(((Unit)leftInd).Point - ((Unit)lineWidth).Point);
              else
                leftInd = Unit.FromPoint(((Unit)leftInd).Point - 0.5);
            }
          }
        }
      }
      return (Unit)leftInd;
    }
    Rows rows;
  }


}
