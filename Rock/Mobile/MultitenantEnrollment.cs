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
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Rock.Configuration.ConnectedServices;
using Rock.Configuration.ConnectedServices.MobileApp;
using Rock.Configuration.ConnectedServices.MobileApp.DataTransferObjects;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Mobile
{
    /// <summary>
    /// Enrolls the church in the shared (multitenant) mobile application, updates its
    /// directory listing, and takes it out again. Called by the Connected Services card.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Every step before the first gateway call is local and changes nothing, so a
    ///         validation or reachability failure costs nothing and leaves nothing to undo.
    ///     </para>
    ///     <para>
    ///         Key rotation (card impl spec, section 9) is not here yet. Whether it ships as a
    ///         card button in v1 is the card spec's open question 7.
    ///     </para>
    /// </remarks>
    internal static class MultitenantEnrollment
    {
        #region Fields

        /// <summary>
        /// The longest a church name in the directory may be.
        /// </summary>
        internal const int MaximumNameLength = 100;

        /// <summary>
        /// The directory color format, <c>#RRGGBB</c>.
        /// </summary>
        private static readonly Regex BrandColorPattern = new Regex( "^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled );

        /// <summary>
        /// The client for the reachability test. Shared, because a new client per call
        /// would exhaust sockets on a busy server.
        /// </summary>
        private static readonly HttpClient ReachabilityHttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds( 10 )
        };

        #endregion Fields

        #region Messages

        /// <summary>
        /// Shown when the platform Site has not been built and deployed.
        /// </summary>
        internal const string BuildHintMessage = "Run Build App in the mobile app control panel to enable this.";

        /// <summary>
        /// Shown when the directory could not save the church's details.
        /// </summary>
        private const string ConfigWriteFailedMessage = "Spark could not save your church's details. Nothing was changed. Try again in a moment.";

        /// <summary>
        /// Shown when the column was flipped but the gateway could not be told.
        /// </summary>
        private const string DisableGatewayFailedMessage = "Rock has stopped the shared app for your church, but could not tell Spark. Click Disable again to finish.";

        #endregion Messages

        #region Methods

        /// <summary>
        /// Gets the platform Site and its bootstrap key, if the app has been built and
        /// deployed. The service only enables once Build App has created all three.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <returns>The platform Site and key, or <c>null</c> if the app is not built yet.</returns>
        internal static PlatformSiteDetails GetPlatformSite( RockContext rockContext )
        {
            var site = new SiteService( rockContext ).Get( SystemGuid.Site.PLATFORM_MOBILE_APPLICATION.AsGuid() );
            var settings = site?.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>();

            if ( settings?.ApiKeyId == null || !settings.LastDeploymentDate.HasValue )
            {
                return null;
            }

            var apiKey = new UserLoginService( rockContext ).GetSelect( settings.ApiKeyId.Value, u => u.ApiKey );

            // A blank key matches a request that sends no key at all, so it is never published.
            if ( apiKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new PlatformSiteDetails
            {
                Site = site,
                ApiKey = apiKey
            };
        }

        /// <summary>
        /// Checks the administrator's directory details with the directory's own rules, so
        /// a bad value never reaches the gateway.
        /// </summary>
        /// <param name="name">The church name.</param>
        /// <param name="brandColor">The directory color.</param>
        /// <returns>The error message, or <c>null</c> if both are valid.</returns>
        internal static string ValidateDirectoryDetails( string name, string brandColor )
        {
            var trimmedName = name?.Trim() ?? string.Empty;

            if ( trimmedName.Length == 0 )
            {
                return "The church name is required.";
            }

            if ( trimmedName.Length > MaximumNameLength )
            {
                return $"The church name must be {MaximumNameLength} characters or fewer.";
            }

            if ( brandColor == null || !BrandColorPattern.IsMatch( brandColor.Trim() ) )
            {
                return "The brand color must be a six digit hex color, such as #2E6BE6.";
            }

            return null;
        }

        /// <summary>
        /// Gets the API URL the shared app is handed for this church. It always comes from
        /// the Public Application Root global attribute and is never typed.
        /// </summary>
        /// <returns>The API URL.</returns>
        internal static string GetApiUrl()
        {
            return MobileHelper.BuildPublicApplicationRootUrl( "api" );
        }

        /// <summary>
        /// Calls this Rock over its public URL exactly the way a shared shell will. A
        /// <c>200</c> proves the URL is reachable from outside, points at this Rock, and the
        /// key and deployment are valid, all in one call.
        /// </summary>
        /// <param name="apiUrl">The API URL from <see cref="GetApiUrl"/>.</param>
        /// <param name="siteId">The platform Site's identifier.</param>
        /// <param name="apiKey">The platform Site's bootstrap key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><c>true</c> if this Rock answered with a launch packet.</returns>
        internal static async Task<bool> IsReachableAsync( string apiUrl, int siteId, string apiKey, CancellationToken cancellationToken )
        {
            /*
                10/2/2026 - CLAUDE

                The integer app id header is used, not the Guid, because the Guid branch
                refuses every request while IsMultitenantApp is false, which it always is
                before the first enable. The DeviceData header is required: GetLaunchPacket
                dereferences its DeviceType with no null check, so a request without it is
                a 500 that would be reported as an unreachable URL. No device identifier and
                no auth cookie are sent, so no PersonalDevice row is created and no login is
                touched.

                Reason: Prove the public URL works the way a shell will use it.
            */
            try
            {
                var request = new HttpRequestMessage( HttpMethod.Get, $"{apiUrl.TrimEnd( '/' )}/mobile/GetLaunchPacket" );
                request.Headers.Add( "X-Rock-App-Id", siteId.ToString() );
                request.Headers.Add( "X-Rock-Mobile-Api-Key", apiKey );
                request.Headers.Add( "X-Rock-DeviceData", "{\"DeviceType\":\"Phone\"}" );

                using ( var response = await ReachabilityHttpClient.SendAsync( request, cancellationToken ) )
                {
                    return response.IsSuccessStatusCode;
                }
            }
            catch ( Exception ex ) when ( ex is HttpRequestException || ex is TaskCanceledException || ex is UriFormatException || ex is InvalidOperationException )
            {
                // Any failure to connect, a timeout or a malformed URL all mean the same thing to
                // the administrator: the Public Application Root needs fixing.
                return false;
            }
        }

        /// <summary>
        /// Gets the absolute URL of the directory logo, or <c>null</c> when there is none.
        /// </summary>
        /// <param name="logoBinaryFileGuid">The logo's binary file Guid.</param>
        /// <returns>The logo URL or <c>null</c>.</returns>
        internal static string GetLogoUrl( Guid? logoBinaryFileGuid )
        {
            if ( !logoBinaryFileGuid.HasValue )
            {
                return null;
            }

            return FileUrlHelper.GetImageUrl( logoBinaryFileGuid.Value, new GetImageUrlOptions
            {
                PublicAppRoot = GlobalAttributesCache.Value( "PublicApplicationRoot" )
            } );
        }

        /// <summary>
        /// Builds the directory request for the church.
        /// </summary>
        /// <param name="name">The trimmed church name.</param>
        /// <param name="brandColor">The directory color, uppercase.</param>
        /// <param name="logoUrl">The logo URL, or <c>null</c>.</param>
        /// <param name="apiUrl">The API URL.</param>
        /// <param name="apiKey">The bootstrap key.</param>
        /// <param name="campusesJson">The campuses array as serialized by <see cref="PlatformMobileAppCampusPayload"/>.</param>
        /// <returns>The request.</returns>
        internal static MobileAppConfigurationRequest BuildConfigurationRequest( string name, string brandColor, string logoUrl, string apiUrl, string apiKey, string campusesJson )
        {
            using ( var campuses = JsonDocument.Parse( campusesJson ) )
            {
                return new MobileAppConfigurationRequest
                {
                    Name = name,
                    Branding = new MobileAppBranding
                    {
                        BrandColor = brandColor,
                        LogoUrl = logoUrl
                    },
                    Connection = new MobileAppConnection
                    {
                        ApiUrl = apiUrl,
                        ApiKey = apiKey
                    },

                    // Clone so the element outlives the document it was parsed from.
                    Campuses = campuses.RootElement.Clone()
                };
            }
        }

        /// <summary>
        /// Enables the church, or updates its listing when it is already enabled. Safe to
        /// repeat: the gateway enable is idempotent, the config write is an upsert, and every
        /// local write is an overwrite.
        /// </summary>
        /// <param name="options">The administrator's directory details.</param>
        /// <param name="gateway">The gateway to enroll through.</param>
        /// <param name="provider">The provider that stores the enrollment.</param>
        /// <param name="rockContext">The database context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result.</returns>
        internal static async Task<MultitenantEnrollmentResult> SaveAsync( MultitenantEnrollmentOptions options, IMobileAppGateway gateway, ConnectedServicesProvider provider, RockContext rockContext, CancellationToken cancellationToken )
        {
            var platformSite = GetPlatformSite( rockContext );

            if ( platformSite == null )
            {
                return MultitenantEnrollmentResult.Failure( BuildHintMessage );
            }

            var validationError = ValidateDirectoryDetails( options.Name, options.BrandColor );

            if ( validationError != null )
            {
                return MultitenantEnrollmentResult.Failure( validationError );
            }

            var name = options.Name.Trim();
            var brandColor = options.BrandColor.Trim().ToUpperInvariant();
            var apiUrl = GetApiUrl();

            if ( !await IsReachableAsync( apiUrl, platformSite.Site.Id, platformSite.ApiKey, cancellationToken ) )
            {
                return MultitenantEnrollmentResult.Failure( $"Rock could not reach itself at {apiUrl}. Check the Public Application Root global attribute, then try again." );
            }

            var campuses = PlatformMobileAppCampusPayload.GetCampuses();
            var campusesJson = PlatformMobileAppCampusPayload.Serialize( campuses );
            var request = BuildConfigurationRequest( name, brandColor, GetLogoUrl( options.LogoBinaryFileGuid ), apiUrl, platformSite.ApiKey, campusesJson );

            var stored = provider.GetConfiguration()?.MobileApp;
            var isAlreadyEnrolled = stored?.IsEnrolled == true;

            // Update is the config write alone. Enable and re-enable turn the service on first.
            if ( !isAlreadyEnrolled )
            {
                var enabledResult = await gateway.SetEnabledAsync( true, cancellationToken );

                if ( !enabledResult.IsSuccess )
                {
                    return MultitenantEnrollmentResult.Failure( enabledResult.ErrorMessage );
                }
            }

            var configResult = await gateway.SetConfigurationAsync( request, cancellationToken );
            var isChurchCodeReturned = configResult.IsSuccess && configResult.Data?.ChurchCode.IsNotNullOrWhiteSpace() == true;

            if ( !isChurchCodeReturned )
            {
                if ( isAlreadyEnrolled )
                {
                    return MultitenantEnrollmentResult.Failure( ConfigWriteFailedMessage );
                }

                // Do not leave the church enabled at the gateway with no details behind it.
                var compensateResult = await gateway.SetEnabledAsync( false, cancellationToken );

                return MultitenantEnrollmentResult.Failure( compensateResult.IsSuccess
                    ? ConfigWriteFailedMessage
                    : $"{ConfigWriteFailedMessage} Rock also could not undo the enable, so click Enable again to finish." );
            }

            MarkBinaryFilePermanent( options.LogoBinaryFileGuid, rockContext );

            provider.SaveMobileAppConfiguration( new ServiceConfiguration
            {
                IsEnrolled = true,
                ChurchCode = configResult.Data.ChurchCode,
                Link = configResult.Data.Link,
                Name = name,
                BrandColor = brandColor,
                LogoBinaryFileGuid = options.LogoBinaryFileGuid
            } );

            // The 5.7 toggle: push asks the directory for its credential, and the shared shell's Guid launch resolves.
            platformSite.Site.IsMultitenantApp = true;
            rockContext.SaveChanges();

            // The same array as was sent, so the daily campus job's first run finds nothing to do.
            platformSite.Site.SaveMetadataValue( MetadataKey.PlatformMobileAppCampusHash, PlatformMobileAppCampusPayload.ComputeHash( campuses ), rockContext );

            return MultitenantEnrollmentResult.Success();
        }

        /// <summary>
        /// Takes the church out of the shared app. The column flips first, so installed
        /// shared shells stop at their next launch even if the gateway cannot be reached; the
        /// key is never touched, so a branded build on the same Site keeps working.
        /// </summary>
        /// <param name="gateway">The gateway to disable through.</param>
        /// <param name="provider">The provider that stores the enrollment.</param>
        /// <param name="rockContext">The database context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result.</returns>
        internal static async Task<MultitenantEnrollmentResult> DisableAsync( IMobileAppGateway gateway, ConnectedServicesProvider provider, RockContext rockContext, CancellationToken cancellationToken )
        {
            var site = new SiteService( rockContext ).Get( SystemGuid.Site.PLATFORM_MOBILE_APPLICATION.AsGuid() );

            if ( site != null && site.IsMultitenantApp )
            {
                site.IsMultitenantApp = false;
                rockContext.SaveChanges();
            }

            var disableResult = await gateway.SetEnabledAsync( false, cancellationToken );

            if ( !disableResult.IsSuccess )
            {
                disableResult = await gateway.SetEnabledAsync( false, cancellationToken );
            }

            if ( !disableResult.IsSuccess )
            {
                return MultitenantEnrollmentResult.Failure( DisableGatewayFailedMessage );
            }

            // Keep the code, link and inputs: the posters come back to life on re-enable.
            var stored = provider.GetConfiguration()?.MobileApp ?? new ServiceConfiguration();
            stored.IsEnrolled = false;
            provider.SaveMobileAppConfiguration( stored );

            return MultitenantEnrollmentResult.Success();
        }

        /// <summary>
        /// Marks an uploaded logo permanent so the temporary file cleanup does not remove it.
        /// </summary>
        /// <param name="binaryFileGuid">The logo's binary file Guid, or <c>null</c>.</param>
        /// <param name="rockContext">The database context.</param>
        private static void MarkBinaryFilePermanent( Guid? binaryFileGuid, RockContext rockContext )
        {
            if ( !binaryFileGuid.HasValue )
            {
                return;
            }

            var binaryFile = new BinaryFileService( rockContext ).Get( binaryFileGuid.Value );

            if ( binaryFile != null && binaryFile.IsTemporary )
            {
                binaryFile.IsTemporary = false;
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// The platform Site together with the bootstrap key the directory hands out.
    /// </summary>
    internal class PlatformSiteDetails
    {
        /// <summary>
        /// Gets or sets the platform Site, tracked by the context it was loaded with.
        /// </summary>
        public Site Site { get; set; }

        /// <summary>
        /// Gets or sets the platform Site's bootstrap key.
        /// </summary>
        public string ApiKey { get; set; }
    }

    /// <summary>
    /// The administrator's directory details from the Connected Services card.
    /// </summary>
    internal class MultitenantEnrollmentOptions
    {
        /// <summary>
        /// Gets or sets the church name shown in the directory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the directory color, <c>#RRGGBB</c>.
        /// </summary>
        public string BrandColor { get; set; }

        /// <summary>
        /// Gets or sets the directory logo, or <c>null</c> for none.
        /// </summary>
        public Guid? LogoBinaryFileGuid { get; set; }
    }

    /// <summary>
    /// The outcome of an enrollment change.
    /// </summary>
    internal class MultitenantEnrollmentResult
    {
        /// <summary>
        /// Gets a value indicating whether the change was made.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Gets the message to show the administrator when it was not.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>The result.</returns>
        public static MultitenantEnrollmentResult Success()
        {
            return new MultitenantEnrollmentResult { IsSuccess = true };
        }

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        /// <param name="errorMessage">The message to show the administrator.</param>
        /// <returns>The result.</returns>
        public static MultitenantEnrollmentResult Failure( string errorMessage )
        {
            return new MultitenantEnrollmentResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
