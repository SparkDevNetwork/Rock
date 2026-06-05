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

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// Reference to the group's parent group. <c>Value</c> carries the parent group Guid and
    /// <c>Text</c> carries the parent's friendly name; <see cref="Url"/> carries a pre-resolved
    /// detail link for the view-mode Overview card. The link prefers the Group
    /// <c>EntityType.LinkUrlLavaTemplate</c> (so customer-customized link rules are honored)
    /// and falls back to <c>/Group/{IdKey}</c>.
    /// </summary>
    public class ParentGroupBag : ListItemBag
    {
        /// <summary>
        /// Gets or sets the pre-resolved detail URL, or <c>null</c> when no link can be constructed.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the referenced parent group is currently
        /// active. Defaults to <c>true</c> so an unset value is treated as non-alarming.
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
