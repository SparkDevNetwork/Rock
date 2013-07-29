//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Web.UI;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock;
using System.Web.UI.WebControls;
using System.Data;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
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

            ScheduleService scheduleService = new ScheduleService();

            // limit Schedules to ones that have a CheckInStartOffsetMinutes
            var scheduleQry = scheduleService.Queryable().Where( a => a.CheckInStartOffsetMinutes != null );

            // limit Schedules to the Category from the Filter
            int? scheduleCategoryId = pCategory.SelectedValueAsInt();
            if ( scheduleCategoryId.HasValue )
            {
                // open question on how [All] would be selected 
                if ( scheduleCategoryId != Rock.Constants.All.Id )
                {
                    scheduleQry = scheduleQry.Where( a => a.CategoryId == scheduleCategoryId );
                }
            }
            else
            {
                // NULL means Shared, so specifically filter so to show only Schedules with CategoryId NULL
                scheduleQry = scheduleQry.Where( a => a.CategoryId == null );
            }

            var scheduleList = scheduleQry.OrderBy( a => a.Name ).Select( a => new { a.Id, a.Name } ).ToList();

            foreach ( var item in scheduleList )
            {
                string dataFieldName = string.Format( "scheduleField_{0}", item.Id );
                if ( !gGroupLocationSchedule.Columns.OfType<CheckBoxField>().Any( a => a.DataField.Equals( dataFieldName ) ) )
                {
                    CheckBoxField field = new CheckBoxField { DataField = dataFieldName, HeaderText = item.Name };
                    gGroupLocationSchedule.Columns.Add( field );
                }
                else
                {
                    // temp to see if this actually would ever happen
                    var temp = true;
                }
            }

            gGroupLocationSchedule.DataKeyNames = new string[] { "GroupLocationId" };
            gGroupLocationSchedule.Actions.ShowAdd = false;
            gGroupLocationSchedule.IsDeleteEnabled = false;
            gGroupLocationSchedule.GridRebind += gGroupLocationSchedule_GridRebind;
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

        #endregion

        #region Grid Events

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            var groupLocationService = new GroupLocationService();

            var groupLocationQry = groupLocationService.Queryable();

            int? parentLocationId = ddlParentLocation.SelectedValueAsInt();
            if ( parentLocationId.HasValue )
            {
                // open question on whether to also include all descendants instead of just immediate children
                groupLocationQry = groupLocationQry.Where( a => a.Location.ParentLocationId == parentLocationId );
            }

            var qryList = groupLocationQry.Select( a =>
                new
                {
                    GroupLocationId = a.Id,
                    GroupNameLocationName = a.Group.Name + " - " + a.Location.Name,
                    GroupName = a.Group.Name,
                    LocationName = a.Location.Name,
                    ScheduleIdList = a.Schedules.Select( s => s.Id )
                } ).ToList();

            // put stuff in a datatable so we can dynamically have columns for each Schedule
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add( "GroupLocationId" );
            dataTable.Columns.Add( "GroupNameLocationName" );
            dataTable.Columns.Add( "GroupName" );
            dataTable.Columns.Add( "LocationName" );
            foreach ( var field in gGroupLocationSchedule.Columns.OfType<CheckBoxField>() )
            {
                dataTable.Columns.Add( field.DataField );
            };

            foreach ( var row in qryList )
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["GroupLocationId"] = row.GroupLocationId;
                dataRow["GroupNameLocationName"] = row.GroupNameLocationName;
                dataRow["GroupName"] = row.GroupName;
                dataRow["LocationName"] = row.LocationName;
                foreach ( var field in gGroupLocationSchedule.Columns.OfType<CheckBoxField>() )
                {
                    int scheduleId = int.Parse( field.DataField.Replace( "scheduleField_", string.Empty ) );
                    dataRow[field.DataField] = row.ScheduleIdList.Any( a => a == scheduleId );
                }

                dataTable.Rows.Add( dataRow );
            }

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
            //todo

            NavigateToParentPage();
        }

        #endregion

    }
}