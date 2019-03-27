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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using NuGet;

using Rock.Data;
using Rock.Model;

namespace Rock.Services.NuGet
{
    /// <summary>
    /// Facade class to provide interaction with NuGet internals
    /// </summary>
    public class PackageService
    {
        /// <summary>
        /// Gets or sets the error messages.
        /// </summary>
        /// <value>
        /// The error messages.
        /// </value>
        public List<string> ErrorMessages { get; private set; }

        /// <summary>
        /// Gets or sets the warning messages.
        /// </summary>
        /// <value>
        /// The warning messages.
        /// </value>
        public List<string> WarningMessages { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageService"/> class.
        /// </summary>
        public PackageService()
        {
            ErrorMessages = new List<string>();
            WarningMessages = new List<string>();
        }

        /// <summary>
        /// Exports the page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="isRecursive">if set to <c>true</c> [should export children].</param>
        /// <returns>a <see cref="T:System.IO.MemoryStream"/> of the exported page package.</returns>
        public MemoryStream ExportPage( Page page, bool isRecursive )
        {
            // Create a temp directory to hold package contents in staging area
            string packageId = Guid.NewGuid().ToString();
            var packageDirectory = CreatePackageDirectory( page.InternalName, packageId );
            var webRootPath = HttpContext.Current.Server.MapPath( "~" );

            // Create a manifest for this page export...
            Manifest manifest = new Manifest();
            manifest.Metadata = new ManifestMetadata();
            manifest.Metadata.Authors = "PageExport";
            manifest.Metadata.Version = "0.0";
            manifest.Metadata.Id = packageId;
            manifest.Metadata.Description = ( string.IsNullOrEmpty( page.Description ) ) ? "a page export" : page.Description;
            manifest.Files = new List<ManifestFile>();

            // Generate the JSON from model data and write it to disk, then add it to the manifest
            var json = GetJson( page, isRecursive );
            using ( var file = new StreamWriter( Path.Combine( packageDirectory.FullName, "export.json" ) ) )
            {
                file.Write( json );
            }
            AddToManifest( manifest, packageDirectory.FullName, webRootPath, "export.json", SearchOption.TopDirectoryOnly );

            // * Find out from `page` which Blocks need to be packaged up (recursively if ExportChildren box is checked)
            // * Grab asset folders for each Block's code file uniquely (Scripts, Assets, CSS folders)
            // * Add them to the package manifest
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

            // Second pass through blocks. Determine which folders (by convention) need to be added
            // to the package manifest.
            foreach ( var dir in uniqueDirectories.Keys )
            {
                var sourcePath = HttpContext.Current.Server.MapPath( dir );
                // Are there any folders present named "CSS", "Scripts" or "Assets"?
                AddToManifest( manifest, Path.Combine( sourcePath, "CSS" ), webRootPath );
                AddToManifest( manifest, Path.Combine( sourcePath, "Scripts" ), webRootPath );
                AddToManifest( manifest, Path.Combine( sourcePath, "Assets" ), webRootPath );
            }

            // Save the manifest as "pageexport.nuspec" in the temp folder
            string basePath = packageDirectory.FullName;
            string manifestPath = Path.Combine( basePath, "pageexport.nuspec" );
            using ( Stream fileStream = File.Create( manifestPath ) )
            {
                manifest.Save( fileStream );
            }

            // Create a NuGet package from the manifest and 'save' it to a memory stream for the consumer...
            // BTW - don't use anything older than nuget 2.1 due to Manifest bug (http://nuget.codeplex.com/workitem/2813)
            // which will cause the PackageBuilder constructor to fail due to <Files> vs <files> being in the manifest.
            PackageBuilder packageBuilder = new PackageBuilder( manifestPath, basePath, NullPropertyProvider.Instance, false );

            var outputStream = new MemoryStream();
            packageBuilder.Save( outputStream );

            // Clean up staging area
            packageDirectory.Delete( recursive: true );
            return outputStream;
        }

        /// <summary>
        /// Imports the page.
        /// </summary>
        /// <param name="uploadedPackage">Byte array of the uploaded package</param>
        /// <param name="fileName">File name of uploaded package</param>
        /// <param name="pageId">The Id of the Page to save new data underneath</param>
        /// <param name="siteId">The Id of the Site tha the Page is being imported into</param>
        public bool ImportPage( byte[] uploadedPackage, string fileName, int pageId, int siteId )
        {
            // Write .nupkg file to the PackageStaging folder...
            var path = Path.Combine( HttpContext.Current.Server.MapPath( "~/App_Data/PackageStaging" ), fileName );
            using ( var file = new FileStream( path, FileMode.Create ) )
            {
                file.Write( uploadedPackage, 0, uploadedPackage.Length );
            }

            var package = new ZipPackage( path );
            var packageFiles = package.GetFiles().ToList();
            var exportFile = packageFiles.FirstOrDefault( f => f.Path.Contains( "export.json" ) );
            Page page = null;

            // If export.json is present, deserialize data
            // * Are there any new BlockTypes to register? If so, save them first.
            // * Scrub out any `Id` and `Guid` fields that came over from export
            // * Save page data via PageService

            if ( exportFile != null )
            {
                string json;

                using ( var stream = exportFile.GetStream() )
                {
                    json = stream.ReadToEnd();
                }

                page = Page.FromJson( json );
            }

            // Validate package...
            // + Does it have any executable .dll files? Should those go to the bin folder, or into a plugins directory to be loaded via MEF?
            // - Does it have code or asset files that need to go on the file system? (Done)
            // - Does it have an export.json file? Should that be a requirement? (Done)
            // + Does it have any corresponding SQL, migrations, seed methods to run, etc.

            if ( page != null )
            {
                var rockContext = new RockContext();

                // Find new block types and save them prior to scrubbing data...
                var newBlockTypes = FindNewBlockTypes( page, new BlockTypeService( rockContext ).Queryable() ).ToList();
                rockContext.WrapTransaction( () =>
                    {
                        try
                        {
                            var blockTypeService = new BlockTypeService( rockContext );

                            foreach ( var blockType in newBlockTypes )
                            {
                                blockTypeService.Add( blockType );
                            }
                            rockContext.SaveChanges();

                            ValidateImportData( page, newBlockTypes );
                            SavePages( rockContext, page, newBlockTypes, pageId, siteId );
                            ExpandFiles( packageFiles );
                        }
                        catch ( Exception e )
                        {
                            ErrorMessages.Add( e.Message );
                        }
                    });

                // Clean up PackageStaging folder on successful import.
                var file = new FileInfo( path );
                file.Delete();
                return ErrorMessages.Count <= 0;
            }

            ErrorMessages.Add( "The export package uploaded does not appear to have any data associated with it." );
            return false;
        }

        /// <summary>
        /// Creates a unique directory for temporarily holding the page export package.
        /// </summary>
        /// <param name="pageName">Name of the page.</param>
        /// <param name="packageId">The unique package id.</param>
        /// <returns>a <see cref="T:System.IO.DirectoryInfo"/> of the new directory.</returns>
        private DirectoryInfo CreatePackageDirectory( string pageName, string packageId )
        {
            var name = Regex.Replace( pageName, @"[^A-Za-z0-9]", string.Empty );
            var rootPath = HttpContext.Current.Server.MapPath( "~/App_Data" );
            var packageName = string.Format( "{0}_{1}", name, packageId );
            return Directory.CreateDirectory( Path.Combine( rootPath, "PackageStaging", packageName ) );
        }

        /// <summary>
        /// Gets the json for this page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="isRecursive">if set to <c>true</c> [is recursive].</param>
        /// <returns></returns>
        private string GetJson( Page page, bool isRecursive )
        {
            string json;

            if ( isRecursive )
            {
                json = page.ToJson();
            }
            else
            {
                // make a deep copy, remove its child pages, and ToJson that...
                var pageCopy = page.Clone() as Page ?? new Page();
                pageCopy.Pages = new List<Page>();
                json = pageCopy.ToJson();
            }

            return json;
        }

        /// <summary>
        /// Finds the unique block types and directories for the given page and adds them to the <paramref name="blockTypes"/> and <paramref name="directories"/> dictionaries.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="isRecursive">if set to <c>true</c> [child pages are recursively searched too].</param>
        /// <param name="blockTypes">a Dictionary of BlockTypes.</param>
        /// <param name="directories">a Dictionary of directory names.</param>
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
                        directories.Add( directory, directory );
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

        /// <summary>
        /// Add the given directories files (matching the given file filter and search options)
        /// to the manifest.
        /// </summary>
        /// <param name="manifest">A NuGet Manifest</param>
        /// <param name="directory">the directory containing the file(s)</param>
        /// <param name="webRootPath">the physical path to the app's webroot</param>
        /// <param name="filterPattern"> A file filter pattern such as *.* or *.cs</param>
        /// <param name="searchOption">A <see cref="T:System.IO.SearchOption"/> search option to define the scope of the search</param>
        private void AddToManifest( Manifest manifest, string directory, string webRootPath,
            string filterPattern = "*.*", SearchOption searchOption = SearchOption.AllDirectories )
        {
            if ( !Directory.Exists( directory ) )
            {
                return;
            }

            // In our trivial case, the files we're adding need to have a target folder under the "content\"
            // folder and the source path suffix will be the relative path to the file's physical location.
            // ex: `Blocks\Foo\Foo.ascx` and `App_Data\PackageStaging\{PageName}_{Guid}\export.json`
            var files = from file in Directory.EnumerateFiles( directory, filterPattern, searchOption )
                        let pathSuffix = file.Substring( webRootPath.Length )
                        select new ManifestFile()
                        {
                            Source = Path.Combine( "..", "..", "..", pathSuffix ),
                            Target = Path.Combine( "content", pathSuffix )
                        };

            manifest.Files.AddRange( files );
        }

        /// <summary>
        /// Iterates recursively through all BlockTypes associated with a Page and its children and compares them with
        /// the list of BlockTypes that are currently installed.
        /// </summary>
        /// <param name="page">The page to interrogate</param>
        /// <param name="installedBlockTypes">The list of currently installed Blocks</param>
        /// <returns>A List&lt;BlockType&gt; of BlockTypes that are not currently installed.</returns>
        private IEnumerable<BlockType> FindNewBlockTypes( Page page, IEnumerable<BlockType> installedBlockTypes )
        {
            var newBlockTypes = new List<BlockType>();
            installedBlockTypes = installedBlockTypes.ToList();

            foreach ( var block in page.Blocks ?? new List<Block>() )
            {
                var blockType = block.BlockType;

                if ( installedBlockTypes.All( b => b.Path != blockType.Path ) && !newBlockTypes.Contains( blockType ) )
                {
                    // Scrub out the Id from any previous Rock installation
                    blockType.Id = 0;
                    newBlockTypes.Add( blockType );
                }
            }

            foreach ( var p in page.Pages ?? new List<Page>() )
            {
                newBlockTypes.AddRange( FindNewBlockTypes( p, installedBlockTypes ) );
            }

            return newBlockTypes;
        }

        /// <summary>
        /// Validates the import data.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="newBlockTypes">Collection of newly created BlockTypes</param>
        private void ValidateImportData( Page page, IEnumerable<BlockType> newBlockTypes )
        {
            ScrubIds( page );
            page.PageContexts.ToList().ForEach( ScrubIds );
            page.PageRoutes.ToList().ForEach( ScrubIds );
            // TODO: HtmlContent is no longer a property of Block. Need to engineer a way
            // for an entity to register a collection of things it needs to import and export.
            var blockTypes = newBlockTypes.ToList();

            foreach ( var block in page.Blocks ?? new List<Block>() )
            {
                var blockType = blockTypes.FirstOrDefault( bt => block.BlockType.Path == bt.Path );
                ScrubIds( block );
                block.PageId = 0;

                if ( blockType == null )
                {
                    // If we get here, we should be able to assume the blockType is already installed.
                    continue;
                }

                block.BlockTypeId = blockType.Id;
            }

            foreach ( var childPage in page.Pages )
            {
                ValidateImportData( childPage, blockTypes );
            }
        }

        /// <summary>
        /// Recursively saves Pages and associated Blocks, PageRoutes and PageContexts.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="page">The current Page to save</param>
        /// <param name="newBlockTypes">List of BlockTypes not currently installed</param>
        /// <param name="parentPageId">Id of the the current Page's parent</param>
        /// <param name="siteId">Id of the site the current Page is being imported into</param>
        private void SavePages( RockContext rockContext, Page page, IEnumerable<BlockType> newBlockTypes, int parentPageId, int siteId )
        {
            rockContext = rockContext ?? new RockContext();

            // find layout
            var layoutService = new LayoutService( rockContext );
            Layout layout = new Layout();
            if ( page.Layout != null )
            {
                layout = layoutService.GetBySiteId( siteId ).Where( l => l.Name == page.Layout.Name && l.FileName == page.Layout.FileName ).FirstOrDefault();
                if ( layout == null )
                {
                    layout = new Layout();
                    layout.FileName = page.Layout.FileName;
                    layout.Name = page.Layout.Name;
                    layout.SiteId = siteId;
                    layoutService.Add( layout );
                    rockContext.SaveChanges();
                }
            }
            else
            {
                layout = layoutService.GetBySiteId( siteId ).Where( l => l.Name.Contains( "Full" ) || l.Name.Contains( "Home" ) ).First();
            }
            int layoutId = layout.Id;

            // Force shallow copies on entities so save operations are more atomic and don't get hosed
            // by nested object references.
            var pg = page.Clone(deepCopy: false);
            var blockTypes = newBlockTypes.ToList();
            pg.ParentPageId = parentPageId;
            pg.LayoutId = layoutId;

            var pageService = new PageService( rockContext );
            pageService.Add( pg );
            rockContext.SaveChanges();

            var blockService = new BlockService( rockContext );

            foreach ( var block in page.Blocks ?? new List<Block>() )
            {
                var blockType = blockTypes.FirstOrDefault( bt => block.BlockType.Path == bt.Path );
                var b = block.Clone( deepCopy: false );
                b.PageId = pg.Id;

                if ( blockType != null )
                {
                    b.BlockTypeId = blockType.Id;
                }

                blockService.Add( b );
            }
            rockContext.SaveChanges();

            var pageRouteService = new PageRouteService( rockContext );

            foreach ( var pageRoute in page.PageRoutes ?? new List<PageRoute>() )
            {
                var pr = pageRoute.Clone(deepCopy: false);
                pr.PageId = pg.Id;
                pageRouteService.Add( pr );
            }
            rockContext.SaveChanges();

            var pageContextService = new PageContextService( rockContext );

            foreach ( var pageContext in page.PageContexts ?? new List<PageContext>() )
            {
                var pc = pageContext.Clone(deepCopy: false);
                pc.PageId = pg.Id;
                pageContextService.Add( pc );
            }
            rockContext.SaveChanges();

            foreach ( var p in page.Pages ?? new List<Page>() )
            {
                SavePages( rockContext, p, blockTypes, pg.Id, siteId );
            }
        }

        private static void ScrubIds( IEntity entity )
        {
            entity.Id = 0;
            entity.Guid = Guid.NewGuid();
        }

        /// <summary>
        /// Expands the files.
        /// </summary>
        /// <param name="packageFiles">The package files.</param>
        private void ExpandFiles( IEnumerable<IPackageFile> packageFiles )
        {
            // Remove export.json file from the list of files to be unzipped
            var filesToUnzip = packageFiles.Where( f => !f.Path.Contains( "export.json" ) ).ToList();
            var blockTypeService = new BlockTypeService( new RockContext() );
            var installedBlockTypes = blockTypeService.Queryable();
            var webRoot = HttpContext.Current.Server.MapPath( "~" );

            // Compare the packages files with currently installed block types, removing anything that already exists
            foreach ( var blockType in installedBlockTypes )
            {
                var blockFileName = blockType.Path.Substring( blockType.Path.LastIndexOf( "/", StringComparison.InvariantCultureIgnoreCase ) );
                blockFileName = blockFileName.Replace( '/', Path.DirectorySeparatorChar );
                filesToUnzip.RemoveAll( f => f.Path.Contains( blockFileName ) );
            }

            foreach ( var packageFile in filesToUnzip )
            {
                var path = Path.Combine( webRoot, packageFile.EffectivePath );
                var file = new FileInfo( path );

                // Err on the side of not being destructive for now. Consider refactoring to give user a choice
                // on whether or not to overwrite these files.
                if ( file.Exists )
                {
                    WarningMessages.Add( string.Format( "Skipping '{0}', found duplicate file at '{1}'.", file.Name, path ) );
                    continue;
                }
                
                // Write each file out to disk
                using ( var fileStream = new FileStream( path, FileMode.Create ) )
                {
                    var stream = packageFile.GetStream();
                    var bytes = stream.ReadAllBytes();
                    fileStream.Write( bytes, 0, bytes.Length );
                    stream.Close();
                }
            }
        }
    }
}