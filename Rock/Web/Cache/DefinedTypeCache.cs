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

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a definedType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheDefinedType instead" )]
    public class DefinedTypeCache : CachedModel<DefinedType>
    {
        #region Constructors

        private DefinedTypeCache( CacheDefinedType cacheDefinedType )
        {
            CopyFromNewCache( cacheDefinedType );
        }

        #endregion

        #region Properties

        private readonly object _obj = new object();

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
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        public int? CategoryId { get; set; }

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
        /// Gets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public CategoryCache Category
        {
            get
            {
                if ( CategoryId.HasValue )
                {
                    return CategoryCache.Read( CategoryId.Value );
                }

                return null;
            }
        }

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
                if ( FieldTypeId.HasValue )
                {
                    return FieldTypeCache.Read( FieldTypeId.Value );
                }

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

                lock ( _obj )
                {
                    if ( definedValueIds == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            definedValueIds = new DefinedValueService( rockContext )
                                .GetByDefinedTypeId( Id )
                                .Select( v => v.Id )
                                .ToList();
                        }
                    }
                }

                foreach ( int id in definedValueIds )
                {
                    var definedValue = DefinedValueCache.Read( id );
                    if ( definedValue != null )
                    {
                        definedValues.Add( definedValue );
                    }
                }

                return definedValues;
            }
        }
        private List<int> definedValueIds;

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is DefinedType ) ) return;

            var definedType = (DefinedType)model;
            IsSystem = definedType.IsSystem;
            FieldTypeId = definedType.FieldTypeId;
            Order = definedType.Order;
            CategoryId = definedType.CategoryId;
            Name = definedType.Name;
            Description = definedType.Description;

            // set definedValueIds to null so it load them all at once on demand
            definedValueIds = null;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheDefinedType ) ) return;

            var definedType = (CacheDefinedType)cacheEntity;
            IsSystem = definedType.IsSystem;
            FieldTypeId = definedType.FieldTypeId;
            Order = definedType.Order;
            CategoryId = definedType.CategoryId;
            Name = definedType.Name;
            Description = definedType.Description;

            // set definedValueIds to null so it load them all at once on demand
            definedValueIds = null;
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
        /// Returns DefinedType object from cache.  If definedType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static DefinedTypeCache Read( int id, RockContext rockContext = null )
        {
            return new DefinedTypeCache( CacheDefinedType.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static DefinedTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            return new DefinedTypeCache( CacheDefinedType.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="definedTypeModel">The defined type model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static DefinedTypeCache Read( DefinedType definedTypeModel, RockContext rockContext = null )
        {
            return new DefinedTypeCache( CacheDefinedType.Get( definedTypeModel ) );
        }

        /// <summary>
        /// Removes definedType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheDefinedType.Remove( id );
        }

        #endregion
    }
}