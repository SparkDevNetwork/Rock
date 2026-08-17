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

using Microsoft.Web.XmlTransform;

using Rock;
using Rock.Attribute;
using Rock.Configuration;
using Rock.Data;
using Rock.Store;
using Rock.Utility;
using Rock.ViewModels.Blocks.Store.PackageInstall;
using Rock.Web.Cache;

namespace Rock.Blocks.Store
{
    /// <summary>
    /// Installs a package that has been downloaded in the Rock Shop.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Package Install" )]
    [Category( "Store" )]
    [Description( "Installs a package." )]
    [IconCssClass( "ti ti-gift" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Link Organization Page",
        Key = AttributeKey.LinkOrganizationPage,
        Description = "Page to allow the user to link an organization to the store.",
        IsRequired = false,
        Order = 0 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "1078292D-78F0-48C2-93C5-594B2BFA7C10" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "00B39CC5-CEFA-4A2C-AF09-D8DC35275C5B" )]
    [Rock.SystemGuid.BlockTypeGuid( "EA60C1AB-ADAB-4EDF-94F8-B0FE214B6F15" )]
    public class PackageInstall : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string LinkOrganizationPage = "LinkOrganizationPage";
        }

        private static class PageParameterKey
        {
            public const string PackageId = "PackageId";
        }

        #endregion Keys

        #region Constants

        /// <summary>
        /// The login message shown when a prior version of the package is
        /// already installed.
        /// </summary>
        private const string UpdateMessage = "Log in below with your Rock Store account to upgrade this package.";

        /// <summary>
        /// The login message shown when the package was previously purchased
        /// but is not currently installed.
        /// </summary>
        private const string InstallPreviousMessage = "Log in below with your Rock Store account to install this previously purchased package.";

        /// <summary>
        /// The extension used by XML data transform files inside a package's
        /// content folder.
        /// </summary>
        private const string XdtExtension = ".rock.xdt";

        /// <summary>
        /// Files that must never be installed into or deleted from the Bin
        /// directory. These are Rock Core dependencies that a plugin replacing
        /// or removing them could break.
        /// </summary>
        private static readonly string[] BinDirectoryBlacklist = new[]
        {
            "Azure.AI.OpenAI.dll",
            "Azure.Core.dll",
            "Google.Protobuf.dll",
            "Microsoft.ML.Tokenizers.Data.O200kBase.dll",
            "Microsoft.ML.Tokenizers.dll",
            "Microsoft.Bcl.AsyncInterfaces.dll",
            "Microsoft.Bcl.HashCode.dll",
            "Microsoft.Bcl.Memory.dll",
            "Microsoft.Bcl.Numerics.dll",
            "Microsoft.Extensions.AI.dll",
            "Microsoft.Extensions.AI.Abstractions.dll",
            "Microsoft.Extensions.AI.OpenAI.dll",
            "Microsoft.Extensions.Caching.Memory.dll",
            "Microsoft.Extensions.Caching.Abstractions.dll",
            "Microsoft.Extensions.DependencyInjection.dll",
            "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
            "Microsoft.Extensions.Logging.Abstractions.dll",
            "Microsoft.Extensions.Options.dll",
            "Microsoft.Extensions.Primitives.dll",
            "Microsoft.Extensions.VectorData.Abstractions.dll",
            "Microsoft.Recognizers.Definitions.dll",
            "Microsoft.Recognizers.Text.dll",
            "Microsoft.Recognizers.Text.DateTime.dll",
            "Microsoft.Recognizers.Text.DataTypes.TimexExpression.dll",
            "Microsoft.Recognizers.Text.Number.dll",
            "Microsoft.Recognizers.Text.NumberWithUnit.dll",
            "Microsoft.SemanticKernel.dll",
            "Microsoft.SemanticKernel.Abstractions.dll",
            "Microsoft.SemanticKernel.Connectors.AzureOpenAI.dll",
            "Microsoft.SemanticKernel.Connectors.OpenAI.dll",
            "Microsoft.SemanticKernel.Core.dll",
            "OpenAI.dll",
            "System.Buffers.dll",
            "System.ClientModel.dll",
            "System.Collections.Immutable.dll",
            "System.Diagnostics.DiagnosticSource.dll",
            "System.IO.Pipelines.dll",
            "System.Memory.dll",
            "System.Memory.Data.dll",
            "System.Numerics.Tensors.dll",
            "System.Numerics.Vectors.dll",
            "System.Runtime.CompilerServices.Unsafe.dll",
            "System.Text.Json.dll",
            "System.Text.Encodings.Web.dll",
            "System.Threading.Channels.dll",
            "System.Threading.Tasks.Extensions.dll",
            "System.ValueTuple.dll",
        };

        #endregion Constants

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetInitializationBox();
        }

        /// <summary>
        /// Builds the initialization box describing the package and the current
        /// store/install state for the component to render.
        /// </summary>
        /// <returns>The initialization box.</returns>
        private PackageInstallInitializationBox GetInitializationBox()
        {
            // When the store has not been linked to an organization, the
            // component redirects to the Link Organization page, so no package
            // data needs to be loaded.
            if ( !StoreService.OrganizationIsConfigured() )
            {
                return new PackageInstallInitializationBox
                {
                    IsStoreConfigured = false,
                    LinkOrganizationPageUrl = GetLinkOrganizationPageUrl()
                };
            }

            var box = new PackageInstallInitializationBox
            {
                IsStoreConfigured = true
            };

            var packageId = PageParameter( PageParameterKey.PackageId ).AsIntegerOrNull() ?? -1;

            var package = new PackageService().GetPackage( packageId, out var errorResponse );

            // Surface a store error or a missing package as the unavailable
            // notice instead of the install form.
            if ( errorResponse.IsNotNullOrWhiteSpace() )
            {
                box.ErrorMessage = errorResponse;
                return box;
            }

            if ( package == null )
            {
                box.ErrorMessage = "The requested package could not be found.";
                return box;
            }

            box.PackageName = package.Name;
            box.PackageIconImageUrl = package.PackageIconBinaryFile?.ImageUrl;
            box.InstallButtonText = "Install";
            box.InstallMessage = package.IsFree
                ? GetInstallFreeMessage( package.Name )
                : GetInstallPurchaseMessage( package.Name, package.Price );

            if ( package.IsPurchased )
            {
                var installedPackage = InstalledPackageService.InstalledPackageVersion( package.Id );

                if ( installedPackage == null )
                {
                    // Previously purchased but not currently installed.
                    box.InstallMessage = InstallPreviousMessage;
                }
                else if ( IsInstalledVersionCurrent( package, installedPackage.VersionId ) )
                {
                    // The installed version is already the latest version that can
                    // be installed on this version of Rock; there is nothing to do.
                    box.IsUpToDate = true;
                }
                else
                {
                    // A newer compatible version exists; the action upgrades to it.
                    box.InstallMessage = UpdateMessage;
                    box.InstallButtonText = "Update";
                }
            }

            return box;
        }

        /// <summary>
        /// Resolves the Link Organization page URL, carrying a ReturnUrl back to
        /// this page so the user returns here after configuring the store.
        /// </summary>
        /// <returns>The Link Organization page URL.</returns>
        private string GetLinkOrganizationPageUrl()
        {
            var queryParams = new Dictionary<string, string>
            {
                { "ReturnUrl", RequestContext.RequestUri?.PathAndQuery }
            };

            return this.GetLinkedPageUrl( AttributeKey.LinkOrganizationPage, queryParams );
        }

        /// <summary>
        /// Gets the version id of the currently installed package.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <returns>The installed version id, or -1 if the package is not installed.</returns>
        private int GetInstalledPackageVersionId( int packageId )
        {
            return InstalledPackageService.InstalledPackageVersion( packageId )?.VersionId ?? -1;
        }

        /// <summary>
        /// Determines whether the installed version is already the latest version
        /// of the package that can be installed on this version of Rock. Mirrors
        /// the compatible-version selection used by the Package Detail block and
        /// the version filter applied by the install pipeline.
        /// </summary>
        /// <param name="package">The package whose available versions are inspected.</param>
        /// <param name="installedVersionId">The currently installed version id.</param>
        /// <returns><c>true</c> if no newer compatible version is available; otherwise <c>false</c>.</returns>
        private static bool IsInstalledVersionCurrent( Package package, int installedVersionId )
        {
            var rockVersion = RockSemanticVersion.Parse( Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber() );

            var latestCompatibleVersion = ( package.Versions ?? new List<PackageVersion>() )
                .Where( v => v.RequiredRockSemanticVersion <= rockVersion )
                .OrderByDescending( v => v.Id )
                .FirstOrDefault();

            // No compatible version means nothing can be installed on this Rock
            // version, so an already-installed package is treated as current.
            if ( latestCompatibleVersion == null )
            {
                return true;
            }

            return installedVersionId >= latestCompatibleVersion.Id;
        }

        /// <summary>
        /// Builds the login message shown for a paid package.
        /// </summary>
        /// <param name="name">The package name.</param>
        /// <param name="price">The package price.</param>
        /// <returns>The HTML message.</returns>
        private static string GetInstallPurchaseMessage( string name, decimal? price ) =>
            $"Log in below with your Rock Store account to install the <em>{name}</em> package. Your credit card on file will be charged ${price}.";

        /// <summary>
        /// Builds the login message shown for a free package.
        /// </summary>
        /// <param name="name">The package name.</param>
        /// <returns>The HTML message.</returns>
        private static string GetInstallFreeMessage( string name ) =>
            $"Log in below with your Rock Store account to install free <em>{name}</em> package.";

        private string GetMessage( PurchaseResult purchaseResult, string detail )
        {
            var accountUrl = RequestContext.ResolveRockUrl( "~/RockShop/Account" );

            return purchaseResult switch
            {
                PurchaseResult.AuthenticationFailed => $"<strong>Could Not Authenticate</strong> {detail} If you need further help see the <a href='https://rockrms.com/RockShopHelp'>Rock Shop Help Page</a>.",
                PurchaseResult.NoCardOnFile => $"<strong>No Card On File</strong> No credit card is on file for your organization. Please add a card from your <a href='{accountUrl}'>Account Page</a> or see the <a href='https://rockrms.com/RockShopHelp'>Rock Shop Help Page</a>.",
                PurchaseResult.NotAuthorized => "<strong>Unauthorized</strong> You are not currently authorized to make purchases for this organization. Please see your organization's primary contact to enable your account for purchases or see the <a href='https://rockrms.com/RockShopHelp'>Rock Shop Help Page</a>.",
                PurchaseResult.PaymentFailed => $"<strong>Payment Error</strong> An error occurred while processing the credit card on file for your organization. The error was: {detail}. Please update your card's information from your <a href='{accountUrl}'>Account Page</a> or see the <a href='https://rockrms.com/RockShopHelp'>Rock Shop Help Page</a>.",
                _ => $"<strong>An Error Occurred</strong> {detail}"
            };
        }

        #endregion Methods

            #region Install Pipeline

            /// <summary>
            /// Runs the install steps from the purchase response that are newer than
            /// the installed version and compatible with this version of Rock.
            /// </summary>
            /// <param name="purchaseResponse">The purchase response from the Rock Shop.</param>
            /// <returns>The result of the install attempt.</returns>
        private InstallPackageResponseBag ProcessInstall( PurchaseResponse purchaseResponse )
        {
            if ( purchaseResponse.PackageInstallSteps == null )
            {
                throw new PackageInstallException( "Install package was not valid. Please try again later." );
            }

            var currentlyInstalledVersionId = GetInstalledPackageVersionId( purchaseResponse.PackageId );
            var rockVersion = RockSemanticVersion.Parse( Rock.VersionInfo.VersionInfo.GetRockSemanticVersionNumber() );

            // Only the steps newer than the installed version that also target
            // this version of Rock should be applied.
            var packageInstallSteps = purchaseResponse.PackageInstallSteps
                .Where( s => s.RequiredRockSemanticVersion <= rockVersion )
                .Where( s => s.VersionId > currentlyInstalledVersionId )
                .ToList();

            var appRoot = RockApp.Current.HostingSettings.WebRootPath;

            var rockShopWorkingDir = $"{appRoot}App_Data/RockShop";
            EnsureDirectoryExists( rockShopWorkingDir );

            // The result of the last applied step carries the post-install
            // instructions shown to the user. A failing step throws and aborts
            // the remaining steps.
            var result = new InstallPackageResponseBag();

            foreach ( var installStep in packageInstallSteps )
            {
                result = InstallPackageStep( installStep, purchaseResponse, appRoot, rockShopWorkingDir );
            }

            return result;
        }

        /// <summary>
        /// Downloads and applies a single package install step (content files,
        /// XDT transforms, SQL, and file deletions).
        /// </summary>
        /// <param name="installStep">The install step to process.</param>
        /// <param name="purchaseResponse">The purchase response from the Rock Shop.</param>
        /// <param name="appRoot">The application root directory path.</param>
        /// <param name="rockShopWorkingDir">The Rock Shop working directory path.</param>
        /// <returns>The result carrying the step's post-install instructions.</returns>
        private InstallPackageResponseBag InstallPackageStep( PackageInstallStep installStep, PurchaseResponse purchaseResponse, string appRoot, string rockShopWorkingDir )
        {
            var wasActionTaken = false;

            var packageDirectory = $"{rockShopWorkingDir}/{purchaseResponse.PackageId} - {purchaseResponse.PackageName}";
            EnsureDirectoryExists( packageDirectory );

            var destinationFile = $"{packageDirectory}/{installStep.VersionId} - {installStep.VersionLabel}.plugin";

            // Download the package file.
            try
            {
                using ( var client = new WebClient() )
                {
                    client.DownloadFile( installStep.InstallPackageUrl, destinationFile );
                }
            }
            catch ( Exception ex )
            {
                CleanUpPackage( destinationFile );
                throw new PackageInstallException( $"An error occurred while downloading package from the store. Please try again later. <br><em>Error: {ex.Message}</em>" );
            }

            // Extract content files and apply SQL and file deletions.
            try
            {
                using ( var packageZip = ZipFile.OpenRead( destinationFile ) )
                {
                    foreach ( var entry in packageZip.Entries )
                    {
                        wasActionTaken = ProcessZipEntry( entry, appRoot ) || wasActionTaken;
                    }

                    wasActionTaken = ProcessRunSQL( packageZip ) || wasActionTaken;
                    wasActionTaken = ProcessDeleteFileList( packageZip, appRoot ) || wasActionTaken;
                }
            }
            catch ( PackageInstallException )
            {
                // A processor reported a specific failure; surface it as-is.
                throw;
            }
            catch ( Exception ex )
            {
                throw new PackageInstallException( $"An error occurred while extracting the contents of the package. <br><em>Error: {ex.Message}</em>" );
            }

            if ( !wasActionTaken )
            {
                throw new PackageInstallException( $"Package version {installStep.VersionLabel} failed to install because no actions were completed. This may be due to an improperly packaged plugin file. Please contact the package administrator for support." );
            }

            // Record the install and clear caches so the new content is picked up.
            InstalledPackageService.SaveInstall( purchaseResponse.PackageId, purchaseResponse.PackageName, installStep.VersionId, installStep.VersionLabel, purchaseResponse.VendorId, purchaseResponse.VendorName, purchaseResponse.InstalledBy );

            RockCache.ClearAllCachedItems();

            return new InstallPackageResponseBag
            {
                PostInstallInstructions = installStep.PostInstallInstructions
            };
        }

        /// <summary>
        /// Extracts a single zip entry from the package's content folder,
        /// applying any XDT transform or honoring the bin blacklist.
        /// </summary>
        /// <param name="entry">The zip entry to process.</param>
        /// <param name="appRoot">The application root directory path.</param>
        /// <returns><c>true</c> if some action was taken; otherwise <c>false</c>.</returns>
        private bool ProcessZipEntry( ZipArchiveEntry entry, string appRoot )
        {
            // Directory entries have no content; skip them.
            if ( entry.Length == 0 )
            {
                return false;
            }

            // Normalize separators in case the archive was encoded with
            // backslashes, then only process the content folder.
            var fullName = entry.FullName.Replace( "\\", "/" );
            if ( !fullName.StartsWith( "content/", StringComparison.OrdinalIgnoreCase ) )
            {
                return false;
            }

            var relativeName = fullName.ReplaceFirstOccurrence( "content/", string.Empty );

            if ( fullName.EndsWith( XdtExtension, StringComparison.OrdinalIgnoreCase ) )
            {
                // Apply the XML data transform to its target file.
                var transformTargetFile = appRoot + relativeName.Substring( 0, relativeName.LastIndexOf( XdtExtension ) );

                using ( var document = new XmlTransformableDocument() )
                {
                    document.PreserveWhitespace = true;
                    document.Load( transformTargetFile );

                    using ( var transform = new XmlTransformation( entry.Open(), null ) )
                    {
                        if ( transform.Apply( document ) )
                        {
                            document.Save( transformTargetFile );
                            return true;
                        }
                    }
                }

                return false;
            }

            // A blacklisted Bin file must not be installed.
            if ( IsBlacklisted( relativeName ) )
            {
                return false;
            }

            var fullPath = Path.Combine( appRoot, relativeName );
            EnsureDirectoryExists( Path.GetDirectoryName( fullPath ) );
            entry.ExtractToFile( fullPath, true );

            // Update the file's last write time so other parts of Rock detect
            // the change. Intentionally using DateTime.Now for local system time.
            File.SetLastWriteTime( fullPath, DateTime.Now );

            return true;
        }

        /// <summary>
        /// Runs the package's install/run.sql script if present.
        /// </summary>
        /// <param name="packageZip">The package archive.</param>
        /// <returns><c>true</c> if a script was run; otherwise <c>false</c>.</returns>
        private bool ProcessRunSQL( ZipArchive packageZip )
        {
            // Look for either separator in case the archive was encoded incorrectly.
            var sqlInstallEntry = packageZip.Entries.FirstOrDefault( e => e.FullName == "install/run.sql" || e.FullName == "install\\run.sql" );
            if ( sqlInstallEntry == null )
            {
                return false;
            }

            var sqlScript = Encoding.Default.GetString( sqlInstallEntry.Open().ReadBytesToEnd() );
            if ( sqlScript.IsNullOrWhiteSpace() )
            {
                return false;
            }

            try
            {
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.ExecuteSqlCommand( sqlScript );
                }
            }
            catch ( Exception ex )
            {
                throw new PackageInstallException( $"An error occurred while updating the database. <br><em>Error: {ex.Message}</em>" );
            }

            return true;
        }

        /// <summary>
        /// Processes the package's install/deletefile.lst, removing the listed
        /// files and folders that are not blacklisted.
        /// </summary>
        /// <param name="packageZip">The package archive.</param>
        /// <param name="appRoot">The application root directory path.</param>
        /// <returns><c>true</c> if some action was taken; otherwise <c>false</c>.</returns>
        private bool ProcessDeleteFileList( ZipArchive packageZip, string appRoot )
        {
            // Look for either separator in case the archive was encoded incorrectly.
            var deleteListEntry = packageZip.Entries.FirstOrDefault( e => e.FullName == "install/deletefile.lst" || e.FullName == "install\\deletefile.lst" );
            if ( deleteListEntry == null )
            {
                return false;
            }

            var wasActionTaken = false;

            try
            {
                var deleteList = Encoding.Default.GetString( deleteListEntry.Open().ReadBytesToEnd() );
                var itemsToDelete = deleteList.Split( new string[] { Environment.NewLine }, StringSplitOptions.None );

                foreach ( var deleteItem in itemsToDelete )
                {
                    if ( deleteItem.IsNullOrWhiteSpace() )
                    {
                        continue;
                    }

                    // Never delete a blacklisted Bin file.
                    if ( IsBlacklisted( deleteItem ) )
                    {
                        continue;
                    }

                    // Treat the line as handled even if the file is already gone
                    // (it may have been removed manually) so the step is not
                    // flagged as having taken no action.
                    wasActionTaken = true;

                    var deleteItemFullPath = appRoot + deleteItem;

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
                throw new PackageInstallException( $"An error occurred while modifying files. <br><em>Error: {ex.Message}</em>" );
            }

            return wasActionTaken;
        }

        /// <summary>
        /// Removes a downloaded package file after a failed install.
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
            catch
            {
                // the caller is already reporting the original download failure.
            }
        }

        /// <summary>
        /// Creates a directory if it does not already exist.
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
        /// Determines whether a file from the archive is blacklisted and should
        /// neither be installed into nor deleted from the Bin directory.
        /// </summary>
        /// <param name="fullName">The full name from the archive, such as 'bin/some.dll'.</param>
        /// <returns><c>true</c> if the file is blacklisted; otherwise <c>false</c>.</returns>
        private static bool IsBlacklisted( string fullName )
        {
            if ( !fullName.StartsWith( "bin/", StringComparison.OrdinalIgnoreCase ) )
            {
                return false;
            }

            var filename = Path.GetFileName( fullName );

            return BinDirectoryBlacklist.Contains( filename, StringComparer.OrdinalIgnoreCase );
        }

        #endregion Install Pipeline

        #region Block Actions

        /// <summary>
        /// Authenticates with the store, purchases the package if needed, and
        /// installs it.
        /// </summary>
        /// <param name="bag">The store credentials.</param>
        /// <returns>The result of the purchase and install attempt.</returns>
        [BlockAction]
        public BlockActionResult Install( InstallPackageRequestBag bag )
        {
            var packageId = PageParameter( PageParameterKey.PackageId ).AsIntegerOrNull() ?? -1;

            var installResponse = new StoreService().Purchase( bag.Username, bag.Password, packageId, out var errorResponse );

            // A null response means the store could not be reached or the
            // request itself failed before a purchase result was determined.
            if ( installResponse == null )
            {
                var detail = errorResponse.IsNotNullOrWhiteSpace() ? errorResponse : "Unknown";

                return ActionBadRequest( $"<strong>Install Error</strong> An error occurred while attempting to authenticate your install of this package. The error was: {detail}." );
            }

            if ( installResponse.PurchaseResult != PurchaseResult.Success )
            {
                return ActionBadRequest( GetMessage( installResponse.PurchaseResult, installResponse.Message ) );
            }

            try
            {
                return ActionOk( ProcessInstall( installResponse ) );
            }
            catch ( PackageInstallException ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        #endregion Block Actions

        #region Support Classes

        /// <summary>
        /// Raised when a package install step cannot complete. The message is
        /// the user-facing explanation surfaced beneath the install form.
        /// </summary>
        private class PackageInstallException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PackageInstallException"/> class.
            /// </summary>
            /// <param name="message">The user-facing explanation of the failure.</param>
            public PackageInstallException( string message ) : base( message )
            {
            }
        }

        #endregion Support Classes
    }
}
