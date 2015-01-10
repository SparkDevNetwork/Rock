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
using Rock.Utility;

namespace RockWeb.Blocks.Store
{
    /// <summary>
    /// Lists packages that have been purchased in the Rock Store.
    /// </summary>
    [DisplayName( "Purchased Products" )]
    [Category( "Store" )]
    [Description( "Lists packages that have been purchased in the Rock Store." )]
    [LinkedPage( "Detail Page", "Page reference to use for the detail page.", false, "", "")]
    [LinkedPage( "Install Page", "Page reference to use for the install / update page.", false, "", "")]
    [LinkedPage( "Link Organization Page", "Page to allow the user to link an organization to the store.", false, "", "" )]
    public partial class PurchasedPackages : Rock.Web.UI.RockBlock
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
                DisplayPackages();
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
            DisplayPackages();
        }

        protected void rptPurchasedProducts_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var package = e.Item.DataItem as Package;
            var lbInstall = e.Item.FindControl( "lbInstall" ) as LinkButton;
            var lVersionNotes = e.Item.FindControl( "lVersionNotes" ) as Literal;

            InstalledPackage installedPackage = InstalledPackageService.InstalledPackageVersion( package.Id );
            PackageVersion latestVersion = null;

            // if package is installed
            if ( installedPackage != null )
            {
                // check that latest version is installed
                if ( package.Versions.Count > 0 )
                {
                    RockSemanticVersion rockVersion = RockSemanticVersion.Parse( Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber() );
                    latestVersion = package.Versions.Where( v => v.RequiredRockSemanticVersion <= rockVersion ).OrderByDescending( v => v.Id ).FirstOrDefault();
                }

                if ( installedPackage.VersionId != latestVersion.Id ) {
                    lbInstall.Text = "Update";

                    lVersionNotes.Text = String.Format( "<p><strong>Installed Version</strong><br/>{0}</p><p><strong>Latest Version</strong><br/>{1}</p>", installedPackage.VersionLabel, latestVersion.VersionLabel );
                }
                else 
                {
                    lbInstall.Text = "Installed";
                    lbInstall.Attributes.Add( "disabled", "disabled" );
                    lbInstall.CssClass = "btn btn-default margin-b-md";

                    lVersionNotes.Text = String.Format( "<p><strong>Installed Version</strong><br/>{0}</p>", installedPackage.VersionLabel );
                }
            }
        }

        protected void rptPurchasedProducts_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            int id = Convert.ToInt32( e.CommandArgument );

            switch( e.CommandName){
                case "PackageDetails":
                    queryParams = new Dictionary<string, string>();
                    queryParams.Add( "PackageId", id.ToString() );
                    NavigateToLinkedPage( "DetailPage", queryParams );
                    break;
                case "Install":
                    queryParams = new Dictionary<string, string>();
                    queryParams.Add( "PackageId", id.ToString() );
                    NavigateToLinkedPage( "InstallPage", queryParams );
                    break;
            }
        }

        #endregion

        #region Methods

        private void DisplayPackages()
        {
            string errorResponse = string.Empty;
            
            // check that the store is configured with an organization
            if ( StoreService.OrganizationIsConfigured() )
            {
                PackageService packageService = new PackageService();
                var purchases = packageService.GetPurchasedPackages( out errorResponse );

                // check errors
                ErrorCheck( errorResponse );

                if ( purchases.Count == 0 )
                {
                    lMessages.Text = "<div class='alert alert-warning'>No packages have been purchased for this organization.</div>";
                }

                rptPurchasedProducts.DataSource = purchases;
                rptPurchasedProducts.DataBind();
            }
            else
            {
                var queryParams = new Dictionary<string, string>();
                queryParams.Add( "ReturnUrl", Request.RawUrl );
                
                NavigateToLinkedPage( "LinkOrganizationPage", queryParams );
            }
        }

        private void ErrorCheck( string errorResponse )
        {
            if ( errorResponse != string.Empty )
            {
                pnlPackages.Visible = false;
                pnlError.Visible = true;
                lErrorMessage.Text = errorResponse;
            }
        }

        #endregion

    }
}