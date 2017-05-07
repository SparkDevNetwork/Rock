// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.CheckIn
{
    /// <summary>
    /// A RockBlock specific to check-in
    /// </summary>
    [WorkflowTypeField( "Workflow Type", "The workflow type to activate for check-in", false, false, "", "", 0 )]
    [TextField( "Workflow Activity", "The name of the workflow activity to run on selection.", false, "", "", 1 )]
    [LinkedPage( "Home Page", "", false, "", "", 2 )]
    [LinkedPage( "Previous Page", "", false, "", "", 3 )]
    [LinkedPage( "Next Page", "", false, "", "", 4 )]
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
        /// The current primary checkin-type id
        /// </summary>
        protected int? CurrentCheckinTypeId
        {
            get { return _currentCheckinTypeId; }
            set
            {
                _currentCheckinTypeId = value;
                _currentCheckinType = null;
            }
        }
        private int? _currentCheckinTypeId;

        /// <summary>
        /// Gets the type of the current check in.
        /// </summary>
        /// <value>
        /// The type of the current check in.
        /// </value>
        protected CheckinType CurrentCheckInType
        {
            get
            {
                if ( _currentCheckinType != null )
                {
                    return _currentCheckinType;
                }

                if ( CurrentCheckinTypeId.HasValue )
                {
                    _currentCheckinType = new CheckinType( CurrentCheckinTypeId.Value );
                    return _currentCheckinType;
                }

                return null;
            }
        }
        private CheckinType _currentCheckinType;

        /// <summary>
        /// Gets a value indicating whether check-in is currently in override mode
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is override; otherwise, <c>false</c>.
        /// </value>
        protected bool IsOverride
        {
            get
            {
                return Request["Override"] != null && Request["Override"].AsBoolean();
            }
        }

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
        /// Gets or sets a value indicating whether [manager logged in].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [manager logged in]; otherwise, <c>false</c>.
        /// </value>
        protected bool ManagerLoggedIn
        {
            get
            {
                return this.CurrentCheckInState != null && this.CurrentCheckInState.ManagerLoggedIn;
            }

            set
            {
                if (this.CurrentCheckInState != null)
                {
                    this.CurrentCheckInState.ManagerLoggedIn = value;
                }
            }
        }

        /// <summary>
        /// Gets the person schedule sub title.
        /// </summary>
        /// <returns></returns>
        protected string GetPersonScheduleSubTitle()
        {
            if ( CurrentCheckInState != null )
            {
                var person = CurrentCheckInState.CheckIn.CurrentPerson;
                if ( person != null )
                {
                    var schedule = person.CurrentSchedule;
                    if ( schedule != null )
                    {
                        // If check-in is not configured to automatically select same options for each service
                        // or option was not available (i.e. not first service ) then show name/service
                        if ( !CurrentCheckInState.CheckInType.UseSameOptions ||
                            ( schedule.Schedule.Id != person.SelectedSchedules.First().Schedule.Id ) )
                        {
                            return string.Format( "{0} @ {1}", person, schedule );
                        }
                    }

                    return person.ToString();

                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns the locations for this Kiosk for the configured group types
        /// </summary>
        /// <value>
        /// The locations.
        /// </value>
        protected IEnumerable<Location> GetGroupTypesLocations( RockContext rockContext )
        {
            return CurrentCheckInState.Kiosk.Locations( CurrentGroupTypeIds, rockContext );
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
                    var workflowService = new WorkflowService( rockContext );

                    var workflowType = WorkflowTypeCache.Read( guid.Value );
                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                    {
                        if ( CurrentWorkflow == null )
                        {
                            CurrentWorkflow = Rock.Model.Workflow.Activate( workflowType, CurrentCheckInState.Kiosk.Device.Name, rockContext );
                            
                            if ( IsOverride )
                            {
                                CurrentWorkflow.SetAttributeValue( "Override", "True" );
                            }
                        }

                        var activityType = workflowType.ActivityTypes.Where( a => a.Name == activityName ).FirstOrDefault();
                        if ( activityType != null )
                        {
                            WorkflowActivity.Activate( activityType, CurrentWorkflow, rockContext );
                            if ( workflowService.Process( CurrentWorkflow, CurrentCheckInState, out errorMessages ) )
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

            if ( CurrentCheckinTypeId.HasValue )
            {
                Session["CheckinTypeId"] = CurrentCheckinTypeId.Value;
            }
            else
            {
                Session.Remove( "CheckinTypeId" );
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
            return ProcessSelection( modalAlert, false );
        }

        /// <summary>
        /// Processes the selection, save state and navigates to the next page if no errors
        /// are encountered during processing the activity.
        /// </summary>
        /// <param name="modalAlert">The modal alert control to show if errors occur.</param>
        /// <returns>a list of errors encountered during processing the activity</returns>
        /// <param name="validateSelectionRequired">if set to <c>true</c> will check that block on next page has a selection required before redirecting.</param>
        protected virtual List<string> ProcessSelection( Rock.Web.UI.Controls.ModalAlert modalAlert, bool validateSelectionRequired )
        {
            var errors = new List<string>();

            string workflowActivity = GetAttributeValue( "WorkflowActivity" );
            if ( string.IsNullOrEmpty( workflowActivity ) || ProcessActivity( workflowActivity, out errors ) )
            {
                SaveState();
                NavigateToNextPage( validateSelectionRequired );
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
            return ProcessSelection( modalAlert, doNotProceedCondition, conditionMessage, false );
        }

        /// <summary>
        /// Processes the selection, save state and navigates to the next page if no errors
        /// are encountered during processing the activity.
        /// </summary>
        /// <param name="modalAlert">The modal alert control to show if errors occur.</param>
        /// <param name="doNotProceedCondition">A condition that must be met after processing
        /// the activity in order to save state and continue to the next page.</param>
        /// <param name="conditionMessage">The message to display in the modal if the condition fails.</param>
        /// <param name="validateSelectionRequired">if set to <c>true</c> will check that block on next page has a selection required before redirecting.</param>
        /// <returns></returns>
        protected virtual bool ProcessSelection( Rock.Web.UI.Controls.ModalAlert modalAlert, Func<bool> doNotProceedCondition, string conditionMessage, bool validateSelectionRequired )
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
                    NavigateToNextPage( validateSelectionRequired );
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
            GoBack( false );
        }

        /// <summary>
        /// Goes the back.
        /// </summary>
        /// <param name="validateSelectionRequired">if set to <c>true</c> will check that block on prev page has a selection required before redirecting.</param>
        protected virtual void GoBack( bool validateSelectionRequired )
        {
            SaveState();
            NavigateToPreviousPage( validateSelectionRequired );
        }

        /// <summary>
        /// Cancels the check-in.
        /// </summary>
        protected virtual void CancelCheckin()
        {
            NavigateToHomePage();
        }

        /// <summary>
        /// Navigates to the check-in home page.
        /// </summary>
        protected virtual void NavigateToHomePage()
        {
            NavigateToLinkedPage( "HomePage" );
        }

        /// <summary>
        /// Navigates to next page.
        /// </summary>
        protected virtual void NavigateToNextPage()
        {
            NavigateToNextPage( null, false );
        }

        /// <summary>
        /// Navigates to next page.
        /// </summary>
        /// <param name="validateSelectionRequired">if set to <c>true</c> will check that block on next page has a selection required before redirecting.</param>
        protected virtual void NavigateToNextPage( bool validateSelectionRequired )
        {
            NavigateToNextPage( null, validateSelectionRequired );
        }

        /// <summary>
        /// Navigates to next page.
        /// </summary>
        /// <param name="queryParams">The query parameters.</param>
        protected virtual void NavigateToNextPage( Dictionary<string, string> queryParams )
        {
            NavigateToNextPage( queryParams, false );
        }

        /// <summary>
        /// Navigates to next page.
        /// </summary>
        /// <param name="queryParams">The query parameters.</param>
        /// <param name="validateSelectionRequired">if set to <c>true</c> will check that block on next page has a selection required before redirecting.</param>
        protected virtual void NavigateToNextPage( Dictionary<string, string> queryParams, bool validateSelectionRequired )
        {
            queryParams = CheckForOverride( queryParams );

            if ( validateSelectionRequired )
            {
                var nextBlock = GetCheckInBlock( "NextPage" );
                if ( nextBlock != null && nextBlock.RequiresSelection( false ) )
                {
                    NavigateToLinkedPage( "NextPage", queryParams );
                }
            }
            else
            {
                NavigateToLinkedPage( "NextPage", queryParams );
            }
        }

        /// <summary>
        /// Navigates to previous page.
        /// </summary>
        protected virtual void NavigateToPreviousPage()
        {
            NavigateToPreviousPage( false );
        }

        /// <summary>
        /// Navigates to previous page.
        /// </summary>
        /// <param name="validateSelectionRequired">if set to <c>true</c> will check that block on previous page has a selection required before redirecting.</param>
        protected virtual void NavigateToPreviousPage( bool validateSelectionRequired )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "back", "true" );

            queryParams = CheckForOverride( queryParams );

            NavigateToPreviousPage( queryParams, validateSelectionRequired );
        }

        /// <summary>
        /// Navigates to previous page.
        /// </summary>
        /// <param name="queryParams">The query parameters.</param>
        protected virtual void NavigateToPreviousPage( Dictionary<string, string> queryParams )
        {
            NavigateToPreviousPage( queryParams, false );
        }

        /// <summary>
        /// Navigates to previous page.
        /// </summary>
        /// <param name="queryParams">The query parameters.</param>
        /// <param name="validateSelectionRequired">if set to <c>true</c> will check that block on previous page has a selection required before redirecting.</param>
        protected virtual void NavigateToPreviousPage( Dictionary<string, string> queryParams, bool validateSelectionRequired )
        {
            if ( validateSelectionRequired )
            {
                var nextBlock = GetCheckInBlock( "PreviousPage" );
                if ( nextBlock != null && nextBlock.RequiresSelection( true ) )
                {
                    NavigateToLinkedPage( "PreviousPage", queryParams );
                }
            }
            else
            {
                NavigateToLinkedPage( "PreviousPage", queryParams );
            }
        }

        /// <summary>
        /// Checks if the override option is currently being used.
        /// </summary>
        /// <param name="queryParams">The query parameters.</param>
        protected Dictionary<string, string> CheckForOverride( Dictionary<string, string> queryParams = null )
        {
            if ( IsOverride )
            {
                if ( queryParams == null )
                {
                    queryParams = new Dictionary<string, string>();
                }
                queryParams.AddOrReplace( "Override", "True" );
            }
            return queryParams;
        }

        /// <summary>
        /// Determines if the block requires that a selection be made. This is used to determine if user should
        /// be redirected to this block or not.
        /// </summary>
        /// <param name="backingUp">if set to <c>true</c> [backing up].</param>
        /// <returns></returns>
        public virtual bool RequiresSelection( bool backingUp )
        {
            return true;
        }

        /// <summary>
        /// Loads a check-in block to determine if it will require a selection or not. This is used to find the
        /// next page/block that does require a selection so that user can be redirected once to that block, 
        /// rather than just continuesly redirected to next/prev page blocks and possibly exceeding the maximum
        /// number of redirects.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        protected CheckInBlock GetCheckInBlock( string attributeKey )
        {
            var pageReference = new PageReference( GetAttributeValue( attributeKey ) );
            if ( pageReference.PageId > 0 )
            {
                var page = Rock.Web.Cache.PageCache.Read( pageReference.PageId );
                if ( page != null )
                {
                    foreach( var block in page.Blocks.OrderBy( b => b.Order ) )
                    { 
                        var control = TemplateControl.LoadControl( block.BlockType.Path );
                        if ( control != null )
                        {
                            var checkinBlock = control as CheckInBlock;
                            if ( checkinBlock != null )
                            {
                                checkinBlock.SetBlock( page, block, true, true );
                                checkinBlock.GetState();
                                return checkinBlock;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
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

            if ( Session["CheckinTypeId"] != null )
            {
                CurrentCheckinTypeId = (int)Session["CheckinTypeId"];
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
                CurrentCheckInState = new CheckInState( CurrentKioskId.Value, CurrentCheckinTypeId, CurrentGroupTypeIds );
            }
        }

    }
}