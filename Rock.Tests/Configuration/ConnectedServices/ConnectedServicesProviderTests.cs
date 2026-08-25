using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Configuration.ConnectedServices;
using Rock.Configuration.ConnectedServices.DataTransferObjects;
using Rock.Configuration.ConnectedServices.RockIntelligence;
using Rock.Enums.Configuration;
using Rock.Store;
using Rock.SystemKey;
using Rock.Tests.Shared;
using Rock.Tests.Shared.TestFramework;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Tests.Configuration.ConnectedServices
{
    /// <summary>
    /// Tests for <see cref="ConnectedServicesProvider"/>.
    /// </summary>
    [TestClass]
    public class ConnectedServicesProviderTests : MockDatabaseTestsBase
    {
        #region Constants

        /// <summary>
        /// Mirrors the private constant of the same name in
        /// <see cref="ConnectedServicesProvider"/>. Kept here as a plain
        /// string so tests can build URL paths that match the production
        /// code without exposing the constant more broadly.
        /// </summary>
        private const string RockIntelligenceServiceKey = "rock-iq";

        /// <summary>
        /// Mirrors the private constant of the same name in
        /// the Rock Shop service.
        /// </summary>
        private const string StoreOrganizationKeySettingKey = "StoreOrganizationKey";

        /// <summary>
        /// A sentinel bundle id used by tests that need "an id that is
        /// definitely not in the seeded manifest." Kept as a distinctive
        /// value so a failure log points straight at the test intent.
        /// </summary>
        private static readonly Guid UnknownBundleId = new Guid( "deadbeef-0000-0000-0000-000000000000" );

        #endregion

        #region Helpers

        /// <summary>
        /// Creates the scoped RockApp, a <see cref="RecordingMessageHandler"/>
        /// (our in-process stand-in for a real HTTP server), and a
        /// <see cref="ConnectedServicesProvider"/> whose HttpClient routes
        /// through that handler.
        /// </summary>
        /// <param name="deploymentEnvironment">The deployment environment the provider should report itself as running in. Defaults to <see cref="DeploymentEnvironment.Production"/> so normal tests exercise the production code path.</param>
        private static TestContext CreateTestContext( DeploymentEnvironment deploymentEnvironment = DeploymentEnvironment.Production )
        {
            var scope = TestHelper.CreateScopedRockAppWithMockDatabase();

            // Pre-seed empty attribute rows for every system-setting key the
            // provider (or the legacy Rock Store) will write to. This keeps
            // SystemSettings.SetValue on the UPDATE branch so it doesn't try
            // to resolve the TEXT FieldTypeCache -- which returns null in a
            // fresh mock context and would NRE on the .Id access.
            PrimeSystemSettingKeys(
                SystemSetting.CONNECTED_SERVICES_AUTH,
                SystemSetting.CONNECTED_SERVICES_CONFIGURATION,
                SystemSetting.CONNECTED_SERVICES_MANIFEST,
                StoreOrganizationKeySettingKey );

            var handler = new RecordingMessageHandler();
            var httpClient = new HttpClient( handler )
            {
                // The handler ignores the host and matches on the request's
                // AbsolutePath, so any well-formed base address works here.
                BaseAddress = new Uri( "http://test.local/" )
            };

            var provider = new ConnectedServicesProvider( httpClient, deploymentEnvironment );

            return new TestContext( scope, handler, httpClient, provider );
        }

        /// <summary>
        /// Builds a scoped RockApp + provider whose HTTP layer throws
        /// <see cref="HttpRequestException"/> on every request, mimicking a
        /// network-level failure. This exercises the inner catch blocks in
        /// the provider without needing a real broken endpoint.
        /// </summary>
        /// <param name="provider">On return, the provider bound to the broken HTTP client.</param>
        /// <param name="exception">The exception the handler will raise on every request. Defaults to a bare <see cref="HttpRequestException"/>.</param>
        private static IDisposable CreateBrokenNetworkScope( out ConnectedServicesProvider provider, Exception exception = null )
        {
            var scope = TestHelper.CreateScopedRockAppWithMockDatabase();

            PrimeSystemSettingKeys(
                SystemSetting.CONNECTED_SERVICES_AUTH,
                SystemSetting.CONNECTED_SERVICES_CONFIGURATION,
                SystemSetting.CONNECTED_SERVICES_MANIFEST,
                StoreOrganizationKeySettingKey );

            var handler = new RecordingMessageHandler();
            handler.SetAlwaysThrow( exception ?? new HttpRequestException( "Simulated network failure." ) );

            var httpClient = new HttpClient( handler )
            {
                BaseAddress = new Uri( "http://test.local/" )
            };

            provider = new ConnectedServicesProvider( httpClient, DeploymentEnvironment.Production );

            return scope;
        }

        /// <summary>
        /// Inserts empty placeholder Attribute rows into the mock DbContext
        /// for each supplied system-setting key. See <see cref="CreateTestContext"/>
        /// for why this is required.
        /// </summary>
        private static void PrimeSystemSettingKeys( params string[] keys )
        {
            var rockContext = RockApp.Current.CreateRockContext();
            var attributeSet = rockContext.Set<Rock.Model.Attribute>();
            var nextId = 1;

            foreach ( var key in keys )
            {
                attributeSet.Add( new Rock.Model.Attribute
                {
                    Id = nextId++,
                    EntityTypeId = null,
                    EntityTypeQualifierColumn = Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER,
                    EntityTypeQualifierValue = string.Empty,
                    Key = key,
                    Name = key.SplitCase(),
                    DefaultValue = string.Empty,
                    Guid = Guid.NewGuid(),
                    // Initialize collection navigation properties so
                    // AttributeCache.SetFromEntity doesn't NRE when it
                    // touches attribute.Categories and attribute.AttributeQualifiers.
                    Categories = new List<Rock.Model.Category>(),
                    AttributeQualifiers = new List<Rock.Model.AttributeQualifier>()
                } );
            }

            // Force the next SystemSettings load to hydrate from the mock
            // context so these primed rows are visible to the provider.
            SystemSettings.Remove();
        }

        /// <summary>
        /// Seeds a full <see cref="ConnectedServicesConfiguration"/> into the
        /// mock system settings, then flushes the SystemSettings cache so the
        /// next read from the provider hydrates from the mock database.
        /// </summary>
        private static void SeedConfiguration( ConnectedServicesConfiguration configuration )
        {
            SystemSettings.SetValue( SystemSetting.CONNECTED_SERVICES_CONFIGURATION, configuration.ToJson() );
            SystemSettings.Remove();
        }

        /// <summary>
        /// Seeds an auth token by writing a minimal configuration into system
        /// settings. Every method that requires the organization to be linked
        /// can be unblocked by calling this.
        /// </summary>
        private static void SeedAuthToken( string token )
        {
            SeedConfiguration( new ConnectedServicesConfiguration { AuthToken = token } );
        }

        /// <summary>
        /// Seeds the raw manifest JSON into system settings so the provider's
        /// <c>GetManifest</c> path returns the exact bytes we want to test
        /// against.
        /// </summary>
        private static void SeedManifest( string manifestJson )
        {
            SystemSettings.SetValue( SystemSetting.CONNECTED_SERVICES_MANIFEST, manifestJson );
            SystemSettings.Remove();
        }

        /// <summary>
        /// Seeds the pending auth-handshake context that a real
        /// <c>StartLinkOrganizationAsync</c> call would have persisted.
        /// </summary>
        private static void SeedAuthContext( string requestId, string verifier )
        {
            var context = new SparkAuthContext
            {
                RequestId = requestId,
                Verifier = verifier
            };

            SystemSettings.SetValue( SystemSetting.CONNECTED_SERVICES_AUTH, context.ToJson() );
            SystemSettings.Remove();
        }

        /// <summary>
        /// Flushes every cache SystemSettings.GetValue could be reading from.
        /// The mock DbContext doesn't fire the Attribute.PostSaveChanges hook
        /// that ordinarily invalidates AttributeCache/SystemSettings, so tests
        /// must do it themselves whenever a provider call has written through
        /// SystemSettings.SetValue and we then want to observe the new value.
        /// </summary>
        private static void FlushAllCaches()
        {
            RockCache.ClearAllCachedItems( false );
        }

        /// <summary>
        /// Reads back the persisted configuration by flushing all caches so
        /// we observe the mock DbContext's current state, not the AttributeCache
        /// snapshot captured on the first read.
        /// </summary>
        private static ConnectedServicesConfiguration ReadPersistedConfiguration()
        {
            FlushAllCaches();

            return SystemSettings.GetValue( SystemSetting.CONNECTED_SERVICES_CONFIGURATION )
                .FromJsonOrNull<ConnectedServicesConfiguration>();
        }

        /// <summary>
        /// Reads back the persisted auth-handshake context. Empty string is
        /// returned when the value has been cleared. Callers can use this to
        /// assert that a value was cleared as part of a flow.
        /// </summary>
        private static string ReadPersistedAuthSetting()
        {
            FlushAllCaches();

            return SystemSettings.GetValue( SystemSetting.CONNECTED_SERVICES_AUTH );
        }

        /// <summary>
        /// Reads back the persisted (decrypted) legacy store organization key.
        /// Flushes caches so provider-written values are observable.
        /// </summary>
        private static string ReadPersistedLegacyOrganizationKey()
        {
            FlushAllCaches();

            return StoreServiceBase.GetOrganizationKey();
        }

        /// <summary>
        /// Reads back the persisted raw manifest JSON.
        /// </summary>
        private static string ReadPersistedManifest()
        {
            FlushAllCaches();

            return SystemSettings.GetValue( SystemSetting.CONNECTED_SERVICES_MANIFEST );
        }

        /// <summary>
        /// Returns the value of the X-Gateway-Api-Key header on the request
        /// captured by <paramref name="handler"/>, or <c>null</c> when the
        /// header was not sent. Assumes exactly one request was captured.
        /// </summary>
        private static string GetSentAuthHeader( RecordingMessageHandler handler )
        {
            var request = handler.Requests.Single();

            if ( request.Headers.TryGetValue( "X-Gateway-Api-Key", out var values ) )
            {
                return values.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Returns the raw request body string of the single request captured
        /// by <paramref name="handler"/>.
        /// </summary>
        private static string GetSentRequestBody( RecordingMessageHandler handler )
        {
            return handler.Requests.Single().Body;
        }

        #endregion

        #region Constructor

        [TestMethod]
        public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>( () => new ConnectedServicesProvider( null, DeploymentEnvironment.Production ) );
        }

        #endregion

        #region StartLinkOrganizationAsync

        [TestMethod]
        public async Task StartLinkOrganizationAsync_WithSuccessResponse_ReturnsAuthUrlFromServer()
        {
            using ( var ctx = CreateTestContext() )
            {
                var responseBody = @"{
	""requestId"": ""eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImxvY2FsLWRldi12MSJ9.eyJyZXR1cm5VcmwiOiJodHRwOi8vbG9jYWxob3N0OjYyMjkvcGFnZS8xMjMiLCJ2ZXJpZmllckhhc2giOiI3R1NXdEhRZVFJY2g0V3pwWVlId1Npc0hnM25TUkh4TzBJR2VSOERTemw0IiwiY29udGV4dCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NjIyOS9vcmlnaW5hbCIsImV4cCI6MTc4NDg0MzQwN30.Y4Au9OZ7q6ojiA1_samm939oQvEt4Xd5SQAQMKWatoN-Xt6HTgcenKHe6zPPqIuhTN_pdBVmI3cVVH-eN2Z5yw"",
	""authUrl"": ""https://apigateway.rockrms.com/auth/v1/authorize?request_id=eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImxvY2FsLWRldi12MSJ9.eyJyZXR1cm5VcmwiOiJodHRwOi8vbG9jYWxob3N0OjYyMjkvcGFnZS8xMjMiLCJ2ZXJpZmllckhhc2giOiI3R1NXdEhRZVFJY2g0V3pwWVlId1Npc0hnM25TUkh4TzBJR2VSOERTemw0IiwiY29udGV4dCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NjIyOS9vcmlnaW5hbCIsImV4cCI6MTc4NDg0MzQwN30.Y4Au9OZ7q6ojiA1_samm939oQvEt4Xd5SQAQMKWatoN-Xt6HTgcenKHe6zPPqIuhTN_pdBVmI3cVVH-eN2Z5yw""
}";

                ctx.Handler.SetResponse( HttpMethod.Post, "/auth/v1/start", HttpStatusCode.OK, responseBody );

                var url = await ctx.Provider.StartLinkOrganizationAsync( "https://return.example/here", "ctx", CancellationToken.None );

                // The provider should return the AuthUrl field from the
                // response verbatim -- pull it back out of the body so the
                // assertion tracks whatever the paste currently contains.
                var expectedUrl = responseBody.FromJsonOrThrow<Dictionary<string, string>>()["authUrl"];
                Assert.AreEqual( expectedUrl, url );
            }
        }

        [TestMethod]
        public async Task StartLinkOrganizationAsync_WithSuccessResponse_PersistsAuthContext()
        {
            using ( var ctx = CreateTestContext() )
            {
                var responseBody = @"{
	""requestId"": ""eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImxvY2FsLWRldi12MSJ9.eyJyZXR1cm5VcmwiOiJodHRwOi8vbG9jYWxob3N0OjYyMjkvcGFnZS8xMjMiLCJ2ZXJpZmllckhhc2giOiI3R1NXdEhRZVFJY2g0V3pwWVlId1Npc0hnM25TUkh4TzBJR2VSOERTemw0IiwiY29udGV4dCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NjIyOS9vcmlnaW5hbCIsImV4cCI6MTc4NDg0MzQwN30.Y4Au9OZ7q6ojiA1_samm939oQvEt4Xd5SQAQMKWatoN-Xt6HTgcenKHe6zPPqIuhTN_pdBVmI3cVVH-eN2Z5yw"",
	""authUrl"": ""https://apigateway.rockrms.com/auth/v1/authorize?request_id=eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImxvY2FsLWRldi12MSJ9.eyJyZXR1cm5VcmwiOiJodHRwOi8vbG9jYWxob3N0OjYyMjkvcGFnZS8xMjMiLCJ2ZXJpZmllckhhc2giOiI3R1NXdEhRZVFJY2g0V3pwWVlId1Npc0hnM25TUkh4TzBJR2VSOERTemw0IiwiY29udGV4dCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NjIyOS9vcmlnaW5hbCIsImV4cCI6MTc4NDg0MzQwN30.Y4Au9OZ7q6ojiA1_samm939oQvEt4Xd5SQAQMKWatoN-Xt6HTgcenKHe6zPPqIuhTN_pdBVmI3cVVH-eN2Z5yw""
}";

                ctx.Handler.SetResponse( HttpMethod.Post, "/auth/v1/start", HttpStatusCode.OK, responseBody );

                await ctx.Provider.StartLinkOrganizationAsync( "https://return.example/here", "ctx", CancellationToken.None );

                var authContext = ReadPersistedAuthSetting().FromJsonOrNull<SparkAuthContext>();

                Assert.IsNotNull( authContext );

                var expectedRequestId = responseBody.FromJsonOrThrow<Dictionary<string, string>>()["requestId"];
                Assert.AreEqual( expectedRequestId, authContext.RequestId );
                Assert.IsFalse( authContext.Verifier.IsNullOrWhiteSpace(), "Verifier should have been generated and persisted." );
            }
        }

        [TestMethod]
        public async Task StartLinkOrganizationAsync_WithSuccessResponse_SendsVerifierHashMatchingStoredVerifier()
        {
            using ( var ctx = CreateTestContext() )
            {
                var responseBody = @"{
	""requestId"": ""eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImxvY2FsLWRldi12MSJ9.eyJyZXR1cm5VcmwiOiJodHRwOi8vbG9jYWxob3N0OjYyMjkvcGFnZS8xMjMiLCJ2ZXJpZmllckhhc2giOiI3R1NXdEhRZVFJY2g0V3pwWVlId1Npc0hnM25TUkh4TzBJR2VSOERTemw0IiwiY29udGV4dCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NjIyOS9vcmlnaW5hbCIsImV4cCI6MTc4NDg0MzQwN30.Y4Au9OZ7q6ojiA1_samm939oQvEt4Xd5SQAQMKWatoN-Xt6HTgcenKHe6zPPqIuhTN_pdBVmI3cVVH-eN2Z5yw"",
	""authUrl"": ""https://apigateway.rockrms.com/auth/v1/authorize?request_id=eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6ImxvY2FsLWRldi12MSJ9.eyJyZXR1cm5VcmwiOiJodHRwOi8vbG9jYWxob3N0OjYyMjkvcGFnZS8xMjMiLCJ2ZXJpZmllckhhc2giOiI3R1NXdEhRZVFJY2g0V3pwWVlId1Npc0hnM25TUkh4TzBJR2VSOERTemw0IiwiY29udGV4dCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NjIyOS9vcmlnaW5hbCIsImV4cCI6MTc4NDg0MzQwN30.Y4Au9OZ7q6ojiA1_samm939oQvEt4Xd5SQAQMKWatoN-Xt6HTgcenKHe6zPPqIuhTN_pdBVmI3cVVH-eN2Z5yw""
}";

                ctx.Handler.SetResponse( HttpMethod.Post, "/auth/v1/start", HttpStatusCode.OK, responseBody );

                await ctx.Provider.StartLinkOrganizationAsync( "https://return.example/here", "ctx", CancellationToken.None );

                // Recover the verifier we generated inside the provider from
                // the persisted auth context, then hash it ourselves and
                // compare against the hash the provider sent on the wire.
                var storedContext = ReadPersistedAuthSetting().FromJsonOrNull<SparkAuthContext>();
                Assert.IsNotNull( storedContext );

                var expectedHash = ComputeExpectedVerifierHash( storedContext.Verifier );

                var sentBody = ctx.Handler.Requests.Single().Body.FromJsonOrThrow<Dictionary<string, string>>();

                Assert.AreEqual( expectedHash, sentBody["verifierHash"] );
                Assert.AreEqual( "https://return.example/here", sentBody["returnUrl"] );
                Assert.AreEqual( "ctx", sentBody["context"] );
            }
        }

        [TestMethod]
        public async Task StartLinkOrganizationAsync_WithServerError_ThrowsHttpRequestException()
        {
            using ( var ctx = CreateTestContext() )
            {
                ctx.Handler.SetResponse( HttpMethod.Post, "/auth/v1/start", HttpStatusCode.InternalServerError );

                await Assert.ThrowsExactlyAsync<HttpRequestException>( () =>
                    ctx.Provider.StartLinkOrganizationAsync( "https://return.example/here", "ctx", CancellationToken.None ) );
            }
        }

        #endregion

        #region CompleteLinkOrganizationAsync

        [TestMethod]
        public async Task CompleteLinkOrganizationAsync_WithMatchingRequestId_ReturnsLinkResult()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthContext( "req-123", "verifier-abc" );

                var responseBody = @"{
	""token"": ""spk-m-v1-babxj6k28dcdbeags95fmklabce2qkomw5dx0bd61fiwjkfgmqjfs3oow95ugw77"",
	""context"": ""http://localhost:6229/link-organization"",
    ""organizationName"": ""Test Church"",
    ""organizationGuid"": ""1a2b3c4d-5e6f-7089-90ab-cdef12345678""
}";

                ctx.Handler.SetResponse( HttpMethod.Post, "/auth/v1/token", HttpStatusCode.OK, responseBody );

                var result = await ctx.Provider.CompleteLinkOrganizationAsync( "req-123", CancellationToken.None );

                Assert.AreEqual( "http://localhost:6229/link-organization", result.Context );
                Assert.AreEqual( "Test Church", result.OrganizationName );
            }
        }

        [TestMethod]
        public async Task CompleteLinkOrganizationAsync_WithMatchingRequestId_PersistsAuthToken()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthContext( "req-123", "verifier-abc" );

                var responseBody = @"{
	""token"": ""spk-m-v1-babxj6k28dcdbeags95fmklabce2qkomw5dx0bd61fiwjkfgmqjfs3oow95ugw77"",
	""context"": ""http://localhost:6229/link-organization"",
    ""organizationName"": ""Test Church"",
    ""organizationGuid"": ""1a2b3c4d-5e6f-7089-90ab-cdef12345678""
}";

                ctx.Handler.SetResponse( HttpMethod.Post, "/auth/v1/token", HttpStatusCode.OK, responseBody );

                await ctx.Provider.CompleteLinkOrganizationAsync( "req-123", CancellationToken.None );

                var persisted = ReadPersistedConfiguration();

                Assert.IsNotNull( persisted );
                Assert.AreEqual( "spk-m-v1-babxj6k28dcdbeags95fmklabce2qkomw5dx0bd61fiwjkfgmqjfs3oow95ugw77", persisted.AuthToken );
            }
        }

        [TestMethod]
        public async Task CompleteLinkOrganizationAsync_WithMatchingRequestId_ClearsAuthContextSetting()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthContext( "req-123", "verifier-abc" );

                var responseBody = @"{
	""token"": ""spk-m-v1-babxj6k28dcdbeags95fmklabce2qkomw5dx0bd61fiwjkfgmqjfs3oow95ugw77"",
	""context"": ""http://localhost:6229/link-organization"",
    ""organizationName"": ""Test Church"",
    ""organizationGuid"": ""1a2b3c4d-5e6f-7089-90ab-cdef12345678""
}";

                ctx.Handler.SetResponse( HttpMethod.Post, "/auth/v1/token", HttpStatusCode.OK, responseBody );

                await ctx.Provider.CompleteLinkOrganizationAsync( "req-123", CancellationToken.None );

                Assert.IsTrue( ReadPersistedAuthSetting().IsNullOrWhiteSpace(), "Auth-handshake setting should have been cleared." );
            }
        }

        [TestMethod]
        public async Task CompleteLinkOrganizationAsync_WithMatchingRequestId_SetsLegacyStoreOrganizationKey()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthContext( "req-123", "verifier-abc" );

                var responseBody = @"{
	""token"": ""spk-m-v1-babxj6k28dcdbeags95fmklabce2qkomw5dx0bd61fiwjkfgmqjfs3oow95ugw77"",
	""context"": ""http://localhost:6229/link-organization"",
    ""organizationName"": ""Test Church"",
    ""organizationGuid"": ""1a2b3c4d-5e6f-7089-90ab-cdef12345678""
}";

                ctx.Handler.SetResponse( HttpMethod.Post, "/auth/v1/token", HttpStatusCode.OK, responseBody );

                await ctx.Provider.CompleteLinkOrganizationAsync( "req-123", CancellationToken.None );

                Assert.AreEqual( "1a2b3c4d5e6f708990abcdef12345678", ReadPersistedLegacyOrganizationKey() );
            }
        }

        [TestMethod]
        public async Task CompleteLinkOrganizationAsync_WithoutStoredAuthContext_ThrowsAndDoesNotHitServer()
        {
            // No SeedAuthContext -- the CONNECTED_SERVICES_AUTH row is empty
            // from priming, so FromJsonOrNull returns null and the null-safe
            // RequestId comparison fires the guard.
            using ( var ctx = CreateTestContext() )
            {
                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.CompleteLinkOrganizationAsync( "any-request-id", CancellationToken.None ) );

                Assert.IsEmpty( ctx.Handler.Requests, "No HTTP call should be attempted when the handshake context is missing." );
            }
        }

        [TestMethod]
        public async Task CompleteLinkOrganizationAsync_WithMismatchedRequestId_ThrowsAndClearsAuthContext()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthContext( "expected-req-id", "verifier-abc" );

                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.CompleteLinkOrganizationAsync( "wrong-req-id", CancellationToken.None ) );

                Assert.IsTrue( ReadPersistedAuthSetting().IsNullOrWhiteSpace(),
                    "The stored handshake should be cleared even when the request id does not match." );
            }
        }

        #endregion

        #region UpgradeLegacyIdentifierAsync

        [TestMethod]
        public async Task UpgradeLegacyIdentifierAsync_WithoutLegacyKey_ThrowsInvalidOperation()
        {
            using ( var ctx = CreateTestContext() )
            {
                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.UpgradeLegacyIdentifierAsync( CancellationToken.None ) );
            }
        }

        [TestMethod]
        public async Task UpgradeLegacyIdentifierAsync_WithLegacyKey_PersistsAuthToken()
        {
            using ( var ctx = CreateTestContext() )
            {
                StoreService.SetOrganizationKey( "legacy-org-guid" );
                SystemSettings.Remove();

                var responseBody = @"{
	""token"": ""spk-m-v1-babxj6k28dcdbeags95fmklabce2qkomw5dx0bd61fiwjkfgmqjfs3oow95ugw77"",
    ""organizationName"": ""Legacy Church"",
    ""organizationGuid"": ""1a2b3c4d-5e6f-7089-90ab-cdef12345678""
}";

                ctx.Handler.SetResponse( HttpMethod.Post, "/auth/v1/upgrade", HttpStatusCode.OK, responseBody );

                var result = await ctx.Provider.UpgradeLegacyIdentifierAsync( CancellationToken.None );

                Assert.AreEqual( "Legacy Church", result.OrganizationName );
                Assert.AreEqual( "spk-m-v1-babxj6k28dcdbeags95fmklabce2qkomw5dx0bd61fiwjkfgmqjfs3oow95ugw77", ReadPersistedConfiguration()?.AuthToken );
            }
        }

        #endregion

        #region IsOrganizationLinked / IsLegacyOrganizationLinked

        [TestMethod]
        public void IsOrganizationLinked_WithoutAuthToken_ReturnsFalse()
        {
            using ( var ctx = CreateTestContext() )
            {
                Assert.IsFalse( ctx.Provider.IsOrganizationLinked() );
            }
        }

        [TestMethod]
        public void IsOrganizationLinked_WithAuthToken_ReturnsTrue()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "any-token" );

                Assert.IsTrue( ctx.Provider.IsOrganizationLinked() );
            }
        }

        [TestMethod]
        public void IsLegacyOrganizationLinked_WithoutLegacyKey_ReturnsFalse()
        {
            using ( var ctx = CreateTestContext() )
            {
                Assert.IsFalse( ctx.Provider.IsLegacyOrganizationLinked() );
            }
        }

        [TestMethod]
        public void IsLegacyOrganizationLinked_WithLegacyKey_ReturnsTrue()
        {
            using ( var ctx = CreateTestContext() )
            {
                StoreService.SetOrganizationKey( "legacy-org-guid" );
                SystemSettings.Remove();

                Assert.IsTrue( ctx.Provider.IsLegacyOrganizationLinked() );
            }
        }

        #endregion

        #region UpdateManifestAsync

        [TestMethod]
        public async Task UpdateManifestAsync_WithoutAuthToken_ThrowsInvalidOperation()
        {
            using ( var ctx = CreateTestContext() )
            {
                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.UpdateManifestAsync( CancellationToken.None ) );
            }
        }

        [TestMethod]
        public async Task UpdateManifestAsync_WithSuccessResponse_PersistsManifestJson()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                var manifestJson = @"{
	""createdDateTime"": ""2026-07-16T17:29:53.4671315+00:00"",
    ""services"": [
	    {
            ""serviceId"": ""rock-iq"",
            ""status"": ""ok"",
            ""configuration"": {},
		    ""bundle"": {
				""id"": ""95B3D6AB-831B-BEA5-4CD2-735AB1E71212"",
				""name"": ""Standard"",
				""order"": 1,
				""replacesBundles"": [],
				""settings"": {
					""url"": ""https://openrouter.ai/api/v1"",
					""apiKey"": ""sk-or-v1-fakekey"",
					""models"": [
						{
							""type"": ""General"",
							""id"": ""std-general""
						},
						{
							""type"": ""Code"",
							""id"": ""std-code""
						},
						{
							""type"": ""Moderation"",
							""id"": ""std-moderation""
						}
					]
				}
			}
		}
	]
}";

                ctx.Handler.SetResponse( HttpMethod.Get, "/api/v1/config/manifest", HttpStatusCode.OK, manifestJson );

                await ctx.Provider.UpdateManifestAsync( CancellationToken.None );

                // The provider stores the manifest verbatim so downstream
                // code sees the exact bytes the server returned.
                Assert.AreEqual( manifestJson, ReadPersistedManifest() );
            }
        }

        [TestMethod]
        public async Task UpdateManifestAsync_WithSuccessResponse_SendsAuthTokenHeader()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "my-auth-token" );

                ctx.Handler.SetResponse( HttpMethod.Get, "/api/v1/config/manifest", HttpStatusCode.OK, "{\"services\":[]}" );

                await ctx.Provider.UpdateManifestAsync( CancellationToken.None );

                Assert.AreEqual( "my-auth-token", GetSentAuthHeader( ctx.Handler ) );
            }
        }

        [TestMethod]
        public async Task UpdateManifestAsync_WithCancelledToken_PropagatesOperationCanceled()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                // Long delay so the token has to actually be honored to
                // short-circuit the request; a fresh, already-cancelled
                // token proves the provider is passing it through to
                // HttpClient.SendAsync rather than swallowing it.
                ctx.Handler.SetResponse( HttpMethod.Get, "/api/v1/config/manifest", HttpStatusCode.OK, "{\"services\":[]}", TimeSpan.FromSeconds( 30 ) );

                using ( var cts = new CancellationTokenSource() )
                {
                    cts.Cancel();

                    await Assert.ThrowsAsync<OperationCanceledException>( () =>
                        ctx.Provider.UpdateManifestAsync( cts.Token ) );
                }
            }
        }

        [TestMethod]
        public async Task UpdateManifestAsync_WithMalformedManifest_ThrowsAndDoesNotPersist()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                ctx.Handler.SetResponse( HttpMethod.Get, "/api/v1/config/manifest", HttpStatusCode.OK, "this is not json" );

                await Assert.ThrowsAsync<JsonException>( () =>
                    ctx.Provider.UpdateManifestAsync( CancellationToken.None ) );

                Assert.IsTrue( ReadPersistedManifest().IsNullOrWhiteSpace() );
            }
        }

        [TestMethod]
        public async Task UpdateManifestAsync_WithCurrentBundleStillPresent_RefreshesSelectionFromManifest()
        {
            using ( var ctx = CreateTestContext() )
            {
                var currentBundleId = Guid.Parse( "95B3D6AB-831B-BEA5-4CD2-735AB1E71212" );

                SeedConfiguration( new ConnectedServicesConfiguration
                {
                    AuthToken = "token",
                    RockIntelligence = new ServiceConfiguration
                    {
                        Bundle = new ServiceEntryBundle<Settings>
                        {
                            Id = currentBundleId,
                            Name = "Original Bundle"
                        }
                    }
                } );

                var manifestJson = @"{
	""createdDateTime"": ""2026-07-16T17:29:53.4671315+00:00"",
    ""services"": [
	    {
            ""serviceId"": ""rock-iq"",
            ""status"": ""ok"",
            ""configuration"": {},
		    ""bundle"": {
				""id"": ""95B3D6AB-831B-BEA5-4CD2-735AB1E71212"",
				""name"": ""Standard"",
				""order"": 1,
				""replacesBundles"": [],
				""settings"": {
					""url"": ""https://openrouter.ai/api/v1"",
					""apiKey"": ""sk-or-v1-fakekey"",
					""models"": [
						{
							""type"": ""General"",
							""id"": ""std-general""
						},
						{
							""type"": ""Code"",
							""id"": ""std-code""
						},
						{
							""type"": ""Moderation"",
							""id"": ""std-moderation""
						}
					]
				}
			}
		}
	]
}";

                ctx.Handler.SetResponse( HttpMethod.Get, "/api/v1/config/manifest", HttpStatusCode.OK, manifestJson );

                await ctx.Provider.UpdateManifestAsync( CancellationToken.None );

                var persisted = ReadPersistedConfiguration();
                Assert.AreEqual( currentBundleId, persisted?.RockIntelligence?.Bundle?.Id );
                // The same-id bundle in the manifest is named "Standard" and
                // carries a populated Settings block; the locally-cached
                // name/settings must be overwritten so remote edits made by
                // Spark propagate to Rock on the next manifest pull.
                Assert.AreEqual( "Standard", persisted?.RockIntelligence?.Bundle?.Name );
                Assert.IsNotNull( persisted?.RockIntelligence?.Bundle?.Settings, "Settings from the manifest should have replaced the previously-empty local Settings." );
                Assert.AreEqual( "https://openrouter.ai/api/v1", persisted.RockIntelligence.Bundle.Settings.Url );
            }
        }

        #endregion

        #region SetRockIntelligenceBundle

        [TestMethod]
        public async Task SetRockIntelligenceBundle_WithoutLink_ThrowsInvalidOperation()
        {
            using ( var ctx = CreateTestContext() )
            {
                // Any non-empty Guid works here; the "not linked" guard fires
                // before the identifier is looked up in the manifest.
                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.SetRockIntelligenceBundleAsync( UnknownBundleId, CancellationToken.None ) );
            }
        }

        [TestMethod]
        public async Task SetRockIntelligenceBundle_WithUnknownIdentifier_ReturnsFailureResult()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/bundle", HttpStatusCode.BadRequest, @"{ ""error"": ""invalid bundle"" }" );

                var result = await ctx.Provider.SetRockIntelligenceBundleAsync( UnknownBundleId, CancellationToken.None );

                Assert.IsFalse( result.IsSuccess );
                Assert.IsFalse( result.ErrorMessage.IsNullOrWhiteSpace() );
            }
        }

        [TestMethod]
        public async Task SetRockIntelligenceBundle_WithKnownIdentifier_SetsCurrentBundle()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                var bundleId = Guid.Parse( "423AFA09-0408-49FC-AA75-CA9F335E82A3" );
                var responseJson = @"{
	""bundleId"": ""423AFA09-0408-49FC-AA75-CA9F335E82A3"",
	""serviceEntry"": {
        ""serviceId"": ""rock-iq"",
        ""status"": ""ok"",
		""bundle"": {
			""id"": ""423AFA09-0408-49FC-AA75-CA9F335E82A3"",
			""name"": ""Economy""
		}
	}
}";

                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/bundle", HttpStatusCode.OK, responseJson );

                var result = await ctx.Provider.SetRockIntelligenceBundleAsync( bundleId, CancellationToken.None );

                Assert.IsTrue( result.IsSuccess );
                var persisted = ReadPersistedConfiguration();
                Assert.AreEqual( bundleId, persisted?.RockIntelligence?.Bundle?.Id );
                // 423AFA09-... in the seeded manifest is the "Economy" bundle.
                Assert.AreEqual( "Economy", persisted?.RockIntelligence?.Bundle?.Name );
            }
        }

        #endregion

        #region GetRockIntelligenceUsageAsync

        [TestMethod]
        public async Task GetRockIntelligenceUsageAsync_WithoutAuthToken_ThrowsInvalidOperation()
        {
            using ( var ctx = CreateTestContext() )
            {
                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.GetRockIntelligenceUsageAsync( CancellationToken.None ) );
            }
        }

        [TestMethod]
        public async Task GetRockIntelligenceUsageAsync_WithPopulatedResponses_ReturnsMergedUsage()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                var usageBody = @"{
	""balance"": 573.7508,
	""monthlyUsage"": 0.9831
}";
                var spendingLimitBody = @"{
	""spendingLimit"": 10
}";

                ctx.Handler.SetResponse( HttpMethod.Get, $"/svcs/v1/{RockIntelligenceServiceKey}/usage", HttpStatusCode.OK, usageBody );
                ctx.Handler.SetResponse( HttpMethod.Get, $"/svcs/v1/{RockIntelligenceServiceKey}/spending-limit", HttpStatusCode.OK, spendingLimitBody );

                var usage = await ctx.Provider.GetRockIntelligenceUsageAsync( CancellationToken.None );

                Assert.AreEqual( 573.7508m, usage.BalanceRemaining );
                Assert.AreEqual( 0.9831m, usage.CurrentMonthSpending );
                Assert.AreEqual( 10m, usage.MonthlySpendLimit );
            }
        }

        [TestMethod]
        public async Task GetRockIntelligenceUsageAsync_WithNullFields_DefaultsToZero()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                var usageBody = @"{
	""balance"": null,
	""monthlyUsage"": null
}";
                var spendingLimitBody = @"{
	""spendingLimit"": null
}";

                ctx.Handler.SetResponse( HttpMethod.Get, $"/svcs/v1/{RockIntelligenceServiceKey}/usage", HttpStatusCode.OK, usageBody );
                ctx.Handler.SetResponse( HttpMethod.Get, $"/svcs/v1/{RockIntelligenceServiceKey}/spending-limit", HttpStatusCode.OK, spendingLimitBody );

                var usage = await ctx.Provider.GetRockIntelligenceUsageAsync( CancellationToken.None );

                Assert.AreEqual( 0m, usage.BalanceRemaining );
                Assert.AreEqual( 0m, usage.CurrentMonthSpending );
                Assert.AreEqual( 0m, usage.MonthlySpendLimit );
            }
        }

        #endregion

        #region SetRockIntelligenceMonthlySpendLimitAsync

        [TestMethod]
        public async Task SetRockIntelligenceMonthlySpendLimitAsync_WithoutAuthToken_ThrowsInvalidOperation()
        {
            using ( var ctx = CreateTestContext() )
            {
                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.SetRockIntelligenceMonthlySpendLimitAsync( 25m, CancellationToken.None ) );
            }
        }

        [TestMethod]
        public async Task SetRockIntelligenceMonthlySpendLimitAsync_WithSuccessResponse_ReturnsSuccessResult()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                var responseJson = @"{
	""spendingLimit"": 10
}";
                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/spending-limit", HttpStatusCode.OK, responseJson );

                var result = await ctx.Provider.SetRockIntelligenceMonthlySpendLimitAsync( 25m, CancellationToken.None );

                Assert.IsTrue( result.IsSuccess );
                Assert.IsTrue( result.ErrorMessage.IsNullOrWhiteSpace() );

                // Confirm the amount actually went out on the wire; otherwise
                // a "silently sent zero" bug would look like a success.
                var sent = GetSentRequestBody( ctx.Handler ).FromJsonOrThrow<SpendingLimitResponse>();
                Assert.AreEqual( 25m, sent.SpendingLimit );
            }
        }

        [TestMethod]
        public async Task SetRockIntelligenceMonthlySpendLimitAsync_WithSuccessResponse_SendsAuthTokenHeader()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "my-auth-token" );

                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/spending-limit", HttpStatusCode.OK );

                await ctx.Provider.SetRockIntelligenceMonthlySpendLimitAsync( 25m, CancellationToken.None );

                Assert.AreEqual( "my-auth-token", GetSentAuthHeader( ctx.Handler ) );
            }
        }

        [TestMethod]
        public async Task SetRockIntelligenceMonthlySpendLimitAsync_WithCancelledToken_PropagatesOperationCanceled()
        {
            // Same shape as the OneTimeBoost cancellation test: the inner
            // catch is HttpRequestException-only, so cancellation must
            // propagate rather than be turned into a failure result.
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/spending-limit", HttpStatusCode.OK, delay: TimeSpan.FromSeconds( 30 ) );

                using ( var cts = new CancellationTokenSource() )
                {
                    cts.Cancel();

                    await Assert.ThrowsAsync<OperationCanceledException>( () =>
                        ctx.Provider.SetRockIntelligenceMonthlySpendLimitAsync( 25m, cts.Token ) );
                }
            }
        }

        [TestMethod]
        public async Task SetRockIntelligenceMonthlySpendLimitAsync_WithNetworkFault_ReturnsFailureResult()
        {
            // See the OneTimeBoost network-fault test for the shape of the
            // "always throw HttpRequestException" scope used here. No
            // InnerException, so the outer message surfaces.
            using ( var scope = CreateBrokenNetworkScope( out var provider,
                new HttpRequestException( "outer network message" ) ) )
            {
                SeedAuthToken( "token" );

                var result = await provider.SetRockIntelligenceMonthlySpendLimitAsync( 25m, CancellationToken.None );

                Assert.IsFalse( result.IsSuccess );
                StringAssert.Contains( result.ErrorMessage, "outer network message" );
            }
        }

        [TestMethod]
        public async Task SetRockIntelligenceMonthlySpendLimitAsync_WithNetworkFaultWithInnerException_ReturnsInnerMessage()
        {
            using ( var scope = CreateBrokenNetworkScope( out var provider,
                new HttpRequestException( "outer wrapper message", new Exception( "inner socket detail" ) ) ) )
            {
                SeedAuthToken( "token" );

                var result = await provider.SetRockIntelligenceMonthlySpendLimitAsync( 25m, CancellationToken.None );

                Assert.IsFalse( result.IsSuccess );
                StringAssert.Contains( result.ErrorMessage, "inner socket detail" );
                Assert.DoesNotContain( "outer wrapper message", result.ErrorMessage, "Outer message should not leak through when an InnerException is present." );
            }
        }

        [TestMethod]
        public async Task SetRockIntelligenceMonthlySpendLimitAsync_WithServerError_ReturnsFailureResult()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/spending-limit", HttpStatusCode.InternalServerError );

                var result = await ctx.Provider.SetRockIntelligenceMonthlySpendLimitAsync( 25m, CancellationToken.None );

                Assert.IsFalse( result.IsSuccess );
                Assert.IsFalse( result.ErrorMessage.IsNullOrWhiteSpace() );
            }
        }

        #endregion

        #region ApplyRockIntelligenceOneTimeBoostAsync

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_WithoutAuthToken_ThrowsInvalidOperation()
        {
            using ( var ctx = CreateTestContext() )
            {
                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, CancellationToken.None ) );
            }
        }

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_WithCompleteBody_ReturnsCompleteStatus()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                var body = @"{
	""status"": ""complete"",
	""message"": ""Payment accepted. Your balance has been updated."",
	""amount"": 10.00
}";

                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/one-time-payment", HttpStatusCode.OK, body );

                var result = await ctx.Provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, CancellationToken.None );

                Assert.AreEqual( OneTimeBoostStatus.Complete, result.Status );
                Assert.AreEqual( 10.00m, result.Amount );
                Assert.AreEqual( "Payment accepted. Your balance has been updated.", result.Message );

                // Confirm the amount charged actually went out on the wire.
                var sent = GetSentRequestBody( ctx.Handler ).FromJsonOrThrow<Dictionary<string, decimal>>();
                Assert.AreEqual( 10m, sent["amount"] );
            }
        }

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_WithPendingBody_ReturnsPendingStatus()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                var body = @"{
	""status"": ""pending"",
	""message"": ""Payment accepted. Your balance update is in progress; it may take a few minutes to appear."",
	""amount"": 10.00
}";

                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/one-time-payment", HttpStatusCode.OK, body );

                var result = await ctx.Provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, CancellationToken.None );

                Assert.AreEqual( OneTimeBoostStatus.Pending, result.Status );
            }
        }

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_WithDeclinedBody_ReturnsDeclinedStatus()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                var body = @"{
	""status"": ""declined"",
	""message"": ""Your card could not be charged."",
	""amount"": 10.00
}";

                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/one-time-payment", HttpStatusCode.BadRequest, body );

                var result = await ctx.Provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, CancellationToken.None );

                Assert.AreEqual( OneTimeBoostStatus.Declined, result.Status );
            }
        }

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_With200AndEmptyBody_InfersCompleteFromStatusCode()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                // 200 with no body exercises the OK arm of the HTTP-status
                // fallback in InferOneTimeBoostResultFromHttpStatus, which
                // the body-driven happy-path tests can't reach.
                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/one-time-payment", HttpStatusCode.OK );

                var result = await ctx.Provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, CancellationToken.None );

                Assert.AreEqual( OneTimeBoostStatus.Complete, result.Status );
            }
        }

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_With402AndEmptyBody_InfersDeclinedFromStatusCode()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                // PaymentRequired (402) is the sibling of 400 in the switch
                // statement and represents a payment-declined-by-gateway path
                // that a well-behaved server can legitimately return.
                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/one-time-payment", HttpStatusCode.PaymentRequired );

                var result = await ctx.Provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, CancellationToken.None );

                Assert.AreEqual( OneTimeBoostStatus.Declined, result.Status );
            }
        }

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_With202AndEmptyBody_InfersPendingFromStatusCode()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/one-time-payment", HttpStatusCode.Accepted );

                var result = await ctx.Provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, CancellationToken.None );

                Assert.AreEqual( OneTimeBoostStatus.Pending, result.Status );
            }
        }

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_With400AndEmptyBody_InfersDeclinedFromStatusCode()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/one-time-payment", HttpStatusCode.BadRequest );

                var result = await ctx.Provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, CancellationToken.None );

                Assert.AreEqual( OneTimeBoostStatus.Declined, result.Status );
            }
        }

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_WithCancelledToken_PropagatesOperationCanceled()
        {
            // The inner catch matches HttpRequestException only, so a
            // cancelled token should surface as OperationCanceledException
            // rather than being converted into an Error result.
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/one-time-payment", HttpStatusCode.OK, "{\"status\":\"complete\"}", TimeSpan.FromSeconds( 30 ) );

                using ( var cts = new CancellationTokenSource() )
                {
                    cts.Cancel();

                    await Assert.ThrowsAsync<OperationCanceledException>( () =>
                        ctx.Provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, cts.Token ) );
                }
            }
        }

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_WithNetworkFault_ReturnsErrorStatus()
        {
            // The broken-network scope makes every request raise
            // HttpRequestException at the SendAsync layer, which is the
            // path the provider's inner catch handles. No InnerException is
            // attached, so the outer message is what surfaces.
            using ( var scope = CreateBrokenNetworkScope( out var provider,
                new HttpRequestException( "outer network message" ) ) )
            {
                SeedAuthToken( "token" );

                var result = await provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, CancellationToken.None );

                Assert.AreEqual( OneTimeBoostStatus.Error, result.Status );
                StringAssert.Contains( result.Message, "outer network message" );
            }
        }

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_WithNetworkFaultWithInnerException_ReturnsInnerMessage()
        {
            // When the wrapped exception carries an InnerException (as real
            // .NET network failures typically do), the provider surfaces the
            // inner message instead of the outer one.
            using ( var scope = CreateBrokenNetworkScope( out var provider,
                new HttpRequestException( "outer wrapper message", new Exception( "inner socket detail" ) ) ) )
            {
                SeedAuthToken( "token" );

                var result = await provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, CancellationToken.None );

                Assert.AreEqual( OneTimeBoostStatus.Error, result.Status );
                StringAssert.Contains( result.Message, "inner socket detail" );
                Assert.DoesNotContain( "outer wrapper message", result.Message, "Outer message should not leak through when an InnerException is present." );
            }
        }

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_With500AndEmptyBody_ReturnsErrorStatus()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                ctx.Handler.SetResponse( HttpMethod.Post, $"/svcs/v1/{RockIntelligenceServiceKey}/one-time-payment", HttpStatusCode.InternalServerError );

                var result = await ctx.Provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, CancellationToken.None );

                Assert.AreEqual( OneTimeBoostStatus.Error, result.Status );
            }
        }

        #endregion

        #region GetCreditCardSummaryAsync

        [TestMethod]
        public async Task GetCreditCardSummaryAsync_WithoutAuthToken_ThrowsInvalidOperation()
        {
            using ( var ctx = CreateTestContext() )
            {
                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.GetCreditCardSummaryAsync( CancellationToken.None ) );
            }
        }

        [TestMethod]
        public async Task GetCreditCardSummaryAsync_WithSuccessResponse_ReturnsBag()
        {
            using ( var ctx = CreateTestContext() )
            {
                SeedAuthToken( "token" );

                var body = @"{
	""cardType"": ""Visa"",
	""expirationMonth"": 7,
	""expirationYear"": 2026,
	""lastFourDigits"": ""9251"",
	""isCardOnFile"": true,
	""isCardExpired"": false,
	""isCardExpiringSoon"": false
}";

                ctx.Handler.SetResponse( HttpMethod.Get, "/api/v1/billing/credit-card-summary", HttpStatusCode.OK, body );

                var bag = await ctx.Provider.GetCreditCardSummaryAsync( CancellationToken.None );

                Assert.IsNotNull( bag );
                Assert.AreEqual( "Visa", bag.CardType );
                Assert.AreEqual( 7, bag.ExpirationMonth );
                Assert.AreEqual( 2026, bag.ExpirationYear );
                Assert.AreEqual( "9251", bag.LastFourDigits );
                Assert.IsTrue( bag.IsCardOnFile );
                Assert.IsFalse( bag.IsCardExpired );
                Assert.IsFalse( bag.IsCardExpiringSoon );
            }
        }

        #endregion

        #region Demo Environment

        [TestMethod]
        public async Task StartLinkOrganizationAsync_WhenDemo_ThrowsAndDoesNotHitServer()
        {
            using ( var ctx = CreateTestContext( DeploymentEnvironment.Demo ) )
            {
                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.StartLinkOrganizationAsync( "https://return.example/here", "ctx", CancellationToken.None ) );

                Assert.IsEmpty( ctx.Handler.Requests, "No HTTP call should be attempted when running in the demo environment." );
                Assert.IsTrue( ReadPersistedAuthSetting().IsNullOrWhiteSpace(), "No auth-handshake context should be persisted in the demo environment." );
            }
        }

        [TestMethod]
        public async Task UpgradeLegacyIdentifierAsync_WhenDemo_ThrowsAndDoesNotHitServer()
        {
            using ( var ctx = CreateTestContext( DeploymentEnvironment.Demo ) )
            {
                // Seed a legacy key so we can prove the demo guard fires
                // BEFORE the "missing legacy identifier" guard. Without this,
                // an unrelated InvalidOperationException from the missing-key
                // branch could pass the assertion for the wrong reason.
                StoreService.SetOrganizationKey( "legacy-org-guid" );
                SystemSettings.Remove();

                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.UpgradeLegacyIdentifierAsync( CancellationToken.None ) );

                Assert.IsEmpty( ctx.Handler.Requests, "No HTTP call should be attempted when running in the demo environment." );
            }
        }

        [TestMethod]
        public async Task UpdateManifestAsync_WhenDemo_ReturnsWithoutHittingServerOrPersisting()
        {
            using ( var ctx = CreateTestContext( DeploymentEnvironment.Demo ) )
            {
                // A seeded auth token would normally unblock the manifest
                // pull; in demo the method should short-circuit before it
                // reads the token or dispatches a request.
                SeedAuthToken( "token" );

                await ctx.Provider.UpdateManifestAsync( CancellationToken.None );

                Assert.IsEmpty( ctx.Handler.Requests, "No HTTP call should be attempted when running in the demo environment." );
                Assert.IsTrue( ReadPersistedManifest().IsNullOrWhiteSpace(), "No manifest should be persisted when running in the demo environment." );
            }
        }

        [TestMethod]
        public void IsOrganizationLinked_WhenDemo_ReturnsFalseEvenWhenAuthTokenIsPresent()
        {
            // GetConfiguration short-circuits to null in demo, so the auth
            // token appears absent regardless of what's on disk. This is what
            // hides the connected-services UI on demo servers.
            using ( var ctx = CreateTestContext( DeploymentEnvironment.Demo ) )
            {
                SeedAuthToken( "token" );

                Assert.IsFalse( ctx.Provider.IsOrganizationLinked() );
            }
        }

        [TestMethod]
        public async Task SetRockIntelligenceBundle_WhenDemo_ThrowsInvalidOperation()
        {
            // In demo, IsOrganizationLinked always returns false (see the
            // previous test), so the "not linked" guard at the top of
            // SetRockIntelligenceBundle fires before any configuration write
            // is attempted.
            using ( var ctx = CreateTestContext( DeploymentEnvironment.Demo ) )
            {
                SeedAuthToken( "token" );

                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.SetRockIntelligenceBundleAsync( UnknownBundleId, CancellationToken.None ) );
            }
        }

        [TestMethod]
        public async Task GetRockIntelligenceUsageAsync_WhenDemo_ThrowsAndDoesNotHitServer()
        {
            // In demo, GetConfiguration returns null so GetAuthToken returns
            // null and the auth-token guard trips before any HTTP call.
            using ( var ctx = CreateTestContext( DeploymentEnvironment.Demo ) )
            {
                SeedAuthToken( "token" );

                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.GetRockIntelligenceUsageAsync( CancellationToken.None ) );

                Assert.IsEmpty( ctx.Handler.Requests, "No HTTP call should be attempted when running in the demo environment." );
            }
        }

        [TestMethod]
        public async Task SetRockIntelligenceMonthlySpendLimitAsync_WhenDemo_ThrowsAndDoesNotHitServer()
        {
            using ( var ctx = CreateTestContext( DeploymentEnvironment.Demo ) )
            {
                SeedAuthToken( "token" );

                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.SetRockIntelligenceMonthlySpendLimitAsync( 25m, CancellationToken.None ) );

                Assert.IsEmpty( ctx.Handler.Requests, "No HTTP call should be attempted when running in the demo environment." );
            }
        }

        [TestMethod]
        public async Task ApplyRockIntelligenceOneTimeBoostAsync_WhenDemo_ThrowsAndDoesNotHitServer()
        {
            using ( var ctx = CreateTestContext( DeploymentEnvironment.Demo ) )
            {
                SeedAuthToken( "token" );

                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.ApplyRockIntelligenceOneTimeBoostAsync( 10m, CancellationToken.None ) );

                Assert.IsEmpty( ctx.Handler.Requests, "No HTTP call should be attempted when running in the demo environment." );
            }
        }

        [TestMethod]
        public async Task GetCreditCardSummaryAsync_WhenDemo_ThrowsAndDoesNotHitServer()
        {
            using ( var ctx = CreateTestContext( DeploymentEnvironment.Demo ) )
            {
                SeedAuthToken( "token" );

                await Assert.ThrowsExactlyAsync<InvalidOperationException>( () =>
                    ctx.Provider.GetCreditCardSummaryAsync( CancellationToken.None ) );

                Assert.IsEmpty( ctx.Handler.Requests, "No HTTP call should be attempted when running in the demo environment." );
            }
        }

        #endregion

        #region Support Types

        /// <summary>
        /// Aggregates the disposables that every test needs so a single
        /// <c>using</c> statement in each test drives the whole teardown.
        /// </summary>
        private sealed class TestContext : IDisposable
        {
            public TestContext( IDisposable scope, RecordingMessageHandler handler, HttpClient httpClient, ConnectedServicesProvider provider )
            {
                Scope = scope;
                Handler = handler;
                HttpClient = httpClient;
                Provider = provider;
            }

            public IDisposable Scope { get; }

            public RecordingMessageHandler Handler { get; }

            public HttpClient HttpClient { get; }

            public ConnectedServicesProvider Provider { get; }

            public void Dispose()
            {
                // HttpClient owns the handler and disposes it for us.
                HttpClient?.Dispose();
                Scope?.Dispose();
            }
        }

        #endregion

        #region Local Helpers

        /// <summary>
        /// Recomputes the verifier hash the same way the provider does. Used
        /// to prove the SHA-256 + base64url encoding on the wire matches
        /// the verifier the provider persisted.
        /// </summary>
        private static string ComputeExpectedVerifierHash( string verifier )
        {
            using ( var sha = System.Security.Cryptography.SHA256.Create() )
            {
                var hash = sha.ComputeHash( System.Text.Encoding.UTF8.GetBytes( verifier ) );

                return Base64UrlEncoder.Encode( hash );
            }
        }

        #endregion
    }
}
