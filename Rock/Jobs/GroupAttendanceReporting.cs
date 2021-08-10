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
using System.Linq;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisplayName( "Group Attendance Reporting" )]
    [Description( @"Helps with the reporting of attendance within a defined set of groups at the person level.
The data view provides a list of groups to consider (the data view must return groups). A set of attribute values
will be created and set for each person who is a attendee of the groups for the metrics tracked/selected below." )]

    [DataViewField( "Group Data View",
        Description = "This groups data view determines which groups should be considered for the reporting.",
        EntityType = typeof( Rock.Model.Group ),
        Key = AttributeKey.GroupDataView,
        IsRequired = true,
        Order = 1 )]

    [TextField( "Reporting Label",
        Description = @"This text represents what the selected groups represent (e.g 'Serving Groups'). This will
be used in the naming of the required Attributes (e.g. 'Serving Groups - First Attended Date').",
        Key = AttributeKey.ReportingLabel,
        IsRequired = true,
        Order = 2 )]

    [CustomCheckboxListField( "Tracked Values",
        Description = @"Determines what should be tracked for each attendee. Note that tracking 'Times Attended'
values adds more processing time and effort to the job.",
        ListSource = @"
FirstAttendedDate^First Attended Date,
LastAttendedDate^Last Attended Date,
TimesAttendedInLast12Months^Times Attended in Last 12 Months,
TimesAttendedInLast16Weeks^Times Attended in Last 16 Weeks",
        Key = AttributeKey.TrackedValues,
        RepeatDirection = System.Web.UI.WebControls.RepeatDirection.Vertical,
        Order = 3 )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeoutSeconds,
        Description = "Maximum amount of time (in seconds) to wait for the SQL operations to complete. Leave blank to use the default for this job (180).",
        IsRequired = false,
        DefaultIntegerValue = 60 * 3,
        Category = "General",
        Order = 7 )]

    [DisallowConcurrentExecution]
    public class GroupAttendanceReporting : IJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string GroupDataView = "GroupDataView";
            public const string ReportingLabel = "ReportingLabel";
            public const string TrackedValues = "TrackedValues";
            public const string CommandTimeoutSeconds = "CommandTimeoutSeconds";
        }

        private static class TrackedValueKey
        {
            public const string FirstAttendedDate = "FirstAttendedDate";
            public const string LastAttendedDate = "LastAttendedDate";
            public const string TimesAttendedInLast12Months = "TimesAttendedInLast12Months";
            public const string TimesAttendedInLast16Weeks = "TimesAttendedInLast16Weeks";
        }

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GroupAttendanceReporting()
        {
            //
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var groupDataViewGuid = dataMap.GetString( AttributeKey.GroupDataView ).AsGuidOrNull();
            var reportingLabel = dataMap.GetString( AttributeKey.ReportingLabel );
            var trackedValues = dataMap.GetString( AttributeKey.TrackedValues ).SplitDelimitedValues();
            var commandTimeoutSeconds = dataMap.GetString( AttributeKey.CommandTimeoutSeconds ).AsIntegerOrNull() ?? 180;

            if ( !groupDataViewGuid.HasValue )
            {
                context.UpdateLastStatusMessage( "No Group Data View defined." );
                return;
            }

            if ( reportingLabel.IsNullOrWhiteSpace() )
            {
                context.UpdateLastStatusMessage( "No Reporting Label defined." );
                return;
            }

            if ( trackedValues.Length == 0 )
            {
                context.UpdateLastStatusMessage( "No Tracked Values defined." );
                return;
            }

            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = commandTimeoutSeconds;
            var groupDataView = new DataViewService( rockContext ).Get( groupDataViewGuid.Value );
            if ( groupDataView == null )
            {
                context.UpdateLastStatusMessage( "No Group Data View defined." );
                return;
            }

            var groupsQuery = groupDataView.GetQuery( new DataViewGetQueryArgs { DbContext = rockContext, DatabaseTimeoutSeconds = commandTimeoutSeconds } ) as IQueryable<Group>;
            var groupIds = groupsQuery.Select( a => a.Id ).ToList();

            // limit attendances to groups returned from the DataView
            var attendanceQuery = new AttendanceService( rockContext ).Queryable()
                .Where( a =>
                    a.PersonAliasId.HasValue
                    && a.Occurrence.GroupId.HasValue
                    && groupIds.Contains( a.Occurrence.GroupId.Value )
                    && a.DidAttend == true );

            // store updated attributes in a dictionary, and then we'll take care of updating person attributes in one sweep
            Dictionary<int, Dictionary<string, string>> attributeValuesByPersonId = new Dictionary<int, Dictionary<string, string>>();

            if ( trackedValues.Contains( TrackedValueKey.FirstAttendedDate ) )
            {
                context.UpdateLastStatusMessage( $"Calculating First Attended Date" );
                UpdatePersonsFirstAttendedDate( attributeValuesByPersonId, reportingLabel, attendanceQuery );
            }

            if ( trackedValues.Contains( TrackedValueKey.LastAttendedDate ) )
            {
                context.UpdateLastStatusMessage( $"Calculating Last Attended Date" );
                UpdatePersonsLastAttendedDate( attributeValuesByPersonId, reportingLabel, attendanceQuery );
            }

            if ( trackedValues.Contains( TrackedValueKey.TimesAttendedInLast12Months ) )
            {
                context.UpdateLastStatusMessage( $"Calculating Times Attended In Last 12 Months" );
                var timesAttendedLast12MonthsAttributeKey = $"_core_GroupAttendanceReporting_{reportingLabel.RemoveSpecialCharacters()}-{TrackedValueKey.TimesAttendedInLast12Months}";
                EnsureTrackedAttributeExists( timesAttendedLast12MonthsAttributeKey, $"{reportingLabel} - Times Attended in Last 12 Months", Rock.SystemGuid.FieldType.INTEGER.AsGuid() );

                var startDateTime = RockDateTime.Now.AddMonths( -12 );
                UpdatePersonsTimesAttended( attributeValuesByPersonId, attendanceQuery, timesAttendedLast12MonthsAttributeKey, startDateTime );
            }

            if ( trackedValues.Contains( TrackedValueKey.TimesAttendedInLast16Weeks ) )
            {
                context.UpdateLastStatusMessage( $"Calculating Times Attended In Last 16 Weeks" );
                var timesAttendedInLast16WeeksAttributeKey = $"_core_GroupAttendanceReporting_{reportingLabel.RemoveSpecialCharacters()}-{TrackedValueKey.TimesAttendedInLast16Weeks}";
                EnsureTrackedAttributeExists( timesAttendedInLast16WeeksAttributeKey, $"{reportingLabel} - Times Attended in Last 16 Weeks", Rock.SystemGuid.FieldType.INTEGER.AsGuid() );

                var startDateTime = RockDateTime.Now.AddDays( -16 * 7 );
                UpdatePersonsTimesAttended( attributeValuesByPersonId, attendanceQuery, timesAttendedInLast16WeeksAttributeKey, startDateTime );
            }

            int progress = 0;
            int updatedTrackedValueCount = 0;
            int updatedPersonCount = 0;
            int total = attributeValuesByPersonId.Count();

            foreach ( var personAttributeValues in attributeValuesByPersonId.OrderBy( a => a.Key ) )
            {
                var personId = personAttributeValues.Key;
                var attributeValues = personAttributeValues.Value;
                Person person;
                using ( var updateAttributesContext = new RockContext() )
                {
                    var personService = new PersonService( updateAttributesContext );
                    person = personService.Get( personId );
                    person.LoadAttributes( updateAttributesContext );

                    bool changesMade = false;
                    foreach ( var attributeValue in attributeValues )
                    {
                        string existingValue = person.GetAttributeValue( attributeValue.Key );
                        string newValue = attributeValue.Value;
                        if ( newValue == existingValue )
                        {
                            continue;
                        }

                        updatedTrackedValueCount++;
                        person.SetAttributeValue( attributeValue.Key, newValue );
                        changesMade = true;
                    }

                    if ( changesMade )
                    {
                        updatedPersonCount++;
                        person.SaveAttributeValues( updateAttributesContext );
                    }

                    progress++;
                    if ( progress % 1000 == 0 )
                    {
                        var percentProgress = Math.Round( 100 * progress / ( double ) total );
                        context.UpdateLastStatusMessage( $"Updating Person Attributes: {percentProgress}%" );
                    }
                }
            }

            context.UpdateLastStatusMessage( $"Updated attendance values for { updatedPersonCount } people." );
        }

        /// <summary>
        /// Updates each persons first attended date.
        /// </summary>
        /// <param name="attributeValuesByPersonId">The attribute values by person identifier.</param>
        /// <param name="reportingLabel">The reporting label.</param>
        /// <param name="attendanceQuery">The attendance query.</param>
        private void UpdatePersonsFirstAttendedDate( Dictionary<int, Dictionary<string, string>> attributeValuesByPersonId, string reportingLabel, IQueryable<Attendance> attendanceQuery )
        {
            var attendancesByPerson = attendanceQuery.GroupBy( a => a.PersonAlias.PersonId );

            var firstAttendedDateAttributeKey = $"_core_GroupAttendanceReporting_{reportingLabel.RemoveSpecialCharacters()}-{TrackedValueKey.FirstAttendedDate}";
            EnsureTrackedAttributeExists( firstAttendedDateAttributeKey, $"{reportingLabel} - First Attended Date", Rock.SystemGuid.FieldType.DATE.AsGuid() );

            var firstAttendanceByPersonList = attendancesByPerson.Select( s => new
            {
                PersonId = s.Key,
                FirstAttendedDateTime = s.Min( x => x.StartDateTime )
            } ).ToList();

            foreach ( var firstAttendanceByPerson in firstAttendanceByPersonList )
            {
                var personAttributeValues = attributeValuesByPersonId.GetValueOrNull( firstAttendanceByPerson.PersonId );
                if ( personAttributeValues == null )
                {
                    personAttributeValues = new Dictionary<string, string>();
                    attributeValuesByPersonId.AddOrReplace( firstAttendanceByPerson.PersonId, personAttributeValues );
                }

                personAttributeValues.AddOrReplace( firstAttendedDateAttributeKey, firstAttendanceByPerson.FirstAttendedDateTime.ToISO8601DateString() );
            }
        }

        /// <summary>
        /// Updates each persons last attended date.
        /// </summary>
        /// <param name="attributeValuesByPersonId">The attribute values by person identifier.</param>
        /// <param name="reportingLabel">The reporting label.</param>
        /// <param name="attendanceQuery">The attendance query.</param>
        private void UpdatePersonsLastAttendedDate( Dictionary<int, Dictionary<string, string>> attributeValuesByPersonId, string reportingLabel, IQueryable<Attendance> attendanceQuery )
        {
            var attendancesByPerson = attendanceQuery.GroupBy( a => a.PersonAlias.PersonId );
            var lastAttendedDateAttributeKey = $"_core_GroupAttendanceReporting_{reportingLabel.RemoveSpecialCharacters()}-{TrackedValueKey.LastAttendedDate}";
            EnsureTrackedAttributeExists( lastAttendedDateAttributeKey, $"{reportingLabel} - Last Attended Date", Rock.SystemGuid.FieldType.DATE.AsGuid() );

            var lastAttendanceByPersonList = attendancesByPerson.Select( s => new
            {
                PersonId = s.Key,
                LastAttendedDateTime = s.Max( x => x.StartDateTime )
            } ).ToList();

            foreach ( var lastAttendanceByPerson in lastAttendanceByPersonList )
            {
                var personAttributeValues = attributeValuesByPersonId.GetValueOrNull( lastAttendanceByPerson.PersonId );
                if ( personAttributeValues == null )
                {
                    personAttributeValues = new Dictionary<string, string>();
                    attributeValuesByPersonId.AddOrReplace( lastAttendanceByPerson.PersonId, personAttributeValues );
                }

                personAttributeValues.AddOrReplace( lastAttendedDateAttributeKey, lastAttendanceByPerson.LastAttendedDateTime.ToISO8601DateString() );
            }
        }

        /// <summary>
        /// Updates the persons times attended.
        /// </summary>
        /// <param name="attributeValuesByPersonId">The attribute values by person identifier.</param>
        /// <param name="attendanceQuery">The attendance query.</param>
        /// <param name="timesAttendedAttributeKey">The times attended attribute key.</param>
        /// <param name="startDate">The start date.</param>
        private void UpdatePersonsTimesAttended( Dictionary<int, Dictionary<string, string>> attributeValuesByPersonId, IQueryable<Attendance> attendanceQuery, string timesAttendedAttributeKey, DateTime startDate )
        {
            var attendancesByPerson = attendanceQuery.Where( a => a.StartDateTime >= startDate ).GroupBy( a => a.PersonAlias.PersonId );

            var timesAttendedByPersonList = attendancesByPerson.Select( s => new
            {
                PersonId = s.Key,
                TimesAttended = s.Count()
            } ).ToList();

            foreach ( var timesAttendedByPerson in timesAttendedByPersonList )
            {
                var personAttributeValues = attributeValuesByPersonId.GetValueOrNull( timesAttendedByPerson.PersonId );
                if ( personAttributeValues == null )
                {
                    personAttributeValues = new Dictionary<string, string>();
                    attributeValuesByPersonId.AddOrReplace( timesAttendedByPerson.PersonId, personAttributeValues );
                }

                personAttributeValues.AddOrReplace( timesAttendedAttributeKey, timesAttendedByPerson.TimesAttended.ToString() );
            }
        }

        /// <summary>
        /// Ensures the tracked attributes exists.
        /// </summary>
        /// <param name="trackedAttributeKey">The tracked attribute key.</param>
        /// <param name="trackedAttributeName">Name of the tracked attribute.</param>
        /// <param name="fieldTypeGuid">The field type unique identifier.</param>
        private void EnsureTrackedAttributeExists( string trackedAttributeKey, string trackedAttributeName, Guid fieldTypeGuid )
        {
            var entityTypeIdPerson = EntityTypeCache.GetId<Rock.Model.Person>();
            var fieldTypeId = FieldTypeCache.GetId( fieldTypeGuid );

            if ( !entityTypeIdPerson.HasValue || !fieldTypeId.HasValue )
            {
                // shouldn't happen, but just in case
                return;
            }

            var rockContext = new RockContext();

            var attributeService = new AttributeService( rockContext );

            var trackedAttribute = attributeService.GetByEntityTypeId( entityTypeIdPerson.Value ).Where( a => a.Key == trackedAttributeKey ).FirstOrDefault();
            if ( trackedAttribute == null )
            {
                trackedAttribute = new Rock.Model.Attribute();
                trackedAttribute.EntityTypeId = entityTypeIdPerson;
                trackedAttribute.Key = trackedAttributeKey;
                trackedAttribute.Name = trackedAttributeName;
                trackedAttribute.FieldTypeId = fieldTypeId.Value;
                attributeService.Add( trackedAttribute );
                rockContext.SaveChanges();
            }
            else
            {
                if ( trackedAttribute.FieldTypeId != fieldTypeId.Value )
                {
                    trackedAttribute.FieldTypeId = fieldTypeId.Value;
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
