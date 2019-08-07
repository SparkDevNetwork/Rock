// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Store;
using System.Text;
using Rock.Security;

namespace RockWeb.Blocks.Store
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Promo List Lava" )]
    [Category( "Store" )]
    [Description( "Lists Rock Store promotions using a Lava template." )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the promotions", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~/Assets/Lava/Store/PromoList.lava' %}", "", 2 )]
    [CustomRadioListField("Promo Type", "Display the promos of the specified type", "All, Top Paid, Top Free, Featured", true, "Normal", "", 0)]
    [TextField("Category Id", "Filters promos for a specific category id. If none is provided it will show promos with no category.", false, "","", 1)]
    [LinkedPage( "Detail Page", "Page reference to use for the detail page.", false, "", "", 4 )]
    public partial class PromoListLava : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                LoadPromos();
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadPromos();
        }

        #endregion

        #region Methods

        private void LoadPromos()
        {
            string errorResponse = string.Empty;

            PromoService promoService = new PromoService();
            var promos = new List<Promo>();

            // get category
            int? categoryId = null;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "CategoryId" ) ) )
            {
                categoryId = Convert.ToInt32(GetAttributeValue( "CategoryId" ));
            }

            // get promo type
            bool isTopFree = false;
            bool isTopPaid = false;
            bool isFeatured = false;

            string promoType = GetAttributeValue( "PromoType" ); //"Top Paid, Top Free, Featured"
            switch ( promoType )
            {
                case "Top Paid":
                    isTopPaid = true;
                    break;
                case "Top Free":
                    isTopFree = true;
                    break;
                case "Featured":
                    isFeatured = true;
                    break;
            }

            promos = promoService.GetPromos( categoryId, out errorResponse, isTopFree, isFeatured, isTopPaid );

            // check for errors
            ErrorCheck( errorResponse );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "Promos", promos );

            // add link to detail page
            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add( "DetailPage", LinkedPageRoute( "DetailPage" ) );
            mergeFields.Add( "LinkedPages", linkedPages );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

        }

        private void ErrorCheck( string errorResponse )
        {
            if ( errorResponse != string.Empty )
            {
                pnlPromos.Visible = false;
                pnlError.Visible = true;
                lErrorMessage.Text = errorResponse;
            }
        }

        #endregion
    }
}