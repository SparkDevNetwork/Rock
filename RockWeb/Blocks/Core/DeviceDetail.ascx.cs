// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Device Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given device." )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.MAP_STYLES, "Map Style", "The map theme that should be used for styling the GeoPicker map.", true, false, Rock.SystemGuid.DefinedValue.MAP_STYLE_ROCK )]
    public partial class DeviceDetail : RockBlock, IDetailBlock
    {
        #region Properties

        private Dictionary<int, string> Locations
        {
            get
            {
                var locations = ViewState["Locations"] as Dictionary<int, string>;
                if (locations == null)
                {
                    locations = new Dictionary<int, string>();
                    ViewState["Locations"] = locations;
                }
                return locations;
            }
            set
            {
                ViewState["Locations"] = value;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlDevice );

            gLocations.DataKeyNames = new string[] { "Id" };
            gLocations.Actions.ShowAdd = true;
            gLocations.Actions.AddClick += gLocations_AddClick;
            gLocations.GridRebind += gLocations_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbDuplicateDevice.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "DeviceId" ).AsInteger() );
            }
            else
            {
                var device = new Device();
                device.Id = hfDeviceId.ValueAsInt();
                device.DeviceTypeValueId = hfTypeId.ValueAsInt();
                device.LoadAttributes();
                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( device, phAttributes, false, BlockValidationGroup );
            }

            if ( hfAddLocationId.Value.AsIntegerOrNull().HasValue )
            {
                mdLocationPicker.Show();
            }

        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            Guid mapStyleValueGuid = GetAttributeValue( "MapStyle" ).AsGuid();
            geopPoint.MapStyleValueGuid = mapStyleValueGuid;
            geopFence.MapStyleValueGuid = mapStyleValueGuid;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Device Device = null;

            var rockContext = new RockContext();
            var deviceService = new DeviceService( rockContext );
            var attributeService = new AttributeService( rockContext );
            var locationService = new LocationService( rockContext );

            int DeviceId = int.Parse( hfDeviceId.Value );

            if ( DeviceId != 0 )
            {
                Device = deviceService.Get( DeviceId );
            }

            if ( Device == null )
            {
                // Check for existing
                var existingDevice = deviceService.Queryable()
                    .Where( d => d.Name == tbName.Text )
                    .FirstOrDefault();
                if ( existingDevice != null )
                {
                    nbDuplicateDevice.Text = string.Format( "A device already exists with the name '{0}'. Please use a different device name.", existingDevice.Name );
                    nbDuplicateDevice.Visible = true;
                }
                else
                {
                    Device = new Device();
                    deviceService.Add( Device );
                }
            }

            if ( Device != null )
            {
                Device.Name = tbName.Text;
                Device.Description = tbDescription.Text;
                Device.IPAddress = tbIpAddress.Text;
                Device.DeviceTypeValueId = ddlDeviceType.SelectedValueAsInt().Value;
                Device.PrintToOverride = (PrintTo)System.Enum.Parse( typeof( PrintTo ), ddlPrintTo.SelectedValue );
                Device.PrinterDeviceId = ddlPrinter.SelectedValueAsInt();
                Device.PrintFrom = (PrintFrom)System.Enum.Parse( typeof( PrintFrom ), ddlPrintFrom.SelectedValue );

                if ( Device.Location == null )
                {
                    Device.Location = new Location();
                }
                Device.Location.GeoPoint = geopPoint.SelectedValue;
                Device.Location.GeoFence = geopFence.SelectedValue;

                Device.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, Device );

                if ( !Device.IsValid || !Page.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                // Remove any deleted locations
                foreach ( var location in Device.Locations
                    .Where( l =>
                        !Locations.Keys.Contains( l.Id ) )
                    .ToList() )
                {
                    Device.Locations.Remove( location );
                }

                // Add any new locations
                var existingLocationIDs = Device.Locations.Select( l => l.Id ).ToList();
                foreach ( var location in locationService.Queryable()
                    .Where( l =>
                        Locations.Keys.Contains( l.Id ) &&
                        !existingLocationIDs.Contains( l.Id ) ) )
                {
                    Device.Locations.Add( location );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    Device.SaveAttributeValues( rockContext );
                } );

                Rock.CheckIn.KioskDevice.Flush( Device.Id );

                NavigateToParentPage();
            }
        }

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
        /// Handles the ServerValidate event of the cvIpAddress control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvIpAddress_ServerValidate( object source, ServerValidateEventArgs args )
        {
            args.IsValid = VerifyUniqueIpAddress();
        }

        /// <summary>
        /// Handles when the device type selection is changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlDeviceType_SelectedIndexChanged( object sender, EventArgs e )
        {
            Device device = null;

            int deviceId = hfDeviceId.ValueAsInt();
            if ( deviceId != 0 )
            {
                device = new DeviceService( new RockContext() ).Get( deviceId );
            }

            if ( device == null )
            {
                device = new Device();
            }

            SetPrinterSettingsVisibility();
            UpdateControlsForDeviceType( device );
        }

        /// <summary>
        /// Handles when the Print To selection is changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlPrintTo_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetPrinterVisibility();
        }

        protected void gLocations_AddClick( object sender, EventArgs e )
        {
            hfAddLocationId.Value = "0";
            mdLocationPicker.Show();
        }

        protected void gLocations_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            if (Locations.ContainsKey(e.RowKeyId))
            {
                Locations.Remove( e.RowKeyId );
            }
            BindLocations();
        }

        protected void gLocations_GridRebind( object sender, EventArgs e )
        {
            BindLocations();
        }

        protected void btnAddLocation_Click( object sender, EventArgs e )
        {
            // Add the location (ignore if they didn't pick one, or they picked one that already is selected)
            var location = new LocationService( new RockContext() ).Get( locationPicker.SelectedValue.AsInteger() );
            if ( location != null )
            {
                string path = location.Name;
                var parentLocation = location.ParentLocation;
                while ( parentLocation != null )
                {
                    path = parentLocation.Name + " > " + path;
                    parentLocation = parentLocation.ParentLocation;
                }
                Locations.Add( location.Id, path );
            }

            BindLocations();

            hfAddLocationId.Value = string.Empty;
            mdLocationPicker.Hide();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlDeviceType.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.DEVICE_TYPE ) ) );
            ddlDeviceType.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

            ddlPrintFrom.BindToEnum<PrintFrom>();

            ddlPrinter.Items.Clear();
            ddlPrinter.DataSource = new DeviceService( new RockContext() )
                .GetByDeviceTypeGuid( new Guid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER ) )
                .OrderBy( d => d.Name )
                .ToList();
            ddlPrinter.DataBind();
            ddlPrinter.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="DeviceId">The device identifier.</param>
        public void ShowDetail( int DeviceId )
        {
            pnlDetails.Visible = true;
            Device Device = null;

            var rockContext = new RockContext();

            if ( !DeviceId.Equals( 0 ) )
            {
                Device = new DeviceService( rockContext ).Get( DeviceId );
                lActionTitle.Text = ActionTitle.Edit( Device.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( Device, ResolveRockUrl( "~" ) );
            }

            if ( Device == null )
            {
                Device = new Device { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( Device.FriendlyTypeName ).FormatAsHtmlTitle();
                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            LoadDropDowns();

            hfDeviceId.Value = Device.Id.ToString();

            tbName.Text = Device.Name;
            tbDescription.Text = Device.Description;
            tbIpAddress.Text = Device.IPAddress;
            ddlDeviceType.SetValue( Device.DeviceTypeValueId );
            ddlPrintTo.SetValue( Device.PrintToOverride.ConvertToInt().ToString() );
            ddlPrinter.SetValue( Device.PrinterDeviceId );
            ddlPrintFrom.SetValue( Device.PrintFrom.ConvertToInt().ToString() );

            SetPrinterVisibility();
            SetPrinterSettingsVisibility();

            string orgLocGuid = GlobalAttributesCache.Read().GetValue( "OrganizationAddress" );
            if ( !string.IsNullOrWhiteSpace( orgLocGuid ) )
            {
                Guid locGuid = Guid.Empty;
                if ( Guid.TryParse( orgLocGuid, out locGuid ) )
                {
                    var location = new LocationService( rockContext ).Get( locGuid );
                    if ( location != null )
                    {
                        geopPoint.CenterPoint = location.GeoPoint;
                        geopFence.CenterPoint = location.GeoPoint;
                    }
                }
            }

            if ( Device.Location != null )
            {
                geopPoint.SetValue( Device.Location.GeoPoint );
                geopFence.SetValue( Device.Location.GeoFence );
            }

            Locations = new Dictionary<int,string>();
            foreach ( var location in Device.Locations)
            {
                string path = location.Name;
                var parentLocation = location.ParentLocation;
                while ( parentLocation != null )
                {
                    path = parentLocation.Name + " > " + path;
                    parentLocation = parentLocation.ParentLocation;
                }
                Locations.Add( location.Id, path );
            }
            BindLocations();

            Guid mapStyleValueGuid = GetAttributeValue( "MapStyle" ).AsGuid();
            geopPoint.MapStyleValueGuid = mapStyleValueGuid;
            geopFence.MapStyleValueGuid = mapStyleValueGuid;

            UpdateControlsForDeviceType( Device );

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( Device.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( Device.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            tbIpAddress.ReadOnly = readOnly;
            ddlDeviceType.Enabled = !readOnly;
            ddlPrintTo.Enabled = !readOnly;
            ddlPrinter.Enabled = !readOnly;
            ddlPrintFrom.Enabled = !readOnly;

            btnSave.Visible = !readOnly;
        }

        /// <summary>
        /// Adds the attribute controls.
        /// </summary>
        /// <param name="device">The device.</param>
        private void AddAttributeControls( Device device)
        {
            int typeId = ddlDeviceType.SelectedValueAsInt() ?? 0;
            hfTypeId.Value = typeId.ToString();

            device.DeviceTypeValueId = typeId;
            device.LoadAttributes();
            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( device, phAttributes, true, BlockValidationGroup );
        }

        /// <summary>
        /// Updates the type of the controls for device.
        /// </summary>
        /// <param name="device">The device.</param>
        private void UpdateControlsForDeviceType( Device device )
        {
            AddAttributeControls( device );
        }

        /// <summary>
        /// Verifies the ip address is unique.
        /// </summary>
        private bool VerifyUniqueIpAddress()
        {
            bool isValid = true;
            int currentDeviceId = int.Parse( hfDeviceId.Value );
            int? deviceTypeId = ddlDeviceType.SelectedValueAsInt().Value;
            if ( !string.IsNullOrWhiteSpace( tbIpAddress.Text ) && deviceTypeId != null )
            {
                var rockContext = new RockContext();
                bool ipExists = new DeviceService( rockContext ).Queryable()
                    .Any( d => d.IPAddress.Equals( tbIpAddress.Text )
                        && d.DeviceTypeValueId == deviceTypeId
                        && d.Id != currentDeviceId );
                isValid = !ipExists;
            }

            return isValid;
        }

        /// <summary>
        /// Decide if the printer settings section should be hidden.
        /// </summary>
        private void SetPrinterSettingsVisibility()
        {
            var checkinKioskDeviceTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;
            pnlPrinterSettings.Visible = ( ddlDeviceType.SelectedValue.AsIntegerOrNull() == checkinKioskDeviceTypeId );
        }

        /// <summary>
        /// Decide if the printer drop down list should be hidden.
        /// </summary>
        private void SetPrinterVisibility()
        {
            var printTo = (PrintTo)System.Enum.Parse( typeof( PrintTo ), ddlPrintTo.SelectedValue );
            ddlPrinter.Visible = printTo != PrintTo.Location;
        }

        private void BindLocations()
        {
            gLocations.DataSource = Locations
                .OrderBy( l => l.Value)
                .Select( l => new
                    {
                        Id = l.Key,
                        LocationPath = l.Value
                    } )
                .ToList();
            gLocations.DataBind();
        }

        #endregion

    }
}