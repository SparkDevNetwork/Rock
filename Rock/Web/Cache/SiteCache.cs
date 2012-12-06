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
    public class SiteCache : SiteDto
    {
        private SiteCache() : base() { }
        private SiteCache( Rock.Model.Site model ) : base( model ) { }

        /// <summary>
        /// Gets the attribute values.
        /// </summary>
        public Dictionary<string, List<Rock.Model.AttributeValueDto>> AttributeValues { get; private set; }

        /// <summary>
        /// Gets a list of attributes associated with the site.  This object will not include values.
        /// To get values associated with the current site instance, use the AttributeValues
        /// </summary>
        public List<Rock.Web.Cache.AttributeCache> Attributes
        {
            get
            {
                List<Rock.Web.Cache.AttributeCache> attributes = new List<Rock.Web.Cache.AttributeCache>();

                foreach ( int id in AttributeIds )
                    attributes.Add( AttributeCache.Read( id ) );

                return attributes;
            }
        }
        private List<int> AttributeIds = new List<int>();

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

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        /// <param name="personId">The person id.</param>
        public void SaveAttributeValues(int? personId)
        {
            Rock.Model.SiteService siteService = new Model.SiteService();
            Rock.Model.Site siteModel = siteService.Get( this.Id );
            if ( siteModel != null )
            {
                siteModel.LoadAttributes();

                if ( siteModel.Attributes != null )
                    foreach ( var attribute in siteModel.Attributes )
                        Rock.Attribute.Helper.SaveAttributeValues( siteModel, attribute.Value, this.AttributeValues[attribute.Key], personId );
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
                return site;
            else
            {
                Rock.Model.SiteService siteService = new Model.SiteService();
                Rock.Model.Site siteModel = siteService.Get( id );
                if ( siteModel != null )
                {
                    site = new SiteCache( siteModel );

                    siteModel.LoadAttributes();

                    foreach ( var attribute in siteModel.Attributes )
                        site.AttributeIds.Add( attribute.Value.Id );

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
            cache.Remove( SiteCache.CacheKey( id ) );
        }

        #endregion

    }
}