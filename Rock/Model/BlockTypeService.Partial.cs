//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for <see cref="Rock.Model.BlockType"/> entity type objects, and extends the functionality of <see cref="Rock.Model.Service"/>
    /// </summary>
    public partial class BlockTypeService 
    {

        /// <summary>
        /// Gets a <see cref="Rock.Model.BlockType"/> by it's Guid.
        /// </summary>
        /// <param name="guid"><see cref="System.Guid"/> identifier  filter to search by.</param>
        /// <returns>The <see cref="Rock.BlockType"/> that has a Guid that matches the provided value, if none are found returns null. </returns>
        public BlockType GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }


        /// <summary>
        /// Gets a collection of <see cref="Rock.Model.BlockType"/> entities by Name
        /// </summary>
        /// <param name="name">A <see cref="System.String"/> containing the Name filter to search for. </param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.BlockType"/> entities who's Name property matches the search criteria.</returns>
        public IEnumerable<BlockType> GetByName( string name )
        {
            return Repository.Find( t => t.Name == name );
        }


        /// <summary>
        /// Gets a collection of <see cref="Rock.Model.BlockType" /> entities by path.
        /// </summary>
        /// <param name="path">A <see cref="System.String"/> containing the path to search for.</param>
        /// <returns>A collection of <see cref="Rock.Model.BlockType"/> entities who's Path property matches the search criteria.</returns>
        public IEnumerable<BlockType> GetByPath( string path )
        {
            return Repository.Find( t => t.Path == path );
        }

        /// <summary>
        /// Registers any block types that are not currently registered in RockChMS.
        /// </summary>
        /// <param name="physWebAppPath">A <see cref="System.String"/> containing the physical path to RockChMS on the server.</param>
        /// <param name="page">The <see cref="System.Web.UI.Page"/>.</param>
        /// <param name="currentPersonId">A <see cref="System.Int32"/> that contains the Id of the currently logged on <see cref="Rock.Model.Person"/>.</param>
        public void RegisterBlockTypes( string physWebAppPath, System.Web.UI.Page page, int? currentPersonId )
        {
            // Dictionary for block types.  Key is path, value is friendly name
            var list = new Dictionary<string, string>();

            // Find all the blocks in the Blocks folder...
            FindAllBlocksInPath( physWebAppPath, list, "Blocks" );

            // Now do the exact same thing for the Plugins folder...
            FindAllBlocksInPath( physWebAppPath, list, "Plugins" );

            // Get a list of the BlockTypes already registered (via the path)
            var registered = from r in Repository.GetAll() select r.Path;

            // for each unregistered BlockType
            foreach ( string path in list.Keys.Except( registered, StringComparer.CurrentCultureIgnoreCase ) )
            {
                // Attempt to load the control
                System.Web.UI.Control control = page.LoadControl( path );
                if ( control is Rock.Web.UI.RockBlock )
                {
                    // Parse the relative path to get the name
                    var nameParts = list[path].Split( '/' );
                    for ( int i = 0; i < nameParts.Length; i++ )
                    {
                        if (i == nameParts.Length - 1)
                        {
                            nameParts[i] = Path.GetFileNameWithoutExtension(nameParts[i]);
                        }
                        nameParts[i] = nameParts[i].SplitCase();
                    }

                    // Create new BlockType record and save it
                    BlockType blockType = new BlockType();
                    blockType.Path = path;
                    blockType.Guid = new Guid();

                    blockType.Name = string.Join( " - ", nameParts );

                    // limit name to 100 chars so it fits into the .Name column
                    if (blockType.Name.Length > 100)
                    {
                        blockType.Name = blockType.Name.Substring( 0, 97 ) + "...";
                    }
                    
                    blockType.Description = Rock.Reflection.GetDescription( control.GetType() ) ?? string.Empty;

                    this.Add( blockType, currentPersonId );
                    this.Save( blockType, currentPersonId );
                }
            }
        }

        /// <summary>
        /// Finds all the <see cref="Rock.Model.BlockType">BlockTypes</see> within a given path.
        /// </summary>
        /// <param name="physWebAppPath">The physical web application path.</param>
        /// <param name="list">A <see cref="System.Collection.Generic.Dictionary"/> containing all the <see cref="BlockTypes">BlockTypes</see> that have been found.</param>
        /// <param name="folder">A <see cref="System.String"/> containing the subdirectory to to search through.</param>
        private static void FindAllBlocksInPath( string physWebAppPath, Dictionary<string, string> list, string folder )
        {
            // Determine the physical path (it will be something like "C:\blahblahblah\Blocks\" or "C:\blahblahblah\Plugins\")
            string physicalPath = string.Format( @"{0}{1}{2}\", physWebAppPath, ( physWebAppPath.EndsWith( @"\" ) ) ? "" : @"\", folder );
            
            // Determine the virtual path (it will be either "~/Blocks/" or "~/Plugins/")
            string virtualPath = string.Format( "~/{0}/", folder );

            // search for all blocks under the physical path 
            DirectoryInfo di = new DirectoryInfo( physicalPath );
            if ( di.Exists )
            {
                var allBlockNames = di.GetFiles( "*.ascx", SearchOption.AllDirectories );
                string fileName = string.Empty;

                // Convert them to virtual file/path: ~/<folder>/foo/bar.ascx
                for ( int i = 0; i < allBlockNames.Length; i++ )
                {
                    fileName = allBlockNames[i].FullName.Replace( physicalPath, virtualPath ).Replace( @"\", "/" );
                    list.Add( fileName, fileName.Replace( virtualPath, string.Empty ) );
                }
            }
        }
    }
}
