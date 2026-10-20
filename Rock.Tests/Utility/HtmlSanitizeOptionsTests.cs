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

            Assert.ThrowsException<InvalidOperationException>( () => options.AllowTag( "b" ) );
            Assert.ThrowsException<InvalidOperationException>( () => options.DisallowTag( "b" ) );
            Assert.ThrowsException<InvalidOperationException>( () => options.AllowAttribute( "b", "title" ) );
            Assert.ThrowsException<InvalidOperationException>( () => options.DisallowAttribute( "b", "title" ) );
            Assert.ThrowsException<InvalidOperationException>( () => options.AllowDataAttributes() );
            Assert.ThrowsException<InvalidOperationException>( () => options.DisallowDataAttributes() );
            Assert.ThrowsException<InvalidOperationException>( () => options.AllowUrlScheme( "https" ) );
            Assert.ThrowsException<InvalidOperationException>( () => options.DisallowUrlScheme( "https" ) );
            Assert.ThrowsException<InvalidOperationException>( () => options.AllowCssProperty( "color" ) );
            Assert.ThrowsException<InvalidOperationException>( () => options.DisallowCssProperty( "color" ) );
            Assert.ThrowsException<InvalidOperationException>( () => options.SetRemovalMode( "font", HtmlTagRemovalMode.Unwrap ) );
            Assert.ThrowsException<InvalidOperationException>( () => options.SetDefaultRemovalMode( HtmlTagRemovalMode.Unwrap ) );
        }

        [TestMethod]
        public void EditMethods_OnPresets_ThrowInvalidOperationException()
        {
            Assert.ThrowsException<InvalidOperationException>( () => HtmlSanitizeOptions.Simple.AllowTag( "font" ) );
            Assert.ThrowsException<InvalidOperationException>( () => HtmlSanitizeOptions.Rich.DisallowTag( "img" ) );
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
            Assert.ThrowsException<ArgumentNullException>( () => new HtmlSanitizeOptions( null ) );
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
            Assert.ThrowsException<ArgumentNullException>( () => Sanitize( "<b>x</b>", null ) );
            Assert.ThrowsException<ArgumentNullException>( () => Sanitize( null, null ) );
        }

        [DataTestMethod]
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

            Assert.ThrowsException<ArgumentException>( () => options.AllowTag( tag ) );
            Assert.ThrowsException<ArgumentException>( () => options.SetRemovalMode( tag, HtmlTagRemovalMode.Unwrap ) );
            Assert.ThrowsException<ArgumentException>( () => options.SetRemovalMode( tag, HtmlTagRemovalMode.Drop ) );
        }

        [DataTestMethod]
        [DataRow( "svg" )]
        [DataRow( "MATH" )]
        public void ForeignContentRoot_CanBeAllowedButNotGivenARemovalMode( string tag )
        {
            var options = new HtmlSanitizeOptions();

            Assert.AreSame( options, options.AllowTag( tag ) );
            Assert.ThrowsException<ArgumentException>( () => options.SetRemovalMode( tag, HtmlTagRemovalMode.Unwrap ) );
            Assert.ThrowsException<ArgumentException>( () => options.SetRemovalMode( tag, HtmlTagRemovalMode.Drop ) );
        }

        [DataTestMethod]
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

            Assert.ThrowsException<ArgumentException>( () => options.AllowAttribute( "img", attribute ) );
            Assert.ThrowsException<ArgumentException>( () => options.AllowAttribute( "", attribute ) );
        }

        [DataTestMethod]
        [DataRow( "javascript" )]
        [DataRow( "JavaScript" )]
        [DataRow( "vbscript" )]
        [DataRow( "file" )]
        [DataRow( "data" )]
        public void AllowUrlScheme_BlockedScheme_ThrowsArgumentException( string scheme )
        {
            Assert.ThrowsException<ArgumentException>( () => new HtmlSanitizeOptions().AllowUrlScheme( scheme ) );
        }

        [DataTestMethod]
        [DataRow( "behavior" )]
        [DataRow( "BEHAVIOR" )]
        [DataRow( "-moz-binding" )]
        public void AllowCssProperty_NeverAllowedProperty_ThrowsArgumentException( string property )
        {
            Assert.ThrowsException<ArgumentException>( () => new HtmlSanitizeOptions().AllowCssProperty( property ) );
        }

        [DataTestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "  " )]
        public void BlankName_ThrowsArgumentException( string name )
        {
            var options = new HtmlSanitizeOptions();

            Assert.ThrowsException<ArgumentException>( () => options.AllowTag( name ) );
            Assert.ThrowsException<ArgumentException>( () => options.DisallowTag( name ) );
            Assert.ThrowsException<ArgumentException>( () => options.SetRemovalMode( name, HtmlTagRemovalMode.Unwrap ) );
            Assert.ThrowsException<ArgumentException>( () => options.AllowAttribute( "span", name ) );
            Assert.ThrowsException<ArgumentException>( () => options.AllowAttribute( "", name ) );
            Assert.ThrowsException<ArgumentException>( () => options.DisallowAttribute( "span", name ) );
            Assert.ThrowsException<ArgumentException>( () => options.DisallowAttribute( "", name ) );
            Assert.ThrowsException<ArgumentException>( () => options.AllowUrlScheme( name ) );
            Assert.ThrowsException<ArgumentException>( () => options.DisallowUrlScheme( name ) );
            Assert.ThrowsException<ArgumentException>( () => options.AllowCssProperty( name ) );
            Assert.ThrowsException<ArgumentException>( () => options.DisallowCssProperty( name ) );
        }

        [DataTestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "  " )]
        public void AllowAttribute_BlankTag_AllowsOnEveryTag( string tag )
        {
            var options = new HtmlSanitizeOptions().AllowTag( "b" ).AllowTag( "i" ).AllowAttribute( tag, "data-x" );

            Assert.AreEqual( "<b data-x=\"1\">a</b><i data-x=\"2\">b</i>", Sanitize( "<b data-x=\"1\">a</b><i data-x=\"2\">b</i>", options ) );
        }

        [DataTestMethod]
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

            Assert.ThrowsException<ArgumentException>( () => options.DisallowAttribute( "a", "title" ) );
            Assert.ThrowsException<ArgumentException>( () => options.DisallowAttribute( "span", "CLASS" ) );
        }

        [TestMethod]
        public void UndefinedRemovalMode_ThrowsArgumentOutOfRangeException()
        {
            var options = new HtmlSanitizeOptions();

            Assert.ThrowsException<ArgumentOutOfRangeException>( () => options.SetRemovalMode( "font", ( HtmlTagRemovalMode ) 2 ) );
            Assert.ThrowsException<ArgumentOutOfRangeException>( () => options.SetDefaultRemovalMode( ( HtmlTagRemovalMode ) 2 ) );
        }
    }
}
