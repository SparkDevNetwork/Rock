//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a block type that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class BlockTypeCache : CachedModel<BlockType>
    {
        #region Constructors

        private BlockTypeCache()
        {
        }
        
        private BlockTypeCache( BlockType blockType )
        {
            CopyFromModel( blockType );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Rock.Attribute.TextFieldAttribute" /> attributes have been
        /// verified for the block type.  If not, Rock will create and/or update the attributes associated with the block.
        /// </summary>
        /// <value>
        /// <c>true</c> if attributes have already been verified; otherwise, <c>false</c>.
        /// </value>
        public bool IsInstancePropertiesVerified { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether [checked additional security actions].
        /// </summary>
        /// <value>
        /// <c>true</c> if [checked additional security actions]; otherwise, <c>false</c>.
        /// </value>
        public bool CheckedAdditionalSecurityActions { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is BlockType )
            {
                var blockType = (BlockType)model;
                this.IsSystem = blockType.IsSystem;
                this.Path = blockType.Path;
                this.Name = blockType.Name;
                this.Description = blockType.Description;

                this.IsInstancePropertiesVerified = false;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:BlockType:{0}", id );
        }

        /// <summary>
        /// Returns Block Type object from cache.  If block does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static BlockTypeCache Read( int id )
        {
            string cacheKey = BlockTypeCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            BlockTypeCache blockType = cache[cacheKey] as BlockTypeCache;

            if ( blockType != null )
            {
                return blockType;
            }
            else
            {
                var blockTypeService = new BlockTypeService();
                var blockTypeModel = blockTypeService.Get( id );
                if ( blockTypeModel != null )
                {
                    blockTypeModel.LoadAttributes();
                    blockType = new BlockTypeCache( blockTypeModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, blockType, cachePolicy );
                    cache.Set( blockType.Guid.ToString(), blockType.Id, cachePolicy );
                    AddChangeMonitor( cachePolicy, blockType.Path );

                    return blockType;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static BlockTypeCache Read( Guid guid )
        {
            ObjectCache cache = MemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            if ( cacheObj != null )
            {
                return Read( (int)cacheObj );
            }
            else
            {
                var blockTypeService = new BlockTypeService();
                var blockTypeModel = blockTypeService.Get( guid );
                if ( blockTypeModel != null )
                {
                    blockTypeModel.LoadAttributes();
                    var blockType = new BlockTypeCache( blockTypeModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( BlockTypeCache.CacheKey( blockType.Id ), blockType, cachePolicy );
                    cache.Set( blockType.Guid.ToString(), blockType.Id, cachePolicy );
                    AddChangeMonitor( cachePolicy, blockType.Path );

                    return blockType;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads the specified block type model.
        /// </summary>
        /// <param name="blockTypeModel">The block type model.</param>
        /// <returns></returns>
        public static BlockTypeCache Read( BlockType blockTypeModel )
        {
            string cacheKey = BlockTypeCache.CacheKey( blockTypeModel.Id );

            ObjectCache cache = MemoryCache.Default;
            BlockTypeCache blockType = cache[cacheKey] as BlockTypeCache;

            if ( blockType != null )
            {
                return blockType;
            }
            else
            {
                blockType = new BlockTypeCache( blockTypeModel );

                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, blockType, cachePolicy );
                cache.Set( blockType.Guid.ToString(), blockType.Id, cachePolicy );
                AddChangeMonitor( cachePolicy, blockType.Path );

                return blockType;
            }
        }

        private static void AddChangeMonitor(CacheItemPolicy cacheItemPolicy, string filePath )
        {
            // Block Type cache expiration monitors the actual block on the file system so that it is flushed from 
            // memory anytime the file contents change.  This is to force the cmsPage object to revalidate any
            // BlockPropery attributes that may have been added or modified
            string physicalPath = System.Web.HttpContext.Current.Request.MapPath( filePath );
            List<string> filePaths = new List<string>();
            filePaths.Add( physicalPath );
            filePaths.Add( physicalPath + ".cs" );

            cacheItemPolicy.ChangeMonitors.Add( new HostFileChangeMonitor( filePaths ) );
        }

        /// <summary>
        /// Removes block type from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( BlockTypeCache.CacheKey( id ) );
        }

        #endregion
    }
}