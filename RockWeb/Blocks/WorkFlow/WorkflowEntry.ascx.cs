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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.ElectronicSignature;
using Rock.Model;
using Rock.Pdf;
using Rock.Security;
using Rock.Transactions;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Workflow.FormBuilder;

namespace RockWeb.Blocks.WorkFlow
{
    /// <summary>
    /// </summary>
    [DisplayName( "Workflow Entry" )]
    [Category( "WorkFlow" )]
    [Description( "Used to enter information for a workflow form entry action." )]

    #region Block Attributes

    [WorkflowTypeField(
        "Workflow Type",
        Description = "Type of workflow to start.",
        Key = AttributeKey.WorkflowType,
        Order = 0 )]

    [BooleanField(
        "Show Summary View",
        Description = "If workflow has been completed, should the summary view be displayed?",
        Key = AttributeKey.ShowSummaryView,
        Order = 1 )]

    [CodeEditorField(
        "Block Title Template",
        Description = "Lava template for determining the title of the block. If not specified, the name of the Workflow Type will be shown.",
        Key = AttributeKey.BlockTitleTemplate,
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 100,
        IsRequired = false,
        Order = 2 )]

    [TextField(
        "Block Title Icon CSS Class",
        Description = "The CSS class for the icon displayed in the block title. If not specified, the icon for the Workflow Type will be shown.",
        Key = AttributeKey.BlockTitleIconCssClass,
        IsRequired = false,
        Order = 3 )]

    [BooleanField(
        "Disable Passing WorkflowId",
        Description = "If disabled, prevents the use of a Workflow Id (WorkflowId=) from being passed in and only accepts a WorkflowGuid.",
        Key = AttributeKey.DisablePassingWorkflowId,
        DefaultBooleanValue = false,
        Order = 4
        )]

    [BooleanField(
        "Disable Passing WorkflowTypeId",
        Description = "If set, it prevents the use of a Workflow Type Id (WorkflowTypeId=) from being passed in and only accepts a WorkflowTypeGuid.  " +
        "To use this block setting on your external site, you will need to create a new page and add the Workflow Entry block to it.  " +
        "You may also add a new route so that URLs are in the pattern .../{PageRoute}/{WorkflowTypeGuid}.  " +
        "If your workflow uses a form, you will also need to adjust email content to ensure that your URLs are correct.",
        Key = AttributeKey.DisablePassingWorkflowTypeId,
        DefaultBooleanValue = false,
        Order = 5 )]

    [BooleanField(
        "Log Interaction when Form is Viewed",
        Key = AttributeKey.LogInteractionOnView,
        DefaultBooleanValue = true,
        Order = 6 )]

    [BooleanField(
        "Log Interaction when Form is Completed",
        Key = AttributeKey.LogInteractionOnCompletion,
        DefaultBooleanValue = true,
        Order = 7 )]

    [BooleanField(
        "Disable Captcha Support",
        Description = "If set to 'Yes' the CAPTCHA verification step will not be performed.",
        Key = AttributeKey.DisableCaptchaSupport,
        DefaultBooleanValue = false,
        Order = 8
        )]
    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.WORKFLOW_ENTRY )]
    public partial class WorkflowEntry : Rock.Web.UI.RockBlock, IPostBackEventHandler
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string WorkflowType = "WorkflowType";
            public const string ShowSummaryView = "ShowSummaryView";
            public const string BlockTitleTemplate = "BlockTitleTemplate";
            public const string BlockTitleIconCssClass = "BlockTitleIconCssClass";
            public const string DisablePassingWorkflowId = "DisablePassingWorkflowId";
            public const string DisablePassingWorkflowTypeId = "DisablePassingWorkflowTypeId";
            public const string LogInteractionOnView = "LogInteractionOnView";
            public const string LogInteractionOnCompletion = "LogInteractionOnCompletion";
            public const string DisableCaptchaSupport = "DisableCaptchaSupport";
        }

        #endregion Attribute Keys

        #region PageParameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string WorkflowId = "WorkflowId";
            public const string WorkflowGuid = "WorkflowGuid";
            public const string WorkflowName = "WorkflowName";
            public const string ActionId = "ActionId";
            public const string WorkflowTypeId = "WorkflowTypeId";
            public const string WorkflowTypeGuid = "WorkflowTypeGuid";
            public const string Command = "Command";
            public const string GroupId = "GroupId";
            public const string PersonId = "PersonId";
            public const string InteractionStartDateTime = "InteractionStartDateTime";

            // NOTE that the actual parameter for CampusId and CampusGuid is just 'Campus', but making them different internally to make it clearer
            public const string CampusId = "Campus";
            public const string CampusGuid = "Campus";
        }

        #endregion PageParameter Keys

        private static class ViewStateKey
        {
            public const string WorkflowTypeGuid = "WorkflowTypeGuid";
            public const string ActionTypeId = "ActionTypeId";
            public const string WorkflowId = "WorkflowId";
            public const string WorkflowTypeDeterminedByBlockAttribute = "WorkflowTypeDeterminedByBlockAttribute";
            public const string InteractionStartDateTime = "InteractionStartDateTime";
            public const string SignatureDocumentHtml = "SignatureDocumentHtml";
            public const string IsCaptchaValid = "IsCaptchaValid";
        }

        #region Fields

        // Have a class level RockContext, WorkflowService and Workflow, etc that will be used for all WorkflowService operations
        // This will allow workflow values to be persisted correctly since they are all using the same RockContext
        private RockContext _workflowRockContext;
        private WorkflowService _workflowService;
        private Workflow _workflow = null;
        private WorkflowActivity _activity = null;
        private WorkflowAction _action = null;

        private WorkflowActionTypeCache _actionType = null;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the workflow type guid.
        /// </summary>
        /// <value>
        /// The workflow type guid.
        /// </value>
        public string WorkflowTypeGuid
        {
            get { return ViewState[ViewStateKey.WorkflowTypeGuid] as string; }
            set { ViewState[ViewStateKey.WorkflowTypeGuid] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the workflow type was set by attribute.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [configured type]; otherwise, <c>false</c>.
        /// </value>
        public bool WorkflowTypeDeterminedByBlockAttribute
        {
            get { return ViewState[ViewStateKey.WorkflowTypeDeterminedByBlockAttribute] as bool? ?? false; }
            set { ViewState[ViewStateKey.WorkflowTypeDeterminedByBlockAttribute] = value; }
        }

        /// <summary>
        /// Gets or sets the workflow identifier.
        /// </summary>
        /// <value>
        /// The workflow identifier.
        /// </value>
        public int? WorkflowId
        {
            get { return ViewState[ViewStateKey.WorkflowId] as int?; }
            set { ViewState[ViewStateKey.WorkflowId] = value; }
        }

        /// <summary>
        /// Gets or sets the action type identifier.
        /// </summary>
        /// <value>
        /// The action type identifier.
        /// </value>
        public int? ActionTypeId
        {
            get { return ViewState[ViewStateKey.ActionTypeId] as int?; }
            set { ViewState[ViewStateKey.ActionTypeId] = value; }
        }

        /// <summary>
        /// Gets or sets the interaction start date time.
        /// </summary>
        /// <value>
        /// The interaction start date time.
        /// </value>
        public DateTime? InteractionStartDateTime
        {
            get { return ViewState[ViewStateKey.InteractionStartDateTime] as DateTime?; }
            set { ViewState[ViewStateKey.InteractionStartDateTime] = value; }
        }

        /// <summary>
        /// Gets or sets the signature document HTML, not including the SignatureData.
        /// </summary>
        /// <value>The signature document HTML.</value>
        public string SignatureDocumentHtml
        {
            get { return ViewState[ViewStateKey.SignatureDocumentHtml] as string; }
            set { ViewState[ViewStateKey.SignatureDocumentHtml] = value; }
        }

        public bool IsCaptchaValid
        {
            get { return ViewState[ViewStateKey.IsCaptchaValid] as bool? ?? false; }
            set { ViewState[ViewStateKey.IsCaptchaValid] = value; }
        }

        #endregion Properties

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            if ( HydrateObjects() )
            {
                BuildWorkflowActionUI( false );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            SetBlockTitle();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // If PostBack is triggered by captcha leave message notification as is.
            if ( this.Page.Request.Params["__EVENTARGUMENT"] != "TokenReceived" )
            {
                nbMessage.Visible = false;
            }

            if ( !Page.IsPostBack )
            {
                if ( HydrateObjects() )
                {
                    InitializeInteractions();

                    BuildWorkflowActionUI( true );
                    ProcessActionRequest();
                }
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// OnInit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            var workflowType = GetWorkflowType();

            if ( workflowType != null && !WorkflowTypeDeterminedByBlockAttribute )
            {
                breadCrumbs.Add( new BreadCrumb( workflowType.Name, pageReference ) );
            }

            return breadCrumbs;
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( tbRockFullName.Text.IsNotNullOrWhiteSpace() )
            {
                /* 03/22/2021 MDP
                    see https://app.asana.com/0/1121505495628584/1200018171012738/f on why this is done
                */

                nbRockFullName.Visible = true;
                nbRockFullName.NotificationBoxType = NotificationBoxType.Validation;
                nbRockFullName.Text = "Invalid Form Value";
                return;
            }

            var disableCaptchaSupport = GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean();
            if ( !disableCaptchaSupport && cpCaptcha.IsAvailable && !IsCaptchaValid )
            {
                ShowMessage( NotificationBoxType.Validation, string.Empty, "There was an issue processing your request. Please try again. If the issue persists please contact us." );
                return;
            }

            /* 
                01/20/2023 ETD
                Update to Mike's comment on 5/18/2022
                The cancel button is being used as a psuedo "save form" button that multiple churches are depending on.
                This functionality needs to remain until a new button is created to perform a "Save Form" function. With this in mind the changes
                in commit 70a484a9 have been undone.

                05/18/2022 MDP (this comment is no longer valid as of 01/20/2023)

                Update on the 04/27/2022 note. After discussing with Product team,
                the intended behavior is that *none* of the form values should save
                if the button doesn't do validation.  It was sort of a bug that it used to do that.

                04/27/2022 CWR

                The only Form Action that should not get PersonEntry values is "Cancel",
                but to avoid a string comparison, we will check the Action's "Causes Validation".
                "Cancel" should be the only Form Action that does not cause validation.
                If the Form Action exists, complete the Form Action, regardless of the Form Action validation.
            
            */
            var formUserActions = WorkflowActionFormUserAction.FromUriEncodedString( _actionType.WorkflowForm.Actions );
            var formUserAction = formUserActions.FirstOrDefault( x => x.ActionName == eventArgument );

            if ( formUserAction != null && formUserAction.CausesValidation )
            {
                using ( var personEntryRockContext = new RockContext() )
                {
                    GetWorkflowFormPersonEntryValues( personEntryRockContext );
                }
            }

            SetWorkflowFormAttributeValues();
            CompleteFormAction( eventArgument );
        }

        /// <summary>
        /// Event raised after the Captcha control receives a token for a solved Captcha.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void cpCaptcha_TokenReceived( object sender, Captcha.TokenReceivedEventArgs e )
        {
            if ( e.IsValid )
            {
                cpCaptcha.Visible = false;
                AddSubmitButtons( _actionType?.WorkflowForm );
            }

            IsCaptchaValid = e.IsValid;
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Hydrates the objects.
        /// </summary>
        /// <returns></returns>
        private bool HydrateObjects()
        {
            var workflowType = GetWorkflowType();

            // Set the note type if this is first request
            if ( !Page.IsPostBack )
            {
                var entityType = EntityTypeCache.Get( typeof( Rock.Model.Workflow ) );
                var noteTypes = NoteTypeCache.GetByEntity( entityType.Id, string.Empty, string.Empty );
                ncWorkflowNotes.NoteOptions.SetNoteTypes( noteTypes );
            }

            // Get the block setting to disable passing WorkflowTypeID set.
            bool allowPassingWorkflowTypeId = !this.GetAttributeValue( AttributeKey.DisablePassingWorkflowTypeId ).AsBoolean();

            var disableCaptchaSupport = GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() || !cpCaptcha.IsAvailable;
            cpCaptcha.Visible = !disableCaptchaSupport;

            if ( workflowType == null )
            {
                ShowNotes( false );

                // Include an additional message if the block setting to 'disable passing WorkflowTypeId' is true.
                string additionalMessage = allowPassingWorkflowTypeId ? string.Empty : "  Please verify the block settings for this Workflow Entry.";
                ShowMessage( NotificationBoxType.Danger, "Configuration Error", "Workflow type was not configured or specified correctly." + additionalMessage );
                return false;
            }

            bool isLoginRequired;
            if ( workflowType.FormBuilderTemplate != null )
            {
                isLoginRequired = workflowType.FormBuilderTemplate.IsLoginRequired;
            }
            else
            {
                isLoginRequired = workflowType.IsLoginRequired;
            }

            // Check Login Requirement
            if ( isLoginRequired == true && CurrentUser == null )
            {
                var site = RockPage.Site;
                if ( site.LoginPageId.HasValue )
                {
                    site.RedirectToLoginPage( true );
                }
                else
                {
                    System.Web.Security.FormsAuthentication.RedirectToLoginPage();
                }
            }

            if ( !workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                ShowNotes( false );
                ShowMessage( NotificationBoxType.Warning, "Sorry", "You are not authorized to view this type of workflow." );
                return false;
            }

            if ( !( workflowType.IsActive ?? true ) )
            {
                ShowNotes( false );
                ShowMessage( NotificationBoxType.Warning, "Sorry", "This type of workflow is not active." );
                return false;
            }

            if ( workflowType.FormStartDateTime.HasValue && workflowType.FormStartDateTime.Value > RockDateTime.Now )
            {
                ShowNotes( false );
                ShowMessage( NotificationBoxType.Warning, "Sorry", "This type of workflow is not active." );
                return false;
            }

            if ( workflowType.FormEndDateTime.HasValue && workflowType.FormEndDateTime.Value < RockDateTime.Now )
            {
                ShowNotes( false );
                ShowMessage( NotificationBoxType.Warning, "Sorry", "This type of workflow is not active." );
                return false;
            }

            if ( workflowType.WorkflowExpireDateTime.HasValue && workflowType.WorkflowExpireDateTime.Value < RockDateTime.Now )
            {
                ShowNotes( false );
                ShowMessage( NotificationBoxType.Warning, "Sorry", "This type of workflow is not active." );
                return false;
            }

            if ( workflowType.FormBuilderTemplate != null && workflowType.FormBuilderTemplate.IsActive == false )
            {
                ShowNotes( false );
                ShowMessage( NotificationBoxType.Warning, "Sorry", "This type of workflow is not active." );
                return false;
            }

            _workflowRockContext = new RockContext();
            _workflowService = new WorkflowService( _workflowRockContext );

            // If operating against an existing workflow, get the workflow and load attributes
            if ( !WorkflowId.HasValue )
            {
                bool allowPassingWorkflowId = !this.GetAttributeValue( AttributeKey.DisablePassingWorkflowId ).AsBoolean();
                if ( allowPassingWorkflowId )
                {
                    var workflowIdValue = PageParameter( PageParameterKey.WorkflowId );
                    WorkflowId = workflowIdValue.AsIntegerOrNull();
                }

                if ( !WorkflowId.HasValue )
                {
                    Guid guid = PageParameter( PageParameterKey.WorkflowGuid ).AsGuid();
                    if ( !guid.IsEmpty() )
                    {
                        _workflow = _workflowService.Queryable().Where( w => w.Guid.Equals( guid ) && w.WorkflowTypeId == workflowType.Id ).FirstOrDefault();
                        if ( _workflow != null )
                        {
                            WorkflowId = _workflow.Id;
                        }
                    }
                }
            }

            if ( WorkflowId.HasValue )
            {
                if ( _workflow == null )
                {
                    _workflow = _workflowService.Queryable()
                        .Where( w => w.Id == WorkflowId.Value && w.WorkflowTypeId == workflowType.Id )
                        .FirstOrDefault();
                }

                if ( _workflow != null )
                {
                    hlblWorkflowId.Text = _workflow.WorkflowId;

                    _workflow.LoadAttributes();
                    foreach ( var activity in _workflow.Activities )
                    {
                        activity.LoadAttributes();
                    }
                }
            }

            // If an existing workflow was not specified, activate a new instance of workflow and start processing
            if ( _workflow == null )
            {
                string workflowName = PageParameter( PageParameterKey.WorkflowName );
                if ( string.IsNullOrWhiteSpace( workflowName ) )
                {
                    workflowName = "New " + workflowType.WorkTerm;
                }

                _workflow = Rock.Model.Workflow.Activate( workflowType, workflowName );

                if ( _workflow == null )
                {
                    ShowNotes( false );
                    ShowMessage( NotificationBoxType.Danger, "Workflow Activation Error", "Workflow could not be activated." );
                    return false;
                }

                foreach ( var param in RockPage.PageParameters() )
                {
                    if ( param.Value != null && param.Value.ToString().IsNotNullOrWhiteSpace() )
                    {
                        _workflow.SetAttributeValue( param.Key, param.Value.ToString() );
                    }
                }

                var isWorkflowProcessSuccess = ProcessWorkflow();
                if ( isWorkflowProcessSuccess == false )
                {
                    return false;
                }

                WorkflowId = _workflow.Id != 0 ? _workflow.Id : WorkflowId;
            }
            else
            {
                // A workflow already exists, run WorkflowService.Process to ensure that any actions that were not completed due to delays, incomplete previous actions, or errors are run.
                // This is to ensure that any forms available in the workflow are presented and not delayed until the Process Workflows job has completed.
                var isWorkflowProcessSuccess = ProcessWorkflow();
                if ( isWorkflowProcessSuccess == false )
                {
                    return false;
                }
            }

            var canEdit = UserCanEdit || _workflow.IsAuthorized( Authorization.EDIT, CurrentPerson );

            if ( _workflow.IsActive )
            {
                if ( ActionTypeId.HasValue )
                {
                    foreach ( var activity in _workflow.ActiveActivities )
                    {
                        _action = activity.ActiveActions.Where( a => a.ActionTypeId == ActionTypeId.Value ).FirstOrDefault();
                        if ( _action != null )
                        {
                            _activity = activity;
                            _activity.LoadAttributes();

                            _actionType = _action.ActionTypeCache;
                            ActionTypeId = _actionType.Id;
                            return true;
                        }
                    }
                }

                // Find first active action form
                int personId = CurrentPerson != null ? CurrentPerson.Id : 0;

                // get active workflow activities
                // this is an Enumerable since _workflow.Activities is a collection that is lazy loaded
                IEnumerable<WorkflowActivity> activeWorkflowActivitiesList = _workflow.Activities.Where( a => a.IsActive );

                int? actionId = PageParameter( PageParameterKey.ActionId ).AsIntegerOrNull();
                if ( actionId.HasValue )
                {
                    // if a specific ActionId was specified, narrow it down to ones with the specified actionId
                    activeWorkflowActivitiesList = activeWorkflowActivitiesList.Where( a => a.Actions.Any( ac => ac.Id == actionId.Value ) );
                }

                if ( !canEdit )
                {
                    /* if user isn't authorized to edit, limit to ones that are any of the following conditions
                    // - Not assigned
                    // - Assigned to current person
                    // - Assigned to a group that the current user is a member of
                    */

                    activeWorkflowActivitiesList = activeWorkflowActivitiesList.Where( a =>
                    {
                        if ( !a.AssignedGroupId.HasValue && !a.AssignedPersonAliasId.HasValue )
                        {
                            // not assigned
                            return true;
                        }

                        if ( a.AssignedPersonAlias != null && a.AssignedPersonAlias.PersonId == personId )
                        {
                            // assigned to current person
                            return true;
                        }

                        if ( a.AssignedGroup != null && a.AssignedGroup.Members.Any( m => m.PersonId == personId ) )
                        {
                            // Assigned to a group that the current user is a member of
                            return true;
                        }

                        return false;
                    } );
                }

                activeWorkflowActivitiesList = activeWorkflowActivitiesList.OrderBy( a => a.ActivityTypeCache.Order ).ToList();

                foreach ( var activity in activeWorkflowActivitiesList )
                {
                    if ( canEdit || activity.ActivityTypeCache.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        var actions = activity.ActiveActions.Where( a => !actionId.HasValue || a.Id == actionId.Value ).ToList();

                        // Check each active action in the activity for valid criteria and get the first one. This is to prevent a conditional action that didn't meet criteria from preventing a form from showing.
                        WorkflowAction action = actions.Where( a => a.IsCriteriaValid ).FirstOrDefault();

                        if ( action == null )
                        {
                            continue;
                        }

                        if ( action.ActionTypeCache.WorkflowForm != null )
                        {
                            _activity = activity;
                            if ( _activity.Id != 0 || _activity.AttributeValues == null )
                            {
                                _activity.LoadAttributes();
                            }

                            _action = action;
                            _actionType = _action.ActionTypeCache;
                            ActionTypeId = _actionType.Id;
                            return true;
                        }

                        if ( action.ActionTypeCache.WorkflowAction is Rock.Workflow.Action.ElectronicSignature )
                        {
                            _activity = activity;
                            if ( _activity.Id != 0 || _activity.AttributeValues == null )
                            {
                                _activity.LoadAttributes();
                            }

                            _action = action;
                            _actionType = _action.ActionTypeCache;
                            ActionTypeId = _actionType.Id;
                            return true;
                        }
                    }
                }

                lSummary.Text = string.Empty;
            }
            else
            {
                if ( GetAttributeValue( AttributeKey.ShowSummaryView ).AsBoolean() && !string.IsNullOrWhiteSpace( workflowType.SummaryViewText ) )
                {
                    Dictionary<string, object> mergeFields = GetWorkflowEntryMergeFields();

                    lSummary.Text = workflowType.SummaryViewText.ResolveMergeFields( mergeFields, CurrentPerson );
                    lSummary.Visible = true;
                }
            }

            if ( lSummary.Text.IsNullOrWhiteSpace() )
            {
                if ( workflowType.NoActionMessage.IsNullOrWhiteSpace() )
                {
                    ShowMessage( NotificationBoxType.Warning, string.Empty, "The selected workflow is not in a state that requires you to enter information." );
                }
                else
                {
                    Dictionary<string, object> mergeFields = GetWorkflowEntryMergeFields();
                    ShowMessage( NotificationBoxType.Warning, string.Empty, workflowType.NoActionMessage.ResolveMergeFields( mergeFields, CurrentPerson ) );
                }
            }

            // If we are returning False (Workflow is not active), make sure the form and notes are not shown
            ShowNotes( false );
            pnlWorkflowUserForm.Visible = false;
            pnlWorkflowActionElectronicSignature.Visible = false;
            return false;
        }

        /// <summary>
        /// Gets the workflow entry merge fields.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetWorkflowEntryMergeFields()
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "Action", _action );
            mergeFields.Add( "Activity", _activity );
            mergeFields.Add( "Workflow", _workflow );
            return mergeFields;
        }

        /// <summary>
        /// Gets the workflow entity using the values stored in the workflow for an existing workflow, or the page parameters for a new workflow.
        /// </summary>
        /// <returns></returns>
        private IEntity GetWorkflowEntity()
        {
            IEntity iEntity = null;
            if ( _workflow.Id == 0 )
            {
                // This is a new workflow, so get the EntityType and Id from the PersonId or GroupId page parameters if they exist.
                // If this is an existing workflow the EntityType and Id are already inserted into the Workflow instance.
                int? personId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    iEntity = new PersonService( _workflowRockContext ).Get( personId.Value );
                }
                else
                {
                    int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                    if ( groupId.HasValue )
                    {
                        iEntity = new GroupService( _workflowRockContext ).Get( groupId.Value );
                    }
                }
            }

            return iEntity;
        }

        /// <summary>
        /// Processes the workflow.
        /// </summary>
        /// <returns></returns>
        private bool ProcessWorkflow()
        {
            var entity = GetWorkflowEntity();
            if ( !_workflowService.Process( _workflow, entity, out List<string> errorMessages ) )
            {
                ShowNotes( false );
                ShowMessage(
                    NotificationBoxType.Danger,
                    "Workflow Processing Error(s):",
                    "<ul><li>" + errorMessages.AsDelimited( "</li><li>" ) + "</li></ul>" );

                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads the WorkflowType
        /// </summary>
        private WorkflowTypeCache GetWorkflowType()
        {
            // Get the block setting to disable passing WorkflowTypeID.
            bool allowPassingWorkflowTypeId = !this.GetAttributeValue( AttributeKey.DisablePassingWorkflowTypeId ).AsBoolean();
            WorkflowTypeCache _workflowType = null;

            // If the ViewState value for WorkflowTypeGuid is empty, try to set it.
            if ( WorkflowTypeGuid.AsGuid().IsEmpty() )
            {
                // Get workflow type set by attribute value of this block.
                Guid workflowTypeGuidFromAttribute = GetAttributeValue( AttributeKey.WorkflowType ).AsGuid();

                if ( !workflowTypeGuidFromAttribute.IsEmpty() )
                {
                    _workflowType = WorkflowTypeCache.Get( workflowTypeGuidFromAttribute );
                    WorkflowTypeDeterminedByBlockAttribute = true;
                }

                if ( _workflowType == null )
                {
                    // If an attribute value was not provided, check for query parameter or route value.
                    WorkflowTypeDeterminedByBlockAttribute = false;
                    if ( allowPassingWorkflowTypeId )
                    {
                        // Try to find a WorkflowTypeID from either the query or route, via the PageParameter.
                        int? workflowTypeId = PageParameter( PageParameterKey.WorkflowTypeId ).AsIntegerOrNull();
                        if ( workflowTypeId.HasValue )
                        {
                            _workflowType = WorkflowTypeCache.Get( workflowTypeId.Value );
                        }
                    }

                    if ( _workflowType == null )
                    {
                        // If the workflowType is still not set, try to find a WorkflowTypeGuid from either the query or route, via the PageParameter.
                        var workflowTypeGuidFromURL = PageParameter( PageParameterKey.WorkflowTypeGuid ).AsGuid();
                        WorkflowTypeGuid = PageParameter( PageParameterKey.WorkflowTypeGuid );
                        if ( !workflowTypeGuidFromURL.IsEmpty() )
                        {
                            _workflowType = WorkflowTypeCache.Get( workflowTypeGuidFromURL );
                        }
                    }
                }
            }

            // If the ViewState WorkflowTypeGuid is still empty
            if ( WorkflowTypeGuid == null )
            {
                // If the workflowType is not set, set the ViewState WorkflowTypeGuid to empty, otherwise set it to the Guid of the workflowType.
                WorkflowTypeGuid = _workflowType == null ? string.Empty : _workflowType.Guid.ToString();
            }
            else
            {
                // Get the WorkflowType via the ViewState WorkflowTypeGuid.
                _workflowType = WorkflowTypeCache.Get( WorkflowTypeGuid );
            }

            return _workflowType;
        }

        /// <summary>
        /// Processes the action request.
        /// </summary>
        private void ProcessActionRequest()
        {
            string action = PageParameter( PageParameterKey.Command );
            if ( !string.IsNullOrWhiteSpace( action ) )
            {
                CompleteFormAction( action );
            }
        }

        /// <summary>
        /// Builds the workflow action UI.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildWorkflowActionUI( bool setValues )
        {
            var form = _actionType.WorkflowForm;

            if ( form != null )
            {
                BuildWorkflowActionForm( form, setValues );
                return;
            }

            if ( _actionType.WorkflowAction is Rock.Workflow.Action.ElectronicSignature )
            {
                var electronicSignatureWorkflowAction = _actionType.WorkflowAction as Rock.Workflow.Action.ElectronicSignature;
                BuildWorkflowActionDigitalSignature( electronicSignatureWorkflowAction, _action, setValues );
            }
        }

        /// <summary>
        /// Builds any UI needed by the workflow action.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildWorkflowActionForm( WorkflowActionFormCache form, bool setValues )
        {
            divWorkflowActionUserFormNotes.Visible = true;
            pnlWorkflowActionElectronicSignature.Visible = false;

            Dictionary<string, object> mergeFields = GetWorkflowEntryMergeFields();

            var workflowType = GetWorkflowType();
            string headerTemplate = string.Empty;
            string footerTemplate = string.Empty;

            if ( workflowType?.FormBuilderTemplateId != null )
            {
                /* If a Template Is Defined, use both the Forms.Header/Footer and Template.HeaderFooter in case they defined both
                So it looks like this

                Header from Template
                Header from FormDetail
                <inner form content>
                Footer from FormDetail
                Footer from Template

                */
                var formBuilderTemplate = new WorkflowFormBuilderTemplateService( new RockContext() ).Get( workflowType.FormBuilderTemplateId.Value );
                headerTemplate = formBuilderTemplate.FormHeader + form.Header;
                footerTemplate = form.Footer + formBuilderTemplate.FormFooter;
            }
            else
            {
                headerTemplate = form.Header;
                footerTemplate = form.Footer;
            }

            if ( setValues )
            {
                lFormHeaderText.Text = headerTemplate.ResolveMergeFields( mergeFields );
                lFormFooterText.Text = footerTemplate.ResolveMergeFields( mergeFields );
            }

            if ( _workflow != null && _workflow.CreatedDateTime.HasValue )
            {
                hlblDateAdded.Text = string.Format( "Added: {0}", _workflow.CreatedDateTime.Value.ToShortDateString() );
            }
            else
            {
                hlblDateAdded.Visible = false;
            }

            BuildPersonEntryForm( _action, form, setValues, mergeFields );

            phWorkflowFormAttributes.Controls.Clear();

            // Use PlaceHolder for non-folderbuilder sections.
            // Real sections will render a div, but PlaceHolder will not.
            var formSectionNone = new PlaceHolder()
            {
                ID = $"pnlFormSection_none",
                Visible = form.FormAttributes.Any( x => !x.ActionFormSectionId.HasValue )
            };

            phWorkflowFormAttributes.Controls.Add( formSectionNone );

            var formSections = form.FormAttributes.Select( a => a.ActionFormSectionId ).Where( a => a.HasValue ).Distinct().ToList()
                .Select( a => WorkflowActionFormSectionCache.Get( a.Value ) )
                .OrderBy( a => a.Order ).ThenBy( a => a.Title )
                .ToList();

            Dictionary<int, Control> formSectionControlLookup = new Dictionary<int, Control>();
            Dictionary<int, Control> formSectionRowLookup = new Dictionary<int, Control>();

            foreach ( var formSection in formSections )
            {
                var formSectionControl = new Panel
                {
                    ID = $"formSectionControl_{formSection.Id}",
                    CssClass = "form-section"
                };

                formSectionControlLookup.Add( formSection.Id, formSectionControl );

                if ( formSection.SectionTypeValueId.HasValue )
                {
                    var sectionTypeValue = DefinedValueCache.Get( formSection.SectionTypeValueId.Value );
                    var sectionTypeCssClass = sectionTypeValue?.GetAttributeValue( "CSSClass" );
                    if ( sectionTypeCssClass.IsNotNullOrWhiteSpace() )
                    {
                        formSectionControl.AddCssClass( sectionTypeCssClass );
                    }
                }

                if ( formSection.Title.IsNotNullOrWhiteSpace() )
                {
                    var formSectionHeader = new HtmlGenericControl( "h3" );
                    formSectionHeader.InnerText = formSection.Title;
                    formSectionControl.Controls.Add( formSectionHeader );
                }

                if ( formSection.Description.IsNotNullOrWhiteSpace() )
                {
                    var formSectionDescription = new HtmlGenericControl( "p" );
                    formSectionDescription.InnerText = formSection.Description;
                    formSectionControl.Controls.Add( formSectionDescription );
                }

                if ( formSection.ShowHeadingSeparator )
                {
                    var formSectionSeparator = new Literal { Text = "<hr>" };
                    formSectionControl.Controls.Add( formSectionSeparator );
                }

                var formSectionFields = new Panel
                {
                    ID = $"formSectionFields_{formSection.Id}",
                    CssClass = "form-section-fields"
                };

                HtmlGenericControl formSectionRow = new HtmlGenericControl( "div" );
                formSectionRow.AddCssClass( "row" );
                formSectionFields.Controls.Add( formSectionRow );
                formSectionControl.Controls.Add( formSectionFields );

                phWorkflowFormAttributes.Controls.Add( formSectionControl );

                formSectionRowLookup.Add( formSection.Id, formSectionRow );
            }

            foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
            {
                if ( !formAttribute.IsVisible )
                {
                    continue;
                }

                var attribute = AttributeCache.Get( formAttribute.AttributeId );
                var value = attribute.DefaultValue;

                // Get the identifiers for both Workflow and WorkflowActivity EntityTypes.
                var workflowEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.WORKFLOW.AsGuid() );
                var workflowActivityEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.WORKFLOW_ACTIVITY.AsGuid() );

                if ( _workflow != null && _workflow.AttributeValues.ContainsKey( attribute.Key ) && _workflow.AttributeValues[attribute.Key] != null && attribute.EntityTypeId == workflowEntityTypeId )
                {
                    // If the key is in the Workflow's Attributes, get the value from that.
                    value = _workflow.AttributeValues[attribute.Key].Value;
                }
                else if ( _activity != null && _activity.AttributeValues.ContainsKey( attribute.Key ) && _activity.AttributeValues[attribute.Key] != null && attribute.EntityTypeId == workflowActivityEntityTypeId )
                {
                    // If the key is in the Workflow Activity's Attributes, get the value from that.
                    value = _activity.AttributeValues[attribute.Key].Value;
                }

                var fieldVisibilityWrapper = new FieldVisibilityWrapper
                {
                    ID = "_fieldVisibilityWrapper_attribute_" + formAttribute.Id.ToString(),
                    FormFieldId = formAttribute.AttributeId,
                    FormType = FieldVisibilityWrapper.FormTypes.Workflow,
                    FieldVisibilityRules = formAttribute.FieldVisibilityRules
                };

                fieldVisibilityWrapper.EditValueUpdated += ( object sender, FieldVisibilityWrapper.FieldEventArgs args ) =>
                {
                    FieldVisibilityWrapper.ApplyFieldVisibilityRules( phWorkflowFormAttributes );
                    ApplySectionVisibilityRules( formSections, formSectionControlLookup );
                };

                Control formSectionRow;
                if ( formAttribute.ActionFormSectionId.HasValue )
                {
                    formSectionRow = formSectionRowLookup.GetValueOrNull( formAttribute.ActionFormSectionId.Value ) ?? formSectionNone;
                }
                else
                {
                    formSectionRow = formSectionNone;
                }

                Control fieldColumnContainer;
                if ( formSectionRow == formSectionNone )
                {
                    // use PlaceHolder for non-formbuilder sections
                    // Placeholder is only a container for other controls, it doesn't render any markup
                    fieldColumnContainer = new PlaceHolder();
                }
                else
                {
                    fieldColumnContainer = new HtmlGenericControl( "div" );
                    ( fieldColumnContainer as HtmlGenericControl ).AddCssClass( $"col-md-{formAttribute.ColumnSize ?? 12}" );
                }

                formSectionRow.Controls.Add( fieldColumnContainer );

                fieldColumnContainer.Controls.Add( fieldVisibilityWrapper );

                if ( !string.IsNullOrWhiteSpace( formAttribute.PreHtml ) )
                {
                    fieldVisibilityWrapper.Controls.Add( new LiteralControl( formAttribute.PreHtml.ResolveMergeFields( mergeFields ) ) );
                }

                if ( formAttribute.IsReadOnly )
                {
                    var field = attribute.FieldType.Field;

                    string formattedValue = null;

                    // get formatted value
                    if ( attribute.FieldType.Class == typeof( Rock.Field.Types.ImageFieldType ).FullName )
                    {
                        formattedValue = field.FormatValueAsHtml( fieldVisibilityWrapper, attribute.EntityTypeId, _activity.Id, value, attribute.QualifierValues, true );
                    }
                    else
                    {
                        formattedValue = field.FormatValueAsHtml( fieldVisibilityWrapper, attribute.EntityTypeId, _activity.Id, value, attribute.QualifierValues );
                    }

                    if ( formAttribute.HideLabel )
                    {
                        fieldVisibilityWrapper.Controls.Add( new LiteralControl( formattedValue ) );
                    }
                    else
                    {
                        RockLiteral lAttribute = new RockLiteral();
                        lAttribute.ID = "lAttribute_" + formAttribute.Id.ToString();
                        lAttribute.Label = attribute.Name;

                        if ( field is Rock.Field.ILinkableFieldType )
                        {
                            string url = ( ( Rock.Field.ILinkableFieldType ) field ).UrlLink( value, attribute.QualifierValues );
                            url = ResolveRockUrl( "~" ).EnsureTrailingForwardslash() + url;
                            lAttribute.Text = string.Format( "<a href='{0}' target='_blank' rel='noopener noreferrer'>{1}</a>", url, formattedValue );
                        }
                        else
                        {
                            lAttribute.Text = formattedValue;
                        }

                        fieldVisibilityWrapper.Controls.Add( lAttribute );
                    }
                }
                else
                {
                    AttributeControlOptions attributeControlOptions = new AttributeControlOptions
                    {
                        Value = value,
                        ValidationGroup = BlockValidationGroup,
                        SetValue = setValues,
                        SetId = true,
                        Required = formAttribute.IsRequired,
                        LabelText = formAttribute.HideLabel ? string.Empty : attribute.Name
                    };

                    var editControl = attribute.AddControl( fieldVisibilityWrapper.Controls, attributeControlOptions );
                    fieldVisibilityWrapper.EditControl = editControl;

                    var hasDependantVisibilityRule = form.FormAttributes.Any( a => a.FieldVisibilityRules.RuleList.Any( r => r.ComparedToFormFieldGuid == attribute.Guid ) );

                    if ( !hasDependantVisibilityRule )
                    {
                        // also check if this field is involved in any of the section visibility rules;
                        hasDependantVisibilityRule = formSections.Any( a => a.SectionVisibilityRules != null && a.SectionVisibilityRules.RuleList.Any( r => r.ComparedToFormFieldGuid == attribute.Guid ) );
                    }

                    if ( hasDependantVisibilityRule && attribute.FieldType.Field.HasChangeHandler( editControl ) )
                    {
                        attribute.FieldType.Field.AddChangeHandler(
                            editControl,
                            () =>
                            {
                                fieldVisibilityWrapper.TriggerEditValueUpdated( editControl, new FieldVisibilityWrapper.FieldEventArgs( attribute, editControl ) );
                            } );
                    }
                }

                if ( !string.IsNullOrWhiteSpace( formAttribute.PostHtml ) )
                {
                    fieldVisibilityWrapper.Controls.Add( new LiteralControl( formAttribute.PostHtml.ResolveMergeFields( mergeFields ) ) );
                }
            }

            if ( setValues )
            {
                // Apply the field visibility rules only if the Edit Values on the FieldVisibilityWrappers are set.
                // This solves the issue with the Attributes having a partial postback. Reason: GitHub Issue #5602.
                FieldVisibilityWrapper.ApplyFieldVisibilityRules( phWorkflowFormAttributes );
                ApplySectionVisibilityRules( formSections, formSectionControlLookup );
            }

            if ( form.AllowNotes.HasValue && form.AllowNotes.Value && _workflow != null && _workflow.Id != 0 )
            {
                ncWorkflowNotes.NoteOptions.EntityId = _workflow.Id;
                ShowNotes( true );
            }
            else
            {
                ShowNotes( false );
            }

            var disableCaptcha = GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() || !cpCaptcha.IsAvailable;

            if ( disableCaptcha || IsCaptchaValid )
            {
                AddSubmitButtons( form );
            }
        }

        /// <summary>
        /// Applies the section visibility rules.
        /// </summary>
        /// <param name="formSections">The form sections.</param>
        /// <param name="formSectionControlLookup">The form section control lookup.</param>
        private void ApplySectionVisibilityRules( List<WorkflowActionFormSectionCache> formSections, Dictionary<int, Control> formSectionControlLookup )
        {
            foreach ( var formSection in formSections )
            {
                var sectionVisibilityRules = formSection.SectionVisibilityRules;
                if ( sectionVisibilityRules != null )
                {
                    var formSectionControl = formSectionControlLookup.GetValueOrNull( formSection.Id );
                    if ( formSectionControl != null )
                    {
                        // the conditions for a section's visibility should not include its own controls
                        var otherSectionsFormEditValues = GetWorkflowFormEditAttributeValues( formSection.Id );
                        var showVisible = sectionVisibilityRules.Evaluate( otherSectionsFormEditValues, new Dictionary<RegistrationPersonFieldType, string>() );
                        formSectionControl.Visible = showVisible;
                    }
                }
            }
        }

        /// <summary>
        /// Builds the person entry form.
        /// </summary>
        /// <param name="action">The current action related to the form.</param>
        /// <param name="form">The form.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="mergeFields">The merge fields.</param>
        private void BuildPersonEntryForm( WorkflowAction action, WorkflowActionFormCache actionForm, bool setValues, Dictionary<string, object> mergeFields )
        {
            var workflowType = GetWorkflowType();
            string preHtml = actionForm.PersonEntryPreHtml;
            string postHtml = actionForm.PersonEntryPostHtml;
            var formPersonEntrySettings = actionForm.GetFormPersonEntrySettings( workflowType.FormBuilderTemplate );
            var allowPersonEntry = actionForm.GetAllowPersonEntry( workflowType.FormBuilderTemplate );

            pnlPersonEntrySection.Visible = allowPersonEntry;
            if ( !allowPersonEntry )
            {
                return;
            }

            if ( formPersonEntrySettings.HideIfCurrentPersonKnown && CurrentPerson != null )
            {
                pnlPersonEntrySection.Visible = false;
                return;
            }

            lPersonEntryPreHtml.Text = preHtml.ResolveMergeFields( mergeFields );
            var personEntrySectionHeaderHtml = new StringBuilder();

            if ( actionForm.PersonEntrySectionTypeValueId.HasValue )
            {
                var sectionTypeValue = DefinedValueCache.Get( actionForm.PersonEntrySectionTypeValueId.Value );
                var sectionTypeCssClass = sectionTypeValue?.GetAttributeValue( "CSSClass" );
                if ( sectionTypeCssClass.IsNotNullOrWhiteSpace() )
                {
                    pnlPersonEntrySection.AddCssClass( sectionTypeCssClass );
                }
            }

            if ( actionForm.PersonEntryTitle.IsNotNullOrWhiteSpace() )
            {
                personEntrySectionHeaderHtml.AppendLine( $"<h1>{actionForm.PersonEntryTitle}</h1>" );
            }

            if ( actionForm.PersonEntryDescription.IsNotNullOrWhiteSpace() )
            {
                personEntrySectionHeaderHtml.AppendLine( $"<p>{actionForm.PersonEntryDescription}</p>" );
            }

            if ( actionForm.PersonEntryShowHeadingSeparator )
            {
                personEntrySectionHeaderHtml.AppendLine( "<hr>" );
            }

            lPersonEntrySectionHeaderHtml.Text = personEntrySectionHeaderHtml.ToString();

            if ( formPersonEntrySettings.ShowCampus )
            {
                // NOTE: If there is only one Campus in the system, this control be always be hidden
                cpPersonEntryCampus.Visible = true;

                /* 7/15/2020 - MDP
                 The list of campus should include Inactive, but limited to
                 the configured CampusStatus and/or CampusType.
                 See https://app.asana.com/0/1121505495628584/1200153314028124/f
                 */

                cpPersonEntryCampus.IncludeInactive = true;
                if ( formPersonEntrySettings.CampusStatusValueId.HasValue )
                {
                    cpPersonEntryCampus.CampusStatusFilter = new List<int> { formPersonEntrySettings.CampusStatusValueId.Value };
                }

                if ( formPersonEntrySettings.CampusTypeValueId.HasValue )
                {
                    cpPersonEntryCampus.CampusTypesFilter = new List<int> { formPersonEntrySettings.CampusTypeValueId.Value };
                }
            }
            else
            {
                cpPersonEntryCampus.Visible = false;
            }

            SetPersonEditorOptions( pePerson1, formPersonEntrySettings );
            SetPersonEditorOptions( pePerson2, formPersonEntrySettings );
            pePerson2.PersonLabelPrefix = formPersonEntrySettings.SpouseLabel;
            cbShowPerson2.TextCssClass = "font-weight-semibold";
            cbShowPerson2.ContainerCssClass = "show-spouse mt-3 mb-4";
            cbShowPerson2.Text = string.Format( "Show {0}", formPersonEntrySettings.SpouseLabel );
            switch ( formPersonEntrySettings.SpouseEntry )
            {
                case WorkflowActionFormPersonEntryOption.Required:
                    {
                        // if Spouse is required, don't show the option to show/hide spouse, an have the Spouse entry be visible
                        cbShowPerson2.Checked = true;
                        cbShowPerson2.Visible = false;
                        break;
                    }

                case WorkflowActionFormPersonEntryOption.Optional:
                    {
                        // if Spouse is enabled, show the option to show/hide spouse
                        cbShowPerson2.Visible = true;
                        break;
                    }

                case WorkflowActionFormPersonEntryOption.Hidden:
                default:
                    {
                        cbShowPerson2.Visible = false;
                        break;
                    }
            }

            if ( setValues )
            {
                pePerson2.Visible = cbShowPerson2.Checked;
            }

            dvpMaritalStatus.DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() );
            dvpMaritalStatus.Required = formPersonEntrySettings.MaritalStatus == WorkflowActionFormPersonEntryOption.Required;

            if ( formPersonEntrySettings.MaritalStatus == WorkflowActionFormPersonEntryOption.Hidden )
            {
                dvpMaritalStatus.Visible = false;
            }

            acPersonEntryAddress.Required = formPersonEntrySettings.Address == WorkflowActionFormPersonEntryOption.Required;

            lPersonEntryPostHtml.Text = postHtml.ResolveMergeFields( mergeFields );

            var promptForAddress = ( formPersonEntrySettings.Address != WorkflowActionFormPersonEntryOption.Hidden ) && formPersonEntrySettings.AddressTypeValueId.HasValue;
            acPersonEntryAddress.Visible = promptForAddress;

            if ( setValues )
            {
                SetPersonEntryValues( action, formPersonEntrySettings );
            }
        }

        /// <summary>
        /// Sets the person entry values.
        /// </summary>
        /// <param name="action">The current action related to the form.</param>
        /// <param name="form">The form details.</param>
        private void SetPersonEntryValues( WorkflowAction action, Rock.Workflow.FormBuilder.FormPersonEntrySettings formPersonEntrySettings )
        {
            var rockContext = new RockContext();
            Person personEntryPerson = null;
            Person personEntrySpouse = null;
            int? personEntryFamilyId = null;

            action.GetPersonEntryPeople( rockContext, CurrentPersonId, out personEntryPerson, out personEntrySpouse );

            if ( personEntryPerson != null )
            {
                cpPersonEntryCampus.SetValue( personEntryPerson.PrimaryCampusId );
                dvpMaritalStatus.SetValue( personEntryPerson.MaritalStatusValueId );
                personEntryFamilyId = personEntryPerson.PrimaryFamilyId;
            }
            else
            {
                // default to Married if this is a new person
                var maritalStatusMarriedValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() );
                dvpMaritalStatus.SetValue( maritalStatusMarriedValueId );
            }

            pePerson1.SetFromPerson( personEntryPerson );

            if ( formPersonEntrySettings.SpouseEntry != WorkflowActionFormPersonEntryOption.Hidden )
            {
                pePerson2.SetFromPerson( personEntrySpouse );
            }

            var promptForAddress = ( formPersonEntrySettings.Address != WorkflowActionFormPersonEntryOption.Hidden ) && formPersonEntrySettings.AddressTypeValueId.HasValue;

            if ( promptForAddress && personEntryFamilyId != null )
            {
                var personEntryGroupLocationTypeValueId = formPersonEntrySettings.AddressTypeValueId.Value;

                var familyLocation = new GroupLocationService( rockContext ).Queryable()
                    .Where( a => a.GroupId == personEntryFamilyId.Value && a.GroupLocationTypeValueId == formPersonEntrySettings.AddressTypeValueId ).Select( a => a.Location ).FirstOrDefault();

                if ( familyLocation != null )
                {
                    acPersonEntryAddress.SetValues( familyLocation );
                }
                else
                {
                    acPersonEntryAddress.SetValues( null );
                }
            }
        }

        /// <summary>
        /// Sets the person editor options.
        /// </summary>
        /// <param name="personBasicEditor">The person basic editor.</param>
        /// <param name="form">The form.</param>
        private static void SetPersonEditorOptions( PersonBasicEditor personBasicEditor, Rock.Workflow.FormBuilder.FormPersonEntrySettings formPersonEntrySettings )
        {
            personBasicEditor.ShowInColumns = false;
            personBasicEditor.ShowTitle = false;
            personBasicEditor.ShowSuffix = false;

            // Connection Status is determined by form.PersonEntryConnectionStatusValueId
            personBasicEditor.ShowConnectionStatus = false;

            // Role will always be Adult
            personBasicEditor.ShowPersonRole = false;
            personBasicEditor.ShowGrade = false;

            personBasicEditor.RequireGender = formPersonEntrySettings.Gender == WorkflowActionFormPersonEntryOption.Required;
            personBasicEditor.ShowGender = formPersonEntrySettings.Gender != WorkflowActionFormPersonEntryOption.Hidden;

            personBasicEditor.RequireEmail = formPersonEntrySettings.Email == WorkflowActionFormPersonEntryOption.Required;
            personBasicEditor.ShowEmail = formPersonEntrySettings.Email != WorkflowActionFormPersonEntryOption.Hidden;

            personBasicEditor.RequireMobilePhone = formPersonEntrySettings.MobilePhone == WorkflowActionFormPersonEntryOption.Required;
            personBasicEditor.ShowMobilePhone = formPersonEntrySettings.MobilePhone != WorkflowActionFormPersonEntryOption.Hidden;

            personBasicEditor.ShowSmsOptIn = formPersonEntrySettings.SmsOptIn != WorkflowActionFormShowHideOption.Hide && personBasicEditor.ShowMobilePhone;

            personBasicEditor.RequireBirthdate = formPersonEntrySettings.Birthdate == WorkflowActionFormPersonEntryOption.Required;
            personBasicEditor.ShowBirthdate = formPersonEntrySettings.Birthdate != WorkflowActionFormPersonEntryOption.Hidden;

            // we have a another MaritalStatus picker that will apply to both Person and Person's Spouse
            personBasicEditor.ShowMaritalStatus = false;

            personBasicEditor.ShowRace = formPersonEntrySettings.RaceEntry != WorkflowActionFormPersonEntryOption.Hidden;
            personBasicEditor.RequireRace = formPersonEntrySettings.RaceEntry == WorkflowActionFormPersonEntryOption.Required;

            personBasicEditor.ShowEthnicity = formPersonEntrySettings.EthnicityEntry != WorkflowActionFormPersonEntryOption.Hidden;
            personBasicEditor.RequireEthnicity = formPersonEntrySettings.EthnicityEntry == WorkflowActionFormPersonEntryOption.Required;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the cbShowPerson2 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbShowPerson2_CheckedChanged( object sender, EventArgs e )
        {
            pePerson2.Visible = cbShowPerson2.Checked;
        }

        /// <summary>
        /// Shows the notes.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        private void ShowNotes( bool visible )
        {
            divWorkflowActionUserFormNotes.Visible = visible;

            if ( visible )
            {
                divForm.RemoveCssClass( "col-md-12" );
                divForm.AddCssClass( "col-md-6" );
            }
            else
            {
                divForm.AddCssClass( "col-md-12" );
                divForm.RemoveCssClass( "col-md-6" );
            }
        }

        /// <summary>
        /// Gets the workflow form person entry values.
        /// </summary>
        private void GetWorkflowFormPersonEntryValues( RockContext personEntryRockContext )
        {
            if ( _workflow == null || _actionType == null )
            {
                return;
            }

            var form = _actionType.WorkflowForm;

            if ( form == null )
            {
                return;
            }

            var workflowType = GetWorkflowType();

            if ( !form.GetAllowPersonEntry( workflowType.FormBuilderTemplate ) )
            {
                return;
            }

            var formPersonEntrySettings = form.GetFormPersonEntrySettings( workflowType.FormBuilderTemplate );

            int? existingPersonId;
            int? existingPersonSpouseId = null;

            if ( CurrentPersonId.HasValue && ( formPersonEntrySettings.AutofillCurrentPerson || formPersonEntrySettings.HideIfCurrentPersonKnown ) )
            {
                existingPersonId = CurrentPersonId.Value;
                var existingPersonSpouse = CurrentPerson.GetSpouse( personEntryRockContext );
                if ( existingPersonSpouse != null )
                {
                    existingPersonSpouseId = existingPersonSpouse.Id;
                }

                if ( formPersonEntrySettings.HideIfCurrentPersonKnown )
                {
                    if ( CurrentPerson.PrimaryFamily == null )
                    {
                        CurrentPerson.PrimaryFamily = GetFamily( CurrentPerson, personEntryRockContext );
                    }

                    SavePersonEntryToAttributeValues( existingPersonId.Value, existingPersonSpouseId, CurrentPerson.PrimaryFamily );
                    return;
                }
            }
            else
            {
                existingPersonId = pePerson1.PersonId;
            }

            existingPersonSpouseId = pePerson2.PersonId;

            var personEntryPerson = CreateOrUpdatePersonFromPersonEditor( existingPersonId, null, pePerson1, personEntryRockContext );
            if ( personEntryPerson.Id == 0 )
            {
                personEntryPerson.ConnectionStatusValueId = formPersonEntrySettings.ConnectionStatusValueId;
                personEntryPerson.RecordStatusValueId = formPersonEntrySettings.RecordStatusValueId;
                personEntryPerson.RaceValueId = formPersonEntrySettings.RaceValueId;
                personEntryPerson.EthnicityValueId = formPersonEntrySettings.EthnicityValueId;
                PersonService.SaveNewPerson( personEntryPerson, personEntryRockContext, cpPersonEntryCampus.SelectedCampusId );
            }

            // if we ended up matching an existing person, get their spouse as the selected spouse
            var matchedPersonsSpouse = personEntryPerson.GetSpouse();

            if ( matchedPersonsSpouse != null )
            {
                existingPersonSpouseId = matchedPersonsSpouse.Id;
            }

            if ( formPersonEntrySettings.MaritalStatus != WorkflowActionFormPersonEntryOption.Hidden )
            {
                personEntryPerson.MaritalStatusValueId = dvpMaritalStatus.SelectedDefinedValueId;
            }

            // save person 1 to database and re-fetch to get any newly created family, or other things that would happen on PreSave changes, etc
            personEntryRockContext.SaveChanges();

            var personAliasService = new PersonAliasService( personEntryRockContext );

            int personEntryPersonId = personEntryPerson.Id;
            int? personEntryPersonSpouseId = null;

            var personService = new PersonService( personEntryRockContext );
            var primaryFamily = personService.GetSelect( personEntryPersonId, s => s.PrimaryFamily ) ?? GetFamily( personEntryPerson, personEntryRockContext );

            if ( pePerson2.Visible )
            {
                var personEntryPersonSpouse = CreateOrUpdatePersonFromPersonEditor( existingPersonSpouseId, primaryFamily, pePerson2, personEntryRockContext );
                if ( personEntryPersonSpouse.Id == 0 )
                {
                    personEntryPersonSpouse.ConnectionStatusValueId = formPersonEntrySettings.ConnectionStatusValueId;
                    personEntryPersonSpouse.RecordStatusValueId = formPersonEntrySettings.RecordStatusValueId;
                    personEntryPersonSpouse.RaceValueId = formPersonEntrySettings.RaceValueId;
                    personEntryPersonSpouse.EthnicityValueId = formPersonEntrySettings.EthnicityValueId;

                    // if adding/editing the 2nd Person (should normally be the spouse), set both people to selected Marital Status

                    /* 2020-11-16 MDP
                     *  It is possible that the Spouse label could be something other than spouse. So, we won't prevent them
                     *  from changing the Marital status on the two people. However, this should be considered a mis-use of this feature.
                     *  Unexpected things could happen.
                     *
                     *  Example of what would happen if 'Daughter' was the label for 'Spouse':
                     *  Ted Decker is Person1, and Cindy Decker gets auto-filled as Person2. but since the label is 'Daughter', he changes
                     *  Cindy's information to Alex Decker's information, then sets Marital status to Single.
                     *
                     *  This would result in Ted Decker no longer having Cindy as his spouse (and vice-versa). This was discussed on 2020-11-13
                     *  and it was decided we shouldn't do anything to prevent this type of problem.
                     *  
                     *  2023-11-21 PA
                     *  After further discussion we would be defaulting the marital status, if not provided, to married

                     */
                    if ( dvpMaritalStatus.SelectedDefinedValueId.HasValue )
                    {
                        personEntryPersonSpouse.MaritalStatusValueId = dvpMaritalStatus.SelectedDefinedValueId;
                        personEntryPerson.MaritalStatusValueId = dvpMaritalStatus.SelectedDefinedValueId;
                    }
                    else
                    {
                        var maritalStatusMarriedValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() );
                        personEntryPersonSpouse.MaritalStatusValueId = maritalStatusMarriedValueId;
                        personEntryPerson.MaritalStatusValueId = maritalStatusMarriedValueId;
                    }

                    PersonService.AddPersonToFamily( personEntryPersonSpouse, true, primaryFamily.Id, pePerson2.PersonGroupRoleId, personEntryRockContext );
                }

                personEntryRockContext.SaveChanges();

                personEntryPersonSpouseId = personEntryPersonSpouse.Id;
            }

            SavePersonEntryToAttributeValues( personEntryPersonId, personEntryPersonSpouseId, primaryFamily );

            if ( cpPersonEntryCampus.Visible )
            {
                primaryFamily.CampusId = cpPersonEntryCampus.SelectedCampusId;
            }

            if ( acPersonEntryAddress.Visible && formPersonEntrySettings.AddressTypeValueId.HasValue && acPersonEntryAddress.HasValue )
            {
                // a Person should always have a PrimaryFamilyId, but check to make sure, just in case
                if ( primaryFamily != null )
                {
                    var groupLocationService = new GroupLocationService( personEntryRockContext );
                    var familyLocation = primaryFamily.GroupLocations.Where( a => a.GroupLocationTypeValueId == formPersonEntrySettings.AddressTypeValueId.Value ).FirstOrDefault();

                    var newOrExistingLocation = new LocationService( personEntryRockContext ).Get(
                            acPersonEntryAddress.Street1,
                            acPersonEntryAddress.Street2,
                            acPersonEntryAddress.City,
                            acPersonEntryAddress.State,
                            acPersonEntryAddress.PostalCode,
                            acPersonEntryAddress.Country );

                    if ( newOrExistingLocation != null )
                    {
                        if ( familyLocation == null )
                        {
                            familyLocation = new GroupLocation
                            {
                                GroupLocationTypeValueId = formPersonEntrySettings.AddressTypeValueId.Value,
                                GroupId = primaryFamily.Id,
                                IsMailingLocation = true,
                                IsMappedLocation = true
                            };

                            groupLocationService.Add( familyLocation );
                        }

                        if ( newOrExistingLocation.Id != familyLocation.LocationId )
                        {
                            familyLocation.LocationId = newOrExistingLocation.Id;
                        }
                    }
                }
            }

            personEntryRockContext.SaveChanges();
        }

        /// <summary>
        /// Assigns the person to a family group, If there is no existing family a new one is created.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="personEntryRockContext">The person entry rock context.</param>
        /// <returns></returns>
        private Group GetFamily( Person person, RockContext personEntryRockContext )
        {
            Group family = null;
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            var groupService = new GroupService( personEntryRockContext );
            var groupMemberService = new GroupMemberService( personEntryRockContext );

            if ( person.PrimaryFamilyId.HasValue )
            {
                family = groupService.Get( person.PrimaryFamilyId.Value );
            }

            if ( family == null )
            {
                family = groupMemberService.Queryable()
                    .Where( gm => gm.PersonId == person.Id && gm.GroupTypeId == familyGroupType.Id )
                    .Select( gm => gm.Group )
                    .FirstOrDefault();
            }

            if ( family == null )
            {
                // Create new context so we don't inadvertently save any changes being tracked by the personEntryRockContext
                using ( var newFamilyRockContext = new RockContext() )
                {
                    family = new Rock.Model.Group
                    {
                        Name = person.LastName,
                        GroupTypeId = familyGroupType.Id,
                        CampusId = person.PrimaryCampusId
                    };

                    newFamilyRockContext.Groups.Add( family );

                    var adultRoleId = familyGroupType.Roles.Find( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
                    var familyMember = new GroupMember
                    {
                        PersonId = person.Id,
                        GroupRoleId = familyGroupType.DefaultGroupRoleId ?? adultRoleId,
                        GroupMemberStatus = GroupMemberStatus.Active
                    };

                    family.Members.Add( familyMember );

                    newFamilyRockContext.SaveChanges();
                }

                // Reload family with the personEntryRockContext so any changes to the family is tracked by it.
                family = groupService.Get( family.Id );
            }

            return family;
        }

        /// <summary>
        /// Saves the person entry to attribute values.
        /// </summary>
        /// <param name="personEntryPersonId">The person entry person identifier.</param>
        /// <param name="personEntryPersonSpouseId">The person entry person spouse identifier.</param>
        /// <param name="primaryFamily">The primary family.</param>
        private void SavePersonEntryToAttributeValues( int personEntryPersonId, int? personEntryPersonSpouseId, Group primaryFamily )
        {
            var form = _actionType.WorkflowForm;
            var personAliasService = new PersonAliasService( new RockContext() );

            var workflowType = GetWorkflowType();

            var personEntryPersonAttribute = form.GetPersonEntryPersonAttribute( _workflow );
            var personEntryFamilyAttribute = form.GetPersonEntryFamilyAttribute( _workflow );
            var personEntrySpouseAttribute = form.GetPersonEntrySpouseAttribute( _workflow );

            if ( personEntryPersonAttribute != null )
            {
                var item = GetWorkflowAttributeEntity( personEntryPersonAttribute );
                if ( item != null )
                {
                    var primaryAliasGuid = personAliasService.GetPrimaryAliasGuid( personEntryPersonId );
                    item.SetAttributeValue( personEntryPersonAttribute.Key, primaryAliasGuid );
                }
            }

            if ( personEntryFamilyAttribute != null )
            {
                var item = GetWorkflowAttributeEntity( personEntryFamilyAttribute );
                if ( item != null )
                {
                    item.SetAttributeValue( personEntryFamilyAttribute.Key, primaryFamily.Guid );
                }
            }

            if ( personEntrySpouseAttribute != null && personEntryPersonSpouseId.HasValue )
            {
                var item = GetWorkflowAttributeEntity( personEntrySpouseAttribute );
                if ( item != null )
                {
                    var primaryAliasGuid = personAliasService.GetPrimaryAliasGuid( personEntryPersonSpouseId.Value );
                    item.SetAttributeValue( personEntrySpouseAttribute.Key, primaryAliasGuid );
                }
            }
        }

        /// <summary>
        /// Gets the workflow attribute entity.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        private IHasAttributes GetWorkflowAttributeEntity( AttributeCache attribute )
        {
            Rock.Attribute.IHasAttributes item = null;
            if ( attribute.EntityTypeId == _workflow.TypeId )
            {
                item = _workflow;
            }
            else if ( attribute.EntityTypeId == _activity.TypeId )
            {
                item = _activity;
            }

            return item;
        }

        /// <summary>
        /// Gets the workflow attribute entity attribute value.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        private string GetWorkflowAttributeEntityAttributeValue( AttributeCache attribute )
        {
            var workflowAttributeEntity = GetWorkflowAttributeEntity( attribute );
            if ( workflowAttributeEntity != null )
            {
                return workflowAttributeEntity.GetAttributeValue( attribute.Key );
            }

            return null;
        }

        /// <summary>
        /// Creates or Updates person from person editor.
        /// </summary>
        /// <param name="existingPersonId">The existing person identifier.</param>
        /// <param name="limitMatchToFamily">Limit matches to people in specified family</param>
        /// <param name="personEditor">The person editor.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static Person CreateOrUpdatePersonFromPersonEditor( int? existingPersonId, Group limitMatchToFamily, PersonBasicEditor personEditor, RockContext rockContext )
        {
            var personService = new PersonService( rockContext );
            if ( existingPersonId.HasValue )
            {
                var existingPerson = personService.Get( existingPersonId.Value );
                var firstNameMatchesExistingFirstOrNickName = personEditor.FirstName.Equals( existingPerson.FirstName, StringComparison.OrdinalIgnoreCase )
                    || personEditor.FirstName.Equals( existingPerson.NickName, StringComparison.OrdinalIgnoreCase );
                var lastNameMatchesExistingLastName = personEditor.LastName.Equals( existingPerson.LastName, StringComparison.OrdinalIgnoreCase );
                bool useExistingPerson;
                if ( firstNameMatchesExistingFirstOrNickName && lastNameMatchesExistingLastName )
                {
                    // if the existing person (the one that was used to auto-fill the fields) has the same FirstName and LastName as what is in the PersonEditor,
                    // then we can safely assume they mean to update the existing person
                    useExistingPerson = true;
                }
                else
                {
                    /*  10-07-2021 MDP

                    Special Logic if AutoFill CurrentPerson is enabled, but the Person Name fields were changed:

                    If the existing person (the one that used to auto-fill the fields) changed the FirstName or LastName PersonEditor,
                    then assume they mean they mean to create (or match) a new person. Note that if this happens, this matched or new person won't
                    be added to Ted Decker's family. PersonEntry isn't smart enough to figure that out and isn't intended to be a family editor. Here are a few examples
                    to clarify what this means:

                    Example 1: If Ted Decker is auto-filled because Ted Decker is logged in, but he changes the fields to Noah Decker, then we'll see if we have enough to make a match
                    to the existing Noah Decker. However, a match to the existing Noah Decker would need to match Noah's email and/or cell phone too, so it could easily create a new Noah Decker.

                    Example 2: If Ted Decker is auto-filled because Ted Decker is logged in, but he changes the fields to NewBaby Decker, we'll have to do the same thing as Example 1
                    even though Ted might be thinking he is adding his new baby to the family. So NewBaby Decker will probably be a new person in a new family.

                    Example 3: If Ted Decker is auto-filled because Ted Decker is logged in, but he changes the fields to Bob Smith (Ted's Neighbor), we also do the same thing as Example 1. However,
                    in this case, we are mostly doing what Ted expected to happen.

                    Summary. PersonEntry is not a family editor, it just collects data to match or create a person (and spouse if enabled).

                    Note: The logic for Spouse entry is slightly different. See notes below...

                    */

                    useExistingPerson = false;
                }

                if ( useExistingPerson )
                {
                    // Update Person from personEditor
                    personEditor.UpdatePerson( existingPerson, rockContext );
                    return existingPerson;
                }
            }

            // Match or Create Person from personEditor
            var personMatchQuery = new PersonService.PersonMatchQuery( personEditor.FirstName, personEditor.LastName, personEditor.Email, personEditor.MobilePhoneNumber )
            {
                Gender = personEditor.ShowGender ? personEditor.PersonGender : null,
                BirthDate = personEditor.ShowBirthdate ? personEditor.PersonBirthDate : null,
                SuffixValueId = personEditor.ShowSuffix ? personEditor.PersonSuffixValueId : null
            };

            bool updatePrimaryEmail = false;
            var matchedPerson = personService.FindPerson( personMatchQuery, updatePrimaryEmail );

            /*
            2020-11-06 MDP
            ** Special Logic when doing matches for Spouses**
            * See discussion on https://app.asana.com/0/0/1198971294248209/f for more details
            *
            If we are trying to find a matching person record for the Spouse, only consider matches that are in the same family as the primary person.
            If we find a matching person but they are in a different family, create a new person record instead.
            We don't want to risk causing two person records from different families to get married due to our matching logic.

            This avoids a problem such as these
            #1
            - Person1 fields match on Tom Miller (Existing Single guy)
            - Spouse fields match on Cindy Decker (married to Ted Decker)

            Instead of causing Tom Miller and the existing Cindy Decker to get married, create a new "duplicate" Cindy decker instead.

            #2
            - Person1 fields match on Tom Miller (Existing single guy)
            - Spouse fields match on Mary Smith (an unmarried female in another family)

            Even in case #2, create a duplicate Mary Smith instead.

            The exception is a situation like this
            #3
            - Person1 Fields match on Steve Rogers. Steve Rogers' family contains a Sally Rogers, but Sally isn't his spouse because
              one (or both) of them doesn't have a marital status of Married.
            - Spouse Fields match on Sally Rogers (in Steve Rogers' family)

            In case #3, use the matched Sally Rogers record, and change Steve and Sally's marital status to married

            Note that in the case of matching on an existing person that has a spouse, for example
            #4
            - Person1 Fields match Bill Hills.
            - Bill has a spouse named Jill Hills
            -

            In case #4, since Bill has a spouse, the data in the Spouse fields will be used to update Bill's spouse Jill Hills

             */

            if ( matchedPerson != null && limitMatchToFamily != null )
            {
                if ( matchedPerson.PrimaryFamilyId != limitMatchToFamily.Id )
                {
                    matchedPerson = null;
                }
            }

            if ( matchedPerson != null )
            {
                // If we are using a matched person let the PersonEditor which PersonId we are editing
                personEditor.SetPersonId( matchedPerson.Id );

                // if a match was found, update that person
                personEditor.UpdatePerson( matchedPerson, rockContext );
                return matchedPerson;
            }
            else
            {
                var newPerson = new Person();

                // If we are using a matched person let the PersonEditor know we are editing a new person (personId = 0)
                personEditor.SetPersonId( newPerson.Id );

                personEditor.UpdatePerson( newPerson, rockContext );
                return newPerson;
            }
        }

        /// <summary>
        /// Saves the form values to the Workflow attributes
        /// </summary>
        private void SetWorkflowFormAttributeValues()
        {
            if ( _workflow == null || _actionType == null )
            {
                return;
            }

            var form = _actionType.WorkflowForm;

            var formEditAttributesValues = GetWorkflowFormEditAttributeValues();

            if ( formEditAttributesValues == null )
            {
                return;
            }

            foreach ( var formEditAttributesValue in formEditAttributesValues.Values )
            {
                var attribute = AttributeCache.Get( formEditAttributesValue.AttributeId );
                var control = phWorkflowFormAttributes.FindControl( string.Format( "attribute_field_{0}", attribute.Id ) );

                if ( attribute != null && control != null )
                {
                    Rock.Attribute.IHasAttributes item = GetWorkflowAttributeEntity( attribute );

                    if ( item != null )
                    {
                        item.SetAttributeValue( attribute.Key, attribute.FieldType.Field.GetEditValue( attribute.GetControl( control ), attribute.QualifierValues ) );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the workflow form edit attribute values.
        /// </summary>
        /// <param name="excludeSectionId">The exclude section identifier.</param>
        /// <returns></returns>
        private Dictionary<int, AttributeValueCache> GetWorkflowFormEditAttributeValues( int? excludeSectionId = null )
        {
            var result = new Dictionary<int, AttributeValueCache>();

            if ( _workflow == null || _actionType == null )
            {
                return result;
            }

            var form = _actionType.WorkflowForm;

            var values = new Dictionary<int, string>();
            var editableFormAttributes = form.FormAttributes.Where( a => a.IsVisible && !a.IsReadOnly );

            if ( excludeSectionId.HasValue )
            {
                editableFormAttributes = editableFormAttributes.Where( a => a.ActionFormSectionId.HasValue && a.ActionFormSectionId.Value != excludeSectionId.Value );
            }

            editableFormAttributes = editableFormAttributes.OrderBy( a => a.Order );

            foreach ( WorkflowActionFormAttributeCache formAttribute in editableFormAttributes )
            {
                var attribute = AttributeCache.Get( formAttribute.AttributeId );
                var control = phWorkflowFormAttributes.FindControl( string.Format( "attribute_field_{0}", formAttribute.AttributeId ) );

                if ( attribute != null && control != null )
                {
                    var editValue = attribute.FieldType.Field.GetEditValue( attribute.GetControl( control ), attribute.QualifierValues );
                    result.Add( attribute.Id, new AttributeValueCache( attribute.Id, null, editValue ) );
                }
            }

            return result;
        }

        /// <summary>
        /// Completes the form action.
        /// </summary>
        /// <param name="formAction">The form action.</param>
        private void CompleteFormAction( string formAction )
        {
            if ( string.IsNullOrWhiteSpace( formAction )
                || _workflow == null
                || _actionType == null
                || _actionType.WorkflowForm == null
                || _activity == null
                || _action == null )
            {
                return;
            }

            var mergeFields = GetWorkflowEntryMergeFields();

            Guid? activityTypeGuid = Guid.Empty;
            string buttonResponseText = "Your information has been submitted successfully.";

            // If the selected action requires valid form data, trigger page validation and discontinue processing if there are any errors.
            var buttons = WorkflowActionFormUserAction.FromUriEncodedString( _actionType.WorkflowForm.Actions );

            var button = buttons.FirstOrDefault( x => x.ActionName == formAction );

            if ( button != null )
            {
                if ( button.CausesValidation )
                {
                    Page.Validate( this.BlockValidationGroup );

                    if ( !Page.IsValid )
                    {
                        return;
                    }
                }

                activityTypeGuid = button.ActivateActivityTypeGuid.AsGuidOrNull();

                if ( !string.IsNullOrWhiteSpace( button.ResponseText ) )
                {
                    buttonResponseText = button.ResponseText;
                }
            }

            if ( _actionType.WorkflowForm.ActionAttributeGuid.HasValue )
            {
                var attribute = AttributeCache.Get( _actionType.WorkflowForm.ActionAttributeGuid.Value );
                if ( attribute != null )
                {
                    Rock.Attribute.IHasAttributes item = GetWorkflowAttributeEntity( attribute );
                    if ( item != null )
                    {
                        item.SetAttributeValue( attribute.Key, formAction );
                    }
                }
            }

            _action.FormAction = formAction;
            _action.AddLogEntry( "Form Action Selected: " + _action.FormAction );

            var workflowType = GetWorkflowType();

            string responseTextTemplate;

            FormCompletionActionSettings completionActionSettings;
            if ( workflowType?.FormBuilderTemplate != null )
            {
                completionActionSettings = workflowType?.FormBuilderTemplate.CompletionActionSettings;
            }
            else if ( workflowType.FormBuilderSettings != null )
            {
                completionActionSettings = workflowType?.FormBuilderSettings.CompletionAction;
            }
            else
            {
                // not a formbuilder or formbuilder template, so use UserForm buttons
                completionActionSettings = null;
            }

            if ( completionActionSettings != null && completionActionSettings.Type == FormCompletionActionType.DisplayMessage )
            {
                // if this is a FormBuilder and a completion action of DisplayMessage, set responseText from that
                responseTextTemplate = completionActionSettings.Message;
            }
            else
            {
                responseTextTemplate = buttonResponseText;
            }

            var responseText = responseTextTemplate.ResolveMergeFields( mergeFields );
            var workflowCampusSetFrom = workflowType?.FormBuilderSettings?.CampusSetFrom;
            switch ( workflowCampusSetFrom )
            {
                case CampusSetFrom.CurrentPerson:
                    {
                        _workflow.CampusId = this.CurrentPerson?.PrimaryCampusId;
                    }

                    break;
                case CampusSetFrom.WorkflowPerson:
                    {
                        Person personEntryPerson;
                        Person personEntrySpouse;
                        _action.GetPersonEntryPeople( new RockContext(), CurrentPersonId, out personEntryPerson, out personEntrySpouse );
                        if ( personEntryPerson != null )
                        {
                            _workflow.CampusId = personEntryPerson.PrimaryCampusId;
                        }
                    }

                    break;
                default:
                    {
                        var campusIdFromUrl = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();
                        var campusGuidFromUrl = PageParameter( PageParameterKey.CampusGuid ).AsGuidOrNull();
                        if ( campusIdFromUrl.HasValue )
                        {
                            _workflow.CampusId = campusIdFromUrl;
                        }
                        else if ( campusGuidFromUrl.HasValue )
                        {
                            _workflow.CampusId = CampusCache.GetId( campusGuidFromUrl.Value );
                        }
                    }

                    break;
            }

            if ( workflowType.IsPersisted == false && workflowType.IsFormBuilder )
            {
                /* 3/14/2022 MP
                 If this is a FormBuilder workflow, the WorkflowType probably has _workflowType.IsPersisted == false.
                 This is because we don't want to persist the workflow until they have submitted.
                 So, in the case of FormBuilder, we'll persist when they submit regardless of the _workflowType.IsPersisted setting
                */
                _workflowService.PersistImmediately( _action );
            }

            CompleteCurrentWorkflowAction( activityTypeGuid, responseText );
        }

        /// <summary>
        /// Completes the current workflow action.
        /// </summary>
        /// <param name="activateActivityTypeGuid">The activate activity type unique identifier.</param>
        /// <param name="responseText">The response text.</param>
        private void CompleteCurrentWorkflowAction( Guid? activateActivityTypeGuid, string responseText )
        {
            _action.MarkComplete();

            if ( _action.ActionTypeCache.IsActivityCompletedOnSuccess )
            {
                _action.Activity.MarkComplete();
            }

            var _workflowType = GetWorkflowType();

            if ( activateActivityTypeGuid.HasValue )
            {
                var activityType = _workflowType.ActivityTypes.Where( a => a.Guid.Equals( activateActivityTypeGuid.Value ) ).FirstOrDefault();
                if ( activityType != null )
                {
                    WorkflowActivity.Activate( activityType, _workflow );
                }
            }

            // If the LastProcessedDateTime is equal to RockDateTime.Now we need to pause for a bit so the workflow will actually process here.
            // The resolution of System.DateTime.UTCNow is between .5 and 15 ms which can cause the workflow processing to not properly pick up
            // where it left off.
            // Without this you might see random failures of workflows to save automatically.
            // https://docs.microsoft.com/en-us/dotnet/api/system.datetime.utcnow?view=netframework-4.7#remarks
            while ( _workflow.LastProcessedDateTime == RockDateTime.Now )
            {
                System.Threading.Thread.Sleep( 1 );
            }

            var isWorkflowProcessSuccess = ProcessWorkflow();

            WorkflowId = _workflow.Id != 0 ? _workflow.Id : WorkflowId;

            if ( !isWorkflowProcessSuccess )
            {
                return;
            }

            Guid? previousActionGuid = null;

            // just in case this is the Form Completion, keep track of that actionTypeId is
            int? completionActionTypeId = _actionType?.Id;

            if ( _action != null )
            {
                // Compare GUIDs since the IDs are DB generated and will be 0 if the workflow is not persisted.
                previousActionGuid = _action.Guid;
            }

            ActionTypeId = null;
            _action = null;
            _actionType = null;
            _activity = null;
            bool hydrateObjectsResult = HydrateObjects();

            if ( hydrateObjectsResult && _action != null && _action.Guid != previousActionGuid )
            {
                // The block reloads the page with the workflow IDs as a parameter. At this point the workflow must be persisted regardless of user settings in order for the workflow to work.
                _workflowService.PersistImmediately( _action );

                // If we are already being directed (presumably from the Redirect Action), don't redirect again.
                if ( !Response.IsRequestBeingRedirected )
                {
                    var pageReference = new PageReference( CurrentPageReference );
                    bool allowPassingWorkflowId = !this.GetAttributeValue( AttributeKey.DisablePassingWorkflowId ).AsBoolean();
                    if ( allowPassingWorkflowId )
                    {
                        pageReference.Parameters.AddOrReplace( PageParameterKey.WorkflowId, _workflow.Id.ToString() );
                    }

                    pageReference.Parameters.AddOrReplace( PageParameterKey.WorkflowGuid, _workflow.Guid.ToString() );
                    if ( this.GetAttributeValue( AttributeKey.LogInteractionOnCompletion ).AsBoolean() || this.GetAttributeValue( AttributeKey.LogInteractionOnView ).AsBoolean() )
                    {
                        // we only need InteractionStartDateTime in the URL if we the Interaction Block settings are enabled.
                        pageReference.Parameters.AddOrReplace( PageParameterKey.InteractionStartDateTime, this.InteractionStartDateTime.ToISO8601DateString() );
                    }

                    foreach ( var key in pageReference.QueryString.AllKeys.Where( k => !k.Equals( PageParameterKey.Command, StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        pageReference.Parameters.AddOrIgnore( key, pageReference.QueryString[key] );
                    }

                    pageReference.QueryString = new System.Collections.Specialized.NameValueCollection();
                    Response.Redirect( pageReference.BuildUrl(), false );
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            else
            {
                // final form completed
                LogWorkflowEntryInteraction( _workflow, completionActionTypeId, WorkflowInteractionOperationType.FormCompleted );

                if ( lSummary.Text.IsNullOrWhiteSpace() )
                {
                    var hideForm = _action == null || _action.Guid != previousActionGuid;
                    ShowMessage( NotificationBoxType.Success, string.Empty, responseText, hideForm );
                }
                else
                {
                    pnlWorkflowUserForm.Visible = false;
                    pnlWorkflowActionElectronicSignature.Visible = false;
                }

                // Confirmation email can come FormBuilderSettings or FormBuilderTemplate
                FormConfirmationEmailSettings confirmationEmailSettings;
                if ( _workflowType?.FormBuilderTemplate?.ConfirmationEmailSettings?.Enabled == true )
                {
                    // Use FormBuilderTemplate
                    confirmationEmailSettings = _workflowType?.FormBuilderTemplate.ConfirmationEmailSettings;
                }
                else if ( _workflowType?.FormBuilderSettings?.ConfirmationEmail != null )
                {
                    // Use FormBuilderSettings
                    confirmationEmailSettings = _workflowType.FormBuilderSettings.ConfirmationEmail;
                }
                else
                {
                    // Not a FormBuilder
                    confirmationEmailSettings = null;
                }

                if ( confirmationEmailSettings != null )
                {
                    if ( confirmationEmailSettings.Enabled == true )
                    {
                        SendFormBuilderConfirmationEmail( confirmationEmailSettings );
                    }
                }

                // Completion Action can come FormBuilderSettings or FormBuilderTemplate
                FormCompletionActionSettings completionActionSettings;
                if ( _workflowType?.FormBuilderTemplate?.CompletionActionSettings != null )
                {
                    // Use FormBuilderTemplate
                    completionActionSettings = _workflowType?.FormBuilderTemplate.CompletionActionSettings;
                }
                else if ( _workflowType?.FormBuilderSettings?.CompletionAction != null )
                {
                    // Use FormBuilderSettings
                    completionActionSettings = _workflowType.FormBuilderSettings.CompletionAction;
                }
                else
                {
                    // Not a FormBuilder
                    completionActionSettings = null;
                }

                // Notification Email is only defined on FormBuilder. FormBuilderTemplate doesn't have NotificationEmailSettings
                FormNotificationEmailSettings notificationEmailSettings = _workflowType?.FormBuilderSettings?.NotificationEmail;

                if ( notificationEmailSettings != null )
                {
                    SendFormBuilderNotificationEmail( notificationEmailSettings );
                }

                if ( completionActionSettings != null )
                {
                    if ( completionActionSettings.Type == FormCompletionActionType.Redirect )
                    {
                        // if this is a FormBuilder and has a completion action of Redirect, navigate to the specified URL
                        Response.Redirect( completionActionSettings.RedirectUrl, false );
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
            }
        }

        /// <summary>
        /// Sends the form builder confirmation email.
        /// </summary>
        /// <param name="confirmationEmailSettings">The confirmation email settings.</param>
        private void SendFormBuilderConfirmationEmail( FormConfirmationEmailSettings confirmationEmailSettings )
        {
            if ( confirmationEmailSettings == null || confirmationEmailSettings.Enabled == false )
            {
                return;
            }

            var formConfirmationEmailDestination = confirmationEmailSettings.Destination;

            Dictionary<string, object> workflowMergeFields = GetWorkflowEntryMergeFields();

            // If the RecipientType indicates that we should use the Person or Spouse key. We'll get the attribute from the Workflow.
            // Note it will only be a Workflow Attribute, not a Action attribute.
            AttributeCache recipientWorkflowAttribute;
            if ( formConfirmationEmailDestination == FormConfirmationEmailDestination.Person )
            {
                recipientWorkflowAttribute = _workflow.Attributes.GetValueOrNull( "Person" );
            }
            else if ( formConfirmationEmailDestination == FormConfirmationEmailDestination.Spouse )
            {
                // If the RecipientType indicates that we should use the Spouse key. We'll get the attribute from the Workflow
                recipientWorkflowAttribute = _workflow.Attributes.GetValueOrNull( "Spouse" );
            }
            else
            {
                Guid? recipientAttributeGuid = confirmationEmailSettings.RecipientAttributeGuid;
                recipientWorkflowAttribute = recipientAttributeGuid.HasValue
                    ? AttributeCache.Get( recipientAttributeGuid.Value )
                    : null;
            }

            if ( recipientWorkflowAttribute == null )
            {
                // Unable to to determine Recipient Attribute
                return;
            }

            var recipients = new List<RockMessageRecipient>();

            var rockContext = new RockContext();

            var recipientWorkflowAttributeValue = GetWorkflowAttributeEntityAttributeValue( recipientWorkflowAttribute );
            if ( recipientWorkflowAttribute.FieldTypeId == FieldTypeCache.GetId( Rock.SystemGuid.FieldType.PERSON.AsGuid() ) )
            {
                Guid personAliasGuid = recipientWorkflowAttributeValue.AsGuid();
                if ( !personAliasGuid.IsEmpty() )
                {
                    var recipientPerson = new PersonAliasService( rockContext ).GetPerson( personAliasGuid );
                    if ( recipientPerson != null && !string.IsNullOrWhiteSpace( recipientPerson.Email ) )
                    {
                        recipients.Add( new RockEmailMessageRecipient( recipientPerson, workflowMergeFields ) );
                    }
                }
            }
            else
            {
                // If this isn't a Person, assume it is an email address.
                string recipientEmailAddress = recipientWorkflowAttributeValue;
                recipients.Add( RockEmailMessageRecipient.CreateAnonymous( recipientEmailAddress, workflowMergeFields ) );
            }

            SendFormBuilderCommunication( confirmationEmailSettings.Source, recipients );
        }

        /// <summary>
        /// Sends the form builder notification email.
        /// </summary>
        /// <param name="notificationEmailSettings">The notification email settings.</param>
        private void SendFormBuilderNotificationEmail( FormNotificationEmailSettings notificationEmailSettings )
        {
            if ( notificationEmailSettings == null || notificationEmailSettings.Enabled == false )
            {
                return;
            }

            var rockContext = new RockContext();

            var formNotificationEmailDestination = notificationEmailSettings.Destination;

            Dictionary<string, object> workflowMergeFields = GetWorkflowEntryMergeFields();
            var recipients = new List<RockMessageRecipient>();

            if ( formNotificationEmailDestination == FormNotificationEmailDestination.EmailAddress
                && notificationEmailSettings.EmailAddress.IsNotNullOrWhiteSpace() )
            {
                string[] recipientEmailAddresses = notificationEmailSettings.EmailAddress.Replace( ";", "," ).Split( ',' );

                foreach ( var eachRcipient in recipientEmailAddresses )
                {
                    recipients.Add( RockEmailMessageRecipient.CreateAnonymous( eachRcipient, workflowMergeFields ) );
                }
            }
            else if ( formNotificationEmailDestination == FormNotificationEmailDestination.SpecificIndividual
                && notificationEmailSettings.RecipientAliasId.HasValue )
            {
                var recipientPerson = new PersonAliasService( rockContext ).GetPerson( notificationEmailSettings.RecipientAliasId.Value );
                if ( recipientPerson == null )
                {
                    return;
                }

                recipients.Add( new RockEmailMessageRecipient( recipientPerson, workflowMergeFields ) );
            }
            else if ( formNotificationEmailDestination == FormNotificationEmailDestination.CampusTopic
                && notificationEmailSettings.CampusTopicValueId.HasValue )
            {
                var workflowCampusId = _workflow?.CampusId;
                if ( workflowCampusId.HasValue )
                {
                    var campusTopicEmail = new CampusTopicService( rockContext ).Queryable()
                        .Where( a => a.TopicTypeValueId == notificationEmailSettings.CampusTopicValueId.Value && a.CampusId == workflowCampusId )
                        .Select( a => a.Email ).FirstOrDefault();

                    if ( campusTopicEmail.IsNullOrWhiteSpace() )
                    {
                        return;
                    }

                    recipients.Add( RockEmailMessageRecipient.CreateAnonymous( campusTopicEmail, workflowMergeFields ) );
                }
            }
            else
            {
                return;
            }

            SendFormBuilderCommunication( notificationEmailSettings.Source, recipients );
        }

        /// <summary>
        /// Sends a form builder communication.
        /// </summary>
        /// <param name="formEmailSourceSettings">The form email source settings.</param>
        /// <param name="recipients">The recipients.</param>
        private void SendFormBuilderCommunication( FormEmailSourceSettings formEmailSourceSettings, List<RockMessageRecipient> recipients )
        {
            if ( formEmailSourceSettings.Type == FormEmailSourceType.UseTemplate && formEmailSourceSettings.SystemCommunicationId.HasValue )
            {
                var systemCommunication = new SystemCommunicationService( new RockContext() ).Get( formEmailSourceSettings.SystemCommunicationId.Value );
                if ( systemCommunication != null )
                {
                    var emailMessage = new RockEmailMessage( systemCommunication );
                    emailMessage.SetRecipients( recipients );
                    emailMessage.Send();
                }
            }
            else if ( formEmailSourceSettings.Type == FormEmailSourceType.Custom )
            {
                string customBody;
                if ( formEmailSourceSettings.AppendOrgHeaderAndFooter )
                {
                    var globalEmailHeader = "{{ 'Global' | Attribute:'EmailHeader' }}";
                    var globalEmailFooter = "{{ 'Global' | Attribute:'EmailFooter' }}";

                    customBody = $@"
{globalEmailHeader}
{formEmailSourceSettings.Body}
{globalEmailFooter}
";
                }
                else
                {
                    customBody = formEmailSourceSettings.Body;
                }

                Dictionary<string, object> workflowMergeFields = GetWorkflowEntryMergeFields();

                var emailMessage = new RockEmailMessage
                {
                    ReplyToEmail = formEmailSourceSettings.ReplyTo,
                    Subject = formEmailSourceSettings.Subject,
                    Message = customBody?.ResolveMergeFields( workflowMergeFields )
                };

                emailMessage.SetRecipients( recipients );
                emailMessage.Send();
            }
        }

        /// <summary>
        /// Shows the message.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="hideForm">if set to <c>true</c> [hide form].</param>
        private void ShowMessage( NotificationBoxType type, string title, string message, bool hideForm = true )
        {
            nbMessage.NotificationBoxType = type;
            nbMessage.Title = title;
            nbMessage.Text = message;
            nbMessage.Visible = true;
            nbMessage.Dismissable = false;

            if ( hideForm )
            {
                pnlWorkflowUserForm.Visible = false;
                pnlWorkflowActionElectronicSignature.Visible = false;
            }
        }

        /// <summary>
        /// Set the properties of the block title bar.
        /// </summary>
        private void SetBlockTitle()
        {
            // If the block title is specified by a configuration setting, use it.
            var blockTitle = GetAttributeValue( AttributeKey.BlockTitleTemplate );
            var workflowType = GetWorkflowType();

            if ( !string.IsNullOrWhiteSpace( blockTitle ) )
            {
                // Resolve the block title using the specified Lava template.
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );

                mergeFields.Add( "WorkflowType", workflowType );

                // Add the WorkflowType as the default Item.
                mergeFields.Add( "Item", workflowType );

                blockTitle = blockTitle.ResolveMergeFields( mergeFields );
            }

            // If the block title is not configured, use the Workflow Type if it is available.
            if ( string.IsNullOrWhiteSpace( blockTitle ) )
            {
                if ( workflowType != null )
                {
                    blockTitle = string.Format( "{0} Entry", workflowType.WorkTerm );
                }
                else
                {
                    blockTitle = "Workflow Entry";
                }
            }

            lTitle.Text = blockTitle;

            // Set the Page Title to the Workflow Type name, unless the Workflow Type has been specified by a configuration setting.
            if ( workflowType != null && !WorkflowTypeDeterminedByBlockAttribute )
            {
                RockPage.PageTitle = workflowType.Name;
            }

            // Set the Block Icon.
            var blockTitleIconCssClass = GetAttributeValue( AttributeKey.BlockTitleIconCssClass );

            if ( string.IsNullOrWhiteSpace( blockTitleIconCssClass ) )
            {
                if ( workflowType != null )
                {
                    blockTitleIconCssClass = workflowType.IconCssClass;
                }
            }

            if ( !string.IsNullOrWhiteSpace( blockTitleIconCssClass ) )
            {
                lIconHtml.Text = string.Format( "<i class='{0}' ></i>", blockTitleIconCssClass );

                // If the Page Icon is not configured, set it to the same icon as the block.
                if ( string.IsNullOrWhiteSpace( RockPage.PageIcon ) )
                {
                    RockPage.PageIcon = blockTitleIconCssClass;
                }
            }
        }

        /// <summary>
        /// Adds the Submit button to the WorkflowEntry form
        /// </summary>
        /// <param name="form"></param>
        private void AddSubmitButtons( WorkflowActionFormCache form )
        {
            if ( form == null )
            {
                return;
            }

            phActions.Controls.Clear();

            var buttons = WorkflowActionFormUserAction.FromUriEncodedString( form.Actions );

            foreach ( var button in buttons )
            {
                // Get the button HTML. If actionParts has a guid at [1],
                // get the buttonHtml from the DefinedValue with that Guid.
                // Otherwise, use a default
                string buttonHtml = string.Empty;
                DefinedValueCache buttonDefinedValue = null;

                Guid? buttonHtmlDefinedValueGuid = button.ButtonTypeGuid.AsGuidOrNull();
                if ( buttonHtmlDefinedValueGuid.HasValue )
                {
                    buttonDefinedValue = DefinedValueCache.Get( buttonHtmlDefinedValueGuid.Value );
                }

                if ( buttonDefinedValue != null )
                {
                    buttonHtml = buttonDefinedValue.GetAttributeValue( "ButtonHTML" );
                }

                if ( buttonHtml.IsNullOrWhiteSpace() )
                {
                    buttonHtml = "<a href=\"{{ ButtonLink }}\" onclick=\"{{ ButtonClick }}\" class='btn btn-primary' data-loading-text='<i class=\"fa fa-refresh fa-spin\"></i> {{ ButtonText }}'>{{ ButtonText }}</a>";
                }

                var buttonMergeFields = new Dictionary<string, object>();
                var buttonText = button.ActionName.EncodeHtml();
                buttonMergeFields.Add( "ButtonText", buttonText );

                string buttonClickScript = string.Format(
                    "handleWorkflowActionButtonClick('{0}', {1});",
                    BlockValidationGroup,
                    button.CausesValidation.ToJavaScriptValue() );

                buttonMergeFields.Add( "ButtonClick", buttonClickScript );

                var buttonLinkScript = Page.ClientScript.GetPostBackClientHyperlink( this, button.ActionName );
                buttonMergeFields.Add( "ButtonLink", buttonLinkScript );

                buttonHtml = buttonHtml.ResolveMergeFields( buttonMergeFields );

                phActions.Controls.Add( new LiteralControl( buttonHtml ) );
                phActions.Controls.Add( new LiteralControl( " " ) );
            }
        }

        #endregion Methods

        #region ElectronicSignature Related stuff

        /// <summary>
        /// Builds the workflow action digital signature.
        /// </summary>
        /// <param name="electronicSignatureWorkflowAction">The electronic signature workflow action.</param>
        /// <param name="workflowAction">The workflow action.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildWorkflowActionDigitalSignature( Rock.Workflow.Action.ElectronicSignature electronicSignatureWorkflowAction, WorkflowAction workflowAction, bool setValues )
        {
            ShowNotes( false );
            pnlWorkflowUserForm.Visible = false;
            pnlWorkflowActionElectronicSignature.Visible = true;

            var rockContext = new RockContext();

            var signatureDocumentTemplate = electronicSignatureWorkflowAction.GetSignatureDocumentTemplate( rockContext, workflowAction );
            if ( signatureDocumentTemplate == null )
            {
                return;
            }

            escElectronicSignatureControl.SignatureType = signatureDocumentTemplate.SignatureType;
            escElectronicSignatureControl.DocumentTerm = signatureDocumentTemplate.DocumentTerm;

            var signedByPersonAliasId = electronicSignatureWorkflowAction.GetSignedByPersonAliasId( rockContext, workflowAction, this.CurrentPersonAliasId );
            if ( signedByPersonAliasId.HasValue )
            {
                // Default email to the SignedByPerson's email
                var signedByPerson = new PersonAliasService( rockContext ).GetPerson( signedByPersonAliasId.Value );
                escElectronicSignatureControl.SignedByEmail = signedByPerson?.Email;

                // When in Drawn Mode, we want to prefill the 'Confirm' Legal Name. (But not the Signed Name)
                escElectronicSignatureControl.LegalName = signedByPerson?.FullName;
            }

            // If not logged-in or the Workflow hasn't specified a SignedByPerson, show the name that was typed when on the Completion step
            escElectronicSignatureControl.ShowNameOnCompletionStepWhenInTypedSignatureMode = ( signedByPersonAliasId == null );

            escElectronicSignatureControl.EmailAddressPrompt = signatureDocumentTemplate.CompletionSystemCommunicationId.HasValue
                ? ElectronicSignatureControl.EmailAddressPromptType.CompletionEmail
                : ElectronicSignatureControl.EmailAddressPromptType.PersonEmail;

            if ( setValues )
            {
                var mergeFields = GetWorkflowEntryMergeFields();
                var lavaTemplate = signatureDocumentTemplate.LavaTemplate;
                this.SignatureDocumentHtml = lavaTemplate?.ResolveMergeFields( mergeFields );
                iframeSignatureDocumentHTML.Attributes["srcdoc"] = this.SignatureDocumentHtml;
                iframeSignatureDocumentHTML.Attributes.Add( "onload", "resizeIframe(this)" );
                iframeSignatureDocumentHTML.Attributes.Add( "onresize", "resizeIframe(this)" );
            }
        }

        /// <summary>
        /// Handles the Click event of the <see cref="ElectronicSignatureControl" />
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSignSignature_Click( object sender, EventArgs e )
        {
            // From Workflow
            var electronicSignatureWorkflowAction = _actionType?.WorkflowAction as Rock.Workflow.Action.ElectronicSignature;
            if ( electronicSignatureWorkflowAction == null )
            {
                ShowMessage( NotificationBoxType.Danger, "Configuration Error", "Unable to determine Signature Action." );
                return;
            }

            var rockContext = new RockContext();
            var workflowAction = _action;
            var signatureDocumentTemplate = electronicSignatureWorkflowAction.GetSignatureDocumentTemplate( rockContext, workflowAction );
            if ( signatureDocumentTemplate == null )
            {
                ShowMessage( NotificationBoxType.Danger, "Configuration Error", "Unable to determine Signature Template." );
                return;
            }

            var signedByPersonAliasId = electronicSignatureWorkflowAction.GetSignedByPersonAliasId( rockContext, workflowAction, this.CurrentPersonAliasId );
            Person signedByPerson;
            if ( signedByPersonAliasId.HasValue )
            {
                signedByPerson = new PersonAliasService( rockContext ).GetPerson( signedByPersonAliasId.Value );
            }
            else
            {
                signedByPerson = null;
            }

            var appliesToPersonAliasId = electronicSignatureWorkflowAction.GetAppliesToPersonAliasId( rockContext, workflowAction );

            Dictionary<string, object> mergeFields = GetWorkflowEntryMergeFields();
            mergeFields.Add( "SignatureDocumentTemplate", signatureDocumentTemplate );

            var signatureDocumentName = electronicSignatureWorkflowAction.GetSignatureDocumentName( workflowAction, mergeFields );
            if ( signatureDocumentName.IsNullOrWhiteSpace() )
            {
                signatureDocumentName = "Signed Document";
            }

            var assignedToPersonAliasId = electronicSignatureWorkflowAction.GetAssignedToPersonAliasId( rockContext, workflowAction );

            // Glue stuff into the signature document
            var signatureDocument = new SignatureDocument();

            // From Workflow Action
            signatureDocument.SignatureDocumentTemplateId = signatureDocumentTemplate.Id;
            signatureDocument.Status = SignatureDocumentStatus.Signed;
            signatureDocument.Name = signatureDocumentName;
            signatureDocument.EntityTypeId = EntityTypeCache.GetId<Workflow>();
            signatureDocument.EntityId = _workflow?.Id;
            signatureDocument.SignedByPersonAliasId = signedByPersonAliasId;
            signatureDocument.AssignedToPersonAliasId = assignedToPersonAliasId;
            signatureDocument.AppliesToPersonAliasId = appliesToPersonAliasId;

            // From Workflow Entry
            signatureDocument.SignedDocumentText = this.SignatureDocumentHtml;
            signatureDocument.LastStatusDate = RockDateTime.Now;
            signatureDocument.SignedDateTime = RockDateTime.Now;

            // From ElectronicSignatureControl
            signatureDocument.SignatureData = escElectronicSignatureControl.DrawnSignatureImageDataUrl;
            signatureDocument.SignedName = escElectronicSignatureControl.SignedName;
            signatureDocument.SignedByEmail = escElectronicSignatureControl.SignedByEmail;

            // From System.Web
            signatureDocument.SignedClientIp = this.GetClientIpAddress();
            signatureDocument.SignedClientUserAgent = Request.UserAgent;

            // Needed before determing SignatureInformation (Signed Name, metadata)
            signatureDocument.SignatureVerificationHash = SignatureDocumentService.CalculateSignatureVerificationHash( signatureDocument );

            var signatureInformationHtmlArgs = new GetSignatureInformationHtmlOptions
            {
                SignatureType = signatureDocumentTemplate.SignatureType,
                SignedName = escElectronicSignatureControl.SignedName,
                DrawnSignatureDataUrl = escElectronicSignatureControl.DrawnSignatureImageDataUrl,
                SignedByPerson = signedByPerson,
                SignedDateTime = signatureDocument.SignedDateTime,
                SignedClientIp = signatureDocument.SignedClientIp,
                SignatureVerificationHash = signatureDocument.SignatureVerificationHash
            };

            // Helper takes care of generating HTML and combining SignatureDocumentHTML and signedSignatureDocumentHtml into the final Signed Document
            var signatureInformationHtml = ElectronicSignatureHelper.GetSignatureInformationHtml( signatureInformationHtmlArgs );
            var signedSignatureDocumentHtml = ElectronicSignatureHelper.GetSignedDocumentHtml( this.SignatureDocumentHtml, signatureInformationHtml );

            // PDF Generator to BinaryFile
            BinaryFile pdfFile;
            try
            {
                using ( var pdfGenerator = new PdfGenerator() )
                {
                    var binaryFileTypeId = signatureDocumentTemplate.BinaryFileTypeId;
                    if ( !binaryFileTypeId.HasValue )
                    {
                        binaryFileTypeId = BinaryFileTypeCache.GetId( Rock.SystemGuid.BinaryFiletype.DIGITALLY_SIGNED_DOCUMENTS.AsGuid() );
                    }

                    pdfFile = pdfGenerator.GetAsBinaryFileFromHtml( binaryFileTypeId ?? 0, signatureDocumentName, signedSignatureDocumentHtml );
                }
            }
            catch ( PdfGeneratorException pdfGeneratorException )
            {
                LogException( pdfGeneratorException );
                ShowMessage( NotificationBoxType.Danger, "Document Error", pdfGeneratorException.Message );
                return;
            }

            pdfFile.IsTemporary = false;
            new BinaryFileService( rockContext ).Add( pdfFile );
            rockContext.SaveChanges();
            signatureDocument.BinaryFileId = pdfFile.Id;

            // Save Signature Documen to database
            var signatureDocumentService = new SignatureDocumentService( rockContext );
            signatureDocumentService.Add( signatureDocument );
            rockContext.SaveChanges();

            // reload with new context to get navigation properties to load. This wil be needed to save values back to Workflow Attributes
            signatureDocument = new SignatureDocumentService( new RockContext() ).Get( signatureDocument.Id );

            // Save to Workflow Attributes
            electronicSignatureWorkflowAction.SaveSignatureDocumentValuesToAttributes( _workflowRockContext, workflowAction, signatureDocument );

            // Send Communication
            if ( signatureDocumentTemplate.CompletionSystemCommunication != null )
            {
                ElectronicSignatureHelper.SendSignatureCompletionCommunication( signatureDocument.Id, out _ );
            }

            // Workflow
            CompleteSignatureAction();
        }

        /// <summary>
        /// Completes the signature action.
        /// </summary>
        private void CompleteSignatureAction()
        {
            CompleteCurrentWorkflowAction( null, "Your signature has been submitted successfully." );
        }

        #endregion Electronic Signature Related stuff

        #region Interaction Methods

        /// <summary>
        /// Initializes the interactions.
        /// </summary>
        private void InitializeInteractions()
        {
            var urlInteractionStartDateTime = this.PageParameter( PageParameterKey.InteractionStartDateTime ).AsDateTime();
            this.InteractionStartDateTime = urlInteractionStartDateTime ?? RockDateTime.Now;

            if ( this.GetAttributeValue( AttributeKey.LogInteractionOnView ).AsBoolean() )
            {
                // if this is the First Viewed Form (which we can detect if the URL doesn't contain a StartDateTime) log a FormView interaction
                if ( !urlInteractionStartDateTime.HasValue )
                {
                    LogWorkflowEntryInteraction( _workflow, _actionType?.Id, WorkflowInteractionOperationType.FormViewed );
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        private enum WorkflowInteractionOperationType
        {
            FormViewed,
            FormCompleted,
        }

        /// <summary>
        /// Logs a 'Form Viewed' or 'Form Completed' Interaction
        /// </summary>
        private void LogWorkflowEntryInteraction( Workflow workflow, int? workflowActionTypeId, WorkflowInteractionOperationType workflowInteractionOperationType )
        {
            if ( workflowInteractionOperationType == WorkflowInteractionOperationType.FormCompleted )
            {
                if ( !this.GetAttributeValue( AttributeKey.LogInteractionOnCompletion ).AsBoolean() )
                {
                    return;
                }
            }
            else
            {
                if ( !this.GetAttributeValue( AttributeKey.LogInteractionOnView ).AsBoolean() )
                {
                    return;
                }
            }

            var workflowLaunchInteractionChannelId = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.WORKFLOW_LAUNCHES.AsGuid() );

            var interactionTransactionInfo = new InteractionTransactionInfo
            {
                // NOTE: InteractionTransactionInfo.PersonAliasId will do this same logic if PersonAliasId isn't specified. Doing it here to
                // make it more obvious.
                PersonAliasId = this.CurrentPersonAliasId ?? this.CurrentVisitor?.Id,

                InteractionEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.WORKFLOW.AsGuid() ),
                InteractionDateTime = RockDateTime.Now,
                InteractionChannelId = workflowLaunchInteractionChannelId ?? 0,
                InteractionRelatedEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.WORKFLOW_ACTION_TYPE.AsGuid() ),
                InteractionRelatedEntityId = workflowActionTypeId,
                LogCrawlers = false
            };

            /* 7-30-2021 MDP

             If the workflow isn't persisted, the WorkflowId would be 0. If so, just leave the InteractionEntityId
             null. The InteractionData will still have WorkflowType and ActionType, which are the main things that will
             be needed when looking at WorkflowEntry Interactions. So, leaving InteractionEntityId null (workflow.Id)
             is OK.
             see https://app.asana.com/0/0/1200679813013532/f
            */
            if ( workflow.Id > 0 )
            {
                interactionTransactionInfo.InteractionEntityId = workflow.Id;
                interactionTransactionInfo.InteractionEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.WORKFLOW.AsGuid() );
            }

            var workflowType = GetWorkflowType();

            if ( workflowInteractionOperationType == WorkflowInteractionOperationType.FormCompleted )
            {
                interactionTransactionInfo.InteractionSummary = $"Completed a workflow of type: { workflowType?.Name }";
                interactionTransactionInfo.InteractionOperation = "Form Completed";

                if ( this.InteractionStartDateTime.HasValue )
                {
                    interactionTransactionInfo.InteractionLength = ( RockDateTime.Now - this.InteractionStartDateTime.Value ).TotalSeconds;
                }
            }
            else
            {
                interactionTransactionInfo.InteractionSummary = $"Launched a workflow of type: { workflowType?.Name }";
                interactionTransactionInfo.InteractionOperation = "Form Viewed";
            }

            // there is only one Channel for Workflow Entry (Rock.SystemGuid.InteractionChannel.WORKFLOW_LAUNCHES)
            // so there isn't a channel entity
            IEntity channelEntity = null;

            var componentEntity = workflowType;

            var interactionTransaction = new InteractionTransaction(
                DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_SYSTEM_EVENTS.AsGuid() ),
                channelEntity,
                componentEntity,
                interactionTransactionInfo );

            interactionTransaction.Enqueue();
        }

        #endregion Interaction Methods
    }
}
