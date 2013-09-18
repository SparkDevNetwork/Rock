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

namespace HelloMigraDoc
{
  /// <summary>
  /// Defines the styles used in the document.
  /// </summary>
  public class Styles
  {
    /// <summary>
    /// Defines the styles used in the document.
    /// </summary>
    public static void DefineStyles(Document document)
    {
      // Get the predefined style Normal.
      Style style = document.Styles["Normal"];
      // Because all styles are derived from Normal, the next line changes the 
      // font of the whole document. Or, more exactly, it changes the font of
      // all styles and paragraphs that do not redefine the font.
      style.Font.Name = "Times New Roman";

      // Heading1 to Heading9 are predefined styles with an outline level. An outline level
      // other than OutlineLevel.BodyText automatically creates the outline (or bookmarks) 
      // in PDF.

      style = document.Styles["Heading1"];
      style.Font.Name = "Tahoma";
      style.Font.Size = 14;
      style.Font.Bold = true;
      style.Font.Color = Colors.DarkBlue;
      style.ParagraphFormat.PageBreakBefore = true;
      style.ParagraphFormat.SpaceAfter = 6;

      style = document.Styles["Heading2"];
      style.Font.Size = 12;
      style.Font.Bold = true;
      style.ParagraphFormat.PageBreakBefore = false;
      style.ParagraphFormat.SpaceBefore = 6;
      style.ParagraphFormat.SpaceAfter = 6;

      style = document.Styles["Heading3"];
      style.Font.Size = 10;
      style.Font.Bold = true;
      style.Font.Italic = true;
      style.ParagraphFormat.SpaceBefore = 6;
      style.ParagraphFormat.SpaceAfter = 3;

      style = document.Styles[StyleNames.Header];
      style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

      style = document.Styles[StyleNames.Footer];
      style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

      // Create a new style called TextBox based on style Normal
      style = document.Styles.AddStyle("TextBox", "Normal");
      style.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
      style.ParagraphFormat.Borders.Width = 2.5;
      style.ParagraphFormat.Borders.Distance = "3pt";
      //TODO: Colors
      style.ParagraphFormat.Shading.Color = Colors.SkyBlue;

      // Create a new style called TOC based on style Normal
      style = document.Styles.AddStyle("TOC", "Normal");
      style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right, TabLeader.Dots);
      style.ParagraphFormat.Font.Color = Colors.Blue;
    }
  }
}
