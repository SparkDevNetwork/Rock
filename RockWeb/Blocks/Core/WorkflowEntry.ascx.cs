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
        private WorkflowAction _action = null;

        #endregion

        #region Properties

        public int? WorkflowTypeId
        {
            get { return ViewState["WorkflowTypeId"] as int?; }
            set { ViewState["WorkflowTypeId"] = value; }
        }

        public int? WorkflowId
        {
            get { return ViewState["WorkflowId"] as int?; }
            set { ViewState["WorkflowId"] = value; }
        }

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
                    BuildForm(true);
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

        void lbAction_Click(object sender, EventArgs e)
        {
            var lb = sender as LinkButton;
            if (lb != null && _workflow != null && _actionType != null && _action != null)
            {
                GetFormValues();

                _action.MarkComplete();
                _action.FormAction = lb.Text;

                Guid activityTypeGuid = lb.Attributes["data-activity"].AsGuid();
                if (!activityTypeGuid.IsEmpty())
                {
                    var activityType = _workflowType.ActivityTypes.Where(a => a.Guid.Equals(activityTypeGuid)).FirstOrDefault();
                    if (activityType != null)
                    {
                        WorkflowActivity.Activate(activityType, _workflow);
                    }
                }

                List<string> errorMessages;
                if (_workflow.Process(out errorMessages))
                {
                    if ( _workflow.IsPersisted || _workflowType.IsPersisted )
                    {
                        if (_workflow.Id == 0)
                        {
                            _workflowService.Add(_workflow);
                        }
                        _rockContext.SaveChanges();
                        WorkflowId = _workflow.Id;
                    }

                    ActionTypeId = null;
                    _action = null;
                    _actionType = null;

                    if (HydrateObjects())
                    {
                        BuildForm(true);
                    }
                }
                else
                {
                    nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                    nbMessage.Title = "Workflow Processing Error(s):";
                    nbMessage.Text = errorMessages.AsDelimited("<br/>");
                    nbMessage.Visible = true;
                }

            }
        }

        #endregion

        #region Methods

        private bool HydrateObjects()
        {
            _rockContext = new RockContext();
            _workflowService = new WorkflowService(_rockContext);
            _workflowTypeService = new WorkflowTypeService(_rockContext);

            // Get the workflow type id (initial page request)
            if (!WorkflowTypeId.HasValue)
            {
                // Get workflow type set by attribute value
                Guid workflowTypeguid = GetAttributeValue("WorkflowType").AsGuid();
                if (!workflowTypeguid.IsEmpty())
                {
                    _workflowType = _workflowTypeService.Get(workflowTypeguid);
                }

                // If an attribute value was not provided, check for query/route value
                if (_workflowType != null)
                {
                    WorkflowTypeId = _workflowType.Id;
                }
                else
                {
                    WorkflowTypeId = PageParameter("WorkflowTypeId").AsInteger(false);
                }
            }

            // Get the workflow type 
            if (_workflowType == null && WorkflowTypeId.HasValue)
            {
                _workflowType = _workflowTypeService.Get(WorkflowTypeId.Value);
            }

            if (_workflowType != null)
            {
                // If operating against an existing workflow, get the workflow and load attributes
                if (!WorkflowId.HasValue)
                {
                    WorkflowId = PageParameter("WorkflowId").AsInteger(false);
                }

                if (WorkflowId.HasValue)
                {
                    _workflow = _workflowService.Queryable()
                        .Where(w => w.Id == WorkflowId.Value && w.WorkflowTypeId == _workflowType.Id)
                        .FirstOrDefault();
                    if (_workflow != null)
                    {
                        _workflow.LoadAttributes();
                    }
                }

                // If an existing workflow was not specified, activate a new instance of workflow and start processing
                if (_workflow == null)
                {
                    _workflow = Rock.Model.Workflow.Activate(_workflowType, "Workflow");

                    List<string> errorMessages;
                    if (_workflow.Process(out errorMessages))
                    {
                        // If the workflow type is persisted, save the workflow
                        if ( _workflow.IsPersisted || _workflowType.IsPersisted )
                        {
                            _workflowService.Add(_workflow);
                            _rockContext.SaveChanges();
                            WorkflowId = _workflow.Id;
                        }
                    }
                }

                if (_workflow != null)
                {
                    if (ActionTypeId.HasValue)
                    {
                        foreach (var activity in _workflow.Activities)
                        {
                            _action = activity.Actions.Where(a => a.ActionTypeId == ActionTypeId.Value).FirstOrDefault();
                            if (_action != null)
                            {
                                _actionType = _action.ActionType;
                                return true;
                            }
                        }
                    }

                    // Find first active action form
                    foreach (var activity in _workflow.ActiveActivities)
                    {
                        foreach (var action in activity.ActiveActions)
                        {
                            if (action.ActionType.WorkflowForm != null && action.ActionType.WorkflowForm.IsAuthorized(Authorization.VIEW, CurrentPerson))
                            {
                                _action = action;
                                _actionType = _action.ActionType;
                                ActionTypeId = _actionType.Id;
                                return true;
                            }
                        }
                    }

                    if (_actionType == null)
                    {
                        ShowMessage(NotificationBoxType.Warning, string.Empty, "No entry required.");
                    }
                }
                else
                {
                    ShowMessage(NotificationBoxType.Danger, "Workflow Activation Error", "Workflow could not be activated.");
                }
            }
            else
            {
                ShowMessage(NotificationBoxType.Danger, "Configuration Error", "Workflow type was not configured or specified correctly.");
            }

            return false;
        }


        private void BuildForm(bool setValues)
        {
            var form = _actionType.WorkflowForm;

            if (setValues)
            {
                var mergeFields = (Dictionary<string, object>)_workflow.ToLiquid(false, false);
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
                    LinkButton lb = new LinkButton();
                    lb.Text = details[0];
                    lb.Click += lbAction_Click;
                    lb.CssClass = "btn btn-primary";
                    if (details.Length == 2)
                    {
                        lb.Attributes.Add("data-activity", details[1]);
                    }
                    lb.ValidationGroup = BlockValidationGroup;
                    phActions.Controls.Add(lb);
                }
            }

        }

        private void GetFormValues()
        {
            var form = _actionType.WorkflowForm;

            var values = new Dictionary<int, string>();
            int i = 0;
            foreach (var formAttribute in form.FormAttributes.OrderBy(a => a.Order))
            {
                if (formAttribute.IsVisible && !formAttribute.IsReadOnly)
                {
                    var attribute = AttributeCache.Read(formAttribute.AttributeId);
                    if (attribute != null)
                    {
                        _workflow.SetAttributeValue(attribute.Key, attribute.FieldType.Field.GetEditValue(attribute.GetControl(phAttributes.Controls[i++]), attribute.QualifierValues));
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