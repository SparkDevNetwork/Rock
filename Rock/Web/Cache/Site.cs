//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

using Rock.CMS;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a site that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class Site : Security.ISecured
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
        public Dictionary<string, KeyValuePair<string, List<Rock.Web.Cache.AttributeValue>>> AttributeValues { get; private set; }

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
        public List<Rock.Web.Cache.Attribute> Attributes
        {
            get
            {
                List<Rock.Web.Cache.Attribute> attributes = new List<Rock.Web.Cache.Attribute>();

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
            Rock.CMS.SiteService siteService = new CMS.SiteService();
            Rock.CMS.Site siteModel = siteService.Get( this.Id );
            if ( siteModel != null )
            {
                Rock.Attribute.Helper.LoadAttributes( siteModel );

                if ( siteModel.Attributes != null )
                    foreach ( var category in siteModel.Attributes )
                        foreach ( var attribute in category.Value )
                            Rock.Attribute.Helper.SaveAttributeValues( siteModel, attribute, this.AttributeValues[attribute.Key].Value, personId );
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
                Rock.CMS.SiteService siteService = new CMS.SiteService();
                Rock.CMS.Site siteModel = siteService.Get( id );
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

                    foreach ( var category in siteModel.Attributes )
                        foreach ( var attribute in category.Value )
                            site.AttributeIds.Add( attribute.Id );

                    site.AttributeValues = siteModel.AttributeValues;

                    site.AuthEntity = siteModel.AuthEntity;
                    site.SupportedActions = siteModel.SupportedActions;

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

        #region ISecure Implementation

        /// <summary>
        /// Gets or sets the auth entity.
        /// </summary>
        /// <value>
        /// The auth entity.
        /// </value>
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
        /// A list of actions that this class supports.
        /// </summary>
        public List<string> SupportedActions { get; set; }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsAuthorized( string action, Rock.CRM.Person person )
        {
            return Security.Authorization.Authorized( this, action, person );
        }

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
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
}