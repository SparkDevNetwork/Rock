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
    /// Payload for the SaveSlug block action (immediate slug commit for existing items).
    /// </summary>
    public class SaveSlugRequestBag
    {
        /// <summary>
        /// Gets or sets the parent item IdKey; used to resolve and authorize EDIT before persisting.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the slug text. Normalized server-side by MakeSlugValid.
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the slug row Id to update (inline edit), or null to create a new row.
        /// </summary>
        public int? ContentChannelItemSlugId { get; set; }
    }
}
