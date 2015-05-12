// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Runtime.Caching;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a RestController that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class RestControllerCache : CachedModel<RestController>
    {
        #region Constructors

        private RestControllerCache( RestController model )
        {
            CopyFromModel( model );
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
                            restActionIds = new Model.RestActionService( rockContext )
                                .Queryable()
                                .Where( a => a.ControllerId == this.Id )
                                .Select( a => a.Id )
                                .ToList();
                        }
                    }
                }

                foreach ( int id in restActionIds )
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
        private List<int> restActionIds = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is RestController )
            {
                var RestController = (RestController)model;
                this.Name = RestController.Name;
                this.ClassName = RestController.ClassName;

                SetCache( RestController.ClassName, new Lazy<int>( () => AsLazy( model.Id ) ), new CacheItemPolicy() );
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:RestController:{0}", id );
        }

        /// <summary>
        /// Returns RestController object from cache.  If RestController does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static RestControllerCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( RestControllerCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static RestControllerCache LoadById( int id, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadById2( id, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadById2( id, rockContext2 );
            }
        }

        private static RestControllerCache LoadById2( int id, RockContext rockContext )
        {
            var RestControllerService = new RestControllerService( rockContext );
            var RestControllerModel = RestControllerService.Get( id );
            if ( RestControllerModel != null )
            {
                return new RestControllerCache( RestControllerModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static RestControllerCache Read( Guid guid, RockContext rockContext = null )
        {
            int id = GetOrAddExisting( guid.ToString(),
                () => LoadByGuid( guid, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByGuid( Guid guid, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByGuid2( guid, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByGuid2( guid, rockContext2 );
            }
        }

        private static int LoadByGuid2( Guid guid, RockContext rockContext )
        {
            var RestControllerService = new RestControllerService( rockContext );
            return RestControllerService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified class name.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static RestControllerCache Read( string className, RockContext rockContext = null )
        {
            int id = GetOrAddExisting( className,
                () => LoadByName( className, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByName( string className, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByName2( className, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByName2( className, rockContext2 );
            }
        }

        private static int LoadByName2( string className, RockContext rockContext )
        {
            var RestControllerService = new RestControllerService( rockContext );
            return RestControllerService
                .Queryable().AsNoTracking()
                .Where( a => a.ClassName == className )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="RestControllerModel">The defined type model.</param>
        /// <returns></returns>
        public static RestControllerCache Read( RestController restControllerModel )
        {
            return GetOrAddExisting( RestControllerCache.CacheKey( restControllerModel.Id ),
                () => LoadByModel( restControllerModel ) );
        }

        private static RestControllerCache LoadByModel( RestController restControllerModel )
        {
            if ( restControllerModel != null )
            {
                return new RestControllerCache( restControllerModel );
            }
            return null;
        }

        /// <summary>
        /// Removes RestController from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( RestControllerCache.CacheKey( id ) );
        }

        #endregion

    }
}