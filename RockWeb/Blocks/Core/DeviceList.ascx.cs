//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [LinkedPage("Detail Page")]
    public partial class DeviceList : RockBlock
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
            
            gDevice.DataKeyNames = new string[] { "id" };
            gDevice.Actions.ShowAdd = true;
            gDevice.Actions.AddClick += gDevice_Add;
            gDevice.GridRebind += gDevice_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
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
                        var service = new DeviceService();
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
                        var definedValue = DefinedValueCache.Read( definedValueId );
                        if ( definedValue != null )
                        {
                            e.Value = definedValue.Name;
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
        /// Handles the Add event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDevice_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "DeviceId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDevice_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "DeviceId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDevice_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                DeviceService DeviceService = new DeviceService();
                Device Device = DeviceService.Get( (int)e.RowKeyValue );

                if ( Device != null )
                {
                    string errorMessage;
                    if ( !DeviceService.CanDelete( Device, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    DeviceService.Delete( Device, CurrentPersonId );
                    DeviceService.Save( Device, CurrentPersonId );
                }
            } );

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
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            ddlDeviceType.BindToDefinedType( DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.DEVICE_TYPE ) ) );
            ddlDeviceType.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

            ddlPrintTo.BindToEnum( typeof( PrintTo ) );
            ddlPrintTo.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

            ddlPrintFrom.BindToEnum( typeof( PrintFrom ) );
            ddlPrintFrom.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );

            ddlPrinter.Items.Clear();
            ddlPrinter.DataSource = new DeviceService()
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
            var deviceService = new DeviceService();
            var sortProperty = gDevice.SortProperty;

            var queryable = deviceService.Queryable().Select( a =>
                new
                {
                    a.Id,
                    a.Name,
                    DeviceTypeName = a.DeviceType.Name,
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

            int? deviceTypeId = fDevice.GetUserPreference( "Device Type" ).AsInteger();
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

            int? printerId = fDevice.GetUserPreference( "Printer" ).AsInteger();
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

            gDevice.DataBind();
        }

        #endregion
    }
}