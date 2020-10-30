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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Collection of all entity note type Ids
    /// </summary>
    [Serializable]
    [DataContract]
    public class EntityNoteTypesCache : ItemCache<EntityNoteTypesCache>
    {
        private const string KEY = "AllEntityNoteTypes";

        /// <summary>
        /// Gets or sets the entity note types.
        /// </summary>
        /// <value>
        /// The entity note types.
        /// </value>
		[DataMember]
        public List<EntityNoteTypes> EntityNoteTypes { get; private set; }

        /// <summary>
        /// Use Static Get() method to instantiate a new Global Attributes object
        /// </summary>
        private EntityNoteTypesCache()
        {
        }

        #region Public Methods

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        public static EntityNoteTypesCache Get()
        {
            return Get( null );
        }

        /// <summary>
        /// Gets the specified rock context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityNoteTypesCache Get( RockContext rockContext )
        {
            return GetOrAddExisting( KEY, () => QueryDb( rockContext ) );
        }

        /// <summary>
        /// Removes this instance.
        /// </summary>
        public static void Remove()
        {
            Remove( KEY );
        }

        #endregion

        #region Private Methods 

        private static EntityNoteTypesCache QueryDb( RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return QueryDbWithContext( rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return QueryDbWithContext( rockContext2 );
            }
        }

        private static EntityNoteTypesCache QueryDbWithContext( RockContext rockContext )
        {
            var entityNoteTypes = new NoteTypeService( rockContext )
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

            var value = new EntityNoteTypesCache { EntityNoteTypes = entityNoteTypes };
            return value;

        }

        #endregion

    }

    /// <summary>
    /// Helper Class
    /// </summary>
    [Serializable]
    [DataContract]
    public class EntityNoteTypes
    {
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
        /// Gets or sets the note type ids.
        /// </summary>
        /// <value>
        /// The note type ids.
        /// </value>
        [DataMember]
        public List<int> NoteTypeIds { get; set; }
    }
}
