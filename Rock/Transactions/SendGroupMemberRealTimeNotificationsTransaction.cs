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
    /// Sends any real-time notifications for group member records that have
    /// been recently created, modified or deleted.
    /// </summary>
    internal class SendGroupMemberRealTimeNotificationsTransaction : AggregateAsyncTransaction<GroupMemberService.GroupMemberUpdatedState>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendGroupMemberRealTimeNotificationsTransaction"/> class.
        /// </summary>
        /// <param name="state">The state object that represents the entity when it was saved.</param>
        public SendGroupMemberRealTimeNotificationsTransaction( GroupMemberService.GroupMemberUpdatedState state )
            : base( state )
        {
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync( IList<GroupMemberService.GroupMemberUpdatedState> items )
        {
            var deletedNotificationTask = Task.Run( () =>
            {
                // Get the set of items that were deleted.
                var deletedItems = items
                    .Where( i => i.State == EntityContextState.Deleted )
                    .ToList();

                return GroupMemberService.SendGroupMemberDeletedRealTimeNotificationsAsync( deletedItems );
            } );

            var modifiedNotificationTask = Task.Run( () =>
            {
                // Get the set of items that were deleted.
                var updatedItems = items
                    .Where( i => i.State != EntityContextState.Deleted )
                    .ToList();

                return GroupMemberService.SendGroupMemberUpdatedRealTimeNotificationsAsync( updatedItems );
            } );

            await Task.WhenAll( deletedNotificationTask, modifiedNotificationTask );
        }
    }
}
