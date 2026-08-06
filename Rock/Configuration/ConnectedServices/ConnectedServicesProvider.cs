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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Configuration.ConnectedServices.DataTransferObjects;
using Rock.Enums.Configuration;
using Rock.Store;
using Rock.SystemKey;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Configuration.ConnectedServices
{
    /// <summary>
    /// Provides functionality for managing connected services within the Rock
    /// application.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    ///     <para>
    ///         This can be made internal after the WebForms blocks are converted.
    ///     </para>
    /// </remarks>
    [RockInternal( "19.4", true )]
    public class ConnectedServicesProvider
    {
        #region Constants

        /// <summary>
        /// The service identifier for the Rock Intelligence service.
        /// </summary>
        private const string RockIntelligenceServiceId = "rock-iq";

        /// <summary>
        /// The cache key used to store the connected services configuration in
        /// the system settings.
        /// </summary>
        private const string ConfigurationCacheKey = "Rock.Configuration.ConnectedServices.ConnectedServicesProvider.Configuration";

        #endregion

        #region Fields

        /// <summary>
        /// The HTTP client used for making requests to the connected services API.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// The last cache flush count used to determine if the cached configuration
        /// is still valid.
        /// </summary>
        private int _lastCacheFlushCount = 0;

        /// <summary>
        /// Serializes configuration writes on this node so concurrent
        /// callers can't clobber each other's read-modify-write sequences.
        /// Cross-node writers still race at the SystemSettings row level.
        /// </summary>
        private readonly object _configLock = new object();

        /// <summary>
        /// The deployment environment of the Rock application, used for
        /// determining if connected services are available in the current
        /// environment.
        /// </summary>
        private readonly DeploymentEnvironment _deploymentEnvironment;

        /// <summary>
        /// The JSON serializer options used for serializing and deserializing.
        /// </summary>
        internal static readonly JsonSerializerOptions JsonOptions;

        #endregion

        #region Constructors

        static ConnectedServicesProvider()
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };

            jsonOptions.Converters.Add( new FlexibleReadEnumConverterFactory() );

            JsonOptions = jsonOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedServicesProvider"/> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public ConnectedServicesProvider( IInitializationSettings initializationSettings )
        {
            _deploymentEnvironment = initializationSettings.DeploymentEnvironment;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri( "https://apigateway.rockrms.com/" )
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedServicesProvider"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for making requests to the connected services API.</param>
        /// <param name="deploymentEnvironment">The deployment environment of the Rock application.</param>
        internal ConnectedServicesProvider( HttpClient httpClient, DeploymentEnvironment deploymentEnvironment )
        {
            _httpClient = httpClient ?? throw new ArgumentNullException( nameof( httpClient ) );
            _deploymentEnvironment = deploymentEnvironment;
        }

        #endregion

        #region Link Organization

        /// <summary>
        /// <para>
        /// Starts the process of linking an organization by initiating an
        /// authentication flow with the connected services API.
        /// </para>
        /// <para>
        /// For any network errors or unexpected responses, the caller should
        /// handle the <see cref="HttpRequestException"/> exception.
        /// </para>
        /// </summary>
        /// <param name="returnUrl">The URL to return to after the authentication process is complete.</param>
        /// <param name="context">The context for the authentication process.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The URL to redirect the user to for authentication.</returns>
        public async Task<string> StartLinkOrganizationAsync( string returnUrl, string context, CancellationToken cancellationToken )
        {
            if ( _deploymentEnvironment == DeploymentEnvironment.Demo )
            {
                throw new InvalidOperationException( "Connected services are not available in the demo environment." );
            }

            var request = new HttpRequestMessage( HttpMethod.Post, "/auth/v1/start" );
            var verifier = Guid.NewGuid().ToString() + Guid.NewGuid().ToString();
            var verifierHash = Base64UrlEncoder.Encode( Sha256( Encoding.UTF8.GetBytes( verifier ) ) );

            var data = new AuthStartRequest
            {
                ReturnUrl = returnUrl,
                VerifierHash = verifierHash,
                Context = context,
            };

            request.Content = new StringContent( Serialize( data ), Encoding.UTF8, "application/json" );

            var response = await _httpClient.SendAsync( request, cancellationToken );
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var authResponse = Deserialize<AuthStartResponse>( json );
            var authContext = new SparkAuthContext
            {
                Verifier = verifier,
                RequestId = authResponse.RequestId
            };

            SystemSettings.SetValue( SystemSetting.CONNECTED_SERVICES_AUTH, Serialize( authContext ) );

            return authResponse.AuthUrl;
        }

        /// <summary>
        /// <para>
        /// Completes the process of linking an organization by exchanging the
        /// request ID and verifier for an API token.
        /// </para>
        /// <para>
        /// For any network errors or unexpected responses, the caller should
        /// handle the <see cref="HttpRequestException"/> exception.
        /// </para>
        /// </summary>
        /// <param name="requestId">The request ID from the authentication process.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The context associated with the linked organization.</returns>
        public async Task<LinkOrganizationResult> CompleteLinkOrganizationAsync( string requestId, CancellationToken cancellationToken )
        {
            var authContextJson = SystemSettings.GetValue( SystemSetting.CONNECTED_SERVICES_AUTH );
            var authContext = DeserializeOrDefault<SparkAuthContext>( authContextJson );

            SystemSettings.SetValue( SystemSetting.CONNECTED_SERVICES_AUTH, string.Empty );

            if ( authContext?.RequestId != requestId )
            {
                throw new InvalidOperationException( "The request ID does not match the stored authentication context." );
            }

            var request = new HttpRequestMessage( HttpMethod.Post, "/auth/v1/token" );

            var data = new AuthTokenRequest
            {
                RequestId = requestId,
                Verifier = authContext.Verifier,
            };

            request.Content = new StringContent( Serialize( data ), Encoding.UTF8, "application/json" );

            var response = await _httpClient.SendAsync( request, cancellationToken );
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var authResponse = Deserialize<AuthTokenResponse>( json );

            SetAuthToken( authResponse.Token );

            // Set the legacy organization key for compatibility with the Rock Store.
            StoreService.SetOrganizationKey( authResponse.OrganizationGuid.ToString( "N" ) );

            return new LinkOrganizationResult
            {
                Context = authResponse.Context,
                OrganizationName = authResponse.OrganizationName,
            };
        }

        /// <summary>
        /// <para>
        /// Starts the process of upgrading legacy authorization by initiating an
        /// upgrade flow with the connected services API.
        /// </para>
        /// <para>
        /// For any network errors or unexpected responses, the caller should
        /// handle the <see cref="HttpRequestException"/> exception.
        /// </para>
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The URL to redirect the user to for authentication.</returns>
        public async Task<LinkOrganizationResult> UpgradeLegacyIdentifierAsync( CancellationToken cancellationToken )
        {
            if ( _deploymentEnvironment == DeploymentEnvironment.Demo )
            {
                throw new InvalidOperationException( "Connected services are not available in the demo environment." );
            }

            var legacyIdentifier = GetLegacyOrganizationIdentifier();

            if ( legacyIdentifier.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Legacy organization identifier is not available." );
            }

            var request = new HttpRequestMessage( HttpMethod.Post, "/auth/v1/upgrade" );
            var data = new AuthUpgradeRequest
            {
                RockGroupGuid = legacyIdentifier,
            };

            request.Content = new StringContent( Serialize( data ), Encoding.UTF8, "application/json" );

            var response = await _httpClient.SendAsync( request, cancellationToken );
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var authResponse = Deserialize<AuthTokenResponse>( json );

            SetAuthToken( authResponse.Token );

            // Set the legacy organization key for compatibility with the Rock Store.
            StoreService.SetOrganizationKey( authResponse.OrganizationGuid.ToString( "N" ) );

            return new LinkOrganizationResult
            {
                Context = authResponse.Context,
                OrganizationName = authResponse.OrganizationName,
            };
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Gets the connected services configuration from the system settings.
        /// </summary>
        /// <returns>The connected services configuration.</returns>
        internal ConnectedServicesConfiguration GetConfiguration()
        {
            if ( _deploymentEnvironment == DeploymentEnvironment.Demo )
            {
                return null;
            }

            var cache = RockCache.Get( ConfigurationCacheKey ) as ConnectedServicesConfiguration;
            var cacheFlushCount = SystemSettings.CacheFlushCount;

            if ( _lastCacheFlushCount != cacheFlushCount || cache == null )
            {
                cache = DeserializeOrDefault<ConnectedServicesConfiguration>( SystemSettings.GetValue( SystemSetting.CONNECTED_SERVICES_CONFIGURATION ) );

                _lastCacheFlushCount = cacheFlushCount;
                RockCache.Remove( ConfigurationCacheKey );
            }

            return cache;
        }

        /// <summary>
        /// Atomically reads the current configuration as a fresh
        /// deserialization from system settings, hands it to
        /// <paramref name="mutator"/>, and writes the result back only when
        /// the mutator returns <c>true</c>.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Callers on the same node are serialized on the internal
        ///         write lock so concurrent read-modify-write flows can't
        ///         clobber each other. Cross-node writers still race at the
        ///         SystemSettings row; there is no versioning or optimistic-
        ///         concurrency mechanism here.
        ///     </para>
        ///     <para>
        ///         The mutator always receives a non-null configuration. If
        ///         nothing has been persisted yet (e.g. during the initial
        ///         link flow) a fresh default instance is supplied. Nothing
        ///         else has a reference to the instance passed in, so the
        ///         mutator is free to mutate it in place.
        ///     </para>
        ///     <para>
        ///         Return <c>false</c> from the mutator to signal that no
        ///         state change is needed and the SystemSettings write
        ///         should be skipped. Nothing about the mutator's changes
        ///         (if any) survives in that case.
        ///     </para>
        /// </remarks>
        /// <param name="mutator">Callback that mutates the configuration and returns whether a write is required.</param>
        private void UpdateConfiguration( Func<ConnectedServicesConfiguration, bool> mutator )
        {
            if ( _deploymentEnvironment == DeploymentEnvironment.Demo )
            {
                throw new InvalidOperationException( "Connected services are not available in the demo environment." );
            }

            lock ( _configLock )
            {
                var json = SystemSettings.GetValue( SystemSetting.CONNECTED_SERVICES_CONFIGURATION );
                var configuration = DeserializeOrDefault<ConnectedServicesConfiguration>( json )
                    ?? new ConnectedServicesConfiguration();

                if ( !mutator( configuration ) )
                {
                    return;
                }

                SystemSettings.SetValue( SystemSetting.CONNECTED_SERVICES_CONFIGURATION, Serialize( configuration ) );
                RockCache.Remove( ConfigurationCacheKey );
            }
        }

        /// <summary>
        /// Gets the authentication token for the connected services API.
        /// </summary>
        /// <returns>The authentication token or an empty/null string if not configured.</returns>
        private string GetAuthToken()
        {
            return GetConfiguration()?.AuthToken;
        }

        /// <summary>
        /// Sets the authentication token for the connected services API.
        /// </summary>
        /// <param name="token">The authentication token to set.</param>
        private void SetAuthToken( string token )
        {
            UpdateConfiguration( configuration =>
            {
                configuration.AuthToken = token;
                return true;
            } );
        }

        /// <summary>
        /// Determines if the Rock instance is linked to an organization correctly.
        /// </summary>
        /// <returns><c>true</c> if the Rock instance is linked; otherwise, <c>false</c>.</returns>
        public bool IsOrganizationLinked()
        {
            return GetAuthToken().IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Determines if the Rock instance has a legacy link to an organization.
        /// </summary>
        /// <returns><c>true</c> if the Rock instance is linked; otherwise, <c>false</c>.</returns>
        public bool IsLegacyOrganizationLinked()
        {
            return StoreServiceBase.GetOrganizationKey().IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Gets the legacy organization identifier that can be used to attempt
        /// to upgrade the authentication method and also for legacy Rock Shop
        /// API requests.
        /// </summary>
        /// <returns>The legacy organization identifier.</returns>
        internal string GetLegacyOrganizationIdentifier()
        {
            return StoreServiceBase.GetOrganizationKey();
        }

        #endregion

        #region Bundle Manifest

        /// <summary>
        /// Requests the latest connected services manifest from the API and
        /// updates the configuration with the new manifest.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task UpdateManifestAsync( CancellationToken cancellationToken )
        {
            if ( _deploymentEnvironment == DeploymentEnvironment.Demo )
            {
                return;
            }

            var apiKey = GetAuthToken();

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Connected Services API Key is not configured." );
            }

            var request = new HttpRequestMessage( HttpMethod.Get, "api/v1/config/manifest" );
            request.Headers.Add( "X-Gateway-Api-Key", apiKey );

            var response = await _httpClient.SendAsync( request, cancellationToken );
            response.EnsureSuccessStatusCode();

            var manifestJson = await response.Content.ReadAsStringAsync();

            // Parse now to verify the manifest is valid JSON before we store it.
            var manifest = Deserialize<ConnectedServicesManifest>( manifestJson );

            SystemSettings.SetValue( SystemSetting.CONNECTED_SERVICES_MANIFEST, manifestJson );

            UpdateConfiguration( configuration =>
            {
                var rockIqEntry = manifest.Services.FirstOrDefault( se => se.ServiceId == "rock-iq" );

                configuration.RockIntelligence = RockIntelligence.ServiceConfiguration.FromEntry( rockIqEntry );

                return true;
            } );
        }

        /// <summary>
        /// Gets the connected services manifest from the configuration.
        /// </summary>
        /// <returns>The connected services manifest.</returns>
        internal ConnectedServicesManifest GetManifest()
        {
            var manifestJson = SystemSettings.GetValue( SystemSetting.CONNECTED_SERVICES_MANIFEST );

            return DeserializeOrDefault<ConnectedServicesManifest>( manifestJson );
        }

        #endregion

        #region Shared Services

        /// <summary>
        /// Gets the enabled status for a given service on the connected services API.
        /// </summary>
        /// <param name="serviceId">The identifier of the target service on the connected services API.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="ConfigurationResult"/> indicating the success or failure of the operation.</returns>
        private async Task<ConfigurationResult<bool>> GetEnabled( string serviceId, CancellationToken cancellationToken )
        {
            var apiKey = GetAuthToken();

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Connected Services API Key is not configured." );
            }

            try
            {
                var request = new HttpRequestMessage( HttpMethod.Get, $"svcs/v1/{serviceId}/enabled" );
                request.Headers.Add( "X-Gateway-Api-Key", apiKey );

                var response = await _httpClient.SendAsync( request, cancellationToken );
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = Deserialize<GetEnabledResponse>( responseJson );

                return new ConfigurationResult<bool>
                {
                    IsSuccess = true,
                    Data = responseData.Enabled,
                };
            }
            catch ( HttpRequestException ex )
            {
                return new ConfigurationResult<bool>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Failed to get the enabled status: {ex.InnerException?.Message ?? ex.Message}"
                };
            }
        }

        /// <summary>
        /// Gets the enabled status for a given service on the connected services API.
        /// </summary>
        /// <param name="serviceId">The identifier of the target service on the connected services API.</param>
        /// <param name="enabled">A value indicating whether the service should be enabled.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="ConfigurationResult"/> indicating the success or failure of the operation.</returns>
        private async Task<ConfigurationResult<SetEnabledResponse>> SetEnabled( string serviceId, bool enabled, CancellationToken cancellationToken )
        {
            var apiKey = GetAuthToken();

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Connected Services API Key is not configured." );
            }

            try
            {
                var request = new HttpRequestMessage( HttpMethod.Post, $"svcs/v1/{serviceId}/enabled" );
                request.Headers.Add( "X-Gateway-Api-Key", apiKey );

                var data = new SetEnabledRequest
                {
                    Enabled = enabled,
                };

                request.Content = new StringContent( Serialize( data ), Encoding.UTF8, "application/json" );

                var response = await _httpClient.SendAsync( request, cancellationToken );
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = Deserialize<SetEnabledResponse>( responseJson );

                return new ConfigurationResult<SetEnabledResponse>
                {
                    IsSuccess = true,
                    Data = responseData,
                };
            }
            catch ( HttpRequestException ex )
            {
                return new ConfigurationResult<SetEnabledResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Failed to set the enabled status: {ex.InnerException?.Message ?? ex.Message}"
                };
            }
        }

        /// <summary>
        /// Sets the selected bundle for a given service on the connected services API.
        /// </summary>
        /// <param name="serviceId">The identifier of the target service on the connected services API.</param>
        /// <param name="bundleId">The identifier of the bundle to set for the service.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="ConfigurationResult"/> indicating the success or failure of the operation.</returns>
        private async Task<ConfigurationResult<ServiceEntry>> SetBundle( string serviceId, Guid bundleId, CancellationToken cancellationToken )
        {
            var apiKey = GetAuthToken();

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Connected Services API Key is not configured." );
            }

            try
            {
                var request = new HttpRequestMessage( HttpMethod.Post, $"svcs/v1/{serviceId}/bundle" );
                request.Headers.Add( "X-Gateway-Api-Key", apiKey );

                var data = new SetBundleRequest
                {
                    BundleId = bundleId
                };

                request.Content = new StringContent( Serialize( data ), Encoding.UTF8, "application/json" );

                var response = await _httpClient.SendAsync( request, cancellationToken );
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = Deserialize<SetBundleResponse>( responseJson );

                return new ConfigurationResult<ServiceEntry>
                {
                    IsSuccess = true,
                    Data = responseData.ServiceEntry,
                };
            }
            catch ( HttpRequestException ex )
            {
                return new ConfigurationResult<ServiceEntry>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Failed to set current bundle: {ex.InnerException?.Message ?? ex.Message}"
                };
            }
        }

        /// <summary>
        /// Posts a new monthly spending limit to the given service.
        /// </summary>
        /// <param name="serviceId">The identifier of the target service on the connected services API.</param>
        /// <param name="amount">The new monthly spending limit, in the account's currency.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="ConfigurationResult"/> indicating the success or failure of the operation.</returns>
        private async Task<ConfigurationResult> SetMonthlySpendLimitAsync( string serviceId, decimal amount, CancellationToken cancellationToken )
        {
            var apiKey = GetAuthToken();

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Connected Services API Key is not configured." );
            }

            try
            {
                var request = new HttpRequestMessage( HttpMethod.Post, $"svcs/v1/{serviceId}/spending-limit" );
                request.Headers.Add( "X-Gateway-Api-Key", apiKey );

                var data = new SpendingLimitResponse
                {
                    SpendingLimit = amount
                };

                request.Content = new StringContent( Serialize( data ), Encoding.UTF8, "application/json" );

                var response = await _httpClient.SendAsync( request, cancellationToken );
                response.EnsureSuccessStatusCode();
            }
            catch ( HttpRequestException ex )
            {
                return new ConfigurationResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Failed to set monthly spend limit: {ex.InnerException?.Message ?? ex.Message}"
                };
            }

            return new ConfigurationResult
            {
                IsSuccess = true
            };
        }

        /// <summary>
        /// Gets the monthly spending limit for the given service.
        /// </summary>
        /// <param name="serviceId">The identifier of the target service on the connected services API.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The monthly spending limit, in the account's currency.</returns>
        private async Task<ConfigurationResult<decimal?>> GetMonthlySpendLimitAsync( string serviceId, CancellationToken cancellationToken )
        {
            var apiKey = GetAuthToken();

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Connected Services API Key is not configured." );
            }

            try
            {
                var request = new HttpRequestMessage( HttpMethod.Get, $"svcs/v1/{serviceId}/spending-limit" );
                request.Headers.Add( "X-Gateway-Api-Key", apiKey );

                var response = await _httpClient.SendAsync( request, cancellationToken );
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseData = Deserialize<SpendingLimitResponse>( responseJson );

                return new ConfigurationResult<decimal?>
                {
                    IsSuccess = true,
                    Data = responseData.SpendingLimit,
                };
            }
            catch ( HttpRequestException ex )
            {
                return new ConfigurationResult<decimal?>
                {
                    IsSuccess = false,
                    ErrorMessage = $"Failed to set monthly spend limit: {ex.InnerException?.Message ?? ex.Message}"
                };
            }
        }

        /// <summary>
        /// Applies a one-time boost purchase to the given service by
        /// charging the credit card on file for the specified amount.
        /// </summary>
        /// <remarks>
        /// The outcome is first read from the response body when the server
        /// returns a structured payload; otherwise it is inferred from the
        /// HTTP status code. Errors that prevent us from determining a
        /// definitive outcome (network failure, malformed body, unexpected
        /// status) are surfaced as <see cref="OneTimeBoostStatus.Error"/>.
        /// </remarks>
        /// <param name="serviceId">The identifier of the target service on the connected services API.</param>
        /// <param name="amount">The boost amount to charge.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="OneTimeBoostResult"/> describing the outcome of the boost attempt.</returns>
        private async Task<OneTimeBoostResult> ApplyOneTimeBoostAsync( string serviceId, decimal amount, CancellationToken cancellationToken )
        {
            var apiKey = GetAuthToken();

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Connected Services API Key is not configured." );
            }

            HttpResponseMessage response;
            string responseJson;

            try
            {
                var request = new HttpRequestMessage( HttpMethod.Post, $"svcs/v1/{serviceId}/one-time-payment" );
                request.Headers.Add( "X-Gateway-Api-Key", apiKey );

                var data = new
                {
                    Amount = amount
                };

                request.Content = new StringContent( Serialize( data ), Encoding.UTF8, "application/json" );

                response = await _httpClient.SendAsync( request, cancellationToken );
                responseJson = await response.Content.ReadAsStringAsync();
            }
            catch ( HttpRequestException ex )
            {
                return new OneTimeBoostResult
                {
                    Status = OneTimeBoostStatus.Error,
                    Message = $"Failed to apply one-time boost: {ex.InnerException?.Message ?? ex.Message}"
                };
            }

            // Prefer the structured server response so future server-side
            // status changes don't require an update here. Fall back to the
            // HTTP status code when the body is missing or malformed.
            var body = DeserializeOrDefault<OneTimePaymentResponse>( responseJson );

            if ( body != null )
            {
                return new OneTimeBoostResult
                {
                    Status = MapPaymentStatus( body.Status ),
                    Message = body.Message,
                    Amount = body.Amount
                };
            }

            return InferOneTimeBoostResultFromHttpStatus( response.StatusCode );
        }

        /// <summary>
        /// Maps the wire-DTO <see cref="OneTimePaymentStatus"/> to the
        /// Rock-owned <see cref="OneTimeBoostStatus"/>. Any unrecognized
        /// wire value maps to <see cref="OneTimeBoostStatus.Error"/> so a
        /// server-side enum addition can't silently look like success.
        /// </summary>
        /// <param name="status">The wire-DTO status value to map.</param>
        /// <returns>The equivalent Rock-owned status value.</returns>
        [ExcludeFromCodeCoverage]
        private static OneTimeBoostStatus MapPaymentStatus( OneTimePaymentStatus status )
        {
            switch ( status )
            {
                case OneTimePaymentStatus.Complete:
                    return OneTimeBoostStatus.Complete;

                case OneTimePaymentStatus.Pending:
                    return OneTimeBoostStatus.Pending;

                case OneTimePaymentStatus.Declined:
                    return OneTimeBoostStatus.Declined;

                default:
                    return OneTimeBoostStatus.Error;
            }
        }

        /// <summary>
        /// Fallback classification used when the server did not return a
        /// deserializable response body.
        /// </summary>
        /// <param name="statusCode">The HTTP status code from the one-time payment response.</param>
        /// <returns>An <see cref="OneTimeBoostResult"/> whose status is inferred from the HTTP status code.</returns>
        private static OneTimeBoostResult InferOneTimeBoostResultFromHttpStatus( HttpStatusCode statusCode )
        {
            switch ( statusCode )
            {
                case HttpStatusCode.OK:
                    return new OneTimeBoostResult
                    {
                        Status = OneTimeBoostStatus.Complete
                    };

                case HttpStatusCode.Accepted:
                    return new OneTimeBoostResult
                    {
                        Status = OneTimeBoostStatus.Pending,
                        Message = "The credit card has been charged but we are still working on applying the credit. If it doesn't show up within 24 hours please contact support."
                    };

                case HttpStatusCode.BadRequest:
                case HttpStatusCode.PaymentRequired:
                    return new OneTimeBoostResult
                    {
                        Status = OneTimeBoostStatus.Declined,
                        Message = "The one-time boost payment was declined."
                    };

                default:
                    return new OneTimeBoostResult
                    {
                        Status = OneTimeBoostStatus.Error,
                        Message = $"Failed to apply one-time boost: Service responded with status code {( int ) statusCode}."
                    };
            }
        }

        /// <summary>
        /// Sets the selected bundle for a given service on the connected services API.
        /// </summary>
        /// <param name="serviceId">The identifier of the target service on the connected services API.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An object that contains the available service bundles and the currently selected bundle.</returns>
        private async Task<GetServiceBundlesResponse> GetServiceBundlesAsync( string serviceId, CancellationToken cancellationToken )
        {
            var apiKey = GetAuthToken();

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Connected Services API Key is not configured." );
            }

            var request = new HttpRequestMessage( HttpMethod.Get, $"svcs/v1/{serviceId}/bundles" );
            request.Headers.Add( "X-Gateway-Api-Key", apiKey );

            var response = await _httpClient.SendAsync( request, cancellationToken );
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseData = Deserialize<GetServiceBundlesResponse>( responseJson );

            return responseData;
        }

        #endregion

        #region Rock Intelligence

        /// <summary>
        /// Gets the current usage information for Rock Intelligence from
        /// the connected services API.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The current usage information for Rock Intelligence.</returns>
        internal async Task<RockIntelligence.Usage> GetRockIntelligenceUsageAsync( CancellationToken cancellationToken )
        {
            var apiKey = GetAuthToken();

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Connected Services API Key is not configured." );
            }

            var request = new HttpRequestMessage( HttpMethod.Get, $"svcs/v1/{RockIntelligenceServiceId}/usage" );
            request.Headers.Add( "X-Gateway-Api-Key", apiKey );

            var response = await _httpClient.SendAsync( request, cancellationToken );
            response.EnsureSuccessStatusCode();

            var usage = new RockIntelligence.Usage();
            var json = await response.Content.ReadAsStringAsync();
            var usageResponse = Deserialize<RockIntelligence.DataTransferObjects.UsageResponse>( json );

            usage.CurrentMonthSpending = usageResponse.MonthlyUsage ?? 0;
            usage.BalanceRemaining = usageResponse.Balance ?? 0;

            request = new HttpRequestMessage( HttpMethod.Get, $"svcs/v1/{RockIntelligenceServiceId}/spending-limit" );
            request.Headers.Add( "X-Gateway-Api-Key", apiKey );

            response = await _httpClient.SendAsync( request, cancellationToken );
            response.EnsureSuccessStatusCode();
            json = await response.Content.ReadAsStringAsync();

            var spendingLimitResponse = Deserialize<SpendingLimitResponse>( json );

            usage.MonthlySpendLimit = spendingLimitResponse.SpendingLimit ?? 0;

            return usage;
        }

        /// <summary>
        /// Sets whether the Rock Intelligence service is enabled or disabled.
        /// </summary>
        /// <param name="enabled">A value indicating whether the Rock Intelligence service should be enabled.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="ConfigurationResult"/> indicating the success or failure of the operation.</returns>
        internal async Task<ConfigurationResult<SetEnabledResponse>> SetRockIntelligenceEnabledAsync( bool enabled, CancellationToken cancellationToken )
        {
            var result = await SetEnabled( RockIntelligenceServiceId, enabled, cancellationToken );

            if ( result.IsSuccess )
            {
                UpdateConfiguration( configuration =>
                {
                    configuration.RockIntelligence = RockIntelligence.ServiceConfiguration.FromEntry( result.Data?.ServiceEntry );

                    return true;
                } );
            }

            return result;
        }

        /// <summary>
        /// Selects the Rock Intelligence bundle to use.
        /// </summary>
        /// <param name="bundleIdentifier">The identifier of the bundle to select.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="ConfigurationResult"/> indicating the success or failure of the operation.</returns>
        internal async Task<ConfigurationResult> SetRockIntelligenceBundleAsync( Guid bundleIdentifier, CancellationToken cancellationToken )
        {
            var result = await SetBundle( RockIntelligenceServiceId, bundleIdentifier, cancellationToken );

            if ( result.IsSuccess )
            {
                UpdateConfiguration( configuration =>
                {
                    configuration.RockIntelligence = RockIntelligence.ServiceConfiguration.FromEntry( result.Data );

                    return true;
                } );
            }

            return result;
        }

        /// <summary>
        /// Sets the monthly spending limit for the Rock Intelligence service.
        /// </summary>
        /// <param name="amount">The new monthly spending limit, in the account's currency.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="ConfigurationResult"/> indicating the success or failure of the operation.</returns>
        internal Task<ConfigurationResult> SetRockIntelligenceMonthlySpendLimitAsync( decimal amount, CancellationToken cancellationToken )
        {
            return SetMonthlySpendLimitAsync( RockIntelligenceServiceId, amount, cancellationToken );
        }

        /// <summary>
        /// Gets the monthly spending limit for the Rock Intelligence service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The monthly spending limit, in the account's currency.</returns>
        internal Task<ConfigurationResult<decimal?>> GetRockIntelligenceMonthlySpendLimitAsync( CancellationToken cancellationToken )
        {
            return GetMonthlySpendLimitAsync( RockIntelligenceServiceId, cancellationToken );
        }

        /// <summary>
        /// Applies a one-time boost to the Rock Intelligence balance by
        /// charging the credit card on file for the specified amount.
        /// </summary>
        /// <param name="amount">The boost amount to charge.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="OneTimeBoostResult"/> describing the outcome of the boost attempt.</returns>
        internal Task<OneTimeBoostResult> ApplyRockIntelligenceOneTimeBoostAsync( decimal amount, CancellationToken cancellationToken )
        {
            return ApplyOneTimeBoostAsync( RockIntelligenceServiceId, amount, cancellationToken );
        }

        /// <summary>
        /// Gets the Rock Intelligence bundles.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An object that contains the available Rock Intelligence bundles and the currently selected bundle.</returns>
        internal Task<GetServiceBundlesResponse> GetRockIntelligenceBundlesAsync( CancellationToken cancellationToken )
        {
            return GetServiceBundlesAsync( RockIntelligenceServiceId, cancellationToken );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the credit card summary information from the connected services API.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The credit card summary information.</returns>
        internal async Task<CreditCardSummary> GetCreditCardSummaryAsync( CancellationToken cancellationToken )
        {
            var apiKey = GetAuthToken();

            if ( apiKey.IsNullOrWhiteSpace() )
            {
                throw new InvalidOperationException( "Connected Services API Key is not configured." );
            }

            var request = new HttpRequestMessage( HttpMethod.Get, "api/v1/billing/credit-card-summary" );
            request.Headers.Add( "X-Gateway-Api-Key", apiKey );

            var response = await _httpClient.SendAsync( request, cancellationToken );
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return Deserialize<CreditCardSummary>( json );
        }

        /// <summary>
        /// Computes the SHA-256 hash of the given byte array.
        /// </summary>
        /// <param name="source">The byte array to hash.</param>
        /// <returns>The SHA-256 hash of the input byte array.</returns>
        private static byte[] Sha256( byte[] source )
        {
#if NET9_0_OR_GREATER
            using ( var crypt = SHA256.Create() )
#else
            using ( var crypt = new SHA256Managed() )
#endif
            {
                return crypt.ComputeHash( source );
            }
        }

        private static string Serialize<T>( T obj )
        {
            return JsonSerializer.Serialize( obj, JsonOptions );
        }

        private static T Deserialize<T>( string json )
        {
            return JsonSerializer.Deserialize<T>( json, JsonOptions );
        }

        private static T DeserializeOrDefault<T>( string json )
        {
            try
            {
                return JsonSerializer.Deserialize<T>( json, JsonOptions );
            }
            catch
            {
                return default;
            }
        }

        #endregion
    }
}
