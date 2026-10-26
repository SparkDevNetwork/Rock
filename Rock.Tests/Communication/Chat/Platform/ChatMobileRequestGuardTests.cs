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
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Security;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Session;

namespace Rock.Tests.Communication.Chat.Platform
{
    /// <summary>
    /// The guard in front of Rock's mobile chat routes. The person's session cookie is the
    /// credential there, and a browser sends that cookie on its own, so the guard must let
    /// through only the shape the native app sends and nothing a web page can make a browser
    /// send. Each test changes one fact of the app's own request and expects a refusal.
    /// </summary>
    [TestClass]
    public class ChatMobileRequestGuardTests
    {
        #region The app's own request

        [TestMethod]
        public void TheAppsOwnRequest_Passes()
        {
            Assert.AreEqual( HttpStatusCode.OK, Check() );
        }

        [TestMethod]
        public void JsonWithACharset_Passes()
        {
            // The app's HTTP client names the charset beside the media type.
            Assert.AreEqual( HttpStatusCode.OK, Check( contentType: "application/json; charset=utf-8" ) );
            Assert.AreEqual( HttpStatusCode.OK, Check( contentType: "Application/JSON" ) );
        }

        #endregion

        #region Browsers

        [TestMethod]
        public void AnyOrigin_IsRefused_EvenWithAValidCookieAndTheAppHeaders()
        {
            // The native app sends no Origin and every browser sends one on a cross-site POST, so
            // a church's own CORS allow-list can never open these routes to a web page.
            Assert.AreEqual( HttpStatusCode.Forbidden, Check( origin: "https://evil.example" ) );
            Assert.AreEqual( HttpStatusCode.Forbidden, Check( origin: "null" ) );
            Assert.AreEqual( HttpStatusCode.Forbidden, Check( origin: string.Empty ) );
        }

        [TestMethod]
        public void WithoutTheAppHeaders_IsRefused()
        {
            // No site resolved from X-Rock-App-Id and X-Rock-Mobile-Api-Key, or a site that is
            // not a mobile application.
            Assert.AreEqual( HttpStatusCode.Forbidden, Check( isMobileSite: false ) );
        }

        [TestMethod]
        public void AFormOrPlainTextPost_IsRefused()
        {
            // The three bodies a page can post across sites without asking the server first.
            Assert.AreEqual( HttpStatusCode.UnsupportedMediaType, Check( contentType: "application/x-www-form-urlencoded" ) );
            Assert.AreEqual( HttpStatusCode.UnsupportedMediaType, Check( contentType: "multipart/form-data; boundary=x" ) );
            Assert.AreEqual( HttpStatusCode.UnsupportedMediaType, Check( contentType: "text/plain" ) );
            Assert.AreEqual( HttpStatusCode.UnsupportedMediaType, Check( contentType: "text/plain; a=application/json" ) );
            Assert.AreEqual( HttpStatusCode.UnsupportedMediaType, Check( contentType: null ) );
        }

        [TestMethod]
        public void AGet_IsRefused()
        {
            Assert.AreEqual( HttpStatusCode.MethodNotAllowed, Check( method: "GET" ) );
            Assert.AreEqual( HttpStatusCode.MethodNotAllowed, Check( method: "PUT" ) );
        }

        #endregion

        #region Who is asking

        [TestMethod]
        public void AnApiKeyOrJwtPrincipal_IsRefused()
        {
            // Rock's API key and JSON web token sign-ins name the user on a plain identity, so a
            // REST user or an integration key can never mint a chat token.
            var apiKey = new GenericPrincipal( new GenericIdentity( "integration" ), null );

            Assert.AreEqual( HttpStatusCode.Forbidden, CheckAs( apiKey ) );
        }

        [TestMethod]
        public void AnOAuthClientPrincipal_IsRefused()
        {
            var oauth = new ClaimsPrincipal( new ClaimsIdentity( new[] { new Claim( "client_id", "app" ), new Claim( ClaimTypes.Name, "someone" ) }, "Bearer" ) );

            Assert.AreEqual( HttpStatusCode.Forbidden, CheckAs( oauth ) );
        }

        [TestMethod]
        public void NobodySignedIn_IsToldToSignIn()
        {
            Assert.AreEqual( HttpStatusCode.Unauthorized, CheckAs( null ) );
            Assert.AreEqual( HttpStatusCode.Unauthorized, CheckAs( new GenericPrincipal( new GenericIdentity( string.Empty ), null ) ) );
        }

        [TestMethod]
        public void ABrowserRefusal_ComesBeforeTheSignInCheck()
        {
            // A page probing these routes learns nothing about whether its visitor is signed in.
            Assert.AreEqual( HttpStatusCode.Forbidden, CheckAs( null, origin: "https://evil.example" ) );
        }

        #endregion

        #region Support

        private static IPrincipal CookiePrincipal()
        {
            return new GenericPrincipal( new FormsIdentity( new FormsAuthenticationTicket( "ted.decker", false, 30 ) ), null );
        }

        // The app's own request, with one fact changed.
        private static HttpStatusCode Check( string method = "POST", string origin = null, string contentType = "application/json", bool isMobileSite = true )
        {
            return ChatSessionHelper.CheckMobileRequest( method, origin, contentType, isMobileSite, CookiePrincipal() );
        }

        // The app's own request, from someone signed in some other way, or nobody.
        private static HttpStatusCode CheckAs( IPrincipal principal, string origin = null )
        {
            return ChatSessionHelper.CheckMobileRequest( "POST", origin, "application/json", true, principal );
        }

        #endregion
    }
}
