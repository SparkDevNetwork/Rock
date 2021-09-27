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

using Rock.Data;
using Rock.Tasks;

namespace Rock.Model
{
    public partial class Attendance
    {
        /// <summary>
        /// Save hook implementation for <see cref="Attendance"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Attendance>
        {
            /// <summary>
            /// Method that will be called on an entity immediately before the item is saved by context
            /// </summary>
            protected override void PreSave()
            {
                _isDeleted = State == EntityContextState.Deleted;
                bool previousDidAttendValue;

                bool previouslyDeclined;

                if ( State == EntityContextState.Added )
                {
                    previousDidAttendValue = false;
                    previouslyDeclined = false;
                }
                else
                {
                    // get original values so we can detect whether the value changed
                    previousDidAttendValue = ( bool ) Entry.OriginalValues.GetReadOnlyValueOrDefault( "DidAttend", false );
                    previouslyDeclined = ( Entry.OriginalValues.GetReadOnlyValueOrDefault( "RSVP", null ) as RSVP? ) == RSVP.No;
                }

                // if the record was changed to Declined, queue a GroupScheduleCancellationTransaction in PostSaveChanges
                _declinedScheduledAttendance = ( previouslyDeclined == false ) && Entity.IsScheduledPersonDeclined();

                if ( previousDidAttendValue == false && Entity.DidAttend == true )
                {
                    var launchMemberAttendedGroupWorkflowMsg = GetLaunchMemberAttendedGroupWorkflowMessage();
                    launchMemberAttendedGroupWorkflowMsg.Send();
                }

                base.PreSave();
            }

            private bool _declinedScheduledAttendance = false;
            private bool _isDeleted = false;

            /// <summary>
            /// Method that will be called on an entity immediately after the item is saved by context
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                if ( _declinedScheduledAttendance )
                {
                    new LaunchGroupScheduleCancellationWorkflow.Message()
                    {
                        AttendanceId = Entity.Id
                    }.Send();
                }

                if ( !_isDeleted )
                {
                    // Process any streaks that may occur as a result of adding/modifying an attendance record.
                    // If there are any, they need to be processed in this thread in case there are any achievement changes
                    // that need to be detected as a result of this attendance.
                    StreakTypeService.HandleAttendanceRecord( Entity.Id );
                }

                base.PostSave();
            }

            private LaunchMemberAttendedGroupWorkflow.Message GetLaunchMemberAttendedGroupWorkflowMessage()
            {
                var launchMemberAttendedGroupWorkflowMsg = new LaunchMemberAttendedGroupWorkflow.Message();
                if ( State != EntityContextState.Deleted )
                {
                    // Get the attendance record
                    var attendance = Entity as Attendance;

                    // If attendance record is valid and the DidAttend is true (not null or false)
                    if ( attendance != null && ( attendance.DidAttend == true ) )
                    {
                        // Save for all adds
                        bool valid = State == EntityContextState.Added;

                        // If not an add, check previous DidAttend value
                        if ( !valid )
                        {
                            // Only use changes where DidAttend was previously not true
                            valid = !( bool ) Entry.OriginalValues.GetReadOnlyValueOrDefault( "DidAttend", false );
                        }

                        if ( valid )
                        {
                            var occ = attendance.Occurrence;
                            if ( occ == null )
                            {
                                occ = new AttendanceOccurrenceService( new RockContext() ).Get( attendance.OccurrenceId );
                            }

                            if ( occ != null )
                            {
                                // Save the values
                                launchMemberAttendedGroupWorkflowMsg.GroupId = occ.GroupId;
                                launchMemberAttendedGroupWorkflowMsg.AttendanceDateTime = occ.OccurrenceDate;
                                launchMemberAttendedGroupWorkflowMsg.PersonAliasId = attendance.PersonAliasId;

                                if ( occ.Group != null )
                                {
                                    launchMemberAttendedGroupWorkflowMsg.GroupTypeId = occ.Group.GroupTypeId;
                                }
                            }
                        }
                    }
                }

                return launchMemberAttendedGroupWorkflowMsg;
            }
        }
    }
}
