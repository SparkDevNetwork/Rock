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
    public partial class ReportList : RockBlock
    { 
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gReport.DataKeyNames = new string[] { "id" };
            gReport.Actions.IsAddEnabled = true;
            gReport.Actions.AddClick += gReport_Add;
            gReport.GridRebind += gReport_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gReport.Actions.IsAddEnabled = canAddEditDelete;
            gReport.IsDeleteEnabled = canAddEditDelete;
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
        /// Handles the Add event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gReport_Add( object sender, EventArgs e )
        {
            NavigateToDetailPage( "reportId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gReport_Edit( object sender, RowEventArgs e )
        {
            NavigateToDetailPage( "reportId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gReport_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                ReportService reportService = new ReportService();
                Report report = reportService.Get( (int)e.RowKeyValue );

                if ( report != null )
                {
                    string errorMessage;
                    if ( !reportService.CanDelete( report, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    reportService.Delete( report, CurrentPersonId );
                    reportService.Save( report, CurrentPersonId );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gReport_GridRebind( object sender, EventArgs e )
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
            ReportService reportService = new ReportService();
            SortProperty sortProperty = gReport.SortProperty;

            if ( sortProperty != null )
            {
                gReport.DataSource = reportService.Queryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gReport.DataSource = reportService.Queryable().OrderBy( p => p.Name ).ToList();
            }

            gReport.DataBind();
        }

        #endregion
    }
}