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

namespace Rock.ViewModels.Blocks.Group.GroupRegistration
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.BlockBox" />
    public class GroupRegistrationBlockBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public GroupRegistrationBag Entity { get; set; }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public string Mode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether /[automatic fill].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic fill]; otherwise, <c>false</c>.
        /// </value>
        public bool AutoFill { get; set; }

        /// <summary>
        /// Gets or sets the lava over view.
        /// </summary>
        /// <value>
        /// The lava over view.
        /// </value>
        public string LavaOverview { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is email required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is email required; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmailRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is mobile phone required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is mobile phone required; otherwise, <c>false</c>.
        /// </value>
        public bool IsMobilePhoneRequired { get; set; }

        /// <summary>
        /// Gets or sets the phone label.
        /// </summary>
        /// <value>
        /// The phone label.
        /// </value>
        public string PhoneLabel { get; set; }

        /// <summary>
        /// Gets or sets the open spots.
        /// </summary>
        /// <value>
        /// The open spots.
        /// </value>
        public int OpenSpots { get; set; }

        /// <summary>
        /// Gets or sets the register button alt text
        /// </summary>
        public string RegisterButtonAltText { get; set; }

        /// <summary>
        /// Gets or sets the text to display for the SMS Opt In checkbox
        /// </summary>
        public string SmsOptInDisplayText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is hidden.
        /// </summary>
        public bool SmsIsHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is shown.
        /// </summary>
        public bool SmsIsShowFirstAdult { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is required.
        /// </summary>
        public bool SmsIsShowAllAdults { get; set; }
    }
}
