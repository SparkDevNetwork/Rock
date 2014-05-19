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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;


using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Workflow Type Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given workflow type." )]
    public partial class WorkflowTypeDetail : RockBlock, IDetailBlock
    {
        #region WorkflowActivityType ViewStateList

        /// <summary>
        /// Gets or sets the state of the group type attributes.
        /// </summary>
        /// <value>
        /// The state of the group type attributes.
        /// </value>
        private ViewStateList<Attribute> AttributesState
        {
            get
            {
                return ViewState["AttributesState"] as ViewStateList<Attribute>;
            }

            set
            {
                ViewState["AttributesState"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the state of the workflow activity types.
        /// </summary>
        /// <value>
        /// The state of the workflow activity types.
        /// </value>
        private ViewStateList<WorkflowActivityType> WorkflowActivityTypesState
        {
            get
            {
                return ViewState["WorkflowActivityTypesState"] as ViewStateList<WorkflowActivityType>;
            }

            set
            {
                ViewState["WorkflowActivityTypesState"] = value;
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // assign attributes grid actions
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
                string itemId = PageParameter( "workflowTypeId" );
                string parentCategoryId = PageParameter( "ParentCategoryId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    if ( string.IsNullOrWhiteSpace( parentCategoryId ) )
                    {
                        ShowDetail( "workflowTypeId", int.Parse( itemId ) );
                    }
                    else
                    {
                        ShowDetail( "workflowTypeId", int.Parse( itemId ), int.Parse( parentCategoryId ) );
                    }
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
            else
            {
                ShowDialog();
            }

            string postbackArgs = Request.Params["__EVENTARGUMENT"];
            if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
            {
                string[] nameValue = postbackArgs.Split( new char[] { ':' } );
                if ( nameValue.Count() == 2 )
                {
                    string eventParam = nameValue[0];
                    if ( eventParam.Equals( "re-order-activity" ) || 
                        eventParam.Equals( "re-order-action" ) ||
                        eventParam.Equals( "re-order-formfield" ) )
                    {
                        string[] values = nameValue[1].Split( new char[] { ';' } );
                        if ( values.Count() == 2 )
                        {
                            SortWorkflowActivityListContents( eventParam, values );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sorts the workflow activity list contents.
        /// </summary>
        /// <param name="eventParam">The event param.</param>
        /// <param name="values">The values.</param>
        private void SortWorkflowActivityListContents( string eventParam, string[] values )
        {
            // put viewstate list into a new list, shuffle the contents, then save it back to the viewstate list
            // SaveWorkflowActivityControlsToViewState();
            List<WorkflowActivityType> workflowActivityTypeSortList = WorkflowActivityTypesState.ToList();
            Guid? activeWorkflowActivityTypeGuid = null;
            Guid? activeWorkflowActionTypeGuid = null;

            var workflowActivityTypeEditorList = phActivities.Controls.OfType<WorkflowActivityEditor>().ToList();

            if ( eventParam.Equals( "re-order-activity" ) )
            {
                Guid workflowActivityTypeGuid = new Guid( values[0] );
                int newIndex = int.Parse( values[1] );
                WorkflowActivityType workflowActivityType = workflowActivityTypeSortList.FirstOrDefault( a => a.Guid.Equals( workflowActivityTypeGuid ) );
                workflowActivityTypeSortList.RemoveEntity( workflowActivityTypeGuid );
                if ( workflowActivityType != null )
                {
                    if ( newIndex >= workflowActivityTypeSortList.Count() )
                    {
                        workflowActivityTypeSortList.Add( workflowActivityType );
                    }
                    else
                    {
                        workflowActivityTypeSortList.Insert( newIndex, workflowActivityType );
                    }
                }

                int order = 0;
                foreach ( var item in workflowActivityTypeSortList )
                {
                    item.Order = order++;
                }
            }
            else if ( eventParam.Equals( "re-order-action" ) )
            {
                Guid workflowActionTypeGuid = new Guid( values[0] );
                int newIndex = int.Parse( values[1] );
                WorkflowActivityType workflowActivityType = workflowActivityTypeSortList.FirstOrDefault( a => a.ActionTypes.Any( b => b.Guid.Equals( workflowActionTypeGuid ) ) );
                if ( workflowActivityType != null )
                {
                    WorkflowActionType workflowActionType = workflowActivityType.ActionTypes.FirstOrDefault( a => a.Guid.Equals( workflowActionTypeGuid ) );
                    if ( workflowActionType != null )
                    {
                        activeWorkflowActivityTypeGuid = workflowActivityType.Guid;
                        List<WorkflowActionType> workflowActionTypes = workflowActivityType.ActionTypes.ToList();
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
                    }
                }
            }
            else if ( eventParam.Equals( "re-order-formfield" ) )
            {
                Guid workflowFormFieldGuid = new Guid( values[0] );
                int newIndex = int.Parse( values[1] );
                WorkflowActivityType workflowActivityType = workflowActivityTypeSortList
                    .FirstOrDefault( a => 
                        a.ActionTypes.Any( b => 
                            b.WorkflowForm != null &&
                            b.WorkflowForm.FormAttributes.Any ( c =>
                                c.Guid.Equals(workflowFormFieldGuid) ) ) );
                if ( workflowActivityType != null )
                {
                    WorkflowActionType workflowActionType = workflowActivityType.ActionTypes
                        .FirstOrDefault( a =>
                            a.WorkflowForm != null &&
                            a.WorkflowForm.FormAttributes != null &&
                            a.WorkflowForm.FormAttributes.Any( b =>
                                b.Guid.Equals( workflowFormFieldGuid ) ) );
                    if ( workflowActionType != null )
                    {
                        WorkflowActionFormAttribute workflowFormAttribute = workflowActionType.WorkflowForm.FormAttributes
                            .Where( a => a.Guid.Equals( workflowFormFieldGuid ) )
                            .FirstOrDefault();
                        if ( workflowFormAttribute != null )
                        {
                            activeWorkflowActivityTypeGuid = workflowActivityType.Guid;
                            activeWorkflowActionTypeGuid = workflowActionType.Guid;

                            List<WorkflowActionFormAttribute> workflowFormAttributes = workflowActionType.WorkflowForm.FormAttributes.ToList();
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
                        }
                    }
                }
            }

            WorkflowActivityTypesState = new ViewStateList<WorkflowActivityType>();
            WorkflowActivityTypesState.AddAll( workflowActivityTypeSortList );
            BuildWorkflowActivityControlsFromViewState( activeWorkflowActivityTypeGuid, activeWorkflowActionTypeGuid );
        }

        #endregion

        #region Edit Events

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

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            WorkflowTypeService service = new WorkflowTypeService( rockContext );
            WorkflowType item = service.Get( int.Parse( hfWorkflowTypeId.Value ) );
            ShowEditDetails( item, rockContext );
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
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            WorkflowType workflowType;
            WorkflowTypeService service = new WorkflowTypeService( rockContext );

            int workflowTypeId = int.Parse( hfWorkflowTypeId.Value );

            if ( workflowTypeId == 0 )
            {
                workflowType = new WorkflowType();
                workflowType.IsSystem = false;
                workflowType.Name = string.Empty;
            }
            else
            {
                workflowType = service.Get( workflowTypeId );
            }

            workflowType.Name = tbName.Text;
            workflowType.Description = tbDescription.Text;
            workflowType.CategoryId = cpCategory.SelectedValueAsInt();
            workflowType.Order = 0;
            workflowType.WorkTerm = tbWorkTerm.Text;
            if ( !string.IsNullOrWhiteSpace( tbProcessingInterval.Text ) )
            {
                workflowType.ProcessingIntervalSeconds = int.Parse( tbProcessingInterval.Text );
            }

            workflowType.IsPersisted = cbIsPersisted.Checked;
            workflowType.LoggingLevel = ddlLoggingLevel.SelectedValueAsEnum<WorkflowLoggingLevel>();
            workflowType.IsActive = cbIsActive.Checked;

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !workflowType.IsValid )
            {
                // Controls will render the error messages                    
                return;
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
                List<WorkflowActivityEditor> workflowActivityTypeEditorList = phActivities.Controls.OfType<WorkflowActivityEditor>().ToList();
                List<WorkflowActionType> actionTypesInDB = workflowActionTypeService.Queryable().Where( a => a.ActivityType.WorkflowTypeId.Equals( workflowType.Id ) ).ToList();
                List<WorkflowActionType> actionTypesInUI = new List<WorkflowActionType>();
                foreach ( WorkflowActivityEditor workflowActivityTypeEditor in workflowActivityTypeEditorList )
                {
                    foreach ( WorkflowActionEditor editor in workflowActivityTypeEditor.Controls.OfType<WorkflowActionEditor>() )
                    {
                        actionTypesInUI.Add( editor.WorkflowActionType );
                    }
                }
                var deletedActionTypes = from actionType in actionTypesInDB
                                         where !actionTypesInUI.Select( u => u.Guid ).Contains( actionType.Guid )
                                         select actionType;
                deletedActionTypes.ToList().ForEach( actionType =>
                {
                    if (actionType.WorkflowForm != null)
                    {
                        workflowFormService.Delete( actionType.WorkflowForm );
                    }
                    workflowActionTypeService.Delete( actionType );
                } );
                rockContext.SaveChanges();

                // delete WorkflowActivityTypes that aren't assigned in the UI anymore
                List<WorkflowActivityType> activityTypesInDB = workflowActivityTypeService.Queryable().Where( a => a.WorkflowTypeId.Equals( workflowType.Id ) ).ToList();
                List<WorkflowActivityType> activityTypesInUI = workflowActivityTypeEditorList.Select( a => a.GetWorkflowActivityType() ).ToList();
                var deletedActivityTypes = from activityType in activityTypesInDB
                                           where !activityTypesInUI.Select( u => u.Guid ).Contains( activityType.Guid )
                                           select activityType;
                deletedActivityTypes.ToList().ForEach( activityType =>
                {
                    workflowActivityTypeService.Delete( activityType );
                } );
                rockContext.SaveChanges();

                // add or update WorkflowActivityTypes(and Actions) that are assigned in the UI
                int workflowActivityTypeOrder = 0;
                foreach ( WorkflowActivityEditor workflowActivityTypeEditor in workflowActivityTypeEditorList )
                {
                    // Add or Update the activity type
                    WorkflowActivityType editorWorkflowActivityType = workflowActivityTypeEditor.GetWorkflowActivityType();
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
                    foreach ( WorkflowActionEditor workflowActionTypeEditor in workflowActivityTypeEditor.Controls.OfType<WorkflowActionEditor>() )
                    {
                        WorkflowActionType editorWorkflowActionType = workflowActionTypeEditor.WorkflowActionType;
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

                        if (editorWorkflowActionType.WorkflowForm != null)
                        {
                            if (workflowActionType.WorkflowForm == null)
                            {
                                workflowActionType.WorkflowForm = new WorkflowActionForm();
                            }

                            workflowActionType.WorkflowForm.Header = editorWorkflowActionType.WorkflowForm.Header;
                            workflowActionType.WorkflowForm.Footer = editorWorkflowActionType.WorkflowForm.Footer;
                            workflowActionType.WorkflowForm.InactiveMessage = editorWorkflowActionType.WorkflowForm.InactiveMessage;
                            workflowActionType.WorkflowForm.Actions = editorWorkflowActionType.WorkflowForm.Actions;

                            var editorGuids = editorWorkflowActionType.WorkflowForm.FormAttributes
                                .Select( a => a.Attribute.Guid)
                                .ToList();

                            foreach( var formAttribute in workflowActionType.WorkflowForm.FormAttributes
                                .Where( a => !editorGuids.Contains(a.Attribute.Guid) ) )
                            {
                                workflowFormAttributeService.Delete(formAttribute);
                            }

                            int attributeOrder = 0;
                            foreach ( var editorAttribute in editorWorkflowActionType.WorkflowForm.FormAttributes.OrderBy( a => a.Order ) )
                            {
                                int attributeId = AttributeCache.Read( editorAttribute.Attribute.Guid ).Id;

                                var formAttribute = workflowActionType.WorkflowForm.FormAttributes
                                    .Where( a => a.AttributeId == attributeId )
                                    .FirstOrDefault();

                                if (formAttribute == null)
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


        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlLoggingLevel.BindToEnum( typeof( WorkflowLoggingLevel ) );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        /// <param name="parentCategoryId">The parent category id.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? parentCategoryId )
        {
            if ( !itemKey.Equals( "workflowTypeId" ) )
            {
                pnlDetails.Visible = false;
                return;
            }

            var rockContext = new RockContext();

            WorkflowType workflowType = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                workflowType = new WorkflowTypeService( rockContext ).Get( itemKeyValue );
            }
            else
            {
                workflowType = new WorkflowType { Id = 0, IsActive = true, IsPersisted = true, IsSystem = false, CategoryId = parentCategoryId };
                workflowType.ActivityTypes.Add( new WorkflowActivityType { Guid = Guid.NewGuid(), IsActive = true } );
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
        private void ShowEditDetails( WorkflowType workflowType, RockContext rockContext )
        {
            if ( workflowType.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( WorkflowType.FriendlyTypeName ).FormatAsHtmlTitle();
                hlInactive.Visible = false;
            }

            SetEditMode( true );

            LoadDropDowns();

            tbName.Text = workflowType.Name;
            tbDescription.Text = workflowType.Description;
            cbIsActive.Checked = workflowType.IsActive ?? false;
            cpCategory.SetValue( workflowType.CategoryId );
            tbWorkTerm.Text = workflowType.WorkTerm;
            tbProcessingInterval.Text = workflowType.ProcessingIntervalSeconds != null ? workflowType.ProcessingIntervalSeconds.ToString() : string.Empty;
            cbIsPersisted.Checked = workflowType.IsPersisted;
            ddlLoggingLevel.SetValue( (int)workflowType.LoggingLevel );

            var attributeService = new AttributeService( rockContext );
            AttributesState = new ViewStateList<Attribute>();
            AttributesState.AddAll( attributeService.GetByEntityTypeId( new Workflow().TypeId ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( workflowType.Id.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList() );
            BindAttributesGrid();

            phActivities.Controls.Clear();
            foreach ( WorkflowActivityType workflowActivityType in workflowType.ActivityTypes.OrderBy( a => a.Order ) )
            {
                CreateWorkflowActivityTypeEditorControls( workflowActivityType );
            }

            RefreshActivityLists();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="workflowType">Type of the workflow.</param>
        private void ShowReadonlyDetails( WorkflowType workflowType )
        {
            SetEditMode( false );
            hfWorkflowTypeId.SetValue( workflowType.Id );
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
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        private void SetAttributeListOrder( ViewStateList<Attribute> itemList )
        {
            int order = 0;
            itemList.OrderBy( a => a.Order ).ToList().ForEach( a => a.Order = order++ );
        }

        /// <summary>
        /// Reorders the attribute list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void ReorderAttributeList( ViewStateList<Attribute> itemList, int oldIndex, int newIndex )
        {
            var movedItem = itemList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in itemList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in itemList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        private void SaveAttributes( int entityTypeId, string qualifierColumn, string qualifierValue, ViewStateList<Attribute> viewStateAttributes, RockContext rockContext )
        {
            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var attributes = attributeService.Get( entityTypeId, qualifierColumn, qualifierValue );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = viewStateAttributes.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
                rockContext.SaveChanges();
                Rock.Web.Cache.AttributeCache.Flush( attr.Id );
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in viewStateAttributes )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeId, qualifierColumn, qualifierValue, rockContext );
            }
        }

        #endregion

        #region Attributes Grid and Picker

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
            ReorderAttributeList( AttributesState, e.OldIndex, e.NewIndex );
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

            BindAttributesGrid();
            HideDialog();
        }

        /// <summary>
        /// Binds the group type attributes grid.
        /// </summary>
        private void BindAttributesGrid()
        {
            gAttributes.AddCssClass( "attribute-grid" );
            SetAttributeListOrder( AttributesState );
            gAttributes.DataSource = AttributesState.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            gAttributes.DataBind();

            var workflowAttributes = new Dictionary<Guid, string>();
            AttributesState.OrderBy( a => a.Order).ToList().ForEach( a => workflowAttributes.Add( a.Guid, a.Name ) );

            foreach ( var activityEditor in phActivities.Controls.OfType<WorkflowActivityEditor>() )
            {
                foreach ( WorkflowActionEditor workflowActionTypeEditor in activityEditor.Controls.OfType<WorkflowActionEditor>() )
                {
                    workflowActionTypeEditor.WorkflowAttributes = workflowAttributes;
                }
            }
        }

        private void RefreshActivityLists()
        {
            var activityControls = new List<WorkflowActivityEditor>();
            var activities = new Dictionary<Guid, string>();

            foreach ( var activityEditor in phActivities.Controls.OfType<WorkflowActivityEditor>() )
            {
                activityControls.Add(activityEditor);
                activities.Add(activityEditor.ActivityTypeGuid, activityEditor.Name);
            }

            foreach( var activityEditor in activityControls)
            {
                var workflowActivities = new Dictionary<string, string>();
                activities
                    .Where( a => !a.Key.Equals( activityEditor.ActivityTypeGuid ) )
                    .ToList()
                    .ForEach( a => workflowActivities.Add( a.Key.ToString().ToUpper(), a.Value ) );

                foreach ( WorkflowActionEditor workflowActionTypeEditor in activityEditor.Controls.OfType<WorkflowActionEditor>() )
                {
                    workflowActionTypeEditor.WorkflowActivities = workflowActivities;
                }
            }
        }

        #endregion

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

        #region Activities and Actions

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            BuildWorkflowActivityControlsFromViewState();
        }

        /// <summary>
        /// Builds the state of the workflow activity controls from view.
        /// </summary>
        private void BuildWorkflowActivityControlsFromViewState( Guid? activeWorkflowActivityTypeGuid = null, Guid? activeWorkflowActionTypeGuid = null )
        {
            phActivities.Controls.Clear();

            foreach ( WorkflowActivityType workflowActivityType in WorkflowActivityTypesState )
            {
                CreateWorkflowActivityTypeEditorControls( workflowActivityType, workflowActivityType.Guid.Equals( activeWorkflowActivityTypeGuid ?? Guid.Empty ), activeWorkflowActionTypeGuid );
            }

            RefreshActivityLists();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            if ( AttributesState != null )
            {
                AttributesState.SaveViewState();
            }

            SaveWorkflowActivityControlsToViewState();

            return base.SaveViewState();
        }

        /// <summary>
        /// Saves the state of the workflow activity controls to view.
        /// </summary>
        private void SaveWorkflowActivityControlsToViewState()
        {
            var activityTypes = new List<WorkflowActivityType>();
            int order = 0;
            foreach ( var activityEditor in phActivities.Controls.OfType<WorkflowActivityEditor>() )
            {
                WorkflowActivityType workflowActivityType = activityEditor.GetWorkflowActivityType();
                workflowActivityType.Order = order++;
                activityTypes.Add( workflowActivityType );
            }

            WorkflowActivityTypesState = new ViewStateList<WorkflowActivityType>();
            WorkflowActivityTypesState.AddAll( activityTypes );
        }

        /// <summary>
        /// Handles the Click event of the lbAddActivity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddActivityType_Click( object sender, EventArgs e )
        {
            WorkflowActivityType workflowActivityType = new WorkflowActivityType();
            workflowActivityType.Guid = Guid.NewGuid();
            workflowActivityType.IsActive = true;

            CreateWorkflowActivityTypeEditorControls( workflowActivityType );

            RefreshActivityLists();
        }

        /// <summary>
        /// Creates the workflow activity type editor control.
        /// </summary>
        /// <param name="workflowActivityType">Type of the workflow activity.</param>
        /// <param name="forceContentVisible">if set to <c>true</c> [force content visible].</param>
        private void CreateWorkflowActivityTypeEditorControls( WorkflowActivityType workflowActivityType, bool forceContentVisible = false, Guid? activeWorkflowActionTypeGuid = null )
        {
            WorkflowActivityEditor workflowActivityTypeEditor = new WorkflowActivityEditor();
            workflowActivityTypeEditor.ID = "WorkflowActivityTypeEditor_" + workflowActivityType.Guid.ToString( "N" );
            workflowActivityTypeEditor.SetWorkflowActivityType( workflowActivityType );
            workflowActivityTypeEditor.DeleteActivityTypeClick += workflowActivityTypeEditor_DeleteActivityClick;
            workflowActivityTypeEditor.AddActionTypeClick += workflowActivityTypeEditor_AddActionTypeClick;
            workflowActivityTypeEditor.ForceContentVisible = forceContentVisible;
            foreach ( WorkflowActionType actionType in workflowActivityType.ActionTypes.OrderBy( a => a.Order ) )
            {
                CreateWorkflowActionTypeEditorControl( workflowActivityTypeEditor, actionType, actionType.Guid.Equals( activeWorkflowActionTypeGuid ?? Guid.Empty ) );
            }

            phActivities.Controls.Add( workflowActivityTypeEditor );
        }

        /// <summary>
        /// Handles the AddActionTypeClick event of the workflowActivityTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void workflowActivityTypeEditor_AddActionTypeClick( object sender, EventArgs e )
        {
            if ( sender is WorkflowActivityEditor )
            {
                WorkflowActivityEditor workflowActivityTypeEditor = sender as WorkflowActivityEditor;
                var actionEditor = CreateWorkflowActionTypeEditorControl( workflowActivityTypeEditor, new WorkflowActionType { Guid = Guid.NewGuid() } );

                var workflowActivities = new Dictionary<string, string>();
                foreach ( var activityEditor in phActivities.Controls.OfType<WorkflowActivityEditor>() )
                {
                    if (!activityEditor.ActivityTypeGuid.Equals(workflowActivityTypeEditor.ActivityTypeGuid))
                    {
                        workflowActivities.Add( activityEditor.ActivityTypeGuid.ToString().ToUpper(), activityEditor.Name );
                    }
                }
                actionEditor.WorkflowActivities = workflowActivities;
            }
        }

        /// <summary>
        /// Creates the workflow action type editor control.
        /// </summary>
        /// <param name="workflowActivityTypeEditor">The workflow activity type editor.</param>
        /// <param name="workflowActionType">Type of the workflow action.</param>
        private WorkflowActionEditor CreateWorkflowActionTypeEditorControl( WorkflowActivityEditor workflowActivityTypeEditor, WorkflowActionType workflowActionType, bool forceContentVisible = false )
        {
            WorkflowActionEditor workflowActionTypeEditor = new WorkflowActionEditor();
            workflowActionTypeEditor.ID = "WorkflowActionTypeEditor_" + workflowActionType.Guid.ToString( "N" );
            workflowActionTypeEditor.DeleteActionTypeClick += workflowActionTypeEditor_DeleteActionTypeClick;
            
            var workflowAttributes = new Dictionary<Guid, string>();
            AttributesState.OrderBy( a => a.Order).ToList().ForEach( a => workflowAttributes.Add( a.Guid, a.Name ) );
            workflowActionTypeEditor.WorkflowAttributes = workflowAttributes;

            workflowActionTypeEditor.WorkflowActionType = workflowActionType;
            workflowActionTypeEditor.ForceContentVisible = forceContentVisible;

            workflowActivityTypeEditor.Controls.Add( workflowActionTypeEditor );

            return workflowActionTypeEditor;
        }

        /// <summary>
        /// Handles the DeleteActionTypeClick event of the workflowActionTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void workflowActionTypeEditor_DeleteActionTypeClick( object sender, EventArgs e )
        {
            if ( sender is WorkflowActionEditor )
            {
                WorkflowActionEditor workflowActionTypeEditor = sender as WorkflowActionEditor;
                WorkflowActivityEditor workflowActivityTypeEditor = workflowActionTypeEditor.Parent as WorkflowActivityEditor;
                workflowActivityTypeEditor.Controls.Remove( workflowActionTypeEditor );
            }
        }

        /// <summary>
        /// Handles the DeleteActivityClick event of the workflowActivityTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void workflowActivityTypeEditor_DeleteActivityClick( object sender, EventArgs e )
        {
            if ( sender is WorkflowActivityEditor )
            {
                WorkflowActivityEditor editor = sender as WorkflowActivityEditor;
                if ( editor != null )
                {
                    phActivities.Controls.Remove( editor );
                }
            }

            RefreshActivityLists();
        }

        #endregion
    }
}