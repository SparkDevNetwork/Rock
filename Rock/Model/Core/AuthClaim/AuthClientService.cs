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
using System.Threading.Tasks;

using Newtonsoft.Json;

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
                : authClient.ClientId.XxHash();

            return $"{ScopeCookiePrefix}{clientHash}-{IdHasher.Instance.GetHash( userLogin.Id )}";
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
        private static AuthClient GetAuthClientFromMetadata( ClientMetadata metadata, string clientId )
        {
            if ( metadata.ClientId != clientId )
            {
                return null;
            }

            if ( metadata.RedirectUris == null || metadata.RedirectUris.Count == 0 )
            {
                return null;
            }

            return new AuthClient
            {
                IsActive = true,
                ClientId = metadata.ClientId,
                ClientSecretHash = string.Empty,
                Name = $"{metadata.ClientName} ({metadata.ClientUri})",
                AllowedClaims = new string[0].ToJson(),
                AllowedScopes = AllowedDynamicScopes.ToJson(),
                RedirectUri = metadata.RedirectUris != null ? string.Join( ",", metadata.RedirectUris ) : string.Empty,
                ScopeApprovalExpiration = 365,
            };
        }

        private class ClientMetadata
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