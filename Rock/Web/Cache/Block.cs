//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Linq;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a block that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class Block : Rock.Cms.BlockDto, Security.ISecured, Rock.Attribute.IHasAttributes
    {
		private Block() : base() { }
		private Block( Rock.Cms.Block model ) : base( model ) { }

        /// <summary>
        /// Gets the location of the block (Layout or Page)
        /// </summary>
        public BlockLocation BlockLocation { get; private set; }

        /// <summary>
        /// Dictionary of all attributes and their values.
        /// </summary>
        public Dictionary<string, KeyValuePair<string, List<Rock.Core.AttributeValueDto>>> AttributeValues { get; set; }

        private List<int> AttributeIds = new List<int>();
        /// <summary>
        /// List of attributes associated with the Block.  This object will not include values.
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
		/// Gets the page.
		/// </summary>
		public Page Page
		{
			get
			{
				if ( PageId.HasValue )
					return Page.Read( PageId.Value );
				else
					return null;
			}
		}

        /// <summary>
        /// Gets the block type
        /// </summary>
        public BlockType BlockType
        {
            get
            {
                return BlockType.Read( BlockTypeId );
            }
        }

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="personId">The person id.</param>
        public void SaveAttributeValues(int? personId)
        {
            var blockService = new Cms.BlockService();
            var blockModel = blockService.Get( this.Id );

            if ( blockModel != null )
            {
                Rock.Attribute.Helper.LoadAttributes( blockModel );
                foreach ( var category in blockModel.Attributes )
                    foreach ( var attribute in category.Value )
                        Rock.Attribute.Helper.SaveAttributeValues( blockModel, attribute, this.AttributeValues[attribute.Key].Value, personId );
            }
        }

        /// <summary>
        /// Reloads the attribute values.
        /// </summary>
        public void ReloadAttributeValues()
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var blockService = new Cms.BlockService();
                var blockModel = blockService.Get( this.Id );

                if ( blockModel != null )
                {
                    Rock.Attribute.Helper.LoadAttributes( blockModel );

                    this.AttributeValues = blockModel.AttributeValues;

                    this.AttributeIds = new List<int>();
                    if ( blockModel.Attributes != null )
                        foreach ( var category in blockModel.Attributes )
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
            return string.Format( "Rock:Block:{0}", id );
        }

        /// <summary>
        /// Adds Block model to cache, and returns cached object
        /// </summary>
        /// <param name="blockModel"></param>
        /// <returns></returns>
        public static Block Read( Rock.Cms.Block blockModel )
        {
			string cacheKey = Block.CacheKey( blockModel.Id );

            ObjectCache cache = MemoryCache.Default;
            Block block = cache[cacheKey] as Block;

			if ( block != null )
				return block;
			else
			{
				block = Block.CopyModel( blockModel );
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
        public static Block Read( int id )
        {
            string cacheKey = Block.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            Block block = cache[cacheKey] as Block;

            if ( block != null )
                return block;
            else
            {
                var blockService = new Cms.BlockService();
                var blockModel = blockService.Get( id );
                if ( blockModel != null )
                {
                    Rock.Attribute.Helper.LoadAttributes( blockModel );

                    block = Block.CopyModel( blockModel );

                    cache.Set( cacheKey, block, new CacheItemPolicy() );

                    return block;
                }
                else
                    return null;
            }
        }

        private static Block CopyModel ( Rock.Cms.Block blockModel )
        {
            Block block = new Block(blockModel);

			block.BlockLocation = blockModel.Page != null ? BlockLocation.Page : BlockLocation.Layout;
            block.AttributeValues = blockModel.AttributeValues;
            
			block.AttributeIds = new List<int>();
            if (blockModel.Attributes != null)
                foreach ( var category in blockModel.Attributes )
                    foreach ( var attribute in category.Value )
                        block.AttributeIds.Add( attribute.Id );

            block.AuthEntity = blockModel.AuthEntity;
            block.BlockActions = blockModel.SupportedActions;

            return block;
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
            get 
			{
				if ( this.BlockLocation == Cache.BlockLocation.Page )
					return this.Page;
				return null;
			}
        }

        /// <summary>
        /// The list of actions that this class supports.
        /// </summary>
        public List<string> SupportedActions 
        {
            get 
            {
                List<string> combinedActions = new List<string>();
                
                if (BlockActions != null)
                    combinedActions.AddRange(BlockActions);
                
                if (BlockTypeActions != null)
                    combinedActions.AddRange(BlockTypeActions);

                return combinedActions;
            }
        }

        internal List<string> BlockActions { get; set; }
        internal List<string> BlockTypeActions { get; set; }

        /// <summary>
        /// Returns <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsAuthorized( string action, Rock.Crm.Person person )
        {
            return Security.Authorization.Authorized( this, action, person );
        }

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// returna <c>true</c> if they will be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public bool IsAllowedByDefault( string action )
        {
            return action == "View";
        }

        public IQueryable<AuthRule> FindAuthRules()
        {
            return Authorization.FindAuthRules( this );
        }

        #endregion
    }

    /// <summary>
    /// The location of the block 
    /// </summary>
    [Serializable]
    public enum BlockLocation
    {
        /// <summary>
        /// Block is located in the layout (will be rendered for every page using the layout)
        /// </summary>
        Layout,

        /// <summary>
        /// Block is located on the page
        /// </summary>
        Page,
    }
}