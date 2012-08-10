//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Runtime.Caching;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a definedValue that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class DefinedValue
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new DefinedValue object
        /// </summary>
        private DefinedValue() { }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; private set; }

        private int DefinedTypeId { get; set; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public DefinedType DefinedType
        {
            get { return DefinedType.Read( DefinedTypeId ); }
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; private set; }

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
        public static DefinedValue Read( int id )
        {
            string cacheKey = DefinedValue.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            DefinedValue definedValue = cache[cacheKey] as DefinedValue;

            if ( definedValue != null )
                return definedValue;
            else
            {
                Rock.Core.DefinedValueService definedValueService = new Rock.Core.DefinedValueService();
                Rock.Core.DefinedValue definedValueModel = definedValueService.Get( id );
                if ( definedValueModel != null )
                {
                    definedValue = CopyModel( definedValueModel );

                    cache.Set( cacheKey, definedValue, new CacheItemPolicy() );

                    return definedValue;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// Reads the specified defined value model.
        /// </summary>
        /// <param name="definedValueModel">The defined value model.</param>
        /// <returns></returns>
        public static DefinedValue Read( Rock.Core.DefinedValue definedValueModel )
        {
            DefinedValue definedValue = DefinedValue.CopyModel( definedValueModel );

            string cacheKey = DefinedValue.CacheKey( definedValueModel.Id );
            ObjectCache cache = MemoryCache.Default;
            cache.Set( cacheKey, definedValue, new CacheItemPolicy() );

            return definedValue;
        }

        /// <summary>
        /// Copies the model.
        /// </summary>
        /// <param name="definedValueModel">The defined value model.</param>
        /// <returns></returns>
        public static DefinedValue CopyModel( Rock.Core.DefinedValue definedValueModel )
        {
            DefinedValue definedValue = new DefinedValue();
            definedValue.Id = definedValueModel.Id;
            definedValue.Order = definedValueModel.Order;
            definedValue.Name = definedValueModel.Name;
            definedValue.Description = definedValueModel.Description;
            definedValue.DefinedTypeId = definedValueModel.DefinedTypeId;

            return definedValue;
        }

        /// <summary>
        /// Removes definedValue from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( DefinedValue.CacheKey( id ) );
        }

        #endregion
    }
}