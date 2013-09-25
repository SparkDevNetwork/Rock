//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User control for managing metrics
    /// </summary>
    [LinkedPage( "Detail Page" )]
    public partial class MetricList : Rock.Web.UI.RockBlock
    {        
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gMetrics.DataKeyNames = new string[] { "id" };
            gMetrics.Actions.ShowAdd = true;
            gMetrics.Actions.AddClick += gMetrics_Add;
            gMetrics.GridRebind += gMetrics_GridRebind;                    
           
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            BindCategoryFilter();                
                       
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gMetrics.Actions.ShowAdd = canAddEditDelete;
            gMetrics.IsDeleteEnabled = canAddEditDelete;
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

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Category", ddlCategoryFilter.SelectedValue );
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gMetrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gMetrics_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "metricId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gMetrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMetrics_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "metricId", (int)e.RowKeyValue );
        }
        
        /// <summary>
        /// Handles the Delete event of the gMetrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMetrics_Delete( object sender, RowEventArgs e )
        {
            var metricService = new MetricService();

            Metric metric = metricService.Get( (int)e.RowKeyValue );
            if ( metric != null )
            {
                metricService.Delete( metric, CurrentPersonId );
                metricService.Save( metric, CurrentPersonId );
            }

            BindGrid();
        }
        
        /// <summary>
        /// Handles the GridRebind event of the gMetrics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gMetrics_GridRebind( object sender, EventArgs e )
        {
            BindCategoryFilter();
            BindGrid();
        }
        
        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the category filter.
        /// </summary>
        private void BindCategoryFilter()
        {
            ddlCategoryFilter.Items.Clear();
            ddlCategoryFilter.Items.Add( Rock.Constants.All.Text );

            var metricService = new MetricService();
            var items = metricService.Queryable().
                Where( a => a.Category != "" && a.Category != null ).
                OrderBy( a => a.Category ).
                Select( a => a.Category ).
                Distinct().ToList();

            foreach ( var item in items )
            {
                var li = new ListItem( item );
                li.Selected = ( !Page.IsPostBack && rFilter.GetUserPreference( "Category" ) == item );
                ddlCategoryFilter.Items.Add( li );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var queryable = new MetricService().Queryable();

            if ( ddlCategoryFilter.SelectedValue != Rock.Constants.All.Text )
            {
                queryable = queryable.Where( a => a.Category == ddlCategoryFilter.SelectedValue );
            }

            var sortProperty = gMetrics.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderBy( a => a.Category ).ThenBy( a => a.Title );
            }

            gMetrics.DataSource = queryable.ToList();
            gMetrics.DataBind();
        }       

        #endregion
    }
}