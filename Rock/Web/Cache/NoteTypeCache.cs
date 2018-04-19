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
    /// Information about a NoteType that is cached by Rock. 
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.NoteTypeCache instead" )]
    public class NoteTypeCache : CachedModel<NoteType>
    {
        #region constructors

        private NoteTypeCache( CacheNoteType cacheNoteType )
        {
            CopyFromNewCache( cacheNoteType );
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
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is NoteType ) ) return;

            var NoteType = (NoteType)model;
            IsSystem = NoteType.IsSystem;
            EntityTypeId = NoteType.EntityTypeId;
            EntityTypeQualifierColumn = NoteType.EntityTypeQualifierColumn;
            EntityTypeQualifierValue = NoteType.EntityTypeQualifierValue;
            Name = NoteType.Name;
            UserSelectable = NoteType.UserSelectable;
            CssClass = NoteType.CssClass;
            IconCssClass = NoteType.IconCssClass;
            Order = NoteType.Order;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheNoteType ) ) return;

            var NoteType = (CacheNoteType)cacheEntity;
            IsSystem = NoteType.IsSystem;
            EntityTypeId = NoteType.EntityTypeId;
            EntityTypeQualifierColumn = NoteType.EntityTypeQualifierColumn;
            EntityTypeQualifierValue = NoteType.EntityTypeQualifierValue;
            Name = NoteType.Name;
            UserSelectable = NoteType.UserSelectable;
            CssClass = NoteType.CssClass;
            IconCssClass = NoteType.IconCssClass;
            Order = NoteType.Order;
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
        /// Returns NoteType object from cache.  If NoteType does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id of the NoteType to read</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static NoteTypeCache Read( int id, RockContext rockContext = null )
        {
            return new NoteTypeCache( CacheNoteType.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static NoteTypeCache Read( Guid guid, RockContext rockContext = null )
        {
            return new NoteTypeCache( CacheNoteType.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Adds NoteType model to cache, and returns cached object
        /// </summary>
        /// <param name="NoteTypeModel">The NoteTypeModel to cache</param>
        /// <returns></returns>
        public static NoteTypeCache Read( NoteType NoteTypeModel )
        {
            return new NoteTypeCache( CacheNoteType.Get( NoteTypeModel ) );
        }

        /// <summary>
        /// Removes NoteType from cache
        /// </summary>
        /// <param name="id">The id of the NoteType to remove from cache</param>
        public static void Flush( int id )
        {
            CacheNoteType.Remove( id );
        }

        #endregion

        #region Entity Note Types Cache

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
            var entityNoteTypes = new List<NoteTypeCache>();

            var cacheEntityNoteTypes = CacheNoteType.GetByEntity( entityTypeid, entityTypeQualifierColumn, entityTypeQualifierValue );
            if ( cacheEntityNoteTypes == null ) return entityNoteTypes;

            foreach ( var cacheEntityNoteType in cacheEntityNoteTypes )
            {
                entityNoteTypes.Add( new NoteTypeCache( cacheEntityNoteType ) );
            }

            return entityNoteTypes;
        }

        /// <summary>
        /// Flushes the entity noteTypes.
        /// </summary>
        public static void FlushEntityNoteTypes()
        {
            CacheNoteType.RemoveEntityNoteTypes();
        }

        #endregion
    }

    #region Helper class for entity note types

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.EntityNoteTypes instead" )]
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