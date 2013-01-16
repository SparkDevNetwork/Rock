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

            // Generate the JSON from model data recursively, or not
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

            using ( var file = new StreamWriter( Path.Combine( packageDirectory.FullName, "export.json" ) ) )
            {
                file.Write( json );   
            }

            // Create a temp directory to hold package contents in staging area
            // * Find out from `page` which Blocks need to be packaged up (recursively if ExportChildren box is checked)
            // * Grab asset folders for each Block's code file uniquely (Scripts, Assets, CSS folders)

            // Generate the `.nuspec` file with some default values (file names, etc)

            var manifest = new Manifest
            {
                Metadata = {
                    Id = packageDirectory.Name,
                    Version = "0.0.0",
                    Description = page.Description,
                    Authors = "PageExport"
                },
                Files = new List<ManifestFile>()
            };

            var webRootPath = HttpContext.Current.Server.MapPath( "~" );
            AddToManifest( manifest, packageDirectory.FullName, webRootPath, "export.json", SearchOption.TopDirectoryOnly );
            var blockTypes = new Dictionary<Guid, BlockType>();
            var uniqueDirectories = new Dictionary<string, string>();
            FindUniqueBlockTypesAndDirectories( page, isRecursive, blockTypes, uniqueDirectories );

            foreach ( var blockType in blockTypes.Values )
            {
                var sourcePath = HttpContext.Current.Server.MapPath( blockType.Path );
                var directory = sourcePath.Substring( 0, sourcePath.LastIndexOf( Path.DirectorySeparatorChar ) );
                var fileName = Path.GetFileNameWithoutExtension( sourcePath );
                var pattern = string.Format( "{0}.*", fileName );
                AddToManifest( manifest, directory, webRootPath, pattern, SearchOption.TopDirectoryOnly );
            }

            // Second pass through blocks. Determine which folders need to be added to the manifest
            foreach ( var dir in uniqueDirectories.Keys )
            {
                var sourcePath = HttpContext.Current.Server.MapPath( dir );

                // Are there any folders present named "CSS", "Scripts" or "Assets"?
                AddToManifest( manifest, Path.Combine(sourcePath, "CSS" ), webRootPath );
                AddToManifest( manifest, Path.Combine(sourcePath, "Scripts" ), webRootPath );
                AddToManifest( manifest, Path.Combine(sourcePath, "Assets" ), webRootPath );
            }

            var outputStream = new MemoryStream();
            using ( var manifestStream = new MemoryStream() )
            {
                manifest.Save( manifestStream );

                // Use NuGet API to generate the `.nupkg` file
                var packageBuilder = new PackageBuilder( manifestStream, packageDirectory.FullName );
                packageBuilder.Save( outputStream );
            }
            
            // TODO: Clean up staging area...
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

        private void AddToManifest( Manifest manifest, string directory, string webRootPath, 
            string filterPattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories )
        {
            if ( !Directory.Exists( directory ) )
            {
                return;
            }

            var files = from file in Directory.EnumerateFiles( directory, filterPattern, searchOption )
                        // Remove physical root blockTypeFilePath path (`C:\inetpub\...\`)
                        let pathSuffix = file.Substring( webRootPath.Length + 1 )
                        // Make location relative to ~/App_Data/PackageStaging/{PackageName}/{package.nuspec}
                        let source = string.Format( "../../../../{0}", pathSuffix )
                        let target = pathSuffix
                        select new ManifestFile
                            {
                                Source = source,
                                Target = target
                            };

            manifest.Files.AddRange( files );
        }
    }
}