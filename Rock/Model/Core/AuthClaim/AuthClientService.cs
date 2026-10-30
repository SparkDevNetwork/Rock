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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Rock.Attribute;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.AuthClient"/> entities.
    /// </summary>
    public partial class AuthClientService
    {
        private const string ScopeCookiePrefix = ".ROCK-OidcScopeApproval-";

        /// <summary>
        /// The OAuth scopes that are allowed for dynamically registered clients.
        /// </summary>
        internal static readonly string[] AllowedDynamicScopes = new string[]
        {
            "mcp:invoke",
        };

        /// <summary>
        /// The prefix of the client identifier given to clients created by dynamic client registration.
        /// </summary>
        internal const string DynamicClientIdPrefix = "dcr.";

        /// <summary>
        /// Matches an http URI whose host is <c>localhost</c>, <c>127.x.x.x</c> or <c>[::1]</c>, with an optional port.
        /// </summary>
        private static readonly Regex LoopbackHttpRegex = new Regex(
            @"^http://(?:localhost|127\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}|\[::1\])(?::[0-9]+)?(?=[/?]|$)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled );

        /// <summary>
        /// Matches the port of an http or https loopback URI (<c>localhost</c>, <c>127.x.x.x</c> or <c>[::1]</c>),
        /// so it can be removed. Only a port directly after a loopback host at the start of the URI matches.
        /// The port must be followed by "/", "?" or the end; redirect URIs can't have a fragment
        /// (https://www.rfc-editor.org/rfc/rfc6749#section-3.1.2).
        /// </summary>
        private static readonly Regex LoopbackPortRegex = new Regex(
            @"^(?<origin>https?://(?:localhost|127\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}|\[::1\])):[0-9]+(?=[/?]|$)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled );

        /// <summary>
        /// Determines whether a client has at least one redirect URI and all of them are valid for that client.
        /// </summary>
        /// <param name="authClient">The <see cref="AuthClient"/> whose comma-separated redirect URIs are checked.</param>
        /// <returns><c>true</c> if the client's redirect URIs are valid; otherwise, <c>false</c>.</returns>
        internal static bool AreRedirectUrisValid( AuthClient authClient )
        {
            var redirectUris = GetRedirectUris( authClient );

            return redirectUris.Count > 0 && redirectUris.All( redirectUri => IsValidRedirectUri( authClient, redirectUri ) );
        }

        /// <summary>
        /// Gets the redirect URIs stored on a client.
        /// </summary>
        /// <param name="authClient">The <see cref="AuthClient"/> whose comma-separated redirect URIs are returned.</param>
        /// <returns>The client's redirect URIs.</returns>
        private static List<string> GetRedirectUris( AuthClient authClient )
        {
            var redirectUris = authClient?.RedirectUri?.SplitDelimitedValues( "," ) ?? new string[0];

            return redirectUris
                .Select( redirectUri => redirectUri.Trim() )
                .Where( redirectUri => redirectUri.Length > 0 )
                .ToList();
        }

        /// <summary>
        /// Determines whether a redirect URI is valid for a client.
        /// </summary>
        /// <param name="authClient">The <see cref="AuthClient"/> the redirect URI belongs to.</param>
        /// <param name="redirectUri">A single redirect URI.</param>
        /// <returns><c>true</c> if the redirect URI is valid for the client; otherwise, <c>false</c>.</returns>
        private static bool IsValidRedirectUri( AuthClient authClient, string redirectUri )
        {
            // Dynamic and non-dynamic clients must first pass the same parsing rules.
            if ( !TryParseRedirectUri( redirectUri, out var uri ) )
            {
                return false;
            }

            if ( IsDynamicClient( authClient ) )
            {
                // Dynamic (DCR or CIMD) clients must use an http loopback address or use https:
                // https://modelcontextprotocol.io/specification/2025-11-25/basic/authorization#communication-security.
                // .NET's "Uri.IsLoopback" isn't used here because it also accepts the host "loopback", which the
                // browser's device may not resolve to itself.
                return uri.Scheme == Uri.UriSchemeHttps || LoopbackHttpRegex.IsMatch( redirectUri );
            }

            if ( uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp )
            {
                // Non-dynamic clients may use any http/s address.
                return true;
            }

            // Otherwise, a non-dynamic client must use a private-use app scheme in reverse domain form,
            // such as "com.example.app". A dot in the scheme is the minimum check.
            // (RFC 8252 section 7.1: https://www.rfc-editor.org/rfc/rfc8252#section-7.1)
            // (RFC 8252 section 8.4: https://www.rfc-editor.org/rfc/rfc8252#section-8.4)
            return uri.Scheme.Contains( "." );
        }

        /// <summary>
        /// Parses a redirect URI that
        /// is absolute (RFC 6749 section 3.1.2: https://www.rfc-editor.org/rfc/rfc6749#section-3.1.2),
        /// has no fragment (RFC 6749 section 3.1.2: https://www.rfc-editor.org/rfc/rfc6749#section-3.1.2),
        /// has no user info (deprecated in RFC 3986 section 3.2.1: https://www.rfc-editor.org/rfc/rfc3986#section-3.2.1),
        /// has no whitespace or control characters (RFC 3986 section 2: https://www.rfc-editor.org/rfc/rfc3986#section-2).
        /// </summary>
        /// <param name="redirectUri">The redirect URI to parse.</param>
        /// <param name="uri">On <c>true</c> return, contains the parsed URI.</param>
        /// <returns><c>true</c> if the redirect URI could be parsed; otherwise, <c>false</c>.</returns>
        private static bool TryParseRedirectUri( string redirectUri, out Uri uri )
        {
            uri = null;

            if ( redirectUri.IsNullOrWhiteSpace() || redirectUri.Any( c => char.IsWhiteSpace( c ) || char.IsControl( c ) ) )
            {
                return false;
            }

            if ( !Uri.TryCreate( redirectUri, UriKind.Absolute, out uri ) )
            {
                return false;
            }

            return uri.Fragment.IsNullOrWhiteSpace() && uri.UserInfo.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Determines whether a client was registered dynamically, by DCR or CIMD, rather than by an admin.
        /// </summary>
        /// <param name="authClient">The <see cref="AuthClient"/> to check.</param>
        /// <returns><c>true</c> if the client was registered dynamically; otherwise, <c>false</c>.</returns>
        internal static bool IsDynamicClient( AuthClient authClient )
        {
            var clientId = authClient.ClientId ?? string.Empty;

            // CIMD clients use the address of their metadata document as their client identifier.
            return clientId.StartsWith( DynamicClientIdPrefix, StringComparison.Ordinal )
                || clientId.StartsWith( "https://", StringComparison.Ordinal );
        }

        /// <summary>
        /// Gets the by client identifier.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public async Task<AuthClient> GetByClientIdAsync( string clientId )
        {
            // Check if this is a CIMD request. If so we need to fetch the
            // client metadata from the provided URL and construct an
            // AuthClient on the fly.
            if ( clientId != null && clientId.StartsWith( "https://" ) )
            {
                var cache = GetMetadataCache();

                if ( cache.TryGetValue( clientId, out AuthClient authClient ) )
                {
                    return authClient;
                }

                using ( var httpClient = new HttpClient() )
                {
                    var response = await httpClient.GetAsync( clientId );

                    if ( !response.IsSuccessStatusCode )
                    {
                        return null;
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var payload = json.FromJsonOrNull<ClientMetadata>();

                    if ( payload == null )
                    {
                        return null;
                    }

                    authClient = GetAuthClientFromMetadata( payload, clientId );

                    cache.AddOrReplace( clientId, authClient );

                    return authClient;
                }
            }

            return await Queryable().AsNoTracking().FirstOrDefaultAsync( ac => ac.ClientId == clientId );
        }

        /// <summary>
        /// Gets the by client identifier.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public AuthClient GetByClientId( string clientId )
        {
            // Check if this is a CIMD request. If so we need to fetch the
            // client metadata from the provided URL and construct an
            // AuthClient on the fly.
            if ( clientId != null && clientId.StartsWith( "https://" ) )
            {
                var cache = GetMetadataCache();

                if ( cache.TryGetValue( clientId, out AuthClient authClient ) )
                {
                    return authClient;
                }

                using ( var httpClient = new HttpClient() )
                {
                    var response = httpClient.GetAsync( clientId ).GetAwaiter().GetResult();

                    if ( !response.IsSuccessStatusCode )
                    {
                        return null;
                    }

                    var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var payload = json.FromJsonOrNull<ClientMetadata>();

                    if ( payload == null )
                    {
                        return null;
                    }

                    authClient = GetAuthClientFromMetadata( payload, clientId );

                    cache.AddOrReplace( clientId, authClient );

                    return authClient;
                }
            }

            return Queryable().AsNoTracking().FirstOrDefault( ac => ac.ClientId == clientId );
        }

        /// <summary>
        /// Gets the by post logout redirect URL.
        /// </summary>
        /// <param name="postLogoutRedirectUrl">The logout redirect URL.</param>
        /// <returns></returns>
        public async Task<AuthClient> GetByPostLogoutRedirectUrlAsync( string postLogoutRedirectUrl )
        {
            return await Queryable().AsNoTracking().FirstOrDefaultAsync( ac =>
                ac.PostLogoutRedirectUri.Equals( postLogoutRedirectUrl, StringComparison.OrdinalIgnoreCase ) );
        }

        /// <summary>
        /// Gets the by identifier and secret.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="clientSecret">The client secret.</param>
        /// <returns></returns>
        public async Task<AuthClient> GetByClientIdAndSecretAsync( string clientId, string clientSecret )
        {
            var authClient = await GetByClientIdAsync( clientId );

            if ( authClient == null )
            {
                return null;
            }

            // Special case. If the client is a CIMD client, then we don't
            // have a secret hash in the database, and it will also have an
            // Id of 0 since it's not actually stored in the database.
            if ( authClient.Id == 0 && authClient.ClientSecretHash.IsNullOrWhiteSpace() && clientSecret.IsNullOrWhiteSpace() )
            {
                return authClient;
            }

            var entityTypeName = EntityTypeCache.Get<Security.Authentication.Database>().Name;
            var databaseAuth = AuthenticationContainer.GetComponent( entityTypeName ) as Security.Authentication.Database;
            var success = databaseAuth.IsBcryptMatch( authClient.ClientSecretHash, clientSecret );

            return success ? authClient : null;
        }

        /// <summary>
        /// Gets the scope cookie name for the current <see cref="AuthClient"/> and <see cref="UserLogin"/> combination.
        /// </summary>
        /// <param name="authClient">The <see cref="AuthClient"/> for which authorization is being requested.</param>
        /// <param name="userLogin">The <see cref="UserLogin"/> that is currently logged in.</param>
        /// <returns>The scope cookie name for the current <see cref="AuthClient"/> and <see cref="UserLogin"/> combination.</returns>
        public static string GetScopeCookieName( AuthClient authClient, UserLogin userLogin )
        {
            var clientHash = authClient.Id != 0
                ? IdHasher.Instance.GetHash( authClient.Id )
                : authClient.ClientId.Sha256Hash();

            return $"{ScopeCookiePrefix}{clientHash}-{IdHasher.Instance.GetHash( userLogin.Id )}";
        }

        /// <summary>
        /// Gets the encrypted scope approval cookie value for the <see cref="AuthClient"/> and <see cref="UserLogin"/>
        /// combination. The value names both, so an approval can't be reused by any other client or login.
        /// </summary>
        /// <param name="authClient">The <see cref="AuthClient"/> whose scopes were approved.</param>
        /// <param name="userLogin">The <see cref="UserLogin"/> that approved the scopes.</param>
        /// <returns>The encrypted scope approval cookie value.</returns>
        [RockInternal( "19.8", true )]
        public static string GetScopeCookieValue( AuthClient authClient, UserLogin userLogin )
        {
            var cookieData = new ScopeApprovalCookieData
            {
                ClientId = authClient.ClientId,
                UserLoginId = userLogin.Id,
                AllowedScopes = authClient.AllowedScopes
            };

            return Encryption.EncryptString( cookieData.ToJson() );
        }

        /// <summary>
        /// Determines whether a scope approval cookie value was issued for this exact <see cref="AuthClient"/> and
        /// <see cref="UserLogin"/>, and for the client's current allowed scopes.
        /// </summary>
        /// <param name="cookieValue">The encrypted scope approval cookie value.</param>
        /// <param name="authClient">The <see cref="AuthClient"/> for which authorization is being requested.</param>
        /// <param name="userLogin">The <see cref="UserLogin"/> that is currently logged in.</param>
        /// <returns><c>true</c> if the cookie value approves this client for this login; otherwise, <c>false</c>.</returns>
        [RockInternal( "19.8", true )]
        public static bool IsScopeCookieValueValid( string cookieValue, AuthClient authClient, UserLogin userLogin )
        {
            if ( cookieValue.IsNullOrWhiteSpace() || authClient == null || userLogin == null )
            {
                return false;
            }

            var cookieData = Encryption.DecryptString( cookieValue )?.FromJsonOrNull<ScopeApprovalCookieData>();

            if ( cookieData == null )
            {
                return false;
            }

            return string.Equals( cookieData.ClientId, authClient.ClientId, StringComparison.Ordinal )
                && cookieData.UserLoginId == userLogin.Id
                && string.Equals( cookieData.AllowedScopes, authClient.AllowedScopes, StringComparison.Ordinal );
        }

        /// <summary>
        /// The data stored, encrypted, in a scope approval cookie.
        /// </summary>
        private class ScopeApprovalCookieData
        {
            /// <summary>
            /// The client identifier of the <see cref="AuthClient"/> whose scopes were approved.
            /// </summary>
            public string ClientId { get; set; }

            /// <summary>
            /// The identifier of the <see cref="UserLogin"/> that approved the scopes.
            /// </summary>
            public int UserLoginId { get; set; }

            /// <summary>
            /// The client's allowed scopes at the time they were approved.
            /// </summary>
            public string AllowedScopes { get; set; }
        }

        /// <summary>
        /// Determines whether a redirect URI is one of the client's registered redirect URIs. URIs must match exactly,
        /// except that the port is ignored when both are loopback addresses, since native apps listen on a port they
        /// pick at startup (RFC 8252 section 7.3: https://www.rfc-editor.org/rfc/rfc8252#section-7.3). A registered
        /// redirect URI that isn't valid for the client, such as one saved before redirect URIs were checked, never
        /// matches.
        /// </summary>
        /// <param name="authClient">The <see cref="AuthClient"/> whose comma-separated redirect URIs are checked.</param>
        /// <param name="redirectUri">The <c>redirect_uri</c> from the authorization request.</param>
        /// <returns><c>true</c> if <paramref name="redirectUri"/> is registered for the client; otherwise, <c>false</c>.</returns>
        [RockInternal( "19.7", true )]
        public static bool IsRedirectUriAllowed( AuthClient authClient, string redirectUri )
        {
            if ( authClient == null || authClient.RedirectUri.IsNullOrWhiteSpace() || redirectUri.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var registeredRedirectUris = GetRedirectUris( authClient )
                .Where( registeredRedirectUri => IsValidRedirectUri( authClient, registeredRedirectUri ) )
                .ToList();

            if ( registeredRedirectUris.Contains( redirectUri, StringComparer.Ordinal ) )
            {
                return true;
            }

            // Only a loopback port is removed, so everything else must still match exactly.
            var redirectUriWithoutPort = LoopbackPortRegex.Replace( redirectUri, "${origin}" );

            return registeredRedirectUris
                .Any( registeredRedirectUri => LoopbackPortRegex.Replace( registeredRedirectUri, "${origin}" ) == redirectUriWithoutPort );
        }

        /// <summary>
        /// Determines whether a client name supplied by a client during dynamic registration passes the validation
        /// rules of <see cref="AuthClient.Name"/>, such as having no HTML or Lava. A missing name is valid.
        /// </summary>
        /// <param name="clientName">The <c>client_name</c> supplied by the client.</param>
        /// <returns><c>true</c> if the client name is valid; otherwise, <c>false</c>.</returns>
        internal static bool IsClientNameValid( string clientName )
        {
            var nameProperty = typeof( AuthClient ).GetProperty( nameof( AuthClient.Name ) );

            try
            {
                StringValueValidator.Validate( clientName, StringValueValidator.GetEffectiveRules( nameProperty ), typeof( AuthClient ), nameProperty.Name );

                return true;
            }
            catch ( PropertyValidationException )
            {
                return false;
            }
        }

        /// <summary>
        /// Get the cache object used to store dynamically registered client metadata.
        /// </summary>
        /// <returns>A <see cref="ConcurrentDictionary{TKey, TValue}"/> used to store dynamically registered client metadata.</returns>
        private static ConcurrentDictionary<string, AuthClient> GetMetadataCache()
        {
            return RockCache.GetOrAddExisting( "core.AuthClient.MetadataLookup", () => new ConcurrentDictionary<string, AuthClient>() ) as ConcurrentDictionary<string, AuthClient>;
        }

        /// <summary>
        /// Constructs a new in-memory <see cref="AuthClient"/> from the metadata
        /// provided by a CIMD client registration request.
        /// </summary>
        /// <param name="metadata">The metadata provided by the CIMD client registration request.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>An <see cref="AuthClient"/> constructed from the metadata, or null if the metadata is invalid.</returns>
        internal static AuthClient GetAuthClientFromMetadata( ClientMetadata metadata, string clientId )
        {
            if ( metadata.ClientId != clientId || !IsClientNameValid( metadata.ClientName ) )
            {
                return null;
            }

            var authClient = new AuthClient
            {
                IsActive = true,
                ClientId = metadata.ClientId,
                ClientSecretHash = string.Empty,
                Name = metadata.ClientName,
                AllowedClaims = new string[0].ToJson(),
                AllowedScopes = AllowedDynamicScopes.ToJson(),
                RedirectUri = metadata.RedirectUris != null ? string.Join( ",", metadata.RedirectUris ) : string.Empty,
                ScopeApprovalExpiration = 365,
            };

            return AreRedirectUrisValid( authClient ) ? authClient : null;
        }

        internal class ClientMetadata
        {
            [JsonProperty( "client_name" )]
            public string ClientName { get; set; }

            [JsonProperty( "client_id" )]
            public string ClientId { get; set; }

            [JsonProperty( "client_uri" )]
            public string ClientUri { get; set; }

            [JsonProperty( "logo_uri" )]
            public string LogoUri { get; set; }

            [JsonProperty( "application_type" )]
            public string ApplicationType { get; set; }

            [JsonProperty( "grant_types" )]
            public List<string> GrantTypes { get; set; }

            [JsonProperty( "response_types" )]
            public List<string> ResponseTypes { get; set; }

            [JsonProperty( "token_endpoint_auth_method" )]
            public string TokenEndpointAuthMethod { get; set; }

            [JsonProperty( "redirect_uris" )]
            public List<string> RedirectUris { get; set; }
        }
    }
}