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
using Rock.Security;


namespace RockWeb.Blocks.WorkFlow
{
    [DisplayName( "Workflow Trigger Detail" )]
    [Category( "WorkFlow" )]
    [Description( "Displays the details of the given workflow trigger." )]
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
                ShowDetail( PageParameter( "WorkflowTriggerId" ).AsInteger() );
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
            var rockContext = new RockContext();
            new EntityTypeService( rockContext ).GetEntityListItems().ForEach( l => ddlEntityType.Items.Add( l ) );

            ddlWorkflowType.Items.Clear();
            ddlWorkflowType.Items.Add( new ListItem( string.Empty, string.Empty));

            foreach ( var workflowType in new WorkflowTypeService( rockContext ).Queryable().OrderBy( w => w.Name ) )
            {
                if ( workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
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
        /// <param name="workflowTriggerId">The workflow trigger identifier.</param>
        public void ShowDetail( int workflowTriggerId )
        {
            pnlDetails.Visible = true;
            WorkflowTrigger workflowTrigger = null;

            if ( !workflowTriggerId.Equals( 0 ) )
            {
                workflowTrigger = new WorkflowTriggerService( new RockContext() ).Get( workflowTriggerId );
                lActionTitle.Text = ActionTitle.Edit( WorkflowTrigger.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            if ( workflowTrigger == null )
            {
                workflowTrigger = new WorkflowTrigger { Id = 0, WorkflowTriggerType = WorkflowTriggerType.PostSave, IsActive = true };
                lActionTitle.Text = ActionTitle.Add( WorkflowTrigger.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            LoadDropDowns();

            hfWorkflowTriggerId.Value = workflowTrigger.Id.ToString(); 
            ddlEntityType.SetValue( workflowTrigger.EntityTypeId );
            LoadColumnNames();
            ddlQualifierColumn.SetValue( workflowTrigger.EntityTypeQualifierColumn );
            ShowQualifierValues( workflowTrigger );
            ddlWorkflowType.SetValue( workflowTrigger.WorkflowTypeId );
            rblTriggerType.SelectedValue = workflowTrigger.WorkflowTriggerType.ConvertToInt().ToString();
            tbWorkflowName.Text = workflowTrigger.WorkflowName ?? string.Empty;
            cbIsActive.Checked = workflowTrigger.IsActive ?? false;

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( WorkflowTrigger.FriendlyTypeName );
            }

            if ( workflowTrigger.IsSystem )
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
            cbIsActive.Enabled = !readOnly;

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
            WorkflowTrigger workflowTrigger;
            var rockContext = new RockContext();
            WorkflowTriggerService WorkflowTriggerService = new WorkflowTriggerService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );

            int WorkflowTriggerId = int.Parse( hfWorkflowTriggerId.Value );

            if ( WorkflowTriggerId == 0 )
            {
                workflowTrigger = new WorkflowTrigger();
                WorkflowTriggerService.Add( workflowTrigger );
            }
            else
            {
                workflowTrigger = WorkflowTriggerService.Get( WorkflowTriggerId );
            }

            workflowTrigger.WorkflowTypeId = ddlWorkflowType.SelectedValueAsInt().Value;
            workflowTrigger.WorkflowTriggerType = (WorkflowTriggerType)System.Enum.Parse( typeof( WorkflowTriggerType ), rblTriggerType.SelectedValue );

            workflowTrigger.EntityTypeId = ddlEntityType.SelectedValueAsInt().Value;
            workflowTrigger.EntityTypeQualifierColumn = ddlQualifierColumn.SelectedValue;

            // If the trigger type is PreSave and the tbQualifierValue does not exist,
            // use the previous and alt qualifier value
            if ( workflowTrigger.WorkflowTriggerType == WorkflowTriggerType.PreSave ) 
            {
                if ( !string.IsNullOrEmpty( tbQualifierValue.Text ) )
                {
                    // in this case, use the same value as the previous and current qualifier value
                    workflowTrigger.EntityTypeQualifierValue = tbQualifierValue.Text;
                    workflowTrigger.EntityTypeQualifierValuePrevious = tbQualifierValue.Text;
                }
                else
                {
                    workflowTrigger.EntityTypeQualifierValue = tbQualifierValueAlt.Text;
                    workflowTrigger.EntityTypeQualifierValuePrevious = tbPreviousQualifierValue.Text;
                }
            }
            else
            {
                // use the regular qualifier and clear the previous value qualifier since it does not apply.
                workflowTrigger.EntityTypeQualifierValue = tbQualifierValue.Text;
                workflowTrigger.EntityTypeQualifierValuePrevious = string.Empty;
            }

            workflowTrigger.IsActive = cbIsActive.Checked;
            if ( string.IsNullOrWhiteSpace( tbWorkflowName.Text ) )
            {
                workflowTrigger.WorkflowName = null;
            }
            else
            {
                workflowTrigger.WorkflowName = tbWorkflowName.Text;
            }

            if ( !workflowTrigger.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.SaveChanges();

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

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblTriggerType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblTriggerType_SelectedIndexChanged( object sender, EventArgs e )
        {
            ShowQualifierValues(null);
        }

        /// <summary>
        /// Shows the qualifier values in the correct fields using the given 
        /// workflow trigger if available.
        /// </summary>
        /// <param name="workflowTrigger">The workflow trigger.</param>
        private void ShowQualifierValues( WorkflowTrigger workflowTrigger )
        {
            bool showPreSave = false;
            if ( workflowTrigger != null )
            {
                showPreSave = ( workflowTrigger.WorkflowTriggerType == WorkflowTriggerType.PreSave );
                if ( showPreSave
                    && ! string.IsNullOrEmpty( workflowTrigger.EntityTypeQualifierValue )
                    && workflowTrigger.EntityTypeQualifierValue != workflowTrigger.EntityTypeQualifierValuePrevious )
                {
                    tbQualifierValueAlt.Text = workflowTrigger.EntityTypeQualifierValue;
                    tbPreviousQualifierValue.Text = workflowTrigger.EntityTypeQualifierValuePrevious;
                }
                else
                {
                    tbQualifierValue.Text = workflowTrigger.EntityTypeQualifierValue;
                }
            }

            if ( rblTriggerType.SelectedValue == ( (int)WorkflowTriggerType.PreSave ).ToStringSafe() || showPreSave )
            {
                tbQualifierValue.Label = "Or value is";
                tbPreviousQualifierValue.Visible = true;
                tbQualifierValueAlt.Visible = true;
            }
            else
            {
                tbQualifierValue.Label = "Value is";
                tbPreviousQualifierValue.Visible = false;
                tbQualifierValueAlt.Visible = false;
            }
        }

        #endregion
    }
}