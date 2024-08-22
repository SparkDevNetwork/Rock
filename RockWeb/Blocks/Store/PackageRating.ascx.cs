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
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Store;
using Rock.Utility;
using Rock.VersionInfo;

namespace RockWeb.Blocks.Store
{

    [DisplayName( "Package Rating" )]
    [Category( "Store" )]
    [Description( "Enters ratings for a given package." )]
    [LinkedPage( "Package Detail Page", "Page reference to use for the package detail page.", false, "", "", 1 )]
    [Rock.SystemGuid.BlockTypeGuid( "5A7C11C2-4E9F-4AF6-8149-CB2093CE9727" )]
    public partial class PackageRating : Rock.Web.UI.RockBlock
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

            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddScriptLink( "~/Scripts/imagesloaded.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowPackage();
            }

            base.OnLoad( e );
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
            ShowPackage();
        }

        protected void lbInstall_Click( object sender, EventArgs e )
        {
            // get package id
            int packageId = -1;

            if ( !string.IsNullOrWhiteSpace( PageParameter( "PackageId" ) ) )
            {
                packageId = Convert.ToInt32( PageParameter( "PackageId" ) );

                var queryParams = new Dictionary<string, string>();
                queryParams = new Dictionary<string, string>();
                queryParams.Add( "PackageId", packageId.ToString() );
                NavigateToLinkedPage( "InstallPage", queryParams );
            }
        }

        protected void lbRate_Click( object sender, EventArgs e )
        {
            // get package id
            int packageId = -1;

            if ( !string.IsNullOrWhiteSpace( PageParameter( "PackageId" ) ) )
            {
                packageId = Convert.ToInt32( PageParameter( "PackageId" ) );

                var queryParams = new Dictionary<string, string>();
                queryParams = new Dictionary<string, string>();
                queryParams.Add( "PackageId", packageId.ToString() );

                NavigateToLinkedPage( "RatingPage", queryParams );
            }
        }

        #endregion

        #region Methods

        private void ShowPackage()
        {
            string errorResponse = string.Empty;

            // get package id
            int packageId = -1;

            if ( !string.IsNullOrWhiteSpace( PageParameter( "PackageId" ) ) )
            {
                packageId = Convert.ToInt32( PageParameter( "PackageId" ) );
            }

            PackageService packageService = new PackageService();
            var package = packageService.GetPackage( packageId, out errorResponse );

            string storeKey = StoreService.GetOrganizationKey();

            // check for errors
            ErrorCheck( errorResponse );

            lPackageName.Text = package.Name;
            lPackageDescription.Text = package.Description;
            lVendorName.Text = package.Vendor.Name;
            imgPackageImage.ImageUrl = package.PackageIconBinaryFile.ImageUrl;
            lbPackageLink.PostBackUrl = package.SupportUrl;
            lRatingSummary.Text = string.Format( "<div class='rating pull-left margin-r-sm'><small>{0}</small></div>", CreateRatingStars( package.Rating.ToString().AsInteger() ) );

            lAuthorInfo.Text = string.Format( "<a href='{0}'>{1}</a>", package.Vendor.Url, package.Vendor.Name );

            if ( package.IsFree )
            {
                lCost.Text = "<div class='pricelabel free'><h4>Free</h4></div>";
            }
            else
            {
                lCost.Text = string.Format( "<div class='pricelabel cost'><h4>${0}</h4></div>", package.Price );
            }

            // get latest version
            PackageVersion latestVersion = null;
            if ( package.Versions.Count > 0 )
            {
                RockSemanticVersion rockVersion = RockSemanticVersion.Parse( VersionInfo.GetRockSemanticVersionNumber() );
                latestVersion = package.Versions.Where( v => v.RequiredRockSemanticVersion <= rockVersion ).OrderByDescending( v => v.Id ).FirstOrDefault();
            }

            // determine the state of the install button (install, update, buy or installed)
            InstalledPackage installedPackage = InstalledPackageService.InstalledPackageVersion( package.Id );

            if ( installedPackage == null )
            {
                // it's not installed
                // todo add logic that it's not installed but has been purchased
                if ( package.IsFree )
                {
                    // the package is free
                    lbInstall.Text = "Install";
                }
                else
                {
                    if ( package.IsPurchased )
                    {
                        lbInstall.Text = "Install";
                        lInstallNotes.Text = string.Format( "<small>Purchased {0}</small>", package.PurchasedDate.ToShortDateString() );

                        // set rating link button
                        lbRate.Visible = true;
                        lbRate.PostBackUrl = GetRockPostbackUrl( storeKey, packageId.ToString(), null );
                    }
                    else
                    {
                        lbInstall.Text = "Buy";
                    }
                }
            }
            else
            {
                if ( installedPackage.VersionId == latestVersion.Id )
                {
                    // have the latest version installed
                    lbInstall.Text = "Installed";
                    lbInstall.Enabled = false;
                    lbInstall.CssClass = "btn btn-default btn-install";
                    lbInstall.Attributes.Add( "disabled", "disabled" );

                    // set rating link button
                    lbRate.Visible = true;
                    lbRate.PostBackUrl = GetRockPostbackUrl( storeKey, packageId.ToString(), installedPackage.VersionId.ToString() );

                }
                else
                {
                    // have a previous version installed
                    lbInstall.Text = "Update";
                    lInstallNotes.Text = string.Format( "<small>You have {0} installed</small>", installedPackage.VersionLabel );

                    // set rating link button
                    lbRate.Visible = true;
                    lbRate.PostBackUrl = GetRockPostbackUrl( storeKey, packageId.ToString(), installedPackage.VersionId.ToString() );

                }
            }

            if ( latestVersion != null )
            {

                rptScreenshots.DataSource = latestVersion.Screenshots;
                rptScreenshots.DataBind();

                lLatestVersionLabel.Text = latestVersion.VersionLabel;
                lLatestVersionDescription.Text = latestVersion.Description;

                // alert the user if a newer version exists but requires a rock update
                if ( package.Versions.Where( v => v.Id > latestVersion.Id ).Count() > 0 )
                {
                    var lastVersion = package.Versions.OrderByDescending( v => v.RequiredRockSemanticVersion ).FirstOrDefault();
                    lVersionWarning.Text = string.Format( "<div class='alert alert-info'>A newer version of this item is available but requires Rock v{0}.{1}.</div>",
                                                    lastVersion.RequiredRockSemanticVersion.Minor.ToString(),
                                                    lastVersion.RequiredRockSemanticVersion.Patch.ToString() );
                }

                lLastUpdate.Text = latestVersion.AddedDate.ToShortDateString();
                lRequiredRockVersion.Text = string.Format( "v{0}.{1}",
                                                latestVersion.RequiredRockSemanticVersion.Minor.ToString(),
                                                latestVersion.RequiredRockSemanticVersion.Patch.ToString() );
                lDocumenationLink.Text = string.Format( "<a href='{0}'>Support Link</a>", latestVersion.DocumentationUrl );

                // fill in previous version info
                rptAdditionalVersions.DataSource = package.Versions.Where( v => v.Id < latestVersion.Id );
                rptAdditionalVersions.DataBind();

                // get the details for the latest version
                PackageVersion latestVersionDetails = new PackageVersionService().GetPackageVersion( latestVersion.Id, out errorResponse );

                // check for errors
                ErrorCheck( errorResponse );
            }
            else
            {
                // hide install button
                lbInstall.Visible = false;

                // display info on what Rock version you need to be on to run this package
                if ( package.Versions.Count > 0 )
                {
                    var firstVersion = package.Versions.OrderBy( v => v.RequiredRockSemanticVersion ).FirstOrDefault();
                    var lastVersion = package.Versions.OrderByDescending( v => v.RequiredRockSemanticVersion ).FirstOrDefault();

                    if ( firstVersion == lastVersion )
                    {
                        lVersionWarning.Text = string.Format( "<div class='alert alert-warning'>This item requires Rock version v{0}.{1}.</div>",
                                                    lastVersion.RequiredRockSemanticVersion.Minor.ToString(),
                                                    lastVersion.RequiredRockSemanticVersion.Patch.ToString() );
                    }
                    else
                    {
                        lVersionWarning.Text = string.Format( "<div class='alert alert-warning'>This item requires at least Rock version v{0}.{1} but the latest version requires v{2}.{3}.</div>",
                                                    firstVersion.RequiredRockSemanticVersion.Minor.ToString(),
                                                    firstVersion.RequiredRockSemanticVersion.Patch.ToString(),
                                                    lastVersion.RequiredRockSemanticVersion.Minor.ToString(),
                                                    lastVersion.RequiredRockSemanticVersion.Patch.ToString() );
                    }
                }

            }
        }

        private string GetRockPostbackUrl( string storeKey, string packageId, string installedVersionId )
        {
            var baseUrl = ConfigurationManager.AppSettings["RockStoreUrl"].EnsureTrailingForwardslash();

            if ( installedVersionId.IsNullOrWhiteSpace() )
            {
                return string.Format( "{0}Store/Rate?OrganizationKey={1}&PackageId={2}", baseUrl, storeKey, packageId );
            }
            return string.Format( "{0}Store/Rate?OrganizationKey={1}&PackageId={2}&InstalledVersionId={3}", baseUrl, storeKey, packageId, installedVersionId );
        }

        private string CreateRatingStars( int rating )
        {
            var starCounter = 0;
            StringBuilder starMarkup = new StringBuilder();

            for ( int i = 0; i < rating; i++ )
            {
                starMarkup.Append( "<i class='fa fa-rating-on'></i>" );
                starCounter++;
            }

            for ( int i = starCounter; i <= 5; i++ )
            {
                starMarkup.Append( "<i class='fa fa-rating-off'></i>" );
            }

            return starMarkup.ToString();
        }


        private void ErrorCheck( string errorResponse )
        {
            if ( errorResponse != string.Empty )
            {
                pnlPackageDetails.Visible = false;
                pnlError.Visible = true;
                lErrorMessage.Text = errorResponse;
            }
        }

        #endregion

    }
}