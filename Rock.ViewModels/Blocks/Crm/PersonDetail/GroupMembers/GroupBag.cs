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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.GroupMembers
{
    /// <summary>
    /// Describes one group card displayed by the Group Members block.
    /// </summary>
    public class GroupBag
    {
        /// <summary>
        /// Gets or sets the IdKey identifier of the group.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the group name formatted as HTML title markup.
        /// </summary>
        public string TitleHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved group header Lava HTML, if the block is
        /// configured with header Lava.
        /// </summary>
        public string HeaderHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved group footer Lava HTML, if the block is
        /// configured with footer Lava.
        /// </summary>
        public string FooterHtml { get; set; }

        /// <summary>
        /// Gets or sets the group type name, used in the group attributes
        /// section title.
        /// </summary>
        public string GroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the page used to edit the members of this
        /// group.
        /// </summary>
        public string GroupEditPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the members of the group other than the person being
        /// viewed, in display order.
        /// </summary>
        public List<GroupMemberBag> Members { get; set; }

        /// <summary>
        /// Gets or sets the group attribute values that are always visible.
        /// </summary>
        public List<GroupAttributeBag> GridAttributes { get; set; }

        /// <summary>
        /// Gets or sets the group attribute values that are collapsed behind
        /// the show-more toggle.
        /// </summary>
        public List<GroupAttributeBag> MoreAttributes { get; set; }

        /// <summary>
        /// Gets or sets the addresses of the group, in address type order.
        /// </summary>
        public List<GroupAddressBag> Addresses { get; set; }
    }
}
