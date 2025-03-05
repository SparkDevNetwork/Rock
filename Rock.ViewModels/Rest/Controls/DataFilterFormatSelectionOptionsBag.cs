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
    /// The options that can be passed to the FormatSelection API action
    /// of the DataFilter control (this is not a real control). This is
    /// used by the Obsidian data filters so that the UI can update the
    /// formatted description of the filter settings.
    /// </summary>
    public class DataFilterFormatSelectionOptionsBag
    {
        /// <summary>
        /// The data from the UI component that needs to be formatted.
        /// </summary>
        public string ComponentData { get; set; }

        /// <summary>
        /// The unique identifier of the entity type that the filter applies to.
        /// </summary>
        public Guid EntityTypeGuid { get; set; }

        /// <summary>
        /// The unique identifier of the entity type that represents the filter
        /// class.
        /// </summary>
        public Guid FilterTypeGuid { get; set; }

        /// <summary>
        /// The security grant token to use when authorizing the request.
        /// </summary>
        public string SecurityGrantToken { get; set; }
    }
}

