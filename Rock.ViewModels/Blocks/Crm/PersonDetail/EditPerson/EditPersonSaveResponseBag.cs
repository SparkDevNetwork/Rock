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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.EditPerson
{
    /// <summary>
    /// The response returned from the Edit Person block's Save action.
    /// </summary>
    public class EditPersonSaveResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the save succeeded.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the URL to redirect to upon a successful save.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets the warning shown when the communication preference is SMS but no
        /// SMS-enabled phone number exists.
        /// </summary>
        public string CommunicationPreferenceWarning { get; set; }

        /// <summary>
        /// Gets or sets the error shown when the deceased date is before the birth date.
        /// </summary>
        public string DeceasedDateError { get; set; }

        /// <summary>
        /// Gets or sets the inline error shown when one or more alternate identifiers are already in use.
        /// </summary>
        public string AlternateIdError { get; set; }

        /// <summary>
        /// Gets or sets the confirmation prompt shown when the giving envelope number is already
        /// assigned to other people. When set, the client must re-submit with confirmation.
        /// </summary>
        public string EnvelopeNumberConfirmationMessage { get; set; }
    }
}
