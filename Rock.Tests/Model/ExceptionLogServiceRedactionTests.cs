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
using System.Web.Routing;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

namespace Rock.Tests.Model
{
    /// <summary>
    /// This suite checks the <see cref="ExceptionLogService"/> redaction helpers
    /// that keep cookies, credential headers, rckipid tokens and other sensitive
    /// values out of the exception log.
    /// </summary>
    [TestClass]
    public class ExceptionLogServiceRedactionTests
    {
        private const string Redacted = "obfuscated";

        #region RedactQueryParameters

        [TestMethod]
        [DataRow( "a=1&apikey=x", "a=1&apikey=obfuscated" )]
        [DataRow( "/page/1?rckipid=x&PersonId=2", "/page/1?rckipid=obfuscated&PersonId=2" )]
        [DataRow( "?RCKIPID=x", "?RCKIPID=obfuscated" )]
        [DataRow( "?api%6Bey=x", "?api%6Bey=obfuscated" )]
        [DataRow( "?rckid=x", "?rckid=obfuscated" )]
        [DataRow( "?PersonActionIdentifier=x", "?PersonActionIdentifier=obfuscated" )]
        [DataRow( "?access_token=x&id_token=y", "?access_token=obfuscated&id_token=obfuscated" )]
        [DataRow( "?Password=x", "?Password=obfuscated" )]
        [DataRow( "?client_secret=x", "?client_secret=obfuscated" )]
        [DataRow( "?cc=x", "?cc=obfuscated" )]
        [DataRow( "?State=s&Code=1", "?State=s&Code=obfuscated" )]
        [DataRow( "?zipcode=1", "?zipcode=1" )]
        [DataRow( "PersonId=2", "PersonId=2" )]
        [DataRow( "rckipid=abc", "rckipid=obfuscated" )]
        public void RedactQueryParameters_RedactsSensitiveValues( string text, string expected )
        {
            Assert.AreEqual( expected, ExceptionLogService.RedactQueryParameters( text ) );
        }

        [TestMethod]
        [DataRow( "?password=hunter%202&a=1", "?password=obfuscated&a=1" )]
        [DataRow( "?password=hunter 2&a=1", "?password=obfuscated&a=1" )]
        [DataRow( "?apikey=ab;cd&a=1", "?apikey=obfuscated&a=1" )]
        [DataRow( "?token=ab cd;ef#frag", "?token=obfuscated#frag" )]
        [DataRow( "a=1;token=x", "a=1;token=obfuscated" )]
        [DataRow( "?a=b c&d=1", "?a=b c&d=1" )]
        public void RedactQueryParameters_SensitiveValue_IsRedactedToNextAmpersandOrHash( string text, string expected )
        {
            Assert.AreEqual( expected, ExceptionLogService.RedactQueryParameters( text ) );
        }

        [TestMethod]
        [DataRow( "?returnurl=%2Fpage%3Frckipid%3DDDD", "?returnurl=%2Fpage%3Frckipid%3Dobfuscated" )]
        [DataRow( "?redirect=/page?rckipid=CCC&x=1", "?redirect=/page?rckipid=obfuscated&x=1" )]
        [DataRow( "?a=1?rckipid=BBB", "?a=1?rckipid=obfuscated" )]
        [DataRow( "?x=a%26rckipid%3DMMM", "?x=a%26rckipid%3Dobfuscated" )]
        [DataRow( "?x=a%26rckipid%3DMMM%26b%3D1&c=2", "?x=a%26rckipid%3Dobfuscated%26b%3D1&c=2" )]
        [DataRow( "?returnurl=%2fpage%3fRCKIPID%3dDDD", "?returnurl=%2fpage%3fRCKIPID%3dobfuscated" )]
        [DataRow( "?returnurl=%2Fauth%3Faccess_token%3DT1%26Code%3DC1", "?returnurl=%2Fauth%3Faccess_token%3Dobfuscated%26Code%3Dobfuscated" )]
        [DataRow( "?returnurl=%2Fpage%3Fapikey%3DK1#frag", "?returnurl=%2Fpage%3Fapikey%3Dobfuscated#frag" )]
        [DataRow( "?returnurl=%2Fpage%3Fzipcode%3D1", "?returnurl=%2Fpage%3Fzipcode%3D1" )]
        public void RedactQueryParameters_NestedSensitiveValue_IsRedacted( string text, string expected )
        {
            Assert.AreEqual( expected, ExceptionLogService.RedactQueryParameters( text ) );
        }

        [TestMethod]
        public void RedactQueryParameters_NullOrEmpty_ReturnsInput()
        {
            Assert.IsNull( ExceptionLogService.RedactQueryParameters( null ) );
            Assert.AreEqual( string.Empty, ExceptionLogService.RedactQueryParameters( string.Empty ) );
        }

        #endregion

        #region RedactServerVariable

        [TestMethod]
        [DataRow( "HTTP_AUTHORIZATION_TOKEN" )]
        [DataRow( "HTTP_AUTHORIZATION" )]
        [DataRow( "HTTP_COOKIE" )]
        [DataRow( "ALL_HTTP" )]
        [DataRow( "ALL_RAW" )]
        [DataRow( "AUTH_PASSWORD" )]
        [DataRow( "HTTP_X_ROCK_MOBILE_API_KEY" )]
        [DataRow( "CERT_COOKIE" )]
        [DataRow( "HTTP_CF_ACCESS_JWT_ASSERTION" )]
        [DataRow( "HTTP_X_AMZN_OIDC_DATA" )]
        [DataRow( "HTTP_X_AMZN_OIDC_IDENTITY" )]
        public void RedactServerVariable_CredentialVariable_IsRedacted( string name )
        {
            Assert.AreEqual( Redacted, ExceptionLogService.RedactServerVariable( name, "secret-value", null ) );
        }

        [TestMethod]
        public void RedactServerVariable_UserAgent_IsUnchanged()
        {
            Assert.AreEqual( "Mozilla/5.0", ExceptionLogService.RedactServerVariable( "HTTP_USER_AGENT", "Mozilla/5.0", null ) );
        }

        [TestMethod]
        public void RedactServerVariable_AuthUserWithRckipid_IsRedacted()
        {
            Assert.AreEqual( "rckipid=obfuscated", ExceptionLogService.RedactServerVariable( "AUTH_USER", "rckipid=abc", null ) );
        }

        [TestMethod]
        public void RedactServerVariable_QueryString_RedactsSensitiveValues()
        {
            Assert.AreEqual( "apikey=obfuscated&a=1", ExceptionLogService.RedactServerVariable( "QUERY_STRING", "apikey=x&a=1", null ) );
        }

        [TestMethod]
        public void RedactServerVariable_RouteToken_IsReplaced()
        {
            var result = ExceptionLogService.RedactServerVariable( "URL", "/MyAccount/TOK123456789ABCDEF", "TOK123456789ABCDEF" );

            Assert.IsFalse( result.Contains( "TOK123456789ABCDEF" ) );
            Assert.IsTrue( result.StartsWith( "/MyAccount/" ) );
        }

        [TestMethod]
        [DataRow( "UNENCODED_URL", "/MyAccount/AbCd%2BEfGh%212bIjKlMnOp?a=1" )]
        [DataRow( "HTTP_X_ORIGINAL_URL", "/MyAccount/AbCd%2bEfGh!2bIjKlMnOp?a=1" )]
        [DataRow( "PATH_INFO", "/MyAccount/AbCd+EfGh!2bIjKlMnOp" )]
        public void RedactServerVariable_EscapedRouteToken_IsReplaced( string name, string value )
        {
            var result = ExceptionLogService.RedactServerVariable( name, value, "AbCd+EfGh!2bIjKlMnOp" );

            Assert.IsFalse( result.Contains( "EfGh" ), result );
            Assert.IsTrue( result.StartsWith( "/MyAccount/XXXX" ), result );
        }

        [TestMethod]
        public void RedactServerVariable_ShortRouteToken_IsNotReplaced()
        {
            Assert.AreEqual( "/page/e/edit", ExceptionLogService.RedactServerVariable( "URL", "/page/e/edit", "e" ) );
            Assert.AreEqual( "/page/123456789012345", ExceptionLogService.RedactServerVariable( "URL", "/page/123456789012345", "123456789012345" ) );
        }

        [TestMethod]
        public void RedactServerVariable_ReferrerRouteSegment_IsReplaced()
        {
            var route = new Route( "TH087RedactionTest/{rckipid}", new StopRoutingHandler() );
            RouteTable.Routes.Add( route );

            try
            {
                var result = ExceptionLogService.RedactServerVariable( "HTTP_REFERER", "http://h/TH087RedactionTest/AbCdEfGhIjKlMnOpQr?PersonId=2", null );

                Assert.AreEqual( "http://h/TH087RedactionTest/XXXXXXXXXXXXXXXXXXXXXXXXXXXX?PersonId=2", result );
            }
            finally
            {
                RouteTable.Routes.Remove( route );
            }
        }

        [TestMethod]
        public void RedactServerVariable_Referrer_RedactsRckipid()
        {
            var result = ExceptionLogService.RedactServerVariable( "HTTP_REFERER", "http://h/page/1?rckipid=TOK123&PersonId=2", null );

            Assert.IsFalse( result.Contains( "TOK123" ) );
            Assert.IsTrue( result.Contains( "PersonId=2" ) );
        }

        [TestMethod]
        public void RedactServerVariable_NullOrEmpty_ReturnsInput()
        {
            Assert.IsNull( ExceptionLogService.RedactServerVariable( "HTTP_COOKIE", null, null ) );
            Assert.AreEqual( string.Empty, ExceptionLogService.RedactServerVariable( "HTTP_COOKIE", string.Empty, null ) );
        }

        #endregion

        #region RedactUrl

        [TestMethod]
        public void RedactUrl_RedactsRckipidAndToken()
        {
            var result = ExceptionLogService.RedactUrl( "http://h/p?rckipid=x1&token=y1&PersonId=2" );

            Assert.AreEqual( "http://h/p?rckipid=obfuscated&token=obfuscated&PersonId=2", result );
        }

        [TestMethod]
        public void RedactUrl_UppercaseRckipid_IsRedacted()
        {
            var result = ExceptionLogService.RedactUrl( "http://h/p?RCKIPID=x1" );

            Assert.AreEqual( "http://h/p?RCKIPID=obfuscated", result );
        }

        [TestMethod]
        [DataRow( "http://h/p?password=hunter%202&a=1", "http://h/p?password=obfuscated&a=1" )]
        [DataRow( "http://h/p?apikey=ab;cd&a=1", "http://h/p?apikey=obfuscated&a=1" )]
        [DataRow( "http://h/p?password=a%26b&a=1", "http://h/p?password=obfuscated&a=1" )]
        [DataRow( "http://h/p?returnurl=%2Fpage%3Frckipid%3DDDD", "http://h/p?returnurl=%2Fpage%3Frckipid%3Dobfuscated" )]
        public void RedactUrl_SensitiveValue_IsFullyRedacted( string url, string expected )
        {
            Assert.AreEqual( expected, ExceptionLogService.RedactUrl( url ) );
        }

        [TestMethod]
        public void RedactUrl_NoSensitiveValues_IsUnchanged()
        {
            Assert.AreEqual( "http://h/page/1?PersonId=2", ExceptionLogService.RedactUrl( "http://h/page/1?PersonId=2" ) );
        }

        #endregion
    }
}
