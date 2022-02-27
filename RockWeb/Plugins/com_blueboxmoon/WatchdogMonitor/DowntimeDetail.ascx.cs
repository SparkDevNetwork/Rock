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

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Downtime Detail" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "View and edit the details of a single downtime schedule." )]

    public partial class DowntimeDetail : RockBlock
    {
        #region Properties

        private List<NameValueEntity> DeviceListState { get; set; }

        private List<NameValueEntity> DeviceGroupListState { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            DeviceListState = ViewState["DeviceList"] as List<NameValueEntity>;
            DeviceGroupListState = ViewState["DeviceGroupList"] as List<NameValueEntity>;
        }

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                if ( PageParameter( "DowntimeId" ).AsInteger() != 0 )
                {
                    ShowDetails();
                }
                else if ( IsUserAuthorized( Authorization.EDIT ) )
                {
                    ShowEdit( 0 );
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["DeviceList"] = DeviceListState;
            ViewState["DeviceGroupList"] = DeviceGroupListState;

            return base.SaveViewState();
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            var downtimeId = PageParameter( "DowntimeId" ).AsInteger();
            var downtime = new WatchdogDowntimeService( new RockContext() ).Get( downtimeId );

            if ( downtime == null || !downtime.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                nbWarningMessage.Text = Rock.Constants.EditModeMessage.NotAuthorizedToView( typeof( WatchdogDowntime ).GetFriendlyTypeName() );
                pnlDetails.Visible = false;
                pnlEdit.Visible = false;

                return;
            }

            lName.Text = downtime.Name;
            hlInactive.Visible = !downtime.IsActive;
            lDescription.Text = downtime.Description.EncodeHtml();
            lSchedule.Text = downtime.Schedule != null ? downtime.Schedule.Name : string.Empty;

            lDevices.Text = string.Join( "<br>", downtime.Devices.Select( d => d.Name ) );
            lDeviceGroups.Text = string.Join( "<br>", downtime.DeviceGroups.Select( g => g.Name ) );

            lbEdit.Visible = downtime.IsAuthorized( Authorization.EDIT, CurrentPerson );
            pnlDetails.Visible = true;
            pnlEdit.Visible = false;
        }

        /// <summary>
        /// Shows the edit dialog.
        /// </summary>
        /// <param name="id">The identifier.</param>
        private void ShowEdit( int id )
        {
            var rockContext = new RockContext();
            var downtime = new WatchdogDowntimeService( rockContext ).Get( id );

            if ( downtime == null )
            {
                downtime = new WatchdogDowntime()
                {
                    IsActive = true
                };
            }

            hfEditId.Value = downtime.Id.ToString();
            tbEditName.Text = downtime.Name;
            cbEditIsActive.Checked = downtime.IsActive;
            tbEditDescription.Text = downtime.Description;

            //
            // Bind the schedules drop down list.
            //
            ddlEditSchedule.Items.Clear();
            ddlEditSchedule.Items.Add( new ListItem() );
            new WatchdogScheduleCollectionService( rockContext ).Queryable().AsNoTracking()
                .Select( d => new
                {
                    d.Id,
                    d.Name
                } )
                .ToList()
                .ForEach( d => ddlEditSchedule.Items.Add( new ListItem( d.Name, d.Id.ToString() ) ) );
            ddlEditSchedule.SetValue( downtime.ScheduleId );

            //
            // Set the devices repeater.
            //
            DeviceListState = downtime.Devices
                .Select( d => new NameValueEntity( d.Id, d.Name ) )
                .OrderBy( n => n.Name )
                .ToList();
            BindDevicesRepeater();

            //
            // Set the device groups repeater.
            //
            DeviceGroupListState = downtime.DeviceGroups
                .Select( g => new NameValueEntity( g.Id, g.Name ) )
                .OrderBy( n => n.Name )
                .ToList();
            BindDeviceGroupsRepeater();

            //
            // Bind the device groups drop down list.
            //
            ddlDeviceGroups.Items.Clear();
            ddlDeviceGroups.Items.Add( new ListItem() );
            new WatchdogDeviceGroupService( rockContext ).Queryable().AsNoTracking()
                .ToList()
                .ForEach( g => ddlDeviceGroups.Items.Add( new ListItem( g.Name, g.Id.ToString() ) ) );

            //
            // Bind the devices drop down list.
            //
            ddlDevices.Items.Clear();
            ddlDevices.Items.Add( new ListItem() );
            new WatchdogDeviceService( rockContext ).Queryable().AsNoTracking()
                .Select( d => new
                {
                    d.Id,
                    d.Name
                } )
                .ToList()
                .ForEach( d => ddlDevices.Items.Add( new ListItem( d.Name, d.Id.ToString() ) ) );

            nbEditError.Text = string.Empty;

            pnlDetails.Visible = false;
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
            ShowEdit( PageParameter( "DowntimeId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var downtimeService = new WatchdogDowntimeService( rockContext );
            var downtime = downtimeService.Get( hfEditId.ValueAsInt() );

            if ( downtime == null )
            {
                downtime = new WatchdogDowntime();
                downtimeService.Add( downtime );
            }

            if ( downtime.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                downtime.Name = tbEditName.Text;
                downtime.IsActive = cbEditIsActive.Checked;
                downtime.ScheduleId = ddlEditSchedule.SelectedValueAsId().Value;
                downtime.Description = tbEditDescription.Text;

                //
                // Update the devices.
                //
                downtime.Devices.Clear();
                var deviceService = new WatchdogDeviceService( rockContext );
                DeviceListState.ForEach( a => downtime.Devices.Add( deviceService.Get( a.Id ) ) );

                //
                // Update the device groups.
                //
                downtime.DeviceGroups.Clear();
                var deviceGroupService = new WatchdogDeviceGroupService( rockContext );
                DeviceGroupListState.ForEach( a => downtime.DeviceGroups.Add( deviceGroupService.Get( a.Id ) ) );

                nbEditError.Text = string.Empty;
                if ( !downtime.IsValid )
                {
                    nbEditError.Text = "<ul><li>" + string.Join( "</li><li>", downtime.ValidationResults.Select( v => v.ErrorMessage ) ) + "</li></ul>";
                    return;
                }

                rockContext.SaveChanges();
                WatchdogDowntimeCache.Remove( downtime.Id );
            }

            NavigateToCurrentPage( new Dictionary<string, string> { { "DowntimeId", downtime.Id.ToString() } } );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
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

        #endregion

        #region Devices Repeater

        /// <summary>
        /// Handles the ItemCommand event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rpDevices_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int personId = e.CommandArgument.ToString().AsInteger();

            if ( e.CommandName == "Delete" )
            {
                var nve = DeviceListState.Where( a => a.Id == personId ).FirstOrDefault();

                if ( nve != null )
                {
                    DeviceListState.Remove( nve );
                    BindDevicesRepeater();
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlDevices control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlDevices_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( ddlDevices.SelectedValueAsId().HasValue && !DeviceListState.Any( d => d.Id == ddlDevices.SelectedValueAsId().Value ) )
            {
                DeviceListState.Add( new NameValueEntity( ddlDevices.SelectedValueAsId().Value, ddlDevices.SelectedItem.Text ) );
            }

            ddlDevices.SetValue( ( int? ) null );
            BindDevicesRepeater();
        }

        /// <summary>
        /// Binds the devices repeater.
        /// </summary>
        private void BindDevicesRepeater()
        {
            rpDevices.DataSource = DeviceListState;
            rpDevices.DataBind();
        }

        #endregion

        #region Device Groups Repeater

        /// <summary>
        /// Handles the ItemCommand event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rpDeviceGroups_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int personId = e.CommandArgument.ToString().AsInteger();

            if ( e.CommandName == "Delete" )
            {
                var nve = DeviceGroupListState.Where( a => a.Id == personId ).FirstOrDefault();

                if ( nve != null )
                {
                    DeviceGroupListState.Remove( nve );
                    BindDeviceGroupsRepeater();
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlDeviceGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlDeviceGroups_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( ddlDeviceGroups.SelectedValueAsId().HasValue && !DeviceGroupListState.Any( d => d.Id == ddlDeviceGroups.SelectedValueAsId().Value ) )
            {
                DeviceGroupListState.Add( new NameValueEntity( ddlDeviceGroups.SelectedValueAsId().Value, ddlDeviceGroups.SelectedItem.Text ) );
            }

            ddlDeviceGroups.SetValue( ( int? ) null );
            BindDeviceGroupsRepeater();
        }

        /// <summary>
        /// Binds the device groups repeater.
        /// </summary>
        private void BindDeviceGroupsRepeater()
        {
            rpDeviceGroups.DataSource = DeviceGroupListState;
            rpDeviceGroups.DataBind();
        }

        #endregion

        #region Private Classes

        [Serializable]
        private class NameValueEntity
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public NameValueEntity( int id, string name )
            {
                Id = id;
                Name = name;
            }
        }

        #endregion
    }
}