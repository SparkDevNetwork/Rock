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
using System.Collections;
using System.Reflection;
using System.Text;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Content.Objects;

namespace PdfSharp.Pdf.Content
{
  /// <summary>
  /// Provides the functionality to parse PDF content streams.
  /// </summary>
  internal sealed class CParser
  {
    PdfPage page;
    CLexer lexer;

    public CParser(PdfPage page)
    {
      this.page = page;
      PdfContent content = page.Contents.CreateSingleContent();
      byte[] bytes = content.Stream.Value;
      this.lexer = new CLexer(bytes);
    }

    public CParser(byte[] content)
    {
      this.lexer = new CLexer(content);
    }

    public CSymbol Symbol
    {
      get { return this.lexer.Symbol; }
    }

    public CSequence ReadContent()
    {
      CSequence sequence = new CSequence();
      ParseObject(sequence, CSymbol.Eof);

      return sequence;
    }

    /// <summary>
    /// Parses whatever comes until the specified stop symbol is reached.
    /// </summary>
    void ParseObject(CSequence sequence, CSymbol stop)
    {
      CSymbol symbol;
      while ((symbol = ScanNextToken()) != CSymbol.Eof)
      {
        if (symbol == stop)
          return;

        switch (symbol)
        {
          case CSymbol.Comment:
            // ignore comments
            break;

          case CSymbol.Integer:
            CInteger n = new CInteger();
            n.Value = this.lexer.TokenToInteger;
            this.operands.Add(n);
            break;

          case CSymbol.Real:
            CReal r = new CReal();
            r.Value = this.lexer.TokenToReal;
            this.operands.Add(r);
            break;

          case CSymbol.String:
          case CSymbol.HexString:
          case CSymbol.UnicodeString:
          case CSymbol.UnicodeHexString:
            CString s = new CString();
            s.Value = this.lexer.Token;
            this.operands.Add(s);
            break;

          case CSymbol.Name:
            CName name = new CName();
            name.Name = this.lexer.Token;
            this.operands.Add(name);
            break;

          case CSymbol.Operator:
            COperator op = CreateOperator();
            this.operands.Clear();
            sequence.Add(op);
            break;

          case CSymbol.BeginArray:
            CArray array = new CArray();
            Debug.Assert(this.operands.Count == 0, "Array within array...");
            ParseObject(array, CSymbol.EndArray);
            array.Add(this.operands);
            this.operands.Clear();
            this.operands.Add((CObject)array);
            break;

          case CSymbol.EndArray:
            throw new ContentReaderException("Unexpected: ']'");
        }
      }
    }

    COperator CreateOperator()
    {
      string name = this.lexer.Token;
      COperator op = OpCodes.OperatorFromName(name);
      if (op.OpCode.OpCodeName== OpCodeName.ID)
      {
        this.lexer.ScanInlineImage();
      }

#if DEBUG
      if (op.OpCode.Operands != -1 && op.OpCode.Operands != this.operands.Count)
      {
        if (op.OpCode.OpCodeName != OpCodeName.ID)
        {
          GetType();
          Debug.Assert(false, "Invalid number of operands.");
        }
      }
#endif
      op.Operands.Add(this.operands);
      return op;
    }

    CSymbol ScanNextToken()
    {
      return this.lexer.ScanNextToken();
    }

    CSymbol ScanNextToken(out string token)
    {
      CSymbol symbol = this.lexer.ScanNextToken();
      token = this.lexer.Token;
      return symbol;
    }

    /// <summary>
    /// Reads the next symbol that must be the specified one.
    /// </summary>
    CSymbol ReadSymbol(CSymbol symbol)
    {
      CSymbol current = this.lexer.ScanNextToken();
      if (symbol != current)
        throw new ContentReaderException(PSSR.UnexpectedToken(this.lexer.Token));
      return current;
    }

    CSequence operands = new CSequence();
  }
}
