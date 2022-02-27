using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using com.blueboxmoon.WatchdogMonitor.Cache;
using com.blueboxmoon.WatchdogMonitor.Checks.Support.NRPE;
using com.blueboxmoon.WatchdogMonitor.Checks.Support.SNMP;
using com.blueboxmoon.WatchdogMonitor.Model;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Device Detail" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "View and edit the details of a single device." )]

    [LinkedPage( "Service Check Page", "The page that allows the user to view the details of a service check.", true, "", "", 0 )]
    [LinkedPage( "Event Page", "The page that allows the user to view events related to this device.", true, "", "", 1 )]
    public partial class DeviceDetail: RockBlock
    {
        #region Properties

        private List<NameValueEntity> GroupListState { get; set; }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            GroupListState = ViewState["GroupList"] as List<NameValueEntity>;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/moment.min.js" );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/WatchdogMonitor/Scripts/hammer.min.js" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/WatchdogMonitor/Scripts/chartjs-plugin-zoom.min.js" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/WatchdogMonitor/Scripts/pluralize.min.js" );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/WatchdogMonitor/Scripts/Graphs.js" );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gServiceChecks.DataKeyNames = new[] { "Id" };
            gRecentEvents.DataKeyNames = new[] { "Id" };
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                if ( PageParameter( "DeviceId" ).AsInteger() != 0 )
                {
                    ShowDetails();
                }
                else if ( IsUserAuthorized( Authorization.EDIT))
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
            ViewState["GroupList"] = GroupListState;

            return base.SaveViewState();
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="T:Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.List`1" /> of block related <see cref="T:Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            var device = new WatchdogDeviceService( new RockContext() ).Get( PageParameter( pageReference, "DeviceId" ).AsInteger() );
            if ( device != null )
            {
                breadCrumbs.Add( new BreadCrumb( device.Name, pageReference ) );
            }
            else
            {
                var serviceCheck = new WatchdogServiceCheckService( new RockContext() ).Get( PageParameter( pageReference, "ServiceCheckId" ).AsInteger() );
                if ( serviceCheck != null )
                {
                    breadCrumbs.Add( new BreadCrumb( serviceCheck.Device.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "Add Device" ) );
                }
            }

            return breadCrumbs;
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            var deviceId = PageParameter( "DeviceId" ).AsInteger();
            var device = new WatchdogDeviceService( new RockContext() ).Get( deviceId );

            if ( device == null || !device.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                nbWarningMessage.Text = Rock.Constants.EditModeMessage.NotAuthorizedToView( typeof( WatchdogDevice ).GetFriendlyTypeName() );
                pnlDetails.Visible = false;
                pnlServiceChecks.Visible = false;
                pnlRecentEvents.Visible = false;
                pnlGraphs.Visible = false;
                pnlEdit.Visible = false;

                return;
            }

            lDetailsName.Text = device.Name;
            lDetailsAddress.Text = device.Address;
            lDetailsParent.Text = device.ParentDevice != null ? device.ParentDevice.Name : string.Empty;
            hlDetailsProfile.Text = device.DeviceProfile.Name;
            lDetailsDescription.Text = device.Description.EncodeHtml();
            lDetailsCollector.Text = device.Collector.Value;

            lDetailsIcon.Text = string.Empty;
            if ( !string.IsNullOrWhiteSpace( device.DeviceProfile.IconCssClass ) )
            {
                lDetailsIcon.Text = string.Format( "<i class='{0}'></i>", device.DeviceProfile.IconCssClass );
            }

            hlDetailsInactive.Visible = !device.IsActive;
            lDetailsOverallState.Text = WatchdogServiceCheck.GetLabelForState( device.OverallState, device.OverallSoftState );
            lDetailsDeviceState.Text = WatchdogServiceCheck.GetLabelForState( device.State, device.SoftState );

            lDetailsGroups.Text = string.Join( " ", device.DeviceGroups.Select( g => string.Format( "<span class='label label-default'>{0}</span>", g.Name ) ) );

            BindServiceChecks();
            BindRecentEventsGrid();

            lbDetailsEdit.Visible = device.IsAuthorized( Authorization.EDIT, CurrentPerson );

            pnlDetails.Visible = true;
            pnlServiceChecks.Visible = true;
            pnlRecentEvents.Visible = true;
            pnlGraphs.Visible = true;
            pnlEdit.Visible = false;
        }

        /// <summary>
        /// Binds the sevice checks controls.
        /// </summary>
        private void BindServiceChecks()
        {
            var deviceId = PageParameter( "DeviceId" ).AsInteger();
            var serviceChecks = new WatchdogServiceCheckService( new RockContext() ).Queryable()
                .AsNoTracking()
                .Where( s => s.DeviceId == deviceId )
                .OrderBy( s => s.ServiceCheckType.Name )
                .ToList()
                .Select( s => new
                {
                    s.Id,
                    s.ServiceCheckType.Name,
                    s.FormattedValue,
                    StateHtml = WatchdogServiceCheck.GetLabelForState( s.State, s.SoftState ),
                    s.LastCheckDateTime,
                    s.Summary
                } )
                .ToList();

            gServiceChecks.DataSource = serviceChecks;
            gServiceChecks.DataBind();
            rpGraphs.DataSource = serviceChecks;
            rpGraphs.DataBind();
        }

        /// <summary>
        /// Binds the recent events grid.
        /// </summary>
        private void BindRecentEventsGrid()
        {
            var deviceId = PageParameter( "DeviceId" ).AsInteger();
            var rockContext = new RockContext();
            var eventService = new WatchdogServiceCheckEventService( rockContext );
            var limitDate = RockDateTime.Now.AddDays( -7 );

            var events = new WatchdogServiceCheckEventService( new RockContext() ).Queryable()
                .AsNoTracking()
                .Where( e => e.ServiceCheck.DeviceId == deviceId )
                .Where( e => !e.EndDateTime.HasValue || e.EndDateTime >= limitDate )
                .OrderBy( e => e.EndDateTime.HasValue )
                .ThenByDescending( e => e.EndDateTime )
                .ToList()
                .Select( e => new
                {
                    e.Id,
                    ServiceCheckName = e.ServiceCheck.ServiceCheckType.Name,
                    State = !e.EndDateTime.HasValue ? WatchdogServiceCheck.GetLabelForState( e.State ) : e.State.ToString(),
                    e.StartDateTime,
                    e.EndDateTime,
                    e.LastSummary,
                    e.IsSilenced
                } )
                .ToList();

            gRecentEvents.DataSource = events;
            gRecentEvents.DataBind();
        }

        /// <summary>
        /// Shows the edit dialog.
        /// </summary>
        /// <param name="id">The identifier.</param>
        private void ShowEdit( int id )
        {
            var rockContext = new RockContext();
            var device = new WatchdogDeviceService( rockContext ).Get( id );

            if ( device == null )
            {
                device = new WatchdogDevice
                {
                    IsActive = true
                };
            }

            if ( !device.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                nbWarningMessage.Text = Rock.Constants.EditModeMessage.NotAuthorizedToEdit( typeof( WatchdogDevice ).GetFriendlyTypeName() );
                pnlDetails.Visible = false;
                pnlServiceChecks.Visible = false;
                pnlRecentEvents.Visible = false;
                pnlGraphs.Visible = false;
                pnlEdit.Visible = false;
            }

            hfEditId.Value = device.Id.ToString();
            tbEditName.Text = device.Name;
            cbEditIsActive.Checked = device.IsActive;
            tbEditAddress.Text = device.Address;
            tbEditDescription.Text = device.Description;

            ddlEditProfile.Items.Clear();
            ddlEditProfile.Items.Add( new ListItem() );
            foreach ( var profile in new WatchdogDeviceProfileService( rockContext ).Queryable().OrderBy( p => p.Name ) )
            {
                ddlEditProfile.Items.Add( new ListItem( profile.Name, profile.Id.ToString() ) );
            }
            ddlEditProfile.SetValue( device.DeviceProfileId );

            ddlEditParentDevice.Items.Clear();
            ddlEditParentDevice.Items.Add( new ListItem() );
            foreach ( var d in new WatchdogDeviceService( rockContext ).Queryable().OrderBy( s => s.Name ) )
            {
                ddlEditParentDevice.Items.Add( new ListItem( d.Name, d.Id.ToString() ) );
            }
            ddlEditParentDevice.SetValue( device.ParentDeviceId );

            if ( !string.IsNullOrWhiteSpace( device.SnmpSettingsJson ) )
            {
                sseEditSnmpSettings.Value = device.SnmpSettingsJson.FromJsonOrNull<SnmpConfiguration>();
                cbEditOverrideSnmp.Checked = true;
                sseEditSnmpSettings.Visible = true;
            }
            else
            {
                cbEditOverrideSnmp.Checked = false;
                sseEditSnmpSettings.Visible = false;
            }

            if ( !string.IsNullOrWhiteSpace( device.NrpeSettingsJson ) )
            {
                nseEditNrpeSettings.Value = device.NrpeSettingsJson.FromJsonOrNull<NrpeConfiguration>();
                cbEditOverrideNrpe.Checked = true;
                nseEditNrpeSettings.Visible = true;
            }
            else
            {
                nseEditNrpeSettings.Value = null;
                cbEditOverrideNrpe.Checked = false;
                nseEditNrpeSettings.Visible = false;
            }

            dvCollector.DefinedTypeId = DefinedTypeCache.Get( com.blueboxmoon.WatchdogMonitor.SystemGuid.DefinedType.WATCHDOG_MONITOR_COLLECTORS.AsGuid() ).Id;
            dvCollector.SetValue( device.CollectorId );

            //
            // Set the devices repeater.
            //
            var deviceEntityTypeId = EntityTypeCache.Get<WatchdogDevice>().Id;
            GroupListState = device.DeviceGroups
                .ToList()
                .Select( g => new NameValueEntity( g.Id, g.Name ) )
                .OrderBy( n => n.Name )
                .ToList();
            BindGroupsRepeater();

            //
            // Bind the devices drop down list.
            //
            ddlGroups.Items.Clear();
            ddlGroups.Items.Add( new ListItem() );
            new WatchdogDeviceGroupService( rockContext ).Queryable().AsNoTracking()
                .OrderBy( g => g.Name )
                .ToList()
                .ForEach( g => ddlGroups.Items.Add( new ListItem( g.Name, g.Id.ToString() ) ) );

            nbEditError.Text = string.Empty;

            pnlDetails.Visible = false;
            pnlServiceChecks.Visible = false;
            pnlRecentEvents.Visible = false;
            pnlGraphs.Visible = false;
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
        /// Handles the CheckedChanged event of the cbEditOverrideSnmp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbEditOverrideSnmp_CheckedChanged( object sender, EventArgs e )
        {
            sseEditSnmpSettings.Visible = cbEditOverrideSnmp.Checked;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbEditOverrideNrpe control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbEditOverrideNrpe_CheckedChanged( object sender, EventArgs e )
        {
            nseEditNrpeSettings.Visible = cbEditOverrideNrpe.Checked;
        }

        /// <summary>
        /// Handles the Click event of the lbDetailsEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDetailsEdit_Click( object sender, EventArgs e )
        {
            ShowEdit( PageParameter( "DeviceId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var deviceService = new WatchdogDeviceService( rockContext );
            var device = deviceService.Get( hfEditId.ValueAsInt() );

            if ( device == null )
            {
                device = new WatchdogDevice();
                deviceService.Add( device );
            }

            device.Name = tbEditName.Text;
            device.IsActive = cbEditIsActive.Checked;
            device.Address = tbEditAddress.Text;
            device.DeviceProfileId = ddlEditProfile.SelectedValueAsId().Value;
            device.ParentDeviceId = ddlEditParentDevice.SelectedValueAsId();
            device.Description = tbEditDescription.Text;
            device.CollectorId = dvCollector.SelectedValueAsId().Value;

            if ( cbEditOverrideSnmp.Checked )
            {
                device.SnmpSettingsJson = sseEditSnmpSettings.Value.ToJson();
            }
            else
            {
                device.SnmpSettingsJson = string.Empty;
            }

            if ( cbEditOverrideNrpe.Checked )
            {
                device.NrpeSettingsJson = nseEditNrpeSettings.Value.ToJson();
            }
            else
            {
                device.NrpeSettingsJson = string.Empty;
            }

            //
            // Update the device groups.
            //
            var deviceGroupService = new WatchdogDeviceGroupService( rockContext );
            device.DeviceGroups.Clear();
            foreach ( var group in GroupListState )
            {
                device.DeviceGroups.Add( deviceGroupService.Get( group.Id ) );
            }
            var existingGroupIds = device.DeviceGroups.Select( g => g.Id ).ToList();
            var removeGroupIds = existingGroupIds.Except( GroupListState.Select( g => g.Id ) ).ToList();
            var addGroupIds = GroupListState.Select( g => g.Id ).Except( existingGroupIds ).ToList();

            foreach ( var id in removeGroupIds )
            {
                device.DeviceGroups.Remove( device.DeviceGroups.First( g => g.Id == id ) );
            }
            foreach ( var id in addGroupIds )
            {
                device.DeviceGroups.Add( deviceGroupService.Get( id ) );
            }

            nbEditError.Text = string.Empty;
            if ( !device.IsValid )
            {
                nbEditError.Text = "<ul><li>" + string.Join( "</li><li>", device.ValidationResults.Select( v => v.ErrorMessage ) ) + "</li></ul>";
                return;
            }

            rockContext.SaveChanges();
            removeGroupIds.ForEach( id => WatchdogDeviceGroupCache.Remove( id ) );
            addGroupIds.ForEach( id => WatchdogDeviceGroupCache.Remove( id ) );
            WatchdogServiceCheckService.SynchronizeDeviceServiceChecks( device.Id );

            NavigateToCurrentPage( new Dictionary<string, string> { { "DeviceId", device.Id.ToString() } } );
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

        /// <summary>
        /// Handles the GridRebind event of the gServiceChecks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gServiceChecks_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindServiceChecks();
        }

        /// <summary>
        /// Handles the RowSelected event of the gServiceChecks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gServiceChecks_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "ServiceCheckPage", "ServiceCheckId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gRecentEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gRecentEvents_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindRecentEventsGrid();
        }

        /// <summary>
        /// Handles the ToggleSilenceClick event of the gRecentEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gRecentEvents_ToggleSilenceClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var serviceCheckEvent = new WatchdogServiceCheckEventService( rockContext ).Get( e.RowKeyId );

                serviceCheckEvent.IsSilenced = !serviceCheckEvent.IsSilenced;

                rockContext.SaveChanges();
            }

            BindRecentEventsGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gRecentEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gRecentEvents_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var lbSilence = e.Row.ControlsOfTypeRecursive<LinkButton>()
                    .Where( c => c.CssClass.Contains( "js-toggle-silence" ) )
                    .Cast<LinkButton>()
                    .FirstOrDefault();

                DateTime? endDateTime = ( DateTime? ) e.Row.DataItem.GetPropertyValue( "EndDateTime" );
                bool isSilenced = ( bool ) e.Row.DataItem.GetPropertyValue( "IsSilenced" );

                if ( endDateTime.HasValue )
                {
                    lbSilence.Visible = false;
                }
                else
                {
                    lbSilence.Visible = true;
                    lbSilence.ToolTip = isSilenced ? "Enable notifications" : "Silence notifications";

                    if ( isSilenced )
                    {
                        lbSilence.RemoveCssClass( "btn-default" ).AddCssClass( "btn-warning" );
                    }
                    else
                    {
                        lbSilence.RemoveCssClass( "btn-warning" ).AddCssClass( "btn-default" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbShowAllEvents control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbShowAllEvents_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "EventPage", "DeviceId", PageParameter( "DeviceId" ).AsInteger() );
        }

        #endregion

        #region Groups Repeater

        /// <summary>
        /// Handles the ItemCommand event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rpGroups_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int personId = e.CommandArgument.ToString().AsInteger();

            if ( e.CommandName == "Delete" )
            {
                var nve = GroupListState.Where( a => a.Id == personId ).FirstOrDefault();

                if ( nve != null )
                {
                    GroupListState.Remove( nve );
                    BindGroupsRepeater();
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGroups control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGroups_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( ddlGroups.SelectedValueAsId().HasValue && !GroupListState.Any( d => d.Id == ddlGroups.SelectedValueAsId().Value ) )
            {
                GroupListState.Add( new NameValueEntity( ddlGroups.SelectedValueAsId().Value, ddlGroups.SelectedItem.Text ) );
            }

            ddlGroups.SetValue( ( int? ) null );
            BindGroupsRepeater();
        }

        /// <summary>
        /// Binds the groups repeater.
        /// </summary>
        private void BindGroupsRepeater()
        {
            rpGroups.DataSource = GroupListState;
            rpGroups.DataBind();
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