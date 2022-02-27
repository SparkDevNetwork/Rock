using System;
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
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Device Group List" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "Lists device groups in the system." )]

    [LinkedPage( "Detail Page", "The page that allows the user to edit the details of a device group.", true, "", "", 0 )]
    public partial class DeviceGroupList : RockBlock
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

            var entityType = new EntityTypeService( new RockContext() ).Get( typeof( WatchdogDeviceGroup ).FullName );

            gDeviceGroup.DataKeyNames = new string[] { "Id" };
            gDeviceGroup.Actions.AddClick += gDeviceGroup_Add;
            gDeviceGroup.GridRebind += gDeviceGroup_GridRebind;
            gDeviceGroup.Actions.ShowAdd = entityType.IsAuthorized( Authorization.EDIT, CurrentPerson );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Bind the data grid to the list of project types in the system.
        /// </summary>
        private void BindGrid()
        {
            var watchdogDeviceGroupService = new WatchdogDeviceGroupService( new RockContext() );
            int deviceEntityTypeId = EntityTypeCache.Get<WatchdogDevice>().Id;
            var sortProperty = gDeviceGroup.SortProperty;

            var groups = watchdogDeviceGroupService.Queryable().AsNoTracking()
                .ToList()
                .Where( g => g.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                .ToList();

            if ( sortProperty != null )
            {
                groups = groups.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                groups = groups.OrderBy( t => t.Name ).ThenBy( t => t.Id ).ToList();
            }

            gDeviceGroup.EntityTypeId = EntityTypeCache.Get<WatchdogDeviceGroup>().Id;
            gDeviceGroup.DataSource = groups;
            gDeviceGroup.DataBind();
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
        protected void gDeviceGroup_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "DeviceGroupId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDeviceGroup_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var watchdogDeviceGroupService = new WatchdogDeviceGroupService( rockContext );
            var group = watchdogDeviceGroupService.Get( e.RowKeyId );

            if ( group != null && group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                watchdogDeviceGroupService.Delete( group );
                rockContext.SaveChanges();

                WatchdogDeviceGroupCache.Get( e.RowKeyId );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gDeviceGroup_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDeviceGroup_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "DeviceGroupId", 0 );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gDeviceGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gDeviceGroup_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int deleteIndex = gDeviceGroup.Columns.IndexOf( gDeviceGroup.Columns.OfType<DeleteField>().Single() );
                var deleteButton = e.Row.Cells[deleteIndex].ControlsOfTypeRecursive<LinkButton>().Single();

                deleteButton.Enabled = deleteButton.Enabled && ( ( WatchdogDeviceGroup ) e.Row.DataItem ).IsAuthorized( Authorization.EDIT, CurrentPerson );
            }
        }

        #endregion
    }
}