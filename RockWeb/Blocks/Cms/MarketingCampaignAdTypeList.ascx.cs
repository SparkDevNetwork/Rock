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
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName("Marketing Campaign - Ad Type List")]
    [Category("CMS")]
    [Description("Lists ad types in the system.")]
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

            // Block Security and special attributes (RockPage takes care of View)
            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
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
            var rockContext = new RockContext();
            MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService( rockContext );
            MarketingCampaignAdType marketingCampaignAdType = marketingCampaignAdTypeService.Get( (int)e.RowKeyValue );

            if ( marketingCampaignAdType != null )
            {
                string errorMessage;
                if ( !marketingCampaignAdTypeService.CanDelete( marketingCampaignAdType, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                marketingCampaignAdTypeService.Delete( marketingCampaignAdType );

                rockContext.SaveChanges();
            }

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
            MarketingCampaignAdTypeService marketingCampaignAdTypeService = new MarketingCampaignAdTypeService( new RockContext() );
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