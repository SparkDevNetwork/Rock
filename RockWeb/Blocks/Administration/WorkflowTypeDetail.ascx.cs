//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class WorkflowTypeDetail : RockBlock, IDetailBlock
    {
        #region WorkflowActivityType ViewStateList

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
            gWorkflowTypeAttributes.DataKeyNames = new string[] { "Guid" };
            gWorkflowTypeAttributes.Actions.ShowAdd = true;
            gWorkflowTypeAttributes.Actions.AddClick += gWorkflowTypeAttributes_Add;
            gWorkflowTypeAttributes.GridRebind += gWorkflowTypeAttributes_GridRebind;
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
                string parentCategoryId = PageParameter( "parentCategoryId" );
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

            string postbackArgs = Request.Params["__EVENTARGUMENT"];
            if ( !string.IsNullOrWhiteSpace( postbackArgs ) )
            {
                string[] nameValue = postbackArgs.Split( new char[] { ':' } );
                if ( nameValue.Count() == 2 )
                {
                    string eventParam = nameValue[0];
                    if ( eventParam.Equals( "re-order-activity" ) || eventParam.Equals( "re-order-action" ) )
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
            SaveWorkflowActivityControlsToViewState();
            List<WorkflowActivityType> workflowActivityTypeSortList = WorkflowActivityTypesState.ToList();
            Guid? activeWorkflowActivityTypeGuid = null;

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
                WorkflowActionType workflowActionType = workflowActivityType.ActionTypes.FirstOrDefault( a => a.Guid.Equals( workflowActionTypeGuid ) );
                workflowActivityTypeSortList.RemoveEntity( workflowActionTypeGuid );
                if ( workflowActivityType != null )
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

            WorkflowActivityTypesState = new ViewStateList<WorkflowActivityType>();
            WorkflowActivityTypesState.AddAll( workflowActivityTypeSortList );
            BuildWorkflowActivityControlsFromViewState( activeWorkflowActivityTypeGuid );
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
                // Cancelling on Add.  Return to tree view with parent category selected
                NavigateToParentPage();

                var qryParams = new Dictionary<string, string>();

                string parentCategoryId = PageParameter( "parentCategoryId" );
                if ( !string.IsNullOrWhiteSpace( parentCategoryId ) )
                {
                    qryParams["CategoryId"] = parentCategoryId;
                }
                NavigateToPage( this.CurrentPage.Guid, qryParams );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                WorkflowTypeService service = new WorkflowTypeService();
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
            WorkflowTypeService service = new WorkflowTypeService();
            WorkflowType item = service.Get( int.Parse( hfWorkflowTypeId.Value ) );
            ShowEditDetails( item );
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
            WorkflowType workflowType;
            WorkflowTypeService service = new WorkflowTypeService();

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
            workflowType.Order = int.Parse( tbOrder.Text );
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
                List<WorkflowActivityTypeEditor> workflowActivityTypeEditorList = phActivities.Controls.OfType<WorkflowActivityTypeEditor>().ToList();

                // delete WorkflowActionTypes that aren't assigned in the UI anymore
                WorkflowActionTypeService workflowActionTypeService = new WorkflowActionTypeService();
                List<WorkflowActionType> actionTypesInDB = workflowActionTypeService.Queryable().Where( a => a.ActivityType.WorkflowTypeId.Equals( workflowType.Id ) ).ToList();
                List<WorkflowActionType> actionTypesInUI = new List<WorkflowActionType>();
                foreach ( WorkflowActivityTypeEditor workflowActivityTypeEditor in workflowActivityTypeEditorList )
                {
                    foreach ( WorkflowActionTypeEditor editor in workflowActivityTypeEditor.Controls.OfType<WorkflowActionTypeEditor>() )
                    {
                        actionTypesInUI.Add( editor.WorkflowActionType );
                    }
                }

                var deletedActionTypes = from actionType in actionTypesInDB
                                         where !actionTypesInUI.Select( u => u.Guid ).Contains( actionType.Guid )
                                         select actionType;

                deletedActionTypes.ToList().ForEach( actionType =>
                {
                    workflowActionTypeService.Delete( actionType, CurrentPersonId );
                    workflowActionTypeService.Save( actionType, CurrentPersonId );
                } );

                // delete WorkflowActivityTypes that aren't assigned in the UI anymore
                WorkflowActivityTypeService workflowActivityTypeService = new WorkflowActivityTypeService();
                List<WorkflowActivityType> activityTypesInDB = workflowActivityTypeService.Queryable().Where( a => a.WorkflowTypeId.Equals( workflowType.Id ) ).ToList();
                List<WorkflowActivityType> activityTypesInUI = workflowActivityTypeEditorList.Select( a => a.GetWorkflowActivityType() ).ToList();

                var deletedActivityTypes = from activityType in activityTypesInDB
                                           where !activityTypesInUI.Select( u => u.Guid ).Contains( activityType.Guid )
                                           select activityType;

                deletedActivityTypes.ToList().ForEach( activityType =>
                {
                    workflowActivityTypeService.Delete( activityType, CurrentPersonId );
                    workflowActivityTypeService.Save( activityType, CurrentPersonId );
                } );

                // add or update WorkflowActivityTypes(and Actions) that are assigned in the UI
                int workflowActivityTypeOrder = 0;
                foreach ( WorkflowActivityTypeEditor workflowActivityTypeEditor in workflowActivityTypeEditorList )
                {
                    WorkflowActivityType editorWorkflowActivityType = workflowActivityTypeEditor.GetWorkflowActivityType();
                    WorkflowActivityType workflowActivityType = workflowType.ActivityTypes.FirstOrDefault( a => a.Guid.Equals( editorWorkflowActivityType.Guid ) );

                    if ( workflowActivityType == null )
                    {
                        workflowActivityType = editorWorkflowActivityType;
                        workflowType.ActivityTypes.Add( workflowActivityType );
                    }
                    else
                    {
                        workflowActivityType.Name = editorWorkflowActivityType.Name;
                        workflowActivityType.Description = editorWorkflowActivityType.Description;
                        workflowActivityType.IsActive = editorWorkflowActivityType.IsActive;
                        workflowActivityType.IsActivatedWithWorkflow = editorWorkflowActivityType.IsActivatedWithWorkflow;
                    }

                    workflowActivityType.Order = workflowActivityTypeOrder++;

                    int workflowActionTypeOrder = 0;
                    foreach ( WorkflowActionTypeEditor workflowActionTypeEditor in workflowActivityTypeEditor.Controls.OfType<WorkflowActionTypeEditor>() )
                    {
                        WorkflowActionType editorWorkflowActionType = workflowActionTypeEditor.WorkflowActionType;
                        WorkflowActionType workflowActionType = workflowActivityType.ActionTypes.FirstOrDefault( a => a.Guid.Equals( editorWorkflowActionType.Guid ) );
                        if ( workflowActionType == null )
                        {
                            workflowActionType = editorWorkflowActionType;
                            workflowActivityType.ActionTypes.Add( workflowActionType );
                        }
                        else
                        {
                            workflowActionType.Name = editorWorkflowActionType.Name;
                            workflowActionType.EntityTypeId = editorWorkflowActionType.EntityTypeId;
                            workflowActionType.IsActionCompletedOnSuccess = editorWorkflowActionType.IsActionCompletedOnSuccess;
                            workflowActionType.IsActivityCompletedOnSuccess = editorWorkflowActionType.IsActivityCompletedOnSuccess;
                            workflowActionType.Attributes = editorWorkflowActionType.Attributes;
                            workflowActionType.AttributeValues = editorWorkflowActionType.AttributeValues;
                        }
                        

                        workflowActionType.Order = workflowActionTypeOrder++;
                    }
                }

                if ( workflowType.Id.Equals( 0 ) )
                {
                    service.Add( workflowType, CurrentPersonId );
                }

                service.Save( workflowType, CurrentPersonId );

                foreach ( var activityType in workflowType.ActivityTypes )
                {
                    foreach ( var workflowActionType in activityType.ActionTypes )
                    {
                        Rock.Attribute.Helper.SaveAttributeValues( workflowActionType, CurrentPersonId );
                    }
                }

                
            } );
            
            var qryParams = new Dictionary<string, string>();
            qryParams["workflowTypeId"] = workflowType.Id.ToString();
            NavigateToPage( this.CurrentPage.Guid, qryParams );
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

            WorkflowType workflowType = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                workflowType = new WorkflowTypeService().Get( itemKeyValue );
            }
            else
            {
                workflowType = new WorkflowType { Id = 0, IsActive = true, IsPersisted = true, IsSystem = false, CategoryId = parentCategoryId };
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
            if ( !IsUserAuthorized( "Edit" ) )
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
                    ShowEditDetails( workflowType );
                }
            }

            BindWorkflowTypeAttributesGrid();
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="workflowType">Type of the workflow.</param>
        private void ShowEditDetails( WorkflowType workflowType )
        {
            if ( workflowType.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( WorkflowType.FriendlyTypeName );
            }

            SetEditMode( true );

            LoadDropDowns();

            tbName.Text = workflowType.Name;
            tbDescription.Text = workflowType.Description;
            cbIsActive.Checked = workflowType.IsActive ?? false;
            cpCategory.SetValue( workflowType.CategoryId );
            tbWorkTerm.Text = workflowType.WorkTerm;
            tbOrder.Text = workflowType.Order.ToString();
            tbProcessingInterval.Text = workflowType.ProcessingIntervalSeconds != null ? workflowType.ProcessingIntervalSeconds.ToString() : string.Empty;
            cbIsPersisted.Checked = workflowType.IsPersisted;
            ddlLoggingLevel.SetValue( (int)workflowType.LoggingLevel );

            phActivities.Controls.Clear();
            foreach ( WorkflowActivityType workflowActivityType in workflowType.ActivityTypes.OrderBy( a => a.Order ) )
            {
                CreateWorkflowActivityTypeEditorControls( workflowActivityType );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="workflowType">Type of the workflow.</param>
        private void ShowReadonlyDetails( WorkflowType workflowType )
        {
            SetEditMode( false );
            hfWorkflowTypeId.SetValue( workflowType.Id );
            lReadOnlyTitle.Text = workflowType.Name;
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

        #endregion

        #region WorkflowTypeAttributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gWorkflowTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gWorkflowTypeAttributes_Add( object sender, EventArgs e )
        {
            gWorkflowTypeAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gWorkflowTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gWorkflowTypeAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            gWorkflowTypeAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Gs the workflow type attributes_ show edit.
        /// </summary>
        /// <param name="attributeGuid">The attribute GUID.</param>
        protected void gWorkflowTypeAttributes_ShowEdit( Guid attributeGuid )
        {
            pnlDetails.Visible = false;
            vsDetails.Enabled = false;
            pnlWorkflowTypeAttributes.Visible = true;

            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                edtWorkflowTypeAttributes.ActionTitle = ActionTitle.Add( "attribute for workflow type " + tbName.Text );
            }
            else
            {
                AttributeService attributeService = new AttributeService();
                attribute = attributeService.Get( attributeGuid );
                edtWorkflowTypeAttributes.ActionTitle = ActionTitle.Edit( "attribute for workflow type " + tbName.Text );
            }

            edtWorkflowTypeAttributes.SetAttributeProperties( attribute );
        }

        /// <summary>
        /// Handles the Delete event of the gWorkflowTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gWorkflowTypeAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = (Guid)e.RowKeyValue;
            AttributeService attributeService = new AttributeService();
            Attribute attribute = attributeService.Get( attributeGuid );

            if ( attribute != null )
            {
                string errorMessage;
                if ( !attributeService.CanDelete( attribute, out errorMessage ) )
                {
                    mdGridWarningAttributes.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
                attributeService.Delete( attribute, CurrentPersonId );
                attributeService.Save( attribute, CurrentPersonId );
            }

            // reload page so that other blocks respond to any data that was changed
            var qryParams = new Dictionary<string, string>();
            qryParams["workflowTypeId"] = hfWorkflowTypeId.Value;
            NavigateToPage( this.CurrentPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the GridRebind event of the gWorkflowTypeAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gWorkflowTypeAttributes_GridRebind( object sender, EventArgs e )
        {
            BindWorkflowTypeAttributesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveWorkflowTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveWorkflowTypeAttribute_Click( object sender, EventArgs e )
        {
            Attribute attribute = null;
            AttributeService attributeService = new AttributeService();
            if ( edtWorkflowTypeAttributes.AttributeId.HasValue )
            {
                attribute = attributeService.Get( edtWorkflowTypeAttributes.AttributeId.Value );
            }

            if (attribute == null)
            {
                attribute = new Attribute();
            }

            edtWorkflowTypeAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                if ( attribute.Id.Equals( 0 ) )
                {
                    attribute.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( typeof( Workflow ) ).Id;
                    attribute.EntityTypeQualifierColumn = "WorkflowTypeId";
                    attribute.EntityTypeQualifierValue = hfWorkflowTypeId.Value;
                    attributeService.Add( attribute, CurrentPersonId );
                }

                Rock.Web.Cache.AttributeCache.Flush( attribute.Id );
                attributeService.Save( attribute, CurrentPersonId );
            } );

            pnlDetails.Visible = true;
            pnlWorkflowTypeAttributes.Visible = false;

            // reload page so that other blocks respond to any data that was changed
            var qryParams = new Dictionary<string, string>();
            qryParams["workflowTypeId"] = hfWorkflowTypeId.Value;
            NavigateToPage( this.CurrentPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelWorkflowTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelWorkflowTypeAttribute_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = true;
            pnlWorkflowTypeAttributes.Visible = false;
        }

        /// <summary>
        /// Binds the workflow type attributes grid.
        /// </summary>
        private void BindWorkflowTypeAttributesGrid()
        {
            AttributeService attributeService = new AttributeService();

            int workflowTypeId = hfWorkflowTypeId.ValueAsInt();

            string qualifierValue = workflowTypeId.ToString();
            var qryWorkflowTypeAttributes = attributeService.GetByEntityTypeId( new Workflow().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( qualifierValue ) );

            gWorkflowTypeAttributes.DataSource = qryWorkflowTypeAttributes.OrderBy( a => a.Name ).ToList();
            gWorkflowTypeAttributes.DataBind();
        }

        #endregion

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
        private void BuildWorkflowActivityControlsFromViewState( Guid? activeWorkflowActivityTypeGuid = null )
        {
            phActivities.Controls.Clear();

            foreach ( WorkflowActivityType workflowActivityType in WorkflowActivityTypesState )
            {
                CreateWorkflowActivityTypeEditorControls( workflowActivityType, workflowActivityType.Guid.Equals( activeWorkflowActivityTypeGuid ?? Guid.Empty ) );
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
            SaveWorkflowActivityControlsToViewState();

            return base.SaveViewState();
        }

        /// <summary>
        /// Saves the state of the workflow activity controls to view.
        /// </summary>
        private void SaveWorkflowActivityControlsToViewState()
        {
            WorkflowActivityTypesState = new ViewStateList<WorkflowActivityType>();
            int order = 0;
            foreach ( var activityEditor in phActivities.Controls.OfType<WorkflowActivityTypeEditor>() )
            {
                WorkflowActivityType workflowActivityType = activityEditor.GetWorkflowActivityType();
                workflowActivityType.Order = order++;
                WorkflowActivityTypesState.Add( workflowActivityType );
            }
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
        }

        /// <summary>
        /// Creates the workflow activity type editor control.
        /// </summary>
        /// <param name="workflowActivityType">Type of the workflow activity.</param>
        /// <param name="forceContentVisible">if set to <c>true</c> [force content visible].</param>
        private void CreateWorkflowActivityTypeEditorControls( WorkflowActivityType workflowActivityType, bool forceContentVisible = false )
        {
            WorkflowActivityTypeEditor workflowActivityTypeEditor = new WorkflowActivityTypeEditor();
            workflowActivityTypeEditor.ID = "WorkflowActivityTypeEditor_" + workflowActivityType.Guid.ToString( "N" );
            workflowActivityTypeEditor.SetWorkflowActivityType( workflowActivityType );
            workflowActivityTypeEditor.DeleteActivityTypeClick += workflowActivityTypeEditor_DeleteActivityClick;
            workflowActivityTypeEditor.AddActionTypeClick += workflowActivityTypeEditor_AddActionTypeClick;
            workflowActivityTypeEditor.ForceContentVisible = forceContentVisible;
            foreach ( WorkflowActionType actionType in workflowActivityType.ActionTypes.OrderBy( a => a.Order ) )
            {
                CreateWorkflowActionTypeEditorControl( workflowActivityTypeEditor, actionType );
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
            if ( sender is WorkflowActivityTypeEditor )
            {
                WorkflowActivityTypeEditor workflowActivityTypeEditor = sender as WorkflowActivityTypeEditor;
                CreateWorkflowActionTypeEditorControl( workflowActivityTypeEditor, new WorkflowActionType { Guid = Guid.NewGuid() } );
            }
        }

        /// <summary>
        /// Creates the workflow action type editor control.
        /// </summary>
        /// <param name="workflowActivityTypeEditor">The workflow activity type editor.</param>
        /// <param name="workflowActionType">Type of the workflow action.</param>
        private void CreateWorkflowActionTypeEditorControl( WorkflowActivityTypeEditor workflowActivityTypeEditor, WorkflowActionType workflowActionType )
        {
            WorkflowActionTypeEditor workflowActionTypeEditor = new WorkflowActionTypeEditor();
            workflowActionTypeEditor.ID = "WorkflowActionTypeEditor_" + workflowActionType.Guid.ToString( "N" );
            workflowActionTypeEditor.DeleteActionTypeClick += workflowActionTypeEditor_DeleteActionTypeClick;
            workflowActionTypeEditor.WorkflowActionType = workflowActionType;
            workflowActivityTypeEditor.Controls.Add( workflowActionTypeEditor );
        }

        /// <summary>
        /// Handles the DeleteActionTypeClick event of the workflowActionTypeEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void workflowActionTypeEditor_DeleteActionTypeClick( object sender, EventArgs e )
        {
            if ( sender is WorkflowActionTypeEditor )
            {
                WorkflowActionTypeEditor workflowActionTypeEditor = sender as WorkflowActionTypeEditor;
                WorkflowActivityTypeEditor workflowActivityTypeEditor = workflowActionTypeEditor.Parent as WorkflowActivityTypeEditor;
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
            if ( sender is WorkflowActivityTypeEditor )
            {
                WorkflowActivityTypeEditor editor = sender as WorkflowActivityTypeEditor;
                if ( editor != null )
                {
                    phActivities.Controls.Remove( editor );
                }
            }
        }

        #endregion
    }
}