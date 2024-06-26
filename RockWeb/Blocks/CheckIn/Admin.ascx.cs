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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName( "Administration" )]
    [Category( "Check-in" )]
    [Description( "Check-in Administration block" )]

    [BooleanField(
        "Allow Manual Setup",
        Key = AttributeKey.AllowManualSetup,
        Description = "If enabled, the block will allow the kiosk to be setup manually if it was not set via other means.",
        Category = AttributeCategory.CategoryNone,
        DefaultBooleanValue = true,
        Order = 5 )]

    [BooleanField(
        "Enable Location Sharing",
        Key = AttributeKey.EnableLocationSharing,
        Description = "If enabled, the block will attempt to determine the kiosk's location via location sharing geocode.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.GeoLocation,
        Order = 6 )]

    [IntegerField(
        "Time to Cache Kiosk GeoLocation",
        Key = AttributeKey.TimetoCacheKioskGeoLocation,
        Description = "Time in minutes to cache the coordinates of the kiosk. A value of zero (0) means cache forever. Default 20 minutes.",
        IsRequired = false,
        DefaultIntegerValue = 20,
        Category = AttributeCategory.GeoLocation,
        Order = 7 )]

    [BooleanField(
        "Enable Kiosk Match By Name",
        Key = AttributeKey.EnableKioskMatchByName,
        Description = "Enable a kiosk match by computer name by doing reverseIP lookup to get computer name based on IP address",
        DefaultBooleanValue = false,
        Category = AttributeCategory.CategoryNone,
        Order = 8 )]

    [Rock.SystemGuid.BlockTypeGuid( "3B5FBE9A-2904-4220-92F3-47DD16E805C0" )]
    public partial class Admin : CheckInBlock
    {
        #region Attribute Keys

        /* 2021-05/07 ETD
         * Use new here because the parent CheckInBlock also had inherited class AttributeKey.
         */
        private new static class AttributeKey
        {
            public const string AllowManualSetup = "AllowManualSetup";
            public const string EnableLocationSharing = "EnableLocationSharing";
            public const string TimetoCacheKioskGeoLocation = "TimetoCacheKioskGeoLocation";
            public const string EnableKioskMatchByName = "EnableReverseLookup";
        }

        private static class AttributeCategory
        {
            public const string CategoryNone = "";
            public const string GeoLocation = "Geo Location";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        /// <summary>
        /// Just in case something passes in specific configuration in the URL
        /// </summary>
        private static class PageParameterKey
        {
            public const string KioskId = "KioskId";
            public const string CheckinConfigId = "CheckinConfigId";
            public const string GroupTypeIds = "GroupTypeIds";
            public const string GroupIds = "GroupIds";
            public const string FamilyId = "FamilyId";
            public const string CameraIndex = "CameraIndex";
            public const string Theme = "Theme";
        }

        #endregion PageParameterKeys

        protected override bool LoadUnencryptedLocalDeviceConfig { get { return true; } }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            RockPage.AddScriptLink( "~/Blocks/CheckIn/Scripts/geo-min.js" );
            RockPage.AddScriptLink( "~/Blocks/CheckIn/Scripts/html5-qrcode.min.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            if ( !Page.IsPostBack )
            {
                // if a specific configuration was included in the URL, go directly to the next page (Welcome/Start Page)
                if ( SetConfigurationFromURL() )
                {
                    // Navigate to the check-in home (welcome) page, passing family ID if it was included in the query string
                    var queryParams = new Dictionary<string, string>();
                    string familyId = PageParameter( PageParameterKey.FamilyId );
                    if ( familyId.IsNotNullOrWhiteSpace() )
                    {
                        queryParams.Add( PageParameterKey.FamilyId, familyId );
                    }

                    // if the HTML5 CameraIndex was specified, pass that onto the Welcome/Start page so that it can set the Camera in LocalStorage 
                    var cameraIndex = PageParameter( PageParameterKey.CameraIndex );
                    if ( cameraIndex.IsNotNullOrWhiteSpace() )
                    {
                        queryParams.Add( PageParameterKey.CameraIndex, cameraIndex );
                    }

                    NavigateToNextPage( queryParams );
                    return;
                }

                ShowDetail();
            }
            else
            {
                phGeoCodeScript.Controls.Clear();
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            // just in case the local device config had some overrides (from the MobileLauncher Block)
            LocalDeviceConfig.ClearOverrides();

            SaveState();

            bool enableLocationSharing = GetAttributeValue( AttributeKey.EnableLocationSharing ).AsBoolean();
            bool allowManualSetup = GetAttributeValue( AttributeKey.AllowManualSetup ).AsBoolean( true );

            // 1. Match by IP/DNS first.
            AttemptKioskMatchByIpOrName();

            // 2. Then attempt to match by Geo location if enabled.
            if ( enableLocationSharing )
            {
                lbRetry.Visible = true;

                // We'll re-enable things if the geo lookup fails.
                pnlManualConfig.AddCssClass( "hidden" );
                lbOk.AddCssClass( "hidden" );

                // Inject script used for geo location determination
                AddGeoLocationScript();
            }

            // 3. Allow manual setup if enabled.
            if ( allowManualSetup )
            {
                pnlManualConfig.Visible = true;
                lbOk.Visible = true;
            }

            //
            // If neither location sharing nor manual setup are enabled
            // then display a friendly message.
            //
            if ( !enableLocationSharing && !allowManualSetup )
            {
                lbRetry.Visible = true;
                nbGeoMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbGeoMessage.Text = "Manual configuration is not currently enabled.";
            }

            // Load the themes selector and set the active theme.
            ddlTheme.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( this.Page.Request.MapPath( ResolveRockUrl( "~~" ) ) );
            foreach ( var themeDir in di.Parent.EnumerateDirectories().OrderBy( a => a.Name ) )
            {
                ddlTheme.Items.Add( new ListItem( themeDir.Name, themeDir.Name.ToLower() ) );
            }

            // If a theme has been specified in the URL, prefer it.
            // If not, use the local device configuration cookie or the default theme for the site.
            var activeTheme = PageParameter( PageParameterKey.Theme ).ToLower();
            if ( string.IsNullOrWhiteSpace( activeTheme ) )
            {
                activeTheme = LocalDeviceConfig.CurrentTheme;
            }
            if ( string.IsNullOrWhiteSpace( activeTheme ) )
            {
                activeTheme = RockPage.Site.Theme.ToLower();
            }

            ddlTheme.SetValue( activeTheme );
            SetSelectedTheme( activeTheme );

            int? kioskDeviceTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid() );

            ddlKiosk.Items.Clear();
            using ( var rockContext = new RockContext() )
            {
                var deviceList = new DeviceService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( d => d.DeviceTypeValueId == kioskDeviceTypeValueId
                    && d.IsActive )
                    .OrderBy( d => d.Name )
                    .Select( d => new
                    {
                        d.Id,
                        d.Name,
                        d.KioskType
                    } )
                    .ToList();

                foreach ( var device in deviceList )
                {
                    ddlKiosk.Items.Add( new ListItem
                    {
                        Text = device.KioskType.HasValue ? $"{device.Name} ({device.KioskType.Value.GetDescription()})" : device.Name,
                        Value = device.Id.ToString()
                    } );
                }

            }

            ddlKiosk.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );

            ddlKiosk.SetValue( this.LocalDeviceConfig.CurrentKioskId );
            DisplayControlsForSelectedKiosk();
            BindCheckinTypes( this.LocalDeviceConfig.CurrentCheckinTypeId );
            BindGroupTypes( this.LocalDeviceConfig.CurrentGroupTypeIds );
        }

        /// <summary>
        /// Sets the configuration from URL, returning true if valid parameters were include in the URL
        /// </summary>
        /// <returns></returns>
        private bool SetConfigurationFromURL()
        {
            var urlKioskId = PageParameter( PageParameterKey.KioskId ).AsIntegerOrNull();
            var urlCheckinTypeId = PageParameter( PageParameterKey.CheckinConfigId ).AsIntegerOrNull();
            var urlGroupTypeIds = ( PageParameter( PageParameterKey.GroupTypeIds ) ?? string.Empty ).SplitDelimitedValues().AsIntegerList();
            var urlGroupIds = ( PageParameter( PageParameterKey.GroupIds ) ?? string.Empty ).SplitDelimitedValues().AsIntegerList();

            // Rock check-in will set configuration using Group IDs or GroupType IDs but not both. This is to remove the possiblilty of a Group/GroupType mismatch.
            // Check for groups first since the GroupTypes of those groups will overwrite the URL provided GroupType IDs.
            if ( urlGroupIds.Any() )
            {
                // Determine the GroupType(s) from the provided Group IDs and add them to the configuration, replacing explicit ones if they were provided
                urlGroupTypeIds = new GroupService( new RockContext() ).Queryable().Where( g => urlGroupIds.Contains( g.Id ) ).Select( g => g.GroupTypeId ).Distinct().ToList();
                this.LocalDeviceConfig.CurrentGroupIds = urlGroupIds;
            }

            /*
                2021-04-30 MSB
                There is a route that supports not passing in the check-in type id. If that route is used
                we need to try to get the check-in type id from the selected group types.
            */
            if ( urlKioskId.HasValue && urlGroupTypeIds.Any() && !urlCheckinTypeId.HasValue )
            {
                // If Kiosk and GroupTypes were passed, but not a checkin type, try to calculate it from the group types.
                foreach ( int groupTypeId in urlGroupTypeIds )
                {
                    var checkinType = GetCheckinType( groupTypeId );
                    if ( checkinType != null )
                    {
                        urlCheckinTypeId = checkinType.Id;
                        break;
                    }
                }
            }

            // Need to display the admin UI if Rock didn't find the check-in type
            if ( !urlCheckinTypeId.HasValue )
            {
                return false;
            }

            this.LocalDeviceConfig.CurrentCheckinTypeId = urlCheckinTypeId;

            /*
                2020-09-10 MDP
                If both PageParameterKey.CheckinConfigId and PageParameterKey.GroupTypeIds are specified, set the local device configuration from those.
                Then if PageParameterKey.KioskId is also specified set the KioskId from that, otherwise determine it from the IP Address
                see https://app.asana.com/0/1121505495628584/1191546188992881/f
            */

            if ( urlGroupTypeIds.Any() )
            {
                this.LocalDeviceConfig.CurrentGroupTypeIds = urlGroupTypeIds;

                if ( !urlKioskId.HasValue )
                {
                    // If the kiosk device ID wasn't provided in the URL attempt to get it using the IPAddress.
                    // If the kiosk device ID cannot be determined return false so the configuration can be set manually.
                    var device = GetKioskFromIpOrName();
                    if ( device == null )
                    {
                        return false;
                    }

                    urlKioskId = device.Id;
                }

                this.LocalDeviceConfig.CurrentKioskId = urlKioskId;
            }

            // If the local device is fully configured return true
            if ( this.LocalDeviceConfig.IsConfigured() )
            {
                // These need to be cleared so they can be correctly reloaded with the new data.
                CurrentCheckInState = null;
                CurrentWorkflow = null;

                // Since we changed the config, save state
                SaveState();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to find the Device record for this kiosk by looking
        /// for a matching Device that has kiosk's IP Address, and optional host name
        /// if it can't be found from IP Address.
        /// </summary>
        /// <returns></returns>
        public Device GetKioskFromIpOrName()
        {
            // try to find matching kiosk by REMOTE_ADDR (ip/name).
            var checkInDeviceTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;
            bool skipReverseLookup = !GetAttributeValue( AttributeKey.EnableKioskMatchByName ).AsBoolean();
            using ( var rockContext = new RockContext() )
            {
                var device = new DeviceService( rockContext ).GetByIPAddress( Rock.Web.UI.RockPage.GetClientIpAddress(), checkInDeviceTypeId, skipReverseLookup );
                return device;
            }
        }

        /// <summary>
        /// Attempts to match a known kiosk based on the IP address of the client.
        /// </summary>
        private void AttemptKioskMatchByIpOrName()
        {
            using ( var rockContext = new RockContext() )
            {
                var device = GetKioskFromIpOrName();
                if ( device == null )
                {
                    return;
                }

                ClearMobileCookie();
                LocalDeviceConfig.CurrentKioskId = device.Id;
                LocalDeviceConfig.CurrentGroupTypeIds = GetAllKiosksGroupTypes( device, rockContext );

                if ( !LocalDeviceConfig.CurrentCheckinTypeId.HasValue )
                {
                    foreach ( int groupTypeId in LocalDeviceConfig.CurrentGroupTypeIds )
                    {
                        var checkinType = GetCheckinType( groupTypeId );
                        if ( checkinType != null )
                        {
                            LocalDeviceConfig.CurrentCheckinTypeId = checkinType.Id;
                            break;
                        }
                    }
                }

                CurrentCheckInState = null;
                CurrentWorkflow = null;
                SaveState();
                NavigateToNextPage();

            }
        }

        /// <summary>
        /// Adds GeoLocation script and calls its init() to get client's latitude/longitude before firing
        /// the server side lbCheckGeoLocation_Click click event. Puts the two values into the two corresponding
        /// hidden variables, hfLatitude and hfLongitude.
        /// </summary>
        private void AddGeoLocationScript()
        {
            string geoScript = string.Format( @"
    <script>
        $(document).ready(function (e) {{

debugger
tryGeoLocation();

            function tryGeoLocation() {{
                if ( geo_position_js.init() ) {{
                    geo_position_js.getCurrentPosition(success_callback, error_callback, {{ enableHighAccuracy: true }});
                }}
                else {{
                    $(""div.checkin-header h1"").html( ""We're Sorry!"" );
                    $(""div.checkin-header h1"").after( ""<p>We don't support that kind of device yet. Please Check in using the on-site kiosks.</p>"" );
                    alert(""We don't support that kind of device yet. Please Check in using the on-site kiosks."");
                }}
            }}

            function success_callback( p ) {{
                var latitude = p.coords.latitude.toFixed(4);
                var longitude = p.coords.longitude.toFixed(4);
                $(""input[id$='hfLatitude']"").val( latitude );
                $(""input[id$='hfLongitude']"").val( longitude );
                $(""div.checkin-header h1"").html( 'Checking Your Location...' );
                $(""div.checkin-header"").append( ""<p class='text-muted'>"" + latitude + "" "" + longitude + ""</p>"" );
                // now perform a postback to fire the check geo location
                window.location = ""javascript:{0}"";
            }}

            function error_callback( p ) {{
                $(""input[id$='hfGeoError']"").val(p.message);
                window.location = ""javascript:{0}"";
            }}
        }});
    </script>
", this.Page.ClientScript.GetPostBackEventReference( lbCheckGeoLocation, "" ) );
            phGeoCodeScript.Controls.Add( new LiteralControl( geoScript ) );
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
            var latitude = hfLatitude.Value.AsDoubleOrNull();
            var longitude = hfLongitude.Value.AsDoubleOrNull();
            var error = hfGeoError.Value;
            Device kiosk = null;

            //
            // If we have an error, display all the manual config stuff (if enabled),
            // otherwise we will redirect so it won't matter.
            //
            pnlManualConfig.RemoveCssClass( "hidden" );
            lbOk.RemoveCssClass( "hidden" );

            if ( !error.IsNullOrWhiteSpace() )
            {
                nbGeoMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbGeoMessage.Text = error;

                return;
            }

            if ( latitude.HasValue && longitude.HasValue )
            {
                kiosk = GetCurrentKioskByGeoFencing( latitude.Value, longitude.Value );
            }

            if ( kiosk != null )
            {
                SetDeviceIdCookie( kiosk );

                LocalDeviceConfig.CurrentKioskId = kiosk.Id;
                using ( var rockContext = new RockContext() )
                {
                    LocalDeviceConfig.CurrentGroupTypeIds = GetAllKiosksGroupTypes( kiosk, rockContext );
                }

                if ( !LocalDeviceConfig.CurrentCheckinTypeId.HasValue )
                {
                    foreach ( int groupTypeId in LocalDeviceConfig.CurrentGroupTypeIds )
                    {
                        var checkinType = GetCheckinType( groupTypeId );
                        if ( checkinType != null )
                        {
                            LocalDeviceConfig.CurrentCheckinTypeId = checkinType.Id;
                            break;
                        }
                    }
                }

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
            double timeCacheMinutes = GetAttributeValue( AttributeKey.TimetoCacheKioskGeoLocation ).AsDouble();
            DateTime cookieExpirationDate = ( timeCacheMinutes == 0 ) ? DateTime.MaxValue : RockDateTime.Now.AddMinutes( timeCacheMinutes );

            Rock.Web.UI.RockPage.AddOrUpdateCookie( CheckInCookieKey.DeviceId, kiosk.Id.ToString(), cookieExpirationDate );
            Rock.Web.UI.RockPage.AddOrUpdateCookie( CheckInCookieKey.IsMobile, "true", cookieExpirationDate );
        }

        /// <summary>
        /// Clears the flag cookie that indicates this is a "mobile" device kiosk.
        /// </summary>
        private void ClearMobileCookie()
        {
            Rock.Web.UI.RockPage.AddOrUpdateCookie( CheckInCookieKey.IsMobile, "true", RockDateTime.Now.AddDays( -1d ) );
        }

        /// <summary>
        /// Returns a list of IDs that are the GroupTypes the kiosk is responsible for.
        /// </summary>
        /// <param name="kiosk"></param>
        /// <returns></returns>
        private List<int> GetAllKiosksGroupTypes( Device kiosk, RockContext rockContext )
        {
            var groupTypes = GetDeviceGroupTypes( kiosk.Id, rockContext );
            var groupTypeIds = groupTypes.Select( gt => gt.Id ).ToList();
            return groupTypeIds;
        }

        /// <summary>
        /// Display a "too far" message.
        /// </summary>
        private void TooFar()
        {
            bool allowManualSetup = GetAttributeValue( AttributeKey.AllowManualSetup ).AsBoolean( true );

            nbGeoMessage.NotificationBoxType = NotificationBoxType.Warning;

            if ( allowManualSetup )
            {
                nbGeoMessage.Text = "We could not automatically determine your configuration.";
            }
            else
            {
                pnlManualConfig.Visible = false;
                lbOk.Visible = false;

                nbGeoMessage.Text = "You are too far. Try again later.";
            }
        }

        #endregion

        #region Manually Setting Kiosks related

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlTheme control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlTheme_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetSelectedTheme( ddlTheme.SelectedValue );
        }

        /// <summary>
        /// Sets the configured theme and updates the theme cookie if needed
        /// </summary>
        /// <param name="theme">The theme.</param>
        private void SetSelectedTheme( string theme )
        {
            if ( LocalDeviceConfig.CurrentTheme != theme )
            {
                LocalDeviceConfig.CurrentTheme = ddlTheme.SelectedValue;
                LocalDeviceConfig.SaveToCookie();
            }

            if ( !RockPage.Site.Theme.Equals( LocalDeviceConfig.CurrentTheme, StringComparison.OrdinalIgnoreCase ) )
            {
                // if the site's theme doesn't match the configured theme, reload the page with the theme parameter so that the correct theme gets loaded and the theme cookie gets set
                Dictionary<string, string> themeParameters = new Dictionary<string, string>();
                themeParameters.Add( "theme", LocalDeviceConfig.CurrentTheme );

                NavigateToCurrentPageReference( themeParameters );
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlKiosk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlKiosk_SelectedIndexChanged( object sender, EventArgs e )
        {
            DisplayControlsForSelectedKiosk();
            BindCheckinTypes( ddlCheckinType.SelectedValueAsInt() );
            BindGroupTypes( GetSelectedGroupTypeIds() );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCheckinType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCheckinType_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindGroupTypes( GetSelectedGroupTypeIds() );
        }

        /// <summary>
        /// Handles the Click event of the lbOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbOk_Click( object sender, EventArgs e )
        {
            if ( ddlKiosk.SelectedValue == None.IdValue )
            {
                maWarning.Show( "A Kiosk Device needs to be selected", ModalAlertType.Warning );
                return;
            }

            var groupTypeIds = new List<int>();
            foreach ( ListItem item in cblPrimaryGroupTypes.Items )
            {
                if ( item.Selected )
                {
                    groupTypeIds.Add( item.Value.AsInteger() );
                }
            }

            foreach ( ListItem item in cblAlternateGroupTypes.Items )
            {
                if ( item.Selected )
                {
                    groupTypeIds.Add( item.Value.AsInteger() );
                }
            }

            ClearMobileCookie();
            LocalDeviceConfig.CurrentTheme = ddlTheme.SelectedValue;
            LocalDeviceConfig.CurrentKioskId = ddlKiosk.SelectedValueAsInt();
            LocalDeviceConfig.CurrentCheckinTypeId = ddlCheckinType.SelectedValueAsInt();
            LocalDeviceConfig.CurrentGroupTypeIds = groupTypeIds;

            CurrentCheckInState = null;
            CurrentWorkflow = null;
            SaveState();

            NavigateToNextPage();
        }

        /// <summary>
        /// Gets the device group types.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        private List<GroupTypeCache> GetDeviceGroupTypes( int deviceId, RockContext rockContext )
        {
            var locationService = new LocationService( rockContext );
            var groupLocationService = new GroupLocationService( rockContext );

            // Get all locations (and their children) associated with device
            var locationIds = locationService
                .GetByDevice( deviceId, true )
                .Select( l => l.Id )
                .ToList();

            var locationGroupTypes = groupLocationService
                .Queryable().AsNoTracking()
                .Where( l => locationIds.Contains( l.LocationId ) )
                .Where( gl => gl.Group.GroupType.TakesAttendance )
                .Select( gl => gl.Group.GroupTypeId )
                .ToList()
                .Select( a => GroupTypeCache.Get( a ) ).ToList();

            // get all distinct group types
            var groupTypes = new Dictionary<int, GroupTypeCache>();

            foreach ( var groupType in locationGroupTypes )
            {
                groupTypes.TryAdd( groupType.Id, groupType );
            }

            return groupTypes
                .Select( g => g.Value )
                .OrderBy( g => g.Order )
                .ToList();
        }

        /// <summary>
        /// Displays the controls based on the Device kiosk type, etc
        /// </summary>
        private void DisplayControlsForSelectedKiosk()
        {
            pnlHtml5CameraOptions.Visible = false;

            var deviceId = ddlKiosk.SelectedValueAsInt();
            if ( !deviceId.HasValue )
            {
                return;
            }

            var device = new DeviceService( new RockContext() ).GetSelect( deviceId.Value, s => new { s.HasCamera, s.KioskType } );
            if ( device == null )
            {
                return;
            }

            // Only show HTML5 Camera options if all the following are true
            // -- HasCamera is true
            // -- KioskType has been set (the HTML5 camera feature won't be enabled until they specifically set the KioskType)
            // -- The KioskType is not an IPad
            // -- The current Theme supports the HTML5 Camera feature
            // Also, Javascript will hide this option if it detects this is running an on IPad, even though they didn't select Ipad as the KioskType
            bool showHtml5CameraOptions = device.HasCamera && device.KioskType.HasValue && device?.KioskType != KioskType.IPad && this.CurrentThemeSupportsHTML5Camera();
            hfKioskType.Value = device?.KioskType?.ConvertToString( false );
            pnlHtml5CameraOptions.Visible = showHtml5CameraOptions;
        }

        /// <summary>
        /// Binds the checkin types.
        /// </summary>
        /// <param name="selectedValue">The selected value.</param>
        private void BindCheckinTypes( int? checkinTypeId )
        {
            ddlCheckinType.Items.Clear();

            if ( ddlKiosk.SelectedValue != None.IdValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupTypeService = new GroupTypeService( rockContext );

                    var checkinTemplateTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );

                    ddlCheckinType.DataSource = groupTypeService
                        .Queryable().AsNoTracking()
                        .Where( t => t.GroupTypePurposeValueId.HasValue && t.GroupTypePurposeValueId == checkinTemplateTypeId )
                        .OrderBy( t => t.Name )
                        .Select( t => new
                        {
                            t.Name,
                            t.Id
                        } )
                        .ToList();
                    ddlCheckinType.DataBind();
                }

                if ( checkinTypeId.HasValue )
                {
                    ddlCheckinType.SetValue( checkinTypeId );
                }
                else
                {
                    if ( LocalDeviceConfig.CurrentCheckinTypeId.HasValue )
                    {
                        ddlCheckinType.SetValue( LocalDeviceConfig.CurrentCheckinTypeId );
                    }
                    else
                    {
                        var groupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_WEEKLY_SERVICE_CHECKIN_AREA.AsGuid() );
                        if ( groupType != null )
                        {
                            ddlCheckinType.SetValue( groupType.Id );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the type of the checkin.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        private GroupTypeCache GetCheckinType( int? groupTypeId )
        {
            Guid templateTypeGuid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid();
            var templateType = DefinedValueCache.Get( templateTypeGuid );
            if ( templateType != null )
            {
                return GetCheckinType( GroupTypeCache.Get( groupTypeId.Value ), templateType.Id );
            }

            return null;
        }

        /// <summary>
        /// Gets the type of the checkin.
        /// </summary>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="templateTypeId">The template type identifier.</param>
        /// <param name="recursionControl">The recursion control.</param>
        /// <returns></returns>
        private GroupTypeCache GetCheckinType( GroupTypeCache groupType, int templateTypeId, List<int> recursionControl = null )
        {
            if ( groupType != null )
            {
                recursionControl = recursionControl ?? new List<int>();
                if ( !recursionControl.Contains( groupType.Id ) )
                {
                    recursionControl.Add( groupType.Id );
                    if ( groupType.GroupTypePurposeValueId.HasValue && groupType.GroupTypePurposeValueId == templateTypeId )
                    {
                        return groupType;
                    }

                    foreach ( var parentGroupType in groupType.ParentGroupTypes )
                    {
                        var checkinType = GetCheckinType( parentGroupType, templateTypeId, recursionControl );
                        if ( checkinType != null )
                        {
                            return checkinType;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the selected GroupType Ids
        /// </summary>
        private List<int> GetSelectedGroupTypeIds()
        {
            var groupTypeIds = new List<int>();
            foreach ( ListItem item in cblPrimaryGroupTypes.Items )
            {
                if ( item.Selected )
                {
                    groupTypeIds.Add( item.Value.AsInteger() );
                }
            }

            foreach ( ListItem item in cblAlternateGroupTypes.Items )
            {
                if ( item.Selected )
                {
                    groupTypeIds.Add( item.Value.AsInteger() );
                }
            }

            return groupTypeIds;
        }

        /// <summary>
        /// Binds the group types (checkin areas)
        /// </summary>
        /// <param name="selectedValues">The selected values.</param>
        private void BindGroupTypes( List<int> groupTypeIds )
        {
            cblPrimaryGroupTypes.Items.Clear();
            cblAlternateGroupTypes.Items.Clear();

            if ( ddlKiosk.SelectedValue != None.IdValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var deviceGroupTypes = GetDeviceGroupTypes( ddlKiosk.SelectedValueAsInt() ?? 0, rockContext );

                    var checkinType = GroupTypeCache.Get( ddlCheckinType.SelectedValue.AsInteger() );
                    if ( checkinType == null )
                    {
                        return;
                    }

                    var primaryGroupTypeIds = checkinType.GetDescendentGroupTypes().Select( a => a.Id ).ToList();

                    cblPrimaryGroupTypes.DataSource = deviceGroupTypes.Where( t => primaryGroupTypeIds.Contains( t.Id ) ).ToList();
                    cblPrimaryGroupTypes.DataBind();
                    cblPrimaryGroupTypes.Visible = cblPrimaryGroupTypes.Items.Count > 0;

                    cblAlternateGroupTypes.DataSource = deviceGroupTypes.Where( t => !primaryGroupTypeIds.Contains( t.Id ) ).ToList();
                    cblAlternateGroupTypes.DataBind();
                    cblAlternateGroupTypes.Visible = cblPrimaryGroupTypes.Items.Count > 0;
                }

                if ( groupTypeIds != null )
                {
                    foreach ( var groupTypeId in groupTypeIds )
                    {
                        ListItem item = cblPrimaryGroupTypes.Items.FindByValue( groupTypeId.ToString() );
                        if ( item != null )
                        {
                            item.Selected = true;
                        }

                        item = cblAlternateGroupTypes.Items.FindByValue( groupTypeId.ToString() );
                        if ( item != null )
                        {
                            item.Selected = true;
                        }
                    }
                }
                else
                {
                    if ( LocalDeviceConfig.CurrentGroupTypeIds != null )
                    {
                        foreach ( int id in LocalDeviceConfig.CurrentGroupTypeIds )
                        {
                            ListItem item = cblPrimaryGroupTypes.Items.FindByValue( id.ToString() );
                            if ( item != null )
                            {
                                item.Selected = true;
                            }

                            item = cblAlternateGroupTypes.Items.FindByValue( id.ToString() );
                            if ( item != null )
                            {
                                item.Selected = true;
                            }
                        }
                    }
                }
            }
            else
            {
                cblPrimaryGroupTypes.Visible = false;
                cblAlternateGroupTypes.Visible = false;
            }
        }

        /// <summary>
        /// Returns a kiosk based on finding a geo location match for the given latitude and longitude.
        /// </summary>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <returns></returns>
        public static Device GetCurrentKioskByGeoFencing( double latitude, double longitude )
        {
            var checkInDeviceTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;

            // We need to use the DeviceService until we can get the GeoFence to JSON Serialize/Deserialize.
            using ( var rockContext = new RockContext() )
            {
                Device kiosk = new DeviceService( rockContext ).GetByGeocode( latitude, longitude, checkInDeviceTypeId );
                return kiosk;
            }
        }

        #endregion
    }
}