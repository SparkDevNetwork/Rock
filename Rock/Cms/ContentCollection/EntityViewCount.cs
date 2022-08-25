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

namespace Rock.Cms.ContentCollection
{
    /// <summary>
    /// The trending view count for a single entity item.
    /// </summary>
    internal class EntityViewCount
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>The entity identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date time the entity is considered active or started.
        /// </summary>
        /// <value>The date time the entity is considered active or started.</value>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the number of views in the time period.
        /// </summary>
        /// <value>The number of views in the time period.</value>
        public int Views { get; set; }
    }
}
