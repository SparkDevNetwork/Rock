//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Runtime.Caching;

using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a definedValue that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class DefinedValueCache : CachedModel<DefinedValue>
    {
        #region Constructors

        private DefinedValueCache()
        {
        }

        private DefinedValueCache( DefinedValue model )
        {
            CopyFromModel( model );
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
        /// Gets or sets the defined type id.
        /// </summary>
        /// <value>
        /// The defined type id.
        /// </value>
        public int DefinedTypeId { get; set; }

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
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public DefinedTypeCache DefinedType
        {
            get { return DefinedTypeCache.Read( DefinedTypeId ); }
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
                return DefinedType;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies the model property values to the DTO properties
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is DefinedValue )
            {
                var definedValue = (DefinedValue)model;
                this.IsSystem = definedValue.IsSystem;
                this.DefinedTypeId = definedValue.DefinedTypeId;
                this.Order = definedValue.Order;
                this.Name = definedValue.Name;
                this.Description = definedValue.Description;
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
            return string.Format( "Rock:DefinedValue:{0}", id );
        }

        /// <summary>
        /// Returns DefinedValue object from cache.  If definedValue does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static DefinedValueCache Read( int id )
        {
            string cacheKey = DefinedValueCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            DefinedValueCache definedValue = cache[cacheKey] as DefinedValueCache;

            if ( definedValue != null )
            {
                return definedValue;
            }
            else
            {
                var definedValueService = new DefinedValueService();
                var definedValueModel = definedValueService.Get( id );
                if ( definedValueModel != null )
                {
                    definedValueModel.LoadAttributes();
                    definedValue = new DefinedValueCache( definedValueModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, definedValue, cachePolicy );
                    cache.Set( definedValue.Guid.ToString(), definedValue.Id, cachePolicy );

                    return definedValue;
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
        public static DefinedValueCache Read( Guid guid )
        {
            ObjectCache cache = MemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            if ( cacheObj != null )
            {
                return Read( (int)cacheObj );
            }
            else
            {
                var definedValueService = new DefinedValueService();
                var definedValueModel = definedValueService.Get( guid );
                if ( definedValueModel != null )
                {
                    definedValueModel.LoadAttributes();
                    var definedValue = new DefinedValueCache( definedValueModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( DefinedValueCache.CacheKey( definedValue.Id ), definedValue, cachePolicy );
                    cache.Set( definedValue.Guid.ToString(), definedValue.Id, cachePolicy );

                    return definedValue;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads the specified defined value model.
        /// </summary>
        /// <param name="definedValueModel">The defined value model.</param>
        /// <returns></returns>
        public static DefinedValueCache Read( DefinedValue definedValueModel )
        {
            string cacheKey = DefinedValueCache.CacheKey( definedValueModel.Id );

            ObjectCache cache = MemoryCache.Default;
            DefinedValueCache definedValue = cache[cacheKey] as DefinedValueCache;

            if ( definedValue != null )
            {
                return definedValue;
            }
            else
            {
                definedValue = new DefinedValueCache( definedValueModel );

                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, definedValue, cachePolicy );
                cache.Set( definedValue.Guid.ToString(), definedValue.Id, cachePolicy );

                return definedValue;
            }
        }

        /// <summary>
        /// Removes definedValue from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( DefinedValueCache.CacheKey( id ) );
        }

        #endregion

    }
}