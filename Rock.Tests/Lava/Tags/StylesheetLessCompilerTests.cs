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
using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Lava.Blocks;
using Rock.Tests.Shared;

namespace Rock.Tests.Lava.Tags
{
    /// <summary>
    /// Tests the Less compiler used by the stylesheet command. A temporary
    /// folder stands in for the website so the file rules can be tested.
    /// </summary>
    [TestClass]
    [TestCategory( TestFeatures.Lava )]
    public class StylesheetLessCompilerTests
    {
        #region Fields

        private static string _siteRoot;

        #endregion Fields

        #region Setup

        /// <summary>
        /// Creates the temporary website folder and its files.
        /// </summary>
        /// <param name="context">The test context.</param>
        [ClassInitialize]
        public static void ClassInitialize( TestContext context )
        {
            _siteRoot = Path.Combine( Path.GetTempPath(), "StylesheetLessCompilerTests-" + Guid.NewGuid().ToString( "N" ) );

            WriteSiteFile( "Styles/_vars.less", "@brand: #336699;" );
            WriteSiteFile( "Styles/ok.css", ".ok-css{color:red}" );
            WriteSiteFile( "Styles/secret.config", ".leak{content:'STYLES-CONFIG'}" );
            WriteSiteFile( "Themes/Test/Styles/theme.less", "@import \"../../../Styles/_vars\";\n.theme{color:@brand}" );
            WriteSiteFile( "Themes/Test/Styles/uses-datauri.less", ".x{background:data-uri('" + GetSitePath( "App_Data/secret.css" ) + "')}" );
            WriteSiteFile( "Plugins/test/plugin.less", ".plugin{color:@brand}" );
            WriteSiteFile( "App_Data/secret.css", ".leak{content:'APPDATA-SECRET'}" );
            WriteSiteFile( "App_Data/secret.less", ".leak{content:'APPDATA-SECRET'}" );
            WriteSiteFile( "StylesEvil/evil.less", ".leak{content:'PREFIX-SECRET'}" );
        }

        /// <summary>
        /// Deletes the temporary website folder.
        /// </summary>
        [ClassCleanup]
        public static void ClassCleanup()
        {
            try
            {
                Directory.Delete( _siteRoot, true );
            }
            catch
            {
                // Intentionally ignored: the temporary folder is only test data.
            }
        }

        #endregion Setup

        #region Compile

        [TestMethod]
        public void Compile_PlainLess_ReturnsMinifiedCss()
        {
            var less = "@c:#336699; .m(@w){width:@w} .a{color:@c; .m(10px); .b{color:darken(@c,10%)}} @media (max-width:600px){.a{display:none}}";

            var css = Compile( less );

            Assert.AreEqual( ".a{color:#369;width:10px}.a .b{color:#264d73}@media (max-width:600px){.a{display:none}}", css );
        }

        [TestMethod]
        public void Compile_EmptyContent_ReturnsEmptyString()
        {
            Assert.AreEqual( string.Empty, Compile( string.Empty ) );
        }

        [TestMethod]
        public void Compile_InvalidLess_ReturnsEmptyString()
        {
            Assert.AreEqual( string.Empty, Compile( ".a{color:@missing}" ) );
        }

        [TestMethod]
        public void Compile_ProtocolUrls_AreLeftAsIs()
        {
            var css = Compile( ".z{background:url('/Assets/x.png')} @import url(https://fonts.example/x.css);" );

            Assert.AreEqual( "@import url(https://fonts.example/x.css);.z{background:url('/Assets/x.png')}", css );
        }

        #endregion Compile

        #region Allowed Imports

        [TestMethod]
        [DataRow( "~/Styles/_vars.less" )]
        [DataRow( "/Styles/_vars.less" )]
        [DataRow( "~/Styles/_vars" )]
        public void Compile_ImportFromStylesFolder_IsIncluded( string importPath )
        {
            var css = Compile( $"@import \"{importPath}\"; .a{{color:@brand}}" );

            Assert.AreEqual( ".a{color:#369}", css );
        }

        [TestMethod]
        public void Compile_ImportByPhysicalPath_IsIncluded()
        {
            // This is the form the import parameter of the stylesheet command produces.
            var physicalPath = _siteRoot + "\\" + "/Styles/_vars.less";

            var css = Compile( $"@import \"{physicalPath}\"; .a{{color:@brand}}" );

            Assert.AreEqual( ".a{color:#369}", css );
        }

        [TestMethod]
        public void Compile_ThemeImportWithNestedRelativeImport_IsIncluded()
        {
            var css = Compile( "@import \"~/Themes/Test/Styles/theme.less\";" );

            Assert.AreEqual( ".theme{color:#369}", css );
        }

        [TestMethod]
        public void Compile_ImportFromPluginsFolder_IsIncluded()
        {
            var css = Compile( "@import \"~/Styles/_vars.less\"; @import \"~/Plugins/test/plugin.less\";" );

            Assert.AreEqual( ".plugin{color:#369}", css );
        }

        [TestMethod]
        public void Compile_InlineCssFromStylesFolder_IsIncluded()
        {
            var css = Compile( "@import (inline) \"~/Styles/ok.css\"; .y{a:b}" );

            StringAssert.Contains( css, ".ok-css{color:red}" );
        }

        #endregion Allowed Imports

        #region Refused Imports

        [TestMethod]
        [DataRow( "@import (inline) \"{0}\";", "App_Data/secret.css" )]
        [DataRow( "@import (less) \"{0}\";", "App_Data/secret.css" )]
        [DataRow( "@import \"{0}\";", "App_Data/secret.less" )]
        [DataRow( "@import (less) \"{0}\";", "Styles/secret.config" )]
        [DataRow( "@import \"{0}\";", "StylesEvil/evil.less" )]
        public void Compile_ImportByPhysicalPathOutsideAllowedFiles_IsNotRead( string importFormat, string sitePath )
        {
            var less = string.Format( importFormat, GetSitePath( sitePath ) ) + " .y{a:b}";

            var css = Compile( less );

            AssertNoLeak( css );
        }

        [TestMethod]
        [DataRow( "@import \"~/App_Data/secret.less\";" )]
        [DataRow( "@import \"/App_Data/secret.less\";" )]
        [DataRow( "@import (inline) \"~/App_Data/secret.css\";" )]
        [DataRow( "@import \"~/Styles/../App_Data/secret.less\";" )]
        [DataRow( "@import \"~/Styles/../../outside.less\";" )]
        [DataRow( "@import (less) \"~/Styles/secret.config\";" )]
        [DataRow( "@import \"Styles/_vars.less\";" )]
        public void Compile_ImportByVirtualPathOutsideAllowedFiles_IsNotRead( string import )
        {
            var css = Compile( import + " .y{a:b}" );

            AssertNoLeak( css );
        }

        [TestMethod]
        public void Compile_ImportByNetworkPath_IsNotRead()
        {
            var css = Compile( "@import (inline) \"\\\\127.0.0.1\\share\\x.css\"; @import \"//127.0.0.1/share/x.less\"; .y{a:b}" );

            Assert.AreEqual( string.Empty, css );
        }

        [TestMethod]
        [DataRow( "@import \"dll://Rock.Tests.dll#Rock.Tests.Lava.Tags.StylesheetLessCompilerTests.less\";" )]
        [DataRow( "@import \"x/../dll://Rock.Tests.dll#Rock.Tests.Lava.Tags.StylesheetLessCompilerTests.less\";" )]
        [DataRow( "@import url(\"dll://Rock.Tests.dll#Rock.Tests.Lava.Tags.StylesheetLessCompilerTests.less\");" )]
        public void Compile_EmbeddedResourceImport_IsRefused( string import )
        {
            // The resource is embedded in this test assembly and would be readable without the refusal.
            var css = Compile( import + " .y{a:b}" );

            AssertNoLeak( css );
        }

        [TestMethod]
        public void Compile_WithoutWebsite_RefusesImports()
        {
            var css = StylesheetLessCompiler.Compile( "@import \"~/Styles/_vars.less\"; .a{color:@brand}", path => null );

            Assert.AreEqual( string.Empty, css );
        }

        #endregion Refused Imports

        #region Data URI

        [TestMethod]
        [DataRow( ".x{background:data-uri('text/plain','{0}')}" )]
        [DataRow( ".x{background:DATA-URI('text/plain','{0}')}" )]
        [DataRow( ".x{background:datauri('text/plain','{0}')}" )]
        [DataRow( ".x{background:DataUri('{0}')}" )]
        [DataRow( ".mixin(){background:datauri('{0}')} .x{.mixin();}" )]
        [DataRow( "@f:'{0}'; .x{background:data-uri(@f)}" )]
        public void Compile_DataUriFunction_IsRefused( string lessFormat )
        {
            var less = lessFormat.Replace( "{0}", GetSitePath( "Styles/ok.css" ).Replace( "\\", "/" ) );

            var css = Compile( less );

            Assert.AreEqual( string.Empty, css );
        }

        [TestMethod]
        public void Compile_ImportedFileUsingDataUri_IsRefused()
        {
            var css = Compile( "@import \"~/Themes/Test/Styles/uses-datauri.less\"; .y{a:b}" );

            Assert.AreEqual( string.Empty, css );
        }

        [TestMethod]
        public void Compile_DataUrlInCss_IsAllowed()
        {
            // A data URL written directly in the CSS is not the data-uri function.
            var css = Compile( ".x{background:url(\"data:image/png;base64,AAAA\")}" );

            Assert.AreEqual( ".x{background:url(\"data:image/png;base64,AAAA\")}", css );
        }

        #endregion Data URI

        #region IsAllowedPath

        [TestMethod]
        [DataRow( "Styles/ok.css" )]
        [DataRow( "Styles/_vars.less" )]
        [DataRow( "STYLES/OK.CSS" )]
        [DataRow( "Themes/Test/Styles/theme.less" )]
        [DataRow( "Plugins/test/plugin.less" )]
        public void IsAllowedPath_LessOrCssInAllowedFolder_ReturnsTrue( string sitePath )
        {
            Assert.IsTrue( StylesheetLessCompiler.IsAllowedPath( GetSitePath( sitePath ), MapVirtualPath ) );
        }

        [TestMethod]
        [DataRow( "Styles/secret.config" )]
        [DataRow( "Styles/secret.config." )]
        [DataRow( "Styles/secret.config " )]
        [DataRow( "App_Data/secret.css" )]
        [DataRow( "StylesEvil/evil.less" )]
        [DataRow( "Styles/../App_Data/secret.css" )]
        [DataRow( "web.config" )]
        public void IsAllowedPath_OtherFiles_ReturnsFalse( string sitePath )
        {
            Assert.IsFalse( StylesheetLessCompiler.IsAllowedPath( GetSitePath( sitePath ), MapVirtualPath ) );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "   " )]
        [DataRow( "Styles/ok.css" )]
        [DataRow( @"\\127.0.0.1\share\Styles\ok.css" )]
        public void IsAllowedPath_EmptyRelativeOrNetworkPath_ReturnsFalse( string path )
        {
            Assert.IsFalse( StylesheetLessCompiler.IsAllowedPath( path, MapVirtualPath ) );
        }

        [TestMethod]
        public void IsAllowedPath_WithoutWebsite_ReturnsFalse()
        {
            Assert.IsFalse( StylesheetLessCompiler.IsAllowedPath( GetSitePath( "Styles/ok.css" ), path => null ) );
        }

        #endregion IsAllowedPath

        #region Helpers

        /// <summary>
        /// Compiles the Less content against the temporary website.
        /// </summary>
        /// <param name="less">The Less content.</param>
        /// <returns>The compiled CSS.</returns>
        private static string Compile( string less )
        {
            return StylesheetLessCompiler.Compile( less, MapVirtualPath );
        }

        /// <summary>
        /// Maps a virtual path to the temporary website, the same way the web server
        /// does, including refusing paths above the website root.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns>The physical path.</returns>
        private static string MapVirtualPath( string virtualPath )
        {
            var relativePath = virtualPath.TrimStart( '~' ).TrimStart( '/', '\\' );
            var physicalPath = Path.GetFullPath( Path.Combine( _siteRoot, relativePath ) );

            if ( !physicalPath.StartsWith( _siteRoot, StringComparison.OrdinalIgnoreCase ) )
            {
                throw new InvalidOperationException( "Cannot use a leading .. to exit above the top directory." );
            }

            return physicalPath;
        }

        /// <summary>
        /// Gets the physical path of a file in the temporary website.
        /// </summary>
        /// <param name="sitePath">The path relative to the website root.</param>
        /// <returns>The physical path.</returns>
        private static string GetSitePath( string sitePath )
        {
            return Path.Combine( _siteRoot, sitePath.Replace( '/', Path.DirectorySeparatorChar ) );
        }

        /// <summary>
        /// Writes a file to the temporary website.
        /// </summary>
        /// <param name="sitePath">The path relative to the website root.</param>
        /// <param name="contents">The file contents.</param>
        private static void WriteSiteFile( string sitePath, string contents )
        {
            var physicalPath = GetSitePath( sitePath );

            Directory.CreateDirectory( Path.GetDirectoryName( physicalPath ) );
            File.WriteAllText( physicalPath, contents );
        }

        /// <summary>
        /// Asserts that none of the test secrets appear in the compiled CSS.
        /// </summary>
        /// <param name="css">The compiled CSS.</param>
        private static void AssertNoLeak( string css )
        {
            Assert.IsFalse( css.Contains( "APPDATA-SECRET" ), "A file outside the allowed folders was read." );
            Assert.IsFalse( css.Contains( "STYLES-CONFIG" ), "A file with a disallowed extension was read." );
            Assert.IsFalse( css.Contains( "PREFIX-SECRET" ), "A folder that only starts with an allowed name was read." );
            Assert.IsFalse( css.Contains( "RESOURCE-SECRET" ), "An embedded resource was read." );
        }

        #endregion Helpers
    }
}
