using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Rock.Model;
using Rock.Attribute;

namespace Rock.Storage.AssetStorage
{
    [Description( "Server File System" )]
    [Export( typeof( AssetStorageComponent ) )]
    [ExportMetadata( "ComponentName", "ServerFileSystem" )]

    [TextField( name: "Root Folder", description: "", required: true, defaultValue: "~/", category: "", order: 0, key: "RootFolder" )]

    public class FileSystemComponent : AssetStorageComponent
    {
        #region Properties
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
        public FileSystemComponent() : base()
        {
        }

        #endregion Constructors

        #region Abstract Methods

        /// <summary>
        /// Creates the download link.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override string CreateDownloadLink( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            try
            {
                string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
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
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override bool CreateFolder( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
            HasRequirementsFolder( asset );
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
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override bool DeleteAsset( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            try
            {
                string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
                asset.Key = FixKey( asset, rootFolder );
                string physicalPath = FileSystemCompontHttpContext.Server.MapPath( asset.Key );

                if ( asset.Type == AssetType.File )
                {
                    File.Delete( Path.Combine( physicalPath ) );
                }
                else
                {
                    Directory.Delete( physicalPath, true );
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
        /// Gets the object as an Asset.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override Asset GetObject( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            try
            {
                string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );

                asset.Key = FixKey( asset, rootFolder );
                string physicalFile = FileSystemCompontHttpContext.Server.MapPath( asset.Key );
                FileInfo fileInfo = new FileInfo( physicalFile );

                var objAsset = CreateAssetFromFileInfo( fileInfo );
                FileStream fs = new FileStream( physicalFile, FileMode.Open );
                objAsset.AssetStream = fs;

                return objAsset;
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw;
            }
        }

        /// <summary>
        /// Lists the files in AssetStorageSystem.RootFolder.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <returns></returns>
        public override List<Asset> ListFilesInFolder( AssetStorageSystem assetStorageSystem )
        {
            var asset = new Asset();
            asset.Type = AssetType.Folder;
            return ListFilesInFolder( assetStorageSystem, asset );
        }

        /// <summary>
        /// Lists the files in folder. Asset.Key or Asset.Name is the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key and Name are not provided then list all files in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in the key is the folder name.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override List<Asset> ListFilesInFolder( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );

            asset.Key = FixKey( asset, rootFolder );
            string physicalFolder = FileSystemCompontHttpContext.Server.MapPath( asset.Key );

            return GetListOfObjects( physicalFolder, SearchOption.TopDirectoryOnly, AssetType.File );
        }

        /// <summary>
        /// Lists the folders in AssetStorageSystem.Rootfolder.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <returns></returns>
        public override List<Asset> ListFoldersInFolder( AssetStorageSystem assetStorageSystem )
        {
            var asset = new Asset();
            asset.Type = AssetType.Folder;
            return ListFoldersInFolder( assetStorageSystem, asset );
        }

        /// <summary>
        /// Lists the folder in folder. Asset.Key or Asset.Name is the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If Key and Name are not provided the list then list all files in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in the key is the folder name.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override List<Asset> ListFoldersInFolder( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );
            asset.Key = FixKey( asset, rootFolder );
            string physicalFolder = FileSystemCompontHttpContext.Server.MapPath( asset.Key );

            return GetListOfObjects( physicalFolder, SearchOption.TopDirectoryOnly, AssetType.Folder );
        }

        /// <summary>
        /// Lists the objects from the current root folder.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <returns></returns>
        public override List<Asset> ListObjects( AssetStorageSystem assetStorageSystem )
        {
            var asset = new Asset();
            asset.Type = AssetType.Folder;
            return ListObjects( assetStorageSystem, asset );
        }

        /// <summary>
        /// Lists the objects. If Asset.Key is not provided then one is created using the RootFolder and Asset.Name.
        /// If key and name are not provided then list all objects from the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder is not used, and Name is not used.
        /// The last segment in Key is treated as a begins with search if it does not end in a '/'. e.g. to get all
        /// files starting with 'mr' in folder 'pictures/cats/' set key = 'pictures/cats/mr' to get 'mr. whiskers'
        /// and 'mrs. whiskers' but not 'fluffy' or 'carnage the attack cat'.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override List<Asset> ListObjects( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );

            asset.Key = FixKey( asset, rootFolder );
            var assets = new List<Asset>();

            string physicalFolder = FileSystemCompontHttpContext.Server.MapPath( asset.Key );

            assets.AddRange( GetListOfObjects( physicalFolder, SearchOption.AllDirectories, AssetType.Folder ) );
            assets.AddRange( GetListOfObjects( physicalFolder, SearchOption.AllDirectories, AssetType.File ) );
            return assets.OrderBy( a => a.Key ).ToList();
        }

        /// <summary>
        /// Lists the objects in folder. The asset key or name should be the folder.
        /// If Asset.Key is not provided then one is created using the RootFolder and Asset.Name
        /// If Key and Name are not provided then list all objects in the current RootFolder.
        /// If a key is provided it MUST use the full path, RootFolder and Name are not used.
        /// The last segment in key is the folder name.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override List<Asset> ListObjectsInFolder( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );

            asset.Key = FixKey( asset, rootFolder );
            var assets = new List<Asset>();

            string physicalFolder = FileSystemCompontHttpContext.Server.MapPath( asset.Key );

            assets.AddRange( GetListOfObjects( physicalFolder, SearchOption.TopDirectoryOnly, AssetType.Folder ) );
            assets.AddRange( GetListOfObjects( physicalFolder, SearchOption.TopDirectoryOnly, AssetType.File ) );
            return assets.OrderBy( a => a.Key ).ToList();
        }

        /// <summary>
        /// Renames the asset.
        /// </summary>
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset">The asset.</param>
        /// <param name="newName">The new name.</param>
        /// <returns></returns>
        public override bool RenameAsset( AssetStorageSystem assetStorageSystem, Asset asset, string newName )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );

            try
            {
                asset.Key = FixKey( asset, rootFolder );
                string filePath = GetPathFromKey( asset.Key );
                string physicalFolder = FileSystemCompontHttpContext.Server.MapPath( filePath );
                string physicalFile = FileSystemCompontHttpContext.Server.MapPath( asset.Key );

                File.Move( physicalFile, Path.Combine( physicalFolder, newName ) );

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
        /// <param name="assetStorageSystem"></param>
        /// <param name="asset">The asset.</param>
        /// <returns></returns>
        public override bool UploadObject( AssetStorageSystem assetStorageSystem, Asset asset )
        {
            string rootFolder = FixRootFolder( GetAttributeValue( assetStorageSystem, "RootFolder" ) );

            asset.Key = FixKey( asset, rootFolder );

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
        /// <param name="directoryName">Name of the directory.</param>
        /// <param name="searchOption">The search option.</param>
        /// <param name="assetType">Type of the asset.</param>
        /// <returns></returns>
        private List<Asset> GetListOfObjects( string directoryName, SearchOption searchOption, AssetType assetType )
        {
            List<Asset> assets = new List<Asset>();
            var baseDirectory = new DirectoryInfo( directoryName );

            if ( assetType == AssetType.Folder )
            {
                var directoryInfos = baseDirectory.GetDirectories( "*", searchOption );

                foreach ( var directoryInfo in directoryInfos )
                {
                    var asset = CreateAssetFromDirectoryInfo( directoryInfo );
                    assets.Add( asset );
                }
            }
            else
            {
                var fileInfos = baseDirectory.GetFiles( "*", searchOption );

                foreach( var fileInfo in fileInfos )
                {
                    var asset = CreateAssetFromFileInfo( fileInfo );
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
        /// <param name="fileInfo">The file information.</param>
        /// <returns></returns>
        private Asset CreateAssetFromFileInfo( FileInfo fileInfo )
        {
            string relativePath = ReverseMapPath( fileInfo.FullName );

            return new Asset
            {
                Name = fileInfo.Name,
                Key = relativePath,
                Uri = $"{FileSystemCompontHttpContext.Request.Url.GetLeftPart( UriPartial.Authority )}/{relativePath.TrimStart( '~' )}",
                Type = AssetType.File,
                IconPath = GetFileTypeIcon( fileInfo.Name ),
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
