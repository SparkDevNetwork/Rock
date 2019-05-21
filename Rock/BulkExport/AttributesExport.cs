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

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.BulkExport
{
    /// <summary>
    /// 
    /// </summary>
    [RockClientInclude( "Export of Person record Attributes from ~/api/People/Export" )]
    public class AttributesExport
    {
        /// <summary>
        /// A Dictionary of AttributeKey and AttributeValue <see cref="AttributeReturnType"/>
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        [DataMember]
        public Dictionary<string, object> AttributeValues { get; set; }

        /// <summary>
        /// Gets the attributes from attribute keys.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attributeKeys">The attribute keys.</param>
        /// <returns></returns>
        public static List<AttributeCache> GetAttributesFromAttributeKeys<T>( string attributeKeys ) where T : Rock.Data.IModel
        {
            List<AttributeCache> attributeList = null;

            if ( attributeKeys.IsNotNullOrWhiteSpace() )
            {
                var rockContext = new RockContext();
                var entityTypeId = EntityTypeCache.Get<T>().Id;
                var attributesQry = new AttributeService( rockContext ).Queryable().Where( a => a.EntityTypeId == entityTypeId );
                if ( attributeKeys.Equals( "all", StringComparison.OrdinalIgnoreCase ) )
                {
                    // include all attributes
                }
                else
                {
                    // limit
                    string[] attributeKeyList = attributeKeys?.Split( new char[] { ',' } );
                    attributesQry = attributesQry.Where( a => attributeKeyList.Contains( a.Key ) );
                }

                attributeList = attributesQry.ToAttributeCacheList();
            }

            return attributeList;
        }

        /// <summary>
        /// Loads the attribute values
        /// </summary>
        /// <param name="exportOptions">The export options.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="modelExportList">The model export list.</param>
        /// <param name="pagedEntityQry">The paged entity query.</param>
        public static void LoadAttributeValues( ExportOptions exportOptions, RockContext rockContext, IEnumerable<ModelExport> modelExportList, IQueryable<IEntity> pagedEntityQry )
        {
            if ( exportOptions.AttributeList?.Any() != true )
            {
                return;
            }

            var attributeIdsList = exportOptions.AttributeList.Select( a => a.Id ).ToList();
            var attributeValuesQuery = new AttributeValueService( rockContext ).Queryable()
                .Where( a => attributeIdsList.Contains( a.AttributeId ) )
                .Where( a => pagedEntityQry.Any( p => p.Id == a.EntityId.Value ) )
                .Select( a => new
                {
                    EntityId = a.EntityId.Value,
                    AttributeId = a.AttributeId,
                    AttributeValue = a.Value
                } );

            var attributeValuesList = attributeValuesQuery.ToList();

            var attributeValuesLookup = attributeValuesList.GroupBy( a => a.EntityId ).ToDictionary( k => k.Key, v => v.Select( a => new AttributeValueCache { AttributeId = a.AttributeId, EntityId = a.EntityId, Value = a.AttributeValue } ) );
            Dictionary<string, object> defaultAttributeValues;
            if ( exportOptions.AttributeReturnType == AttributeReturnType.Formatted )
            {
                defaultAttributeValues = exportOptions.AttributeList.ToDictionary( k => k.Key, v => ( object ) v.DefaultValueAsFormatted );
            }
            else
            {
                defaultAttributeValues = exportOptions.AttributeList.ToDictionary( k => k.Key, v => v.DefaultValueAsType );
            }

            foreach ( var modelExport in modelExportList )
            {
                var databaseAttributeValues = attributeValuesLookup.GetValueOrNull( modelExport.Id );
                modelExport.AttributesExport = new AttributesExport();

                // initialize with DefaultValues 
                modelExport.AttributesExport.AttributeValues = new Dictionary<string, object>( defaultAttributeValues );

                // update with values specific to Person
                if ( databaseAttributeValues?.Any() == true )
                {
                    foreach ( var databaseAttributeValue in databaseAttributeValues )
                    {
                        var attributeCache = AttributeCache.Get( databaseAttributeValue.AttributeId );
                        if ( exportOptions.AttributeReturnType == AttributeReturnType.Formatted )
                        {
                            modelExport.AttributesExport.AttributeValues[attributeCache.Key] = databaseAttributeValue.ValueFormatted;
                        }
                        else
                        {
                            modelExport.AttributesExport.AttributeValues[attributeCache.Key] = databaseAttributeValue.ValueAsType;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum AttributeReturnType
    {
        /// <summary>
        /// The raw attribute value
        /// </summary>
        Raw,

        /// <summary>
        /// The formatted attribute value
        /// </summary>
        Formatted
    }
}
