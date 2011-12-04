using System;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace Rock.Cms.Cached
{
    /// <summary>
    /// Information about a site that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    public class Site
    {
        /// <summary>
        /// Use Static Read() method to instantiate a new Site object
        /// </summary>
        private Site() { }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Gets the org id.
        /// </summary>
        public string OrgId { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the theme.
        /// </summary>
        public string Theme { get; private set; }

        /// <summary>
        /// Gets the favicon URL.
        /// </summary>
        public string FaviconUrl { get; private set; }

        /// <summary>
        /// Gets the apple touch URL.
        /// </summary>
        public string AppleTouchUrl { get; private set; }

        /// <summary>
        /// Gets the attribute values.
        /// </summary>
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; private set; }

        /// <summary>
        /// Gets the facebook app id.
        /// </summary>
        public string FacebookAppId { get; private set; }

        /// <summary>
        /// Gets the facebook app secret.
        /// </summary>
        public string FacebookAppSecret { get; private set; }

        private List<int> AttributeIds = new List<int>();

        /// <summary>
        /// Gets a list of attributes associated with the site.  This object will not include values.
        /// To get values associated with the current site instance, use the AttributeValues
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

        /// <summary>
        /// Gets the default page id.
        /// </summary>
        public int? DefaultPageId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the default page.
        /// </summary>
        public Page DefaultPage
        {
            get
            {
                if ( DefaultPageId != null && DefaultPageId.Value != 0 )
                    return Page.Read( DefaultPageId.Value );
                else
                    return null;
            }
        }

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="personId">The person id.</param>
        public void SaveAttributeValues(int? personId)
        {
            Rock.Services.Cms.SiteService siteService = new Services.Cms.SiteService();
            Rock.Models.Cms.Site siteModel = siteService.Get( this.Id );
            if ( siteModel != null )
            {
                Rock.Attribute.Helper.LoadAttributes( siteModel );

                if ( siteModel.Attributes != null )
                    foreach ( Rock.Cms.Cached.Attribute attribute in siteModel.Attributes )
                        Rock.Attribute.Helper.SaveAttributeValue( siteModel, attribute, this.AttributeValues[attribute.Key].Value, personId );
            }
        }

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
        public static Site Read( int id )
        {
            string cacheKey = Site.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            Site site = cache[cacheKey] as Site;

            if ( site != null )
                return site;
            else
            {
                Rock.Services.Cms.SiteService siteService = new Services.Cms.SiteService();
                Rock.Models.Cms.Site siteModel = siteService.Get( id );
                if ( siteModel != null )
                {
                    site = new Site();
                    site.Id = siteModel.Id;
                    site.Name = siteModel.Name;
                    site.Description = siteModel.Description;
                    site.Theme = siteModel.Theme;
                    site.DefaultPageId = siteModel.DefaultPageId;
                    site.AppleTouchUrl = siteModel.AppleTouchIconUrl;
                    site.FaviconUrl = siteModel.FaviconUrl;
                    site.FacebookAppId = siteModel.FacebookAppId;
                    site.FacebookAppSecret = siteModel.FacebookAppSecret;

                    Rock.Attribute.Helper.LoadAttributes( siteModel );

                    foreach ( Rock.Cms.Cached.Attribute attribute in siteModel.Attributes )
                        site.AttributeIds.Add( attribute.Id );

                    site.AttributeValues = siteModel.AttributeValues;

                    cache.Set( cacheKey, site, new CacheItemPolicy() );

                    return site;
                }
                else
                    return null;

            }
        }

        /// <summary>
        /// Removes site from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( Site.CacheKey( id ) );
        }

        #endregion
    }
}