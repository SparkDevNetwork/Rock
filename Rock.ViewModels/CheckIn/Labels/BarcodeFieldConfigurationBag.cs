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

namespace Rock.ViewModels.CheckIn.Labels
{
    /// <summary>
    /// The configuration options for a barcode field.
    /// </summary>
    public class BarcodeFieldConfigurationBag
    {
        /// <summary>
        /// Determines which barcode format to use when rendering.
        /// </summary>
        /// <value>
        /// This should be a string containing the integer value of the
        /// BarcodeFormat enumeration.
        /// </value>
        public string Format { get; set; }

        /// <summary>
        /// Determines if the barcode content should be the person identifier
        /// (false) or if it should be custom dynamic text (true).
        /// </summary>
        /// <value>
        /// This should be the string "False" or "True".
        /// </value>
        public string IsDynamic { get; set; }

        /// <summary>
        /// When the barcode is going to use dynamic text, this is the Lava
        /// template that will be used to render the content.
        /// </summary>
        public string DynamicTextTemplate { get; set; }
    }
}
