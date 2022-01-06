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
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// POCO to store the common fields of <see cref="CommunicationRecipient"/> and <see cref="CommunicationResponse"/>
    /// </summary>
    public class CommunicationRecipientResponse
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationRecipientResponse"/> class.
        /// </summary>
        public CommunicationRecipientResponse()
        {
            //
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationRecipientResponse"/> class.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="recordTypeValueId">The record type value identifier.</param>
        public CommunicationRecipientResponse( int personId, int recordTypeValueId )
        {
            PersonId = personId;
            RecordTypeValueId = recordTypeValueId;
        }

        #endregion

        /// <summary>
        /// Gets or sets the recipient person alias identifier.
        /// </summary>
        /// <value>
        /// The recipient person alias identifier.
        /// </value>
        public int? RecipientPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the message key.
        /// </summary>
        /// <value>
        /// The message key.
        /// </value>
        public string MessageKey { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the message was sent from Rock, vs a mobile device
        /// </summary>
        /// <value>
        ///   <c>true</c> if outbound; otherwise, <c>false</c>.
        /// </value>
        public bool IsOutbound { get; set; }

        /// <summary>
        /// Humanizes the date time to relative if not on the same day and short time if it is.
        /// </summary>
        /// <value>
        /// The humanized created date time.
        /// </value>
        public string HumanizedCreatedDateTime
        {
            get
            {
                if ( CreatedDateTime == null )
                {
                    return string.Empty;
                }

                DateTime dtCompare = RockDateTime.Now;

                if ( dtCompare.Date == CreatedDateTime.Value.Date )
                {
                    return CreatedDateTime.Value.ToShortTimeString();
                }

                // Method Name "Truncate" collision between Humanizer and Rock ExtensionMethods so have to call as a static with full name.
                return Humanizer.DateHumanizeExtensions.Humanize( CreatedDateTime, true, dtCompare, null );
            }
        }

        /// <summary>
        /// Gets or sets the SMS message.
        /// </summary>
        /// <value>
        /// The SMS message.
        /// </value>
        public string SMSMessage { get; set; }

        /// <summary>
        /// Gets or sets the binary file unique identifier.
        /// </summary>
        /// <value>
        /// The binary file unique identifier.
        /// </value>
        public List<Guid> BinaryFileGuids { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is read.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is read; otherwise, <c>false</c>.
        /// </value>
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId { get; internal set; }

        /// <summary>
        /// Gets the record type value identifier.
        /// </summary>
        /// <value>
        /// The record type value identifier.
        /// </value>
        public int? RecordTypeValueId { get; internal set; }

        /// <summary>
        /// The record type value identifier nameless
        /// </summary>
        private static int _recordTypeValueIdNameless = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Value;

        /// <summary>
        /// Gets a value indicating whether this instance is nameless person.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is nameless person; otherwise, <c>false</c>.
        /// </value>
        public bool IsNamelessPerson => this.RecordTypeValueId == _recordTypeValueIdNameless;

        /// <summary>
        /// Gets the message status.
        /// </summary>
        /// <value>
        /// The message status.
        /// </value>
        public CommunicationRecipientStatus MessageStatus { get; internal set; }
    }
}
