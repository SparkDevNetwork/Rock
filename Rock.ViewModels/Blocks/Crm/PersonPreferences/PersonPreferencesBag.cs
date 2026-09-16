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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.PersonPreferences
{
    /// <summary>
    /// The person's editable values for the Person Preferences block.
    /// </summary>
    public class PersonPreferencesBag
    {
        /// <summary>
        /// Gets or sets the selected default SMS phone number identifier.
        /// </summary>
        public string DefaultSmsPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the closing phrase appended to e-mails drafted by the person.
        /// </summary>
        public string EmailClosingPhrase { get; set; }

        /// <summary>
        /// Gets or sets the phone type used as the source when originating click-to-call requests.
        /// </summary>
        public ListItemBag CallOriginationSource { get; set; }
    }
}
