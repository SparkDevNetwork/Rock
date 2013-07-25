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
using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    public partial class DeviceDetail : RockBlock, IDetailBlock
    {

        #region Control Methods

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

            ddlPrintTo.BindToEnum( typeof( PrintTo ) );
            ddlPrintFrom.BindToEnum( typeof( PrintFrom ) );

            ddlPrinter.Items.Clear();
            ddlPrinter.DataSource = new DeviceService()
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

            using (new Rock.Data.UnitOfWorkScope())
            {
            pnlDetails.Visible = true;
            Device Device = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                Device = new DeviceService().Get( itemKeyValue );
                lActionTitle.Text = ActionTitle.Edit( Device.FriendlyTypeName );
            }
            else
            {
                Device = new Device { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( Device.FriendlyTypeName );
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

            string orgLocGuid = GlobalAttributesCache.Read().GetValue( "OrganizationAddress" );
            if ( !string.IsNullOrWhiteSpace( orgLocGuid ) )
            {
                Guid locGuid = Guid.Empty;
                if ( Guid.TryParse( orgLocGuid, out locGuid ) )
                {
                    var location = new LocationService().Get( locGuid );
                    if ( location != null )
                    {
                        gpGeoPoint.CenterPoint = location.GeoPoint;
                        gpGeoFence.CenterPoint = location.GeoPoint;
                    }
                }
            }

            if ( Device.Location != null )
            {
                gpGeoPoint.SetValue( Device.Location.GeoPoint );
                gpGeoFence.SetValue( Device.Location.GeoFence );
            }

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
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
            DeviceService DeviceService = new DeviceService();
            AttributeService attributeService = new AttributeService();

            int DeviceId = int.Parse( hfDeviceId.Value );

            if ( DeviceId == 0 )
            {
                Device = new Device();
                DeviceService.Add( Device, CurrentPersonId );
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
            Device.Location.GeoPoint = gpGeoPoint.SelectedValue;
            Device.Location.GeoFence = gpGeoFence.SelectedValue;

            if ( !Device.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            DeviceService.Save( Device, CurrentPersonId );

            NavigateToParentPage();
        }

        #endregion

        #region Activities and Actions

        #endregion

}
}