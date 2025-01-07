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
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Web.XmlTransform;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Store;
using Rock.Utility;
using Rock.VersionInfo;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Store
{
    /// <summary>
    /// Installs a package that has been downloaded in the Rock Shop.
    /// </summary>
    [DisplayName( "Package Install" )]
    [Category( "Store" )]
    [Description( "Installs a package." )]

    #region Block Attributes

    [LinkedPage( "Link Organization Page",
        Key = AttributeKey.LinkOrganizationPage,
        Description = "Page to allow the user to link an organization to the store.",
        IsRequired = false,
        Order = 0 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "EA60C1AB-ADAB-4EDF-94F8-B0FE214B6F15" )]
    public partial class PackageInstall : Rock.Web.UI.RockBlock
    {
        #region Constants

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string LinkOrganizationPage = "LinkOrganizationPage";
        }

        #endregion Attribute Keys

        private const string INSTALL_PURCHASE_MESSAGE = "Log in below with your Rock Store account to install the <em>{0}</em> package. Your credit card on file will be charged ${1}.";
        private const string INSTALL_FREE_MESSAGE = "Log in below with your Rock Store account to install free <em>{0}</em> package.";
        private const string UPDATE_MESSAGE = "Log in below with your Rock Store account to upgrade this package.";
        private const string INSTALL_PREVIOUS_MESSAGE = "Log in below with your Rock Store account to install this previously purchased package.";
        private const string XDT_EXTENSION = ".rock.xdt";

        #endregion Constants

        #region Fields

        private int _packageId = -1;

        #endregion Fields

        #region Base Control Methods

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
            // get package id
            if ( !string.IsNullOrWhiteSpace( PageParameter( "PackageId" ) ) )
            {
                _packageId = Convert.ToInt32( PageParameter( "PackageId" ) );
            }

            if ( !Page.IsPostBack )
            {
                DisplayPackageInfo();
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the CheckedChanged event of the cbAgreeToTerms control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cbAgreeToTerms_CheckedChanged( object sender, EventArgs e )
        {
            CheckBox cbAgreeToTerms = sender as CheckBox;
            btnInstall.Enabled = cbAgreeToTerms.Checked;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            DisplayPackageInfo();
        }

        /// <summary>
        /// Handles the Click event of the btnInstall control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnInstall_Click( object sender, EventArgs e )
        {
            StoreService storeService = new StoreService();

            string errorResponse = string.Empty;
            var installResponse = storeService.Purchase( txtUsername.Text, txtPassword.Text, _packageId, out errorResponse );
            if ( installResponse != null )
            {
                switch ( installResponse.PurchaseResult )
                {
                    case PurchaseResult.AuthenticationFailed:
                        lMessages.Text = $"<div class='alert alert-warning margin-t-md'><strong>Could Not Authenticate</strong> {installResponse.Message} If you need further help see the <a href='https://rockrms.com/RockShopHelp'>Rock Shop Help Page</a>.</div>";
                        break;
                    case PurchaseResult.Error:
                        lMessages.Text = $"<div class='alert alert-warning margin-t-md'><strong>An Error Occurred</strong> {installResponse.Message}</div>";
                        break;
                    case PurchaseResult.NoCardOnFile:
                        lMessages.Text = $"<div class='alert alert-warning margin-t-md'><strong>No Card On File</strong> No credit card is on file for your organization. Please add a card from your <a href='{ResolveRockUrl( "~/RockShop/Account" )}'>Account Page</a> or see the <a href='https://rockrms.com/RockShopHelp'>Rock Shop Help Page</a>.</div>";
                        break;
                    case PurchaseResult.NotAuthorized:
                        lMessages.Text = $"<div class='alert alert-warning margin-t-md'><strong>Unauthorized</strong> You are not currently authorized to make purchases for this organization. Please see your organization's primary contact to enable your account for purchases or see the <a href='https://rockrms.com/RockShopHelp'>Rock Shop Help Page</a>.</div>";
                        break;
                    case PurchaseResult.PaymentFailed:
                        lMessages.Text = $"<div class='alert alert-warning margin-t-md'><strong>Payment Error</strong> An error occurred while processing the credit card on file for your organization. The error was: {installResponse.Message}. Please update your card's information from your <a href='{ResolveRockUrl( "~/RockShop/Account" )}'>Account Page</a> or see the <a href='https://rockrms.com/RockShopHelp'>Rock Shop Help Page</a>.</div>";
                        break;
                    case PurchaseResult.Success:
                        ProcessInstall( installResponse );
                        break;
                }
            }
            else
            {
                lMessages.Text = $"<div class='alert alert-danger margin-t-md'><strong>Install Error</strong> An error occurred while attempting to authenticate your install of this package. The error was: {( string.IsNullOrWhiteSpace( errorResponse ) ? "Unknown" : errorResponse )}.</div>";
            }
        }

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Gets the version of the currently installed package.
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns>-1 if the package is not installed, otherwise the version id of the installed package.</returns>
        private int GetCurrentlyInstalledPackageVersion( string packageName )
        {
            var installedPackages = InstalledPackageService.GetInstalledPackages().OrderBy( p => p.PackageName ).OrderByDescending( p => p.InstallDateTime );
            foreach ( var installedPackage in installedPackages )
            {
                if ( installedPackage.PackageName == packageName )
                {
                    return installedPackage.VersionId;
                }
            }
            return -1;
        }

        /// <summary>
        /// Process the package install.
        /// </summary>
        /// <param name="purchaseResponse">The <see cref="PurchaseResponse"/> object from the Rock Shop.</param>
        private void ProcessInstall( PurchaseResponse purchaseResponse )
        {
            if ( purchaseResponse.PackageInstallSteps == null )
            {
                lMessages.Text = "<div class='alert alert-warning margin-t-md'><strong>Error</strong> Install package was not valid. Please try again later.";
                return;
            }

            var currentlyInstalledPackageVersion = GetCurrentlyInstalledPackageVersion( purchaseResponse.PackageName );
            RockSemanticVersion rockVersion = RockSemanticVersion.Parse( VersionInfo.GetRockSemanticVersionNumber() );

            // Get package install steps that are newer than the currently installed package and apply to this
            // version of Rock.
            var packageInstallSteps = purchaseResponse.PackageInstallSteps
                .Where( s => s.RequiredRockSemanticVersion <= rockVersion )
                .Where( s => s.VersionId > currentlyInstalledPackageVersion );

            string appRoot = Server.MapPath( "~/" );

            // check that the RockShop directory exists
            string rockShopWorkingDir = appRoot + "App_Data/RockShop";
            EnsureDirectoryExists( rockShopWorkingDir );

            foreach ( var installStep in packageInstallSteps )
            {
                var wasStepInstalled = InstallPackageStep( installStep, purchaseResponse, appRoot, rockShopWorkingDir );

                if ( !wasStepInstalled )
                {
                    // If an install step failed, exit the loop to stop processing.  InstallPackageStep() should
                    // have already notified the user of the problem by adding a message to the lMessages control.
                    break;
                }
            }
        }

        /// <summary>
        /// Checks that a directory exists and creates it if it doesn't.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        private void EnsureDirectoryExists( string directoryPath )
        {
            if ( !Directory.Exists( directoryPath ) )
            {
                Directory.CreateDirectory( directoryPath );
            }
        }

        /// <summary>
        /// Process a package install step.
        /// </summary>
        /// <param name="installStep">The <see cref="PackageInstallStep"/>.</param>
        /// <param name="purchaseResponse">The <see cref="PurchaseResponse"/>.</param>
        /// <param name="appRoot">The application root directory path.</param>
        /// <param name="rockShopWorkingDir">The Rock Shop working directory path.</param>
        /// <returns>True if the installation was successful.</returns>
        private bool InstallPackageStep( PackageInstallStep installStep, PurchaseResponse purchaseResponse, string appRoot, string rockShopWorkingDir )
        {
            bool wasActionTaken = false;

            // create package directory
            string packageDirectory = $"{rockShopWorkingDir}/{purchaseResponse.PackageId} - {purchaseResponse.PackageName}";
            EnsureDirectoryExists( packageDirectory );

            string destinationFile = $"{packageDirectory}/{installStep.VersionId} - {installStep.VersionLabel}.plugin";

            // download file
            try
            {
                string sourceFile = installStep.InstallPackageUrl;
                WebClient wc = new WebClient();
                wc.DownloadFile( sourceFile, destinationFile );
            }
            catch ( Exception ex )
            {
                CleanUpPackage( destinationFile );
                lMessages.Text = $"<div class='alert alert-warning margin-t-md'><strong>Error Downloading Package</strong> An error occurred while downloading package from the store. Please try again later. <br><em>Error: {ex.Message}</em></div>";
                return false;
            }

            // process zip folder
            try
            {
                using ( ZipArchive packageZip = ZipFile.OpenRead( destinationFile ) )
                {
                    // unzip content folder and process xdts
                    foreach ( ZipArchiveEntry entry in packageZip.Entries )
                    {
                        wasActionTaken = ProcessZipEntry( entry, appRoot ) || wasActionTaken;
                    }

                    // process run.sql
                    wasActionTaken = ProcessRunSQL( packageZip ) || wasActionTaken;

                    // process deletefile.lst
                    wasActionTaken = ProcessDeleteFileList( packageZip, appRoot ) || wasActionTaken;
                }
            }
            catch ( Exception ex )
            {
                lMessages.Text = $"<div class='alert alert-warning margin-t-md'><strong>Error Extracting Package</strong> An error occurred while extracting the contents of the package. <br><em>Error: {ex.Message}</em></div>";
                return false;
            }

            if ( !wasActionTaken )
            {
                lMessages.Text = $"<div class='alert alert-warning margin-t-md'><strong>Package Installation Error</strong> Package version {installStep.VersionLabel} failed to install because no actions were completed.  This may be due to an improperly packaged plugin file.  Please contact the package administrator for support.</div>";
                return false;
            }

            // update package install json file
            InstalledPackageService.SaveInstall( purchaseResponse.PackageId, purchaseResponse.PackageName, installStep.VersionId, installStep.VersionLabel, purchaseResponse.VendorId, purchaseResponse.VendorName, purchaseResponse.InstalledBy );

            // Clear all cached items
            RockCache.ClearAllCachedItems();

            // Hide store login on success
            lInstallMessage.Visible = false;
            txtUsername.Visible = false;
            txtPassword.Visible = false;
            cbAgreeToTerms.Visible = false;
            btnInstall.Visible = false;
            // show result message
            lMessages.Text = $"<div class='alert alert-success margin-t-md'><strong>Package Installed</strong><p>{installStep.PostInstallInstructions}</p>";

            return true;
        }

        /// <summary>
        /// Process the ZIP entry and extract the content folder and apply any XML data transformations (from
        /// content/*.rock.xdt files).
        /// </summary>
        /// <param name="packageZip">The <see cref="ZipArchive"/>.</param>
        /// <param name="appRoot">The application root directory path.</param>
        /// <returns>True if some action was successfully taken.</returns>
        private bool ProcessZipEntry( ZipArchiveEntry entry, string appRoot )
        {
            bool wasActionTaken = false;

            // if entry is a directory ignore it
            if ( entry.Length == 0 )
            {
                return false;
            }

            // replace backslashes with forward slashes in case the ZIP file entries were encoded incorrectly.
            var fullName = entry.FullName.Replace( "\\", "/" );
            if ( !fullName.StartsWith( "content/", StringComparison.OrdinalIgnoreCase ) )
            {
                return false;
            }

            if ( fullName.EndsWith( XDT_EXTENSION, StringComparison.OrdinalIgnoreCase ) )
            {
                // process xdt
                string filename = fullName.ReplaceFirstOccurrence( "content/", string.Empty );
                string transformTargetFile = appRoot + filename.Substring( 0, filename.LastIndexOf( XDT_EXTENSION ) );

                // process transform
                using ( XmlTransformableDocument document = new XmlTransformableDocument() )
                {
                    document.PreserveWhitespace = true;
                    document.Load( transformTargetFile );

                    using ( XmlTransformation transform = new XmlTransformation( entry.Open(), null ) )
                    {
                        if ( transform.Apply( document ) )
                        {
                            document.Save( transformTargetFile );
                            wasActionTaken = true;
                        }
                    }
                }
            }
            else
            {
                // process content files
                string fullpath = Path.Combine( appRoot, fullName.ReplaceFirstOccurrence( "content/", string.Empty ) );
                string directory = Path.GetDirectoryName( fullpath ).ReplaceFirstOccurrence( "content/", string.Empty );

                EnsureDirectoryExists( directory );
                entry.ExtractToFile( fullpath, true );
                wasActionTaken = true;
            }

            return wasActionTaken;
        }

        /// <summary>
        /// Process the run.sql file.
        /// </summary>
        /// <param name="packageZip">The <see cref="ZipArchive"/>.</param>
        /// <returns>True if some action was taken.</returns>
        private bool ProcessRunSQL( ZipArchive packageZip )
        {
            bool wasActionTaken = false;

            try
            {
                // Find the run.sql file, if it exists.  (Note: look for either forward or back slashes, in case the ZIP file is encoded incorrectly.)
                var sqlInstallEntry = packageZip.Entries.Where( e => e.FullName == "install/run.sql" || e.FullName == "install\\run.sql" ).FirstOrDefault();
                if ( sqlInstallEntry == null )
                {
                    return false; // file is not present, nothing to do.
                }

                string sqlScript = Encoding.Default.GetString( sqlInstallEntry.Open().ReadBytesToEnd() );
                if ( string.IsNullOrWhiteSpace( sqlScript ) )
                {
                    return false; // nothing to run.
                }

                wasActionTaken = true;

                using ( var context = new RockContext() )
                {
                    context.Database.ExecuteSqlCommand( sqlScript );
                }
            }
            catch ( Exception ex )
            {
                lMessages.Text = $"<div class='alert alert-warning margin-t-md'><strong>Error Updating Database</strong> An error occurred while updating the database. <br><em>Error: {ex.Message}</em></div>";
                return false;
            }

            return wasActionTaken;
        }

        /// <summary>
        /// Process the delete file list.
        /// </summary>
        /// <param name="packageZip">The <see cref="ZipArchive"/>.</param>
        /// <param name="appRoot">The application root directory path.</param>
        /// <returns>True if some action was taken.</returns>
        private bool ProcessDeleteFileList( ZipArchive packageZip, string appRoot )
        {
            bool wasActionTaken = false;

            try
            {
                // Find the delete list file, if it exists.  (Note: look for either forward or back slashes, in case the ZIP file is encoded incorrectly.)
                var deleteListEntry = packageZip.Entries.Where( e => e.FullName == "install/deletefile.lst" || e.FullName == "install\\deletefile.lst" ).FirstOrDefault();
                if ( deleteListEntry == null )
                {
                    return false; // file is not present, nothing to do.
                }

                string deleteList = Encoding.Default.GetString( deleteListEntry.Open().ReadBytesToEnd() );

                string[] itemsToDelete = deleteList.Split( new string[] { Environment.NewLine }, StringSplitOptions.None );

                foreach ( string deleteItem in itemsToDelete )
                {
                    if ( string.IsNullOrWhiteSpace( deleteItem ) )
                    {
                        continue; // empty line, nothing to delete, ignore it.
                    }

                    // We will assume an action was taken at this point so that the install step does not get
                    // flagged as invalid, even if the file does not actually get deleted, in case files were
                    // removed manually.
                    wasActionTaken = true;

                    string deleteItemFullPath = appRoot + deleteItem;

                    if ( Directory.Exists( deleteItemFullPath ) )
                    {
                        Directory.Delete( deleteItemFullPath, true );
                    }

                    if ( File.Exists( deleteItemFullPath ) )
                    {
                        File.Delete( deleteItemFullPath );
                    }
                }
            }
            catch ( Exception ex )
            {
                lMessages.Text = $"<div class='alert alert-warning margin-t-md'><strong>Error Modifying Files</strong> An error occurred while modifying files. <br><em>Error: {ex.Message}</em></div>";
                return false;
            }

            return wasActionTaken;
        }

        /// <summary>
        /// Cleans up/removes the package file.
        /// </summary>
        /// <param name="packageFile">The package file path.</param>
        private void CleanUpPackage( string packageFile )
        {
            try
            {
                if ( File.Exists( packageFile ) )
                {
                    File.Delete( packageFile );
                }
            }
            catch( Exception ex )
            {
                lMessages.Text = $"<div class='alert alert-warning margin-t-md'><strong>Error Cleaning Up</strong> An error occurred while cleaning up after the install. <br><em>Error: {ex.Message}</em></div>";
                return;
            }
        }

        /// <summary>
        /// Display the package info.
        /// </summary>
        private void DisplayPackageInfo()
        {
            string errorResponse = string.Empty;

            // check that store is configured
            if ( StoreService.OrganizationIsConfigured() )
            {
                PackageService packageService = new PackageService();
                var package = packageService.GetPackage( _packageId, out errorResponse );

                // check for errors
                ErrorCheck( errorResponse );

                //lPackageName.Text = package.Name;
                imgPackageImage.ImageUrl = package.PackageIconBinaryFile.ImageUrl;

                if ( package.IsFree )
                {
                    //lCost.Text = "<div class='pricelabel free'><h4>Free</h4></div>";
                    lInstallMessage.Text = string.Format( INSTALL_FREE_MESSAGE, package.Name );
                }
                else
                {
                    //lCost.Text = string.Format( "<div class='pricelabel cost'><h4>${0}</h4></div>", package.Price );
                    lInstallMessage.Text = string.Format( INSTALL_PURCHASE_MESSAGE, package.Name, package.Price.ToString() );
                }

                if ( package.IsPurchased )
                {
                    // check if it's installed
                    // determine the state of the install button (install, update, buy or installed)
                    InstalledPackage installedPackage = InstalledPackageService.InstalledPackageVersion( package.Id );

                    if ( installedPackage == null )
                    {
                        //lCost.Visible = false;
                        lInstallMessage.Text = INSTALL_PREVIOUS_MESSAGE;
                    }
                    else
                    {
                        //lCost.Visible = false;
                        lInstallMessage.Text = UPDATE_MESSAGE;
                        btnInstall.Text = "Update";
                    }
                }
            }
            else
            {
                var queryParams = new Dictionary<string, string>
                {
                    { "ReturnUrl", Request.RawUrl }
                };

                NavigateToLinkedPage( AttributeKey.LinkOrganizationPage, queryParams );
            }
        }

        /// <summary>
        /// Displays an error to the user if appropriate.
        /// </summary>
        /// <param name="errorResponse">The error response string.</param>
        private void ErrorCheck( string errorResponse )
        {
            if ( errorResponse != string.Empty )
            {
                pnlInstall.Visible = false;
                pnlError.Visible = true;
                lErrorMessage.Text = errorResponse;
            }
        }

        #endregion Private Methods
    }
}