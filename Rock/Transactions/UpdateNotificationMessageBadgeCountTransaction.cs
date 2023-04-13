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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Updates the badge count for the specified person alias identifier. This
    /// will send out any push notifications or other such notifications that
    /// need to be delivered.
    /// </summary>
    internal class UpdateNotificationBadgeCountTransaction : AggregateAsyncTransaction<int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendAttendanceRealTimeNotificationsTransaction"/> class.
        /// </summary>
        /// <param name="personAliasId">The <see cref="PersonAlias"/> identifier that had notification messages updated.</param>
        public UpdateNotificationBadgeCountTransaction( int personAliasId )
            : base( personAliasId )
        {
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync( IList<int> items )
        {
            // Get the distinct set of items that were added/modified.
            var personAliasIds = items
                .Distinct()
                .ToList();

            // What we have is a bunch of Person Alias identifiers.
            // What we need, is a set of PersonId+SiteId values and
            // then the associated device registration identifiers.
            // This gives us a way to lookup the badge counts and then
            // know what devices to notify.

            using ( var rockContext = new RockContext() )
            {
                var personAliasService = new PersonAliasService( rockContext );

                // We have all the person alias Ids, but really we need the
                // Person Ids.
                var personIds = new List<int>();

                while ( personAliasIds.Any() )
                {
                    var aliasIds = personAliasIds.Take( 10_000 ).ToList();
                    personAliasIds = personAliasIds.Skip( 10_000 ).ToList();

                    var ids = personAliasService.Queryable()
                        .Where( pa => aliasIds.Contains( pa.Id ) )
                        .Select( pa => pa.PersonId );

                    personIds.AddRange( ids );
                }

                await NotificationMessageService.SendBadgeCountUpdatesAsync( personIds );
            }
        }
    }
}
