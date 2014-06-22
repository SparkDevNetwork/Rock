// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "DeviceId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "DeviceId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlDeviceType.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.DEVICE_TYPE ) ) );
            ddlDeviceType.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

            ddlPrintFrom.BindToEnum( typeof( PrintFrom ) );

            ddlPrinter.Items.Clear();
            ddlPrinter.DataSource = new DeviceService( new RockContext() )
                .GetByDeviceTypeGuid( new Guid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER ) )
                .ToList();
            ddlPrinter.DataBind();
            ddlPrinter.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "DeviceId" ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            Device Device = null;

            var rockContext = new RockContext();

            if ( !itemKeyValue.Equals( 0 ) )
            {
                Device = new DeviceService( rockContext ).Get( itemKeyValue );
                lActionTitle.Text = ActionTitle.Edit( Device.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                Device = new Device { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( Device.FriendlyTypeName ).FormatAsHtmlTitle();
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

            Guid mapStyleValueGuid = GetAttributeValue( "MapStyle" ).AsGuid();
            geopPoint.MapStyleValueGuid = mapStyleValueGuid;
            geopFence.MapStyleValueGuid = mapStyleValueGuid;

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
            Device Device;
            var rockContext = new RockContext();
            DeviceService DeviceService = new DeviceService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );

            int DeviceId = int.Parse( hfDeviceId.Value );

            if ( DeviceId == 0 )
            {
                Device = new Device();
                DeviceService.Add( Device );
            }
            else
            {
                Device = DeviceService.Get( DeviceId );
            }

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

            if ( !Device.IsValid || !Page.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            rockContext.SaveChanges();

            NavigateToParentPage();
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
        /// Handles the ServerValidate event of the cvIpAddress control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvIpAddress_ServerValidate( object source, ServerValidateEventArgs args )
        {
            args.IsValid = VerifyUniqueIpAddress();
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
        /// Decide if the printer drop down list should be hidden.
        /// </summary>
        private void SetPrinterVisibility()
        {
            var printTo = (PrintTo)System.Enum.Parse( typeof( PrintTo ), ddlPrintTo.SelectedValue );
            ddlPrinter.Visible = printTo != PrintTo.Location;
        }

        /// <summary>
        /// Handles when the device type selection is changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlDeviceType_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetPrinterSettingsVisibility();
        }

        /// <summary>
        /// Decide if the printer settings section should be hidden.
        /// </summary>
        private void SetPrinterSettingsVisibility()
        {
            var checkinKioskDeviceTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;
            pnlPrinterSettings.Visible = ( ddlDeviceType.SelectedValue.AsIntegerOrNull() == checkinKioskDeviceTypeId );
        }

        #endregion
    }
}