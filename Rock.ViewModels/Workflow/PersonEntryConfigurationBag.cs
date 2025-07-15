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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Workflow
{
    /// <summary>
    /// Defines the configuration information required to display the person entry
    /// section inside a user entry form.
    /// </summary>
    public class PersonEntryConfigurationBag
    {
        /// <summary>
        /// The title to display at the top of the person entry form.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The description to display just below the title.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// If <c>true</c> then a separator will be displayed between the
        /// title/description and the rest of the form.
        /// </summary>
        public bool ShowHeadingSeparator { get; set; }

        /// <summary>
        /// The CSS class to apply to the div that wraps the entire form.
        /// </summary>
        public string SectionCssClass { get; set; }

        /// <summary>
        /// Custom HTML to display before the editable fields on the form.
        /// </summary>
        public string PreHtml { get; set; }

        /// <summary>
        /// Custom HTML to display after the editable fields on the form.
        /// </summary>
        public string PostHtml { get; set; }

        /// <summary>
        /// If <c>true</c> then the campus picker will be visible.
        /// </summary>
        public bool IsCampusVisible { get; set; }

        /// <summary>
        /// The list of campuses to display in the campus picker.
        /// </summary>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Determines if the gender field should be hidden, optional, or
        /// required.
        /// </summary>
        public WorkflowActionFormPersonEntryOption GenderOption { get; set; }

        /// <summary>
        /// Determines if the e-mail field should be hidden, optional, or
        /// required.
        /// </summary>
        public WorkflowActionFormPersonEntryOption EmailOption { get; set; }

        /// <summary>
        /// Determines if the mobile phone field should be hidden, optional,
        /// or required.
        /// </summary>
        public WorkflowActionFormPersonEntryOption MobilePhoneOption { get; set; }

        /// <summary>
        /// Determines if the SMS opt-in field should be visible.
        /// </summary>
        public bool IsSmsVisible { get; set; }

        /// <summary>
        /// Determines if the address field should be hidden, optional, or
        /// required.
        /// </summary>
        public WorkflowActionFormPersonEntryOption AddressOption { get; set; }

        /// <summary>
        /// Determines if the marital status field should be hidden, optional,
        /// or required.
        /// </summary>
        public WorkflowActionFormPersonEntryOption MaritalStatusOption { get; set; }

        /// <summary>
        /// The list of marital status options to display in the marital status
        /// picker.
        /// </summary>
        public List<ListItemBag> MaritalStatuses { get; set; }

        /// <summary>
        /// Determines if the birthdate field should be hidden, optional, or
        /// required.
        /// </summary>
        public WorkflowActionFormPersonEntryOption BirthDateOption { get; set; }

        /// <summary>
        /// Determines if the spouse panel should be hidden, optional or
        /// required.
        /// </summary>
        public WorkflowActionFormPersonEntryOption SpouseOption { get; set; }

        /// <summary>
        /// The term to use for "Spouse". This will be used in the checkbox to
        /// show/hide the spouse fields and also prefixed on the fields in the
        /// spouse area.
        /// </summary>
        public string SpouseLabel { get; set; }

        /// <summary>
        /// Determines if the race field should be hidden, optional, or
        /// required.
        /// </summary>
        public WorkflowActionFormPersonEntryOption RaceOption { get; set; }

        /// <summary>
        /// Determines if the ethnicity field should be hidden, optional, or
        /// required.
        /// </summary>
        public WorkflowActionFormPersonEntryOption EthnicityOption { get; set; }
    }
}
