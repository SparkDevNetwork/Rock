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
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;

namespace Rock.Storage.AssetStorage
{
    /// <summary>
    /// Azure Cloud Platform Storage Provider
    /// </summary>
    /// <seealso cref="AssetStorageComponent" />
    [Description( "Azure Cloud Storage Service" )]
    [Export( typeof( AssetStorageComponent ) )]
    [ExportMetadata( "ComponentName", "Azure Cloud Storage" )]

    [TextField( "Storage Account Name",
        Description = "The name must be unique across all existing storage account names in Azure. It must be 3 to 24 characters long, and can contain only lowercase letters and numbers.",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.StorageAccountName )]

    [EncryptedTextField( "Account Access Key",
        Description = "The access key for the account.",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.AccountAccessKey )]

    [TextField( "Default Container Name",
        Description = "The blob container.",
        IsRequired = true,
        Order = 3,
        Key = AttributeKey.DefaultContainerName )]

    [TextField( "Custom Domain",
        Description = "Optional custom domain.",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.CustomDomain )]

    [TextField( "Root Folder",
        Description = "Optional root folder. Must be the full path to the root folder starting from the first after the container name.",
        IsRequired = false,
        Order = 5,
        Key = AttributeKey.RootFolder )]
    public class AzureCloudStorageComponent : AssetStorageComponent
    {
        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The storage account name
            /// </summary>
            public const string StorageAccountName = "StorageAccountName";

            /// <summary>
            /// The API key
            /// </summary>
            public const string AccountAccessKey = "AccountAccessKey";

            /// <summary>
            /// The default container name
            /// </summary>
            public const string DefaultContainerName = "DefaultContainerName";

            /// <summary>
            /// The custom domain
            /// </summary>
            public const string CustomDomain = "CustomDomain";

            /// <summary>
            /// The Root Folder
            /// </summary>
            public const string RootFolder = CommonAttributeKey.RootFolder;
        }

        #region Fields

        private const string DEFAULT_FILE_NAME = "$$$.$$$";

        #endregion Fields

        #region Properties

        /// <summary>
        /// Specify the icon for the AssetStorageComponent here. It will display in the folder tree.
        /// Default is server.png.
        /// </summary>
        /// <value>
        /// The component icon path.
        /// </value>
        public override string IconCssClass
        {
            get
            {
                return "fa fa-microsoft";
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCloudStorageComponent"/> class.
        /// </summary>
        public AzureCloudStorageComponent() : base()
        {
        }

        #endregion Constructors

        #region Override Methods

        /// <summary>
        /// Lists the objects from the current root folder.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <returns></returns>
        public override List<Asset> ListObjects( AssetStorageProvider assetStorageProvider )
        {
            return ListObjects( assetStorageProvider, new Asset { Type = AssetType.Folder } );
        }

        /// <summary>
        /// Lists the objects. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If key and name are not provided then list all objects from the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder is not used, and Name is not used.
        /// The last segment in Key is treated as a begins with search if it does not end in a '/'. e.g. to get all
        /// files starting with 'mr' in folder 'pictures/cats/' set key = 'pictures/cats/mr' to get 'mr. whiskers'
        /// and 'mrs. whiskers' but not 'fluffy' or 'carnage the attack cat'.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override List<Asset> ListObjects( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKey.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );

            try
            {
                return GetAssetsFromAzureCloud( assetStorageProvider, asset.Key, null );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Lists the files in AssetStorageProvider.RootFolder.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <returns></returns>
        public override List<Asset> ListFilesInFolder( AssetStorageProvider assetStorageProvider )
        {
            return ListFilesInFolder( assetStorageProvider, new Asset { Type = AssetType.Folder } );
        }

        /// <summary>
        /// Lists the files in folder. Asset.Key or Asset.Name is the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key and Name are not provided then list all files in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in the key is the folder name.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override List<Asset> ListFilesInFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKey.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFolder( asset );

            try
            {
                return GetAssetsFromAzureCloud( assetStorageProvider, asset.Key, AssetType.File );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Lists the folders in AssetStorageProvider.Rootfolder.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <returns></returns>
        public override List<Asset> ListFoldersInFolder( AssetStorageProvider assetStorageProvider )
        {
            return ListFoldersInFolder( assetStorageProvider, new Asset { Type = AssetType.Folder } );
        }

        /// <summary>
        /// Lists the folder in folder. Asset.Key or Asset.Name is the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key and Name are not provided the list then list all files in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in the key is the folder name.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override List<Asset> ListFoldersInFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKey.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFolder( asset );

            try
            {
                return GetAssetsFromAzureCloud( assetStorageProvider, asset.Key, AssetType.Folder );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Returns an asset with the stream of the specified file and creates a thumbnail.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override Asset GetObject( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            return GetObject( assetStorageProvider, asset, true );
        }

        /// <summary>
        /// Returns an asset with the stream of the specified file with the option to create a thumbnail.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="createThumbnail">if set to <c>true</c> [create thumbnail].</param>
        /// <returns></returns>
        public override Asset GetObject( AssetStorageProvider assetStorageProvider, Asset asset, bool createThumbnail )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKey.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFile( asset );

            try
            {
                var container = GetCloudBlobContainer( assetStorageProvider );

                // Get a reference to a blob with a request to the server.
                // If the blob does not exist, this call will fail with a 404 (Not Found).
                var blob = container.GetBlobReferenceFromServer( asset.Key ) as CloudBlob;

                var responseAsset = TranslateBlobToRockAsset( assetStorageProvider, blob as CloudBlob, createThumbnail );

                responseAsset.AssetStream = new MemoryStream();
                blob.DownloadToStream( responseAsset.AssetStream );
                responseAsset.AssetStream.Position = 0;

                return responseAsset;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Uploads a file. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If a key is provided it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override bool UploadObject( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKey.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );

            try
            {
                var container = GetCloudBlobContainer( assetStorageProvider );

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference( asset.Key );
                blockBlob.UploadFromStream( asset.AssetStream );
                return true;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Creates a folder. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key is provided it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override bool CreateFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKey.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );

            try
            {
                var container = GetCloudBlobContainer( assetStorageProvider );

                // Retrieve reference to a blob named "myblob".
                CloudBlockBlob blockBlob = container.GetBlockBlobReference( asset.Key + "/" + DEFAULT_FILE_NAME );
                if ( !blockBlob.Exists() )
                {
                    blockBlob.UploadText( string.Empty );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }

            return true;
        }

        /// <summary>
        /// Deletes the asset. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key is provided then it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override bool DeleteAsset( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKey.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );

            try
            {
                var container = GetCloudBlobContainer( assetStorageProvider );

                if ( asset.Type == AssetType.File )
                {
                    // Get a reference to a blob with a request to the server.
                    // If the blob does not exist, this call will fail with a 404 (Not Found).
                    var blob = container.GetBlobReferenceFromServer( asset.Key ) as CloudBlob;
                    return blob.DeleteIfExists( DeleteSnapshotsOption.IncludeSnapshots );
                }
                else
                {

                    BlobResultSegment response;
                    BlobContinuationToken continuationToken = null;
                    // Call ListBlobsSegmentedAsync recursively and enumerate the result segment returned, while the continuation token is non-null.
                    // When the continuation token is null, the last segment has been returned and execution can exit the loop.
                    // Note that blob snapshots cannot be listed in a hierarchical listing operation.
                    do
                    {
                        response = container.ListBlobsSegmented( asset.Key, true, BlobListingDetails.None, null, continuationToken, null, null );

                        if ( response.Results != null )
                        {
                            //show folders then files
                            foreach ( var item in response.Results )
                            {
                                if ( item is CloudBlob )
                                {
                                    var blobItem = item as CloudBlob;
                                    blobItem.DeleteIfExists( DeleteSnapshotsOption.IncludeSnapshots );
                                }
                            }
                        }

                        // Get the continuation token, if there are additional segments of results.
                        continuationToken = response.ContinuationToken;

                    } while ( continuationToken != null );

                    return true;
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Renames the asset.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <param name="newName">The new name.</param>
        /// <returns></returns>
        public override bool RenameAsset( AssetStorageProvider assetStorageProvider, Asset asset, string newName )
        {
            try
            {
                var originalAsset = GetObject( assetStorageProvider, asset );

                if ( originalAsset.Name.Equals( newName, StringComparison.OrdinalIgnoreCase ) )
                {
                    return true;
                }

                var newAsset = originalAsset.Clone();
                newAsset.Name = newName;

                var path = GetPathFromKey( originalAsset.Key );
                newAsset.Key = $"{path}{newName}";

                UploadObject( assetStorageProvider, newAsset );
                DeleteAsset( assetStorageProvider, originalAsset );

                return true;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Creates the download link.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override string CreateDownloadLink( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKey.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFile( asset );

            try
            {
                var container = GetCloudBlobContainer( assetStorageProvider );

                // Get a reference to a blob with a request to the server.
                // If the blob does not exist, this call will fail with a 404 (Not Found).
                var blob = container.GetBlobReferenceFromServer( asset.Key ) as CloudBlob;

                // Create a new access policy and define its constraints.
                // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad-hoc SAS, and 
                // to construct a shared access policy that is saved to the container's shared access policies. 
                SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
                {
                    // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request. 
                    // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours( 24 ),
                    Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create
                };

                // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                var sasBlobToken = blob.GetSharedAccessSignature( adHocSAS );

                // Return the URI string for the container, including the SAS token.
                return blob.Uri + sasBlobToken;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Gets the objects in folder without recursion. i.e. will get the list of files
        /// and folders in the folder but not the contents of the subfolders. Subfolders
        /// will not have the ModifiedDate prop filled in as Amazon doesn't provide it in
        /// this context.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage Provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override List<Asset> ListObjectsInFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, AttributeKey.RootFolder ) );
            asset.Key = FixKey( asset, rootFolder );
            HasRequirementsFolder( asset );

            try
            {
                return GetAssetsFromAzureCloud( assetStorageProvider, asset.Key, null );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Gets the thumbnail image for the provided Asset key. If one does not exist it will be created. If one exists but is older than the file
        /// a new thumbnail is created and the old one overwritten.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="assetKey">The asset key.</param>
        /// <param name="lastModifiedDateTime">The last modified date time.</param>
        /// <returns></returns>
        public override string GetThumbnail( AssetStorageProvider assetStorageProvider, string assetKey, DateTime? lastModifiedDateTime )
        {
            string name = GetNameFromKey( assetKey );
            string path = GetPathFromKey( assetKey );

            string mimeType = System.Web.MimeMapping.GetMimeMapping( name );
            if ( !mimeType.StartsWith( "image/" ) )
            {
                return GetFileTypeIcon( assetKey );
            }

            // check if thumbnail exists
            string thumbDir = $"{ThumbnailRootPath}/{assetStorageProvider.Id}/{path}";
            Directory.CreateDirectory( FileSystemCompontHttpContext.Server.MapPath( thumbDir ) );

            string virtualThumbPath = Path.Combine( thumbDir, name );
            string physicalThumbPath = FileSystemCompontHttpContext.Server.MapPath( virtualThumbPath );

            // Encode the name thumb path since it can contain special characters
            virtualThumbPath = virtualThumbPath.EncodeHtml();

            if ( File.Exists( physicalThumbPath ) )
            {
                var thumbLastModDate = File.GetLastWriteTimeUtc( physicalThumbPath );
                if ( lastModifiedDateTime <= thumbLastModDate )
                {
                    // thumbnail is still good so just return the virtual file path.
                    return virtualThumbPath;
                }
            }

            CreateImageThumbnail( assetStorageProvider, new Asset { Name = name, Key = assetKey, Type = AssetType.File }, physicalThumbPath, false );

            return virtualThumbPath;
        }

        /// <summary>
        /// Deletes the image thumbnail for the provided Asset. If the asset is a file then the singel thumbnail
        /// is deleted. If the asset is a directory then a recurrsive delete is done.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        protected override void DeleteImageThumbnail( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            base.DeleteImageThumbnail( assetStorageProvider, asset );
        }

        #endregion Override Methods

        #region Private Methods

        /// <summary>
        /// Determines whether Asset has the required elements for an Azure file.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <exception cref="Exception">Asset Type is set to 'Folder' instead of 'File.'</exception>
        private void HasRequirementsFile( Asset asset )
        {
            if ( asset.Type == AssetType.Folder )
            {
                throw new Exception( "Asset Type is set to 'Folder' instead of 'File.'" );
            }
        }

        /// <summary>
        /// Determines whether the Asset has the required elements for a Azure folder.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <exception cref="Exception">
        /// Asset Type is set to 'File' instead of 'Folder.'
        /// or
        /// Name and key cannot both be null or empty.
        /// </exception>
        private void HasRequirementsFolder( Asset asset )
        {
            if ( asset.Type == AssetType.File )
            {
                throw new Exception( "Asset Type is set to 'File' instead of 'Folder.'" );
            }
        }

        /// <summary>
        /// Makes adjustments to the Key string based on the root folder, the name, and the AssetType.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="rootFolder">The root folder.</param>
        /// <returns></returns>
        private string FixKey( Asset asset, string rootFolder )
        {
            if ( asset.Key.IsNullOrWhiteSpace() && asset.Name.IsNullOrWhiteSpace() )
            {
                asset.Key = rootFolder;
            }
            else if ( asset.Key.IsNullOrWhiteSpace() && asset.Name.IsNotNullOrWhiteSpace() )
            {
                asset.Key = rootFolder + asset.Name;
            }

            if ( asset.Type == AssetType.Folder && !asset.Key.EndsWith( "/" ) )
            {
                asset.Key += "/";
            }

            if ( asset.Key == "/" )
            {
                asset.Key = "";
            }

            return asset.Key;
        }

        /// <summary>
        /// Returns an cloud blob client obj using the settings in the provided AssetStorageProvider obj.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="createIfNotExisits">if set to <c>true</c> [create if not exists].</param>
        /// <returns></returns>
        private CloudBlobContainer GetCloudBlobContainer( AssetStorageProvider assetStorageProvider, bool createIfNotExisits = false )
        {

            string storageAccountName = GetAttributeValue( assetStorageProvider, AttributeKey.StorageAccountName );
            string accessKey = Encryption.DecryptString( GetAttributeValue( assetStorageProvider, AttributeKey.AccountAccessKey ) );
            string customDomain = GetAttributeValue( assetStorageProvider, AttributeKey.CustomDomain );

            var storageCredentials = new StorageCredentials( storageAccountName, accessKey );
            var storageAccount = new CloudStorageAccount( storageCredentials, true );
            if ( customDomain.IsNotNullOrWhiteSpace() )
            {
                storageAccount = new CloudStorageAccount( storageCredentials, customDomain, true );
            }
            var client = storageAccount.CreateCloudBlobClient();

            string containerName = GetAttributeValue( assetStorageProvider, AttributeKey.DefaultContainerName );
            var container = client.GetContainerReference( containerName );
            if ( createIfNotExisits )
            {
                container.CreateIfNotExists();
            }
            return container;
        }

        /// <summary>
        /// Gets the name from key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetNameFromKey( string key )
        {
            if ( key.LastIndexOf( '/' ) < 1 )
            {
                return key;
            }

            string[] pathSegments = key.Split( '/' );

            if ( key.EndsWith( "/" ) )
            {
                return pathSegments[pathSegments.Length - 2];
            }

            return pathSegments[pathSegments.Length - 1];
        }

        /// <summary>
        /// Gets the path from key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The file prefix used by Google to mimic a folder structure.</returns>
        private string GetPathFromKey( string key )
        {
            var lastSlashIndex = key.LastIndexOf( '/' );

            if ( lastSlashIndex < 1 )
            {
                return string.Empty;
            }

            return key.Substring( 0, lastSlashIndex + 1 );
        }

        /// <summary>
        /// Gets the assets from cloud.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="prefix">The directory.</param>
        /// <param name="assetTypeToList">The asset type to list.</param>
        /// <returns></returns>
        private List<Asset> GetAssetsFromAzureCloud( AssetStorageProvider assetStorageProvider, string prefix, AssetType? assetTypeToList )
        {
            var container = GetCloudBlobContainer( assetStorageProvider, true );

            var assets = new List<Asset>();

            BlobResultSegment response;
            BlobContinuationToken continuationToken = null;
            // Call ListBlobsSegmentedAsync recursively and enumerate the result segment returned, while the continuation token is non-null.
            // When the continuation token is null, the last segment has been returned and execution can exit the loop.
            // Note that blob snapshots cannot be listed in a hierarchical listing operation.
            do
            {
                response = container.ListBlobsSegmented( prefix, false, BlobListingDetails.Metadata, null, continuationToken, null, null );

                if ( response.Results != null )
                {
                    //show folders then files
                    foreach ( var item in response.Results )
                    {
                        if ( item.Uri == null )
                        {
                            continue;
                        }

                        if ( item is CloudBlobDirectory && ( !assetTypeToList.HasValue || assetTypeToList == AssetType.Folder ) )
                        {
                            var directory = item as CloudBlobDirectory;
                            var responseAsset = TranslateDirectoryObjectToRockAsset( assetStorageProvider, directory );
                            // Azure returns the current directory in the list along with its children.
                            // We only want the children.
                            if ( responseAsset.Key != $"{prefix}/" )
                            {
                                assets.Add( responseAsset );
                            }
                        }

                        if ( item is CloudBlob && ( !assetTypeToList.HasValue || assetTypeToList == AssetType.File ) )
                        {
                            var blobItem = item as CloudBlob;
                            if ( !DEFAULT_FILE_NAME.Equals( GetNameFromKey( blobItem.Name ) ) )
                            {
                                var responseAsset = TranslateBlobToRockAsset( assetStorageProvider, blobItem );
                                assets.Add( responseAsset );
                            }
                        }


                    }
                }

                // Get the continuation token, if there are additional segments of results.
                continuationToken = response.ContinuationToken;

            } while ( continuationToken != null );

            return assets.OrderBy( a => a.Key, StringComparer.OrdinalIgnoreCase ).ToList();
        }

        /// <summary>
        /// Creates the asset from blob object.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="blobItem">The blob object.</param>
        /// <param name="createThumbnail">if set to <c>true</c> [create thumbnail].</param>
        /// <returns></returns>
        private Asset TranslateBlobToRockAsset( AssetStorageProvider assetStorageProvider, CloudBlob blobItem, bool createThumbnail = true )
        {
            var asset = new Asset
            {
                Name = GetNameFromKey( blobItem.Name ),
                Key = blobItem.Name,
                Uri = blobItem.Uri.ToString(),
                Type = AssetType.File,
                AssetStorageProviderId = assetStorageProvider.Id
            };

            if ( blobItem.Properties != null )
            {
                asset.FileSize = blobItem.Properties.Length;
                asset.Description = $"{blobItem.Properties.Length} byte{( blobItem.Properties.Length == 1 ? string.Empty : "s" )}";
                if ( blobItem.Properties.LastModified != null )
                {
                    asset.LastModifiedDateTime = RockDateTime.ConvertLocalDateTimeToRockDateTime( blobItem.Properties.LastModified.Value.LocalDateTime );
                }
            }

            if ( createThumbnail )
            {
                asset.IconPath = createThumbnail ?
                        GetThumbnail( assetStorageProvider, blobItem.Name, asset.LastModifiedDateTime ) :
                        GetFileTypeIcon( blobItem.Name );
            }
            return asset;
        }

        /// <summary>
        /// Creates the asset from blob object.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="directory">The directory object.</param>
        /// <returns></returns>
        private Asset TranslateDirectoryObjectToRockAsset( AssetStorageProvider assetStorageProvider, CloudBlobDirectory directory )
        {
            var name = GetNameFromKey( directory.Prefix );
            return new Asset
            {
                Name = name,
                Key = directory.Prefix,
                Uri = directory.Uri.ToString(),
                Type = AssetType.Folder,
                AssetStorageProviderId = assetStorageProvider.Id
            };
        }

        #endregion Private Methods

    }
}
