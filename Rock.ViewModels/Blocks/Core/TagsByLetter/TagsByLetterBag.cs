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

namespace Rock.ViewModels.Blocks.Core.TagsByLetter
{
    /// <summary>
    /// Contains the tag data grouped by first letter for the Tags By Letter block.
    /// Used for both initial load and block action responses.
    /// </summary>
    public class TagsByLetterBag
    {
        /// <summary>
        /// Gets or sets the tags grouped by the first letter of their name.
        /// Keys are single characters: A-Z for letters, # for digits, * for other characters.
        /// Each key maps to a list of tag summaries for that letter group.
        /// </summary>
        public Dictionary<string, List<TagItemBag>> TagsByLetter { get; set; }
    }
}
