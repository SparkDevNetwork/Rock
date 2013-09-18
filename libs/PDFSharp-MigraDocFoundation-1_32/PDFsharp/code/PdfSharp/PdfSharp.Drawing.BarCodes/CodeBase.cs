#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Klaus Potzesny (mailto:Klaus.Potzesny@pdfsharp.com)
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
  /// Represents the base class of all codes.
  /// </summary>
  public abstract class CodeBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CodeBase"/> class.
    /// </summary>
    public CodeBase(string text, XSize size, CodeDirection direction)
    {
      this.text = text;
      this.size = size;
      this.direction = direction;
    }

    //public static CodeBase FromType(CodeType type, string text, XSize size, CodeDirection direction)
    //{
    //  switch (type)
    //  {
    //    case CodeType.Code2of5Interleaved:
    //      return new Code2of5Interleaved(text, size, direction);

    //    case CodeType.Code3of9Standard:
    //      return new Code3of9Standard(text, size, direction);

    //    default:
    //      throw new InvalidEnumArgumentException("type", (int)type, typeof(CodeType));
    //  }
    //}

    //public static CodeBase FromType(CodeType type, string text, XSize size)
    //{
    //  return FromType(type, text, size, CodeDirection.LeftToRight);
    //}

    //public static CodeBase FromType(CodeType type, string text)
    //{
    //  return FromType(type, text, XSize.Empty, CodeDirection.LeftToRight);
    //}

    //public static CodeBase FromType(CodeType type)
    //{
    //  return FromType(type, String.Empty, XSize.Empty, CodeDirection.LeftToRight);
    //}

    /// <summary>
    /// Gets or sets the size.
    /// </summary>
    public XSize Size
    {
      get { return this.size; }
      set { this.size = value; }
    }
    internal XSize size;

    /// <summary>
    /// Gets or sets the text the bar code shall represent.
    /// </summary>
    public string Text
    {
      get { return this.text; }
      set
      {
        CheckCode(value);
        this.text = value;
      }
    }
    internal string text;

    /// <summary>
    /// Always MiddleCenter.
    /// </summary>
    public AnchorType Anchor
    {
      get { return this.anchor; }
      set { this.anchor = value; }
    }
    internal AnchorType anchor;

    /// <summary>
    /// Gets or sets the drawing direction.
    /// </summary>
    public CodeDirection Direction
    {
      get { return this.direction; }
      set { this.direction = value; }
    }
    internal CodeDirection direction;

    /// <summary>
    /// When implemented in a derived class, determines whether the specified string can be used as Text
    /// for this bar code type.
    /// </summary>
    /// <param name="text">The code string to check.</param>
    /// <returns>True if the text can be used for the actual barcode.</returns>
    protected abstract void CheckCode(string text);

    /// <summary>
    /// Calculates the distance between an old anchor point and a new anchor point.
    /// </summary>
    /// <param name="oldType"></param>
    /// <param name="newType"></param>
    /// <param name="size"></param>
    public static XVector CalcDistance(AnchorType oldType, AnchorType newType, XSize size)
    {
      if (oldType == newType)
        return new XVector();

      XVector result;
      Delta delta = CodeBase.deltas[(int)oldType, (int)newType];
      result = new XVector(size.width / 2 * delta.x, size.height / 2 * delta.y);
      return result;
    }

    struct Delta
    {
      public Delta(int x, int y)
      {
        this.x = x;
        this.y = y;
      }
      public int x;
      public int y;
    }
    static Delta[,] deltas = new Delta[9, 9]
    {
      {new Delta(0, 0),   new Delta(1, 0),   new Delta(2, 0),  new Delta(0, 1),   new Delta(1, 1),   new Delta(2, 1),  new Delta(0, 2),  new Delta(1, 2),  new Delta(2, 2)},
      {new Delta(-1, 0),  new Delta(0, 0),   new Delta(1, 0),  new Delta(-1, 1),  new Delta(0, 1),   new Delta(1, 1),  new Delta(-1, 2), new Delta(0, 2),  new Delta(1, 2)},
      {new Delta(-2, 0),  new Delta(-1, 0),  new Delta(0, 0),  new Delta(-2, 1),  new Delta(-1, 1),  new Delta(0, 1),  new Delta(-2, 2), new Delta(-1, 2), new Delta(0, 2)},
      {new Delta(0, -1),  new Delta(1, -1),  new Delta(2, -1), new Delta(0, 0),   new Delta(1, 0),   new Delta(2, 0),  new Delta(0, 1),  new Delta(1, 1),  new Delta(2, 1)},
      {new Delta(-1, -1), new Delta(0, -1),  new Delta(1, -1), new Delta(-1, 0),  new Delta(0, 0),   new Delta(1, 0),  new Delta(-1, 1), new Delta(0, 1),  new Delta(1, 1)},
      {new Delta(-2, -1), new Delta(-1, -1), new Delta(0, -1), new Delta(-2, 0),  new Delta(-1, 0),  new Delta(0, 0),  new Delta(-2, 1), new Delta(-1, 1), new Delta(0, 1)},
      {new Delta(0, -2),  new Delta(1, -2),  new Delta(2, -2), new Delta(0, -1),  new Delta(1, -1),  new Delta(2, -1), new Delta(0, 0),  new Delta(1, 0),  new Delta(2, 0)},
      {new Delta(-1, -2), new Delta(0, -2),  new Delta(1, -2), new Delta(-1, -1), new Delta(0, -1),  new Delta(1, -1), new Delta(-1, 0), new Delta(0, 0),  new Delta(1, 0)},
      {new Delta(-2, -2), new Delta(-1, -2), new Delta(0, -2), new Delta(-2, -1), new Delta(-1, -1), new Delta(0, -1), new Delta(-2, 0), new Delta(-1, 0), new Delta(0, 0)},
    };
  }
}