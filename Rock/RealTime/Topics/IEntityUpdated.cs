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
using System.Threading.Tasks;

using Rock.ViewModels.Engagement;
using Rock.ViewModels.Event;

namespace Rock.RealTime.Topics
{
    /// <summary>
    /// A topic interface for sending messages to clients that are monitoring
    /// for changes in various core entities.
    /// </summary>
    internal interface IEntityUpdated
    {
        #region Achievement Methods

        /// <summary>
        /// Called when an Achievement Attempt record is created or udpated in
        /// a way that would consider it complete when it wasn't complete before.
        /// </summary>
        /// <param name="bag">The message bag that represents the achievement completion.</param>
        Task AchievementCompleted( AchievementCompletedMessageBag bag );

        #endregion

        #region Attendance Methods

        /// <summary>
        /// Called when an Attendance record is created or updated in a way that
        /// would change the values of the message bag.
        /// </summary>
        /// <param name="bag">The message bag that represents the attendance.</param>
        Task AttendanceUpdated( AttendanceUpdatedMessageBag bag );

        /// <summary>
        /// Called when an Attendance record has been deleted.
        /// </summary>
        /// <param name="attendanceGuid">The attendance unique identifier.</param>
        /// <param name="bag">The message bag that represents the attendance, may be <c>null</c> in some cases.</param>
        Task AttendanceDeleted( Guid attendanceGuid, AttendanceUpdatedMessageBag bag );

        #endregion

        #region Attendance Occurrence Methods

        /// <summary>
        /// Called when an AttendanceOccurrence record is created or updated in a way that
        /// would change the values of the message bag.
        /// </summary>
        /// <param name="bag">The message bag that represents the attendance occurrence.</param>
        Task AttendanceOccurrenceUpdated( AttendanceOccurrenceUpdatedMessageBag bag );

        /// <summary>
        /// Called when an AttendanceOccurrence record has been deleted.
        /// </summary>
        /// <param name="attendanceOccurrenceGuid">The attendance occurrence unique identifier.</param>
        Task AttendanceOccurrenceDeleted( Guid attendanceOccurrenceGuid );

        #endregion
    }
}
