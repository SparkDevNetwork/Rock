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
    public class RockEmailMessageRecipient : RockMessageRecipient
    {
        /// <summary>
        /// Gets the email address.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        public string EmailAddress
        {
            get => To;
            private set => To = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockEmailMessageRecipient" /> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="mergeFields">The merge fields.</param>
        public RockEmailMessageRecipient( Person person, Dictionary<string, object> mergeFields ) : base( person, person?.Email, mergeFields )
        {
        }

        /// <summary>
        /// Create a RockEmailMessageRecipient from an email address that is not associated with a Person record
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <returns></returns>
        public static RockEmailMessageRecipient CreateAnonymous( string emailAddress, Dictionary<string, object> mergeFields )
        {
            var result = new RockEmailMessageRecipient( null, mergeFields );
            result.EmailAddress = emailAddress;
            return result;
        }
    }
}
