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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Security;
using Rock.Workflow;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Used to enter information for a workflow form entry action.
    /// </summary>
    [DisplayName( "Workflow Entry" )]
    [Category( "Core" )]
    [Description( "Used to enter information for a workflow form entry action." )]

    [WorkflowTypeField("Workflow Type", "Type of workflow to start.")]
    public partial class WorkflowEntry : Rock.Web.UI.RockBlock
    {
        #region Fields

        private RockContext _rockContext = null;
        private WorkflowService _workflowService = null;
        private WorkflowTypeService _workflowTypeService = null;

        private WorkflowType _workflowType = null;
        private WorkflowActionType _actionType = null;
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
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);

            if (HydrateObjects())
            {
                BuildForm(false);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMessage.Visible = false;

            if (!Page.IsPostBack)
            {
                if (HydrateObjects())
                {
                    BuildForm( true );
                    ProcessActionRequest();
                }
            }
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

        }

        /// <summary>
        /// Handles the Click event of the lbAction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void lbAction_Click( object sender, EventArgs e )
        {
            GetFormValues();
            
            CompleteFormAction( sender as LinkButton );
        }

        #endregion

        #region Methods

        private bool HydrateObjects()
        {
            _rockContext = new RockContext();
            _workflowService = new WorkflowService( _rockContext );
            _workflowTypeService = new WorkflowTypeService( _rockContext );

            // Get the workflow type id (initial page request)
            if ( !WorkflowTypeId.HasValue )
            {
                // Get workflow type set by attribute value
                Guid workflowTypeguid = GetAttributeValue( "WorkflowType" ).AsGuid();
                if ( !workflowTypeguid.IsEmpty() )
                {
                    _workflowType = _workflowTypeService.Get( workflowTypeguid );
                }

                // If an attribute value was not provided, check for query/route value
                if ( _workflowType != null )
                {
                    WorkflowTypeId = _workflowType.Id;
                }
                else
                {
                    WorkflowTypeId = PageParameter( "WorkflowTypeId" ).AsIntegerOrNull();
                }
            }

            // Get the workflow type 
            if ( _workflowType == null && WorkflowTypeId.HasValue )
            {
                _workflowType = _workflowTypeService.Get( WorkflowTypeId.Value );
            }

            if ( _workflowType == null )
            {
                ShowMessage( NotificationBoxType.Danger, "Configuration Error", "Workflow type was not configured or specified correctly." );
                return false;
            }

            if ( !_workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
            {
                ShowMessage( NotificationBoxType.Warning, "Sorry", "You are not authorized to view this type of workflow." );
                return false;
            }

            // If operating against an existing workflow, get the workflow and load attributes
            if ( !WorkflowId.HasValue )
            {
                WorkflowId = PageParameter( "WorkflowId" ).AsIntegerOrNull();
                if ( !WorkflowId.HasValue )
                {
                    Guid guid = PageParameter( "WorkflowGuid" ).AsGuid();
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
                if (_workflow == null)
                {
                    _workflow = _workflowService.Queryable()
                        .Where( w => w.Id == WorkflowId.Value && w.WorkflowTypeId == _workflowType.Id )
                        .FirstOrDefault();
                }
                if ( _workflow != null )
                {
                    _workflow.LoadAttributes();
                    foreach(var activity in _workflow.Activities)
                    {
                        activity.LoadAttributes();
                    }
                }

            }

            // If an existing workflow was not specified, activate a new instance of workflow and start processing
            if ( _workflow == null )
            {
                _workflow = Rock.Model.Workflow.Activate( _workflowType, "Workflow" );

                List<string> errorMessages;
                if ( _workflow.Process( _rockContext, out errorMessages ) )
                {
                    // If the workflow type is persisted, save the workflow
                    if ( _workflow.IsPersisted || _workflowType.IsPersisted )
                    {
                        _workflowService.Add( _workflow );

                        RockTransactionScope.WrapTransaction( () =>
                        {
                            _rockContext.SaveChanges();
                            _workflow.SaveAttributeValues( _rockContext );
                        } );

                        WorkflowId = _workflow.Id;
                    }
                }
            }

            if ( _workflow == null )
            {
                ShowMessage( NotificationBoxType.Danger, "Workflow Activation Error", "Workflow could not be activated." );
                return false;
            }

            if ( ActionTypeId.HasValue )
            {
                foreach ( var activity in _workflow.ActiveActivities )
                {
                    _action = activity.Actions.Where( a => a.ActionTypeId == ActionTypeId.Value ).FirstOrDefault();
                    if ( _action != null )
                    {
                        _activity = activity;
                        _activity.LoadAttributes();

                        _actionType = _action.ActionType;
                        ActionTypeId = _actionType.Id;
                        return true;
                    }
                }
            }

            // Find first active action form
            foreach ( var activity in _workflow.ActiveActivities )
            {
                if ( activity.ActivityType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    foreach ( var action in activity.ActiveActions )
                    {
                        if ( action.ActionType.WorkflowForm != null && action.ActionType.WorkflowForm.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            _activity = activity;
                            _activity.LoadAttributes();

                            _action = action;
                            _actionType = _action.ActionType;
                            ActionTypeId = _actionType.Id;
                            return true;
                        }
                    }
                }
            }

            ShowMessage( NotificationBoxType.Warning, string.Empty, "The selected workflow is not in a state that requires you to enter information." );
            return false;

        }

        private void ProcessActionRequest()
        {
            string action = PageParameter( "action" );
            if (!string.IsNullOrWhiteSpace(action))
            {
                foreach( var linkButton in phActions.Controls.OfType<LinkButton>())
                {
                    if (linkButton.Text.Equals(action, StringComparison.OrdinalIgnoreCase))
                    {
                        CompleteFormAction( linkButton );
                    }
                }
            }
        }

        private void BuildForm(bool setValues)
        {
            var form = _actionType.WorkflowForm;

            if (setValues)
            {
                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Action", _action );
                mergeFields.Add( "Activity", _activity );
                mergeFields.Add( "Workflow", _workflow );

                lheadingText.Text = form.Header.ResolveMergeFields(mergeFields);
                lFootingText.Text = form.Footer.ResolveMergeFields(mergeFields);
            }

            phAttributes.Controls.Clear();
            foreach (var formAttribute in form.FormAttributes.OrderBy(a => a.Order))
            {
                if (formAttribute.IsVisible)
                {
                    var attribute = AttributeCache.Read(formAttribute.AttributeId);

                    string value = attribute.DefaultValue;
                    if (_workflow != null && _workflow.AttributeValues.ContainsKey(attribute.Key) && _workflow.AttributeValues[attribute.Key].Any())
                    {
                        value = _workflow.AttributeValues[attribute.Key][0].Value;
                    }

                    if (formAttribute.IsReadOnly)
                    {
                        RockLiteral lAttribute = new RockLiteral();
                        lAttribute.ID = "lAttribute_" + formAttribute.Id.ToString();
                        lAttribute.Label = formAttribute.Attribute.Name;
                        lAttribute.Text = attribute.FieldType.Field.FormatValue(phAttributes, value, attribute.QualifierValues, false);
                        phAttributes.Controls.Add(lAttribute);
                    }
                    else
                    {
                        attribute.AddControl(phAttributes.Controls, value, BlockValidationGroup, setValues, true, formAttribute.IsRequired);
                    }
                }
            }

            phActions.Controls.Clear();
            foreach (var action in form.Actions.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var details = action.Split(new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries);
                if (details.Length >= 1)
                {
                    var lb = new BootstrapButton();
                    lb.ID = "lb" + details[0];
                    lb.Text = details[0];
                    lb.Click += lbAction_Click;
                    lb.CssClass = "btn btn-primary";
                    if (details.Length >= 2)
                    {
                        lb.Attributes.Add("data-activity", details[1]);
                    }
                    lb.ValidationGroup = BlockValidationGroup;
                    phActions.Controls.Add(lb);

                    phActions.Controls.Add( new LiteralControl( " " ) );
                }
            }

        }

        private void GetFormValues()
        {
            if ( _workflow != null && _actionType != null )
            {
                var form = _actionType.WorkflowForm;

                var values = new Dictionary<int, string>();
                int i = 0;
                foreach ( var formAttribute in form.FormAttributes.OrderBy( a => a.Order ) )
                {
                    if ( formAttribute.IsVisible && !formAttribute.IsReadOnly )
                    {
                        var attribute = AttributeCache.Read( formAttribute.AttributeId );
                        if ( attribute != null )
                        {
                            IHasAttributes item = null;
                            if ( attribute.EntityTypeId == _workflow.TypeId )
                            {
                                item = _workflow;
                            }
                            else if ( attribute.EntityTypeId == _activity.TypeId )
                            {
                                item = _activity;
                            }

                            if (item != null)
                            {
                                item.SetAttributeValue( attribute.Key, attribute.FieldType.Field.GetEditValue( attribute.GetControl( phAttributes.Controls[i++] ), attribute.QualifierValues ) );
                            }
                        }
                    }
                }
            }
        }

        private void CompleteFormAction( LinkButton linkButton )
        {
            if ( linkButton != null )
            {
                string formAction = linkButton.Text;
                Guid activityTypeGuid = linkButton.Attributes["data-activity"].AsGuid();

                if ( !string.IsNullOrWhiteSpace( formAction ) &&
                    _workflow != null &&
                    _actionType != null &&
                    _activity != null &&
                    _action != null )
                {
                    _action.MarkComplete();
                    _action.FormAction = formAction;
                    _action.AddLogEntry( "Form Action Selected: " + _action.FormAction );

                    // save current activity form's actions (to formulate response if needed).
                    string currentActions = _actionType.WorkflowForm != null ? _actionType.WorkflowForm.Actions : string.Empty;
                    var mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "Action", _action );
                    mergeFields.Add( "Activity", _activity );
                    mergeFields.Add( "Workflow", _workflow );

                    if ( !activityTypeGuid.IsEmpty() )
                    {
                        var activityType = _workflowType.ActivityTypes.Where( a => a.Guid.Equals( activityTypeGuid ) ).FirstOrDefault();
                        if ( activityType != null )
                        {
                            WorkflowActivity.Activate( activityType, _workflow );
                        }
                    }

                    List<string> errorMessages;
                    if ( _workflow.Process( _rockContext, out errorMessages ) )
                    {
                        if ( _workflow.IsPersisted || _workflowType.IsPersisted )
                        {
                            if ( _workflow.Id == 0 )
                            {
                                _workflowService.Add( _workflow );
                            }

                            RockTransactionScope.WrapTransaction( () =>
                            {
                                _rockContext.SaveChanges();
                                _workflow.SaveAttributeValues( _rockContext );
                                foreach(var activity in _workflow.Activities)
                                {
                                    activity.SaveAttributeValues();
                                }
                            } );

                            WorkflowId = _workflow.Id;
                        }

                        ActionTypeId = null;
                        _action = null;
                        _actionType = null;
                        _activity = null;

                        if ( HydrateObjects() )
                        {
                            BuildForm( true );
                        }
                        else
                        {
                            string response = "Your information has been submitted succesfully.";

                            foreach ( var action in currentActions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
                            {
                                var details = action.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                                if ( details.Length >= 3 )
                                {
                                    if ( details[0] == formAction && !string.IsNullOrWhiteSpace( details[2] ) )
                                    {
                                        response = details[2].ResolveMergeFields( mergeFields );
                                        break;
                                    }
                                }
                            }

                            ShowMessage( NotificationBoxType.Success, string.Empty, response );
                        }
                    }
                    else
                    {
                        ShowMessage( NotificationBoxType.Danger, "Workflow Processing Error(s):", errorMessages.AsDelimited( "<br/>" ) );
                    }
                }
            }
        }

        private void ShowMessage(NotificationBoxType type, string title, string message)
        {
            nbMessage.NotificationBoxType = type;
            nbMessage.Title = title;
            nbMessage.Text = message;
            nbMessage.Visible = true;

            pnlForm.Visible = false;

        }
        #endregion
    }

}