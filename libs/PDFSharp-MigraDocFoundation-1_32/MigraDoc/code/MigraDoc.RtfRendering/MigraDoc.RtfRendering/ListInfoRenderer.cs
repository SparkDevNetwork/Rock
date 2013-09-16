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
using System.Collections;
using System.Globalization;
using MigraDoc.DocumentObjectModel;
using System.Collections.Generic;

namespace MigraDoc.RtfRendering
{
  /// <summary>
  /// ListInfoRenderer.
  /// </summary>
  internal class ListInfoRenderer : RendererBase
  {
    public ListInfoRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
      : base(domObj, docRenderer)
    {
      this.listInfo = domObj as ListInfo;
    }

    public static void Clear()
    {
        idList.Clear();
        listID = 1;
        templateID = 2;
    }
    /// <summary>
    /// Renders a ListIfo to RTF.
    /// </summary>
    internal override void Render()
    {
      if (prevListInfoID.Key != null && listInfo.ContinuePreviousList)
      {
          idList.Add(this.listInfo, prevListInfoID.Value);
          return;
      }
      idList.Add(listInfo, listID);
      
      rtfWriter.StartContent();
      rtfWriter.WriteControl("list");
     // rtfWriter.WriteControl("listtemplateid", templateID.ToString());
      rtfWriter.WriteControl("listsimple", 1);
      WriteListLevel();
      rtfWriter.WriteControl("listrestarthdn", 0);
      rtfWriter.WriteControl("listid", listID.ToString(CultureInfo.InvariantCulture));
      this.rtfWriter.EndContent();

      prevListInfoID = new KeyValuePair<ListInfo, int>(listInfo, listID);
      listID += 2;
      templateID += 2;
    }
    private static KeyValuePair<ListInfo, int> prevListInfoID = new KeyValuePair<ListInfo,int>();
    private ListInfo listInfo;
    private static int listID = 1;
    private static int templateID = 2;
    private static Hashtable idList = new Hashtable();

    /// <summary>
    /// Gets the corresponding List ID of the ListInfo Object.
    /// </summary>
    internal static int GetListID(ListInfo li)
    {
        if (idList.ContainsKey(li))
            return (int)idList[li];
        
        return -1;
    }

    private void WriteListLevel()
    {
        ListType listType = this.listInfo.ListType;
        string levelText1 = "";
        string levelText2 = "";
        string levelNumbers = "";
        int fontIdx = -1;
        switch(listType)
        {
            case ListType.NumberList1:
                levelText1 = "'02";
                levelText2 = "'00.";
                levelNumbers = "'01";
                break;

            case ListType.NumberList2:
            case ListType.NumberList3:
                levelText1 = "'02";
                levelText2 = "'00)";
                levelNumbers = "'01";
                break;

                //levelText1 = "'02";
                //levelText2 = "'00)";
                //levelNumbers = "'01";
                //break;
            
            case ListType.BulletList1:
                levelText1 = "'01";
                levelText2 = "u-3913 ?";
                fontIdx = this.docRenderer.GetFontIndex("Symbol");
                break;

            case ListType.BulletList2:
                levelText1 = "'01o";
                levelText2 = "";
                fontIdx = this.docRenderer.GetFontIndex("Courier New");
                break;

            case ListType.BulletList3:
                levelText1 = "'01";
                levelText2 = "u-3929 ?";
                fontIdx = this.docRenderer.GetFontIndex("Wingdings");
                break;
        }
        WriteListLevel(levelText1, levelText2, levelNumbers, fontIdx);
    }

    private void WriteListLevel(string levelText1, string levelText2, string levelNumbers, int fontIdx)
    {
        rtfWriter.StartContent();
        rtfWriter.WriteControl("listlevel");
        // Start
        Translate("ListType", "levelnfcn", RtfUnit.Undefined, "4", false);
        Translate("ListType", "levelnfc", RtfUnit.Undefined, "4", false);
        rtfWriter.WriteControl("leveljcn", 0);
        rtfWriter.WriteControl("levelstartat", 1); //Start-At immer auf 1.

        rtfWriter.WriteControl("levelold", 0); //Kompatibel mit Word 2000?

        rtfWriter.StartContent();
        rtfWriter.WriteControl("leveltext");
        rtfWriter.WriteControl("leveltemplateid", templateID);
        rtfWriter.WriteControl(levelText1);
        if (levelText2 != "")
          rtfWriter.WriteControl(levelText2);
        
        this.rtfWriter.WriteSeparator();

        rtfWriter.EndContent();
        rtfWriter.StartContent();
        rtfWriter.WriteControl("levelnumbers");
        if (levelNumbers != "")
          rtfWriter.WriteControl(levelNumbers);
        
        this.rtfWriter.WriteSeparator();
        rtfWriter.EndContent();

        if (fontIdx >= 0)
            rtfWriter.WriteControl("f", fontIdx);

        rtfWriter.WriteControl("levelfollow", 0);

        rtfWriter.EndContent();
    }
  }
}
