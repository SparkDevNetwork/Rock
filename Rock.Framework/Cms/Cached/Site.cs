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

        public int Id { get; private set; }
        public string OrgId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Theme { get; private set; }
        public string FaviconUrl { get; private set; }
        public string AppleTouchUrl { get; private set; }
        public Dictionary<string, KeyValuePair<string, string>> AttributeValues { get; private set; }
        public string FacebookAppId { get; private set; }
        public string FacebookAppSecret { get; private set; }

        private List<int> AttributeIds = new List<int>();
        /// <summary>
        /// List of attributes associated with the site.  This object will not include values.
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

        private int? DefaultPageId;
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

        public void SaveAttributeValues(int? personId)
        {
            Rock.Services.Cms.SiteService siteService = new Services.Cms.SiteService();
            Rock.Models.Cms.Site siteModel = siteService.GetSite( this.Id );
            if ( siteModel != null )
            {
                siteService.LoadAttributes( siteModel );

                if ( siteModel.Attributes != null )
                    foreach ( Rock.Models.Core.Attribute attribute in siteModel.Attributes )
                        siteService.SaveAttributeValue( siteModel, attribute, this.AttributeValues[attribute.Key].Value, personId );
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
        /// <param name="guid"></param>
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
                Rock.Models.Cms.Site siteModel = siteService.GetSite( id );
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

                    siteService.LoadAttributes( siteModel );

                    foreach ( Rock.Models.Core.Attribute attribute in siteModel.Attributes )
                    {
                        site.AttributeIds.Add( attribute.Id );
                        Attribute.Read( attribute );
                    }

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
        /// <param name="guid"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( Site.CacheKey( id ) );
        }

        #endregion
    }
}