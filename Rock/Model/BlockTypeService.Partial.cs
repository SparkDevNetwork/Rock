// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.BlockType"/> objects.
    /// </summary>
    public partial class BlockTypeService 
    {

        /// <summary>
        /// Gets a <see cref="Rock.Model.BlockType"/> by it's Guid.
        /// </summary>
        /// <param name="guid"><see cref="System.Guid"/> identifier  filter to search by.</param>
        /// <returns>The <see cref="Rock.Model.BlockType"/> that has a Guid that matches the provided value, if none are found returns null. </returns>
        public BlockType GetByGuid( Guid guid )
        {
            return Queryable().FirstOrDefault( t => t.Guid == guid );
        }


        /// <summary>
        /// Gets a collection of <see cref="Rock.Model.BlockType"/> entities by Name
        /// </summary>
        /// <param name="name">A <see cref="System.String"/> containing the Name filter to search for. </param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.BlockType"/> entities who's Name property matches the search criteria.</returns>
        public IQueryable<BlockType> GetByName( string name )
        {
            return Queryable().Where( t => t.Name == name );
        }


        /// <summary>
        /// Gets a collection of <see cref="Rock.Model.BlockType" /> entities by path.
        /// </summary>
        /// <param name="path">A <see cref="System.String"/> containing the path to search for.</param>
        /// <returns>A collection of <see cref="Rock.Model.BlockType"/> entities who's Path property matches the search criteria.</returns>
        public IQueryable<BlockType> GetByPath( string path )
        {
            return Queryable().Where( t => t.Path == path );
        }

        /// <summary>
        /// Registers any block types that are not currently registered in Rock.
        /// </summary>
        /// <param name="physWebAppPath">A <see cref="System.String" /> containing the physical path to Rock on the server.</param>
        /// <param name="page">The <see cref="System.Web.UI.Page" />.</param>
        /// <param name="refreshAll">if set to <c>true</c> will refresh name, category, and description for all block types (not just the new ones)</param>
        public static void RegisterBlockTypes( string physWebAppPath, System.Web.UI.Page page, bool refreshAll = false)
        {
            // Dictionary for block types.  Key is path, value is friendly name
            var list = new Dictionary<string, string>();

            // Find all the blocks in the Blocks folder...
            FindAllBlocksInPath( physWebAppPath, list, "Blocks" );

            // Now do the exact same thing for the Plugins folder...
            FindAllBlocksInPath( physWebAppPath, list, "Plugins" );

            // Get a list of the BlockTypes already registered (via the path)
            var rockContext = new RockContext();
            var blockTypeService = new BlockTypeService( rockContext );
            var registered = blockTypeService.Queryable().ToList();

            // for each BlockType
            foreach ( string path in list.Keys)
            {
                if ( refreshAll || !registered.Any( b => b.Path.Equals( path, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    // Attempt to load the control
                    try
                    {
                        System.Web.UI.Control control = page.LoadControl( path );

                        if ( control is Rock.Web.UI.RockBlock )
                        {
                            var blockType = registered.FirstOrDefault( b => b.Path.Equals( path, StringComparison.OrdinalIgnoreCase ) );
                            if ( blockType == null )
                            {
                                // Create new BlockType record and save it
                                blockType = new BlockType();
                                blockType.Path = path;
                                blockTypeService.Add( blockType );
                            }

                            Type controlType = control.GetType();

                            // Update Name, Category, and Description based on block's attribute definitions
                            blockType.Name = Rock.Reflection.GetDisplayName( controlType ) ?? string.Empty;
                            if ( string.IsNullOrWhiteSpace( blockType.Name ) )
                            {
                                // Parse the relative path to get the name
                                var nameParts = list[path].Split( '/' );
                                for ( int i = 0; i < nameParts.Length; i++ )
                                {
                                    if ( i == nameParts.Length - 1 )
                                    {
                                        nameParts[i] = Path.GetFileNameWithoutExtension( nameParts[i] );
                                    }
                                    nameParts[i] = nameParts[i].SplitCase();
                                }
                                blockType.Name = string.Join( " > ", nameParts );
                            }
                            if ( blockType.Name.Length > 100 )
                            {
                                blockType.Name = blockType.Name.Truncate( 100 );
                            }
                            blockType.Category = Rock.Reflection.GetCategory( controlType ) ?? string.Empty;
                            blockType.Description = Rock.Reflection.GetDescription( controlType ) ?? string.Empty;
                        }
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( string.Format("Problem processing block with path '{0}'.", path ), ex ), null );
                    }
                }
            }

            rockContext.SaveChanges();
        
        }

        /// <summary>
        /// Finds all the <see cref="Rock.Model.BlockType">BlockTypes</see> within a given path.
        /// </summary>
        /// <param name="physWebAppPath">The physical web application path.</param>
        /// <param name="list">A <see cref="System.Collections.Generic.Dictionary{String, String}"/> containing all the <see cref="Rock.Model.BlockType">BlockTypes</see> that have been found.</param>
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

        /// <summary>
        /// Gets the Guid for the BlockType that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = Rock.Web.Cache.BlockTypeCache.Read( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }
    }
}
