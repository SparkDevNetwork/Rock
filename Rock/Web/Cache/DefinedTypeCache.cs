﻿// <copyright>
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

        private DefinedTypeCache( DefinedType model )
        {
            CopyFromModel( model );
        }

        #endregion

        #region Properties

        private object _obj = new object();

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
                            definedValueIds = new Model.DefinedValueService( rockContext )
                                .GetByDefinedTypeId( this.Id )
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
                this.CategoryId = definedType.CategoryId;
                this.Name = definedType.Name;
                this.Description = definedType.Description;

                // set definedValueIds to null so it load them all at once on demand
                this.definedValueIds = null;
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
            return GetOrAddExisting( DefinedTypeCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static DefinedTypeCache LoadById( int id, RockContext rockContext )
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

        private static DefinedTypeCache LoadById2( int id, RockContext rockContext )
        {
            var definedTypeService = new DefinedTypeService( rockContext );
            var definedTypeModel = definedTypeService
                .Queryable()
                .Where( t => t.Id == id )
                .FirstOrDefault();

            if ( definedTypeModel != null )
            {
                definedTypeModel.LoadAttributes( rockContext );
                return new DefinedTypeCache( definedTypeModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static DefinedTypeCache Read( Guid guid, RockContext rockContext = null )
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
            var definedTypeService = new DefinedTypeService( rockContext );
            return definedTypeService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="definedTypeModel">The defined type model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static DefinedTypeCache Read( DefinedType definedTypeModel, RockContext rockContext = null )
        {
            return GetOrAddExisting( DefinedTypeCache.CacheKey( definedTypeModel.Id ),
                () => LoadByModel( definedTypeModel ) );
        }

        private static DefinedTypeCache LoadByModel( DefinedType definedTypeModel )
        {
            if ( definedTypeModel != null )
            {
                return new DefinedTypeCache( definedTypeModel );
            }
            return null;
        }

        /// <summary>
        /// Removes definedType from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            FlushCache( DefinedTypeCache.CacheKey( id ) );
        }

        #endregion
    }
}