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
using System.Collections.Generic;

namespace Rock.Field
{
    /// <summary>
    /// A field that that references entities. These entities will be monitored
    /// for changes. When a change is detected then the persisted values will
    /// automatically update.
    /// </summary>
    internal interface IEntityReferenceFieldType
    {
        /// <summary>
        /// Gets the referenced entities for the given raw value.
        /// </summary>
        /// <param name="privateValue">The private database value that will be associated with the entities.</param>
        /// <param name="privateConfigurationValues">The private configuration values that describe the field type settings.</param>
        /// <returns>A list of <see cref="ReferencedEntity"/> objects that identify which entities this value depends on.</returns>
        List<ReferencedEntity> GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues );

        /// <summary>
        /// Gets property (database column) names that will trigger an update of
        /// the persisted values when they change.
        /// </summary>
        /// <param name="privateConfigurationValues">The private configuration values that describe the field type settings.</param>
        /// <returns>A dictionary whose key is the entity type identifier and the values are a list of property names on that entity type to be monitored.</returns>
        List<ReferencedProperty> GetReferencedProperties( Dictionary<string, string> privateConfigurationValues );
    }
}
