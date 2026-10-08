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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The chat platform the environment names: provisioning a church on it, and reading its rows
    /// back. Shared by the tests that make the real call rather than stopping at a stand-in.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <c>ROCK_CHAT_PLATFORM_URL</c> is the platform's API address,
    ///         <c>ROCK_CHAT_PLATFORM_PUBLISHABLE_KEY</c> its publishable key,
    ///         <c>ROCK_CHAT_PLATFORM_SERVICE_ROLE_KEY</c> its service role key, used only to read
    ///         rows back, and <c>ROCK_CHAT_PLATFORM_PROVISION_SECRET</c> the secret its tenant
    ///         provisioning function compares. With any of them missing a test reports Inconclusive,
    ///         so the continuous integration run, which has no platform, passes over it.
    ///     </para>
    ///     <para>
    ///         To run against a local platform, start the stack from the platform repository with
    ///         <c>npx supabase start</c>, and serve its functions with the signing key and provision
    ///         secret in <c>supabase/functions/.env</c>. <c>npx supabase status -o env</c> prints
    ///         <c>API_URL</c>, <c>PUBLISHABLE_KEY</c> and <c>SERVICE_ROLE_KEY</c> for the first three
    ///         variables, and the fourth is <c>CHAT_PROVISION_SECRET</c> from that file. Then run
    ///         <c>dotnet test Rock.Tests.Integration/Rock.Tests.Integration.csproj --filter
    ///         "TestCategory=ChatPlatformEndToEnd"</c> from this repository. Each run leaves its own
    ///         church's rows on the platform.
    ///     </para>
    /// </remarks>
    internal sealed class LocalChatPlatform
    {
        #region Constants

        private const string PlatformUrlVariable = "ROCK_CHAT_PLATFORM_URL";

        private const string PublishableKeyVariable = "ROCK_CHAT_PLATFORM_PUBLISHABLE_KEY";

        private const string ServiceRoleKeyVariable = "ROCK_CHAT_PLATFORM_SERVICE_ROLE_KEY";

        private const string ProvisionSecretVariable = "ROCK_CHAT_PLATFORM_PROVISION_SECRET";

        // Far past what a push to a local platform takes, so a row that is not there by then was
        // never sent.
        private static readonly TimeSpan RowWait = TimeSpan.FromSeconds( 10 );

        #endregion Constants

        #region Fields

        private static readonly HttpClient _http = new HttpClient();

        private string _url;

        private string _publishableKey;

        private string _serviceRoleKey;

        private string _provisionSecret;

        #endregion Fields

        #region Properties

        /// <summary>
        /// How often a wait reads the row again. Every wait adds up to this much to what it times.
        /// </summary>
        public static TimeSpan PollInterval { get; } = TimeSpan.FromMilliseconds( 5 );

        #endregion Properties

        #region Methods

        /// <summary>
        /// The platform the environment names, or an Inconclusive result naming what is missing.
        /// </summary>
        /// <returns>The platform.</returns>
        public static LocalChatPlatform FromEnvironment()
        {
            var platform = new LocalChatPlatform
            {
                _url = Environment.GetEnvironmentVariable( PlatformUrlVariable ),
                _publishableKey = Environment.GetEnvironmentVariable( PublishableKeyVariable ),
                _serviceRoleKey = Environment.GetEnvironmentVariable( ServiceRoleKeyVariable ),
                _provisionSecret = Environment.GetEnvironmentVariable( ProvisionSecretVariable )
            };

            var missing = new[]
            {
                platform._url.IsNullOrWhiteSpace() ? PlatformUrlVariable : null,
                platform._publishableKey.IsNullOrWhiteSpace() ? PublishableKeyVariable : null,
                platform._serviceRoleKey.IsNullOrWhiteSpace() ? ServiceRoleKeyVariable : null,
                platform._provisionSecret.IsNullOrWhiteSpace() ? ProvisionSecretVariable : null
            }.Where( v => v != null ).ToList();

            if ( missing.Any() )
            {
                Assert.Inconclusive( "Set " + string.Join( ", ", missing ) + " to run this against a chat platform." );
            }

            platform._url = platform._url.TrimEnd( '/' );

            return platform;
        }

        /// <summary>
        /// Provisions a new church with a key made on the spot, and returns the settings Rock
        /// would hold for it after enabling chat.
        /// </summary>
        /// <returns>The church's settings.</returns>
        public ChatPlatformConfiguration ProvisionChurch()
        {
            var tenantId = Guid.NewGuid();
            var kid = "kid-" + tenantId.ToString( "N" ).Substring( 0, 12 );
            var key = ChatSyncProjectionFixture.CreateSigningKey( kid );

            var body = new JObject
            {
                ["tenant_id"] = tenantId.ToString(),
                ["name"] = "Rock end to end " + tenantId.ToString( "N" ).Substring( 0, 8 ),
                ["rock_public_key"] = key.PublicJwk
            };

            using ( var request = new HttpRequestMessage( HttpMethod.Post, _url + "/functions/v1/provision-tenant" ) )
            {
                request.Headers.TryAddWithoutValidation( "apikey", _publishableKey );
                request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + _provisionSecret );
                request.Content = new StringContent( body.ToString( Formatting.None ), Encoding.UTF8, "application/json" );

                using ( var response = _http.SendAsync( request ).GetAwaiter().GetResult() )
                {
                    var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Assert.IsTrue( response.IsSuccessStatusCode, $"the platform would not provision a church: HTTP {( int ) response.StatusCode} {text}" );
                }
            }

            return new ChatPlatformConfiguration
            {
                TenantId = tenantId,
                ProjectUrl = _url,
                PublishableKey = _publishableKey,
                Kid = kid,
                PrivateKey = key.PrivateJwk,
                AreChatProfilesVisible = true,
                IsOpenDirectMessagingAllowed = true,
                ChatBadgeDataViewGuids = new List<Guid>()
            };
        }

        /// <summary>
        /// Reads a membership row until it satisfies a condition, or null when it never does.
        /// </summary>
        /// <param name="tenantId">The church.</param>
        /// <param name="channelId">The channel, which is the chat group's Guid.</param>
        /// <param name="aliasGuid">The member's primary alias Guid.</param>
        /// <param name="isReady">Whether the row read is the one waited for; given null for no row.</param>
        /// <returns>The row, or null.</returns>
        public JObject WaitForMember( Guid tenantId, Guid channelId, Guid aliasGuid, Func<JObject, bool> isReady )
        {
            return WaitFor(
                $"chat_channel_members?select=synced_at,absent_since,can_mention_all,can_post_announcements,is_banned&tenant_id=eq.{tenantId}&channel_id=eq.{channelId}&person_alias_guid=eq.{aliasGuid}",
                isReady );
        }

        /// <summary>
        /// Reads an alias row until it satisfies a condition, or null when it never does.
        /// </summary>
        /// <param name="tenantId">The church.</param>
        /// <param name="aliasGuid">The person's primary alias Guid.</param>
        /// <param name="isReady">Whether the row read is the one waited for; given null for no row.</param>
        /// <returns>The row, or null.</returns>
        public JObject WaitForAlias( Guid tenantId, Guid aliasGuid, Func<JObject, bool> isReady )
        {
            return WaitFor(
                $"chat_aliases?select=synced_at,is_globally_banned,is_open_dm_allowed,primary_person_alias_guid&tenant_id=eq.{tenantId}&person_alias_guid=eq.{aliasGuid}",
                isReady );
        }

        /// <summary>
        /// Reads a channel row until it satisfies a condition, or null when it never does.
        /// </summary>
        /// <param name="tenantId">The church.</param>
        /// <param name="channelId">The channel, which is the chat group's Guid.</param>
        /// <param name="isReady">Whether the row read is the one waited for; given null for no row.</param>
        /// <returns>The row, or null.</returns>
        public JObject WaitForChannel( Guid tenantId, Guid channelId, Func<JObject, bool> isReady )
        {
            return WaitFor(
                $"chat_channels?select=channel_type,name,absent_since&tenant_id=eq.{tenantId}&channel_id=eq.{channelId}",
                isReady );
        }

        /// <summary>
        /// Reads a channel's system lines until one with this text is there, or null when none ever is.
        /// </summary>
        /// <param name="tenantId">The church.</param>
        /// <param name="channelId">The channel, which is the chat group's Guid.</param>
        /// <param name="body">The line's whole text.</param>
        /// <returns>The line, or null.</returns>
        public JObject WaitForSystemLine( Guid tenantId, Guid channelId, string body )
        {
            return WaitFor(
                $"messages?select=id,person_alias_guid,body&tenant_id=eq.{tenantId}&channel_id=eq.{channelId}&message_type=eq.system&body=eq.{Uri.EscapeDataString( body )}",
                r => r != null );
        }

        /// <summary>
        /// How many messages a channel holds on the platform, read until a condition holds.
        /// </summary>
        /// <param name="tenantId">The church.</param>
        /// <param name="channelId">The channel, which is the chat group's Guid.</param>
        /// <param name="isReady">Whether the count read is the one waited for.</param>
        /// <returns>The last count read.</returns>
        public int WaitForMessageCount( Guid tenantId, Guid channelId, Func<int, bool> isReady )
        {
            var count = 0;
            WaitFor( $"messages?select=count&tenant_id=eq.{tenantId}&channel_id=eq.{channelId}", r =>
            {
                count = ( int ) r["count"];
                return isReady( count );
            } );

            return count;
        }

        /// <summary>
        /// One message as the platform stored it, or null where there is none.
        /// </summary>
        /// <param name="tenantId">The church.</param>
        /// <param name="messageId">The id the post answered.</param>
        /// <returns>The row, or null.</returns>
        public JObject ReadMessage( Guid tenantId, long messageId )
        {
            return Read( $"messages?select=channel_id,person_alias_guid,message_type,body&tenant_id=eq.{tenantId}&id=eq.{messageId}" );
        }

        /// <summary>
        /// Puts the Rock system chat person in a church's mirror, as the church's first full sync
        /// does, so a system line has an author without running a whole sync.
        /// </summary>
        /// <param name="tenantId">The church.</param>
        public void AddSystemAuthor( Guid tenantId )
        {
            var author = Rock.SystemGuid.Person.CHAT_SYSTEM_AUTHOR.AsGuid();
            var row = new JObject
            {
                ["tenant_id"] = tenantId.ToString(),
                ["person_alias_guid"] = author.ToString(),
                ["primary_person_alias_guid"] = author.ToString(),
                ["nick_name"] = "Rock",
                ["last_name"] = "Chat",
                ["synced_at"] = "2026-01-01T00:00:00Z"
            };

            using ( var request = new HttpRequestMessage( HttpMethod.Post, _url + "/rest/v1/chat_aliases" ) )
            {
                request.Headers.TryAddWithoutValidation( "apikey", _serviceRoleKey );
                request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + _serviceRoleKey );
                request.Content = new StringContent( row.ToString( Formatting.None ), Encoding.UTF8, "application/json" );

                using ( var response = _http.SendAsync( request ).GetAwaiter().GetResult() )
                {
                    var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Assert.IsTrue( response.IsSuccessStatusCode, $"the system person could not be written: HTTP {( int ) response.StatusCode} {text}" );
                }
            }
        }

        /// <summary>
        /// Reads one row again and again until it satisfies a condition or the wait runs out.
        /// </summary>
        /// <param name="query">The data API path and query.</param>
        /// <param name="isReady">Whether the row read is the one waited for.</param>
        /// <returns>The row, or null.</returns>
        private JObject WaitFor( string query, Func<JObject, bool> isReady )
        {
            var stopwatch = Stopwatch.StartNew();

            while ( stopwatch.Elapsed < RowWait )
            {
                var row = Read( query );
                if ( isReady( row ) )
                {
                    return row;
                }

                Thread.Sleep( PollInterval );
            }

            return null;
        }

        /// <summary>
        /// One row from the platform's data API under the service role, or null where there is none.
        /// </summary>
        /// <param name="query">The data API path and query.</param>
        /// <returns>The row, or null.</returns>
        private JObject Read( string query )
        {
            using ( var request = new HttpRequestMessage( HttpMethod.Get, _url + "/rest/v1/" + query ) )
            {
                request.Headers.TryAddWithoutValidation( "apikey", _serviceRoleKey );
                request.Headers.TryAddWithoutValidation( "Authorization", "Bearer " + _serviceRoleKey );

                using ( var response = _http.SendAsync( request ).GetAwaiter().GetResult() )
                {
                    var text = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Assert.IsTrue( response.IsSuccessStatusCode, $"the platform's rows could not be read: HTTP {( int ) response.StatusCode} {text}" );

                    // Dates stay text, so a test compares the platform's instant as written.
                    using ( var reader = new JsonTextReader( new StringReader( text ) ) { DateParseHandling = DateParseHandling.None } )
                    {
                        return JArray.Load( reader ).OfType<JObject>().FirstOrDefault();
                    }
                }
            }
        }

        #endregion Methods
    }
}
