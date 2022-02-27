using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using com.blueboxmoon.WatchdogMonitor.Checks.Support.NRPE;
using com.blueboxmoon.WatchdogMonitor.Checks.Support.SNMP;
using com.blueboxmoon.WatchdogMonitor.Model;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Device Profile List" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "Lists device profiles in the system." )]
    public partial class DeviceProfileList : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the state of the service check data stored in memory.
        /// </summary>
        /// <value>
        /// The state of the service check.
        /// </value>
        protected List<ServiceCheckItem> ServiceCheckState
        {
            get
            {
                return ( List<ServiceCheckItem> ) ViewState["ServiceCheckState"];
            }
            set
            {
                ViewState["ServiceCheckState"] = value;
            }
        }

        #endregion

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

            var entityType = new EntityTypeService( new RockContext() ).Get( typeof( WatchdogDeviceProfile ).FullName );

            gDeviceProfile.DataKeyNames = new string[] { "Id" };
            gDeviceProfile.Actions.AddClick += gDeviceProfile_Add;
            gDeviceProfile.GridRebind += gDeviceProfile_GridRebind;
            gDeviceProfile.Actions.ShowAdd = entityType.IsAuthorized( Authorization.EDIT, CurrentPerson );

            gEditServiceChecks.DataKeyNames = new string[] { "TypeId" };
            gEditServiceChecks.Actions.AddClick += gEditServiceChecks_Add;
            gEditServiceChecks.Actions.ShowAdd = true;

            mdlEditAdd.OnCancelScript = string.Format( "$('#{0}').get(0).click(); return false;", lbEditAddCancel.ClientID );
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
            else
            {
                if ( pnlServiceChecks.Visible )
                {
                    UpdateServiceCheckState();
                }
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Bind the data grid to the list of project types in the system.
        /// </summary>
        private void BindGrid()
        {
            var deviceProfileService = new WatchdogDeviceProfileService( new RockContext() );
            var sortProperty = gDeviceProfile.SortProperty;

            var types = deviceProfileService.Queryable()
                .ToList()
                .Where( t => t.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                .ToList();

            if ( sortProperty != null )
            {
                types = types.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                types = types.OrderBy( t => t.Name ).ToList();
            }

            gDeviceProfile.EntityTypeId = EntityTypeCache.Get<WatchdogDeviceProfile>().Id;
            gDeviceProfile.DataSource = types;
            gDeviceProfile.DataBind();
        }

        /// <summary>
        /// Shows the edit dialog.
        /// </summary>
        /// <param name="id">The identifier.</param>
        private void ShowEdit( int id )
        {
            var rockContext = new RockContext();
            var deviceProfile = new WatchdogDeviceProfileService( rockContext ).Get( id );

            if ( deviceProfile == null )
            {
                deviceProfile = new WatchdogDeviceProfile();
            }

            if ( !deviceProfile.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                return;
            }

            hfEditId.Value = deviceProfile.Id.ToString();
            tbEditName.Text = deviceProfile.Name;
            tbEditIconCssClass.Text = deviceProfile.IconCssClass;

            ddlEditCheckSchedule.Items.Clear();
            ddlEditCheckSchedule.Items.Add( new ListItem() );
            foreach ( var schedule in new WatchdogScheduleCollectionService( rockContext ).Queryable().OrderBy( p => p.Name ) )
            {
                ddlEditCheckSchedule.Items.Add( new ListItem( schedule.Name, schedule.Id.ToString() ) );
            }
            ddlEditCheckSchedule.SetValue( deviceProfile.CheckScheduleId );

            ddlEditInheritedProfile.Items.Clear();
            ddlEditInheritedProfile.Items.Add( new ListItem() );
            foreach ( var profile in new WatchdogDeviceProfileService( rockContext ).Queryable().OrderBy( p => p.Name ) )
            {
                ddlEditInheritedProfile.Items.Add( new ListItem( profile.Name, profile.Id.ToString() ) );
            }
            ddlEditInheritedProfile.SetValue( deviceProfile.InheritedProfileId );

            ddlEditHostServiceCheckType.Items.Clear();
            ddlEditHostServiceCheckType.Items.Add( new ListItem() );
            foreach ( var check in new WatchdogServiceCheckTypeService( rockContext ).Queryable().OrderBy( s => s.Name ) )
            {
                ddlEditHostServiceCheckType.Items.Add( new ListItem( check.Name, check.Id.ToString() ) );
            }
            ddlEditHostServiceCheckType.SetValue( deviceProfile.HostServiceCheckTypeId );

            if ( !string.IsNullOrWhiteSpace( deviceProfile.SnmpSettingsJson ) )
            {
                sseEditSnmpSettings.Value = deviceProfile.SnmpSettingsJson.FromJsonOrNull<SnmpConfiguration>();
                cbEditOverrideSnmp.Checked = true;
                sseEditSnmpSettings.Visible = true;
            }
            else
            {
                cbEditOverrideSnmp.Checked = false;
                sseEditSnmpSettings.Visible = false;
            }

            if ( !string.IsNullOrWhiteSpace( deviceProfile.NrpeSettingsJson ) )
            {
                nseEditNrpeSettings.Value = deviceProfile.NrpeSettingsJson.FromJsonOrNull<NrpeConfiguration>();
                cbEditOverrideNrpe.Checked = true;
                nseEditNrpeSettings.Visible = true;
            }
            else
            {
                nseEditNrpeSettings.Value = null;
                cbEditOverrideNrpe.Checked = false;
                nseEditNrpeSettings.Visible = false;
            }

            lbServiceChecks_Click( this, null );

            BindServiceChecksGrid( true );
            BindInheritedServiceChecks();

            nbEditError.Text = string.Empty;
            mdlEdit.Show();
        }

        /// <summary>
        /// Updates the state of the service check data.
        /// </summary>
        protected void UpdateServiceCheckState()
        {
            var rows = gEditServiceChecks.Rows;
            var state = ServiceCheckState;

            for ( int i = 0; i < state.Count; i++ )
            {
                state[i].Enabled = ( ( CheckBox ) rows[i].Cells[1].Controls[0] ).Checked;
                state[i].CollectorId = ( ( DefinedValuePicker ) rows[i].FindControl( "dvCollector" ) ).SelectedValueAsId();
            }

            ServiceCheckState = state;
        }

        /// <summary>
        /// Binds the service checks grid.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        protected void BindServiceChecksGrid( bool setValues = false )
        {
            if ( setValues )
            {
                var rockContext = new RockContext();
                var profile = new WatchdogDeviceProfileService( rockContext ).Get( hfEditId.Value.AsInteger() );

                ServiceCheckState = new List<ServiceCheckItem>();

                if ( profile != null )
                {
                    var serviceChecks = profile.ServiceChecksJson.FromJsonOrNull<List<ServiceCheckStateData>>();
                    var serviceCheckTypeNames = new WatchdogServiceCheckTypeService( rockContext ).Queryable()
                        .ToDictionary( t => t.Id, t => t.Name );

                    if ( serviceChecks != null )
                    {
                        ServiceCheckState = serviceChecks.Select( s => new ServiceCheckItem
                        {
                            TypeId = s.ServiceCheckTypeId,
                            Name = serviceCheckTypeNames.ContainsKey( s.ServiceCheckTypeId ) ? serviceCheckTypeNames[s.ServiceCheckTypeId] : null,
                            Enabled = s.Active,
                            CollectorId = s.CollectorId
                        } )
                        .Where( s => s.Name != null )
                        .ToList();
                    }
                }
            }

            gEditServiceChecks.DataSource = ServiceCheckState;
            gEditServiceChecks.DataBind();
        }

        /// <summary>
        /// Binds the inherited service checks.
        /// </summary>
        protected void BindInheritedServiceChecks()
        {
            var rockContext = new RockContext();
            var profile = new WatchdogDeviceProfileService( rockContext ).Get( ddlEditInheritedProfile.SelectedValueAsId() ?? 0 );

            if ( profile != null )
            {
                var enabledCheckIds = profile.GetEnabledServiceCheckConfiguration( false ).Select( c => c.ServiceCheckTypeId ).ToList();
                var serviceCheckTypes = new WatchdogServiceCheckTypeService( rockContext ).Queryable()
                    .Where( t => enabledCheckIds.Contains( t.Id ) )
                    .Select( t => new
                    {
                        t.Name
                    } )
                    .ToList();

                rptrEditInheritedServiceChecks.DataSource = serviceCheckTypes;
                rptrEditInheritedServiceChecks.DataBind();
                pnlEditInheritedServiceChecks.Visible = true;
            }
            else
            {
                pnlEditInheritedServiceChecks.Visible = false;
            }
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
        protected void gDeviceProfile_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDeviceProfile_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var deviceProfileService = new WatchdogDeviceProfileService( rockContext );
            var deviceProfile = deviceProfileService.Get( e.RowKeyId );

            if ( deviceProfile != null && deviceProfile.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                deviceProfileService.Delete( deviceProfile );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gDeviceProfile_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDeviceProfile_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gDeviceProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gDeviceProfile_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int deleteIndex = gDeviceProfile.Columns.IndexOf( gDeviceProfile.Columns.OfType<DeleteField>().Single() );
                var deleteButton = e.Row.Cells[deleteIndex].ControlsOfTypeRecursive<LinkButton>().Single();

                deleteButton.Enabled = deleteButton.Enabled && ( ( WatchdogDeviceProfile ) e.Row.DataItem ).IsAuthorized( Authorization.EDIT, CurrentPerson );
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdlEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdlEdit_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var deviceProfileService = new WatchdogDeviceProfileService( rockContext );
            var deviceProfile = deviceProfileService.Get( hfEditId.ValueAsInt() );

            nbEditError.Text = string.Empty;

            if ( deviceProfile == null )
            {
                deviceProfile = new WatchdogDeviceProfile();
                deviceProfileService.Add( deviceProfile );
            }

            deviceProfile.Name = tbEditName.Text;
            deviceProfile.InheritedProfileId = ddlEditInheritedProfile.SelectedValueAsId();
            deviceProfile.HostServiceCheckTypeId = ddlEditHostServiceCheckType.SelectedValueAsId();
            deviceProfile.IconCssClass = tbEditIconCssClass.Text;
            deviceProfile.CheckScheduleId = ddlEditCheckSchedule.SelectedValueAsId();
            deviceProfile.Description = tbEditDescription.Text;

            if ( cbEditOverrideSnmp.Checked )
            {
                deviceProfile.SnmpSettingsJson = sseEditSnmpSettings.Value.ToJson();
            }
            else
            {
                deviceProfile.SnmpSettingsJson = string.Empty;
            }

            if ( cbEditOverrideNrpe.Checked )
            {
                deviceProfile.NrpeSettingsJson = sseEditSnmpSettings.Value.ToJson();
            }
            else
            {
                deviceProfile.NrpeSettingsJson = string.Empty;
            }

            deviceProfile.ServiceChecksJson = ServiceCheckState
                .Select( s => new ServiceCheckStateData
                {
                    ServiceCheckTypeId = s.TypeId,
                    Active = s.Enabled,
                    CollectorId = s.CollectorId
                } )
                .ToList()
                .ToJson();

            nbEditError.Text = string.Empty;
            if ( !deviceProfile.IsValid )
            {
                nbEditError.Text = "<ul><li>" + string.Join( "</li><li>", deviceProfile.ValidationResults.Select( v => v.ErrorMessage ) ) + "</li></ul>";
                return;
            }

            rockContext.SaveChanges();

            WatchdogServiceCheckService.SynchronizeDeviceServiceChecks();

            BindGrid();
            mdlEdit.Hide();
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
        /// Handles the Click event of the lbServiceChecks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbServiceChecks_Click( object sender, EventArgs e )
        {
            liServiceChecks.AddCssClass( "active" );
            liSnmp.RemoveCssClass( "active" );
            liNrpe.RemoveCssClass( "active" );

            pnlServiceChecks.Visible = true;
            pnlSnmpSettings.Visible = false;
            pnlNrpeSettings.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbSnmpSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSnmpSettings_Click( object sender, EventArgs e )
        {
            liSnmp.AddCssClass( "active" );
            liServiceChecks.RemoveCssClass( "active" );
            liNrpe.RemoveCssClass( "active" );

            pnlSnmpSettings.Visible = true;
            pnlServiceChecks.Visible = false;
            pnlNrpeSettings.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbNrpeSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbNrpeSettings_Click( object sender, EventArgs e )
        {
            liNrpe.AddCssClass( "active" );
            liServiceChecks.RemoveCssClass( "active" );
            liSnmp.RemoveCssClass( "active" );

            pnlNrpeSettings.Visible = true;
            pnlServiceChecks.Visible = false;
            pnlSnmpSettings.Visible = false;
        }

        /// <summary>
        /// Handles the GridRebind event of the gEditServiceChecks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gEditServiceChecks_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindServiceChecksGrid();
        }

        /// <summary>
        /// Handles the Delete event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gEditServiceChecks_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var state = ServiceCheckState;

            state.RemoveAll( s => s.TypeId == e.RowKeyId );
            state = ServiceCheckState;

            BindServiceChecksGrid();
        }

        /// <summary>
        /// Handles the Add event of the gEditServiceChecks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gEditServiceChecks_Add( object sender, EventArgs e )
        {
            var existingIds = ServiceCheckState.Select( s => s.TypeId );
            var availableTypes = new WatchdogServiceCheckTypeService( new RockContext() ).Queryable()
                .Where( t => !existingIds.Contains( t.Id ) )
                .ToList()
                .Where( t => t.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                .OrderBy ( t=> t.Name )
                .Select( t => new
                {
                    t.Id,
                    t.Name
                } )
                .ToList();

            ddlEditAddType.Items.Clear();
            ddlEditAddType.Items.Add( new ListItem() );
            availableTypes.ForEach( t => ddlEditAddType.Items.Add( new ListItem( t.Name, t.Id.ToString() ) ) );            

            mdlEdit.Hide();
            mdlEditAdd.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdlEditAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdlEditAdd_SaveClick( object sender, EventArgs e )
        {
            var state = ServiceCheckState;

            state.Add( new ServiceCheckItem
            {
                TypeId = ddlEditAddType.SelectedItem.Value.AsInteger(),
                Name = ddlEditAddType.SelectedItem.Text,
                Enabled = true
            } );

            ServiceCheckState = state.OrderBy( s => s.Name ).ToList();

            BindServiceChecksGrid();
            mdlEditAdd.Hide();
            mdlEdit.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbEditAddCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditAddCancel_Click( object sender, EventArgs e )
        {
            mdlEditAdd.Hide();
            mdlEdit.Show();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlEditInheritedProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlEditInheritedProfile_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindInheritedServiceChecks();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gEditServiceChecks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gEditServiceChecks_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var dvCollector = ( DefinedValuePicker ) e.Row.FindControl( "dvCollector" );
                var item = ( ServiceCheckItem ) e.Row.DataItem;

                dvCollector.DefinedTypeId = DefinedTypeCache.Get( com.blueboxmoon.WatchdogMonitor.SystemGuid.DefinedType.WATCHDOG_MONITOR_COLLECTORS.AsGuid() ).Id;
                dvCollector.SetValue( item.CollectorId );
            }
        }

        #endregion

        [Serializable]
        protected class ServiceCheckItem
        {
            public int TypeId { get; set; }

            public string Name { get; set; }

            public bool Enabled { get; set; }

            public int? CollectorId { get; set; }
        }
    }
}