#region PDFsharp Charting - A .NET charting library based on PDFsharp
//
// Authors:
//   Niklas Schneider (mailto:Niklas.Schneider@pdfsharp.com)
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
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

//
// PDFsharp.Charting - A charting library based on PDFsharp
//
// Authors:
//
// Copyright (c) 2005-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using PdfSharp.Drawing;

namespace PdfSharp.Charting.Demo
{
  /// <summary>
  /// Defines some chart samples.
  /// </summary>
  public class ChartSamples
  {
    public static Chart LineChart()
    {
      Chart chart = new Chart(ChartType.Line);
      Series series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 1";
      series.Add(new double[] { 1, 5, -3, 20, 11 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 2";
      series.Add(new double[] { 22, 4, 12, 8, 12 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 3";
      series.Add(new double[] { 12, 14, -3, 18, 1 });

      chart.XAxis.MajorTickMark = TickMarkType.Outside;
      chart.XAxis.Title.Caption = "X-Axis";

      chart.YAxis.MajorTickMark = TickMarkType.Outside;
      chart.YAxis.Title.Caption = "Y-Axis";
      chart.YAxis.HasMajorGridlines = true;

      chart.PlotArea.LineFormat.Color = XColors.DarkGray;
      chart.PlotArea.LineFormat.Width = 1;
      chart.PlotArea.LineFormat.Visible = true;

      chart.Legend.Docking = DockingType.Bottom;
      chart.Legend.LineFormat.Visible = true;

      XSeries xseries = chart.XValues.AddXSeries();
      xseries.Add("A", "B", "C", "D", "E", "F");

      return chart;
    }

    public static Chart ColumnChart()
    {
      Chart chart = new Chart(ChartType.Column2D);
      Series series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 1";
      series.Add(new double[] { 1, 5, -3, 20, 11 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 2";
      series.Add(new double[] { 22, 4, 12, 8, 12 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 3";
      series.Add(new double[] { 12, 14, 2, 18, 1 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 4";
      series.Add(new double[] { 17, 13, 10, 9, 15 });

      chart.XAxis.TickLabels.Format = "00";
      chart.XAxis.MajorTickMark = TickMarkType.Outside;
      chart.XAxis.Title.Caption = "X-Axis";

      chart.YAxis.MajorTickMark = TickMarkType.Outside;
      chart.YAxis.HasMajorGridlines = true;

      chart.PlotArea.LineFormat.Color = XColors.DarkGray;
      chart.PlotArea.LineFormat.Width = 1;
      chart.PlotArea.LineFormat.Visible = true;

      chart.Legend.Docking = DockingType.Right;

      chart.DataLabel.Type = DataLabelType.Value;
      chart.DataLabel.Position = DataLabelPosition.OutsideEnd;

      return chart;
    }

    public static Chart ColumnStackedChart()
    {
      Chart chart = new Chart(ChartType.ColumnStacked2D);
      Series series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 1";
      series.Add(new double[] { 1, 5, -3, 20, 11 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 2";
      series.Add(new double[] { 22, 4, 12, 8, 12 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 3";
      series.Add(new double[] { 12, 14, 2, 18, 1 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 4";
      series.Add(new double[] { 17, 13, 10, 9, 15 });

      chart.XAxis.TickLabels.Format = "00";
      chart.XAxis.MajorTickMark = TickMarkType.Outside;
      chart.XAxis.Title.Caption = "X-Axis";

      chart.YAxis.MajorTickMark = TickMarkType.Outside;
      chart.YAxis.HasMajorGridlines = true;

      chart.PlotArea.LineFormat.Color = XColors.DarkGray;
      chart.PlotArea.LineFormat.Width = 1;
      chart.PlotArea.LineFormat.Visible = true;

      chart.Legend.Docking = DockingType.Right;

      chart.DataLabel.Type = DataLabelType.Value;
      chart.DataLabel.Position = DataLabelPosition.Center;

      return chart;
    }

    public static Chart BarChart()
    {
      Chart chart = new Chart(ChartType.Bar2D);
      Series series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 1";
      series.Add(new double[] { 1, 5, -3, 20, 11 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 2";
      series.Add(new double[] { 22, 4, 12, 8, 12 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 3";
      series.Add(new double[] { 12, 14, 2, 18, 1 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 4";
      series.Add(new double[] { 17, 13, 10, 9, 15 });

      chart.XAxis.MajorTickMark = TickMarkType.Outside;
      chart.XAxis.Title.Caption = "X-Axis";

      chart.YAxis.MajorTickMark = TickMarkType.Outside;
      chart.YAxis.HasMajorGridlines = true;

      chart.PlotArea.LineFormat.Color = XColors.DarkGray;
      chart.PlotArea.LineFormat.Width = 1;
      chart.PlotArea.LineFormat.Visible = true;

      chart.Legend.Docking = DockingType.Right;

      chart.DataLabel.Type = DataLabelType.Value;
      chart.DataLabel.Position = DataLabelPosition.InsideEnd;

      return chart;
    }

    public static Chart BarStackedChart()
    {
      Chart chart = new Chart(ChartType.BarStacked2D);
      Series series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 1";
      series.Add(new double[] { 1, 5, -3, 20, 11 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 2";
      series.Add(new double[] { 22, 4, 12, 8, 12 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 3";
      series.Add(new double[] { 12, 14, 2, 18, 1 });

      series = chart.SeriesCollection.AddSeries();
      series.Name = "Series 4";
      series.Add(new double[] { 17, 13, 10, 9, 15 });

      chart.XAxis.MajorTickMark = TickMarkType.Outside;
      chart.XAxis.Title.Caption = "X-Axis";

      chart.YAxis.MajorTickMark = TickMarkType.Outside;
      chart.YAxis.HasMajorGridlines = true;

      chart.PlotArea.LineFormat.Color = XColors.DarkGray;
      chart.PlotArea.LineFormat.Width = 1;
      chart.PlotArea.LineFormat.Visible = true;

      chart.Legend.Docking = DockingType.Right;

      chart.DataLabel.Type = DataLabelType.Value;
      chart.DataLabel.Position = DataLabelPosition.Center;

      return chart;
    }

    public static Chart AreaChart()
    {
      Chart chart = new Chart(ChartType.Area2D);
      Series series = chart.SeriesCollection.AddSeries();
      series.Add(new double[] { 31, 9, 15, 28, 13 });

      series = chart.SeriesCollection.AddSeries();
      series.Add(new double[] { 22, 7, 12, 21, 12 });

      series = chart.SeriesCollection.AddSeries();
      series.Add(new double[] { 16, 5, 3, 20, 11 });

      chart.XAxis.TickLabels.Format = "00";
      chart.XAxis.MajorTickMark = TickMarkType.Outside;
      chart.XAxis.Title.Caption = "X-Axis";

      chart.YAxis.MajorTickMark = TickMarkType.Outside;
      chart.YAxis.HasMajorGridlines = true;

      chart.PlotArea.LineFormat.Color = XColors.DarkGray;
      chart.PlotArea.LineFormat.Width = 1;
      chart.PlotArea.LineFormat.Visible = true;

      chart.Legend.Docking = DockingType.Top;

      return chart;
    }

    public static Chart PieChart()
    {
      Chart chart = new Chart(ChartType.Pie2D);
      Series series = chart.SeriesCollection.AddSeries();
      series.Add(new double[] { 1, 5, 11, -3, 20 });

      XSeries xseries = chart.XValues.AddXSeries();
      xseries.Add("Production", "Lab", "Licenses", "Taxes", "Insurances");
      chart.Legend.Docking = DockingType.Right;

      chart.DataLabel.Type = DataLabelType.Percent;
      chart.DataLabel.Position = DataLabelPosition.OutsideEnd;

      return chart;
    }

    public static Chart PieExplodedChart()
    {
      Chart chart = new Chart(ChartType.PieExploded2D);
      Series series = chart.SeriesCollection.AddSeries();
      series.Add(new double[] { 1, 17, 45, 5, 3, 20, 11, 23, 8, 19, 34, 56, 23, 45 });

      chart.Legend.Docking = DockingType.Left;
      chart.DataLabel.Type = DataLabelType.Percent;
      chart.DataLabel.Position = DataLabelPosition.Center;

      return chart;
    }

    public static Chart CombinationChart()
    {
      Chart chart = new Chart();
      Series series = chart.SeriesCollection.AddSeries();
      series.ChartType = ChartType.Column2D;
      series.Add(new double[] { 1, 17, 45, 5, 3, 20, 11, 23, 8, 19 });
      series.HasDataLabel = true;

      series = chart.SeriesCollection.AddSeries();
      series.ChartType = ChartType.Line;
      series.Add(new double[] { 41, 7, 5, 45, 13, 10, 21, 13, 18, 9 });

      XSeries xseries = chart.XValues.AddXSeries();
      xseries.Add("A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N");

      chart.XAxis.MajorTickMark = TickMarkType.Outside;
      chart.XAxis.Title.Caption = "X-Axis";

      chart.YAxis.MajorTickMark = TickMarkType.Outside;
      chart.YAxis.HasMajorGridlines = true;

      chart.PlotArea.LineFormat.Color = XColors.DarkGray;
      chart.PlotArea.LineFormat.Width = 1;
      chart.PlotArea.LineFormat.Visible = true;

      chart.Legend.Docking = DockingType.Left;
      chart.Legend.LineFormat.Visible = true;

      return chart;
    }
  }
}
