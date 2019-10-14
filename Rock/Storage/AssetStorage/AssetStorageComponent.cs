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

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Storage.AssetStorage
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Extension.Component" />
    public abstract class AssetStorageComponent : Component
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetStorageComponent"/> class.
        /// </summary>
        public AssetStorageComponent() : base(false)
        {
            // Override default constructor of Component that loads attributes (not needed for asset storage components, needs to be done by each AssetStorageProvider)
        }

        #endregion Constructors

        #region Properties
        /// <summary>
        /// Gets or sets the file system compont HTTP context.
        /// </summary>
        /// <value>
        /// The file system compont HTTP context.
        /// </value>
        public System.Web.HttpContext FileSystemCompontHttpContext
        {
            get
            {
                return _fileSystemCompontHttpContext ?? System.Web.HttpContext.Current;
            }

            set
            {
                _fileSystemCompontHttpContext = value;
            }
        }

        private System.Web.HttpContext _fileSystemCompontHttpContext;

        /// <summary>
        /// Specify the font awesome icon for the AssetStorageComponent here. It will display in the folder tree.
        /// Default is fa fa-server.
        /// </summary>
        /// <value>
        /// The component icon path.
        /// </value>
        public virtual string IconCssClass
        {
            get
            {
                return "fa fa-server";
            }
            set
            {
                _iconCssClass = value;
            }
        }

        private string _iconCssClass;

        /// <summary>
        /// The thumbnail root path.
        /// </summary>
        protected readonly string ThumbnailRootPath = "/Content/ASM_Thumbnails";

        #endregion Properties

        #region Component Overrides
        /// <summary>
        /// Always returns 0.  
        /// </summary>
        /// <value></value>
        public override int Order
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Always returns true. 
        /// </summary>
        /// <value></value>
        public override bool IsActive
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Use GetAttributeValue( AssetStorageProvider assetStorageProvider, string key ) instead.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Exception</returns>
        /// <exception cref="Exception">Asset Storage attributes are saved for specific asset storage components. Use GetAttributeValue( AssetStorageProvider assetStorageProvider, string key ) instead.</exception>
        public override string GetAttributeValue( string key )
        {
            throw new Exception( "Asset Storage attributes are saved for specific asset storage components. Use GetAttributeValue( AssetStorageProvider assetStorageProvider, string key ) instead." );
        }

        #endregion Component Overrides

        #region Public Methods

        /// <summary>
        /// Gets the attribute value for the provided AssetStorageProvider and Key.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( AssetStorageProvider assetStorageProvider, string key )
        {
            if ( assetStorageProvider.AttributeValues == null )
            {
                assetStorageProvider.LoadAttributes();
            }

            var values = assetStorageProvider.AttributeValues;
            if ( values != null && values.ContainsKey( key ) )
            {
                var keyValues = values[key];
                if ( keyValues != null )
                {
                    return keyValues.Value;
                }
            }

            return string.Empty;
        }

        #endregion Public Methods

        #region Abstract Methods

        /// <summary>
        /// Gets the thumbnail image for the provided Asset key. If one does not exist it will be created. If one exists but is older than the file
        /// a new thumbnail is created and the old one overwritten.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="assetKey">The asset key.</param>
        /// <param name="lastModifiedDateTime">The last modified date time.</param>
        /// <returns></returns>
        public abstract string GetThumbnail( AssetStorageProvider assetStorageProvider, string assetKey, DateTime? lastModifiedDateTime );

        /// <summary>
        /// Gets the object as an Asset.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public abstract Asset GetObject( AssetStorageProvider assetStorageProvider, Asset asset );

        /// <summary>
        /// Gets the object as an asset with an option to also create a thumbnail
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="createThumbnail">if set to <c>true</c> [create thumbnail].</param>
        /// <returns></returns>
        public abstract Asset GetObject( AssetStorageProvider assetStorageProvider, Asset asset, bool createThumbnail );

        /// <summary>
        /// Lists the objects from the current root folder.
        /// </summary>
        /// <returns></returns>
        public abstract List<Asset> ListObjects( AssetStorageProvider assetStorageProvider );

        /// <summary>
        /// Lists the objects. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If key and name are not provided then list all objects from the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder is not used, and Name is not used.
        /// The last segment in Key is treated as a begins with search if it does not end in a '/'. e.g. to get all
        /// files starting with 'mr' in folder 'pictures/cats/' set key = 'pictures/cats/mr' to get 'mr. whiskers'
        /// and 'mrs. whiskers' but not 'fluffy' or 'carnage the attack cat'.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public abstract List<Asset> ListObjects( AssetStorageProvider assetStorageProvider, Asset asset );

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
        public abstract List<Asset> ListObjectsInFolder( AssetStorageProvider assetStorageProvider, Asset asset );

        /// <summary>
        /// Lists the files in AssetStorageProvider.RootFolder.
        /// </summary>
        /// <returns></returns>
        public abstract List<Asset> ListFilesInFolder( AssetStorageProvider assetStorageProvider );

        /// <summary>
        /// Lists the files in folder. Asset.Key or Asset.Name is the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key and Name are not provided then list all files in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in the key is the folder name.
        /// </summary>
        /// <returns></returns>
        public abstract List<Asset> ListFilesInFolder( AssetStorageProvider assetStorageProvider, Asset asset );

        /// <summary>
        /// Lists the folders in AssetStorageProvider.Rootfolder.
        /// </summary>
        /// <returns></returns>
        public abstract List<Asset> ListFoldersInFolder( AssetStorageProvider assetStorageProvider );

        /// <summary>
        /// Lists the folder in folder. Asset.Key or Asset.Name is the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key and Name are not provided the list then list all files in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in the key is the folder name.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public abstract List<Asset> ListFoldersInFolder( AssetStorageProvider assetStorageProvider, Asset asset );

        /// <summary>
        /// Uploads a file. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If a key is provided it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public abstract bool UploadObject( AssetStorageProvider assetStorageProvider, Asset asset );

        /// <summary>
        /// Creates a folder. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key is provided it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public abstract bool CreateFolder( AssetStorageProvider assetStorageProvider, Asset asset );

        /// <summary>
        /// Deletes the asset. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key is provided then it MUST use the full path, RootFolder is not used.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public abstract bool DeleteAsset( AssetStorageProvider assetStorageProvider, Asset asset );

        /// <summary>
        /// Renames the asset.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="newName">The new name.</param>
        /// <returns></returns>
        public abstract bool RenameAsset( AssetStorageProvider assetStorageProvider, Asset asset, string newName );

        /// <summary>
        /// Creates the download link.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public abstract string CreateDownloadLink( AssetStorageProvider assetStorageProvider, Asset asset );

        #endregion Abstract Methods

        #region Protected Methods

        /// <summary>
        /// Fixes the root folder syntax if it was entered incorrectly.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <returns></returns>
        protected virtual string FixRootFolder( string rootFolder )
        {
            if ( rootFolder.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }
            else if ( rootFolder.EndsWith( "/" ) )
            {
                return rootFolder;
            }
            else
            {
                return rootFolder + "/";
            }
        }

        /// <summary>
        /// Gets the icon for the file type based on the extension of the provided file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        protected virtual string GetFileTypeIcon( string fileName )
        {
            string fileExtension = Path.GetExtension( fileName ).TrimStart( '.' );
            string virtualThumbnailFilePath = string.Format( "/Assets/Icons/FileTypes/{0}.png", fileExtension );
            string thumbnailFilePath = FileSystemCompontHttpContext.Request.MapPath( virtualThumbnailFilePath );

            if ( !File.Exists( thumbnailFilePath ) )
            {
                virtualThumbnailFilePath = "/Assets/Icons/FileTypes/other.png";
                thumbnailFilePath = FileSystemCompontHttpContext.Request.MapPath( virtualThumbnailFilePath );
            }

            return virtualThumbnailFilePath;
        }

        /// <summary>
        /// Creates the image thumbnail using the provided Asset. If a thumbnail already exists it will be overwritten.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="physicalThumbPath">The physical thumb path.</param>
        /// <param name="isLocal">True if the image is on the local server, false if not.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        protected virtual void CreateImageThumbnail( AssetStorageProvider assetStorageProvider, Asset asset, string physicalThumbPath, bool isLocal = true, int? width = null, int? height = null )
        {
            if ( isLocal )
            {
                CreateImageThumbnailFromFile( assetStorageProvider, asset, physicalThumbPath, width, height );
            }
            else
            {
                CreateImageThumbnailFromStream( assetStorageProvider, asset, physicalThumbPath, width, height );
            }
        }

        /// <summary>
        /// Deletes the image thumbnail for the provided Asset. If the asset is a file then the singel thumbnail
        /// is deleted. If the asset is a directory then a recurrsive delete is done.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        protected virtual void DeleteImageThumbnail( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string cleanKey = asset.Key.TrimStart( '~' );
            string virtualPath = $"{ThumbnailRootPath}/{assetStorageProvider.Id}/{cleanKey}";
            string physicalPath = FileSystemCompontHttpContext.Server.MapPath( virtualPath );

            try
            {
                if ( asset.Type == AssetType.File )
                {
                    if ( File.Exists( physicalPath ) )
                    {
                        File.Delete( physicalPath );
                    }
                }
                else if ( asset.Type == AssetType.Folder )
                {
                    if ( Directory.Exists( physicalPath ) )
                    {
                        Directory.Delete( physicalPath, true );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Checks the file extension against the Content File Type White list.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>
        ///   <c>true</c> if [is file type allowed by white list] [the specified asset]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsFileTypeAllowedByWhiteList( string fileName )
        {
            // validate file type (applies to all uploaded files)
            var globalAttributesCache = GlobalAttributesCache.Get();

            IEnumerable<string> contentFileTypeWhiteList = ( globalAttributesCache.GetValue( "ContentFiletypeWhitelist" ) ?? string.Empty ).Split( new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries );
            contentFileTypeWhiteList = contentFileTypeWhiteList.Select( a => a.ToLower().TrimStart( new char[] { '.', ' ' } ) );

            // Get file extension and then trim any trailing spaces (to catch any nefarious stuff).
            string fileExtension = Path.GetExtension( fileName ).ToLower().TrimStart( new char[] { '.' } ).Trim();
            
            if ( contentFileTypeWhiteList.Any() && !contentFileTypeWhiteList.Contains( fileExtension ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks the file extension against the Content File Type Black list.
        /// </summary>
        /// <param name="fileName">Name of the file with the extension. Can inclued the full path.</param>
        /// <returns>
        ///   <c>true</c> if [is file type allowed by black list] [the specified asset]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsFileTypeAllowedByBlackList( string fileName )
        {
            // validate file type (applies to all uploaded files)
            var globalAttributesCache = GlobalAttributesCache.Get();

            IEnumerable<string> contentFileTypeBlackList = ( globalAttributesCache.GetValue( "ContentFiletypeBlacklist" ) ?? string.Empty ).Split( new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries );
            contentFileTypeBlackList = contentFileTypeBlackList.Select( a => a.ToLower().TrimStart( new char[] { '.', ' ' } ) );

            // Get file extension and then trim any trailing spaces (to catch any nefarious stuff).
            string fileExtension = Path.GetExtension( fileName ).ToLower().TrimStart( new char[] { '.' } ).Trim();

            if ( contentFileTypeBlackList.Contains( fileExtension ) )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks the file extension against the Content File Type Black and White lists.
        /// Checks the Blacklist first so that one takes precedence.
        /// </summary>
        /// <param name="fileName">Name of the file. Can inluced the full path.</param>
        /// <returns>
        ///   <c>true</c> if [is file type allowed by black and white lists] [the specified file name]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsFileTypeAllowedByBlackAndWhiteLists( string fileName )
        {
            if ( !IsFileTypeAllowedByBlackList( fileName ) )
            {
                return false;
            }
            else if ( !IsFileTypeAllowedByWhiteList( fileName ) )
            {
                return false;
            }

            return true;
        }

        #endregion Protected Methods

        #region Private Methods

        /// <summary>
        /// Creates the image thumbnail from file.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="physicalThumbPath">The physical thumb path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        private void CreateImageThumbnailFromFile( AssetStorageProvider assetStorageProvider, Asset asset, string physicalThumbPath, int? width = null, int? height = null )
        {
            string assetFilePath = FileSystemCompontHttpContext.Request.MapPath( asset.Key );

            if ( Path.GetExtension( asset.Name ).Equals( ".svg", StringComparison.OrdinalIgnoreCase ) ||
                Path.GetExtension( asset.Name ).Equals( ".ico", StringComparison.OrdinalIgnoreCase ) )
            {
                // just save the ico or svg to the thumbnail dir as there is no need to make a thumbnail
                File.Copy( assetFilePath, physicalThumbPath, true );
                return;
            }

            using ( var resizedStream = new FileStream( physicalThumbPath, FileMode.Create ) )
            using ( var origImageStream = new MemoryStream() )
            using ( var image = System.Drawing.Image.FromFile( assetFilePath ) )
            {
                image.Save( origImageStream, image.RawFormat );
                origImageStream.Position = 0;
                ImageResizer.ImageBuilder.Current.Build( origImageStream, resizedStream, new ImageResizer.ResizeSettings { Width = width ?? 100, Height = height ?? 100 } );
                resizedStream.Flush();
            }
        }

        /// <summary>
        /// Creates the image thumbnail from stream.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="physicalThumbPath">The physical thumb path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        private void CreateImageThumbnailFromStream( AssetStorageProvider assetStorageProvider, Asset asset, string physicalThumbPath, int? width = null, int? height = null )
        {
            asset = GetObject( assetStorageProvider, asset, false );

            using ( var resizedStream = new FileStream( physicalThumbPath, FileMode.Create ) )
            {
                if ( Path.GetExtension( asset.Name ).Equals( ".svg", StringComparison.OrdinalIgnoreCase ) )
                {
                    // just save the svg to the thumbnail dir
                    asset.AssetStream.CopyTo( resizedStream );
                }
                else
                {
                    using ( var origImageStream = new MemoryStream() )
                    {
                        asset.AssetStream.CopyTo( origImageStream );
                        origImageStream.Position = 0;
                        ImageResizer.ImageBuilder.Current.Build( origImageStream, resizedStream, new ImageResizer.ResizeSettings { Width = width ?? 100, Height = height ?? 100 } );
                    }
                }

                resizedStream.Flush();
            }
        }

        #endregion Private Methods
    }
}
