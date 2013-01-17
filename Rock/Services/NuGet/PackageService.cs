using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using NuGet;
using Rock.Model;

namespace Rock.Services.NuGet
{
    /// <summary>
    /// Facade class to provide intaraction with NuGet internals
    /// </summary>
    public class PackageService
    {
        /// <summary>
        /// Exports the page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="isRecursive">if set to <c>true</c> [should export children].</param>
        /// <returns></returns>
        public MemoryStream ExportPage( Page page, bool isRecursive )
        {
            // Create temp blockTypeFilePath to store package
            var packageDirectory = CreatePackageDirectory( page.Name );

            // Generate the JSON from model data and write it to disk
            var json = GetJson( page, isRecursive );
            using ( var file = new StreamWriter( Path.Combine( packageDirectory.FullName, "export.json" ) ) )
            {
                file.Write( json );   
            }

            // Create a temp directory to hold package contents in staging area
            // * Find out from `page` which Blocks need to be packaged up (recursively if ExportChildren box is checked)
            // * Grab asset folders for each Block's code file uniquely (Scripts, Assets, CSS folders)

            var packageBuilder = new PackageBuilder
            {
                Id = packageDirectory.Name,
                Version = new SemanticVersion( "0.0" ),
                Description = page.Description
            };

            packageBuilder.Authors.Add( "PageExport" );

            var webRootPath = HttpContext.Current.Server.MapPath( "~" );
            AddToPackage( packageBuilder, packageDirectory.FullName, webRootPath, "export.json", SearchOption.TopDirectoryOnly );
            var blockTypes = new Dictionary<Guid, BlockType>();
            var uniqueDirectories = new Dictionary<string, string>();
            FindUniqueBlockTypesAndDirectories( page, isRecursive, blockTypes, uniqueDirectories );

            foreach ( var blockType in blockTypes.Values )
            {
                var sourcePath = HttpContext.Current.Server.MapPath( blockType.Path );
                var directory = sourcePath.Substring( 0, sourcePath.LastIndexOf( Path.DirectorySeparatorChar ) );
                var fileName = Path.GetFileNameWithoutExtension( sourcePath );
                var pattern = string.Format( "{0}.*", fileName );
                AddToPackage( packageBuilder, directory, webRootPath, pattern, SearchOption.TopDirectoryOnly );
            }

            // Second pass through blocks. Determine which folders need to be added to the packageBuilder
            foreach ( var dir in uniqueDirectories.Keys )
            {
                var sourcePath = HttpContext.Current.Server.MapPath( dir );

                // Are there any folders present named "CSS", "Scripts" or "Assets"?
                AddToPackage( packageBuilder, Path.Combine(sourcePath, "CSS" ), webRootPath );
                AddToPackage( packageBuilder, Path.Combine(sourcePath, "Scripts" ), webRootPath );
                AddToPackage( packageBuilder, Path.Combine(sourcePath, "Assets" ), webRootPath );
            }
            
            var outputStream = new MemoryStream();
            packageBuilder.Save( outputStream );

            // Clean up staging area
            packageDirectory.Delete( recursive: true );
            return outputStream;
        }

        /// <summary>
        /// Imports the page.
        /// </summary>
        /// <param name="json">The json.</param>
        public void ImportPage( string json )
        {
            
        }


        private DirectoryInfo CreatePackageDirectory( string pageName )
        {
            var name = Regex.Replace( pageName, @"[^A-Za-z0-9]", string.Empty );
            var rootPath = HttpContext.Current.Server.MapPath( "~/App_Data" );
            var packageName = string.Format( "{0}_{1}", name, Guid.NewGuid() );
            return Directory.CreateDirectory( Path.Combine( rootPath, "PackageStaging", packageName ) );
        }


        private string GetJson( Page page, bool isRecursive )
        {
            string json;

            if ( isRecursive )
            {
                json = page.ToJson();
            }
            else
            {
                // make a deep copy, remove it's child pages, and ToJson that...
                var pageCopy = page.Clone() as Page ?? new Page();
                pageCopy.Pages = new List<Page>();
                json = pageCopy.ToJson();
            }

            return json;
        }


        private void FindUniqueBlockTypesAndDirectories( Page page, bool isRecursive, Dictionary<Guid, BlockType> blockTypes, Dictionary<string, string> directories )
        {
            foreach ( var block in page.Blocks )
            {
                var blockType = block.BlockType;

                if ( !blockTypes.ContainsKey( blockType.Guid ) )
                {
                    blockTypes.Add( blockType.Guid, blockType );

                    var path = blockType.Path;
                    var directory = path.Substring( 0, path.LastIndexOf( '/' ) );

                    if ( !directories.ContainsKey( directory ) )
                    {
                        directories.Add(directory, directory);
                    }
                }
            }

            if ( isRecursive )
            {
                foreach ( var child in page.Pages )
                {
                    FindUniqueBlockTypesAndDirectories( child, true, blockTypes, directories );
                }
            }
        }

        private void AddToPackage( PackageBuilder packageBuilder, string directory, string webRootPath, 
            string filterPattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories )
        {
            if ( !Directory.Exists( directory ) )
            {
                return;
            }

            var files = from file in Directory.EnumerateFiles( directory, filterPattern, searchOption )
                        let pathSuffix = file.Substring( webRootPath.Length + 1 )
                        select new ManifestFile()
                            {
                                Source = pathSuffix,
                                Target = pathSuffix,
                            };

            packageBuilder.PopulateFiles( webRootPath, files );
        }
    }
}