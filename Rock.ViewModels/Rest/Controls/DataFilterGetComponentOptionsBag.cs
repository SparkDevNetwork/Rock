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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// Options used to get the Obsidian component configuration for a data filter.
    /// </summary>
    public class DataFilterGetComponentOptionsBag
    {
        /// <summary>
        /// The filtered entity type unique identifier.
        /// </summary>
        public Guid EntityTypeGuid { get; set; }

        /// <summary>
        /// The selected data filter type unique identifier.
        /// </summary>
        public Guid FilterTypeGuid { get; set; }

        /// <summary>
        /// The current persisted selection.
        /// </summary>
        public string Selection { get; set; }
    }
}
