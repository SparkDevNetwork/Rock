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
using System.Runtime.Caching;
using System.Runtime.Serialization;
using System.Linq;

using Rock.Model;
using Rock.Data;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a fieldType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class FieldTypeCache : CachedModel<FieldType>
    {
        #region Constructors

        private FieldTypeCache()
        {
        }

        private FieldTypeCache( FieldType fieldType )
        {
            CopyFromModel( fieldType );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the assembly.
        /// </summary>
        /// <value>
        /// The assembly.
        /// </value>
        [DataMember]
        public string Assembly { get; set; }

        /// <summary>
        /// Gets or sets the class.
        /// </summary>
        /// <value>
        /// The class.
        /// </value>
        [DataMember]
        public string Class { get; set; }

        /// <summary>
        /// Gets the field 
        /// </summary>
        [DataMember]
        public Rock.Field.IFieldType Field { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is FieldType )
            {
                var fieldType = (FieldType)model;
                this.IsSystem = fieldType.IsSystem;
                this.Name = fieldType.Name;
                this.Description = fieldType.Description;
                this.Assembly = fieldType.Assembly;
                this.Class = fieldType.Class;

                this.Field = Rock.Field.Helper.InstantiateFieldType( fieldType.Assembly, fieldType.Class );
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

        /// <summary>
        /// All the field types
        /// </summary>
        /// <returns></returns>
        public static List<FieldTypeCache> All()
        {
            string cacheKey = "Rock:FieldType:All";

            ObjectCache cache = MemoryCache.Default;
            var allfieldTypes = cache[cacheKey] as List<FieldTypeCache>;
            if ( allfieldTypes == null )
            {
                allfieldTypes = new List<FieldTypeCache>();
                foreach ( var fieldType in new FieldTypeService( new RockContext() ).Queryable()
                    .OrderBy( f => f.Name ) )
                {
                    allfieldTypes.Add( Read( fieldType ) );
                }

                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, allfieldTypes, cachePolicy );
            }

            return allfieldTypes;
        }

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:FieldType:{0}", id );
        }

        /// <summary>
        /// Returns FieldType object from cache.  If fieldType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static FieldTypeCache Read( int id, RockContext rockContext = null )
        {
            string cacheKey = FieldTypeCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            FieldTypeCache fieldType = cache[cacheKey] as FieldTypeCache;

            if ( fieldType == null )
            {
                rockContext = rockContext ?? new RockContext();
                var fieldTypeService = new FieldTypeService( rockContext );
                var fieldTypeModel = fieldTypeService.Get( id );
                if ( fieldTypeModel != null )
                {
                    fieldType = new FieldTypeCache( fieldTypeModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, fieldType, cachePolicy );
                    cache.Set( fieldType.Guid.ToString(), fieldType.Id, cachePolicy );
                }
            }

            return fieldType;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static FieldTypeCache Read( string guid, RockContext rockContext = null )
        {
            return Read( new Guid( guid ), rockContext );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static FieldTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            ObjectCache cache = MemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            FieldTypeCache fieldType = null;
            if ( cacheObj != null )
            {
                fieldType = Read( (int)cacheObj, rockContext );
            }

            if ( fieldType == null )
            {
                rockContext = rockContext ?? new RockContext();
                var fieldTypeService = new FieldTypeService( rockContext );
                var fieldTypeModel = fieldTypeService.Get( guid );
                if ( fieldTypeModel != null )
                {
                    fieldType = new FieldTypeCache( fieldTypeModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( FieldTypeCache.CacheKey( fieldType.Id ), fieldType, cachePolicy );
                    cache.Set( fieldType.Guid.ToString(), fieldType.Id, cachePolicy );
                }
            }

            return fieldType;
        }

        /// <summary>
        /// Reads the specified field type model.
        /// </summary>
        /// <param name="fieldTypeModel">The field type model.</param>
        /// <returns></returns>
        public static FieldTypeCache Read( FieldType fieldTypeModel )
        {
            string cacheKey = FieldTypeCache.CacheKey( fieldTypeModel.Id );
            ObjectCache cache = MemoryCache.Default;
            FieldTypeCache fieldType = cache[cacheKey] as FieldTypeCache;

            if ( fieldType != null )
            {
                fieldType.CopyFromModel( fieldTypeModel );
            }
            else
            {
                fieldType = new FieldTypeCache( fieldTypeModel );
                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, fieldType, cachePolicy );
                cache.Set( fieldType.Guid.ToString(), fieldType.Id, cachePolicy );
            }

            return fieldType;
        }

        /// <summary>
        /// Removes fieldType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( FieldTypeCache.CacheKey( id ) );
            cache.Remove( "Rock:FieldType:All" );
        }

        #endregion
    }
}