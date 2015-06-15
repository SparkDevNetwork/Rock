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
using Rock.CheckIn;
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
    [DisplayName( "Check-in Scheduled Locations" )]
    [Category( "Check-in" )]
    [Description( "Helps to enable/disable schedules associated with the configured group types at a kiosk" )]
    public partial class CheckinScheduledLocations : Rock.CheckIn.CheckInBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

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
                if (this.ManagerLoggedIn)
                {
                    BindGrid();
                }
                else
                {
                    NavigateToHomePage();
                }
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
            int scheduleCategoryId = CategoryCache.Read( Rock.SystemGuid.Category.SCHEDULE_SERVICE_TIMES.AsGuid() ).Id;

            scheduleQry = scheduleQry.Where( a => a.CategoryId == scheduleCategoryId );

            // clear out any existing schedule columns just in case schedules been added/removed
            var scheduleList = scheduleQry.ToList().OrderBy( a => a.ToString() ).ToList();

            var checkBoxEditableFields = gGroupLocationSchedule.Columns.OfType<CheckBoxEditableField>().ToList();
            foreach ( var field in checkBoxEditableFields )
            {
                gGroupLocationSchedule.Columns.Remove( field );
            }

            foreach ( var item in scheduleList )
            {
                string dataFieldName = string.Format( "scheduleField_{0}", item.Id );

                CheckBoxEditableField field = new CheckBoxEditableField { HeaderText = item.FriendlyScheduleText.Replace( " at ", "<br/>" ), DataField = dataFieldName };

                gGroupLocationSchedule.Columns.Add( field );
            }
        }

        #endregion

        #region grid

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

            List<int> currentAndDescendantGroupTypeIds = new List<int>();
            var currentGroupTypeIds = this.CurrentGroupTypeIds.ToList();
            currentAndDescendantGroupTypeIds.AddRange( currentGroupTypeIds );
            foreach ( var templateGroupType in groupTypeService.Queryable().Where( a => currentGroupTypeIds.Contains( a.Id ) ) )
            {
                foreach ( var childGroupType in groupTypeService.GetChildGroupTypes( templateGroupType.Id ) )
                {
                    currentAndDescendantGroupTypeIds.Add( childGroupType.Id );
                    currentAndDescendantGroupTypeIds.AddRange( groupTypeService.GetAllAssociatedDescendents( childGroupType.Id ).Select( a => a.Id ).ToList() );
                }
            }

            groupLocationQry = groupLocationQry.Where( a => currentAndDescendantGroupTypeIds.Contains( a.Group.GroupTypeId ) );

            groupLocationQry = groupLocationQry.OrderBy( a => a.Group.Name ).ThenBy( a => a.Location.Name );

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
            NavigateToHomePage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                GroupLocationService groupLocationService = new GroupLocationService( rockContext );
                ScheduleService scheduleService = new ScheduleService( rockContext );
                bool schedulesChanged = false;

                var gridViewRows = gGroupLocationSchedule.Rows;
                foreach ( GridViewRow row in gridViewRows.OfType<GridViewRow>() )
                {
                    int groupLocationId = int.Parse( gGroupLocationSchedule.DataKeys[row.RowIndex].Value as string );
                    GroupLocation groupLocation = groupLocationService.Get( groupLocationId );
                    if ( groupLocation != null )
                    {
                        foreach ( var fieldCell in row.Cells.OfType<DataControlFieldCell>() )
                        {
                            var checkBoxTemplateField = fieldCell.ContainingField as CheckBoxEditableField;
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
                                        schedulesChanged = true;
                                    }
                                }
                                else
                                {
                                    // This schedule is not selected, so if GroupLocationSchedule has this schedule, delete it
                                    if ( groupLocation.Schedules.Any( a => a.Id == scheduleId ) )
                                    {
                                        groupLocation.Schedules.Remove( groupLocation.Schedules.FirstOrDefault( a => a.Id == scheduleId ) );
                                        schedulesChanged = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if ( schedulesChanged )
                {
                    rockContext.SaveChanges();
                    if ( this.CurrentKioskId.HasValue )
                    {
                        KioskDevice.Flush( this.CurrentKioskId.Value );
                    }
                }
            }

            NavigateToHomePage();
        }

        #endregion
    }
}