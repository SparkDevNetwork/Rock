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
    /// Information about a definedType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    public class DefinedTypeCache : CachedModel<DefinedType>
    {
        #region Constructors

        private DefinedTypeCache() 
        {
        }

        private DefinedTypeCache( DefinedType model )
        {
            CopyFromModel( model );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the field type id.
        /// </summary>
        /// <value>
        /// The field type id.
        /// </value>
        public int? FieldTypeId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

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
        public string Description { get; set; }

        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The type of the field.
        /// </value>
        public FieldTypeCache FieldType
        {
            get 
            { 
                if (FieldTypeId.HasValue)
                    return FieldTypeCache.Read( FieldTypeId.Value );
                return null;
            }
        }

        /// <summary>
        /// Gets the defined values.
        /// </summary>
        /// <value>
        /// The defined values.
        /// </value>
        public List<DefinedValueCache> DefinedValues
        {
            get
            {
                var definedValues = new List<DefinedValueCache>();

                if ( definedValueIds != null )
                {
                    foreach ( int id in definedValueIds.ToList() )
                    {
                        definedValues.Add( DefinedValueCache.Read( id ) );
                    }
                }
                else
                {
                    definedValueIds = new List<int>();

                    var rockContext = new RockContext();
                    var definedValueService = new Model.DefinedValueService( rockContext );
                    foreach ( var definedValue in definedValueService.GetByDefinedTypeId( this.Id ) )
                    {
                        definedValueIds.Add( definedValue.Id );
                        definedValues.Add( DefinedValueCache.Read( definedValue, rockContext ) );
                    }

                }
                return definedValues;
            }
        }
        private List<int> definedValueIds = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is DefinedType )
            {
                var definedType = (DefinedType)model;
                this.IsSystem = definedType.IsSystem;
                this.FieldTypeId = definedType.FieldTypeId;
                this.Order = definedType.Order;
                this.Category = definedType.Category;
                this.Name = definedType.Name;
                this.Description = definedType.Description;

                this.definedValueIds = definedType.DefinedValues
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
            return string.Format( "Rock:DefinedType:{0}", id );
        }

        /// <summary>
        /// Returns DefinedType object from cache.  If definedType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static DefinedTypeCache Read( int id, RockContext rockContext = null )
        {
            string cacheKey = DefinedTypeCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            DefinedTypeCache definedType = cache[cacheKey] as DefinedTypeCache;

            if ( definedType != null )
            {
                return definedType;
            }
            else
            {
                var definedTypeService = new DefinedTypeService( rockContext ?? new RockContext() );
                var definedTypeModel = definedTypeService
                    .Queryable( "DefinedValues" )
                    .Where( t => t.Id == id )
                    .FirstOrDefault();

                if ( definedTypeModel != null )
                {
                    definedTypeModel.LoadAttributes();
                    definedType = new DefinedTypeCache( definedTypeModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, definedType, cachePolicy );
                    cache.Set( definedType.Guid.ToString(), definedType.Id, cachePolicy );

                    return definedType;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static DefinedTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            ObjectCache cache = MemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            if ( cacheObj != null )
            {
                return Read( (int)cacheObj );
            }
            else
            {
                var definedTypeService = new DefinedTypeService( rockContext ?? new RockContext() );
                var definedTypeModel = definedTypeService
                    .Queryable( "DefinedValues" )
                    .Where( t => t.Guid == guid )
                    .FirstOrDefault();

                if ( definedTypeModel != null )
                {
                    definedTypeModel.LoadAttributes();
                    var definedType = new DefinedTypeCache( definedTypeModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( DefinedTypeCache.CacheKey( definedType.Id ), definedType, cachePolicy );
                    cache.Set( definedType.Guid.ToString(), definedType.Id, cachePolicy );

                    return definedType;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="definedTypeModel">The defined type model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static DefinedTypeCache Read( DefinedType definedTypeModel, RockContext rockContext = null )
        {
            string cacheKey = DefinedTypeCache.CacheKey( definedTypeModel.Id );

            ObjectCache cache = MemoryCache.Default;
            DefinedTypeCache definedType = cache[cacheKey] as DefinedTypeCache;

            if ( definedType != null )
            {
                definedType.CopyFromModel( definedTypeModel );
                return definedType;
            }
            else
            {
                definedTypeModel.LoadAttributes( rockContext );
                definedType = new DefinedTypeCache( definedTypeModel );

                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, definedType, cachePolicy );
                cache.Set( definedType.Guid.ToString(), definedType.Id, cachePolicy );

                return definedType;
            }
        }

        /// <summary>
        /// Removes definedType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( DefinedTypeCache.CacheKey( id ) );
        }

        #endregion

     }
}