#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   PDFsharp Team (mailto:PDFsharpSupport@pdfsharp.de)
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

namespace HelloMigraDoc
{
  public class Tables
  {
    public static void DefineTables(Document document)
    {
      Paragraph paragraph = document.LastSection.AddParagraph("Table Overview", "Heading1");
      paragraph.AddBookmark("Tables");

      DemonstrateSimpleTable(document);
      DemonstrateAlignment(document);
      DemonstrateCellMerge(document);
    }

    public static void DemonstrateSimpleTable(Document document)
    {
      document.LastSection.AddParagraph("Simple Tables", "Heading2");

      Table table = new Table();
      table.Borders.Width = 0.75;

      Column column = table.AddColumn(Unit.FromCentimeter(2));
      column.Format.Alignment = ParagraphAlignment.Center;

      table.AddColumn(Unit.FromCentimeter(5));

      Row row = table.AddRow();
      row.Shading.Color = Colors.PaleGoldenrod;
      Cell cell = row.Cells[0];
      cell.AddParagraph("Itemus");
      cell = row.Cells[1];
      cell.AddParagraph("Descriptum");

      row = table.AddRow();
      cell = row.Cells[0];
      cell.AddParagraph("1");
      cell = row.Cells[1];
      cell.AddParagraph(FillerText.ShortText);

      row = table.AddRow();
      cell = row.Cells[0];
      cell.AddParagraph("2");
      cell = row.Cells[1];
      cell.AddParagraph(FillerText.Text);

      table.SetEdge(0, 0, 2, 3, Edge.Box, BorderStyle.Single, 1.5, Colors.Black);

      document.LastSection.Add(table);
    }

    public static void DemonstrateAlignment(Document document)
    {
      document.LastSection.AddParagraph("Cell Alignment", "Heading2");

      Table table = document.LastSection.AddTable();
      table.Borders.Visible = true;
      table.Format.Shading.Color = Colors.LavenderBlush;
      table.Shading.Color = Colors.Salmon;
      table.TopPadding = 5;
      table.BottomPadding = 5;

      Column column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Left;

      column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Center;
      
      column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Right;

      table.Rows.Height = 35;

      Row row = table.AddRow();
      row.VerticalAlignment = VerticalAlignment.Top;
      row.Cells[0].AddParagraph("Text");
      row.Cells[1].AddParagraph("Text");
      row.Cells[2].AddParagraph("Text");

      row = table.AddRow();
      row.VerticalAlignment = VerticalAlignment.Center;
      row.Cells[0].AddParagraph("Text");
      row.Cells[1].AddParagraph("Text");
      row.Cells[2].AddParagraph("Text");

      row = table.AddRow();
      row.VerticalAlignment = VerticalAlignment.Bottom;
      row.Cells[0].AddParagraph("Text");
      row.Cells[1].AddParagraph("Text");
      row.Cells[2].AddParagraph("Text");
    }

    public static void DemonstrateCellMerge(Document document)
    {
      document.LastSection.AddParagraph("Cell Merge", "Heading2");

      Table table = document.LastSection.AddTable();
      table.Borders.Visible = true;
      table.TopPadding = 5;
      table.BottomPadding = 5;

      Column column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Left;

      column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Center;

      column = table.AddColumn();
      column.Format.Alignment = ParagraphAlignment.Right;

      table.Rows.Height = 35;

      Row row = table.AddRow();
      row.Cells[0].AddParagraph("Merge Right");
      row.Cells[0].MergeRight = 1;

      row = table.AddRow();
      row.VerticalAlignment = VerticalAlignment.Bottom;
      row.Cells[0].MergeDown = 1;
      row.Cells[0].VerticalAlignment = VerticalAlignment.Bottom;
      row.Cells[0].AddParagraph("Merge Down");

      table.AddRow();
    }
  }
}
