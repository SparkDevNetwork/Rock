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
    /// The box that contains all the initialization information for the Edit Person block.
    /// </summary>
    public class EditPersonBox : BlockBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether a person to edit was found.
        /// When false, the block renders a not-found message instead of the form.
        /// </summary>
        public bool IsPersonFound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is authorized to save changes.
        /// </summary>
        public bool IsEditAllowed { get; set; }

        /// <summary>
        /// Gets or sets the identifier key of the person being edited (echoed back on save).
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets the current editable values for the person.
        /// </summary>
        public EditPersonBag Person { get; set; }

        /// <summary>
        /// Gets or sets the configuration, feature flags, and option sources for the form.
        /// </summary>
        public EditPersonOptionsBag Options { get; set; }

        /// <summary>
        /// Gets or sets the account protection profile warning message, when applicable.
        /// </summary>
        public string AccountProtectionProfileMessage { get; set; }

        /// <summary>
        /// Gets or sets the notification box type to use for the account protection profile message
        /// (e.g., "Warning" or "Danger").
        /// </summary>
        public string AccountProtectionProfileAlertType { get; set; }

        /// <summary>
        /// Gets or sets the URL to redirect to when the user cancels editing.
        /// </summary>
        public string CancelUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the person's primary family, shown as a panel header label.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the person's primary campus, shown as a panel header label.
        /// </summary>
        public string CampusName { get; set; }
    }
}
