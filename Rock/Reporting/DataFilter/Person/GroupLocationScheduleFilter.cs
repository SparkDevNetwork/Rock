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
using System.Linq.Expressions;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

using static Rock.Web.UI.Controls.SlidingDateRangePicker;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// Filters people based on group membership, assigned location, schedule, member status, and date range.
    /// </summary>
    [Description( "Filter people on whether they are assigned to a specific location and schedule." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person Group Location Schedule Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "D7220948-DBC2-418C-9769-5DC1E20EFAC9" )]
    public class GroupLocationScheduleFilter : DataFilterComponent
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
            /// Gets or sets the ID of the selected group.
            /// </summary>
            /// <value>
            /// The group ID.
            /// </value>
            public int GroupId { get; set; }

            /// <summary>
            /// Gets or sets the ID of the selected location.
            /// </summary>
            /// <value>
            /// The location ID.
            /// </value>
            public int LocationId { get; set; }

            /// <summary>
            /// Gets or sets the list of schedule GUIDs associated with the selected group location.
            /// </summary>
            /// <value>
            /// A list of schedule GUIDs.
            /// </value>
            public List<Guid> GroupLocationSchedules { get; set; }

            /// <summary>
            /// Gets or sets the group member status to filter by.
            /// </summary>
            /// <value>
            /// The group member status.
            /// </value>
            public GroupMemberStatus? GroupMemberStatus { get; set; }

            /// <summary>
            /// Gets or sets the delimited date range values.
            /// </summary>
            /// <value>
            /// The delimited date range values.
            /// </value>
            public string DelimitedDateRangeValues { get; set; }

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

        /// <inheritdoc/>
        public override string AppliesToEntityType
        {
            get
            {
                return "Rock.Model.Person";
            }
        }

        /// <inheritdoc/>
        public override string Section
        {
            get
            {
                return "Additional Filters";
            }
        }

        #endregion

        #region Public Methods

        /// <inheritdoc/>
        public override string GetTitle( Type entityType )
        {
            return "Group Location Schedule(s)";
        }

        /// <inheritdoc/>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
  var groupName = $('.js-group-picker', $content).find('.selected-names').text()
  var groupLocation = $('.js-group-location', $content).find(':selected').text()
  var groupLocationSchedules = $('.js-group-location-schedules', $content).find(':checked').closest('label');
  var groupMemberStatus = $('.js-group-member-status option:selected', $content).text();
  var slidingDateRange = $('.js-sliding-date-range', $content).val()
  var result = 'In group: ' + groupName;
  if (groupLocation) {
     result = result + ', assigned to location: ' + groupLocation;
  }

  if (groupLocationSchedules.length > 0) {
     var scheduleList = groupLocationSchedules.map(function() { return $(this).text() }).get().join(', ');
     result = result + ', and scheduled at: ' + scheduleList;
  }

  if (groupMemberStatus) {
     result = result + ', with member status: ' + groupMemberStatus;
  }

  if (slidingDateRange) {
     result = result + '. Date Range: ' + slidingDateRange;
  }

  return result;
}
";
        }

        /// <inheritdoc/>
        public override string FormatSelection( Type entityType, string selection )
        {
            var resultSb = new StringBuilder( "Group Member Assignment" );

            var selectionConfig = SelectionConfig.Parse( selection );

            if ( selectionConfig != null )
            {
                var group = GroupCache.Get( selectionConfig.GroupId );
                var location = new LocationService( new RockContext() ).Get( selectionConfig.LocationId );

                List<Schedule> groupLocationSchedules = new List<Schedule>();
                if ( selectionConfig.GroupLocationSchedules?.Count > 0)
                {
                    groupLocationSchedules = new ScheduleService( new RockContext() )
                        .Queryable()
                        .Where( s => selectionConfig.GroupLocationSchedules.Contains( s.Guid ) )
                        .ToList();
                }

                var groupMemberStatus = selectionConfig.GroupMemberStatus;

                string dateRangeString = string.Empty;
                if ( selectionConfig.DelimitedDateRangeValues.IsNotNullOrWhiteSpace() )
                {
                    dateRangeString = SlidingDateRangePicker.FormatDelimitedValues( selectionConfig.DelimitedDateRangeValues );
                }

                if ( group != null )
                {
                    resultSb.Clear();
                    resultSb.Append( $"In group: {group.Name}" );
                    if ( location != null )
                    {
                        resultSb.Append( $", assigned to location: {location.ToString( true )}" );
                    }

                    if ( groupLocationSchedules.Count() > 0 )
                    {
                        resultSb.Append( $", and scheduled at: {groupLocationSchedules.Select( a => a.ToFriendlyScheduleText( true ) ).ToList().AsDelimited( ", " )}" );
                    }

                    if ( groupMemberStatus.HasValue )
                    {
                        resultSb.Append( $", with member status: {groupMemberStatus.ConvertToString()}" );
                    }

                    if ( dateRangeString.IsNotNullOrWhiteSpace() )
                    {
                        resultSb.Append( $". Date Range: {dateRangeString}" );
                    }
                }
            }

            return resultSb.ToString();
        }

        /// <inheritdoc/>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var gpGroup = new GroupPicker();
            gpGroup.ID = filterControl.ID + "_gpGroup";
            gpGroup.Label = "Group";
            gpGroup.Required = true;
            gpGroup.CssClass = "js-group-picker";
            gpGroup.SelectItem += gpGroup_SelectedIndexChanged;
            filterControl.Controls.Add( gpGroup );

            var ddlGroupLocation = new RockDropDownList();
            ddlGroupLocation.ID = filterControl.ID + "_ddlGroupLocation";
            ddlGroupLocation.Label = "Group Location";
            ddlGroupLocation.Required = true;
            ddlGroupLocation.CssClass = "js-group-location";
            ddlGroupLocation.SelectedIndexChanged += ddlGroupLocation_SelectedIndexChanged;
            ddlGroupLocation.AutoPostBack = true;
            filterControl.Controls.Add( ddlGroupLocation );

            PopulateGroupLocations( filterControl );

            var cblGroupScheduleLocations = new RockCheckBoxList();
            cblGroupScheduleLocations.ID = filterControl.ID + "_cblGroupScheduleLocations";
            cblGroupScheduleLocations.Label = "Group Location Schedules";
            cblGroupScheduleLocations.CssClass = "js-group-location-schedules";
            filterControl.Controls.Add( cblGroupScheduleLocations );

            PopulateGroupLocationSchedules( filterControl );

            var ddlGroupMemberStatus = new RockDropDownList();
            ddlGroupMemberStatus.ID = filterControl.ID + "_ddlGroupMemberStatus";
            ddlGroupMemberStatus.Label = "Group Member Status";
            ddlGroupMemberStatus.CssClass = "js-group-member-status";
            ddlGroupMemberStatus.BindToEnum<GroupMemberStatus>( true );
            filterControl.Controls.Add( ddlGroupMemberStatus );

            var slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.ID = filterControl.ID + "_slidingDateRangePicker";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            filterControl.Controls.Add( slidingDateRangePicker );

            return new Control[5] { gpGroup, ddlGroupLocation, cblGroupScheduleLocations, ddlGroupMemberStatus, slidingDateRangePicker };
        }

        /// <summary>
        /// Handles the SelectItem event of the gpGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            FilterField filterField = ( sender as Control ).FirstParentControlOfType<FilterField>();

            PopulateGroupLocations( filterField );
            PopulateGroupLocationSchedules( filterField );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroupLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroupLocation_SelectedIndexChanged( object sender, EventArgs e )
        {
            FilterField filterField = ( sender as Control ).FirstParentControlOfType<FilterField>();

            PopulateGroupLocationSchedules( filterField );
        }

        /// <summary>
        /// Populates the group locations.
        /// </summary>
        /// <param name="filterField">The filter field.</param>
        private void PopulateGroupLocations( FilterField filterField )
        {
            var groupPicker = filterField.ControlsOfTypeRecursive<GroupPicker>().FirstOrDefault( a => a.HasCssClass( "js-group-picker" ) );
            var ddlGroupLocation = filterField.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault( a => a.HasCssClass( "js-group-location" ) );
            int? groupId = groupPicker.SelectedValueAsId();

            if ( groupId.HasValue )
            {
                ddlGroupLocation.Items.Clear();

                var locations = new GroupLocationService( new RockContext() ).Queryable()
                    .Where( gl => gl.GroupId == groupId )
                    .Select( gl => new ListItem
                    {
                        Text = string.IsNullOrEmpty( gl.Location.Name ) ? gl.Location.Description : gl.Location.Name, // Nameless Locations have a friendly name in their Description.
                        Value = gl.Location.Id.ToString()
                    } )
                    .ToList();

                foreach ( var item in locations )
                {
                    ddlGroupLocation.Items.Add( item );
                }

                ddlGroupLocation.Visible = ddlGroupLocation.Items.Count > 0;
            }
            else
            {
                ddlGroupLocation.SelectedValue = null;
                ddlGroupLocation.Visible = false;
            }
        }

        /// <summary>
        /// Populates the group location schedules.
        /// </summary>
        /// <param name="filterField">The filter field.</param>
        private void PopulateGroupLocationSchedules( FilterField filterField )
        {
            var groupPicker = filterField.ControlsOfTypeRecursive<GroupPicker>().FirstOrDefault( a => a.HasCssClass( "js-group-picker" ) );
            var ddlGroupLocation = filterField.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault( a => a.HasCssClass( "js-group-location" ) );
            var cblGroupScheduleLocations = filterField.ControlsOfTypeRecursive<RockCheckBoxList>().FirstOrDefault( a => a.HasCssClass( "js-group-location-schedules" ) );
            int? groupId = groupPicker.SelectedValueAsId();
            int? groupLocationId = ddlGroupLocation.SelectedValueAsId();

            if ( groupId.HasValue && groupLocationId.HasValue )
            {
                cblGroupScheduleLocations.Items.Clear();

                var groupLocationSchedules = new GroupLocationService( new RockContext() ).Queryable()
                    .Where( gl => gl.GroupId == groupId && gl.LocationId == groupLocationId )
                    .SelectMany( gl => gl.Schedules )
                    .Select( s => new ListItem
                    {
                        Text = string.IsNullOrEmpty( s.Name ) ? s.Description : s.Name, // Nameless Schedules have a friendly name in their Description.
                        Value = s.Guid.ToString()
                    } )
                    .Distinct()
                    .ToList();

                foreach ( var item in groupLocationSchedules )
                {
                    cblGroupScheduleLocations.Items.Add( item );
                }

                cblGroupScheduleLocations.Visible = cblGroupScheduleLocations.Items.Count > 0;
            }
            else
            {
                cblGroupScheduleLocations.SelectedValue = null;
                cblGroupScheduleLocations.Visible = false;
            }
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        /// <inheritdoc/>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var groupPicker = ( controls[0] as GroupPicker );
            var ddlGroupLocation = ( controls[1] as RockDropDownList );
            var cblGroupLocationSchedules = ( controls[2] as RockCheckBoxList );
            var ddlGroupMemberStatus = ( controls[3] as RockDropDownList );
            var slidingDateRangePicker = ( controls[4] as SlidingDateRangePicker );
            var selectionConfig = new SelectionConfig();

            var groupId = groupPicker.SelectedValueAsId();

            if ( groupId.HasValue )
            {
                selectionConfig.GroupId = groupId.Value;
                selectionConfig.LocationId = ddlGroupLocation.SelectedValue.AsInteger();
                selectionConfig.GroupLocationSchedules = cblGroupLocationSchedules.SelectedValues.AsGuidList();
                selectionConfig.GroupMemberStatus = ddlGroupMemberStatus.SelectedValue.ConvertToEnumOrNull<GroupMemberStatus>();
                selectionConfig.DelimitedDateRangeValues = slidingDateRangePicker.DelimitedValues;
            }

            return selectionConfig.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );

            var groupPicker = ( controls[0] as GroupPicker );
            var ddlGroupLocation = ( controls[1] as RockDropDownList );
            var cblGroupLocationSchedules = ( controls[2] as RockCheckBoxList );
            var ddlGroupMemberStatus = ( controls[3] as RockDropDownList );
            var slidingDateRangePicker = ( controls[4] as SlidingDateRangePicker );

            groupPicker.SetValue( selectionConfig.GroupId );
            gpGroup_SelectedIndexChanged( groupPicker, new EventArgs() );
            ddlGroupLocation.SetValue( selectionConfig.LocationId.ToString() );
            ddlGroupLocation_SelectedIndexChanged( ddlGroupLocation, new EventArgs() );
            cblGroupLocationSchedules.SetValues( selectionConfig.GroupLocationSchedules );
            if ( selectionConfig.GroupMemberStatus.HasValue )
            {
                ddlGroupMemberStatus.SetValue( ( ( int ) selectionConfig.GroupMemberStatus.Value ).ToString() );
            }
            slidingDateRangePicker.DelimitedValues = selectionConfig.DelimitedDateRangeValues;

        }

        /// <inheritdoc/>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var selectionConfig = SelectionConfig.Parse( selection );
            GroupMemberAssignmentService groupMemberAssignmentService = new GroupMemberAssignmentService( ( RockContext ) serviceInstance.Context );

            // Filtering by Group Id
            var groupMemberAssignmentServiceQry = groupMemberAssignmentService.Queryable().Where( gma => gma.GroupMember.GroupId == selectionConfig.GroupId );


            // Filtering by Group Location
            groupMemberAssignmentServiceQry = groupMemberAssignmentServiceQry.Where( gma => gma.LocationId == selectionConfig.LocationId );

            // Filtering by Group Location Schedules
            if ( selectionConfig.GroupLocationSchedules?.Count > 0 )
            {
                groupMemberAssignmentServiceQry = groupMemberAssignmentServiceQry.Where( gma => selectionConfig.GroupLocationSchedules.Contains( gma.Schedule.Guid ) );
            }

            // Filtering by Group Member Status
            if ( selectionConfig.GroupMemberStatus.HasValue )
            {
                groupMemberAssignmentServiceQry = groupMemberAssignmentServiceQry.Where( gma => gma.GroupMember.GroupMemberStatus == selectionConfig.GroupMemberStatus.Value );
            }

            // Filtering by Delimited Date Range Values
            if ( selectionConfig.DelimitedDateRangeValues.IsNotNullOrWhiteSpace() )
            {
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( selectionConfig.DelimitedDateRangeValues );
                if ( dateRange.Start.HasValue )
                {
                    groupMemberAssignmentServiceQry = groupMemberAssignmentServiceQry.Where( gma => gma.CreatedDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    groupMemberAssignmentServiceQry = groupMemberAssignmentServiceQry.Where( gma => gma.CreatedDateTime <= dateRange.End.Value );
                }
            }

            var personIdList = groupMemberAssignmentServiceQry
                    .Select( xx => xx.GroupMember.PersonId )
                    .ToList();

            var qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                .Where( p => personIdList.Contains( p.Id ) );

            Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}