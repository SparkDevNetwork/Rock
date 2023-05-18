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
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Engagement;
using Rock.ViewModels.Event;

namespace Rock.RealTime.Topics
{
    /// <summary>
    /// A topic for client devices to use when monitoring for changes in
    /// various core entities.
    /// </summary>
    [RealTimeTopic]
    internal sealed class EntityUpdatedTopic : Topic<IEntityUpdated>
    {
        #region Achievement Methods

        /// <summary>
        /// Gets the achievement completed channels that should be used when
        /// sending notifications for this bag.
        /// </summary>
        /// <param name="bag">The bag that contains the achievement completed data.</param>
        /// <returns>A string that represents the channel name.</returns>
        public static List<string> GetAchievementCompletedChannelsForBag( AchievementCompletedMessageBag bag )
        {
            return new List<string>
            {
                GetAchievementCompletedChannelForAchievementType( bag.AchievementTypeGuid )
            };
        }

        /// <summary>
        /// Gets the channel for monitoring achievement completion notifications
        /// for the specified achievement type.
        /// </summary>
        /// <param name="guid">The achievement type unique identifier.</param>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetAchievementCompletedChannelForAchievementType( Guid guid )
        {
            return $"AchievementAttempt:Completed:{guid}";
        }

        #endregion

        #region Attendance Methods

        /// <summary>
        /// Gets the attendance channels that should be used when sending
        /// notifications for this bag.
        /// </summary>
        /// <param name="bag">The bag that contains the attendance data.</param>
        /// <returns>A list of strings that contain the channel names.</returns>
        public static List<string> GetAttendanceChannelsForBag( AttendanceUpdatedMessageBag bag )
        {
            var channels = new List<string>();

            if ( bag.GroupGuid.HasValue )
            {
                channels.Add( GetAttendanceChannelForGroup( bag.GroupGuid.Value ) );
            }

            if ( bag.LocationGuid.HasValue )
            {
                channels.Add( GetAttendanceChannelForLocation( bag.LocationGuid.Value ) );
            }

            return channels;
        }

        /// <summary>
        /// Gets the channel for monitoring attendance notifications for the
        /// specified group.
        /// </summary>
        /// <param name="guid">The group unique identifier.</param>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetAttendanceChannelForGroup( Guid guid )
        {
            return $"Attendance:Group:{guid}";
        }

        /// <summary>
        /// Gets the channel for monitoring attendance notifications for the
        /// specified location.
        /// </summary>
        /// <param name="guid">The location unique identifier.</param>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetAttendanceChannelForLocation( Guid guid )
        {
            return $"Attendance:Location:{guid}";
        }

        /// <summary>
        /// Gets the channel for monitoring attendance deleted notifications.
        /// </summary>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetAttendanceDeletedChannel()
        {
            return "Attendance:Deleted";
        }

        #endregion

        #region AttendanceOccurrence Methods

        /// <summary>
        /// Gets the attendance occurrence channels that should be used when sending
        /// notifications for this bag.
        /// </summary>
        /// <param name="bag">The bag that contains the attendance occurrence data.</param>
        /// <returns>A list of strings that contain the channel names.</returns>
        public static List<string> GetAttendanceOccurrenceChannelsForBag( AttendanceOccurrenceUpdatedMessageBag bag )
        {
            var channels = new List<string>();

            if ( bag.GroupGuid.HasValue )
            {
                channels.Add( GetAttendanceOccurrenceChannelForGroup( bag.GroupGuid.Value ) );
            }

            if ( bag.LocationGuid.HasValue )
            {
                channels.Add( GetAttendanceOccurrenceChannelForLocation( bag.LocationGuid.Value ) );
            }

            return channels;
        }

        /// <summary>
        /// Gets the channel for monitoring attendance occurrence notifications for the
        /// specified group.
        /// </summary>
        /// <param name="guid">The group unique identifier.</param>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetAttendanceOccurrenceChannelForGroup( Guid guid )
        {
            return $"AttendanceOccurrence:Group:{guid}";
        }

        /// <summary>
        /// Gets the channel for monitoring attendance occurrence notifications for the
        /// specified location.
        /// </summary>
        /// <param name="guid">The location unique identifier.</param>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetAttendanceOccurrenceChannelForLocation( Guid guid )
        {
            return $"AttendanceOccurrence:Location:{guid}";
        }

        /// <summary>
        /// Gets the channel for monitoring attendance occurrence deleted notifications.
        /// </summary>
        /// <returns>A string that represents the channel name.</returns>
        public static string GetAttendanceOccurrenceDeletedChannel()
        {
            return "AttendanceOccurrence:Deleted";
        }

        #endregion
    }
}
