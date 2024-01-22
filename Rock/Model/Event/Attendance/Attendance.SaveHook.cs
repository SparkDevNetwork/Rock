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
using Rock.Enums.Event;
using Rock.Tasks;
using Rock.Transactions;
using Rock.Web.Cache;

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
            /// Changes to the Person's Attendance's Group, Schedule or Location 
            /// </summary>
            /// <value>The person attendance history change list.</value>
            private History.HistoryChangeList PersonAttendanceHistoryChangeList { get; set; }

            private int? preSavePersonAliasId { get; set; }

            private bool previousDidAttendValue { get; set; }

            /// <summary>
            /// Method that will be called on an entity immediately before the item is saved by context
            /// </summary>
            protected override void PreSave()
            {
                PersonAttendanceHistoryChangeList = new History.HistoryChangeList();

                _isDeleted = State == EntityContextState.Deleted;

                bool previouslyDeclined;

                if ( State == EntityContextState.Added )
                {
                    previousDidAttendValue = false;
                    previouslyDeclined = false;
                }
                else
                {
                    // get original values so we can detect whether the value changed
                    previousDidAttendValue = ( bool? )Entry.OriginalValues.GetReadOnlyValueOrDefault( "DidAttend", false ) == true;
                    previouslyDeclined = ( Entry.OriginalValues.GetReadOnlyValueOrDefault( "RSVP", null ) as RSVP? ) == RSVP.No;
                }

                // if the record was changed to Declined, queue a GroupScheduleCancellationTransaction in PostSaveChanges
                _declinedScheduledAttendance = ( previouslyDeclined == false ) && Entity.IsScheduledPersonDeclined();

                /*
                    06/21/2023 ETD
                    Launch the workflow in post save to avoid a race condition between the bus message and the saving of the Attendance record.
                    The LaunchMemberAttendedGroupWorkflow needs to be run post save to work correctly.

                    if ( previousDidAttendValue == false && Entity.DidAttend == true )
                    {
                        var launchMemberAttendedGroupWorkflowMsg = GetLaunchMemberAttendedGroupWorkflowMessage();
                        launchMemberAttendedGroupWorkflowMsg.Send();
                    }

                */


                var attendance = this.Entity;

                if ( State == EntityContextState.Added )
                {
                    if ( attendance.CheckInStatus != CheckInStatus.Unknown )
                    {
                        UpdateCheckInDatesFromCheckInStatus();
                    }
                    else
                    {
                        UpdateCheckInStatusFromCheckInDates();
                    }
                }
                else if ( State == EntityContextState.Modified )
                {
                    if ( IsCheckInStatusModified() )
                    {
                        UpdateCheckInDatesFromCheckInStatus();
                    }
                    else if ( AreCheckInDatesModified() )
                    {
                        UpdateCheckInStatusFromCheckInDates();
                    }

                    preSavePersonAliasId = attendance.PersonAliasId;
                    var originalOccurrenceId = ( int? ) OriginalValues[nameof( attendance.OccurrenceId )];
                    if ( originalOccurrenceId.HasValue && attendance.OccurrenceId != originalOccurrenceId.Value )
                    {
                        var attendanceOccurrenceService = new AttendanceOccurrenceService( this.RockContext );
                        var originalOccurrence = attendanceOccurrenceService.GetNoTracking( originalOccurrenceId.Value );
                        var currentOccurrence = attendanceOccurrenceService.GetNoTracking( attendance.OccurrenceId );
                        if ( originalOccurrence != null && currentOccurrence != null )
                        {
                            if ( originalOccurrence.GroupId != currentOccurrence.GroupId )
                            {
                                History.EvaluateChange( PersonAttendanceHistoryChangeList, "Group", originalOccurrence.Group?.Name, currentOccurrence.Group?.Name );
                            }

                            if ( originalOccurrence.ScheduleId.HasValue && currentOccurrence.ScheduleId.HasValue && originalOccurrence.ScheduleId.Value != currentOccurrence.ScheduleId.Value )
                            {
                                History.EvaluateChange( PersonAttendanceHistoryChangeList, "Schedule", NamedScheduleCache.Get( originalOccurrence.ScheduleId.Value )?.Name, NamedScheduleCache.Get( currentOccurrence.ScheduleId.Value )?.Name );
                            }

                            if ( originalOccurrence.LocationId.HasValue && currentOccurrence.LocationId.HasValue && originalOccurrence.LocationId.Value != currentOccurrence.LocationId.Value )
                            {
                                History.EvaluateChange( PersonAttendanceHistoryChangeList, "Location", NamedLocationCache.Get( originalOccurrence.LocationId.Value )?.Name, NamedLocationCache.Get( currentOccurrence.LocationId.Value )?.Name );
                            }
                        }
                    }

                    // Add the checkin and the checkout to the history if
                    var previousCheckInValue = ( DateTime? ) Entry.OriginalValues.GetReadOnlyValueOrDefault( "StartDateTime", null );
                    var previousCheckOutValue = ( DateTime? ) Entry.OriginalValues.GetReadOnlyValueOrDefault( "EndDateTime", null );
                    History.EvaluateChange( PersonAttendanceHistoryChangeList, "Check-in", previousCheckInValue?.ToShortDateTimeString(), attendance.StartDateTime.ToShortDateTimeString() );
                    History.EvaluateChange( PersonAttendanceHistoryChangeList, "Check-out", previousCheckOutValue?.ToShortDateTimeString(), attendance.EndDateTime?.ToShortDateTimeString() );
                }
                else if ( State == EntityContextState.Deleted )
                {
                    preSavePersonAliasId = ( int? ) OriginalValues[nameof( attendance.PersonAliasId )];
                    PersonAttendanceHistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Attendance" );
                }

                // If we need to send a real-time notification then do so after
                // this change has been committed to the database.
                if ( ShouldSendRealTimeMessage() )
                {
                    RockContext.ExecuteAfterCommit( () =>
                    {
                        // Use the fast queue for this because it is real-time.
                        new SendAttendanceRealTimeNotificationsTransaction( Entity.Guid, State == EntityContextState.Deleted ).Enqueue( true );
                    } );
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

                // Do this in post save to avoid a race condition between the bus message and the saving of the Attendance record. See engineering note in PreSave().
                if ( previousDidAttendValue == false && Entity.DidAttend == true )
                {
                    var launchMemberAttendedGroupWorkflowMsg = GetLaunchMemberAttendedGroupWorkflowMessage();
                    launchMemberAttendedGroupWorkflowMsg.Send();
                }

                var rockContext = ( RockContext ) this.RockContext;

                if ( PersonAttendanceHistoryChangeList?.Any() == true )
                {
                    var attendanceId = Entity.Id;

                    if ( preSavePersonAliasId.HasValue )
                    {
                        var attendeePersonId = new PersonAliasService( this.RockContext ).GetPersonId( preSavePersonAliasId.Value );
                        if ( attendeePersonId.HasValue )
                        {
                            var entityTypeType = typeof( Person );
                            var relatedEntityTypeType = typeof( Attendance );
                            HistoryService.SaveChanges(
                                rockContext,
                                entityTypeType,
                                Rock.SystemGuid.Category.HISTORY_ATTENDANCE_CHANGES.AsGuid(),
                                attendeePersonId.Value,
                                this.PersonAttendanceHistoryChangeList,
                                $"Attendance {attendanceId}",
                                relatedEntityTypeType,
                                attendanceId,
                                true,
                                Entity.ModifiedByPersonAliasId,
                                rockContext.SourceOfChange );
                        }
                    }
                }

                base.PostSave();
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
                    if ( ( Entity.DidAttend ?? false ) != ( ( ( bool? ) OriginalValues[nameof( Entity.DidAttend )] ) ?? false ) )
                    {
                        return true;
                    }
                    else if ( Entity.RSVP != ( RSVP ) OriginalValues[nameof( Entity.RSVP )] )
                    {
                        return true;
                    }
                    else if ( Entity.PresentDateTime != ( DateTime? ) OriginalValues[nameof( Entity.PresentDateTime )] )
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

            /// <summary>
            /// Determines whether CheckInStatus property has been modified.
            /// </summary>
            /// <returns><c>true</c> if the property was modified; otherwise, <c>false</c>.</returns>
            private bool IsCheckInStatusModified()
            {
                var originalCheckInStatus = ( CheckInStatus ) OriginalValues[nameof( Entity.CheckInStatus )];

                return originalCheckInStatus != Entity.CheckInStatus;
            }

            /// <summary>
            /// Determines whether any of the check-in DateTime properties
            /// have been modified.
            /// </summary>
            /// <returns><c>true</c> if any properties were modified, <c>false</c> otherwise.</returns>
            private bool AreCheckInDatesModified()
            {
                var originalStartDateTime = ( DateTime ) OriginalValues[nameof( Entity.StartDateTime )];
                var originalEndDateTime = ( DateTime? ) OriginalValues[nameof( Entity.EndDateTime )];
                var originalPresentDateTime = ( DateTime? ) OriginalValues[nameof( Entity.PresentDateTime )];

                return originalStartDateTime != Entity.StartDateTime
                    || originalEndDateTime != Entity.EndDateTime
                    || originalPresentDateTime != Entity.PresentDateTime;
            }

            /// <summary>
            /// Updates the check in dates from the CheckInStatus property. This
            /// is called when the CheckInStatus has been modified.
            /// </summary>
            private void UpdateCheckInDatesFromCheckInStatus()
            {
                if ( Entity.CheckInStatus == CheckInStatus.CheckedOut )
                {
                    Entity.EndDateTime = Entity.EndDateTime ?? RockDateTime.Now;
                }
                else if ( Entity.CheckInStatus == CheckInStatus.Present )
                {
                    Entity.PresentDateTime = Entity.PresentDateTime ?? RockDateTime.Now;
                    Entity.EndDateTime = null;
                }
                else if ( Entity.CheckInStatus == CheckInStatus.NotPresent )
                {
                    Entity.PresentDateTime = null;
                    Entity.EndDateTime = null;
                }
                else if ( Entity.CheckInStatus == CheckInStatus.Pending )
                {
                    Entity.PresentDateTime = null;
                    Entity.EndDateTime = null;
                }
            }

            /// <summary>
            /// Updates the CheckInStatus property from the check in dates. This
            /// is called if the CheckInStatus property has not changed but one
            /// of the date values has.
            /// </summary>
            private void UpdateCheckInStatusFromCheckInDates()
            {
                if ( Entity.EndDateTime.HasValue )
                {
                    Entity.CheckInStatus = CheckInStatus.CheckedOut;
                }
                else if ( Entity.PresentDateTime.HasValue )
                {
                    Entity.CheckInStatus = CheckInStatus.Present;
                }
                else
                {
                    Entity.CheckInStatus = CheckInStatus.NotPresent;
                }
            }

            private LaunchMemberAttendedGroupWorkflow.Message GetLaunchMemberAttendedGroupWorkflowMessage()
            {
                var launchMemberAttendedGroupWorkflowMsg = new LaunchMemberAttendedGroupWorkflow.Message();
                if ( State == EntityContextState.Deleted )
                {
                    return launchMemberAttendedGroupWorkflowMsg;
                }

                // Get the attendance record
                var attendance = Entity as Attendance;

                // If attendance record is not valid or the DidAttend is false
                if ( attendance == null || ( attendance.DidAttend.GetValueOrDefault( false ) == false ) )
                {
                    return launchMemberAttendedGroupWorkflowMsg;
                }

                // Save for all adds
                bool valid = State == EntityContextState.Added;

                // If not an add, check previous DidAttend value
                if ( !valid )
                {
                    // Only use changes where DidAttend was previously not true
                    valid = ( bool? ) Entry.OriginalValues.GetReadOnlyValueOrDefault( "DidAttend", false ) != true;
                }

                if ( valid )
                {
                    var occ = attendance.Occurrence ?? new AttendanceOccurrenceService( new RockContext() ).Get( attendance.OccurrenceId );

                    if ( occ != null )
                    {
                        // Save the values
                        launchMemberAttendedGroupWorkflowMsg.GroupId = occ.GroupId;
                        launchMemberAttendedGroupWorkflowMsg.AttendanceDateTime = occ.OccurrenceDate;
                        launchMemberAttendedGroupWorkflowMsg.PersonAliasId = attendance.PersonAliasId;
                        launchMemberAttendedGroupWorkflowMsg.AttendanceId = attendance.Id;

                        if ( occ.Group != null )
                        {
                            launchMemberAttendedGroupWorkflowMsg.GroupTypeId = occ.Group.GroupTypeId;
                        }
                    }
                }

                return launchMemberAttendedGroupWorkflowMsg;
            }
        }
    }
}
