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

using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Enums.Security;
using Rock.Field.Types;
using Rock.Jobs;

namespace Rock.Tests.Field.Types
{
    /// <summary>
    /// Unit tests for the <see cref="SocialMediaAccountFieldType"/> field type
    /// that do not require any database access.
    /// </summary>
    [TestClass]
    public class SocialMediaAccountFieldTypeTests
    {
        #region GetSafeUrl

        /*
            The field type and the post update job each have their own copy
            of GetSafeUrl so the job does not depend on the field type class.
            Every test checks both copies so they can't drift apart.
        */

        [TestMethod]
        [DataRow( "https://www.facebook.com/john.doe" )]
        [DataRow( "http://www.facebook.com/john.doe" )]
        [DataRow( "HTTPS://www.facebook.com/john.doe" )]
        [DataRow( "https://www.facebook.com/profile.php?id=123&ref=abc" )]
        [DataRow( "https://twitter.com/+name" )]
        [DataRow( "//www.facebook.com/john.doe" )]
        [DataRow( "myname" )]
        [DataRow( "facebook.com/john.doe" )]
        [DataRow( "" )]
        public void GetSafeUrl_WithSafeValue_ReturnsSameValue( string value )
        {
            Assert.AreEqual( value, SocialMediaAccountFieldType.GetSafeUrl( value ) );
            Assert.AreEqual( value, PostV1711CleanSocialMediaAccountValues.GetSafeUrl( value ) );
        }

        [TestMethod]
        [DataRow( "  https://www.facebook.com/john.doe  ", "https://www.facebook.com/john.doe" )]
        [DataRow( "https://www.facebook.com/+'+autofocus+onfocus='bad_javascript'", "https://www.facebook.com/+%27+autofocus+onfocus=%27bad_javascript%27" )]
        [DataRow( "' autofocus onfocus='x'", "%27%20autofocus%20onfocus=%27x%27" )]
        [DataRow( "\" onmouseover=\"x\"", "%22%20onmouseover=%22x%22" )]
        [DataRow( "\"><script>alert(1)</script>", "%22%3E%3Cscript%3Ealert(1)%3C/script%3E" )]
        [DataRow( "` onfocus=`x`", "%60%20onfocus=%60x%60" )]
        [DataRow( "java\tscript:alert(1)", "java%09script:alert(1)" )]
        [DataRow( "\u0001javascript:alert(1)", "%01javascript:alert(1)" )]
        public void GetSafeUrl_WithUnsafeCharacters_ReturnsEncodedValue( string value, string expected )
        {
            Assert.AreEqual( expected, SocialMediaAccountFieldType.GetSafeUrl( value ) );
            Assert.AreEqual( expected, PostV1711CleanSocialMediaAccountValues.GetSafeUrl( value ) );
        }

        [TestMethod]
        [DataRow( "javascript:alert(1)" )]
        [DataRow( "JaVaScRiPt:alert(1)" )]
        [DataRow( "  javascript:alert(1)" )]
        [DataRow( "vbscript:msgbox(1)" )]
        [DataRow( "data:text/html,<script>alert(1)</script>" )]
        [DataRow( "&#106;avascript:alert(1)" )]
        [DataRow( "&#x6A;avascript:alert(1)" )]
        [DataRow( "javascript&colon;alert(1)" )]
        [DataRow( "java&Tab;script:alert(1)" )]
        public void GetSafeUrl_WithUnsafeValue_ReturnsNull( string value )
        {
            Assert.IsNull( SocialMediaAccountFieldType.GetSafeUrl( value ) );
            Assert.IsNull( PostV1711CleanSocialMediaAccountValues.GetSafeUrl( value ) );
        }

        #endregion

        #region GetHtmlValue

        [TestMethod]
        public void GetHtmlValue_WithUnsafeValue_ReturnsEncodedText()
        {
            var fieldType = new SocialMediaAccountFieldType();

            var html = fieldType.GetHtmlValue( "javascript:alert('<x>')", new Dictionary<string, string>() );

            Assert.AreEqual( "javascript:alert(&#39;&lt;x&gt;&#39;)", html );
        }

        [TestMethod]
        public void GetHtmlValue_WithoutConfiguration_ReturnsSafeValue()
        {
            var fieldType = new SocialMediaAccountFieldType();

            var html = fieldType.GetHtmlValue( "https://www.facebook.com/+'+onfocus='x'", null );

            Assert.AreEqual( "https://www.facebook.com/+%27+onfocus=%27x%27", html );
        }

        #endregion

        #region GetValidationRules

        [TestMethod]
        public void GetValidationRules_IncludesEventHandlerAttributes()
        {
            var fieldType = new SocialMediaAccountFieldType();

            var rules = fieldType.GetValidationRules( new Dictionary<string, string>() );

            Assert.IsTrue( rules.HasFlag( StringValidationRule.EventHandlerAttributes ) );
            Assert.IsTrue( rules.HasFlag( StringValidationRule.AnyHtmlTags ) );
        }

        #endregion
    }
}
