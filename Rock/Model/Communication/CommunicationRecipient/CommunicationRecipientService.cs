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
using Rock.Utility;
using Rock.ViewModels.Communication;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Communication Recipient POCO Service class
    /// </summary>
    public partial class CommunicationRecipientService 
    {

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> by <see cref="Rock.Model.Communication"/> and <see cref="Rock.Model.CommunicationRecipientStatus"/>
        /// </summary>
        /// <param name="communicationId">A <see cref="System.Int32"/> representing the CommunicationId of the <see cref="Rock.Model.Communication"/> to search by.</param>
        /// <param name="status">A <see cref="Rock.Model.CommunicationRecipientStatus"/> Enum value representing the status of the communication submission.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> for the specified <see cref="Rock.Model.Communication"/> and <see cref="Rock.Model.CommunicationRecipientStatus"/></returns>
        public IQueryable<Rock.Model.CommunicationRecipient> Get( int communicationId, CommunicationRecipientStatus status )
        {
            return Queryable( "Communication,PersonAlias.Person" )
                .Where( r =>
                    r.CommunicationId == communicationId &&
                    r.Status == status && r.PersonAlias.Person.IsDeceased == false );
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> by <see cref="Rock.Model.Communication"/>.
        /// </summary>
        /// <param name="communicationId">A <see cref="System.Int32"/> representing the CommunicationId of a  <see cref="Rock.Model.Communication"/> to search by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> for the specified <see cref="Rock.Model.Communication"/>.</returns>
        public IQueryable<Rock.Model.CommunicationRecipient> GetByCommunicationId( int communicationId )
        {
            return Queryable( "Communication,PersonAlias.Person" )
                .Where( r => r.CommunicationId == communicationId );
        }

        /// <summary>
        /// Gets the conversation message bag that will represent the specified
        /// communication recipient message.
        /// </summary>
        /// <param name="communicationRecipientId">The communication recipient identifier.</param>
        /// <returns>A <see cref="ConversationMessageBag"/> that will represent the communication recipient message.</returns>
        internal ConversationMessageBag GetConversationMessageBag( int communicationRecipientId )
        {
            var recipient = Get( communicationRecipientId );
            var publicUrl = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );
            var rockPhoneNumber = SystemPhoneNumberCache.Get( recipient.Communication.SmsFromSystemPhoneNumberId ?? 0 );

            if ( rockPhoneNumber == null )
            {
                throw new Exception( "Unable to determine Rock phone number." );
            }

            var bag = new ConversationMessageBag
            {
                ConversationKey = $"SMS:{rockPhoneNumber.Guid}:{recipient.PersonAlias.Person.Guid}",
                MessageKey = $"C:{recipient.Guid}",
                RockContactKey = rockPhoneNumber.Guid.ToString(),
                ContactKey = recipient.PersonAlias.Person.IsNameless() ? recipient.PersonAlias.Person.PhoneNumbers.FirstOrDefault()?.Number : null,
                MessageDateTime = recipient.CreatedDateTime,
                Message = recipient.SentMessage.IsNotNullOrWhiteSpace() ? recipient.SentMessage : recipient.Communication.SMSMessage,
                IsRead = true,
                PersonGuid = recipient.PersonAlias.Person.Guid,
                FullName = recipient.PersonAlias.Person.FullName,
                IsNamelessPerson = recipient.PersonAlias.Person.IsNameless(),
                IsOutbound = true,
                OutboundSenderFullName = recipient.Communication.SenderPersonAlias.Person.FullName,
                Attachments = new List<ConversationAttachmentBag>()
            };

            if ( recipient.PersonAlias.Person.PhotoId.HasValue )
            {
                bag.PhotoUrl = FileUrlHelper.GetImageUrl( recipient.PersonAlias.Person.PhotoId.Value, new GetImageUrlOptions { PublicAppRoot = publicUrl, Width = 256, Height = 256 } );
            }

            var attachmentGuids = recipient.Communication.Attachments
                .Where( ca => ca.CommunicationType == CommunicationType.SMS )
                .Select( ca => ca.Guid )
                .ToList();

            foreach ( var attachment in recipient.Communication.Attachments )
            {
                var isImage = attachment.BinaryFile.MimeType.StartsWith( "image/", StringComparison.OrdinalIgnoreCase );

                bag.Attachments.Add( new ConversationAttachmentBag
                {
                    FileName = attachment.BinaryFile.FileName,
                    Url = FileUrlHelper.GetImageUrl( attachment.BinaryFile.Guid, new GetImageUrlOptions { PublicAppRoot = publicUrl } ),
                    ThumbnailUrl = isImage ? FileUrlHelper.GetImageUrl( attachment.BinaryFile.Guid, new GetImageUrlOptions { PublicAppRoot = publicUrl, Width = 512, Height = 512 } ) : null
                } );
            }

            return bag;
        }

    }
}
