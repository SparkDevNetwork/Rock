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

namespace RockWeb.Blocks.CheckIn
{
    [Description( "Check-In Search screen" )]
    [TextField( 1, "Welcome Page Url", "", "The url of the Check-In welcome page", false, "~/checkin/welcome" )]
    [TextField( 2, "Family Select Page Url", "", "The url of the Check-In admin page", false, "~/checkin/selectfamily" )]
    [IntegerField( 3, "Workflow Type Id", "0", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in" )]
    public partial class Search : Rock.Web.UI.RockBlock
    {
        private KioskStatus _kiosk;

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                CurrentPage.AddScriptLink( this.Page, "~/scripts/jquery.countdown.min" );

                string script = string.Format( @"
Sys.Application.add_load(function () {{
    window.setInterval(function() {{
    }}, 1000);
}});", this.Page.ClientScript.GetPostBackEventReference( lbRefresh, "" ) );
                this.Page.ClientScript.RegisterStartupScript( this.Page.GetType(), "checkInWelcomeInterval", script, true );

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

        // TODO: Add support for scanner
        private void SomeScannerSearch( DefinedValue searchType, string searchValue )
        {
            int workflowTypeId = 0;
            if ( Int32.TryParse( AttributeValue( "WorkflowTypeId" ), out workflowTypeId ) )
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
                            Response.Redirect( AttributeValue( "FamilySelectPageUrl" ), false );
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

        protected void lbSearch_Click( object sender, EventArgs e )
        {
            Response.Redirect( AttributeValue( "SearchPageUrl" ), false );
        }

        private void RefreshKioskData()
        {
            pnlNotActive.Visible = false;
            pnlNotActiveYet.Visible = false;
            pnlClosed.Visible = false;
            pnlActive.Visible = false;

            if ( Session["CheckInKioskId"] == null || Session["CheckInGroupTypeIds"] == null )
            {
                Response.Redirect( AttributeValue( "AdminPageUrl" ), false );
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
                lblTimeUntilActive.Text = activeAt.Subtract( DateTimeOffset.Now ).ToString();
                string script = string.Format( @"
Sys.Application.add_load(function () {{
    var timeActive = new Date({0});
    $('.coundown-timer').countdown({{until: timeActive, compact:true, layout:'{{dn}}{{dl}} {{hnn}}{{sep}}{{mnn}}{{sep}}{{snn}}'}});
}});", this.Page.ClientScript.GetPostBackEventReference( lbRefresh, "" ) );
                this.Page.ClientScript.RegisterStartupScript( this.Page.GetType(), "checkInWelcomeInterval", script, true );
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