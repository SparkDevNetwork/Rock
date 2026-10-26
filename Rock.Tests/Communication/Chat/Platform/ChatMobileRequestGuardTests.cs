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
        public void AnImpersonationCookie_IsRefused()
        {
            // An impersonation link signs in under the person's token, not their login, and
            // whoever opened the link is not the person, so it gets no chat.
            var byName = new GenericPrincipal( new FormsIdentity( new FormsAuthenticationTicket( "rckipid=abc123", false, 30 ) ), null );
            var byTicket = new GenericPrincipal( new FormsIdentity( new FormsAuthenticationTicket( 1, "ted.decker", System.DateTime.Now, System.DateTime.Now.AddMinutes( 30 ), false, "true" ) ), null );

            Assert.AreEqual( HttpStatusCode.Forbidden, CheckAs( byName ) );
            Assert.AreEqual( HttpStatusCode.Forbidden, CheckAs( byTicket ) );
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

        #region Which logins may use chat

        [TestMethod]
        public void MayUseChat_AConfirmedLoginThatIsNotLockedOut_May()
        {
            Assert.IsTrue( ChatSessionHelper.MayUseChat( "ted.decker", Login() ) );
        }

        [TestMethod]
        public void MayUseChat_ALoginWithNoConfirmationRecorded_May()
        {
            // Rock's web sign-in counts a login with no confirmation recorded as confirmed, so
            // nobody who can sign in today loses chat.
            var login = Login();
            login.IsConfirmed = null;
            login.IsLockedOut = null;

            Assert.IsTrue( ChatSessionHelper.MayUseChat( "ted.decker", login ) );
        }

        [TestMethod]
        public void MayUseChat_ALockedOutOrUnconfirmedLogin_MayNot()
        {
            // Rock's mobile launch treats these as signed out, so chat does too.
            var lockedOut = Login();
            lockedOut.IsLockedOut = true;
            var unconfirmed = Login();
            unconfirmed.IsConfirmed = false;

            Assert.IsFalse( ChatSessionHelper.MayUseChat( "ted.decker", lockedOut ), "locked out" );
            Assert.IsFalse( ChatSessionHelper.MayUseChat( "ted.decker", unconfirmed ), "unconfirmed" );
            Assert.IsFalse( ChatSessionHelper.MayUseChat( "ted.decker", null ), "no login" );
        }

        [TestMethod]
        public void MayUseChat_AnImpersonation_MayNot()
        {
            // On the web an impersonation's login is made up for the request: the person's name,
            // no id. On mobile the sign-in names the token.
            var madeUp = new Rock.Model.UserLogin { UserName = "Ted Decker", PersonId = 1, IsConfirmed = true };

            Assert.IsFalse( ChatSessionHelper.MayUseChat( "rckipid=abc123", madeUp ), "the web's impersonated login" );
            Assert.IsFalse( ChatSessionHelper.MayUseChat( "ted.decker", madeUp ), "a login Rock never stored" );
            Assert.IsFalse( ChatSessionHelper.MayUseChat( "rckipid=abc123", Login() ), "a token sign-in" );
        }

        #endregion

        #region Wiring

        [TestMethod]
        public void BothMobileRoutes_RunTheGuardBeforeAnythingElse()
        {
            // Each route is called as a browser would call it. Were the guard skipped, the route
            // would go on to read the database, which a unit test has none of.
            var controller = new Rock.Rest.v2.ChatController();

            foreach ( var route in new System.Func<System.Web.Http.IHttpActionResult>[] { controller.MobileSession, controller.MobileToken } )
            {
                controller.Request = BrowserRequest( System.Net.Http.HttpMethod.Post );
                Assert.AreEqual( HttpStatusCode.Forbidden, ( ( System.Web.Http.Results.StatusCodeResult ) route() ).StatusCode, route.Method.Name );

                controller.Request = BrowserRequest( System.Net.Http.HttpMethod.Get );
                Assert.AreEqual( HttpStatusCode.MethodNotAllowed, ( ( System.Web.Http.Results.StatusCodeResult ) route() ).StatusCode, route.Method.Name );
            }
        }

        [TestMethod]
        public void TheWebChatBlock_AsksWhoMayUseChat_InEveryAction()
        {
            // The unit tests cannot load the blocks project, so its source is read: the block finds
            // the person in one place, and that place asks the same question the mobile routes do.
            var source = System.IO.File.ReadAllText( System.IO.Path.Combine( RepositoryRoot(), "Rock.Blocks", "Communication", "Chat", "ChatShell.cs" ) );

            Assert.AreEqual( 1, System.Text.RegularExpressions.Regex.Matches( source, @"GetCurrentPerson\(\)" ).Count, "the block reads the person somewhere other than its chat person" );
            Assert.IsTrue( source.Contains( "ChatSessionHelper.MayUseChat(" ), "the block's chat person does not ask who may use chat" );
            var actions = System.Text.RegularExpressions.Regex.Matches( source, @"\[BlockAction\]" ).Count + 1;
            Assert.AreEqual( actions, System.Text.RegularExpressions.Regex.Matches( source, @"(?<!Person )GetChatPerson\(\)" ).Count, "every action and the page load find the person as chat's" );
        }

        #endregion

        #region Support

        private static Rock.Model.UserLogin Login()
        {
            return new Rock.Model.UserLogin { Id = 7, UserName = "ted.decker", PersonId = 1, IsConfirmed = true, IsLockedOut = false };
        }

        private static System.Net.Http.HttpRequestMessage BrowserRequest( System.Net.Http.HttpMethod method )
        {
            var request = new System.Net.Http.HttpRequestMessage( method, "https://church.example/api/v2/chat/mobile/session" )
            {
                Content = new System.Net.Http.StringContent( "{}", System.Text.Encoding.UTF8, "application/json" )
            };
            request.Headers.Add( "Origin", "https://evil.example" );

            return request;
        }

        private static string RepositoryRoot()
        {
            var directory = new System.IO.DirectoryInfo( System.AppDomain.CurrentDomain.BaseDirectory );
            while ( directory != null && !System.IO.Directory.Exists( System.IO.Path.Combine( directory.FullName, "Rock.Blocks" ) ) )
            {
                directory = directory.Parent;
            }

            Assert.IsNotNull( directory, "the test runs outside the repository" );
            return directory.FullName;
        }

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
