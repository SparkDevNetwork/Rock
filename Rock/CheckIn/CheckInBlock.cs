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
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.CheckIn
{
    /// <summary>
    /// A RockBlock specific to check-in
    /// </summary>
    [LinkedPage("Home Page")]
    [LinkedPage("Next Page")]
    [LinkedPage("Previous Page")]
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
        /// The current ministry group type ids
        /// </summary>
        protected List<int> CurrentRoomGroupTypeIds;

        /// <summary>
        /// The current check in state
        /// </summary>
        protected CheckInState CurrentCheckInState;

        /// <summary>
        /// The current workflow
        /// </summary>
        protected Rock.Model.Workflow CurrentWorkflow;

        /// <summary>
        /// Holds cookie names shared across certain checkin blocks.
        /// </summary>
        public struct CheckInCookie
        {
            /// <summary>
            /// The name of the cookie that holds the DeviceId. Setters of this cookie should
            /// be sure to set the expiration to a time when the device is no longer valid.
            /// </summary>
            public static readonly string DEVICEID = "Checkin.DeviceId";

            /// <summary>
            /// The name of the cookie that holds whether or not the device was a mobile device.
            /// </summary>
            public static readonly string ISMOBILE = "Checkin.IsMobile";
        }

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

            if ( CurrentRoomGroupTypeIds != null )
            {
                Session["CheckInRoomGroupTypeIds"] = CurrentRoomGroupTypeIds;
            }
            else
            {
                Session.Remove( "CheckInRoomGroupTypeIds" );
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
            NavigateToHomePage();
        }

        /// <summary>
        /// Navigates to the checkin home page.
        /// </summary>
        protected void NavigateToHomePage()
        {
            NavigateToLinkedPage( "HomePage" );
        }

        /// <summary>
        /// Navigates to next page.
        /// </summary>
        protected void NavigateToNextPage()
        {
            NavigateToLinkedPage( "NextPage" );
        }

        /// <summary>
        /// Navigates to previous page.
        /// </summary>
        protected void NavigateToPreviousPage()
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "back", "true" );
            NavigateToLinkedPage( "PreviousPage", queryParams );
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

            if ( Session["CheckInRoomGroupTypeIds"] != null )
            {
                CurrentRoomGroupTypeIds = Session["CheckInRoomGroupTypeIds"] as List<int>;
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
                if ( kioskStatus != null )
                {
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
}