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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a NoteType that is cached by Rock. 
    /// </summary>
    [Serializable]
    public class NoteTypeCache : CachedModel<NoteType>
    {
        #region constructors

        private NoteTypeCache( Rock.Model.NoteType model )
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
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [user selectable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [user selectable]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool UserSelectable { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is NoteType )
            {
                var NoteType = (NoteType)model;
                this.IsSystem = NoteType.IsSystem;
                this.EntityTypeId = NoteType.EntityTypeId;
                this.EntityTypeQualifierColumn = NoteType.EntityTypeQualifierColumn;
                this.EntityTypeQualifierValue = NoteType.EntityTypeQualifierValue;
                this.Name = NoteType.Name;
                this.UserSelectable = NoteType.UserSelectable;
                this.CssClass = NoteType.CssClass;
                this.IconCssClass = NoteType.IconCssClass;
                this.Order = NoteType.Order;
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
            return string.Format( "Rock:NoteType:{0}", id );
        }

        /// <summary>
        /// Returns NoteType object from cache.  If NoteType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id of the NoteType to read</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static NoteTypeCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( NoteTypeCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static NoteTypeCache LoadById( int id, RockContext rockContext )
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

        private static NoteTypeCache LoadById2( int id, RockContext rockContext )
        {
            var NoteTypeService = new Rock.Model.NoteTypeService( rockContext );
            var NoteTypeModel = NoteTypeService.Get( id );
            if ( NoteTypeModel != null )
            {
                return new NoteTypeCache( NoteTypeModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static NoteTypeCache Read( Guid guid, RockContext rockContext = null )
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
            var NoteTypeService = new NoteTypeService( rockContext );
            return NoteTypeService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Adds NoteType model to cache, and returns cached object
        /// </summary>
        /// <param name="NoteTypeModel">The NoteTypeModel to cache</param>
        /// <returns></returns>
        public static NoteTypeCache Read( Rock.Model.NoteType NoteTypeModel )
        {
            return GetOrAddExisting( CampusCache.CacheKey( NoteTypeModel.Id ),
                () => LoadByModel( NoteTypeModel ) );
        }

        private static NoteTypeCache LoadByModel( Rock.Model.NoteType NoteTypeModel )
        {
            if ( NoteTypeModel != null )
            {
                return new NoteTypeCache( NoteTypeModel );
            }
            return null;
        }

        /// <summary>
        /// Removes NoteType from cache
        /// </summary>
        /// <param name="id">The id of the NoteType to remove from cache</param>
        public static void Flush( int id )
        {
            FlushCache( NoteTypeCache.CacheKey( id ) );
        }

        #endregion

        #region Entity Note Types Cache

        /// <summary>
        /// The _lock
        /// </summary>
        private static object _lock = new object();

        /// <summary>
        /// Gets or sets all entity note types.
        /// </summary>
        /// <value>
        /// All entity noteTypes.
        /// </value>
        private static List<EntityNoteTypes> AllEntityNoteTypes { get; set; }

        /// <summary>
        /// Gets the by entity.
        /// </summary>
        /// <param name="entityTypeid">The entity typeid.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="includeNonSelectable">if set to <c>true</c> [include non selectable].</param>
        /// <returns></returns>
        public static List<NoteTypeCache> GetByEntity( int? entityTypeid, string entityTypeQualifierColumn, string entityTypeQualifierValue, bool includeNonSelectable = false )
        {
            LoadEntityNoteTypes();

            var matchingNoteTypeIds = AllEntityNoteTypes
                .Where( a =>
                    a.EntityTypeId.Equals( entityTypeid ) &&
                    a.EntityTypeQualifierColumn.Equals( entityTypeQualifierColumn ) &&
                    a.EntityTypeQualifierValue.Equals( entityTypeQualifierValue ) )
                .SelectMany( a => a.NoteTypeIds )
                .ToList();

            var noteTypes = new List<NoteTypeCache>();
            foreach ( int noteTypeId in matchingNoteTypeIds )
            {
                var noteType = NoteTypeCache.Read( noteTypeId );
                if ( noteType != null && ( includeNonSelectable || noteType.UserSelectable ) )
                {
                    noteTypes.Add( noteType );
                }
            }

            return noteTypes;
        }

        /// <summary>
        /// Loads the entity noteTypes.
        /// </summary>
        private static void LoadEntityNoteTypes()
        {
            lock ( _lock )
            {
                if ( AllEntityNoteTypes == null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        AllEntityNoteTypes = new NoteTypeService( rockContext )
                            .Queryable().AsNoTracking()
                            .GroupBy( a => new
                            {
                                a.EntityTypeId,
                                a.EntityTypeQualifierColumn,
                                a.EntityTypeQualifierValue
                            } )
                            .Select( a => new EntityNoteTypes()
                            {
                                EntityTypeId = a.Key.EntityTypeId,
                                EntityTypeQualifierColumn = a.Key.EntityTypeQualifierColumn,
                                EntityTypeQualifierValue = a.Key.EntityTypeQualifierValue,
                                NoteTypeIds = a.Select( v => v.Id ).ToList()
                            } )
                            .ToList();
                    }
                }
            }
        }

        /// <summary>
        /// Flushes the entity noteTypes.
        /// </summary>
        public static void FlushEntityNoteTypes()
        {
            lock ( _lock )
            {
                AllEntityNoteTypes = null;
            }
        }

        #endregion
    }

    #region Helper class for entity note types

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    internal class EntityNoteTypes
    {
        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the note type ids.
        /// </summary>
        /// <value>
        /// The note type ids.
        /// </value>
        public List<int> NoteTypeIds { get; set; }
    }

    #endregion
}