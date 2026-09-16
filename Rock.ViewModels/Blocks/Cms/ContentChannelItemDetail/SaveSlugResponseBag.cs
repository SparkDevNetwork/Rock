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
    /// Result of the SaveSlug block action.
    /// </summary>
    public class SaveSlugResponseBag
    {
        /// <summary>
        /// Gets or sets the persisted slug row Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the server-normalized slug.
        /// </summary>
        public string Slug { get; set; }
    }
}
