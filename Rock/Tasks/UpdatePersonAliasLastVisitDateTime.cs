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

using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Updates <see cref="PersonAlias.LastVisitDateTime"/> based on visitor activity
    /// </summary>
    public sealed class UpdatePersonAliasLastVisitDateTime : BusStartedTask<UpdatePersonAliasLastVisitDateTime.Message>
    {
        private static ConcurrentDictionary<int, DateTime> _previousLastVisitDateTimeByPersonAliasId = new ConcurrentDictionary<int, DateTime>();

        /// <summary>
        /// Determines if the LastVisitDateTime on this person alias (visitor) needs to be updated.
        /// base on how recently it was last updated.
        /// </summary>
        /// <param name="previousLastVisitDateTime">The previous last visit date time.</param>
        /// <param name="lastVisitDateTime">The last visit date time.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool NeedsToBeUpdated( DateTime? previousLastVisitDateTime, DateTime lastVisitDateTime )
        {
            /*   MP 2022-06-15
                We don't want to update LastVisitDateTime any more often then every 2 minutes.

                This is because this GetPersonAlias method could be called on the same Person
                100s of times a minute under heavy load especially when Visitor Tracking is enabled.
                Since this involves an UPDATE on the PersonAlias table, at the same time there
                are SELECTS on the same row, we can minimize database contention
                and unnecessary overhead in the message bus.
            */

            if ( !previousLastVisitDateTime.HasValue )
            {
                // If the don't have a LastVisitDateTime yet, we need to update it.
                return true;
            }

            var timeSinceLastVisitDateTimeUpdate = lastVisitDateTime - previousLastVisitDateTime.Value;
            return timeSinceLastVisitDateTimeUpdate.TotalMinutes > 2;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var personAliasService = new PersonAliasService( rockContext );
                var personAlias = personAliasService.Get( message.PersonAliasId );

                if ( personAlias == null )
                {
                    return;
                }

                // Double check that it needs to be updated in case the last check occurred before the
                // record was updated from a queued message.
                if ( NeedsToBeUpdated( personAlias.LastVisitDateTime, message.LastVisitDateTime ) )
                {
                    personAlias.LastVisitDateTime = message.LastVisitDateTime;

                    rockContext.SaveChanges( true );
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the person alias identifier.
            /// </summary>
            /// <value>The person alias identifier.</value>
            public int PersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets the last visit date time.
            /// </summary>
            /// <value>The last visit date time.</value>
            public DateTime LastVisitDateTime { get; set; }

            /// <summary>
            /// If this LastActivityDateTime hasn't been recently updated,
            /// this will Send this message and return true;
            /// </summary>
            public bool SendIfNeeded()
            {
                DateTime? previousLastOnlineActivityDateTime = _previousLastVisitDateTimeByPersonAliasId.GetValueOrNull( this.PersonAliasId );

                if ( NeedsToBeUpdated( previousLastOnlineActivityDateTime, this.LastVisitDateTime ) )
                {
                    this.Send();

                    _previousLastVisitDateTimeByPersonAliasId[this.PersonAliasId] = this.LastVisitDateTime;
                    return true;
                }

                return false;
            }
        }
    }
}