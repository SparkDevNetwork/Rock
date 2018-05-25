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
using System.Runtime.Caching;

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a RestController that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheRestController instead" )]
    public class RestControllerCache : CachedModel<RestController>
    {
        #region Constructors

        private RestControllerCache( CacheRestController cacheRestController )
        {
            CopyFromNewCache( cacheRestController );
        }

        #endregion

        #region Properties

        private object _obj = new object();

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
        public string ClassName { get; set; }

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

                lock ( _obj )
                {
                    if ( restActionIds == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            restActionIds = new RestActionService( rockContext )
                                .Queryable()
                                .Where( a => a.ControllerId == Id )
                                .Select( a => a.Id )
                                .ToList();
                        }
                    }
                }

                foreach ( var id in restActionIds )
                {
                    var restAction = RestActionCache.Read( id );
                    if ( restAction != null )
                    {
                        restActions.Add( restAction );
                    }
                }

                return restActions;
            }
        }
        private List<int> restActionIds;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is RestController ) ) return;

            var RestController = (RestController)model;
            Name = RestController.Name;
            ClassName = RestController.ClassName;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheRestController ) ) return;

            var RestController = (CacheRestController)cacheEntity;
            Name = RestController.Name;
            ClassName = RestController.ClassName;
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
        /// Returns RestController object from cache.  If RestController does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static RestControllerCache Read( int id, RockContext rockContext = null )
        {
            return new RestControllerCache( CacheRestController.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static RestControllerCache Read( Guid guid, RockContext rockContext = null )
        {
            return new RestControllerCache( CacheRestController.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Reads the specified class name.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static RestControllerCache Read( string className, RockContext rockContext = null )
        {
            return new RestControllerCache( CacheRestController.Get( className ) );
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="restControllerModel">The rest controller model.</param>
        /// <returns></returns>
        public static RestControllerCache Read( RestController restControllerModel )
        {
            return new RestControllerCache( CacheRestController.Get( restControllerModel ) );
        }

        /// <summary>
        /// Removes RestController from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheRestController.Remove( id );
        }

        #endregion

    }
}