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

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// A registered Rock entity type (an <c>[EntityType]</c> row where
    /// <c>IsEntity</c> is true).
    /// </summary>
    internal class ModelMapEntityType
    {
        /// <summary>
        /// Gets or sets the fully-qualified type name (e.g. "Rock.Model.Person").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the short model name (the <see cref="Name"/> with the
        /// "Rock.Model." prefix removed).
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the entity type's unique identifier.
        /// </summary>
        public Guid Guid { get; set; }
    }
}
