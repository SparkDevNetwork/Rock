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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Cache
{
    /// <summary>
    /// Information about a NoteType that is cached by Rock. 
    /// </summary>
    [Serializable]
    [DataContract]
    public class CacheNoteType : ModelCache<CacheNoteType, NoteType>
    {

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
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var noteType = entity as NoteType;
            if ( noteType == null ) return;

            IsSystem = noteType.IsSystem;
            EntityTypeId = noteType.EntityTypeId;
            EntityTypeQualifierColumn = noteType.EntityTypeQualifierColumn;
            EntityTypeQualifierValue = noteType.EntityTypeQualifierValue;
            Name = noteType.Name;
            UserSelectable = noteType.UserSelectable;
            CssClass = noteType.CssClass;
            IconCssClass = noteType.IconCssClass;
            Order = noteType.Order;
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

        #region Entity Note Types Cache

        /// <summary>
        /// Gets the by entity.
        /// </summary>
        /// <param name="entityTypeid">The entity typeid.</param>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <param name="includeNonSelectable">if set to <c>true</c> [include non selectable].</param>
        /// <returns></returns>
        public static List<CacheNoteType> GetByEntity( int? entityTypeid, string entityTypeQualifierColumn, string entityTypeQualifierValue, bool includeNonSelectable = false )
        {
            var allEntityNoteTypes = CacheEntityNoteTypes.Get();

            if ( allEntityNoteTypes == null ) return new List<CacheNoteType>();

            var matchingNoteTypeIds = allEntityNoteTypes.EntityNoteTypes
                .Where( a => a.EntityTypeId.Equals( entityTypeid ) )
                .ToList()
                .Where( a =>
                    ( a.EntityTypeQualifierColumn ?? string.Empty ) == ( entityTypeQualifierColumn ?? string.Empty ) &&
                    ( a.EntityTypeQualifierValue ?? string.Empty ) == ( entityTypeQualifierValue ?? string.Empty ) )
                .SelectMany( a => a.NoteTypeIds )
                .ToList();

            var noteTypes = new List<CacheNoteType>();
            foreach ( var noteTypeId in matchingNoteTypeIds )
            {
                var noteType = Get( noteTypeId );
                if ( noteType != null && ( includeNonSelectable || noteType.UserSelectable ) )
                {
                    noteTypes.Add( noteType );
                }
            }

            return noteTypes;
        }

        /// <summary>
        /// Flushes the entity noteTypes.
        /// </summary>
        public static void RemoveEntityNoteTypes()
        {
            CacheEntityNoteTypes.Remove();
        }

        #endregion
    }


}