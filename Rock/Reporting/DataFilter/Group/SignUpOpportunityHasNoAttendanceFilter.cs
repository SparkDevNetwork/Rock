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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

using static Rock.Web.UI.Controls.SlidingDateRangePicker;

namespace Rock.Reporting.DataFilter.Group
{
    /// <summary>
    /// A Data Filter to select Groups that have at least one opportunity (GroupLocationScheduleConfig) with at least one non-leader member, with no corresponding non-leader, [DidAttend] == true attendance records.
    /// </summary>
    [Description( "Lists any groups (projects) that have at least one opportunity (GroupLocationScheduleConfig) with at least one non-leader member, with no corresponding non-leader, [DidAttend] == true attendance records." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Sign-Up Opportunity Has No Attendance" )]
    [Rock.SystemGuid.EntityTypeGuid( "3700E8B9-B259-42A4-A6D1-EE122E3DAA0A" )]
    public class SignUpOpportunityHasNoAttendanceFilter : DataFilterComponent
    {
        #region Settings

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection.
        /// </summary>
        private class SelectionConfig
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionConfig"/> class.
            /// </summary>
            public SelectionConfig()
            {
                // Add values to set defaults / populate upon object creation.
            }

            /// <summary>
            /// The selected <see cref="Rock.Model.GroupType"/> identifier.
            /// </summary>
            public string GroupTypeGuid { get; set; }

            /// <summary>
            /// The project type <see cref="Rock.Model.DefinedValue"/>s to include.
            /// </summary>
            public List<string> IncludeProjectTypes { get; set; } = new List<string>();

            /// <summary>
            /// The pipe-delimited SlidingDateRangeMode, NumberOfTimeUnits, and TimeUnit values an event's date should be within.
            /// </summary>
            public string EventDateWithin { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON string.
            /// </summary>
            /// <param name="selection">The filter selection control.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                return selection.FromJsonOrNull<SelectionConfig>();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the entity type that the filter applies to.
        /// </summary>
        /// <value>
        /// The namespace-qualified Type name of the entity that the filter applies to, or an empty string if the filter applies to all entities.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Model.Group ).FullName; }
        }

        /// <summary>
        /// Gets the name of the section in which the filter should be displayed in a browsable list.
        /// </summary>
        /// <value>
        /// The section name.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        /// <summary>
        /// Gets the Sign-Up Group GroupType identifier.
        /// </summary>
        /// <value>
        /// The Sign-Up Group GroupType identifier.
        /// </value>
        private int SignUpGroupGroupTypeId
        {
            get { return GroupTypeCache.GetId( Rock.SystemGuid.GroupType.GROUPTYPE_SIGNUP_GROUP.AsGuid() ).ToIntSafe(); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the user-friendly title used to identify the filter component.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The name of the filter.
        /// </returns>
        public override string GetTitle( Type entityType )
        {
            return "Sign-Up Opportunity Has No Attendance";
        }

        /// <summary>
        /// Creates the model representation of the child controls used to display and edit the filter settings.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the filter controls.</param>
        /// <returns>
        /// The array of new controls created to implement the filter.
        /// </returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            // Define control: GroupType picker.
            var groupTypePicker = new GroupTypePicker();
            groupTypePicker.ID = filterControl.GetChildControlInstanceName( "_groupTypePicker" );
            groupTypePicker.Label = "Group Type";
            groupTypePicker.AddCssClass( "js-group-type-picker" );
            groupTypePicker.UseGuidAsValue = true;
            groupTypePicker.Required = true;
            using ( var rockContext = new RockContext() )
            {
                groupTypePicker.GroupTypes = new GroupTypeService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( gt => gt.Id == this.SignUpGroupGroupTypeId || gt.InheritedGroupTypeId == this.SignUpGroupGroupTypeId )
                    .ToList();
            }
            filterControl.Controls.Add( groupTypePicker );

            // Define control: include project types checkbox list.
            var cblIncludeProjectTypes = new RockCheckBoxList();
            cblIncludeProjectTypes.ID = filterControl.GetChildControlInstanceName( "_cblIncludeProjectTypes" );
            cblIncludeProjectTypes.Label = "Include Project Types";
            cblIncludeProjectTypes.RepeatDirection = RepeatDirection.Horizontal;
            cblIncludeProjectTypes.AddCssClass( "js-cbl-include-project-types" );
            cblIncludeProjectTypes.Items.Add( new ListItem( "In-Person", Rock.SystemGuid.DefinedValue.PROJECT_TYPE_IN_PERSON ) );
            cblIncludeProjectTypes.Items.Add( new ListItem( "Project Due", Rock.SystemGuid.DefinedValue.PROJECT_TYPE_PROJECT_DUE ) );
            filterControl.Controls.Add( cblIncludeProjectTypes );

            // Define controls: event date within sliding date range picker.
            var sdrpEventDateWithin = new SlidingDateRangePicker();
            sdrpEventDateWithin.ID = filterControl.GetChildControlInstanceName( "_sdrpEventDateWithin" );
            sdrpEventDateWithin.Label = "Event Date Within";
            sdrpEventDateWithin.Help = "The time frame the opportunity's schedule should be within.";
            sdrpEventDateWithin.EnabledSlidingDateRangeTypes = new[] { SlidingDateRangeType.Previous, SlidingDateRangeType.Upcoming };
            sdrpEventDateWithin.EnabledSlidingDateRangeUnits = new[] { TimeUnitType.Day, TimeUnitType.Week, TimeUnitType.Month };
            sdrpEventDateWithin.AddCssClass( "js-sdrp-event-date-within" );
            filterControl.Controls.Add( sdrpEventDateWithin );

            return new Control[] { groupTypePicker, cblIncludeProjectTypes, sdrpEventDateWithin };
        }

        /// <summary>
        /// Renders the child controls used to display and edit the filter settings for HTML presentation.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="filterControl">The control that serves as the container for the controls being rendered.</param>
        /// <param name="writer">The writer being used to generate the HTML for the output page.</param>
        /// <param name="controls">The model representation of the child controls for this component.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            // Get references to the controls we created in CreateChildControls.
            var groupTypePicker = controls[0] as GroupTypePicker;
            var cblIncludeProjectTypes = controls[1] as RockCheckBoxList;
            var sdrpEventDateWithin = controls[2] as SlidingDateRangePicker;

            writer.AddAttribute( "class", "row field-criteria" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Render GroupType picker control.
            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            groupTypePicker.RenderControl( writer );
            writer.RenderEndTag();

            // Render include project types checkbox list control.
            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            cblIncludeProjectTypes.RenderControl( writer );
            writer.RenderEndTag();

            // Render event date within sliding date range picker control.
            writer.AddAttribute( "class", "col-md-8" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            sdrpEventDateWithin.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Formats the selection on the client-side. When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property. If including script, the
        /// control's parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The client format script.
        /// </returns>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function () {
    var groupTypeName = $('.js-group-type-picker', $content).find(':selected').text();
    var checkedProjectTypes = $('.js-cbl-include-project-types', $content).find(':checked').closest('label');

    var eventDateWithin = $('.js-sdrp-event-date-within', $content);
    var dateRangeType = eventDateWithin.find('.slidingdaterange-select').find(':selected').text();
    var dateRangeNumber = eventDateWithin.find('.slidingdaterange-number').val();
    var dateRangeUnitsSingular = eventDateWithin.find('.slidingdaterange-timeunits-singular').find(':selected').text();
    var dateRangeUnitsPlural = eventDateWithin.find('.slidingdaterange-timeunits-plural').find(':selected').text();

    var result = groupTypeName + ' opportunity'

    if (checkedProjectTypes.length) {
        var projectTypeLabel = checkedProjectTypes.length > 1 ? 'project types' : 'project type';
        var projectCommaList = checkedProjectTypes.map(function () {
            return $(this).text()
        })
        .get().join(', ').trim();

        result += ', with ' + projectTypeLabel + ': ' + projectCommaList;
    }

    if (dateRangeType && dateRangeNumber && dateRangeUnitsSingular) {
        var dateRangeUnits = dateRangeNumber === '1' ? dateRangeUnitsSingular : dateRangeUnitsPlural;
        result += ', within the ' + dateRangeType + ' ' + dateRangeNumber + ' ' + dateRangeUnits;
    }

    result += ' has no attendance'
    return result;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns>
        /// The formatted selection.
        /// </returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            string groupTypeName = null;
            using ( var rockContext = new RockContext() )
            {
                var groupTypeGuid = selectionConfig.GroupTypeGuid.AsGuidOrNull();
                if ( groupTypeGuid.HasValue )
                {
                    groupTypeName = new GroupTypeService( rockContext )
                        .GetNoTracking( selectionConfig.GroupTypeGuid.AsGuid() )
                        ?.Name;
                }
            }

            string projectTypes = string.Empty;
            if ( selectionConfig.IncludeProjectTypes.Any() )
            {
                var projectTypeNames = new List<string>();
                foreach ( var projectTypeGuid in selectionConfig.IncludeProjectTypes )
                {
                    var definedValueCache = DefinedValueCache.Get( projectTypeGuid );
                    if ( string.IsNullOrWhiteSpace( definedValueCache?.Value ) )
                    {
                        continue;
                    }

                    projectTypeNames.Add( definedValueCache.Value );
                }

                projectTypes = $", with project {"type".PluralizeIf( projectTypeNames.Count > 1 )}: {string.Join( ", ", projectTypeNames )}";
            }

            string eventDateWithin = string.Empty;
            if ( selectionConfig.EventDateWithin.IsNotNullOrWhiteSpace() )
            {
                var parts = selectionConfig.EventDateWithin.SplitDelimitedValues();
                if ( parts.Count( p => p.IsNotNullOrWhiteSpace() ) >= 3 )
                {
                    int.TryParse( parts[1], out int dateRangeNumber );
                    eventDateWithin = $", within the {parts[0]} {parts[1]} {parts[2].PluralizeIf( Math.Abs( dateRangeNumber ) != 1 )}";
                }
            }

            return $"{groupTypeName} opportunity{projectTypes}{eventDateWithin} has no attendance";
        }

        /// <summary>
        /// Returns a JSON representation of selected values.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns>A JSON representation of selected values.</returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var groupTypePicker = controls[0] as GroupTypePicker;
            var cblIncludeProjectTypes = controls[1] as RockCheckBoxList;
            var sdrpEventDateWithin = controls[2] as SlidingDateRangePicker;

            var selectionConfig = new SelectionConfig
            {
                GroupTypeGuid = groupTypePicker.SelectedValue,
                IncludeProjectTypes = cblIncludeProjectTypes.SelectedValues,
                EventDateWithin = sdrpEventDateWithin.DelimitedValues
            };

            return selectionConfig.ToJson();
        }

        /// <summary>
        /// Sets the selection from a JSON string.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            var groupTypePicker = controls[0] as GroupTypePicker;
            var cblIncludeProjectTypes = controls[1] as RockCheckBoxList;
            var sdrpEventDateWithin = controls[2] as SlidingDateRangePicker;

            groupTypePicker.SetValue( selectionConfig.GroupTypeGuid );
            cblIncludeProjectTypes.SetValues( selectionConfig.IncludeProjectTypes );
            sdrpEventDateWithin.DelimitedValues = selectionConfig.EventDateWithin;
        }

        /// <summary>
        /// Creates a LINQ Expression that can be applied to an IQueryable to filter the result set.
        /// </summary>
        /// <param name="entityType">The type of entity in the result set.</param>
        /// <param name="serviceInstance">A service instance that can be queried to obtain the result set.</param>
        /// <param name="parameterExpression">The input parameter that will be injected into the filter expression.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A LINQ Expression that can be used to filter an IQueryable.
        /// </returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            var groupTypeGuid = selectionConfig.GroupTypeGuid.AsGuidOrNull();
            if ( !groupTypeGuid.HasValue )
            {
                return null;
            }

            var groupTypeId = GroupTypeCache.GetId( groupTypeGuid.Value );
            if ( !groupTypeId.HasValue )
            {
                return null;
            }

            var dateRange = CalculateDateRangeFromDelimitedValues( selectionConfig.EventDateWithin );
            var startDate = dateRange?.Start;
            var endDate = dateRange?.End;
            if ( endDate.HasValue )
            {
                /*
                 * We'll add a day since the above returns the selected end date @ 11:59.999PM.
                 * This way, we can use "less than" in our filtering below, to follow Rock's rule: let your start be "inclusive" and your end be "exclusive".
                 */
                endDate = endDate.Value.AddDays( 1 ).StartOfDay();
            }

            var attendanceOccurrencesQry = new AttendanceOccurrenceService( ( RockContext ) serviceInstance.Context )
                .Queryable()
                .Include( o => o.Attendees )
                .AsNoTracking();

            var opportunities = new GroupLocationService( ( RockContext ) serviceInstance.Context )
                .Queryable()
                .AsNoTracking()
                .Where( gl => gl.Group.GroupTypeId == groupTypeId.Value )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    gl.Group,
                    gl.LocationId,
                    Schedule = s
                } )
                /*
                 * We now have [GroupLocationSchedule] instances:
                 * One for each combination of Group, Location & Schedule, where [Group].[GroupTypeId] == selectionConfig's GroupTypeId.
                 * 
                 * Adding the following commented-out line to point out that it won't work for our purposes, because:
                 *  1) The Attribute we care about exists on the Group Entity, and not the GroupLocation Entity that our IQueryable is querying against.
                 *  2) According to our 202 guide, "Group attributes are the most complicated to load since they can inherit attributes from their parent
                 *     GroupType(s) and the [below] snippet wouldn't work if a Group inherited an attribute value from a GroupType." This is our exact
                 *     scenario: we can have GroupTypes that inherit from the default "Sign-Up Group" GroupType, and the Attribute we care about filtering
                 *     against - "ProjectType" - exists on the parent "Sign-Up Group" GroupType; we'll need to get all Groups up front, and filter out
                 *     the ones we don't care about below, in memory.
                 */
                //.WhereAttributeValue( rockContext, "ProjectType", Rock.SystemGuid.DefinedValue.PROJECT_TYPE_IN_PERSON )
                .Where( gls =>
                    !startDate.HasValue || !endDate.HasValue // Either the individual did not choose a date range, OR...
                    /*
                     * We need to perform phase 1 of Schedule filtering to minimize the number of records being returned by the query; we'll
                     * perform additional, in-memory Schedule filtering below, once we materialize the opportunities' Schedule instances.
                     * 
                     * Keep in mind that Schedules with a null EffectiveEndDate represent recurring schedules that have no end date.
                     * 
                     * This logic seems a little backwards at first, but the plan is to get opportunities whose Schedule's:
                     *  1) EffectiveStartDate < endDate (so we don't get any Schedules that start AFTER the specified date range), AND
                     *  2) EffectiveEndDate is null OR >= startDate (so we don't get any Schedules that have already ended BEFORE the specified date range).
                     * 
                     **********************************
                     * Consider the following example:
                     **********************************
                     * 
                     * Opportunity A (EffectiveStartDate: 01/01/2023, EffectiveEndDate: 01/29/2023, Recurrences: Once a week on Sundays, end after 5 occurrences)
                     * Opportunity B (EffectiveStartDate: 01/29/2023, EffectiveEndDate: null      , Recurrences: Once a month on the last Sunday, no end date)
                     * Opportunity C (EffectiveStartDate: 02/05/2023, EffectiveEndDate: 02/05/2023, Recurrences: none)
                     * Opportunity D (EffectiveStartDate: 02/05/2023, EffectiveEndDate: 02/26/2023, Recurrences: Once a week on Sundays, end after 4 occurrences)
                     * Opportunity E (EffectiveStartDate: 02/26/2023, EffectiveEndDate: null      , Recurrences: Once a month on the last Sunday, no end date)
                     * Opportunity F (EffectiveStartDate: 04/30/2023, EffectiveEndDate: 08/27/2023, Recurrences: Once a month on the last Sunday, end after 5 occurrences)
                     * 
                     * Today is 03/03/2023.
                     * Selected date range for this filter is "Previous 30 Days", which translates to "02/01/2023 12:00 AM to 03/03/2023 12:00 AM".
                     * In this example, we want opportunities B, C, D & E to be returned from the query.
                     * 
                     *                    ( DateRange )
                     *                    (           )
                     *                    (           )
                     *    |---Jan--------Feb--------Mar--------Apr--------May--------Jun--------Jul--------Aug--------Sep--------Oct-->>
                     *  A |    |x-x-x-x-x|(           )
                     *  B |            |x-(--------x--)-------x----------x----------x----------x----------x----------x----------x----->>
                     *  C |               ( |x|       )
                     *  D |               ( |x-x-x-x| )
                     *  E |               (       |x--)-------x----------x----------x----------x----------x----------x----------x----->>
                     *  F |               (           )                 |x----------x----------x----------x----------x|
                     *    |---------------(-----------)------------------------------------------------------------------------------->>
                     *                    (           )
                     */
                    || (
                        gls.Schedule.EffectiveStartDate.HasValue
                        && gls.Schedule.EffectiveStartDate.Value < endDate
                        && (
                            !gls.Schedule.EffectiveEndDate.HasValue
                            || gls.Schedule.EffectiveEndDate.Value >= startDate
                        )
                    )
                )
                /* 
                 * If indicated by selectionConfig.EventDateWithin, we've now filtered out any opportunities whose Schedule cannot have any occurrences
                 * within the specified date range. This doesn't yet guarantee all of the returned Schedules will have occurrences that match; we'll
                 * determine this with additional, in-memory filtering below.
                 * 
                 * If not, we have all opportunities (GroupLocationSchedules): past, present and future (this could be a big list).
                 */
                .Select( gls => new
                {
                    gls.Group,
                    gls.LocationId,
                    gls.Schedule,
                    Members = gls.Group.Members
                        .SelectMany( gm => gm.GroupMemberAssignments, ( gm, gma ) => new
                        {
                            GroupMember = gm,
                            Assignment = gma
                        } )
                        .Where( gmas =>
                            gmas.Assignment.LocationId == gls.LocationId
                            && gmas.Assignment.ScheduleId == gls.Schedule.Id
                        )
                        .Select( gmas => new
                        {
                            gmas.GroupMember,
                            PersonAliasIds = gmas.GroupMember.Person.Aliases.Select( a => a.Id ).ToList(),
                            gmas.GroupMember.GroupRole.IsLeader
                        } )
                        .ToList(),
                    AttendanceOccurrences = attendanceOccurrencesQry.Where( o =>
                        o.GroupId == gls.Group.Id
                        && o.LocationId == gls.LocationId
                        && o.ScheduleId == gls.Schedule.Id
                    )
                    .ToList()
                } )
                .ToList();

            /*
             * And lastly, we have:
             *  1) A collection of members (if any), as well as each member's list of PersonAlias IDs & whether they're a leader within this Group;
             *  2) A collection AttendanceOccurrences (if any), as well as each occurrence's Attendance records.
             * All grouped by opportunity and materialized in-memory.
             * 
             * We'll be able to perform the final filtering against this data.
             */

            // We need to have start and end dates to compare against below, so provide defaults if no date range was selected.
            startDate = startDate ?? DateTime.MinValue.Date;
            endDate = endDate ?? DateTime.MaxValue.Date;

            var groupsMissingAttendance = new List<Rock.Model.Group>();

            // Only consider opportunities that have at least one non-leader member.
            foreach ( var opportunity in opportunities.Where( o => o.Members.Any( m => !m.IsLeader ) ) )
            {
                if ( groupsMissingAttendance.Any( g => g.Id == opportunity.Group.Id ) )
                {
                    // No need to check this Group again, as it's already been included due to another Schedule's missing attendance data.
                    continue;
                }

                /*
                 * Narrow down the results to include only those Schedules that:
                 *  1) Actually have at least one occurrence within the selected date range AND
                 *  2a) Any of those occurrences don't have a corresponding AttendanceOccurrence record, OR
                 *  2b) Any of those occurrences don't have at least one non-leader, [DidAttend] == true Attendance record.
                 */
                var occurrenceDateTimes = opportunity.Schedule.GetScheduledStartTimes( startDate.Value, endDate.Value );
                if ( !occurrenceDateTimes.Any() )
                {
                    // This opportunity's Schedule doesn't have any occurrences that fall within the selected date range; move on to the next opportunity.
                    continue;
                }

                foreach ( var occurrenceDateTime in occurrenceDateTimes )
                {
                    var attendanceOccurrence = opportunity.AttendanceOccurrences.FirstOrDefault( ao => ao.OccurrenceDate == occurrenceDateTime.Date );
                    if ( attendanceOccurrence == null )
                    {
                        // This occurrence doesn't even have an AttendanceOccurrence record; no need to look any further within this opportunity.
                        groupsMissingAttendance.Add( opportunity.Group );
                        break;
                    }

                    var nonLeaderPersonAliasIds = opportunity.Members
                        .Where( m => !m.IsLeader )
                        .SelectMany( m => m.PersonAliasIds )
                        .ToList();

                    if ( !attendanceOccurrence.Attendees.Any( a =>
                        a.DidAttend.HasValue && a.DidAttend.Value
                        && a.PersonAliasId.HasValue
                        && nonLeaderPersonAliasIds.Contains( a.PersonAliasId.Value ) ) )
                    {
                        // This occurrence doesn't have any non-leader, [DidAttend] == true Attendance records; no need to look any further within this opportunity.
                        groupsMissingAttendance.Add( opportunity.Group );
                        break;
                    }
                }
            }

            var groupIdsToInclude = new List<int>();

            /*
             * The last check is to compare each Group's project type, if indicated by the presence of any selections within
             * selectionConfig.IncludeProjectTypes. Otherwise, we'll just include the IDs for all Groups missing attendance.
             */
            if ( selectionConfig.IncludeProjectTypes.Any() && groupsMissingAttendance.Any() )
            {
                groupsMissingAttendance.LoadAttributes( new RockContext() );
                var selectedProjectTypeGuids = selectionConfig.IncludeProjectTypes.Select( pt => pt.AsGuidOrNull() ).ToList();

                foreach ( var group in groupsMissingAttendance )
                {
                    var projectTypeGuid = group.GetAttributeValue( "ProjectType" ).AsGuidOrNull();
                    if ( projectTypeGuid.HasValue && selectedProjectTypeGuids.Any( selectedGuid => selectedGuid == projectTypeGuid ) )
                    {
                        groupIdsToInclude.Add( group.Id );
                    }
                }
            }
            else
            {
                groupIdsToInclude.AddRange( groupsMissingAttendance.Select( g => g.Id ) );
            }

            // This should be a reasonably small list of Group IDs, so a "WHERE IN" clause should be safe here.
            var query = new GroupService( ( RockContext ) serviceInstance.Context )
                .Queryable()
                .AsNoTracking()
                .Where( g => groupIdsToInclude.Contains( g.Id ) );

            return FilterExpressionExtractor.Extract<Rock.Model.Group>( query, parameterExpression, "g" );
        }

        #endregion
    }
}
