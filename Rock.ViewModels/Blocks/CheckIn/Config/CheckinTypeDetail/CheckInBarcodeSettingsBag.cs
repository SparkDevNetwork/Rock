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

namespace Rock.ViewModels.Blocks.CheckIn.Config.CheckinTypeDetail
{
    /// <summary>
    /// The item details for the Check-In Type Detail block Barcode Settings.
    /// </summary>
    public class CheckInBarcodeSettingsBag
    {
        /// <summary>
        /// Gets or sets the length of the code alpha numeric.
        /// </summary>
        /// <value>
        /// The length of the code alpha numeric.
        /// </value>
        public int CodeAlphaNumericLength { get; set; }

        /// <summary>
        /// Gets or sets the length of the code alpha.
        /// </summary>
        /// <value>
        /// The length of the code alpha.
        /// </value>
        public int CodeAlphaLength { get; set; }

        /// <summary>
        /// Gets or sets the length of the code numeric.
        /// </summary>
        /// <value>
        /// The length of the code numeric.
        /// </value>
        public int CodeNumericLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [code random].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [code random]; otherwise, <c>false</c>.
        /// </value>
        public bool CodeRandom { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [reuse code].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [reuse code]; otherwise, <c>false</c>.
        /// </value>
        public bool ReuseCode { get; set; }
    }
}
