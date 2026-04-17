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

namespace Rock.ViewModels.Blocks.Cms.PersonalLinks
{
    /// <summary>
    /// A single personal link section with the links it contains.
    /// </summary>
    public class PersonalLinksSectionBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier of the section.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the display name of the section.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the section is shared across
        /// the system (rather than private to a single person).
        /// </summary>
        public bool IsShared { get; set; }

        /// <summary>
        /// Gets or sets the sort order of the section within the person's popover.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the links contained in this section.
        /// </summary>
        public List<PersonalLinkBag> PersonalLinks { get; set; }
    }
}
