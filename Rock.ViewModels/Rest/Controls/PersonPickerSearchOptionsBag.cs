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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the Search API action of
    /// the PersonPicker control.
    /// </summary>
    public class PersonPickerSearchOptionsBag
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>The address.</value>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>The phone.</value>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>The email.</value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include details].
        /// </summary>
        /// <value><c>true</c> if [include details]; otherwise, <c>false</c>.</value>
        public bool IncludeDetails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include businesses in the results.
        /// </summary>
        /// <value><c>true</c> if businesses should be included in the results; otherwise, <c>false</c>.</value>
        public bool IncludeBusinesses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include deceased people in the results.
        /// </summary>
        /// <value><c>true</c> if deceased people should be included in the results; otherwise, <c>false</c>.</value>
        public bool IncludeDeceased { get; set; }
    }

}
