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
using System.Data.Entity;
using System.Linq;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// Rock SMS Message
    /// </summary>
    /// <seealso cref="Rock.Communication.RockMessage" />
    public class RockSMSMessage : RockMessage
    {
        /// <summary>
        /// Gets the medium entity type identifier.
        /// </summary>
        /// <value>
        /// The medium entity type identifier.
        /// </value>
        public override int MediumEntityTypeId
        {
            get
            {
                return EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ).Id;
            }
        }

        /// <summary>
        /// Gets or sets from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        [Obsolete( "Use FromSystemPhoneNumber instead." )]
        [RockObsolete( "1.15" )]
        public DefinedValueCache FromNumber
        {
            get
            {
                if ( !_fromSystemPhoneNumberId.HasValue )
                {
                    return null;
                }

                var systemPhoneNumberCache = SystemPhoneNumberCache.Get( _fromSystemPhoneNumberId.Value );

                if ( systemPhoneNumberCache == null )
                {
                    return null;
                }

                return DefinedValueCache.Get( systemPhoneNumberCache.Guid );
            }

            set
            {
                _fromSystemPhoneNumberId = SystemPhoneNumberCache.Get( value.Guid )?.Id;
            }
        }

        /// <summary>
        /// Gets or sets system phone number that will be used to send this message.
        /// </summary>
        /// <value>
        /// The system phone number that will be used to send this message.
        /// </value>
        public SystemPhoneNumberCache FromSystemPhoneNumber
        {
            get
            {
                if ( _fromSystemPhoneNumberId.HasValue )
                {
                    return SystemPhoneNumberCache.Get( _fromSystemPhoneNumberId.Value );
                }

                return null;
            }

            set
            {
                _fromSystemPhoneNumberId = value?.Id;
            }
        }

        private int? _fromSystemPhoneNumberId = null;

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the name of the communication.
        /// When CreateCommunicationRecord = true this value will insert Communication.Name
        /// </summary>
        /// <value>
        /// The name of the communication.
        /// </value>
        [Obsolete( "Use CommunicationName instead" )]
        [RockObsolete("1.12")]
        public string communicationName
        {
            get => CommunicationName;
            set => CommunicationName = value;
        }

        /// <summary>
        /// Gets or sets the name of the communication.
        /// When CreateCommunicationRecord = true this value will insert Communication.Name
        /// </summary>
        /// <value>
        /// The name of the communication.
        /// </value>
        public string CommunicationName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockSMSMessage"/> class.
        /// </summary>
        public RockSMSMessage() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockSMSMessage"/> class.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        public RockSMSMessage( SystemCommunication systemCommunication ) : this()
        {
            if ( systemCommunication != null )
            {
                InitializeSmsMessage( systemCommunication );
            }
        }

        /// <summary>
        /// Initializes the SMS message.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        private void InitializeSmsMessage( SystemCommunication systemCommunication )
        {
            if ( systemCommunication == null )
            {
                return;
            }

            this.FromSystemPhoneNumber = systemCommunication.SmsFromSystemPhoneNumberId.HasValue
                ? SystemPhoneNumberCache.Get( systemCommunication.SmsFromSystemPhoneNumberId.Value )
                : null;
            this.CommunicationName = systemCommunication.Title;
            this.Message = systemCommunication.SMSMessage;
            this.SystemCommunicationId = systemCommunication.Id;
        }

        /// <summary>
        /// Returns the Person sending the SMS communication.
        /// Will use the Response Recipient if one exists otherwise the Current Person.
        /// </summary>
        /// <returns></returns>
        public Rock.Model.Person GetSMSFromPerson()
        {
            // Try to get a from person
            Rock.Model.Person person = CurrentPerson;

            // If the response recipient exists use it
            if ( FromSystemPhoneNumber.AssignedToPersonAliasId.HasValue )
            {
                person = new Rock.Model.PersonAliasService( new Data.RockContext() )
                    .GetPerson( FromSystemPhoneNumber.AssignedToPersonAliasId.Value );
            }

            return person;
        }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        public void SetRecipients( List<RockSMSMessageRecipient> recipients )
        {
            this.Recipients = new List<RockMessageRecipient>();
            this.Recipients.AddRange( recipients );
        }
    }
}
