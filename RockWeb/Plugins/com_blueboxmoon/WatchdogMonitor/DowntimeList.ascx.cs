using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using com.blueboxmoon.WatchdogMonitor.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Downtime List" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "Lists downtimes in the system." )]

    [LinkedPage( "Detail Page", "The page that allows the user to edit the details of a downtime.", true, "", "", 0 )]
    public partial class DowntimeList : RockBlock
    {
        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            var entityType = new EntityTypeService( new RockContext() ).Get( typeof( WatchdogDowntime ).FullName );

            gDowntime.DataKeyNames = new string[] { "Id" };
            gDowntime.Actions.AddClick += gDowntime_Add;
            gDowntime.GridRebind += gDowntime_GridRebind;
            gDowntime.Actions.ShowAdd = entityType.IsAuthorized( Authorization.EDIT, CurrentPerson );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Bind the data grid to the list of downtimes in the system.
        /// </summary>
        private void BindGrid()
        {
            var downtimeService = new WatchdogDowntimeService( new RockContext() );
            var sortProperty = gDowntime.SortProperty;

            var downtimes = downtimeService.Queryable()
                .ToList()
                .Where( d => d.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                .ToList();

            if ( sortProperty != null )
            {
                downtimes = downtimes.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                downtimes = downtimes.OrderBy( t => t.Name ).ThenBy( t => t.Id ).ToList();
            }

            gDowntime.EntityTypeId = EntityTypeCache.Get<WatchdogDowntime>().Id;
            gDowntime.DataSource = downtimes;
            gDowntime.DataBind();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDowntime_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "DowntimeId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDowntime_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var downtimeService = new WatchdogDowntimeService( rockContext );
            var downtime = downtimeService.Get( e.RowKeyId );

            if ( downtime != null && downtime.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                downtimeService.Delete( downtime );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gDowntime_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDowntime_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "DowntimeId", 0 );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gDowntime control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gDowntime_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int deleteIndex = gDowntime.Columns.IndexOf( gDowntime.Columns.OfType<DeleteField>().Single() );
                var deleteButton = e.Row.Cells[deleteIndex].ControlsOfTypeRecursive<LinkButton>().Single();

                deleteButton.Enabled = deleteButton.Enabled && ( bool ) ( ( WatchdogServiceCheckEvent ) e.Row.DataItem ).IsAuthorized( Authorization.EDIT, CurrentPerson );
            }
        }

        #endregion
    }
}