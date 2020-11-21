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
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Collection of all entity document type Ids
    /// </summary>
    [Serializable]
    [DataContract]
    public class EntityDocumentTypesCache : ItemCache<EntityDocumentTypesCache>
    {
        private const string KEY = "AllEntityDocumentTypes";

        /// <summary>
        /// Gets or sets the entity document types.
        /// </summary>
        /// <value>
        /// The entity document types.
        /// </value>
		[DataMember]
        public List<EntityDocumentTypes> EntityDocumentTypes { get; private set; }

        /// <summary>
        /// Use Static Get() method to instantiate a new Global Attributes object
        /// </summary>
        private EntityDocumentTypesCache()
        {
        }

        #region Public Methods

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        public static EntityDocumentTypesCache Get()
        {
            return Get( null );
        }

        /// <summary>
        /// Gets the specified rock context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static EntityDocumentTypesCache Get( RockContext rockContext )
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

        private static EntityDocumentTypesCache QueryDb( RockContext rockContext )
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

        private static EntityDocumentTypesCache QueryDbWithContext( RockContext rockContext )
        {
            var entityDocumentTypes = new DocumentTypeService( rockContext )
                .Queryable().AsNoTracking()
                .GroupBy( a => new
                {
                    a.EntityTypeId,
                    a.EntityTypeQualifierColumn,
                    a.EntityTypeQualifierValue
                } )
                .Select( a => new EntityDocumentTypes()
                {
                    EntityTypeId = a.Key.EntityTypeId,
                    EntityTypeQualifierColumn = a.Key.EntityTypeQualifierColumn,
                    EntityTypeQualifierValue = a.Key.EntityTypeQualifierValue,
                    DocumentTypesIds = a.Select( v => v.Id ).ToList()
                } )
                .ToList();

            var value = new EntityDocumentTypesCache { EntityDocumentTypes = entityDocumentTypes };
            return value;

        }

        #endregion

    }

    /// <summary>
    /// Helper Class
    /// </summary>
    [Serializable]
    [DataContract]
    public class EntityDocumentTypes
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
        /// Gets or sets the document type ids.
        /// </summary>
        /// <value>
        /// The document type ids.
        /// </value>
        [DataMember]
        public List<int> DocumentTypesIds { get; set; }
    }
}
