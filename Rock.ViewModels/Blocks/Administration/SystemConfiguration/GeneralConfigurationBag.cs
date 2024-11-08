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

namespace Rock.ViewModels.Blocks.Administration.SystemConfiguration
{
    /// <summary>
    /// Contains the details required for general system configuration.
    /// </summary>
    public class GeneralConfigurationBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is multiple time zone support enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is multiple time zone support enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultipleTimeZoneSupportEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include business in person picker].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include business in person picker]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeBusinessInPersonPicker { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable keep alive].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable keep alive]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableKeepAlive { get; set; }

        /// <summary>
        /// Gets or sets the PDF external render endpoint.
        /// </summary>
        /// <value>
        /// The PDF external render endpoint.
        /// </value>
        public string PDFExternalRenderEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the visitor cookie persistence length days.
        /// </summary>
        /// <value>
        /// The visitor cookie persistence length days.
        /// </value>
        public int? VisitorCookiePersistenceLengthDays { get; set; }

        /// <summary>
        /// Gets or sets the personalization cookie cache length minutes.
        /// </summary>
        /// <value>
        /// The personalization cookie cache length minutes.
        /// </value>
        public int? PersonalizationCookieCacheLengthMinutes { get; set; }
    }
}
