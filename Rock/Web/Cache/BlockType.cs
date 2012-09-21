//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a block type that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class BlockType : Rock.Cms.BlockTypeDto
    {
        private BlockType() : base() { }
		private BlockType( Rock.Cms.BlockType blockType ) : base( blockType ) { }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Rock.Attribute.PropertyAttribute"/> attributes have been 
        /// verified for the block type.  If not, Rock will create and/or update the attributes associated with the block.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if attributes have already been verified; otherwise, <c>false</c>.
        /// </value>
        public bool IsInstancePropertiesVerified { get; internal set; }

        private List<int> AttributeIds = new List<int>();
        /// <summary>
        /// List of attributes associated with the block type.  This object will not include values.
        /// To get values associated with the current block instance, use the AttributeValues
        /// </summary>
        public List<Rock.Web.Cache.Attribute> Attributes
        {
            get
            {
                List<Rock.Web.Cache.Attribute> attributes = new List<Rock.Web.Cache.Attribute>();

                foreach ( int id in AttributeIds )
                    attributes.Add( Attribute.Read( id ) );

                return attributes;
            }
        }

        /// <summary>
        /// Gets the attribute values.
        /// </summary>
        public Dictionary<string, KeyValuePair<string, List<Rock.Core.AttributeValueDto>>> AttributeValues { get; private set; }

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="personId">The person id.</param>
        public void SaveAttributeValues(int? personId)
        {
            Rock.Cms.BlockTypeService blockTypeService = new Cms.BlockTypeService();
            Rock.Cms.BlockType blockTypeModel = blockTypeService.Get( this.Id );

            if ( blockTypeModel != null )
            {
                Rock.Attribute.Helper.LoadAttributes( blockTypeModel );
                foreach ( var category in blockTypeModel.Attributes )
                    foreach ( var attribute in category.Value )
                        Rock.Attribute.Helper.SaveAttributeValues( blockTypeModel, attribute, this.AttributeValues[attribute.Key].Value, personId );
            }
        }

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
        public static BlockType Read( int id )
        {
            string cacheKey = BlockType.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            BlockType blockType = cache[cacheKey] as BlockType;

            if ( blockType != null )
                return blockType;
            else
            {
                Rock.Cms.BlockTypeService blockTypeService = new Cms.BlockTypeService();
                Rock.Cms.BlockType blockTypeModel = blockTypeService.Get( id );
                if ( blockTypeModel != null )
                {
					blockType = new BlockType(blockTypeModel);

                    blockType.IsInstancePropertiesVerified = false;

                    Rock.Attribute.Helper.LoadAttributes( blockTypeModel );

                    blockType.AttributeValues = blockTypeModel.AttributeValues;

                    if (blockTypeModel.Attributes != null)
                        foreach ( var category in blockTypeModel.Attributes )
                            foreach ( var attribute in category.Value )
                                blockType.AttributeIds.Add( attribute.Id );

                    // Block Type cache expiration monitors the actual block on the file system so that it is flushed from 
                    // memory anytime the file contents change.  This is to force the cmsPage object to revalidate any
                    // BlockInstancePropery attributes that may have been added or modified
                    string physicalPath = System.Web.HttpContext.Current.Request.MapPath( blockType.Path );
                    List<string> filePaths = new List<string>();
                    filePaths.Add( physicalPath );
                    filePaths.Add( physicalPath + ".cs" );

                    CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
                    cacheItemPolicy.ChangeMonitors.Add( new HostFileChangeMonitor( filePaths ) );
                    cache.Set( cacheKey, blockType, cacheItemPolicy );

                    return blockType;
                }
                else
                    return null;

            }
        }

        /// <summary>
        /// Removes block type from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( BlockType.CacheKey( id ) );
        }

        #endregion
    }
}