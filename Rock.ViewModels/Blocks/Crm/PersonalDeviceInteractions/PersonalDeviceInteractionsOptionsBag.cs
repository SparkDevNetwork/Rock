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

namespace Rock.ViewModels.Blocks.Crm.PersonalDeviceInteractions
{
    /// <summary>
    ///  Options for the Personal Device Interactions block.
    /// </summary>
    public class PersonalDeviceInteractionsOptionsBag
    {
        /// <summary>
        /// Gets or sets the block title, which is dependent on if we're filtering by a specific personal device.
        /// </summary>
        public string BlockTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the interactions are being filtered by a specific device.
        /// </summary>
        public bool IsFilteredByDevice { get; set; }

        /// <summary>
        /// Gets or sets the platform of the device.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Gets or sets the version of the device.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the IconCssClass corresponding to the device type.
        /// </summary>
        public string IconCssClass { get; set; }
    }
}