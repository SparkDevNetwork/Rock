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
    [Description( "Check-In Search screen" )]
    [TextField( 1, "Welcome Page Url", "", "The url of the Check-In welcome page", false, "~/checkin/welcome" )]
    [TextField( 2, "Family Select Page Url", "", "The url of the Check-In admin page", false, "~/checkin/selectfamily" )]
    [IntegerField( 3, "Workflow Type Id", "0", "WorkflowTypeId", "", "The Id of the workflow type to activate for check-in" )]
    public partial class Search : Rock.Web.UI.RockBlock
    {
        private KioskStatus _kiosk;

        protected override void OnLoad( EventArgs e )
        {
            RefreshKioskData();
        }

        protected void lbSearch_Click( object sender, EventArgs e )
        {
            if ( _kiosk != null )
            {
                // TODO: Validate text entered

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
                        checkInState.CheckIn.SearchType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER );
                        checkInState.CheckIn.SearchValue = tbPhone.Text;

                        workflow.SetAttributeValue( "CheckInState", checkInState.ToJson() );

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
                                string errorMsg = "<ul><li>" + errors.AsDelimited("</li><li>") + "</li></ul>";
                                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
                            }
                        }
                        else
                        {
                            throw new Exception( "Workflow type does not have a 'Family Search' activity type" );
                        }

                    }
                }
            }
        }

        protected void lbBack_Click( object sender, EventArgs e )
        {
            Response.Redirect( GetAttributeValue( "WelcomePageUrl" ), false );
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            Response.Redirect( GetAttributeValue( "WelcomePageUrl" ), false );
        }

        private void RefreshKioskData()
        {
            if ( Session["CheckInKioskId"] == null || Session["CheckInGroupTypeIds"] == null )
            {
                Response.Redirect( GetAttributeValue( "WelcomePageUrl" ), false );
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

            if ( _kiosk.KioskGroupTypes.Count == 0 || !_kiosk.HasActiveLocations )
            {
                Response.Redirect( GetAttributeValue( "WelcomePageUrl" ), false );
            }
        }
    }
}