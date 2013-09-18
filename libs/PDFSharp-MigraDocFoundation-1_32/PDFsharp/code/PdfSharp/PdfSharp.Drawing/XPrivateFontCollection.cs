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
using System.Globalization;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
#if GDI
using System.Drawing;
using System.Drawing.Text;
#endif
#if WPF
using System.Windows.Media;
#endif
using PdfSharp.Internal;
using PdfSharp.Fonts.OpenType;

namespace PdfSharp.Drawing
{
  ///<summary>
  /// Makes fonts that are not installed on the system available within the current application domain.
  /// </summary>
  public sealed class XPrivateFontCollection : IDisposable
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="XPrivateFontCollection"/> class.
    /// </summary>
    public XPrivateFontCollection()
    {
      //// HACK: Use one global PrivateFontCollection in GDI+
      //// TODO: Make a list of it
      //if (s_global != null)
      //  throw new InvalidOperationException("Because of limitations in GDI+ you can only have one instance of XPrivateFontCollection in your application.");
    }
    internal static XPrivateFontCollection s_global = new XPrivateFontCollection();

    //static XPrivateFontCollection()
    //{
    //  // HACK: Use one global PrivateFontCollection in GDI+
    //  // TODO: Make a list of it
    //  if (global != null)
    //    throw new InvalidOperationException("Because of limitations in GDI+ you can only have one instance of XPrivateFontCollection in your application.");
    //  global = this;
    //}
    //internal static XPrivateFontCollection global;

    /// <summary>
    /// Disposes all fonts from the collection.
    /// </summary>
    public void Dispose()
    {
#if GDI
      //privateFonts.Clear();
      this.privateFontCollection.Dispose();
      this.privateFontCollection = new PrivateFontCollection();
#endif
      s_global = null;
      //GC.SuppressFinalize(this);
    }

#if GDI
    internal static PrivateFontCollection GlobalPrivateFontCollection
    {
      get { return s_global != null ? s_global.privateFontCollection : null; }
    }
#endif


#if GDI
    //internal PrivateFontCollection PrivateFontCollection
    //{
    //  get { return privateFontCollection; }
    //  set { privateFontCollection = value; }
    //}
    // PrivateFontCollection of GDI+
    PrivateFontCollection privateFontCollection = new PrivateFontCollection();
#endif

    /// <summary>
    /// Gets the global font collection.
    /// </summary>
    public static XPrivateFontCollection Global
    {
      get { return s_global; }
    }

    /// <summary>
    /// Sets a new global font collection and returns the previous one, or null if no previous one exists.
    /// </summary>
    public static XPrivateFontCollection SetGlobalFontCollection(XPrivateFontCollection fontCollection)
    {
      if (fontCollection==null)
        throw new ArgumentNullException("fontCollection");

      XPrivateFontCollection old = s_global;
      s_global = fontCollection;
      return old;
    }

#if GDI
    /// <summary>
    /// Adds the font data to the font collections.
    /// </summary>
    public void AddFont(byte[] data, string familyName)
    {
      if (String.IsNullOrEmpty(familyName))
        throw new ArgumentNullException("familyName");

      //if (glyphTypeface == null)
      //  throw new ArgumentNullException("glyphTypeface");

      // Add to GDI+ PrivateFontCollection
      int length = data.Length;

      // Copy data without unsafe code 
      IntPtr ip = Marshal.AllocCoTaskMem(length);
      Marshal.Copy(data, 0, ip, length);
      this.privateFontCollection.AddMemoryFont(ip, length);
      Marshal.FreeCoTaskMem(ip);
      //privateFonts.Add(glyphTypeface);
    }
#endif

    //    /// <summary>
    //    /// Adds the glyph typeface to this collection.
    //    /// </summary>
    //    public void AddGlyphTypeface(XGlyphTypeface glyphTypeface)
    //    {
    //      if (glyphTypeface == null)
    //        throw new ArgumentNullException("glyphTypeface");

    //#if GDI
    //      // Add to GDI+ PrivateFontCollection
    //      byte[] data = glyphTypeface.FontData.Data;
    //      int length = data.Length;

    //      // Do it w/o unsafe code (do it like VB programmers do): 
    //      IntPtr ip = Marshal.AllocCoTaskMem(length);
    //      Marshal.Copy(data, 0, ip, length);
    //      this.privateFontCollection.AddMemoryFont(ip, length);
    //      Marshal.FreeCoTaskMem(ip);
    //      privateFonts.Add(glyphTypeface);
    //#endif
    //    }

#if GDI
    /// <summary>
    /// HACK: to be removed.
    /// </summary>
    //[Obsolete("Just make QBX compile. Will be removed when Private Fonts are working.", false)]
    public void AddFont(byte[] data, string fontName, bool bold, bool italic)
    {
      throw new NotImplementedException("AddFont");
      //AddGlyphTypeface(new XGlyphTypeface(data));
    }

    /// <summary>
    /// Adds a font from the specified file to this collection.
    /// </summary>
    public void AddFont(string filename)
    {
      throw new NotImplementedException("AddFont");
      //AddGlyphTypeface(new XGlyphTypeface(filename));
    }

    /// <summary>
    /// Adds a font from memory to this collection.
    /// </summary>
    public void AddFont(byte[] data)
    {
      throw new NotImplementedException("AddFont");
      //AddGlyphTypeface(new XGlyphTypeface(data));
    }
#endif

#if WPF
    /// <summary>
    /// Initializes a new instance of the FontFamily class from the specified font family name and an optional base uniform resource identifier (URI) value.
    /// Sample: Add(new Uri("pack://application:,,,/"), "./myFonts/#FontFamilyName");)
    /// </summary>
    /// <param name="baseUri">Specifies the base URI that is used to resolve familyName.</param>
    /// <param name="familyName">The family name or names that comprise the new FontFamily. Multiple family names should be separated by commas.</param>
    public void Add(Uri baseUri, string familyName)
    {
      // TODO: What means 'Multiple family names should be separated by commas.'?
      // does not work

      if (String.IsNullOrEmpty(familyName))
        throw new ArgumentNullException("familyName");
      if (familyName.Contains(","))
        throw new NotImplementedException("Only one family name is supported.");

      // family name starts right of '#'
      int idxHash = familyName.IndexOf('#');
      if (idxHash < 0)
        throw new ArgumentException("Family name must contain a '#'. Example './#MyFontFamilyName'", "familyName");

      string key = familyName.Substring(idxHash + 1);
      if (String.IsNullOrEmpty(key))
        throw new ArgumentException("familyName has invalid format.");

      if (this.fontFamilies.ContainsKey(key))
        throw new ArgumentException("An entry with the specified family name already exists.");

#if !SILVERLIGHT
      System.Windows.Media.FontFamily fontFamily = new System.Windows.Media.FontFamily(baseUri, familyName);
#else
      System.Windows.Media.FontFamily fontFamily = new System.Windows.Media.FontFamily(familyName);
#endif

      // Check whether font data really exists
#if DEBUG && !SILVERLIGHT
      ICollection<Typeface> list = fontFamily.GetTypefaces();
      foreach (Typeface typeFace in list)
      {
        //Debug.WriteLine(String.Format("{0}, {1}, {2}, {3}", typeFace.FaceNames.Values.First(), typeFace.Style, typeFace.Weight, typeFace.Stretch));
        GlyphTypeface glyphTypeface;
        if (!typeFace.TryGetGlyphTypeface(out glyphTypeface))
          throw new ArgumentException("Font with the specified family name does not exist.");
      }
#endif

      this.fontFamilies.Add(key, fontFamily);
    }
#endif

#if GDI
    internal static Font TryFindPrivateFont(string name, double size, FontStyle style)
    {
      try
      {
        PrivateFontCollection pfc = GlobalPrivateFontCollection;
        if (pfc == null)
          return null;
        foreach (System.Drawing.FontFamily family in pfc.Families)
        {
          if (String.Compare(family.Name, name, true) == 0)
            return new Font(family, (float)size, style, GraphicsUnit.World);
        }
      }
      catch
      {
#if DEBUG
#endif
      }
      return null;
    }
#endif

#if WPF
    internal static Typeface TryFindTypeface(string name, XFontStyle style, out System.Windows.Media.FontFamily fontFamily)
    {
      if (s_global.fontFamilies.TryGetValue(name, out fontFamily))
      {
        Typeface typeface = FontHelper.CreateTypeface(fontFamily, style);
        return typeface;
      }
      return null;
    }
#endif

#if GDI___
    internal XGlyphTypeface FindFont(string fontName, bool bold, bool italic)
    {
      //if (privateFonts != null)
      {
        for (int i = 0; i < 3; ++i)
        {
          // We make 4 passes.
          // On second pass, we ignore Italic.
          // On third pass, we ignore Bold.
          // On fourth pass, we ignore Bold and Italic
          foreach (XGlyphTypeface pf in privateFonts)
          {
            if (string.Compare(pf.FamilyName, fontName, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
              switch (i)
              {
                case 0:
                  if (pf.IsBold == bold && pf.IsItalic == italic)
                    return pf;
                  break;
                case 1:
                  if (pf.IsBold == bold /*&& pf.Italic == italic*/)
                    return pf;
                  break;
                case 2:
                  if (/*pf.Bold == bold &&*/ pf.IsItalic == italic)
                    return pf;
                  break;
                case 3:
                  //if (pf.Bold == bold && pf.Italic == italic)
                  return pf;
              }
            }
          }
        }
      }
      return null;
    }
#endif

#if GDI
    //List<XGlyphTypeface> privateFonts = new List<XGlyphTypeface>();
#endif
#if WPF
    readonly Dictionary<string, System.Windows.Media.FontFamily> fontFamilies = new Dictionary<string, System.Windows.Media.FontFamily>(StringComparer.InvariantCultureIgnoreCase);
#endif
  }
}
