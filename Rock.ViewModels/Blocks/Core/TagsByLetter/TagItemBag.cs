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

namespace Rock.ViewModels.Blocks.Core.TagsByLetter
{
    /// <summary>
    /// Represents a single tag summary with its name and tagged item count
    /// for display in the Tags By Letter block.
    /// </summary>
    public class TagItemBag
    {
        /// <summary>
        /// Gets or sets the IdKey of the tag, used for constructing
        /// the detail page URL on the client.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the display name of the tag.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the number of items that have been tagged with this tag.
        /// </summary>
        public int Count { get; set; }
    }
}
