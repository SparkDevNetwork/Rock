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
    /// Information about a block that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class Block : Rock.Cms.BlockDto
    {
        private Block() : base() { }
		private Block( Rock.Cms.Block block ) : base( block ) { }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Rock.Attribute.PropertyAttribute"/> attributes have been 
        /// verified for the block.  If not, Rock will create and/or update the attributes associated with the block.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if attributes have already been verified; otherwise, <c>false</c>.
        /// </value>
        public bool IsInstancePropertiesVerified { get; internal set; }

        private List<int> AttributeIds = new List<int>();
        /// <summary>
        /// List of attributes associated with the block.  This object will not include values.
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
            Rock.Cms.BlockService blockService = new Cms.BlockService();
            Rock.Cms.Block blockModel = blockService.Get( this.Id );

            if ( blockModel != null )
            {
                Rock.Attribute.Helper.LoadAttributes( blockModel );
                foreach ( var category in blockModel.Attributes )
                    foreach ( var attribute in category.Value )
                        Rock.Attribute.Helper.SaveAttributeValues( blockModel, attribute, this.AttributeValues[attribute.Key].Value, personId );
            }
        }

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:Block:{0}", id );
        }

        /// <summary>
        /// Returns Block object from cache.  If block does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Block Read( int id )
        {
            string cacheKey = Block.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            Block block = cache[cacheKey] as Block;

            if ( block != null )
                return block;
            else
            {
                Rock.Cms.BlockService blockService = new Cms.BlockService();
                Rock.Cms.Block blockModel = blockService.Get( id );
                if ( blockModel != null )
                {
					block = new Block(blockModel);

                    block.IsInstancePropertiesVerified = false;

                    Rock.Attribute.Helper.LoadAttributes( blockModel );

                    block.AttributeValues = blockModel.AttributeValues;

                    if (blockModel.Attributes != null)
                        foreach ( var category in blockModel.Attributes )
                            foreach ( var attribute in category.Value )
                                block.AttributeIds.Add( attribute.Id );

                    // Block cache expiration monitors the actual block on the file system so that it is flushed from 
                    // memory anytime the file contents change.  This is to force the cmsPage object to revalidate any
                    // BlockInstancePropery attributes that may have been added or modified
                    string physicalPath = System.Web.HttpContext.Current.Request.MapPath( block.Path );
                    List<string> filePaths = new List<string>();
                    filePaths.Add( physicalPath );
                    filePaths.Add( physicalPath + ".cs" );

                    CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
                    cacheItemPolicy.ChangeMonitors.Add( new HostFileChangeMonitor( filePaths ) );
                    cache.Set( cacheKey, block, cacheItemPolicy );

                    return block;
                }
                else
                    return null;

            }
        }

        /// <summary>
        /// Removes block from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( Block.CacheKey( id ) );
        }

        #endregion
    }
}