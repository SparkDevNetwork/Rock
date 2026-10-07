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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Model;

namespace Rock.Tests.Model
{
    /// <summary>
    /// This suite checks the AuthClientService object to make sure that all logic works as intended.
    /// </summary>
    /// <seealso cref="AuthClientService"/>
    [TestClass]
    public class AuthClientServiceTests
    {
        #region IsRedirectUriAllowed

        private const string NativeAppRedirectUris = "http://127.0.0.1/callback,http://[::1]/callback,http://localhost/callback";

        private static AuthClient CreateAuthClient( string redirectUri )
        {
            return new AuthClient
            {
                RedirectUri = redirectUri
            };
        }

        [TestMethod]
        // Exact matches.
        [DataRow( "https://app.example.com/oauth/callback", "https://app.example.com/oauth/callback" )]
        [DataRow( "https://a.example/cb,https://b.example/cb", "https://b.example/cb" )]

        // Loopback addresses ignore the port.
        [DataRow( NativeAppRedirectUris, "http://127.0.0.1:53211/callback" )]
        [DataRow( NativeAppRedirectUris, "http://[::1]:53211/callback" )]
        [DataRow( NativeAppRedirectUris, "http://localhost:53211/callback" )]
        [DataRow( "http://127.0.0.1:8080/callback", "http://127.0.0.1:53211/callback" )]
        [DataRow( "http://127.0.0.1:8080/callback", "http://127.0.0.1/callback" )]
        [DataRow( "http://127.0.0.2/callback", "http://127.0.0.2:53211/callback" )]
        [DataRow( "http://localhost/callback?client=app", "http://localhost:53211/callback?client=app" )]
        [DataRow( "http://localhost?client=app", "http://localhost:53211?client=app" )]
        [DataRow( "http://LOCALHOST/callback", "http://LOCALHOST:53211/callback" )]
        public void IsRedirectUriAllowed_AllowsRegisteredUri( string registeredRedirectUris, string redirectUri )
        {
            var output = AuthClientService.IsRedirectUriAllowed( CreateAuthClient( registeredRedirectUris ), redirectUri );
            Assert.IsTrue( output, redirectUri );
        }

        [TestMethod]
        // Anything other than an exact match.
        [DataRow( "https://app.example.com/oauth/callback", "https://unsafe.com/oauth/callback" )]
        [DataRow( "https://app.example.com/oauth/callback", "https://app.example.com/oauth/callback/x" )]
        [DataRow( "https://app.example.com/oauth/callback", "https://app.example.com/oauth/callback/" )]
        [DataRow( "https://app.example.com/oauth/callback", "https://app.example.com/OAuth/callback" )]
        [DataRow( "https://app.example.com/oauth/callback", "https://APP.example.com/oauth/callback" )]
        [DataRow( "https://app.example.com/cb", "https://app.example.com:8443/cb" )]
        [DataRow( "https://a.example/cb,https://b.example/cb", "https://a.example/cb,https://b.example/cb" )]
        [DataRow( "https://a.example/cb, https://b.example/cb", "https://b.example/cb" )]

        // Loopback addresses must match everything except the port, character for character.
        [DataRow( "http://127.0.0.1/callback", "https://127.0.0.1:53211/callback" )]
        [DataRow( "http://127.0.0.1/callback", "http://127.0.0.1:53211/other" )]
        [DataRow( "http://localhost/callback", "HTTP://localhost:53211/callback" )]
        [DataRow( "http://localhost/callback", "http://LOCALHOST:53211/callback" )]
        [DataRow( "http://localhost/callback", "http://localhost:53211/CALLBACK" )]
        [DataRow( "http://localhost/callback", "http://localhost:53211/callback/" )]
        [DataRow( "http://127.0.0.1/callback", "http://localhost:53211/callback" )]
        [DataRow( "http://localhost/callback?client=app", "http://localhost:53211/callback?client=other" )]

        // Loopback text that isn't in the plain "http(s)://host:port/path?query" form.
        [DataRow( "http://localhost/callback", "http:\\\\localhost:53211\\callback" )]
        [DataRow( "http://localhost/callback", " http://localhost:53211/callback" )]
        [DataRow( "http://localhost/callback", "http://localhost:abc/callback" )]
        [DataRow( "http://localhost/callback", "http://localhost:53211@unsafe.com/callback" )]

        // Not a loopback address, or not allowed in a redirect URI.
        [DataRow( "http://localhost/callback", "http://localhost.unsafe.com:53211/callback" )]
        [DataRow( "http://127.0.0.1/callback", "http://127.0.0.1.unsafe.com:53211/callback" )]
        [DataRow( "http://192.168.1.10/callback", "http://192.168.1.10:53211/callback" )]
        [DataRow( "ftp://localhost/callback", "ftp://localhost:21/callback" )]
        [DataRow( "http://localhost/callback", "http://unsafe.com@localhost:53211/callback" )]
        [DataRow( "http://localhost/callback", "http://localhost:53211/callback#x" )]

        // Missing values.
        [DataRow( "http://localhost/callback", "" )]
        [DataRow( "http://localhost/callback", null )]
        [DataRow( "", "http://localhost/callback" )]
        public void IsRedirectUriAllowed_RejectsUnregisteredUri( string registeredRedirectUris, string redirectUri )
        {
            var output = AuthClientService.IsRedirectUriAllowed( CreateAuthClient( registeredRedirectUris ), redirectUri );
            Assert.IsFalse( output, redirectUri );
        }

        [TestMethod]
        public void IsRedirectUriAllowed_RejectsNullClient()
        {
            var output = AuthClientService.IsRedirectUriAllowed( null, "https://app.example.com/oauth/callback" );
            Assert.IsFalse( output );
        }

        #endregion
    }
}
