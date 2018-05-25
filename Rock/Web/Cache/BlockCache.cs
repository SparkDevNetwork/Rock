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

using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a block that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheBlock instead" )]
    public class BlockCache : CachedModel<Block>
    {
        #region Constructors

        private BlockCache()
        {
        }

        private BlockCache( CacheBlock cacheBlock )
        {
            CopyFromNewCache( cacheBlock );
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
        /// Gets or sets the Id of the <see cref="Rock.Model.Page"/> that this Block is implemented on. This property will only be populated
        /// if the Block is implemented on a <see cref="Rock.Model.Page"/>.
        /// Blocks that have a specific PageId will only be shown in the specified Page
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Page"/> that this Block is implemented on.  This value will be null if this Block is implemented 
        /// as part of a <see cref="Rock.Model.Layout"/> or <see cref="Rock.Model.Site"/>.
        /// </value>
        public int? PageId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Layout"/> that this Block is implemented on. This property will only be populated
        /// if the Block is implemented on a <see cref="Rock.Model.Layout"/>.
        /// Blocks that have a specific LayoutId will be shown on all pages on a site that have the specified LayoutId
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Layout"/> that this Block is implemented on.  This value will be null if this Block is implemented 
        /// as part of a <see cref="Rock.Model.Page"/> or <see cref="Rock.Model.Site"/>.
        /// </value>
        public int? LayoutId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Site"/> that this Block is implemented on. This property will only be populated
        /// if the Block is implemented on a <see cref="Rock.Model.Site"/>.
        /// Blocks that have a specific SiteId will be shown on all pages on a site
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the Id of the <see cref="Rock.Model.Site"/> that this Block is implemented on.  This value will be null if this Block is implemented 
        /// as part of a <see cref="Rock.Model.Page"/> or <see cref="Rock.Model.Layout"/> .
        /// </value>
        public int? SiteId { get; set; }

        /// <summary>
        /// Gets or sets the block type id.
        /// </summary>
        /// <value>
        /// The block type id.
        /// </value>
        public int BlockTypeId { get; set; }

        /// <summary>
        /// Gets or sets the zone.
        /// </summary>
        /// <value>
        /// The zone.
        /// </value>
        public string Zone { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets the pre HTML.
        /// </summary>
        /// <value>
        /// The pre HTML.
        /// </value>
        public string PreHtml { get; set; }

        /// <summary>
        /// Gets or sets the post HTML.
        /// </summary>
        /// <value>
        /// The post HTML.
        /// </value>
        public string PostHtml { get; set; }

        /// <summary>
        /// Gets or sets the duration of the output cache.
        /// </summary>
        /// <value>
        /// The duration of the output cache.
        /// </value>
        public int OutputCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the Page that this Block is implemented on. This 
        /// property will be null if this Block is being implemented on as part of a Layout or Site
        /// </summary>
        /// <value>
        /// The Page that this Block is being implemented on. This value will 
        /// be null if the Block is implemented as part of a Layout or Site
        /// </value>
        public PageCache Page
        {
            get
            {
                if ( PageId.HasValue )
                {
                    return PageCache.Read( PageId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the Layout that this Block is implemented on. This 
        /// property will be null if this Block is being implemented on as part of a Page or Site
        /// </summary>
        /// <value>
        /// The Layout that this Block is being implemented on. This value will 
        /// be null if the Block is implemented as part of a Page or Site
        /// </value>
        public LayoutCache Layout
        {
            get
            {
                if ( LayoutId != null && LayoutId.Value != 0 )
                {
                    return LayoutCache.Read( LayoutId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the Site that this Block is implemented on. This 
        /// property will be null if this Block is being implemented on as part of a Page or Layout
        /// </summary>
        /// <value>
        /// The Site that this Block is being implemented on. This value will 
        /// be null if the Block is implemented as part of a Page or Layout
        /// </value>
        public SiteCache Site
        {
            get
            {
                if ( SiteId != null && SiteId.Value != 0 )
                {
                    return SiteCache.Read( SiteId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the block type
        /// </summary>
        public BlockTypeCache BlockType => BlockTypeCache.Read( BlockTypeId );

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority
        {
            get
            {
                switch ( BlockLocation )
                {
                    case BlockLocation.Page:
                        return Page ?? base.ParentAuthority;
                    case BlockLocation.Layout:
                        return Layout ?? base.ParentAuthority;
                    case BlockLocation.Site:
                        return Site ?? base.ParentAuthority;
                    default:
                        return base.ParentAuthority;
                }
            }
        }

        /// <summary>
        /// Gets the block location.
        /// </summary>
        /// <value>
        /// The block location.
        /// </value>
        public virtual BlockLocation BlockLocation
        {
            get
            {
                if ( PageId.HasValue )
                {
                    return BlockLocation.Page;
                }

                if ( LayoutId.HasValue )
                {
                    return BlockLocation.Layout;
                }

                if ( SiteId.HasValue )
                {
                    return BlockLocation.Site;
                }

                return BlockLocation.None;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is Block ) ) return;

            var block = (Block)model;
            IsSystem = block.IsSystem;
            PageId = block.PageId;
            LayoutId = block.LayoutId;
            SiteId = block.SiteId;
            BlockTypeId = block.BlockTypeId;
            Zone = block.Zone;
            Order = block.Order;
            Name = block.Name;
            CssClass = block.CssClass;
            PreHtml = block.PreHtml;
            PostHtml = block.PostHtml;
            OutputCacheDuration = block.OutputCacheDuration;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheBlock ) ) return;

            var block = (CacheBlock)cacheEntity;
            IsSystem = block.IsSystem;
            PageId = block.PageId;
            LayoutId = block.LayoutId;
            SiteId = block.SiteId;
            BlockTypeId = block.BlockTypeId;
            Zone = block.Zone;
            Order = block.Order;
            Name = block.Name;
            CssClass = block.CssClass;
            PreHtml = block.PreHtml;
            PostHtml = block.PostHtml;
            OutputCacheDuration = block.OutputCacheDuration;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns Block object from cache.  If block does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static BlockCache Read( int id, RockContext rockContext = null )
        {
            return new BlockCache( CacheBlock.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static BlockCache Read( Guid guid, RockContext rockContext = null )
        {
            return new BlockCache( CacheBlock.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Adds Block model to cache, and returns cached object
        /// </summary>
        /// <param name="blockModel">The block model.</param>
        /// <returns></returns>
        public static BlockCache Read( Block blockModel )
        {
            return new BlockCache( CacheBlock.Get( blockModel ) );
        }

        /// <summary>
        /// Removes block from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheBlock.Remove( id );
        }

        #endregion
    }
}