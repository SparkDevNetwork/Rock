#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   David Stephensen (mailto:David.Stephensen@pdfsharp.com)
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
using PdfSharp.Charting;

namespace MigraDoc.Rendering.ChartMapper
{
  /// <summary>
  /// The XValuesMapper class.
  /// </summary>
  public class XValuesMapper
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="XValuesMapper"/> class.
    /// </summary>
    public XValuesMapper()
    {
    }

    void MapObject(XValues xValues, MigraDoc.DocumentObjectModel.Shapes.Charts.XValues domXValues)
    {
      foreach (MigraDoc.DocumentObjectModel.Shapes.Charts.XSeries domXSeries in domXValues)
      {
        XSeries xSeries = xValues.AddXSeries();
        MigraDoc.DocumentObjectModel.Shapes.Charts.XSeriesElements domXSeriesElements = domXSeries.GetValue("XSeriesElements") as MigraDoc.DocumentObjectModel.Shapes.Charts.XSeriesElements;
        foreach (MigraDoc.DocumentObjectModel.Shapes.Charts.XValue domXValue in domXSeriesElements)
        {
          if (domXValue == null)
            xSeries.AddBlank();
          else
            xSeries.Add(domXValue.GetValue("Value").ToString());
        }
      }
    }

    internal static void Map(XValues xValues, MigraDoc.DocumentObjectModel.Shapes.Charts.XValues domXValues)
    {
      XValuesMapper mapper = new XValuesMapper();
      mapper.MapObject(xValues, domXValues);
    }
  }
}
