//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
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

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
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

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
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
            RockTransactionScope.WrapTransaction( () =>
            {
                WorkflowService workflowService = new WorkflowService();
                Workflow workflow = workflowService.Get( (int)e.RowKeyValue );
                if ( workflow != null )
                {
                    string errorMessage;
                    if ( !workflowService.CanDelete( workflow, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    workflowService.Delete( workflow, CurrentPersonId );
                    workflowService.Save( workflow, CurrentPersonId );
                }
            } );

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
            WorkflowService workflowService = new WorkflowService();
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

            AttributeService attributeService = new AttributeService();

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