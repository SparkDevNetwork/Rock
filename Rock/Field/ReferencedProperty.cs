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

namespace Rock.Field
{
    /// <summary>
    /// Identifies a single property that should be monitored for changes.
    /// </summary>
    internal class ReferencedProperty
    {
        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>The entity type identifier.</value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; set; }

        public ReferencedProperty( int entityTypeId, string propertyName )
        {
            EntityTypeId = entityTypeId;
            PropertyName = propertyName;
        }
    }
}
