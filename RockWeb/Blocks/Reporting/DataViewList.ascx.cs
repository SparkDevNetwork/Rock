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

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DetailPage]
    public partial class DataViewList : RockBlock
    { 
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gDataView.DataKeyNames = new string[] { "id" };
            gDataView.Actions.IsAddEnabled = true;
            gDataView.Actions.AddClick += gDataView_Add;
            gDataView.GridRebind += gDataView_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gDataView.Actions.IsAddEnabled = canAddEditDelete;
            gDataView.IsDeleteEnabled = canAddEditDelete;
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
        /// Handles the Add event of the gDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gDataView_Add( object sender, EventArgs e )
        {
            NavigateToDetailPage( "dataViewId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDataView_Edit( object sender, RowEventArgs e )
        {
            NavigateToDetailPage( "dataViewId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gDataView_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                DataViewService dataViewService = new DataViewService();
                DataView dataView = dataViewService.Get( (int)e.RowKeyValue );

                if ( dataView != null )
                {
                    string errorMessage;
                    if ( !dataViewService.CanDelete( dataView, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    dataViewService.Delete( dataView, CurrentPersonId );
                    dataViewService.Save( dataView, CurrentPersonId );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gDataView_GridRebind( object sender, EventArgs e )
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
            DataViewService dataViewService = new DataViewService();
            SortProperty sortProperty = gDataView.SortProperty;

            if ( sortProperty != null )
            {
                gDataView.DataSource = dataViewService.Queryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gDataView.DataSource = dataViewService.Queryable().OrderBy( p => p.Name ).ToList();
            }

            gDataView.DataBind();
        }

        #endregion
    }
}