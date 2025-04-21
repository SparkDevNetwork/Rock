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
using Rock.Mobile;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Communication;
using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// Extension methods for <see cref="CommunicationRecipientResponse"/> objects.
    /// </summary>
    internal static class CommunicationRecipientResponseExtensions
    {
        /// <summary>
        /// Converts the <see cref="CommunicationRecipientResponse"/> to a
        /// <see cref="ConversationMessageBag"/>.
        /// </summary>
        /// <param name="response">The response to be converted.</param>
        /// <param name="loadAttachments">if set to <c>true</c> then attachments will be loaded from the database.</param>
        /// <returns>A <see cref="ConversationMessageBag"/> that represents the response.</returns>
        internal static ConversationMessageBag ToMessageBag( this CommunicationRecipientResponse response, bool loadAttachments )
        {
            var bag = new ConversationMessageBag
            {
                ConversationKey = response.ConversationKey,
                MessageKey = response.MessageKey,
                ContactKey = response.ContactKey,
                MessageDateTime = response.CreatedDateTime,
                Message = response.SMSMessage,
                IsRead = response.IsRead,
                PersonGuid = response.RecipientPersonGuid.Value,
                FullName = response.FullName,
                IsNamelessPerson = response.IsNamelessPerson,
                IsOutbound = response.IsOutbound,
                OutboundSenderFullName = response.OutboundSenderFullName,
                Attachments = new List<ConversationAttachmentBag>()
            };

            // Initially set the photo URL using the recipient photo ID.
            if ( response.RecipientPhotoId.HasValue )
            {
                bag.PhotoUrl = MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( response.RecipientPhotoId.Value, new GetImageUrlOptions { MaxWidth = 256, MaxHeight = 256 } ) );
            }

            if ( response.RecipientPersonGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    // We want to use the recipient person guid to get the avatar view for the person.
                    var photoUrl = new PersonService( rockContext )
                        .Queryable()
                        .FirstOrDefault( p => p.Guid == response.RecipientPersonGuid.Value )?.PhotoUrl;

                    // Update the photo URL to use the avatar if there is one.
                    if ( photoUrl.IsNotNullOrWhiteSpace() )
                    {
                        bag.PhotoUrl = MobileHelper.BuildPublicApplicationRootUrl( photoUrl );
                    }
                }
            }

            if ( loadAttachments )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Load attachments from either the CommunicationAttachment
                    // table or the CommunicationResponseAttachment table.
                    var attachments = response.CommunicationId.HasValue
                        ? new CommunicationAttachmentService( rockContext ).Queryable()
                            .Where( ca => ca.CommunicationId == response.CommunicationId.Value )
                            .Select( ca => new
                            {
                                ca.BinaryFile.Guid,
                                ca.BinaryFile.FileName,
                                ca.BinaryFile.MimeType
                            } )
                            .ToList()
                        : new CommunicationResponseAttachmentService( rockContext ).Queryable()
                            .Where( cra => cra.CommunicationResponseId == response.CommunicationResponseId.Value )
                            .Select( cra => new
                            {
                                cra.BinaryFile.Guid,
                                cra.BinaryFile.FileName,
                                cra.BinaryFile.MimeType
                            } )
                            .ToList();

                    foreach ( var attachment in attachments )
                    {
                        var ext = System.IO.Path.GetExtension( attachment.FileName ).ToLower();
                        var isImage = attachment.MimeType.StartsWith( "image/", StringComparison.OrdinalIgnoreCase ) == true;

                        bag.Attachments.Add( new ConversationAttachmentBag
                        {
                            FileName = attachment.FileName,
                            Url = MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( attachment.Guid ) ),
                            ThumbnailUrl = isImage ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( attachment.Guid, new GetImageUrlOptions { Width = 512, Height = 512 } ) ) : null
                        } );
                    }
                }
            }

            return bag;
        }

        /// <summary>
        /// Converts a collection of <see cref="CommunicationRecipientResponse"/> objects to a collection of <see cref="ConversationMessageBag"/> objects.
        /// This includes loading and associating attachments with the appropriate messages.
        /// </summary>
        /// <param name="responses">The collection of responses to be converted into message bags.</param>
        /// <returns>A collection of <see cref="ConversationMessageBag"/> objects, each representing a response along with its associated attachments.</returns>
        internal static IEnumerable<ConversationMessageBag> ToMessageBags( this IEnumerable<CommunicationRecipientResponse> responses )
        {
            var attachmentsLookup = new Dictionary<string, List<(Guid Guid, string FileName, string MimeType)>>();

            // Load the attachments for all responses in two queries rather
            // than executing a query for every single response.
            using ( var rockContext = new RockContext() )
            {
                // Communication recipient responses can have duplicate communication IDs,
                // so we want to ensure that we get each unique communication ID with all of
                // its associated message keys.
                var communicationIdMap = responses.Where( r => r.CommunicationId.HasValue )
                    .GroupBy( r => r.CommunicationId.Value )
                    .ToDictionary( g => g.Key, g => g.Select( r => r.MessageKey ).ToList() );

                // CommunicationResponseId is unique, so we can use a more straightforward
                // dictionary to map the response ID to the message key.
                var communicationResponseIdMap = responses
                    .Where( r => !r.CommunicationId.HasValue && r.CommunicationResponseId.HasValue )
                    .ToDictionary( r => r.CommunicationResponseId.Value, r => r.MessageKey );

                // Load all of the communication attachments for the communication
                // and ensure to add it for the specific message.
                if ( communicationIdMap.Count > 0 )
                {
                    var communicationIds = communicationIdMap.Keys;

                    var results = new CommunicationAttachmentService( rockContext )
                        .Queryable()
                        .Where( ca => communicationIds.Contains( ca.CommunicationId ) )
                        .Select( ca => new
                        {
                            ca.CommunicationId,
                            ca.BinaryFile.Guid,
                            ca.BinaryFile.FileName,
                            ca.BinaryFile.MimeType
                        } )
                        .ToList()
                        .GroupBy( ca => ca.CommunicationId );

                    // We need to add the attachments to each individual
                    // message key that is associated with the communication ID.
                    foreach ( var result in results )
                    {
                        var messageKeys = communicationIdMap[result.Key];

                        foreach ( var messageKey in messageKeys )
                        {
                            if ( messageKey.IsNotNullOrWhiteSpace() )
                            {
                                attachmentsLookup.AddOrReplace( messageKey, result.Select( r => (r.Guid, r.FileName, r.MimeType) ).ToList() );
                            }
                        }
                    }
                }

                // Load all of the communication response attachments for the communication
                // and ensure to add it for the specific message.
                if ( communicationResponseIdMap.Count > 0 )
                {
                    var communicationResponseIds = communicationResponseIdMap.Keys.ToList();

                    var results = new CommunicationResponseAttachmentService( rockContext )
                        .Queryable()
                        .Where( cra => communicationResponseIds.Contains( cra.CommunicationResponseId ) )
                        .Select( cra => new
                        {
                            cra.CommunicationResponseId,
                            cra.BinaryFile.Guid,
                            cra.BinaryFile.FileName,
                            cra.BinaryFile.MimeType
                        } )
                        .ToList()
                        .GroupBy( cra => cra.CommunicationResponseId );

                    foreach ( var result in results )
                    {
                        attachmentsLookup.AddOrReplace( communicationResponseIdMap[result.Key], result.Select( r => (r.Guid, r.FileName, r.MimeType) ).ToList() );
                    }
                }
            }

            return responses
                    .Select( response =>
                    {
                        // Convert the response to a message bag without attachments.
                        var bag = ToMessageBag( response, false );

                        // Use our attachment lookup data to find any attachments.
                        if ( attachmentsLookup.TryGetValue( bag.MessageKey, out var attachments ) )
                        {
                            foreach ( var attachment in attachments )
                            {
                                var ext = System.IO.Path.GetExtension( attachment.FileName ).ToLower();
                                var isImage = attachment.MimeType.StartsWith( "image/", StringComparison.OrdinalIgnoreCase ) == true;

                                bag.Attachments.Add( new ConversationAttachmentBag
                                {
                                    FileName = attachment.FileName,
                                    Url = MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( attachment.Guid ) ),
                                    ThumbnailUrl = isImage ? MobileHelper.BuildPublicApplicationRootUrl( FileUrlHelper.GetImageUrl( attachment.Guid, new GetImageUrlOptions { Width = 512, Height = 512 } ) ) : null
                                } );
                            }
                        }

                        return bag;
                    } )
                    .ToList();
        }
    }
}
