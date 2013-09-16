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
using System.Text;
using System.IO;
using PdfSharp.Internal;

namespace PdfSharp.Pdf.Advanced
{
  /// <summary>
  /// Represents a PDF resource object.
  /// </summary>
  public sealed class PdfResources : PdfDictionary
  {
    // Resource management works roughly like this:
    // When the user creates an XFont and uses it in the XGraphics of a PdfPage, then at the first time
    // a PdfFont is created and cached in the document global font table. If the user creates a new
    // XFont object for an exisisting PdfFont, the PdfFont object is reused. When the PdfFont is added
    // to the resources of a PdfPage for the first time, it is added to the page local PdfResourceMap for 
    // fonts and automatically associated with a local resource name.

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfResources"/> class.
    /// </summary>
    /// <param name="document">The document.</param>
    public PdfResources(PdfDocument document)
      : base(document)
    {
      Elements[Keys.ProcSet] = new PdfLiteral("[/PDF/Text/ImageB/ImageC/ImageI]");
    }

    internal PdfResources(PdfDictionary dict)
      : base(dict)
    { }

    /// <summary>
    /// Adds the specified font to this resource dictionary and returns its local resource name.
    /// </summary>
    public string AddFont(PdfFont font)
    {
      string name;
      if (!this.resources.TryGetValue(font, out name))
      {
        name = NextFontName;
        this.resources[font] = name;
        if (font.Reference == null)
          Owner.irefTable.Add(font);
        Fonts.Elements[name] = font.Reference;
      }
      return name;
    }

    /// <summary>
    /// Adds the specified image to this resource dictionary
    /// and returns its local resource name.
    /// </summary>
    public string AddImage(PdfImage image)
    {
      string name;
      if (!this.resources.TryGetValue(image, out name))
      {
        name = NextImageName;
        this.resources[image] = name;
        if (image.Reference == null)
          Owner.irefTable.Add(image);
        XObjects.Elements[name] = image.Reference;
      }
      return name;
    }

    /// <summary>
    /// Adds the specified form object to this resource dictionary
    /// and returns its local resource name.
    /// </summary>
    public string AddForm(PdfFormXObject form)
    {
      string name;
      if (!this.resources.TryGetValue(form, out name))
      {
        name = NextFormName;
        this.resources[form] = name;
        if (form.Reference == null)
          Owner.irefTable.Add(form);
        XObjects.Elements[name] = form.Reference;
      }
      return name;
    }

    /// <summary>
    /// Adds the specified graphics state to this resource dictionary
    /// and returns its local resource name.
    /// </summary>
    public string AddExtGState(PdfExtGState extGState)
    {
      string name;
      if (!this.resources.TryGetValue(extGState, out name))
      {
        name = NextExtGStateName;
        this.resources[extGState] = name;
        if (extGState.Reference == null)
          Owner.irefTable.Add(extGState);
        ExtGStates.Elements[name] = extGState.Reference;
      }
      return name;
    }

    /// <summary>
    /// Adds the specified pattern to this resource dictionary
    /// and returns its local resource name.
    /// </summary>
    public string AddPattern(PdfShadingPattern pattern)
    {
      string name;
      if (!this.resources.TryGetValue(pattern, out name))
      {
        name = NextPatternName;
        this.resources[pattern] = name;
        if (pattern.Reference == null)
          Owner.irefTable.Add(pattern);
        Patterns.Elements[name] = pattern.Reference;
      }
      return name;
    }

    /// <summary>
    /// Adds the specified pattern to this resource dictionary
    /// and returns its local resource name.
    /// </summary>
    public string AddPattern(PdfTilingPattern pattern)
    {
      string name;
      if (!this.resources.TryGetValue(pattern, out name))
      {
        name = NextPatternName;
        this.resources[pattern] = name;
        if (pattern.Reference == null)
          Owner.irefTable.Add(pattern);
        Patterns.Elements[name] = pattern.Reference;
      }
      return name;
    }

    /// <summary>
    /// Adds the specified shading to this resource dictionary
    /// and returns its local resource name.
    /// </summary>
    public string AddShading(PdfShading shading)
    {
      string name;
      if (!this.resources.TryGetValue(shading, out name))
      {
        name = NextShadingName;
        this.resources[shading] = name;
        if (shading.Reference == null)
          Owner.irefTable.Add(shading);
        Shadings.Elements[name] = shading.Reference;
      }
      return name;
    }

    /// <summary>
    /// Gets the fonts map.
    /// </summary>
    internal PdfResourceMap Fonts
    {
      get
      {
        if (this.fonts == null)
          this.fonts = (PdfResourceMap)Elements.GetValue(Keys.Font, VCF.Create);
        return this.fonts;
      }
    }
    PdfResourceMap fonts;

    /// <summary>
    /// Gets the external objects map.
    /// </summary>
    internal PdfResourceMap XObjects
    {
      get
      {
        if (this.xObjects == null)
          this.xObjects = (PdfResourceMap)Elements.GetValue(Keys.XObject, VCF.Create);
        return this.xObjects;
      }
    }
    PdfResourceMap xObjects;

    // TODO: make own class
    internal PdfResourceMap ExtGStates
    {
      get
      {
        if (this.extGStates == null)
          this.extGStates = (PdfResourceMap)Elements.GetValue(Keys.ExtGState, VCF.Create);
        return this.extGStates;
      }
    }
    PdfResourceMap extGStates;

    // TODO: make own class
    internal PdfResourceMap ColorSpaces
    {
      get
      {
        if (this.colorSpaces == null)
          this.colorSpaces = (PdfResourceMap)Elements.GetValue(Keys.ColorSpace, VCF.Create);
        return this.colorSpaces;
      }
    }
    PdfResourceMap colorSpaces;

    // TODO: make own class
    internal PdfResourceMap Patterns
    {
      get
      {
        if (this.patterns == null)
          this.patterns = (PdfResourceMap)Elements.GetValue(Keys.Pattern, VCF.Create);
        return this.patterns;
      }
    }
    PdfResourceMap patterns;

    // TODO: make own class
    internal PdfResourceMap Shadings
    {
      get
      {
        if (this.shadings == null)
          this.shadings = (PdfResourceMap)Elements.GetValue(Keys.Shading, VCF.Create);
        return this.shadings;
      }
    }
    PdfResourceMap shadings;

    // TODO: make own class
    internal PdfResourceMap Properties
    {
      get
      {
        if (this.properties == null)
          this.properties = (PdfResourceMap)Elements.GetValue(Keys.Properties, VCF.Create);
        return this.properties;
      }
    }
    PdfResourceMap properties;

    /// <summary>
    /// Gets a new local name for this resource.
    /// </summary>
    string NextFontName
    {
      get
      {
        string name;
        while (ExistsResourceNames(name = String.Format("/F{0}", this.fontNumber++))) { }
        return name;
      }
    }
    int fontNumber;

    /// <summary>
    /// Gets a new local name for this resource.
    /// </summary>
    string NextImageName
    {
      get
      {
        string name;
        while (ExistsResourceNames(name = String.Format("/I{0}", this.imageNumber++))) { }
        return name;
      }
    }
    int imageNumber;

    /// <summary>
    /// Gets a new local name for this resource.
    /// </summary>
    string NextFormName
    {
      get
      {
        string name;
        while (ExistsResourceNames(name = String.Format("/Fm{0}", this.formNumber++))) { }
        return name;
      }
    }
    int formNumber;

    /// <summary>
    /// Gets a new local name for this resource.
    /// </summary>
    string NextExtGStateName
    {
      get
      {
        string name;
        while (ExistsResourceNames(name = String.Format("/GS{0}", this.ExtGStateNumber++))) { }
        return name;
      }
    }
    int ExtGStateNumber;

    /// <summary>
    /// Gets a new local name for this resource.
    /// </summary>
    string NextPatternName
    {
      get
      {
        string name;
        while (ExistsResourceNames(name = String.Format("/Pa{0}", this.PatternNumber++))) ;
        return name;
      }
    }
    int PatternNumber;

    /// <summary>
    /// Gets a new local name for this resource.
    /// </summary>
    string NextShadingName
    {
      get
      {
        string name;
        while (ExistsResourceNames(name = String.Format("/Sh{0}", this.ShadingNumber++))) ;
        return name;
      }
    }
    int ShadingNumber;

    /// <summary>
    /// Check whether a resource name is already used in the context of this resource dictionary.
    /// PDF4NET uses GUIDs as resource names, but I think this weapon is to heavy.
    /// </summary>
    internal bool ExistsResourceNames(string name)
    {
      // TODO: more precise: is this page imported and is PageOptions != Replace
      // BUG: 
      //if (!this.Owner.IsImported)
      //  return false;

      // Collect all resouce names of all imported resources.
      if (this.importedResourceNames == null)
      {
        this.importedResourceNames = new Dictionary<string, object>();

        if (Elements[Keys.Font] != null)
          Fonts.CollectResourceNames(this.importedResourceNames);

        if (Elements[Keys.XObject] != null)
          XObjects.CollectResourceNames(this.importedResourceNames);

        if (Elements[Keys.ExtGState] != null)
          ExtGStates.CollectResourceNames(this.importedResourceNames);

        if (Elements[Keys.ColorSpace] != null)
          ColorSpaces.CollectResourceNames(this.importedResourceNames);

        if (Elements[Keys.Pattern] != null)
          Patterns.CollectResourceNames(this.importedResourceNames);

        if (Elements[Keys.Shading] != null)
          Shadings.CollectResourceNames(this.importedResourceNames);

        if (Elements[Keys.Properties] != null)
          Properties.CollectResourceNames(this.importedResourceNames);
      }
      return this.importedResourceNames.ContainsKey(name);
      // This is superfluous because PDFsharp resource names cannot be double.
      // this.importedResourceNames.Add(name, null);
    }

    /// <summary>
    /// All the names of imported resources.
    /// </summary>
    Dictionary<string, object> importedResourceNames;

    /// <summary>
    /// Maps all PDFsharp resources to their local resource names.
    /// </summary>
    readonly Dictionary<PdfObject, string> resources = new Dictionary<PdfObject, string>();

    /// <summary>
    /// Predefined keys of this dictionary.
    /// </summary>
    public sealed class Keys : KeysBase
    {
      /// <summary>
      /// (Optional) A dictionary that maps resource names to graphics state 
      /// parameter dictionaries.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfResourceMap))]
      public const string ExtGState = "/ExtGState";

      /// <summary>
      /// (Optional) A dictionary that maps each resource name to either the name of a
      /// device-dependent color space or an array describing a color space.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfResourceMap))]
      public const string ColorSpace = "/ColorSpace";

      /// <summary>
      /// (Optional) A dictionary that maps each resource name to either the name of a
      /// device-dependent color space or an array describing a color space.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfResourceMap))]
      public const string Pattern = "/Pattern";

      /// <summary>
      /// (Optional; PDF 1.3) A dictionary that maps resource names to shading dictionaries.
      /// </summary>
      [KeyInfo("1.3", KeyType.Dictionary | KeyType.Optional, typeof(PdfResourceMap))]
      public const string Shading = "/Shading";

      /// <summary>
      /// (Optional) A dictionary that maps resource names to external objects.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfResourceMap))]
      public const string XObject = "/XObject";

      /// <summary>
      /// (Optional) A dictionary that maps resource names to font dictionaries.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfResourceMap))]
      public const string Font = "/Font";

      /// <summary>
      /// (Optional) An array of predefined procedure set names.
      /// </summary>
      [KeyInfo(KeyType.Array | KeyType.Optional)]
      public const string ProcSet = "/ProcSet";

      /// <summary>
      /// (Optional; PDF 1.2) A dictionary that maps resource names to property list
      /// dictionaries for marked content.
      /// </summary>
      [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfResourceMap))]
      public const string Properties = "/Properties";

      /// <summary>
      /// Gets the KeysMeta for these keys.
      /// </summary>
      internal static DictionaryMeta Meta
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
