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

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// Selectable family-member address for the Location modal's Member tab. Pairs the
    /// underlying <c>Location</c> with the primary <c>PersonAlias</c> of the family member
    /// who owns the address.
    /// </summary>
    public class FamilyMemberLocationBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the underlying <c>Location</c>.
        /// </summary>
        public Guid LocationGuid { get; set; }

        /// <summary>
        /// Gets or sets the primary <c>PersonAlias.Guid</c> of the member who owns this address.
        /// </summary>
        public Guid PersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the friendly dropdown text, formatted as
        /// <c>"{Member.FullName} {AddressType.Value} ({Address})"</c>.
        /// </summary>
        public string Text { get; set; }
    }
}
