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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Common.Mobile.Blocks.Groups.GroupAttendanceEntry;
using Rock.Data;
using Rock.Model;

namespace Rock.Blocks.Types.Mobile.Groups
{
    /// <summary>
    /// Displays a page to allow the user to mark attendance for a group.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Group Attendance Entry" )]
    [Category( "Mobile > Groups" )]
    [Description( "Allows the user to mark attendance for a group." )]
    [IconCssClass( "fa fa-user-check" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [IntegerField( "Number of Days Forward to Allow",
        Description = "",
        IsRequired = true,
        DefaultIntegerValue = 0,
        Key = AttributeKeys.NumberOfDaysForwardToAllow,
        Order = 0 )]

    [IntegerField( "Number of Days Back to Allow",
        Description = "",
        IsRequired = true,
        DefaultIntegerValue = 30,
        Key = AttributeKeys.NumberOfDaysBackToAllow,
        Order = 1 )]

    [LinkedPage( "Save Redirect Page",
        Description = "If set, redirect user to this page on save. If not set, page is popped off the navigation stack.",
        IsRequired = false,
        Key = AttributeKeys.SaveRedirectPage,
        Order = 2 )]

    [BooleanField( "Show Save Button",
        Description = "If enabled a save button will be shown (recommended for large groups), otherwise no save button will be displayed and a save will be triggered with each selection (recommended for smaller groups).",
        IsRequired = true,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowSaveButton,
        Order = 3 )]

    [BooleanField( "Allow Any Date Selection",
        Description = "If enabled a date picker will be shown, otherwise a dropdown with only the valid dates will be shown.",
        IsRequired = true,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.AllowAnyDateSelection,
        Order = 4 )]

    [BooleanField( "Show Attendance Notes",
        Description = "Enables collecting notes about the attendance. This will automatically show the save button as well.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKeys.ShowAttendanceNotes,
        Order = 5 )]

    [TextField( "Attendance Note Label",
        Description = "The label that will describe how the notes will be used.",
        IsRequired = true,
        DefaultValue = "Notes",
        Key = AttributeKeys.AttendanceNoteLabel,
        Order = 6 )]

    [BooleanField( "Show Pending Members",
        Description = "Shows pending members of the group.",
        DefaultBooleanValue = false,
        Key = AttributeKeys.ShowPendingMembers,
        Order = 7 )]

    [BooleanField( "Show Inactive Members",
        Description = "Shows inactive members of the group.",
        DefaultBooleanValue = false,
        Key = AttributeKeys.ShowInactiveMembers,
        Order = 8 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_GROUPS_GROUP_ATTENDANCE_ENTRY_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( "08AE409C-9E4C-42D1-A93C-A554A3EEA0C3" )]
    public class GroupAttendanceEntry : RockBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the GroupAttendanceEntry block.
        /// </summary>
        public static class AttributeKeys
        {
            /// <summary>
            /// The number of days forward to allow attribute key.
            /// </summary>
            public const string NumberOfDaysForwardToAllow = "NumberOfDaysForwardToAllow";

            /// <summary>
            /// The number of days back to allow attribute key.
            /// </summary>
            public const string NumberOfDaysBackToAllow = "NumberOfDaysBackToAllow";

            /// <summary>
            /// The on-save redirect page attribute key.
            /// </summary>
            public const string SaveRedirectPage = "SaveRedirectPage";

            /// <summary>
            /// The show save button attribute key.
            /// </summary>
            public const string ShowSaveButton = "ShowSaveButton";

            /// <summary>
            /// The allow any date selection attribute key.
            /// </summary>
            public const string AllowAnyDateSelection = "AllowAnyDateSelection";

            /// <summary>
            /// The show attendance notes attribute key.
            /// </summary>
            public const string ShowAttendanceNotes = "ShowAttendanceNotes";

            /// <summary>
            /// The attendance note label attribute key.
            /// </summary>
            public const string AttendanceNoteLabel = "AttendanceNoteLabel";

            /// <summary>
            /// The show pending members attribute key.
            /// </summary>
            public const string ShowPendingMembers = "ShowPendingMembers";

            /// <summary>
            /// The show inactive members attribute key.
            /// </summary>
            public const string ShowInactiveMembers = "ShowInactiveMembers";
        }

        /// <summary>
        /// Gets the number of days forward to allow attendance to be entered for.
        /// </summary>
        /// <value>
        /// The number of days forward to allow attendance to be entered for.
        /// </value>
        protected int NumberOfDaysForwardToAllow => GetAttributeValue( AttributeKeys.NumberOfDaysForwardToAllow ).AsInteger();

        /// <summary>
        /// Gets the number of days back to allow attendance to be entered for.
        /// </summary>
        /// <value>
        /// The number of days back to allow attendance to be entered for.
        /// </value>
        protected int NumberOfDaysBackToAllow => GetAttributeValue( AttributeKeys.NumberOfDaysBackToAllow ).AsInteger();

        /// <summary>
        /// Gets the on-save redirect page.
        /// </summary>
        /// <value>
        /// The on-save redirect page.
        /// </value>
        protected Guid? SaveRedirectPage => GetAttributeValue( AttributeKeys.SaveRedirectPage ).AsGuidOrNull();

        /// <summary>
        /// Gets a value indicating whether the save button should be shown.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the save button should be shown; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowSaveButton => GetAttributeValue( AttributeKeys.ShowSaveButton ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether to allow any custom date to be selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if to allow any custom date to be selected; otherwise, <c>false</c>.
        /// </value>
        protected bool AllowAnyDateSelection => GetAttributeValue( AttributeKeys.AllowAnyDateSelection ).AsBoolean();

        /// <summary>
        /// Gets a value indicating whether a text field for note entry will be visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a text field for note entry will be visible; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowAttendanceNotes => GetAttributeValue( AttributeKeys.ShowAttendanceNotes ).AsBoolean();

        /// <summary>
        /// Gets the attendance note label text that appears above the notes field.
        /// </summary>
        /// <value>
        /// The attendance note label text that appears above the notes field.
        /// </value>
        protected string AttendanceNoteLabel => GetAttributeValue( AttributeKeys.AttendanceNoteLabel );

        /// <summary>
        ///  Gets whether or not we should display inactive members.
        /// </summary>
        /// <value>The show inactive members.</value>
        protected bool ShowInactiveMembers => GetAttributeValue( AttributeKeys.ShowInactiveMembers ).AsBoolean();

        /// <summary>
        /// Gets whether or not we should display pending members.
        /// </summary>
        /// <value>The show pending members.</value>
        protected bool ShowPendingMembers => GetAttributeValue( AttributeKeys.ShowPendingMembers ).AsBoolean();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 1 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                AllowAnyDateSelection = AllowAnyDateSelection,
                ShowSaveButton = ShowSaveButton,
                SaveRedirectPage = SaveRedirectPage,

                // These are only supported in mobile shell 2.1 and later.
                ShowAttendanceNotes,
                AttendanceNoteLabel
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the attendees.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="occurrence">The occurrence.</param>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        private List<Attendee> GetAttendees( RockContext rockContext, AttendanceOccurrence occurrence, Group group )
        {
            var attendedIds = new List<int>();

            if ( occurrence != null )
            {
                // Get the list of people who attended
                attendedIds = new AttendanceService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        a.OccurrenceId == occurrence.Id &&
                        a.DidAttend.HasValue &&
                        a.DidAttend.Value &&
                        a.PersonAlias != null )
                    .Select( a => a.PersonAlias.PersonId )
                    .Distinct()
                    .ToList();
            }

            // Filtering out pending members.
            if ( !ShowPendingMembers )
            {
                group.Members = group.Members.Where( m => m.GroupMemberStatus != GroupMemberStatus.Pending ).ToList();
            }

            // Filtering out inactive members.
            if ( !ShowInactiveMembers )
            {
                group.Members = group.Members.Where( m => m.GroupMemberStatus != GroupMemberStatus.Inactive ).ToList();
            }

            return group.Members
                .Select( a => new Attendee
                {
                    PersonGuid = a.Person.Guid,
                    Attended = attendedIds.Contains( a.PersonId ),
                    Name = a.Person.FullName
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the header XAML for the group.
        /// </summary>
        /// <returns>A string containing the XAML content to be displayed.</returns>
        private string GetHeader( Group group )
        {
            return $@"<StackLayout Orientation=""Horizontal"" Spacing=""20"">
    <StackLayout Spacing=""0"" HorizontalOptions=""FillAndExpand"">
        <Label Text=""{group.Name.EncodeXml( true )} Attendance"" StyleClass=""h2, title1, bold, text-interface-strongest"" />
        <Label Text=""{group.Members.Count} members"" StyleClass=""body, text-interface-strong"" />
    </StackLayout>
    <Rock:Icon VerticalOptions=""Center"" IconClass=""user-check"" FontSize=""28"" StyleClass=""text-interface-strongest"" />
</StackLayout>";
        }

        /// <summary>
        /// Gets the occurrence items.
        /// </summary>
        private AttendanceOccurrence GetOccurrence( RockContext rockContext, Group group, DateTime date, bool allowAdd )
        {
            var occurrenceService = new AttendanceOccurrenceService( rockContext );

            var occurrence = occurrenceService.Get( date.Date, group.Id, null, group.ScheduleId );

            // If an occurrence date was included, but no occurrence was found with that date, and new 
            // occurrences can be added, create a new one
            if ( occurrence == null && allowAdd )
            {
                // Create a new occurrence record and return it
                occurrence = new AttendanceOccurrence
                {
                    Group = group,
                    GroupId = group.Id,
                    OccurrenceDate = date.Date,
                    LocationId = null,
                    ScheduleId = group.ScheduleId,
                };

                occurrenceService.Add( occurrence );

                return occurrence;
            }

            return occurrence;
        }

        /// <summary>
        /// Gets the valid dates for the group.
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        private List<DateTime> GetValidDates( Group group )
        {
            if ( group.Schedule == null )
            {
                return null;
            }

            if ( group.Schedule.ScheduleType == ScheduleType.Weekly )
            {
                var dates = new List<DateTime>();
                var startDate = RockDateTime.Now.Date.AddDays( -NumberOfDaysBackToAllow );
                var endDate = RockDateTime.Now.Date.AddDays( NumberOfDaysForwardToAllow );

                for ( var date = startDate; date <= endDate; date = date.AddDays( 1 ) )
                {
                    if ( date.DayOfWeek == group.Schedule.WeeklyDayOfWeek )
                    {
                        dates.Add( date );
                    }
                }

                return dates;
            }

            return group.Schedule
                .GetScheduledStartTimes( RockDateTime.Now.AddDays( -NumberOfDaysBackToAllow ), RockDateTime.Now.AddDays( NumberOfDaysForwardToAllow ) )
                .Select( d => d.Date ) // Strip off time portion
                .ToList();
        }

        /// <summary>
        /// Saves the attendance data.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="group">The group.</param>
        /// <param name="date">The date.</param>
        /// <param name="attendees">The attendees.</param>
        /// <param name="didNotMeet">if set to <c>true</c> then the group is marked as having not met.</param>
        /// <param name="notes">Contains the note text to save with the occurrence.</param>
        private void SaveAttendanceData( RockContext rockContext, Group group, DateTime date, List<Attendee> attendees, bool didNotMeet, string notes )
        {
            var attendanceService = new AttendanceService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );

            var occurrence = GetOccurrence( rockContext, group, date, true );
            var existingAttendees = occurrence.Attendees.ToList();

            if ( didNotMeet )
            {
                if ( !occurrence.ScheduleId.HasValue )
                {
                    //
                    // If the attendance is not part of a schedule, just delete all the records.
                    //
                    foreach ( var attendance in existingAttendees )
                    {
                        attendanceService.Delete( attendance );
                    }
                }
                else
                {
                    //
                    // If the occurrence is based on a schedule, clear the DidAttend property.
                    //
                    foreach ( var attendance in existingAttendees )
                    {
                        attendance.DidAttend = null;
                    }
                }
            }
            else
            {
                foreach ( var attendee in attendees )
                {
                    var attendance = existingAttendees
                        .Where( a => a.PersonAlias.Person.Guid == attendee.PersonGuid )
                        .FirstOrDefault();

                    if ( attendance == null )
                    {
                        int? personAliasId = personAliasService.GetPrimaryAliasId( attendee.PersonGuid );
                        if ( personAliasId.HasValue )
                        {
                            attendance = new Attendance
                            {
                                PersonAliasId = personAliasId,
                                CampusId = null,
                                StartDateTime = occurrence.Schedule != null && occurrence.Schedule.HasSchedule() ? occurrence.OccurrenceDate.Date.Add( occurrence.Schedule.StartTimeOfDay ) : occurrence.OccurrenceDate,
                                DidAttend = attendee.Attended
                            };

                            occurrence.Attendees.Add( attendance );
                        }
                    }
                    else
                    {
                        // Otherwise, only record that they attended -- don't change their attendance startDateTime 
                        attendance.DidAttend = attendee.Attended;
                    }
                }
            }

            occurrence.DidNotOccur = didNotMeet;

            // Only update the occurrence Notes if 'notes' is not null.
            occurrence.Notes = notes ?? occurrence.Notes;

            rockContext.SaveChanges();
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the current configuration for this block.
        /// </summary>
        /// <returns>A collection of string/string pairs.</returns>
        [BlockAction]
        public BlockActionResult GetGroupData( Guid groupGuid, DateTimeOffset? date = null )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Get( groupGuid );

                if ( group == null )
                {
                    return ActionNotFound();
                }

                // Special case, we are dealing with absolute dates so don't
                // convert to organization date time.
                var absDate = date?.Date;

                var dates = GetValidDates( group );

                if ( !AllowAnyDateSelection && dates != null )
                {
                    if ( !date.HasValue || !dates.Contains( absDate.Value ) )
                    {
                        absDate = dates.Where( a => a <= RockDateTime.Now.Date ).LastOrDefault();
                    }
                }

                if ( !date.HasValue )
                {
                    if ( dates != null )
                    {
                        absDate = dates.Where( a => a <= RockDateTime.Now.Date ).LastOrDefault();
                    }

                    absDate = absDate ?? RockDateTime.Now.Date;
                }

                var occurrence = GetOccurrence( rockContext, group, absDate ?? DateTime.MinValue, false );
                var rockDates = dates
                    .Select( d => d.ToRockDateTimeOffset() )
                    .ToList();

                return ActionOk( new
                {
                    Header = GetHeader( group ),
                    Date = absDate.Value,
                    Dates = rockDates,
                    DidNotMeet = occurrence?.DidNotOccur ?? false,
                    Attendees = GetAttendees( rockContext, occurrence, group ),
                    Notes = occurrence?.Notes ?? string.Empty
                } );
            }
        }

        /// <summary>
        /// Saves a single attendance record.
        /// </summary>
        /// <param name="groupGuid">The group identifier.</param>
        /// <param name="date">The date.</param>
        /// <param name="personGuid">The person identifier.</param>
        /// <param name="attended">if set to <c>true</c> the person is marked as attended.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SaveSingleAttendance( Guid groupGuid, DateTimeOffset date, Guid personGuid, bool attended )
        {
            return SaveAttendance( groupGuid, date, new List<Attendee>
            {
                new Attendee
                {
                    PersonGuid = personGuid,
                    Attended = attended
                }
            }, false );
        }

        /// <summary>
        /// Saves the attendance.
        /// </summary>
        /// <param name="groupGuid">The group identifier.</param>
        /// <param name="date">The date.</param>
        /// <param name="attendees">The attendees.</param>
        /// <param name="didNotMeet">if set to <c>true</c> the group did not meet.</param>
        /// <param name="notes">Contains the notes to be saved with the occurrence.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SaveAttendance( Guid groupGuid, DateTimeOffset date, List<Attendee> attendees, bool didNotMeet, string notes = null )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Get( groupGuid );

                if ( group == null )
                {
                    return ActionNotFound();
                }

                // Special case, we are dealing with absolute dates so don't
                // convert to organization date time.
                var absDate = date.Date;

                var dates = GetValidDates( group );
                if ( !AllowAnyDateSelection && dates != null && !dates.Contains( absDate ) )
                {
                    return ActionNotFound();
                }

                SaveAttendanceData( rockContext, group, absDate, attendees, didNotMeet, notes );

                return ActionOk();
            }
        }

        /// <summary>
        /// Marks the attendance for the group as having met or not met. This is only used
        /// in the "no save button" mode.
        /// </summary>
        /// <param name="groupGuid">The group identifier.</param>
        /// <param name="date">The date.</param>
        /// <param name="didNotMeet">if set to <c>true</c> then the group did not meet.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult DidNotMeet( Guid groupGuid, DateTimeOffset date, bool didNotMeet )
        {
            using ( var rockContext = new RockContext() )
            {
                var attendanceService = new AttendanceService( rockContext );
                var group = new GroupService( rockContext ).Get( groupGuid );

                if ( group == null )
                {
                    return ActionNotFound();
                }

                // Special case, we are dealing with absolute dates so don't
                // convert to organization date time.
                var absDate = date.Date;

                var dates = GetValidDates( group );
                if ( !AllowAnyDateSelection && dates != null && !dates.Contains( absDate ) )
                {
                    return ActionNotFound();
                }

                SaveAttendanceData( rockContext, group, absDate, new List<Attendee>(), didNotMeet, null );

                return GetGroupData( groupGuid, absDate );
            }
        }

        #endregion
    }
}