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

namespace Rock.ViewModels.Blocks.Group.GroupFinder
{
    /// <summary>
    /// A single group rendered as a card in the Group Finder results.
    /// </summary>
    public class GroupFinderCardBag
    {
        /// <summary>
        /// Gets or sets the group's unique identifier.
        /// </summary>
        public string GroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the card's content HTML, rendered from the configured group card Lava template.
        /// </summary>
        public string ContentHtml { get; set; }
    }

    /// <summary>
    /// A single attribute displayed on a Group Finder card.
    /// </summary>
    public class GroupFinderCardAttributeBag
    {
        /// <summary>
        /// Gets or sets the attribute's display label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the attribute's formatted value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the attribute's icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }
    }
}
