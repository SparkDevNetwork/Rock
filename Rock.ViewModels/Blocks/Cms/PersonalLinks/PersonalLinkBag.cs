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

namespace Rock.ViewModels.Blocks.Cms.PersonalLinks
{
    /// <summary>
    /// A single personal link rendered in the popover under a section.
    /// </summary>
    public class PersonalLinkBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier of the link.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the link name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL the link points to.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the sort order of the link within its section.
        /// </summary>
        public int Order { get; set; }
    }
}
