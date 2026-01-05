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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupPlacement
{
    /// <summary>
    /// Represents a collection of available attributes and fee items that can be used to filter placement data.
    /// </summary>
    public class AttributeFiltersBag
    {
        /// <summary>
        /// The set of public attributes available for filtering based on registrant or source group member data.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> SourceAttributesForFilter { get; set; }

        /// <summary>
        /// The set of public group-level attributes available for filtering destination groups.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> DestinationGroupAttributesForFilter { get; set; }

        /// <summary>
        /// The set of public attributes available for filtering destination group members.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> DestinationGroupMemberAttributesForFilter { get; set; }

        /// <summary>
        /// A list of fee item options that can be used to filter registrants based on what fees they selected.
        /// </summary>
        public List<ListItemBag> RegistrantFeeItemsForFilter { get; set; }
    }
}
