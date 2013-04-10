//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.CheckIn
{
    /// <summary>
    /// A RockBlock specific to check-in
    /// </summary>
    [TextField( "Admin Page Url", "The url of the Check-In admin page", false, "~/checkin", "Page Routes", 0 )]
    [TextField( "Welcome Page Url", "The url of the Check-In welcome page", false, "~/checkin/welcome", "Page Routes", 1 )]
    [TextField( "Search Page Url", "The url of the Check-In search page", false, "~/checkin/search", "Page Routes", 2 )]
    [TextField( "Family Select Page Url", "The url of the Check-In family select page", false, "~/checkin/family", "Page Routes", 3 )]
    [TextField( "Person Select Page Url", "The url of the Check-In person select page", false, "~/checkin/person", "Page Routes", 4 )]
    [TextField( "Group Type Select Page Url", "The url of the Check-In group type select page", false, "~/checkin/grouptype", "Page Routes", 5 )]
    [TextField( "Location Select Page Url", "The url of the Check-In location select page", false, "~/checkin/location", "Page Routes", 6 )]
    [TextField( "Group Select Page Url", "The url of the Check-In group select page", false, "~/checkin/group", "Page Routes", 7 )]
    [TextField( "Time Select Page Url", "The url of the Check-In group select page", false, "~/checkin/time", "Page Routes", 8 )]
    [TextField( "Success Page Url", "The url of the Check-In success page", false, "~/checkin/success", "Page Routes", 9 )]
    [IntegerField( "Workflow Type Id", "The Id of the workflow type to activate for check-in", false, 0)]
    public abstract class CheckInBlock : RockBlock
    {
        /// <summary>
        /// The current kiosk id
        /// </summary>
        protected int? CurrentKioskId { get; set; }

        /// <summary>
        /// The current group type ids
        /// </summary>
        protected List<int> CurrentGroupTypeIds;

        /// <summary>
        /// The current check in state
        /// </summary>
        protected CheckInState CurrentCheckInState;

        /// <summary>
        /// The current workflow
        /// </summary>
        protected Rock.Model.Workflow CurrentWorkflow;

        /// <summary>
        /// Gets a value indicating whether the kiosk has active group types and locations that 
        /// are open for check-in.
        /// </summary>
        /// <value>
        /// <c>true</c> if kiosk is active; otherwise, <c>false</c>.
        /// </value>
        protected bool KioskCurrentlyActive
        {
            get
            {
                if ( CurrentCheckInState == null ||
                    CurrentCheckInState.Kiosk == null ||
                    CurrentCheckInState.Kiosk.KioskGroupTypes.Count == 0 ||
                    !CurrentCheckInState.Kiosk.HasActiveLocations )
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether page was navigated to by user selecting Back.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [backing up]; otherwise, <c>false</c>.
        /// </value>
        protected bool UserBackedUp
        {
            get
            {
                bool backingUp = false;
                bool.TryParse( Request.QueryString["back"], out backingUp );
                return backingUp;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            GetState();
        }

        /// <summary>
        /// Activates and processes a workflow activity.  If the workflow has not yet been activated, it will
        /// also be activated
        /// </summary>
        /// <param name="activityName">Name of the activity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        protected bool ProcessActivity(string activityName, out List<string> errorMessages)
        {
            errorMessages = new List<string>();

            int workflowTypeId = 0;
            if ( Int32.TryParse( GetAttributeValue( "WorkflowTypeId" ), out workflowTypeId ) )
            {
                var workflowTypeService = new WorkflowTypeService();
                var workflowType = workflowTypeService.Get( workflowTypeId );
                if ( workflowType != null )
                {
                    if ( CurrentWorkflow == null )
                    {
                        CurrentWorkflow = Rock.Model.Workflow.Activate( workflowType, CurrentCheckInState.Kiosk.Device.Name );
                    }

                    CurrentWorkflow.SetAttributeValue( "CheckInState", CurrentCheckInState.ToJson() );

                    var activityType = workflowType.ActivityTypes.Where( a => a.Name == activityName ).FirstOrDefault();
                    if ( activityType != null )
                    {
                        WorkflowActivity.Activate( activityType, CurrentWorkflow );
                        if ( CurrentWorkflow.Process( out errorMessages ) )
                        {
                            string stateString = CurrentWorkflow.GetAttributeValue( "CheckInState" );
                            if ( !String.IsNullOrEmpty( stateString ) )
                            {
                                CurrentCheckInState = CheckInState.FromJson( stateString );
                            }
                            return true;
                        }
                    }
                    else
                    {
                        errorMessages.Add( string.Format( "Workflow type does not have a '{0}' activity type", activityName ) );
                    }
                }
                else
                {
                    errorMessages.Add( string.Format( "Invalid Workflow type Id", activityName ) );
                }

            }
            
            return false;
        }

        /// <summary>
        /// Saves the current state of the kiosk and workflow
        /// </summary>
        protected void SaveState()
        {
            if ( CurrentKioskId.HasValue )
            {
                Session["CheckInKioskId"] = CurrentKioskId.Value;
            }
            else
            {
                Session.Remove( "CheckInKioskId" );
            }

            if ( CurrentGroupTypeIds != null )
            {
                Session["CheckInGroupTypeIds"] = CurrentGroupTypeIds;
            }
            else
            {
                Session.Remove( "CheckInGroupTypeIds" );
            }

            if ( CurrentWorkflow != null )
            {
                if ( CurrentCheckInState != null )
                {
                    CurrentWorkflow.SetAttributeValue( "CheckInState", CurrentCheckInState.ToJson() );
                }

                Session["CheckInWorkflow"] = CurrentWorkflow;
            }
            else
            {
                Session.Remove( "CheckInWorkflow" );
            }
        }

        /// <summary>
        /// Cancels the checkin.
        /// </summary>
        protected void CancelCheckin()
        {
            CurrentWorkflow = null;
            CurrentCheckInState = null;
            SaveState();
            GoToWelcomePage();
        }

        /// <summary>
        /// Redirects to the admin page.
        /// </summary>
        protected void GoToAdminPage()
        {
            Response.Redirect( GetAttributeValue( "AdminPageUrl" ), false );
        }

        /// <summary>
        /// Redirects to the welcome page.
        /// </summary>
        protected void GoToWelcomePage()
        {
            Response.Redirect( GetAttributeValue( "WelcomePageUrl" ), false );
        }

        /// <summary>
        /// Redirects to the search page.
        /// </summary>
        protected void GoToSearchPage( bool goingBack = false )
        {
            Response.Redirect( GetAttributeValue( "SearchPageUrl" ) + ( goingBack ? "?back=true" : "" ), false );
        }

        /// <summary>
        /// Redirects to family select page.
        /// </summary>
        protected void GoToFamilySelectPage( bool goingBack = false )
        {
            Response.Redirect( GetAttributeValue( "FamilySelectPageUrl" ) + ( goingBack ? "?back=true" : "" ), false );
        }

        /// <summary>
        /// Redirects to person select page.
        /// </summary>
        protected void GoToPersonSelectPage( bool goingBack = false )
        {
            Response.Redirect( GetAttributeValue( "PersonSelectPageUrl" ) + ( goingBack ? "?back=true" : "" ), false );
        }

        /// <summary>
        /// Redirects to group type select page.
        /// </summary>
        protected void GoToGroupTypeSelectPage( bool goingBack = false )
        {
            Response.Redirect( GetAttributeValue( "GroupTypeSelectPageUrl" ) + ( goingBack ? "?back=true" : "" ), false );
        }

        /// <summary>
        /// Redirects to location select page.
        /// </summary>
        protected void GoToLocationSelectPage( bool goingBack = false )
        {
            Response.Redirect( GetAttributeValue( "LocationSelectPageUrl" ) + ( goingBack ? "?back=true" : "" ), false );
        }

        /// <summary>
        /// Redirects to group select page.
        /// </summary>
        protected void GoToGroupSelectPage( bool goingBack = false )
        {
            Response.Redirect( GetAttributeValue( "GroupSelectPageUrl" ) + ( goingBack ? "?back=true" : "" ), false );
        }

        /// <summary>
        /// Redirects to time select page.
        /// </summary>
        protected void GoToTimeSelectPage()
        {
            Response.Redirect( GetAttributeValue( "TimeSelectPageUrl" ), false );
        }

        /// <summary>
        /// Redirects to success page.
        /// </summary>
        protected void GoToSuccessPage()
        {
            Response.Redirect( GetAttributeValue( "SuccessPageUrl" ), false );
        }

        private void GetState()
        {
            if ( Session["CheckInKioskId"] != null)
            {
                CurrentKioskId = (int)Session["CheckInKioskId"];
            }

            if ( Session["CheckInGroupTypeIds"] != null )
            {
                CurrentGroupTypeIds = Session["CheckInGroupTypeIds"] as List<int>;
            }

            if ( Session["CheckInWorkflow"] != null )
            {
                CurrentWorkflow = Session["CheckInWorkflow"] as Rock.Model.Workflow;
                if ( CurrentWorkflow != null )
                {
                    string stateString = CurrentWorkflow.GetAttributeValue( "CheckInState" );
                    if ( !String.IsNullOrEmpty( stateString ) )
                    {
                        CurrentCheckInState = CheckInState.FromJson( stateString );
                    }
                }
            }
            
            if (CurrentCheckInState == null && CurrentKioskId.HasValue && CurrentGroupTypeIds != null )
            {
                var kioskStatus = KioskCache.GetKiosk( CurrentKioskId.Value );

                // Remove any group types that were not selected in the admin configuration
                foreach ( var kioskGroupType in kioskStatus.KioskGroupTypes.ToList() )
                {
                    if ( !CurrentGroupTypeIds.Contains( kioskGroupType.GroupType.Id ) )
                    {
                        kioskStatus.KioskGroupTypes.Remove( kioskGroupType );
                    }
                }

                CurrentCheckInState = new CheckInState( kioskStatus );
            }
        }
    }
}