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
namespace Rock.ViewModels.Blocks.CheckIn.Manager.PersonLeft
{
    /// <summary>
    /// Describes a single phone number row rendered by the Check-in Manager
    /// Person Profile (limited) block.
    /// </summary>
    public class PersonLeftPhoneNumberBag
    {
        /// <summary>
        /// Gets or sets the formatted phone number for display. Digits are
        /// replaced with asterisks when the phone number is unlisted so the
        /// actual number never reaches the DOM.
        /// </summary>
        public string NumberFormatted { get; set; }

        /// <summary>
        /// Gets or sets the value used in the <c>tel:</c> href. Digits are
        /// replaced with asterisks when the phone number is unlisted (which
        /// also disables the button in the UI).
        /// </summary>
        public string RawNumber { get; set; }

        /// <summary>
        /// Gets or sets the phone number type label (such as "Mobile").
        /// </summary>
        public string NumberType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the phone number is marked
        /// as unlisted. When true, the tel: button is disabled and the
        /// displayed digits are masked.
        /// </summary>
        public bool IsUnlisted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the SMS icon should be
        /// shown for this phone number. Only the single number selected by
        /// the block's SMS-capable resolution logic sets this to true.
        /// </summary>
        public bool CanSendSms { get; set; }
    }
}
