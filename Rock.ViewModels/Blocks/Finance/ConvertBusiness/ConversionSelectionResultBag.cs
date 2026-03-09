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

namespace Rock.ViewModels.Blocks.Finance.ConvertBusiness
{
    /// <summary>
    /// Contains UI state details after evaluating the selected source record for conversion.
    /// </summary>
    public class ConversionSelectionResultBag
    {
        /// <summary>
        /// The conversion mode to display, such as converting to person or converting to business.
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// The heading text for an error message shown to the user.
        /// </summary>
        public string ErrorHeading { get; set; }

        /// <summary>
        /// The error message text shown when conversion cannot proceed.
        /// </summary>
        public string ErrorText { get; set; }

        /// <summary>
        /// The initial first name value to use when converting a business to a person.
        /// </summary>
        public string PersonFirstName { get; set; }

        /// <summary>
        /// The initial last name value to use when converting a business to a person.
        /// </summary>
        public string PersonLastName { get; set; }

        /// <summary>
        /// The default connection status selection when converting a business to a person.
        /// </summary>
        public ListItemBag PersonConnectionStatus { get; set; }

        /// <summary>
        /// The initial gender value used when converting a business to a person.
        /// </summary>
        public int PersonGender { get; set; }

        /// <summary>
        /// The initial business name value to use when converting a person to a business.
        /// </summary>
        public string BusinessName { get; set; }
    }
}
