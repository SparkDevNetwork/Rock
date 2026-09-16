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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The security code settings for a check-in configuration. Configures the length and format of security
    /// codes printed on labels; Rock generates alpha-numeric characters first, followed by alphabetic, then
    /// numeric. The length of each code is the sum of all three character counts.
    /// </summary>
    public class CheckInSecurityCodesSettingsBag
    {
        /// <summary>
        /// Gets or sets the number of alpha-numeric characters to use when generating a unique security
        /// code for labels. Alpha-numeric characters are printed first, followed by alpha characters, then
        /// numeric characters.
        /// </summary>
        public int? CodeAlphaNumericLength { get; set; }

        /// <summary>
        /// Gets or sets the number of alpha characters to use when generating a unique security code for
        /// labels.
        /// </summary>
        public int? CodeAlphaLength { get; set; }

        /// <summary>
        /// Gets or sets the number of numeric characters to use when generating a unique security code for
        /// labels.
        /// </summary>
        public int? CodeNumericLength { get; set; }

        /// <summary>
        /// Gets or sets whether the numeric portion of security codes is randomized rather than generated
        /// in sequential order.
        /// </summary>
        public bool CodeRandom { get; set; }

        /// <summary>
        /// Gets or sets whether the same security code is reused across all people checking in together.
        /// Only applies when checking in multiple people at the same time, such as during family check-in.
        /// </summary>
        public bool UseSameCodeForFamily { get; set; }
    }
}
