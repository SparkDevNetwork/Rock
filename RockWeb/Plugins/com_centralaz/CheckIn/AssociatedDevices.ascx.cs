// <copyright>
// Copyright by Central Christian Church
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
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.CheckIn
{
    [DisplayName( "Associated Devices" )]
    [Category( "com_centralaz > Check-in" )]
    [Description( "Lists all the devices tied to a location." )]
    public partial class AssociatedDevices : RockBlock, ISecondaryBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BindFilter();
            fDevice.ApplyFilterClick += fDevice_ApplyFilterClick;
            fDevice.DisplayFilterValue += fDevice_DisplayFilterValue;

            gDevice.DataKeyNames = new string[] { "Id" };
            gDevice.GridRebind += gDevice_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var locationId = PageParameter( "LocationId" ).AsIntegerOrNull();
            if ( locationId != null )
            {
                var rockContext = new RockContext();
                var location = new LocationService( rockContext ).Get( locationId.Value );
                if ( location != null )
                {
                    var deviceService = new DeviceService( rockContext );
                    var selectedIds = new List<int>();

                    var checkboxListControls = rptDeviceTypes.ControlsOfTypeRecursive<RockCheckBoxList>();
                    foreach ( var cblDevices in checkboxListControls )
                    {
                        selectedIds.AddRange( cblDevices.SelectedValuesAsInt );
                    }
                    var devices = deviceService.Queryable();

                    // Remove unselected devices
                    foreach ( var device in devices.Where( d => d.Locations.Any( l => l.Id == locationId ) && !selectedIds.Contains( d.Id ) ).ToList() )
                    {
                        device.Locations.Remove( location );
                    }

                    // Add selected devices
                    foreach ( var device in devices.Where( d => !d.Locations.Any( l => l.Id == locationId ) && selectedIds.Contains( d.Id ) ).ToList() )
                    {
                        device.Locations.Add( location );
                    }

                    rockContext.SaveChanges();

                    BindGrid();
                    pnlEdit.Visible = false;
                    pnlReadOnly.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            BindGrid();
            pnlEdit.Visible = false;
            pnlReadOnly.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            rptDeviceTypes.DataSource = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.DEVICE_TYPE.AsGuid() ).DefinedValues;
            rptDeviceTypes.DataBind();
            pnlReadOnly.Visible = false;
            pnlEdit.Visible = true;
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptDeviceTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptDeviceTypes_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var locationId = PageParameter( "LocationId" ).AsIntegerOrNull();
            if ( locationId != null )
            {
                var deviceTypeValue = e.Item.DataItem as DefinedValueCache;
                var deviceService = new DeviceService( new RockContext() );
                var cblDevices = e.Item.FindControl( "cblDevices" ) as RockCheckBoxList;

                cblDevices.Items.Clear();
                cblDevices.DataSource = deviceService.Queryable().Where( d => d.DeviceTypeValueId == deviceTypeValue.Id ).OrderBy( d => d.Name ).ToList();
                cblDevices.DataBind();
                cblDevices.SetValues( deviceService.Queryable().Where( d => d.DeviceTypeValueId == deviceTypeValue.Id && d.Locations.Any( l => l.Id == locationId ) ).Select( d => d.Id ).ToList() );
                cblDevices.Label = deviceTypeValue.Value;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the fDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fDevice_ApplyFilterClick( object sender, EventArgs e )
        {
            fDevice.SaveUserPreference( "Name", tbName.Text );
            fDevice.SaveUserPreference( "Device Type", ddlDeviceType.SelectedValue );
            fDevice.SaveUserPreference( "IP Address", tbIPAddress.Text );
            fDevice.SaveUserPreference( "Print To", ddlPrintTo.SelectedValue );
            fDevice.SaveUserPreference( "Printer", ddlPrinter.SelectedValue );
            fDevice.SaveUserPreference( "Print From", ddlPrintFrom.SelectedValue );

            BindGrid();
        }

        /// <summary>
        /// Displays the text of the current filters
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void fDevice_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Printer":

                    int deviceId = 0;
                    if ( int.TryParse( e.Value, out deviceId ) )
                    {
                        var service = new DeviceService( new RockContext() );
                        var device = service.Get( deviceId );
                        if ( device != null )
                        {
                            e.Value = device.Name;
                        }
                    }

                    break;

                case "Device Type":

                    int definedValueId = 0;
                    if ( int.TryParse( e.Value, out definedValueId ) )
                    {
                        var definedValue = DefinedValueCache.Get( definedValueId );
                        if ( definedValue != null )
                        {
                            e.Value = definedValue.Value;
                        }
                    }

                    break;

                case "Print To":

                    e.Value = ( (PrintTo)System.Enum.Parse( typeof( PrintTo ), e.Value ) ).ToString();
                    break;

                case "Print From":

                    e.Value = ( (PrintFrom)System.Enum.Parse( typeof( PrintFrom ), e.Value ) ).ToString();
                    break;
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gDevice_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlDeviceType.BindToDefinedType( DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.DEVICE_TYPE ) ) );
            ddlDeviceType.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

            ddlPrintTo.BindToEnum<PrintTo>();
            ddlPrintTo.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

            ddlPrintFrom.BindToEnum<PrintFrom>();
            ddlPrintFrom.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

            ddlPrinter.Items.Clear();
            ddlPrinter.DataSource = new DeviceService( new RockContext() )
                .GetByDeviceTypeGuid( new Guid( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER ) )
                .ToList();
            ddlPrinter.DataBind();
            ddlPrinter.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

            if ( !Page.IsPostBack )
            {
                tbName.Text = fDevice.GetUserPreference( "Name" );
                ddlDeviceType.SetValue( fDevice.GetUserPreference( "Device Type" ) );
                tbIPAddress.Text = fDevice.GetUserPreference( "IP Address" );
                ddlPrintTo.SetValue( fDevice.GetUserPreference( "Print To" ) );
                ddlPrinter.SetValue( fDevice.GetUserPreference( "Printer" ) );
                ddlPrintFrom.SetValue( fDevice.GetUserPreference( "Print From" ) );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var locationId = PageParameter( "LocationId" ).AsIntegerOrNull();
            if ( locationId != null )
            {
                var deviceService = new DeviceService( new RockContext() );
                var sortProperty = gDevice.SortProperty;
                gDevice.EntityTypeId = EntityTypeCache.Get<Device>().Id;

                var queryable = deviceService.Queryable()
                    .Where( d => d.Locations.Any( l => l.Id == locationId ) )
                    .Select( a =>
                          new
                          {
                              a.Id,
                              a.Name,
                              DeviceTypeName = a.DeviceType.Value,
                              a.IPAddress,
                              a.PrintToOverride,
                              a.PrintFrom,
                              PrinterDeviceName = a.PrinterDevice.Name,
                              a.PrinterDeviceId,
                              a.DeviceTypeValueId
                          } );

                string name = fDevice.GetUserPreference( "Name" );
                if ( !string.IsNullOrWhiteSpace( name ) )
                {

                    queryable = queryable.Where( d => d.Name.Contains( name ) );
                }

                int? deviceTypeId = fDevice.GetUserPreference( "Device Type" ).AsIntegerOrNull();
                if ( deviceTypeId.HasValue )
                {
                    queryable = queryable.Where( d => d.DeviceTypeValueId == deviceTypeId.Value );
                }

                string ipAddress = fDevice.GetUserPreference( "IP Address" );
                if ( !string.IsNullOrWhiteSpace( ipAddress ) )
                {
                    queryable = queryable.Where( d => d.IPAddress.Contains( ipAddress ) );
                }

                if ( !string.IsNullOrWhiteSpace( fDevice.GetUserPreference( "Print To" ) ) )
                {
                    PrintTo printTo = (PrintTo)System.Enum.Parse( typeof( PrintTo ), fDevice.GetUserPreference( "Print To" ) ); ;
                    queryable = queryable.Where( d => d.PrintToOverride == printTo );
                }

                int? printerId = fDevice.GetUserPreference( "Printer" ).AsIntegerOrNull();
                if ( printerId.HasValue )
                {
                    queryable = queryable.Where( d => d.PrinterDeviceId == printerId );
                }

                if ( !string.IsNullOrWhiteSpace( fDevice.GetUserPreference( "Print From" ) ) )
                {
                    PrintFrom printFrom = (PrintFrom)System.Enum.Parse( typeof( PrintFrom ), fDevice.GetUserPreference( "Print From" ) ); ;
                    queryable = queryable.Where( d => d.PrintFrom == printFrom );
                }

                if ( sortProperty != null )
                {
                    gDevice.DataSource = queryable.Sort( sortProperty ).ToList();
                }
                else
                {
                    gDevice.DataSource = queryable.OrderBy( d => d.Name ).ToList();
                }

                gDevice.EntityTypeId = EntityTypeCache.Get<Rock.Model.Device>().Id;
                gDevice.DataBind();
            }
        }
        #endregion
    }
}