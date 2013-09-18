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
using System.Text;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Fields;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Class to render a Bookmark to RTF.
  /// </summary>
  internal class BookmarkFieldRenderer : RendererBase
  {
    public BookmarkFieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.bookmark = domObj as BookmarkField;
    }
    /// <summary>
    /// Renders a Bookmark.
    /// </summary>
    internal override void Render()
    {
      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("bkmkstart", true);
      string name = MakeValidBookmarkName(this.bookmark.Name);
      this.rtfWriter.WriteText(name);
      rtfWriter.EndContent();

      this.rtfWriter.StartContent();
      this.rtfWriter.WriteControl("bkmkend", true);
      this.rtfWriter.WriteText(name);
      rtfWriter.EndContent();
    }

    /// <summary>
    /// Gets a valid bookmark name for RTF  by the given original name.
    /// </summary>
    internal static string MakeValidBookmarkName(string originalName)
    {
      //Bookmarks (at least in Word) have the following limitations:
      //1. First character must be a letter (umlauts und ß are allowed)
      //2. All further characters must be letters, numbers or underscores. 
      //   For example, '-' is NOT allowed).
      StringBuilder strBuilder = new StringBuilder(originalName.Length);
      if (!Char.IsLetter(originalName[0]))
        strBuilder.Append("BM__");

      for (int idx = 0; idx < originalName.Length; ++idx)
      {
        char ch = originalName[idx];

        if (char.IsLetterOrDigit(ch))
          strBuilder.Append(ch);
        else
          strBuilder.Append('_');
      }
      return strBuilder.ToString();
    }

    BookmarkField bookmark;
  }
}
