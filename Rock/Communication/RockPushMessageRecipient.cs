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
using System.Collections.Generic;

using Rock.Model;

namespace Rock.Communication
{
    /// <summary>
    /// 
    /// </summary>
    public class RockPushMessageRecipient : RockMessageRecipient
    {
        /// <summary>
        /// Gets the device registration identifier.
        /// </summary>
        /// <value>
        /// The device registration identifier.
        /// </value>
        public string DeviceRegistrationId
        {
            get => To;
            private set => To = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockPushMessageRecipient"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="deviceRegistrationId">The device registration identifier.</param>
        /// <param name="mergeFields">The merge fields.</param>
        public RockPushMessageRecipient( Person person, string deviceRegistrationId, Dictionary<string, object> mergeFields ) : base( person, deviceRegistrationId, mergeFields )
        {
        }

        /// <summary>
        /// Create a RockPushMessageRecipient from an deviceRegistrationId that is not associated with a Person record
        /// </summary>
        /// <param name="deviceRegistrationId">The device registration identifier.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns></returns>
        public static RockPushMessageRecipient CreateAnonymous( string deviceRegistrationId, Dictionary<string, object> mergeFields )
        {
            return new RockPushMessageRecipient( null, deviceRegistrationId, mergeFields );
        }
    }
}
