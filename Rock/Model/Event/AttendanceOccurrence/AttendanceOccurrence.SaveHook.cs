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

using System;
using System.Linq;

using Rock.Data;
using Rock.Tasks;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class AttendanceOccurrence
    {
        /// <summary>
        /// Save hook implementation for <see cref="AttendanceOccurrence"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<AttendanceOccurrence>
        {
            /// <summary>
            /// Method that will be called on an entity immediately before the item is saved by context
            /// </summary>
            protected override void PreSave()
            {
                // If we need to send a real-time notification then do so after
                // this change has been committed to the database.
                if ( ShouldSendRealTimeMessage() )
                {
                    RockContext.ExecuteAfterCommit( () =>
                    {
                        // Use the fast queue for this because it is real-time.
                        new SendAttendanceOccurrenceRealTimeNotificationsTransaction( Entity.Guid, State == EntityContextState.Deleted )
                            .Enqueue( true );
                    } );
                }

                base.PreSave();
            }

            /// <summary>
            /// Determines if we need to send any real-time messages for the
            /// changes made to this entity.
            /// </summary>
            /// <returns><c>true</c> if a message should be sent, <c>false</c> otherwise.</returns>
            private bool ShouldSendRealTimeMessage()
            {
                if ( !RockContext.IsRealTimeEnabled )
                {
                    return false;
                }

                if ( PreSaveState == EntityContextState.Added )
                {
                    return true;
                }
                else if ( PreSaveState == EntityContextState.Modified )
                {
                    if ( ( Entity.DidNotOccur ?? false ) != ( ( ( bool? ) OriginalValues[nameof( Entity.DidNotOccur )] ) ?? false ) )
                    {
                        return true;
                    }
                    else if ( Entity.AttendanceTypeValueId != ( int? ) OriginalValues[nameof( Entity.AttendanceTypeValueId )] )
                    {
                        return true;
                    }
                }
                else if ( PreSaveState == EntityContextState.Deleted )
                {
                    return true;
                }

                return false;
            }
        }
    }
}
