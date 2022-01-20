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

using Rock.Common.Tv.Enum;

namespace Rock.Common.Tv
{
    /// <summary>
    /// POCO for holding information about the device
    /// </summary>
    public class DeviceData
    {
        /// <summary>
        /// The type of the device
        /// </summary>
        public DeviceType DeviceType { get; set; }

        /// <summary>
        /// The manufacturer
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// The model of the device
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// The name of the device
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The version of the device (15.0, etc).
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the device platform (TvOS, etc).
        /// </summary>
        public DevicePlatform DevicePlatform { get; set; }

        /// <summary>
        /// Gets or sets the device identifier.
        /// </summary>
        /// <value>
        /// The device identifier.
        /// </value>
        public string DeviceIdentifier { get; set; }
    }
}
