//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Runtime.Caching;

using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a campus that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class CampusCache : CachedModel<Campus>
    {
        #region Constructors

        private CampusCache() 
        {
        }

        private CampusCache( Campus campus )
        {
            CopyFromModel( campus );
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
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the short code.
        /// </summary>
        /// <value>
        /// The short code.
        /// </value>
        public string ShortCode { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is Campus )
            {
                var campus = (Campus)model;
                this.IsSystem = campus.IsSystem;
                this.Name = campus.Name;
                this.ShortCode = campus.ShortCode;
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

        /// <summary>
        /// Gets the cache key for the selected campu id.
        /// </summary>
        /// <param name="id">The campus id.</param>
        /// <returns></returns>
        public static string CacheKey( int id )
        {
            return string.Format( "Rock:Campus:{0}", id );
        }

        /// <summary>
        /// Returns Campus object from cache.  If campus does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static CampusCache Read( int id )
        {
            string cacheKey = CampusCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            CampusCache campus = cache[cacheKey] as CampusCache;

            if ( campus != null )
            {
                return campus;
            }
            else
            {
                CampusService campusService = new CampusService();
                Campus campusModel = campusService.Get( id );
                if ( campusModel != null )
                {
                    campusModel.LoadAttributes();
                    campus = new CampusCache( campusModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, campus, cachePolicy );
                    cache.Set( campus.Guid.ToString(), campus.Id, cachePolicy );
                    
                    return campus;
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
        public static CampusCache Read( Guid guid )
        {
            ObjectCache cache = MemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            if ( cacheObj != null )
            {
                return Read( (int)cacheObj );
            }
            else
            {
                var campusService = new CampusService();
                var campusModel = campusService.Get( guid );
                if ( campusModel != null )
                {
                    campusModel.LoadAttributes();
                    var campus = new CampusCache( campusModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( CampusCache.CacheKey( campus.Id ), campus, cachePolicy );
                    cache.Set( campus.Guid.ToString(), campus.Id, cachePolicy );

                    return campus;
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// Adds Campus model to cache, and returns cached object
        /// </summary>
        /// <param name="campusModel"></param>
        /// <returns></returns>
        public static CampusCache Read( Campus campusModel )
        {
            string cacheKey = CampusCache.CacheKey( campusModel.Id );

            ObjectCache cache = MemoryCache.Default;
            CampusCache campus = cache[cacheKey] as CampusCache;

            if ( campus != null )
            {
                return campus;
            }
            else
            {
                campus = new CampusCache( campusModel );
                
                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, campus, cachePolicy );
                cache.Set( campus.Guid.ToString(), campus.Id, cachePolicy );
                
                return campus;
            }
        }

        /// <summary>
        /// Removes campus from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( CampusCache.CacheKey( id ) );
        }

        #endregion
    }
}