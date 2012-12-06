//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Runtime.Caching;

using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a block that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class BlockCache : Rock.Model.BlockDto, Rock.Attribute.IHasAttributes
    {
        private BlockCache() : base() { }
        private BlockCache( Rock.Model.Block model ) : base( model ) { }

        private List<int> AttributeIds = new List<int>();

        /// <summary>
        /// Dictionary of categorized attributes.  Key is the category name, and Value is list of attributes in the category
        /// </summary>
        /// <value>
        /// The attribute categories.
        /// </value>
        public SortedDictionary<string, List<string>> AttributeCategories
        {
            get
            {
                var attributeCategories = new SortedDictionary<string, List<string>>();

                foreach ( int id in AttributeIds )
                {
                    var attribute = AttributeCache.Read( id );
                    if ( !attributeCategories.ContainsKey( attribute.Category ) )
                        attributeCategories.Add( attribute.Category, new List<string>() );
                    attributeCategories[attribute.Category].Add( attribute.Key );
                }

                return attributeCategories;
            }

            set { }
        }

        /// <summary>
        /// List of attributes associated with the Block.  This object will not include values.
        /// To get values associated with the current page instance, use the AttributeValues
        /// </summary>
        public Dictionary<string, Rock.Web.Cache.AttributeCache> Attributes
        {
            get
            {
                var attributes = new Dictionary<string, Rock.Web.Cache.AttributeCache>();

                foreach ( int id in AttributeIds )
                {
                    Rock.Web.Cache.AttributeCache attribute = AttributeCache.Read( id );
                    attributes.Add( attribute.Key, attribute );
                }

                return attributes;
            }

            set
            {
                this.AttributeIds = new List<int>();
                foreach ( var attribute in value )
                    this.AttributeIds.Add( attribute.Value.Id );
            }
        }

        /// <summary>
        /// Dictionary of all attributes and their values.
        /// </summary>
        public Dictionary<string, List<Rock.Model.AttributeValueDto>> AttributeValues { get; set; }

        /// <summary>
        /// Gets the page.
        /// </summary>
        public PageCache Page
        {
            get
            {
                if ( PageId.HasValue )
                    return PageCache.Read( PageId.Value );
                else
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
        /// Saves the attribute values.
        /// </summary>
        /// <param name="personId">The person id.</param>
        public void SaveAttributeValues(int? personId)
        {
            var blockService = new Model.BlockService();
            var blockModel = blockService.Get( this.Id );

            if ( blockModel != null )
            {
                blockModel.LoadAttributes();
                foreach ( var attribute in blockModel.Attributes )
                    Rock.Attribute.Helper.SaveAttributeValues( blockModel, attribute.Value, this.AttributeValues[attribute.Key], personId );
            }
        }

        /// <summary>
        /// Reloads the attribute values.
        /// </summary>
        public void ReloadAttributeValues()
        {
            var blockService = new Model.BlockService();
            var blockModel = blockService.Get( this.Id );

            if ( blockModel != null )
            {
                blockModel.LoadAttributes();

                this.AttributeValues = blockModel.AttributeValues;

                this.AttributeIds = new List<int>();
                if ( blockModel.Attributes != null )
                    foreach ( var attribute in blockModel.Attributes )
                        this.AttributeIds.Add( attribute.Value.Id );
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
                if ( this.BlockLocation == Model.BlockLocation.Page)
                {
                    return this.Page;
                }
                else
                {
                    return null;
                }
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

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:Block:{0}", id );
        }

        /// <summary>
        /// Adds Block model to cache, and returns cached object
        /// </summary>
        /// <param name="blockModel"></param>
        /// <returns></returns>
        public static BlockCache Read( Rock.Model.Block blockModel )
        {
            string cacheKey = BlockCache.CacheKey( blockModel.Id );

            ObjectCache cache = MemoryCache.Default;
            BlockCache block = cache[cacheKey] as BlockCache;

            if ( block != null )
                return block;
            else
            {
                block = BlockCache.CopyModel( blockModel );
                cache.Set( cacheKey, block, new CacheItemPolicy() );

                return block;
            }
        }

        /// <summary>
        /// Returns Block object from cache.  If block does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static BlockCache Read( int id )
        {
            string cacheKey = BlockCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            BlockCache block = cache[cacheKey] as BlockCache;

            if ( block != null )
                return block;
            else
            {
                var blockService = new Model.BlockService();
                var blockModel = blockService.Get( id );
                if ( blockModel != null )
                {
                    blockModel.LoadAttributes();

                    block = BlockCache.CopyModel( blockModel );

                    cache.Set( cacheKey, block, new CacheItemPolicy() );

                    return block;
                }
                else
                    return null;
            }
        }

        private static BlockCache CopyModel ( Rock.Model.Block blockModel )
        {
            BlockCache block = new BlockCache(blockModel);

            block.AttributeValues = blockModel.AttributeValues;
            
            block.AttributeIds = new List<int>();
            if (blockModel.Attributes != null)
                foreach ( var attribute in blockModel.Attributes )
                    block.AttributeIds.Add( attribute.Value.Id );

            return block;
        }

        /// <summary>
        /// Removes block from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( BlockCache.CacheKey( id ) );
        }

        #endregion

     }

}