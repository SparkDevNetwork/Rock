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
    [DisplayName( "Package Install" )]
    [Category( "Store" )]
    [Description( "Installs a package." )]
    [LinkedPage( "Link Organization Page", "Page to allow the user to link an organization to the store.", false, "", "")]
    public partial class PackageInstall : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        private string _installPurchaseMessage = "Login below with your Rock Store account. Your credit card on file will be charged ${0}.";
        private string _installFreeMessage = "Login below with your Rock Store account to install this free package.";
        private string _updateMessage = "Login below with your Rock Store account to upgrade this package.";
        private string _installPreviousPurchase = "Login below with your Rock Store account to install this previously purchased package.";

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
                DisplayPackageInfo();
            }
        }

        protected void cbAgreeToTerms_CheckedChanged( object sender, EventArgs e )
        {
            CheckBox cbAgreeToTerms = sender as CheckBox;
            btnInstall.Enabled = cbAgreeToTerms.Checked;
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
            DisplayPackageInfo();
        }

        #endregion

        #region Methods

        private void DisplayPackageInfo()
        {
            string errorResponse = string.Empty;

            // check that store is configured
            if ( StoreService.OrganizationIsConfigured() )
            {
                // get package id
                int packageId = -1;

                if ( !string.IsNullOrWhiteSpace( PageParameter( "PackageId" ) ) )
                {
                    packageId = Convert.ToInt32( PageParameter( "PackageId" ) );
                }

                PackageService packageService = new PackageService();
                var package = packageService.GetPackage( packageId, out errorResponse );

                // check for errors
                ErrorCheck( errorResponse );

                lPackageName.Text = package.Name;
                lPackageDescription.Text = package.Description;

                lPackageImage.Text = String.Format( @"<div class=""margin-b-md"" style=""
                                background: url('{0}') no-repeat center;
                                width: 100%;
                                height: 140px;"">
                                </div>", package.PackageIconBinaryFile.ImageUrl );

                if ( package.IsFree )
                {
                    lCost.Text = "<div class='pricelabel free'><h4>Free</h4></div>";
                    lInstallMessage.Text = _installFreeMessage;
                }
                else
                {
                    lCost.Text = string.Format( "<div class='pricelabel cost'><h4>${0}</h4></div>", package.Price );
                    lInstallMessage.Text = string.Format( _installPurchaseMessage, package.Price.ToString() );
                }

                if ( package.IsPurchased )
                {
                    // check if it's installed
                    // determine the state of the install button (install, update, buy or installed)
                    InstalledPackage installedPackage = InstalledPackageService.InstalledPackageVersion( package.Id );

                    if ( installedPackage == null )
                    {
                        lCost.Visible = false;
                        lInstallMessage.Text = _installPreviousPurchase;
                    }
                    else
                    {
                        lCost.Visible = false;
                        lInstallMessage.Text = _updateMessage;
                        btnInstall.Text = "Update";
                    }

                }
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
                pnlInstall.Visible = false;
                pnlError.Visible = true;
                lErrorMessage.Text = errorResponse;
            }
        }

        #endregion
    }
}