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

using Rock.Model;
using Rock.RealTime;
using Rock.RealTime.Topics;

namespace Rock.Transactions
{
    /// <summary>
    /// Sends any real-time notifications for attendance occurrence records that have
    /// been recently created, modified or deleted.
    /// </summary>
    internal class SendAttendanceOccurrenceRealTimeNotificationsTransaction : AggregateAsyncTransaction<(Guid Guid, bool IsDeleted)>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendAttendanceOccurrenceRealTimeNotificationsTransaction"/> class.
        /// </summary>
        /// <param name="attendanceOccurrenceGuid">The attendance occurrence unique identifier.</param>
        /// <param name="isDeleted">if set to <c>true</c> then the attendance record was deleted.</param>
        public SendAttendanceOccurrenceRealTimeNotificationsTransaction( Guid attendanceOccurrenceGuid, bool isDeleted )
            : base( (attendanceOccurrenceGuid, isDeleted) )
        {
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync( IList<(Guid Guid, bool IsDeleted)> items )
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
                var deletedChannelName = EntityUpdatedTopic.GetAttendanceOccurrenceDeletedChannel();
                var deletedChannel = topicContext.Clients.Channel( deletedChannelName );

                foreach ( var deletedGuid in deletedItemGuids )
                {
                    try
                    {
                        await deletedChannel.AttendanceOccurrenceDeleted( deletedGuid );
                    }
                    catch
                    {
                        // Intentionally ignore any error if we failed to send
                        // a message about a single attendance occurrence record.
                    }
                }
            } );

            var modifiedNotificationTask = Task.Run( async () =>
            {
                await AttendanceOccurrenceService.SendAttendanceOccurrenceUpdatedRealTimeNotificationsAsync( itemGuids );
            } );

            await Task.WhenAll( deletedNotificationTask, modifiedNotificationTask );
        }
    }
}
