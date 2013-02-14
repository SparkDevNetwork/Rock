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
            gWorkflowTypeAttributes.Actions.IsAddEnabled = true;
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
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "workflowTypeId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
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
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
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
            workflowType.CategoryId = ddlCategory.SelectedValueAsInt();
            workflowType.Order = int.Parse( tbOrder.Text );
            workflowType.WorkTerm = tbWorkTerm.Text;
            if ( !string.IsNullOrWhiteSpace( tbProcessingInterval.Text ) )
            {
                workflowType.ProcessingIntervalSeconds = int.Parse( tbProcessingInterval.Text );
            }

            workflowType.IsPersisted = cbIsPersisted.Checked;
            workflowType.LoggingLevel = ddlLoggingLevel.SelectedValueAsEnum<WorkflowLoggingLevel>();
            workflowType.IsActive = cbIsActive.Checked;

            // check for duplicates within Category
            if ( service.Queryable().Where( g => ( g.CategoryId == workflowType.CategoryId ) ).Count( a => a.Name.Equals( workflowType.Name, StringComparison.OrdinalIgnoreCase ) && !a.Id.Equals( workflowType.Id ) ) > 0 )
            {
                tbName.ShowErrorMessage( WarningMessage.DuplicateFoundMessage( "name", WorkflowType.FriendlyTypeName ) );
                return;
            }

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
                if ( workflowType.Id.Equals( 0 ) )
                {
                    service.Add( workflowType, CurrentPersonId );
                }

                service.Save( workflowType, CurrentPersonId );
            } );

            // reload item from db using a new context
            workflowType = new WorkflowTypeService().Get( workflowType.Id );
            ShowReadonlyDetails( workflowType );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            CategoryService categoryService = new CategoryService();
            var catList = categoryService.Queryable().OrderBy( a => a.Name ).ToList();
            catList.Insert( 0, new Category { Id = None.Id, Name = None.Text } );
            ddlCategory.DataSource = catList;
            ddlCategory.DataBind();

            ddlLoggingLevel.BindToEnum( typeof( WorkflowLoggingLevel ) );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
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
                workflowType = new WorkflowType { Id = 0, IsActive = true, IsSystem = false };
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
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( WorkflowType.FriendlyTypeName );
            }

            if ( workflowType.IsSystem )
            {
                readOnly = true;
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
            if ( workflowType.Id > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( WorkflowType.FriendlyTypeName );
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( WorkflowType.FriendlyTypeName );
            }

            SetEditMode( true );

            LoadDropDowns();

            tbName.Text = workflowType.Name;
            tbDescription.Text = workflowType.Description;
            cbIsActive.Checked = workflowType.IsActive ?? false;
            ddlCategory.SetValue( workflowType.CategoryId );
            tbWorkTerm.Text = workflowType.WorkTerm;
            tbOrder.Text = workflowType.Order.ToString();
            tbProcessingInterval.Text = workflowType.ProcessingIntervalSeconds != null ? workflowType.ProcessingIntervalSeconds.ToString() : string.Empty;
            cbIsPersisted.Checked = workflowType.IsPersisted;
            ddlLoggingLevel.SetValue( (int)workflowType.LoggingLevel );
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
            string activeHtmlFormat = "<span class='label {0} pull-right' >{1}</span>";
            if ( workflowType.IsActive ?? false )
            {
                lblActiveHtml.Text = string.Empty;
            }
            else
            {
                lblActiveHtml.Text = string.Format( activeHtmlFormat, "label-important", "Inactive" );
            }

            string descriptionFormat = "<dt>{0}</dt><dd>{1}</dd>";
            lblMainDetails.Text = @"
<div>
    <dl>";

            lblMainDetails.Text += string.Format( descriptionFormat, "Description", workflowType.Description );

            if ( workflowType.Category != null )
            {
                lblMainDetails.Text += string.Format( descriptionFormat, "Category", workflowType.Category.Name );
            }

            lblMainDetails.Text += @"
    </dl>
</div>";


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
            Actions:
            <ol>
                {2}
            </ol>
        </li>
";

                    string actionTypeText = string.Empty;

                    foreach ( var actionType in activityType.ActionTypes.OrderBy( a => a.Order ) )
                    {
                        actionTypeText += string.Format( "<li>{0}</li>" + Environment.NewLine, actionType.Name );
                    }

                    lblWorkflowActivitiesReadonly.Text += string.Format( activityTypeTextFormat, activityType.Name, activityType.Description, actionTypeText );
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
            string actionTitle;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                actionTitle = ActionTitle.Add( "attribute for workflow type " + tbName.Text );
            }
            else
            {
                AttributeService attributeService = new AttributeService();
                attribute = attributeService.Get( attributeGuid );
                actionTitle = ActionTitle.Edit( "attribute for workflow type " + tbName.Text );
            }

            edtWorkflowTypeAttributes.EditAttribute( attribute, actionTitle );
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
            Attribute attribute;
            AttributeService attributeService = new AttributeService();
            if ( edtWorkflowTypeAttributes.AttributeId.Equals( 0 ) )
            {
                attribute = new Attribute();
            }
            else
            {
                attribute = attributeService.Get( edtWorkflowTypeAttributes.AttributeId );
            }

            edtWorkflowTypeAttributes.GetAttributeValues( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                if ( attribute.Id.Equals( 0 ) )
                {
                    attribute.EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( new Workflow().TypeName ).Id;
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

            int WorkflowTypeId = hfWorkflowTypeId.ValueAsInt();

            var qryWorkflowTypeAttributes = attributeService.GetByEntityTypeId( new Workflow().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( WorkflowTypeId.ToString() ) );

            gWorkflowTypeAttributes.DataSource = qryWorkflowTypeAttributes.OrderBy( a => a.Name ).ToList();
            gWorkflowTypeAttributes.DataBind();
        }

        #endregion
        
        protected void lbAddActivity_Click( object sender, EventArgs e )
        {

        }
}
}