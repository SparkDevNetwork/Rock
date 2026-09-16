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

using Rock.ViewModels.Controls;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The component metadata and initial data required to render a data filter editor.
    /// </summary>
    public class DataFilterGetComponentResultsBag
    {
        /// <summary>
        /// The display title of the filter.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The optional descriptive text of the filter.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The dynamic component definition.
        /// </summary>
        public DynamicComponentDefinitionBag ComponentDefinition { get; set; }

        /// <summary>
        /// The initial component data.
        /// </summary>
        public Dictionary<string, string> ComponentData { get; set; }

        /// <summary>
        /// The effective persisted selection used to initialize the component.
        /// </summary>
        public string Selection { get; set; }

        /// <summary>
        /// The formatted description of the current selection.
        /// </summary>
        public string FormattedSelection { get; set; }
    }
}
