#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   Klaus Potzesny (mailto:Klaus.Potzesny@pdfsharp.com)
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
using System.Globalization;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.RtfRendering.Resources;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Renders a date field to RTF.
  /// </summary>
  internal class DateFieldRenderer : FieldRenderer
  {
    internal DateFieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.dateField = domObj as DateField;
    }


    /// <summary>
    /// Renders a date field to RTF.
    /// </summary>
    internal override void Render()
    {
      StartField();
      this.rtfWriter.WriteText("DATE ");
      TranslateFormat();
      EndField();
    }


    /// <summary>
    /// Translates the date field format to RTF.
    /// </summary>
    private void TranslateFormat()
    {
      string domFrmt = this.dateField.Format;
      string rtfFrmt = domFrmt;

      //The format is translated using the current culture.
      DateTimeFormatInfo dtfInfo = CultureInfo.CurrentCulture.DateTimeFormat;
      if (domFrmt == "")
        rtfFrmt = dtfInfo.ShortDatePattern + " " + dtfInfo.LongTimePattern;

      else if (domFrmt.Length == 1)
      {
        switch (domFrmt)
        {
          case "d":
            rtfFrmt = dtfInfo.ShortDatePattern;
            break;

          case "D":
            rtfFrmt = dtfInfo.LongDatePattern;
            break;

          case "T":
            rtfFrmt = dtfInfo.LongTimePattern;
            break;

          case "t":
            rtfFrmt = dtfInfo.ShortTimePattern;
            break;

          case "f":
            rtfFrmt = dtfInfo.LongDatePattern + " " + dtfInfo.ShortTimePattern;
            break;

          case "F":
            rtfFrmt = dtfInfo.FullDateTimePattern;
            break;

          case "G":
            rtfFrmt = dtfInfo.ShortDatePattern + " " + dtfInfo.LongTimePattern;
            break;

          case "g":
            rtfFrmt = dtfInfo.ShortDatePattern + " " + dtfInfo.ShortTimePattern;
            break;

          case "M":
          case "m":
            rtfFrmt = dtfInfo.MonthDayPattern;
            break;

          case "R":
          case "r":
            rtfFrmt = dtfInfo.RFC1123Pattern;
            break;

          case "s":
            rtfFrmt = dtfInfo.SortableDateTimePattern;
            break;

          //TODO: Output universal time for u und U.
          case "u":
            rtfFrmt = dtfInfo.UniversalSortableDateTimePattern;
            break;

          case "U":
            rtfFrmt = dtfInfo.FullDateTimePattern;
            break;

          case "Y":
          case "y":
            rtfFrmt = dtfInfo.YearMonthPattern;
            break;

          default:
            break;
        }
      }
      bool isEscaped = false;
      bool isQuoted = false;
      bool isSingleQuoted = false;
      string rtfFrmt2 = "\\@ \"";
      foreach (char c in rtfFrmt)
      {
        switch (c)
        {
          case '\\':
            if (isEscaped)
              rtfFrmt2 += "'" + '\\' + "'";

            isEscaped = !isEscaped;
            break;

          case '\'':
            if (isEscaped)
            {
              //Doesn't work in word format strings.
              Trace.WriteLine(Messages.CharacterNotAllowedInDateFormat(c), "warning");
              isEscaped = false;
            }
            else if (!isSingleQuoted && !isQuoted)
            {
              isSingleQuoted = true;
              rtfFrmt2 += c;
            }
            else if (isQuoted)
            {
              rtfFrmt2 += @"\'";
            }
            else if (isSingleQuoted)
            {
              isSingleQuoted = false;
              rtfFrmt2 += c;
            }
            break;

          case '"':
            if (isEscaped)
            {
              rtfFrmt2 += c;
              isEscaped = false;
            }
            else if (!isQuoted && !isSingleQuoted)
            {
              isQuoted = true;
              rtfFrmt2 += '\'';
            }
            else if (isQuoted)
            {
              isQuoted = false;
              rtfFrmt2 += '\'';
            }
            else if (isSingleQuoted)
            {
              rtfFrmt2 += "\\\"";
            }
            break;

          case '/':
            if (isEscaped || isQuoted || isSingleQuoted)
            {
              isEscaped = false;
              rtfFrmt2 += c;
            }
            else
            {
              rtfFrmt2 += dtfInfo.DateSeparator;
            }
            break;

          case ':':
            if (isEscaped || isQuoted || isSingleQuoted)
            {
              isEscaped = false;
              rtfFrmt2 += c;
            }
            else
            {
              rtfFrmt2 += dtfInfo.TimeSeparator;
            }
            break;

          default:
            if (isEscaped)
              rtfFrmt2 += "'" + c + "'";
            else if (!isQuoted && !isSingleQuoted)
              rtfFrmt2 += TranslateCustomFormatChar(c);
            else
              rtfFrmt2 += c;

            isEscaped = false;
            break;
        }
      }
      this.rtfWriter.WriteText(rtfFrmt2 + @""" \* MERGEFORMAT");
    }


    /// <summary>
    /// Translates an unescaped character of a DateField's custom format to RTF.
    /// </summary>
    string TranslateCustomFormatChar(char ch)
    {
      switch (ch)
      {
        case 'y':
        case 'M':
        case 'd':
        case 'H':
        case 'h':
        case 'm':
        case 's':
          return ch.ToString();

        default:
          return "'" + ch + "'";
      }
    }

    /// <summary>
    /// Gets the current date in the correct format.
    /// </summary>
    protected override string GetFieldResult()
    {
      return DateTime.Now.ToString(this.dateField.Format);
    }
    DateField dateField;
  }
}
