using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using com.blueboxmoon.WatchdogMonitor.Cache;
using com.blueboxmoon.WatchdogMonitor.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Schedule Collection List" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "View the list of monitoring schedules." )]

    [LinkedPage( "Detail Page", "The page that allows the user to edit the details of a schedule.", true, "", "", 0 )]
    public partial class ScheduleCollectionList : RockBlock
    {
        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var entityType = new EntityTypeService( new RockContext() ).Get( typeof( WatchdogScheduleCollection ).FullName );

            gSchedules.DataKeyNames = new string[] { "Id" };
            gSchedules.Actions.AddClick += gSchedules_AddClick;
            gSchedules.Actions.ShowAdd = entityType.IsAuthorized( Authorization.EDIT, CurrentPerson );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if (!IsPostBack)
            {
                BindGrid();
            }
        }

        #endregion

        #region Methds

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            var scheduleService = new WatchdogScheduleCollectionService( new RockContext() );

            var schedules = scheduleService.Queryable().AsNoTracking()
                .ToList()
                .Where( s => s.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                .Select( s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    Schedule = FriendlyScheduleText( s ),
                    CanDelete = s.IsAuthorized( Authorization.EDIT, CurrentPerson )
                } ).ToList();

            gSchedules.DataSource = schedules;
            gSchedules.DataBind();
        }

        /// <summary>
        /// Friendlies the schedule text.
        /// </summary>
        /// <param name="calendar">The calendar.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        protected string FriendlyScheduleText( WatchdogScheduleCollection schedule, bool condensed = true )
        {
            List<string> schedules = new List<string>();
            try
            {
                schedules = schedule.ScheduleJson.FromJsonOrNull<List<string>>();
            }
            catch
            {
                schedules = null;
            }
            if ( schedules == null )
            {
                schedules = new List<string>();
            }

            return string.Join( "<br>", schedules.Select( s => new Rock.Model.Schedule { iCalendarContent = s }.ToFriendlyScheduleText( condensed ) ) );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the AddClick event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gSchedules_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "ScheduleId", 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gSchedules_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "ScheduleId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gSchedules_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gSchedules_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int deleteIndex = gSchedules.Columns.IndexOf( gSchedules.Columns.OfType<DeleteField>().Single() );
                var deleteButton = e.Row.Cells[deleteIndex].ControlsOfTypeRecursive<LinkButton>().Single();

                deleteButton.Enabled = deleteButton.Enabled && ( bool ) e.Row.DataItem.GetPropertyValue( "CanDelete" );
            }
        }

        /// <summary>
        /// Handles the Click event of the gSchedulesDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gSchedulesDelete_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var scheduleService = new WatchdogScheduleCollectionService( rockContext );
            var schedule = scheduleService.Get( e.RowKeyId );

            if ( schedule != null && schedule.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                scheduleService.Delete( schedule );

                rockContext.SaveChanges();
                WatchdogScheduleCollectionCache.Remove( e.RowKeyId );
            }

            BindGrid();
        }

        #endregion
    }
}
