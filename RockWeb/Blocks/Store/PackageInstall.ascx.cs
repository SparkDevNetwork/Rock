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
using System.Net;
using System.IO.Compression;
using Microsoft.Web.XmlTransform;


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

        const string _xdtExtension = ".rock.xdt";
        #endregion

        #region Properties

        // used for public / protected properties

        int packageId = -1;

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

            // get package id
            if ( !string.IsNullOrWhiteSpace( PageParameter( "PackageId" ) ) )
            {
                packageId = Convert.ToInt32( PageParameter( "PackageId" ) );
            }

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

        protected void btnInstall_Click( object sender, EventArgs e )
        {
            StoreService storeService = new StoreService();

            var installResponse = storeService.Purchase( txtUsername.Text, txtPassword.Text, packageId );

            switch ( installResponse.PurchaseResult )
            {
                case PurchaseResult.AuthenicationFailed:
                    lMessages.Text = string.Format("<div class='alert alert-warning margin-t-md'><strong>Could Not Authenicate</strong> {0}</div>", installResponse.Message);
                    break;
                case PurchaseResult.Error:
                    lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>An Error Occurred</strong> {0}</div>", installResponse.Message );
                    break;
                case PurchaseResult.NoCardOnFile:
                    lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>No Card On File</strong> No credit card is on file for your organization. Please add a card from your <a href='{0}'>Account Page</a>.</div>", ResolveRockUrl("~/RockShop/Account") );
                    break;
                case PurchaseResult.NotAuthorized:
                    lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Unauthorized</strong> You are not currently authorized to make purchased for this organization. Please see your organization's primary contact to enable your account for purchases.</div>" );
                    break;
                case PurchaseResult.PaymentFailed:
                    lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Payment Error</strong> An error occurred will processing the credit card on file for your organization. The error was: {0}. Please update your card's information from your <a href='{1}'>Account Page</a>.</div>", installResponse.Message, ResolveRockUrl("~/RockShop/Account") );
                    break;
                case PurchaseResult.Success:
                    ProcessInstall( installResponse );
                    break;
            }
        }

        #endregion

        #region Methods

        private void ProcessInstall( PurchaseResponse purchaseResponse )
        {

            if ( purchaseResponse.PackageInstallSteps != null )
            {

                foreach ( var installStep in purchaseResponse.PackageInstallSteps )
                {
                    string appRoot = Server.MapPath( "~/" );
                    string rockShopWorkingDir = appRoot + "App_Data/RockShop";
                    string packageDirectory = string.Format( "{0}/{1} - {2}", rockShopWorkingDir, purchaseResponse.PackageId, purchaseResponse.PackageName );
                    string sourceFile = installStep.InstallPackageUrl; 
                    string destinationFile = string.Format("{0}/{1} - {2}.plugin", packageDirectory, installStep.VersionId, installStep.VersionLabel);

                    // check that the RockShop directory exists
                    if ( !Directory.Exists( rockShopWorkingDir ) )
                    {
                        Directory.CreateDirectory( rockShopWorkingDir );
                    }

                    // create package directory
                    if ( !Directory.Exists( packageDirectory ) )
                    {
                        Directory.CreateDirectory( packageDirectory );
                    }

                    // download file
                    try
                    {
                        WebClient wc = new WebClient();
                        wc.DownloadFile( sourceFile, destinationFile );
                    }
                    catch ( Exception ex )
                    {
                        CleanUpPackage( destinationFile );
                        lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Downloading Package</strong> An error occurred while downloading package from the store. Please try again later. <br><em>Error: {0}</em></div>", ex.Message );
                        return;
                    }

                   // process zip folder
                    try
                    {
                        using ( ZipArchive packageZip = ZipFile.OpenRead( destinationFile ) )
                        {
                            // unzip content folder and process xdts
                            foreach ( ZipArchiveEntry entry in packageZip.Entries.Where(e => e.FullName.StartsWith("content/")) )
                            {
                               if ( entry.FullName.EndsWith( _xdtExtension, StringComparison.OrdinalIgnoreCase ) )
                                {
                                    // process xdt
                                    string filename = entry.FullName.Replace( "content/", "" );
                                    string transformTargetFile = appRoot + filename.Substring( 0, filename.LastIndexOf( _xdtExtension ) );

                                    // process transform
                                    using ( XmlTransformableDocument document = new XmlTransformableDocument() )
                                    {
                                        document.PreserveWhitespace = true;
                                        document.Load( transformTargetFile );

                                        using ( XmlTransformation transform = new XmlTransformation( entry.Open(), null))
                                        {
                                            if ( transform.Apply( document ) )
                                            {
                                                document.Save( transformTargetFile );
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // process all content files
                                    string fullpath = Path.Combine( appRoot, entry.FullName.Replace("content/", "") );
                                    string directory = Path.GetDirectoryName( fullpath ).Replace("content/", "");

                                    if ( !Directory.Exists( directory ) )
                                    {
                                        Directory.CreateDirectory( directory );
                                    }

                                    entry.ExtractToFile( fullpath, true );
                                }
                            }

                            // process install.sql
                            try
                            {
                                var sqlInstallEntry = packageZip.Entries.Where( e => e.FullName == "install/run.sql" ).FirstOrDefault();
                                if (sqlInstallEntry != null) {
                                    string sqlScript = System.Text.Encoding.Default.GetString(sqlInstallEntry.Open().ReadBytesToEnd());

                                    if ( !string.IsNullOrWhiteSpace( sqlScript ) )
                                    {
                                        using ( var context = new RockContext() )
                                        {
                                            context.Database.ExecuteSqlCommand( sqlScript );
                                        }
                                    }
                                }
                            }
                            catch ( Exception ex )
                            {
                                lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Updating Database</strong> An error occurred while updating the database. <br><em>Error: {0}</em></div>", ex.Message );
                                return;
                            }

                            // process deletefile.lst
                            try
                            {
                                var deleteListEntry = packageZip.Entries.Where( e => e.FullName == "install/deletefile.lst" ).FirstOrDefault();
                                if ( deleteListEntry != null )
                                {
                                
                                    string deleteList = System.Text.Encoding.Default.GetString( deleteListEntry.Open().ReadBytesToEnd() );

                                    string[] itemsToDelete = deleteList.Split( new string[] { Environment.NewLine }, StringSplitOptions.None );

                                    foreach ( string deleteItem in itemsToDelete )
                                    {
                                        if ( !string.IsNullOrWhiteSpace( deleteItem ) )
                                        {
                                            string deleteItemFullPath = appRoot + deleteItem;

                                            if ( Directory.Exists( deleteItemFullPath ) )
                                            {
                                                Directory.Delete( deleteItemFullPath, true);
                                            }

                                            if ( File.Exists( deleteItemFullPath ) )
                                            {
                                                File.Delete( deleteItemFullPath );
                                            }
                                        }
                                    }
                                
                                }
                            }
                            catch ( Exception ex )
                            {
                                lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Modifing Files</strong> An error occurred while modifing files. <br><em>Error: {0}</em></div>", ex.Message );
                                return;
                            }
                            
                        }
                    }
                    catch ( Exception ex )
                    {
                        lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Extracting Package</strong> An error occurred while extracting the contents of the package. <br><em>Error: {0}</em></div>", ex.Message );
                        return;
                    }

                    // update package install json file
                    InstalledPackageService.SaveInstall( purchaseResponse.PackageId, purchaseResponse.PackageName, installStep.VersionId, installStep.VersionLabel, purchaseResponse.VendorId, purchaseResponse.VendorName, purchaseResponse.InstalledBy );
                
                    // Clear all cached items
                    Rock.Web.Cache.RockMemoryCache.Clear();

                    // Clear the static object that contains all auth rules (so that it will be refreshed)
                    Rock.Security.Authorization.Flush();

                    // show result message
                    lMessages.Text = string.Format( "<div class='alert alert-success margin-t-md'><strong>Package Installed</strong><p>{0}</p>", installStep.PostInstallInstructions );
                }
            }
            else
            {
                lMessages.Text = "<div class='alert alert-warning margin-t-md'><strong>Error</strong> Install package was not valid. Please try again later.";
            }
        }

        private void CleanUpPackage(string packageFile)
        {
            try
            {
                if ( File.Exists( packageFile ) )
                {
                    File.Delete( packageFile );
                }

            } catch(Exception ex){
                lMessages.Text = string.Format( "<div class='alert alert-warning margin-t-md'><strong>Error Cleaning Up</strong> An error occurred while cleaning up after the install. <br><em>Error: {0}</em></div>", ex.Message );
                return;
            }
        }

        private void DisplayPackageInfo()
        {
            string errorResponse = string.Empty;

            // check that store is configured
            if ( StoreService.OrganizationIsConfigured() )
            {
                
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