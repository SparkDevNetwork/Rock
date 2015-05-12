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
using System.Data.Entity;
using System.Linq;

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
    public class BlockCache : CachedModel<Block>
    {
        #region Constructors

        private BlockCache()
        {
        }

        private BlockCache( Block block )
        {
            CopyFromModel( block );
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
        /// Gets or sets the page id.
        /// </summary>
        /// <value>
        /// The page id.
        /// </value>
        public int? PageId { get; set; }

        /// <summary>
        /// Gets or sets the layout id.
        /// </summary>
        /// <value>
        /// The layout id.
        /// </value>
        public int? LayoutId { get; set; }

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
        /// Gets the page.
        /// </summary>
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
        /// Gets the <see cref="Layout"/> object for the block.
        /// </summary>
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
        /// Gets the block type
        /// </summary>
        public BlockTypeCache BlockType
        {
            get
            {
                return BlockTypeCache.Read( BlockTypeId );
            }
        }

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
                if ( this.BlockLocation == Model.BlockLocation.Page )
                {
                    return this.Page;
                }
                else
                {
                    return this.Layout;
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
            get { return this.PageId.HasValue ? BlockLocation.Page : BlockLocation.Layout; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is Block )
            {
                var block = (Block)model;
                this.IsSystem = block.IsSystem;
                this.PageId = block.PageId;
                this.LayoutId = block.LayoutId;
                this.BlockTypeId = block.BlockTypeId;
                this.Zone = block.Zone;
                this.Order = block.Order;
                this.Name = block.Name;
                this.CssClass = block.CssClass;
                this.PreHtml = block.PreHtml;
                this.PostHtml = block.PostHtml;
                this.OutputCacheDuration = block.OutputCacheDuration;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:Block:{0}", id );
        }

        /// <summary>
        /// Returns Block object from cache.  If block does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static BlockCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( BlockCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static BlockCache LoadById( int id, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadById2( id, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadById2( id, rockContext2 );
            }
        }

        private static BlockCache LoadById2( int id, RockContext rockContext )
        {
            var blockService = new BlockService( rockContext );
            var blockModel = blockService.Get( id );
            if ( blockModel != null )
            {
                blockModel.LoadAttributes( rockContext );
                return new BlockCache( blockModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static BlockCache Read( Guid guid, RockContext rockContext = null )
        {
            int id = GetOrAddExisting( guid.ToString(),
                () => LoadByGuid( guid, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByGuid( Guid guid, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByGuid2( guid, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByGuid2( guid, rockContext2 );
            }
        }
        
        private static int LoadByGuid2( Guid guid, RockContext rockContext )
        {
            var blockService = new BlockService( rockContext );
            return blockService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Adds Block model to cache, and returns cached object
        /// </summary>
        /// <param name="blockModel">The block model.</param>
        /// <returns></returns>
        public static BlockCache Read( Block blockModel )
        {
            return GetOrAddExisting( CampusCache.CacheKey( blockModel.Id ),
                () => LoadByModel( blockModel ) );
        }

        private static BlockCache LoadByModel( Block blockModel )
        {
            if ( blockModel != null )
            {
                return new BlockCache( blockModel );
            }
            return null;
        }


        /// <summary>
        /// Removes block from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( BlockCache.CacheKey( id ) );
        }

        #endregion
    }
}