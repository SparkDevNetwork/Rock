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

namespace Rock.ViewModels.Blocks.Group.GroupTypeDetail
{
    /// <summary>
    /// Response bag for the default GroupTypeRole attributes and attribute values
    /// used when initializing a new role in the UI.
    /// </summary>
    public class GroupTypeRoleDefaultAttributesResponseBag : IViewModelWithAttributes
    {
        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        public Dictionary<string, PublicAttributeBag> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}

