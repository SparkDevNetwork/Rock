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

                if ( restActionIds != null )
                {
                    foreach ( int id in restActionIds.ToList() )
                    {
                        var restAction = RestActionCache.Read( id );
                        if ( restAction != null )
                        {
                            restActions.Add( restAction );
                        }
                    }
                }
                else
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var restActionService = new Model.RestActionService( rockContext );
                        var restActionModels = restActionService.Queryable()
                            .Where( a => a.ControllerId == this.Id )
                            .ToList();

                        restActionIds = restActionModels.Select( a => a.Id ).ToList();

                        foreach ( var restAction in restActionModels )
                        {
                            restActions.Add( RestActionCache.Read( restAction ) );
                        }
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
                var restController = (RestController)model;
                this.Name = restController.Name;
                this.ClassName = restController.ClassName;

                this.restActionIds = restController.Actions
                    .Select( v => v.Id ).ToList();
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
            string cacheKey = RestControllerCache.CacheKey( id );

            ObjectCache cache = RockMemoryCache.Default;
            RestControllerCache restController = cache[cacheKey] as RestControllerCache;

            if ( restController == null )
            {
                if ( rockContext != null )
                {
                    restController = LoadById( id, rockContext );
                }
                else
                {
                    using ( var myRockContext = new RockContext() )
                    {
                        restController = LoadById( id, myRockContext );
                    }
                }

                if ( restController != null )
                {
                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, restController, cachePolicy );
                    cache.Set( restController.Guid.ToString(), restController.Id, cachePolicy );
                }
            }

            return restController;
        }

        private static RestControllerCache LoadById( int id, RockContext rockContext )
        {
            var restControllerService = new RestControllerService( rockContext );
            var restControllerModel = restControllerService.Get( id );
            if ( restControllerModel != null )
            {
                return new RestControllerCache( restControllerModel );
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
            ObjectCache cache = RockMemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            RestControllerCache restController = null;
            if ( cacheObj != null )
            {
                restController = Read( (int)cacheObj, rockContext );
            }

            if ( restController == null )
            {
                if ( rockContext != null )
                {
                    restController = LoadByGuid( guid, rockContext );
                }
                else
                {
                    using ( var myRockContext = new RockContext() )
                    {
                        restController = LoadByGuid( guid, myRockContext );
                    }
                }

                if ( restController != null )
                {
                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( RestControllerCache.CacheKey( restController.Id ), restController, cachePolicy );
                    cache.Set( restController.Guid.ToString(), restController.Id, cachePolicy );
                }
            }

            return restController;
        }

        private static RestControllerCache LoadByGuid( Guid guid, RockContext rockContext )
        {
            var restControllerService = new RestControllerService( rockContext );
            var restControllerModel = restControllerService
                .Queryable( "RestActions" )
                .Where( t => t.Guid == guid )
                .FirstOrDefault();
            if ( restControllerModel != null )
            {
                return new RestControllerCache( restControllerModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified class name.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static RestControllerCache Read( string className, RockContext rockContext = null )
        {
            ObjectCache cache = RockMemoryCache.Default;
            object cacheObj = cache[className];

            RestControllerCache restController = null;
            if ( cacheObj != null )
            {
                restController = Read( (int)cacheObj, rockContext );
            }

            if ( restController == null )
            {
                if ( rockContext != null )
                {
                    restController = LoadByClassName( className, rockContext );
                }
                else
                {
                    using ( var myRockContext = new RockContext() )
                    {
                        restController = LoadByClassName( className, myRockContext );
                    }
                }

                if ( restController != null )
                {
                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( RestControllerCache.CacheKey( restController.Id ), restController, cachePolicy );
                    cache.Set( className, restController.Id, cachePolicy );
                }
            }

            return restController;
        }

        private static RestControllerCache LoadByClassName( string className, RockContext rockContext )
        {
            var restControllerService = new RestControllerService( rockContext );
            var restControllerModel = restControllerService.Queryable()
                .Where( a => a.ClassName == className )
                .FirstOrDefault();
            if ( restControllerModel != null )
            {
                return new RestControllerCache( restControllerModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="restControllerModel">The defined type model.</param>
        /// <returns></returns>
        public static RestControllerCache Read( RestController restControllerModel )
        {
            string cacheKey = RestControllerCache.CacheKey( restControllerModel.Id );
            ObjectCache cache = RockMemoryCache.Default;
            RestControllerCache restController = cache[cacheKey] as RestControllerCache;

            if ( restController != null )
            {
                restController.CopyFromModel( restControllerModel );
            }
            else
            {
                restController = new RestControllerCache( restControllerModel );
                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, restController, cachePolicy );
                cache.Set( restController.Guid.ToString(), restController.Id, cachePolicy );
            }

            return restController;
        }

        /// <summary>
        /// Removes RestController from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = RockMemoryCache.Default;
            cache.Remove( RestControllerCache.CacheKey( id ) );
        }

        #endregion
    }
}