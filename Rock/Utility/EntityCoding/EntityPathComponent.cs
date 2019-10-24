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
using Rock.Data;

namespace Rock.Utility.EntityCoding
{
    /// <summary>
    /// Describes a single element of an <see cref="EntityPath"/>.
    /// </summary>
    public class EntityPathComponent
    {
        /// <summary>
        /// The entity at this specific location in the path.
        /// </summary>
        public IEntity Entity { get; private set; }

        /// <summary>
        /// The name of the property used to reach the next location in the path.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Create a new entity path component.
        /// </summary>
        /// <param name="entity">The entity at this specific location in the path.</param>
        /// <param name="propertyName">The name of the property used to reach the next location in the path.</param>
        public EntityPathComponent( IEntity entity, string propertyName )
        {
            Entity = entity;
            PropertyName = propertyName;
        }
    }
}
