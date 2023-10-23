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
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Device List" )]
    [Category( "Core" )]
    [Description( "Lists all the devices." )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.BlockTypeGuid( "32183AD6-01CB-4533-858B-1BDA5120AAD5" )]
    public partial class DeviceList : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

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
            gDevice.Actions.ShowAdd = true;
            gDevice.Actions.AddClick += gDevice_Add;
            gDevice.GridRebind += gDevice_GridRebind;

            AddDynamicColumns();

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gDevice.Actions.ShowAdd = canAddEditDelete;
            gDevice.IsDeleteEnabled = canAddEditDelete;
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

        #endregion Control Methods

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the ApplyFilterClick event of the fDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fDevice_ApplyFilterClick( object sender, EventArgs e )
        {
            fDevice.SetFilterPreference( "Name", tbName.Text );
            fDevice.SetFilterPreference( "Device Type", dvpDeviceType.SelectedValue );
            fDevice.SetFilterPreference( "IP Address", tbIPAddress.Text );
            fDevice.SetFilterPreference( "Kiosk Type", ddlKioskType.SelectedValue );
            fDevice.SetFilterPreference( "Print To", ddlPrintTo.SelectedValue );
            fDevice.SetFilterPreference( "Printer", ddlPrinter.SelectedValue );
            fDevice.SetFilterPreference( "Print From", ddlPrintFrom.SelectedValue );
            fDevice.SetFilterPreference( "Active Status", ddlActiveFilter.SelectedValue );

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

                case "Kiosk Type":
                    e.Value = e.Value.ConvertToEnumOrNull<KioskType>()?.GetDescription();
                    break;

                case "Print To":

                    e.Value = ( ( PrintTo ) System.Enum.Parse( typeof( PrintTo ), e.Value ) ).ToString();
                    break;

                case "Print From":

                    e.Value = ( ( PrintFrom ) System.Enum.Parse( typeof( PrintFrom ), e.Value ) ).ToString();
                    break;
                case "Active Status":

                    if ( !string.IsNullOrEmpty( e.Value ) && e.Value == "all" )
                    {
                        e.Value = string.Empty;
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the Add event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDevice_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "DeviceId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDevice_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, "DeviceId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDevice_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            DeviceService deviceService = new DeviceService( rockContext );
            Device device = deviceService.Get( e.RowKeyId );

            if ( device != null )
            {
                int deviceId = device.Id;

                string errorMessage;
                if ( !deviceService.CanDelete( device, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                deviceService.Delete( device );
                rockContext.SaveChanges();

                Rock.CheckIn.KioskDevice.Remove( deviceId );
            }

            BindGrid();
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

        /// <summary>
        /// Handles the RowDataBound event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gDevice_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            DeviceInfo device = e.Row.DataItem as DeviceInfo;
            if ( device == null )
            {
                return;
            }

            var deviceTypeValueIdKiosk = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid() );
            if ( device.DeviceTypeValueId != deviceTypeValueIdKiosk )
            {
                // Print settings only apply to Checkin Kiosks, so if this isn't a Check-in Kiosk, don't populate the Printer settings columns
                return;
            }

            var lPrintToOverride = e.Row.FindControl( "lPrintToOverride" ) as Literal;
            var lPrintFrom = e.Row.FindControl( "lPrintFrom" ) as Literal;
            var lPrinterDeviceName = e.Row.FindControl( "lPrinterDeviceName" ) as Literal;
            lPrintToOverride.Text = device.PrintToOverride.ConvertToString();
            lPrintFrom.Text = device.PrintFrom.ConvertToString();
            lPrinterDeviceName.Text = device.PrinterDeviceName;
        }

        #endregion Grid Events (main grid)

        #region Internal Methods

        /// <summary>
        /// Add all the dynamic columns needed to properly display devices.
        /// </summary>
        protected void AddDynamicColumns()
        {
            // Remove attribute columns
            foreach ( var column in gDevice.Columns.OfType<AttributeField>().ToList() )
            {
                gDevice.Columns.Remove( column );
            }

            // Add attribute columns
            int entityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Device ) ).Id;
            foreach ( var attribute in new AttributeService( new RockContext() ).Queryable()
                .Where( a =>
                    a.EntityTypeId == entityTypeId &&
                    a.IsGridColumn &&
                    a.EntityTypeQualifierColumn == string.Empty )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name ) )
            {
                string dataFieldExpression = attribute.Key;
                bool columnExists = gDevice.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                if ( !columnExists )
                {
                    AttributeField boundField = new AttributeField();
                    boundField.DataField = dataFieldExpression;
                    boundField.AttributeId = attribute.Id;
                    boundField.HeaderText = attribute.Name;

                    var attributeCache = Rock.Web.Cache.AttributeCache.Get( attribute.Id );
                    if ( attributeCache != null )
                    {
                        boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                    }

                    gDevice.Columns.Add( boundField );
                }
            }

            var deleteField = new DeleteField();
            gDevice.Columns.Add( deleteField );
            deleteField.Click += gDevice_Delete;
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            dvpDeviceType.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.DEVICE_TYPE ) ).Id;

            ddlKioskType.BindToEnum<KioskType>( true );

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
                tbName.Text = fDevice.GetFilterPreference( "Name" );
                dvpDeviceType.SetValue( fDevice.GetFilterPreference( "Device Type" ) );
                tbIPAddress.Text = fDevice.GetFilterPreference( "IP Address" );
                ddlKioskType.SetValue( fDevice.GetFilterPreference( "Kiosk Type" ) );
                ddlPrintTo.SetValue( fDevice.GetFilterPreference( "Print To" ) );
                ddlPrinter.SetValue( fDevice.GetFilterPreference( "Printer" ) );
                ddlPrintFrom.SetValue( fDevice.GetFilterPreference( "Print From" ) );
                var itemActiveStatus = ddlActiveFilter.Items.FindByValue( fDevice.GetFilterPreference( "Active Status" ) );
                if ( itemActiveStatus != null )
                {
                    itemActiveStatus.Selected = true;
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var deviceService = new DeviceService( new RockContext() );
            var sortProperty = gDevice.SortProperty;
            gDevice.EntityTypeId = EntityTypeCache.Get<Device>().Id;

            var queryable = deviceService.Queryable();

            string name = fDevice.GetFilterPreference( "Name" );
            if ( !string.IsNullOrWhiteSpace( name ) )
            {
                queryable = queryable.Where( d => d.Name.Contains( name ) );
            }

            int? deviceTypeId = fDevice.GetFilterPreference( "Device Type" ).AsIntegerOrNull();
            if ( deviceTypeId.HasValue )
            {
                queryable = queryable.Where( d => d.DeviceTypeValueId == deviceTypeId.Value );
            }

            KioskType? kioskTypeFilter = fDevice.GetFilterPreference( "Kiosk Type" )?.ConvertToEnumOrNull<KioskType>();
            if ( kioskTypeFilter.HasValue )
            {
                queryable = queryable.Where( d => d.KioskType == kioskTypeFilter.Value );
            }

            string ipAddress = fDevice.GetFilterPreference( "IP Address" );
            if ( !string.IsNullOrWhiteSpace( ipAddress ) )
            {
                queryable = queryable.Where( d => d.IPAddress.Contains( ipAddress ) );
            }

            if ( !string.IsNullOrWhiteSpace( fDevice.GetFilterPreference( "Print To" ) ) )
            {
                PrintTo printTo = ( PrintTo ) System.Enum.Parse( typeof( PrintTo ), fDevice.GetFilterPreference( "Print To" ) );
                queryable = queryable.Where( d => d.PrintToOverride == printTo );
            }

            int? printerId = fDevice.GetFilterPreference( "Printer" ).AsIntegerOrNull();
            if ( printerId.HasValue )
            {
                queryable = queryable.Where( d => d.PrinterDeviceId == printerId );
            }

            if ( !string.IsNullOrWhiteSpace( fDevice.GetFilterPreference( "Print From" ) ) )
            {
                PrintFrom printFrom = ( PrintFrom ) System.Enum.Parse( typeof( PrintFrom ), fDevice.GetFilterPreference( "Print From" ) );
                queryable = queryable.Where( d => d.PrintFrom == printFrom );
            }

            string activeFilterValue = fDevice.GetFilterPreference( "Active Status" );
            if ( !string.IsNullOrWhiteSpace( activeFilterValue ) )
            {
                if ( activeFilterValue != "all" )
                {
                    var activeFilter = activeFilterValue.AsBoolean();
                    queryable = queryable.Where( b => b.IsActive == activeFilter );
                }
            }
            else
            {
                queryable = queryable.Where( b => b.IsActive );
            }

            gDevice.ObjectList = new Dictionary<string, object>();
            queryable.ToList().ForEach( d => gDevice.ObjectList.Add( d.Id.ToString(), d ) );

            var gridList = queryable.Select( a =>
                new DeviceInfo
                {
                    Id = a.Id,
                    Name = a.Name,
                    DeviceTypeName = a.DeviceType.Value,
                    DeviceTypeGuid = a.DeviceType.Guid,
                    KioskType = a.KioskType,
                    IPAddress = a.IPAddress,
                    PrintToOverride = a.PrintToOverride,
                    PrintFrom = a.PrintFrom,
                    PrinterDeviceName = a.PrinterDevice.Name,
                    PrinterDeviceTypeId = a.PrinterDeviceId,
                    DeviceTypeValueId = a.DeviceTypeValueId,
                    IsActive = a.IsActive
                } ).ToList();

            if ( sortProperty != null )
            {
                gDevice.DataSource = gridList.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gDevice.DataSource = gridList.OrderBy( d => d.Name ).ToList();
            }

            gDevice.EntityTypeId = EntityTypeCache.Get<Rock.Model.Device>().Id;
            gDevice.DataBind();
        }

        #endregion Internal Methods

        #region Classes

        private class DeviceInfo : RockDynamic
        {
            public int Id { get; internal set; }

            public string Name { get; internal set; }

            public string DeviceTypeName { get; internal set; }

            private bool IsCheckinKiosk()
            {
                return this.DeviceTypeGuid == Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid();
            }

            public string IPAddress { get; internal set; }

            public PrintTo PrintToOverride { get; internal set; }

            public string PrinterDeviceName { get; internal set; }

            public int? PrinterDeviceTypeId { get; internal set; }

            public PrintFrom PrintFrom { get; internal set; }

            public int DeviceTypeValueId { get; internal set; }

            public bool IsActive { get; internal set; }

            internal KioskType? KioskType { get; set; }

            public string KioskTypeName
            {
                get
                {

                    if ( IsCheckinKiosk() )
                    {
                        return KioskType?.GetDescription();
                    }

                    return null;

                }
            }

            internal Guid DeviceTypeGuid { get; set; }
        }

        #endregion Classes
    }
}