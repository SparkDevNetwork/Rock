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
    [LinkedPage("Detail Page")] 
    public partial class ScheduleList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gSchedules.DataKeyNames = new string[] { "id" };
            gSchedules.Actions.ShowAdd = true;
            gSchedules.Actions.AddClick += gSchedules_Add;
            gSchedules.GridRebind += gSchedules_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gSchedules.Actions.ShowAdd = canAddEditDelete;
            gSchedules.IsDeleteEnabled = canAddEditDelete;
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
        /// Handles the Add event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSchedules_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "scheduleId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSchedules_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "scheduleId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSchedules_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                ScheduleService scheduleService = new ScheduleService();
                Schedule schedule = scheduleService.Get( (int)e.RowKeyValue );
                if ( schedule != null )
                {
                    string errorMessage;
                    if ( !scheduleService.CanDelete( schedule, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    scheduleService.Delete( schedule, CurrentPersonId );
                    scheduleService.Save( schedule, CurrentPersonId );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSchedules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSchedules_GridRebind( object sender, EventArgs e )
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
            ScheduleService scheduleService = new ScheduleService();
            SortProperty sortProperty = gSchedules.SortProperty;
            var qry = scheduleService.Queryable().Select( a =>
                new
                {
                    a.Id,
                    a.Name,
                    CategoryName = a.Category.Name
                } );

            if ( sortProperty != null )
            {
                gSchedules.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gSchedules.DataSource = qry.OrderBy( s => s.Name ).ToList();
            }

            gSchedules.DataBind();
        }

        #endregion
    }
}