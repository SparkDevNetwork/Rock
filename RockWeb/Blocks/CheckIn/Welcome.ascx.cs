//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn
{
    [Description( "Check-in Welcome block" )]
    [LinkedPage( "Family Select Page" )]
    [IntegerField( "Refresh Interval", "How often (seconds) should page automatically query server for new Check-in data", false, 10 )]
    public partial class Welcome : CheckInBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( CurrentCheckInState == null )
            {
                NavigateToPreviousPage();
                return;
            }

            CurrentPage.AddScriptLink( this.Page, "~/scripts/jquery.countdown.min.js" );

            RegisterScript();
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && CurrentCheckInState != null )
            {
                string script = string.Format( @"
    <script>
        $(document).ready(function (e) {{
            if (localStorage) {{
                localStorage.checkInKiosk = '{0}';
                localStorage.checkInGroupTypes = '{1}';
            }}
        }});
    </script>
", CurrentKioskId, CurrentGroupTypeIds.AsDelimited( "," ) );
                phScript.Controls.Add( new LiteralControl( script ) );

                CurrentWorkflow = null;
                SaveState();
                RefreshView();
            }
        }

        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            RefreshView();
        }

        protected void lbSearch_Click( object sender, EventArgs e )
        {
            NavigateToNextPage();
        }

        private void RegisterScript()
        {
            // Note: the OnExpiry property of the countdown jquery plugin seems to add a new callback
            // everytime the setting is set which is why the clearCountdown method is used to prevent 
            // a plethora of partial postbacks occurring when the countdown expires.
            string script = string.Format( @"

var timeout = window.setTimeout(refreshKiosk, {1}000);

var $ActiveWhen = $('.active-when');
var $CountdownTimer = $('.countdown-timer');

function refreshKiosk() {{
    window.clearTimeout(timeout);
    {0};
}}

function clearCountdown() {{
    if ($ActiveWhen.text() != '')
    {{
        $ActiveWhen.text('');
        refreshKiosk();
    }}
}}

if ($ActiveWhen.text() != '')
{{
    var timeActive = new Date($ActiveWhen.text());
    $CountdownTimer.countdown({{
        until: timeActive, 
        compact:true, 
        layout:'{{dn}}{{dl}} {{hnn}}{{sep}}{{mnn}}{{sep}}{{snn}}',
        onExpiry: clearCountdown
    }});
}}

", this.Page.ClientScript.GetPostBackEventReference( lbRefresh, "" ), GetAttributeValue( "RefreshInterval" ) );
            ScriptManager.RegisterStartupScript( Page, Page.GetType(), "RefreshScript", script, true );
        }

        // TODO: Add support for scanner
        private void SomeScannerSearch( DefinedValueCache searchType, string searchValue )
        {
            CurrentCheckInState.CheckIn.UserEnteredSearch = false;
            CurrentCheckInState.CheckIn.ConfirmSingleFamily = false;
            CurrentCheckInState.CheckIn.SearchType = searchType;
            CurrentCheckInState.CheckIn.SearchValue = searchValue;

            var errors = new List<string>();
            if ( ProcessActivity( "Family Search", out errors ) )
            {
                SaveState();
                NavigateToLinkedPage( "FamilySelectPage" );
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }

        private void RefreshView()
        {
            pnlNotActive.Visible = false;
            pnlNotActiveYet.Visible = false;
            pnlClosed.Visible = false;
            pnlActive.Visible = false;

            lblActiveWhen.Text = string.Empty;

            if ( CurrentCheckInState == null || IsMobileAndExpiredDevice() )
            {
                NavigateToPreviousPage();
                return;
            }

            if ( CurrentCheckInState.Kiosk.FilteredGroupTypes( CurrentCheckInState.ConfiguredGroupTypes ).Count == 0 )
            {
                pnlNotActive.Visible = true;
            }
            else if ( !CurrentCheckInState.Kiosk.HasLocations( CurrentCheckInState.ConfiguredGroupTypes ) )
            {
                DateTimeOffset activeAt = CurrentCheckInState.Kiosk.FilteredGroupTypes( CurrentCheckInState.ConfiguredGroupTypes ).Select( g => g.NextActiveTime ).Min();
                lblActiveWhen.Text = activeAt.ToString();
                pnlNotActiveYet.Visible = true;
            }
            else if ( !CurrentCheckInState.Kiosk.HasActiveLocations( CurrentCheckInState.ConfiguredGroupTypes ) )
            {
                pnlClosed.Visible = true;
            }
            else
            {
                pnlActive.Visible = true;
            }

            List<int> locations = new List<int>();

            foreach ( var groupType in CurrentCheckInState.Kiosk.FilteredGroupTypes( CurrentCheckInState.ConfiguredGroupTypes ) )
            {
                foreach ( var location in groupType.KioskGroups.SelectMany( g => g.KioskLocations ).Distinct() )
                {
                    if ( !locations.Contains( location.Location.Id ) )
                    {
                        locations.Add( location.Location.Id );
                        var locationAttendance = KioskLocationAttendance.Read( location.Location.Id );

                        if ( locationAttendance != null )
                        {
                            var lUl = new HtmlGenericControl( "ul" );
                            lUl.AddCssClass( "checkin-count-locations" );
                            phCounts.Controls.Add( lUl );

                            var lLi = new HtmlGenericControl( "li" );
                            lUl.Controls.Add( lLi );
                            lLi.InnerHtml = string.Format( "{0}: <strong>{1}</strong>", locationAttendance.LocationName, locationAttendance.CurrentCount );

                            foreach ( var groupAttendance in locationAttendance.Groups )
                            {
                                var gUl = new HtmlGenericControl( "ul" );
                                gUl.AddCssClass( "checkin-count-groups" );
                                lLi.Controls.Add( gUl );

                                var gLi = new HtmlGenericControl( "li" );
                                gUl.Controls.Add( gLi );
                                gLi.InnerHtml = string.Format( "{0}: <strong>{1}</strong>", groupAttendance.GroupName, groupAttendance.CurrentCount );

                                foreach ( var scheduleAttendance in groupAttendance.Schedules )
                                {
                                    var sUl = new HtmlGenericControl( "ul" );
                                    sUl.AddCssClass( "checkin-count-schedules" );
                                    gLi.Controls.Add( sUl );

                                    var sLi = new HtmlGenericControl( "li" );
                                    sUl.Controls.Add( sLi );
                                    sLi.InnerHtml = string.Format( "{0}: <strong>{1}</strong>", scheduleAttendance.ScheduleName, scheduleAttendance.CurrentCount );
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the device is "mobile" and if it is no longer valid.
        /// </summary>
        /// <returns>true if the mobile device has expired; false otherwise.</returns>
        private bool IsMobileAndExpiredDevice()
        {
            if ( Request.Cookies[CheckInCookie.ISMOBILE] != null
                && Request.Cookies[CheckInCookie.DEVICEID] == null )
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}