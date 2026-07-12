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

namespace Rock.ViewModels.Blocks.Crm.PersonDirectory
{
    /// <summary>
    /// The display configuration for the Person Directory block. These values are
    /// derived from block settings and do not change between searches.
    /// </summary>
    public class PersonDirectoryOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether results are grouped by family
        /// rather than listed as individuals.
        /// </summary>
        public bool IsShowByFamily { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email address is shown for each person.
        /// </summary>
        public bool IsEmailShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the home address is shown.
        /// </summary>
        public bool IsAddressShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any phone numbers are shown.
        /// </summary>
        public bool ArePhonesShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the birthday is shown.
        /// </summary>
        public bool IsBirthdayShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the gender is shown.
        /// </summary>
        public bool IsGenderShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the grade is shown.
        /// </summary>
        public bool IsGradeShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the giving envelope number is shown.
        /// </summary>
        public bool IsEnvelopeNumberShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all people are displayed by default.
        /// When false, a search is required before any results are shown. The A-Z letter
        /// navigation is only available when this is true.
        /// </summary>
        public bool IsShowAllPeopleEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the opt-in/out control is available
        /// (requires a logged-in person and a configured opt-out group).
        /// </summary>
        public bool IsOptOutEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a valid data view is configured. When
        /// false the block cannot display results and a warning is shown instead.
        /// </summary>
        public bool IsDataViewConfigured { get; set; }

        /// <summary>
        /// Gets or sets the number of characters that must be entered in the first name
        /// before a search is allowed.
        /// </summary>
        public int? FirstNameCharactersRequired { get; set; }

        /// <summary>
        /// Gets or sets the number of characters that must be entered in the last name
        /// before a search is allowed.
        /// </summary>
        public int? LastNameCharactersRequired { get; set; }
    }
}
