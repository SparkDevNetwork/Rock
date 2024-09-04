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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The GroupRequirement category information used by the GetData API action of
    /// the GroupMemberRequirementContainer control.
    /// </summary>
    public class GroupMemberRequirementCategoryBag
    {
        /// <summary>
        /// Identifier for the category
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Name of the category
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of requirement information in this category
        /// </summary>
        public List<GroupMemberRequirementCardConfigBag> MemberRequirements { get; set; }
    }
}
