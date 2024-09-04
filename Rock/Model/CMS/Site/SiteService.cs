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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Common.Mobile.Enums;
using Rock.Data;
using Rock.Mobile;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for the <see cref="Rock.Model.Site"/> entity. This inherits from the Service class
    /// </summary>
    public partial class SiteService
    {
        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.Site"/> entities that by their Default <see cref="Rock.Model.Page">Page's</see> PageId.
        /// </summary>
        /// <param name="defaultPageId">An <see cref="System.Int32"/> containing the Id of the default <see cref="Rock.Model.Page"/> to search by. This
        /// value is nullable.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Site"/> entities that use reference the provided PageId.</returns>
        public IQueryable<Site> GetByDefaultPageId( int? defaultPageId )
        {
            return Queryable().Where( t => ( t.DefaultPageId == defaultPageId || ( defaultPageId == null && t.DefaultPageId == null ) ) );
        }

        /// <summary>
        /// Determines whether the specified site can be deleted.
        /// Performs some additional checks that are missing from the
        /// auto-generated SiteService.CanDelete().
        /// TODO This should move into the SiteService CanDelete at some point
        /// once the generator tool is adjusted.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="includeSecondLvl">If set to true, verifies that there are no site layouts with any existing pages.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( Site item, out string errorMessage, bool includeSecondLvl )
        {
            errorMessage = string.Empty;

            bool canDelete = CanDelete( item, out errorMessage );

            if ( canDelete && includeSecondLvl && new Service<Layout>( ( RockContext ) Context ).Queryable().Where( l => l.SiteId == item.Id ).Any( a => a.Pages.Count() > 0 ) )
            {
                errorMessage = string.Format( "This {0} has a {1} which is used by a {2}.", Site.FriendlyTypeName, Layout.FriendlyTypeName, Page.FriendlyTypeName );
                canDelete = false;
            }

            return canDelete;
        }

        /// <summary>
        /// Gets the Guid for the Site that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = SiteCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }

        /// <summary>
        /// Gets the domain URI.
        /// </summary>
        /// <param name="siteId">The site identifier.</param>
        /// <returns></returns>
        public Uri GetDefaultDomainUri( int siteId )
        {
            var site = this.Get( siteId );
            if ( site != null )
            {
                return site.DefaultDomainUri;
            }

            return new Uri( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) );
        }

        #region Mobile Site Deployment

        /// <summary>
        /// Builds the mobile application specified and stores it. Mobile shells will
        /// then retrieve the new bundle the next time they launch.
        /// </summary>
        /// <remarks>
        /// This method will immediately save the changes into the database. A call to
        /// <see cref="DbContext.SaveChanges()"/> is not needed.
        /// </remarks>
        /// <param name="applicationSiteId">The application site identifier representing the application to be built.</param>
        [Obsolete( "Use BuildMobileApplicationAsync() instead." )]
        [RockObsolete( "1.16.4" )]
        public void BuildMobileApplication( int applicationSiteId )
        {
            var task = Task.Run( async () => await BuildMobileApplicationAsync( applicationSiteId ) );

            task.Wait();
        }

        /// <summary>
        /// Builds the mobile application specified and stores it. Mobile shells will
        /// then retrieve the new bundle the next time they launch.
        /// </summary>
        /// <remarks>
        /// This method will immediately save the changes into the database. A call to
        /// <see cref="DbContext.SaveChanges()"/> is not needed.
        /// </remarks>
        /// <param name="applicationSiteId">The application site identifier representing the application to be built.</param>
        public async Task BuildMobileApplicationAsync( int applicationSiteId )
        {
            if ( !( Context is RockContext rockContext ) )
            {
                throw new Exception( "Invalid database context." );
            }

            var deploymentDateTime = RockDateTime.Now;
            var versionId = ( int ) ( deploymentDateTime.ToJavascriptMilliseconds() / 1000 );

            // Generate the packages and then encode to JSON.
            var phonePackage = await MobileHelper.BuildMobilePackageAsync( applicationSiteId, DeviceType.Phone, versionId );
            var tabletPackage = await MobileHelper.BuildMobilePackageAsync( applicationSiteId, DeviceType.Tablet, versionId );
            var phoneJson = phonePackage.ToJson();
            var tabletJson = tabletPackage.ToJson();

            var binaryFileService = new BinaryFileService( rockContext );
            var site = new SiteService( rockContext ).Get( applicationSiteId );
            var binaryFileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.MOBILE_APP_BUNDLE.AsGuid() );
            var additionalSettings = site.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>() ?? new AdditionalSiteSettings();
            var enableCompression = additionalSettings.IsPackageCompressionEnabled;

            // Prepare the phone configuration file.
            var phoneFile = GetMobileApplicationConfigurationFile( binaryFileType.Id, "phone", phoneJson, enableCompression );
            binaryFileService.Add( phoneFile );

            // Prepare the tablet configuration file.
            var tabletFile = GetMobileApplicationConfigurationFile( binaryFileType.Id, "tablet", tabletJson, enableCompression );
            binaryFileService.Add( tabletFile );

            rockContext.SaveChanges();

            // If we blow up after this point, we need to clean up the binary
            // files that we just created since they might have data on cloud
            // storage systems now.
            try
            {
                //
                // Remove old configuration files.
                //
                if ( site.ConfigurationMobilePhoneBinaryFile != null )
                {
                    site.ConfigurationMobilePhoneBinaryFile.IsTemporary = true;
                }

                if ( site.ConfigurationMobileTabletBinaryFile != null )
                {
                    site.ConfigurationMobileTabletBinaryFile.IsTemporary = true;
                }

                //
                // Set new configuration file references.
                //
                site.ConfigurationMobilePhoneBinaryFileId = phoneFile.Id;
                site.ConfigurationMobileTabletBinaryFileId = tabletFile.Id;

                //
                // Update the last deployment date.
                //
                additionalSettings.LastDeploymentDate = deploymentDateTime;
                additionalSettings.LastDeploymentVersionId = versionId;
                additionalSettings.PhoneUpdatePackageUrl = GetMobileApplicationFileUrl( phoneFile );
                additionalSettings.TabletUpdatePackageUrl = GetMobileApplicationFileUrl( tabletFile );
                site.AdditionalSettings = additionalSettings.ToJson();
                site.LatestVersionDateTime = RockDateTime.Now;

                rockContext.SaveChanges();
            }
            catch
            {
                try
                {
                    // Use a new RockContext since our own context is corrupted
                    // by the exception.
                    using ( var deleteRockContext = new RockContext() )
                    {
                        var deleteBinaryFileService = new BinaryFileService( deleteRockContext );

                        var binaryFilesToDelete = deleteBinaryFileService.Queryable()
                            .Where( bf => bf.Id == phoneFile.Id || bf.Id == tabletFile.Id )
                            .ToList();

                        deleteBinaryFileService.DeleteRange( binaryFilesToDelete );

                        deleteRockContext.SaveChanges();
                    }
                }
                catch
                {
                    // Intentionally ignored, we are just trying to clean up.
                }

                // Throw original error.
                throw;
            }
        }

        /// <summary>
        /// Gets the file URL to use for a mobile application configuration file.
        /// </summary>
        /// <param name="file">The file whose URL should be determined.</param>
        /// <returns>A string that represents the URL to use to access the file.</returns>
        private static string GetMobileApplicationFileUrl( BinaryFile file )
        {
            string url = file.Url;

            // FileSystem provider currently returns a bad URL.
            if ( file.BinaryFileType.StorageEntityType.Name == "Rock.Storage.Provider.FileSystem" )
            {
                url = FileUrlHelper.GetFileUrl( file.Id );
                var uri = new Uri( GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" ) );

                url = uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + url;
            }

            return url;
        }

        /// <summary>
        /// Gets the mobile application configuration <see cref="BinaryFile"/> object
        /// that will contain the data in <paramref name="json"/>.
        /// </summary>
        /// <param name="binaryFileTypeId">The binary file type identifier to use when storing the file.</param>
        /// <param name="prefix">The prefix to use with the filename.</param>
        /// <param name="json">The json content for the file.</param>
        /// <param name="enableCompression">If set to <c>true</c> the contents will be compressed.</param>
        /// <returns>A new <see cref="BinaryFile"/> instance that can be added to the database.</returns>
        private static BinaryFile GetMobileApplicationConfigurationFile( int binaryFileTypeId, string prefix, string json, bool enableCompression )
        {
            var mimeType = enableCompression ? "application/gzip" : "application/json";
            var filenameExtension = enableCompression ? "json.gz" : "json";
            Stream jsonStream;

            if ( enableCompression )
            {
                jsonStream = new MemoryStream();
                using ( var gzipStream = new GZipStream( jsonStream, CompressionMode.Compress, true ) )
                {
                    var bytes = Encoding.UTF8.GetBytes( json );
                    gzipStream.Write( bytes, 0, bytes.Length );
                }
                jsonStream.Position = 0;
            }
            else
            {
                jsonStream = new MemoryStream( Encoding.UTF8.GetBytes( json ) );
            }

            return new BinaryFile
            {
                IsTemporary = false,
                BinaryFileTypeId = binaryFileTypeId,
                MimeType = mimeType,
                FileSize = jsonStream.Length,
                FileName = $"{prefix}.{filenameExtension}",
                ContentStream = jsonStream
            };
        }

        #endregion
    }
}
