// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Scedule Builder" )]
    [Category( "Check-in" )]
    [Description( "Helps to build schedules to be used for checkin." )]
    public partial class CheckinScheduleBuilder : RockBlock
    {

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            pCategory.EntityTypeId = EntityTypeCache.GetId( typeof( Rock.Model.Schedule ) ) ?? 0;

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
            BindFilter();

            gGroupLocationSchedule.DataKeyNames = new string[] { "GroupLocationId" };
            gGroupLocationSchedule.Actions.ShowAdd = false;
            gGroupLocationSchedule.IsDeleteEnabled = false;
            gGroupLocationSchedule.GridRebind += gGroupLocationSchedule_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Adds the schedule columns.
        /// </summary>
        private void AddScheduleColumns()
        {
            ScheduleService scheduleService = new ScheduleService( new RockContext() );

            // limit Schedules to ones that have a CheckInStartOffsetMinutes
            var scheduleQry = scheduleService.Queryable().Where( a => a.CheckInStartOffsetMinutes != null );

            // limit Schedules to the Category from the Filter
            int scheduleCategoryId = rFilter.GetUserPreference( "Category" ).AsIntegerOrNull() ?? Rock.Constants.All.Id;
            if ( scheduleCategoryId != Rock.Constants.All.Id )
            {
                scheduleQry = scheduleQry.Where( a => a.CategoryId == scheduleCategoryId );
            }
            else
            {
                // NULL (or 0) means Shared, so specifically filter so to show only Schedules with CategoryId NULL
                scheduleQry = scheduleQry.Where( a => a.CategoryId == null );
            }

            // clear out any existing schedule columns and add the ones that match the current filter setting
            var scheduleList = scheduleQry.ToList().OrderBy( a => a.ToString() ).ToList();

            var checkBoxEditableFields = gGroupLocationSchedule.Columns.OfType<CheckBoxEditableField>().ToList();
            foreach ( var field in checkBoxEditableFields )
            {
                gGroupLocationSchedule.Columns.Remove( field );
            }

            foreach ( var item in scheduleList )
            {
                string dataFieldName = string.Format( "scheduleField_{0}", item.Id );

                CheckBoxEditableField field = new CheckBoxEditableField { HeaderText = item.Name, DataField = dataFieldName };
                gGroupLocationSchedule.Columns.Add( field );
            }

            if ( !scheduleList.Any() )
            {
                nbNotification.Text = nbNotification.Text = String.Format( "<p><strong>Warning</strong></p>No schedules found. Consider <a class='alert-link' href='{0}'>adding a schedule</a> or a different schedule category.", ResolveUrl( "~/Schedules" ) );
                nbNotification.Visible = true;
            }
            else
            {
                nbNotification.Visible = false;
            }
        }

        #endregion

        #region Grid Filter

        /// <summary>
        /// Binds any needed data to the Grid Filter also using the user's stored
        /// preferences.
        /// </summary>
        private void BindFilter()
        {
            ddlGroupType.Items.Clear();
            ddlGroupType.Items.Add( Rock.Constants.All.ListItem );

            // populate the GroupType DropDownList only with GroupTypes with GroupTypePurpose of Checkin Template
            int groupTypePurposeCheckInTemplateId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ) ).Id;

            var rockContext = new RockContext();

            GroupTypeService groupTypeService = new GroupTypeService( rockContext );
            var groupTypeList = groupTypeService.Queryable()
                .Where( a => a.GroupTypePurposeValueId == groupTypePurposeCheckInTemplateId )
                .ToList();
            foreach ( var groupType in groupTypeList )
            {
                ddlGroupType.Items.Add( new ListItem( groupType.Name, groupType.Id.ToString() ) );
            }

            ddlGroupType.SetValue( rFilter.GetUserPreference( "Group Type" ) );

            // hide the GroupType filter if this page has a groupTypeId parameter
            int? groupTypeIdPageParam = this.PageParameter( "groupTypeId" ).AsIntegerOrNull();
            if ( groupTypeIdPageParam.HasValue )
            {
                ddlGroupType.Visible = false;
            }

            var filterCategory = new CategoryService( rockContext ).Get( rFilter.GetUserPreference( "Category" ).AsInteger() );
            pCategory.SetValue( filterCategory );

            pkrParentLocation.SetValue( rFilter.GetUserPreference( "Parent Location" ).AsIntegerOrNull() );
        }

        #endregion

        #region Grid/Filter Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Group Type", ddlGroupType.SelectedValueAsId().ToString() );

            rFilter.SaveUserPreference( "Parent Location", pkrParentLocation.SelectedValueAsId().ToString() );

            BindGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void rFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            int itemId = e.Value.AsInteger();
            switch ( e.Key )
            {
                case "Group Type":

                    int? groupTypeIdPageParam = this.PageParameter( "groupTypeId" ).AsIntegerOrNull();

                    //// we only use the GroupType from the filter in cases where there isn't a PageParam of groupTypeId
                    // but just in case the filter wants to display the GroupName, override the itemId with the groupTypeId PageParam
                    if ( groupTypeIdPageParam.HasValue )
                    {
                        itemId = groupTypeIdPageParam.Value;
                    }

                    var groupType = GroupTypeCache.Read( itemId );
                    if ( groupType != null )
                    {
                        e.Value = groupType.Name;
                    }
                    else
                    {
                        e.Value = Rock.Constants.All.Text;
                    }

                    break;

                case "Category":

                    // even though it is technically a filter, don't show it as a filter since we don't show category in the filter UI
                    e.Value = null;

                    break;

                case "Parent Location":

                    var location = new LocationService( new RockContext() ).Get( itemId );
                    if ( location != null )
                    {
                        e.Value = location.Name;
                    }
                    else
                    {
                        e.Value = Rock.Constants.All.Text;
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gGroupLocationSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gGroupLocationSchedule_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            AddScheduleColumns();

            var rockContext = new RockContext();

            var groupLocationService = new GroupLocationService( rockContext );
            var groupTypeService = new GroupTypeService( rockContext );
            var groupService = new GroupService( rockContext );

            IEnumerable<GroupTypePath> groupPaths = new List<GroupTypePath>();
            var groupLocationQry = groupLocationService.Queryable();
            int groupTypeId;

            // if this page has a PageParam for groupTypeId use that to limit which groupTypeId to see. Otherwise, use the groupTypeId specified in the filter
            int? groupTypeIdPageParam = this.PageParameter( "groupTypeId" ).AsIntegerOrNull();
            if ( groupTypeIdPageParam.HasValue )
            {
                groupTypeId = groupTypeIdPageParam ?? Rock.Constants.All.Id;
            }
            else
            {
                groupTypeId = ddlGroupType.SelectedValueAsInt() ?? Rock.Constants.All.Id;
            }

            if ( groupTypeId != Rock.Constants.All.Id )
            {
                var descendantGroupTypeIds = groupTypeService.GetAllAssociatedDescendents( groupTypeId ).Select( a => a.Id );

                // filter to groups that either are of the GroupType or are of a GroupType that has the selected GroupType as a parent (ancestor)
                groupLocationQry = groupLocationQry.Where( a => a.Group.GroupType.Id == groupTypeId || descendantGroupTypeIds.Contains( a.Group.GroupTypeId ) );

                groupPaths = groupTypeService.GetAllAssociatedDescendentsPath( groupTypeId );
            }
            else
            {
                // if no specific GroupType is specified, show all GroupTypes with GroupTypePurpose of Checkin Template and their descendents (since this blocktype is specifically for Checkin)
                int groupTypePurposeCheckInTemplateId = DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ) ).Id;
                List<int> descendantGroupTypeIds = new List<int>();
                foreach ( var templateGroupType in groupTypeService.Queryable().Where( a => a.GroupTypePurposeValueId == groupTypePurposeCheckInTemplateId ) )
                {
                    foreach ( var childGroupType in groupTypeService.GetChildGroupTypes( templateGroupType.Id ) )
                    {
                        descendantGroupTypeIds.Add( childGroupType.Id );
                        descendantGroupTypeIds.AddRange( groupTypeService.GetAllAssociatedDescendents( childGroupType.Id ).Select( a => a.Id ).ToList() );
                    }
                }

                groupLocationQry = groupLocationQry.Where( a => descendantGroupTypeIds.Contains( a.Group.GroupTypeId ) );
            }

            if ( gGroupLocationSchedule.SortProperty != null )
            {
                groupLocationQry = groupLocationQry.Sort( gGroupLocationSchedule.SortProperty );
            }
            else
            {
                groupLocationQry = groupLocationQry.OrderBy( a => a.Group.Name ).ThenBy( a => a.Location.Name );
            }

            var qryList = groupLocationQry
                .Where( a => a.Location != null )
                .Select( a =>
                new
                {
                    GroupLocationId = a.Id,
                    a.Location,
                    GroupId = a.GroupId,
                    GroupName = a.Group.Name,
                    ScheduleIdList = a.Schedules.Select( s => s.Id ),
                    GroupTypeId = a.Group.GroupTypeId
                } ).ToList();

            var locationService = new LocationService( rockContext );
            int parentLocationId = pkrParentLocation.SelectedValueAsInt() ?? Rock.Constants.All.Id;
            if ( parentLocationId != Rock.Constants.All.Id )
            {
                var descendantLocationIds = locationService.GetAllDescendents( parentLocationId ).Select( a => a.Id );
                qryList = qryList.Where( a => descendantLocationIds.Contains( a.Location.Id ) ).ToList();
            }

            // put stuff in a datatable so we can dynamically have columns for each Schedule
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add( "GroupLocationId" );
            dataTable.Columns.Add( "GroupId" );
            dataTable.Columns.Add( "GroupName" );
            dataTable.Columns.Add( "GroupPath" );
            dataTable.Columns.Add( "LocationName" );
            dataTable.Columns.Add( "LocationPath" );
            foreach ( var field in gGroupLocationSchedule.Columns.OfType<CheckBoxEditableField>() )
            {
                dataTable.Columns.Add( field.DataField, typeof( bool ) );
            }

            var locationPaths = new Dictionary<int, string>();

            foreach ( var row in qryList )
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["GroupLocationId"] = row.GroupLocationId;
                dataRow["GroupName"] = groupService.GroupAncestorPathName( row.GroupId );
                dataRow["GroupPath"] = groupPaths.Where( gt => gt.GroupTypeId == row.GroupTypeId ).Select( gt => gt.Path ).FirstOrDefault();
                dataRow["LocationName"] = row.Location.Name;

                if ( row.Location.ParentLocationId.HasValue )
                {
                    int locationId = row.Location.ParentLocationId.Value;

                    if ( !locationPaths.ContainsKey( locationId ) )
                    {
                        var locationNames = new List<string>();
                        var parentLocation = locationService.Get( locationId );
                        while ( parentLocation != null )
                        {
                            locationNames.Add( parentLocation.Name );
                            parentLocation = parentLocation.ParentLocation;
                        }
                        if ( locationNames.Any() )
                        {
                            locationNames.Reverse();
                            locationPaths.Add( locationId, locationNames.AsDelimited( " > " ) );
                        }
                        else
                        {
                            locationPaths.Add( locationId, string.Empty );
                        }
                    }

                    dataRow["LocationPath"] = locationPaths[locationId];
                }

                foreach ( var field in gGroupLocationSchedule.Columns.OfType<CheckBoxEditableField>() )
                {
                    int scheduleId = int.Parse( field.DataField.Replace( "scheduleField_", string.Empty ) );
                    dataRow[field.DataField] = row.ScheduleIdList.Any( a => a == scheduleId );
                }

                dataTable.Rows.Add( dataRow );
            }

            gGroupLocationSchedule.EntityTypeId = EntityTypeCache.Read<GroupLocation>().Id;
            gGroupLocationSchedule.DataSource = dataTable;
            gGroupLocationSchedule.DataBind();
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            GroupLocationService groupLocationService = new GroupLocationService( rockContext );
            ScheduleService scheduleService = new ScheduleService( rockContext );

            var gridViewRows = gGroupLocationSchedule.Rows;
            foreach ( GridViewRow row in gridViewRows.OfType<GridViewRow>() )
            {
                int groupLocationId = int.Parse( gGroupLocationSchedule.DataKeys[row.RowIndex].Value as string );
                GroupLocation groupLocation = groupLocationService.Get( groupLocationId );
                if ( groupLocation != null )
                {
                    foreach ( var fieldCell in row.Cells.OfType<DataControlFieldCell>() )
                    {
                        CheckBoxEditableField checkBoxTemplateField = fieldCell.ContainingField as CheckBoxEditableField;
                        if ( checkBoxTemplateField != null )
                        {
                            CheckBox checkBox = fieldCell.Controls[0] as CheckBox;
                            string dataField = ( fieldCell.ContainingField as CheckBoxEditableField ).DataField;
                            int scheduleId = int.Parse( dataField.Replace( "scheduleField_", string.Empty ) );

                            // update GroupLocationSchedule depending on if the Schedule is Checked or not
                            if ( checkBox.Checked )
                            {
                                // This schedule is selected, so if GroupLocationSchedule doesn't already have this schedule, add it
                                if ( !groupLocation.Schedules.Any( a => a.Id == scheduleId ) )
                                {
                                    var schedule = scheduleService.Get( scheduleId );
                                    groupLocation.Schedules.Add( schedule );
                                }
                            }
                            else
                            {
                                // This schedule is not selected, so if GroupLocationSchedule has this schedule, delete it
                                if ( groupLocation.Schedules.Any( a => a.Id == scheduleId ) )
                                {
                                    groupLocation.Schedules.Remove( groupLocation.Schedules.FirstOrDefault( a => a.Id == scheduleId ) );
                                }
                            }
                        }
                    }
                }
            }

            rockContext.SaveChanges();

            NavigateToParentPage();

        }

        #endregion

        /// <summary>
        /// Handles the SelectItem event of the pCategory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void pCategory_SelectItem( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Category", pCategory.SelectedValueAsId().ToString() );
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gGroupLocationSchedule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gGroupLocationSchedule_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            // add tooltip to header columns
            if ( e.Row.RowType == DataControlRowType.Header )
            {
                var scheduleService = new ScheduleService( new RockContext() );

                foreach ( var cell in e.Row.Cells.OfType<DataControlFieldCell>() )
                {
                    if ( cell.ContainingField is CheckBoxEditableField )
                    {
                        CheckBoxEditableField checkBoxEditableField = cell.ContainingField as CheckBoxEditableField;
                        int scheduleId = int.Parse( checkBoxEditableField.DataField.Replace( "scheduleField_", string.Empty ) );

                        var schedule = scheduleService.Get( scheduleId );
                        if ( schedule != null )
                        {
                            cell.Attributes["title"] = schedule.ToString();
                        }
                    }
                }
            }
        }
    }
}