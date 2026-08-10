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

namespace Rock.ModelMapBuilder
{
    /// <summary>
    /// A system group type and its system-defined roles.
    /// </summary>
    internal class ModelMapSystemGroupType
    {
        /// <summary>
        /// Gets or sets the group type name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the group type's unique identifier.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the system-defined roles belonging to this group type.
        /// </summary>
        public List<ModelMapGroupTypeRole> Roles { get; set; }
    }
}
