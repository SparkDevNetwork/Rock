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
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Internal;

namespace PdfSharp.Pdf
{
  /// <summary>
  /// Represents an outline item in the outlines tree. An outline is also knows as a bookmark.
  /// </summary>
  public sealed class PdfOutline : PdfDictionary
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfOutline"/> class.
    /// </summary>
    public PdfOutline()
    {
      //this.outlines = new PdfOutlineCollection(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfOutline"/> class.
    /// </summary>
    /// <param name="document">The document.</param>
    internal PdfOutline(PdfDocument document)
      : base(document)
    {
      //this.outlines = new PdfOutlineCollection(this);
    }

    /// <summary>
    /// Initializes a new instance from an existing dictionary. Used for object type transformation.
    /// </summary>
    public PdfOutline(PdfDictionary dict)
      : base(dict)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfOutline"/> class.
    /// </summary>
    /// <param name="title">The outline text.</param>
    /// <param name="destinationPage">The destination page.</param>
    /// <param name="opened">Specifies whether the node is displayed expanded (opened) or collapsed.</param>
    /// <param name="style">The font style used to draw the outline text.</param>
    /// <param name="textColor">The color used to draw the outline text.</param>
    public PdfOutline(string title, PdfPage destinationPage, bool opened, PdfOutlineStyle style, XColor textColor)
    {
      Title = title;
      DestinationPage = destinationPage;
      Opened = opened;
      Style = style;
      TextColor = textColor;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfOutline"/> class.
    /// </summary>
    /// <param name="title">The outline text.</param>
    /// <param name="destinationPage">The destination page.</param>
    /// <param name="opened">Specifies whether the node is displayed expanded (opened) or collapsed.</param>
    /// <param name="style">The font style used to draw the outline text.</param>
    public PdfOutline(string title, PdfPage destinationPage, bool opened, PdfOutlineStyle style)
    {
      Title = title;
      DestinationPage = destinationPage;
      Opened = opened;
      Style = style;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfOutline"/> class.
    /// </summary>
    /// <param name="title">The outline text.</param>
    /// <param name="destinationPage">The destination page.</param>
    /// <param name="opened">Specifies whether the node is displayed expanded (opened) or collapsed.</param>
    public PdfOutline(string title, PdfPage destinationPage, bool opened)
    {
      Title = title;
      DestinationPage = destinationPage;
      Opened = opened;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfOutline"/> class.
    /// </summary>
    /// <param name="title">The outline text.</param>
    /// <param name="destinationPage">The destination page.</param>
    public PdfOutline(string title, PdfPage destinationPage)
    {
      Title = title;
      DestinationPage = destinationPage;
    }

    internal int Count
    {
      get { return this.count; }
      set { this.count = value; }
    }
    int count;

    /// <summary>
    /// The total number of open descendants at all lower levels.
    /// </summary>
    internal int openCount;

    //internal int CountOpen()
    //{
    //  int count = this.opened ? 1 : 0;
    //  if (this.outlines != null)
    //    count += this.outlines.CountOpen();
    //  return count;
    //}

    internal PdfOutline Parent
    {
      get { return this.parent; }
      set { this.parent = value; }
    }
    PdfOutline parent;

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title
    {
      get { return Elements.GetString(Keys.Title); }
      set
      {
        PdfString s = new PdfString(value, PdfStringEncoding.PDFDocEncoding);
        Elements.SetValue(Keys.Title, s);
      }
    }

    /// <summary>
    /// Gets or sets the destination page.
    /// </summary>
    public PdfPage DestinationPage
    {
      get { return this.destinationPage; }
      set { this.destinationPage = value; }
    }
    PdfPage destinationPage;

    /// <summary>
    /// Gets or sets whether the outline item is opened (or expanded).
    /// </summary>
    public bool Opened
    {
      get { return this.opened2; }
      set { this.opened2 = value; }
      // TODO: adjust openCount of ascendant...
#if false
      set 
      {
        if (this.opened != value)
        {
          this.opened = value;
          int sign = value ? 1 : -1;
          PdfOutline parent = this.parent;
          if (this.opened)
          {
            while (parent != null)
            {
              parent.openCount += 1 + this.openCount;
            }
          }
          else
          {
          }
        }
      }
#endif
    }
    bool opened2;

    /// <summary>
    /// Gets or sets the style.
    /// </summary>
    public PdfOutlineStyle Style
    {
      get { return (PdfOutlineStyle)Elements.GetInteger(Keys.F); }
      set { Elements.SetInteger(Keys.F, (int)value); }
    }

    /// <summary>
    /// Gets or sets the color of the text.
    /// </summary>
    /// <value>The color of the text.</value>
    public XColor TextColor
    {
      get { return this.textColor; }
      set { this.textColor = value; }
    }
    XColor textColor;

    /// <summary>
    /// Gets the outline collection of this node.
    /// </summary>
    public PdfOutlineCollection Outlines
    {
      get
      {
        if (this.outlines == null)
          this.outlines = new PdfOutlineCollection(this.Owner, this);
        return this.outlines;
      }
    }
    PdfOutlineCollection outlines;

    /// <summary>
    /// Creates key/values pairs according to the object structure.
    /// </summary>
    internal override void PrepareForSave()
    {
      bool hasKids = this.outlines != null && this.outlines.Count > 0;
      if (this.parent != null || hasKids)
      {
        if (this.parent == null)
        {
          // This is the outline dictionary (the root)
          Elements[Keys.First] = this.outlines[0].Reference;
          Elements[Keys.Last] = this.outlines[this.outlines.Count - 1].Reference;

          // TODO: /Count - the meaning is not completely clear to me
          if (this.openCount > 0)
            Elements[Keys.Count] = new PdfInteger(this.openCount);
        }
        else
        {
          // This is an outline item dictionary
          Elements[Keys.Parent] = this.parent.Reference;

          int count = this.parent.outlines.Count;
          int index = this.parent.outlines.IndexOf(this);
          Debug.Assert(index != -1);

          if (DestinationPage != null)
            Elements[Keys.Dest] = new PdfArray(this.Owner,
              DestinationPage.Reference,
              new PdfLiteral("/XYZ null null 0"));

          if (index > 0)
            Elements[Keys.Prev] = this.parent.outlines[index - 1].Reference;

          if (index < count - 1)
            Elements[Keys.Next] = this.parent.outlines[index + 1].Reference;

          if (hasKids)
          {
            Elements[Keys.First] = this.outlines[0].Reference;
            Elements[Keys.Last] = this.outlines[this.outlines.Count - 1].Reference;
          }
          // TODO: /Count - the meaning is not completely clear to me
          if (this.openCount > 0)
            Elements[Keys.Count] = new PdfInteger((this.opened2 ? 1 : -1) * this.openCount);

          if (this.textColor != XColor.Empty && this.Owner.HasVersion("1.4"))
            Elements[Keys.C] = new PdfLiteral("[{0}]", PdfEncoders.ToString(this.textColor, PdfColorMode.Rgb));

          // if (this.Style != PdfOutlineStyle.Regular && this.Document.HasVersion("1.4"))
          //  //pdf.AppendFormat("/F {0}\n", (int)this.style);
          //  Elements[Keys.F] = new PdfInteger((int)this.style);
        }
        // Prepare kids
        if (hasKids)
          foreach (PdfOutline outline in this.outlines)
            outline.PrepareForSave();
      }
    }

    internal override void WriteObject(PdfWriter writer)
    {
      bool hasKids = this.outlines != null && this.outlines.Count > 0;
      if (this.parent != null || hasKids)
      {
        // Everything done in PrepareForSave
        if (this.parent == null)
        {
          // This is the outline dictionary (the root)
        }
        else
        {
          // This is an outline item dictionary
        }
        base.WriteObject(writer);
      }
    }

    /// <summary>
    /// Represents a collection of outlines.
    /// </summary>
    public class PdfOutlineCollection : PdfObject, IEnumerable
    {
      internal PdfOutlineCollection(PdfDocument document, PdfOutline parent)
        : base(document)
      {
        this.parent = parent;
      }

      /// <summary>
      /// Indicates whether the outline has at least one entry.
      /// </summary>
      public bool HasOutline
      {
        get { return this.outlines != null && this.outlines.Count > 0; }
      }

      /// <summary>
      /// Gets the number of entries in this collection.
      /// </summary>
      public int Count
      {
        get { return this.outlines.Count; }
      }

      //internal int CountOpen()
      //{
      //  int count = 0;
      //  foreach (PdfOutline outline in this.outlines)
      //    count += outline.CountOpen();
      //  return count;
      //}

      /// <summary>
      /// Adds the specified outline.
      /// </summary>
      public void Add(PdfOutline outline)
      {
        if (outline == null)
          throw new ArgumentNullException("outline");

        if (!Object.ReferenceEquals(Owner, outline.DestinationPage.Owner))
          throw new ArgumentException("Destination page must belong to this document.");

        // TODO check the parent problems...
        outline.Document = Owner;
        outline.parent = this.parent;


        this.outlines.Add(outline);
        this.Owner.irefTable.Add(outline);

        if (outline.Opened)
        {
          outline = this.parent;
          while (outline != null)
          {
            outline.openCount++;
            outline = outline.parent;
          }
        }
      }

      /// <summary>
      /// Adds the specified outline entry.
      /// </summary>
      /// <param name="title">The outline text.</param>
      /// <param name="destinationPage">The destination page.</param>
      /// <param name="opened">Specifies whether the node is displayed expanded (opened) or collapsed.</param>
      /// <param name="style">The font style used to draw the outline text.</param>
      /// <param name="textColor">The color used to draw the outline text.</param>
      public PdfOutline Add(string title, PdfPage destinationPage, bool opened, PdfOutlineStyle style, XColor textColor)
      {
        PdfOutline outline = new PdfOutline(title, destinationPage, opened, style, textColor);
        Add(outline);
        return outline;
      }

      /// <summary>
      /// Adds the specified outline entry.
      /// </summary>
      /// <param name="title">The outline text.</param>
      /// <param name="destinationPage">The destination page.</param>
      /// <param name="opened">Specifies whether the node is displayed expanded (opened) or collapsed.</param>
      /// <param name="style">The font style used to draw the outline text.</param>
      public PdfOutline Add(string title, PdfPage destinationPage, bool opened, PdfOutlineStyle style)
      {
        PdfOutline outline = new PdfOutline(title, destinationPage, opened, style);
        Add(outline);
        return outline;
      }

      /// <summary>
      /// Adds the specified outline entry.
      /// </summary>
      /// <param name="title">The outline text.</param>
      /// <param name="destinationPage">The destination page.</param>
      /// <param name="opened">Specifies whether the node is displayed expanded (opened) or collapsed.</param>
      public PdfOutline Add(string title, PdfPage destinationPage, bool opened)
      {
        PdfOutline outline = new PdfOutline(title, destinationPage, opened);
        Add(outline);
        return outline;
      }

      /// <summary>
      /// Adds the specified outline entry.
      /// </summary>
      /// <param name="title">The outline text.</param>
      /// <param name="destinationPage">The destination page.</param>
      public PdfOutline Add(string title, PdfPage destinationPage)
      {
        PdfOutline outline = new PdfOutline(title, destinationPage);
        Add(outline);
        return outline;
      }

      /// <summary>
      /// Gets the index of the specified outline.
      /// </summary>
      public int IndexOf(PdfOutline item)
      {
        return this.outlines.IndexOf(item);
      }

      /// <summary>
      /// Gets the <see cref="PdfSharp.Pdf.PdfOutline"/> at the specified index.
      /// </summary>
      public PdfOutline this[int index]
      {
        get
        {
          if (index < 0 || index >= this.outlines.Count)
            throw new ArgumentOutOfRangeException("index", index, PSSR.OutlineIndexOutOfRange);
          return (PdfOutline)this.outlines[index];
        }
        //set
        //{
        //  if (index < 0 || index >= this.outlines.Count)
        //    throw new ArgumentOutOfRangeException("index", index, PSSR.OutlineIndexOutOfRange);
        //  this.outlines[index] = value;
        //}
      }

      /// <summary>
      /// Returns an enumerator that iterates through a collection.
      /// </summary>
      public IEnumerator GetEnumerator()
      {
        return outlines.GetEnumerator();
      }

      private PdfOutline parent;
      private List<PdfOutline> outlines = new List<PdfOutline>();
    }

    /// <summary>
    /// Predefined keys of this dictionary.
    /// </summary>
    internal sealed class Keys : KeysBase
    {
      /// <summary>
      /// (Optional) The type of PDF object that this dictionary describes; if present,
      /// must be Outlines for an outline dictionary.
      /// </summary>
      [KeyInfo(KeyType.Name | KeyType.Optional, FixedValue = "Outlines")]
      public const string Type = "/Type";

      // Outline and outline item are combined
      ///// <summary>
      ///// (Required if there are any open or closed outline entries; must be an indirect reference)
      ///// An outline item dictionary representing the first top-level item in the outline.
      ///// </summary>
      //[KeyInfo(KeyType.Dictionary)]
      //public const string First = "/First";
      //
      ///// <summary>
      ///// (Required if there are any open or closed outline entries; must be an indirect reference)
      ///// An outline item dictionary representing the last top-level item in the outline.
      ///// </summary>
      //[KeyInfo(KeyType.Dictionary)]
      //public const string Last = "/Last";
      //
      ///// <summary>
      ///// (Required if the document has any open outline entries) The total number of open items at all
      ///// levels of the outline. This entry should be omitted if there are no open outline items.
      ///// </summary>
      //[KeyInfo(KeyType.Integer)]
      //public const string Count = "/Count";

      /// <summary>
      /// (Required) The text to be displayed on the screen for this item.
      /// </summary>
      [KeyInfo(KeyType.String | KeyType.Required)]
      public const string Title = "/Title";

      /// <summary>
      /// (Required; must be an indirect reference) The parent of this item in the outline hierarchy.
      /// The parent of a top-level item is the outline dictionary itself.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Required)]
      public const string Parent = "/Parent";

      /// <summary>
      /// (Required for all but the first item at each level; must be an indirect reference)
      /// The previous item at this outline level.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Required)]
      public const string Prev = "/Prev";

      /// <summary>
      /// (Required for all but the last item at each level; must be an indirect reference)
      /// The next item at this outline level.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Required)]
      public const string Next = "/Next";

      /// <summary>
      /// (Required if the item has any descendants; must be an indirect reference)
      ///  The first of this item’s immediate children in the outline hierarchy.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Required)]
      public const string First = "/First";

      /// <summary>
      /// (Required if the item has any descendants; must be an indirect reference)
      /// The last of this item’s immediate children in the outline hierarchy.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Required)]
      public const string Last = "/Last";

      /// <summary>
      /// (Required if the item has any descendants) If the item is open, the total number of its 
      /// open descendants at all lower levels of the outline hierarchy. If the item is closed, a 
      /// negative integer whose absolute value specifies how many descendants would appear if the 
      /// item were reopened.
      /// </summary>
      [KeyInfo(KeyType.Integer | KeyType.Required)]
      public const string Count = "/Count";

      /// <summary>
      /// (Optional; not permitted if an A entry is present) The destination to be displayed when this 
      /// item is activated.
      /// </summary>
      [KeyInfo(KeyType.ArrayOrNameOrString | KeyType.Optional)]
      public const string Dest = "/Dest";

      /// <summary>
      /// (Optional; not permitted if an A entry is present) The destination to be displayed when 
      /// this item is activated.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
      public const string A = "/A";

      /// <summary>
      /// (Optional; PDF 1.3; must be an indirect reference) The structure element to which the item 
      /// refers.
      /// Note: The ability to associate an outline item with a structure element (such as the beginning 
      /// of a chapter) is a PDF 1.3 feature. For backward compatibility with earlier PDF versions, such
      /// an item should also specify a destination (Dest) corresponding to an area of a page where the
      /// contents of the designated structure element are displayed.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
      public const string SE = "/SE";

      /// <summary>
      /// (Optional; PDF 1.4) An array of three numbers in the range 0.0 to 1.0, representing the 
      /// components in the DeviceRGB color space of the color to be used for the outline entry’s text.
      /// Default value: [0.0 0.0 0.0].
      /// </summary>
      [KeyInfo(KeyType.Array | KeyType.Optional)]
      public const string C = "/C";

      /// <summary>
      /// (Optional; PDF 1.4) A set of flags specifying style characteristics for displaying the outline
      /// item’s text. Default value: 0.
      /// </summary>
      [KeyInfo(KeyType.Integer | KeyType.Optional)]
      public const string F = "/F";

      /// <summary>
      /// Gets the KeysMeta for these keys.
      /// </summary>
      public static DictionaryMeta Meta
      {
        get
        {
          if (Keys.meta == null)
            Keys.meta = CreateMeta(typeof(Keys));
          return Keys.meta;
        }
      }
      static DictionaryMeta meta;
    }

    /// <summary>
    /// Gets the KeysMeta of this dictionary type.
    /// </summary>
    internal override DictionaryMeta Meta
    {
      get { return Keys.Meta; }
    }
  }
}
