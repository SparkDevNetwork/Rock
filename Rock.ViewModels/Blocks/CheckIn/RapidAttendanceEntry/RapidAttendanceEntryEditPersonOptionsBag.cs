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
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// Everything the Add Person and Edit Person modals need to render: the editable person and the block's
    /// role-dependent configuration. Both the adult and the child configuration are sent so the form can react to
    /// a role change without another round trip.
    /// </summary>
    public class RapidAttendanceEntryEditPersonOptionsBag
    {
        /// <summary>
        /// Gets or sets the editable person. For an add this is a blank person defaulted to the adult role.
        /// </summary>
        public RapidAttendanceEntryEditPersonBag Person { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a new family member is being added rather than an existing
        /// person edited. The family banner is shown only when adding.
        /// </summary>
        public bool IsAdd { get; set; }

        /// <summary>
        /// Gets or sets the family's name, shown in the banner when adding a member.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets the family's home address rendered as formatted HTML, shown in the banner when adding a
        /// member.
        /// </summary>
        public string FamilyAddressFormatted { get; set; }

        /// <summary>
        /// Gets or sets the adult and child family roles offered as the Role radio options.
        /// </summary>
        public List<ListItemBag> RoleItems { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the adult family role, used to tell whether the selected role is
        /// adult or child.
        /// </summary>
        public Guid AdultRoleGuid { get; set; }

        /// <summary>
        /// Gets or sets the phone number types shown when the adult role is selected.
        /// </summary>
        public List<ListItemBag> AdultPhoneTypes { get; set; }

        /// <summary>
        /// Gets or sets the phone number types shown when the child role is selected.
        /// </summary>
        public List<ListItemBag> ChildPhoneTypes { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the mobile phone type, so a newly added mobile number defaults to
        /// having SMS messaging enabled. Null when no mobile type exists.
        /// </summary>
        public Guid? MobilePhoneTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the communication preference field is shown for adults, from
        /// the Adult Communication Preference setting.
        /// </summary>
        public bool IsCommunicationPreferenceShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email field is shown and editable for children, from the
        /// Allow Child Email Edit setting.
        /// </summary>
        public bool IsChildEmailEditAllowed { get; set; }

        /// <summary>
        /// Gets or sets how the race field behaves, from the Race setting: "Hide", "Optional", or "Required".
        /// </summary>
        public string RaceVisibility { get; set; }

        /// <summary>
        /// Gets or sets how the ethnicity field behaves, from the Ethnicity setting: "Hide", "Optional", or
        /// "Required".
        /// </summary>
        public string EthnicityVisibility { get; set; }

        /// <summary>
        /// Gets or sets the configured adult person attributes, shown when the adult role is selected.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> AdultAttributes { get; set; }

        /// <summary>
        /// Gets or sets the configured child person attributes, shown when the child role is selected.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> ChildAttributes { get; set; }
    }
}
