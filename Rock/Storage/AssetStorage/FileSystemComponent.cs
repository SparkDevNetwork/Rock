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
using System.Web;

using Rock.Attribute;
using Rock.Model;

namespace Rock.Storage.AssetStorage
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Storage.AssetStorage.AssetStorageComponent" />
    [Description( "Server File System" )]
    [Export( typeof( AssetStorageComponent ) )]
    [ExportMetadata( "ComponentName", "ServerFileSystem" )]

    [TextField( name: "Root Folder", description: "", required: true, defaultValue: "~/", category: "", order: 0, key: "RootFolder" )]
    public class FileSystemComponent : AssetStorageComponent
    {
        #region Properties

        private List<string> HiddenFolders
        {
            get
            {
                return new List<string>()
                {
                    "Content\\ASM_Thumbnails"
                };
            }
        }

        /// <summary>
        /// Fixes the root folder syntax if it was entered incorrectly.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <returns></returns>
        protected override string FixRootFolder( string rootFolder )
        {
            if ( rootFolder.IsNullOrWhiteSpace() )
            {
                rootFolder = "~/";
            }
            else
            {
                rootFolder = rootFolder.EndsWith( "/" ) ? rootFolder : rootFolder += "/";
                rootFolder = rootFolder.StartsWith( "~/" ) ? rootFolder : $"~/{rootFolder}";
            }

            return rootFolder;
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemComponent"/> class.
        /// </summary>
        public FileSystemComponent() : base()
        {
        }

        #endregion Constructors

        #region Abstract Methods

        /// <summary>
        /// Creates the download link.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override string CreateDownloadLink( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            try
            {
                string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );
                asset.Key = FixKey( asset, rootFolder );
                string domainName = FileSystemCompontHttpContext.Request.Url.GetLeftPart( UriPartial.Authority );
                string uriKey = asset.Key.TrimStart( '~' );
                return domainName + uriKey;
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
            HasRequirementsFolder( asset );
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );
            asset.Key = FixKey( asset, rootFolder );
            
            string physicalFolder = FileSystemCompontHttpContext.Server.MapPath( asset.Key );

            try
            {
                Directory.CreateDirectory( physicalFolder );
                return true;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
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
            try
            {
                string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );
                asset.Key = FixKey( asset, rootFolder );
                string physicalPath = FileSystemCompontHttpContext.Server.MapPath( asset.Key );

                if ( asset.Type == AssetType.File )
                {
                    if ( File.Exists( physicalPath ) )
                    {
                        File.Delete( physicalPath );
                    }
                }
                else
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

            DeleteImageThumbnail( assetStorageProvider, asset );

            return true;
        }

        /// <summary>
        /// Gets the object as an Asset and creates the thumbnail.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override Asset GetObject( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            return GetObject( assetStorageProvider, asset, true );
        }

        /// <summary>
        /// Gets the object as an Asset with the option to create a thumbnail.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="asset">The asset.</param>
        /// <param name="createThumbnail">if set to <c>true</c> [create thumbnail].</param>
        /// <returns></returns>
        public override Asset GetObject( AssetStorageProvider assetStorageProvider, Asset asset, bool createThumbnail )
        {
            try
            {
                string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );

                asset.Key = FixKey( asset, rootFolder );
                string physicalFile = FileSystemCompontHttpContext.Server.MapPath( asset.Key );
                FileInfo fileInfo = new FileInfo( physicalFile );

                var objAsset = CreateAssetFromFileInfo( assetStorageProvider, fileInfo, createThumbnail );
                using ( FileStream fs = new FileStream( physicalFile, FileMode.Open ) )
                {
                    objAsset.AssetStream = new MemoryStream();
                    fs.CopyTo( objAsset.AssetStream );
                }

                return objAsset;
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
            var asset = new Asset();
            asset.Type = AssetType.Folder;
            return ListFilesInFolder( assetStorageProvider, asset );
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
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );

            asset.Key = FixKey( asset, rootFolder );
            string physicalFolder = FileSystemCompontHttpContext.Server.MapPath( asset.Key );

            return GetListOfObjects( assetStorageProvider, physicalFolder, SearchOption.TopDirectoryOnly, AssetType.File );
        }

        /// <summary>
        /// Lists the folders in AssetStorageProvider.Rootfolder.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <returns></returns>
        public override List<Asset> ListFoldersInFolder( AssetStorageProvider assetStorageProvider )
        {
            var asset = new Asset();
            asset.Type = AssetType.Folder;
            return ListFoldersInFolder( assetStorageProvider, asset );
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
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );
            asset.Key = FixKey( asset, rootFolder );
            string physicalFolder = FileSystemCompontHttpContext.Server.MapPath( asset.Key );
            var assets = new List<Asset>();
            if ( !HiddenFolders.Any( a => physicalFolder.IndexOf( a, StringComparison.OrdinalIgnoreCase ) > 0 ) )
            {
                assets = GetListOfObjects( assetStorageProvider, physicalFolder, SearchOption.TopDirectoryOnly, AssetType.Folder );
            }
            return assets;
        }

        /// <summary>
        /// Lists the objects from the current root folder.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <returns></returns>
        public override List<Asset> ListObjects( AssetStorageProvider assetStorageProvider )
        {
            var asset = new Asset();
            asset.Type = AssetType.Folder;
            return ListObjects( assetStorageProvider, asset );
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
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );

            asset.Key = FixKey( asset, rootFolder );
            var assets = new List<Asset>();

            string physicalFolder = FileSystemCompontHttpContext.Server.MapPath( asset.Key );

            assets.AddRange( GetListOfObjects( assetStorageProvider, physicalFolder, SearchOption.AllDirectories, AssetType.Folder ) );
            assets.AddRange( GetListOfObjects( assetStorageProvider, physicalFolder, SearchOption.AllDirectories, AssetType.File ) );
            return assets.OrderBy( a => a.Key ).ToList();
        }

        /// <summary>
        /// Lists the objects in folder. The asset key or name should be the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name
        /// If Key and Name are not provided then list all objects in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in key is the folder name.
        /// </summary>
        /// <param name="assetStorageProvider"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override List<Asset> ListObjectsInFolder( AssetStorageProvider assetStorageProvider, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );

            asset.Key = FixKey( asset, rootFolder );
            var assets = new List<Asset>();

            string physicalFolder = FileSystemCompontHttpContext.Server.MapPath( asset.Key );

            assets.AddRange( GetListOfObjects( assetStorageProvider, physicalFolder, SearchOption.TopDirectoryOnly, AssetType.Folder ) );
            assets.AddRange( GetListOfObjects( assetStorageProvider, physicalFolder, SearchOption.TopDirectoryOnly, AssetType.File ) );
            return assets.OrderBy( a => a.Key ).ToList();
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
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );
            
            if ( !IsFileTypeAllowedByBlackAndWhiteLists( newName ) )
            {
                string ext = System.IO.Path.GetExtension( asset.Key );
                var ex = new Rock.Web.FileUploadException( $"Filetype {ext} is not allowed.", System.Net.HttpStatusCode.NotAcceptable );
                ExceptionLogService.LogException( ex );
                throw ex;
            }

            try
            {
                asset.Key = FixKey( asset, rootFolder );
                string filePath = GetPathFromKey( asset.Key );
                string physicalFolder = FileSystemCompontHttpContext.Server.MapPath( filePath );
                string physicalFile = FileSystemCompontHttpContext.Server.MapPath( asset.Key );
                string newPhysicalFile = Path.Combine( physicalFolder, newName );

                File.Move( physicalFile, newPhysicalFile );
                DeleteImageThumbnail( assetStorageProvider, asset );

                return true;
            }
            catch ( Exception )
            {

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
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageProvider, "RootFolder" ) );

            asset.Key = FixKey( asset, rootFolder );

            if ( !IsFileTypeAllowedByBlackAndWhiteLists( asset.Key ) )
            {
                string ext = System.IO.Path.GetExtension( asset.Key );
                var ex = new Rock.Web.FileUploadException( $"Filetype {ext} is not allowed.", System.Net.HttpStatusCode.NotAcceptable );
                ExceptionLogService.LogException( ex );
                throw ex;
            }

            try
            {
                string physicalPath = FileSystemCompontHttpContext.Server.MapPath( asset.Key );

                using ( FileStream fs = new FileStream( physicalPath, FileMode.Create ) )
                using ( asset.AssetStream )
                {
                    asset.AssetStream.CopyTo( fs );
                }

                return true;
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
            string path = GetPathFromKey( assetKey ).TrimStart( '~', '/' );

            // Let's not create thumbnails of thumbnails
            if ( path.Contains( ThumbnailRootPath ) )
            {
                return assetKey;
            }

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

            CreateImageThumbnail( assetStorageProvider, new Asset { Name = name, Key = assetKey, Type = AssetType.File }, physicalThumbPath, true );

            return virtualThumbPath;
        }

        #endregion Abstract Methods

        #region Private Methods

        /// <summary>
        /// Takes a server path and returns a virtual path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private string ReverseMapPath( string path )
        {
            string appPath = HttpContext.Current.Server.MapPath( "~" );
            string res = string.Format( "~/{0}", path.Replace( appPath, "" ).Replace( "\\", "/" ) );
            return res;
        }

        /// <summary>
        /// Gets the list of objects.
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="searchOption">The search option.</param>
        /// <param name="assetType">Type of the asset.</param>
        /// <returns></returns>
        private List<Asset> GetListOfObjects( AssetStorageProvider assetStorageProvider, string directoryName, SearchOption searchOption, AssetType assetType )
        {
            List<Asset> assets = new List<Asset>();
            var baseDirectory = new DirectoryInfo( directoryName );

            if ( assetType == AssetType.Folder )
            {
                var directoryInfos = baseDirectory.GetDirectories( "*", searchOption );

                foreach ( var directoryInfo in directoryInfos )
                {
                    if ( !HiddenFolders.Any( a => directoryInfo.FullName.IndexOf( a, StringComparison.OrdinalIgnoreCase ) > 0 ) )
                    {
                        var asset = CreateAssetFromDirectoryInfo( directoryInfo );
                        assets.Add( asset );
                    }
                }
            }
            else
            {
                var fileInfos = baseDirectory.GetFiles( "*", searchOption );

                foreach( var fileInfo in fileInfos )
                {
                    var asset = CreateAssetFromFileInfo( assetStorageProvider, fileInfo, true );
                    assets.Add( asset );
                }
            }

            return assets;
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

            if ( asset.Type == AssetType.Folder )
            {
                asset.Key = asset.Key.EndsWith( "/" ) == true ? asset.Key : asset.Key += "/";
            }

            if ( !asset.Key.StartsWith( "~/" ) )
            {
                asset.Key = $"~/{asset.Key}";
            }

            return asset.Key;
        }

        /// <summary>
        /// Determines whether the Asset meets the requirements to be a folder.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <exception cref="Exception">
        /// Asset Type is set to 'File' instead of 'Folder.'
        /// or
        /// Name and key cannot both be null or empty.
        /// or
        /// Invalid characters in Asset.Name
        /// or
        /// Invalid characters in Asset.Key
        /// </exception>
        private void HasRequirementsFolder( Asset asset )
        {
            if ( asset.Type == AssetType.File )
            {
                throw new Exception( "Asset Type is set to 'File' instead of 'Folder.'" );
            }

            if ( asset.Name.IsNullOrWhiteSpace() && asset.Key.IsNullOrWhiteSpace() )
            {
                throw new Exception( "Name and key cannot both be null or empty." );
            }

            // validate the string for legal characters
            var invalidChars = Path.GetInvalidPathChars().ToList();
            invalidChars.Add( '\\' );
            invalidChars.Add( '~' );
            invalidChars.Add( '/' );

            if ( asset.Name.IsNotNullOrWhiteSpace() )
            {
                if ( asset.Name.ToList().Any( c => invalidChars.Contains( c ) ) )
                {
                    throw new Exception( "Invalid characters in Asset.Name" );
                }
            }

            if ( asset.Key.IsNotNullOrWhiteSpace() )
            {
                invalidChars.Remove( '/' );
                invalidChars.Remove( '~' );
                if ( asset.Key.ToList().Any( c => invalidChars.Contains( c ) ) )
                {
                    throw new Exception( "Invalid characters in Asset.Key" );
                }
            }

        }

        /// <summary>
        /// Creates an Asset from DirectoryInfo
        /// </summary>
        /// <param name="directoryInfo">The directory information.</param>
        /// <returns></returns>
        private Asset CreateAssetFromDirectoryInfo( DirectoryInfo directoryInfo )
        {
            return new Asset
            {
                Name = directoryInfo.Name,
                Key = ReverseMapPath( directoryInfo.FullName ) +"/",
                Uri = string.Empty,
                Type = AssetType.Folder,
                IconPath = "fa fa-folder",
                FileSize = 0,
                LastModifiedDateTime = directoryInfo.LastWriteTime,
                Description = string.Empty
            };
        }

        /// <summary>
        /// Creates an Asset from FileInfo
        /// </summary>
        /// <param name="assetStorageProvider">The asset storage provider.</param>
        /// <param name="fileInfo">The file information.</param>
        /// <param name="createThumbnail">if set to <c>true</c> [create thumbnail].</param>
        /// <returns></returns>
        private Asset CreateAssetFromFileInfo( AssetStorageProvider assetStorageProvider, FileInfo fileInfo, bool createThumbnail )
        {
            string relativePath = ReverseMapPath( fileInfo.FullName );

            return new Asset
            {
                Name = fileInfo.Name,
                Key = relativePath,
                Uri = $"{FileSystemCompontHttpContext.Request.Url.GetLeftPart( UriPartial.Authority )}/{relativePath.TrimStart( '~' )}",
                Type = AssetType.File,
                IconPath = createThumbnail ? GetThumbnail( assetStorageProvider, relativePath, fileInfo.LastWriteTime ) : string.Empty,
                FileSize = fileInfo.Length,
                LastModifiedDateTime = fileInfo.LastWriteTime,
                Description = string.Empty
            };
        }

        /// <summary>
        /// Gets the path from key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetPathFromKey( string key )
        {
            int i = key.LastIndexOf( '/' );
            if ( i < 1 )
            {
                return string.Empty;
            }

            return key.Substring( 0, i + 1 );
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

        #endregion Private Methods
    }
}
