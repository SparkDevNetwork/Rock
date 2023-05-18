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
using System.Linq.Expressions;
using System.Threading.Tasks;

using Rock.Communication;
using Rock.Data;
using Rock.Web.Cache;

using Z.EntityFramework.Plus;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.NotificationMessage"/> entity objects.
    /// </summary>
    public partial class NotificationMessageService : Service<NotificationMessage>
    {
        /// <summary>
        /// Gets the active messages for the person specified.
        /// A message is considered active as long as it is within the proper
        /// date range. Unread messages are still considered active and will be
        /// included.
        /// </summary>
        /// <param name="personId">The person identifier to use when filtering messages.</param>
        /// <param name="site">The site the messages will be displayed on.</param>
        /// <returns>A <see cref="IQueryable"/> of <see cref="NotificationMessage"/> objects that match the parameters.</returns>
        public IQueryable<NotificationMessage> GetActiveMessagesForPerson( int personId, SiteCache site )
        {
            if ( site == null )
            {
                throw new ArgumentNullException( nameof( site ) );
            }

            return GetActiveMessagesForPerson( nm => nm.PersonAlias.PersonId == personId, site );
        }

        /// <summary>
        /// Gets the active messages for the person specified.
        /// A message is considered active as long as it is within the proper
        /// date range. Unread messages are still considered active and will be
        /// included.
        /// </summary>
        /// <param name="personGuid">The person unique identifier to use when filtering messages.</param>
        /// <param name="site">The site the messages will be displayed on.</param>
        /// <returns>A <see cref="IQueryable"/> of <see cref="NotificationMessage"/> objects that match the parameters.</returns>
        public IQueryable<NotificationMessage> GetActiveMessagesForPerson( Guid personGuid, SiteCache site )
        {
            if ( site == null )
            {
                throw new ArgumentNullException( nameof( site ) );
            }

            return GetActiveMessagesForPerson( nm => nm.PersonAlias.Person.Guid == personGuid, site );
        }

        /// <summary>
        /// Gets the unread and active messages for the person specified.
        /// A message is considered active as long as it is within the proper
        /// date range.
        /// </summary>
        /// <param name="personId">The person identifier to use when filtering messages.</param>
        /// <param name="site">The site the messages will be displayed on.</param>
        /// <returns>A <see cref="IQueryable"/> of <see cref="NotificationMessage"/> objects that match the parameters.</returns>
        public IQueryable<NotificationMessage> GetUnreadMessagesForPerson( int personId, SiteCache site )
        {
            if ( site == null )
            {
                throw new ArgumentNullException( nameof( site ) );
            }

            return GetActiveMessagesForPerson( nm => nm.PersonAlias.PersonId == personId, site )
                .Where( nm => !nm.IsRead );
        }

        /// <summary>
        /// Gets the unread and active messages for the person specified.
        /// A message is considered active as long as it is within the proper
        /// date range.
        /// </summary>
        /// <param name="personGuid">The person unique identifier to use when filtering messages.</param>
        /// <param name="site">The site the messages will be displayed on.</param>
        /// <returns>A <see cref="IQueryable"/> of <see cref="NotificationMessage"/> objects that match the parameters.</returns>
        public IQueryable<NotificationMessage> GetUnreadMessagesForPerson( Guid personGuid, SiteCache site )
        {
            if ( site == null )
            {
                throw new ArgumentNullException( nameof( site ) );
            }

            return GetActiveMessagesForPerson( nm => nm.PersonAlias.Person.Guid == personGuid, site )
                .Where( nm => !nm.IsRead );
        }

        /// <summary>
        /// Gets the active messages for the person specified by the predicate.
        /// A message is considered active as long as it is within the proper
        /// date range. Unread messages are still considered active and will be
        /// included.
        /// </summary>
        /// <param name="personPredicate">The person predicate to use when filtering messages.</param>
        /// <param name="site">The site the messages will be displayed on.</param>
        /// <returns>A <see cref="IQueryable"/> of <see cref="NotificationMessage"/> objects that match the parameters.</returns>
        private IQueryable<NotificationMessage> GetActiveMessagesForPerson( Expression<Func<NotificationMessage, bool>> personPredicate, SiteCache site )
        {
            var now = RockDateTime.Now;

            return GetMessagesForPerson( personPredicate, site )
                .Where( nm => nm.MessageDateTime <= now && nm.ExpireDateTime > now );
        }

        /// <summary>
        /// Gets the messages for the person specified by the predicate.
        /// </summary>
        /// <param name="personPredicate">The person predicate to use when filtering messages.</param>
        /// <param name="site">The site the messages will be displayed on.</param>
        /// <returns>A <see cref="IQueryable"/> of <see cref="NotificationMessage"/> objects that match the parameters.</returns>
        private IQueryable<NotificationMessage> GetMessagesForPerson( Expression<Func<NotificationMessage, bool>> personPredicate, SiteCache site )
        {
            var now = RockDateTime.Now;

            // Filter to messages for this person.
            var qry = Queryable().Where( personPredicate );

            if ( site.SiteType == SiteType.Web )
            {
                // Filter to messages that support web and are either not
                // limited to a single site or match this site.
                qry = qry.Where( nm => nm.NotificationMessageType.IsWebSupported
                    && ( !nm.NotificationMessageType.RelatedWebSiteId.HasValue || nm.NotificationMessageType.RelatedWebSiteId == site.Id ) );
            }
            else if ( site.SiteType == SiteType.Mobile )
            {
                // Filter to messages that support mobile and are either not
                // limited to a single site or match this site.
                qry = qry.Where( nm => nm.NotificationMessageType.IsMobileApplicationSupported
                    && ( !nm.NotificationMessageType.RelatedMobileApplicationSiteId.HasValue || nm.NotificationMessageType.RelatedMobileApplicationSiteId == site.Id ) );
            }
            else if ( site.SiteType == SiteType.Tv )
            {
                // Filter to messages that support TV and are either not
                // limited to a single site or match this site.
                qry = qry.Where( nm => nm.NotificationMessageType.IsTvApplicationSupported
                    && ( !nm.NotificationMessageType.RelatedTvApplicationSiteId.HasValue || nm.NotificationMessageType.RelatedTvApplicationSiteId == site.Id ) );
            }
            else
            {
                throw new Exception( $"The site type '{site.SiteType}' is not supported by notification messages." );
            }

            return qry;
        }

        /// <summary>
        /// Send badge count updates for the set of person identifiers.
        /// </summary>
        /// <param name="personIds">The person identifiers that need a badge count update.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        internal static async Task SendBadgeCountUpdatesAsync( List<int> personIds )
        {
            using ( var rockContext = new RockContext() )
            {
                var messageService = new NotificationMessageService( rockContext );
                List<(int PersonId, int SiteId, List<string> DeviceRegistrationIds)> personSites;

                // Now that we have the person Ids, get all the personal devcies
                // that need to be updated. Right now we only care about mobile.
                personSites = new PersonalDeviceService( rockContext ).Queryable()
                    .Where( pd => pd.NotificationsEnabled
                        && !string.IsNullOrEmpty( pd.DeviceRegistrationId )
                        && pd.SiteId.HasValue
                        && pd.Site.SiteType == SiteType.Mobile
                        && pd.PersonAliasId.HasValue
                        && personIds.Contains( pd.PersonAlias.PersonId ) )
                    .Select( pd => new
                    {
                        pd.PersonAlias.PersonId,
                        SiteId = pd.SiteId.Value,
                        pd.DeviceRegistrationId
                    } )
                    .GroupBy( pd => new
                    {
                        pd.PersonId,
                        pd.SiteId
                    } )
                    .ToList()
                    .Select( g => (g.Key.PersonId, g.Key.SiteId, g.Select( item => item.DeviceRegistrationId ).ToList()) )
                    .ToList();

                var badgeCounts = new List<(int Count, List<string> DeviceRegistrationIds)>();

                // Loop through all our person site information and run queries
                // to get all the counts for each pair. We are using the ZZZ
                // framework Future queries to batch the queries into groups
                // of 100 queries per round-trip to the database. So if we have
                // 1,000 pairs, we only send 10 queries instead of 1,000.
                while ( personSites.Any() )
                {
                    // SQL has a maximum limit on the number of parameters a query
                    // can take. Since this method multiplies those queries, we use
                    // a relatively small batch size.
                    var items = personSites.Take( 100 ).ToList();
                    personSites = personSites.Skip( 100 ).ToList();

                    var counts = items
                        .Select( ps => new
                        {
                            Count = messageService.GetUnreadMessagesForPerson( ps.PersonId, SiteCache.Get( ps.SiteId ) )
                                .DeferredSum( nm => ( int? ) nm.Count ).FutureValue(),
                            ps.DeviceRegistrationIds
                        } )
                        .ToList()
                        .Select( a => (a.Count.Value ?? 0, a.DeviceRegistrationIds) )
                        .ToList();

                    badgeCounts.AddRange( counts );
                }

                // Group all the device identifiers by badge count. There is
                // nothing per-user about the push notification. So we can send
                // the same "badge count" notification to all devices with
                // matching badge numbers.
                var badgeGroups = badgeCounts.GroupBy( bc => bc.Count );
                var tasks = new List<Task>();

                foreach ( var badgeGroup in badgeGroups )
                {
                    var count = badgeGroup.Key;
                    var deviceRegistrationIds = badgeGroup.SelectMany( a => a.DeviceRegistrationIds ).ToList();

                    while ( deviceRegistrationIds.Any() )
                    {
                        // The docs say you can send to 1,000 registration ids at once.
                        // Let's send to at most 100 at a time for safety.
                        var registrationIds = deviceRegistrationIds.Take( 100 ).ToList();
                        deviceRegistrationIds = deviceRegistrationIds.Skip( 100 ).ToList();

                        tasks.Add( Task.Run( () => SendBadgeCountPushNotificationAsync( count, registrationIds ) ) );
                    }
                }

                await Task.WhenAll( tasks );
            }
        }

        /// <summary>
        /// Sends out a push notification to all of the device registration
        /// identifiers.
        /// </summary>
        /// <param name="count">The number to use in the application badge.</param>
        /// <param name="deviceRegistrationIds">The tokens that specify the devices to receive the push notifications.</param>
        /// <returns>A task that represents this operation.</returns>
        private static async Task SendBadgeCountPushNotificationAsync( int count, List<string> deviceRegistrationIds )
        {
            if ( !MediumContainer.HasActivePushTransport() )
            {
                return;
            }

            var pushMessage = new RockPushMessage
            {
                Data = new PushData(),
                SendSeperatelyToEachRecipient = false
            };

            pushMessage.Data.ApplicationBadgeCount = count;

            var mergeFields = new Dictionary<string, object>();

            pushMessage.SetRecipients( deviceRegistrationIds.Select( d => RockPushMessageRecipient.CreateAnonymous( d, mergeFields ) ).ToList() );

            await pushMessage.SendAsync();
        }
    }
}
