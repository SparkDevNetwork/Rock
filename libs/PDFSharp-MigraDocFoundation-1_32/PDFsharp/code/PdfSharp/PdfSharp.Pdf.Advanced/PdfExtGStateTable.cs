#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Stefan Lange (mailto:Stefan.Lange@pdfsharp.com)
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

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Internal;

namespace PdfSharp.Pdf.Advanced
{
  /// <summary>
  /// Contains all used ExtGState objects of a document.
  /// </summary>
  internal sealed class PdfExtGStateTable : PdfResourceTable
  {
    /// <summary>
    /// Initializes a new instance of this class, which is a singleton for each document.
    /// </summary>
    public PdfExtGStateTable(PdfDocument document)
      : base(document)
    { }

    /// <summary>
    /// Gets a PdfExtGState with the keys 'CA' and 'ca' set to the specified alpha value.
    /// </summary>
    public PdfExtGState GetExtGState(double alpha)
    {
      string key = MakeKey(alpha);
      PdfExtGState extGState;
      if (!this.alphaValues.TryGetValue(key, out extGState))
      {
        extGState = new PdfExtGState(this.owner);
        extGState.Elements[PdfExtGState.Keys.CA] = new PdfReal(alpha);
        extGState.Elements[PdfExtGState.Keys.ca] = new PdfReal(alpha);

        this.alphaValues[key] = extGState;
      }
      return extGState;
    }

    /// <summary>
    /// Gets a PdfExtGState with the key 'CA' set to the specified alpha value.
    /// </summary>
    public PdfExtGState GetExtGStateStroke(double alpha)
    {
      string key = MakeKey(alpha);
      PdfExtGState extGState;
      if (!this.strokeAlphaValues.TryGetValue(key, out extGState))
      {
        extGState = new PdfExtGState(this.owner);
        extGState.Elements[PdfExtGState.Keys.CA] = new PdfReal(alpha);

        this.strokeAlphaValues[key] = extGState;
      }
      return extGState;
    }

    /// <summary>
    /// Gets a PdfExtGState with the key 'ca' set to the specified alpha value.
    /// </summary>
    public PdfExtGState GetExtGStateNonStroke(double alpha)
    {
      string key = MakeKey(alpha);
      PdfExtGState extGState; ;
      if (!this.nonStrokeAlphaValues.TryGetValue(key, out extGState))
      {
        extGState = new PdfExtGState(this.owner);
        extGState.Elements[PdfExtGState.Keys.ca] = new PdfReal(alpha);

        this.nonStrokeAlphaValues[key] = extGState;
      }
      return extGState;
    }

    ///// <summary>
    ///// Gets a PdfExtGState with the key 'ca' set to the specified alpha value.
    ///// </summary>
    //public PdfExtGState GetExtGState(XColor strokeColor, XColor nonStrokeColor)
    //{
    //  if (strokeColor.IsEmpty)
    //  {
    //  }
    //  else if (nonStrokeColor.IsEmpty)
    //  {
    //  }
    //  else
    //  {
    //  }

    //  return null;
    //  //string key = MakeKey(alpha);
    //  //PdfExtGState extGState = this.nonStrokeAlphaValues[key] as PdfExtGState;
    //  //if (extGState == null)
    //  //{
    //  //  extGState = new PdfExtGState(this.document);
    //  //  extGState.Elements[PdfExtGState.Keys.ca] = new PdfReal(alpha);
    //  //
    //  //  this.nonStrokeAlphaValues[key] = extGState;
    //  //}
    //  //return extGState;
    //}

    static string MakeKey(double alpha)
    {
      return ((int)(1000 * alpha)).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Maps from alpha values (range "0" to "1000") to PdfExtGState objects.
    /// </summary>
    Dictionary<string, PdfExtGState> alphaValues = new Dictionary<string, PdfExtGState>();
    Dictionary<string, PdfExtGState> strokeAlphaValues = new Dictionary<string, PdfExtGState>();
    Dictionary<string, PdfExtGState> nonStrokeAlphaValues = new Dictionary<string, PdfExtGState>();
  }
}