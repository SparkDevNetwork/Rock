//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

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
            workflowType.ProcessingIntervalSeconds = int.Parse( tbProcessingInterval.Text );
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
            catList.Insert( 0, new Category { Id = None.Id, Name = None.TextHtml } );
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

            pnlDetails.Visible = true;
            WorkflowType workflowType = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                workflowType = new WorkflowTypeService().Get( itemKeyValue );
            }
            else
            {
                workflowType = new WorkflowType { Id = 0, IsActive = true, IsSystem = false };
            }

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
                lblActiveHtml.Text = string.Format( activeHtmlFormat, "label-success", "Active" );
            }
            else
            {
                lblActiveHtml.Text = string.Format( activeHtmlFormat, "label-important", "Inactive" );
            }

            string descriptionFormat = "<dt>{0}</dt><dd>{1}</dd>";
            lblMainDetails.Text = @"
<div class='span5'>
    <dl>";

            lblMainDetails.Text += string.Format( descriptionFormat, "Description", workflowType.Description );

            if ( workflowType.Category != null )
            {
                lblMainDetails.Text += string.Format( descriptionFormat, "Category", workflowType.Category.Name );
            }

            lblMainDetails.Text += @"
    </dl>
</div>";
        }

        #endregion
        
        protected void gWorkflows_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {

        }
        
        protected void gWorkflows_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {

        }
}
}