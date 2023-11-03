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

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// Address Control View Model
    /// </summary>
    public sealed class PhoneNumberBoxWithSmsControlBag
    {
        /// <summary>
        /// Gets or sets the numbers.
        /// </summary>
        /// <value>
        /// The numbers.
        /// </value>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is messaging enbabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is messaging enbabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsMessagingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        /// <value>
        /// The country code.
        /// </value>
        public string CountryCode { get; set; }
    }
}
