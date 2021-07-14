// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Net
{
    /// <summary>
    /// Provides an abstraction from user-code and the incoming request. The user code (such as
    /// a block, page or API callback) does not need to interact directly with any low-level
    /// request objects. This allows for easier testing as well as adding new request types.
    /// </summary>
    public class RockRequestContext
    {
        #region Properties

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <value>
        /// The current user.
        /// </value>
        public virtual UserLogin CurrentUser { get; protected set; }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <value>
        /// The current person.
        /// </value>
        public virtual Person CurrentPerson => CurrentUser?.Person;

        /// <summary>
        /// Gets or sets the root URL path of this request, e.g. https://www.rocksolidchurchdemo.com/
        /// </summary>
        /// <remarks>
        /// May be empty if the request came from a non-web source.
        /// </remarks>
        /// <value>
        /// The root URL path.
        /// </value>
        public virtual string RootUrlPath { get; protected set; }

        /// <summary>
        /// Gets the client information related to the client sending the request.
        /// </summary>
        /// <value>
        /// The client information related to the client sending the request.
        /// </value>
        public virtual ClientInformation ClientInformation { get; protected set; }

        /// <summary>
        /// Gets or sets the page parameters.
        /// </summary>
        /// <value>
        /// The page parameters.
        /// </value>
        internal protected virtual IDictionary<string, string> PageParameters { get; private set; }

        /// <summary>
        /// Gets or sets the context entities.
        /// </summary>
        /// <value>
        /// The context entities.
        /// </value>
        internal protected IDictionary<Type, Lazy<IEntity>> ContextEntities { get; set; }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>
        /// The headers.
        /// </value>
        private IDictionary<string, IEnumerable<string>> Headers { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an empty instance of the <see cref="RockRequestContext"/> class.
        /// </summary>
        internal RockRequestContext()
        {
            PageParameters = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );
            ContextEntities = new Dictionary<Type, Lazy<IEntity>>();
            Headers = new Dictionary<string, IEnumerable<string>>( StringComparer.InvariantCultureIgnoreCase );
            RootUrlPath = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockRequestContext" /> class.
        /// </summary>
        /// <param name="request">The request from an HttpContext load that we will initialize from.</param>
        internal RockRequestContext( HttpRequest request )
        {
            CurrentUser = UserLoginService.GetCurrentUser( true );

            var uri = new Uri( request.Url.ToString() );
            RootUrlPath = uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + request.ApplicationPath;

            ClientInformation = new ClientInformation( request );

            //
            // Setup the page parameters.
            //
            PageParameters = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );
            foreach ( var key in request.QueryString.AllKeys.Where( k => !k.IsNullOrWhiteSpace() ) )
            {
                PageParameters.AddOrReplace( key, request.QueryString[key] );
            }
            foreach ( var kvp in request.RequestContext.RouteData.Values )
            {
                PageParameters.AddOrReplace( kvp.Key, kvp.Value.ToStringSafe() );
            }

            //
            // Setup the headers
            //
            Headers = request.Headers.AllKeys
                .Select( k => new KeyValuePair<string, IEnumerable<string>>( k, request.Headers.GetValues( k ) ) )
                .ToDictionary( kvp => kvp.Key, kvp => kvp.Value, StringComparer.InvariantCultureIgnoreCase );

            //
            // Initialize any context entities found.
            //
            ContextEntities = new Dictionary<Type, Lazy<IEntity>>();
            AddContextEntitiesFromHeaders();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockRequestContext" /> class.
        /// </summary>
        /// <param name="request">The request from an API call that we will initialize from.</param>
        internal RockRequestContext( HttpRequestMessage request )
        {
            CurrentUser = UserLoginService.GetCurrentUser( true );

            var uri = request.RequestUri;
            RootUrlPath = uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped );

            ClientInformation = new ClientInformation( request );

            //
            // Setup the page parameters, only use query string for now. Route
            // parameters don't make a lot of sense with an API call.
            //
            PageParameters = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );
            foreach ( var kvp in request.GetQueryNameValuePairs() )
            {
                PageParameters.AddOrReplace( kvp.Key, kvp.Value );
            }

            //
            // Setup the headers
            //
            Headers = request.Headers.ToDictionary( kvp => kvp.Key, kvp => kvp.Value, StringComparer.InvariantCultureIgnoreCase );

            //
            // Initialize any context entities found.
            //
            ContextEntities = new Dictionary<Type, Lazy<IEntity>>();
            AddContextEntitiesFromHeaders();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the context entities from headers.
        /// </summary>
        protected virtual void AddContextEntitiesFromHeaders()
        {
            foreach ( var kvp in Headers )
            {
                //
                // Skip any header that isn't an entity context header.
                //
                if ( !kvp.Key.StartsWith( "X-ENTITYCONTEXT-", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    continue;
                }

                //
                // Determine the entity type in question.
                //
                var entityName = kvp.Key.Substring( 16 );
                var type = EntityTypeCache.All()
                    .Where( a => a.IsEntity )
                    .FirstOrDefault( a => a.FriendlyName.Equals( entityName, StringComparison.InvariantCultureIgnoreCase ) )
                    ?.GetEntityType();
                string entityKey = kvp.Value.First();

                //
                // If we got an unknown type or no Id/Guid then skip.
                //
                if ( type == null || entityKey.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                //
                // Lazy load the entity so we don't actually load if it is never
                // accessed.
                //
                ContextEntities.AddOrReplace( type, new Lazy<IEntity>( () =>
                {
                    IEntity entity = null;

                    if ( int.TryParse( entityKey, out int entityId ) )
                    {
                        entity = Reflection.GetIEntityForEntityType( type, entityId );
                    }
                    else if ( Guid.TryParse( entityKey, out Guid entityGuid ) )
                    {
                        entity = Reflection.GetIEntityForEntityType( type, entityGuid );
                    }

                    if ( entity != null && entity is IHasAttributes attributedEntity )
                    {
                        Helper.LoadAttributes( attributedEntity );
                    }

                    return entity;
                } ) );
            }
        }

        /// <summary>
        /// Adds the context entities for page.
        /// </summary>
        /// <param name="pageCache">The page cache.</param>
        internal virtual void AddContextEntitiesForPage( PageCache pageCache )
        {
            foreach ( var pageContext in pageCache.PageContexts )
            {
                var entityType = EntityTypeCache.Get( pageContext.Key )?.GetEntityType();
                if ( entityType == null )
                {
                    continue;
                }

                int? contextId = GetPageParameter( pageContext.Value ).AsIntegerOrNull();
                if ( contextId.HasValue )
                {
                    ContextEntities.AddOrReplace( entityType, new Lazy<IEntity>( () =>
                    {
                        return Reflection.GetIEntityForEntityType( entityType, contextId.Value );
                    } ) );
                }

                Guid? contextGuid = GetPageParameter( pageContext.Value ).AsGuidOrNull();
                if ( contextGuid.HasValue )
                {
                    ContextEntities.AddOrReplace( entityType, new Lazy<IEntity>( () =>
                    {
                        return Reflection.GetIEntityForEntityType( entityType, contextGuid.Value );
                    } ) );
                }
            }
        }

        /// <summary>
        /// Sets the page parameters. This is used by things like block actions
        /// so they can update the request with the original page parameters
        /// rather than what is currently on the query string.
        /// </summary>
        /// <param name="parameters">The parameters to use for the page.</param>
        internal virtual void SetPageParameters( IDictionary<string, string> parameters )
        {
            PageParameters = new Dictionary<string, string>( parameters, StringComparer.InvariantCultureIgnoreCase );
        }

        /// <summary>
        /// Gets the page parameter value given it's name.
        /// </summary>
        /// <param name="name">The name of the page parameter to retrieve.</param>
        /// <returns>The text string representation of the page parameter or an empty string if no matching parameter was found.</returns>
        public virtual string GetPageParameter( string name )
        {
            if ( PageParameters.ContainsKey( name ) )
            {
                return PageParameters[name];
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the page parameters.
        /// </summary>
        /// <returns></returns>
        public virtual IDictionary<string, string> GetPageParameters()
        {
            return new Dictionary<string, string>( PageParameters );
        }

        /// <summary>
        /// Gets the entity object given it's type.
        /// </summary>
        /// <typeparam name="T">The IEntity type to retrieve.</typeparam>
        /// <returns>A reference to the IEntity object or null if none was found.</returns>
        public virtual T GetContextEntity<T>() where T : IEntity
        {
            return ( T ) GetContextEntity( typeof( T ) );
        }

        /// <summary>
        /// Gets the entity object given it's type.
        /// </summary>
        /// <returns>A reference to the IEntity object or null if none was found.</returns>
        public virtual IEntity GetContextEntity( Type entityType )
        {
            if ( ContextEntities.ContainsKey( entityType ) )
            {
                return ContextEntities[entityType].Value;
            }

            return default;
        }

        /// <summary>
        /// Gets the common merge fields to be used with a Lava merge process.
        /// </summary>
        /// <param name="currentPersonOverride">The current person override.</param>
        /// <param name="options">The options to use when initializing the merge fields.</param>
        /// <returns>A new dictionary of merge fields.</returns>
        public virtual Dictionary<string, object> GetCommonMergeFields( Person currentPersonOverride = null, CommonMergeFieldsOptions options = null )
        {
            var mergeFields = new Dictionary<string, object>();

            options = options ?? new CommonMergeFieldsOptions();

            if ( options.GetPageContext )
            {
                var contextObjects = new LazyDictionary<string, object>();

                foreach ( var ctx in ContextEntities )
                {
                    contextObjects.Add( ctx.Key.Name, () => ctx.Value.Value );
                }

                if ( contextObjects.Any() )
                {
                    mergeFields.Add( "Context", contextObjects );
                }
            }

            if ( options.GetPageParameters )
            {
                mergeFields.Add( "PageParameter", PageParameters );
            }

            if ( options.GetOSFamily )
            {
                mergeFields.Add( "OSFamily", ClientInformation.Browser.OS.Family.ToLower() );
            }

            if ( options.GetDeviceFamily )
            {
                mergeFields.Add( "DeviceFamily", ClientInformation.Browser.Device.Family );
            }

            var person = currentPersonOverride ?? CurrentPerson;
            if ( options.GetCurrentPerson && person != null )
            {
                mergeFields.Add( "CurrentPerson", person );
            }

            if ( options.GetCampuses )
            {
                mergeFields.Add( "Campuses", CampusCache.All() );
            }

            if ( Headers.ContainsKey( "X-Rock-DeviceData" ) )
            {
                mergeFields.Add( "Device", Headers["X-Rock-DeviceData"].FirstOrDefault().FromJsonOrNull<Common.Mobile.DeviceData>() );
            }

            return mergeFields;
        }

        /// <summary>
        /// Gets the values associated with the specified header.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of all string values associated with the requested header.</returns>
        public virtual IEnumerable<string> GetHeader( string header )
        {
            if ( !Headers.ContainsKey( header ) )
            {
                return new string[0];
            }

            return Headers[header];
        }

        #endregion
    }
}
