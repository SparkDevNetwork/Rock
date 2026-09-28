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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Communication.Chat.Platform.Sync;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// The immediate sync against a real chat platform: a save in Rock, the push after its commit,
    /// and the row on the platform, with the time between the two.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Every other immediate sync test stops at a stand-in for the platform, which can agree
    ///         with Rock about a request the real platform refuses: a header in the wrong form, a
    ///         token it cannot verify, a body it will not parse. This one makes the real call. Each
    ///         test provisions its own church on the platform with a key made on the spot, so it
    ///         never depends on what another run left behind, and it reads the rows back through the
    ///         platform's data API under the service role.
    ///     </para>
    ///     <para>
    ///         It runs only when a chat platform is named in the environment, and reports
    ///         Inconclusive otherwise, so the continuous integration run, which has none, passes
    ///         over it. <c>ROCK_CHAT_PLATFORM_URL</c> is the platform's API address,
    ///         <c>ROCK_CHAT_PLATFORM_PUBLISHABLE_KEY</c> its publishable key,
    ///         <c>ROCK_CHAT_PLATFORM_SERVICE_ROLE_KEY</c> its service role key, used only to read
    ///         rows back, and <c>ROCK_CHAT_PLATFORM_PROVISION_SECRET</c> the secret its tenant
    ///         provisioning function compares.
    ///     </para>
    ///     <para>
    ///         To run it against a local platform, start the stack from the platform repository with
    ///         <c>npx supabase start</c>, and serve its functions with the signing key and provision
    ///         secret in <c>supabase/functions/.env</c>. <c>npx supabase status -o env</c> prints
    ///         <c>API_URL</c>, <c>PUBLISHABLE_KEY</c> and <c>SERVICE_ROLE_KEY</c> for the first three
    ///         variables, and the fourth is <c>CHAT_PROVISION_SECRET</c> from that file. Then run
    ///         <c>dotnet test Rock.Tests.Integration/Rock.Tests.Integration.csproj --filter
    ///         "FullyQualifiedName~ImmediateSyncEndToEnd"</c> from this repository. The timings go
    ///         to the test output. Each run leaves its own church's rows on the platform.
    ///     </para>
    /// </remarks>
    [TestClass]
    [TestCategory( "ChatPlatformEndToEnd" )]
    public class ImmediateSyncEndToEndTests : DatabaseTestsBase
    {
        #region Constants

        private const string PlatformUrlVariable = "ROCK_CHAT_PLATFORM_URL";

        private const string PublishableKeyVariable = "ROCK_CHAT_PLATFORM_PUBLISHABLE_KEY";

        private const string ServiceRoleKeyVariable = "ROCK_CHAT_PLATFORM_SERVICE_ROLE_KEY";

        private const string ProvisionSecretVariable = "ROCK_CHAT_PLATFORM_PROVISION_SECRET";

        private const int TimedSaves = 20;

        // Far past what a push to a local platform takes, so a row that is not there by then was
        // never pushed.
        private static readonly TimeSpan RowWait = TimeSpan.FromSeconds( 10 );

        private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds( 5 );

        #endregion Constants

        #region Tests

        [TestMethod]
        public void AMemberSavedInsideARequestReachesThePlatformAtItsReadTimeAndItsDeletionStampsItAbsent()
        {
            var platform = LocalPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "End to end channel" );
                var personId = fixture.AddPerson( "EndToEnd" );
                var alias = fixture.PrimaryAliasGuid( personId );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, channel, personId );
                    rockContext.SaveChanges();
                }

                var row = platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null );
                Assert.IsNotNull( row, "the membership saved in Rock never reached the platform" );
                Assert.IsTrue( row["absent_since"].Type == JTokenType.Null, "a membership just added is present" );

                var readAt = recorder.ReadTimes.LastOrDefault();
                Assert.IsNotNull( readAt, "the push carried no read time" );
                Assert.AreEqual( ParseInstant( readAt ), ParseInstant( ( string ) row["synced_at"] ),
                    "the platform stamps the row with the moment Rock read it, never its own clock" );

                using ( ChatSyncProjectionFixture.InsideRequest() )
                using ( var rockContext = new RockContext() )
                {
                    var service = new GroupMemberService( rockContext );
                    service.DeleteRange( service.Queryable().Where( m => m.Group.Guid == channel && m.PersonId == personId ).ToList() );
                    rockContext.SaveChanges();
                }

                var removed = platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null && r["absent_since"].Type != JTokenType.Null );
                Assert.IsNotNull( removed, "the membership deleted in Rock was never stamped absent on the platform" );
            }
        }

        [TestMethod]
        public void ABanListAddSetsTheGloballyBannedFlagOnThePlatform()
        {
            var platform = LocalPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var personId = fixture.AddPerson( "Banned" );
                var alias = fixture.PrimaryAliasGuid( personId );

                // Outside a request, as a workflow the job engine runs would add it.
                using ( var rockContext = new RockContext() )
                {
                    AddMember( rockContext, Rock.SystemGuid.Group.GROUP_CHAT_BAN_LIST.AsGuid(), personId );
                    rockContext.SaveChanges();
                }

                var row = platform.WaitForAlias( configuration.TenantId.Value, alias, r => r != null && ( bool? ) r["is_globally_banned"] == true );
                Assert.IsNotNull( row, "the ban made in Rock never reached the platform" );
            }
        }

        [TestMethod]
        public void TwentySavesAreTimedFromTheSaveToTheRowOnThePlatform()
        {
            var platform = LocalPlatform.FromEnvironment();

            using ( var fixture = new ChatSyncProjectionFixture() )
            using ( var recorder = new PushRecorder() )
            using ( ChatPlatformSyncHelper.OverrideImmediateSync( recorder ) )
            {
                var configuration = platform.ProvisionChurch();
                fixture.StoreConfiguration( configuration );

                var channel = fixture.AddChannel( fixture.SharedGroupTypeId, "Timed channel" );
                var elapsed = new List<double>();

                // One more than is timed: the first push in a process signs in, which later ones do not.
                for ( var i = 0; i <= TimedSaves; i++ )
                {
                    var personId = fixture.AddPerson( "Timed" + i );
                    var alias = fixture.PrimaryAliasGuid( personId );
                    Stopwatch stopwatch;

                    using ( ChatSyncProjectionFixture.InsideRequest() )
                    using ( var rockContext = new RockContext() )
                    {
                        AddMember( rockContext, channel, personId );
                        rockContext.SaveChanges();
                        stopwatch = Stopwatch.StartNew();
                    }

                    var row = platform.WaitForMember( configuration.TenantId.Value, channel, alias, r => r != null );
                    stopwatch.Stop();

                    Assert.IsNotNull( row, $"save {i} never reached the platform" );

                    if ( i > 0 )
                    {
                        elapsed.Add( stopwatch.Elapsed.TotalMilliseconds );
                    }
                }

                elapsed.Sort();

                TestContext.WriteLine( string.Join( Environment.NewLine,
                    $"saves timed: {elapsed.Count}, from SaveChanges returning to the row readable on the platform, polled every {PollInterval.TotalMilliseconds:F0} ms",
                    "elapsed ms: " + string.Join( ", ", elapsed.Select( e => e.ToString( "F1", CultureInfo.InvariantCulture ) ) ),
                    $"p50 ms: {Percentile( elapsed, 0.50 ):F1}",
                    $"p95 ms: {Percentile( elapsed, 0.95 ):F1}",
                    $"max ms: {elapsed.Last():F1}" ) );
            }
        }

        #endregion Tests

        #region Support

        private static void AddMember( RockContext rockContext, Guid groupGuid, int personId )
        {
            var group = new GroupService( rockContext ).Queryable( "GroupType" ).Single( g => g.Guid == groupGuid );

            new GroupMemberService( rockContext ).Add( new GroupMember
            {
                Guid = Guid.NewGuid(),
                GroupId = group.Id,
                GroupTypeId = group.GroupTypeId,
                PersonId = personId,
                GroupRoleId = group.GroupType.DefaultGroupRoleId.Value,
                GroupMemberStatus = GroupMemberStatus.Active
            } );
        }

        private static DateTimeOffset ParseInstant( string value )
        {
            return DateTimeOffset.Parse( value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind );
        }

        /// <summary>
        /// The nearest-rank percentile of values already sorted.
        /// </summary>
        private static double Percentile( IList<double> sorted, double fraction )
        {
            var rank = ( int ) Math.Ceiling( fraction * sorted.Count );

            return sorted[Math.Max( 0, rank - 1 )];
        }

        /// <summary>
        /// Sends every request on to the real platform, and keeps the read time of each push.
        /// </summary>
        private sealed class PushRecorder : DelegatingHandler
        {
            private readonly object _sync = new object();

            private readonly List<string> _readTimes = new List<string>();

            public PushRecorder()
                : base( new HttpClientHandler() )
            {
            }

            public IList<string> ReadTimes
            {
                get
                {
                    lock ( _sync )
                    {
                        return _readTimes.ToList();
                    }
                }
            }

            protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
            {
                if ( request.RequestUri.AbsolutePath == "/rest/v1/rpc/sync_push" && request.Headers.TryGetValues( "x-sync-read-at", out var values ) )
                {
                    lock ( _sync )
                    {
                        _readTimes.Add( values.First() );
                    }
                }

                return base.SendAsync( request, cancellationToken );
            }
        }

        /// <summary>
        /// The chat platform the environment names: provisioning a church on it, and reading its
        /// rows back.
        /// </summary>
        private sealed class LocalPlatform
        {
            private static readonly HttpClient _http = new HttpClient();

            private string _url;

            private string _publishableKey;

            private string _serviceRoleKey;

            private string _provisionSecret;

            /// <summary>
            /// The platform the environment names, or an Inconclusive result naming what is missing.
            /// </summary>
            public static LocalPlatform FromEnvironment()
            {
                var platform = new LocalPlatform
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
            public ChatPlatformConfiguration ProvisionChurch()
            {
                var tenantId = Guid.NewGuid();
                var kid = "kid-" + tenantId.ToString( "N" ).Substring( 0, 12 );
                var key = ChatSyncProjectionFixture.CreateSigningKey( kid );

                var body = new JObject
                {
                    ["tenant_id"] = tenantId.ToString(),
                    ["name"] = "Rock immediate sync " + tenantId.ToString( "N" ).Substring( 0, 8 ),
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
            public JObject WaitForMember( Guid tenantId, Guid channelId, Guid aliasGuid, Func<JObject, bool> isReady )
            {
                return WaitFor(
                    $"chat_channel_members?select=synced_at,absent_since&tenant_id=eq.{tenantId}&channel_id=eq.{channelId}&person_alias_guid=eq.{aliasGuid}",
                    isReady );
            }

            /// <summary>
            /// Reads an alias row until it satisfies a condition, or null when it never does.
            /// </summary>
            public JObject WaitForAlias( Guid tenantId, Guid aliasGuid, Func<JObject, bool> isReady )
            {
                return WaitFor(
                    $"chat_aliases?select=synced_at,is_globally_banned&tenant_id=eq.{tenantId}&person_alias_guid=eq.{aliasGuid}",
                    isReady );
            }

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

                        using ( var reader = new JsonTextReader( new StringReader( text ) ) { DateParseHandling = DateParseHandling.None } )
                        {
                            return JArray.Load( reader ).OfType<JObject>().FirstOrDefault();
                        }
                    }
                }
            }
        }

        #endregion Support
    }
}
