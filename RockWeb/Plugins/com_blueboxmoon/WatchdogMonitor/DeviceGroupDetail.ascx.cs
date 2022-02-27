using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using com.blueboxmoon.WatchdogMonitor.Cache;
using com.blueboxmoon.WatchdogMonitor.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Device Group Detail" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "View and edit the details of a device group." )]

    public partial class DeviceGroupDetail : RockBlock
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

            gDevice.DataKeyNames = new string[] { "Guid" };
            gDevice.Actions.AddClick += gDevice_Add;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                if ( PageParameter( "DeviceGroupId" ).AsInteger() != 0 )
                {
                    ShowDetails();
                }
                else if ( IsUserAuthorized( Authorization.EDIT ) )
                {
                    ShowEdit( 0 );
                }
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            var deviceGroupId = PageParameter( "DeviceGroupId" ).AsInteger();
            var group = new WatchdogDeviceGroupService( new RockContext() ).Get( deviceGroupId );

            if ( group == null || !group.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                nbWarningMessage.Text = Rock.Constants.EditModeMessage.NotAuthorizedToView( typeof( WatchdogDeviceGroup ).GetFriendlyTypeName() );
                pnlDetails.Visible = false;
                pnlDevices.Visible = false;
                pnlEdit.Visible = false;

                return;
            }

            lName.Text = group.Name;
            lDescription.Text = group.Description.EncodeHtml();

            lbEdit.Visible = group.IsAuthorized( Authorization.EDIT, CurrentPerson );
            gDevice.Actions.ShowAdd = lbEdit.Visible;
            gDevice.Columns.OfType<DeleteField>().Single().Visible = lbEdit.Visible;

            BindDeviceGrid();

            pnlDetails.Visible = true;
            pnlDevices.Visible = true;
            pnlEdit.Visible = false;
        }

        /// <summary>
        /// Binds the devices grid.
        /// </summary>
        private void BindDeviceGrid()
        {
            var rockContext = new RockContext();
            var deviceGroupId = PageParameter( "DeviceGroupId" ).AsInteger();
            var group = new WatchdogDeviceGroupService( new RockContext() ).Get( deviceGroupId );

            gDevice.DataSource = group.Devices.ToList();
            gDevice.DataBind();
        }

        /// <summary>
        /// Shows the edit dialog.
        /// </summary>
        /// <param name="id">The identifier.</param>
        private void ShowEdit( int id )
        {
            var rockContext = new RockContext();
            var group = new WatchdogDeviceGroupService( rockContext ).Get( id );

            if ( group == null )
            {
                group = new WatchdogDeviceGroup();
            }

            if ( !group.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                nbWarningMessage.Text = Rock.Constants.EditModeMessage.NotAuthorizedToEdit( typeof( WatchdogDeviceGroup ).GetFriendlyTypeName() );
                pnlDetails.Visible = false;
                pnlDevices.Visible = false;
                pnlEdit.Visible = false;

                return;
            }

            hfEditId.Value = group.Id.ToString();
            tbEditName.Text = group.Name;
            tbEditDescription.Text = group.Description;

            lEditTitle.Text = group.Id == 0 ? "Add Device Group" : "Edit Device Group";
            pnlDetails.Visible = false;
            pnlDevices.Visible = false;
            pnlEdit.Visible = true;
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
            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEdit( PageParameter( "DeviceGroupId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var watchdogDeviceGroupService = new WatchdogDeviceGroupService( rockContext );
            var group = watchdogDeviceGroupService.Get( hfEditId.ValueAsInt() );

            if ( group == null )
            {
                group = new WatchdogDeviceGroup();
                watchdogDeviceGroupService.Add( group );
            }

            group.Name = tbEditName.Text;
            group.Description = tbEditDescription.Text;

            rockContext.SaveChanges();
            WatchdogDeviceGroupCache.Remove( group.Id );

            NavigateToCurrentPage( new Dictionary<string, string> { { "DeviceGroupId", group.Id.ToString() } } );
        }

        /// <summary>
        /// Handles the Click event of the lbCancelSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancelSave_Click( object sender, EventArgs e )
        {
            if ( hfEditId.Value.AsInteger() == 0 )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowDetails();
            }
        }

        /// <summary>
        /// Handles the Add event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gDevice_Add( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var deviceGroupId = PageParameter( "DeviceGroupId" ).AsInteger();
            var group = new WatchdogDeviceGroupService( new RockContext() ).Get( deviceGroupId );

            ddlAddDevice.Items.Clear();
            ddlAddDevice.Items.Add( Rock.Constants.None.ListItem );

            var deviceIds = group.Devices.Select( d => d.Id );
            var devices = new WatchdogDeviceService( rockContext ).Queryable().AsNoTracking()
                .Where( d => !deviceIds.Contains( d.Id ) )
                .ToList()
                .Where( d => d.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                .ToList();

            devices.ForEach( d => ddlAddDevice.Items.Add( new ListItem( d.Name, d.Guid.ToString() ) ) );

            mdlAdd.Show();
        }

        /// <summary>
        /// Handles the GridRebind event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gDevice_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindDeviceGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gDevice_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var deviceGuid = ( Guid ) e.RowKeyValue;
            var deviceGroupId = PageParameter( "DeviceGroupId" ).AsInteger();
            var group = new WatchdogDeviceGroupService( rockContext ).Get( deviceGroupId );

            group.Devices.Remove( group.Devices.First( d => d.Guid == deviceGuid ) );

            rockContext.SaveChanges();
            WatchdogDeviceGroupCache.Remove( deviceGroupId );

            BindDeviceGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdlAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void mdlAdd_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var deviceGuid = ddlAddDevice.SelectedValue.AsGuid();
            var deviceGroupId = PageParameter( "DeviceGroupId" ).AsInteger();
            var group = new WatchdogDeviceGroupService( rockContext ).Get( deviceGroupId );

            group.Devices.Add( new WatchdogDeviceService( rockContext ).Get( deviceGuid ) );

            rockContext.SaveChanges();
            WatchdogDeviceGroupCache.Remove( deviceGroupId );

            BindDeviceGrid();
            mdlAdd.Hide();
        }

        #endregion
    }
}
