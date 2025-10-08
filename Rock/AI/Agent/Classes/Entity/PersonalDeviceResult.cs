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

using System;

using Newtonsoft.Json;

using Rock.Enums.Mobile;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Lightweight DTO representing a person's registered mobile or desktop device
    /// as exposed through the AI agent layer. Sensitive or internal identifiers
    /// are intentionally excluded or marked with <see cref="JsonIgnoreAttribute"/>.
    ///
    /// Derived, display-friendly text properties (<see cref="PersonalDeviceType"/>, <see cref="Platform"/>,
    /// <see cref="LocationPermissionStatus"/>) are automatically populated from their corresponding
    /// backing identifier or enum properties when those are set.
    /// </summary>
    internal class PersonalDeviceResult : EntityResultBase
    {
        /// <summary>
        /// Gets or sets the friendly name assigned to the device (often user-specified by the end user or the OS).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the hardware manufacturer (e.g., Apple, Samsung, Google).
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the model identifier (e.g., "iPhone 15 Pro", "Pixel 9", vendor-specific model code).
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets a human-readable description of the location permission state for this device.
        /// This value is automatically updated whenever <see cref="LocationPermissionStatusValue"/> is set.
        /// </summary>
        public string LocationPermissionStatus { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether OS-level beacon (Bluetooth proximity) monitoring is enabled for this device/app pairing.
        /// </summary>
        public bool IsBeaconMonitoringEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether precise/high‑accuracy location collection is enabled (as opposed to coarse/fuzzy location).
        /// </summary>
        public bool IsPreciseLocationEnabled { get; set; }

        /// <summary>
        /// Gets or sets the date/time when the user disabled location permission (if applicable). Null when permission has not been explicitly revoked.
        /// </summary>
        public DateTime? LocationPermissionDisabledDateTime { get; set; }

        /// <summary>
        /// Gets or sets the most recent timestamp the device checked in (heartbeat, push registration, API call) or was otherwise observed by the system.
        /// </summary>
        public DateTime? LastSeenDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether push or local notifications are enabled for the application on this device.
        /// </summary>
        public bool NotificationsEnabled { get; set; }

        /// <summary>
        /// Gets the platform name (e.g., "iOS", "Android", "Windows").
        /// This is a derived value resolved from <see cref="PlatformValueId"/> (Defined Value) when that identifier is set.
        /// </summary>
        public string Platform { get; private set; }

        /// <summary>
        /// Gets the device form factor/type (e.g., "Phone", "Tablet", "Desktop") as a descriptive label.
        /// This is a derived value resolved from <see cref="PersonalDeviceTypeValueId"/> (Defined Value) when that identifier is set.
        /// </summary>
        public string PersonalDeviceType { get; private set; }

        /// <summary>
        /// Gets or sets the internal defined value identifier for the device type.
        /// When set, <see cref="PersonalDeviceType"/> is updated to the corresponding defined value text. Hidden from JSON output.
        /// </summary>
        [JsonIgnore]
        public int? PersonalDeviceTypeValueId
        {
            get => _personalDeviceTypeValueId;
            set
            {
                _personalDeviceTypeValueId = value;

                if ( value.HasValue )
                {
                    PersonalDeviceType = DefinedValueCache.Get( value.Value )?.Value;
                }
                else
                {
                    PersonalDeviceType = null;
                }
            }
        }
        private int? _personalDeviceTypeValueId;

        /// <summary>
        /// Gets or sets the internal defined value identifier for the platform.
        /// When set, <see cref="Platform"/> is updated to the corresponding defined value text. Hidden from JSON output.
        /// </summary>
        [JsonIgnore]
        public int? PlatformValueId
        {
            get => _platformValueId;
            set
            {
                _platformValueId = value;
                if ( value.HasValue )
                {
                    Platform = DefinedValueCache.Get( value.Value )?.Value;
                }
                else
                {
                    Platform = null;
                }
            }
        }
        private int? _platformValueId;

        /// <summary>
        /// Gets or sets the raw enumeration value representing the location permission status.
        /// Hidden from JSON output; <see cref="LocationPermissionStatus"/> provides the display text.
        /// Setting this property automatically updates <see cref="LocationPermissionStatus"/>.
        /// </summary>
        [JsonIgnore]
        public LocationPermissionStatus LocationPermissionStatusValue
        {
            get => _locationPermissionStatusValue;
            set
            {
                _locationPermissionStatusValue = value;
                LocationPermissionStatus = value.ConvertToString();
            }
        }
        private LocationPermissionStatus _locationPermissionStatusValue;
    }
}
