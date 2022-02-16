using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RockWeb.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Shows the details of a FormBuilder template.
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    [DisplayName( "Form Template Detail" )]
    [Category( "WorkFlow > FormBuilder" )]
    [Description( "Shows the details of a FormBuilder template." )]
    public partial class FormTemplateDetail : RockBlock
    {
        #region Page Parameter Keys

        /// <summary>
        /// Keys for page parameters extracted from the page route
        /// </summary>
        private static class PageParameterKeys
        {
            public const string FormTemplateId = "FormTemplateId";
        }

        #endregion Page Parameter

        #region Properties

        private int HiddenFormBuilderTemplateId
        {
            get
            {
                int formBuilderTemplateId;
                int.TryParse( hfFormTemplateId.Value, out formBuilderTemplateId );

                return formBuilderTemplateId;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKeys.FormTemplateId ).AsInteger() );
            }
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var formBuilderTemplate = GetFormBuilderTemplate( HiddenFormBuilderTemplateId );

            ShowEditDetails( formBuilderTemplate );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            //
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( HiddenFormBuilderTemplateId == 0 )
            {
                NavigateToParentPage();
            }
            else
            {
                var formBuilderTemplate = GetFormBuilderTemplate( HiddenFormBuilderTemplateId );
                ShowReadOnlyDetails( formBuilderTemplate );
            }
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Shows the details of the selected <see cref="WorkflowFormBuilderTemplate"/>.
        /// </summary>
        /// <param name="formBuilderTemplateId">The form template identifier.</param>
        private void ShowDetail( int formBuilderTemplateId )
        {
            pnlDetails.Visible = false;
            WorkflowFormBuilderTemplate formBuilderTemplate = null;

            if ( !formBuilderTemplateId.Equals( 0 ) )
            {
                formBuilderTemplate = GetFormBuilderTemplate( formBuilderTemplateId );
            }

            if ( formBuilderTemplate == null )
            {
                formBuilderTemplate = new WorkflowFormBuilderTemplate { Id = 0 };
            }

            hfFormTemplateId.SetValue( formBuilderTemplate.Id );
            SetStatusLabel( formBuilderTemplate );

            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( WorkflowFormBuilderTemplate.FriendlyTypeName );
                btnEdit.Visible = false;
                ShowReadOnlyDetails( formBuilderTemplate );
            }
            else
            {
                btnEdit.Visible = true;
                pnlDetails.Visible = true;

                if ( formBuilderTemplate.Id > 0 )
                {
                    ShowReadOnlyDetails( formBuilderTemplate );
                }
                else
                {
                    ShowEditDetails( formBuilderTemplate );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="formBuilderTemplate">The form builder template.</param>
        private void ShowEditDetails( WorkflowFormBuilderTemplate formBuilderTemplate )
        {
            if ( formBuilderTemplate == null )
            {
                return;
            }

            SetEditMode( true );

            tbFormBuilderTemplateName.Text = formBuilderTemplate.Name;
            tbDescription.Text = formBuilderTemplate.Description;
            cbIsActive.Checked = formBuilderTemplate.IsActive;
            cbIsLoginRequired.Checked = formBuilderTemplate.IsLoginRequired;
        }

        /// <summary>
        /// Shows the read only details.
        /// </summary>
        /// <param name="formBuilderTemplate">The form builder template.</param>
        private void ShowReadOnlyDetails( WorkflowFormBuilderTemplate formBuilderTemplate )
        {
            SetEditMode( false );

            if ( formBuilderTemplate == null )
            {
                return;
            }

            lActionTitle.Text = formBuilderTemplate.Name.FormatAsHtmlTitle();

            var dl = new DescriptionList();

            if ( !string.IsNullOrWhiteSpace( formBuilderTemplate.Name ) )
            {
                dl.Add( "Name", formBuilderTemplate.Name );
            }

            if ( !string.IsNullOrWhiteSpace( formBuilderTemplate.Description ) )
            {
                dl.Add( "Description", formBuilderTemplate.Description );
            }

            List<Workflow> workflowInstances = GetWorkflowInstances( formBuilderTemplate );
            if ( workflowInstances.Count > 0 )
            {

                dl.Add( "Used By", GenerateInstanceListString( workflowInstances ) );
            }

            lMainDetails.Text = dl.Html;
        }

        private string GenerateInstanceListString( List<Workflow> workflowInstances )
        {
            StringBuilder result = new StringBuilder( "<ul>" );
            const string template = "<li>{0}</li>";

            foreach ( var workflow in workflowInstances )
            {
                result.AppendFormat( template, workflow.Name );
            }

            result.Append( "</ul>" );

            return result.ToString();
        }

        /// <summary>
        /// Gets the workflow instances of the tenplate.
        /// </summary>
        /// <param name="formBuilderTemplate">The form builder template.</param>
        private List<Workflow> GetWorkflowInstances( WorkflowFormBuilderTemplate formBuilderTemplate )
        {
            //var service = new WorkflowService( new RockContext() );
            //var workflowInstances = service.Queryable().Where( w => w.WorkflowTypeId == formBuilderTemplate.Id ).ToList();
            //return workflowInstances;
            return new List<Workflow>
            {
                new Workflow{ Name = "Workflow 1" },
                new Workflow{ Name = "Workflow 2" },
                new Workflow{ Name = "Workflow 3" },
                new Workflow{ Name = "Workflow 4" },
            };
        }

        /// <summary>
        /// Gets the form builder template.
        /// </summary>
        /// <param name="formBuilderTemplateId">The form builder template identifier.</param>
        /// <returns></returns>
        private WorkflowFormBuilderTemplate GetFormBuilderTemplate( int formBuilderTemplateId )
        {
            var service = new WorkflowFormBuilderTemplateService( new RockContext() );
            return service.Get( formBuilderTemplateId );
        }

        /// <summary>
        /// Sets the Active/Inactive status label.
        /// </summary>
        /// <param name="formBuilderTemplate">The <see cref="Campus"/>.</param>
        private void SetStatusLabel( WorkflowFormBuilderTemplate formBuilderTemplate )
        {
            if ( !formBuilderTemplate.IsActive )
            {
                hlStatus.Text = "Inactive";
                hlStatus.LabelType = LabelType.Danger;
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

        #endregion Methods
    }
}
