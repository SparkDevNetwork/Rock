﻿// <copyright>
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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///
    /// </summary>
    [Description( "Filter people on whether they are in the specified group, groups or child groups, with specific roles, status and dates" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Group(s) Filter (Advanced)" )]
    [Rock.SystemGuid.EntityTypeGuid( "A40B4BD4-FA17-4A82-97CC-2C71805F9D7C" )]
    public class InGroupFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataFilters/Person/inGroupFilter.obs" )
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var selectionValues = selection.Split( '|' );

            List<ListItemBag> groups = new List<ListItemBag>();
            List<int> groupIds = new List<int>();

            if ( selectionValues.Length > 0 )
            {
                var groupGuids = selectionValues[0]
                    .SplitDelimitedValues()
                    .AsGuidList();

                if ( groupGuids.Any() )
                {
                    var groupData = new GroupService( rockContext )
                        .GetByGuids( groupGuids )
                        .Select( g => new
                        {
                            g.Id,
                            g.Guid,
                            g.Name
                        } )
                        .ToList();

                    groups = groupData
                        .Select( g => new ListItemBag
                        {
                            Value = g.Guid.ToString(),
                            Text = g.Name
                        } )
                        .ToList();

                    groupIds = groupData.Select( g => g.Id ).ToList();
                }
            }

            var data = new Dictionary<string, string>
            {
                ["groups"] = groups.ToCamelCaseJson( false, false ),
                ["groupMemberRoles"] = selectionValues.Length > 1
                    ? selectionValues[1]
                    : string.Empty,
                ["includeChildGroups"] = selectionValues.Length > 2
                    ? selectionValues[2]
                    : false.ToString(),
                ["groupMemberStatus"] = selectionValues.Length > 3
                    ? selectionValues[3]
                    : string.Empty,
                ["includeSelectedGroups"] = selectionValues.Length > 4
                    ? selectionValues[4]
                    : false.ToString(),
                ["includeAllDescendants"] = selectionValues.Length > 5
                    ? selectionValues[5]
                    : false.ToString(),
                ["includeInactiveGroups"] = selectionValues.Length > 6
                    ? selectionValues[6]
                    : false.ToString(),
                ["dateAdded"] = selectionValues.Length > 7
                    ? selectionValues[7].Replace( ',', '|' )
                    : string.Empty,
                ["firstAttendance"] = selectionValues.Length > 8
                    ? selectionValues[8].Replace( ',', '|' )
                    : string.Empty,
                ["lastAttendance"] = selectionValues.Length > 9
                    ? selectionValues[9].Replace( ',', '|' )
                    : string.Empty,
            };

            // Add some data that is only required during first paint of the UI.
            data["groupMemberRoleItems"] = GetGroupTypeRolesForSelectedGroups( groupIds,
                data["includeChildGroups"].AsBoolean(),
                data["includeSelectedGroups"].AsBoolean(),
                data["includeAllDescendants"].AsBoolean(),
                data["includeInactiveGroups"].AsBoolean(),
                rockContext )
                .ToCamelCaseJson( false, false );

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var selections = new List<string>( 10 );

            if ( data.TryGetValue( "groups", out var groupsJson ) )
            {
                var groupGuids = groupsJson.FromJsonOrNull<List<ListItemBag>>()
                    ?.Select( g => g.Value )
                    .JoinStrings( "," )
                    ?? string.Empty;

                selections.Add( groupGuids );
            }
            else
            {
                selections.Add( string.Empty );
            }

            // Use .AsBoolean().ToString() on boolean values to force empty strings to become "False".
            selections.Add( data.GetValueOrDefault( "groupMemberRoles", string.Empty ) );
            selections.Add( data.GetValueOrDefault( "includeChildGroups", string.Empty ).AsBoolean().ToString() );
            selections.Add( data.GetValueOrDefault( "groupMemberStatus", string.Empty ) );
            selections.Add( data.GetValueOrDefault( "includeSelectedGroups", string.Empty ).AsBoolean().ToString() );
            selections.Add( data.GetValueOrDefault( "includeAllDescendants", string.Empty ).AsBoolean().ToString() );
            selections.Add( data.GetValueOrDefault( "includeInactiveGroups", string.Empty ).AsBoolean().ToString() );
            selections.Add( data.GetValueOrDefault( "dateAdded", string.Empty ).Replace( '|', ',' ) );
            selections.Add( data.GetValueOrDefault( "firstAttendance", string.Empty ).Replace( '|', ',' ) );
            selections.Add( data.GetValueOrDefault( "lastAttendance", string.Empty ).Replace( '|', ',' ) );

            return selections.JoinStrings( "|" );
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> ExecuteComponentRequest( Dictionary<string, string> request, SecurityGrant securityGrant, RockContext rockContext, RockRequestContext requestContext )
        {
            var action = request.GetValueOrNull( "action" );
            var options = request.GetValueOrNull( "options" )?.FromJsonOrNull<InGroupFilterGetGroupRolesForGroupsOptionsBag>();

            if ( action == "GetGroupRolesForGroups" && options != null )
            {
                var groupIds = GroupCache.GetMany( options.GroupGuids ).Select( gc => gc.Id ).ToList();

                var groupRoles = GetGroupTypeRolesForSelectedGroups(
                        groupIds,
                        options.IncludeChildGroups,
                        options.IncludeSelectedGroups,
                        options.IncludeAllDescendants,
                        options.IncludeInactiveGroups,
                        rockContext
                    );

                return new Dictionary<string, string> { { "groupRoles", groupRoles.ToCamelCaseJson( false, true ) } };
            }
            return null;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "In Group(s) (Advanced)";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return GetGroupFilterClientSelection( false );
        }

        /// <summary>
        /// Gets the group filter client selection based on if we are in "Not" mode
        /// </summary>
        /// <param name="not">if set to <c>true</c> [not].</param>
        /// <returns></returns>
        public virtual string GetGroupFilterClientSelection( bool not )
        {
            return string.Format( @"Rock.reporting.formatFilterForGroupFilterField('{0}', $content)", not ? "Not in groups:" : "In groups:" );
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            return GroupFilterFormatSelection( selection, false );
        }

        /// <summary>
        /// Formats the selection for the InGroupFilter/NotInGroupFilter based on if we are in "Not" mode
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <param name="not">if set to <c>true</c> [not].</param>
        /// <returns></returns>
        public virtual string GroupFilterFormatSelection( string selection, bool not )
        {
            string result = "Group Member";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                var rockContext = new RockContext();
                var groupGuids = selectionValues[0].Split( ',' ).AsGuidList();
                var groups = new GroupService( rockContext ).GetByGuids( groupGuids );

                var groupTypeRoleGuidList = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.AsGuid() ).ToList();

                var groupTypeRoles = new GroupTypeRoleService( rockContext ).Queryable().Where( a => groupTypeRoleGuidList.Contains( a.Guid ) ).ToList();

                bool includeChildGroups = false;
                bool includeChildGroupsPlusDescendants = false;
                bool includeChildGroupsIncludeSelected = false;
                bool includeInactiveGroups = false;
                string addedOnDateRangeText = null;
                string firstAttendanceDateRangeText = null;
                string lastAttendanceDateRangeText = null;
                if ( selectionValues.Length >= 3 )
                {
                    includeChildGroups = selectionValues[2].AsBooleanOrNull() ?? false;
                    if ( selectionValues.Length >= 6 )
                    {
                        includeChildGroupsIncludeSelected = selectionValues[4].AsBooleanOrNull() ?? false;
                        includeChildGroupsPlusDescendants = selectionValues[5].AsBooleanOrNull() ?? false;
                    }

                    if ( selectionValues.Length >= 7 )
                    {
                        includeInactiveGroups = selectionValues[6].AsBooleanOrNull() ?? false;
                    }

                    if ( selectionValues.Length >= 8 )
                    {
                        // convert comma delimited to pipe then get date range text
                        addedOnDateRangeText = SlidingDateRangePicker.FormatDelimitedValues( selectionValues[7].Replace( ',', '|' ) );
                    }

                    if ( selectionValues.Length >= 10 )
                    {
                        // convert comma delimited to pipe then get date range text
                        firstAttendanceDateRangeText = SlidingDateRangePicker.FormatDelimitedValues( selectionValues[8].Replace( ',', '|' ) );

                        // convert comma delimited to pipe then get date range text
                        lastAttendanceDateRangeText = SlidingDateRangePicker.FormatDelimitedValues( selectionValues[9].Replace( ',', '|' ) );
                    }
                }

                GroupMemberStatus? groupMemberStatus = null;
                if ( selectionValues.Length >= 4 )
                {
                    groupMemberStatus = selectionValues[3].ConvertToEnumOrNull<GroupMemberStatus>();
                }

                if ( groups != null )
                {
                    result = string.Format( not ? "Not in groups: {0}" : "In groups: {0}", groups.Select( a => a.Name ).ToList().AsDelimited( ", ", " or " ) );
                    if ( includeChildGroups )
                    {
                        if ( includeChildGroupsPlusDescendants )
                        {
                            result += " or descendant groups";
                        }
                        else
                        {
                            result += " or child groups";
                        }

                        if ( includeInactiveGroups )
                        {
                            result += ", including inactive groups";
                        }

                        if ( !includeChildGroupsIncludeSelected )
                        {
                            result += ", not including selected groups";
                        }
                    }

                    if ( groupTypeRoles.Count() > 0 )
                    {
                        result += string.Format( ", with role(s): {0}", groupTypeRoles.Select( a => string.Format( "{0} ({1})", a.Name, a.GroupType.Name ) ).ToList().AsDelimited( "," ) );
                    }

                    if ( groupMemberStatus.HasValue )
                    {
                        result += string.Format( ", with member status: {0}", groupMemberStatus.ConvertToString() );
                    }

                    if ( !string.IsNullOrEmpty( addedOnDateRangeText ) )
                    {
                        result += string.Format( ", added to group in Date Range: {0}", addedOnDateRangeText );
                    }

                    if ( !string.IsNullOrEmpty( firstAttendanceDateRangeText ) )
                    {
                        result += string.Format( ", first attendance to group in Date Range: {0}", firstAttendanceDateRangeText );
                    }

                    if ( !string.IsNullOrEmpty( lastAttendanceDateRangeText ) )
                    {
                        result += string.Format( ", last attendance to  group in Date Range: {0}", lastAttendanceDateRangeText );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a list of group type roles that should be available for
        /// selection in the filter settings UI.
        /// </summary>
        /// <param name="groupIds">The integer identifiers of the selected groups.</param>
        /// <param name="includeChildGroups">If <c>true</c> then child groups will be included when determining which group types to include.</param>
        /// <param name="includeSelectedGroups">If <paramref name="includeChildGroups"/> and this are <c>true</c> then the originally selected groups will be included along with the child groups when determining the group types.</param>
        /// <param name="includeAllDescendants">If <paramref name="includeChildGroups"/> and this are <c>true</c> then all descendant groups will be included when determining the group types.</param>
        /// <param name="includeInactiveGroups">If <paramref name="includeChildGroups"/> and this are <c>true</c> then inactive groups will be included when determining the group types.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A list of <see cref="ListItemBag"/> objects that represent the options to display to the individual.</returns>
        private List<ListItemBag> GetGroupTypeRolesForSelectedGroups( List<int> groupIds, bool includeChildGroups, bool includeSelectedGroups, bool includeAllDescendants, bool includeInactiveGroups, RockContext rockContext )
        {
            var groupService = new GroupService( rockContext );
            var groupTypeRoleService = new GroupTypeRoleService( rockContext );
            var qryGroupTypeRoles = groupTypeRoleService.Queryable();

            var selectedGroups = groupService.GetByIds( groupIds )
                .Select( s => new
                {
                    s.Id,
                    s.GroupTypeId
                } )
                .ToList();

            var selectedGroupTypeIds = selectedGroups.Select( a => a.GroupTypeId )
                .Distinct()
                .ToList();

            if ( includeChildGroups )
            {
                var childGroupTypeIds = new List<int>();

                foreach ( var groupId in selectedGroups.Select( a => a.Id ).ToList() )
                {
                    if ( includeAllDescendants )
                    {
                        // Get all children and descendants of the selected group(s).
                        var descendantGroupTypes = groupService.GetAllDescendentsGroupTypes( groupId, includeInactiveGroups );

                        childGroupTypeIds.AddRange( descendantGroupTypes.Select( a => a.Id ).ToList() );
                    }
                    else
                    {
                        // Get only immediate children of the selected group(s).
                        var childGroups = groupService.Queryable().Where( a => a.ParentGroupId == groupId );

                        if ( !includeInactiveGroups )
                        {
                            childGroups = childGroups.Where( a => a.IsActive == true );
                        }

                        childGroupTypeIds.AddRange( childGroups.Select( a => a.GroupTypeId ).Distinct().ToList() );
                    }
                }

                childGroupTypeIds = childGroupTypeIds.Distinct().ToList();

                if ( includeSelectedGroups )
                {
                    qryGroupTypeRoles = qryGroupTypeRoles.Where( a => a.GroupTypeId.HasValue
                        && ( selectedGroupTypeIds.Contains( a.GroupTypeId.Value ) || childGroupTypeIds.Contains( a.GroupTypeId.Value ) ) );
                }
                else
                {
                    qryGroupTypeRoles = qryGroupTypeRoles.Where( a => a.GroupTypeId.HasValue
                        && childGroupTypeIds.Contains( a.GroupTypeId.Value ) );
                }
            }
            else
            {
                qryGroupTypeRoles = qryGroupTypeRoles.Where( a => a.GroupTypeId.HasValue
                    && selectedGroupTypeIds.Contains( a.GroupTypeId.Value ) );
            }

            return qryGroupTypeRoles.OrderBy( a => a.GroupType.Order )
                .ThenBy( a => a.GroupType.Name )
                .ThenBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new
                {
                    a.Guid,
                    a.Name,
                    GroupTypeName = a.GroupType.Name
                } )
                .ToList()
                .Select( a => new ListItemBag
                {
                    Value = a.Guid.ToString(),
                    Text = $"{a.Name} ({a.GroupTypeName})"
                } )
                .ToList();
        }

#if WEBFORMS

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var gp = new GroupPicker();
            gp.ID = filterControl.ID + "_gp";
            gp.Label = "Group(s)";
            gp.SelectItem += gp_SelectItem;
            gp.CssClass = "js-group-picker";
            gp.AllowMultiSelect = true;
            filterControl.Controls.Add( gp );

            var cbChildGroups = new RockCheckBox();
            cbChildGroups.ID = filterControl.ID + "_cbChildsGroups";
            cbChildGroups.Text = "Include Child Group(s)";
            cbChildGroups.CssClass = "js-include-child-groups";
            cbChildGroups.AutoPostBack = true;
            cbChildGroups.CheckedChanged += gp_SelectItem;
            filterControl.Controls.Add( cbChildGroups );

            var cbIncludeSelectedGroup = new RockCheckBox();
            cbIncludeSelectedGroup.ID = filterControl.ID + "_cbIncludeSelectedGroup";
            cbIncludeSelectedGroup.Text = "Include Selected Group(s)";
            cbIncludeSelectedGroup.CssClass = "js-include-selected-groups";
            cbIncludeSelectedGroup.AutoPostBack = true;
            cbIncludeSelectedGroup.CheckedChanged += gp_SelectItem;
            filterControl.Controls.Add( cbIncludeSelectedGroup );

            var cbChildGroupsPlusDescendants = new RockCheckBox();
            cbChildGroupsPlusDescendants.ID = filterControl.ID + "_cbChildGroupsPlusDescendants";
            cbChildGroupsPlusDescendants.Text = "Include All Descendants";
            cbChildGroupsPlusDescendants.CssClass = "js-include-child-groups-descendants";
            cbChildGroupsPlusDescendants.AutoPostBack = true;
            cbChildGroupsPlusDescendants.CheckedChanged += gp_SelectItem;
            filterControl.Controls.Add( cbChildGroupsPlusDescendants );

            var cbIncludeInactiveGroups = new RockCheckBox();
            cbIncludeInactiveGroups.ID = filterControl.ID + "_cbIncludeInactiveGroups";
            cbIncludeInactiveGroups.Text = "Include Inactive Groups";
            cbIncludeInactiveGroups.CssClass = "js-include-inactive-groups";
            cbIncludeInactiveGroups.AutoPostBack = true;
            cbIncludeInactiveGroups.CheckedChanged += gp_SelectItem;
            filterControl.Controls.Add( cbIncludeInactiveGroups );

            var cblRole = new RockCheckBoxList();
            cblRole.Label = "with Group Member Role(s) (optional)";
            cblRole.ID = filterControl.ID + "_cblRole";
            cblRole.CssClass = "js-roles";
            cblRole.Visible = false;
            filterControl.Controls.Add( cblRole );

            RockDropDownList ddlGroupMemberStatus = new RockDropDownList();
            ddlGroupMemberStatus.CssClass = "js-group-member-status";
            ddlGroupMemberStatus.ID = filterControl.ID + "_ddlGroupMemberStatus";
            ddlGroupMemberStatus.Label = "with Group Member Status";
            ddlGroupMemberStatus.Help = "Select a specific group member status to only include group members with that status. Leaving this blank will return all members.";
            ddlGroupMemberStatus.BindToEnum<GroupMemberStatus>( true );
            ddlGroupMemberStatus.SetValue( GroupMemberStatus.Active.ConvertToInt() );
            filterControl.Controls.Add( ddlGroupMemberStatus );

            PanelWidget pwAdvanced = new PanelWidget();
            filterControl.Controls.Add( pwAdvanced );
            pwAdvanced.ID = filterControl.ID + "_pwAttributes";
            pwAdvanced.Title = "Advanced Filters";
            pwAdvanced.CssClass = "advanced-panel";

            SlidingDateRangePicker addedOnDateRangePicker = new SlidingDateRangePicker();
            addedOnDateRangePicker.ID = pwAdvanced.ID + "_addedOnDateRangePicker";
            addedOnDateRangePicker.AddCssClass( "js-dateadded-sliding-date-range" );
            addedOnDateRangePicker.Label = "Date Added:";
            addedOnDateRangePicker.Help = "Select the date range that the person was added to the group. Leaving this blank will not restrict results to a date range.";
            pwAdvanced.Controls.Add( addedOnDateRangePicker );

            SlidingDateRangePicker firstAttendanceDateRangePicker = new SlidingDateRangePicker();
            firstAttendanceDateRangePicker.ID = filterControl.ID + "_firstAttendanceDateRangePicker";
            firstAttendanceDateRangePicker.AddCssClass( "js-firstattendance-sliding-date-range" );
            firstAttendanceDateRangePicker.Label = "First Attendance";
            firstAttendanceDateRangePicker.Help = "The date range of the first attendance using the 'Sunday Date' of each attendance";
            pwAdvanced.Controls.Add( firstAttendanceDateRangePicker );

            SlidingDateRangePicker lastAttendanceDateRangePicker = new SlidingDateRangePicker();
            lastAttendanceDateRangePicker.ID = filterControl.ID + "_lastAttendanceDateRangePicker";
            lastAttendanceDateRangePicker.AddCssClass( "js-lastattendance-sliding-date-range" );
            lastAttendanceDateRangePicker.Label = "Last Attendance";
            lastAttendanceDateRangePicker.Help = "The date range of the last attendance using the 'Sunday Date' of each attendance";
            pwAdvanced.Controls.Add( lastAttendanceDateRangePicker );

            return new Control[11] { gp, cbChildGroups, cbIncludeSelectedGroup, cbChildGroupsPlusDescendants, cblRole, ddlGroupMemberStatus, cbIncludeInactiveGroups, addedOnDateRangePicker, pwAdvanced, firstAttendanceDateRangePicker, lastAttendanceDateRangePicker };
        }

        /// <summary>
        /// Handles the SelectItem event of the gp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gp_SelectItem( object sender, EventArgs e )
        {
            FilterField filterField = ( sender as Control ).FirstParentControlOfType<FilterField>();

            GroupPicker groupPicker = filterField.ControlsOfTypeRecursive<GroupPicker>().FirstOrDefault( a => a.HasCssClass( "js-group-picker" ) );
            RockCheckBox cbChildGroups = filterField.ControlsOfTypeRecursive<RockCheckBox>().FirstOrDefault( a => a.HasCssClass( "js-include-child-groups" ) );
            RockCheckBox cbChildGroupsPlusDescendants = filterField.ControlsOfTypeRecursive<RockCheckBox>().FirstOrDefault( a => a.HasCssClass( "js-include-child-groups-descendants" ) );
            RockCheckBox cbIncludeInactiveGroups = filterField.ControlsOfTypeRecursive<RockCheckBox>().FirstOrDefault( a => a.HasCssClass( "js-include-inactive-groups" ) );
            RockCheckBox cbIncludeSelectedGroup = filterField.ControlsOfTypeRecursive<RockCheckBox>().FirstOrDefault( a => a.HasCssClass( "js-include-selected-groups" ) );
            RockCheckBoxList cblRoles = filterField.ControlsOfTypeRecursive<RockCheckBoxList>().FirstOrDefault( a => a.HasCssClass( "js-roles" ) );

            var rockContext = new RockContext();

            var groupIdList = groupPicker.SelectedValues.AsIntegerList();
            var groupService = new GroupService( rockContext );

            var roles = GetGroupTypeRolesForSelectedGroups( groupIdList,
                cbChildGroups.Checked,
                cbIncludeSelectedGroup.Checked,
                cbChildGroupsPlusDescendants.Checked,
                cbIncludeInactiveGroups.Checked, rockContext );

            if ( roles.Any() )
            {
                cblRoles.Items.Clear();
                foreach ( var item in roles )
                {
                    cblRoles.Items.Add( new ListItem( item.Text, item.Value ) );
                }

                cblRoles.Visible = roles.Count > 0;
            }
            else
            {
                cblRoles.Visible = false;
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
            if ( controls.Count() < 9 )
            {
                return;
            }

            GroupPicker groupPicker = controls[0] as GroupPicker;
            RockCheckBox cbChildGroups = controls[1] as RockCheckBox;
            RockCheckBox cbIncludeSelectedGroup = controls[2] as RockCheckBox;
            RockCheckBox cbChildGroupsPlusDescendants = controls[3] as RockCheckBox;
            RockCheckBoxList cblRoles = controls[4] as RockCheckBoxList;
            RockDropDownList ddlGroupMemberStatus = controls[5] as RockDropDownList;
            RockCheckBox cbIncludeInactiveGroups = controls[6] as RockCheckBox;
            PanelWidget pwAdvanced = controls[8] as PanelWidget;

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            groupPicker.RenderControl( writer );
            cbChildGroups.RenderControl( writer );
            if ( !cbChildGroups.Checked )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Disabled, "disabled" );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "margin-l-md" );
                cbIncludeSelectedGroup.Checked = false;
                cbChildGroupsPlusDescendants.Checked = false;
                cbIncludeInactiveGroups.Checked = false;
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            cbIncludeSelectedGroup.ContainerCssClass = "margin-l-md";
            cbIncludeSelectedGroup.Enabled = cbChildGroups.Checked;
            cbIncludeSelectedGroup.RenderControl( writer );

            cbChildGroupsPlusDescendants.ContainerCssClass = "margin-l-md";
            cbChildGroupsPlusDescendants.Enabled = cbChildGroups.Checked;
            cbChildGroupsPlusDescendants.RenderControl( writer );

            cbIncludeInactiveGroups.ContainerCssClass = "margin-l-md";
            cbIncludeInactiveGroups.Enabled = cbChildGroups.Checked;
            cbIncludeInactiveGroups.RenderControl( writer );

            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            cblRoles.RenderControl( writer );

            ddlGroupMemberStatus.RenderControl( writer );
            pwAdvanced.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            if ( controls.Count() < 8 )
            {
                return null;
            }

            GroupPicker groupPicker = controls[0] as GroupPicker;
            RockCheckBox cbChildGroups = controls[1] as RockCheckBox;
            RockCheckBox cbIncludeSelectedGroup = controls[2] as RockCheckBox;
            RockCheckBox cbChildGroupsPlusDescendants = controls[3] as RockCheckBox;
            RockCheckBoxList cblRoles = controls[4] as RockCheckBoxList;
            RockDropDownList ddlGroupMemberStatus = controls[5] as RockDropDownList;
            RockCheckBox cbIncludeInactiveGroups = controls[6] as RockCheckBox;
            SlidingDateRangePicker addedOnDateRangePicker = controls[7] as SlidingDateRangePicker;
            SlidingDateRangePicker firstAttendanceDateRangePicker = controls[9] as SlidingDateRangePicker;
            SlidingDateRangePicker lastAttendanceDateRangePicker = controls[10] as SlidingDateRangePicker;

            List<int> groupIdList = groupPicker.SelectedValues.AsIntegerList();
            var groupGuids = new GroupService( new RockContext() ).GetByIds( groupIdList ).Select( a => a.Guid ).Distinct().ToList();

            //// NOTE: convert slidingdaterange delimitedvalues from pipe to comma delimited

            return string.Format(
                "{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                groupGuids.AsDelimited( "," ),
                cblRoles.SelectedValues.AsDelimited( "," ),
                cbChildGroups.Checked.ToString(),
                ddlGroupMemberStatus.SelectedValue,
                cbIncludeSelectedGroup.Checked.ToString(),
                cbChildGroupsPlusDescendants.Checked.ToString(),
                cbIncludeInactiveGroups.Checked.ToString(),
                addedOnDateRangePicker.DelimitedValues.Replace( "|", "," ),
                firstAttendanceDateRangePicker.DelimitedValues.Replace( "|", "," ),
                lastAttendanceDateRangePicker.DelimitedValues.Replace( "|", "," ) );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            if ( controls.Count() < 8 )
            {
                return;
            }

            GroupPicker groupPicker = controls[0] as GroupPicker;
            RockCheckBox cbChildGroups = controls[1] as RockCheckBox;
            RockCheckBox cbIncludeSelectedGroup = controls[2] as RockCheckBox;
            RockCheckBox cbChildGroupsPlusDescendants = controls[3] as RockCheckBox;
            RockCheckBoxList cblRoles = controls[4] as RockCheckBoxList;
            RockDropDownList ddlGroupMemberStatus = controls[5] as RockDropDownList;
            RockCheckBox cbIncludeInactiveGroups = controls[6] as RockCheckBox;
            SlidingDateRangePicker addedOnDateRangePicker = controls[7] as SlidingDateRangePicker;
            SlidingDateRangePicker firstAttendanceDateRangePicker = controls[9] as SlidingDateRangePicker;
            SlidingDateRangePicker lastAttendanceDateRangePicker = controls[10] as SlidingDateRangePicker;

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                List<Guid> groupGuids = selectionValues[0].Split( ',' ).AsGuidList();
                var groups = new GroupService( new RockContext() ).GetByGuids( groupGuids );
                if ( groups != null )
                {
                    groupPicker.SetValues( groups );
                }

                if ( selectionValues.Length >= 3 )
                {
                    cbChildGroups.Checked = selectionValues[2].AsBooleanOrNull() ?? false;
                }

                if ( selectionValues.Length >= 6 )
                {
                    cbIncludeSelectedGroup.Checked = selectionValues[4].AsBooleanOrNull() ?? false;
                    cbChildGroupsPlusDescendants.Checked = selectionValues[5].AsBooleanOrNull() ?? false;
                }
                else
                {
                    cbIncludeSelectedGroup.Checked = true;
                    cbChildGroupsPlusDescendants.Checked = true;
                }

                if ( selectionValues.Length >= 7 )
                {
                    cbIncludeInactiveGroups.Checked = selectionValues[6].AsBooleanOrNull() ?? false;
                }
                else
                {
                    // if options where saved before this option was added, set to false, even though it would have included inactive before
                    cbIncludeInactiveGroups.Checked = false;
                }

                gp_SelectItem( groupPicker, new EventArgs() );

                string[] selectedRoleGuids = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );

                foreach ( var item in cblRoles.Items.OfType<ListItem>() )
                {
                    item.Selected = selectedRoleGuids.Contains( item.Value );
                }

                if ( selectionValues.Length >= 4 )
                {
                    ddlGroupMemberStatus.SetValue( selectionValues[3] );
                }
                else
                {
                    ddlGroupMemberStatus.SetValue( string.Empty );
                }

                if ( selectionValues.Length >= 8 )
                {
                    // convert comma delimited to pipe
                    addedOnDateRangePicker.DelimitedValues = selectionValues[7].Replace( ',', '|' );
                }

                if ( selectionValues.Length >= 10 )
                {
                    // convert comma delimited to pipe
                    firstAttendanceDateRangePicker.DelimitedValues = selectionValues[8].Replace( ',', '|' );

                    // convert comma delimited to pipe
                    lastAttendanceDateRangePicker.DelimitedValues = selectionValues[9].Replace( ',', '|' );
                }
            }
        }

#endif

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            var rockContext = ( RockContext ) serviceInstance.Context;
            if ( selectionValues.Length >= 2 )
            {
                GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                List<Guid> groupGuids = selectionValues[0].Split( ',' ).AsGuidList();
                var groupService = new GroupService( rockContext );
                var groupIds = groupService.GetByGuids( groupGuids ).Select( a => a.Id ).Distinct().ToList();

                bool includeChildGroups = false;
                bool includeChildGroupsIncludeSelected = false;
                bool includeChildGroupsPlusDescendants = false;
                bool includeInactiveGroups = false;
                if ( selectionValues.Length >= 3 )
                {
                    includeChildGroups = selectionValues[2].AsBooleanOrNull() ?? false;
                }

                if ( selectionValues.Length >= 6 )
                {
                    includeChildGroupsIncludeSelected = selectionValues[4].AsBooleanOrNull() ?? false;
                    includeChildGroupsPlusDescendants = selectionValues[5].AsBooleanOrNull() ?? false;
                }
                else if ( includeChildGroups )
                {
                    // in case the selection was saved before these options where added
                    includeChildGroupsIncludeSelected = true;
                    includeChildGroupsPlusDescendants = true;
                }

                if ( selectionValues.Length >= 7 )
                {
                    includeInactiveGroups = selectionValues[6].AsBooleanOrNull() ?? true;
                }
                else
                {
                    // if options where saved before this option was added, set to false, even though it would have included inactive before
                    includeInactiveGroups = false;
                }

                GroupMemberStatus? groupMemberStatus = null;
                if ( selectionValues.Length >= 4 )
                {
                    groupMemberStatus = selectionValues[3].ConvertToEnumOrNull<GroupMemberStatus>();
                }

                var groupMemberServiceQry = groupMemberService.Queryable( true );

                List<int> childGroupIds = new List<int>();

                if ( includeChildGroups )
                {
                    foreach ( var groupId in groupIds )
                    {
                        if ( includeChildGroupsPlusDescendants )
                        {
                            // get all children and descendants of the selected group(s)
                            var descendantGroupIds = groupService.GetAllDescendentGroupIds( groupId, includeInactiveGroups );

                            childGroupIds.AddRange( descendantGroupIds );
                        }
                        else
                        {
                            // get only immediate children of the selected group(s)
                            var childGroups = groupService.Queryable().Where( a => a.ParentGroupId == groupId );
                            if ( !includeInactiveGroups )
                            {
                                childGroups = childGroups.Where( a => a.IsActive == true );
                            }

                            childGroupIds.AddRange( childGroups.Select( a => a.Id ) );
                        }
                    }

                    if ( includeChildGroupsIncludeSelected )
                    {
                        groupMemberServiceQry = groupMemberServiceQry.Where( xx => groupIds.Contains( xx.GroupId ) || childGroupIds.Contains( xx.GroupId ) );
                    }
                    else
                    {
                        groupMemberServiceQry = groupMemberServiceQry.Where( xx => childGroupIds.Contains( xx.GroupId ) );
                    }
                }
                else
                {
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => groupIds.Contains( xx.GroupId ) );
                }

                if ( groupMemberStatus.HasValue )
                {
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => xx.GroupMemberStatus == groupMemberStatus.Value );
                }

                var groupRoleGuids = selectionValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( n => n.AsGuid() ).ToList();
                if ( groupRoleGuids.Count() > 0 )
                {
                    var groupRoleIds = new GroupTypeRoleService( ( RockContext ) serviceInstance.Context ).Queryable().Where( a => groupRoleGuids.Contains( a.Guid ) ).Select( a => a.Id ).ToList();
                    groupMemberServiceQry = groupMemberServiceQry.Where( xx => groupRoleIds.Contains( xx.GroupRoleId ) );
                }

                if ( selectionValues.Length >= 8 )
                {
                    string addedOnSlidingDelimitedValues = selectionValues[7].Replace( ',', '|' );
                    DateRange dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( addedOnSlidingDelimitedValues );
                    if ( dateRange.Start.HasValue )
                    {
                        groupMemberServiceQry = groupMemberServiceQry.Where( xx => xx.DateTimeAdded >= dateRange.Start.Value );
                    }

                    if ( dateRange.End.HasValue )
                    {
                        groupMemberServiceQry = groupMemberServiceQry.Where( xx => xx.DateTimeAdded < dateRange.End.Value );
                    }
                }

                IQueryable<PersonIdFirstAttendance> firstAttendanceDateQry = null;
                IQueryable<PersonIdLastAttendance> lastAttendanceDateQry = null;

                if ( selectionValues.Length >= 10 )
                {
                    List<int> attendanceGroupIds = null;
                    if ( includeChildGroups )
                    {
                        if ( includeChildGroupsIncludeSelected )
                        {
                            attendanceGroupIds = new List<int>();
                            attendanceGroupIds.AddRange( groupIds );
                            attendanceGroupIds.AddRange( childGroupIds );
                        }
                        else
                        {
                            attendanceGroupIds = childGroupIds;
                        }
                    }
                    else
                    {
                        attendanceGroupIds = groupIds;
                    }

                    var groupAttendanceQuery = new AttendanceService( rockContext ).Queryable()
                        .Where( a =>
                            a.DidAttend == true &&
                            a.Occurrence.GroupId.HasValue &&
                            attendanceGroupIds.Contains( a.Occurrence.GroupId.Value ) );

                    string firstAttendanceSlidingDelimitedValues = selectionValues[8].Replace( ',', '|' );
                    DateRange firstAttendanceDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( firstAttendanceSlidingDelimitedValues );

                    if ( firstAttendanceDateRange.Start.HasValue || firstAttendanceDateRange.End.HasValue )
                    {
                        firstAttendanceDateQry = groupAttendanceQuery
                            .GroupBy( xx => xx.PersonAlias.PersonId )
                            .Select( ss => new PersonIdFirstAttendance
                            {
                                PersonId = ss.Key,
                                FirstAttendanceSundayDate = ss.Min( a => a.Occurrence.SundayDate )
                            } );

                        if ( firstAttendanceDateRange.Start.HasValue )
                        {
                            firstAttendanceDateQry = firstAttendanceDateQry.Where( xx => xx.FirstAttendanceSundayDate >= firstAttendanceDateRange.Start.Value );
                        }

                        if ( firstAttendanceDateRange.End.HasValue )
                        {
                            firstAttendanceDateQry = firstAttendanceDateQry.Where( xx => xx.FirstAttendanceSundayDate < firstAttendanceDateRange.End.Value );
                        }
                    }

                    string lastAttendanceSlidingDelimitedValues = selectionValues[9].Replace( ',', '|' );
                    DateRange lastAttendanceDateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( lastAttendanceSlidingDelimitedValues );

                    if ( lastAttendanceDateRange.Start.HasValue || lastAttendanceDateRange.End.HasValue )
                    {
                        lastAttendanceDateQry = groupAttendanceQuery
                            .GroupBy( xx => xx.PersonAlias.PersonId )
                            .Select( ss => new PersonIdLastAttendance
                            {
                                PersonId = ss.Key,
                                LastAttendanceSundayDate = ss.Max( a => a.Occurrence.SundayDate )
                            } );

                        if ( lastAttendanceDateRange.Start.HasValue )
                        {
                            lastAttendanceDateQry = lastAttendanceDateQry.Where( xx => xx.LastAttendanceSundayDate >= lastAttendanceDateRange.Start.Value );
                        }

                        if ( lastAttendanceDateRange.End.HasValue )
                        {
                            lastAttendanceDateQry = lastAttendanceDateQry.Where( xx => xx.LastAttendanceSundayDate < lastAttendanceDateRange.End.Value );
                        }
                    }
                }

                IQueryable<Rock.Model.Person> qry = null;
                if ( lastAttendanceDateQry == null && firstAttendanceDateQry == null )
                {
                    qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                        .Where( p => groupMemberServiceQry.Any( xx => xx.PersonId == p.Id ) );
                }
                else
                {
                    if ( firstAttendanceDateQry != null && lastAttendanceDateQry != null )
                    {
                        qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                            .Where( p => groupMemberServiceQry.Any( xx => xx.PersonId == p.Id )
                                && firstAttendanceDateQry.Any( aa => aa.PersonId == p.Id ) && lastAttendanceDateQry.Any( bb => bb.PersonId == p.Id ) );
                    }
                    else if ( firstAttendanceDateQry != null )
                    {
                        qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                            .Where( p => groupMemberServiceQry.Any( xx => xx.PersonId == p.Id )
                                && firstAttendanceDateQry.Any( aa => aa.PersonId == p.Id ) );
                    }
                    else if ( lastAttendanceDateQry != null )
                    {
                        qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                            .Where( p => groupMemberServiceQry.Any( xx => xx.PersonId == p.Id )
                                && lastAttendanceDateQry.Any( aa => aa.PersonId == p.Id ) );
                    }
                }

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion

        #region Support Classes

        /// <summary>
        ///
        /// </summary>
        private class PersonIdFirstAttendance
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the first attendance sunday date.
            /// </summary>
            /// <value>
            /// The first attendance sunday date.
            /// </value>
            public DateTime FirstAttendanceSundayDate { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        private class PersonIdLastAttendance
        {
            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the last attendance sunday date.
            /// </summary>
            /// <value>
            /// The last attendance sunday date.
            /// </value>
            public DateTime LastAttendanceSundayDate { get; set; }
        }

        #endregion
    }
}