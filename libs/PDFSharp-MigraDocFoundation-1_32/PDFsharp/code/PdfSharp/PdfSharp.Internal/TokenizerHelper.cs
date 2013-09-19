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
using System.Text;
using System.Globalization;

namespace PdfSharp.Internal
{
  // Relected from WPF to ensure compatibility
  // Use netmassdownloader -d "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0" -output g:\cachetest -v
  class TokenizerHelper
  {
    internal char PeekNextCharacter()
    {
      if (this.charIndex >= this.strLen)
        return 'X';
      char ch = this.str[this.charIndex];
      return ch;
    }

    private static readonly IFormatProvider NeutralCulture = CultureInfo.InvariantCulture; //.GetCultureInfo("en-us");

    public TokenizerHelper(string str)
      : this(str, NeutralCulture)
    { }

    public TokenizerHelper(string str, IFormatProvider formatProvider)
    {
      char numericListSeparator = GetNumericListSeparator(formatProvider);
      Initialize(str, '\'', numericListSeparator);
    }

    public TokenizerHelper(string str, char quoteChar, char separator)
    {
      Initialize(str, quoteChar, separator);
    }

    internal string GetCurrentToken()
    {
      if (this.currentTokenIndex < 0)
        return null;
      return this.str.Substring(this.currentTokenIndex, this.currentTokenLength);
    }

    internal static char GetNumericListSeparator(IFormatProvider provider)
    {
      char ch = ',';
      NumberFormatInfo instance = NumberFormatInfo.GetInstance(provider);
      if (instance.NumberDecimalSeparator.Length > 0 && ch == instance.NumberDecimalSeparator[0])
        ch = ';';
      return ch;
    }

    void Initialize(string str, char quoteChar, char separator)
    {
      this.str = str;
      this.strLen = (str == null) ? 0 : str.Length;
      this.currentTokenIndex = -1;
      this.quoteChar = quoteChar;
      this.argSeparator = separator;
      while (this.charIndex < this.strLen)
      {
        if (!Char.IsWhiteSpace(this.str, this.charIndex))
          return;
        this.charIndex++;
      }
    }

    internal void LastTokenRequired()
    {
      if (this.charIndex != this.strLen)
        throw new InvalidOperationException("Extra data encountered"); //SR.Get(SRID.TokenizerHelperExtraDataEncountered, new object[0]));
    }

    internal bool NextToken()
    {
      return NextToken(false);
    }

    internal bool NextToken(bool allowQuotedToken)
    {
      return NextToken(allowQuotedToken, this.argSeparator);
    }

    internal bool NextToken(bool allowQuotedToken, char separator)
    {
      this.currentTokenIndex = -1;
      this.foundSeparator = false;
      if (this.charIndex >= this.strLen)
        return false;

      char c = this.str[this.charIndex];
      int charCount = 0;
      if (allowQuotedToken && c == this.quoteChar)
      {
        charCount++;
        this.charIndex++;
      }
      int index = this.charIndex;
      int num3 = 0;
      while (this.charIndex < this.strLen)
      {
        c = this.str[this.charIndex];
        if (charCount > 0)
        {
          if (c != this.quoteChar)
            goto Label_00AA;

          charCount--;
          if (charCount != 0)
            goto Label_00AA;

          this.charIndex++;
          break;
        }
        if (char.IsWhiteSpace(c) || c == separator)
        {
          if (c == separator)
            this.foundSeparator = true;
          break;
        }
      Label_00AA:
        this.charIndex++;
        num3++;
      }
      if (charCount > 0)
        throw new InvalidOperationException("Missing end quote"); //SR.Get(SRID.TokenizerHelperMissingEndQuote, new object[0]));
      ScanToNextToken(separator);
      this.currentTokenIndex = index;
      this.currentTokenLength = num3;
      if (this.currentTokenLength < 1)
        throw new InvalidOperationException("Empty token"); // SR.Get(SRID.TokenizerHelperEmptyToken, new object[0]));
#if DEBUG_
      string s = GetCurrentToken();
      if (s == "169.466971230985")
        Debugger.Break();
      //Debug.WriteLine(GetCurrentToken());
#endif
      return true;
    }

    public string NextTokenRequired()
    {
      if (!NextToken(false))
        throw new InvalidOperationException("PrematureStringTermination"); //SR.Get(SRID.TokenizerHelperPrematureStringTermination, new object[0]));
      return GetCurrentToken();
    }

    public string NextTokenRequired(bool allowQuotedToken)
    {
      if (!NextToken(allowQuotedToken))
        throw new InvalidOperationException("PrematureStringTermination");  //SR.Get(SRID.TokenizerHelperPrematureStringTermination, new object[0]));
      return GetCurrentToken();
    }

    private void ScanToNextToken(char separator)
    {
      if (this.charIndex < this.strLen)
      {
        char c = this.str[this.charIndex];
        if (c != separator && !char.IsWhiteSpace(c))
        {
          throw new InvalidOperationException("ExtraDataEncountered"); //SR.Get(SRID.TokenizerHelperExtraDataEncountered, new object[0]));
        }
        int num = 0;
        while (this.charIndex < this.strLen)
        {
          c = this.str[this.charIndex];
          if (c == separator)
          {
            this.foundSeparator = true;
            num++;
            this.charIndex++;
            if (num > 1)
              throw new InvalidOperationException("EmptyToken"); //SR.Get(SRID.TokenizerHelperEmptyToken, new object[0]));
          }
          else
          {
            if (!char.IsWhiteSpace(c))
              break;
            this.charIndex++;
          }
        }
        if (num > 0 && this.charIndex >= this.strLen)
          throw new InvalidOperationException("EmptyToken"); // SR.Get(SRID.TokenizerHelperEmptyToken, new object[0]));
      }
    }

    internal bool FoundSeparator
    {
      get { return this.foundSeparator; }
    }
    private bool foundSeparator;

    private char argSeparator;
    private int charIndex;
    internal int currentTokenIndex;
    internal int currentTokenLength;
    private char quoteChar;
    private string str;
    private int strLen;
  }
}