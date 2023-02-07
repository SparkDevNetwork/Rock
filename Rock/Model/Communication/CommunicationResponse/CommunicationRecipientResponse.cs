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

using Rock.Data;
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
        /// Gets the communication identifier if this is a response based on <see cref="Rock.Model.CommunicationRecipient"/>
        /// </summary>
        /// <value>
        /// The communication identifier.
        /// </value>
        public int? CommunicationId { get; internal set; }

        /// <summary>
        /// Gets the communication response identifier if this a response based on <see cref="Rock.Model.CommunicationResponse"/>
        /// </summary>
        /// <value>
        /// The communication response identifier.
        /// </value>
        public int? CommunicationResponseId { get; internal set; }

        /// <summary>
        /// Gets or sets the recipient person alias identifier. This is the
        /// individual outside of Rock that is being communicated with.
        /// </summary>
        /// <value>
        /// The recipient person alias identifier.
        /// </value>
        public int? RecipientPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the recipient person unique identifier. This is
        /// the individual outside of Rock that is being communicated with.
        /// </summary>
        /// <value>
        /// The recipient person unique identifier.
        /// </value>
        public Guid? RecipientPersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the conversation key. All messages that are part of the
        /// same conversation will share a common ConversationKey.
        /// </summary>
        /// <value>
        /// The conversation key.
        /// </value>
        public string ConversationKey { get; set; }

        /// <summary>
        /// Gets or sets the message key. This uniquely identifiers a single message.
        /// </summary>
        /// <value>
        /// The message key.
        /// </value>
        public string MessageKey { get; set; }

        /// <summary>
        /// Gets or sets the contact key of the recipient. This would contain
        /// a phone number, e-mail address, or other transport specific key
        /// to allow communication.
        /// </summary>
        /// <value>The contact key of the recipient.</value>
        public string ContactKey { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person that send the message from
        /// Rock. This is only valid if <see cref="IsOutbound"/> is true.
        /// </summary>
        /// <value>
        /// The full name of the person that sent the message from Rock.
        /// </value>
        public string OutboundSenderFullName { get; set; }

        /// <summary>
        /// Gets or sets the full name. This represents the individual outside
        /// of Rock that is being communicated with.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the recipient person photo identifier. This is
        /// the photo of the individual outside of Rock that is being
        /// communicated with.
        /// </summary>
        /// <value>
        /// The recipient person photo identifier.
        /// </value>
        public int? RecipientPhotoId { get; set; }

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
        [RockObsolete("1.13")]
        [Obsolete( "Use HasAttachments() or GetBinaryFileGuids() instead" )]
        public List<Guid> BinaryFileGuids { get; set; }

        /// <summary>
        /// Determines whether the specified rock context has attachments.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        ///   <c>true</c> if the specified rock context has attachments; otherwise, <c>false</c>.
        /// </returns>
        public bool HasAttachments( RockContext rockContext )
        {
            return GetBinaryFileGuids( rockContext ).Any();
        }

        private List<Guid> _binaryFileGuids = null;

        /// <summary>
        /// Gets the binary file guids of any attachments to the message
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public List<Guid> GetBinaryFileGuids( RockContext rockContext )
        {
            if ( _binaryFileGuids != null)
            {
                return _binaryFileGuids;
            }

            if ( CommunicationId.HasValue )
            {
                _binaryFileGuids = new CommunicationService( rockContext ).GetSelect( CommunicationId.Value, s => s.Attachments.Select( x => x.BinaryFile.Guid ) ).ToList();
            }
            else if ( CommunicationResponseId.HasValue )
            {
                _binaryFileGuids = new CommunicationResponseService( rockContext ).GetSelect( CommunicationResponseId.Value, s => s.Attachments.Select( x => x.BinaryFile.Guid ) ).ToList();
            }
            else
            {
                _binaryFileGuids = new List<Guid>();
            }

            return _binaryFileGuids;
        }

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
