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

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// Renders a special character to RTF.
  /// </summary>
  internal class CharacterRenderer : RendererBase
  {
    /// <summary>
    /// Creates a new instance of the CharacterRenderer class.
    /// </summary>
    internal CharacterRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.character = domObj as Character;
    }

    /// <summary>
    /// Renders a character to rtf.
    /// </summary>
    internal override void Render()
    {
      if (this.character.Char != '\0')
      {
        this.rtfWriter.WriteHex((uint)this.character.Char);
      }
      else
      {
        int count = this.character.IsNull("Count") ? 1 : this.character.Count;
        switch (this.character.SymbolName)
        {
          case SymbolName.Blank:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteBlank();
            //WriteText wouldn't work if there was a control before.
            break;

          case SymbolName.Bullet:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteControl("bullet");
            break;

          case SymbolName.Copyright:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteHex(0xa9);
            break;

          case SymbolName.Em:
            for (int i = 0; i < count; ++i)
            {
              this.rtfWriter.WriteControl("u", "8195");
              //I don't know why, but it works:
              this.rtfWriter.WriteHex(0x20);
            }
            break;

          case SymbolName.Em4:
            for (int i = 0; i < count; ++i)
            {
              this.rtfWriter.WriteControl("u", "8197");
              //I don't know why, but it works:
              this.rtfWriter.WriteHex(0x20);
            }
            break;

          case SymbolName.En:
            for (int i = 0; i < count; ++i)
            {
              this.rtfWriter.WriteControl("u", "8194");
              //I don't know why, but it works:
              this.rtfWriter.WriteHex(0x20);
            }
            break;

          case SymbolName.EmDash:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteControl("emdash");
            break;

          case SymbolName.EnDash:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteControl("endash");
            break;

          case SymbolName.Euro:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteHex(0x80);
            break;

          case SymbolName.NonBreakableBlank:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteControl("~");
            break;

          case SymbolName.LineBreak:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteControl("line");
            break;

          case SymbolName.Not:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteHex(0xac);
            break;

          case SymbolName.ParaBreak:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteControl("par");
            break;

          case SymbolName.RegisteredTrademark:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteHex(0xae);
            break;

          case SymbolName.Tab:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteControl("tab");
            break;

          case SymbolName.Trademark:
            for (int i = 0; i < count; ++i)
              this.rtfWriter.WriteHex(0x99);
            break;
        }
      }
    }
    Character character;
  }
}
