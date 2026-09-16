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
    /// Payload for the DeleteSlug block action.
    /// </summary>
    public class DeleteSlugRequestBag
    {
        /// <summary>
        /// Gets or sets the parent item IdKey; used to resolve and authorize EDIT before deleting the slug.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the Id of the slug row to delete. Must belong to the resolved parent item.
        /// </summary>
        public int ContentChannelItemSlugId { get; set; }
    }
}
