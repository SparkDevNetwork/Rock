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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Communication.CommunicationQueue;
using Rock.Web.UI;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays the status of all communications that are queued to be sent.
    /// </summary>
    [DisplayName( "Communication Queue" )]
    [Category( "Communication" )]
    [Description( "Lists the status of all communications." )]
    [IconCssClass( "ti ti-messages" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [Rock.SystemGuid.EntityTypeGuid( "772F9FB1-55C4-4E65-9DB0-DD7233E61266" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "A430EF64-CCC6-4A29-827B-17CEB438FBBD" )]
    [Rock.SystemGuid.BlockTypeGuid( "694EB2F6-018D-4E99-A956-202B1FAF7FB9" )]
    [CustomizedGrid]
    public class CommunicationQueue : RockListBlockType<CommunicationQueue.QueuedCommunication>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
        }

        /*
            7/10/26 - MSE

            These preference keys intentionally reuse the legacy WebForms storage keys
            ("FutureCommunication", "PendingApproval", "CommunicationType") so that filter
            selections saved by the original WebForms block carry over to this Obsidian block.
        */
        private static class PreferenceKey
        {
            public const string FilterShowFutureCommunications = "FutureCommunication";
            public const string FilterShowPendingApproval = "PendingApproval";
            public const string FilterCommunicationTypes = "CommunicationType";
        }

        #endregion Keys

        #region Constants

        /*
            7/10/26 - MSE

            The SendCommunications job no longer exposes a DelayPeriod attribute, so the delay window
            is fixed at 30 minutes here to keep the queue's view consistent with its long-standing behavior.

            Reason: DelayPeriod attribute was removed from the job; preserve the historical 30-minute delay.
        */
        private const int DelayMinutes = 30;

        private const int DefaultExpirationDays = 3;

        private const string SendCommunicationsJobClass = "Rock.Jobs.SendCommunications";

        private const string ExpirationPeriodAttributeKey = "ExpirationPeriod";

        #endregion Constants

        #region Properties

        /// <summary>
        /// Gets whether communications scheduled for the future should be included.
        /// </summary>
        protected bool ShowFutureCommunications => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterShowFutureCommunications ).AsBoolean();

        /// <summary>
        /// Gets whether communications that are still pending approval should be included.
        /// </summary>
        protected bool ShowPendingApproval => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterShowPendingApproval ).AsBoolean();

        /// <summary>
        /// Gets the communication types that the list should be filtered to. An empty list means no filtering.
        /// </summary>
        protected List<int> FilterCommunicationTypes => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterCommunicationTypes )
            .SplitDelimitedValues()
            .Select( v => v.AsIntegerOrNull() )
            .Where( v => v.HasValue )
            .Select( v => v.Value )
            .ToList();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<CommunicationQueueOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = new CommunicationQueueOptionsBag();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.CommunicationId, "((Key))" ),
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<QueuedCommunication> GetListQueryable( RockContext rockContext )
        {
            var expirationDays = GetExpirationDays( rockContext );

            var communications = new CommunicationService( rockContext )
                .GetQueued( expirationDays, DelayMinutes, ShowFutureCommunications, ShowPendingApproval );

            var communicationTypes = FilterCommunicationTypes;
            if ( communicationTypes.Any() )
            {
                communications = communications.Where( c => communicationTypes.Contains( ( int ) c.CommunicationType ) );
            }

            // Project the sender and pending-recipient count alongside the entity so both are
            // resolved in a single query rather than lazy-loaded per row.
            return communications.Select( c => new QueuedCommunication
            {
                Communication = c,
                Sender = c.SenderPersonAlias.Person,
                PendingRecipientCount = c.Recipients.Count( r => r.Status == CommunicationRecipientStatus.Pending )
            } );
        }

        /// <inheritdoc/>
        protected override IQueryable<QueuedCommunication> GetOrderedListQueryable( IQueryable<QueuedCommunication> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( q =>
                q.Communication.FutureSendDateTime.HasValue && q.Communication.FutureSendDateTime > q.Communication.CreatedDateTime
                    ? q.Communication.FutureSendDateTime
                    : q.Communication.CreatedDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<QueuedCommunication> GetGridBuilder()
        {
            return new GridBuilder<QueuedCommunication>()
                .WithBlock( this )
                .AddTextField( "idKey", q => q.Communication.IdKey )
                .AddDateTimeField( "sendDateTime", q => GetSendDateTime( q.Communication ) )
                .AddPersonField( "sender", q => q.Sender )
                .AddTextField( "subject", q => q.Communication.Subject )
                .AddField( "status", q => q.Communication.Status )
                .AddField( "pendingRecipientCount", q => q.PendingRecipientCount )
                .AddField( "communicationType", q => q.Communication.CommunicationType );
        }

        /// <summary>
        /// Gets the effective send date/time for a communication, favoring a future send date when
        /// one is scheduled after the communication was created.
        /// </summary>
        /// <param name="communication">The communication to evaluate.</param>
        /// <returns>The date/time the communication is expected to send.</returns>
        private static DateTime? GetSendDateTime( Model.Communication communication )
        {
            return communication.FutureSendDateTime.HasValue && communication.FutureSendDateTime > communication.CreatedDateTime
                ? communication.FutureSendDateTime
                : communication.CreatedDateTime;
        }

        /// <summary>
        /// Gets the number of days a communication can remain queued before it is considered expired,
        /// read from the SendCommunications job's ExpirationPeriod attribute.
        /// </summary>
        /// <param name="rockContext">The database context to use for the lookup.</param>
        /// <returns>The configured expiration period in days, or the default when the job is not found.</returns>
        private int GetExpirationDays( RockContext rockContext )
        {
            var sendCommunicationsJob = new ServiceJobService( rockContext ).Queryable()
                .FirstOrDefault( j => j.Class == SendCommunicationsJobClass );

            if ( sendCommunicationsJob == null )
            {
                return DefaultExpirationDays;
            }

            sendCommunicationsJob.LoadAttributes( rockContext );

            return sendCommunicationsJob.GetAttributeValue( ExpirationPeriodAttributeKey ).AsIntegerOrNull() ?? DefaultExpirationDays;
        }

        #endregion Methods

        #region Helper Classes

        /// <summary>
        /// A queued communication along with its sender and pending-recipient count.
        /// </summary>
        public class QueuedCommunication
        {
            /// <summary>
            /// Gets or sets the communication.
            /// </summary>
            public Model.Communication Communication { get; set; }

            /// <summary>
            /// Gets or sets the person who sent the communication.
            /// </summary>
            public Person Sender { get; set; }

            /// <summary>
            /// Gets or sets the number of recipients still pending delivery.
            /// </summary>
            public int PendingRecipientCount { get; set; }
        }

        #endregion Helper Classes
    }
}
