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
using Rock.ViewModels.Controls;

namespace Rock.ViewModels.Blocks.Crm.FamilyPreRegistration
{
    /// <summary>
    /// The bag that contains all the save request information for the Family Pre-Registration block.
    /// </summary>
    public class FamilyPreRegistrationSaveRequestBag
    {
        /// <summary>
        /// Gets or sets the family unique identifier.
        /// </summary>
        public Guid? FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        public AddressControlBag Address { get; set; }

        /// <summary>
        /// Gets or sets the first adult.
        /// </summary>
        public FamilyPreRegistrationPersonBag Adult1 { get; set; }

        /// <summary>
        /// Gets or sets the second adult.
        /// </summary>
        public FamilyPreRegistrationPersonBag Adult2 { get; set; }

        /// <summary>
        /// Gets or sets the create account request.
        /// </summary>
        public FamilyPreRegistrationCreateAccountRequestBag CreateAccount { get; set; }

        /// <summary>
        /// Gets or sets the planned visit date.
        /// </summary>
        public DateTimeOffset? PlannedVisitDate { get; set; }

        /// <summary>
        /// Gets or sets the schedule unique identifier.
        /// </summary>
        public Guid? ScheduleGuid { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<FamilyPreRegistrationPersonBag> Children { get; set; }

        /// <summary>
        /// Gets or sets the campus unique identifier.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the family attribute values.
        /// </summary>
        public Dictionary<string, string> FamilyAttributeValues { get; set; }
    }
}
