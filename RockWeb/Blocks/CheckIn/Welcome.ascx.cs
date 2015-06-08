// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName("Welcome")]
    [Category("Check-in")]
    [Description("Welcome screen for check-in.")]
    [LinkedPage( "Family Select Page" )]
    [IntegerField( "Refresh Interval", "How often (seconds) should page automatically query server for new Check-in data", false, 10 )]
    [BooleanField("Enable Override", "Allows the override link to be placed on the page.", true)]
    public partial class Welcome : CheckInBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            if ( CurrentCheckInState == null )
            {
                NavigateToPreviousPage();
                return;
            }

            RockPage.AddScriptLink( "~/scripts/jquery.plugin.min.js" );
            RockPage.AddScriptLink( "~/scripts/jquery.countdown.min.js" );

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
                localStorage.theme = '{0}'
                localStorage.checkInKiosk = '{1}';
                localStorage.checkInGroupTypes = '{2}';
            }}
        }});
    </script>
", CurrentTheme, CurrentKioskId, CurrentGroupTypeIds.AsDelimited( "," ) );
                phScript.Controls.Add( new LiteralControl( script ) );

                CurrentWorkflow = null;
                CurrentCheckInState.CheckIn = new CheckInStatus();
                SaveState();
                RefreshView();

                // enable override
                btnOverride.Visible = GetAttributeValue( "EnableOverride" ).AsBoolean();

            }
        }

        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            RefreshView();
        }

        protected void btnOverride_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "Override", "True" );
            NavigateToNextPage( queryParams );
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
                string errorMsg = "<p>" + errors.AsDelimited( "<br/>" ) + "</p>";
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
                DateTime activeAt = CurrentCheckInState.Kiosk.FilteredGroupTypes( CurrentCheckInState.ConfiguredGroupTypes ).Select( g => g.NextActiveTime ).Min();
                lblActiveWhen.Text = activeAt.ToString( "o" );
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