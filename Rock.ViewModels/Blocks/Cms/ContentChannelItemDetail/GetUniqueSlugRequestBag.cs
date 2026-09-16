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

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemDetail
{
    /// <summary>
    /// Payload for the GetUniqueSlug block action (preview only; no write).
    /// </summary>
    public class GetUniqueSlugRequestBag
    {
        /// <summary>
        /// Gets or sets the parent item IdKey (empty for a new item).
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the slug text to make unique within the item's channel.
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the slug row Id to exclude from the uniqueness check, or null for a new row.
        /// </summary>
        public int? ContentChannelItemSlugId { get; set; }
    }
}
