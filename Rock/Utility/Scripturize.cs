// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

namespace Rock.Utility
{
    /// <summary>
    /// Helper that parses strings for scriptures
    /// </summary>
    public static class Scripturize
    {
        #region Private Members
        private static List<BibleBook> _bibleBooks;

        private static List<BibleTranslation> _bibleTranslations;
        #endregion

        #region Public Methods

        /// <summary>
        /// Parses the specified text looking for scripture references and converting them to links if they are not in
        /// code blocks, pre tags or anchors.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="defaultTranslation">The default translation.</param>
        /// <param name="landingSite">The landing site.</param>
        /// <param name="cssClass">The CSS class.</param>
        /// <returns></returns>
        public static string Parse( string text, string defaultTranslation = "NLT", LandingSite landingSite = LandingSite.YouVersion, string cssClass = "" )
        {
            var scripturizedString = new StringBuilder();

            var tokens = TokenizeString( text );

            foreach ( var token in tokens )
            {
                if ( IsIgnoredToken( token ) )
                {
                    scripturizedString.Append( token );
                }
                else
                {
                    scripturizedString.Append( AddScriptureLinks( token, defaultTranslation, landingSite, cssClass ) );
                }

            }

            return scripturizedString.ToString();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Tokenizes the string into chunks of text that need to be checked for scriptures.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private static string[] TokenizeString( string text )
        {
            string regexAnchor = @"<a\s+href.*?<\/a>";
            string regexPre = @"<pre>.*<\/pre>";
            string regexCode = @"<code>.*<\/code>";
            string regexTag = @"<(?:[^<>\s]*)(?:\s[^<>]*){0,1}>";
            string regexEsvPlugin = @"\[bible\].*\[\/bible\]";
            string regexBibleBlock = @"\[bibleblock\].*\[\/bibleblock\]";

            string regexSplit = string.Format( @"((?:{0})|(?:{1})|(?:{2})|(?:{3})|(?:{4})|(?:{5}))",
                                       regexAnchor,     // 0
                                       regexPre,        // 1
                                       regexCode,       // 2
                                       regexEsvPlugin,  // 3
                                       regexBibleBlock, // 4
                                       regexTag         // 5
                                       );

            return Regex.Split( text, regexSplit );
        }

        /// <summary>
        /// Determines whether the token should be inspected for scripture references.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>
        ///   <c>true</c> if [is ignored token] [the specified token]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsIgnoredToken( string token )
        {
            string regexAnchor = @"<a\s+href.*?<\/a>";
            string regexPre = @"<pre>.*<\/pre>";
            string regexCode = @"<code>.*<\/code>";
            //string regexHeadingTag = @"<h\d.*>.*<\/h\d>";
            string regexTag = @"<(?:[^<>\s]*)(?:\s[^<>]*){0,1}>";
            string regexEsvPlugin = @"\[bible\].*\[\/bible\]";
            string regexBibleBlock = @"\[bibleblock\].*\[\/bibleblock\]";

            string regexSplit = string.Format( @"((?:{0})|(?:{1})|(?:{2})|(?:{3})|(?:{4})|(?:{5}))",
                                       regexAnchor,     // 0
                                       regexPre,        // 1
                                       regexCode,       // 2
                                       regexEsvPlugin,  // 3
                                       regexBibleBlock, // 4
                                       regexTag  // 5
                                        );

            var regex = new Regex( regexSplit );
            return regex.Match( token ).Success;
        }

        /// <summary>
        /// Adds the scripture links to the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="defaultTranslation">The default translation.</param>
        /// <param name="landingSite">The landing site.</param>
        /// <param name="cssClass">The CSS class.</param>
        /// <returns></returns>
        private static string AddScriptureLinks( string token, string defaultTranslation, LandingSite landingSite, string cssClass = "" )
        {
            string regexVolumes = @"1|2|3|I|II|III|1st|2nd|3rd|First|Second|Third";

            string regexBook = GetBookRegex();
            string regexTranslations = GetTranslationRegex();

            string regexChapterVerse = @"\d{1,3}(?::\d{1,3})?(?:\s?(?:[-&,]\s?\d+))*";

            string regexPassageRegex = string.Format( @"(?:({0})\s)?({1})\s({2})(?:\s?[,-]?\s?((?:{3})|\s?\((?:{3})\)))?",
                        regexVolumes, // 0
                        regexBook, // 1
                        regexChapterVerse, // 2
                        regexTranslations // 3
                );

            return Regex.Replace( token, regexPassageRegex, delegate ( Match match )
            {
                // Ensure the match was in the correct format
                if ( match.Groups.Count != 5 )
                {
                    return match.Value;
                }

                var volume = NormalizeVolume( match.Groups[1].ToString() );
                var book = match.Groups[2].ToString();
                var verses = match.Groups[3].ToString();
                var translation = match.Groups[4].ToString().Replace( ")", "" ).Replace( "(", "" );

                // Check that the reference isn't for a chapter range, this is not supported (e.g. John 1-16)
                if (verses.Contains("-") && !verses.Contains( ":" ) )
                {
                    return match.Value;
                }

                // Catch the case of 'The 3 of us went downtown' which triggers a link to 1 Thess 3
                if ( book.ToLower() == "the" && volume.IsNullOrWhiteSpace() )
                {
                    return match.Value;
                }

                // Apply default translation if needed
                if ( translation.IsNullOrWhiteSpace() )
                {
                    translation = defaultTranslation;
                }

                var scriptureLink = string.Empty;
                var serviceLabel = string.Empty;

                switch ( landingSite )
                {
                    case LandingSite.YouVersion:
                        {
                            scriptureLink = FormatScriptureLinkYouVersion( volume, book, verses, translation );
                            serviceLabel = "YouVersion";
                            break;
                        }
                    case LandingSite.BibleGateway:
                        {
                            scriptureLink = FormatScriptureLinkBibleGateway( volume, book, verses, translation );
                            serviceLabel = "Bible Gateway";
                            break;
                        }
                }

                if ( scriptureLink.IsNullOrWhiteSpace() )
                {
                    return match.Value;
                }

                if ( cssClass.IsNullOrWhiteSpace() )
                {
                    return string.Format( "<a href=\"{0}\" title=\"{1}\">{2}</a>",
                                scriptureLink,  // 0
                                serviceLabel,   // 1
                                match.Value     // 2
                    );
                }

                return string.Format( "<a href=\"{0}\" class=\"{1}\" title=\"{2}\">{3}</a>",
                                scriptureLink,  // 0
                                cssClass,       // 1
                                serviceLabel,   // 2
                                match.Value     // 3
                    );
            } );
        }

        /// <summary>
        /// Gets the book regex from the book configuration
        /// </summary>
        /// <returns></returns>
        private static string GetBookRegex()
        {
            var bookList = new StringBuilder();
            foreach ( var bookConfig in _bibleBooks )
            {
                bookList.Append( bookConfig.Name + "|" );

                foreach ( var bookAlias in bookConfig.Aliases )
                {
                    bookList.Append( bookAlias + "|" );
                }
            }

            return bookList.ToString().TrimEnd( '|' );
        }

        /// <summary>
        /// Gets the translation regex.
        /// </summary>
        /// <returns></returns>
        private static string GetTranslationRegex()
        {
            return string.Join( "|", _bibleTranslations.Select( t => t.Abbreviation ) );
        }

        /// <summary>
        /// Normalizes the volume to a standard format.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <returns></returns>
        private static string NormalizeVolume( string volume )
        {
            volume = volume.Replace( "III", "3" );
            volume = volume.Replace( "Third", "3" );
            volume = volume.Replace( "II", "2" );
            volume = volume.Replace( "Second", "2" );
            volume = volume.Replace( "I", "1" );
            volume = volume.Replace( "First", "3" );

            return volume;
        }

        /// <summary>
        /// Formats the scripture link to you version standards.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <param name="book">The book.</param>
        /// <param name="verses">The verses.</param>
        /// <param name="translation">The translation.</param>
        /// <returns></returns>
        private static string FormatScriptureLinkYouVersion( string volume, string book, string verses, string translation )
        {
            // Get the YouVersion translation id
            int translationId = 1; // This is the YouVersion default KJV

            if ( _bibleTranslations.Where( t => t.Abbreviation == translation ).Count() > 0 )
            {
                translationId = _bibleTranslations.Where( t => t.Abbreviation == translation ).FirstOrDefault().YouVersionId;
            }
            else
            {
                translation = "KJV";
            }

            // Split chapter/verse
            var chapterVerse = verses.Split( ':' );
            var chapter = chapterVerse[0];
            var verse = chapterVerse.Length > 1 ? chapterVerse[1] : string.Empty;

            // Normalize the book name to match YouVersions requirements
            var bookConfig = _bibleBooks.Where( b => b.Name == book || b.Aliases.Contains( book ) ).FirstOrDefault();

            // Return an empty string if we could not find the book
            if ( bookConfig.IsNull() )
            {
                return string.Empty;
            }

            var youVersionBook = bookConfig.YouVersionAbbreviation.ToUpper();

            // Append the volume if needed
            if ( bookConfig.HasVolume )
            {
                youVersionBook = volume + youVersionBook;
            }

            return string.Format( "https://www.bible.com/bible/{0}/{1}.{2}.{3}.{4}",
                    translationId,                                  // 0
                    youVersionBook,    // 1
                    chapter,                                        // 2
                    verse,                                          // 3
                    translation                                     // 4
                );
        }

        /// <summary>
        /// Formats the scripture link bible gateway standards.
        /// </summary>
        /// <param name="volume">The volume.</param>
        /// <param name="book">The book.</param>
        /// <param name="verses">The verses.</param>
        /// <param name="translation">The translation.</param>
        /// <returns></returns>
        private static string FormatScriptureLinkBibleGateway( string volume, string book, string verses, string translation )
        {
            var passage = string.Format( "{0} {1}+{2}",
                            volume, // 0
                            book,   // 1
                            verses  // 2
                ).Trim().Replace( " ", "%20" );

            return string.Format( "http://biblegateway.com/bible?version={0}&passage={1}",
                    translation.ToUpper(),  // 0
                    passage                 // 1
                );
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the <see cref="Scripturize"/> class.
        /// </summary>
        static Scripturize() {

            // Hydrate the Bible books
            _bibleBooks = JsonConvert.DeserializeObject<List<BibleBook>>( _bibleBooksJson );

            // Add version configurations (may move this to json at some point)
            _bibleTranslations = new List<BibleTranslation>
            {
                new BibleTranslation(){ Abbreviation = "AMP", YouVersionId = 8 },
                new BibleTranslation(){ Abbreviation = "ASV", YouVersionId = 12 },
                new BibleTranslation(){ Abbreviation = "CEB", YouVersionId = 37 },
                new BibleTranslation(){ Abbreviation = "CEV", YouVersionId = 37 },
                new BibleTranslation(){ Abbreviation = "CEVUS06", YouVersionId = 392 },
                new BibleTranslation(){ Abbreviation = "CPDV", YouVersionId = 42 },
                new BibleTranslation(){ Abbreviation = "DARBY", YouVersionId = 478 },
                new BibleTranslation(){ Abbreviation = "DRA", YouVersionId = 55 },
                new BibleTranslation(){ Abbreviation = "ESV", YouVersionId = 59 },
                new BibleTranslation(){ Abbreviation = "GNBDC", YouVersionId = 416 },
                new BibleTranslation(){ Abbreviation = "GWT", YouVersionId = 70 },
                new BibleTranslation(){ Abbreviation = "GNB", YouVersionId = 296 },
                new BibleTranslation(){ Abbreviation = "GNT", YouVersionId = 68 },
                new BibleTranslation(){ Abbreviation = "HCSB", YouVersionId = 72 },
                new BibleTranslation(){ Abbreviation = "KJV", YouVersionId = 1 },
                new BibleTranslation(){ Abbreviation = "MSG", YouVersionId = 97 },
                new BibleTranslation(){ Abbreviation = "NASB", YouVersionId = 100 },
                new BibleTranslation(){ Abbreviation = "NCV", YouVersionId = 105 },
                new BibleTranslation(){ Abbreviation = "NIV", YouVersionId = 111 },
                new BibleTranslation(){ Abbreviation = "NET", YouVersionId = 107 },
                new BibleTranslation(){ Abbreviation = "NIRV", YouVersionId = 110 },
                new BibleTranslation(){ Abbreviation = "NKJV", YouVersionId = 114 },
                new BibleTranslation(){ Abbreviation = "NLT", YouVersionId = 116 },
                new BibleTranslation(){ Abbreviation = "OJB", YouVersionId = 130 },
                new BibleTranslation(){ Abbreviation = "RSV", YouVersionId = 2020 },
                new BibleTranslation(){ Abbreviation = "TLV", YouVersionId = 314 },
                new BibleTranslation(){ Abbreviation = "WEB", YouVersionId = 206 }
            };
        }
        #endregion

        #region Configuration

        /// <summary>
        /// The bible books in JSON format
        /// see https://www.logos.com/bible-book-abbreviations
        /// </summary>
        public static readonly string _bibleBooksJson = @"[
	{
		""name"": ""Genesis"",
		""aliases"": [
            ""Gen"",
            ""Ge"",
            ""Gn""
        ],
        ""you_version_abbrev"": ""gen"",
        ""has_volume"": false
	},
	{
		""name"": ""Exodus"",
		""aliases"": [
			""Exod"",
			""Ex"",
			""Exo""
        ],
        ""you_version_abbrev"": ""exo"",
        ""has_volume"": false
	},
	{
		""name"": ""Leviticus"",
		""aliases"": [
            ""Lev"",
            ""Le"",
            ""Lv""
        ],
        ""you_version_abbrev"": ""lev"",
        ""has_volume"": false
	},
	{
		""name"": ""Numbers"",
		""aliases"": [
            ""Num"",
            ""Nu"",
            ""Nm"",
            ""Nb""
        ],
        ""you_version_abbrev"": ""num"",
        ""has_volume"": false
	},
	{
		""name"": ""Deuteronomy"",
		""aliases"": [
            ""Deut"",
            ""De"",
            ""Dt"",
            ""Deut?""
        ],
        ""you_version_abbrev"": ""deu"",
        ""has_volume"": false
	},
	{
		""name"": ""Joshua"",
		""aliases"": [
            ""Josh"",
            ""Jos"",
            ""Jsh"",
            ""Josh?""
        ],
        ""you_version_abbrev"": ""jos"",
        ""has_volume"": false
	},
	{
		""name"": ""Judges"",
		""aliases"": [
            ""Judg"",
            ""Jdg"",
            ""Jdgs"",
            ""Judg?""
        ],
        ""you_version_abbrev"": ""jdg"",
        ""has_volume"": false
	},
	{
		""name"": ""Ruth"",
        ""aliases"": [
            ""Rth"",
            ""Ru"",
            ""rut""
        ],
        ""you_version_abbrev"": ""rut"",
        ""has_volume"": false
	},
	{
		""name"": ""Samuel"",
		""aliases"": [
			""Sam"",
            ""Samuel"",
            ""Sm"",
            ""Sa""
        ],
        ""you_version_abbrev"": ""sa"",
        ""has_volume"": true
	},
	{
		""name"": ""Kings"",
		""aliases"": [
            ""Kgs"",
            ""Ki"",
            ""Ki?n""
        ],
        ""you_version_abbrev"": ""ki"",
        ""has_volume"": true
	},
	{
		""name"": ""Chronicles"",
		""aliases"": [
            ""Chr"",
            ""Ch"",
            ""Chron"",
            ""Chr(?:on?)?""
        ],
        ""you_version_abbrev"": ""ch"",
        ""has_volume"": true
	},
	{
		""name"": ""Ezra"",
		""aliases"": [
            ""Ez"",
            ""Ezr"",
        ],
        ""you_version_abbrev"": ""ezr"",
        ""has_volume"": false
	},
	{
		""name"": ""Nehemiah"",
		""aliases"": [
            ""Neh"",
            ""Ne""
        ],
        ""you_version_abbrev"": ""neh"",
        ""has_volume"": false
	},
	{
		""name"": ""Esther"",
		""aliases"": [
			""Esth"",
            ""Est"",
            ""Es"",
        ],
        ""you_version_abbrev"": ""est"",
        ""has_volume"": false
	},
	{
		""name"": ""Job"",
        ""aliases"": [
            ""Jb""
        ],
        ""you_version_abbrev"": ""job"",
        ""has_volume"": false
	},
	{
		""name"": ""Psalms"",
		""aliases"": [
			""Ps"",
            ""Psalm"",
            ""Psa"",
            ""Psm"",
            ""Pss"",
            ""Psalms?"",
            ""Psa?""
        ],
        ""you_version_abbrev"": ""psa"",
        ""has_volume"": false
	},
	{
		""name"": ""Proverbs"",
		""aliases"": [
			""Prov"",
            ""Pro"",
            ""Prv"",
            ""Pr"",
            ""Proverbs?"",
            ""Pr(?:ov?)?""
        ],
        ""you_version_abbrev"": ""pro"",
        ""has_volume"": false
	},
	{
		""name"": ""Ecclesiastes"",
		""aliases"": [
			""Eccl"",
            ""Ecc"",
            ""Eccles"",
            ""Eccl?""
        ],
        ""you_version_abbrev"": ""ecc"",
        ""has_volume"": false
	},
	{
		""name"": ""Song of Solomon"",
		""aliases"": [
			""Song"",
            ""Song of Songs"",
            ""So"",
            ""Sng"",
            ""Songs? of Solomon"",
            ""Song?""
        ],
        ""you_version_abbrev"": ""sng"",
        ""has_volume"": false
	},
	{
		""name"": ""Isaiah"",
		""aliases"": [
            ""Isa"",
            ""Is""
        ],
        ""you_version_abbrev"": ""isa"",
        ""has_volume"": false
	},
	{
		""name"": ""Jeremiah"",
		""aliases"": [
            ""Jer"",
            ""Je"",
            ""Jr""
        ],
        ""you_version_abbrev"": ""jer"",
        ""has_volume"": false
	},
	{
		""name"": ""Lamentations"",
		""aliases"": [
            ""Lam"",
            ""La""
        ],
        ""you_version_abbrev"": ""lam"",
        ""has_volume"": false
	},
	{
		""name"": ""Ezekiel"",
		""aliases"": [
            ""Ezek"",
            ""Eze"",
            ""Ezk""
        ],
        ""you_version_abbrev"": ""ezk"",
        ""has_volume"": false
	},
	{
		""name"": ""Daniel"",
		""aliases"": [
            ""Dan"",
            ""Da"",
            ""Dn""
        ],
        ""you_version_abbrev"": ""dan"",
        ""has_volume"": false
	},
	{
		""name"": ""Hosea"",
		""aliases"": [
            ""Hos"",
            ""Ho""
        ],
        ""you_version_abbrev"": ""hos"",
        ""has_volume"": false
	},
	{
		""name"": ""Joel"",
        ""aliases"": [
            ""Jl"",
            ""Jol""
        ],
        ""you_version_abbrev"": ""jol"",
        ""has_volume"": false
	},
	{
		""name"": ""Amos"",
        ""aliases"": [
            ""Am"",
            ""Amo""
        ],
        ""you_version_abbrev"": ""amo"",
        ""has_volume"": false
	},
	{
		""name"": ""Obadiah"",
		""aliases"": [
			""Obad"",
            ""Oba"",
            ""Ob""
        ],
        ""you_version_abbrev"": ""oba"",
        ""has_volume"": false
	},
	{
		""name"": ""Jonah"",
		""aliases"": [
            ""Jon"",
            ""Jnh"",
            ""Jon""
        ],
        ""you_version_abbrev"": ""jon"",
        ""has_volume"": false
	},
	{
		""name"": ""Micah"",
		""aliases"": [
            ""Mic"",
            ""Mc""
        ],
        ""you_version_abbrev"": ""mic"",
        ""has_volume"": false
	},
	{
		""name"": ""Nahum"",
		""aliases"": [
            ""Nah"",
            ""Na"",
            ""Nam""
        ],
        ""you_version_abbrev"": ""nam"",
        ""has_volume"": false
	},
	{
		""name"": ""Habakkuk"",
		""aliases"": [
            ""Hab"",
            ""Hb""
        ],
        ""you_version_abbrev"": ""hab"",
        ""has_volume"": false
	},
	{
		""name"": ""Zephaniah"",
		""aliases"": [
            ""Zeph"",
            ""Zep"",
            ""Zp"",
            ""Zeph?""
        ],
        ""you_version_abbrev"": ""zep"",
        ""has_volume"": false
	},
	{
		""name"": ""Haggai"",
		""aliases"": [
            ""Hag"",
            ""Hg""
        ],
        ""you_version_abbrev"": ""hag"",
        ""has_volume"": false
	},
	{
		""name"": ""Zechariah"",
		""aliases"": [
            ""Zech"",
            ""Zec"",
            ""Zc"",
            ""Zech?""
        ],
        ""you_version_abbrev"": ""zec"",
        ""has_volume"": false
	},
	{
		""name"": ""Malachi"",
		""aliases"": [
            ""Mal"",
            ""Ml""
        ],
        ""you_version_abbrev"": ""mal"",
        ""has_volume"": false
	},
	{
		""name"": ""Matthew"",
		""aliases"": [
            ""Matt"",
            ""Mt"",
            ""Mat"",
            ""Mat+hew"",
            ""Mat+""
        ],
        ""you_version_abbrev"": ""mat"",
        ""has_volume"": false
	},
	{
		""name"": ""Mark"",
        ""aliases"": [
            ""Mrk"",
            ""Mar"",
            ""Mk"",
            ""Mr"",
            ""Mr?k""
        ],
        ""you_version_abbrev"": ""mrk"",
        ""has_volume"": false
	},
	{
		""name"": ""Luke"",
        ""aliases"": [
            ""Luk"",
            ""Lk"",
            ""Lu?k""
        ],
        ""you_version_abbrev"": ""luk"",
        ""has_volume"": false
	},
	{
		""name"": ""John"",
        ""aliases"": [
            ""Joh"",
            ""Jhn"",
            ""Jn"",
            ""Jh?n""
        ],
        ""you_version_abbrev"": ""Jhn"",
        ""has_volume"": false
	},
	{
		""name"": ""Acts"",
        ""aliases"": [
            ""Act"",
            ""Ac"",
            ""Acts?""
        ],
        ""you_version_abbrev"": ""act"",
        ""has_volume"": false
	},
	{
		""name"": ""Romans"",
		""aliases"": [
            ""Rom"",
            ""Ro"",
            ""Rm""
        ],
        ""you_version_abbrev"": ""rom"",
        ""has_volume"": false
	},
	{
		""name"": ""Corinthians"",
		""aliases"": [
            ""Cor"",
            ""Co""
        ],
        ""you_version_abbrev"": ""co"",
        ""has_volume"": true
	},
	{
		""name"": ""Galatians"",
		""aliases"": [
            ""Gal"",
            ""Ga""
        ],
        ""you_version_abbrev"": ""gal"",
        ""has_volume"": false
	},
	{
		""name"": ""Ephesians"",
		""aliases"": [
            ""Eph"",
            ""Ephes""
        ],
        ""you_version_abbrev"": ""eph"",
        ""has_volume"": false
	},
	{
		""name"": ""Philippians"",
		""aliases"": [
            ""Phil"",
            ""Php"",
            ""Pp"",
            ""Phil+ippians"",
            ""Phil?""
        ],
        ""you_version_abbrev"": ""php"",
        ""has_volume"": false
	},
	{
		""name"": ""Colossians"",
		""aliases"": [
            ""Col"",
            ""Co""
        ],
        ""you_version_abbrev"": ""col"",
        ""has_volume"": false
	},
	{
		""name"": ""Thessalonians"",
		""aliases"": [
            ""Thess"",
            ""Th"",
            ""The"",
            ""The?"",
            ""Thess?""
        ],
        ""you_version_abbrev"": ""th"",
        ""has_volume"": true
	},
	{
		""name"": ""Timothy"",
		""aliases"": [
            ""Tim"",
            ""Ti""
        ],
        ""you_version_abbrev"": ""ti"",
        ""has_volume"": true
	},
	{
		""name"": ""Titus"",
        ""aliases"": [
            ""Tit""
        ],
        ""you_version_abbrev"": ""tit"",
        ""has_volume"": false
	},
	{
		""name"": ""Philemon"",
		""aliases"": [
            ""Phlm"",
            ""Phm"",
            ""Pm""
        ],
        ""you_version_abbrev"": ""phm"",
        ""has_volume"": false
	},
	{
		""name"": ""Hebrews"",
		""aliases"": [
			""Heb""
        ],
        ""you_version_abbrev"": ""heb"",
        ""has_volume"": false
	},
	{
		""name"": ""James"",
		""aliases"": [
            ""Jas"",
            ""Jm"",
            ""Ja?m""
        ],
        ""you_version_abbrev"": ""jas"",
        ""has_volume"": false
	},
	{
		""name"": ""Peter"",
		""aliases"": [
            ""Pet"",
            ""Pt"",
            ""P"",
            ""Pe"",
            ""Pe?t""
        ],
        ""you_version_abbrev"": ""pe"",
        ""has_volume"": true
	},
	{
		""name"": ""John"",
		""aliases"": [
			""Joh"",
            ""Jhn"",
            ""Jn""
        ],
        ""you_version_abbrev"": ""Jn"",
        ""has_volume"": false
	},
	{
		""name"": ""Jude"",
        ""aliases"": [
            ""Jud"",
            ""Jd"",
            ""Ju?d""
        ],
        ""you_version_abbrev"": ""jud"",
        ""has_volume"": false
	},
	{
		""name"": ""Revelation"",
		""aliases"": [
            ""Rev"",
            ""Re""
        ],
        ""you_version_abbrev"": ""rev"",
        ""has_volume"": false
    }
]";
        #endregion
    }

    #region Enums

    /// <summary>
    /// Enum for the various scripture landing sites that are supported
    /// </summary>
    public enum LandingSite
    {
        /// <summary>
        /// YouVersion
        /// </summary>
        YouVersion = 0,

        /// <summary>
        /// Bible Gateway
        /// </summary>
        BibleGateway = 1
    }
    #endregion

    #region POCOS
    /// <summary>
    /// POCO used to store configuration about a book in the Bible
    /// </summary>
    public class BibleBook
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [JsonProperty( "name" )]
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the aliases.
        /// </summary>
        /// <value>
        /// The aliases.
        /// </value>
        [JsonProperty( "aliases" )]
        public List<string> Aliases { get; set; }
        /// <summary>
        /// Gets or sets you version abbreviation.
        /// </summary>
        /// <value>
        /// You version abbreviation.
        /// </value>
        [JsonProperty( "you_version_abbrev" )]
        public string YouVersionAbbreviation { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance has volume.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has volume; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty( "has_volume" )]
        public bool HasVolume { get; set; }
    }

    /// <summary>
    /// POCO use for storing Bible translations
    /// </summary>
    public class BibleTranslation {
        /// <summary>
        /// Gets or sets the abbreviation.
        /// </summary>
        /// <value>
        /// The abbreviation.
        /// </value>
        public string Abbreviation { get; set; }

        /// <summary>
        /// Gets or sets you version identifier.
        /// </summary>
        /// <value>
        /// You version identifier.
        /// </value>
        public int YouVersionId { get; set; }
    }
    #endregion
}
