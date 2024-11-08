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

namespace Rock.ViewModels.Blocks.Cms.PersonalLinkList
{
    /// <summary>
    /// The additional configuration options for the Personal Link List block.
    /// </summary>
    public class PersonalLinkListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block should be rendered.
        /// This is based on whether or not the provided Personal Link Section exists.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the block visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets the name of the personal link section.
        /// </summary>
        /// <value>
        /// The name of the personal link section.
        /// </value>
        public string PersonalLinkSectionName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the personal link section shared.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the is personal link section shared; otherwise, <c>false</c>.
        /// </value>
        public bool IsPersonalLinkSectionShared { get; set; }
    }
}
