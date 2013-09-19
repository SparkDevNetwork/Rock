//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
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

            gWorkflowTrigger.DataKeyNames = new string[] { "id" };
            gWorkflowTrigger.Actions.ShowAdd = true;
            gWorkflowTrigger.Actions.AddClick += gWorkflowTrigger_Add;
            gWorkflowTrigger.GridRebind += gWorkflowTrigger_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
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
            NavigateToLinkedPage( "DetailPage", "WorkflowTriggerId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gWorkflowTrigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gWorkflowTrigger_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                WorkflowTriggerService WorkflowTriggerService = new WorkflowTriggerService();
                WorkflowTrigger WorkflowTrigger = WorkflowTriggerService.Get( (int)e.RowKeyValue );

                if ( WorkflowTrigger != null )
                {
                    string errorMessage;
                    if ( !WorkflowTriggerService.CanDelete( WorkflowTrigger, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    WorkflowTriggerService.Delete( WorkflowTrigger, CurrentPersonId );
                    WorkflowTriggerService.Save( WorkflowTrigger, CurrentPersonId );
                }
            } );

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

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            gWorkflowTrigger.DataSource = new WorkflowTriggerService().Queryable()
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
                            a.IsSystem
                        } ).ToList();
            
            gWorkflowTrigger.DataBind();
        }

        #endregion
    }
}