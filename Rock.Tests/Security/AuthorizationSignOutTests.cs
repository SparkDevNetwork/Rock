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
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Communication.Chat.Platform.Session;
using Rock.Security;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Security
{
    /// <summary>
    /// Signing out expires the cookie the Chat block keeps while nobody has signed out on the
    /// browser, so the next Chat block there forgets the unsent drafts the last person left. A
    /// browser that never had the cookie is sent nothing.
    /// </summary>
    [TestClass]
    public class AuthorizationSignOutTests
    {
        private TestHelper.RockAppScope _app;

        [TestInitialize]
        public void TestInitialize()
        {
            // Sign-out reads the cookie domain and the SameSite setting from the global attributes.
            _app = TestHelper.CreateScopedRockApp();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            HttpContext.Current = null;
            _app.Dispose();
        }

        [TestMethod]
        public void SignOut_WithTheChatSignedInCookie_ExpiresItAtTheSiteRoot()
        {
            SignInWithCookies( $"{ChatSessionHelper.SignedInCookieName}=1" );

            Authorization.SignOut();

            Assert.IsTrue( HttpContext.Current.Response.Cookies.AllKeys.Contains( ChatSessionHelper.SignedInCookieName ) );
            var cookie = HttpContext.Current.Response.Cookies.Get( ChatSessionHelper.SignedInCookieName );

            // Rock writes the SameSite setting after the path.
            Assert.AreEqual( "/", cookie.Path.Split( ';' )[0] );
            Assert.IsTrue( cookie.Expires < DateTime.Now, "The cookie must be expired so the browser drops it." );
        }

        [TestMethod]
        public void SignOut_WithoutTheChatSignedInCookie_SendsNone()
        {
            SignInWithCookies( string.Empty );

            Authorization.SignOut();

            Assert.IsFalse( HttpContext.Current.Response.Cookies.AllKeys.Contains( ChatSessionHelper.SignedInCookieName ) );
        }

        /// <summary>
        /// Starts a request that carries the cookies in its header, as a browser sends them, beside
        /// a sign-in under a cookie domain, which sign-out expires itself; the framework's own
        /// sign-out, taken without one, needs a configured site a test host does not have.
        /// </summary>
        /// <param name="cookies">The other cookies, as a Cookie header lists them.</param>
        private static void SignInWithCookies( string cookies )
        {
            var domainCookie = $"{FormsAuthentication.FormsCookieName}_DOMAIN=church.example";
            var header = cookies.Length > 0 ? $"{cookies}; {domainCookie}" : domainCookie;
            HttpContext.Current = new HttpContext( new CookieRequest( header ) );
        }

        /// <summary>
        /// A request from a browser that sends the given Cookie header.
        /// </summary>
        private sealed class CookieRequest : SimpleWorkerRequest
        {
            private readonly string _cookieHeader;

            public CookieRequest( string cookieHeader )
                : base( "/", AppDomain.CurrentDomain.BaseDirectory, "page", string.Empty, new StringWriter() )
            {
                _cookieHeader = cookieHeader;
            }

            public override string GetKnownRequestHeader( int index )
            {
                return index == HeaderCookie ? _cookieHeader : base.GetKnownRequestHeader( index );
            }
        }
    }
}
