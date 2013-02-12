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
using Rock.Constants;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [ContextAware( typeof( MarketingCampaign ) )]
    [DetailPage]
    public partial class MarketingCampaignAdList : RockBlock, IDimmableBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gMarketingCampaignAds.DataKeyNames = new string[] { "Id" };
            gMarketingCampaignAds.Actions.AddClick += gMarketingCampaignAds_Add;
            gMarketingCampaignAds.GridRebind += gMarketingCampaignAds_GridRebind;
            gMarketingCampaignAds.EmptyDataText = Server.HtmlEncode( None.Text );

            // Block Security on Ads grid (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gMarketingCampaignAds.Actions.IsAddEnabled = canAddEditDelete;
            gMarketingCampaignAds.IsDeleteEnabled = canAddEditDelete;
            gMarketingCampaignAds.Actions.IsAddEnabled = canAddEditDelete;
            gMarketingCampaignAds.IsDeleteEnabled = canAddEditDelete;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                MarketingCampaign marketingCampaign = this.ContextEntity<MarketingCampaign>();
                if ( marketingCampaign != null )
                {
                    hfMarketingCampaignId.Value = marketingCampaign.Id.ToString();
                    BindMarketingCampaignAdsGrid();
                }
                else
                {
                    pnlMarketingCampaignAds.Visible = false;
                }
            }
        }

        #endregion

        #region MarketingCampaignAds Grid

        /// <summary>
        /// Handles the Add event of the gMarketingCampaignAds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAds_Add( object sender, EventArgs e )
        {
            gMarketingCampaignAds_ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gMarketingCampaignAds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAds_Edit( object sender, RowEventArgs e )
        {
            gMarketingCampaignAds_ShowEdit( (int)e.RowKeyValue );
        }

        /// <summary>
        /// Gs the marketing campaign ads_ show edit.
        /// </summary>
        /// <param name="marketingCampaignAdId">The marketing campaign ad id.</param>
        protected void gMarketingCampaignAds_ShowEdit( int marketingCampaignAdId )
        {
            if ( marketingCampaignAdId == 0 )
            {
                NavigateToDetailPage( "marketingCampaignAdId",  0, "marketingCampaignId", hfMarketingCampaignId.Value.AsInteger().Value );
            }
            else
            {
                NavigateToDetailPage( "marketingCampaignAdId", marketingCampaignAdId );
            }
        }

        /// <summary>
        /// Handles the Delete event of the gMarketingCampaignAds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAds_Delete( object sender, RowEventArgs e )
        {
            MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService();
            MarketingCampaignAd marketingCampaignAd = marketingCampaignAdService.Get( (int)e.RowKeyValue );

            marketingCampaignAdService.Delete( marketingCampaignAd, CurrentPersonId );
            marketingCampaignAdService.Save( marketingCampaignAd, CurrentPersonId );
            BindMarketingCampaignAdsGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMarketingCampaignAds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAds_GridRebind( object sender, EventArgs e )
        {
            BindMarketingCampaignAdsGrid();
        }

        /// <summary>
        /// Binds the marketing campaign ads grid.
        /// </summary>
        private void BindMarketingCampaignAdsGrid()
        {
            MarketingCampaignAdService marketingCampaignAdService = new MarketingCampaignAdService();
            int marketingCampaignId = int.Parse( hfMarketingCampaignId.Value );
            var qry = marketingCampaignAdService.Queryable().Where( a => a.MarketingCampaignId.Equals( marketingCampaignId ) );

            gMarketingCampaignAds.DataSource = qry.OrderBy( a => a.StartDate ).ThenBy( a => a.Priority ).ThenBy( a => a.MarketingCampaignAdType.Name ).ToList();
            gMarketingCampaignAds.DataBind();
        }

        #endregion

        #region IDimmableBlock

        /// <summary>
        /// Sets the dimmed.
        /// </summary>
        /// <param name="dimmed">if set to <c>true</c> [dimmed].</param>
        public void SetDimmed( bool dimmed )
        {
            pnlMarketingCampaignAds.Disabled = dimmed;
            gMarketingCampaignAds.Enabled = !dimmed;
        }

        #endregion
    }
}