﻿// <copyright>
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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

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

    #endregion

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
            public const string Command = "Command";
            public const string GroupId = "GroupId";
            public const string PersonId = "PersonId";
        }

        #endregion PageParameter Keys

        private static class ViewStateKey
        {
            public const string WorkflowTypeId = "WorkflowTypeId";
            public const string ActionTypeId = "ActionTypeId";
            public const string WorkflowId = "WorkflowId";
            public const string WorkflowTypeDeterminedByBlockAttribute = "WorkflowTypeDeterminedByBlockAttribute";
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

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the workflow type identifier.
        /// </summary>
        /// <value>
        /// The workflow type identifier.
        /// </value>
        public int? WorkflowTypeId
        {
            get { return ViewState[ViewStateKey.WorkflowTypeId] as int?; }
            set { ViewState[ViewStateKey.WorkflowTypeId] = value; }
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

        #endregion

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
                BuildWorkflowActionForm( false );
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

            nbMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                if ( HydrateObjects() )
                {
                    BuildWorkflowActionForm( true );
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

        #endregion

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
            using ( var personEntryRockContext = new RockContext() )
            {
                GetWorkflowFormPersonEntryValues( personEntryRockContext );
            }

            GetWorkflowFormAttributeValues();
            CompleteFormAction( eventArgument );
        }

        #endregion

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

            if ( workflowType == null )
            {
                ShowNotes( false );
                ShowMessage( NotificationBoxType.Danger, "Configuration Error", "Workflow type was not configured or specified correctly." );
                return false;
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
                        _workflow = _workflowService.Queryable()
                            .Where( w => w.Guid.Equals( guid ) && w.WorkflowTypeId == workflowType.Id )
                            .FirstOrDefault();
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

                // If a PersonId or GroupId parameter was included, load the corresponding
                // object and pass that to the actions for processing
                IEntity entity = null;
                int? personId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    entity = new PersonService( _workflowRockContext ).Get( personId.Value );
                }
                else
                {
                    int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                    if ( groupId.HasValue )
                    {
                        entity = new GroupService( _workflowRockContext ).Get( groupId.Value );
                    }
                }

                // Loop through all the query string parameters and try to set any workflow
                // attributes that might have the same key
                foreach ( var param in RockPage.PageParameters() )
                {
                    if ( param.Value != null && param.Value.ToString().IsNotNullOrWhiteSpace() )
                    {
                        _workflow.SetAttributeValue( param.Key, param.Value.ToString() );
                    }
                }

                List<string> errorMessages;
                if ( !_workflowService.Process( _workflow, entity, out errorMessages ) )
                {
                    ShowNotes( false );
                    ShowMessage(
                        NotificationBoxType.Danger,
                        "Workflow Processing Error(s):",
                        "<ul><li>" + errorMessages.AsDelimited( "</li><li>" ) + "</li></ul>" );
                    return false;
                }

                if ( _workflow.Id != 0 )
                {
                    WorkflowId = _workflow.Id;
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
                        foreach ( var action in activity.ActiveActions
                            .Where( a => ( !actionId.HasValue || a.Id == actionId.Value ) ) )
                        {
                            if ( action.ActionTypeCache.WorkflowForm != null && action.IsCriteriaValid )
                            {
                                _activity = activity;
                                _activity.LoadAttributes();

                                _action = action;
                                _actionType = _action.ActionTypeCache;
                                ActionTypeId = _actionType.Id;
                                return true;
                            }
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

            ShowNotes( false );
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
        /// Loads the WorkflowType
        /// </summary>
        private WorkflowTypeCache GetWorkflowType()
        {
            // Get the workflow type id (initial page request)
            if ( !WorkflowTypeId.HasValue )
            {
                // Get workflow type set by attribute value
                Guid workflowTypeguid = GetAttributeValue( AttributeKey.WorkflowType ).AsGuid();

                WorkflowTypeCache _workflowType = null;
                if ( !workflowTypeguid.IsEmpty() )
                {
                    _workflowType = WorkflowTypeCache.Get( workflowTypeguid );
                }

                // If an attribute value was not provided, check for query/route value
                if ( _workflowType != null )
                {
                    WorkflowTypeId = _workflowType.Id;
                    WorkflowTypeDeterminedByBlockAttribute = true;
                }
                else
                {
                    WorkflowTypeId = PageParameter( PageParameterKey.WorkflowTypeId ).AsIntegerOrNull();
                    WorkflowTypeDeterminedByBlockAttribute = false;
                }
            }

            // Get the workflow type
            if ( WorkflowTypeId.HasValue )
            {
                return WorkflowTypeCache.Get( WorkflowTypeId.Value );
            }
            else
            {
                return null;
            }
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
        /// Builds the WorkflowActionForm.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildWorkflowActionForm( bool setValues )
        {
            Dictionary<string, object> mergeFields = GetWorkflowEntryMergeFields();

            var form = _actionType.WorkflowForm;

            if ( setValues )
            {
                lFormHeaderText.Text = form.Header.ResolveMergeFields( mergeFields );
                lFormFooterText.Text = form.Footer.ResolveMergeFields( mergeFields );
            }

            if ( _workflow != null && _workflow.CreatedDateTime.HasValue )
            {
                hlblDateAdded.Text = string.Format( "Added: {0}", _workflow.CreatedDateTime.Value.ToShortDateString() );
            }
            else
            {
                hlblDateAdded.Visible = false;
            }

            pnlPersonEntry.Visible = form.AllowPersonEntry;
            if ( form.AllowPersonEntry )
            {
                BuildPersonEntryForm( form, setValues, mergeFields );
            }

            phAttributes.Controls.Clear();

            foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
            {
                if ( !formAttribute.IsVisible )
                {
                    continue;
                }

                var attribute = AttributeCache.Get( formAttribute.AttributeId );

                string value = attribute.DefaultValue;
                if ( _workflow != null && _workflow.AttributeValues.ContainsKey( attribute.Key ) && _workflow.AttributeValues[attribute.Key] != null )
                {
                    // if the key is in the workflow's attributes get the value from that
                    value = _workflow.AttributeValues[attribute.Key].Value;
                }
                else if ( _activity != null && _activity.AttributeValues.ContainsKey( attribute.Key ) && _activity.AttributeValues[attribute.Key] != null )
                {
                    // if the key is in the activity's attributes get the value from that
                    value = _activity.AttributeValues[attribute.Key].Value;
                }

                if ( !string.IsNullOrWhiteSpace( formAttribute.PreHtml ) )
                {
                    phAttributes.Controls.Add( new LiteralControl( formAttribute.PreHtml.ResolveMergeFields( mergeFields ) ) );
                }

                if ( formAttribute.IsReadOnly )
                {
                    var field = attribute.FieldType.Field;

                    string formattedValue = null;

                    // get formatted value
                    if ( attribute.FieldType.Class == typeof( Rock.Field.Types.ImageFieldType ).FullName )
                    {
                        formattedValue = field.FormatValueAsHtml( phAttributes, attribute.EntityTypeId, _activity.Id, value, attribute.QualifierValues, true );
                    }
                    else
                    {
                        formattedValue = field.FormatValueAsHtml( phAttributes, attribute.EntityTypeId, _activity.Id, value, attribute.QualifierValues );
                    }

                    if ( formAttribute.HideLabel )
                    {
                        phAttributes.Controls.Add( new LiteralControl( formattedValue ) );
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
                            lAttribute.Text = string.Format( "<a href='{0}' target='_blank'>{1}</a>", url, formattedValue );
                        }
                        else
                        {
                            lAttribute.Text = formattedValue;
                        }

                        phAttributes.Controls.Add( lAttribute );
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

                    attribute.AddControl( phAttributes.Controls, attributeControlOptions );
                }

                if ( !string.IsNullOrWhiteSpace( formAttribute.PostHtml ) )
                {
                    phAttributes.Controls.Add( new LiteralControl( formAttribute.PostHtml.ResolveMergeFields( mergeFields ) ) );
                }
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

                var buttonLinkScript = Page.ClientScript.GetPostBackClientHyperlink( this, buttonText );
                buttonMergeFields.Add( "ButtonLink", buttonLinkScript );

                buttonHtml = buttonHtml.ResolveMergeFields( buttonMergeFields );

                phActions.Controls.Add( new LiteralControl( buttonHtml ) );
                phActions.Controls.Add( new LiteralControl( " " ) );
            }
        }

        /// <summary>
        /// Builds the person entry form.
        /// </summary>
        /// <param name="form">The form.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="mergeFields">The merge fields.</param>
        private void BuildPersonEntryForm( WorkflowActionFormCache form, bool setValues, Dictionary<string, object> mergeFields )
        {
            pnlPersonEntry.Visible = form.AllowPersonEntry;
            if ( !form.AllowPersonEntry )
            {
                return;
            }

            if ( form.PersonEntryHideIfCurrentPersonKnown && CurrentPerson != null )
            {
                pnlPersonEntry.Visible = false;
                return;
            }

            lPersonEntryPreHtml.Text = form.PersonEntryPreHtml.ResolveMergeFields( mergeFields );

            // NOTE: If there is only one Campus in the system, this control be always be hidden
            cpPersonEntryCampus.Visible = form.PersonEntryCampusIsVisible;

            SetPersonEditorOptions( pePerson1, form );
            SetPersonEditorOptions( pePerson2, form );
            pePerson2.PersonLabelPrefix = form.PersonEntrySpouseLabel;
            cbShowPerson2.TextCssClass = "font-weight-semibold";
            cbShowPerson2.ContainerCssClass = "show-spouse mt-3 mb-4";
            cbShowPerson2.Text = string.Format( "Show {0}", form.PersonEntrySpouseLabel );
            switch ( form.PersonEntrySpouseEntryOption )
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
            dvpMaritalStatus.Required = form.PersonEntryMaritalStatusEntryOption == WorkflowActionFormPersonEntryOption.Required;

            if ( form.PersonEntryMaritalStatusEntryOption == WorkflowActionFormPersonEntryOption.Hidden )
            {
                dvpMaritalStatus.Visible = false;
            }

            acPersonEntryAddress.Required = form.PersonEntryAddressEntryOption == WorkflowActionFormPersonEntryOption.Required;

            lPersonEntryPostHtml.Text = form.PersonEntryPostHtml.ResolveMergeFields( mergeFields );

            var promptForAddress = ( form.PersonEntryAddressEntryOption != WorkflowActionFormPersonEntryOption.Hidden ) && form.PersonEntryGroupLocationTypeValueId.HasValue;
            acPersonEntryAddress.Visible = promptForAddress;

            if ( setValues )
            {
                SetPersonEntryValues( form );
            }
        }

        /// <summary>
        /// Sets the person entry values.
        /// </summary>
        /// <param name="form">The form.</param>
        private void SetPersonEntryValues( WorkflowActionFormCache form )
        {
            var rockContext = new RockContext();
            Person personEntryPerson = null;
            Person personEntrySpouse = null;
            int? personEntryFamilyId = null;

            if ( form.PersonEntryAutofillCurrentPerson && CurrentPersonId.HasValue )
            {
                var personService = new PersonService( rockContext );
                personEntryPerson = personService.Get( CurrentPersonId.Value );
                personEntrySpouse = personEntryPerson.GetSpouse();
            }
            else
            {
                // Not using the current person, so initialize with the current value of PersonEntryPersonAttributeGuid (normally this would be null unless then also had a PersonEntry form on previous Activities)
                if ( form.PersonEntryPersonAttributeGuid.HasValue )
                {
                    AttributeCache personEntryPersonAttribute = form.FormAttributes.Where( a => a.Attribute.Guid == form.PersonEntryPersonAttributeGuid.Value ).Select( a => a.Attribute ).FirstOrDefault();

                    var personAliasGuid = GetWorkflowAttributeEntityAttributeValue( personEntryPersonAttribute ).AsGuidOrNull();

                    if ( personAliasGuid.HasValue )
                    {
                        // the workflow already set a value for the FormEntry person, so use that
                        var personAliasService = new PersonAliasService( rockContext );
                        personEntryPerson = personAliasService.GetPerson( personAliasGuid.Value );
                    }
                }

                // Not using the current person, so initialize with the current value PersonEntrySpouseAttributeGuid (normally this would be null unless then also had a PersonEntry form on previous Activities)
                if ( form.PersonEntrySpouseAttributeGuid.HasValue )
                {
                    AttributeCache personEntrySpouseAttribute = form.FormAttributes.Where( a => a.Attribute.Guid == form.PersonEntrySpouseAttributeGuid.Value ).Select( a => a.Attribute ).FirstOrDefault();

                    var spousePersonAliasGuid = GetWorkflowAttributeEntityAttributeValue( personEntrySpouseAttribute ).AsGuidOrNull();

                    if ( spousePersonAliasGuid.HasValue )
                    {
                        // the workflow already set a value for the FormEntry person, so use that
                        var personAliasService = new PersonAliasService( rockContext );
                        personEntrySpouse = personAliasService.GetPerson( spousePersonAliasGuid.Value );
                    }
                }
            }

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

            if ( form.PersonEntrySpouseEntryOption != WorkflowActionFormPersonEntryOption.Hidden )
            {
                pePerson2.SetFromPerson( personEntrySpouse );
            }

            var promptForAddress = ( form.PersonEntryAddressEntryOption != WorkflowActionFormPersonEntryOption.Hidden ) && form.PersonEntryGroupLocationTypeValueId.HasValue;

            if ( promptForAddress && personEntryFamilyId != null )
            {
                var personEntryGroupLocationTypeValueId = form.PersonEntryGroupLocationTypeValueId.Value;

                var familyLocation = new GroupLocationService( rockContext ).Queryable()
                    .Where( a => a.GroupId == personEntryFamilyId.Value && a.GroupLocationTypeValueId == form.PersonEntryGroupLocationTypeValueId ).Select( a => a.Location ).FirstOrDefault();

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
        private static void SetPersonEditorOptions( PersonBasicEditor personBasicEditor, WorkflowActionFormCache form )
        {
            personBasicEditor.ShowInColumns = false;
            personBasicEditor.ShowTitle = false;
            personBasicEditor.ShowSuffix = false;

            // Connection Status is determined by form.PersonEntryConnectionStatusValueId
            personBasicEditor.ShowConnectionStatus = false;

            // Role will always be Adult
            personBasicEditor.ShowPersonRole = false;
            personBasicEditor.ShowGrade = false;

            personBasicEditor.RequireGender = form.PersonEntryGenderEntryOption == WorkflowActionFormPersonEntryOption.Required;
            personBasicEditor.ShowGender = form.PersonEntryGenderEntryOption != WorkflowActionFormPersonEntryOption.Hidden;

            personBasicEditor.RequireEmail = form.PersonEntryEmailEntryOption == WorkflowActionFormPersonEntryOption.Required;
            personBasicEditor.ShowEmail = form.PersonEntryEmailEntryOption != WorkflowActionFormPersonEntryOption.Hidden;

            personBasicEditor.RequireMobilePhone = form.PersonEntryMobilePhoneEntryOption == WorkflowActionFormPersonEntryOption.Required;
            personBasicEditor.ShowMobilePhone = form.PersonEntryMobilePhoneEntryOption != WorkflowActionFormPersonEntryOption.Hidden;

            personBasicEditor.RequireBirthdate = form.PersonEntryBirthdateEntryOption == WorkflowActionFormPersonEntryOption.Required;
            personBasicEditor.ShowBirthdate = form.PersonEntryBirthdateEntryOption != WorkflowActionFormPersonEntryOption.Hidden;

            // workflow doesn't have a setting for requiring/showing gender
            personBasicEditor.RequireGender = true;
            personBasicEditor.ShowGender = true;

            // we have a another MaritalStatus picker that will apply to both Person and Person's Spouse
            personBasicEditor.ShowMaritalStatus = false;
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
            divNotes.Visible = visible;

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

            if ( !form.AllowPersonEntry )
            {
                return;
            }

            int? existingPersonId;
            int? existingPersonSpouseId = null;

            if ( CurrentPersonId.HasValue && ( form.PersonEntryAutofillCurrentPerson || form.PersonEntryHideIfCurrentPersonKnown ) )
            {
                existingPersonId = CurrentPersonId.Value;
                var existingPersonSpouse = CurrentPerson.GetSpouse( personEntryRockContext );
                if ( existingPersonSpouse != null )
                {
                    existingPersonSpouseId = existingPersonSpouse.Id;
                }

                if ( form.PersonEntryHideIfCurrentPersonKnown )
                {
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
                personEntryPerson.ConnectionStatusValueId = form.PersonEntryConnectionStatusValueId;
                personEntryPerson.RecordStatusValueId = form.PersonEntryRecordStatusValueId;
                PersonService.SaveNewPerson( personEntryPerson, personEntryRockContext, cpPersonEntryCampus.SelectedCampusId );
            }

            // if we ended up matching an existing person, get their spouse as the selected spouse
            var matchedPersonsSpouse = personEntryPerson.GetSpouse();

            if ( matchedPersonsSpouse != null )
            {
                existingPersonSpouseId = matchedPersonsSpouse.Id;
            }

            if ( form.PersonEntryMaritalStatusEntryOption != WorkflowActionFormPersonEntryOption.Hidden )
            {
                personEntryPerson.MaritalStatusValueId = dvpMaritalStatus.SelectedDefinedValueId;
            }

            // save person 1 to database and re-fetch to get any newly created family, or other things that would happen on PreSave changes, etc
            personEntryRockContext.SaveChanges();

            var personAliasService = new PersonAliasService( personEntryRockContext );

            int personEntryPersonId = personEntryPerson.Id;
            int? personEntryPersonSpouseId = null;

            var personService = new PersonService( personEntryRockContext );
            var primaryFamily = personService.GetSelect( personEntryPersonId, s => s.PrimaryFamily );

            if ( pePerson2.Visible )
            {
                var personEntryPersonSpouse = CreateOrUpdatePersonFromPersonEditor( existingPersonSpouseId, primaryFamily, pePerson2, personEntryRockContext );
                if ( personEntryPersonSpouse.Id == 0 )
                {
                    personEntryPersonSpouse.ConnectionStatusValueId = form.PersonEntryConnectionStatusValueId;
                    personEntryPersonSpouse.RecordStatusValueId = form.PersonEntryRecordStatusValueId;

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
                     
                     */
                    personEntryPersonSpouse.MaritalStatusValueId = dvpMaritalStatus.SelectedDefinedValueId;
                    personEntryPerson.MaritalStatusValueId = dvpMaritalStatus.SelectedDefinedValueId;

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

            if ( acPersonEntryAddress.Visible && form.PersonEntryGroupLocationTypeValueId.HasValue && acPersonEntryAddress.HasValue )
            {
                // a Person should always have a PrimaryFamilyId, but check to make sure, just in case
                if ( primaryFamily != null )
                {
                    var groupLocationService = new GroupLocationService( personEntryRockContext );
                    var familyLocation = primaryFamily.GroupLocations.Where( a => a.GroupLocationTypeValueId == form.PersonEntryGroupLocationTypeValueId.Value ).FirstOrDefault();

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
                                GroupLocationTypeValueId = form.PersonEntryGroupLocationTypeValueId.Value,
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
        /// Saves the person entry to attribute values.
        /// </summary>
        /// <param name="personEntryPersonId">The person entry person identifier.</param>
        /// <param name="personEntryPersonSpouseId">The person entry person spouse identifier.</param>
        /// <param name="primaryFamily">The primary family.</param>
        private void SavePersonEntryToAttributeValues( int personEntryPersonId, int? personEntryPersonSpouseId, Group primaryFamily )
        {
            var form = _actionType.WorkflowForm;
            var personAliasService = new PersonAliasService( new RockContext() );

            if ( form.PersonEntryPersonAttributeGuid.HasValue )
            {
                AttributeCache personEntryPersonAttribute = form.FormAttributes.Where( a => a.Attribute.Guid == form.PersonEntryPersonAttributeGuid.Value ).Select( a => a.Attribute ).FirstOrDefault();
                var item = GetWorkflowAttributeEntity( personEntryPersonAttribute );
                if ( item != null )
                {
                    var primaryAliasGuid = personAliasService.GetPrimaryAliasGuid( personEntryPersonId );
                    item.SetAttributeValue( personEntryPersonAttribute.Key, primaryAliasGuid );
                }
            }

            if ( form.PersonEntryFamilyAttributeGuid.HasValue )
            {
                AttributeCache personEntryFamilyAttribute = form.FormAttributes.Where( a => a.Attribute.Guid == form.PersonEntryFamilyAttributeGuid.Value ).Select( a => a.Attribute ).FirstOrDefault();
                var item = GetWorkflowAttributeEntity( personEntryFamilyAttribute );
                if ( item != null )
                {
                    item.SetAttributeValue( personEntryFamilyAttribute.Key, primaryFamily.Guid );
                }
            }

            if ( form.PersonEntrySpouseAttributeGuid.HasValue && personEntryPersonSpouseId.HasValue )
            {
                AttributeCache personEntrySpouseAttribute = form.FormAttributes.Where( a => a.Attribute.Guid == form.PersonEntrySpouseAttributeGuid.Value ).Select( a => a.Attribute ).FirstOrDefault();
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
            Person personEntryPerson = null;
            if ( existingPersonId.HasValue )
            {
                // Update Person from personEditor
                personEntryPerson = personService.Get( existingPersonId.Value );
                personEditor.UpdatePerson( personEntryPerson, rockContext );
                return personEntryPerson;
            }

            // Match or Create Person from personEditor
            var personMatchQuery = new PersonService.PersonMatchQuery( personEditor.FirstName, personEditor.LastName, personEditor.Email, personEditor.MobilePhoneNumber )
            {
                Gender = personEditor.PersonGender,
                BirthDate = personEditor.PersonBirthDate,
                SuffixValueId = personEditor.PersonSuffixValueId
            };

            bool updatePrimaryEmail = false;
            personEntryPerson = personService.FindPerson( personMatchQuery, updatePrimaryEmail );

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

            if ( personEntryPerson != null && limitMatchToFamily != null )
            {
                if ( personEntryPerson.PrimaryFamilyId != limitMatchToFamily.Id )
                {
                    personEntryPerson = null;
                }
            }

            if ( personEntryPerson != null )
            {
                // if a match was found, update that person
                personEditor.UpdatePerson( personEntryPerson, rockContext );
            }
            else
            {
                personEntryPerson = new Person();
                personEditor.UpdatePerson( personEntryPerson, rockContext );
            }

            return personEntryPerson;
        }

        /// <summary>
        /// Gets the form values.
        /// </summary>
        private void GetWorkflowFormAttributeValues()
        {
            if ( _workflow == null || _actionType == null )
            {
                return;
            }

            var form = _actionType.WorkflowForm;

            var values = new Dictionary<int, string>();
            var editableFormAttributes = form.FormAttributes.Where( a => a.IsVisible && !a.IsReadOnly ).OrderBy( a => a.Order );
            foreach ( WorkflowActionFormAttributeCache formAttribute in editableFormAttributes )
            {
                var attribute = AttributeCache.Get( formAttribute.AttributeId );
                var control = phAttributes.FindControl( string.Format( "attribute_field_{0}", formAttribute.AttributeId ) );

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

            Guid activityTypeGuid = Guid.Empty;
            string responseText = "Your information has been submitted successfully.";

            // If the selected action requires valid form data, trigger page validation and discontinue processing if there are any errors.
            var buttons = WorkflowActionFormUserAction.FromUriEncodedString( _actionType.WorkflowForm.Actions );

            var button = buttons.FirstOrDefault( x => x.ActionName == formAction );

            if ( button != null )
            {
                if ( button.CausesValidation )
                {
                    Page.Validate();

                    if ( !Page.IsValid )
                    {
                        return;
                    }
                }

                activityTypeGuid = button.ActivateActivityTypeGuid.AsGuid();

                if ( !string.IsNullOrWhiteSpace( button.ResponseText ) )
                {
                    responseText = button.ResponseText.ResolveMergeFields( mergeFields );
                }
            }

            _action.MarkComplete();
            _action.FormAction = formAction;
            _action.AddLogEntry( "Form Action Selected: " + _action.FormAction );

            if ( _action.ActionTypeCache.IsActivityCompletedOnSuccess )
            {
                _action.Activity.MarkComplete();
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

            var _workflowType = GetWorkflowType();

            if ( !activityTypeGuid.IsEmpty() )
            {
                var activityType = _workflowType.ActivityTypes.Where( a => a.Guid.Equals( activityTypeGuid ) ).FirstOrDefault();
                if ( activityType != null )
                {
                    WorkflowActivity.Activate( activityType, _workflow );
                }
            }

            // If the LastProcessedDateTime is equal to RockDateTime.Now we need to pause for a bit so the workflow will actually process here.
            // The resolution of System.DateTime.UTCNow is between .5 and 15 ms which can cause the workflow processing to not properly pick up
            // where it left off.
            // https://docs.microsoft.com/en-us/dotnet/api/system.datetime.utcnow?view=netframework-4.7#remarks
            while ( _workflow.LastProcessedDateTime == RockDateTime.Now )
            {
                System.Threading.Thread.Sleep( 1 );
            }

            List<string> errorMessages;

            var workflowProcessSuccess = _workflowService.Process( _workflow, out errorMessages );

            if ( _workflow.Id != 0 )
            {
                WorkflowId = _workflow.Id;
            }

            if ( !workflowProcessSuccess )
            {
                ShowMessage(
                    NotificationBoxType.Danger,
                    "Workflow Processing Error(s):",
                    "<ul><li>" + errorMessages.AsDelimited( "</li><li>", null, true ) + "</li></ul>" );
                return;
            }

            Guid? previousActionGuid = null;

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
                if ( lSummary.Text.IsNullOrWhiteSpace() )
                {
                    var hideForm = _action == null || _action.Guid != previousActionGuid;
                    ShowMessage( NotificationBoxType.Success, string.Empty, responseText, hideForm );
                }
                else
                {
                    pnlForm.Visible = false;
                }
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
                pnlForm.Visible = false;
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

        #endregion
    }
}
