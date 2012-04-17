//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Runtime.Caching;
using System.Linq;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a blockInstance that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    public class BlockInstance : Security.ISecured, Rock.Attribute.IHasAttributes
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new BlockInstance object
        /// </summary>
        private BlockInstance() { }

        /// <summary>
        /// The Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the zone.
        /// </summary>
        public string Zone { get; private set; }

        /// <summary>
        /// Gets the location of the block instance (Layout or Page)
        /// </summary>
        public BlockInstanceLocation BlockInstanceLocation { get; private set; }

        /// <summary>
        /// Gets the order.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the duration of the output cache. If value is 0, the output will not be cached
        /// </summary>
        /// <value>
        /// The duration of the output cache.
        /// </value>
        public int OutputCacheDuration { get; private set; }

        /// <summary>
        /// Dictionary of all attributes and their values.
        /// </summary>
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; set; }

        private List<int> AttributeIds = new List<int>();
        /// <summary>
        /// List of attributes associated with the BlockInstance.  This object will not include values.
        /// To get values associated with the current page instance, use the AttributeValues
        /// </summary>
        public SortedDictionary<string, List<Rock.Web.Cache.Attribute>> Attributes
        {
            get
            {
                SortedDictionary<string, List<Rock.Web.Cache.Attribute>> attributes = new SortedDictionary<string, List<Rock.Web.Cache.Attribute>>();

                foreach ( int id in AttributeIds )
                {
                    Rock.Web.Cache.Attribute attribute = Attribute.Read( id );
                    if ( !attributes.ContainsKey( attribute.Category ) )
                        attributes.Add( attribute.Category, new List<Attribute>() );

                    attributes[attribute.Category].Add( attribute );
                }

                return attributes;
            }

            set
            {
                this.AttributeIds = new List<int>();
                foreach ( var category in value )
                    foreach ( var attribute in category.Value )
                        this.AttributeIds.Add( attribute.Id );
            }
        }

        /// <summary>
        /// Gets the block id.
        /// </summary>
        public int BlockId { get; private set; }

        /// <summary>
        /// Gets the block.
        /// </summary>
        public Block Block
        {
            get
            {
                return Block.Read( BlockId );
            }
        }

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="personId">The person id.</param>
        public void SaveAttributeValues(int? personId)
        {
            Rock.CMS.BlockInstanceRepository blockInstanceRepository = new CMS.BlockInstanceRepository();
            Rock.CMS.BlockInstance blockInstanceModel = blockInstanceRepository.Get( this.Id );

            if ( blockInstanceModel != null )
            {
                Rock.Attribute.Helper.LoadAttributes( blockInstanceModel );
                foreach ( var category in blockInstanceModel.Attributes )
                    foreach ( var attribute in category.Value )
                        Rock.Attribute.Helper.SaveAttributeValue( blockInstanceModel, attribute, this.AttributeValues[attribute.Key].Value, personId );
            }
        }

        /// <summary>
        /// Reloads the attribute values.
        /// </summary>
        public void ReloadAttributeValues()
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                Rock.CMS.BlockInstanceRepository blockInstanceRepository = new CMS.BlockInstanceRepository();
                Rock.CMS.BlockInstance blockInstanceModel = blockInstanceRepository.Get( this.Id );

                if ( blockInstanceModel != null )
                {
                    Rock.Attribute.Helper.LoadAttributes( blockInstanceModel );

                    this.AttributeValues = blockInstanceModel.AttributeValues;

                    this.AttributeIds = new List<int>();
                    if ( blockInstanceModel.Attributes != null )
                        foreach ( var category in blockInstanceModel.Attributes )
                            foreach ( var attribute in category.Value )
                                this.AttributeIds.Add( attribute.Id );
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
            return string.Format( "Rock:BlockInstance:{0}", id );
        }

        /// <summary>
        /// Adds BlockInstance model to cache, and returns cached object
        /// </summary>
        /// <param name="blockInstanceModel"></param>
        /// <returns></returns>
        public static BlockInstance Read( Rock.CMS.BlockInstance blockInstanceModel )
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
        /// <param name="id"></param>
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
                Rock.CMS.BlockInstanceRepository blockInstanceRepository = new CMS.BlockInstanceRepository();
                Rock.CMS.BlockInstance blockInstanceModel = blockInstanceRepository.Get( id );
                if ( blockInstanceModel != null )
                {
                    Rock.Attribute.Helper.LoadAttributes( blockInstanceModel );

                    blockInstance = BlockInstance.CopyModel( blockInstanceModel );

                    cache.Set( cacheKey, blockInstance, new CacheItemPolicy() );

                    return blockInstance;
                }
                else
                    return null;
            }
        }

        private static BlockInstance CopyModel ( Rock.CMS.BlockInstance blockInstanceModel )
        {
            BlockInstance blockInstance = new BlockInstance();
            blockInstance.Id = blockInstanceModel.Id;
            blockInstance.BlockId = blockInstanceModel.BlockId;
            blockInstance.Name = blockInstanceModel.Name;
            blockInstance.Zone = blockInstanceModel.Zone;
            blockInstance.BlockInstanceLocation = blockInstanceModel.Page != null ? BlockInstanceLocation.Page : BlockInstanceLocation.Layout;
            blockInstance.Order = blockInstanceModel.Order;
            blockInstance.OutputCacheDuration = blockInstanceModel.OutputCacheDuration;
            blockInstance.AttributeValues = blockInstanceModel.AttributeValues;
            blockInstance.AttributeIds = new List<int>();
            if (blockInstanceModel.Attributes != null)
                foreach ( var category in blockInstanceModel.Attributes )
                    foreach ( var attribute in category.Value )
                        blockInstance.AttributeIds.Add( attribute.Id );

            blockInstance.AuthEntity = blockInstanceModel.AuthEntity;
            blockInstance.InstanceActions = blockInstanceModel.SupportedActions;

            return blockInstance;
        }

        /// <summary>
        /// Removes blockInstance from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( BlockInstance.CacheKey( id ) );
        }

        #endregion

        #region ISecure Implementation

        /// <summary>
        /// The auth entity. The auth entity is a unique identifier for each type of class that implements
        /// the <see cref="Rock.Security.ISecured"/> interface.
        /// </summary>
        public string AuthEntity { get; set; }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        public Security.ISecured ParentAuthority
        {
            get { return null; }
        }

        /// <summary>
        /// The list of actions that this class supports.
        /// </summary>
        public List<string> SupportedActions 
        {
            get 
            {
                List<string> combinedActions = new List<string>();
                
                if (InstanceActions != null)
                    combinedActions.AddRange(InstanceActions);
                
                if (BlockActions != null)
                    combinedActions.AddRange(BlockActions);

                return combinedActions;
            }
        }

        internal List<string> InstanceActions { get; set; }
        internal List<string> BlockActions { get; set; }

        /// <summary>
        /// Returns <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public virtual bool Authorized( string action, Rock.CMS.User user )
        {
            return Security.Authorization.Authorized( this, action, user );
        }

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// returna <c>true</c> if they will be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public bool DefaultAuthorization( string action )
        {
            return action == "View";
        }

        #endregion
    }

    /// <summary>
    /// The location of the block instance
    /// </summary>
    public enum BlockInstanceLocation
    {
        /// <summary>
        /// Block instance is located in the layout (will be rendered for every page using the layout)
        /// </summary>
        Layout,

        /// <summary>
        /// Block instance is located on the page
        /// </summary>
        Page,
    }
}