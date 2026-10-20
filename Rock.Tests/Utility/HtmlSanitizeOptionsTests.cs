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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Utility.HtmlSanitization;

namespace Rock.Tests.Utility
{
    [TestClass]
    public class HtmlSanitizeOptionsTests
    {
        private static string Sanitize( string html, HtmlSanitizeOptions options )
        {
            return new HtmlAllowListSanitizer().Sanitize( html, options );
        }

        [TestMethod]
        public void Presets_AreReadOnly()
        {
            Assert.IsTrue( HtmlSanitizeOptions.Simple.IsReadOnly );
            Assert.IsTrue( HtmlSanitizeOptions.Rich.IsReadOnly );
        }

        [TestMethod]
        public void NewOptions_AreEditableAndAllowOnlyText()
        {
            var options = new HtmlSanitizeOptions();

            Assert.IsFalse( options.IsReadOnly );
            Assert.AreEqual( "a  c", Sanitize( "a <b>b</b> c", options ) );
        }

        [TestMethod]
        public void EditMethods_OnReadOnlyOptions_ThrowInvalidOperationException()
        {
            var options = new HtmlSanitizeOptions().MakeReadOnly();

            Assert.ThrowsExactly<InvalidOperationException>( () => options.AllowTag( "b" ) );
            Assert.ThrowsExactly<InvalidOperationException>( () => options.DisallowTag( "b" ) );
            Assert.ThrowsExactly<InvalidOperationException>( () => options.AllowAttribute( "b", "title" ) );
            Assert.ThrowsExactly<InvalidOperationException>( () => options.DisallowAttribute( "b", "title" ) );
            Assert.ThrowsExactly<InvalidOperationException>( () => options.AllowDataAttributes() );
            Assert.ThrowsExactly<InvalidOperationException>( () => options.DisallowDataAttributes() );
            Assert.ThrowsExactly<InvalidOperationException>( () => options.AllowUrlScheme( "https" ) );
            Assert.ThrowsExactly<InvalidOperationException>( () => options.DisallowUrlScheme( "https" ) );
            Assert.ThrowsExactly<InvalidOperationException>( () => options.AllowCssProperty( "color" ) );
            Assert.ThrowsExactly<InvalidOperationException>( () => options.DisallowCssProperty( "color" ) );
            Assert.ThrowsExactly<InvalidOperationException>( () => options.SetRemovalMode( "font", HtmlTagRemovalMode.Unwrap ) );
            Assert.ThrowsExactly<InvalidOperationException>( () => options.SetDefaultRemovalMode( HtmlTagRemovalMode.Unwrap ) );
        }

        [TestMethod]
        public void EditMethods_OnPresets_ThrowInvalidOperationException()
        {
            Assert.ThrowsExactly<InvalidOperationException>( () => HtmlSanitizeOptions.Simple.AllowTag( "font" ) );
            Assert.ThrowsExactly<InvalidOperationException>( () => HtmlSanitizeOptions.Rich.DisallowTag( "img" ) );
        }

        [TestMethod]
        public void MakeReadOnly_ReturnsSameInstanceAndCanBeCalledAgain()
        {
            var options = new HtmlSanitizeOptions();

            Assert.AreSame( options, options.MakeReadOnly() );
            Assert.AreSame( options, options.MakeReadOnly() );
            Assert.IsTrue( options.IsReadOnly );
        }

        [TestMethod]
        public void CopyOfPreset_IsEditableAndDoesNotChangePreset()
        {
            var copy = new HtmlSanitizeOptions( HtmlSanitizeOptions.Simple );

            Assert.IsFalse( copy.IsReadOnly );

            copy.AllowTag( "font" ).AllowAttribute( "b", "data-x" ).SetDefaultRemovalMode( HtmlTagRemovalMode.Unwrap );

            Assert.AreEqual( "<font>a</font><b data-x=\"1\">b</b>c", Sanitize( "<font>a</font><b data-x=\"1\">b</b><blink>c</blink>", copy ) );
            Assert.AreEqual( "<b>b</b>", Sanitize( "<font>a</font><b data-x=\"1\">b</b><blink>c</blink>", HtmlSanitizeOptions.Simple ) );
        }

        [TestMethod]
        public void CopyOfCopy_SharesNoTagAttributeSets()
        {
            var source = new HtmlSanitizeOptions( HtmlSanitizeOptions.Simple ).AllowAttribute( "span", "data-identifier" );
            var copy = new HtmlSanitizeOptions( source ).DisallowAttribute( "span", "data-identifier" );

            Assert.AreEqual( "<span data-identifier=\"1\">a</span>", Sanitize( "<span data-identifier=\"1\">a</span>", source ) );
            Assert.AreEqual( "<span>a</span>", Sanitize( "<span data-identifier=\"1\">a</span>", copy ) );
        }

        [TestMethod]
        public void CopyConstructor_NullSource_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>( () => new HtmlSanitizeOptions( null ) );
        }

        [TestMethod]
        public void Sanitize_MakesOptionsReadOnly()
        {
            var options = new HtmlSanitizeOptions().AllowTag( "b" );

            Assert.AreEqual( "<b>x</b>", Sanitize( "<b>x</b>", options ) );
            Assert.IsTrue( options.IsReadOnly );
        }

        [TestMethod]
        public void Sanitize_FastPathInput_StillMakesOptionsReadOnly()
        {
            var options = new HtmlSanitizeOptions();

            Assert.AreEqual( "plain", Sanitize( "plain", options ) );
            Assert.IsTrue( options.IsReadOnly );
        }

        [TestMethod]
        public void Sanitize_NullOptions_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>( () => Sanitize( "<b>x</b>", null ) );
            Assert.ThrowsExactly<ArgumentNullException>( () => Sanitize( null, null ) );
        }

        [TestMethod]
        [DataRow( "script" )]
        [DataRow( "SCRIPT" )]
        [DataRow( "style" )]
        [DataRow( "iframe" )]
        [DataRow( "object" )]
        [DataRow( "embed" )]
        [DataRow( "form" )]
        [DataRow( "link" )]
        [DataRow( "meta" )]
        [DataRow( "base" )]
        [DataRow( "noscript" )]
        [DataRow( "template" )]
        [DataRow( "frame" )]
        [DataRow( "frameset" )]
        [DataRow( "applet" )]
        [DataRow( "xmp" )]
        [DataRow( "noembed" )]
        [DataRow( "noframes" )]
        [DataRow( "plaintext" )]
        [DataRow( "title" )]
        [DataRow( "animate" )]
        [DataRow( "set" )]
        [DataRow( "foreignObject" )]
        [DataRow( "annotation-xml" )]
        public void NeverPassElement_CanNotBeAllowedOrGivenARemovalMode( string tag )
        {
            var options = new HtmlSanitizeOptions();

            Assert.ThrowsExactly<ArgumentException>( () => options.AllowTag( tag ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.SetRemovalMode( tag, HtmlTagRemovalMode.Unwrap ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.SetRemovalMode( tag, HtmlTagRemovalMode.Drop ) );
        }

        [TestMethod]
        [DataRow( "svg" )]
        [DataRow( "MATH" )]
        public void ForeignContentRoot_CanBeAllowedButNotGivenARemovalMode( string tag )
        {
            var options = new HtmlSanitizeOptions();

            Assert.AreSame( options, options.AllowTag( tag ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.SetRemovalMode( tag, HtmlTagRemovalMode.Unwrap ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.SetRemovalMode( tag, HtmlTagRemovalMode.Drop ) );
        }

        [TestMethod]
        [DataRow( "onclick" )]
        [DataRow( "OnMouseOver" )]
        [DataRow( "on" )]
        [DataRow( "formaction" )]
        [DataRow( "FormAction" )]
        [DataRow( "srcdoc" )]
        [DataRow( "SRCDOC" )]
        [DataRow( "archive" )]
        [DataRow( "ping" )]
        [DataRow( "srcset" )]
        public void AllowAttribute_FixedRuleAttribute_ThrowsArgumentException( string attribute )
        {
            var options = new HtmlSanitizeOptions();

            Assert.ThrowsExactly<ArgumentException>( () => options.AllowAttribute( "img", attribute ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.AllowAttribute( "", attribute ) );
        }

        [TestMethod]
        [DataRow( "javascript" )]
        [DataRow( "JavaScript" )]
        [DataRow( "vbscript" )]
        [DataRow( "file" )]
        [DataRow( "data" )]
        public void AllowUrlScheme_BlockedScheme_ThrowsArgumentException( string scheme )
        {
            Assert.ThrowsExactly<ArgumentException>( () => new HtmlSanitizeOptions().AllowUrlScheme( scheme ) );
        }

        [TestMethod]
        [DataRow( "behavior" )]
        [DataRow( "BEHAVIOR" )]
        [DataRow( "-moz-binding" )]
        public void AllowCssProperty_NeverAllowedProperty_ThrowsArgumentException( string property )
        {
            Assert.ThrowsExactly<ArgumentException>( () => new HtmlSanitizeOptions().AllowCssProperty( property ) );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "  " )]
        public void BlankName_ThrowsArgumentException( string name )
        {
            var options = new HtmlSanitizeOptions();

            Assert.ThrowsExactly<ArgumentException>( () => options.AllowTag( name ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.DisallowTag( name ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.SetRemovalMode( name, HtmlTagRemovalMode.Unwrap ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.AllowAttribute( "span", name ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.AllowAttribute( "", name ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.DisallowAttribute( "span", name ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.DisallowAttribute( "", name ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.AllowUrlScheme( name ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.DisallowUrlScheme( name ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.AllowCssProperty( name ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.DisallowCssProperty( name ) );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "  " )]
        public void AllowAttribute_BlankTag_AllowsOnEveryTag( string tag )
        {
            var options = new HtmlSanitizeOptions().AllowTag( "b" ).AllowTag( "i" ).AllowAttribute( tag, "data-x" );

            Assert.AreEqual( "<b data-x=\"1\">a</b><i data-x=\"2\">b</i>", Sanitize( "<b data-x=\"1\">a</b><i data-x=\"2\">b</i>", options ) );
        }

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "  " )]
        public void DisallowAttribute_BlankTag_RemovesFromEveryTagAndEachTag( string tag )
        {
            var options = new HtmlSanitizeOptions( HtmlSanitizeOptions.Simple )
                .AllowAttribute( "a", "title" )
                .DisallowAttribute( tag, "title" );

            Assert.AreEqual( "<a href=\"/x\">a</a><b>b</b>", Sanitize( "<a href=\"/x\" title=\"t\">a</a><b title=\"t\">b</b>", options ) );
        }

        [TestMethod]
        public void DisallowAttribute_TagWithEveryTagAttribute_ThrowsArgumentException()
        {
            var options = new HtmlSanitizeOptions( HtmlSanitizeOptions.Simple );

            Assert.ThrowsExactly<ArgumentException>( () => options.DisallowAttribute( "a", "title" ) );
            Assert.ThrowsExactly<ArgumentException>( () => options.DisallowAttribute( "span", "CLASS" ) );
        }

        [TestMethod]
        public void UndefinedRemovalMode_ThrowsArgumentOutOfRangeException()
        {
            var options = new HtmlSanitizeOptions();

            Assert.ThrowsExactly<ArgumentOutOfRangeException>( () => options.SetRemovalMode( "font", ( HtmlTagRemovalMode ) 2 ) );
            Assert.ThrowsExactly<ArgumentOutOfRangeException>( () => options.SetDefaultRemovalMode( ( HtmlTagRemovalMode ) 2 ) );
        }
    }
}
