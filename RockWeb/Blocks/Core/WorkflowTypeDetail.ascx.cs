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

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Workflow Type Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given workflow type." )]
    public partial class WorkflowTypeDetail : RockBlock
    {
        #region Properties

        private List<Attribute> AttributesState { get; set; }
        private List<WorkflowActivityType> ActivityTypesState { get; set; }
        private List<Guid> ExpandedActivities { get; set; }
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

            ExpandedActivities = ViewState["ExpandedActivities"] as List<Guid>;
            if (ExpandedActivities == null)
            {
                ExpandedActivities = new List<Guid>();
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
                            int newIndex = values[1].AsInteger() ?? 0;

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
            var jsonSetting = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            ViewState["AttributesState"] = JsonConvert.SerializeObject( AttributesState, Formatting.None, jsonSetting );
            ViewState["ActivityTypesState"] = JsonConvert.SerializeObject( ActivityTypesState, Formatting.None, jsonSetting );
            
            ViewState["ExpandedActivities"] = ExpandedActivities;
            ViewState["ExpandedActions"] = ExpandedActions;

            return base.SaveViewState();
        }

        #endregion

        #region Events

        #region Edit / Save / Cancel events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var workflowType = new WorkflowTypeService( rockContext ).Get( hfWorkflowTypeId.Value.AsInteger() ?? 0 );
            ShowEditDetails( workflowType, rockContext );
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

            int? workflowTypeId = hfWorkflowTypeId.Value.AsInteger( false );
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
            workflowType.ProcessingIntervalSeconds = tbProcessingInterval.Text.AsInteger();
            workflowType.IsPersisted = cbIsPersisted.Checked;
            workflowType.LoggingLevel = ddlLoggingLevel.SelectedValueAsEnum<WorkflowLoggingLevel>();

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
                    if (!activityType.IsValid)
                    {
                        return;
                    }
                }
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                // Save the entity field changes to workflow type
                if ( workflowType.Id.Equals( 0 ) )
                {
                    service.Add( workflowType );
                }
                rockContext.SaveChanges();

                // Save the attributes
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
                        workflowType.ActivityTypes.Add( workflowActivityType );
                    }
                    workflowActivityType.IsActive = editorWorkflowActivityType.IsActive;
                    workflowActivityType.Name = editorWorkflowActivityType.Name;
                    workflowActivityType.Description = editorWorkflowActivityType.Description;
                    workflowActivityType.IsActivatedWithWorkflow = editorWorkflowActivityType.IsActivatedWithWorkflow;
                    workflowActivityType.Order = workflowActivityTypeOrder++;

                    int workflowActionTypeOrder = 0;
                    foreach ( var editorWorkflowActionType in editorWorkflowActivityType.ActionTypes )
                    {
                        WorkflowActionType workflowActionType = workflowActivityType.ActionTypes.FirstOrDefault( a => a.Guid.Equals( editorWorkflowActionType.Guid ) );
                        if ( workflowActionType == null )
                        {
                            // New action
                            workflowActionType = new WorkflowActionType();
                            workflowActivityType.ActionTypes.Add( workflowActionType );
                        }
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

                            workflowActionType.WorkflowForm.Header = editorWorkflowActionType.WorkflowForm.Header;
                            workflowActionType.WorkflowForm.Footer = editorWorkflowActionType.WorkflowForm.Footer;
                            workflowActionType.WorkflowForm.InactiveMessage = editorWorkflowActionType.WorkflowForm.InactiveMessage;
                            workflowActionType.WorkflowForm.Actions = editorWorkflowActionType.WorkflowForm.Actions;

                            var editorGuids = editorWorkflowActionType.WorkflowForm.FormAttributes
                                .Select( a => a.Attribute.Guid )
                                .ToList();

                            foreach ( var formAttribute in workflowActionType.WorkflowForm.FormAttributes
                                .Where( a => !editorGuids.Contains( a.Attribute.Guid ) ) )
                            {
                                workflowFormAttributeService.Delete( formAttribute );
                            }

                            int attributeOrder = 0;
                            foreach ( var editorAttribute in editorWorkflowActionType.WorkflowForm.FormAttributes.OrderBy( a => a.Order ) )
                            {
                                int attributeId = AttributeCache.Read( editorAttribute.Attribute.Guid ).Id;

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
                int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsInteger( false );
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
            gAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the group attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gAttributes_ShowEdit( Guid attributeGuid )
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

            edtAttributes.SetAttributeProperties( attribute, typeof( Group ) );

            ShowDialog( "Attributes" );
        }

        /// <summary>
        /// Handles the GridReorder event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            SortAttributes( e.OldIndex, e.NewIndex );
            BindAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            AttributesState.RemoveEntity( attributeGuid );

            BindAttributesGrid();
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

            ReOrderAttributes();

            BindAttributesGrid();

            HideDialog();
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

            var workflowActivityTypeEditor = sender as WorkflowActivityEditor;
            if ( workflowActivityTypeEditor != null )
            {
                var activityType = ActivityTypesState.Where( a => a.Guid == workflowActivityTypeEditor.ActivityTypeGuid ).FirstOrDefault();
                if ( activityType != null )
                {
                    if (ExpandedActivities.Contains(activityType.Guid))
                    {
                        ExpandedActivities.Remove( activityType.Guid );
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

            var workflowActivityTypeEditor = sender as WorkflowActivityEditor;
            if ( workflowActivityTypeEditor != null )
            {
                var activityType = ActivityTypesState.Where( a => a.Guid == workflowActivityTypeEditor.ActivityTypeGuid ).FirstOrDefault();
                var actionType = new WorkflowActionType();
                actionType.Guid = Guid.NewGuid();
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

            var workflowActionTypeEditor = sender as WorkflowActionEditor;
            if ( workflowActionTypeEditor != null )
            {
                var workflowActivityTypeEditor = workflowActionTypeEditor.Parent as WorkflowActivityEditor;
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

        #endregion

        #endregion

        #region Methods

        #region Show Details

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            int? workflowTypeId = PageParameter( "workflowTypeId" ).AsInteger( false );
            int? parentCategoryId = PageParameter( "ParentCategoryId" ).AsInteger( false );

            if ( !workflowTypeId.HasValue )
            {
                pnlDetails.Visible = false;
                return;
            }

            var rockContext = new RockContext();

            WorkflowType workflowType = null;

            if ( workflowTypeId.Value.Equals( 0 ) )
            {
                workflowType = new WorkflowType { Id = 0, IsActive = true, IsPersisted = true, IsSystem = false, CategoryId = parentCategoryId };
                workflowType.ActivityTypes.Add( new WorkflowActivityType { Guid = Guid.NewGuid(), IsActive = true } );
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
                ShowReadonlyDetails( workflowType );
            }
            else
            {
                btnEdit.Visible = true;
                if ( workflowType.Id > 0 )
                {
                    ShowReadonlyDetails( workflowType );
                }
                else
                {
                    ShowEditDetails( workflowType, rockContext );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="workflowType">Type of the workflow.</param>
        /// <param name="rockContext">The rock context.</param>
        private void ShowEditDetails( WorkflowType workflowType, RockContext rockContext )
        {
            if ( workflowType.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( WorkflowType.FriendlyTypeName ).FormatAsHtmlTitle();
                hlInactive.Visible = false;
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

            AttributesState = new AttributeService( rockContext )
                .GetByEntityTypeId( new Workflow().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( workflowType.Id.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
            BindAttributesGrid();

            ActivityTypesState = workflowType.ActivityTypes.OrderBy( a => a.Order ).ToList();

            foreach (var activityType in ActivityTypesState)
            {
                foreach( var actionType in activityType.ActionTypes)
                {
                    var action = EntityTypeCache.Read( actionType.EntityTypeId );
                    if ( action != null )
                    {
                        Rock.Attribute.Helper.UpdateAttributes( action.GetEntityType(), actionType.TypeId, "EntityTypeId", actionType.EntityTypeId.ToString(), rockContext );
                        actionType.LoadAttributes( rockContext );
                    }
                }
            }

            ExpandedActivities = new List<Guid>();
            ExpandedActions = new List<Guid>();

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
            ActivityTypesState = null;
            AttributesState = null;
            ExpandedActivities = null;
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
            ddlLoggingLevel.BindToEnum( typeof( WorkflowLoggingLevel ) );
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

                foreach ( var workflowActivityType in ActivityTypesState.OrderBy( a => a.Order ) )
                {
                    BuildActivityControl( phActivities, setValues, workflowActivityType, activeActivityTypeGuid, activeActionTypeGuid );
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
        private WorkflowActivityEditor BuildActivityControl( Control parentControl, bool setValues, WorkflowActivityType activityType, Guid? activeActivityTypeGuid = null, Guid? activeWorkflowActionTypeGuid = null, bool showInvalid = false )
        {
            var control = new WorkflowActivityEditor();
            parentControl.Controls.Add( control );
            control.ValidationGroup = btnSave.ValidationGroup;

            control.ID = "WorkflowActivityTypeEditor_" + activityType.Guid.ToString( "N" );
            control.DeleteActivityTypeClick += workflowActivityTypeEditor_DeleteActivityClick;
            control.AddActionTypeClick += workflowActivityTypeEditor_AddActionTypeClick;

            control.SetWorkflowActivityType( activityType );

            foreach ( WorkflowActionType actionType in activityType.ActionTypes.OrderBy( a => a.Order ) )
            {
                BuildActionControl( control, setValues, actionType, activeWorkflowActionTypeGuid, showInvalid );
            }

            if ( setValues )
            {
                control.Expanded = ExpandedActivities.Contains( activityType.Guid ) ||
                    activityType.ActionTypes.Any( a => ExpandedActions.Contains( a.Guid ) );

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
        private WorkflowActionEditor BuildActionControl( Control parentControl, bool setValues, WorkflowActionType actionType, Guid? activeWorkflowActionTypeGuid = null, bool showInvalid = false )
        {
            var control = new WorkflowActionEditor();
            parentControl.Controls.Add( control );
            control.ValidationGroup = btnSave.ValidationGroup;

            control.ID = "WorkflowActionTypeEditor_" + actionType.Guid.ToString( "N" );
            control.DeleteActionTypeClick += workflowActionTypeEditor_DeleteActionTypeClick;

            var workflowAttributes = new Dictionary<Guid, string>();
            AttributesState.OrderBy( a => a.Order ).ToList().ForEach( a => workflowAttributes.Add( a.Guid, a.Name ) );
            control.WorkflowAttributes = workflowAttributes;

            var workflowActivities = new Dictionary<string, string>();
            ActivityTypesState.OrderBy( a => a.Order ).ToList().ForEach( a => workflowActivities.Add( a.Guid.ToString().ToUpper(), a.Name ) );
            control.WorkflowActivities = workflowActivities;

            control.SetWorkflowActionType( actionType );
            control.Expanded = ExpandedActions.Contains( actionType.Guid );

            if ( setValues )
            {
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
            foreach ( var activityEditor in phActivities.Controls.OfType<WorkflowActivityEditor>() )
            {
                WorkflowActivityType workflowActivityType = activityEditor.GetWorkflowActivityType( expandInvalid );
                workflowActivityType.Order = order++;

                ActivityTypesState.Add( workflowActivityType );

                if (activityEditor.Expanded)
                {
                    ExpandedActivities.Add( workflowActivityType.Guid );
                    ExpandedActions.AddRange( activityEditor.ExpandedActions );
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
        private void SortAttributes( int oldIndex, int newIndex )
        {
            var movedItem = AttributesState.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in AttributesState.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in AttributesState.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }

            ReOrderAttributes();
        }

        #endregion

        #region Attribute Grid

        /// <summary>
        /// Binds the group type attributes grid.
        /// </summary>
        private void BindAttributesGrid()
        {
            gAttributes.DataSource = AttributesState.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            gAttributes.DataBind();

            var workflowAttributes = new Dictionary<Guid, string>();
            AttributesState.ForEach( a => workflowAttributes.Add( a.Guid, a.Name ) );

            foreach ( var activityEditor in phActivities.Controls.OfType<WorkflowActivityEditor>() )
            {
                foreach ( WorkflowActionEditor workflowActionTypeEditor in activityEditor.Controls.OfType<WorkflowActionEditor>() )
                {
                    workflowActionTypeEditor.WorkflowAttributes = workflowAttributes;
                }
            }
        }

        private void ReOrderAttributes()
        {
            AttributesState = AttributesState.OrderBy( a => a.Order ).ToList();

            int order = 0;
            AttributesState.ForEach( a => a.Order = order++ );
        }

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
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion

        #endregion
    }
}