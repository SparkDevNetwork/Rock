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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.WorkFlow
{
    [DisplayName( "Workflow Trigger List" )]
    [Category( "WorkFlow" )]
    [Description( "Lists all the workflow triggers." )]

    [LinkedPage("Detail Page")]
    public partial class WorkflowTriggerList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BindFilter();
            gfWorkflowTrigger.ApplyFilterClick += gfWorkflowTrigger_ApplyFilterClick;

            gWorkflowTrigger.DataKeyNames = new string[] { "Id" };
            gWorkflowTrigger.Actions.ShowAdd = true;
            gWorkflowTrigger.Actions.AddClick += gWorkflowTrigger_Add;
            gWorkflowTrigger.GridRebind += gWorkflowTrigger_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gWorkflowTrigger.Actions.ShowAdd = canAddEditDelete;
            gWorkflowTrigger.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Grid Events (main grid)

        protected void gfWorkflowTrigger_ApplyFilterClick( object sender, EventArgs e )
        {
            gfWorkflowTrigger.SaveUserPreference( "Include Inactive", cbIncludeInactive.Checked ? "Yes" : String.Empty );
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gWorkflowTrigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gWorkflowTrigger_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "WorkflowTriggerId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gWorkflowTrigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gWorkflowTrigger_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "WorkflowTriggerId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gWorkflowTrigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gWorkflowTrigger_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            WorkflowTriggerService WorkflowTriggerService = new WorkflowTriggerService( rockContext );
            WorkflowTrigger WorkflowTrigger = WorkflowTriggerService.Get( e.RowKeyId );

            if ( WorkflowTrigger != null )
            {
                string errorMessage;
                if ( !WorkflowTriggerService.CanDelete( WorkflowTrigger, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                WorkflowTriggerService.Delete( WorkflowTrigger );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gWorkflowTrigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gWorkflowTrigger_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        private void BindFilter()
        {
            if ( !Page.IsPostBack )
            {
                cbIncludeInactive.Checked = gfWorkflowTrigger.GetUserPreference( "Include Inactive" ) == "Yes";
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var triggers = new WorkflowTriggerService( new RockContext() ).Queryable();

            string includeInactive = gfWorkflowTrigger.GetUserPreference( "Include Inactive" );

            if ( String.IsNullOrEmpty( includeInactive ) || !includeInactive.Contains( "Yes" ) )
            {
                triggers = triggers.Where( a => a.IsActive == true );
            }

            gWorkflowTrigger.DataSource = triggers
                .OrderBy( w => w.EntityType.Name )
                .ThenBy( w => w.EntityTypeQualifierColumn )
                .ThenBy( w => w.EntityTypeQualifierValue ).Select( a =>
                    new
                        {
                            a.Id,
                            EntityTypeFriendlyName = a.EntityType.FriendlyName,
                            a.WorkflowTriggerType,
                            a.EntityTypeQualifierColumn,
                            a.EntityTypeQualifierValue,
                            WorkflowTypeName = a.WorkflowType.Name,
                            a.IsSystem,
                            a.IsActive
                        } ).ToList();
            
            gWorkflowTrigger.DataBind();
        }

        #endregion
    }
}