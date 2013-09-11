//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [LinkedPage("Detail Page")]
    public partial class SiteList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gSites.DataKeyNames = new string[] { "id" };
            gSites.Actions.ShowAdd = true;
            gSites.Actions.AddClick += gSites_Add;
            gSites.GridRebind += gSites_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gSites.Actions.ShowAdd = canAddEditDelete;
            gSites.IsDeleteEnabled = canAddEditDelete;

            SecurityField securityField = gSites.Columns[3] as SecurityField;
            securityField.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Site ) ).Id;
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
        /// Handles the Add event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSites_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "siteId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSites_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "siteId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSites_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                SiteService siteService = new SiteService();
                Site site = siteService.Get( (int)e.RowKeyValue );
                if ( site != null )
                {
                    string errorMessage;
                    if ( !siteService.CanDelete( site, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }
                    
                    siteService.Delete( site, CurrentPersonId );
                    siteService.Save( site, CurrentPersonId );

                    SiteCache.Flush( site.Id );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gSites control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSites_GridRebind( object sender, EventArgs e )
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
            SiteService siteService = new SiteService();
            SortProperty sortProperty = gSites.SortProperty;
            var qry = siteService.Queryable();

            if ( sortProperty != null )
            {
                gSites.DataSource = qry.Sort(sortProperty).ToList();
            }
            else
            {
                gSites.DataSource = qry.OrderBy( s => s.Name ).ToList();
            }
            
            
            gSites.DataBind();
        }

        #endregion
    }
}