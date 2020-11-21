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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a RestController that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class RestControllerCache : ModelCache<RestControllerCache, RestController>
    {

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string ClassName { get; private set; }

        /// <summary>
        /// Gets the defined values.
        /// </summary>
        /// <value>
        /// The defined values.
        /// </value>
        public List<RestActionCache> RestActions
        {
            get
            {
                var restActions = new List<RestActionCache>();

                if ( _restActionIds == null )
                {
                    lock ( _obj )
                    {
                        if ( _restActionIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                _restActionIds = new RestActionService( rockContext )
                                    .Queryable()
                                    .Where( a => a.ControllerId == Id )
                                    .Select( a => a.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                if ( _restActionIds == null ) return restActions;

                foreach ( var id in _restActionIds )
                {
                    var restAction = RestActionCache.Get( id );
                    if ( restAction != null )
                    {
                        restActions.Add( restAction );
                    }
                }

                return restActions;
            }
        }
        private List<int> _restActionIds;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var restController = entity as RestController;
            if ( restController == null ) return;

            Name = restController.Name;
            ClassName = restController.ClassName;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets the specified API identifier.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public new static RestControllerCache Get( string className )
        {
            return Get( className, null );
        }

        /// <summary>
        /// Gets the specified API identifier.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static RestControllerCache Get( string className, RockContext rockContext )
        {
            return className.IsNotNullOrWhiteSpace()
                ? GetOrAddExisting( className, () => QueryDbByClassName( className, rockContext ) ) : null;
        }

        /// <summary>
        /// Reads the specified class name.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete("Use Get Instead", true )]
        public static RestControllerCache Read( string className, RockContext rockContext = null )
        {
            return Get( className, rockContext );
        }

        /// <summary>
        /// Queries the database by API identifier.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static RestControllerCache QueryDbByClassName( string className, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return QueryDbByClassNameWithContext( className, rockContext );
            }

            using ( var newRockContext = new RockContext() )
            {
                return QueryDbByClassNameWithContext( className, newRockContext );
            }
        }

        /// <summary>
        /// Queries the database by id with context.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private static RestControllerCache QueryDbByClassNameWithContext( string className, RockContext rockContext )
        {
            var service = new RestControllerService( rockContext );
            var entity = service.Queryable().AsNoTracking()
                .FirstOrDefault( a => a.ClassName == className );

            if ( entity == null ) return null;

            var value = new RestControllerCache();
            value.SetFromEntity( entity );
            return value;
        }

        #endregion

    }
}