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
using System.Runtime.Serialization;

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a fieldType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    [Obsolete( "Use Rock.Cache.CacheFieldType instead" )]
    public class FieldTypeCache : CachedModel<FieldType>
    {
        #region Constructors

        private FieldTypeCache()
        {
        }

        private FieldTypeCache( CacheFieldType cacheFieldType )
        {
            CopyFromNewCache( cacheFieldType );
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
        public Field.IFieldType Field { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is FieldType ) ) return;

            var fieldType = (FieldType)model;
            IsSystem = fieldType.IsSystem;
            Name = fieldType.Name;
            Description = fieldType.Description;
            Assembly = fieldType.Assembly;
            Class = fieldType.Class;

            Field = Rock.Field.Helper.InstantiateFieldType( fieldType.Assembly, fieldType.Class );
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheFieldType ) ) return;

            var fieldType = (CacheFieldType)cacheEntity;
            IsSystem = fieldType.IsSystem;
            Name = fieldType.Name;
            Description = fieldType.Description;
            Assembly = fieldType.Assembly;
            Class = fieldType.Class;

            Field = Rock.Field.Helper.InstantiateFieldType( fieldType.Assembly, fieldType.Class );
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
        /// Returns FieldType object from cache.  If fieldType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static FieldTypeCache Read( int id, RockContext rockContext = null )
        {
            return new FieldTypeCache( CacheFieldType.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static FieldTypeCache Read( string guid, RockContext rockContext = null )
        {
            return new FieldTypeCache( CacheFieldType.Get( guid ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static FieldTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            return new FieldTypeCache( CacheFieldType.Get( guid ) );
        }

        /// <summary>
        /// Reads this instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static FieldTypeCache Read<T>() where T : Field.IFieldType
        {
            return new FieldTypeCache( CacheFieldType.Get<T>() );
        }

        /// <summary>
        /// Reads the specified field type model.
        /// </summary>
        /// <param name="fieldTypeModel">The field type model.</param>
        /// <returns></returns>
        public static FieldTypeCache Read( FieldType fieldTypeModel )
        {
            return new FieldTypeCache( CacheFieldType.Get( fieldTypeModel ) );
        }

        /// <summary>
        /// All the field types
        /// </summary>
        /// <returns></returns>
        public static List<FieldTypeCache> All()
        {
            var fieldTypes = new List<FieldTypeCache>();

            var cacheFieldTypes = CacheFieldType.All();
            if ( cacheFieldTypes == null ) return fieldTypes;

            foreach ( var cacheFieldType in cacheFieldTypes )
            {
                fieldTypes.Add( new FieldTypeCache( cacheFieldType ) );
            }

            return fieldTypes;
        }

        /// <summary>
        /// Removes fieldType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheFieldType.Remove( id );
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static string GetName( int id )
        {
            return CacheFieldType.GetName( id );
        }

        #endregion
    }
}