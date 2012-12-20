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
    /// Information about a site that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class SiteCache : CachedModel<Site>
    {
        #region Constructors

        private SiteCache()
        {
        }

        private SiteCache( Site site )
        {
            CopyFromModel( site );
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
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        /// <value>
        /// The theme.
        /// </value>
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets the default page id.
        /// </summary>
        /// <value>
        /// The default page id.
        /// </value>
        public int? DefaultPageId { get; set; }

        /// <summary>
        /// Gets or sets the favicon URL.
        /// </summary>
        /// <value>
        /// The favicon URL.
        /// </value>
        public string FaviconUrl { get; set; }

        /// <summary>
        /// Gets or sets the apple touch icon URL.
        /// </summary>
        /// <value>
        /// The apple touch icon URL.
        /// </value>
        public string AppleTouchIconUrl { get; set; }

        /// <summary>
        /// Gets or sets the facebook app id.
        /// </summary>
        /// <value>
        /// The facebook app id.
        /// </value>
        public string FacebookAppId { get; set; }

        /// <summary>
        /// Gets or sets the facebook app secret.
        /// </summary>
        /// <value>
        /// The facebook app secret.
        /// </value>
        public string FacebookAppSecret { get; set; }

        /// <summary>
        /// Gets or sets the login page reference.
        /// </summary>
        /// <value>
        /// The login page reference.
        /// </value>
        public string LoginPageReference { get; set; }

        /// <summary>
        /// Gets or sets the registration page reference.
        /// </summary>
        /// <value>
        /// The registration page reference.
        /// </value>
        public string RegistrationPageReference { get; set; }

        /// <summary>
        /// Gets or sets the error page.
        /// </summary>
        /// <value>
        /// The error page.
        /// </value>
        public string ErrorPage { get; set; }

        /// <summary>
        /// Gets the default page.
        /// </summary>
        public PageCache DefaultPage
        {
            get
            {
                if ( DefaultPageId != null && DefaultPageId.Value != 0 )
                    return PageCache.Read( DefaultPageId.Value );
                else
                    return null;
            }
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

            if ( model is Site )
            {
                var site = (Site)model;
                this.IsSystem = site.IsSystem;
                this.Name = site.Name;
                this.Description = site.Description;
                this.Theme = site.Theme;
                this.DefaultPageId = site.DefaultPageId;
                this.FaviconUrl = site.FaviconUrl;
                this.AppleTouchIconUrl = site.AppleTouchIconUrl;
                this.FacebookAppId = site.FacebookAppId;
                this.FacebookAppSecret = site.FacebookAppSecret;
                this.LoginPageReference = site.LoginPageReference;
                this.RegistrationPageReference = site.RegistrationPageReference;
                this.ErrorPage = site.ErrorPage;
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
            return string.Format( "Rock:Site:{0}", id );
        }

        /// <summary>
        /// Returns Site object from cache.  If site does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SiteCache Read( int id )
        {
            string cacheKey = SiteCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            SiteCache site = cache[cacheKey] as SiteCache;

            if ( site != null )
            {
                return site;
            }
            else
            {
                var siteService = new SiteService();
                var siteModel = siteService.Get( id );
                if ( siteModel != null )
                {
                    siteModel.LoadAttributes();
                    site = new SiteCache( siteModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, site, cachePolicy );
                    cache.Set( site.Guid.ToString(), site.Id, cachePolicy );

                    return site;
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
        public static SiteCache Read( Guid guid )
        {
            ObjectCache cache = MemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            if ( cacheObj != null )
            {
                return Read( (int)cacheObj );
            }
            else
            {
                var siteService = new SiteService();
                var siteModel = siteService.Get( guid );
                if ( siteModel != null )
                {
                    siteModel.LoadAttributes();
                    var site = new SiteCache( siteModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( SiteCache.CacheKey( site.Id ), site, cachePolicy );
                    cache.Set( site.Guid.ToString(), site.Id, cachePolicy );

                    return site;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Adds Site model to cache, and returns cached object
        /// </summary>
        /// <param name="siteModel"></param>
        /// <returns></returns>
        public static SiteCache Read( Site siteModel )
        {
            string cacheKey = SiteCache.CacheKey( siteModel.Id );

            ObjectCache cache = MemoryCache.Default;
            SiteCache site = cache[cacheKey] as SiteCache;

            if ( site != null )
            {
                return site;
            }
            else
            {
                site = new SiteCache( siteModel );

                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, site, cachePolicy );
                cache.Set( site.Guid.ToString(), site.Id, cachePolicy );

                return site;
            }
        }

        /// <summary>
        /// Removes site from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( SiteCache.CacheKey( id ) );
        }

        #endregion

    }
}