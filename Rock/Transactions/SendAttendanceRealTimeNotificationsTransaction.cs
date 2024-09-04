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
using System.Threading.Tasks;

using Rock.Enums.Event;
using Rock.Model;
using Rock.RealTime;
using Rock.RealTime.Topics;

namespace Rock.Transactions
{
    /// <summary>
    /// Sends any real-time notifications for attendance records that have
    /// been recently created, modified or deleted.
    /// </summary>
    internal class SendAttendanceRealTimeNotificationsTransaction : AggregateAsyncTransaction<(Guid Guid, bool IsDeleted, bool? DidAttend, DateTime? PresentDateTime, DateTime? EndDateTime, CheckInStatus CheckInStatus)>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendAttendanceRealTimeNotificationsTransaction"/> class.
        /// </summary>
        /// <param name="attendanceGuid">The attendance unique identifier.</param>
        /// <param name="isDeleted">if set to <c>true</c> then the attendance record was deleted.</param>
        /// <param name="didAttend">Matches the value of <see cref="Attendance.DidAttend"/>.</param>
        /// <param name="presentDateTime">Matches the value of <see cref="Attendance.PresentDateTime"/>.</param>
        /// <param name="endDateTime">Matches the value of <see cref="Attendance.EndDateTime"/>.</param>
        /// <param name="checkInStatus">Matches the value of <see cref="Attendance.CheckInStatus"/>.</param>
        public SendAttendanceRealTimeNotificationsTransaction( Guid attendanceGuid, bool isDeleted, bool? didAttend, DateTime? presentDateTime, DateTime? endDateTime, CheckInStatus checkInStatus )
            : base( (attendanceGuid, isDeleted, didAttend, presentDateTime, endDateTime, checkInStatus) )
        {
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync( IList<(Guid Guid, bool IsDeleted, bool? DidAttend, DateTime? PresentDateTime, DateTime? EndDateTime, CheckInStatus CheckInStatus)> items )
        {
            // Get the distinct set of items that were deleted.
            var deletedItemGuids = items
                .Where( i => i.IsDeleted )
                .Select( i => i.Guid )
                .Distinct()
                .ToList();

            // Get the distinct set of items that were added/modified.
            var itemGuids = items
                .Where( i => !i.IsDeleted && !deletedItemGuids.Contains( i.Guid ) )
                .Select( i => i.Guid )
                .Distinct()
                .ToList();

            var topicContext = RealTimeHelper.GetTopicContext<IEntityUpdated>();

            // Start a task to notify about all the deleted attendance records.
            var deletedNotificationTask = Task.Run( async () =>
            {
                var deletedChannelName = EntityUpdatedTopic.GetAttendanceDeletedChannel();
                var deletedChannel = topicContext.Clients.Channel( deletedChannelName );

                foreach ( var deletedGuid in deletedItemGuids )
                {
                    try
                    {
                        await deletedChannel.AttendanceDeleted( deletedGuid );
                    }
                    catch
                    {
                        // Intentionally ignore any error if we failed to send
                        // a message about a single attendance record.
                    }
                }
            } );

            var modifiedNotificationTask = Task.Run( async () =>
            {
                await AttendanceService.SendAttendanceUpdatedRealTimeNotificationsAsync( itemGuids, items );
            } );

            await Task.WhenAll( deletedNotificationTask, modifiedNotificationTask );
        }
    }
}
