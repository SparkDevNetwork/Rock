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
using System;

using Rock.Model;

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// A group requirement inherited from a parent (or grandparent) group type, surfaced
    /// read-only in the "Inherited Requirements" grid on the edit panel.
    /// </summary>
    public class InheritedGroupRequirementBag
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the requirement name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the group role name this requirement applies to. Empty when the
        /// requirement applies to any role.
        /// </summary>
        public string GroupRoleName { get; set; }

        /// <summary>
        /// Gets or sets the age classification this requirement applies to.
        /// </summary>
        public AppliesToAgeClassification AppliesToAgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the name of the group type this requirement was inherited from. May
        /// differ across rows when the inheritance chain spans multiple group types.
        /// </summary>
        public string InheritedFromGroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the URL to the group type this requirement was inherited from.
        /// </summary>
        public string InheritedFromGroupTypeUrl { get; set; }
    }
}
