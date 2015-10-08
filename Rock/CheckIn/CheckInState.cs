// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

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
        /// Gets or sets a value indicating whether [manager logged in].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [manager logged in]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ManagerLoggedIn { get; set; }

        /// <summary>
        /// Gets or sets the configured group types.
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
                return KioskDevice.Read( DeviceId, ConfiguredGroupTypes );
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
        /// Initializes a new instance of the <see cref="CheckInState" /> class.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        /// <param name="configuredGroupTypes">The configured group types.</param>
        public CheckInState( int deviceId, List<int> configuredGroupTypes )
        {
            DeviceId = deviceId;
            ConfiguredGroupTypes = configuredGroupTypes;
            CheckIn = new CheckInStatus();
        }

        /// <summary>
        /// Creates a new CheckInState object Froms a json string.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static CheckInState FromJson( string json )
        {
            return JsonConvert.DeserializeObject( json, typeof( CheckInState ) ) as CheckInState;
          
        }

    }
}