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
    /// Payload for the SaveLink block action, carrying the details of a new personal link.
    /// </summary>
    public class SaveLinkRequestBag
    {
        /// <summary>
        /// Gets or sets the IdKey of the section to add the link to.
        /// When null, a "Links" section is auto-created.
        /// </summary>
        public string SectionIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the link as it will appear in the popover.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL the link points to.
        /// </summary>
        public string Url { get; set; }
    }
}
