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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Rock.CheckIn
{
    /// <summary>
    /// Object for maintaining the state of a check-in kiosk and workflow
    /// </summary>
    [DataContract]
    public class CheckInState
    {
        /// <summary>
        /// Gets or sets the device id
        /// </summary>
        /// <value>
        /// The device id.
        /// </value>
        [DataMember]
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the checkin type identifier.
        /// </summary>
        /// <value>
        /// The checkin type identifier.
        /// </value>
        [DataMember]
        public int? CheckinTypeId
        {
            get
            {
                return _checkinTypeId;
            }

            set
            {
                _checkinTypeId = value;
                _checkinType = null;
            }
        }

        private int? _checkinTypeId;

        /// <summary>
        /// Gets the type of the current check in.
        /// </summary>
        /// <value>
        /// The type of the current check in.
        /// </value>
        public CheckinType CheckInType
        {
            get
            {
                if ( _checkinType != null )
                {
                    return _checkinType;
                }

                if ( CheckinTypeId.HasValue )
                {
                    _checkinType = new CheckinType( CheckinTypeId.Value );
                    return _checkinType;
                }

                return null;
            }

            set
            {
                _checkinType = value;
            }
        }

        private CheckinType _checkinType;

        /// <summary>
        /// Gets or sets a value indicating whether [manager logged in].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [manager logged in]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ManagerLoggedIn { get; set; }

        /// <summary>
        /// Gets or sets the configured group types (Checkin Areas)
        /// </summary>
        /// <value>
        /// The configured group types.
        /// </value>
        [DataMember]
        public List<int> ConfiguredGroupTypes { get; set; }

        /// <summary>
        /// Gets the kiosk.
        /// </summary>
        /// <value>
        /// The kiosk.
        /// </value>
        public KioskDevice Kiosk
        {
            get
            {
                return KioskDevice.Get( DeviceId, ConfiguredGroupTypes );
            }
        }

        /// <summary>
        /// Gets or sets the check-in status
        /// </summary>
        /// <value>
        /// The check-in.
        /// </value>
        [DataMember]
        public CheckInStatus CheckIn { get; set; }

        /// <summary>
        /// Gets or sets the messages.
        /// </summary>
        /// <value>
        /// The messages.
        /// </value>
        public List<CheckInMessage> Messages { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInState" /> class.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <param name="checkinTypeId">The checkin type identifier.</param>
        /// <param name="configuredGroupTypes">The configured group types.</param>
        public CheckInState( int deviceId, int? checkinTypeId, List<int> configuredGroupTypes )
        {
            DeviceId = deviceId;
            CheckinTypeId = checkinTypeId;
            ConfiguredGroupTypes = configuredGroupTypes;
            CheckIn = new CheckInStatus();
            Messages = new List<CheckInMessage>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInState"/> class.
        /// </summary>
        /// <param name="localDeviceConfiguration">The local device configuration.</param>
        public CheckInState( LocalDeviceConfiguration localDeviceConfiguration )
        {
            DeviceId = localDeviceConfiguration.CurrentKioskId ?? 0;
            CheckinTypeId = localDeviceConfiguration.CurrentCheckinTypeId;
            ConfiguredGroupTypes = localDeviceConfiguration.CurrentGroupTypeIds.ToList();
            CheckIn = new CheckInStatus();
            Messages = new List<CheckInMessage>();
        }

        /// <summary>
        /// Creates a new CheckInState object Froms a json string.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        [Obsolete( "use the FromJson() string extension instead" )]
        [RockObsolete( "1.10" )]
        public static CheckInState FromJson( string json )
        {
            return json.FromJsonOrNull<CheckInState>();
        }
    }

    /// <summary>
    /// Checkin Device Configuration
    /// Used for the Checkin Cookie and REST status operations
    /// </summary>
    [System.Diagnostics.DebuggerDisplay( "CurrentTheme:{CurrentTheme}, CurrentKioskId:{CurrentKioskId}, CurrentCheckinTypeId:{CurrentCheckinTypeId}, CurrentGroupTypeIds:{CurrentGroupTypeIds}" )]
    public class LocalDeviceConfiguration
    {
        /// <summary>
        /// Gets or sets the current theme.
        /// </summary>
        /// <value>
        /// The current theme.
        /// </value>
        public string CurrentTheme { get; set; }

        /// <summary>
        /// Gets or sets the current kiosk identifier.
        /// </summary>
        /// <value>
        /// The current kiosk identifier.
        /// </value>
        public int? CurrentKioskId { get; set; }

        /// <summary>
        /// Gets or sets the current checkin type identifier.
        /// </summary>
        /// <value>
        /// The current checkin type identifier.
        /// </value>
        public int? CurrentCheckinTypeId { get; set; }

        /// <summary>
        /// Gets or sets the current group type ids.
        /// </summary>
        /// <value>
        /// The current group type ids.
        /// </value>
        public List<int> CurrentGroupTypeIds { get; set; }

        /// <summary>
        /// Determines whether this instance is configured.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is configured; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConfigured()
        {
            return this.CurrentKioskId.HasValue && this.CurrentGroupTypeIds.Any() && this.CurrentCheckinTypeId.HasValue;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LocalDeviceConfigurationStatus
    {
        /// <summary>
        /// Gets or sets the configuration hash.
        /// </summary>
        /// <value>
        /// The configuration hash.
        /// </value>
        public string ConfigurationHash { get; set; }

        /// <summary>
        /// Gets or sets the next active date time.
        /// </summary>
        /// <value>
        /// The next active date time.
        /// </value>
        public DateTime NextActiveDateTime { get; set; }

        /// <summary>
        /// Gets the campus date time.
        /// </summary>
        /// <value>
        /// The campus date time.
        /// </value>
        public DateTime CampusDateTime { get; set; }

        /// <summary>
        /// Gets the server current date time.
        /// </summary>
        /// <value>
        /// The server current date time.
        /// </value>
        public DateTime ServerCurrentDateTime { get; set; }

        /// <summary>
        /// Gets the campus current date time.
        /// </summary>
        /// <value>
        /// The campus current date time.
        /// </value>
        public DateTime CampusCurrentDateTime { get; set; }
    }
}