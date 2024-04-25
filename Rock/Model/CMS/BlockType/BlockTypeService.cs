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
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Rock.Blocks;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.BlockType"/> objects.
    /// </summary>
    public partial class BlockTypeService
    {
        /// <summary>
        /// Gets a <see cref="Rock.Model.BlockType"/> by its Guid.
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
        /// Lock obj to make sure that we aren't compiling more than one BlockType at a time. This prevents
        /// block types from spending time compiling even though another thread might have started compiling it.
        /// </summary>
        private static readonly object VerifyBlockTypeInstancePropertiesLockObj = new object();

        /// <summary>
        /// Verifies the block type instance properties to make sure they are compiled and have the attributes updated.
        /// </summary>
        /// <param name="blockTypesIdToVerify">The block types identifier to verify.</param>
        public static void VerifyBlockTypeInstanceProperties( int[] blockTypesIdToVerify )
        {
            CancellationToken cancellationToken;
            VerifyBlockTypeInstanceProperties( blockTypesIdToVerify, cancellationToken );
        }

        /// <summary>
        /// Verifies the block type instance properties to make sure they are compiled and have the attributes updated,
        /// with an option to cancel the loop.
        /// </summary>
        public static void VerifyBlockTypeInstanceProperties( int[] blockTypesIdToVerify, CancellationToken cancellationToken )
        {
            if ( blockTypesIdToVerify.Length == 0 )
            {
                return;
            }

            foreach ( int blockTypeId in blockTypesIdToVerify )
            {
                if ( cancellationToken.IsCancellationRequested == true )
                {
                    return;
                }

                try
                {
                    /*
                        02/09/2024 - JSC

                        During Rock startup this method is called to pre-compile
                        all the used block types so that the first page view
                        is as fast as possible. The number of blocks has grown
                        since this was first introduced in version 12 and we've
                        looked for ways to improve performance; since the compilation
                        of 540+ block types takes between 35 and 75 seconds when first
                        starting up. We tried using multiple tasks to split the work,
                        but that just pushed the delay to other startup jobs.
                        The most promising result was creating a separate method used
                        only during startup and removing the locking in that method
                        while we iterated over the array directly. The gains we saw
                        were between 5 and 10 percent and ultimately we decided to
                        leave process as it is with the knowledge that as we migrate
                        away from web forms this compilation time will decrease.                         

                        Reason: Page Zone Editor loads slowly after Rock Restart
                    */
                    /* 
                        09/04/2020 - MDP

                        Notice that we call BlockTypeCache.Get every time we need data from it.
                        We do this because the BlockTypeCache get easily get stale due to other threads.
                    */
                    if ( BlockTypeCache.Get( blockTypeId )?.IsInstancePropertiesVerified == false )
                    {
                        // make sure that only one thread is trying to compile block types and attributes so that we don't get collisions and unneeded compiler overhead
                        lock ( VerifyBlockTypeInstancePropertiesLockObj )
                        {
                            if ( BlockTypeCache.Get( blockTypeId )?.IsInstancePropertiesVerified == false )
                            {
                                using ( var rockContext = new RockContext() )
                                {
                                    var blockTypeCache = BlockTypeCache.Get( blockTypeId );
                                    Type blockCompiledType = blockTypeCache.GetCompiledType();

                                    bool attributesUpdated = RockBlock.CreateAttributes( rockContext, blockCompiledType, blockTypeId );
                                    BlockTypeCache.Get( blockTypeId )?.MarkInstancePropertiesVerified( true );
                                }
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    // ignore if the block couldn't be compiled, it'll get logged and shown when the page tries to load the block into the page
                    Debug.WriteLine( ex );
                }
            }
        }

        /// <summary>
        /// Registers any entity-based block types that are not currently registered in Rock.
        /// </summary>
        /// <param name="refreshAll">if set to <c>true</c> will refresh name, category, and description for all block types (not just the new ones)</param>
        private static void RegisterEntityBlockTypes( bool refreshAll = false )
        {
            var rockBlockTypes = Reflection.FindTypes( typeof( Blocks.IRockBlockType ) );

            List<Type> registeredTypes;
            using ( var rockContext = new RockContext() )
            {
                registeredTypes = new BlockTypeService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( b => b.EntityTypeId.HasValue && !string.IsNullOrEmpty( b.EntityType.AssemblyName ) )
                    .ToList()
                    .Select( b => Type.GetType( b.EntityType.AssemblyName, false ) )
                    .Where( b => b != null )
                    .ToList();
            }

            // Get the Block Entity Type
            int? blockEntityTypeId = EntityTypeCache.Get( typeof( Block ) ).Id;

            // for each BlockType
            foreach ( var type in rockBlockTypes.Values )
            {
                if ( refreshAll || !registeredTypes.Any( t => t == type ) )
                {
                    // Attempt to load the control
                    try
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var entityTypeId = EntityTypeCache.Get( type, true, rockContext ).Id;
                            var blockTypeService = new BlockTypeService( rockContext );
                            var blockType = blockTypeService.Queryable()
                                .FirstOrDefault( b => b.EntityTypeId == entityTypeId );

                            if ( blockType == null )
                            {
                                // Create new BlockType record and save it
                                blockType = new BlockType();
                                blockType.EntityTypeId = entityTypeId;
                                blockTypeService.Add( blockType );
                            }

                            // Update Name, Category, and Description based on block's attribute definitions
                            blockType.Name = Reflection.GetDisplayName( type ) ?? string.Empty;

                            var blockTypeGuidFromAttribute = type.GetCustomAttribute<Rock.SystemGuid.BlockTypeGuidAttribute>( inherit: false )?.Guid;
                            if ( blockTypeGuidFromAttribute != null && blockType.Guid != blockTypeGuidFromAttribute.Value )
                            {
                                blockType.Guid = blockTypeGuidFromAttribute.Value;
                            }

                            if ( string.IsNullOrWhiteSpace( blockType.Name ) )
                            {
                                blockType.Name = type.FullName;
                            }

                            if ( blockType.Name.Length > 100 )
                            {
                                blockType.Name = blockType.Name.Truncate( 100 );
                            }

                            blockType.Category = Rock.Reflection.GetCategory( type ) ?? string.Empty;
                            blockType.Description = Rock.Reflection.GetDescription( type ) ?? string.Empty;

                            rockContext.SaveChanges();

                            // Update the attributes used by the block
                            Rock.Attribute.Helper.UpdateAttributes( type, blockEntityTypeId, "BlockTypeId", blockType.Id.ToString(), rockContext );
                        }
                    }
                    catch ( Exception thrownException )
                    {
                        // if the exception was due to a duplicate Guid, throw a duplicateGuidException. That'll make it easier to troubleshoot.
                        var duplicateGuidException = Rock.SystemGuid.DuplicateSystemGuidException.CatchDuplicateSystemGuidException( thrownException, $"RegisterEntityBlockTypes failed for {type.FullName}" );
                        if ( duplicateGuidException != null )
                        {
                            Debug.WriteLine( duplicateGuidException.Message );
                            ExceptionLogService.LogException( duplicateGuidException );
                        }
                        else
                        {
                            Debug.WriteLine( $"RegisterEntityBlockTypes failed for {type.FullName} with exception: {thrownException.Message}" );
                            ExceptionLogService.LogException( new Exception( string.Format( "Problem processing block with path '{0}'.", type.FullName ), thrownException ), null );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registers any block types that are not currently registered in Rock.
        /// </summary>
        /// <param name="physWebAppPath">A <see cref="System.String" /> containing the physical path to Rock on the server.</param>
        /// <param name="page">The <see cref="System.Web.UI.Page" />.</param>
        /// <param name="refreshAll">if set to <c>true</c> will refresh name, category, and description for all block types (not just the new ones)</param>
        public static void RegisterBlockTypes( string physWebAppPath, System.Web.UI.Page page, bool refreshAll = false )
        {
            // Dictionary for block types.  Key is path, value is friendly name
            var list = new Dictionary<string, string>();

            RegisterEntityBlockTypes( refreshAll );

            // Find all the blocks in the Blocks folder...
            FindAllBlocksInPath( physWebAppPath, list, "Blocks" );

            // Now do the exact same thing for the Plugins folder...
            FindAllBlocksInPath( physWebAppPath, list, "Plugins" );

            // Get a list of the BlockTypes already registered (via the path)
            List<string> registeredPaths;
            if ( refreshAll )
            {
                FlushRegistrationCache();
                registeredPaths = new List<string>();
            }
            else
            {
                using ( var rockContext = new RockContext() )
                {
                    registeredPaths = new BlockTypeService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( b => !string.IsNullOrEmpty( b.Path ) )
                        .Select( b => b.Path )
                        .ToList();
                }
            }

            // Get the Block Entity Type
            int? blockEntityTypeId = EntityTypeCache.Get( typeof( Block ) ).Id;

            // for each BlockType
            foreach ( string path in list.Keys )
            {
                // If the block has been previously processed or successfully registered, ignore it. 
                if ( _processedBlockPaths.Any( b => b.Equals( path, StringComparison.OrdinalIgnoreCase ) )
                     || registeredPaths.Any( b => b.Equals( path, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    continue;
                }

                // Store the block path in the list of processed blocks to avoid re-processing if the registration fails.
                lock ( _processedBlockPathsLock )
                {
                    _processedBlockPaths.Add( path );
                }

                // Attempt to load the control
                try
                {
                    var blockCompiledType = System.Web.Compilation.BuildManager.GetCompiledType( path );
                    if ( blockCompiledType != null && typeof( Web.UI.RockBlock ).IsAssignableFrom( blockCompiledType ) )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var blockTypeService = new BlockTypeService( rockContext );
                            var blockType = blockTypeService.Queryable()
                                .FirstOrDefault( b => b.Path == path );
                            if ( blockType == null )
                            {
                                // Create new BlockType record and save it
                                blockType = new BlockType();
                                blockType.Path = path;
                                blockTypeService.Add( blockType );
                            }

                            Type controlType = blockCompiledType;

                            // Update Name, Category, and Description based on block's attribute definitions
                            blockType.Name = Reflection.GetDisplayName( controlType ) ?? string.Empty;
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

                            var blockTypeGuidFromAttribute = blockCompiledType.GetCustomAttribute<Rock.SystemGuid.BlockTypeGuidAttribute>( inherit: false )?.Guid;
                            if ( blockTypeGuidFromAttribute != null && blockType.Guid != blockTypeGuidFromAttribute.Value )
                            {
                                blockType.Guid = blockTypeGuidFromAttribute.Value;
                            }

                            rockContext.SaveChanges();

                            // Update the attributes used by the block
                            Rock.Attribute.Helper.UpdateAttributes( controlType, blockEntityTypeId, "BlockTypeId", blockType.Id.ToString(), rockContext );
                        }
                    }
                }
                catch ( Exception thrownException )
                {
                    // if the exception was due to a duplicate Guid, log a duplicateGuidException. That'll make it easier to troubleshoot.
                    var duplicateGuidException = Rock.SystemGuid.DuplicateSystemGuidException.CatchDuplicateSystemGuidException( thrownException, $"RegisterBlockTypes failed for {path}" );
                    if ( duplicateGuidException != null )
                    {
                        System.Diagnostics.Debug.WriteLine( duplicateGuidException.Message );
                        ExceptionLogService.LogException( duplicateGuidException );
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine( $"RegisterBlockTypes failed for {path} with exception: {thrownException.Message}" );
                        ExceptionLogService.LogException( new Exception( string.Format( "Problem processing block with path '{0}'.", path ), thrownException ), null );
                    }
                }
            }
        }

        /// <summary>
        /// Get the list of BlockTypes that may be added to the Page Zone for the given site type
        /// </summary>
        /// <param name="siteType">The site type of the blocks to be added.</param>
        /// <returns></returns>
        public static List<BlockTypeCache> BlockTypesToDisplay( SiteType siteType )
        {
            return BlockTypeCache.All()
                .Where( bt =>
                {
                    var type = bt.GetCompiledType();
                    if ( siteType == SiteType.Web )
                    {
                        if ( typeof( RockBlock ).IsAssignableFrom( type ) )
                        {
                            return true;
                        }

                        if ( typeof( RockBlockType ).IsAssignableFrom( type ) )
                        {
                            // if no site type is specified, then it likely is an obsidian block which is yet to be released.
                            // So show it only if it is the develop environment.
                            if ( type.GetCustomAttribute<SupportedSiteTypesAttribute>() == null )
                            {
                                return System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment;
                            }
                            return type.GetCustomAttribute<SupportedSiteTypesAttribute>()?.SiteTypes.Contains( siteType ) == true;
                        }

                        // Failsafe for any blocks that implement this directly.
                        return typeof( IRockObsidianBlockType ).IsAssignableFrom( type );
                    }
                    else if ( siteType == SiteType.Mobile )
                    {
                        if ( typeof( RockBlockType ).IsAssignableFrom( type ) )
                        {
                            return type.GetCustomAttribute<SupportedSiteTypesAttribute>()?.SiteTypes.Contains( siteType ) == true;
                        }

                        // Failsafe for any blocks that implement this directly.
                        return typeof( IRockMobileBlockType ).IsAssignableFrom( type );
                    }
                    return false;
                } )
                .ToList();
        }

        // Stores the list of block type files that have been processed in this application session.
        private static List<string> _processedBlockPaths = new List<string>();
        private static readonly object _processedBlockPathsLock = new object();

        /// <summary>
        /// Flushes the cache used to track block registrations.
        /// </summary>
        public static void FlushRegistrationCache()
        {
            lock ( _processedBlockPathsLock )
            {
                _processedBlockPaths = new List<string>();
            }
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
            var cacheItem = BlockTypeCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }

        /// <summary>
        /// Deletes the specified item.  Will try to determine current person
        /// alias from HttpContext.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public override bool Delete( BlockType item )
        {
            // block has a cascading delete on BlockType, but lets delete it manually so that the BlockCache gets updated correctly
            var blockService = new BlockService( this.Context as RockContext );
            var blocks = blockService.Queryable().Where( a => a.BlockTypeId == item.Id ).ToList();
            foreach ( var block in blocks )
            {
                blockService.Delete( block );
            }

            return base.Delete( item );
        }
    }
}
