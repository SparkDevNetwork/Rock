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
    /// The configuration options for an icon field.
    /// </summary>
    public class IconFieldConfigurationBag
    {
        /// <summary>
        /// The identifier of the icon that will be displayed.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Determines if the color is inverted when drawing. On Zebra printers
        /// will result in an already black background turning white and an
        /// already white background turning black. Other printers may simply
        /// switch to white mode.
        /// </summary>
        /// <value>
        /// This should be the string "False" or "True".
        /// </value>
        public string IsColorInverted { get; set; }
    }
}
