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

namespace Rock.ViewModels.Blocks.Fundraising.FundraisingParticipant
{
    /// <summary>
    /// The details needed to edit a participant's profile.
    /// </summary>
    public class FundraisingParticipantEditBag
    {
        /// <summary>
        /// Gets or sets the title shown above the edit form.
        /// </summary>
        public string ProfileTitle { get; set; }

        /// <summary>
        /// Gets or sets the opportunity date range display text.
        /// </summary>
        public string DateRange { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the participant's photo binary file.
        /// </summary>
        public ListItemBag PhotoBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the URL used when the participant has no photo.
        /// </summary>
        public string NoPictureUrl { get; set; }

        /// <summary>
        /// Gets or sets the group member attributes the participant can edit.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> GroupMemberAttributes { get; set; }

        /// <summary>
        /// Gets or sets the group member attribute values.
        /// </summary>
        public Dictionary<string, string> GroupMemberAttributeValues { get; set; }

        /// <summary>
        /// Gets or sets the person attributes the participant can edit.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> PersonAttributes { get; set; }

        /// <summary>
        /// Gets or sets the person attribute values.
        /// </summary>
        public Dictionary<string, string> PersonAttributeValues { get; set; }
    }
}
