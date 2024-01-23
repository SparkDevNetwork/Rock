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

using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupRegistration
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupRegistrationBag
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the home phone.
        /// </summary>
        /// <value>
        /// The home phone.
        /// </value>
        public string HomePhone { get; set; }

        /// <summary>
        /// Gets or sets the mobile phone.
        /// </summary>
        /// <value>
        /// The mobile phone.
        /// </value>
        public string MobilePhone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable SMS].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable SMS]; otherwise, <c>false</c>.
        /// </value>
        public bool IsMessagingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public AddressControlBag Address { get; set; }

        /// <summary>
        /// Gets or sets the first name of the spouse.
        /// </summary>
        /// <value>
        /// The first name of the spouse.
        /// </value>
        public string SpouseFirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the spouse.
        /// </summary>
        /// <value>
        /// The last name of the spouse.
        /// </value>
        public string SpouseLastName { get; set; }

        /// <summary>
        /// Gets or sets the spouse mobile phone.
        /// </summary>
        /// <value>
        /// The spouse mobile phone.
        /// </value>
        public string SpouseMobilePhone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable spouse SMS].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable spouse SMS]; otherwise, <c>false</c>.
        /// </value>
        public bool SpouseIsMessagingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the spouse email.
        /// </summary>
        /// <value>
        /// The spouse email.
        /// </value>
        public string SpouseEmail { get; set; }

        /// <summary>
        /// Gets or sets the result lava template.
        /// </summary>
        /// <value>
        /// The result lava template.
        /// </value>
        public string ResultLavaTemplate { get; set; }
    }
}
