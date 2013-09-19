#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   David Stephensen (mailto:David.Stephensen@pdfsharp.com)
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
using System.ComponentModel;
using PdfSharp.Drawing;

namespace PdfSharp.Drawing.BarCodes
{
  /// <summary>
  /// Represents the base class of all 2D codes.
  /// </summary>
  public abstract class MatrixCode : CodeBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MatrixCode"/> class.
    /// </summary>
    public MatrixCode(string text, string encoding, int rows, int columns, XSize size)
      : base(text, size, CodeDirection.LeftToRight)
    {
      this.encoding = encoding;
      if (String.IsNullOrEmpty(this.encoding))
        this.encoding = new String('a', this.text.Length);

      if (columns < rows)
      {
        this.rows = columns;
        this.columns = rows;
      }
      else
      {
        this.columns = columns;
        this.rows = rows;
      }

      this.Text = text;
    }

    /// <summary>
    /// Gets or sets the encoding. docDaSt
    /// </summary>
    public string Encoding
    {
      get { return this.encoding; }
      set
      {
        this.encoding = value;
        this.matrixImage = null;
      }
    }
    internal string encoding;

    /// <summary>
    /// docDaSt
    /// </summary>
    public int Columns
    {
      get { return this.columns; }
      set
      {
        this.columns = value;
        this.matrixImage = null;
      }
    }
    internal int columns;

    /// <summary>
    /// docDaSt
    /// </summary>
    public int Rows
    {
      get { return this.rows; }
      set
      {
        this.rows = value;
        this.matrixImage = null;
      }
    }
    internal int rows;

    /// <summary>
    /// docDaSt
    /// </summary>
    public new string Text
    {
      get { return base.Text; }
      set
      {
        base.Text = value;
        this.matrixImage = null;
      }
    }

    internal XImage matrixImage;

    /// <summary>
    /// When implemented in a derived class renders the 2D code.
    /// </summary>
    protected internal abstract void Render(XGraphics gfx, XBrush brush, XPoint center);

    /// <summary>
    /// Determines whether the specified string can be used as Text for this matrix code type.
    /// </summary>
    protected override void CheckCode(string text)
    {
    }
  }
}
