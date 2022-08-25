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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to pick multiple Binary Files Types
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.BINARY_FILE_TYPES )]
    public class BinaryFileTypesFieldType : SelectFromListFieldType, IEntityReferenceFieldType
    {
        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal override Dictionary<string, string> GetListSource( Dictionary<string, ConfigurationValue> configurationValues )
        {
            return new BinaryFileTypeService( new RockContext() ).Queryable().OrderBy( a => a.Name ).ToDictionary( c => c.Guid.ToString(), c => c.Name );
        }

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var valueGuidList = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();

            if ( !valueGuidList.Any() )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var fileTypeIds = new BinaryFileTypeService( rockContext )
                    .Queryable()
                    .Where( bft => valueGuidList.Contains( bft.Guid ) )
                    .Select( b => b.Id )
                    .ToList();

                if ( !fileTypeIds.Any() )
                {
                    return null;
                }

                var referencedEntities = new List<ReferencedEntity>();
                foreach ( var binaryFileTypeId in fileTypeIds )
                {
                    referencedEntities.Add( new ReferencedEntity( EntityTypeCache.GetId<BinaryFileType>().Value, binaryFileTypeId ) );
                }

                return referencedEntities;
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<BinaryFileType>().Value, nameof( BinaryFileType.Name ) ),
            };
        }

        #endregion
    }
}