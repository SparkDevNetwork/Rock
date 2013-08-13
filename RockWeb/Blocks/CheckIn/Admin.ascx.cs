//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    [Description( "Check-in Administration block" )]
    [BooleanField( "Allow Manual Setup", "If enabled, the block will allow the kiosk to be setup manually if it was not set via other means.", true )]
    [BooleanField( "Enable Location Sharing", "If enabled, the block will attempt to determine the kiosk's location via location sharing geocode.", false, "Geo Location", 0 )]
    [IntegerField( "Time to Cache Kiosk GeoLocation", "Time in minutes to cache the coordinates of the kiosk. A value of zero (0) means cache forever. Default 20 minutes.", false, 20, "Geo Location", 1 )]
    public partial class Admin : CheckInBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            CurrentPage.AddScriptLink( this.Page, "~/Blocks/CheckIn/Scripts/geo-min.js" );

            if ( !Page.IsPostBack )
            {
                bool enableLocationSharing = bool.Parse( GetAttributeValue( "EnableLocationSharing" ) ?? "false" );

                // Inject script used for geo location determiniation
                if ( enableLocationSharing )
                {
                    lbRetry.Visible = true;
                    AddGeoLocationScript();
                }
                else
                {
                    pnlManualConfig.Visible = true;
                    lbOk.Visible = true;
                    AttemptKioskMatchByIpOrName();
                }

                string script = string.Format( @"
                    <script>
                        $(document).ready(function (e) {{
                            if (localStorage) {{
                                if (localStorage.checkInKiosk) {{
                                    $('[id$=""hfKiosk""]').val(localStorage.checkInKiosk);
                                    if (localStorage.checkInGroupTypes) {{
                                        $('[id$=""hfGroupTypes""]').val(localStorage.checkInGroupTypes);
                                    }}
                                    {0};
                                }}
                            }}
                        }});
                    </script>
                ", this.Page.ClientScript.GetPostBackEventReference( lbRefresh, "" ) );
                phScript.Controls.Add( new LiteralControl( script ) );

                ddlKiosk.Items.Clear();
                ddlKiosk.DataSource = new DeviceService().Queryable().ToList();
                ddlKiosk.DataBind();
                ddlKiosk.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );

                if ( CurrentKioskId.HasValue )
                {
                    ListItem item = ddlKiosk.Items.FindByValue( CurrentKioskId.Value.ToString() );
                    if ( item != null )
                    {
                        item.Selected = true;
                        BindGroupTypes();
                    }
                }
            }
            else
            {
                phScript.Controls.Clear();
            }
        }

        /// <summary>
        /// Attempts to match a known kiosk based on the IP address of the client.
        /// </summary>
        private void AttemptKioskMatchByIpOrName()
        {
            // try to find matching kiosk by REMOTE_ADDR (ip/name).
            var checkInDeviceTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;
            var device = new DeviceService().GetByIPAddress( Request.ServerVariables["REMOTE_ADDR"], checkInDeviceTypeId, false );
            if ( device != null )
            {
                ClearMobileCookie();
                CurrentKioskId = device.Id;
                CurrentGroupTypeIds = GetAllKiosksGroupTypes( device ); ;
                CurrentCheckInState = null;
                CurrentWorkflow = null;
                SaveState();
                NavigateToNextPage();
            }
        }

        /// <summary>
        /// Adds GeoLocation script and calls its init() to get client's latitude/longitude before firing
        /// the server side lbCheckGeoLocation_Click click event. Puts the two values into the two corresponding
        /// hidden varialbles, hfLatitude and hfLongitude.
        /// </summary>
        private void AddGeoLocationScript()
        {
            string geoScript = string.Format( @"
    <script>
        $(document).ready(function (e) {{

            tryGeoLocation();

            function tryGeoLocation() {{
                if ( geo_position_js.init() ) {{
                    geo_position_js.getCurrentPosition(success_callback, error_callback, {{ enableHighAccuracy: true }});
                }}
                else {{
                    $(""div.checkin-header h1"").html( ""We're Sorry!"" );
                    $(""div.checkin-header h1"").after( ""<p>We don't support that kind of device yet. Please Check in using the on-site kiosks.</p>"" );
                    alert(""We We don't support that kind of device yet. Please Check in using the on-site kiosks."");
                }}
            }}

            function success_callback( p ) {{
                var latitude = p.coords.latitude.toFixed(4);
                var longitude = p.coords.longitude.toFixed(4);
                $(""input[id$='hfLatitude']"").val( latitude );
                $(""input[id$='hfLongitude']"").val( longitude );
                $(""div.checkin-header h1"").html( 'Checking Your Location...' );
                $(""div.checkin-header"").append( ""<p class='muted'>"" + latitude + "" "" + longitude + ""</p>"" );
                // now perform a postback to fire the check geo location 
                {0};
            }}

            function error_callback( p ) {{
                // TODO: decide what to do in this situation...
                alert( 'error=' + p.message );
            }}
        }});
    </script>
", this.Page.ClientScript.GetPostBackEventReference( lbCheckGeoLocation, "" ) );
            phScript.Controls.Add( new LiteralControl( geoScript ) );
        }

        /// <summary>
        /// Used by the local storage script to rebind the group types if they were previously
        /// saved via local storage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            ListItem item = ddlKiosk.Items.FindByValue(hfKiosk.Value);
            if ( item != null )
            {
                ddlKiosk.SelectedValue = item.Value;
            }

            BindGroupTypes(hfGroupTypes.Value);
        }

        #region GeoLocation related
        /// <summary>
        /// Handles attempting to find a registered Device kiosk by it's latitude and longitude.
        /// This event method is called automatically when the GeoLocation script get's the client's location.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbCheckGeoLocation_Click( object sender, EventArgs e )
        {
            var lat = hfLatitude.Value;
            var lon = hfLongitude.Value;
            Device kiosk = null;

            if ( !string.IsNullOrEmpty( lat ) && !string.IsNullOrEmpty( lon ) )
            {
                kiosk = GetCurrentKioskByGeoFencing( lat, lon );
            }

            if ( kiosk != null )
            {
                SetDeviceIdCookie( kiosk );

                CurrentKioskId = kiosk.Id;
                CurrentGroupTypeIds = GetAllKiosksGroupTypes( kiosk ); ;
                CurrentCheckInState = null;
                CurrentWorkflow = null;
                SaveState();

                NavigateToNextPage();
            }
            else
            {
                TooFar();
            }
        }

        /// <summary>
        /// Sets the "DeviceId" cookie to expire after TimeToCacheKioskGeoLocation minutes
        /// if IsMobile is set.
        /// </summary>
        /// <param name="kiosk"></param>
        private void SetDeviceIdCookie( Device kiosk )
        {
            // set an expiration cookie for these coordinates.
            double timeCacheMinutes = double.Parse( GetAttributeValue( "TimetoCacheKioskGeoLocation" ) ?? "0" );

            HttpCookie deviceCookie = Request.Cookies[CheckInCookie.DEVICEID];
            if ( deviceCookie == null )
            {
                deviceCookie = new HttpCookie( CheckInCookie.DEVICEID, kiosk.Id.ToString() );
            }

            deviceCookie.Expires = ( timeCacheMinutes == 0 ) ? DateTime.MaxValue : DateTime.Now.AddMinutes( timeCacheMinutes );
            Response.Cookies.Set( deviceCookie );

            HttpCookie isMobileCookie = new HttpCookie( CheckInCookie.ISMOBILE, "true" );
            Response.Cookies.Set( isMobileCookie );
        }

        /// <summary>
        /// Clears the flag cookie that indicates this is a "mobile" device kiosk.
        /// </summary>
        private void ClearMobileCookie()
        {
            HttpCookie isMobileCookie = new HttpCookie( CheckInCookie.ISMOBILE );
            isMobileCookie.Expires = DateTime.Now.AddDays( -1d );
            Response.Cookies.Set( isMobileCookie );
        }

        /// <summary>
        /// Returns a list of IDs that are the GroupTypes the kiosk is responsible for.
        /// </summary>
        /// <param name="kiosk"></param>
        /// <returns></returns>
        private List<int> GetAllKiosksGroupTypes( Device kiosk )
        {
            var groupTypes = kiosk.GetLocationGroupTypes();
            var groupTypeIds = groupTypes.Select( gt => gt.Id ).ToList();
            return groupTypeIds;
        }

        /// <summary>
        /// Display a "too far" message.
        /// </summary>
        private void TooFar()
        {
            bool allowManualSetup = bool.Parse( GetAttributeValue( "AllowManualSetup" ) ?? "true" );

            if ( allowManualSetup )
            {
                pnlManualConfig.Visible = true;
                lbOk.Visible = true;
                maWarning.Show( "We could not automatically determine your configuration.", ModalAlertType.Information );
            }
            else
            {
                maWarning.Show( "You are too far. Try again later.", ModalAlertType.Alert );
            }
            
        }

        protected void lbRetry_Click( object sender, EventArgs e )
        {
            // TODO
        }
        #endregion

        #region Manually Setting Kiosks related

        protected void ddlKiosk_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindGroupTypes();
        }

        protected void lbOk_Click( object sender, EventArgs e )
        {
            if ( ddlKiosk.SelectedValue == None.IdValue )
            {
                maWarning.Show( "A Kiosk Device needs to be selected!", ModalAlertType.Warning );
                return;
            }

            var groupTypeIds = new List<int>();
            foreach(ListItem item in cblGroupTypes.Items)
            {
                if ( item.Selected )
                {
                    groupTypeIds.Add( Int32.Parse( item.Value ) );
                }
            }

            ClearMobileCookie();
            CurrentKioskId = Int32.Parse( ddlKiosk.SelectedValue );
            CurrentGroupTypeIds = groupTypeIds;
            CurrentCheckInState = null;
            CurrentWorkflow = null;
            SaveState();

            NavigateToNextPage();
        }

        private void BindGroupTypes()
        {
            BindGroupTypes( string.Empty );
        }

        private void BindGroupTypes( string selectedValues )
        {
            var selectedItems = selectedValues.Split( ',' );

            cblGroupTypes.Items.Clear();

            if ( ddlKiosk.SelectedValue != None.IdValue )
            {
                var kiosk = new DeviceService().Get( Int32.Parse( ddlKiosk.SelectedValue ) );
                if ( kiosk != null )
                {
                    cblGroupTypes.DataSource = kiosk.GetLocationGroupTypes();
                    cblGroupTypes.DataBind();
                }

                if ( selectedValues != string.Empty )
                {
                    foreach ( string id in selectedValues.Split(',') )
                    {
                        ListItem item = cblGroupTypes.Items.FindByValue( id );
                        if ( item != null )
                        {
                            item.Selected = true;
                        }
                    }
                }
                else
                {
                    if ( CurrentGroupTypeIds != null )
                    {
                        foreach ( int id in CurrentGroupTypeIds )
                        {
                            ListItem item = cblGroupTypes.Items.FindByValue( id.ToString() );
                            if ( item != null )
                            {
                                item.Selected = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a kiosk based on finding a geo location match for the given latitude and longitude.
        /// </summary>
        /// <param name="sLatitude">latitude as string</param>
        /// <param name="sLongitude">longitude as string</param>
        /// <returns></returns>
        public static Device GetCurrentKioskByGeoFencing( string sLatitude, string sLongitude )
        {
            double latitude = double.Parse( sLatitude );
            double longitude = double.Parse( sLongitude );
            var checkInDeviceTypeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;

            // We need to use the DeviceService until we can get the GeoFence to JSON Serialize/Deserialize.
            Device kiosk = new DeviceService().GetByGeocode( latitude, longitude, checkInDeviceTypeId );

            return kiosk;
        }

        #endregion
    }
}