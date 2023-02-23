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
using Rock.Data;
using Rock.Transactions;

namespace Rock.Model
{
    public partial class NotificationMessage
    {
        /// <summary>
        /// Save hook implementation for <see cref="NotificationMessage"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<NotificationMessage>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                // Any added or deleted messages should trigger a badge update.
                // For modifications, it should only be if the count changed
                // but that would be the only time a message should be updated
                // so don't bother checking.
                RockContext.ExecuteAfterCommit( () =>
                {
                    new UpdateNotificationBadgeCountTransaction( Entity.PersonAliasId )
                        .Enqueue();
                } );

                base.PreSave();
            }
        }
    }
}
