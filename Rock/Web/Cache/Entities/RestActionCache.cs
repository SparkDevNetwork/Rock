﻿// <copyright>
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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

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
    public class RestActionCache : ModelCache<RestActionCache, RestAction>
    {

        #region Properties

        /// <summary>
        /// Gets or sets the defined type id.
        /// </summary>
        /// <value>
        /// The defined type id.
        /// </value>
        [DataMember]
        public int ControllerId { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Method { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string ApiId { get; private set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        [DataMember]
        public string Path { get; private set; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public RestControllerCache RestController => RestControllerCache.Get( ControllerId );

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
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var restAction = entity as RestAction;
            if ( restAction == null ) return;

            ControllerId = restAction.ControllerId;
            Method = restAction.Method;
            ApiId = restAction.ApiId;
            Path = restAction.Path;
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
        /// Reads the specified API identifier.
        /// </summary>
        /// <param name="apiId">The API identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete("Use Get Instead", true )]
        public static RestActionCache Read( string apiId, RockContext rockContext = null )
        {
            return Get( apiId, rockContext );
        }

        /// <summary>
        /// Gets the specified API identifier.
        /// </summary>
        /// <param name="apiId">The API identifier.</param>
        /// <returns></returns>
        public new static RestActionCache Get( string apiId )
        {
            return Get( apiId, null );
        }

        /// <summary>
        /// Gets the specified API identifier.
        /// </summary>
        /// <param name="apiId">The API identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static RestActionCache Get( string apiId, RockContext rockContext )
        {
            return apiId.IsNotNullOrWhiteSpace()
                ? GetOrAddExisting( apiId, () => QueryDbByApiId( apiId, rockContext ) ) : null;
        }

        /// <summary>
        /// Queries the database by API identifier.
        /// </summary>
        /// <param name="apiId">The API identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static RestActionCache QueryDbByApiId( string apiId, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return QueryDbByApiIdWithContext( apiId, rockContext );
            }

            using ( var newRockContext = new RockContext() )
            {
                return QueryDbByApiIdWithContext( apiId, newRockContext );
            }
        }

        /// <summary>
        /// Queries the database by id with context.
        /// </summary>
        /// <param name="apiId">The API identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static RestActionCache QueryDbByApiIdWithContext( string apiId, RockContext rockContext )
        {
            var service = new RestActionService( rockContext );
            var entity = service.Queryable().AsNoTracking()
                .FirstOrDefault( a => a.ApiId == apiId );

            if ( entity == null ) return null;

            var value = new RestActionCache();
            value.SetFromEntity( entity );
            return value;
        }

        /// <summary>
        /// Gets the name of the defined value given an id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static string GetName( int? id )
        {
            if ( !id.HasValue ) return null;

            var restAction = Get( id.Value );
            return restAction?.Method;
        }

        #endregion

    }
}