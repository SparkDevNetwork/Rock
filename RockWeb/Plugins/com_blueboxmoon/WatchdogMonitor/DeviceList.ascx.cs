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
    [DisplayName( "Device List" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "Lists devices in the system." )]

    [LinkedPage( "Detail Page", "The page that allows the user to edit the details of a device.", true, "", "", 0 )]
    public partial class DeviceList : RockBlock
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

            var entityType = new EntityTypeService( new RockContext() ).Get( typeof( WatchdogDevice ).FullName );

            gDevice.DataKeyNames = new string[] { "Id" };
            gDevice.Actions.AddClick += gDevice_Add;
            gDevice.GridRebind += gDevice_GridRebind;
            gDevice.Actions.ShowAdd = entityType.IsAuthorized( Authorization.EDIT, CurrentPerson );
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
            var deviceService = new WatchdogDeviceService( new RockContext() );
            var sortProperty = gDevice.SortProperty;

            var devices = deviceService.Queryable()
                .ToList()
                .Where( d => d.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                .ToList();

            if ( sortProperty != null )
            {
                devices = devices.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                devices = devices.OrderBy( t => t.Name ).ThenBy( t => t.Id ).ToList();
            }

            gDevice.EntityTypeId = EntityTypeCache.Get<WatchdogDevice>().Id;
            gDevice.DataSource = devices;
            gDevice.DataBind();
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
        protected void gDevice_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "DeviceId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDevice_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var deviceService = new WatchdogDeviceService( rockContext );
            var device = deviceService.Get( e.RowKeyId );

            if ( device != null && device.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                deviceService.Delete( device );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gDevice_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDevice_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "DeviceId", 0 );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gDevice_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int deleteIndex = gDevice.Columns.IndexOf( gDevice.Columns.OfType<DeleteField>().Single() );
                var deleteButton = e.Row.Cells[deleteIndex].ControlsOfTypeRecursive<LinkButton>().Single();

                deleteButton.Enabled = deleteButton.Enabled && ( ( WatchdogDevice ) e.Row.DataItem ).IsAuthorized( Authorization.EDIT, CurrentPerson );
            }
        }

        #endregion
    }
}