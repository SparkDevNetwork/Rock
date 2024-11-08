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

namespace Rock.ViewModels.Rest.CheckIn
{
    /// <summary>
    /// The request parameters to use when requesting the options available
    /// for a single attendee.
    /// </summary>
    public class AttendeeOpportunitiesOptionsBag
    {
        /// <summary>
        /// Gets or sets the configuration template identifier.
        /// </summary>
        /// <value>The configuration template identifier.</value>
        public string ConfigurationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the area identifiers that will be used to
        /// determine which options are available for each family member.
        /// </summary>
        /// <value>The check-in area identifiers.</value>
        public List<string> AreaIds { get; set; }

        /// <summary>
        /// Gets or sets the kiosk identifier.
        /// </summary>
        /// <value>The kiosk identifier.</value>
        public string KioskId { get; set; }

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>The person identifier.</value>
        public string PersonId { get; set; }

        /// <summary>
        /// Gets or sets the family identifier.
        /// </summary>
        /// <value>The family identifier.</value>
        public string FamilyId { get; set; }

        /// <summary>
        /// When set and valid, this specifies the PIN code to use for authentication
        /// to enable the override mode in kiosk check-in. Opportunity filtering
        /// will not be performed when in this mode.
        /// </summary>
        public string OverridePinCode { get; set; }
    }
}
