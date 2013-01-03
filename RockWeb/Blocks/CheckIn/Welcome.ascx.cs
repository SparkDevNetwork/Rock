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

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn
{
    [Description( "Check-In Welcome screen" )]
    [TextField( 0, "Admin Page Url", "", "The url of the Check-In admin page", false, "~/checkin" )]
    [TextField( 1, "Search Page Url", "", "The url of the Check-In admin page", false, "~/checkin/search" )]
    [TextField( 2, "Family Select Page Url", "", "The url of the Check-In admin page", false, "~/checkin/selectfamily" )]
    [IntegerField( 3, "Workflow Type Id", "0", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in" )]
    [IntegerField( 4, "Refresh Interval", "10", "RefreshInterval", "", "How often (seconds) should page automatically query server for new check-in data" )]
    public partial class Welcome : Rock.Web.UI.RockBlock
    {
        private KioskStatus _kiosk;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            CurrentPage.AddScriptLink( this.Page, "~/scripts/jquery.countdown.min.js" );

            RegisterScript();
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {

                if ( Session["CheckInWorkflow"] != null )
                {
                    Session.Remove( "CheckInWorkflow" );
                }

                RefreshKioskData();
            }
        }

        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            RefreshKioskData();
        }

        protected void lbSearch_Click( object sender, EventArgs e )
        {
            Response.Redirect( GetAttributeValue( "SearchPageUrl" ), false );
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
            int workflowTypeId = 0;
            if ( Int32.TryParse( GetAttributeValue( "WorkflowTypeId" ), out workflowTypeId ) )
            {
                var workflowTypeService = new WorkflowTypeService();
                var workflowType = workflowTypeService.Get( workflowTypeId );

                if ( workflowType != null )
                {
                    var workflow = Workflow.Activate( workflowType, _kiosk.Device.Name );

                    var checkInState = new CheckInState( _kiosk );
                    checkInState.CheckIn.UserEnteredSearch = false;
                    checkInState.CheckIn.ConfirmSingleFamily = false;
                    checkInState.CheckIn.SearchType = searchType;
                    checkInState.CheckIn.SearchValue = searchValue;

                    //workflow.AttributeValues["CheckInState"]  = checkInState;

                    var activityType = workflowType.ActivityTypes.Where( a => a.Name == "Family Search" ).FirstOrDefault();
                    if ( activityType != null )
                    {
                        WorkflowActivity.Activate( activityType, workflow );
                        var errors = new List<string>();
                        if ( workflow.Process( out errors ) )
                        {
                            Session["CheckInWorkflow"] = workflow;
                            Response.Redirect( GetAttributeValue( "FamilySelectPageUrl" ), false );
                        }
                        else
                        {
                            //TODO: Display errors
                        }
                    }
                    else
                    {
                        throw new Exception( "Workflow type does not have a 'Family Search' activity type" );
                    }

                }
            }
        }

        private void RefreshKioskData()
        {
            pnlNotActive.Visible = false;
            pnlNotActiveYet.Visible = false;
            pnlClosed.Visible = false;
            pnlActive.Visible = false;

            lblActiveWhen.Text = string.Empty;

            if ( Session["CheckInKioskId"] == null || Session["CheckInGroupTypeIds"] == null )
            {
                Response.Redirect( GetAttributeValue( "AdminPageUrl" ), false );
                return;
            }

            int kioskId = (int)Session["CheckInKioskId"];
            var groupTypeIds = Session["CheckInGroupTypeIds"] as List<int>;

            _kiosk = KioskCache.GetKiosk( kioskId );

            // Remove any group types that were not selected in the admin configuration
            foreach ( var kioskGroupType in _kiosk.KioskGroupTypes.ToList() )
            {
                if ( !groupTypeIds.Contains( kioskGroupType.GroupType.Id ) )
                {
                    _kiosk.KioskGroupTypes.Remove( kioskGroupType );
                }
            }

            if ( _kiosk.KioskGroupTypes.Count == 0 )
            {
                pnlNotActive.Visible = true;
            }
            else if ( !_kiosk.HasLocations )
            {
                DateTimeOffset activeAt = _kiosk.KioskGroupTypes.Select( g => g.NextActiveTime ).Min();
                lblActiveWhen.Text = activeAt.ToString();
                pnlNotActiveYet.Visible = true;
            }
            else if ( !_kiosk.HasActiveLocations )
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