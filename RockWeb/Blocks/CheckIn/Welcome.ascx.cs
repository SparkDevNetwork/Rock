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

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn
{
    [Description( "Check-In Welcome block" )]
    [IntegerField( 11, "Refresh Interval", "10", "RefreshInterval", "", "How often (seconds) should page automatically query server for new check-in data" )]
    public partial class Welcome : CheckInBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( CurrentCheckInState == null)
            {
                GoToAdminPage();
                return;
            }

            CurrentPage.AddScriptLink( this.Page, "~/scripts/jquery.countdown.min.js" );

            RegisterScript();
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && CurrentCheckInState != null )
            {
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
            GoToSearchPage();
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
            if (ProcessActivity("Family Search", out errors))
            {
                SaveState();
                GoToFamilySelectPage();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited("</li><li>") + "</li></ul>";
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

            if (CurrentCheckInState.Kiosk.KioskGroupTypes.Count == 0 )
            {
                pnlNotActive.Visible = true;
            }
            else if ( !CurrentCheckInState.Kiosk.HasLocations )
            {
                DateTimeOffset activeAt = CurrentCheckInState.Kiosk.KioskGroupTypes.Select( g => g.NextActiveTime ).Min();
                lblActiveWhen.Text = activeAt.ToString();
                pnlNotActiveYet.Visible = true;
            }
            else if ( !CurrentCheckInState.Kiosk.HasActiveLocations )
            {
                pnlClosed.Visible = true;
            }
            else
            {
                pnlActive.Visible = true;
            }
        }
    }
}