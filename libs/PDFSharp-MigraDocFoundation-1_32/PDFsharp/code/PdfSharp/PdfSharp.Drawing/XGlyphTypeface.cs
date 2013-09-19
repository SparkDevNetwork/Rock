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
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.IO;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
#endif
using PdfSharp.Internal;
using PdfSharp.Fonts.OpenType;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Drawing
{
  /// <summary>
  /// Specifies a physical font face that corresponds to a font file on the disk.
  /// </summary>
  //[DebuggerDisplay("'{Name}', {Size}")]
  public class XGlyphTypeface
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="XGlyphTypeface"/> class from the specified font file.
    /// </summary>
    public XGlyphTypeface(string filename)
    {
      if (String.IsNullOrEmpty(filename))
        throw new ArgumentNullException("filename");

      FileStream stream = null;
      try
      {
        stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
        int length = (int)stream.Length;
        byte[] data = new byte[length];
        stream.Read(data, 0, length);
        Initialize(data);
      }
      finally
      {
        if (stream != null)
          stream.Close();
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XGlyphTypeface"/> class from the specified font bytes.
    /// </summary>
    public XGlyphTypeface(byte[] data)
    {
      if (data == null)
        throw new ArgumentNullException("data");

      Initialize(data);
    }

    void Initialize(byte[] data)
    {
      // Cache data and return a FontData
      this.fontData = FontDataStock.Global.RegisterFontData(data);
    }

    internal FontData FontData
    {
      get { return this.fontData; }
    }
    private FontData fontData;

    /// <summary>
    /// Gets the English family name of the font.
    /// </summary>
    public string FamilyName
    {
      get { return "Times"; }
    }

    /// <summary>
    /// Gets a value indicating whether the font weight is bold.
    /// </summary>
    public bool IsBold
    {
      get { return false; }
    }

    /// <summary>
    /// Gets a value indicating whether the font style is italic.
    /// </summary>
    public bool IsItalic
    {
      get { return false; }
    }

    //internal byte[] GetData()
    //{
    //  return this.data;
    //}
    //private byte[] data;


    //public XPrivateFont(string fontName,
    //  bool bold,
    //  bool italic,
    //  byte[] data,
    //  int length)
    //{
    //  this.FontName = fontName;
    //  this.Bold = bold;
    //  this.Italic = italic;
    //  this.Data = data;
    //  this.Length = length;
    //}

    //internal string FontName;
    //internal bool Bold;
    //internal bool Italic;
    //internal byte[] Data;
    //internal int Length;

    //public int GetFontData(ref byte[] data,
    //   int length)
    //{
    //  if (length == this.Length)
    //  {
    //    // Copy the data:
    //    //Data.CopyTo(data, 0);
    //    Array.Copy(Data, data, length);
    //  }
    //  return this.Length;
    //}

#if true_
    #region Constructors 

        /// <summary> 
        /// Creates an uninitialized GlyphTypeface object. Caller should call ISupportInitialize.BeginInit() 
        /// to begin initializing the object and call ISupportInitialize.EndInit() to finish the initialization.
        /// </summary> 
        public GlyphTypeface()
        {
        }
 
        /// <summary>
        /// Creates a new GlyphTypeface object from a .otf, .ttf or .ttc font face specified by typefaceSource. 
        /// The constructed GlyphTypeface does not use style simulations. 
        /// </summary>
        /// <param name="typefaceSource">Specifies the URI of a font file used by the newly created GlyphTypeface.</param> 
        public GlyphTypeface(Uri typefaceSource) : this(typefaceSource, StyleSimulations.None)
        {}

        /// <summary> 
        /// Creates a new GlyphTypeface object from a .otf, .ttf or .ttc font face specified by typefaceSource.
        /// The constructed GlyphTypeface uses style simulations specified by styleSimulations parameter. 
        /// </summary> 
        /// <param name="typefaceSource">Specifies the URI of a font file used by the newly created GlyphTypeface.</param>
        /// <param name="styleSimulations">Specifies style simulations to be applied to the newly created GlyphTypeface.</param> 
        /// <SecurityNote>
        /// Critical - as this calls the internal constructor that's critical.
        /// Safe - as the internal constructor does a Demand for FileIO for file
        ///        Uris for the case where fromPublic is true.  We block constructing 
        ///        GlyphTypeface directly in SEE since this'd allow guessing fonts on
        ///        a machine by trying to create a GlyphTypeface object. 
        /// 
        /// </SecurityNote>
        [SecurityCritical] 
        public GlyphTypeface(Uri typefaceSource, StyleSimulations styleSimulations) :
                            this (typefaceSource, styleSimulations, /* fromPublic = */ true)
        {}
 
        /// <summary>
        /// Creates a new GlyphTypeface object from a .otf, .ttf or .ttc font face specified by typefaceSource. 
        /// The constructed GlyphTypeface uses style simulations specified by styleSimulations parameter. 
        /// </summary>
        /// <param name="typefaceSource">Specifies the URI of a font file used by the newly created GlyphTypeface.</param> 
        /// <param name="styleSimulations">Specifies style simulations to be applied to the newly created GlyphTypeface.</param>
        /// <param name="fromPublic">Specifies if the call to the constructor is from a public constructor
        /// or if its from an internal method. For public constructor we demand FileIO for all files whereas
        /// for internal calls we don't demand in the constructor. </param> 
        /// <SecurityNote>
        /// Critical - as the instance of GlyphTypeface created with this constructor can 
        ///            expose font information for the case where fromPublic is false. 
        /// </SecurityNote>
        [SecurityCritical] 
        internal GlyphTypeface(Uri typefaceSource, StyleSimulations styleSimulations, bool fromPublic)
        {
            Initialize(typefaceSource, styleSimulations, fromPublic);
        } 

        /// <SecurityNote> 
        /// Critical - this method calls into other critical method. 
        /// </SecurityNote>
        [SecurityCritical] 
        private void Initialize(Uri typefaceSource, StyleSimulations styleSimulations, bool fromPublic)
        {
            if (typefaceSource == null)
                throw new ArgumentNullException("typefaceSource"); 

            if (!typefaceSource.IsAbsoluteUri) 
                throw new ArgumentException(SR.Get(SRID.UriNotAbsolute), "typefaceSource"); 

            // remember the original Uri that contains face index 
            _originalUri = new SecurityCriticalDataClass<Uri>(typefaceSource);

            // split the Uri into the font source Uri and face index
            Uri fontSourceUri; 
            int faceIndex;
            Util.SplitFontFaceIndex(typefaceSource, out fontSourceUri, out faceIndex); 
 
            _fileIOPermObj = new SecurityCriticalDataForSet<CodeAccessPermission>(
                SecurityHelper.CreateUriReadPermission(fontSourceUri) 
                );

            // This permission demand is here so that untrusted callers are unable to check for file existence using GlyphTypeface ctor.
            // Sensitive font data is protected by demands as the user tries to access it. 
            // The demand below is skipped for non-public calls, because in such cases
            // fonts are exposed as logical fonts to the end user. 
            if (fromPublic) 
                DemandPermissionsForFontInformation();
 
            // We skip permission demands for FontSource because the above line already demands them for the right callers.
            _fontFace = new FontFaceLayoutInfo(new FontSource(fontSourceUri, true), faceIndex);
            CacheManager.Lookup(_fontFace);
 
            if ((styleSimulations & ~StyleSimulations.BoldItalicSimulation) != 0)
                throw new InvalidEnumArgumentException("styleSimulations", (int)styleSimulations, typeof(StyleSimulations)); 
            _styleSimulations = styleSimulations; 

            _initializationState = InitializationState.IsInitialized; // fully initialized 
        }

    #endregion Constructors
 
        //------------------------------------------------------
        // 
        //  Public Methods 
        //
        //----------------------------------------------------- 

    #region Public Methods

        /// <summary> 
        /// Return hash code for this GlyphTypeface.
        /// </summary> 
        /// <returns>Hash code.</returns> 
        /// <SecurityNote>
        /// Critical - as this accesses _originalUri. 
        /// Safe - as this only does this to compute the hash code.
        /// </SecurityNote>
        [SecurityCritical]
        public override int GetHashCode() 
        {
            CheckInitialized(); 
            return _originalUri.Value.GetHashCode() ^ (int)StyleSimulations; 
        }
 
        /// <summary>
        /// Compares this GlyphTypeface with another object.
        /// </summary>
        /// <param name="o">Object to compare with.</param> 
        /// <returns>Whether this object is equal to the input object.</returns>
        /// <SecurityNote> 
        /// Critical - as this accesses _originalUri. 
        /// Safe - as this only does this to perform a comparison with another object.
        /// </SecurityNote> 
        [SecurityCritical]
        public override bool Equals(object o)
        {
            CheckInitialized(); 
            GlyphTypeface t = o as GlyphTypeface;
            if (t == null) 
                return false; 

            return StyleSimulations == t.StyleSimulations 
                && _originalUri.Value == t._originalUri.Value;
        }

        /// <summary> 
        /// Returns a geometry describing the path for a single glyph in the font.
        /// The path represents the glyph 
        /// without grid fitting applied for rendering at a specific resolution. 
        /// </summary>
        /// <param name="glyphIndex">Index of the glyph to get outline for.</param> 
        /// <param name="renderingEmSize">Font size in drawing surface units.</param>
        /// <param name="hintingEmSize">Size to hint for in points.</param>
        [CLSCompliant(false)] 
        public Geometry GetGlyphOutline(ushort glyphIndex, double renderingEmSize, double hintingEmSize)
        { 
            CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
            return ComputeGlyphOutline(glyphIndex, false, renderingEmSize, hintingEmSize);
        } 

        /// <summary>
        /// Returns the binary image of font subset.
        /// </summary> 
        /// <param name="glyphs">Collection of glyph indices to be included into the subset.</param>
        /// <returns>Binary image of font subset.</returns> 
        /// <remarks> 
        ///     Callers must have UnmanagedCode permission to call this API.
        ///     Callers must have FileIOPermission or WebPermission to font location to call this API. 
        /// </remarks>
        /// <SecurityNote>
        ///     Critical - returns raw font data.
        ///     Safe - (1) unmanaged code demand.  This ensures PT callers can't directly access the TrueType subsetter in V1. 
        ///            (2) fileIO or web permission demand for location of font.  This ensures that even brokered access
        ///                    via print dialog (which asserts unmanaged code) only succeeds if user has access to font source location. 
        /// </SecurityNote> 
        [SecurityCritical]
        [CLSCompliant(false)] 
        public byte[] ComputeSubset(ICollection<ushort> glyphs)
        {
            SecurityHelper.DemandUnmanagedCode();
            DemandPermissionsForFontInformation(); 
            CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
 
            if (glyphs == null) 
                throw new ArgumentNullException("glyphs");
 
            if (glyphs.Count <= 0)
                throw new ArgumentException(SR.Get(SRID.CollectionNumberOfElementsMustBeGreaterThanZero), "glyphs");

            if (glyphs.Count > ushort.MaxValue) 
                throw new ArgumentException(SR.Get(SRID.CollectionNumberOfElementsMustBeLessOrEqualTo, ushort.MaxValue), "glyphs");
 
            UnmanagedMemoryStream pinnedFontSource = FontSource.GetUnmanagedStream(); 

            try 
            {
                TrueTypeFontDriver trueTypeDriver = new TrueTypeFontDriver(pinnedFontSource, _originalUri.Value);
                trueTypeDriver.SetFace(FaceIndex);
 
                return trueTypeDriver.ComputeFontSubset(glyphs);
            } 
            catch (SEHException e) 
            {
                throw Util.ConvertInPageException(FontSource, e); 
            }
            finally
            {
                pinnedFontSource.Close(); 
            }
        } 
 
        /// <summary>
        /// Returns a font file stream represented by this GlyphTypeface. 
        /// </summary>
        /// <returns>A font file stream represented by this GlyphTypeface.</returns>
        /// <SecurityNote>
        ///     Critical - returns raw font data. 
        ///     Safe - does a demand before it gives out the information asked.
        /// </SecurityNote> 
        [SecurityCritical] 
        public Stream GetFontStream()
        { 
            CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
            DemandPermissionsForFontInformation();
            return FontSource.GetStream();
        } 

        /// <summary> 
        /// Exposed to allow printing code to access GetFontStream() in partial trust 
        /// </summary>
        /// <SecurityNote> 
        ///     Critical - returns a permission allowing access to GetFontStream in partial trust.
        ///                Caller must make sure there is no font data leak
        /// </SecurityNote>
        [FriendAccessAllowed] 
        internal CodeAccessPermission CriticalFileReadPermission
        { 
            [SecurityCritical] 
            get
            { 
                CheckInitialized();
                return _fileIOPermObj.Value;
            }
        } 

        /// <summary> 
        /// Exposed to allow printing code to access FontUri in partial trust 
        /// </summary>
        /// <SecurityNote> 
        ///     Critical - returns a permission allowing access to FontUri
        ///                Caller must make sure there is no data leak
        /// </SecurityNote>
        [FriendAccessAllowed] 
        internal CodeAccessPermission CriticalUriDiscoveryPermission
        { 
            [SecurityCritical] 
            get
            { 
                CheckInitialized();
                return SecurityHelper.CreateUriDiscoveryPermission(_originalUri.Value);
            }
        } 

    #endregion Public Methods 
 
        //------------------------------------------------------
        // 
        //  Public Properties
        //
        //------------------------------------------------------
 
    #region Public Properties
 
        /// <summary> 
        /// Returns the original Uri of this glyph typeface object.
        /// </summary> 
        /// <value>The Uri glyph typeface was constructed with.</value>
        /// <remarks>
        ///     Callers must have FileIOPermission(FileIOPermissionAccess.PathDiscovery) for the given Uri to call this API.
        /// </remarks> 
        /// <SecurityNote>
        /// Critical - as this obtains Uri that can reveal local file system information. 
        /// Safe - as this does a discovery demand before it gives out the information asked. 
        /// </SecurityNote>
        public Uri FontUri 
        {
            [SecurityCritical]
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                SecurityHelper.DemandUriDiscoveryPermission(_originalUri.Value); 
                return _originalUri.Value; 
            }
            [SecurityCritical] 
            set
            {
                CheckInitializing(); // This can only be called in initialization
 
                if (value == null)
                    throw new ArgumentNullException("value"); 
 
                if (!value.IsAbsoluteUri)
                    throw new ArgumentException(SR.Get(SRID.UriNotAbsolute), "value"); 

                _originalUri = new SecurityCriticalDataClass<Uri>(value);
            }
        } 

        /// <summary> 
        /// This property is indexed by a Culture Identifier. 
        /// It returns the family name in the specified language, or,
        /// if the font does not provide a name for the specified language, 
        /// it returns the family name in English.
        /// The family name excludes weight, style and stretch.
        /// </summary>
        public IDictionary<CultureInfo,string> FamilyNames 
        {
            get 
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return _fontFace.GetFamilyNameDictionary(); 
            }
        }

        /// <summary> 
        /// This property is indexed by a Culture Identifier.
        /// It returns the face name in the specified language, or, 
        /// if the font does not provide a name for the specified language, 
        /// it returns the face name in English.
        /// The face name may identify weight, style and/or stretch. 
        /// </summary>
        public IDictionary<CultureInfo, string> FaceNames
        {
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return _fontFace.GetFaceNameDictionary(); 
            }
        } 

        /// <summary>
        /// This property is indexed by a Culture Identifier.
        /// It returns the family name in the specified language, or, 
        /// if the font does not provide a name for the specified language,
        /// it returns the family name in English. 
        /// The Win32FamilyName name excludes regular or bold weights and style, 
        /// but includes other weights and stretch.
        /// </summary> 
        public IDictionary<CultureInfo, string> Win32FamilyNames
        {
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return _fontFace.GetWin32FamilyNameDictionary(); 
            } 
        }
 
        /// <summary>
        /// This property is indexed by a Culture Identifier.
        /// It returns the face name in the specified language, or,
        /// if the font does not provide a name for the specified language, 
        /// it returns the face name in English.
        /// The face name may identify weight, style and/or stretch. 
        /// </summary> 
        IDictionary<XmlLanguage, string> ITypefaceMetrics.AdjustedFaceNames
        { 
            get
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                IDictionary<CultureInfo, string> adjustedFaceNames = _fontFace.GetAdjustedFaceNameDictionary(); 
                IDictionary<XmlLanguage, string> adjustedLanguageFaceNames = new Dictionary<XmlLanguage, string>(adjustedFaceNames.Count);
 
                foreach (KeyValuePair<CultureInfo, string> pair in adjustedFaceNames) 
                {
                    adjustedLanguageFaceNames[XmlLanguage.GetLanguage(pair.Key.IetfLanguageTag)] = pair.Value; 
                }

                if (_styleSimulations != StyleSimulations.None)
                { 
                    adjustedLanguageFaceNames = FontDifferentiator.AppendSimulationsToFaceNames(adjustedLanguageFaceNames, _styleSimulations);
                } 
                return adjustedLanguageFaceNames; 
            }
        } 

        /// <summary>
        /// This property is indexed by a Culture Identifier.
        /// It returns the face name in the specified language, or, 
        /// if the font does not provide a name for the specified language,
        /// it returns the face name in English. 
        /// The Win32Face name may identify weights other than regular or bold and/or style, 
        /// but may not identify stretch or other weights.
        /// </summary> 
        public IDictionary<CultureInfo, string> Win32FaceNames
        {
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return _fontFace.GetWin32FaceNameDictionary(); 
            } 
        }
 
        /// <summary>
        /// This property is indexed by a Culture Identifier.
        /// Version string in the fonts NAME table.
        /// Version strings vary significantly in format - to obtain the version 
        /// as a numeric value use the 'Version' property,
        /// do not attempt to parse the VersionString. 
        /// </summary> 
        /// <SecurityNote>
        /// Critical - as this accesses _fontFace which can reveal Windows font information. 
        /// Safe - as this does a demand before it gives out the information asked.
        /// </SecurityNote>
        public IDictionary<CultureInfo, string> VersionStrings
        { 
            [SecurityCritical]
            get 
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                DemandPermissionsForFontInformation(); 
                return _fontFace.GetVersionStringDictionary();
            }
        }
 
        /// <summary>
        /// This property is indexed by a Culture Identifier. 
        /// Copyright notice. 
        /// </summary>
        /// <SecurityNote> 
        /// Critical - as this accesses _fontFace which can reveal Windows font information.
        /// Safe - as this does a demand before it gives out the information asked.
        /// </SecurityNote>
        public IDictionary<CultureInfo, string> Copyrights 
        {
            [SecurityCritical] 
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                DemandPermissionsForFontInformation();
                return _fontFace.GetCopyrightDictionary();
            }
        } 

        /// <summary> 
        /// This property is indexed by a Culture Identifier. 
        /// Manufacturer Name.
        /// </summary> 
        /// <SecurityNote>
        /// Critical - as this accesses _fontFace which can reveal Windows font information.
        /// Safe - as this does a demand before it gives out the information asked.
        /// </SecurityNote> 
        public IDictionary<CultureInfo, string> ManufacturerNames
        { 
            [SecurityCritical] 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                DemandPermissionsForFontInformation();
                return _fontFace.GetManufacturerNameDictionary();
            } 
        }
 
        /// <summary> 
        /// This property is indexed by a Culture Identifier.
        /// This is used to save any trademark notice/information for this font. 
        /// Such information should be based on legal advice.
        /// This is distinctly separate from the copyright.
        /// </summary>
        /// <SecurityNote> 
        /// Critical - as this accesses _fontFace which can reveal Windows font information.
        /// Safe - as this does a demand before it gives out the information asked. 
        /// </SecurityNote> 
        public IDictionary<CultureInfo, string> Trademarks
        { 
            [SecurityCritical]
            get
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                DemandPermissionsForFontInformation();
                return _fontFace.GetTrademarkDictionary(); 
            } 
        }
 
        /// <summary>
        /// This property is indexed by a Culture Identifier.
        /// Name of the designer of the typeface.
        /// </summary> 
        /// <SecurityNote>
        /// Critical - as this accesses _fontFace which can reveal Windows font information. 
        /// Safe - as this does a demand before it gives out the information asked. 
        /// </SecurityNote>
        public IDictionary<CultureInfo, string> DesignerNames 
        {
            [SecurityCritical]
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                DemandPermissionsForFontInformation(); 
                return _fontFace.GetDesignerNameDictionary(); 
            }
        } 

        /// <summary>
        /// This property is indexed by a Culture Identifier.
        /// Description of the typeface. Can contain revision information, 
        /// usage recommendations, history, features, etc.
        /// </summary> 
        /// <SecurityNote> 
        /// Critical - as this accesses _fontFace which can reveal Windows font information.
        /// Safe - as this does a demand before it gives out the information asked. 
        /// </SecurityNote>
        public IDictionary<CultureInfo, string> Descriptions
        {
            [SecurityCritical] 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                DemandPermissionsForFontInformation();
                return _fontFace.GetDescriptionDictionary(); 
            }
        }

        /// <summary> 
        /// This property is indexed by a Culture Identifier.
        /// URL of font vendor (with protocol, e.g., `http://, `ftp://). 
        /// If a unique serial number is embedded in the URL, 
        /// it can be used to register the font.
        /// </summary> 
        /// <SecurityNote>
        /// Critical - as this accesses _fontFace which can reveal Windows font information.
        /// Safe - as this does a demand before it gives out the information asked.
        /// </SecurityNote> 
        public IDictionary<CultureInfo, string> VendorUrls
        { 
            [SecurityCritical] 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                DemandPermissionsForFontInformation();
                return _fontFace.GetVendorUrlDictionary();
            } 
        }
 
        /// <summary> 
        /// This property is indexed by a Culture Identifier.
        /// URL of typeface designer (with protocol, e.g., `http://, `ftp://). 
        /// </summary>
        /// <SecurityNote>
        /// Critical - as this accesses _fontFace which can reveal Windows font information.
        /// Safe - as this does a demand before it gives out the information asked. 
        /// </SecurityNote>
        public IDictionary<CultureInfo, string> DesignerUrls 
        { 
            [SecurityCritical]
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                DemandPermissionsForFontInformation();
                return _fontFace.GetDesignerUrlDictionary(); 
            }
        } 
 
        /// <summary>
        /// This property is indexed by a Culture Identifier. 
        /// Description of how the font may be legally used,
        /// or different example scenarios for licensed use.
        /// This field should be written in plain language, not legalese.
        /// </summary> 
        /// <SecurityNote>
        /// Critical - as this accesses _fontFace which can reveal Windows font information. 
        /// Safe - as this does a demand before it gives out the information asked. 
        /// </SecurityNote>
        public IDictionary<CultureInfo, string> LicenseDescriptions 
        {
            [SecurityCritical]
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                DemandPermissionsForFontInformation(); 
                return _fontFace.GetLicenseDescriptionDictionary(); 
            }
        } 

        /// <summary>
        /// This property is indexed by a Culture Identifier.
        /// This can be the font name, or any other text that the designer 
        /// thinks is the best sample to display the font in.
        /// </summary> 
        /// <SecurityNote> 
        /// Critical - as this accesses _fontFace which can reveal Windows font information.
        /// Safe - as this does a demand before it gives out the information asked. 
        /// </SecurityNote>
        public IDictionary<CultureInfo, string> SampleTexts
        {
            [SecurityCritical] 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                DemandPermissionsForFontInformation();
                return _fontFace.GetSampleTextDictionary(); 
            }
        }

        /// <summary> 
        /// Returns designed style (regular/italic/oblique) of this font face
        /// </summary> 
        /// <value>Designed style of this font face.</value> 
        public FontStyle Style
        { 
            get
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return _fontFace.Style; 
            }
        } 
 
        /// <summary>
        /// Returns designed weight of this font face. 
        /// </summary>
        /// <value>Designed weight of this font face.</value>
        public FontWeight Weight
        { 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return _fontFace.Weight;
            } 
        }

        /// <summary>
        /// Returns designed stretch of this font face. 
        /// </summary>
        /// <value>Designed stretch of this font face.</value> 
        public FontStretch Stretch 
        {
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return _fontFace.Stretch;
            } 
        }
 
        /// <summary> 
        /// Font face version interpreted from the font's 'NAME' table.
        /// </summary> 
        /// <SecurityNote>
        /// Critical - as this accesses _fontFace which can reveal Windows font information.
        /// Safe - as this does a demand before it gives out the information asked.
        /// </SecurityNote> 
        public double Version
        { 
            [SecurityCritical] 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                DemandPermissionsForFontInformation();
                return _fontFace.Version;
            } 
        }
 
        /// <summary> 
        /// Height of character cell relative to em size.
        /// </summary> 
        public double Height
        {
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return (double)(_fontFace.DesignCellAscent + _fontFace.DesignCellDescent) / DesignEmHeight; 
            } 
        }
 
        /// <summary>
        /// Distance from cell top to English baseline relative to em size.
        /// </summary>
        public double Baseline 
        {
            get 
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return (double)_fontFace.DesignCellAscent / DesignEmHeight; 
            }
        }

        /// <summary> 
        /// Distance from baseline to top of English capital, relative to em size.
        /// </summary> 
        public double CapsHeight 
        {
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return (double)_fontFace.CapsHeight / DesignEmHeight;
            } 
        }
 
        /// <summary> 
        /// Western x-height relative to em size.
        /// </summary> 
        public double XHeight
        {
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return (double)_fontFace.xHeight / DesignEmHeight; 
            } 
        }
 
        /// <summary>
        /// Returns true if this font does not conform to Unicode encoding:
        /// it may be considered as a simple collection of symbols indexed by a codepoint.
        /// </summary> 
        public bool Symbol
        { 
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return _fontFace.Symbol;
            }
        }
 
        /// <summary>
        /// Position of underline relative to baseline relative to em size. 
        /// The value is usually negative, to place the underline below the baseline. 
        /// </summary>
        public double UnderlinePosition 
        {
            get
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return (double)_fontFace.UnderlinePosition / DesignEmHeight;
            } 
        } 

        /// <summary> 
        /// Thickness of underline relative to em size.
        /// </summary>
        public double UnderlineThickness
        { 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return (double)_fontFace.UnderlineThickness / DesignEmHeight;
            } 
        }

        /// <summary>
        /// Position of strikeThrough relative to baseline relative to em size. 
        /// The value is usually positive, to place the Strikethrough above the baseline.
        /// </summary> 
        public double StrikethroughPosition 
        {
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return (double)_fontFace.StrikethroughPosition / DesignEmHeight;
            } 
        }
 
        /// <summary> 
        /// Thickness of Strikethrough relative to em size.
        /// </summary> 
        public double StrikethroughThickness
        {
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return (double)_fontFace.StrikethroughThickness / DesignEmHeight; 
            } 
        }
 
        /// <summary>
        /// EmbeddingRights property describes font embedding permissions
        /// specified in this glyph typeface.
        /// </summary> 
        /// <SecurityNote>
        /// Critical - as this accesses _fontFace which can reveal Windows font information. 
        /// Safe - as this does a demand before it gives out the information asked. 
        /// </SecurityNote>
        public FontEmbeddingRight EmbeddingRights 
        {
            [SecurityCritical]
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                DemandPermissionsForFontInformation(); 
                return _fontFace.EmbeddingRights; 
            }
        } 

    #region ITypefaceMetrics implementation

        /// <summary> 
        /// Distance from baseline to top of English capital, relative to em size.
        /// </summary> 
        double ITypefaceMetrics.CapsHeight 
        {
            get 
            {
                return CapsHeight;
            }
        } 

        /// <summary> 
        /// Western x-height relative to em size. 
        /// </summary>
        double ITypefaceMetrics.XHeight 
        {
            get
            {
                return XHeight; 
            }
        } 
 
        /// <summary>
        /// Returns true if this font does not conform to Unicode encoding: 
        /// it may be considered as a simple collection of symbols indexed by a codepoint.
        /// </summary>
        bool ITypefaceMetrics.Symbol
        { 
            get
            { 
                return Symbol; 
            }
        } 

        /// <summary>
        /// Position of underline relative to baseline relative to em size.
        /// The value is usually negative, to place the underline below the baseline. 
        /// </summary>
        double ITypefaceMetrics.UnderlinePosition 
        { 
            get
            { 
                return UnderlinePosition;
            }
        }
 
        /// <summary>
        /// Thickness of underline relative to em size. 
        /// </summary> 
        double ITypefaceMetrics.UnderlineThickness
        { 
            get
            {
                return UnderlineThickness;
            } 
        }
 
        /// <summary> 
        /// Position of strikeThrough relative to baseline relative to em size.
        /// The value is usually positive, to place the Strikethrough above the baseline. 
        /// </summary>
        double ITypefaceMetrics.StrikethroughPosition
        {
            get 
            {
                return StrikethroughPosition; 
            } 
        }
 
        /// <summary>
        /// Thickness of Strikethrough relative to em size.
        /// </summary>
        double ITypefaceMetrics.StrikethroughThickness 
        {
            get 
            { 
                return StrikethroughThickness;
            } 
        }

    #endregion
 

        // The next several properties return non CLS-compliant types.  This is 
        // tracked by bug 1792236.  For now, suppress the compiler warning. 
        //
#pragma warning disable 3003 

        /// <summary>
        /// Returns advance width for a given glyph.
        /// </summary> 
        public IDictionary<ushort, double> AdvanceWidths
        { 
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return new GlyphIndexer(this.GetAdvanceWidth, _fontFace.GlyphCount);
            }
        }
 
        /// <summary>
        /// Returns Advance height for a given glyph (Used for example in vertical layout). 
        /// </summary> 
        public IDictionary<ushort, double> AdvanceHeights
        { 
            get
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return new GlyphIndexer(this.GetAdvanceHeight, _fontFace.GlyphCount); 
            }
        } 
 
        /// <summary>
        /// Distance from leading end of advance vector to left edge of black box. 
        /// Positive when left edge of black box is within the alignment rectangle
        /// defined by the advance width and font cell height.
        /// Negative when left edge of black box overhangs the alignment rectangle.
        /// </summary> 
        public IDictionary<ushort, double> LeftSideBearings
        { 
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return new GlyphIndexer(this.GetLeftSidebearing, _fontFace.GlyphCount);
            }
        }
 
        /// <summary>
        /// Distance from right edge of black box to right end of advance vector. 
        /// Positive when trailing edge of black box is within the alignment rectangle 
        /// defined by the advance width and font cell height.
        /// Negative when right edge of black box overhangs the alignment rectangle. 
        /// </summary>
        public IDictionary<ushort, double> RightSideBearings
        {
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return new GlyphIndexer(this.GetRightSidebearing, _fontFace.GlyphCount); 
            }
        } 

        /// <summary>
        /// Distance from top end of (vertical) advance vector to top edge of black box.
        /// Positive when top edge of black box is within the alignment rectangle 
        /// defined by the advance height and font cell height.
        /// (The font cell height is a horizontal dimension in vertical layout). 
        /// Negative when top edge of black box overhangs the alignment rectangle. 
        /// </summary>
        public IDictionary<ushort, double> TopSideBearings 
        {
            get
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return new GlyphIndexer(this.GetTopSidebearing, _fontFace.GlyphCount);
            } 
        } 

        /// <summary> 
        /// Distance from bottom edge of black box to bottom end of advance vector.
        /// Positive when bottom edge of black box is within the alignment rectangle
        /// defined by the advance width and font cell height.
        /// (The font cell height is a horizontal dimension in vertical layout). 
        /// Negative when bottom edge of black box overhangs the alignment rectangle.
        /// </summary> 
        public IDictionary<ushort, double> BottomSideBearings 
        {
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return new GlyphIndexer(this.GetBottomSidebearing, _fontFace.GlyphCount);
            } 
        }
 
        /// <summary> 
        /// Offset down from horizontal Western baseline to bottom  of glyph black box.
        /// </summary> 
        public IDictionary<ushort, double> DistancesFromHorizontalBaselineToBlackBoxBottom
        {
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return new GlyphIndexer(this.GetBaseline, _fontFace.GlyphCount); 
            } 
        }
 
        /// <summary>
        /// Returns nominal mapping of Unicode codepoint to glyph index as defined by the font 'CMAP' table.
        /// </summary>
        /// <SecurityNote> 
        ///   Critical: May potentially leak a writeable cmap.
        ///   Safe: The cmap IDictionary exposure is read only. 
        ///  </SecurityNote> 
        public IDictionary<int, ushort> CharacterToGlyphMap
        { 
            [SecurityCritical]
            get
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return _fontFace.CharacterMap;
            } 
        } 

#pragma warning restore 3003 

        /// <summary>
        /// Returns algorithmic font style simulations to be applied to the GlyphTypeface.
        /// </summary> 
        public StyleSimulations StyleSimulations
        { 
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return _styleSimulations;
            }
            set
            { 
                CheckInitializing();
                _styleSimulations = value; 
            } 
        }
 
        /// <summary>
        /// Obtains the number of glyphs in the glyph typeface.
        /// </summary>
        /// <value>The number of glyphs in the glyph typeface.</value> 
        public int GlyphCount
        { 
            get 
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return _fontFace.GlyphCount;
            }
        }
 
    #endregion Public Properties
 
        //----------------------------------------------------- 
        //
        //  Internal Methods 
        //
        //------------------------------------------------------

    #region Internal Methods 

        /// <summary> 
        /// Returns the nominal advance width for a glyph. 
        /// </summary>
        /// <param name="glyph">Glyph index in the font.</param> 
        /// <returns>The nominal advance width for the glyph relative to the em size of the font.</returns>
        /// <SecurityNote>
        /// Critical - as this has unsafe block.
        /// Safe - as this only gives width information which is safe to give out. 
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe] 
        internal double GetAdvanceWidth(ushort glyph) 
        {
            CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 

            // We manually expand GetGlyphMetrics call because GetAdvanceWidth is a very frequently used function.
            // When we get to using GetAdvanceHeight for vertical writing, we need to consider doing the same optimization there.
            unsafe 
            {
                FontFaceLayoutInfo.GlyphMetrics * cachedGlyphMetrics = _fontFace.Metrics(glyph); 
 
                double aw = (double)cachedGlyphMetrics->advanceWidth / DesignEmHeight;
 
                if ((_styleSimulations & StyleSimulations.BoldSimulation) != 0)
                {
                    // Bold simulation increases advance width and advance height by 2% of em size,
                    // except for glyphs that are empty or have zero advance widths. 
                    // So, we compensate for the simulation when aw != 0 && left < right && ah != 0 && bottom > top
                    if (cachedGlyphMetrics->advanceWidth != 0 && 
                        cachedGlyphMetrics->lsb < cachedGlyphMetrics->advanceWidth - cachedGlyphMetrics->rsb && 
                        cachedGlyphMetrics->advanceHeight != 0 &&
                        cachedGlyphMetrics->advanceHeight - cachedGlyphMetrics->tsb - cachedGlyphMetrics->bsb > 0) 
                    {
                        aw += 0.02;
                    }
                } 
                return aw;
            } 
        } 

        /// <summary> 
        /// Returns the nominal advance width for a glyph in font design units.
        /// </summary>
        /// <param name="glyph">Glyph index in the font.</param>
        /// <returns>The nominal advance width for the glyph in font design units.</returns> 
        internal double GetAdvanceWidthInDesignUnits(ushort glyph)
        { 
            return GetAdvanceWidth(glyph) * DesignEmHeight; 
        }
 

        /// <SecurityNote>
        /// This function will demand appropriate permissions depending on what
        /// the source of the font information is.  The value of _fileIOPermObj 
        /// is set correctly whenever _originalUri gets set.
        /// </SecurityNote> 
        internal void DemandPermissionsForFontInformation() 
        {
            if (_fileIOPermObj.Value != null) 
            {
                _fileIOPermObj.Value.Demand();
            }
        } 

        private double GetAdvanceHeight(ushort glyph) 
        { 
            double aw, ah, lsb, rsb, tsb, bsb, baseline;
            GetGlyphMetrics( 
                glyph,
                1.0,
                out aw,
                out ah, 
                out lsb,
                out rsb, 
                out tsb, 
                out bsb,
                out baseline 
            );
            return ah;
        }
 
        private double GetLeftSidebearing(ushort glyph)
        { 
            return (double)_fontFace.GetLeftSidebearing(glyph) / DesignEmHeight; 
        }
 
        private double GetRightSidebearing(ushort glyph)
        {
            return (double)_fontFace.GetRightSidebearing(glyph) / DesignEmHeight;
        } 

        private double GetTopSidebearing(ushort glyph) 
        { 
            return (double)_fontFace.GetTopSidebearing(glyph) / DesignEmHeight;
        } 

        private double GetBottomSidebearing(ushort glyph)
        {
            return (double)_fontFace.GetBottomSidebearing(glyph) / DesignEmHeight; 
        }
 
        private double GetBaseline(ushort glyph) 
        {
            return (double)_fontFace.GetBaseline(glyph) / DesignEmHeight; 
        }

        /// <summary>
        /// Optimized version of obtaining all of glyph metrics from font cache at once 
        /// without repeated checks and divisions.
        /// </summary> 
        /// <SecurityNote> 
        /// Critical - as this uses unsafe code.
        /// Safe - as this only gives information which is safe to give out. 
        /// </SecurityNote>
        [SecurityCritical, SecurityTreatAsSafe]
        internal void GetGlyphMetrics(
            ushort      glyph, 
            double      renderingEmSize,
            out double  aw, 
            out double  ah, 
            out double  lsb,
            out double  rsb, 
            out double  tsb,
            out double  bsb,
            out double  baseline
            ) 
        {
            CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
 
            unsafe
            { 
                FontFaceLayoutInfo.GlyphMetrics * cachedGlyphMetrics = _fontFace.Metrics(glyph);

                double designToEm = renderingEmSize / DesignEmHeight;
 
                aw = designToEm * cachedGlyphMetrics->advanceWidth;
                ah = designToEm * cachedGlyphMetrics->advanceHeight; 
                lsb = designToEm * cachedGlyphMetrics->lsb; 
                rsb = designToEm * cachedGlyphMetrics->rsb;
                tsb = designToEm * cachedGlyphMetrics->tsb; 
                bsb = designToEm * cachedGlyphMetrics->bsb;
                baseline = designToEm * cachedGlyphMetrics->baseline;

                if ((_styleSimulations & StyleSimulations.BoldSimulation) != 0) 
                {
                    // Bold simulation increases advance width and advance height by 2% of em size, 
                    // except for glyphs that are empty or have zero advance widths. 
                    // So, we compensate for the simulation when aw != 0 && left < right && ah != 0 && bottom > top
                    if (cachedGlyphMetrics->advanceWidth != 0 && 
                        cachedGlyphMetrics->lsb < cachedGlyphMetrics->advanceWidth - cachedGlyphMetrics->rsb &&
                        cachedGlyphMetrics->advanceHeight != 0 &&
                        cachedGlyphMetrics->advanceHeight - cachedGlyphMetrics->tsb - cachedGlyphMetrics->bsb > 0)
                    { 
                        aw += 0.02 * renderingEmSize;
                        ah += 0.02 * renderingEmSize; 
                    } 
                }
            } 
        }

        /// <summary>
        /// Returns a geometry describing the path for a single glyph in the font. 
        /// The path represents the glyph
        /// without grid fitting applied for rendering at a specific resolution. 
        /// </summary> 
        /// <param name="glyphIndex">Index of the glyph to get outline for.</param>
        /// <param name="sideways">Specifies whether the glyph should be rotated sideways.</param> 
        /// <param name="renderingEmSize">Font size in drawing surface units.</param>
        /// <param name="hintingEmSize">Size to hint for in points.</param>
        /// <returns>Geometry containing glyph outline.</returns>
        /// <SecurityCritical> 
        /// Critical - as this calls GetGlyphs() which is critical.
        /// Safe - as this doesn't expose font information but just gives out a Geometry. 
        /// </SecurityCritical> 
        [SecurityCritical, SecurityTreatAsSafe]
        internal Geometry ComputeGlyphOutline(ushort glyphIndex, bool sideways, double renderingEmSize, double hintingEmSize) 
        {
            CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
            using (GlyphPathElement pathElement = new GlyphPathElement(
                _fontFace.FaceIndex, 
                _fontFace.FontSource,
                GetRenderingFlags(sideways), 
                DesignEmHeight 
            ))
            { 
                PathGeometry pathGeometry;
                unsafe
                {
                    void*[] glyphOutlines = new void*[1]; 
                    FontCacheAccessor fontCacheAccessor = new FontCacheAccessor();
                    ushort[] glyphIndices = new ushort[1] { glyphIndex }; 
                    fontCacheAccessor.GetGlyphs( 
                        pathElement,
                        glyphIndices, 
                        glyphOutlines
                    );

                    void * outline = glyphOutlines[0]; 

                    Debug.Assert(outline != null); 
 
                    if (FontTechnology == FontTechnology.PostscriptOpenType)
                        ConvertPostscriptOutline(outline, renderingEmSize, sideways, out pathGeometry); 
                    else
                        ConvertTrueTypeOutline(outline, renderingEmSize, sideways, out pathGeometry);

                    // Make sure fontCacheAccessor doesn't go out of scope before the outlines get converted. 
                    GC.KeepAlive(fontCacheAccessor);
                } 
 
                // Make sure to always return Geometry.Empty from public methods for empty geometries.
                if (pathGeometry == null || pathGeometry.IsEmpty()) 
                    return Geometry.Empty;
                return pathGeometry;
            }
        } 

        /// <SecurityNote> 
        /// Critical - unsafe code, accepts pointer parameters, etc. 
        /// </SecurityNote>
        [SecurityCritical] 
        private unsafe void ConvertTrueTypeOutline(void* trueTypeOutline, double renderingEmSize, bool sideways, out PathGeometry pathGeometry)
        {
            GlyphPathData * outline = (GlyphPathData *)trueTypeOutline;
 
            // scale factor from design units to user coordinate system
 
            double designToUserScale = renderingEmSize / DesignEmHeight; 
            Matrix designToUser = new Matrix(designToUserScale, 0, 0, -designToUserScale, 0, 0);
            if (sideways) 
            {
                designToUser.Rotate(-90.0);
                designToUser.Translate(outline->verOriginY * designToUserScale, outline->verOriginX * designToUserScale);
            } 

            ushort* endp = GlyphPathData.EndPointNumbers(outline); 
            short* x = GlyphPathData.X(outline); 
            short* y = GlyphPathData.Y(outline);
            byte* flags = GlyphPathData.Flags(outline); 

            // k is the index of the first point of the current contour
            int k = 0;
 
            pathGeometry = null;
 
            // j is the index of the current contour 
            for (int j = 0; j < outline->numberOfContours; ++j)
            { 
                int lastPointIndex = endp[j];
                if (lastPointIndex <= k)
                {
                    k = lastPointIndex + 1; 
                    continue; //  empty contour
                } 
 
                Point startPoint;
 
                PathFigure figure = new PathFigure();

                // The first point on the curve
                if (OnCurve(flags[k])) 
                {
                    // Easy case 
                    startPoint = designToUser.Transform(new Point(x[k], y[k])); 
                    ++k;
                } 
                else
                {
                    // Is last contour point on the curve
                    if (OnCurve(flags[lastPointIndex])) 
                    {
                        // Make the last point the first point and decrement the last point 
                        startPoint = designToUser.Transform(new Point(x[lastPointIndex], y[lastPointIndex])); 
                        --lastPointIndex;
                    } 
                    else
                    {
                        // First and last point are off the countour, fake a mid point
                        Point firstPoint = designToUser.Transform(new Point(x[k], y[k])); 
                        Point lastPoint = designToUser.Transform(new Point(x[lastPointIndex], y[lastPointIndex]));
                        startPoint = new Point( 
                            (firstPoint.X + lastPoint.X) / 2, 
                            (firstPoint.Y + lastPoint.Y) / 2
                        ); 
                    }
                }

                figure.StartPoint = startPoint; 

                bool inBezier = false; 
                Point bezierB = new Point(); 
                while (k <= lastPointIndex)
                { 
                    Point currentPoint = designToUser.Transform(new Point(x[k], y[k]));

                    if (OnCurve(flags[k]))
                    { 
                        if (!inBezier)
                        { 
                            figure.Segments.Add(new LineSegment(currentPoint, true)); 
                        }
                        else 
                        {
                            figure.Segments.Add(new QuadraticBezierSegment(bezierB, currentPoint, true));
                            inBezier = false;
                        } 
                    }
                    else 
                    { 
                        if (inBezier)
                        { 
                            figure.Segments.Add(new QuadraticBezierSegment(
                                bezierB,
                                new Point(
                                    (bezierB.X + currentPoint.X) / 2, 
                                    (bezierB.Y + currentPoint.Y) / 2
                                ), 
                                true) 
                            );
                        } 
                        inBezier = true;
                        bezierB = currentPoint;
                    }
                    ++k; 
                }
 
                // explicitly set k to the start point of the next contour 
                // since in some cases lastPointIndex is not equal to endp[j]
                k = endp[j] + 1; 

                // close the figure, assume start point is always on curve
                if (inBezier)
                { 
                    figure.Segments.Add(new QuadraticBezierSegment(bezierB, startPoint, true));
                } 
 
                figure.IsClosed = true;
 
                if (pathGeometry == null)
                {
                    pathGeometry = new PathGeometry();
                    pathGeometry.FillRule = FillRule.Nonzero; 
                }
 
                pathGeometry.Figures.Add(figure); 
            }
        } 

        /// <SecurityNote>
        /// Critical - unsafe code, accepts pointer parameters, etc.
        /// </SecurityNote> 
        [SecurityCritical]
        private unsafe void ConvertPostscriptOutline(void * outline, double renderingEmSize, bool sideways, out PathGeometry pathGeometry) 
        { 
            int * postscriptOutline = (int *)outline;
 
            // scale factor from design units to user coordinate system

            double designToUserScale = renderingEmSize / DesignEmHeight;
            Matrix designToUser = new Matrix(designToUserScale, 0, 0, -designToUserScale, 0, 0); 
            if (sideways)
            { 
                int verOriginX = postscriptOutline[0]; 
                int verOriginY = postscriptOutline[1];
 
                designToUser.Rotate(-90.0);
                designToUser.Translate(verOriginY * designToUserScale, verOriginX * designToUserScale);
            }
 
            // Skip vertical origin and length to get to the actual contour data.
            int * p = postscriptOutline + 3; 
            Debug.Assert(postscriptOutline[2] % sizeof(int) == 0); 
            int * end = p + (postscriptOutline[2] / sizeof(int));
 
            pathGeometry = null;

            // Current figure.
            PathFigure figure = null; 

            for (;;) 
            { 
                if (p >= end)
                    break; 

                int tokenValue = *p;

                switch ((OutlineTokenType)tokenValue) 
                {
                case OutlineTokenType.MoveTo: 
                    { 
                        ++p;
                        if (p + 1 >= end) 
                            throw new FileFormatException(_originalUri.Value);

                        Point point = designToUser.Transform(
                            new Point(p[0] * CFFConversionFactor, p[1] * CFFConversionFactor) 
                            );
 
                        if (figure == null) 
                            figure = new PathFigure();
 
                        figure.StartPoint = point;

                        p += 2;
                        break; 
                    }
 
                case OutlineTokenType.LineTo: 
                    {
                        ++p; 
                        if (p + 1 >= end)
                            throw new FileFormatException(_originalUri.Value);

                        Point point = designToUser.Transform( 
                            new Point(p[0] * CFFConversionFactor, p[1] * CFFConversionFactor)
                            ); 
 
                        if (figure == null)
                            throw new FileFormatException(_originalUri.Value); 

                        figure.Segments.Add(new LineSegment(point, true));

                        p += 2; 
                        break;
                    } 
 
                case OutlineTokenType.CurveTo:
                    { 
                        ++p;
                        if (p + 5 >= end)
                            throw new FileFormatException(_originalUri.Value);
 
                        Point point0 = designToUser.Transform(
                            new Point(p[0] * CFFConversionFactor, p[1] * CFFConversionFactor) 
                            ); 
                        Point point1 = designToUser.Transform(
                            new Point(p[2] * CFFConversionFactor, p[3] * CFFConversionFactor) 
                            );
                        Point point2 = designToUser.Transform(
                            new Point(p[4] * CFFConversionFactor, p[5] * CFFConversionFactor)
                            ); 

                        if (figure == null) 
                            throw new FileFormatException(_originalUri.Value); 

                        figure.Segments.Add(new BezierSegment(point0, point1, point2, true)); 
                        p += 6;
                        break;
                    }
 
                case OutlineTokenType.ClosePath:
                    if (figure == null) 
                        throw new FileFormatException(_originalUri.Value); 

                    figure.IsClosed = true; 

                    if (pathGeometry == null)
                    {
                        pathGeometry = new PathGeometry(); 
                        pathGeometry.FillRule = FillRule.Nonzero;
                    } 
 
                    pathGeometry.Figures.Add(figure);
                    figure = null; 
                    ++p;
                    break;

                default: 
                    throw new FileFormatException(_originalUri.Value);
                } 
            } 
        }
 

        /// <summary>
        /// Get advance widths of unshaped characters
        /// </summary> 
        /// <param name="unsafeCharString">character string</param>
        /// <param name="stringLength">character length</param> 
        /// <param name="emSize">character em size</param> 
        /// <param name="advanceWidthsUnshaped">unshaped advance widths </param>
        /// <param name="nullFont">true if all characters map to missing glyph</param> 
        /// <returns>array of character advance widths</returns>
        /// <SecurityNote>
        /// Critical - takes unsafe char string and returns information in an unsafe int array
        /// </SecurityNote> 
        [SecurityCritical]
        internal unsafe void GetAdvanceWidthsUnshaped( 
            char* unsafeCharString, 
            int stringLength,
            double emSize, 
            int* advanceWidthsUnshaped,
            bool nullFont
            )
        { 
            CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
            Invariant.Assert(stringLength > 0); 
 
            if (!nullFont)
            { 
                IDictionary<int, ushort> cmap = CharacterToGlyphMap;
                for (int i = 0; i < stringLength; i++)
                {
                    ushort glyphIndex; 
                    cmap.TryGetValue(unsafeCharString[i], out glyphIndex);
                    advanceWidthsUnshaped[i] = (int)Math.Round(emSize * GetAdvanceWidth(glyphIndex)); 
                } 
            }
            else 
            {
                int missingGlyphWidth = (int)Math.Round(emSize * GetAdvanceWidth(0));
                for (int i = 0; i < stringLength; i++)
                { 
                    advanceWidthsUnshaped[i] = missingGlyphWidth;
                } 
            } 
        }
 
        /// <summary>
        /// Compute an unshaped glyphrun object from specified character-based info
        /// </summary>
        internal GlyphRun ComputeUnshapedGlyphRun( 
            Point origin,
            CharacterBufferRange charBufferRange, 
            IList<double> charWidths, 
            double emSize,
            double emHintingSize, 
            bool nullGlyph,
            CultureInfo cultureInfo,
            string deviceFontName
            ) 
        {
            Debug.Assert(charBufferRange.Length > 0); 
 
            CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
 
            ushort[] nominalGlyphs = new ushort[charBufferRange.Length];


            // compute glyph positions 

            if (nullGlyph) 
            { 
                for (int i = 0; i < charBufferRange.Length; i++)
                { 
                    nominalGlyphs[i] = 0;
                }
            }
            else 
            {
                IDictionary<int, ushort> cmap = CharacterToGlyphMap; 
 
                for (int i = 0; i < charBufferRange.Length; i++)
                { 
                    ushort glyphIndex;
                    cmap.TryGetValue(charBufferRange[i], out glyphIndex);
                    nominalGlyphs[i] = glyphIndex;
                } 
            }
 
            return GlyphRun.TryCreate( 
                this,
                0,      // bidiLevel 
                false,  // sideway
                emSize,
                nominalGlyphs,
                origin, 
                charWidths,
                null,   // glyphOffsets 
                new PartialList<char>(charBufferRange.CharacterBuffer, charBufferRange.OffsetToFirstChar, charBufferRange.Length), 
                deviceFontName,   // device font
                null,   // 1:1 mapping 
                null,   // caret stops at every codepoint
                XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag)
                );
        } 

 
    #endregion Internal Methods 

        //----------------------------------------------------- 
        //
        //  Internal Properties
        //
        //----------------------------------------------------- 

    #region Internal Properties 
 
        internal FontSource FontSource
        { 
            get
            {
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return _fontFace.FontSource; 
            }
        } 
 
        /// <summary>
        /// 0 for TTF files 
        /// Face index within TrueType font collection for TTC files
        /// </summary>
        internal int FaceIndex
        { 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return _fontFace.FaceIndex;
            } 
        }

        internal FontFaceLayoutInfo FontFaceLayoutInfo
        { 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return _fontFace;
            } 
        }

        internal ushort BlankGlyphIndex
        { 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return _fontFace.BlankGlyph;
            } 
        }

        internal FontFaceLayoutInfo.RenderingHints RenderingHints
        { 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return _fontFace.FontRenderingHints;
            } 
        }

        internal FontTechnology FontTechnology
        { 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                return _fontFace.FontTechnology;
            } 
        }

        internal short FontContrastAdjustment
        { 
            get
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface 
                Debug.Assert(-3 <= _fontFace.FontContrastAdjustment && _fontFace.FontContrastAdjustment <= 4);
                return _fontFace.FontContrastAdjustment; 
            }
        }

        internal ushort DesignEmHeight 
        {
            get 
            { 
                CheckInitialized(); // This can only be called on fully initialized GlyphTypeface
                return _fontFace.DesignEmHeight; 
            }
        }

    #endregion Internal Properties 

        //----------------------------------------------------- 
        // 
        //  Private Methods
        // 
        //------------------------------------------------------

    #region Private Methods
 
        private static bool OnCurve(byte flag)
        { 
            return (flag & 0x01) != 0; 
        }
 
        private ushort GetRenderingFlags(bool sideways)
        {
            ushort renderingFlags = 0;
            if ((_styleSimulations & StyleSimulations.BoldSimulation) != 0) 
                renderingFlags |= (ushort)RenderingFlags.BoldSimulation;
            if ((_styleSimulations & StyleSimulations.ItalicSimulation) != 0) 
            { 
                renderingFlags |= (ushort)RenderingFlags.ItalicSimulation;
                if (sideways) 
                    renderingFlags |= (ushort)RenderingFlags.SidewaysItalicSimulation;
            }
            if (FontTechnology != FontTechnology.PostscriptOpenType)
                renderingFlags |= (ushort)MIL_GLYPHRUN_FLAGS.MilGlyphRunIsTrueType; 
            return renderingFlags;
        } 
 
    #endregion Private Methods
 
    #region ISupportInitialize interface

        void ISupportInitialize.BeginInit()
        { 
            if (_initializationState == InitializationState.IsInitialized)
            { 
                // Cannont initialize a GlyphRun this is completely initialized. 
                throw new InvalidOperationException(SR.Get(SRID.OnlyOneInitialization));
            } 

            if (_initializationState == InitializationState.IsInitializing)
            {
                // Cannot initialize a GlyphRun this already being initialized. 
                throw new InvalidOperationException(SR.Get(SRID.InInitialization));
            } 
 
            _initializationState = InitializationState.IsInitializing;
        } 

        /// <SecurityNote>
        /// Critical - this method calls into critical method.
        /// </SecurityNote> 
        [SecurityCritical]
        void ISupportInitialize.EndInit() 
        { 
            if (_initializationState != InitializationState.IsInitializing)
            { 
                // Cannot EndInit a GlyphRun that is not being initialized.
                throw new InvalidOperationException(SR.Get(SRID.NotInInitialization));
            }
 
            Initialize(
                (_originalUri == null ? null : _originalUri.Value), 
                 _styleSimulations, 
                 true
                 ); 
        }

        private void CheckInitialized()
        { 
            if (_initializationState != InitializationState.IsInitialized)
            { 
                throw new InvalidOperationException(SR.Get(SRID.InitializationIncomplete)); 
            }
        } 

        private void CheckInitializing()
        {
            if (_initializationState != InitializationState.IsInitializing) 
            {
                throw new InvalidOperationException(SR.Get(SRID.NotInInitialization)); 
            } 
        }
 
    #endregion

        //-----------------------------------------------------
        // 
        //  Private Nested Classes
        // 
        //------------------------------------------------------ 

    #region Private Nested Classes 

        private delegate double GlyphAccessor(ushort glyphIndex);

        /// <summary> 
        /// This class is a helper to implement named indexers
        /// for glyph metrics. 
        /// </summary> 
        private class GlyphIndexer : IDictionary<ushort, double>
        { 
            internal GlyphIndexer(GlyphAccessor accessor, ushort numberOfGlyphs)
            {
                _accessor = accessor;
                _numberOfGlyphs = numberOfGlyphs; 
            }
 
    #region IDictionary<ushort,double> Members 

            public void Add(ushort key, double value) 
            {
                throw new NotSupportedException();
            }
 
            public bool ContainsKey(ushort key)
            { 
                return (key < _numberOfGlyphs); 
            }
 
            public ICollection<ushort> Keys
            {
                get { return new SequentialUshortCollection(_numberOfGlyphs); }
            } 

            public bool Remove(ushort key) 
            { 
                throw new NotSupportedException();
            } 

            public bool TryGetValue(ushort key, out double value)
            {
                if (ContainsKey(key)) 
                {
                    value = this[key]; 
                    return true; 
                }
                else 
                {
                    value = new double();
                    return false;
                } 
            }
 
            public ICollection<double> Values 
            {
                get { return new ValueCollection(this); } 
            }

            public double this[ushort key]
            { 
                get
                { 
                    return _accessor(key); 
                }
                set 
                {
                    throw new NotSupportedException();
                }
            } 

    #endregion 
 
    #region ICollection<KeyValuePair<ushort,double>> Members
 
            public void Add(KeyValuePair<ushort, double> item)
            {
                throw new NotSupportedException();
            } 

            public void Clear() 
            { 
                throw new NotSupportedException();
            } 

            public bool Contains(KeyValuePair<ushort, double> item)
            {
                return ContainsKey(item.Key); 
            }
 
            public void CopyTo(KeyValuePair<ushort, double>[] array, int arrayIndex) 
            {
                if (array == null) 
                {
                    throw new ArgumentNullException("array");
                }
 
                if (array.Rank != 1)
                { 
                    throw new ArgumentException(SR.Get(SRID.Collection_BadRank)); 
                }
 
                // The extra "arrayIndex >= array.Length" check in because even if _collection.Count
                // is 0 the index is not allowed to be equal or greater than the length
                // (from the MSDN ICollection docs)
                if (arrayIndex < 0 || arrayIndex >= array.Length || (arrayIndex + Count) > array.Length) 
                {
                    throw new ArgumentOutOfRangeException("arrayIndex"); 
                } 

                for (ushort i = 0; i < Count; ++i) 
                    array[arrayIndex + i] = new KeyValuePair<ushort, double>(i, this[i]);
            }

            public int Count 
            {
                get { return _numberOfGlyphs; } 
            } 

            public bool IsReadOnly 
            {
                get { return true; }
            }
 
            public bool Remove(KeyValuePair<ushort, double> item)
            { 
                throw new NotSupportedException(); 
            }
 
    #endregion

    #region IEnumerable<KeyValuePair<ushort,double>> Members
 
            public IEnumerator<KeyValuePair<ushort, double>> GetEnumerator()
            { 
                for (ushort i = 0; i < Count; ++i) 
                    yield return new KeyValuePair<ushort, double>(i, this[i]);
            } 

    #endregion

    #region IEnumerable Members 

            IEnumerator IEnumerable.GetEnumerator() 
            { 
                return ((IEnumerable<KeyValuePair<ushort, double>>)this).GetEnumerator();
            } 

    #endregion

            private class ValueCollection : ICollection<double> 
            {
                public ValueCollection(GlyphIndexer glyphIndexer) 
                { 
                    _glyphIndexer = glyphIndexer;
                } 

    #region ICollection<double> Members

                public void Add(double item) 
                {
                    throw new NotSupportedException(); 
                } 

                public void Clear() 
                {
                    throw new NotSupportedException();
                }
 
                public bool Contains(double item)
                { 
                    foreach (double d in this) 
                    {
                        if (d == item) 
                            return true;
                    }
                    return false;
                } 

                public void CopyTo(double[] array, int arrayIndex) 
                { 
                    if (array == null)
                    { 
                        throw new ArgumentNullException("array");
                    }

                    if (array.Rank != 1) 
                    {
                        throw new ArgumentException(SR.Get(SRID.Collection_BadRank)); 
                    } 

                    // The extra "arrayIndex >= array.Length" check in because even if _collection.Count 
                    // is 0 the index is not allowed to be equal or greater than the length
                    // (from the MSDN ICollection docs)
                    if (arrayIndex < 0 || arrayIndex >= array.Length || (arrayIndex + Count) > array.Length)
                    { 
                        throw new ArgumentOutOfRangeException("arrayIndex");
                    } 
 
                    for (ushort i = 0; i < Count; ++i)
                        array[arrayIndex + i] = _glyphIndexer[i]; 
                }

                public int Count
                { 
                    get { return _glyphIndexer._numberOfGlyphs; }
                } 
 
                public bool IsReadOnly
                { 
                    get { return true; }
                }

                public bool Remove(double item) 
                {
                    throw new NotSupportedException(); 
                } 

    #endregion 

    #region IEnumerable<double> Members

                public IEnumerator<double> GetEnumerator() 
                {
                    for (ushort i = 0; i < Count; ++i) 
                        yield return _glyphIndexer[i]; 
                }
 
    #endregion

    #region IEnumerable Members
 
                IEnumerator IEnumerable.GetEnumerator()
                { 
                    return ((IEnumerable<double>)this).GetEnumerator(); 
                }
 
    #endregion

                private GlyphIndexer _glyphIndexer;
            } 

            private GlyphAccessor _accessor; 
            private ushort _numberOfGlyphs; 
        }
 
    #endregion Private Nested Classes

        //------------------------------------------------------
        // 
        //  Private Fields
        // 
        //----------------------------------------------------- 

    #region Private Fields 

        private FontFaceLayoutInfo  _fontFace;

        private StyleSimulations    _styleSimulations; 

        /// <summary> 
        /// The Uri that was passed in to constructor. 
        /// </summary>
        /// <SecurityNote> 
        ///     This is critical as we do a demand based on this value public functions.
        ///     Only setting this Uri is critical, getting is fine.  Hence using the
        ///     SecurityCriticalDataForSet object.  Note that the object itself does not
        ///     need to be Critical, it's just setting it that makes it Critical. 
        /// </SecurityNote>
        private SecurityCriticalDataClass<Uri> _originalUri; 
 
        /// <SecurityNote>
        /// Critical - as this object controls the Demand that'll be made before accessssing the 
        ///            security sensitive contents of the font file.  This also only Critical
        ///            for set.  This should be correctly whenever _originalUri is set.
        ///
        /// Caching object for perf reasons. 
        /// </SecurityNote>
        private SecurityCriticalDataForSet<CodeAccessPermission> _fileIOPermObj; 
 
        private const double CFFConversionFactor = 1.0 / 65536.0;
 
        private InitializationState _initializationState;

        /// <summary>
        /// Initialization states of GlyphTypeface object. 
        /// </summary>
        private enum InitializationState 
        { 
            /// <summary>
            /// The state in which the GlyphTypeface has not been initialized. 
            /// At this state, all operations on the object would cause InvalidOperationException.
            /// The object can only transit to 'IsInitializing' state with BeginInit() call.
            /// </summary>
            Uninitialized, 

            /// <summary> 
            /// The state in which the GlyphTypeface is being initialized. At this state, user can 
            /// set values into the required properties. The object can only transit to 'IsInitialized' state
            /// with EndInit() call. 
            /// </summary>
            IsInitializing,

            /// <summary> 
            /// The state in which the GlyphTypeface object is fully initialized. At this state the object
            /// is fully functional. There is no valid transition out of the state. 
            /// </summary> 
            IsInitialized,
        } 

    #endregion Private Fields
#endif
  }
}
