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
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Shapes.Charts;

namespace HelloMigraDoc
{
  /// <summary>
  /// 
  /// </summary>
  public class Charts
  {
    public static void DefineCharts(Document document)
    {
      Paragraph paragraph = document.LastSection.AddParagraph("Chart Overview", "Heading1");
      paragraph.AddBookmark("Charts");

      document.LastSection.AddParagraph("Sample Chart", "Heading2");

      Chart chart = new Chart();
      chart.Left = 0;

      chart.Width = Unit.FromCentimeter(16);
      chart.Height = Unit.FromCentimeter(12);
      Series series = chart.SeriesCollection.AddSeries();
      series.ChartType = ChartType.Column2D;
      series.Add(new double[]{1, 17, 45, 5, 3, 20, 11, 23, 8, 19});
      series.HasDataLabel = true;

      series = chart.SeriesCollection.AddSeries();
      series.ChartType = ChartType.Line;
      series.Add(new double[]{41, 7, 5, 45, 13, 10, 21, 13, 18, 9});

      XSeries xseries = chart.XValues.AddXSeries();
      xseries.Add("A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N");

      chart.XAxis.MajorTickMark = TickMarkType.Outside;
      chart.XAxis.Title.Caption = "X-Axis";
      
      chart.YAxis.MajorTickMark = TickMarkType.Outside;
      chart.YAxis.HasMajorGridlines = true;

      chart.PlotArea.LineFormat.Color = Colors.DarkGray;
      chart.PlotArea.LineFormat.Width = 1;

      document.LastSection.Add(chart);
    }
  }
}
