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
    public partial class MarketingCampaignList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gMarketingCampaigns.DataKeyNames = new string[] { "id" };

            gMarketingCampaigns.Actions.AddClick += gMarketingCampaigns_Add;
            gMarketingCampaigns.GridRebind += gMarketingCampaigns_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gMarketingCampaigns.Actions.ShowAdd = canAddEditDelete;
            gMarketingCampaigns.IsDeleteEnabled = canAddEditDelete;
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
        /// Handles the Add event of the gMarketingCampaigns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaigns_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "marketingCampaignId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gMarketingCampaigns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaigns_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "marketingCampaignId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gMarketingCampaigns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaigns_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                MarketingCampaignService marketingCampaignService = new MarketingCampaignService();
                MarketingCampaign marketingCampaign = marketingCampaignService.Get( (int)e.RowKeyValue );

                if ( marketingCampaign != null )
                {
                    string errorMessage;
                    if ( !marketingCampaignService.CanDelete( marketingCampaign, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    marketingCampaignService.Delete( marketingCampaign, CurrentPersonId );
                    marketingCampaignService.Save( marketingCampaign, CurrentPersonId );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMarketingCampaigns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gMarketingCampaigns_GridRebind( object sender, EventArgs e )
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
            MarketingCampaignService marketingCampaignService = new MarketingCampaignService();
            SortProperty sortProperty = gMarketingCampaigns.SortProperty;
            var qry = marketingCampaignService.Queryable().Select( a =>
                new
                {
                    a.Id,
                    a.Title,
                    EventGroupName = a.EventGroup.Name,
                    a.ContactFullName
                } );

            if ( sortProperty != null )
            {
                gMarketingCampaigns.DataSource = qry.Sort( sortProperty ).ToList();
            }
            else
            {
                gMarketingCampaigns.DataSource = qry.OrderBy( p => p.Title ).ToList();
            }

            gMarketingCampaigns.DataBind();
        }

        #endregion
    }
}