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
using System.Web;
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

    public partial class Admin : CheckInBlock
    {
        #region Attribute Keys

        private static class AttributeKey
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
            public const string FamilyId = "FamilyId";
        }

        #endregion PageParameterKe

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            RockPage.AddScriptLink( "~/Blocks/CheckIn/Scripts/geo-min.js" );
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

            ddlTheme.Items.Clear();
            DirectoryInfo di = new DirectoryInfo( this.Page.Request.MapPath( ResolveRockUrl( "~~" ) ) );
            foreach ( var themeDir in di.Parent.EnumerateDirectories().OrderBy( a => a.Name ) )
            {
                ddlTheme.Items.Add( new ListItem( themeDir.Name, themeDir.Name.ToLower() ) );
            }

            if ( !string.IsNullOrWhiteSpace( LocalDeviceConfig.CurrentTheme ) )
            {
                ddlTheme.SetValue( LocalDeviceConfig.CurrentTheme );
                SetSelectedTheme( LocalDeviceConfig.CurrentTheme );
            }
            else
            {
                ddlTheme.SetValue( RockPage.Site.Theme.ToLower() );
            }

            int? kioskDeviceTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid() );

            ddlKiosk.Items.Clear();
            using ( var rockContext = new RockContext() )
            {
                ddlKiosk.DataSource = new DeviceService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( d => d.DeviceTypeValueId == kioskDeviceTypeValueId )
                    .OrderBy( d => d.Name )
                    .Select( d => new
                    {
                        d.Id,
                        d.Name
                    } )
                    .ToList();
            }

            ddlKiosk.DataBind();
            ddlKiosk.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );

            ddlKiosk.SetValue( this.LocalDeviceConfig.CurrentKioskId );
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

            if ( urlKioskId.HasValue && urlCheckinTypeId.HasValue && urlGroupTypeIds.Any() )
            {
                // all parameters that we need is in the URL, so set localDeviceConfig from that
                this.LocalDeviceConfig.CurrentKioskId = urlKioskId;
                this.LocalDeviceConfig.CurrentCheckinTypeId = urlCheckinTypeId;
                this.LocalDeviceConfig.CurrentGroupTypeIds = urlGroupTypeIds;

                // If the local device is fully configured return true
                if ( this.LocalDeviceConfig.IsConfigured() )
                {
                    // Since we changed the config, save state
                    SaveState();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to match a known kiosk based on the IP address of the client.
        /// </summary>
        private void AttemptKioskMatchByIpOrName()
        {
            // try to find matching kiosk by REMOTE_ADDR (ip/name).
            var checkInDeviceTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK ).Id;
            using ( var rockContext = new RockContext() )
            {
                bool skipReverseLookup = !GetAttributeValue( AttributeKey.EnableKioskMatchByName ).AsBoolean();
                var device = new DeviceService( rockContext ).GetByIPAddress( Rock.Web.UI.RockPage.GetClientIpAddress(), checkInDeviceTypeId, skipReverseLookup );
                if ( device != null )
                {
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

            HttpCookie deviceCookie = Request.Cookies[CheckInCookieKey.DeviceId];
            if ( deviceCookie == null )
            {
                deviceCookie = new HttpCookie( CheckInCookieKey.DeviceId, kiosk.Id.ToString() );
            }

            deviceCookie.Expires = ( timeCacheMinutes == 0 ) ? DateTime.MaxValue : RockDateTime.Now.AddMinutes( timeCacheMinutes );
            Response.Cookies.Set( deviceCookie );

            HttpCookie isMobileCookie = new HttpCookie( CheckInCookieKey.IsMobile, "true" );
            Response.Cookies.Set( isMobileCookie );
        }

        /// <summary>
        /// Clears the flag cookie that indicates this is a "mobile" device kiosk.
        /// </summary>
        private void ClearMobileCookie()
        {
            HttpCookie isMobileCookie = new HttpCookie( CheckInCookieKey.IsMobile );
            isMobileCookie.Expires = RockDateTime.Now.AddDays( -1d );
            Response.Cookies.Set( isMobileCookie );
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
                LocalDeviceConfig.SaveToCookie( this.Page );
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

            LocalDeviceConfig.SaveToCookie( this.Page );

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
                groupTypes.AddOrIgnore( groupType.Id, groupType );
            }

            return groupTypes
                .Select( g => g.Value )
                .OrderBy( g => g.Order )
                .ToList();
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