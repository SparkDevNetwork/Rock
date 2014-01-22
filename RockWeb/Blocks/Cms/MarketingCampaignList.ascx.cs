// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
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
using System.ComponentModel;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Marketing Campaign - Campaign List")]
    [Category("CMS")]
    [Description("Lists marketing campaigns.")]
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