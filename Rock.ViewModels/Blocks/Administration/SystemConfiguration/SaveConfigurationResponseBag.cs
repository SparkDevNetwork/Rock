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
    /// Response returned after saving a system configuration
    /// </summary>
    public class SaveConfigurationResponseBag
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the success message.
        /// </summary>
        /// <value>
        /// The success message.
        /// </value>
        public string SuccessMessage { get; set; }

        /// <summary>
        /// Gets or sets the type of the alert.
        /// </summary>
        /// <value>
        /// The type of the alert.
        /// </value>
        public string AlertType { get; set; }

        /// <summary>
        /// Gets or sets the secondary message.
        /// </summary>
        /// <value>
        /// The secondary message.
        /// </value>
        public string SecondaryMessage { get; set; }

        /// <summary>
        /// Gets or sets the type of the secondary message alert.
        /// </summary>
        /// <value>
        /// The type of the secondary message alert.
        /// </value>
        public string SecondaryMessageAlertType { get; set; }
    }
}
