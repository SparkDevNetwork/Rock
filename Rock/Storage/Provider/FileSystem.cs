//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Web;
using Rock.Model;

namespace Rock.Storage.Provider
{
    /// <summary>
    /// Storage provider for saving binary files to file system
    /// </summary>
    [Description( "File System-driven file storage" )]
    [Export( typeof( ProviderComponent ) )]
    [ExportMetadata( "ComponentName", "FileSystem" )]
    public class FileSystem : ProviderComponent
    {
        /// <summary>
        /// Saves the file to the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.ArgumentException">File Data must not be null.</exception>
        public override void SaveFile( BinaryFile file, HttpContext context )
        {
            if ( file.Data == null )
            {
                throw new ArgumentException( "File Data must not be null." );
            }

            var url = GenerateUrl( file );
            var physicalPath = GetPhysicalPath( url, context );
            var directoryName = Path.GetDirectoryName( physicalPath );

            if ( !Directory.Exists( directoryName ) )
            {
                Directory.CreateDirectory( directoryName );
            }

            File.WriteAllBytes( physicalPath, file.Data.Content );

            // Set Data to null after successful OS save so the the binary data is not 
            // written into the database.
            file.Data = null;
            file.Url = url;
        }

        /// <summary>
        /// Removes the file from the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        public override void RemoveFile( BinaryFile file, HttpContext context )
        {
            var physicalPath = GetPhysicalPath( file.Url, context );

            if ( File.Exists( physicalPath ) )
            {
                File.Delete( physicalPath );
            }
        }

        /// <summary>
        /// Gets the file bytes from the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override byte[] GetFileContent( BinaryFile file, HttpContext context )
        {
            var physicalPath = GetPhysicalPath( file.Url, context );

            if ( File.Exists( physicalPath ) )
            {
                return File.ReadAllBytes( physicalPath );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Generate a URL for the file based on the rules of the StorageProvider
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public override string GenerateUrl( BinaryFile file )
        {
            if ( string.IsNullOrWhiteSpace( file.FileName ) )
            {
                return null;
            }

            var urlBuilder = new StringBuilder();

            string rootPath = GetRootPath( file.BinaryFileTypeId ?? 0 );

            urlBuilder.Append( rootPath );

            if ( !rootPath.EndsWith( "/" ) )
            {
                urlBuilder.Append( "/" );
            }

            // Prefix the FileName on disk with the Guid so that we can have multiple files with the same name (for example, setup.exe and setup.exe)
            urlBuilder.Append( file.Guid + "_" + file.FileName );
            return urlBuilder.ToString();
        }

        /// <summary>
        /// Gets the root path.
        /// </summary>
        /// <param name="binaryFileTypeId">The binary file type identifier.</param>
        /// <returns></returns>
        /// <value>
        /// The root path.
        /// </value>
        private string GetRootPath( int binaryFileTypeId )
        {
            BinaryFileType binaryFileType = new BinaryFileTypeService().Get( binaryFileTypeId );
            binaryFileType.LoadAttributes();
            string rootPath = binaryFileType.GetAttributeValue( "RootPath" );
            return rootPath;
        }

        /// <summary>
        /// Gets the physical path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private string GetPhysicalPath( string path, HttpContext context )
        {
            if ( path.StartsWith( "C:" ) || path.StartsWith( "\\\\" ) )
            {
                return path;
            }

            if ( path.StartsWith( "http:" ) || path.StartsWith( "https:" ) || path.StartsWith( "/" ) || path.StartsWith( "~" ) )
            {
                return context.Server.MapPath( path );
            }

            return Path.Combine( AppDomain.CurrentDomain.BaseDirectory, path );
        }
    }
}
