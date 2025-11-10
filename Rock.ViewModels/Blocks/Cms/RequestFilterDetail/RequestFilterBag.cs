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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.RequestFilterDetail
{
    /// <summary>
    /// The item details for the Request Filter Detail block.
    /// </summary>
    public class RequestFilterBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the filter configuration bag.
        /// </summary>
        public AdditionalFilterConfigurationBag AdditionalFilterConfiguration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the request filter key.
        /// </summary>
        public string RequestFilterKey { get; set; }

        /// <summary>
        /// Gets or sets the site
        /// </summary>
        public ListItemBag Site { get; set; }

        /// <summary>
        /// Gets or sets the site identifier.
        /// </summary>
        public int? SiteId { get; set; }
    }
}
