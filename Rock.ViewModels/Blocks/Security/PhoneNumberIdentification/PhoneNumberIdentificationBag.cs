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

namespace Rock.ViewModels.Blocks.Security.PhoneNumberIdentification
{
    /// <summary>
    /// The box that contains the initial display state rendered by the Phone Number Identification block.
    /// </summary>
    public class PhoneNumberIdentificationBag
    {
        /// <summary>
        /// Gets or sets the title shown above the lookup form.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Lava-resolved instructions shown on the phone number entry screen.
        /// </summary>
        public string InitialInstructions { get; set; }

        /// <summary>
        /// Gets or sets the Lava-resolved instructions shown on the verification code entry screen.
        /// </summary>
        public string VerificationInstructions { get; set; }

        /// <summary>
        /// Gets or sets the Lava-resolved instructions shown when the individual must select themselves from multiple matches.
        /// </summary>
        public string IndividualSelectionInstructions { get; set; }

        /// <summary>
        /// Gets or sets the Lava-resolved message shown when the phone number is not found after verification.
        /// </summary>
        public string PhoneNumberNotFoundMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block has been configured with an SMS number and can be used.
        /// </summary>
        public bool IsConfigured { get; set; }
    }
}
