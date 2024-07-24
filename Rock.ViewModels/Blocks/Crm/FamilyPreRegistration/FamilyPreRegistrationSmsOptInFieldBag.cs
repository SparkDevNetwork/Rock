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

namespace Rock.ViewModels.Blocks.Crm.FamilyPreRegistration
{
    /// <summary>
    /// The bag that contains all the field information for the Family Pre-Registration block.
    /// </summary>
    public class FamilyPreRegistrationSmsOptInFieldBag
    {
        /// <summary>
        /// Gets or sets the text to display for the SMS Opt In checkbox
        /// </summary>
        public string SmsOptInDisplayText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is hidden.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is shown.
        /// </summary>
        public bool IsShowFirstAdult { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is required.
        /// </summary>
        public bool IsShowAllAdults { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is optional.
        /// </summary>
        public bool IsShowChildren { get; set; }
    }
}
