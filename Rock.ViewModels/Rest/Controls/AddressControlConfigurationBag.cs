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
using Rock.Enums.Controls;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The configuration options returned from the GetConfiguration API action of
    /// the AddressControl control.
    /// </summary>
    public class AddressControlConfigurationBag
    {
        /// <summary>
        /// Whether to show the country picker
        /// </summary>
        public bool ShowCountrySelection { get; set; }
        /// <summary>
        /// If no other country is set, this should be the default one selected
        /// </summary>
        public string DefaultCountry { get; set; }
        /// <summary>
        /// If no other state is set, this should be the default one selected
        /// </summary>
        public string DefaultState { get; set; }
        /// <summary>
        /// List of countries for the country picker
        /// </summary>
        public List<ListItemBag> Countries { get; set; }
        /// <summary>
        /// List of states for the state picker (if any)
        /// </summary>
        public List<ListItemBag> States { get; set; }

        /// <summary>
        /// Whether there are any states for the picker. If not, use a text field
        /// </summary>
        public bool HasStateList { get; set; }
        /// <summary>
        /// Currently selected country
        /// </summary>
        public string SelectedCountry { get; set; }

        /// <summary>
        /// Configured label for the city field (based on country's configuration)
        /// </summary>
        public string CityLabel { get; set; }
        /// <summary>
        /// Configured label for the locality/county field (based on country's configuration)
        /// </summary>
        public string LocalityLabel { get; set; }
        /// <summary>
        /// Configured label for the state field (based on country's configuration)
        /// </summary>
        public string StateLabel { get; set; }
        /// <summary>
        /// Configured label for the postal code field (based on country's configuration)
        /// </summary>
        public string PostalCodeLabel { get; set; }

        /// <summary>
        /// Requirement level for the address line 1 field
        /// </summary>
        public RequirementLevel AddressLine1Requirement { get; set; }
        /// <summary>
        /// Requirement level for the address line 2 field
        /// </summary>
        public RequirementLevel AddressLine2Requirement { get; set; }
        /// <summary>
        /// Requirement level for the city field
        /// </summary>
        public RequirementLevel CityRequirement { get; set; }
        /// <summary>
        /// Requirement level for the locality field
        /// </summary>
        public RequirementLevel LocalityRequirement { get; set; }
        /// <summary>
        /// Requirement level for the state field
        /// </summary>
        public RequirementLevel StateRequirement { get; set; }
        /// <summary>
        /// Requirement level for the postal code field
        /// </summary>
        public RequirementLevel PostalCodeRequirement { get; set; }
    }
}
