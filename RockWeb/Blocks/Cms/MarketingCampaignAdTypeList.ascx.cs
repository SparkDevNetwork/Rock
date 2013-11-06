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
    public partial class MarketingCampaignAdTypeList : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gMarketingCampaignAdType.DataKeyNames = new string[] { "id" };
            gMarketingCampaignAdType.Actions.AddClick += gMarketingCampaignAdType_Add;
            gMarketingCampaignAdType.GridRebind += gMarketingCampaignAdType_GridRebind;

            // Block Security and special attributes (RockPage takes care of "View")
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            gMarketingCampaignAdType.Actions.ShowAdd = canAddEditDelete;
            gMarketingCampaignAdType.IsDeleteEnabled = canAddEditDelete;
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
        /// Handles the Add event of the gMarketingCampaignAdType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAdType_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "marketingCampaignAdTypeId", 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gMarketingCampaignAdType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAdType_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "marketingCampaignAdTypeId", (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the gMarketingCampaignAdType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gMarketingCampaignAdType_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService();
                MarketingCampaignAdType marketingCampaignAdType = marketingCampaignAdTypeService.Get( (int)e.RowKeyValue );

                if ( marketingCampaignAdType != null )
                {
                    string errorMessage;
                    if ( !marketingCampaignAdTypeService.CanDelete( marketingCampaignAdType, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    marketingCampaignAdTypeService.Delete( marketingCampaignAdType, CurrentPersonId );
                    marketingCampaignAdTypeService.Save( marketingCampaignAdType, CurrentPersonId );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMarketingCampaignAdType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gMarketingCampaignAdType_GridRebind( object sender, EventArgs e )
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
            MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService();
            SortProperty sortProperty = gMarketingCampaignAdType.SortProperty;

            if ( sortProperty != null )
            {
                gMarketingCampaignAdType.DataSource = marketingCampaignAdTypeService.Queryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gMarketingCampaignAdType.DataSource = marketingCampaignAdTypeService.Queryable().OrderBy( p => p.Name ).ToList();
            }

            gMarketingCampaignAdType.DataBind();
        }

        #endregion
    }
}