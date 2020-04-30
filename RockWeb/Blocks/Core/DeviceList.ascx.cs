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
using Rock.Data;
using Rock.Model;
using Rock.Security;
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

        #endregion

        #region Grid Events (main grid)

        /// <summary>
        /// Handles the ApplyFilterClick event of the fDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fDevice_ApplyFilterClick( object sender, EventArgs e )
        {
            fDevice.SaveUserPreference( "Name", tbName.Text );
            fDevice.SaveUserPreference( "Device Type", dvpDeviceType.SelectedValue );
            fDevice.SaveUserPreference( "IP Address", tbIPAddress.Text );
            fDevice.SaveUserPreference( "Print To", ddlPrintTo.SelectedValue );
            fDevice.SaveUserPreference( "Printer", ddlPrinter.SelectedValue );
            fDevice.SaveUserPreference( "Print From", ddlPrintFrom.SelectedValue );
            fDevice.SaveUserPreference( "Active Status", ddlActiveFilter.SelectedValue );

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
            DeviceService DeviceService = new DeviceService( rockContext );
            Device Device = DeviceService.Get( e.RowKeyId );

            if ( Device != null )
            {
                int deviceId = Device.Id;

                string errorMessage;
                if ( !DeviceService.CanDelete( Device, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                DeviceService.Delete( Device );
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

        #endregion

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
                    a.EntityTypeQualifierColumn == string.Empty
                    )
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
                dvpDeviceType.SetValue( fDevice.GetUserPreference( "Device Type" ) );
                tbIPAddress.Text = fDevice.GetUserPreference( "IP Address" );
                ddlPrintTo.SetValue( fDevice.GetUserPreference( "Print To" ) );
                ddlPrinter.SetValue( fDevice.GetUserPreference( "Printer" ) );
                ddlPrintFrom.SetValue( fDevice.GetUserPreference( "Print From" ) );
                var itemActiveStatus = ddlActiveFilter.Items.FindByValue( fDevice.GetUserPreference( "Active Status" ) );
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
                PrintTo printTo = ( PrintTo ) System.Enum.Parse( typeof( PrintTo ), fDevice.GetUserPreference( "Print To" ) );
                ;
                queryable = queryable.Where( d => d.PrintToOverride == printTo );
            }

            int? printerId = fDevice.GetUserPreference( "Printer" ).AsIntegerOrNull();
            if ( printerId.HasValue )
            {
                queryable = queryable.Where( d => d.PrinterDeviceId == printerId );
            }

            if ( !string.IsNullOrWhiteSpace( fDevice.GetUserPreference( "Print From" ) ) )
            {
                PrintFrom printFrom = ( PrintFrom ) System.Enum.Parse( typeof( PrintFrom ), fDevice.GetUserPreference( "Print From" ) );
                ;
                queryable = queryable.Where( d => d.PrintFrom == printFrom );
            }

            string activeFilterValue = fDevice.GetUserPreference( "Active Status" );
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
                    a.DeviceTypeValueId,
                    a.IsActive
                } );

            if ( sortProperty != null )
            {
                gDevice.DataSource = gridList.Sort( sortProperty ).ToList();
            }
            else
            {
                gDevice.DataSource = gridList.OrderBy( d => d.Name ).ToList();
            }

            gDevice.EntityTypeId = EntityTypeCache.Get<Rock.Model.Device>().Id;
            gDevice.DataBind();
        }

        #endregion
    }
}