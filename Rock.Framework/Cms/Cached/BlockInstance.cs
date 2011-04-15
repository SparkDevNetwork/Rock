using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Rock.Cms.Cached
{
    /// <summary>
    /// Information about a blockInstance that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    public class BlockInstance : Security.ISecured
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new BlockInstance object
        /// </summary>
        private BlockInstance() { }

        public int Id { get; set; }
        public string Zone { get; private set; }
        public int Order { get; private set; }
        public int OutputCacheDuration { get; private set; }

        /// <summary>
        /// Dictionary of all attributes and their values.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; private set; }

        private List<int> AttributeIds = new List<int>();
        /// <summary>
        /// List of attributes associated with the BlockInstance.  This object will not include values.
        /// To get values associated with the current page instance, use the AttributeValues
        /// </summary>
        public List<Rock.Cms.Cached.Attribute> Attributes
        {
            get
            {
                List<Rock.Cms.Cached.Attribute> attributes = new List<Rock.Cms.Cached.Attribute>();

                foreach ( int id in AttributeIds )
                    attributes.Add( Attribute.Read( id ) );

                return attributes;
            }
        }

        public int BlockId { get; private set; }
        public Block Block
        {
            get
            {
                return Block.Read( BlockId );
            }
        }

        public void SaveAttributeValues(int? personId)
        {
            using ( new Rock.Helpers.UnitOfWorkScope() )
            {
                Rock.Services.Cms.BlockInstanceService blockInstanceService = new Services.Cms.BlockInstanceService();
                Rock.Models.Cms.BlockInstance blockInstanceModel = blockInstanceService.GetBlockInstance( this.Id );

                if ( blockInstanceModel != null )
                {
                    blockInstanceService.LoadAttributes( blockInstanceModel );
                    foreach ( Rock.Models.Core.Attribute attribute in blockInstanceModel.Attributes )
                        blockInstanceService.SaveAttributeValue( blockInstanceModel, attribute, this.AttributeValues[attribute.Name], personId );
                }
            }
        }

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:BlockInstance:{0}", id );
        }

        /// <summary>
        /// Adds BlockInstance model to cache, and returns cached object
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static BlockInstance Read( Rock.Models.Cms.BlockInstance blockInstanceModel )
        {
            BlockInstance blockInstance = BlockInstance.CopyModel( blockInstanceModel );

            string cacheKey = BlockInstance.CacheKey( blockInstanceModel.Id );
            ObjectCache cache = MemoryCache.Default;
            cache.Set( cacheKey, blockInstance, new CacheItemPolicy() );

            return blockInstance;
        }

        /// <summary>
        /// Returns BlockInstance object from cache.  If blockInstance does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static BlockInstance Read( int id )
        {
            string cacheKey = BlockInstance.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            BlockInstance blockInstance = cache[cacheKey] as BlockInstance;

            if ( blockInstance != null )
                return blockInstance;
            else
            {
                Rock.Services.Cms.BlockInstanceService blockInstanceService = new Services.Cms.BlockInstanceService();
                Rock.Models.Cms.BlockInstance blockInstanceModel = blockInstanceService.GetBlockInstance( id );
                if ( blockInstanceModel != null )
                {
                    blockInstanceService.LoadAttributes( blockInstanceModel );

                    blockInstance = BlockInstance.CopyModel( blockInstanceModel );

                    cache.Set( cacheKey, blockInstance, new CacheItemPolicy() );

                    return blockInstance;
                }
                else
                    return null;
            }
        }

        private static BlockInstance CopyModel ( Rock.Models.Cms.BlockInstance blockInstanceModel )
        {
            BlockInstance blockInstance = new BlockInstance();
            blockInstance.Id = blockInstanceModel.Id;
            blockInstance.BlockId = blockInstanceModel.BlockId;
            blockInstance.Zone = blockInstanceModel.Zone;
            blockInstance.Order = blockInstanceModel.Order;
            blockInstance.OutputCacheDuration = blockInstanceModel.OutputCacheDuration;
            blockInstance.AttributeValues = blockInstanceModel.AttributeValues;

            blockInstance.AttributeIds = new List<int>();
            foreach ( Rock.Models.Core.Attribute attribute in blockInstanceModel.Attributes )
            {
                blockInstance.AttributeIds.Add( attribute.Id );
                Attribute.Read( attribute );
            }

            blockInstance.AuthEntity = blockInstanceModel.AuthEntity;
            blockInstance.SupportedActions = blockInstanceModel.SupportedActions;

            return blockInstance;
        }

        /// <summary>
        /// Removes blockInstance from cache
        /// </summary>
        /// <param name="guid"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( BlockInstance.CacheKey( id ) );
        }

        #endregion

        #region ISecure Implementation

        public string AuthEntity { get; set; }

        public Security.ISecured ParentAuthority
        {
            get { return null; }
        }

        public List<string> SupportedActions { get; set; }

        public virtual bool Authorized( string action, System.Web.Security.MembershipUser user )
        {
            return Rock.Cms.Security.Authorization.Authorized( this, action, user );
        }

        public bool DefaultAuthorization( string action )
        {
            return action == "View";
        }

        #endregion
    }
}