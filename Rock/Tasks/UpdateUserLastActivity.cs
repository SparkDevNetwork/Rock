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
    /// Updates <see cref="UserLogin.LastActivityDateTime"/> and <see cref="UserLogin.IsOnLine"/> based on user activity
    /// </summary>
    public sealed class UpdateUserLastActivity : BusStartedTask<UpdateUserLastActivity.Message>
    {
        private static ConcurrentDictionary<int, DateTime> _previousLastOnlineActivityDateTimeByUserId = new ConcurrentDictionary<int, DateTime>();

        /// <summary>
        /// Determines if the LastActivityDateTime on this user needs to be updated
        /// base on how recently it was last updated.
        /// </summary>
        /// <param name="previousLastOnlineActivityDateTime">The previous last online activity date time.</param>
        /// <param name="lastOnlineActivityDateTime">The last online activity date time.</param>
        /// <param name="isOnline">if set to <c>true</c> [is online].</param>
        private static bool NeedsToBeUpdated( DateTime? previousLastOnlineActivityDateTime, DateTime lastOnlineActivityDateTime, bool isOnline )
        {
            /*   MP 2022-03-10
                We don't want to update LastActivityDate any more often then every 2 minutes.

                This is because this GetCurrentUser method could be called on the same user
                100s or 1000s of times a minute under heavy load, especialy during Checkin.
                Since this involves and UPDATE on the UserLogin table, at the same time there
                are SELECTS on the same row, we can minimize database contention
                and unnecessary overhead in the message bus.
            */

            if ( !previousLastOnlineActivityDateTime.HasValue || isOnline == false )
            {
                // If the don't have a LastActivityDateTime yet, we need to update it.
                // If isOnline == false (which means they got signed out), also need to update it.
                return true;
            }

            var timeSinceLastOnlineUpdate = lastOnlineActivityDateTime - previousLastOnlineActivityDateTime.Value;
            return timeSinceLastOnlineUpdate.TotalMinutes > 2;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                var user = userLoginService.Get( message.UserId );

                if ( user == null )
                {
                    return;
                }

                // Double check that it needs to be updated in case the last check occurred before the
                // record was updated from a queued message.
                if ( NeedsToBeUpdated( user.LastActivityDateTime, message.LastActivityDate, message.IsOnline ) )
                {
                    user.LastActivityDateTime = message.LastActivityDate;
                    user.IsOnLine = message.IsOnline;

                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the user id.
            /// </summary>
            /// <value>
            /// The user id.
            /// </value>
            public int UserId { get; set; }

            /// <summary>
            /// Gets or sets the last activity date.
            /// </summary>
            /// <value>
            /// The last activity date.
            /// </value>
            public DateTime LastActivityDate { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [is on line].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [is on line]; otherwise, <c>false</c>.
            /// </value>
            public bool IsOnline { get; set; } = true;

            /// <summary>
            /// If this LastActivityDateTime hasn't been recently updated,
            /// this will Send this message and return true;
            /// </summary>
            public bool SendIfNeeded()
            {
                DateTime? previousLastOnlineActivityDateTime = _previousLastOnlineActivityDateTimeByUserId.GetValueOrNull( this.UserId );

                if ( NeedsToBeUpdated( previousLastOnlineActivityDateTime, this.LastActivityDate, this.IsOnline ) )
                {
                    this.Send();
                    if ( this.IsOnline )
                    {
                        _previousLastOnlineActivityDateTimeByUserId[this.UserId] = this.LastActivityDate;
                    }

                    return true;
                }

                return false;
            }
        }
    }
}