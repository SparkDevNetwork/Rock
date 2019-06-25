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
    public class RockSMSMessageRecipient : RockMessageRecipient
    {
        /// <summary>
        /// Gets the SMS number.
        /// </summary>
        /// <value>
        /// The SMS number.
        /// </value>
        public string SMSNumber
        {
            get => To;
            private set => To = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockSMSMessageRecipient"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="smsNumber">The SMS number.</param>
        /// <param name="mergeFields">The merge fields.</param>
        public RockSMSMessageRecipient( Person person, string smsNumber, Dictionary<string, object> mergeFields ) : base( person, smsNumber, mergeFields )
        {
        }

        /// <summary>
        /// Create a RockSMSMessageRecipient from an sms number that is not associated with a Person record
        /// </summary>
        /// <param name="smsNumber">The SMS number.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns></returns>
        public static RockSMSMessageRecipient CreateAnonymous( string smsNumber, Dictionary<string, object> mergeFields )
        {
            return new RockSMSMessageRecipient( null, smsNumber, mergeFields );
        }
    }
}
