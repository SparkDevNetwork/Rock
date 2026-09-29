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

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpProjectDetail
{
    /// <summary>
    /// A member attribute inherited from the project's group type (or a group type it inherits
    /// from), displayed read-only in the member attributes section.
    /// </summary>
    public class SignUpProjectInheritedMemberAttributeBag
    {
        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the attribute.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the key of the attribute. Keys of inherited attributes are reserved and
        /// cannot be used by the project's own member attributes.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the attribute.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the group type the attribute is inherited from.
        /// </summary>
        public string InheritedFromGroupTypeName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the detail page of the group type the attribute is inherited
        /// from.
        /// </summary>
        public string InheritedFromGroupTypeUrl { get; set; }
    }
}
