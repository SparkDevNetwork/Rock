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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationList
{
    /// <summary>
    /// A bag that contains information about a check-in configuration for the Check-in Configuration List block.
    /// </summary>
    public class CheckInConfigurationBag : ITranslateIdKey
    {
        /// <inheritdoc />
        public int? Id { get; set; }

        /// <inheritdoc />
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name for this check-in configuration.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description for this check-in configuration.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class for this check-in configuration.
        /// </summary>
        public string IconCssClass { get; set; }
    }
}
