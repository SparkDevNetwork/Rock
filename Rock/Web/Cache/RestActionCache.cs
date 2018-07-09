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
using System.Runtime.Caching;
using System.Runtime.Serialization;

using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a RestAction that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    [Obsolete( "Use Rock.Cache.CacheRestAction instead" )]
    public class RestActionCache : CachedModel<RestAction>
    {
        #region Constructors

        private RestActionCache()
        {
        }

        private RestActionCache( CacheRestAction cacheRestAction )
        {
            CopyFromNewCache( cacheRestAction );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the defined type id.
        /// </summary>
        /// <value>
        /// The defined type id.
        /// </value>
        [DataMember]
        public int ControllerId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string ApiId { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        [DataMember]
        public string Path { get; set; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public RestControllerCache RestController => RestControllerCache.Read( ControllerId );

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority => RestController;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies the model property values to the DTO properties
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is RestAction ) ) return;

            var RestAction = (RestAction)model;
            ControllerId = RestAction.ControllerId;
            Method = RestAction.Method;
            ApiId = RestAction.ApiId;
            Path = RestAction.Path;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheRestAction ) ) return;

            var RestAction = (CacheRestAction)cacheEntity;
            ControllerId = RestAction.ControllerId;
            Method = RestAction.Method;
            ApiId = RestAction.ApiId;
            Path = RestAction.Path;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Path;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns RestAction object from cache.  If RestAction does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static RestActionCache Read( int id, RockContext rockContext = null )
        {
            return new RestActionCache( CacheRestAction.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static RestActionCache Read( Guid guid, RockContext rockContext = null )
        {
            return new RestActionCache( CacheRestAction.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Reads the specified API identifier.
        /// </summary>
        /// <param name="apiId">The API identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static RestActionCache Read( string apiId, RockContext rockContext = null )
        {
            return new RestActionCache( CacheRestAction.Get( apiId ) );
        }

        /// <summary>
        /// Reads the specified defined value model.
        /// </summary>
        /// <param name="restActionModel">The rest action model.</param>
        /// <returns></returns>
        public static RestActionCache Read( RestAction restActionModel )
        {
            return new RestActionCache( CacheRestAction.Get( restActionModel ) );
        }

        /// <summary>
        /// Removes RestAction from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheRestAction.Remove( id );
        }

        /// <summary>
        /// Gets the name of the defined value given an id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static string GetName( int? id )
        {
            return CacheRestAction.GetName( id );
        }

        #endregion

    }
}