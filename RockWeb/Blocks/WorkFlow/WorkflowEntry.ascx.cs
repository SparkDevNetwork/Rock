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
    /// Used to enter information for a workflow form entry action.
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
        Order = 2)]

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

        #region Fields

        private RockContext _rockContext = null;
        private WorkflowService _workflowService = null;

        private WorkflowTypeCache _workflowType = null;
        private WorkflowActionTypeCache _actionType = null;
        private Workflow _workflow = null;
        private WorkflowActivity _activity = null;
        private WorkflowAction _action = null;

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
            get { return ViewState["WorkflowTypeId"] as int?; }
            set { ViewState["WorkflowTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the workflow type was set by attribute.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [configured type]; otherwise, <c>false</c>.
        /// </value>
        public bool ConfiguredType
        {
            get { return ViewState["ConfiguredType"] as bool? ?? false; }
            set { ViewState["ConfiguredType"] = value; }
        }

        /// <summary>
        /// Gets or sets the workflow identifier.
        /// </summary>
        /// <value>
        /// The workflow identifier.
        /// </value>
        public int? WorkflowId
        {
            get { return ViewState["WorkflowId"] as int?; }
            set { ViewState["WorkflowId"] = value; }
        }

        /// <summary>
        /// Gets or sets the action type identifier.
        /// </summary>
        /// <value>
        /// The action type identifier.
        /// </value>
        public int? ActionTypeId
        {
            get { return ViewState["ActionTypeId"] as int?; }
            set { ViewState["ActionTypeId"] = value; }
        }

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            if ( HydrateObjects() )
            {
                BuildForm( false );
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
                    BuildForm( true );
                    ProcessActionRequest();
                }
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            LoadWorkflowType();

            if ( _workflowType != null && !ConfiguredType )
            {
                breadCrumbs.Add( new BreadCrumb( _workflowType.Name, pageReference ) );
            }

            return breadCrumbs;
        }

        protected override void Render( HtmlTextWriter writer )
        {
            base.Render( writer );
        }
        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetBlockTitle();
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            GetFormValues();
            CompleteFormAction( eventArgument );
        }

        #endregion

        #region Methods

        private bool HydrateObjects()
        {
            LoadWorkflowType();

            // Set the note type if this is first request
            if ( !Page.IsPostBack )
            {
                var entityType = EntityTypeCache.Get( typeof( Rock.Model.Workflow ) );
                var noteTypes = NoteTypeCache.GetByEntity( entityType.Id, string.Empty, string.Empty );
                ncWorkflowNotes.NoteOptions.SetNoteTypes( noteTypes );
            }

            if ( _workflowType == null )
            {
                ShowNotes( false );
                ShowMessage( NotificationBoxType.Danger, "Configuration Error", "Workflow type was not configured or specified correctly." );
                return false;
            }

            if ( !_workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                ShowNotes( false );
                ShowMessage( NotificationBoxType.Warning, "Sorry", "You are not authorized to view this type of workflow." );
                return false;
            }

            if ( !( _workflowType.IsActive ?? true ) )
            {
                ShowNotes( false );
                ShowMessage( NotificationBoxType.Warning, "Sorry", "This type of workflow is not active." );
                return false;
            }

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
                            .Where( w => w.Guid.Equals( guid ) && w.WorkflowTypeId == _workflowType.Id )
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
                        .Where( w => w.Id == WorkflowId.Value && w.WorkflowTypeId == _workflowType.Id )
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
                    workflowName = "New " + _workflowType.WorkTerm;
                }

                _workflow = Rock.Model.Workflow.Activate( _workflowType, workflowName );
                if ( _workflow != null )
                {
                    // If a PersonId or GroupId parameter was included, load the corresponding
                    // object and pass that to the actions for processing
                    object entity = null;
                    int? personId = PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        entity = new PersonService( _rockContext ).Get( personId.Value );
                    }
                    else
                    {
                        int? groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                        if ( groupId.HasValue )
                        {
                            entity = new GroupService( _rockContext ).Get( groupId.Value );
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
                        ShowMessage( NotificationBoxType.Danger, "Workflow Processing Error(s):",
                            "<ul><li>" + errorMessages.AsDelimited( "</li><li>" ) + "</li></ul>" );
                        return false;
                    }
                    if ( _workflow.Id != 0 )
                    {
                        WorkflowId = _workflow.Id;
                    }
                }
            }

            if ( _workflow == null )
            {
                ShowNotes( false );
                ShowMessage( NotificationBoxType.Danger, "Workflow Activation Error", "Workflow could not be activated." );
                return false;
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
                int? actionId = PageParameter( PageParameterKey.ActionId ).AsIntegerOrNull();
                foreach ( var activity in _workflow.Activities
                    .Where( a =>
                        a.IsActive &&
                        ( !actionId.HasValue || a.Actions.Any( ac => ac.Id == actionId.Value ) ) &&
                        (
                            ( canEdit ) ||
                            ( !a.AssignedGroupId.HasValue && !a.AssignedPersonAliasId.HasValue ) ||
                            ( a.AssignedPersonAlias != null && a.AssignedPersonAlias.PersonId == personId ) ||
                            ( a.AssignedGroup != null && a.AssignedGroup.Members.Any( m => m.PersonId == personId ) )
                        )
                    )
                    .ToList()
                    .OrderBy( a => a.ActivityTypeCache.Order ) )
                {
                    if ( canEdit || ( activity.ActivityTypeCache.IsAuthorized( Authorization.VIEW, CurrentPerson ) ) )
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
                if ( GetAttributeValue( AttributeKey.ShowSummaryView ).AsBoolean() && !string.IsNullOrWhiteSpace( _workflowType.SummaryViewText ) )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( "Action", _action );
                    mergeFields.Add( "Activity", _activity );
                    mergeFields.Add( "Workflow", _workflow );

                    lSummary.Text = _workflowType.SummaryViewText.ResolveMergeFields( mergeFields, CurrentPerson );
                    lSummary.Visible = true;
                }
            }

            if ( lSummary.Text.IsNullOrWhiteSpace() )
            {
                if ( _workflowType.NoActionMessage.IsNullOrWhiteSpace() )
                {
                    ShowMessage( NotificationBoxType.Warning, string.Empty, "The selected workflow is not in a state that requires you to enter information." );
                }
                else
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( "Action", _action );
                    mergeFields.Add( "Activity", _activity );
                    mergeFields.Add( "Workflow", _workflow );
                    ShowMessage( NotificationBoxType.Warning, string.Empty, _workflowType.NoActionMessage.ResolveMergeFields( mergeFields, CurrentPerson ) );
                }
            }

            ShowNotes( false );
            return false;
        }

        private void LoadWorkflowType()
        {
            if ( _rockContext == null )
            {
                _rockContext = new RockContext();
            }

            if ( _workflowService == null )
            {
                _workflowService = new WorkflowService( _rockContext );
            }

            // Get the workflow type id (initial page request)
            if ( !WorkflowTypeId.HasValue )
            {
                // Get workflow type set by attribute value
                Guid workflowTypeguid = GetAttributeValue( AttributeKey.WorkflowType ).AsGuid();
                if ( !workflowTypeguid.IsEmpty() )
                {
                    _workflowType = WorkflowTypeCache.Get( workflowTypeguid );
                }

                // If an attribute value was not provided, check for query/route value
                if ( _workflowType != null )
                {
                    WorkflowTypeId = _workflowType.Id;
                    ConfiguredType = true;
                }
                else
                {
                    WorkflowTypeId = PageParameter( PageParameterKey.WorkflowTypeId ).AsIntegerOrNull();
                    ConfiguredType = false;
                }
            }

            // Get the workflow type 
            if ( _workflowType == null && WorkflowTypeId.HasValue )
            {
                _workflowType = WorkflowTypeCache.Get( WorkflowTypeId.Value );
            }
        }

        private void ProcessActionRequest()
        {
            string action = PageParameter( PageParameterKey.Command );
            if ( !string.IsNullOrWhiteSpace( action ) )
            {
                CompleteFormAction( action );
            }
        }

        private void BuildForm( bool setValues )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "Action", _action );
            mergeFields.Add( "Activity", _activity );
            mergeFields.Add( "Workflow", _workflow );

            var form = _actionType.WorkflowForm;

            if ( setValues )
            {
                lheadingText.Text = form.Header.ResolveMergeFields( mergeFields );
                lFootingText.Text = form.Footer.ResolveMergeFields( mergeFields );
            }

            if ( _workflow != null && _workflow.CreatedDateTime.HasValue )
            {
                hlblDateAdded.Text = String.Format( "Added: {0}", _workflow.CreatedDateTime.Value.ToShortDateString() );
            }
            else
            {
                hlblDateAdded.Visible = false;
            }

            phAttributes.Controls.Clear();

            foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
            {
                if ( formAttribute.IsVisible )
                {
                    var attribute = AttributeCache.Get( formAttribute.AttributeId );

                    string value = attribute.DefaultValue;
                    if ( _workflow != null && _workflow.AttributeValues.ContainsKey( attribute.Key ) && _workflow.AttributeValues[attribute.Key] != null )
                    {
                        value = _workflow.AttributeValues[attribute.Key].Value;
                    }
                    // Now see if the key is in the activity attributes so we can get it's value
                    else if ( _activity != null && _activity.AttributeValues.ContainsKey( attribute.Key ) && _activity.AttributeValues[attribute.Key] != null )
                    {
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
                            formattedValue = attribute.FieldType.Field.FormatValueAsHtml( phAttributes, attribute.EntityTypeId, _activity.Id, value, attribute.QualifierValues, true );
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
                        attribute.AddControl( phAttributes.Controls, value, BlockValidationGroup, setValues, true, formAttribute.IsRequired,
                            ( formAttribute.HideLabel ? string.Empty : attribute.Name ) );
                    }

                    if ( !string.IsNullOrWhiteSpace( formAttribute.PostHtml ) )
                    {
                        phAttributes.Controls.Add( new LiteralControl( formAttribute.PostHtml.ResolveMergeFields( mergeFields ) ) );
                    }

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
            foreach ( var action in form.Actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                var details = action.Split( new char[] { '^' } );
                if ( details.Length > 0 )
                {
                    // Get the button html
                    string buttonHtml = string.Empty;
                    if ( details.Length > 1 )
                    {
                        var definedValue = DefinedValueCache.Get( details[1].AsGuid() );
                        if ( definedValue != null )
                        {
                            buttonHtml = definedValue.GetAttributeValue( "ButtonHTML" );
                        }
                    }

                    if ( string.IsNullOrWhiteSpace( buttonHtml ) )
                    {
                        buttonHtml = "<a href=\"{{ ButtonLink }}\" onclick=\"{{ ButtonClick }}\" class='btn btn-primary' data-loading-text='<i class=\"fa fa-refresh fa-spin\"></i> {{ ButtonText }}'>{{ ButtonText }}</a>";
                    }

                    var buttonMergeFields = new Dictionary<string, object>();
                    buttonMergeFields.Add( "ButtonText", details[0].EncodeHtml() );
                    buttonMergeFields.Add( "ButtonClick",
                            string.Format( "if ( Page_ClientValidate('{0}') ) {{ $(this).button('loading'); return true; }} else {{ return false; }}",
                            BlockValidationGroup ) );
                    buttonMergeFields.Add( "ButtonLink", Page.ClientScript.GetPostBackClientHyperlink( this, details[0] ) );

                    buttonHtml = buttonHtml.ResolveMergeFields( buttonMergeFields );

                    phActions.Controls.Add( new LiteralControl( buttonHtml ) );
                    phActions.Controls.Add( new LiteralControl( " " ) );
                }
            }

        }

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

        private void GetFormValues()
        {
            if ( _workflow != null && _actionType != null )
            {
                var form = _actionType.WorkflowForm;

                var values = new Dictionary<int, string>();
                foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
                {
                    if ( formAttribute.IsVisible && !formAttribute.IsReadOnly )
                    {
                        var attribute = AttributeCache.Get( formAttribute.AttributeId );
                        var control = phAttributes.FindControl( string.Format( "attribute_field_{0}", formAttribute.AttributeId ) );

                        if ( attribute != null && control != null )
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

                            if ( item != null )
                            {
                                item.SetAttributeValue( attribute.Key, attribute.FieldType.Field.GetEditValue( attribute.GetControl( control ), attribute.QualifierValues ) );
                            }
                        }
                    }
                }
            }
        }

        private void CompleteFormAction( string formAction )
        {
            if ( !string.IsNullOrWhiteSpace( formAction ) &&
                _workflow != null &&
                _actionType != null &&
                _actionType.WorkflowForm != null &&
                _activity != null &&
                _action != null )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "Action", _action );
                mergeFields.Add( "Activity", _activity );
                mergeFields.Add( "Workflow", _workflow );

                Guid activityTypeGuid = Guid.Empty;
                string responseText = "Your information has been submitted successfully.";

                foreach ( var action in _actionType.WorkflowForm.Actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    var actionDetails = action.Split( new char[] { '^' } );
                    if ( actionDetails.Length > 0 && actionDetails[0] == formAction )
                    {
                        if ( actionDetails.Length > 2 )
                        {
                            activityTypeGuid = actionDetails[2].AsGuid();
                        }

                        if ( actionDetails.Length > 3 && !string.IsNullOrWhiteSpace( actionDetails[3] ) )
                        {
                            responseText = actionDetails[3].ResolveMergeFields( mergeFields );
                        }
                        break;
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
                        Rock.Attribute.IHasAttributes item = null;
                        if ( attribute.EntityTypeId == _workflow.TypeId )
                        {
                            item = _workflow;
                        }
                        else if ( attribute.EntityTypeId == _activity.TypeId )
                        {
                            item = _activity;
                        }

                        if ( item != null )
                        {
                            item.SetAttributeValue( attribute.Key, formAction );
                        }
                    }
                }

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
                if ( _workflowService.Process( _workflow, out errorMessages ) )
                {
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
                            ShowMessage( NotificationBoxType.Success, string.Empty, responseText, ( _action == null || _action.Guid != previousActionGuid ) );
                        }
                        else
                        {
                            pnlForm.Visible = false;
                        }
                    }
                }
                else
                {
                    ShowMessage( NotificationBoxType.Danger, "Workflow Processing Error(s):",
                        "<ul><li>" + errorMessages.AsDelimited( "</li><li>", null, true ) + "</li></ul>" );
                }
                if ( _workflow.Id != 0 )
                {
                    WorkflowId = _workflow.Id;
                }
            }
        }

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

            if ( !string.IsNullOrWhiteSpace( blockTitle ) )
            {
                // Resolve the block title using the specified Lava template.
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );

                mergeFields.Add( "WorkflowType", _workflowType );

                // Add the WorkflowType as the default Item.
                mergeFields.Add( "Item", _workflowType );

                blockTitle = blockTitle.ResolveMergeFields( mergeFields );
            }

            // If the block title is not configured, use the Workflow Type if it is available.
            if ( string.IsNullOrWhiteSpace( blockTitle ) )
            {
                if ( _workflowType != null )
                {
                    blockTitle = string.Format( "{0} Entry", _workflowType.WorkTerm );
                }
                else
                {
                    blockTitle = "Workflow Entry";
                }
            }

            lTitle.Text = blockTitle;

            // Set the Page Title to the Workflow Type name, unless the Workflow Type has been specified by a configuration setting.
            if ( _workflowType != null && !ConfiguredType )
            {
                RockPage.PageTitle = _workflowType.Name;
            }

            // Set the Block Icon.
            var blockTitleIconCssClass = GetAttributeValue( AttributeKey.BlockTitleIconCssClass );

            if ( string.IsNullOrWhiteSpace( blockTitleIconCssClass ) )
            {
                if ( _workflowType != null )
                {
                    blockTitleIconCssClass = _workflowType.IconCssClass;
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
