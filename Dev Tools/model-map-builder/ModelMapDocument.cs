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
    /// The root of the generated model map JSON file: run metadata plus every
    /// Rock model grouped by domain.
    /// </summary>
    internal class ModelMapDocument
    {
        /// <summary>
        /// Gets or sets the UTC date and time the map was generated.
        /// </summary>
        public DateTime GeneratedAtUtc { get; set; }

        /// <summary>
        /// Gets or sets the semantic Rock version the map was generated from (e.g. "1.16.0").
        /// </summary>
        public string RockVersion { get; set; }

        /// <summary>
        /// Gets or sets the domains, each containing its models.
        /// </summary>
        public List<ModelMapDomain> Domains { get; set; }

        /// <summary>
        /// Gets or sets every registered entity type (name, model, guid).
        /// </summary>
        public List<ModelMapEntityType> EntityTypes { get; set; }

        /// <summary>
        /// Gets or sets the system defined types and their system-defined values.
        /// </summary>
        public List<ModelMapSystemDefinedType> SystemDefinedTypes { get; set; }

        /// <summary>
        /// Gets or sets the system group types and their system-defined roles.
        /// </summary>
        public List<ModelMapSystemGroupType> SystemGroupTypes { get; set; }
    }
}
