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
#if GDI
using System.Drawing;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp.Drawing;

namespace PdfSharp.Drawing.BarCodes
{
  /// <summary>
  /// Defines the DataMatrix 2D barcode. THIS IS AN EMPIRA INTERNAL IMPLEMENTATION. THE CODE IN
  /// THE OPEN SOURCE VERSION IS A FAKE.
  /// </summary>
  public class CodeDataMatrix : MatrixCode
  {
    /// <summary>
    /// Initializes a new instance of CodeDataMatrix.
    /// </summary>
    public CodeDataMatrix()
      : this("", "", 26, 26, 0, XSize.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of CodeDataMatrix.
    /// </summary>
    public CodeDataMatrix(string code, int length)
      : this(code, "", length, length, 0, XSize.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of CodeDataMatrix.
    /// </summary>
    public CodeDataMatrix(string code, int length, XSize size)
      : this(code, "", length, length, 0, size)
    {
    }

    /// <summary>
    /// Initializes a new instance of CodeDataMatrix.
    /// </summary>
    public CodeDataMatrix(string code, DataMatrixEncoding dmEncoding, int length, XSize size)
      : this(code, CreateEncoding(dmEncoding, code.Length), length, length, 0, size)
    {
    }

    /// <summary>
    /// Initializes a new instance of CodeDataMatrix.
    /// </summary>
    public CodeDataMatrix(string code, int rows, int columns)
      : this(code, "", rows, columns, 0, XSize.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of CodeDataMatrix.
    /// </summary>
    public CodeDataMatrix(string code, int rows, int columns, XSize size)
      : this(code, "", rows, columns, 0, size)
    {
    }

    /// <summary>
    /// Initializes a new instance of CodeDataMatrix.
    /// </summary>
    public CodeDataMatrix(string code, DataMatrixEncoding dmEncoding, int rows, int columns, XSize size)
      : this(code, CreateEncoding(dmEncoding, code.Length), rows, columns, 0, size)
    {
    }

    /// <summary>
    /// Initializes a new instance of CodeDataMatrix.
    /// </summary>
    public CodeDataMatrix(string code, int rows, int columns, int quietZone)
      : this(code, "", rows, columns, quietZone, XSize.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of CodeDataMatrix.
    /// </summary>
    public CodeDataMatrix(string code, string encoding, int rows, int columns, int quietZone, XSize size)
      : base(code, encoding, rows, columns, size)
    {
      QuietZone = quietZone;
    }

    /// <summary>
    /// Sets the encoding of the DataMatrix.
    /// </summary>
    public void SetEncoding(DataMatrixEncoding dmEncoding)
    {
      Encoding = CreateEncoding(dmEncoding, Text.Length);
    }

    static string CreateEncoding(DataMatrixEncoding dmEncoding, int length)
    {
      string tempencoding = "";
      switch (dmEncoding)
      {
        case DataMatrixEncoding.Ascii:
          tempencoding = new String('a', length);
          break;
        case DataMatrixEncoding.C40:
          tempencoding = new String('c', length);
          break;
        case DataMatrixEncoding.Text:
          tempencoding = new String('t', length);
          break;
        case DataMatrixEncoding.X12:
          tempencoding = new String('x', length);
          break;
        case DataMatrixEncoding.EDIFACT:
          tempencoding = new String('e', length);
          break;
        case DataMatrixEncoding.Base256:
          tempencoding = new String('b', length);
          break;
      }
      return tempencoding;
    }

    /// <summary>
    /// Gets or sets the size of the Matrix' Quiet Zone.
    /// </summary>
    public int QuietZone
    {
      get { return this.quietZone; }
      set { this.quietZone = value; }
    }
    int quietZone;

    /// <summary>
    /// Renders the matrix code.
    /// </summary>
    protected internal override void Render(XGraphics gfx, XBrush brush, XPoint position)
    {
      XGraphicsState state = gfx.Save();

      switch (this.direction)
      {
        case CodeDirection.RightToLeft:
          gfx.RotateAtTransform(180, position);
          break;

        case CodeDirection.TopToBottom:
          gfx.RotateAtTransform(90, position);
          break;

        case CodeDirection.BottomToTop:
          gfx.RotateAtTransform(-90, position);
          break;
      }

      XPoint pos = position + CodeBase.CalcDistance(this.anchor, AnchorType.TopLeft, this.size);

      if (this.matrixImage == null)
        this.matrixImage = DataMatrixImage.GenerateMatrixImage(Text, Encoding, Rows, Columns);

      if (QuietZone > 0)
      {
        XSize sizeWithZone = new XSize(this.size.width, this.size.height);
        sizeWithZone.width = sizeWithZone.width / (Columns + 2 * QuietZone) * Columns;
        sizeWithZone.height = sizeWithZone.height / (Rows + 2 * QuietZone) * Rows;

        XPoint posWithZone = new XPoint(pos.X, pos.Y);
        posWithZone.X += size.width / (Columns + 2 * QuietZone) * QuietZone;
        posWithZone.Y += size.height / (Rows + 2 * QuietZone) * QuietZone;

        gfx.DrawRectangle(XBrushes.White, pos.x, pos.y, size.width, size.height);
        gfx.DrawImage(matrixImage, posWithZone.x, posWithZone.y, sizeWithZone.width, sizeWithZone.height);
      }
      else
        gfx.DrawImage(matrixImage, pos.x, pos.y, this.size.width, this.size.height);

      gfx.Restore(state);
    }

    /// <summary>
    /// Determines whether the specified string can be used as data in the DataMatrix.
    /// </summary>
    /// <param name="text">The code to be checked.</param>
    protected override void CheckCode(string text)
    {
      if (text == null)
        throw new ArgumentNullException("text");

      DataMatrixImage mImage = new DataMatrixImage(Text, Encoding, Rows, Columns);
      mImage.Iec16022Ecc200(Columns, Rows, Encoding, Text.Length, Text, 0, 0, 0);
    }
  }
}
