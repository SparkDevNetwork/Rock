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
using System.Linq;
using System.Web.UI;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.WorkFlow
{
    [DisplayName( "Workflow Type Detail" )]
    [Category( "WorkFlow" )]
    [Description( "Displays the details of the given workflow type." )]

    [LinkedPage("Workflow Launch Page", "Page used to launch a workflow.")]
    [LinkedPage( "Manage Workflows Page", "Page used to manage workflows." )]
    public partial class WorkflowTypeDetail : RockBlock
    {
        #region Properties

        private List<Attribute> AttributesState { get; set; }
        private List<WorkflowActivityType> ActivityTypesState { get; set; }
        private Dictionary<Guid, List<Attribute>> ActivityAttributesState { get; set; }
        private List<Guid> ExpandedActivities { get; set; }
        private List<Guid> ExpandedActivityAttributes { get; set; }
        private List<Guid> ExpandedActions { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["AttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                AttributesState = new List<Attribute>();
            }
            else
            {
                AttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }

            json = ViewState["ActivityTypesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ActivityTypesState = new List<WorkflowActivityType>();
            }
            else
            {
                ActivityTypesState = JsonConvert.DeserializeObject<List<WorkflowActivityType>>( json );
            }

            json = ViewState["ActivityAttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                ActivityAttributesState = new Dictionary<Guid, List<Attribute>>();
            }
            else
            {
                ActivityAttributesState = JsonConvert.DeserializeObject<Dictionary<Guid, List<Attribute>>>( json );
            }

            ExpandedActivities = ViewState["ExpandedActivities"] as List<Guid>;
            if (ExpandedActivities == null)
            {
                ExpandedActivities = new List<Guid>();
            }

            ExpandedActivityAttributes = ViewState["ExpandedActivityAttributes"] as List<Guid>;
            if ( ExpandedActivityAttributes == null )
            {
                ExpandedActivityAttributes = new List<Guid>();
            }

            ExpandedActions = ViewState["ExpandedActions"] as List<Guid>;
            if ( ExpandedActions == null )
            {
                ExpandedActions = new List<Guid>();
            }

            BuildControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // assign attributes grid actions
            gAttributes.AddCssClass( "attribute-grid" );
            gAttributes.DataKeyNames = new string[] { "Guid" };
            gAttributes.Actions.ShowAdd = true;
            gAttributes.Actions.AddClick += gAttributes_Add;
            gAttributes.GridRebind += gAttributes_GridRebind;
            gAttributes.GridReorder += gAttributes_GridReorder;

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'This will also delete all the workflows of this type!');", WorkflowType.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.WorkflowType ) ).Id;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail();
            }
            else
            {
                ShowDialog();

                string postbackArgs = Request.Params["__EVENTARGUMENT"];
                if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
                {
                    string[] nameValue = postbackArgs.Split( new char[] { ':' } );
                    if ( nameValue.Count() == 2 )
                    {
                        string[] values = nameValue[1].Split( new char[] { ';' } );
                        if ( values.Count() == 2 )
                        {
                            Guid guid = values[0].AsGuid();
                            int newIndex = values[1].AsInteger();

                            switch ( nameValue[0] )
                            {
                                case "re-order-activity":
                                    {
                                        SortActivities( guid, newIndex );
                                        break;
                                    }
                                case "re-order-action":
                                    {
                                        SortActions( guid, newIndex );
                                        break;
                                    }
                                case "re-order-formfield":
                                    {
                                        SortFormFields( guid, newIndex );
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings 
            { 
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["AttributesState"] = JsonConvert.SerializeObject( AttributesState, Formatting.None, jsonSetting );
            ViewState["ActivityTypesState"] = JsonConvert.SerializeObject( ActivityTypesState, Formatting.None, jsonSetting );
            ViewState["ActivityAttributesState"] = JsonConvert.SerializeObject( ActivityAttributesState, Formatting.None, jsonSetting );
            ViewState["ExpandedActivities"] = ExpandedActivities;
            ViewState["ExpandedActivityAttributes"] = ExpandedActivityAttributes;
            ViewState["ExpandedActions"] = ExpandedActions;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        #region Edit  events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var workflowType = new WorkflowTypeService( rockContext ).Get( hfWorkflowTypeId.Value.AsInteger() );

            LoadStateDetails( workflowType, rockContext );
            ShowEditDetails( workflowType, rockContext );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var service = new WorkflowTypeService( rockContext );
            var workflowType = service.Get( int.Parse( hfWorkflowTypeId.Value ) );

            if ( workflowType != null )
            {
                if ( !workflowType.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this workflow type.", ModalAlertType.Information );
                    return;
                }

                service.Delete( workflowType );

                rockContext.SaveChanges();
            }

            // reload page
            var qryParams = new Dictionary<string, string>();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCopy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var workflowType = new WorkflowTypeService( rockContext ).Get( hfWorkflowTypeId.Value.AsInteger() );

            if ( workflowType != null )
            {
                // Load the state objects for the source workflow type
                LoadStateDetails( workflowType, rockContext );

                // clone the workflow type
                var newWorkflowType = workflowType.Clone( false );
                newWorkflowType.CreatedByPersonAlias = null;
                newWorkflowType.CreatedByPersonAliasId = null;
                newWorkflowType.CreatedDateTime = RockDateTime.Now;
                newWorkflowType.ModifiedByPersonAlias = null;
                newWorkflowType.ModifiedByPersonAliasId = null;
                newWorkflowType.ModifiedDateTime = RockDateTime.Now;
                newWorkflowType.Id = 0;
                newWorkflowType.Guid = Guid.NewGuid();
                newWorkflowType.IsSystem = false;
                newWorkflowType.Name = workflowType.Name + " - Copy";

                // Create temporary state objects for the new workflow type
                var newAttributesState = new List<Attribute>();
                var newActivityTypesState = new List<WorkflowActivityType>();
                var newActivityAttributesState = new Dictionary<Guid, List<Attribute>>();

                // Dictionary to keep the attributes and activity types linked between the source and the target based on their guids
                var guidXref = new Dictionary<Guid, Guid>();

                // Clone the workflow attributes
                foreach ( var attribute in AttributesState )
                {
                    var newAttribute = attribute.Clone( false );
                    newAttribute.Id = 0;
                    newAttribute.Guid = Guid.NewGuid();
                    newAttribute.IsSystem = false;
                    newAttributesState.Add( newAttribute );

                    guidXref.Add( attribute.Guid, newAttribute.Guid );

                    foreach ( var qualifier in attribute.AttributeQualifiers )
                    {
                        var newQualifier = qualifier.Clone( false );
                        newQualifier.Id = 0;
                        newQualifier.Guid = Guid.NewGuid();
                        newQualifier.IsSystem = false;
                        newAttribute.AttributeQualifiers.Add( qualifier );

                        guidXref.Add( qualifier.Guid, newQualifier.Guid );
                    }
                }

                // Create new guids for all the existing activity types
                foreach (var activityType in ActivityTypesState)
                {
                    guidXref.Add( activityType.Guid, Guid.NewGuid() );
                }

                foreach ( var activityType in ActivityTypesState )
                {
                    var newActivityType = activityType.Clone( false );
                    newActivityType.WorkflowTypeId = 0;
                    newActivityType.Id = 0;
                    newActivityType.Guid = guidXref[activityType.Guid];
                    newActivityTypesState.Add( newActivityType );

                    var newActivityAttributes = new List<Attribute>();
                    foreach ( var attribute in ActivityAttributesState[activityType.Guid] )
                    {
                        var newAttribute = attribute.Clone( false );
                        newAttribute.Id = 0;
                        newAttribute.Guid = Guid.NewGuid();
                        newAttribute.IsSystem = false;
                        newActivityAttributes.Add( newAttribute );

                        guidXref.Add( attribute.Guid, newAttribute.Guid );

                        foreach ( var qualifier in attribute.AttributeQualifiers )
                        {
                            var newQualifier = qualifier.Clone( false );
                            newQualifier.Id = 0;
                            newQualifier.Guid = Guid.NewGuid();
                            newQualifier.IsSystem = false;
                            newAttribute.AttributeQualifiers.Add( qualifier );

                            guidXref.Add( qualifier.Guid, newQualifier.Guid );
                        }
                    }
                    newActivityAttributesState.Add( newActivityType.Guid, newActivityAttributes );

                    foreach ( var actionType in activityType.ActionTypes )
                    {
                        var newActionType = actionType.Clone( false );
                        newActionType.ActivityTypeId = 0;
                        newActionType.WorkflowFormId = 0;
                        newActionType.Id = 0;
                        newActionType.Guid = Guid.NewGuid();
                        newActivityType.ActionTypes.Add( newActionType );

                        if ( actionType.CriteriaAttributeGuid.HasValue &&
                            guidXref.ContainsKey( actionType.CriteriaAttributeGuid.Value ) )
                        {
                            newActionType.CriteriaAttributeGuid = guidXref[actionType.CriteriaAttributeGuid.Value];
                        }
                        Guid criteriaAttributeGuid = actionType.CriteriaValue.AsGuid();
                        if ( !criteriaAttributeGuid.IsEmpty() &&
                            guidXref.ContainsKey( criteriaAttributeGuid ) )
                        {
                            newActionType.CriteriaValue = guidXref[criteriaAttributeGuid].ToString();
                        }

                        if ( actionType.WorkflowForm != null )
                        {
                            var newWorkflowForm = actionType.WorkflowForm.Clone( false );
                            newWorkflowForm.Id = 0;
                            newWorkflowForm.Guid = Guid.NewGuid();

                            var newActionButtons = new List<string>();
                            foreach ( var actionButton in actionType.WorkflowForm.Actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
                            {
                                var details = actionButton.Split( new char[] { '^' } );
                                if ( details.Length > 2 )
                                {
                                    Guid oldGuid = details[2].AsGuid();
                                    if (!oldGuid.IsEmpty() && guidXref.ContainsKey(oldGuid))
                                    {
                                        details[2] = guidXref[oldGuid].ToString();
                                    }
                                }
                                newActionButtons.Add( details.ToList().AsDelimited("^") );
                            }
                            newWorkflowForm.Actions = newActionButtons.AsDelimited( "|" );

                            if ( actionType.WorkflowForm.ActionAttributeGuid.HasValue &&
                                guidXref.ContainsKey( actionType.WorkflowForm.ActionAttributeGuid.Value ) )
                            {
                                newWorkflowForm.ActionAttributeGuid = guidXref[actionType.WorkflowForm.ActionAttributeGuid.Value];
                            }
                            newActionType.WorkflowForm = newWorkflowForm;

                            foreach ( var formAttribute in actionType.WorkflowForm.FormAttributes )
                            {
                                if ( guidXref.ContainsKey( formAttribute.Attribute.Guid ) )
                                {
                                    var newFormAttribute = formAttribute.Clone( false );
                                    newFormAttribute.WorkflowActionFormId = 0;
                                    newFormAttribute.Id = 0;
                                    newFormAttribute.Guid = Guid.NewGuid();
                                    newFormAttribute.AttributeId = 0;

                                    newFormAttribute.Attribute = new Rock.Model.Attribute
                                        {
                                            Guid = guidXref[formAttribute.Attribute.Guid],
                                            Name = formAttribute.Attribute.Name
                                        };
                                    newWorkflowForm.FormAttributes.Add( newFormAttribute );
                                }
                            }
                        }

                        newActionType.LoadAttributes( rockContext );
                        if ( actionType.Attributes != null && actionType.Attributes.Any() )
                        {
                            foreach ( var attributeKey in actionType.Attributes.Select( a => a.Key ) )
                            {
                                string value = actionType.GetAttributeValue( attributeKey );
                                Guid guidValue = value.AsGuid();
                                if ( !guidValue.IsEmpty() && guidXref.ContainsKey( guidValue ) )
                                {
                                    newActionType.SetAttributeValue( attributeKey, guidXref[guidValue].ToString() );
                                }
                                else
                                {
                                    newActionType.SetAttributeValue( attributeKey, value );
                                }
                            }
                        }
                    }
                }

                workflowType = newWorkflowType;
                AttributesState = newAttributesState;
                ActivityTypesState = newActivityTypesState;
                ActivityAttributesState = newActivityAttributesState;

                hfWorkflowTypeId.Value = workflowType.Id.ToString();
                ShowEditDetails( workflowType, rockContext );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnLaunch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnLaunch_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "WorkflowTypeId", hfWorkflowTypeId.Value );
            NavigateToLinkedPage( "WorkflowLaunchPage", qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnManage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnManage_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "WorkflowTypeId", hfWorkflowTypeId.Value );
            NavigateToLinkedPage( "ManageWorkflowsPage", qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            ParseControls( true );

            var rockContext = new RockContext();
            var service = new WorkflowTypeService( rockContext );

            WorkflowType workflowType = null;

            int? workflowTypeId = hfWorkflowTypeId.Value.AsIntegerOrNull();
            if ( workflowTypeId.HasValue )
            {
                workflowType = service.Get( workflowTypeId.Value );
            }

            if ( workflowType == null )
            {
                workflowType = new WorkflowType();
            }

            workflowType.IsActive = cbIsActive.Checked;
            workflowType.Name = tbName.Text;
            workflowType.Description = tbDescription.Text;
            workflowType.CategoryId = cpCategory.SelectedValueAsInt();
            workflowType.Order = 0;
            workflowType.WorkTerm = tbWorkTerm.Text;
            workflowType.ProcessingIntervalSeconds = tbProcessingInterval.Text.AsIntegerOrNull();
            workflowType.IsPersisted = cbIsPersisted.Checked;
            workflowType.LoggingLevel = ddlLoggingLevel.SelectedValueAsEnum<WorkflowLoggingLevel>();
            workflowType.IconCssClass = tbIconCssClass.Text;

            if ( !Page.IsValid || !workflowType.IsValid )
            {
                return;
            }

            foreach(var activityType in ActivityTypesState)
            {
                if (!activityType.IsValid)
                {
                    return;
                }
                foreach(var actionType in activityType.ActionTypes)
                {
                    if ( !actionType.IsValid )
                    {
                        return;
                    }
                }
            }

            rockContext.WrapTransaction( () =>
            {
                // Save the entity field changes to workflow type
                if ( workflowType.Id.Equals( 0 ) )
                {
                    service.Add( workflowType );
                }
                rockContext.SaveChanges();

                // Save the workflow type attributes
                SaveAttributes( new Workflow().TypeId, "WorkflowTypeId", workflowType.Id.ToString(), AttributesState, rockContext );

                WorkflowActivityTypeService workflowActivityTypeService = new WorkflowActivityTypeService( rockContext );
                WorkflowActionTypeService workflowActionTypeService = new WorkflowActionTypeService( rockContext );
                WorkflowActionFormService workflowFormService = new WorkflowActionFormService( rockContext );
                WorkflowActionFormAttributeService workflowFormAttributeService = new WorkflowActionFormAttributeService( rockContext );

                // delete WorkflowActionTypes that aren't assigned in the UI anymore
                List<WorkflowActionType> actionTypesInDB = workflowActionTypeService.Queryable().Where( a => a.ActivityType.WorkflowTypeId.Equals( workflowType.Id ) ).ToList();
                List<WorkflowActionType> actionTypesInUI = new List<WorkflowActionType>();

                foreach ( var workflowActivity in ActivityTypesState )
                {
                    foreach ( var workflowAction in workflowActivity.ActionTypes )
                    {
                        actionTypesInUI.Add( workflowAction );
                    }
                }

                var deletedActionTypes = from actionType in actionTypesInDB
                                         where !actionTypesInUI.Select( u => u.Guid ).Contains( actionType.Guid )
                                         select actionType;
                deletedActionTypes.ToList().ForEach( actionType =>
                {
                    if ( actionType.WorkflowForm != null )
                    {
                        workflowFormService.Delete( actionType.WorkflowForm );
                    }
                    workflowActionTypeService.Delete( actionType );
                } );
                rockContext.SaveChanges();

                // delete WorkflowActivityTypes that aren't assigned in the UI anymore
                List<WorkflowActivityType> activityTypesInDB = workflowActivityTypeService.Queryable().Where( a => a.WorkflowTypeId.Equals( workflowType.Id ) ).ToList();
                var deletedActivityTypes = from activityType in activityTypesInDB
                                           where !ActivityTypesState.Select( u => u.Guid ).Contains( activityType.Guid )
                                           select activityType;
                deletedActivityTypes.ToList().ForEach( activityType =>
                {
                    workflowActivityTypeService.Delete( activityType );
                } );
                rockContext.SaveChanges();

                // add or update WorkflowActivityTypes(and Actions) that are assigned in the UI
                int workflowActivityTypeOrder = 0;
                foreach ( var editorWorkflowActivityType in ActivityTypesState )
                {
                    // Add or Update the activity type
                    WorkflowActivityType workflowActivityType = workflowType.ActivityTypes.FirstOrDefault( a => a.Guid.Equals( editorWorkflowActivityType.Guid ) );
                    if ( workflowActivityType == null )
                    {
                        workflowActivityType = new WorkflowActivityType();
                        workflowActivityType.Guid = editorWorkflowActivityType.Guid;
                        workflowType.ActivityTypes.Add( workflowActivityType );
                    }
                    workflowActivityType.IsActive = editorWorkflowActivityType.IsActive;
                    workflowActivityType.Name = editorWorkflowActivityType.Name;
                    workflowActivityType.Description = editorWorkflowActivityType.Description;
                    workflowActivityType.IsActivatedWithWorkflow = editorWorkflowActivityType.IsActivatedWithWorkflow;
                    workflowActivityType.Order = workflowActivityTypeOrder++;

                    // Save Activity Type
                    rockContext.SaveChanges();

                    // Save ActivityType Attributes
                    if ( ActivityAttributesState.ContainsKey( workflowActivityType.Guid ) )
                    {
                        SaveAttributes( new WorkflowActivity().TypeId, "ActivityTypeId", workflowActivityType.Id.ToString(), ActivityAttributesState[workflowActivityType.Guid], rockContext );
                    }

                    int workflowActionTypeOrder = 0;
                    foreach ( var editorWorkflowActionType in editorWorkflowActivityType.ActionTypes )
                    {
                        WorkflowActionType workflowActionType = workflowActivityType.ActionTypes.FirstOrDefault( a => a.Guid.Equals( editorWorkflowActionType.Guid ) );
                        if ( workflowActionType == null )
                        {
                            // New action
                            workflowActionType = new WorkflowActionType();
                            workflowActionType.Guid = editorWorkflowActionType.Guid;
                            workflowActivityType.ActionTypes.Add( workflowActionType );
                        }
                        workflowActionType.CriteriaAttributeGuid = editorWorkflowActionType.CriteriaAttributeGuid;
                        workflowActionType.CriteriaComparisonType = editorWorkflowActionType.CriteriaComparisonType;
                        workflowActionType.CriteriaValue = editorWorkflowActionType.CriteriaValue;
                        workflowActionType.Name = editorWorkflowActionType.Name;
                        workflowActionType.EntityTypeId = editorWorkflowActionType.EntityTypeId;
                        workflowActionType.IsActionCompletedOnSuccess = editorWorkflowActionType.IsActionCompletedOnSuccess;
                        workflowActionType.IsActivityCompletedOnSuccess = editorWorkflowActionType.IsActivityCompletedOnSuccess;
                        workflowActionType.Attributes = editorWorkflowActionType.Attributes;
                        workflowActionType.AttributeValues = editorWorkflowActionType.AttributeValues;
                        workflowActionType.Order = workflowActionTypeOrder++;

                        if ( workflowActionType.WorkflowForm != null && editorWorkflowActionType.WorkflowForm == null )
                        {
                            // Form removed
                            workflowFormService.Delete( workflowActionType.WorkflowForm );
                            workflowActionType.WorkflowForm = null;
                        }

                        if ( editorWorkflowActionType.WorkflowForm != null )
                        {
                            if ( workflowActionType.WorkflowForm == null )
                            {
                                workflowActionType.WorkflowForm = new WorkflowActionForm();
                            }

                            workflowActionType.WorkflowForm.NotificationSystemEmailId = editorWorkflowActionType.WorkflowForm.NotificationSystemEmailId;
                            workflowActionType.WorkflowForm.IncludeActionsInNotification = editorWorkflowActionType.WorkflowForm.IncludeActionsInNotification;
                            workflowActionType.WorkflowForm.AllowNotes = editorWorkflowActionType.WorkflowForm.AllowNotes;
                            workflowActionType.WorkflowForm.Header = editorWorkflowActionType.WorkflowForm.Header;
                            workflowActionType.WorkflowForm.Footer = editorWorkflowActionType.WorkflowForm.Footer;
                            workflowActionType.WorkflowForm.Actions = editorWorkflowActionType.WorkflowForm.Actions;
                            workflowActionType.WorkflowForm.ActionAttributeGuid = editorWorkflowActionType.WorkflowForm.ActionAttributeGuid;

                            var editorGuids = editorWorkflowActionType.WorkflowForm.FormAttributes
                                .Select( a => a.Attribute.Guid )
                                .ToList();

                            foreach ( var formAttribute in workflowActionType.WorkflowForm.FormAttributes
                                .Where( a => !editorGuids.Contains( a.Attribute.Guid ) ).ToList() )
                            {
                                workflowFormAttributeService.Delete( formAttribute );
                            }

                            int attributeOrder = 0;
                            foreach ( var editorAttribute in editorWorkflowActionType.WorkflowForm.FormAttributes.OrderBy( a => a.Order ) )
                            {
                                int attributeId = AttributeCache.Read( editorAttribute.Attribute.Guid, rockContext ).Id;

                                var formAttribute = workflowActionType.WorkflowForm.FormAttributes
                                    .Where( a => a.AttributeId == attributeId )
                                    .FirstOrDefault();

                                if ( formAttribute == null )
                                {
                                    formAttribute = new WorkflowActionFormAttribute();
                                    formAttribute.Guid = editorAttribute.Guid;
                                    formAttribute.AttributeId = attributeId;
                                    workflowActionType.WorkflowForm.FormAttributes.Add( formAttribute );
                                }

                                formAttribute.Order = attributeOrder++;
                                formAttribute.IsVisible = editorAttribute.IsVisible;
                                formAttribute.IsReadOnly = editorAttribute.IsReadOnly;
                                formAttribute.IsRequired = editorAttribute.IsRequired;
                                formAttribute.HideLabel = editorAttribute.HideLabel;
                                formAttribute.PreHtml = editorAttribute.PreHtml;
                                formAttribute.PostHtml = editorAttribute.PostHtml;
                            }
                        }
                    }
                }

                rockContext.SaveChanges();


                foreach ( var activityType in workflowType.ActivityTypes )
                {
                    foreach ( var workflowActionType in activityType.ActionTypes )
                    {
                        workflowActionType.SaveAttributeValues( rockContext );
                    }
                }

            } );

            var qryParams = new Dictionary<string, string>();
            qryParams["workflowTypeId"] = workflowType.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfWorkflowTypeId.Value.Equals( "0" ) )
            {
                int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();
                if ( parentCategoryId.HasValue )
                {
                    // Cancelling on Add, and we know the parentCategoryId, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    qryParams["CategoryId"] = parentCategoryId.ToString();
                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                WorkflowTypeService service = new WorkflowTypeService( new RockContext() );
                WorkflowType item = service.Get( int.Parse( hfWorkflowTypeId.Value ) );
                ShowReadonlyDetails( item );
            }
        }

        #endregion

        #region Attribute Events

        /// <summary>
        /// Handles the Add event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Add( object sender, EventArgs e )
        {
            ParseControls();

            ShowAttributeEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Edit( object sender, RowEventArgs e )
        {
            ParseControls();

            Guid attributeGuid = (Guid)e.RowKeyValue;
            ShowAttributeEdit( attributeGuid );
        }

        /// <summary>
        /// Handles the GridReorder event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            ParseControls();

            SortAttributes( AttributesState, e.OldIndex, e.NewIndex );
            ReOrderAttributes( AttributesState );
            BindAttributesGrid();
            BuildControls( true );
        }

        /// <summary>
        /// Handles the Delete event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gAttributes_Delete( object sender, RowEventArgs e )
        {
            ParseControls();

            Guid attributeGuid = (Guid)e.RowKeyValue;
            AttributesState.RemoveEntity( attributeGuid );

            BindAttributesGrid();
            BuildControls();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttributes_GridRebind( object sender, EventArgs e )
        {
            BindAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( AttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = AttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                AttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = AttributesState.Any() ? AttributesState.Max( a => a.Order ) + 1 : 0;
            }

            AttributesState.Add( attribute );

            ReOrderAttributes( AttributesState );

            BindAttributesGrid();

            HideDialog();

            BuildControls(true );
        }

        #endregion

        #region Activity/Action Events

        /// <summary>
        /// Handles the Click event of the lbAddActivity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddActivityType_Click( object sender, EventArgs e )
        {
            ParseControls();

            WorkflowActivityType workflowActivityType = new WorkflowActivityType();
            workflowActivityType.Guid = Guid.NewGuid();
            workflowActivityType.IsActive = true;
            workflowActivityType.Order = ActivityTypesState.Any() ? ActivityTypesState.Max( a => a.Order ) + 1 : 0;
            ActivityTypesState.Add( workflowActivityType );

            ActivityAttributesState.Add( workflowActivityType.Guid, new List<Attribute>() );
            
            ExpandedActivities.Add( workflowActivityType.Guid );

            BuildControls( true, workflowActivityType.Guid );
        }


        /// <summary>
        /// Handles the DeleteActivityClick event of the workflowActivityTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void workflowActivityTypeEditor_DeleteActivityClick( object sender, EventArgs e )
        {
            ParseControls();

            var workflowActivityTypeEditor = sender as WorkflowActivityTypeEditor;
            if ( workflowActivityTypeEditor != null )
            {
                var activityType = ActivityTypesState.Where( a => a.Guid == workflowActivityTypeEditor.ActivityTypeGuid ).FirstOrDefault();
                if ( activityType != null )
                {
                    if (ExpandedActivities.Contains(activityType.Guid))
                    {
                        ExpandedActivities.Remove( activityType.Guid );
                    }

                    if ( ExpandedActivityAttributes.Contains( activityType.Guid ) )
                    {
                        ExpandedActivityAttributes.Remove( activityType.Guid );
                    }

                    ActivityTypesState.Remove( activityType );
                }

                BuildControls( true );
            }
        }

        /// <summary>
        /// Handles the AddActionTypeClick event of the workflowActivityTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void workflowActivityTypeEditor_AddActionTypeClick( object sender, EventArgs e )
        {
            ParseControls();

            var workflowActivityTypeEditor = sender as WorkflowActivityTypeEditor;
            if ( workflowActivityTypeEditor != null )
            {
                var activityType = ActivityTypesState.Where( a => a.Guid == workflowActivityTypeEditor.ActivityTypeGuid ).FirstOrDefault();
                var actionType = new WorkflowActionType();
                actionType.Guid = Guid.NewGuid();
                actionType.IsActionCompletedOnSuccess = true;
                actionType.Order = activityType.ActionTypes.Any() ? activityType.ActionTypes.Max( a => a.Order ) + 1 : 0;
                activityType.ActionTypes.Add( actionType );

                var action = EntityTypeCache.Read( actionType.EntityTypeId );
                if ( action != null )
                {
                    var rockContext = new RockContext();
                    Rock.Attribute.Helper.UpdateAttributes( action.GetEntityType(), actionType.TypeId, "EntityTypeId", actionType.EntityTypeId.ToString(), rockContext );
                    actionType.LoadAttributes( rockContext );
                }

                ExpandedActions.Add( actionType.Guid );

                BuildControls( true, activityType.Guid, actionType.Guid );
            }
        }

        /// <summary>
        /// Handles the DeleteActionTypeClick event of the workflowActionTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void workflowActionTypeEditor_DeleteActionTypeClick( object sender, EventArgs e )
        {
            ParseControls();

            var workflowActionTypeEditor = sender as WorkflowActionTypeEditor;
            if ( workflowActionTypeEditor != null )
            {
                var workflowActivityTypeEditor = workflowActionTypeEditor.Parent as WorkflowActivityTypeEditor;
                var activityType = ActivityTypesState.Where( a => a.Guid == workflowActivityTypeEditor.ActivityTypeGuid ).FirstOrDefault();
                var actionType = activityType.ActionTypes.Where( a => a.Guid.Equals( workflowActionTypeEditor.ActionTypeGuid ) ).FirstOrDefault();
                if ( activityType != null && actionType != null )
                {
                    if ( ExpandedActions.Contains( actionType.Guid ) )
                    {
                        ExpandedActions.Remove( actionType.Guid );
                    }

                    activityType.ActionTypes.Remove( actionType );
                }

                BuildControls( true, activityType.Guid );
            }
        }

        /// <summary>
        /// Handles the ChangeActionTypeClick event of the workflowActionTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void workflowActionTypeEditor_ChangeActionTypeClick( object sender, EventArgs e )
        {
            ParseControls();

            var workflowActionTypeEditor = sender as WorkflowActionTypeEditor;
            if ( workflowActionTypeEditor != null )
            {
                var workflowActivityTypeEditor = workflowActionTypeEditor.Parent as WorkflowActivityTypeEditor;
                var activityType = ActivityTypesState.Where( a => a.Guid == workflowActivityTypeEditor.ActivityTypeGuid ).FirstOrDefault();
                var actionType = activityType.ActionTypes.Where( a => a.Guid.Equals( workflowActionTypeEditor.ActionTypeGuid ) ).FirstOrDefault();
                if ( actionType != null )
                {
                    var action = EntityTypeCache.Read( actionType.EntityTypeId );
                    if ( action != null )
                    {
                        var rockContext = new RockContext();
                        Rock.Attribute.Helper.UpdateAttributes( action.GetEntityType(), actionType.TypeId, "EntityTypeId", actionType.EntityTypeId.ToString(), rockContext );
                        actionType.LoadAttributes( rockContext );
                    }

                    BuildControls( true, actionType.Guid, actionType.Guid );
                }
            }
        }

        #endregion

        #region Activity Attribute Events

        void control_RebindAttributeClick( object sender, WorkflowActivityTypeAttributeEventArg e )
        {
            BuildControls( true, e.ActivityTypeGuid );
        }

        void control_AddAttributeClick( object sender, WorkflowActivityTypeAttributeEventArg e )
        {
            ParseControls();

            ShowActivityAttributeEdit( e.ActivityTypeGuid, e.AttributeGuid );
        }

        void control_EditAttributeClick( object sender, WorkflowActivityTypeAttributeEventArg e )
        {
            ParseControls();

            ShowActivityAttributeEdit( e.ActivityTypeGuid, e.AttributeGuid );
        }

        void control_ReorderAttributeClick( object sender, WorkflowActivityTypeAttributeEventArg e )
        {
            ParseControls();

            if ( ActivityAttributesState.ContainsKey( e.ActivityTypeGuid ) )
            {
                SortAttributes( ActivityAttributesState[e.ActivityTypeGuid], e.OldIndex, e.NewIndex );
                ReOrderAttributes( ActivityAttributesState[e.ActivityTypeGuid] );
                BuildControls( true, e.ActivityTypeGuid );
            }
        }

        void control_DeleteAttributeClick( object sender, WorkflowActivityTypeAttributeEventArg e )
        {
            ParseControls();

            if ( ActivityAttributesState.ContainsKey( e.ActivityTypeGuid ) )
            {
                ActivityAttributesState[e.ActivityTypeGuid].RemoveEntity( e.AttributeGuid );
                BuildControls( true, e.ActivityTypeGuid );
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgActivityAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtActivityAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            var activityTypeGuid = hfActivityTypeGuid.Value.AsGuid();

            if ( ActivityAttributesState.ContainsKey( activityTypeGuid ) )
            {
                if ( ActivityAttributesState[activityTypeGuid].Any( a => a.Guid.Equals( attribute.Guid ) ) )
                {
                    attribute.Order = ActivityAttributesState[activityTypeGuid].Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                    ActivityAttributesState[activityTypeGuid].RemoveEntity( attribute.Guid );
                }
                else
                {
                    attribute.Order = ActivityAttributesState[activityTypeGuid].Any() ? ActivityAttributesState[activityTypeGuid].Max( a => a.Order ) + 1 : 0;
                }
                ActivityAttributesState[activityTypeGuid].Add( attribute );

                ReOrderAttributes( ActivityAttributesState[activityTypeGuid] );
            }

            HideDialog();

            hfActivityTypeGuid.Value = string.Empty;

            BuildControls( true, activityTypeGuid );
        }

        #endregion

        #endregion

        #region Methods

        #region Show Details

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            int? workflowTypeId = PageParameter( "workflowTypeId" ).AsIntegerOrNull();
            int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();

            if ( !workflowTypeId.HasValue )
            {
                pnlDetails.Visible = false;
                return;
            }

            var rockContext = new RockContext();

            WorkflowType workflowType = null;

            if ( workflowTypeId.Value.Equals( 0 ) )
            {
                workflowType = new WorkflowType();
                workflowType.Id = 0;
                workflowType.IsActive = true;
                workflowType.IsPersisted = true;
                workflowType.IsSystem = false;
                workflowType.CategoryId = parentCategoryId;
                workflowType.IconCssClass = "fa fa-list-ol";
                workflowType.ActivityTypes.Add( new WorkflowActivityType { Guid = Guid.NewGuid(), IsActive = true } );
                workflowType.WorkTerm = "Work";            
            }
            else
            {
                workflowType = new WorkflowTypeService( rockContext ).Get( workflowTypeId.Value );
            }

            if ( workflowType == null )
            {
                return;
            }

            pnlDetails.Visible = true;
            hfWorkflowTypeId.Value = workflowType.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            // User must have 'Edit' rights to block, or 'Administrate' rights to workflow type
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Heading = "Information";
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( WorkflowType.FriendlyTypeName );
            }

            if ( workflowType.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Heading = "Information";
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( WorkflowType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnSecurity.Visible = false;
                ShowReadonlyDetails( workflowType );
            }
            else
            {
                btnEdit.Visible = true;

                btnSecurity.Title = "Secure " + workflowType.Name;
                btnSecurity.EntityId = workflowType.Id;

                if ( workflowType.Id > 0 )
                {
                    ShowReadonlyDetails( workflowType );
                }
                else
                {
                    LoadStateDetails(workflowType, rockContext);
                    ShowEditDetails( workflowType, rockContext );
                }
            }
        }

        private void LoadStateDetails( WorkflowType workflowType, RockContext rockContext )
        {
            if ( workflowType != null )
            {
                var attributeService = new AttributeService( rockContext );
                AttributesState = attributeService
                    .GetByEntityTypeId( new Workflow().TypeId ).AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( workflowType.Id.ToString() ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name )
                    .ToList();

                ActivityTypesState = workflowType.ActivityTypes.OrderBy( a => a.Order ).ToList();
                ActivityAttributesState = new Dictionary<Guid, List<Attribute>>();

                foreach ( var activityType in ActivityTypesState )
                {
                    var activityTypeAttributes = attributeService
                        .GetByEntityTypeId( new WorkflowActivity().TypeId ).AsQueryable()
                        .Where( a =>
                            a.EntityTypeQualifierColumn.Equals( "ActivityTypeId", StringComparison.OrdinalIgnoreCase ) &&
                            a.EntityTypeQualifierValue.Equals( activityType.Id.ToString() ) )
                        .OrderBy( a => a.Order )
                        .ThenBy( a => a.Name )
                        .ToList();

                    ActivityAttributesState.Add( activityType.Guid, activityTypeAttributes );

                    foreach ( var actionType in activityType.ActionTypes )
                    {
                        var action = EntityTypeCache.Read( actionType.EntityTypeId );
                        if ( action != null )
                        {
                            Rock.Attribute.Helper.UpdateAttributes( action.GetEntityType(), actionType.TypeId, "EntityTypeId", actionType.EntityTypeId.ToString(), rockContext );
                            actionType.LoadAttributes( rockContext );
                        }
                    }
                }
            }
            else
            {
                AttributesState = new List<Attribute>();
                ActivityTypesState = new List<WorkflowActivityType>();
                ActivityAttributesState = new Dictionary<Guid, List<Attribute>>();
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="workflowType">Type of the workflow.</param>
        /// <param name="rockContext">The rock context.</param>
        private void ShowEditDetails( WorkflowType workflowType, RockContext rockContext )
        {
            ExpandedActivities = new List<Guid>();
            ExpandedActivityAttributes = new List<Guid>();
            ExpandedActions = new List<Guid>();

            if ( workflowType.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( WorkflowType.FriendlyTypeName ).FormatAsHtmlTitle();
                foreach( var activity in workflowType.ActivityTypes)
                {
                    ExpandedActivities.Add( activity.Guid );
                }
                hlInactive.Visible = false;
            }
            else
            {
                pwDetails.Expanded = false;
            }

            SetEditMode( true );

            LoadDropDowns();

            cbIsActive.Checked = workflowType.IsActive ?? false;
            tbName.Text = workflowType.Name;
            tbDescription.Text = workflowType.Description;
            cpCategory.SetValue( workflowType.CategoryId );
            tbWorkTerm.Text = workflowType.WorkTerm;
            tbProcessingInterval.Text = workflowType.ProcessingIntervalSeconds != null ? workflowType.ProcessingIntervalSeconds.ToString() : string.Empty;
            cbIsPersisted.Checked = workflowType.IsPersisted;
            ddlLoggingLevel.SetValue( (int)workflowType.LoggingLevel );
            tbIconCssClass.Text = workflowType.IconCssClass;

            BindAttributesGrid();

            BuildControls( true );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="workflowType">Type of the workflow.</param>
        private void ShowReadonlyDetails( WorkflowType workflowType )
        {
            SetEditMode( false );

            hfWorkflowTypeId.SetValue( workflowType.Id );
            AttributesState = null;
            ActivityTypesState = null;
            ActivityAttributesState = null;
            ExpandedActivities = null;
            ExpandedActivityAttributes = null;
            ExpandedActions = null;

            lReadOnlyTitle.Text = workflowType.Name.FormatAsHtmlTitle();
            hlInactive.Visible = workflowType.IsActive == false;
            lblActivitiesReadonlyHeaderLabel.Text = string.Format( "<strong>Activities</strong> ({0})", workflowType.ActivityTypes.Count() );

            if ( workflowType.Category != null )
            {
                hlType.Visible = true;
                hlType.Text = workflowType.Category.Name;
            }
            else
            {
                hlType.Visible = false;
            }

            lWorkflowTypeDescription.Text = workflowType.Description;

            if ( workflowType.ActivityTypes.Count > 0 )
            {
                // Activities
                lblWorkflowActivitiesReadonly.Text = @"
<div>
    <ol>";

                foreach ( var activityType in workflowType.ActivityTypes.OrderBy( a => a.Order ) )
                {
                    string activityTypeTextFormat = @"
        <li>
            <strong>{0}</strong>
            {1}
            <br />
            {2}
            <ol>
                {3}
            </ol>
        </li>
";

                    string actionTypeText = string.Empty;

                    foreach ( var actionType in activityType.ActionTypes.OrderBy( a => a.Order ) )
                    {
                        actionTypeText += string.Format( "<li>{0}</li>" + Environment.NewLine, actionType.Name );
                    }

                    string actionsTitle = activityType.ActionTypes.Count > 0 ? "Actions:" : "No Actions";

                    lblWorkflowActivitiesReadonly.Text += string.Format( activityTypeTextFormat, activityType.Name, activityType.Description, actionsTitle, actionTypeText );
                }

                lblWorkflowActivitiesReadonly.Text += @"
    </ol>
</div>
";
            }
            else
            {
                lblWorkflowActivitiesReadonly.Text = "<div>" + None.TextHtml + "</div>";
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlLoggingLevel.BindToEnum<WorkflowLoggingLevel>();
        }

        #endregion

        #region Build/Parse Activity and Action controls

        /// <summary>
        /// Builds the controls.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        /// <param name="activeActivityTypeGuid">The active activity type unique identifier.</param>
        /// <param name="activeActionTypeGuid">The active action type unique identifier.</param>
        private void BuildControls( bool setValues = false, Guid? activeActivityTypeGuid = null, Guid? activeActionTypeGuid = null )
        {
            phActivities.Controls.Clear();

            if ( ActivityTypesState != null )
            {
                // Add a workflowtype object to the Items collection that is used by attribute field types that may get qualifier from
                // the current workflow type (i.e. WorkflowActivityTypeAttribute
                var workflowType = new WorkflowType();
                workflowType.ActivityTypes = ActivityTypesState;
                System.Web.HttpContext.Current.Items["WorkflowType"] = workflowType;

                // Save the current workflow type attributes to state for any action settings that may need them
                var workflowAttributes = new Dictionary<Guid, Attribute>();
                AttributesState.OrderBy( a => a.Order ).ToList().ForEach( a => workflowAttributes.Add( a.Guid, a ) );
                System.Web.HttpContext.Current.Items["WorkflowTypeAttributes"] = workflowAttributes;

                if ( workflowAttributes.Any() )
                {
                    wpAttributes.Title = string.Format( "Attributes <small>Count: {0}</small>", workflowAttributes.Count.ToString( "N0" ) );
                }
                else
                {
                    wpAttributes.Title = "Attributes";
                }

                foreach ( var workflowActivityType in ActivityTypesState.OrderBy( a => a.Order ) )
                {
                    BuildActivityControl( phActivities, setValues, workflowActivityType, workflowAttributes, activeActivityTypeGuid, activeActionTypeGuid );
                }
            }
        }

        /// <summary>
        /// Builds the activity control.
        /// </summary>
        /// <param name="activityType">Type of the activity.</param>
        /// <param name="activeActivityTypeGuid">The active activity type unique identifier.</param>
        /// <param name="activeWorkflowActionTypeGuid">The active workflow action type unique identifier.</param>
        /// <returns></returns>
        private WorkflowActivityTypeEditor BuildActivityControl( Control parentControl, bool setValues, WorkflowActivityType activityType,
            Dictionary<Guid, Attribute> workflowAttributes, Guid? activeActivityTypeGuid = null, Guid? activeWorkflowActionTypeGuid = null, bool showInvalid = false )
        {
            // Save the current activity type attributes to state for any action settings that may need them
            var activityAttributes = new Dictionary<Guid, Attribute>();
            ActivityAttributesState[activityType.Guid].OrderBy( a => a.Order ).ToList().ForEach( a => activityAttributes.Add( a.Guid, a ) );
            System.Web.HttpContext.Current.Items["ActivityTypeAttributes"] = activityAttributes;

            var control = new WorkflowActivityTypeEditor();
            control.ID = activityType.Guid.ToString( "N" );
            parentControl.Controls.Add( control );
            control.ValidationGroup = btnSave.ValidationGroup;

            control.DeleteActivityTypeClick += workflowActivityTypeEditor_DeleteActivityClick;
            control.AddActionTypeClick += workflowActivityTypeEditor_AddActionTypeClick;
            control.RebindAttributeClick += control_RebindAttributeClick;
            control.AddAttributeClick += control_AddAttributeClick;
            control.EditAttributeClick += control_EditAttributeClick;
            control.ReorderAttributeClick += control_ReorderAttributeClick;
            control.DeleteAttributeClick += control_DeleteAttributeClick;

            control.SetWorkflowActivityType( activityType );
            control.BindAttributesGrid( ActivityAttributesState[activityType.Guid] );

            foreach ( WorkflowActionType actionType in activityType.ActionTypes.OrderBy( a => a.Order ) )
            {
                var attributes = new Dictionary<Guid, Attribute>();
                workflowAttributes.ToList().ForEach( a => attributes.Add( a.Key, a.Value ) );
                ActivityAttributesState[activityType.Guid].OrderBy( a => a.Order ).ToList().ForEach( a => attributes.Add( a.Guid, a ) );

                var activities = new Dictionary<string, string>();
                ActivityTypesState.OrderBy( a => a.Order ).ToList().ForEach( a => activities.Add( a.Guid.ToString().ToUpper(), a.Name ) );

                BuildActionControl( control, setValues, actionType, attributes, activities, activeWorkflowActionTypeGuid, showInvalid );
            }

            if ( setValues )
            {
                control.Expanded = ExpandedActivities.Contains( activityType.Guid ) ||
                    activityType.ActionTypes.Any( a => ExpandedActions.Contains( a.Guid ) );

                control.AttributesExpanded = ExpandedActivityAttributes.Contains( activityType.Guid );

                if ( !control.Expanded && showInvalid && !activityType.IsValid )
                {
                    control.Expanded = true;
                }

                if ( !control.Expanded )
                {
                    control.Expanded = activeActivityTypeGuid.HasValue && activeActivityTypeGuid.Equals( activityType.Guid );
                }

            }

            return control;
        }

        /// <summary>
        /// Builds the action control.
        /// </summary>
        /// <param name="actionType">Type of the action.</param>
        /// <param name="activeWorkflowActionTypeGuid">The active workflow action type unique identifier.</param>
        /// <returns></returns>
        private WorkflowActionTypeEditor BuildActionControl( Control parentControl, bool setValues, WorkflowActionType actionType, 
            Dictionary<Guid, Attribute> attributes, Dictionary<string, string> activities, Guid? activeWorkflowActionTypeGuid = null, 
                bool showInvalid = false )
        {
            var control = new WorkflowActionTypeEditor();
            parentControl.Controls.Add( control );
            control.ID = actionType.Guid.ToString( "N" );
            control.ValidationGroup = btnSave.ValidationGroup;

            control.DeleteActionTypeClick += workflowActionTypeEditor_DeleteActionTypeClick;
            control.ChangeActionTypeClick += workflowActionTypeEditor_ChangeActionTypeClick;

            control.WorkflowActivities = activities;

            if (actionType.WorkflowForm != null)
            {
                var formAttributes = actionType.WorkflowForm.FormAttributes;

                // Remove any fields that were removed
                foreach ( var formAttribute in formAttributes.ToList() )
                {
                    if (!attributes.ContainsKey(formAttribute.Attribute.Guid))
                    {
                        formAttributes.Remove( formAttribute );
                    }
                }

                // Add any new attributes
                foreach(var attribute in attributes)
                {
                    if ( !formAttributes.Select( a => a.Attribute.Guid ).Contains( attribute.Key ) )
                    {
                        var formAttribute = new WorkflowActionFormAttribute();
                        formAttribute.Attribute = new Rock.Model.Attribute { Guid = attribute.Key, Name = attribute.Value.Name };
                        formAttribute.Guid = Guid.NewGuid();
                        formAttribute.Order = formAttributes.Any() ? formAttributes.Max( a => a.Order ) + 1 : 0;
                        formAttribute.IsVisible = false;
                        formAttribute.IsReadOnly = true;
                        formAttribute.IsRequired = false;
                        formAttribute.HideLabel = false;
                        formAttribute.PreHtml = string.Empty;
                        formAttribute.PostHtml = string.Empty;
                        formAttributes.Add( formAttribute );
                    }
                }
            }
            control.SetWorkflowActionType( actionType, attributes );

            control.Expanded = ExpandedActions.Contains( actionType.Guid );

            if ( setValues )
            {

                // Set order
                int newOrder = 0;
                foreach ( var attributeRow in control.FormEditor.AttributeRows )
                {
                    attributeRow.Order = newOrder++;
                }

                if ( !control.Expanded && showInvalid && !actionType.IsValid )
                {
                    control.Expanded = true;
                }

                if ( !control.Expanded )
                {
                    control.Expanded = activeWorkflowActionTypeGuid.HasValue && activeWorkflowActionTypeGuid.Equals( actionType.Guid );
                }
            }

            return control;
        }

        /// <summary>
        /// Parses the controls.
        /// </summary>
        private void ParseControls( bool expandInvalid = false )
        {
            ExpandedActivities = new List<Guid>();
            ExpandedActions = new List<Guid>();

            ActivityTypesState = new List<WorkflowActivityType>();
            int order = 0;
            foreach ( var activityEditor in phActivities.Controls.OfType<WorkflowActivityTypeEditor>() )
            {
                WorkflowActivityType workflowActivityType = activityEditor.GetWorkflowActivityType( expandInvalid );
                workflowActivityType.Order = order++;

                ActivityTypesState.Add( workflowActivityType );

                if (activityEditor.Expanded)
                {
                    ExpandedActivities.Add( workflowActivityType.Guid );
                    ExpandedActions.AddRange( activityEditor.ExpandedActions );
                }

                if (activityEditor.AttributesExpanded)
                {
                    ExpandedActivityAttributes.Add( workflowActivityType.Guid );
                }
            }
        }

        #endregion

        #region Sorting

        /// <summary>
        /// Sorts the activities.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="newIndex">The new index.</param>
        private void SortActivities( Guid guid, int newIndex )
        {
            ParseControls();

            Guid? activeWorkflowActivityTypeGuid = null;

            var workflowActivityType = ActivityTypesState.FirstOrDefault( a => a.Guid.Equals( guid ) );
            if ( workflowActivityType != null )
            {
                activeWorkflowActivityTypeGuid = workflowActivityType.Guid;

                ActivityTypesState.Remove( workflowActivityType );
                if ( newIndex >= ActivityTypesState.Count() )
                {
                    ActivityTypesState.Add( workflowActivityType );
                }
                else
                {
                    ActivityTypesState.Insert( newIndex, workflowActivityType );
                }
            }

            int order = 0;
            foreach ( var item in ActivityTypesState )
            {
                item.Order = order++;
            }

            BuildControls( true );
        }

        /// <summary>
        /// Sorts the actions.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="newIndex">The new index.</param>
        private void SortActions( Guid guid, int newIndex )
        {
            ParseControls();

            Guid? activeWorkflowActivityTypeGuid = null;
            Guid? activeWorkflowActionTypeGuid = null;

            var workflowActivityType = ActivityTypesState.FirstOrDefault( a => a.ActionTypes.Any( b => b.Guid.Equals( guid ) ) );
            if ( workflowActivityType != null )
            {
                activeWorkflowActivityTypeGuid = workflowActivityType.Guid;

                WorkflowActionType workflowActionType = workflowActivityType.ActionTypes.FirstOrDefault( a => a.Guid.Equals( guid ) );
                if ( workflowActionType != null )
                {
                    activeWorkflowActionTypeGuid = workflowActionType.Guid;

                    var workflowActionTypes = workflowActivityType.ActionTypes.ToList();

                    workflowActionTypes.Remove( workflowActionType );
                    if ( newIndex >= workflowActionTypes.Count() )
                    {
                        workflowActionTypes.Add( workflowActionType );
                    }
                    else
                    {
                        workflowActionTypes.Insert( newIndex, workflowActionType );
                    }

                    int order = 0;
                    foreach ( var item in workflowActionTypes )
                    {
                        item.Order = order++;
                    }

                    workflowActivityType.ActionTypes = workflowActionTypes;
                }
            }

            BuildControls( true, activeWorkflowActivityTypeGuid );

        }

        /// <summary>
        /// Sorts the form fields.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="newIndex">The new index.</param>
        private void SortFormFields( Guid guid, int newIndex )
        {
            ParseControls();

            Guid? activeWorkflowActivityTypeGuid = null;
            Guid? activeWorkflowActionTypeGuid = null;

            WorkflowActivityType workflowActivityType = ActivityTypesState
                .FirstOrDefault( a =>
                    a.ActionTypes.Any( b =>
                        b.WorkflowForm != null &&
                        b.WorkflowForm.FormAttributes.Any( c =>
                            c.Guid.Equals( guid ) ) ) );
            if ( workflowActivityType != null )
            {
                activeWorkflowActivityTypeGuid = workflowActivityType.Guid;

                WorkflowActionType workflowActionType = workflowActivityType.ActionTypes
                    .FirstOrDefault( a =>
                        a.WorkflowForm != null &&
                        a.WorkflowForm.FormAttributes != null &&
                        a.WorkflowForm.FormAttributes.Any( b =>
                            b.Guid.Equals( guid ) ) );
                if ( workflowActionType != null )
                {
                    activeWorkflowActionTypeGuid = workflowActionType.Guid;

                    WorkflowActionFormAttribute workflowFormAttribute = workflowActionType.WorkflowForm.FormAttributes
                        .Where( a => a.Guid.Equals( guid ) )
                        .FirstOrDefault();
                    if ( workflowFormAttribute != null )
                    {

                        var workflowFormAttributes = workflowActionType.WorkflowForm.FormAttributes.ToList();

                        workflowFormAttributes.Remove( workflowFormAttribute );
                        if ( newIndex >= workflowFormAttributes.Count() )
                        {
                            workflowFormAttributes.Add( workflowFormAttribute );
                        }
                        else
                        {
                            workflowFormAttributes.Insert( newIndex, workflowFormAttribute );
                        }

                        int order = 0;
                        foreach ( var item in workflowFormAttributes )
                        {
                            item.Order = order++;
                        }

                        workflowActionType.WorkflowForm.FormAttributes = workflowFormAttributes;
                    }
                }
            }

            BuildControls( true, activeWorkflowActivityTypeGuid, activeWorkflowActionTypeGuid );
        }

        /// <summary>
        /// Reorders the attribute list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void SortAttributes( List<Attribute> attributeList, int oldIndex, int newIndex )
        {
            var movedItem = attributeList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in attributeList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in attributeList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Reorder attributes.
        /// </summary>
        private void ReOrderAttributes( List<Attribute> attributeList )
        {
            attributeList = attributeList.OrderBy( a => a.Order ).ToList();
            int order = 0;
            attributeList.ForEach( a => a.Order = order++ );
        }

        #endregion

        #region Attribute Grid

        /// <summary>
        /// Binds the group type attributes grid.
        /// </summary>
        private void BindAttributesGrid()
        {
            gAttributes.DataSource = AttributesState
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Guid,
                    a.Name,
                    a.Description,
                    FieldType = FieldTypeCache.GetName(a.FieldTypeId),
                    a.IsRequired
                } )
                .ToList();
            gAttributes.DataBind();
        }

        /// <summary>
        /// Shows the attribute edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        private void ShowAttributeEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
                edtAttributes.ActionTitle = ActionTitle.Add( "attribute for workflows of workflow type " + tbName.Text );
            }
            else
            {
                attribute = AttributesState.First( a => a.Guid.Equals( attributeGuid ) );
                edtAttributes.ActionTitle = ActionTitle.Edit( "attribute for workflows of workflow type " + tbName.Text );
            }

            edtAttributes.ReservedKeyNames = AttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList();

            edtAttributes.SetAttributeProperties( attribute, typeof( Workflow ) );

            ShowDialog( "Attributes" );

            BuildControls( true );
        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="qualifierColumn">The qualifier column.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, List<Attribute> attributes, RockContext rockContext )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var existingAttributes = attributeService.Get( entityTypeId, qualifierColumn, qualifierValue );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                rockContext.SaveChanges();
                Rock.Web.Cache.AttributeCache.Flush( attr.Id );
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attribute in attributes )
            {
                Helper.SaveAttributeEdits( attribute, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }
        }

        #endregion

        #region Activity Attribute Grid

        /// <summary>
        /// Shows the attribute edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        private void ShowActivityAttributeEdit( Guid activityTypeGuid, Guid attributeGuid )
        {
            if ( ActivityAttributesState.ContainsKey( activityTypeGuid ) )
            {
                var attributeList = ActivityAttributesState[activityTypeGuid];

                Attribute attribute;
                if ( attributeGuid.Equals( Guid.Empty ) )
                {
                    attribute = new Attribute();
                    attribute.FieldTypeId = FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT ).Id;
                    edtActivityAttributes.ActionTitle = ActionTitle.Add( "Add Activity Attribute" );
                }
                else
                {
                    attribute = attributeList.First( a => a.Guid.Equals( attributeGuid ) );
                    edtActivityAttributes.ActionTitle = ActionTitle.Edit( "Edit Activity Attribute" );
                }

                edtActivityAttributes.ReservedKeyNames = attributeList.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList();

                edtActivityAttributes.SetAttributeProperties( attribute, typeof( WorkflowActivity ) );

                hfActivityTypeGuid.Value = activityTypeGuid.ToString();

                ShowDialog( "ActivityAttributes" );
            }

            BuildControls( true );
        }

        #endregion

        #region Dialog

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTES":
                    dlgAttribute.Show();
                    break;
                case "ACTIVITYATTRIBUTES":
                    dlgActivityAttribute.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTES":
                    dlgAttribute.Hide();
                    break;
                case "ACTIVITYATTRIBUTES":
                    dlgActivityAttribute.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

        #endregion
    
    }
}