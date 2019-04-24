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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Hosting;

using Humanizer;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;

namespace Rock.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public static class FontAwesomeHelper
    {
        /// <summary>
        /// Gets the font awesome pro key from SystemSettings. Returns string.Empty if there is no key
        /// </summary>
        /// <returns></returns>
        public static string GetFontAwesomeProKey()
        {
            return SystemSettings.GetValue( Rock.SystemKey.SystemSetting.FONT_AWESOME_PRO_KEY ) ?? string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public static class VariableOverridesTokens
        {
            /// <summary>
            /// The start region
            /// </summary>
            public const string StartRegion = "/* FontAwesome Start */";

            /// <summary>
            /// The end region
            /// </summary>
            public const string EndRegion = "/* FontAwesome End */";

            /// <summary>
            /// The font weight name line start
            /// </summary>
            public const string FontWeightNameLineStart = "@fa-theme-weight-name:";

            /// <summary>
            /// The font weight value line start
            /// </summary>
            public const string FontWeightValueLineStart = "@fa-theme-weight:";

            /// <summary>
            /// The font face line start
            /// </summary>
            public const string FontFaceLineStart = ".fa-font-face(";

            /// <summary>
            /// The font awesome edition
            /// </summary>
            public const string FontEditionLineStart = "@fa-edition:";

            /// <summary>
            /// The font awesome pro edition
            /// </summary>
            public const string FontEditionPro = "pro";

            /// <summary>
            /// The font awesome free edition
            /// </summary>
            public const string FontEditionFree = "free";
        }

        /// <summary>
        /// Sets the font awesome pro key.
        /// </summary>
        /// <param name="fontAwesomeProKey">The font awesome pro key.</param>
        public static void SetFontAwesomeProKey( string fontAwesomeProKey )
        {
            SystemSettings.SetValue( Rock.SystemKey.SystemSetting.FONT_AWESOME_PRO_KEY, fontAwesomeProKey );
        }

        /// <summary>
        /// Determines whether a Font Awesome Pro Key is set in SystemSettings
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has font awesome pro key]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasFontAwesomeProKey()
        {
            return !string.IsNullOrWhiteSpace( GetFontAwesomeProKey() );
        }

        /// <summary>
        /// Extracts the font awesome package from the uploaded BinaryFile
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <returns></returns>
        public static bool ExtractFontAwesomePackage( int binaryFileId )
        {
            var rockContext = new RockContext();
            BinaryFileService binaryFileService = new BinaryFileService( rockContext );
            BinaryFile fontawesomePackageBinaryFile = binaryFileService.Get( binaryFileId );

            if ( fontawesomePackageBinaryFile != null )
            {
                using ( ZipArchive fontawesomePackageZip = new ZipArchive( fontawesomePackageBinaryFile.ContentStream ) )
                {
                    string webFontsWithCssFolder = fontawesomePackageZip.Entries.Where( a => a.FullName.EndsWith( "/web-fonts-with-css/" ) ).Select( a => a.FullName ).FirstOrDefault();

                    var webFontsWithCssFiles = fontawesomePackageZip.Entries.Where( a => a.Name.IsNotNullOrWhiteSpace() ).ToList();
                    if ( !string.IsNullOrEmpty( webFontsWithCssFolder ) )
                    {
                        webFontsWithCssFiles = fontawesomePackageZip.Entries.Where( a => a.Name.IsNotNullOrWhiteSpace() && a.FullName.StartsWith( webFontsWithCssFolder ) ).ToList();
                    }



                    var lessFileEntries = webFontsWithCssFiles
                        .Where( a => new DirectoryInfo( Path.GetDirectoryName( a.FullName ) ).Name.Equals( "less" ) ).ToList();

                    var webFontsFileEntries = webFontsWithCssFiles
                        .Where( a => new DirectoryInfo( Path.GetDirectoryName( a.FullName ) ).Name.Equals( "webfonts" ) ).ToList();

                    var fontAwesomeFontsFolder = HostingEnvironment.MapPath( "~/Assets/Fonts/FontAwesome" );
                    var fontAwesomeStylesFolder = HostingEnvironment.MapPath( "~/Styles/FontAwesome" );

                    foreach ( ZipArchiveEntry lessFileEntry in lessFileEntries )
                    {
                        var destLessFileName = Path.Combine( fontAwesomeStylesFolder, lessFileEntry.Name );
                        if ( File.Exists( destLessFileName ) )
                        {
                            File.Delete( destLessFileName );
                        }

                        lessFileEntry.ExtractToFile( destLessFileName );

                        // Fixup LineEndings
                        var updatedFileLines = File.ReadAllLines( destLessFileName );
                        File.WriteAllLines( destLessFileName, updatedFileLines );
                    }

                    foreach ( var webFontsFileEntry in webFontsFileEntries )
                    {
                        var destFontFile = Path.Combine( fontAwesomeFontsFolder, webFontsFileEntry.Name );
                        if ( File.Exists( destFontFile ) )
                        {
                            File.Delete( destFontFile );
                        }

                        webFontsFileEntry.ExtractToFile( destFontFile );
                    }
                }

                binaryFileService.Delete( fontawesomePackageBinaryFile );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the font awesome icon CSS weights.
        /// </summary>
        /// <value>
        /// The font awesome icon CSS weights.
        /// </value>
        public static FontAwesomeIconCssWeight[] FontAwesomeIconCssWeights => new FontAwesomeIconCssWeight[]
        {
            new FontAwesomeIconCssWeight( "solid", 900, false, true, true ),
            new FontAwesomeIconCssWeight( "regular", 400, true, false, true ),
            new FontAwesomeIconCssWeight( "light", 300, true, false, true ),
            new FontAwesomeIconCssWeight( "brands", 400, true, true, false )
        };

        /// <summary>
        /// 
        /// </summary>
        public class FontAwesomeIconCssWeight
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FontAwesomeIconCssWeight" /> class.
            /// </summary>
            /// <param name="weightName">Name of the weight.</param>
            /// <param name="weightValue">The weight value.</param>
            /// <param name="requiresProForPrimary">if set to <c>true</c> [requires pro for primary].</param>
            /// <param name="includedInFree">if set to <c>true</c> [included in free].</param>
            /// <param name="isConfigurable">if set to <c>true</c> [is configurable].</param>
            public FontAwesomeIconCssWeight( string weightName, int weightValue, bool requiresProForPrimary, bool includedInFree, bool isConfigurable )
            {
                this.DisplayName = weightName.Transform( To.TitleCase );
                this.WeightName = weightName;
                this.WeightValue = weightValue;
                this.RequiresProForPrimary = requiresProForPrimary;
                this.IncludedInFree = includedInFree;
                this.IsConfigurable = isConfigurable;
            }

            /// <summary>
            /// Gets the display name.
            /// </summary>
            /// <value>
            /// The display name.
            /// </value>
            public string DisplayName { get; internal set; }

            /// <summary>
            /// Gets the name of the weight that is used in css/less
            /// </summary>
            /// <value>
            /// The name of the weight.
            /// </value>
            public string WeightName { get; internal set; }

            /// <summary>
            /// Gets the numeric value of the weight
            /// </summary>
            /// <value>
            /// The weight.
            /// </value>
            public int WeightValue { get; internal set; }

            /// <summary>
            /// Gets a value indicating whether [requires pro for primary].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [requires pro for primary]; otherwise, <c>false</c>.
            /// </value>
            public bool RequiresProForPrimary { get; internal set; }

            /// <summary>
            /// Gets a value indicating whether [included in free].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [included in free]; otherwise, <c>false</c>.
            /// </value>
            public bool IncludedInFree { get; internal set; }

            /// <summary>
            /// Gets a value indicating whether the font can be individual selected as a Primary or Alternate Font
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is configurable; otherwise, <c>false</c>.
            /// </value>
            public bool IsConfigurable { get; internal set; }
        }
    }
}
