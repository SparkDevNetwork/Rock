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
using System.Collections.Generic;

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

        private static AuthClient CreateDynamicAuthClient( string redirectUri )
        {
            return new AuthClient
            {
                ClientId = $"{AuthClientService.DynamicClientIdPrefix}test",
                RedirectUri = redirectUri
            };
        }

        [TestMethod]
        // Exact matches.
        [DataRow( "https://app.example.com/oauth/callback", "https://app.example.com/oauth/callback" )]
        [DataRow( "https://a.example/cb,https://b.example/cb", "https://b.example/cb" )]

        // Stored entries are trimmed.
        [DataRow( "https://a.example/cb, https://b.example/cb", "https://b.example/cb" )]

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

        // Registered redirect URIs that aren't valid never match (e.g. saved before redirect URIs were checked).
        [DataRow( "javascript:alert(1)", "javascript:alert(1)" )]

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

        [TestMethod]
        public void IsRedirectUriAllowed_AppliesDynamicRuleToDynamicClient()
        {
            const string redirectUri = "http://app.example.com/oauth/callback";

            Assert.IsTrue( AuthClientService.IsRedirectUriAllowed( CreateAuthClient( redirectUri ), redirectUri ) );
            Assert.IsFalse( AuthClientService.IsRedirectUriAllowed( CreateDynamicAuthClient( redirectUri ), redirectUri ) );
        }

        #endregion

        #region AreRedirectUrisValid

        [TestMethod]
        // Web addresses.
        [DataRow( "https://app.example.com/oauth/callback" )]
        [DataRow( "http://app.example.com/oauth/callback" )]
        [DataRow( "HTTPS://app.example.com/oauth/callback" )]

        // Private-use app schemes in reverse domain form.
        [DataRow( "com.example.app:/oauth2redirect/example-provider" )]

        // Stored entries are trimmed, and empty entries are ignored.
        [DataRow( "https://a.example/cb,https://b.example/cb" )]
        [DataRow( "https://a.example/cb, https://b.example/cb" )]
        [DataRow( "https://a.example/cb ,https://b.example/cb" )]
        [DataRow( "https://a.example/cb," )]
        public void AreRedirectUrisValid_AllowsValidUris( string registeredRedirectUris )
        {
            var output = AuthClientService.AreRedirectUrisValid( CreateAuthClient( registeredRedirectUris ) );
            Assert.IsTrue( output, registeredRedirectUris );
        }

        [TestMethod]
        // Schemes that aren't web addresses or app schemes in reverse domain form.
        [DataRow( "javascript:alert(1)" )]
        [DataRow( "javascript:a.b" )]
        [DataRow( "vbscript:msgbox(1)" )]
        [DataRow( "data:hi" )]
        [DataRow( "file:///c:/windows/win.ini" )]
        [DataRow( "ftp://example.com/callback" )]
        [DataRow( "myapp://callback" )]

        // Not an absolute URI.
        [DataRow( "/oauth/callback" )]

        // Not allowed in a redirect URI.
        [DataRow( "https://app.example.com/oauth/callback#x" )]
        [DataRow( "https://user@app.example.com/oauth/callback" )]
        [DataRow( "https://app.example.com/oauth/call back" )]
        [DataRow( "java\tscript:alert(1)" )]
        [DataRow( "https://app.example.com/oauth/\0callback" )]

        // Any invalid entry, including one added after a comma inside what looks like a single URI.
        [DataRow( "https://a.example/cb,myapp://callback" )]
        [DataRow( "https://a.example/x,javascript:alert(1)" )]

        // No entries.
        [DataRow( "" )]
        [DataRow( "," )]
        [DataRow( " , " )]
        [DataRow( null )]
        public void AreRedirectUrisValid_RejectsInvalidUris( string registeredRedirectUris )
        {
            var output = AuthClientService.AreRedirectUrisValid( CreateAuthClient( registeredRedirectUris ) );
            Assert.IsFalse( output, registeredRedirectUris );
        }

        [TestMethod]
        // https, on any host.
        [DataRow( "https://app.example.com/oauth/callback" )]

        // http on a loopback host.
        [DataRow( "http://localhost:53211/callback" )]
        [DataRow( "http://localhost/callback" )]
        [DataRow( "http://LOCALHOST:53211/callback" )]
        [DataRow( "http://localhost:53211" )]
        [DataRow( "http://localhost:53211?client=app" )]
        [DataRow( "http://127.0.0.1:53211/callback" )]
        [DataRow( "http://127.0.0.2/callback" )]
        [DataRow( "http://[::1]:53211/callback" )]
        public void AreRedirectUrisValid_AllowsValidDynamicUris( string registeredRedirectUris )
        {
            var output = AuthClientService.AreRedirectUrisValid( CreateDynamicAuthClient( registeredRedirectUris ) );
            Assert.IsTrue( output, registeredRedirectUris );
        }

        [TestMethod]
        // http on a host that isn't loopback.
        [DataRow( "http://app.example.com/oauth/callback" )]
        [DataRow( "http://192.168.1.10/callback" )]
        [DataRow( "http://0.0.0.0/callback" )]

        // Hosts that Uri.IsLoopback accepts, but a browser may not treat as loopback.
        [DataRow( "http://loopback/callback" )]
        [DataRow( "http://127.1/callback" )]
        [DataRow( "http://2130706433/callback" )]

        // Look like loopback hosts, but aren't.
        [DataRow( "http://localhost./callback" )]
        [DataRow( "http://foo.localhost/callback" )]
        [DataRow( "http://localhost.unsafe.com/callback" )]
        [DataRow( "http://127.0.0.1.unsafe.com/callback" )]

        // App schemes, even in reverse domain form.
        [DataRow( "com.example.app:/oauth2redirect/example-provider" )]

        // Refused by the shared parsing rules, which are tested with admin clients above.
        [DataRow( "https://app.example.com/oauth/callback#x" )]
        public void AreRedirectUrisValid_RejectsInvalidDynamicUris( string registeredRedirectUris )
        {
            var output = AuthClientService.AreRedirectUrisValid( CreateDynamicAuthClient( registeredRedirectUris ) );
            Assert.IsFalse( output, registeredRedirectUris );
        }

        [TestMethod]
        public void AreRedirectUrisValid_RejectsNullClient()
        {
            var output = AuthClientService.AreRedirectUrisValid( null );
            Assert.IsFalse( output );
        }

        #endregion

        #region IsDynamicClient

        [TestMethod]
        // Created by dynamic client registration.
        [DataRow( "dcr.6f0c1f5e-2d3b-4c1a-9e8f-0a1b2c3d4e5f" )]

        // A CIMD client, identified by the address of its metadata document.
        [DataRow( "https://app.example.com/oauth/client.json" )]
        public void IsDynamicClient_DetectsDynamicClient( string clientId )
        {
            var output = AuthClientService.IsDynamicClient( new AuthClient { ClientId = clientId } );
            Assert.IsTrue( output, clientId );
        }

        [TestMethod]
        [DataRow( "my-app" )]
        [DataRow( "http://app.example.com/oauth/client.json" )]
        [DataRow( "" )]
        [DataRow( null )]
        public void IsDynamicClient_IgnoresOtherClients( string clientId )
        {
            var output = AuthClientService.IsDynamicClient( new AuthClient { ClientId = clientId } );
            Assert.IsFalse( output, clientId );
        }

        #endregion

        #region GetAuthClientFromMetadata

        private const string MetadataClientId = "https://app.example.com/oauth/client.json";

        private static AuthClientService.ClientMetadata CreateMetadata( List<string> redirectUris )
        {
            return new AuthClientService.ClientMetadata
            {
                ClientId = MetadataClientId,
                ClientName = "Example App",
                RedirectUris = redirectUris
            };
        }

        [TestMethod]
        public void GetAuthClientFromMetadata_BuildsDynamicClient()
        {
            var metadata = CreateMetadata( new List<string> { "https://app.example.com/callback", "http://127.0.0.1:53211/callback" } );

            var output = AuthClientService.GetAuthClientFromMetadata( metadata, MetadataClientId );

            Assert.IsNotNull( output );
            Assert.AreEqual( MetadataClientId, output.ClientId );
            Assert.AreEqual( "Example App", output.Name );
            Assert.AreEqual( "https://app.example.com/callback,http://127.0.0.1:53211/callback", output.RedirectUri );
            Assert.AreEqual( "[\"mcp:invoke\"]", output.AllowedScopes );
            Assert.AreEqual( "[]", output.AllowedClaims );
            Assert.AreEqual( string.Empty, output.ClientSecretHash );
            Assert.IsTrue( output.IsActive );
            Assert.IsFalse( output.AllowUserApiAccess, "CIMD clients must never get general REST API access." );
        }

        [TestMethod]
        public void GetAuthClientFromMetadata_RejectsMismatchedClientId()
        {
            var metadata = CreateMetadata( new List<string> { "https://app.example.com/callback" } );

            var output = AuthClientService.GetAuthClientFromMetadata( metadata, "https://other.example.com/oauth/client.json" );

            Assert.IsNull( output );
        }

        [TestMethod]
        public void GetAuthClientFromMetadata_RejectsMissingRedirectUris()
        {
            Assert.IsNull( AuthClientService.GetAuthClientFromMetadata( CreateMetadata( null ), MetadataClientId ) );
            Assert.IsNull( AuthClientService.GetAuthClientFromMetadata( CreateMetadata( new List<string>() ), MetadataClientId ) );
        }

        [TestMethod]
        public void GetAuthClientFromMetadata_AppliesDynamicRule()
        {
            var metadata = CreateMetadata( new List<string> { "http://app.example.com/callback" } );

            var output = AuthClientService.GetAuthClientFromMetadata( metadata, MetadataClientId );

            Assert.IsNull( output );
        }

        [TestMethod]
        public void GetAuthClientFromMetadata_RejectsMarkupInClientName()
        {
            var metadata = CreateMetadata( new List<string> { "https://app.example.com/callback" } );
            metadata.ClientName = "<img src=x onerror=alert(1)>";

            var output = AuthClientService.GetAuthClientFromMetadata( metadata, MetadataClientId );

            Assert.IsNull( output );
        }

        #endregion

        #region IsClientNameValid

        [TestMethod]
        [DataRow( null )]
        [DataRow( "" )]
        [DataRow( "Example App" )]
        [DataRow( "Smith & Sons' App, Ltd." )]
        public void IsClientNameValid_AcceptsPlainNames( string clientName )
        {
            Assert.IsTrue( AuthClientService.IsClientNameValid( clientName ) );
        }

        [TestMethod]
        [DataRow( "<img src=x onerror=alert(1)>" )]
        [DataRow( "<script>alert(1)</script>" )]
        [DataRow( "<b>App</b>" )]
        [DataRow( "{{ CurrentPerson.FirstName }}" )]
        public void IsClientNameValid_RejectsMarkup( string clientName )
        {
            Assert.IsFalse( AuthClientService.IsClientNameValid( clientName ) );
        }

        #endregion
    }
}
