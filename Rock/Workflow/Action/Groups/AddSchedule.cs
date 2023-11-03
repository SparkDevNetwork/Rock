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
using System.ComponentModel.Composition;
using System.Linq;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action.Groups
{
    /// <summary>
    /// Adds a new schedule.
    /// </summary>
    [ActionCategory( "Groups" )]
    [Description( "Adds a new schedule." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Schedule Add" )]

    #region Attributes

    [WorkflowTextOrAttribute( "Name",
        "Attribute Value",
        Description = @"The name for the schedule. If ""Weekly:"" options are provided, Name is ignored. <span class='tip tip-lava'></span>",
        Key = AttributeKey.Name,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType" },
        Order = 0 )]

    [WorkflowTextOrAttribute( "Abbreviated Name",
        "Attribute Value",
        Description = @"Shorter name for the schedule. If no value is provided the name will be used. If ""Weekly:"" options are provided, Abbreviated Name is ignored. <span class='tip tip-lava'></span>",
        Key = AttributeKey.AbbreviatedName,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType" },
        Order = 1 )]

    [WorkflowTextOrAttribute( "Category",
        "Attribute Value",
        Description = @"The category to place the schedule in. If ""Weekly:"" options are provided -OR- Name is not provided, Category is ignored. <span class='tip tip-lava'></span>",
        Key = AttributeKey.Category,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.CategoryFieldType" },
        Order = 2 )]

    [WorkflowTextOrAttribute( "Is Public",
        "Attribute Value",
        Description = "Determines if the schedule should be marked to display publicly.",
        Key = AttributeKey.IsPublic,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.BooleanFieldType" },
        Order = 3 )]

    [WorkflowTextOrAttribute( "Start Date/Time",
        "Attribute Value",
        Description = @"The date and time that the schedule starts. If ""Weekly:"" options are provided, Start Date/Time is ignored. <span class='tip tip-lava'></span>",
        Key = AttributeKey.StartDateTime,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.DateTimeFieldType" },
        Order = 4 )]

    [WorkflowTextOrAttribute( "Duration (mins)",
        "Attribute Value",
        Description = @"The duration of the schedule (in minutes). If ""Weekly:"" options are provided, Duration is ignored.",
        Key = AttributeKey.Duration,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.IntegerFieldType", "Rock.Field.Types.TextFieldType" },
        Order = 5 )]

    [WorkflowTextOrAttribute( "Weekly: Day of Week",
        "Attribute Value",
        Description = @"The day of the week when creating a weekly schedule. 0 = Sunday, 6 = Saturday. If ""Weekly: Time of Day"" is supplied then ""Weekly: Day of Week"" is required.",
        Key = AttributeKey.WeeklyDayOfWeek,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.IntegerFieldType", "Rock.Field.Types.DayOfWeekFieldType" },
        Order = 6 )]

    [WorkflowTextOrAttribute( "Weekly: Time of Day",
        "Attribute Value",
        Description = @"The time of day when creating a weekly schedule. If ""Weekly: Day of Week"" is supplied then ""Weekly: Time of Day"" is required.",
        Key = AttributeKey.WeeklyTimeOfDay,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TimeFieldType" },
        Order = 7 )]

    [WorkflowAttribute( "Schedule Attribute",
        Description = "The workflow attribute to store the newly created schedule into.",
        Key = AttributeKey.ScheduleAttribute,
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.ScheduleFieldType" },
        Order = 8 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "A2EB81CA-8897-406B-B208-CB9BE8CB2BAD" )]
    public class AddSchedule : ActionComponent
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Name = "Name";
            public const string AbbreviatedName = "AbbreviatedName";
            public const string Category = "Category";
            public const string IsPublic = "IsPublic";
            public const string StartDateTime = "StartDateTime";
            public const string Duration = "Duration";
            public const string WeeklyDayOfWeek = "WeeklyDayOfWeek";
            public const string WeeklyTimeOfDay = "WeeklyTimeOfDay";
            public const string ScheduleAttribute = "ScheduleAttribute";
        }

        #endregion

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><see langword="true"/> if successful; <see langword="false"/> otherwise</returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var isPublic = GetAttributeValue( action, AttributeKey.IsPublic, true ).AsBoolean();

            // If "Weekly:" options are provided, ignore most other options.
            var dayOfWeekString = GetAttributeValue( action, AttributeKey.WeeklyDayOfWeek, true );
            var dayOfWeekInt = dayOfWeekString.AsIntegerOrNull() ?? -1;
            var dayOfWeekIsValid = Enum.IsDefined( typeof( DayOfWeek ), dayOfWeekInt );
            Enum.TryParse( dayOfWeekString, out DayOfWeek weeklyDayOfWeek );

            var weeklyTimeOfDay = GetAttributeValue( action, AttributeKey.WeeklyTimeOfDay, true ).AsTimeSpan();
            if ( dayOfWeekIsValid && weeklyTimeOfDay.HasValue )
            {
                SaveNewSchedule( new Schedule
                {
                    WeeklyDayOfWeek = weeklyDayOfWeek,
                    WeeklyTimeOfDay = weeklyTimeOfDay,
                    IsPublic = isPublic
                }, rockContext, action );

                return true;
            }
            else if ( dayOfWeekIsValid )
            {
                errorMessages.Add( @"""Weekly: Day of Week"" was parsed, but could not parse ""Weekly: Time of Day""." );
                return false;
            }
            else if ( weeklyTimeOfDay.HasValue )
            {
                errorMessages.Add( @"""Weekly: Time of Day"" was parsed, but could not parse ""Weekly: Day of Week""." );
                return false;
            }

            // Otherwise, this is an iCalendarContent schedule.
            // Start by getting the merge fields
            var mergeFields = GetMergeFields( action );

            // At the very least, we need a start date/time; if we don't have this, we can't create a schedule.
            var startDateTimeString = GetAttributeValue( action, AttributeKey.StartDateTime, true );
            var startDateTime = startDateTimeString.ResolveMergeFields( mergeFields ).AsDateTime();
            if ( !startDateTime.HasValue )
            {
                errorMessages.Add( $@"Could not parse ""Start Date/Time"": {startDateTimeString}" );
                return false;
            }

            // Get duration.
            var durationMinutes = GetAttributeValue( action, AttributeKey.Duration, true ).AsInteger();
            var duration = durationMinutes > 0
                ? new TimeSpan( 0, durationMinutes, 0 )
                : new TimeSpan( 0, 0, 1 ); // Make a one second duration since a zero duration won't be included in occurrences.

            // Build iCalendarContent.
            var calendarEvent = new CalendarEvent
            {
                DtStart = new CalDateTime( startDateTime.Value ),
                Duration = duration,
                DtStamp = new CalDateTime( RockDateTime.Now )
            };
            calendarEvent.DtStart.HasTime = true;

            var calendar = new Calendar();
            calendar.Events.Add( calendarEvent );

            var iCalendarSerializer = new CalendarSerializer( calendar );
            var iCalendarContent = iCalendarSerializer.SerializeToString();

            // Get the name and abbreviated name.
            var name = GetAttributeValue( action, AttributeKey.Name, true ).ResolveMergeFields( mergeFields );
            var abbreviatedName = GetAttributeValue( action, AttributeKey.AbbreviatedName, true ).ResolveMergeFields( mergeFields );

            // Only consider category if a schedule name was provided (categories are only relevant for named schedules).
            int? categoryId = null;
            var categoryString = GetAttributeValue( action, AttributeKey.Category, true ).ResolveMergeFields( mergeFields );
            if ( categoryString.IsNotNullOrWhiteSpace() && name.IsNotNullOrWhiteSpace() )
            {
                var scheduleEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.SCHEDULE ).GetValueOrDefault();

                var categoryGuid = categoryString.AsGuidOrNull();
                if ( categoryGuid.HasValue )
                {
                    // If the provided category is a guid, and doesn't represent an existing [Schedule] category, fail the action.
                    var categoryCache = CategoryCache.All()
                        .FirstOrDefault( c =>
                            c.EntityTypeId == scheduleEntityTypeId
                            && c.Guid == categoryGuid.Value
                        );

                    if ( categoryCache == null )
                    {
                        errorMessages.Add( $"Could not parse a valid schedule category from the value provided: {categoryString}" );
                        return false;
                    }

                    categoryId = categoryCache.Id;

                    action.AddLogEntry( $@"Found existing ""{categoryCache.Name}"" category." );
                }
                else
                {
                    // Otherwise, search by name & create a new category if a match isn't found.
                    categoryId = CategoryCache.All()
                        .FirstOrDefault( c =>
                            c.EntityTypeId == scheduleEntityTypeId
                            & c.Name.Equals( categoryString, StringComparison.OrdinalIgnoreCase )
                        )?.Id;

                    if ( !categoryId.HasValue )
                    {
                        var categoryService = new CategoryService( rockContext );
                        var category = new Category
                        {
                            EntityTypeId = scheduleEntityTypeId,
                            Name = categoryString
                        };

                        categoryService.Add( category );

                        rockContext.SaveChanges();

                        categoryId = category.Id;

                        action.AddLogEntry( $@"Added new ""{categoryString}"" category." );
                    }
                    else
                    {
                        action.AddLogEntry( $@"Found existing ""{categoryString}"" category." );
                    }
                }
            }

            SaveNewSchedule( new Schedule
            {
                Name = name,
                AbbreviatedName = abbreviatedName,
                iCalendarContent = iCalendarContent,
                CategoryId = categoryId,
                IsPublic = isPublic
            }, rockContext, action );

            return true;
        }

        /// <summary>
        /// Saves the provided schedule and sets the schedule attribute if specified.
        /// </summary>
        /// <param name="schedule">The schedule to save.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        private void SaveNewSchedule( Schedule schedule, RockContext rockContext, WorkflowAction action )
        {
            new ScheduleService( rockContext ).Add( schedule );

            rockContext.SaveChanges();

            SetWorkflowAttributeValue( action, AttributeKey.ScheduleAttribute, schedule.Guid );

            action.AddLogEntry( $@"Added new ""{schedule.ScheduleType:G}"" schedule{( schedule.Name.IsNotNullOrWhiteSpace() ? $@" (""{schedule.Name}"")" : string.Empty )}: {schedule.ToFriendlyScheduleText()}" );
        }
    }
}
