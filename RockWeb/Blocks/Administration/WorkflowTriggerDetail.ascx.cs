//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;


namespace RockWeb.Blocks.Administration
{
    public partial class WorkflowTriggerDetail : RockBlock, IDetailBlock
    {

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "WorkflowTriggerId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "WorkflowTriggerId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlEntityType.Items.Clear();
            new EntityTypeService().GetEntityListItems().ForEach( l => ddlEntityType.Items.Add( l ) );

            ddlWorkflowType.Items.Clear();
            ddlWorkflowType.Items.Add( new ListItem( string.Empty, string.Empty));

            foreach ( var workflowType in new WorkflowTypeService().Queryable().OrderBy( w => w.Name ) )
            {
                if ( workflowType.IsAuthorized( "View", CurrentPerson ) )
                {
                    ddlWorkflowType.Items.Add( new ListItem( workflowType.Name, workflowType.Id.ToString() ) );
                }
            }

            rblTriggerType.Items.Clear();
            Type type = typeof(WorkflowTriggerType);
            foreach ( var value in Enum.GetValues( type ) )
            {
                rblTriggerType.Items.Add( new ListItem( Enum.GetName( type, value ).SplitCase().Replace( " ", "-" ), Convert.ToInt32( value ).ToString() ) );
            }
            
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "WorkflowTriggerId" ) )
            {
                return;
            }

            pnlDetails.Visible = true;
            WorkflowTrigger WorkflowTrigger = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                WorkflowTrigger = new WorkflowTriggerService().Get( itemKeyValue );
                lActionTitle.Text = ActionTitle.Edit( WorkflowTrigger.FriendlyTypeName );
            }
            else
            {
                WorkflowTrigger = new WorkflowTrigger { Id = 0, WorkflowTriggerType = WorkflowTriggerType.PostSave };
                lActionTitle.Text = ActionTitle.Add( WorkflowTrigger.FriendlyTypeName );
            }

            LoadDropDowns();

            hfWorkflowTriggerId.Value = WorkflowTrigger.Id.ToString(); 
            ddlEntityType.SetValue( WorkflowTrigger.EntityTypeId );
            LoadColumnNames();
            tbQualifierValue.Text = WorkflowTrigger.EntityTypeQualifierValue;
            ddlWorkflowType.SetValue( WorkflowTrigger.WorkflowTypeId );
            rblTriggerType.SelectedValue = WorkflowTrigger.WorkflowTriggerType.ConvertToInt().ToString();
            tbWorkflowName.Text = WorkflowTrigger.WorkflowName ?? string.Empty;

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( WorkflowTrigger.FriendlyTypeName );
            }

            if ( WorkflowTrigger.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( WorkflowTrigger.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( WorkflowTrigger.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            ddlEntityType.Enabled = !readOnly;
            ddlQualifierColumn.Enabled = !readOnly;
            tbQualifierValue.ReadOnly = readOnly;
            ddlWorkflowType.Enabled = !readOnly;
            rblTriggerType.Enabled = !readOnly;
            tbWorkflowName.ReadOnly = readOnly;

            btnSave.Visible = !readOnly;

        }

        /// <summary>
        /// Loads the column names.
        /// </summary>
        private void LoadColumnNames()
        {
            ddlQualifierColumn.Items.Clear();

            var entityType = EntityTypeCache.Read( ddlEntityType.SelectedValueAsInt().Value );
            if ( entityType != null )
            {
                Type type = entityType.GetEntityType();

                if ( type != null )
                {
                    var propertyNames = new List<string>();
                    foreach ( var property in type.GetProperties() )
                    {
                        if ( !property.GetGetMethod().IsVirtual || property.Name == "Id" || property.Name == "Guid" || property.Name == "Order" )
                        {
                            propertyNames.Add( property.Name );
                        }
                    }

                    ddlQualifierColumn.DataSource = propertyNames.OrderBy( n => n );
                    ddlQualifierColumn.DataBind();
                    ddlQualifierColumn.Items.Insert( 0, string.Empty );
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
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            WorkflowTrigger WorkflowTrigger;
            WorkflowTriggerService WorkflowTriggerService = new WorkflowTriggerService();
            AttributeService attributeService = new AttributeService();

            int WorkflowTriggerId = int.Parse( hfWorkflowTriggerId.Value );

            if ( WorkflowTriggerId == 0 )
            {
                WorkflowTrigger = new WorkflowTrigger();
                WorkflowTriggerService.Add( WorkflowTrigger, CurrentPersonId );
            }
            else
            {
                WorkflowTrigger = WorkflowTriggerService.Get( WorkflowTriggerId );
            }

            WorkflowTrigger.EntityTypeId = ddlEntityType.SelectedValueAsInt().Value;
            WorkflowTrigger.EntityTypeQualifierColumn = ddlQualifierColumn.SelectedValue;
            WorkflowTrigger.EntityTypeQualifierValue = tbQualifierValue.Text;
            WorkflowTrigger.WorkflowTypeId = ddlWorkflowType.SelectedValueAsInt().Value;
            WorkflowTrigger.WorkflowTriggerType = (WorkflowTriggerType)System.Enum.Parse( typeof( WorkflowTriggerType ), rblTriggerType.SelectedValue );
            if ( string.IsNullOrWhiteSpace( tbWorkflowName.Text ) )
            {
                WorkflowTrigger.WorkflowName = null;
            }
            else
            {
                WorkflowTrigger.WorkflowName = tbWorkflowName.Text;
            }

            if ( !WorkflowTrigger.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            WorkflowTriggerService.Save( WorkflowTrigger, CurrentPersonId );

            Rock.Workflow.TriggerCache.Refresh();

            NavigateToParentPage();
        }

        #endregion

        #region Activities and Actions

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ddlEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadColumnNames();
        }

        #endregion
    }
}