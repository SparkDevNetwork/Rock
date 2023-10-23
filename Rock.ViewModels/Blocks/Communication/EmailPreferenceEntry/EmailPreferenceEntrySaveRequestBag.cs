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

namespace Rock.ViewModels.Blocks.Communication.EmailPreferenceEntry
{
    /// <summary>
    /// 
    /// </summary>
    public class EmailPreferenceEntrySaveRequestBag
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the email preference.
        /// </summary>
        /// <value>
        /// The email preference.
        /// </value>
        public string EmailPreference { get; set; }

        /// <summary>
        /// Gets or sets the unsubscribe from list.
        /// </summary>
        /// <value>
        /// The unsubscribe from list.
        /// </value>
        public List<string> UnsubscribeFromList { get; set; }

        /// <summary>
        /// Gets or sets the ina active reason.
        /// </summary>
        /// <value>
        /// The ina active reason.
        /// </value>
        public string InaActiveReason { get; set; }

        /// <summary>
        /// Gets or sets the in active reason note.
        /// </summary>
        /// <value>
        /// The in active reason note.
        /// </value>
        public string InActiveReasonNote { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [in activate family].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [in activate family]; otherwise, <c>false</c>.
        /// </value>
        public bool InActivateFamily { get; set; }
    }
}
