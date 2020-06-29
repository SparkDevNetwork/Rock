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
using System.Web;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.CheckIn
{
    /// <summary>
    /// A RockBlock specific to check-in
    /// </summary>
    [WorkflowTypeField(
        "Workflow Type",
        Key = AttributeKey.WorkflowType,
        Description = "The workflow type to activate for check-in",
        AllowMultiple = false,
        IsRequired = false,
        Order = 0 )]

    [TextField(
        "Workflow Activity",
        Key = AttributeKey.WorkflowActivity,
        Description = "The name of the workflow activity to run on selection.",
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Home Page",
        Key = AttributeKey.HomePage,
        Description = "",
        IsRequired = false,
        Order = 2 )]

    [LinkedPage(
        "Previous Page",
        Key = AttributeKey.PreviousPage,
        IsRequired = false,
        Order = 3 )]

    [LinkedPage(
        "Next Page",
        Key = AttributeKey.NextPage,
        IsRequired = false,
        Order = 4 )]

    public abstract class CheckInBlock : RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string WorkflowType = "WorkflowType";
            public const string WorkflowActivity = "WorkflowActivity";
            public const string HomePage = "HomePage";
            public const string PreviousPage = "PreviousPage";
            public const string NextPage = "NextPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        private static class PageParameterKey
        {
            public const string IsActive = "IsActive";
            public const string Override = "Override";
            public const string Back = "back";
        }

        #endregion Page Parameter Keys

        /// <summary>
        /// Gets or sets the local device configuration.
        /// </summary>
        /// <value>
        /// The local device configuration.
        /// </value>
        public LocalDeviceConfiguration LocalDeviceConfig { get; set; } = new LocalDeviceConfiguration();

        /// <summary>
        /// The current theme.
        /// </summary>
        [Obsolete( "Use LocalDeviceConfig..." )]
        [RockObsolete( "1.10" )]
        protected string CurrentTheme
        {
            get => LocalDeviceConfig.CurrentTheme;
            set => LocalDeviceConfig.CurrentTheme = value;
        }

        /// <summary>
        /// The current kiosk id
        /// </summary>
        [Obsolete( "Use LocalDeviceConfig..." )]
        [RockObsolete( "1.10" )]
        protected int? CurrentKioskId
        {
            get => LocalDeviceConfig.CurrentKioskId;
            set => LocalDeviceConfig.CurrentKioskId = value;
        }

        /// <summary>
        /// The current primary checkin-type id
        /// </summary>
        [Obsolete( "Use LocalDeviceConfig..." )]
        [RockObsolete( "1.10" )]
        protected int? CurrentCheckinTypeId
        {
            get
            {
                return LocalDeviceConfig.CurrentCheckinTypeId;
            }

            set
            {
                LocalDeviceConfig.CurrentCheckinTypeId = value;
                _currentCheckinType = null;
            }
        }

        /// <summary>
        /// The current group type ids (Checkin Areas)
        /// </summary>
        [Obsolete( "Use LocalDeviceConfig..." )]
        [RockObsolete( "1.10" )]
        protected List<int> CurrentGroupTypeIds
        {
            get => LocalDeviceConfig.CurrentGroupTypeIds;
            set => LocalDeviceConfig.CurrentGroupTypeIds = value;
        }

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

                if ( LocalDeviceConfig.CurrentCheckinTypeId.HasValue )
                {
                    _currentCheckinType = new CheckinType( LocalDeviceConfig.CurrentCheckinTypeId.Value );
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
                return this.PageParameter( PageParameterKey.Override )?.AsBoolean() ?? false;
            }
        }

        /// <summary>
        /// The current check-in state
        /// </summary>
        protected CheckInState CurrentCheckInState { get; set; }

        /// <summary>
        /// The current workflow
        /// </summary>
        protected Rock.Model.Workflow CurrentWorkflow
        {
            get
            {
                return _currentWorkflow;
            }

            set
            {
                _currentWorkflow = value;
            }
        }

        private Rock.Model.Workflow _currentWorkflow;

        /// <summary>
        /// Holds cookie names shared across certain check-in blocks.
        /// </summary>
        [Obsolete( "Use CheckInCookieKey instead" )]
        [RockObsolete( "1.10" )]
        public struct CheckInCookie
        {
            /// <summary>
            /// The local device configuration
            /// </summary>
            public static readonly string LocalDeviceConfig = CheckInCookieKey.LocalDeviceConfig;

            /// <summary>
            /// The name of the cookie that holds the DeviceId. Setters of this cookie should
            /// be sure to set the expiration to a time when the device is no longer valid.
            /// </summary>
            public static readonly string DEVICEID = CheckInCookieKey.DeviceId;

            /// <summary>
            /// The name of the cookie that holds whether or not the device was a mobile device.
            /// </summary>
            public static readonly string ISMOBILE = CheckInCookieKey.IsMobile;

            /// <summary>
            /// The phone number used to check in could be in this cookie.
            /// </summary>
            public static readonly string PHONENUMBER = CheckInCookieKey.PhoneNumber;
        }

        /// <summary>
        /// Gets a value indicating whether the kiosk has active group types and locations that 
        /// are open for check-in or check-out.
        /// </summary>
        /// <value>
        /// <c>true</c> if kiosk is active; otherwise, <c>false</c>.
        /// </value>
        protected bool KioskCurrentlyActive
        {
            get
            {
                if ( CurrentCheckInState == null || CurrentCheckInState.Kiosk == null || CurrentCheckInState.Kiosk.FilteredGroupTypes( LocalDeviceConfig.CurrentGroupTypeIds ).Count == 0 )
                {
                    return false;
                }
                else if ( !CurrentCheckInState.AllowCheckout && !CurrentCheckInState.Kiosk.HasActiveLocations( LocalDeviceConfig.CurrentGroupTypeIds ) )
                {
                    return false;
                }
                else if ( CurrentCheckInState.AllowCheckout && CurrentCheckInState.Kiosk.HasActiveCheckOutLocations( LocalDeviceConfig.CurrentGroupTypeIds ) )
                {
                    return true;
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
                if ( this.CurrentCheckInState != null )
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
            return CurrentCheckInState.Kiosk.Locations( LocalDeviceConfig.CurrentGroupTypeIds, rockContext );
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
                bool backingUp = this.PageParameter( PageParameterKey.Back ).AsBoolean();
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

            if ( this.ConfigurationRenderModeIsEnabled )
            {
                return;
            }

            // Tell the browsers to not cache any pages that have a block that inherits from CheckinBlock. This will help prevent browser using stale copy of checkin pages which could cause labels to get reprinted, and other expected things.
            Page.Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
            Page.Response.Cache.SetExpires( DateTime.UtcNow.AddHours( -1 ) );
            Page.Response.Cache.SetNoStore();

            GetState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( LocalDeviceConfig?.DisableIdleRedirect == true )
            {
                DisableIdleRedirectBlocks( true );
            }
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

            Guid? guid = GetAttributeValue( AttributeKey.WorkflowType ).AsGuidOrNull();
            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflowService = new WorkflowService( rockContext );

                    var workflowType = WorkflowTypeCache.Get( guid.Value );
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

            string workflowActivity = GetAttributeValue( AttributeKey.WorkflowActivity );
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

            string workflowActivity = GetAttributeValue( AttributeKey.WorkflowActivity );
            if ( string.IsNullOrEmpty( workflowActivity ) || ProcessActivity( workflowActivity, out errors ) )
            {
                if ( doNotProceedCondition() )
                {
                    modalAlert?.Show( conditionMessage, Rock.Web.UI.Controls.ModalAlertType.None );
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
                modalAlert?.Show( errorMsg.Replace( "'", @"\'" ), Rock.Web.UI.Controls.ModalAlertType.Warning );
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
        /// <param name="validateSelectionRequired">if set to <c>true</c> will check that block on previous page has a selection required before redirecting.</param>
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
            Guid? homePageOverride = LocalDeviceConfig?.HomePageOverride;

            if ( homePageOverride.HasValue )
            {
                NavigateToPage( homePageOverride.Value, null );
            }
            else
            {
                NavigateToLinkedPage( AttributeKey.HomePage );
            }
        }

        /// <summary>
        /// Navigates to next page.
        /// </summary>6
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
            bool pageIsBlocked = IsPageBlocked( GetAttributeValue( AttributeKey.NextPage ), queryParams );

            if ( pageIsBlocked )
            {
                NavigateToHomePage();
                return;
            }

            queryParams = CheckForOverride( queryParams );

            if ( validateSelectionRequired )
            {
                var nextBlock = GetCheckInBlock( AttributeKey.NextPage );
                if ( nextBlock != null && nextBlock.RequiresSelection( false ) )
                {
                    NavigateToLinkedPage( AttributeKey.NextPage, queryParams );
                }
            }
            else
            {
                NavigateToLinkedPage( AttributeKey.NextPage, queryParams );
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
            bool pageIsBlocked = IsPageBlocked( GetAttributeValue( AttributeKey.PreviousPage ), queryParams );

            if ( pageIsBlocked )
            {
                NavigateToHomePage();
                return;
            }

            if ( validateSelectionRequired )
            {
                var nextBlock = GetCheckInBlock( AttributeKey.PreviousPage );
                if ( nextBlock != null && nextBlock.RequiresSelection( true ) )
                {
                    NavigateToLinkedPage( AttributeKey.PreviousPage, queryParams );
                }
            }
            else
            {
                NavigateToLinkedPage( AttributeKey.PreviousPage, queryParams );
            }
        }

        /// <summary>
        /// Returns true of the
        /// </summary>
        /// <param name="pageAttributeKey">The page attribute key.</param>
        /// <param name="queryParams">The query parameters.</param>
        /// <returns>
        ///   <c>true</c> if [is page blocked] [the specified query parameters]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsPageBlocked( string pageAttributeKey, Dictionary<string, string> queryParams )
        {
            if ( LocalDeviceConfig.BlockedPageIds?.Any() == true )
            {
                var previousPagePageReference = new PageReference( pageAttributeKey, queryParams );
                if ( previousPagePageReference != null )
                {
                    // make sure we don't end up in an infinite loop
                    if ( previousPagePageReference.PageId != this.CurrentPageReference?.PageId )
                    {
                        if ( LocalDeviceConfig.BlockedPageIds.Contains( previousPagePageReference.PageId ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Builds and returns the URL for a linked <see cref="Rock.Model.Page" /> from a "linked page attribute" and any necessary query parameters.
        /// </summary>
        /// <param name="attributeKey">A <see cref="System.String" /> representing the name of the linked <see cref="Rock.Model.Page" /> attribute key.</param>
        /// <param name="queryParams">A <see cref="System.Collections.Generic.Dictionary{String,String}" /> containing the query string parameters to be added to the URL.
        /// In each <see cref="System.Collections.Generic.KeyValuePair{String,String}" /> the key value is a <see cref="System.String" /> that represents the name of the query string
        /// parameter, and the value is a <see cref="System.String" /> that represents the query string value..</param>
        /// <returns>
        /// A <see cref="System.String" /> representing the URL to the linked <see cref="Rock.Model.Page" />.
        /// </returns>
        public override string LinkedPageUrl( string attributeKey, Dictionary<string, string> queryParams = null )
        {
            var pageReference = new PageReference( GetAttributeValue( attributeKey ), queryParams );
            if ( pageReference.PageId > 0 )
            {
                var page = PageCache.Get( pageReference.PageId );
                if ( page != null && page.PageTitle == "Welcome" )
                {
                    if ( pageReference.Parameters == null )
                    {
                        pageReference.Parameters = new Dictionary<string, string>();
                    }

                    pageReference.Parameters.AddOrIgnore( PageParameterKey.IsActive, "True" );
                }

                return pageReference.BuildUrl();
            }
            else
            {
                return string.Empty;
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

                queryParams.AddOrReplace( PageParameterKey.Override, "True" );
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
        /// rather than just continuously redirected to next/previous page blocks and possibly exceeding the maximum
        /// number of redirects.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        protected CheckInBlock GetCheckInBlock( string attributeKey )
        {
            var pageReference = new PageReference( GetAttributeValue( attributeKey ) );
            if ( pageReference.PageId > 0 )
            {
                var page = PageCache.Get( pageReference.PageId );
                if ( page != null )
                {
                    foreach ( var block in page.Blocks.OrderBy( b => b.Order ) )
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

        private static class SessionKey
        {
            public const string CheckInState = "CheckInState";
            public const string CheckInWorkflow = "CheckInWorkflow";
        }

        /// <summary>
        /// Saves the current state of the LocalDeviceConfig, Kiosk, and workflow
        /// </summary>
        protected void SaveState()
        {
            this.LocalDeviceConfig.SaveToCookie( this.Page );

            Session[SessionKey.CheckInWorkflow] = CurrentWorkflow;

            if ( CurrentCheckInState != null )
            {
                Session[SessionKey.CheckInState] = CurrentCheckInState;
            }
            else
            {
                Session.Remove( SessionKey.CheckInState );
            }
        }

        /// <summary>
        /// Populates State variables that are stored in Cookies, Session, ViewState
        /// </summary>
        private void GetState()
        {
            this.LocalDeviceConfig = LocalDeviceConfig.GetFromCookie( this.Page );

            if ( this.LocalDeviceConfig == null )
            {
                this.LocalDeviceConfig = new LocalDeviceConfiguration();
            }

            if ( Session[SessionKey.CheckInWorkflow] != null )
            {
                CurrentWorkflow = Session[SessionKey.CheckInWorkflow] as Rock.Model.Workflow;
            }

            if ( Session[SessionKey.CheckInState] != null )
            {
                CurrentCheckInState = Session[SessionKey.CheckInState] as CheckInState;
            }

            if ( CurrentCheckInState == null && this.LocalDeviceConfig.CurrentKioskId.HasValue )
            {
                CurrentCheckInState = new CheckInState( this.LocalDeviceConfig );
            }
        }

        /// <summary>
        /// Gets the URL for the QRCode for the current AttendanceSession(s) that are in the <seealso cref="CheckInCookieKey.AttendanceSessionGuids" /> cookie.
        /// </summary>
        /// <param name="attendanceSessionGuidsCookie">The attendance session guids cookie.</param>
        /// <returns></returns>
        public string GetAttendanceSessionsQrCodeImageUrl( HttpCookie attendanceSessionGuidsCookie )
        {
            if ( attendanceSessionGuidsCookie == null || attendanceSessionGuidsCookie.Value.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var attendanceSessionGuids = attendanceSessionGuidsCookie.Value.Split( ',' ).AsGuidList();

            var guidListAsShortStringList = attendanceSessionGuids.Select( a => GuidHelper.ToShortString( a ) ).ToList().AsDelimited( "," );
            var qrCodeData = HttpUtility.UrlEncode( "PCL+" + guidListAsShortStringList );

            var qrCodeUrl = this.ResolveRockUrl( string.Format( "~/GetQRCode.ashx?data={0}&outputType=svg", qrCodeData ) );

            return qrCodeUrl;
        }
    }
}