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

using Rock.ViewModels.Crm;

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.BioSummary
{
    /// <summary>
    /// Contains the content displayed by the Person Bio Summary block.
    /// </summary>
    public class BioSummaryBag
    {
        /// <summary>
        /// Gets or sets the URL of the person's profile photo.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the person's display name, including the formal
        /// title prefix when one applies. For a business record this is
        /// the business name.
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the record being viewed
        /// is a business rather than a person.
        /// </summary>
        public bool IsBusiness { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person is deceased.
        /// </summary>
        public bool IsDeceased { get; set; }

        /// <summary>
        /// Gets or sets the display text for the person's account protection
        /// profile. This is null when the alert should not be shown, either
        /// because the profile is low or the viewer lacks permission.
        /// </summary>
        public string AccountProtectionProfileText { get; set; }

        /// <summary>
        /// Gets or sets the other members of the person's family, ordered
        /// for display in the family drop-down.
        /// </summary>
        public List<FamilyMemberBag> FamilyMembers { get; set; }

        /// <summary>
        /// Gets or sets the rendered badge content to display below the
        /// person's name.
        /// </summary>
        public List<RenderedBadgeBag> Badges { get; set; }
    }
}
