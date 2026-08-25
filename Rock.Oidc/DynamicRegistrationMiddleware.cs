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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Owin;

using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Oidc
{
    /// <summary>
    /// Provides support for required endpoints to support dynamic registration
    /// of OAuth clients. This is used by MCP.
    /// </summary>
    internal class DynamicRegistrationMiddleware : OwinMiddleware
    {
        #region Constants

        /// <summary>
        /// The template for the /.well-known/oauth-protected-resource response.
        /// </summary>
        private const string ProtectedResourceTemplate = @"{{
  ""resource"": ""{1}"",
  ""authorization_servers"": [
    ""{0}""
  ],
  ""scopes_supported"": [""mcp:invoke""]
}}
";

        /// <summary>
        /// The template for the /.well-known/oauth-authorization-server response.
        /// </summary>
        private const string AuthorizationServerTemplate = @"{{
  ""issuer"": ""{0}"",
  ""authorization_endpoint"": ""{0}/Auth/Authorize"",
  ""token_endpoint"": ""{0}/Auth/Token"",
  ""registration_endpoint"": ""{0}/Auth/Register"",
  ""client_id_metadata_document_supported"": true,
  ""response_types_supported"": [""code""],
  ""grant_types_supported"": [""authorization_code"", ""refresh_token""],
  ""code_challenge_methods_supported"": [""S256""],
  ""token_endpoint_auth_methods_supported"": [""client_secret_basic"", ""client_secret_post"", ""none""]
}}
";

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="DynamicRegistrationMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the OWIN pipeline.</param>
        public DynamicRegistrationMiddleware( OwinMiddleware next )
            : base( next )
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override async System.Threading.Tasks.Task Invoke( IOwinContext context )
        {
            if ( context.Request.Path.StartsWithSegments( new PathString( "/.well-known/oauth-protected-resource/api/v2/mcp" ) ) )
            {
                // The request is for an MCP protected resource.
                var hostUri = GetOriginalHostUri( context );
                var resourceUriBuilder = new UriBuilder( hostUri )
                {
                    Path = context.Request.Path.ToString().Substring( 37 ),
                    Query = string.Empty
                };

                context.Response.ContentType = "application/json";
                context.Response.Write( string.Format( ProtectedResourceTemplate, hostUri.ToString().RemoveTrailingForwardslash(), resourceUriBuilder.Uri ) );

                return;
            }
            else if ( context.Request.Path == new PathString( "/.well-known/oauth-authorization-server" ) )
            {
                // The request is for the authorization server metadata document.
                var hostUri = GetOriginalHostUri( context );

                context.Response.ContentType = "application/json";
                context.Response.Write( string.Format( AuthorizationServerTemplate, hostUri.ToString().RemoveTrailingForwardslash() ) );

                return;
            }
            else if ( context.Request.Path == new PathString( "/Auth/Register" ) && context.Request.Method == "POST" )
            {
                // The request is to dynamically register a new client
                // application. This is used by legacy MCP clients to create
                // a new client for each connected instance.
                var json = Encoding.UTF8.GetString( context.Request.Body.ReadBytesToEnd() );
                var request = json.FromJsonOrThrow<RegisterRequest>();

                var clientId = $"dcr.{Guid.NewGuid()}";
                var clientSecret = Guid.NewGuid().ToString();

                var validScopes = AuthClientService.AllowedDynamicScopes;
                var allowedScopes = new List<string>();

                if ( request.scope.IsNotNullOrWhiteSpace() )
                {
                    allowedScopes = request.scope.SplitDelimitedValues( " " )
                        .Where( s => validScopes.Contains( s ) )
                        .ToList();
                }

                var entityTypeName = EntityTypeCache.Get<Rock.Security.Authentication.Database>().Name;
                var databaseAuth = AuthenticationContainer.GetComponent( entityTypeName ) as Rock.Security.Authentication.Database;
                var encryptedClientSecret = databaseAuth.EncryptString( clientSecret );

                using ( var rockContext = new Data.RockContext() )
                {
                    var authClient = new AuthClient
                    {
                        AllowUserApiAccess = true,
                        AllowedClaims = "[]",
                        AllowedScopes = allowedScopes.ToJson(),
                        IsActive = true,
                        Name = request.client_name,
                        ClientId = clientId,
                        ClientSecretHash = encryptedClientSecret,
                        RedirectUri = request.redirect_uris.JoinStrings( "," ),
                        PostLogoutRedirectUri = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ),
                    };

                    new AuthClientService( rockContext ).Add( authClient );

                    rockContext.SaveChanges();
                }

                var response = new RegisterResponse
                {
                    client_id = clientId,
                    client_id_issued_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    client_secret = clientSecret,
                    token_endpoint_auth_method = "client_secret_basic",
                    redirect_uris = request.redirect_uris,
                    grant_types = new List<string> { "authorization_code" },
                    response_types = new List<string> { "code" },
                    scope = string.Empty,
                };

                context.Response.ContentType = "application/json";
                context.Response.Write( response.ToJson() );

                return;
            }

            await Next.Invoke( context );
        }

        /// <summary>
        /// Gets the original hostname <see cref="Uri"/> for the request, taking
        /// into account common headers that may be added by reverse proxies and
        /// load balancers.
        /// </summary>
        /// <param name="context">The OWIN context for the request.</param>
        /// <returns>A new <see cref="Uri"/> instance that contains the original request information.</returns>
        private Uri GetOriginalHostUri( IOwinContext context )
        {
            var request = context.Request;
            var uriBuilder = new UriBuilder( request.Uri )
            {
                Path = string.Empty,
                Query = string.Empty
            };

            var forwardedHost = request.Headers["X-Forwarded-Host"] ?? request.Headers["X-Original-Host"] ?? request.Headers["weglot-forwarded-host"];

            if ( !string.IsNullOrEmpty( forwardedHost ) )
            {
                uriBuilder.Scheme = request.Headers["X-Forwarded-Proto"]?.ToString() ?? request.Scheme;
                uriBuilder.Host = forwardedHost;

                // If we have the original port then use it, otherwise reset to default port.
                if ( request.Headers["X-Forwarded-Port"] != null )
                {
                    uriBuilder.Port = request.Headers["X-Forwarded-Port"].AsIntegerOrNull() ?? -1;
                }
                else
                {
                    uriBuilder.Port = -1;
                }
            }

            return uriBuilder.Uri;
        }

        #endregion

        #region Support Classes

        private class RegisterRequest
        {
            public List<string> redirect_uris { get; set; }

            public string token_endpoint_auth_method { get; set; }

            public List<string> grant_types { get; set; }

            public List<string> response_types { get; set; }

            public string client_name { get; set; }

            public string client_uri { get; set; }

            public string scope { get; set; }
        }

        private class RegisterResponse
        {
            public string client_id { get; set; }

            public long client_id_issued_at { get; set; }

            public string client_secret { get; set; }

            public string token_endpoint_auth_method { get; set; }

            public List<string> redirect_uris { get; set; }

            public List<string> grant_types { get; set; }

            public List<string> response_types { get; set; }

            public string scope { get; set; }
        }

        #endregion
    }
}
