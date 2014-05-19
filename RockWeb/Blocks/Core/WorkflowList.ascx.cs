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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock;
using Attribute = Rock.Model.Attribute;
using Rock.Security;

namespace RockWeb.Blocks.Core
{
    [DisplayName( "Workflow List" )]
    [Category( "Core" )]
    [Description( "Lists all the workflows." )]

    [LinkedPage("Detail Page")]
    [ContextAware( typeof( WorkflowType ) )]
    public partial class WorkflowList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gWorkflows.DataKeyNames = new string[] { "id" };
            gWorkflows.Actions.ShowAdd = true;
            gWorkflows.Actions.AddClick += gWorkflows_Add;
            gWorkflows.GridRebind += gWorkflows_GridRebind;

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gWorkflows.Actions.ShowAdd = canAddEditDelete;
            gWorkflows.IsDeleteEnabled = canAddEditDelete;
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

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gWorkflows_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "workflowId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gWorkflows_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "workflowId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gWorkflows_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            WorkflowService workflowService = new WorkflowService( rockContext );
            Workflow workflow = workflowService.Get( (int)e.RowKeyValue );
            if ( workflow != null )
            {
                string errorMessage;
                if ( !workflowService.CanDelete( workflow, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                workflowService.Delete( workflow );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();

            WorkflowService workflowService = new WorkflowService( rockContext );
            SortProperty sortProperty = gWorkflows.SortProperty;

            var qry = workflowService.Queryable();

            WorkflowType workflowType = this.ContextEntity<WorkflowType>();

            if ( workflowType == null )
            {
                pnlWorkflowList.Visible = false;
                return;
            }

            // if this isn't currently a persisted workflow type, and there are no records, hide the panel
            if ( !workflowType.IsPersisted )
            {
                if ( qry.Count() == 0 )
                {
                    pnlWorkflowList.Visible = false;
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(workflowType.WorkTerm))
            {
                gWorkflows.RowItemText = workflowType.WorkTerm;
                lGridTitle.Text = workflowType.WorkTerm.Pluralize();
            }

            AttributeService attributeService = new AttributeService( rockContext );

            // add attributes with IsGridColumn to grid
            string qualifierValue = workflowType.Id.ToString();
            var qryWorkflowTypeAttributes = attributeService.GetByEntityTypeId( new Workflow().TypeId ).AsQueryable()
                .Where( a => a.EntityTypeQualifierColumn.Equals( "WorkflowTypeId", StringComparison.OrdinalIgnoreCase )
                && a.EntityTypeQualifierValue.Equals( qualifierValue ) );

            qryWorkflowTypeAttributes = qryWorkflowTypeAttributes.Where( a => a.IsGridColumn );

            List<Attribute> gridItems = qryWorkflowTypeAttributes.ToList();

            foreach ( var item in gWorkflows.Columns.OfType<AttributeField>().ToList() )
            {
                gWorkflows.Columns.Remove( item );
            }

            foreach ( var item in gridItems.OrderBy( a => a.Order ).ThenBy( a => a.Name ) )
            {
                string dataFieldExpression = item.Key;
                bool columnExists = gWorkflows.Columns.OfType<AttributeField>().FirstOrDefault( a => a.DataField.Equals( dataFieldExpression ) ) != null;
                if ( !columnExists )
                {
                    AttributeField boundField = new AttributeField();
                    boundField.DataField = dataFieldExpression;
                    boundField.HeaderText = item.Name;
                    boundField.SortExpression = string.Empty;
                    
                    var attributeCache = Rock.Web.Cache.AttributeCache.Read( item.Id );
                    if ( attributeCache != null )
                    {
                        boundField.ItemStyle.HorizontalAlign = attributeCache.FieldType.Field.AlignValue;
                    }

                    int insertPos = gWorkflows.Columns.IndexOf( gWorkflows.Columns.OfType<DeleteField>().First() );
                    gWorkflows.Columns.Insert( insertPos, boundField );
                }
            }


            pnlWorkflowList.Visible = true;

            qry = qry.Where( a => a.WorkflowTypeId.Equals( workflowType.Id ) );

            if ( sortProperty != null )
            {
                gWorkflows.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gWorkflows.DataSource = qry.OrderBy( s => s.Name ).ToList();
            }

            gWorkflows.DataBind();
        }

        #endregion
    }
}