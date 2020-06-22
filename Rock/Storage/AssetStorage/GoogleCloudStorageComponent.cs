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
using System.Net;
using Google.Apis.Auth.OAuth2;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Storage.AssetStorage.ApiClient;

namespace Rock.Storage.AssetStorage
{
    /// <summary>
    /// Google Cloud Platform Storage Provider
    /// </summary>
    /// <seealso cref="AssetStorageComponent" />
    [Description( "Google Cloud Storage Service" )]
    [Export( typeof( AssetStorageComponent ) )]
    [ExportMetadata( "ComponentName", "Google Cloud Storage" )]

    [TextField( "Bucket Name",
        Description = "The text name of your Google Cloud Storage bucket within the project. See https://console.cloud.google.com/storage/browser",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.BucketName )]

    [EncryptedTextField( "Service Account JSON Key",
        Description = "The Service Account key JSON file contents that is used to access Google Cloud Storage. See https://console.cloud.google.com/iam-admin/serviceaccounts",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.ServiceAccountKey )]

    [TextField( "Root Folder",
        Description = "Optional root folder. Must be the full path to the root folder starting from the first after the bucket name. This must end with a '/'.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.RootFolder )]

    public class GoogleCloudStorageComponent : AssetStorageComponent
    {
        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The bucket name
            /// </summary>
            public const string BucketName = "BucketName";

            /// <summary>
            /// The API key
            /// </summary>
            public const string ServiceAccountKey = "ApiKey";

            /// <summary>
            /// The root folder
            /// </summary>
            public const string RootFolder = CommonAttributeKey.RootFolder;
        }

        #region Properties

        /// <summary>
        /// Specify the icon for the AssetStorageComponent here. It will display in the folder tree.
        /// Default is server.png.
        /// </summary>
        /// <value>
        /// The component icon path.
        /// </value>
        public override string IconCssClass => "fa fa-google";

        #endregion Properties

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
            var rootFolder = GetRootFolder( assetStorageProvider );
            FixKey( asset, rootFolder );
            return GetAssetsFromGoogle( assetStorageProvider, asset.Key, null, false );
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
            var rootFolder = GetRootFolder( assetStorageProvider );
            FixKey( asset, rootFolder );
            return GetAssetsFromGoogle( assetStorageProvider, asset.Key, AssetType.File, false );
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
            var rootFolder = GetRootFolder( assetStorageProvider );
            FixKey( asset, rootFolder );
            return GetAssetsFromGoogle( assetStorageProvider, asset.Key, AssetType.Folder, false );
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
            using ( var client = GetStorageClient( assetStorageProvider ) )
            {
                var bucketName = GetBucketName( assetStorageProvider );
                var response = client.GetObject( bucketName, asset.Key );
                var responseAsset = TranslateGoogleObjectToRockAsset( assetStorageProvider, response, createThumbnail );

                responseAsset.AssetStream = new MemoryStream();
                client.DownloadObject( response, responseAsset.AssetStream );
                responseAsset.AssetStream.Position = 0;

                return responseAsset;
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
            var bucketName = GetBucketName( assetStorageProvider );
            var googleObject = TranslateRockAssetToGoogleObject( asset, bucketName );

            using ( var client = GetStorageClient( assetStorageProvider ) )
            {
                client.UploadObject( googleObject, asset.AssetStream );
                return true;
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
            // https://stackoverflow.com/a/38417397
            // Google storage doesn't actually have folders, so we just upload an empty object ending with
            // a '/' to create the illusion of a directory.
            asset.AssetStream = new MemoryStream();
            UploadObject( assetStorageProvider, asset );
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
            var bucketName = GetBucketName( assetStorageProvider );
            var objectsToDelete = new List<Google.Apis.Storage.v1.Data.Object>();

            using ( var client = GetStorageClient( assetStorageProvider ) )
            {
                if ( asset.Type == AssetType.File )
                {
                    // Get the whole object so that the Generation property is set and the object is permanently deleted
                    objectsToDelete.Add( client.GetObject( bucketName, asset.Key ) );
                }
                else
                {
                    // To delete a folder from Google, just delete everything inside
                    objectsToDelete.AddRange( client.ListObjects( bucketName, asset.Key, string.Empty ) );
                }

                foreach ( var objectToDelete in objectsToDelete )
                {
                    client.DeleteObject( objectToDelete );
                }

                return true;
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
            var originalAsset = GetObject( assetStorageProvider, asset );

            var newAsset = originalAsset.Clone();
            newAsset.Name = newName;

            var path = GetPathFromKey( originalAsset.Key );
            newAsset.Key = $"{path}{newName}";

            UploadObject( assetStorageProvider, newAsset );
            DeleteAsset( assetStorageProvider, originalAsset );

            return true;
        }

        /// <summary>
        /// Creates the download link.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override string CreateDownloadLink( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            using ( var client = GetStorageClient( assetStorageProvider ) )
            {
                var bucketName = GetBucketName( assetStorageProvider );
                var response = client.GetObject( bucketName, asset.Key );
                return response.MediaLink;
            }
        }

        /// <summary>
        /// Lists the objects in folder. The asset key or name should be the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name
        /// If Key and Name are not provided then list all objects in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in key is the folder name.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override List<Asset> ListObjectsInFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            var rootFolder = GetRootFolder( assetStorageProvider );
            FixKey( asset, rootFolder );
            return GetAssetsFromGoogle( assetStorageProvider, asset.Key, null, false );
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
            var name = GetNameFromKey( assetKey );
            var path = GetPathFromKey( assetKey );
            var mimeType = System.Web.MimeMapping.GetMimeMapping( name );

            if ( !mimeType.StartsWith( "image/" ) )
            {
                return GetFileTypeIcon( assetKey );
            }

            // check if thumbnail exists
            var thumbDir = $"{ThumbnailRootPath}/{assetStorageProvider.Id}/{path}";
            Directory.CreateDirectory( FileSystemCompontHttpContext.Server.MapPath( thumbDir ) );

            var virtualThumbPath = Path.Combine( thumbDir, name );
            var physicalThumbPath = FileSystemCompontHttpContext.Server.MapPath( virtualThumbPath );

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
        /// Deletes the image thumbnail for the provided Asset. If the asset is a file then the single thumbnail
        /// is deleted. If the asset is a directory then a recursive delete is done.
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
        /// Translates the rock asset to a Google object.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <returns></returns>
        private Google.Apis.Storage.v1.Data.Object TranslateRockAssetToGoogleObject( Asset asset, string bucketName )
        {
            var name = asset.Key;

            if ( asset.Type == AssetType.Folder )
            {
                name = FixRootFolder( name );

                if ( name.IsNullOrWhiteSpace() && !asset.Name.IsNullOrWhiteSpace() )
                {
                    name = FixRootFolder( asset.Name );
                }
            }

            return new Google.Apis.Storage.v1.Data.Object
            {
                Name = name,
                Bucket = bucketName,
                Size = Convert.ToUInt64( asset.FileSize ),
                Updated = asset.LastModifiedDateTime,
                ContentType = System.Web.MimeMapping.GetMimeMapping( name )
            };
        }

        /// <summary>
        /// Creates the asset from Google object.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="googleObject">The Google object.</param>
        /// <param name="createThumbnail">if set to <c>true</c> [create thumbnail].</param>
        /// <returns></returns>
        private Asset TranslateGoogleObjectToRockAsset( AssetStorageProvider assetStorageProvider, Google.Apis.Storage.v1.Data.Object googleObject, bool createThumbnail = true )
        {
            var isFolder = googleObject.Name.EndsWith( "/" );
            var name = GetNameFromKey( googleObject.Name );

            return new Asset
            {
                Name = name,
                Key = googleObject.Name,
                Uri = googleObject.MediaLink,
                Type = isFolder ? AssetType.Folder : AssetType.File,
                AssetStorageProviderId = assetStorageProvider.Id,
                FileSize = Convert.ToInt64( googleObject.Size ?? 0ul ),
                LastModifiedDateTime = googleObject.Updated,
                Description = $"{googleObject.ContentType} {googleObject.Size} byte{( googleObject.Size == 1 ? string.Empty : "s" )}",
                IconPath = createThumbnail ?
                    GetThumbnail( assetStorageProvider, googleObject.Name, googleObject.Updated ) :
                    GetFileTypeIcon( googleObject.Name )
            };
        }

        /// <summary>
        /// Returns a Google Storage Client using the attribute value settings.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <returns></returns>
        private GoogleClient GetStorageClient( AssetStorageProvider assetStorageProvider )
        {
            var accountKeyJson = GetServiceAccountKeyJson( assetStorageProvider );
            var storageClient = new GoogleClient( accountKeyJson );
            return storageClient;
        }

        /// <summary>
        /// Gets the name of the bucket.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The Google bucket name setting is not valid</exception>
        private string GetBucketName( AssetStorageProvider assetStorageProvider )
        {
            if ( _bucketName == null )
            {
                _bucketName = GetAttributeValue( assetStorageProvider, AttributeKey.BucketName );

                if ( _bucketName.IsNullOrWhiteSpace() )
                {
                    throw new ArgumentException( "The Google bucket name setting is not valid", AttributeKey.BucketName );
                }
            }

            return _bucketName;
        }
        private string _bucketName = null;

        /// <summary>
        /// Gets the service account key JSON.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">The Google Service Account Key JSON setting is not valid</exception>
        private string GetServiceAccountKeyJson( AssetStorageProvider assetStorageProvider )
        {
            if ( _serviceAccountKeyJson == null )
            {
                var encryptedJson = GetAttributeValue( assetStorageProvider, AttributeKey.ServiceAccountKey );
                _serviceAccountKeyJson = Encryption.DecryptString( encryptedJson );

                if ( _serviceAccountKeyJson.IsNullOrWhiteSpace() )
                {
                    throw new ArgumentException( "The Google Service Account Key JSON setting is not valid", AttributeKey.ServiceAccountKey );
                }
            }

            return _serviceAccountKeyJson;
        }
        private string _serviceAccountKeyJson = null;

        /// <summary>
        /// Makes adjustments to the Key string based on the root folder, the name, and the AssetType.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="rootFolder">The root folder.</param>
        /// <returns></returns>
        private void FixKey( Asset asset, string rootFolder )
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
        }

        /// <summary>
        /// Gets the assets from Google.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="directory">The directory.</param>
        /// <param name="assetTypeToList">The asset type to list.</param>
        /// <param name="allowRecursion">if set to <c>true</c> [allow recursion].</param>
        /// <returns></returns>
        private List<Asset> GetAssetsFromGoogle( AssetStorageProvider assetStorageProvider, string directory, AssetType? assetTypeToList, bool allowRecursion )
        {
            using ( var client = GetStorageClient( assetStorageProvider ) )
            {
                var bucketName = GetBucketName( assetStorageProvider );

                // The initial depth is within the "directory", which means it's the depth of the "directory" plus 1
                var initialDepth = GetKeyDepth( directory ) + 1;

                // Get the objects from Google and transform them into assets
                var objects = client.ListObjects( bucketName, directory, assetTypeToList == AssetType.File ? "/" : string.Empty );

                if ( assetTypeToList == AssetType.Folder )
                {
                    objects.RemoveAll( o => !o.Name.EndsWith( "/" ) );
                }

                // If recursion is not allowed, then remove objects that are beyond the initial depth
                if ( !allowRecursion )
                {
                    objects.RemoveAll( o => GetKeyDepth( o.Name ) > initialDepth );
                }

                // Google includes the root directory of the listing request, but Rock does not expect to get "self" in the list
                directory = directory.EndsWith( "/" ) ? directory : $"{directory}/";
                objects.RemoveAll( o => o.Name == directory );

                return objects.Select( o => TranslateGoogleObjectToRockAsset( assetStorageProvider, o ) ).ToList();
            }
        }

        /// <summary>
        /// Gets the key depth. For example a/b/c/d.txt would have a depth of 3.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private int GetKeyDepth( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return 0;
            }

            key = key.Trim();

            // Remove the last character because folders like "a/b/" are actually at depth 1 and sibling to files like "a/1.txt"
            if ( key.EndsWith( "/" ) )
            {
                key.Substring( 0, key.Trim().Length - 1 );
            }

            var depth = key.Count( c => c == '/' );
            return depth;
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

            var pathSegments = key.Split( '/' );

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

        #endregion Private Methods
    }
}
