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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace Rock.CheckIn
{
    /// <summary>
    /// A RockBlock specific to check-in
    /// </summary>
    [LinkedPage( "Home Page" )]
    [LinkedPage( "Next Page" )]
    [LinkedPage( "Previous Page" )]
    [WorkflowTypeField( "Workflow Type", "The workflow type to activate for check-in" )]
    [TextField( "Workflow Activity", "The name of the workflow activity to run on selection.", false, "" )]
    public abstract class CheckInBlock : RockBlock
    {

        /// <summary>
        /// The current theme.
        /// </summary>
        protected string CurrentTheme { get; set; }

        /// <summary>
        /// The current kiosk id
        /// </summary>
        protected int? CurrentKioskId { get; set; }

        /// <summary>
        /// The current group type ids
        /// </summary>
        protected List<int> CurrentGroupTypeIds;

        /// <summary>
        /// The current check-in state
        /// </summary>
        protected CheckInState CurrentCheckInState;

        /// <summary>
        /// The current workflow
        /// </summary>
        protected Rock.Model.Workflow CurrentWorkflow;

        /// <summary>
        /// Holds cookie names shared across certain check-in blocks.
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
                    CurrentCheckInState.Kiosk.FilteredGroupTypes( CurrentGroupTypeIds ).Count == 0 ||
                    !CurrentCheckInState.Kiosk.HasActiveLocations( CurrentGroupTypeIds ) )
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
        protected bool ProcessActivity( string activityName, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            Guid? guid = GetAttributeValue( "WorkflowType" ).AsGuidOrNull();
            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflowTypeService = new WorkflowTypeService( rockContext );
                    var workflowType = workflowTypeService.Queryable( "ActivityTypes" )
                        .Where( w => w.Guid.Equals( guid.Value ) )
                        .FirstOrDefault();

                    if ( workflowType != null )
                    {
                        if ( CurrentWorkflow == null )
                        {
                            CurrentWorkflow = Rock.Model.Workflow.Activate( workflowType, CurrentCheckInState.Kiosk.Device.Name, rockContext );
                            
                            if (Request["Override"] != null)
                            {
                                if ( Request["Override"].ToString() == "True" )
                                {
                                    CurrentWorkflow.SetAttributeValue( "Override", "True" );
                                }
                            }
                            
                        }

                        var activityType = workflowType.ActivityTypes.Where( a => a.Name == activityName ).FirstOrDefault();
                        if ( activityType != null )
                        {
                            WorkflowActivity.Activate( activityType, CurrentWorkflow, rockContext );
                            if ( CurrentWorkflow.Process( rockContext, CurrentCheckInState, out errorMessages ) )
                            {
                                // Keep workflow active for continued processing
                                CurrentWorkflow.CompletedDateTime = null;

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
                        errorMessages.Add( "Invalid Workflow Type" );
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Saves the current state of the kiosk and workflow
        /// </summary>
        protected void SaveState()
        {
            if ( !string.IsNullOrWhiteSpace( CurrentTheme))
            {
                Session["CheckInTheme"] = CurrentTheme;
            }

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

            if ( CurrentCheckInState != null )
            {
                Session["CheckInState"] = CurrentCheckInState;
            }
            else
            {
                Session.Remove( "CheckInState" );
            }

            if ( CurrentWorkflow != null )
            {
                Session["CheckInWorkflow"] = CurrentWorkflow;
            }
            else
            {
                Session.Remove( "CheckInWorkflow" );
            }
        }

        /// <summary>
        /// Processes the selection, save state and navigates to the next page if no errors
        /// are encountered during processing the activity.
        /// </summary>
        /// <param name="modalAlert">The modal alert control to show if errors occur.</param>
        /// <returns>a list of errors encountered during processing the activity</returns>
        protected virtual List<string> ProcessSelection( Rock.Web.UI.Controls.ModalAlert modalAlert )
        {
            var errors = new List<string>();

            string workflowActivity = GetAttributeValue( "WorkflowActivity" );
            if ( string.IsNullOrEmpty( workflowActivity ) || ProcessActivity( workflowActivity, out errors ) )
            {
                SaveState();
                NavigateToNextPage();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                modalAlert.Show( errorMsg.Replace( "'", @"\'" ), Rock.Web.UI.Controls.ModalAlertType.Warning );
            }

            return errors;
        }

        /// <summary>
        /// Processes the selection, save state and navigates to the next page if no errors
        /// are encountered during processing the activity.  
        /// </summary>
        /// <param name="modalAlert">The modal alert control to show if errors occur.</param>
        /// <param name="doNotProceedCondition">A condition that must be met after processing
        /// the activity in order to save state and continue to the next page.</param>
        /// <param name="conditionMessage">The message to display in the modal if the condition fails.</param>
        /// <returns></returns>
        protected virtual bool ProcessSelection( Rock.Web.UI.Controls.ModalAlert modalAlert, Func<bool> doNotProceedCondition, string conditionMessage )
        {
            var errors = new List<string>();

            string workflowActivity = GetAttributeValue( "WorkflowActivity" );
            if ( string.IsNullOrEmpty( workflowActivity ) || ProcessActivity( workflowActivity, out errors ) )
            {
                if ( doNotProceedCondition() )
                {
                    modalAlert.Show( conditionMessage, Rock.Web.UI.Controls.ModalAlertType.Warning );
                    return false;
                }
                else
                {
                    SaveState();
                    NavigateToNextPage();
                    return true;
                }
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                modalAlert.Show( errorMsg.Replace( "'", @"\'" ), Rock.Web.UI.Controls.ModalAlertType.Warning );
                return false;
            }
        }

        /// <summary>
        /// Do nothing (such as unselecting something) but simply return to previous screen.
        /// </summary>
        protected virtual void GoBack()
        {
            SaveState();
            NavigateToPreviousPage();
        }

        /// <summary>
        /// Cancels the check-in.
        /// </summary>
        protected void CancelCheckin()
        {
            NavigateToHomePage();
        }

        /// <summary>
        /// Navigates to the check-in home page.
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
        /// Navigates to next page.
        /// </summary>
        protected void NavigateToNextPage( Dictionary<string, string> queryParams )
        {
            NavigateToLinkedPage( "NextPage", queryParams );
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
            if ( Session["CurrentTheme"] != null )
            {
                CurrentTheme = Session["CurrentTheme"].ToString();
            }

            if ( Session["CheckInKioskId"] != null )
            {
                CurrentKioskId = (int)Session["CheckInKioskId"];
            }

            if ( Session["CheckInGroupTypeIds"] != null )
            {
                CurrentGroupTypeIds = Session["CheckInGroupTypeIds"] as List<int>;
            }

            if ( Session["CheckInState"] != null )
            {
                CurrentCheckInState = Session["CheckInState"] as CheckInState;
            }

            if ( Session["CheckInWorkflow"] != null )
            {
                CurrentWorkflow = Session["CheckInWorkflow"] as Rock.Model.Workflow;
            }

            if ( CurrentCheckInState == null && CurrentKioskId.HasValue )
            {
                CurrentCheckInState = new CheckInState( CurrentKioskId.Value, CurrentGroupTypeIds );
            }
        }
    }
}