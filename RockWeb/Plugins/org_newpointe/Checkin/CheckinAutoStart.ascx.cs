using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock;
using Rock.CheckIn;
using Rock.Data;
using System.IO;

namespace RockWeb.Plugins.org_newpointe.Checkin
{

    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Checkin AutoStart" )]
    [Category( "NewPointe Check-in" )]
    [Description( "Auto-start check-in with all Group Types selected based on Location." )]

    public partial class CheckinAutoStart : CheckInBlock
    {

        protected override void OnLoad( EventArgs e )
        {
            RockPage.AddScriptLink( "~/Blocks/CheckIn/Scripts/geo-min.js" );
            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            if ( !Page.IsPostBack )
            {

                var rockContext = new RockContext();
                Guid kioskDeviceType = Rock.SystemGuid.DefinedValue.DEVICE_TYPE_CHECKIN_KIOSK.AsGuid();
                var devices = new DeviceService( rockContext ).Queryable()
                        .Where( d => d.DeviceType.Guid.Equals( kioskDeviceType ) )
                        .ToList();
                ddlKiosk.Items.Clear();
                ddlKiosk.DataSource = devices;
                ddlKiosk.DataBind();
                ddlKiosk.Items.Insert( 0, new ListItem( None.Text, None.IdValue ) );

                Device device = null;

                // Get the campus from the URL
                var campusParam = PageParameter( "Campus" );
                var campus = CampusCache.All().FirstOrDefault( c => c.ShortCode == campusParam || c.Name == campusParam );
                if ( campus != null )
                {
                    device = devices.FirstOrDefault( d => d.Locations.Select( l => (int?)l.Id ).Contains( campus.LocationId ) );
                }
                else
                {
                    // Get the kiosk from the URL
                    var kioskIdParam = PageParameter( "KioskId" );
                    if ( kioskIdParam != null )
                    {
                        var kioskId = kioskIdParam.AsIntegerOrNull();
                        if ( kioskId != null )
                        {
                            device = devices.FirstOrDefault( d => d.Id == kioskId );
                        }
                    }
                }

                if ( device != null )
                {
                    ddlKiosk.SelectedValue = device.Id.ToString();
                }

                ddlTheme.Items.Clear();
                DirectoryInfo di = new DirectoryInfo( this.Page.Request.MapPath( ResolveRockUrl( "~~" ) ) );
                foreach ( var themeDir in di.Parent.EnumerateDirectories().OrderBy( a => a.Name ) )
                {
                    ddlTheme.Items.Add( new ListItem( themeDir.Name, themeDir.Name.ToLower() ) );
                }

                if ( !string.IsNullOrWhiteSpace( CurrentTheme ) )
                {
                    ddlTheme.SetValue( CurrentTheme );
                }
                else
                {
                    ddlTheme.SetValue( RockPage.Site.Theme.ToLower() );
                }

                bool themeRedirect = PageParameter( "ThemeRedirect" ).AsBoolean( false );

                if ( !themeRedirect )
                {

                    if ( device != null )
                    {
                        RedirectConfig();
                    }
                    else
                    {
                        string script = string.Format( @"
                    <script>
                        $(document).ready(function (e) {{
                            if (localStorage) {{
                                if (localStorage.checkInKiosk) {{
                                    $('[id$=""hfKiosk""]').val(localStorage.checkInKiosk);
                                    if (localStorage.theme) {{
                                        $('[id$=""hfTheme""]').val(localStorage.theme);
                                    }}
                                    {0};
                                }}
                            }}
                        }});
                    </script>
                ", this.Page.ClientScript.GetPostBackEventReference( lbRefresh, "" ) );
                        phScript.Controls.Add( new LiteralControl( script ) );
                    }
                }
            }
            else
            {
                phScript.Controls.Clear();
            }

        }

        /// <summary>
        /// Used by the local storage script to rebind the group types if they were previously
        /// saved via local storage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            ListItem item = ddlKiosk.Items.FindByValue( hfKiosk.Value );
            if ( item != null )
            {
                ddlKiosk.SelectedValue = item.Value;
            }

            RedirectConfig();
        }

        /// <summary>
        /// Gets the device group types.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        private List<GroupType> GetDeviceGroupTypes( int deviceId, RockContext rockContext )
        {
            var groupTypes = new Dictionary<int, GroupType>();

            var locationService = new LocationService( rockContext );

            // Get all locations (and their children) associated with device
            var locationIds = locationService
                .GetByDevice( deviceId, true )
                .Select( l => l.Id )
                .ToList();

            // Requery using EF
            foreach ( var groupType in locationService
                .Queryable().AsNoTracking()
                .Where( l => locationIds.Contains( l.Id ) )
                .SelectMany( l => l.GroupLocations )
                .Where( gl => gl.Group.GroupType.TakesAttendance )
                .Select( gl => gl.Group.GroupType )
                .ToList() )
            {
                groupTypes.AddOrIgnore( groupType.Id, groupType );
            }

            return groupTypes
                .Select( g => g.Value )
                .OrderBy( g => g.Order )
                .ToList();
        }

        protected void RedirectConfig()
        {
            if ( ddlKiosk.SelectedValue != None.IdValue && ddlKiosk.SelectedValue != null )
            {
                var rockContext = new RockContext();
                var kiosk = new DeviceService( rockContext ).Get( ddlKiosk.SelectedValue.AsInteger() );
                if ( kiosk != null )
                {
                    Session["redirectURL"] = "~/checkin?KioskId=" + kiosk.Id + "&GroupTypeIds=" + string.Join( ",", GetDeviceGroupTypes( kiosk.Id, rockContext ).Select( g => g.Id ) );
                    Response.Redirect( Session["redirectURL"].ToString() );
                }

            }
        }

        public void lbOk_Click( object sender, EventArgs e )
        {
            if ( ddlKiosk.SelectedValue == None.IdValue || ddlKiosk.SelectedValue == null )
            {
                maWarning.Show( "A Kiosk Device needs to be selected!", ModalAlertType.Warning );
            }
            else
            {
                RedirectConfig();
            }
        }

        protected void lbManual_Click( object sender, EventArgs e )
        {
            Response.Redirect( "~/checkin" );
        }


        protected void ddlTheme_SelectedIndexChanged( object sender, EventArgs e )
        {
            CurrentTheme = ddlTheme.SelectedValue;
            RedirectToNewTheme( ddlTheme.SelectedValue );
        }

        private void RedirectToNewTheme( string theme )
        {
            var pageRef = RockPage.PageReference;
            pageRef.QueryString = new System.Collections.Specialized.NameValueCollection();
            pageRef.Parameters = new Dictionary<string, string>();
            pageRef.Parameters.Add( "theme", theme );
            pageRef.Parameters.Add( "KioskId", ddlKiosk.SelectedValue );
            pageRef.Parameters.Add( "ThemeRedirect", "True" );

            Response.Redirect( pageRef.BuildUrl(), false );
        }
    }
}