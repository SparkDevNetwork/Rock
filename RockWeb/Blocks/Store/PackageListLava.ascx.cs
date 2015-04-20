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
    [DisplayName( "Package List Lava" )]
    [Category( "Store" )]
    [Description( "Lists Rock Store packages using a Lava template." )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the packages", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 400, true, @"{% include '~/Assets/Lava/Store/PackageList.lava' %}", "", 2 )]
    [BooleanField("Enable Debug", "Display a list of merge fields available for lava.", false, "", 3)]
    [CustomRadioListField("Package Type", "Display the packages of the specified type", "External Application, Theme", true, "", "", 0)]
    [TextField("Category Id", "Filters packages for a specific category id. If none is provided it will show all packages.", false, "","", 1)]
    [LinkedPage( "Detail Page", "Page reference to use for the detail page.", false, "", "", 4 )]
    [BooleanField( "Set Page Title", "Determines if the block should set the page title with the category name (category name must be provided via the query string as &CategoryName=.)", false )]
    public partial class PackageListLava : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

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
                LoadPackages();
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
            LoadPackages();
        }

        #endregion

        #region Methods

        private void LoadPackages()
        {
            // get category
            int? categoryId = null;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "CategoryId" ) ) )
            {
                categoryId = Convert.ToInt32(GetAttributeValue( "CategoryId" ));
            }
            else if ( !string.IsNullOrWhiteSpace( PageParameter( "CategoryId" ) ) )
            {
                categoryId = Convert.ToInt32( PageParameter( "CategoryId" ) );
            }

            string categoryName = PageParameter( "CategoryName" );
            if ( GetAttributeValue( "SetPageTitle" ).AsBoolean() && !string.IsNullOrWhiteSpace(categoryName) )
            {
                string pageTitle = "Items for " + categoryName;
                RockPage.PageTitle = pageTitle;
                RockPage.BrowserTitle = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
                RockPage.Header.Title = String.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
            }
            
            PackageService packageService = new PackageService();
            var packages = packageService.GetAllPackages( categoryId );

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            // add link to detail page
            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add( "DetailPage", LinkedPageUrl( "DetailPage", null ) );
            mergeFields.Add( "LinkedPages", linkedPages );

            var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( CurrentPerson );
            globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

            mergeFields.Add( "Packages", packages );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields );

            // show debug info
            if ( GetAttributeValue( "EnableDebug" ).AsBoolean() && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo(); ;
            }
        }

        #endregion
    }
}