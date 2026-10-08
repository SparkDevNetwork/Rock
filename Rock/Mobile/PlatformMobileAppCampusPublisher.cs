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
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Rock.Configuration.ConnectedServices;
using Rock.Configuration.ConnectedServices.DataTransferObjects;
using Rock.Configuration.ConnectedServices.MobileApp;
using Rock.Configuration.ConnectedServices.MobileApp.DataTransferObjects;
using Rock.Data;
using Rock.Model;
using Rock.SystemKey;

namespace Rock.Mobile
{
    /// <summary>
    /// Keeps the church's campuses in the church directory current for the shared mobile
    /// application. Called by the daily campus job.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Enrollment sends the first copy of the campuses and stamps their hash on the
    ///         platform Site. After that this class only sends them again when the hash of what
    ///         would be sent differs from the stamp, so a quiet run makes no call at all.
    ///     </para>
    ///     <para>
    ///         The stamp is only written after the directory accepts the campuses. A failed
    ///         send therefore leaves the old stamp in place and the next run tries again; the
    ///         send is a full replace, so a missed run heals itself.
    ///     </para>
    /// </remarks>
    internal static class PlatformMobileAppCampusPublisher
    {
        /// <summary>
        /// Sends the church's campuses to the directory if they have changed since they were
        /// last sent.
        /// </summary>
        /// <param name="gateway">The gateway to send through.</param>
        /// <param name="provider">The provider that holds the church's enrollment.</param>
        /// <param name="rockContext">The database context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result, with a message for the job's status.</returns>
        internal static async Task<PlatformMobileAppCampusPublishResult> PublishIfChangedAsync( IMobileAppGateway gateway, ConnectedServicesProvider provider, RockContext rockContext, CancellationToken cancellationToken )
        {
            var isEnrolled = provider.GetConfiguration()?.MobileApp?.IsEnrolled == true;
            var site = new SiteService( rockContext ).Get( SystemGuid.Site.PLATFORM_MOBILE_APPLICATION.AsGuid() );

            /*
                10/8/2026 - CLAUDE

                Both checks are needed. Disable turns IsMultitenantApp off before it calls the
                gateway, and only clears IsEnrolled once the gateway agrees. A disable whose
                gateway call failed therefore leaves IsEnrolled true with the column off. The
                church is on its way out, and the directory may already answer
                409 church_inactive, so nothing is sent in that state.

                Reason: A church that is leaving the shared app never publishes campuses.
            */
            if ( !isEnrolled || site == null || !site.IsMultitenantApp )
            {
                return PlatformMobileAppCampusPublishResult.Skipped( "The church is not enrolled in the shared mobile application, so there is nothing to publish." );
            }

            var campuses = PlatformMobileAppCampusPayload.GetCampuses();
            var hash = PlatformMobileAppCampusPayload.ComputeHash( campuses );
            var stampedHash = site.GetMetadataValue( MetadataKey.PlatformMobileAppCampusHash, rockContext );

            if ( !IsPublishNeeded( stampedHash, hash ) )
            {
                return PlatformMobileAppCampusPublishResult.Skipped( "The campuses have not changed since they were last published." );
            }

            ConfigurationResult<MobileAppCampusesResponse> result;

            try
            {
                result = await gateway.SetCampusesAsync( BuildRequest( campuses ), cancellationToken );
            }
            catch ( InvalidOperationException ex )
            {
                // The provider throws when Connected Services has no token or this is the demo environment.
                return PlatformMobileAppCampusPublishResult.Failure( ex.Message );
            }

            if ( !result.IsSuccess )
            {
                return PlatformMobileAppCampusPublishResult.Failure( result.ErrorMessage.IsNotNullOrWhiteSpace()
                    ? result.ErrorMessage
                    : "The church directory did not accept the campuses." );
            }

            site.SaveMetadataValue( MetadataKey.PlatformMobileAppCampusHash, hash, rockContext );

            return PlatformMobileAppCampusPublishResult.Published( GetPublishedMessage( campuses, result.Data ) );
        }

        /// <summary>
        /// Decides whether the campuses need sending: when nothing has been stamped yet, or
        /// when what would be sent now hashes differently from what was sent last.
        /// </summary>
        /// <param name="stampedHash">The hash stamped on the platform Site, or <c>null</c>.</param>
        /// <param name="currentHash">The hash of the campuses as they would be sent now.</param>
        /// <returns><c>true</c> if the campuses should be sent.</returns>
        internal static bool IsPublishNeeded( string stampedHash, string currentHash )
        {
            if ( stampedHash.IsNullOrWhiteSpace() )
            {
                return true;
            }

            return !string.Equals( stampedHash, currentHash, StringComparison.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Builds the request from the campuses, carrying exactly the array that was hashed.
        /// </summary>
        /// <param name="campuses">The campus entries.</param>
        /// <returns>The request.</returns>
        internal static MobileAppCampusesRequest BuildRequest( List<PlatformMobileAppCampus> campuses )
        {
            using ( var document = JsonDocument.Parse( PlatformMobileAppCampusPayload.Serialize( campuses ) ) )
            {
                return new MobileAppCampusesRequest
                {
                    // Clone so the element outlives the document it was parsed from.
                    Campuses = document.RootElement.Clone()
                };
            }
        }

        /// <summary>
        /// Describes a successful send. Uses the directory's own counts when it returns them,
        /// otherwise the counts of what was sent.
        /// </summary>
        /// <param name="campuses">The campus entries that were sent.</param>
        /// <param name="response">The directory's response, or <c>null</c>.</param>
        /// <returns>The message.</returns>
        internal static string GetPublishedMessage( List<PlatformMobileAppCampus> campuses, MobileAppCampusesResponse response )
        {
            var campusCount = response?.CampusCount ?? campuses.Count;
            var geocodedCount = response?.GeocodedCount ?? campuses.Count( c => c.Latitude.HasValue );
            var campusWord = campusCount == 1 ? "campus" : "campuses";

            return $"Published {campusCount} {campusWord} to the church directory, {geocodedCount} with coordinates.";
        }
    }

    /// <summary>
    /// The outcome of one campus job run.
    /// </summary>
    internal class PlatformMobileAppCampusPublishResult
    {
        /// <summary>
        /// <c>false</c> only when the campuses needed sending and the send failed. A run that
        /// had nothing to send is a success.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// <c>true</c> when the campuses were sent and the hash was stamped.
        /// </summary>
        public bool IsPublished { get; private set; }

        /// <summary>
        /// What happened, for the job's status message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// A run that had nothing to send.
        /// </summary>
        /// <param name="message">Why nothing was sent.</param>
        /// <returns>The result.</returns>
        public static PlatformMobileAppCampusPublishResult Skipped( string message )
        {
            return new PlatformMobileAppCampusPublishResult { IsSuccess = true, Message = message };
        }

        /// <summary>
        /// A run that sent the campuses.
        /// </summary>
        /// <param name="message">What was sent.</param>
        /// <returns>The result.</returns>
        public static PlatformMobileAppCampusPublishResult Published( string message )
        {
            return new PlatformMobileAppCampusPublishResult { IsSuccess = true, IsPublished = true, Message = message };
        }

        /// <summary>
        /// A run whose send failed. The stamp is left alone so the next run tries again.
        /// </summary>
        /// <param name="message">Why the send failed.</param>
        /// <returns>The result.</returns>
        public static PlatformMobileAppCampusPublishResult Failure( string message )
        {
            return new PlatformMobileAppCampusPublishResult { Message = message };
        }
    }
}
