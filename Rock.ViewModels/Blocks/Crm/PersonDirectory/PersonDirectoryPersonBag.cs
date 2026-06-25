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

namespace Rock.ViewModels.Blocks.Crm.PersonDirectory
{
    /// <summary>
    /// A single person displayed in the directory.
    /// </summary>
    public class PersonDirectoryPersonBag
    {
        /// <summary>
        /// Gets or sets the person's encoded identifier, used to build the profile page link.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the person's display name (nick name and last name).
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the person's photo (or the default avatar).
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the person's email address. Only populated when email display is enabled.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the person's formatted home address as HTML. Only populated when
        /// address display is enabled and results are listed as individuals.
        /// </summary>
        public string FormattedHtmlAddress { get; set; }

        /// <summary>
        /// Gets or sets the person's displayable phone numbers.
        /// </summary>
        public List<PersonDirectoryPhoneBag> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the formatted birthday text (e.g. "Jan 5"), or null when not shown.
        /// </summary>
        public string BirthdayText { get; set; }

        /// <summary>
        /// Gets or sets the gender abbreviation ("M" or "F"), or null when not shown or unknown.
        /// </summary>
        public string GenderText { get; set; }

        /// <summary>
        /// Gets or sets the formatted grade, or null when not shown or not applicable.
        /// </summary>
        public string Grade { get; set; }

        /// <summary>
        /// Gets or sets the giving envelope number, or null when not shown.
        /// </summary>
        public string EnvelopeNumber { get; set; }
    }
}
