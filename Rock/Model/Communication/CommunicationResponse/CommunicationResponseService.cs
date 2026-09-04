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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Rock.Data;
using Rock.Enums.Communication;
using Rock.Utility;
using Rock.ViewModels.Communication;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class CommunicationResponseService
    {
        /// <summary>
        /// Gets the responses sent from a person Alias ID without any other filters.
        /// </summary>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <returns></returns>
        public IQueryable GetResponsesFromPersonAliasId( int fromPersonAliasId )
        {
            return Queryable().Where( r => r.FromPersonAliasId == fromPersonAliasId );
        }

        /// <summary>
        /// Gets the responses from a person Alias ID for the SMS Phone number.
        /// </summary>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <param name="relatedSmsFromSystemPhoneNumberId">The related SMS from system phone number identifier.</param>
        /// <returns></returns>
        public IQueryable GetResponsesFromPersonAliasIdForSystemPhoneNumber( int fromPersonAliasId, int relatedSmsFromSystemPhoneNumberId )
        {
            return Queryable()
                .Where( r => r.FromPersonAliasId == fromPersonAliasId )
                .Where( r => r.RelatedSmsFromSystemPhoneNumberId == relatedSmsFromSystemPhoneNumberId );
        }

        /// <summary>
        /// Gets the communications and response recipients.
        /// </summary>
        /// <param name="relatedSmsFromSystemPhoneNumberId">The related SMS from system phone number identifier.</param>
        /// <param name="startDateTime">Messages must be created on or after this date to be considered.</param>
        /// <param name="maxCount">The maximum number of results to return.</param>
        /// <param name="filter">The filter that describes what kind of messages to consider.</param>
        /// <param name="personId">The identifier of the person to limit results to.</param>
        /// <returns>A list of <see cref="CommunicationRecipientResponse"/> objects that describe the recipient conversations.</returns>
        public List<CommunicationRecipientResponse> GetCommunicationAndResponseRecipients( int relatedSmsFromSystemPhoneNumberId, DateTime startDateTime, int maxCount, CommunicationMessageFilter filter, int? personId )
        {
            var smsMediumEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Value;

            // Get all SMS responses that were sent:
            //  1. TO this system SMS phone number;
            //  2. on or after the start date time;
            //  3. FROM a known person.
            IQueryable<CommunicationResponse> communicationResponseQuery = this.Queryable()
                .Where( r =>
                    r.RelatedMediumEntityTypeId == smsMediumEntityTypeId
                    && r.RelatedSmsFromSystemPhoneNumberId == relatedSmsFromSystemPhoneNumberId
                    && r.CreatedDateTime >= startDateTime
                    && r.FromPersonAliasId.HasValue );

            if ( filter == CommunicationMessageFilter.ShowUnreadReplies )
            {
                // Filter down to only unread responses.
                communicationResponseQuery = communicationResponseQuery.Where( r => r.IsRead == false );
            }

            // Get all SMS recipients who received an SMS message:
            //  1. FROM this system SMS phone number;
            //  2. on or after the start date time;
            //  3. that was known to be delivered.
            IQueryable<CommunicationRecipient> communicationRecipientQuery = new CommunicationRecipientService( this.Context as RockContext ).Queryable()
                .Where( r =>
                    r.MediumEntityTypeId == smsMediumEntityTypeId
                    && r.Communication.SmsFromSystemPhoneNumberId == relatedSmsFromSystemPhoneNumberId
                    && r.CreatedDateTime >= startDateTime
                    && r.PersonAliasId.HasValue
                    && r.Status == CommunicationRecipientStatus.Delivered );

            switch ( filter )
            {
                case CommunicationMessageFilter.ShowUnreadReplies:
                case CommunicationMessageFilter.ShowAllReplies:
                    // Filter down to only recipients who have replied to outgoing messages.
                    communicationRecipientQuery = communicationRecipientQuery.Join( communicationResponseQuery,
                        communicationRecipient => communicationRecipient.PersonAliasId,
                        communicationResponse => communicationResponse.FromPersonAliasId,
                        ( communicationRecipient, communicationResponse ) => communicationRecipient );
                    break;
            }

            return GetCommunicationResponseRecipients( maxCount, personId, communicationResponseQuery, communicationRecipientQuery );
        }

        private List<CommunicationRecipientResponse> GetCommunicationResponseRecipients( int maxCount, int? personId, IQueryable<CommunicationResponse> communicationResponseQuery, IQueryable<CommunicationRecipient> communicationRecipientQuery )
        {
            // Get person aliases:
            //  1. If person ID WASN'T provided, get ALL person aliases.
            //  2. If person ID WAS provided, only get that person's aliases.
            var personAliasQuery = personId == null
                ? new PersonAliasService( this.Context as RockContext ).Queryable()
                : new PersonAliasService( this.Context as RockContext ).Queryable().Where( p => p.PersonId == personId );

            // do an explicit LINQ inner join on PersonAlias to avoid performance issue where it would do an outer join instead
            var communicationResponseJoinQuery =
                from cr in communicationResponseQuery
                join pa in personAliasQuery on cr.FromPersonAliasId equals pa.Id
                select new { cr, pa };

            IQueryable<CommunicationResponse> mostRecentCommunicationResponseQuery = communicationResponseJoinQuery
                            .GroupBy( r => r.pa.PersonId )
                            .Select( a => a.OrderByDescending( x => x.cr.CreatedDateTime ).FirstOrDefault() )
                            .OrderByDescending( a => a.cr.CreatedDateTime ).Select( a => a.cr );

            // do an explicit LINQ inner join on PersonAlias to avoid performance issue where it would do an outer join instead
            var communicationRecipientJoinQuery = communicationRecipientQuery
                .Where( a => a.PersonAliasId.HasValue )
                // Join to the person alias.
                .Join( personAliasQuery, cr => cr.PersonAliasId, pa => pa.Id, (cr, pa) => new
                {
                    CommunicationRecipient = cr,
                    PersonAlias = pa
                } )
                .Select( j => new
                {
                    j.CommunicationRecipient.Guid,
                    j.PersonAlias.PersonId,
                    PersonGuid = j.PersonAlias.Person.Guid,
                    PersonRecordTypeValueId = j.PersonAlias.Person.RecordTypeValueId,
                    PersonNickName = j.PersonAlias.Person.NickName,
                    PersonLastName = j.PersonAlias.Person.LastName,
                    PersonSuffixValueId = j.PersonAlias.Person.SuffixValueId,
                    PersonPhotoId = j.PersonAlias.Person.PhotoId,
                    PersonFirstPhoneNumber = j.PersonAlias.Person.PhoneNumbers.FirstOrDefault(),
                    SenderPersonRecordTypeValueId = j.CommunicationRecipient.Communication.SenderPersonAlias.Person.RecordTypeValueId,
                    SenderPersonNickName = j.CommunicationRecipient.Communication.SenderPersonAlias.Person.NickName,
                    SenderPersonLastName = j.CommunicationRecipient.Communication.SenderPersonAlias.Person.LastName,
                    SenderPersonSuffixValueId = j.CommunicationRecipient.Communication.SenderPersonAlias.Person.SuffixValueId,
                    j.CommunicationRecipient.CreatedDateTime,
                    j.CommunicationRecipient.CommunicationId,
                    j.CommunicationRecipient.Communication.SmsFromSystemPhoneNumberId,
                    CommunicationSMSMessage = j.CommunicationRecipient.Communication.SMSMessage,
                    j.CommunicationRecipient.SentMessage,
                    RecipientPersonAliasId = j.CommunicationRecipient.PersonAliasId,
                    RecipientPersonGuid = j.CommunicationRecipient.PersonAlias.Person.Guid,
                    PersonAge = j.PersonAlias.Person.Age,
                    PersonGender = j.PersonAlias.Person.Gender,
                    PersonAgeClassification = j.PersonAlias.Person.AgeClassification,
                    PersonPrimaryAliasGuid = j.PersonAlias.Person.Aliases
                        .Where( a => a.AliasPersonId == j.PersonAlias.PersonId )
                        .Select( a => ( Guid? ) a.Guid )
                        .FirstOrDefault()
                } );

            var mostRecentCommunicationRecipientQuery = communicationRecipientJoinQuery
                .GroupBy( r => r.PersonId )
                .Select( a =>
                    a.Select( s => new
                    {
                        s.Guid,
                        s.SenderPersonRecordTypeValueId,
                        s.SenderPersonNickName,
                        s.SenderPersonLastName,
                        s.SenderPersonSuffixValueId,
                        s.PersonId,
                        s.PersonGuid,
                        s.PersonRecordTypeValueId,
                        s.PersonNickName,
                        s.PersonLastName,
                        s.PersonSuffixValueId,
                        s.PersonPhotoId,
                        s.PersonFirstPhoneNumber,
                        s.CreatedDateTime,
                        s.CommunicationSMSMessage,
                        s.CommunicationId,
                        s.SmsFromSystemPhoneNumberId,
                        s.SentMessage,
                        s.RecipientPersonAliasId,
                        s.RecipientPersonGuid,
                        s.PersonAge,
                        s.PersonGender,
                        s.PersonAgeClassification,
                        s.PersonPrimaryAliasGuid
                    } ).OrderByDescending( s => s.CreatedDateTime ).FirstOrDefault()
                ).OrderByDescending( s => s.CreatedDateTime );

            var mostRecentCommunicationResponseList = mostRecentCommunicationResponseQuery
                .Select( r => new
                {
                    r.Id,
                    r.Guid,
                    r.MessageKey,
                    r.CreatedDateTime,
                    r.IsRead,
                    r.Response,
                    r.RelatedSmsFromSystemPhoneNumberId,
                    r.FromPersonAliasId,
                    FromPersonId = r.FromPersonAlias.PersonId,
                    FromPersonGuid = r.FromPersonAlias.Person.Guid,
                    FromPersonRecordTypeValueId = r.FromPersonAlias.Person.RecordTypeValueId,
                    FromPersonNickName = r.FromPersonAlias.Person.NickName,
                    FromPersonLastName = r.FromPersonAlias.Person.LastName,
                    FromPersonSuffixValueId = r.FromPersonAlias.Person.SuffixValueId,
                    FromPersonPhotoId = r.FromPersonAlias.Person.PhotoId,
                    FromPersonAge = r.FromPersonAlias.Person.Age,
                    FromPersonGender = r.FromPersonAlias.Person.Gender,
                    FromPersonAgeClassification = r.FromPersonAlias.Person.AgeClassification,
                    FromPersonPrimaryAliasGuid = r.FromPersonAlias.Person.Aliases
                        .Where( a => a.AliasPersonId == r.FromPersonAlias.PersonId )
                        .Select( a => ( Guid? ) a.Guid )
                        .FirstOrDefault()
                } )
                .Take( maxCount )
                .ToList();

            List<CommunicationRecipientResponse> communicationRecipientResponseList = new List<CommunicationRecipientResponse>();

            foreach ( var mostRecentResponse in mostRecentCommunicationResponseList )
            {
                var relatedSmsFromSystemPhoneNumber = SystemPhoneNumberCache.Get( mostRecentResponse.RelatedSmsFromSystemPhoneNumberId.Value );
                var communicationRecipientResponse = new CommunicationRecipientResponse
                {
                    CreatedDateTime = mostRecentResponse.CreatedDateTime,
                    PersonId = mostRecentResponse.FromPersonId,
                    RecordTypeValueId = mostRecentResponse.FromPersonRecordTypeValueId,
                    FullName = Person.FormatFullName(
                        mostRecentResponse.FromPersonNickName,
                        mostRecentResponse.FromPersonLastName,
                        mostRecentResponse.FromPersonSuffixValueId,
                        mostRecentResponse.FromPersonRecordTypeValueId ),
                    RecipientPhotoId = mostRecentResponse.FromPersonPhotoId,
                    IsRead = mostRecentResponse.IsRead,
                    ConversationKey = CommunicationService.GetSmsConversationKey( relatedSmsFromSystemPhoneNumber.Guid, mostRecentResponse.FromPersonGuid ),
                    MessageKey = $"R:{mostRecentResponse.Guid}",
                    ContactKey = mostRecentResponse.MessageKey,
                    IsOutbound = false,
                    RecipientPersonAliasId = mostRecentResponse.FromPersonAliasId,
                    RecipientPersonGuid = mostRecentResponse.FromPersonGuid,
                    SMSMessage = mostRecentResponse.Response,
                    CommunicationResponseId = mostRecentResponse.Id,
                    RecipientPrimaryAliasGuid = mostRecentResponse.FromPersonPrimaryAliasGuid,
                    Initials = GetInitials( mostRecentResponse.FromPersonNickName, mostRecentResponse.FromPersonLastName ),
                    Age = mostRecentResponse.FromPersonAge,
                    Gender = mostRecentResponse.FromPersonGender,
                    AgeClassification = mostRecentResponse.FromPersonAgeClassification
                };

                communicationRecipientResponseList.Add( communicationRecipientResponse );
            }

            var mostRecentCommunicationRecipientList = mostRecentCommunicationRecipientQuery.Take( maxCount ).ToList();

            var recordTypeValueIdNamelessId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Id;

            foreach ( var mostRecentCommunicationRecipient in mostRecentCommunicationRecipientList )
            {
                var recipientResponse = communicationRecipientResponseList.FirstOrDefault( a => a.PersonId == mostRecentCommunicationRecipient.PersonId );
                var isConversationRead = recipientResponse?.IsRead ?? true;

                var relatedSmsFromSystemPhoneNumber = SystemPhoneNumberCache.Get( mostRecentCommunicationRecipient.SmsFromSystemPhoneNumberId.Value );
                var communicationRecipientResponse = new CommunicationRecipientResponse
                {
                    CreatedDateTime = mostRecentCommunicationRecipient.CreatedDateTime,
                    PersonId = mostRecentCommunicationRecipient.PersonId,
                    RecordTypeValueId = mostRecentCommunicationRecipient.PersonRecordTypeValueId,
                    OutboundSenderFullName = Person.FormatFullName(
                        mostRecentCommunicationRecipient.SenderPersonNickName,
                        mostRecentCommunicationRecipient.SenderPersonLastName,
                        mostRecentCommunicationRecipient.SenderPersonSuffixValueId,
                        mostRecentCommunicationRecipient.SenderPersonRecordTypeValueId ),
                    FullName = Person.FormatFullName(
                        mostRecentCommunicationRecipient.PersonNickName,
                        mostRecentCommunicationRecipient.PersonLastName,
                        mostRecentCommunicationRecipient.PersonSuffixValueId,
                        mostRecentCommunicationRecipient.PersonRecordTypeValueId ),
                    RecipientPhotoId = mostRecentCommunicationRecipient.PersonPhotoId,
                    IsOutbound = true,
                    IsRead = isConversationRead,
                    ConversationKey = CommunicationService.GetSmsConversationKey( relatedSmsFromSystemPhoneNumber.Guid, mostRecentCommunicationRecipient.PersonGuid ),
                    MessageKey = $"C:{mostRecentCommunicationRecipient.Guid}",
                    RecipientPersonAliasId = mostRecentCommunicationRecipient.RecipientPersonAliasId,
                    RecipientPersonGuid = mostRecentCommunicationRecipient.RecipientPersonGuid,
                    SMSMessage = mostRecentCommunicationRecipient.SentMessage.IsNullOrWhiteSpace() ? mostRecentCommunicationRecipient.CommunicationSMSMessage : mostRecentCommunicationRecipient.SentMessage,
                    CommunicationId = mostRecentCommunicationRecipient.CommunicationId,
                    RecipientPrimaryAliasGuid = mostRecentCommunicationRecipient.PersonPrimaryAliasGuid,
                    Initials = GetInitials( mostRecentCommunicationRecipient.PersonNickName, mostRecentCommunicationRecipient.PersonLastName ),
                    Age = mostRecentCommunicationRecipient.PersonAge,
                    Gender = mostRecentCommunicationRecipient.PersonGender,
                    AgeClassification = mostRecentCommunicationRecipient.PersonAgeClassification
                };

                if ( mostRecentCommunicationRecipient?.PersonRecordTypeValueId == recordTypeValueIdNamelessId )
                {
                    // if the person is nameless, we'll need to know their number since we don't know their name
                    communicationRecipientResponse.ContactKey = mostRecentCommunicationRecipient.PersonFirstPhoneNumber?.Number;
                }
                else
                {
                    // If the Person is not nameless, we just need to show their name, not their number
                    communicationRecipientResponse.ContactKey = null;
                }

                communicationRecipientResponseList.Add( communicationRecipientResponse );
            }

            // NOTE: We actually have up to twice the max count at this point, because we are combining results from
            // CommunicationRecipient and CommunicationResponse, and we took the maxCount of each of those.
            // Now, we see what that combination ends up looking like when we sort it by CreatedDateTime
            communicationRecipientResponseList = communicationRecipientResponseList
                .GroupBy( r => r.PersonId )
                .Select( a => a.OrderByDescending( x => x.CreatedDateTime ).FirstOrDefault() )
                .OrderByDescending( a => a.CreatedDateTime ).Take( maxCount ).ToList();

            return communicationRecipientResponseList;
        }

        /// <summary>
        /// Builds a person's initials (first character of nick name + last name) the same way
        /// <see cref="Person.Initials"/> does, from values the conversation queries already
        /// project - so we don't have to materialize the <see cref="Person"/>.
        /// </summary>
        /// <param name="nickName">The person's nick name.</param>
        /// <param name="lastName">The person's last name.</param>
        /// <returns>The person's initials.</returns>
        private static string GetInitials( string nickName, string lastName )
        {
            var firstInitial = string.IsNullOrEmpty( nickName ) ? string.Empty : nickName.Substring( 0, 1 );
            var lastInitial = string.IsNullOrEmpty( lastName ) ? string.Empty : lastName.Substring( 0, 1 );

            return firstInitial + lastInitial;
        }

        /// <summary>
        /// Gets the SMS conversation history for a person alias ID. Includes the communication sent by Rock that the person may be responding to.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="relatedSmsFromSystemPhoneNumber">The system phone number to use for the conversation with the person.</param>
        /// <returns>A list of </returns>
        public List<CommunicationRecipientResponse> GetCommunicationConversationForPerson( int personId, SystemPhoneNumberCache relatedSmsFromSystemPhoneNumber )
        {
            List<CommunicationRecipientResponse> communicationRecipientResponseList = new List<CommunicationRecipientResponse>();

            var smsMediumEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Value;

            var personAliasQuery = new PersonAliasService( this.Context as RockContext ).Queryable().Where( a => a.PersonId == personId );
            var personAliasIdQuery = personAliasQuery.Select( a => a.Id );

            var communicationResponseQuery = this.Queryable()
                .Where( r => r.RelatedMediumEntityTypeId == smsMediumEntityTypeId
                        && r.RelatedSmsFromSystemPhoneNumberId == relatedSmsFromSystemPhoneNumber.Id
                        && r.FromPersonAliasId.HasValue
                        && personAliasIdQuery.Contains( r.FromPersonAliasId.Value ) )
                .Select( r => new
                {
                    r.Id,
                    r.Guid,
                    r.MessageKey,
                    r.CreatedDateTime,
                    r.IsRead,
                    r.Response,
                    r.FromPersonAliasId,
                    r.FromPersonAlias.PersonId,
                    FromPersonGuid = r.FromPersonAlias.Person.Guid,
                    FromPersonRecordTypeValueId = r.FromPersonAlias.Person.RecordTypeValueId,
                    FromPersonNickName = r.FromPersonAlias.Person.NickName,
                    FromPersonLastName = r.FromPersonAlias.Person.LastName,
                    FromPersonSuffixValueId = r.FromPersonAlias.Person.SuffixValueId,
                    FromPersonPhotoId = r.FromPersonAlias.Person.PhotoId,
                    FromPersonAge = r.FromPersonAlias.Person.Age,
                    FromPersonGender = r.FromPersonAlias.Person.Gender,
                    FromPersonAgeClassification = r.FromPersonAlias.Person.AgeClassification,
                    FromPersonPrimaryAliasGuid = r.FromPersonAlias.Person.Aliases
                        .Where( a => a.AliasPersonId == r.FromPersonAlias.PersonId )
                        .Select( a => ( Guid? ) a.Guid )
                        .FirstOrDefault()
                } );

            var communicationResponseList = communicationResponseQuery.ToList();

            foreach ( var communicationResponse in communicationResponseList )
            {
                var communicationRecipientResponse = new CommunicationRecipientResponse
                {
                    CreatedDateTime = communicationResponse.CreatedDateTime,
                    PersonId = communicationResponse.FromPersonAliasId,
                    FullName = Person.FormatFullName(
                        communicationResponse.FromPersonNickName,
                        communicationResponse.FromPersonLastName,
                        communicationResponse.FromPersonSuffixValueId,
                        communicationResponse.FromPersonRecordTypeValueId ),
                    RecipientPhotoId = communicationResponse.FromPersonPhotoId,
                    IsRead = communicationResponse.IsRead,
                    ConversationKey = CommunicationService.GetSmsConversationKey( relatedSmsFromSystemPhoneNumber.Guid, communicationResponse.FromPersonGuid ),
                    MessageKey = $"R:{communicationResponse.Guid}",
                    ContactKey = communicationResponse.MessageKey,
                    IsOutbound = false,
                    RecipientPersonAliasId = communicationResponse.FromPersonAliasId,
                    RecipientPersonGuid = communicationResponse.FromPersonGuid,
                    SMSMessage = communicationResponse.Response,
                    MessageStatus = CommunicationRecipientStatus.Delivered, // We are just going to call these delivered because we have them. Setting this will tell the UI to not display the status.
                    CommunicationResponseId = communicationResponse.Id,
                    RecipientPrimaryAliasGuid = communicationResponse.FromPersonPrimaryAliasGuid,
                    Initials = GetInitials( communicationResponse.FromPersonNickName, communicationResponse.FromPersonLastName ),
                    Age = communicationResponse.FromPersonAge,
                    Gender = communicationResponse.FromPersonGender,
                    AgeClassification = communicationResponse.FromPersonAgeClassification,
                };

                communicationRecipientResponseList.Add( communicationRecipientResponse );
            }

            var communicationRecipientList = new CommunicationRecipientService( this.Context as RockContext )
                .Queryable()
                .Where( r => r.MediumEntityTypeId == smsMediumEntityTypeId )
                .Where( r => r.Communication.SmsFromSystemPhoneNumberId == relatedSmsFromSystemPhoneNumber.Id )
                .Where( r => r.PersonAliasId.HasValue )
                .Where( r => personAliasIdQuery.Contains( r.PersonAliasId.Value ) )
                .Where( r => r.Status == CommunicationRecipientStatus.Delivered || r.Status == CommunicationRecipientStatus.Pending )
                .Select( r => new
                {
                    r.Guid,
                    r.CreatedDateTime,
                    r.SentMessage,
                    r.Status,
                    r.CommunicationId,
                    r.PersonAliasId,
                    r.PersonAlias.PersonId,
                    PersonGuid = r.PersonAlias.Person.Guid,
                    PersonRecordTypeValueId = r.PersonAlias.Person.RecordTypeValueId,
                    PersonNickName = r.PersonAlias.Person.NickName,
                    PersonLastName = r.PersonAlias.Person.LastName,
                    PersonSuffixValueId = r.PersonAlias.Person.SuffixValueId,
                    PersonPhotoId = r.PersonAlias.Person.PhotoId,
                    PersonFirstPhoneNumber = r.PersonAlias.Person.PhoneNumbers.FirstOrDefault(),
                    SenderPersonRecordTypeValueId = r.Communication.SenderPersonAlias.Person.RecordTypeValueId,
                    SenderPersonNickName = r.Communication.SenderPersonAlias.Person.NickName,
                    SenderPersonLastName = r.Communication.SenderPersonAlias.Person.LastName,
                    SenderPersonSuffixValueId = r.Communication.SenderPersonAlias.Person.SuffixValueId,
                    PersonAge = r.PersonAlias.Person.Age,
                    PersonGender = r.PersonAlias.Person.Gender,
                    PersonAgeClassification = r.PersonAlias.Person.AgeClassification,
                    PersonPrimaryAliasGuid = r.PersonAlias.Person.Aliases
                        .Where( a => a.AliasPersonId == r.PersonAlias.PersonId )
                        .Select( a => ( Guid? ) a.Guid )
                        .FirstOrDefault()
                } )
                .ToList();

            var recordTypeValueIdNamelessId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Id;

            foreach ( var communicationRecipient in communicationRecipientList )
            {
                var communicationRecipientResponse = new CommunicationRecipientResponse
                {
                    CreatedDateTime = communicationRecipient.CreatedDateTime,
                    OutboundSenderFullName = Person.FormatFullName(
                        communicationRecipient.SenderPersonNickName,
                        communicationRecipient.SenderPersonLastName,
                        communicationRecipient.SenderPersonSuffixValueId,
                        communicationRecipient.SenderPersonRecordTypeValueId ),
                    PersonId = communicationRecipient.PersonId,
                    FullName = Person.FormatFullName(
                        communicationRecipient.PersonNickName,
                        communicationRecipient.PersonLastName,
                        communicationRecipient.PersonSuffixValueId,
                        communicationRecipient.PersonRecordTypeValueId ),
                    RecipientPhotoId = communicationRecipient.PersonPhotoId,
                    IsRead = true,
                    IsOutbound = true,
                    ConversationKey = CommunicationService.GetSmsConversationKey( relatedSmsFromSystemPhoneNumber.Guid, communicationRecipient.PersonGuid ),
                    MessageKey = $"C:{communicationRecipient.Guid}",
                    RecipientPersonAliasId = communicationRecipient.PersonAliasId,
                    RecipientPersonGuid = communicationRecipient.PersonGuid,
                    SMSMessage = communicationRecipient.SentMessage,
                    MessageStatus = communicationRecipient.Status,
                    CommunicationId = communicationRecipient.CommunicationId,
                    RecipientPrimaryAliasGuid = communicationRecipient.PersonPrimaryAliasGuid,
                    Initials = GetInitials( communicationRecipient.PersonNickName, communicationRecipient.PersonLastName ),
                    Age = communicationRecipient.PersonAge,
                    Gender = communicationRecipient.PersonGender,
                    AgeClassification = communicationRecipient.PersonAgeClassification,
                };

                if ( communicationRecipient.PersonRecordTypeValueId == recordTypeValueIdNamelessId )
                {
                    // if the person is nameless, we'll need to know their number since we don't know their name
                    communicationRecipientResponse.ContactKey = communicationRecipient.PersonFirstPhoneNumber?.Number;
                }
                else
                {
                    // If the Person is not nameless, we just need to show their name, not their number
                    communicationRecipientResponse.ContactKey = null;
                }

                communicationRecipientResponseList.Add( communicationRecipientResponse );
            }

            return communicationRecipientResponseList.OrderBy( a => a.CreatedDateTime ).ToList();
        }

        /// <summary>
        /// Updates the IsRead property of SMS Responses sent from the provided
        /// person to the System Phone Number.
        /// </summary>
        /// <param name="fromPersonId">From person identifier.</param>
        /// <param name="relatedSmsFromPhoneNumber">The system phone number side of the conversation to be marked as read.</param>
        public void UpdateReadPropertyByFromPersonId( int fromPersonId, SystemPhoneNumberCache relatedSmsFromPhoneNumber )
        {
            var personAliasIdQuery = new PersonAliasService( this.Context as RockContext )
                .Queryable()
                .Where( a => a.PersonId == fromPersonId )
                .Select( a => a.Id );

            var communicationResponsesToUpdateQueryable = Queryable()
                .Where( a => a.FromPersonAliasId.HasValue
                    && personAliasIdQuery.Contains( a.FromPersonAliasId.Value )
                    && a.RelatedSmsFromSystemPhoneNumberId == relatedSmsFromPhoneNumber.Id
                    && a.IsRead == false );

            this.Context.BulkUpdate( communicationResponsesToUpdateQueryable, a => new CommunicationResponse { IsRead = true } );

            var personGuid = new PersonService( Context as RockContext ).GetGuid( fromPersonId );

            if ( personGuid.HasValue )
            {
                var conversationKey = CommunicationService.GetSmsConversationKey( relatedSmsFromPhoneNumber.Guid, personGuid.Value );

                CommunicationService.SendConversationReadSmsRealTimeNotificationsInBackground( conversationKey );
                CommunicationService.SendConversationReadStatusChangedRealTimeNotificationsInBackground( conversationKey, true );
            }

            UpdateResponseNotificationMessagesInBackground( relatedSmsFromPhoneNumber, fromPersonId );
        }

        /// <summary>
        /// Marks the IsRead property of SMS Responses sent from the provided
        /// person to the System Phone Number to false.
        /// </summary>
        /// <param name="fromPersonId">From person identifier.</param>
        /// <param name="relatedSmsFromPhoneNumber">The system phone number side of the conversation to be marked as read.</param>
        public string MarkResponseAsUnread( int fromPersonId, SystemPhoneNumberCache relatedSmsFromPhoneNumber )
        {
            var personAliasIdQuery = new PersonAliasService( this.Context as RockContext )
                .Queryable()
                .Where( a => a.PersonId == fromPersonId )
                .Select( a => a.Id );

            var responseToUpdate = Queryable()
                .Where( a => a.FromPersonAliasId.HasValue
                    && personAliasIdQuery.Contains( a.FromPersonAliasId.Value )
                    && a.RelatedSmsFromSystemPhoneNumberId == relatedSmsFromPhoneNumber.Id
                    && a.IsRead == true )
                .OrderByDescending( a => a.CreatedDateTime )
                .FirstOrDefault();

            if ( responseToUpdate != null )
            {
                responseToUpdate.IsRead = false;
                this.Context.SaveChanges();

                var personGuid = new PersonService( Context as RockContext ).GetGuid( fromPersonId );

                if ( personGuid.HasValue )
                {
                    var conversationKey = CommunicationService.GetSmsConversationKey( relatedSmsFromPhoneNumber.Guid, personGuid.Value );

                    CommunicationService.SendConversationReadStatusChangedRealTimeNotificationsInBackground( conversationKey, false );
                }

                UpdateResponseNotificationMessagesInBackground( relatedSmsFromPhoneNumber, fromPersonId );
                return string.Empty;
            }
            else
            {
                return "Read status can’t be updated without a reply from the recipient.";
            }
        }

        /// <summary>
        /// Gets the conversation message bag that will represent the specified
        /// communication response.
        /// </summary>
        /// <param name="communicationResponseId">The communication response identifier.</param>
        /// <returns>A <see cref="ConversationMessageBag"/> that will represent the communication response message.</returns>
        internal ConversationMessageBag GetConversationMessageBag( int communicationResponseId )
        {
            var publicUrl = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );
            var namelessRecordValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Value;

            var communicationResponse = Queryable()
                .Where( cr => cr.Id == communicationResponseId )
                .Select( cr => new
                {
                    cr.Guid,
                    cr.RelatedSmsFromSystemPhoneNumberId,
                    cr.FromPersonAliasId,
                    FromPerson = cr.FromPersonAlias.Person,
                    cr.CreatedDateTime,
                    cr.IsRead,
                    cr.Response,
                    cr.MessageKey,
                    Attachments = cr.Attachments.Select( a => new
                    {
                        a.BinaryFile.Guid,
                        a.BinaryFile.MimeType,
                        a.BinaryFile.FileName
                    } )
                } )
                .FirstOrDefault();

            var rockPhoneNumber = SystemPhoneNumberCache.Get( communicationResponse.RelatedSmsFromSystemPhoneNumberId ?? 0 );

            // Response must have an associated Rock phone number.
            if ( rockPhoneNumber == null )
            {
                throw new Exception( "Unable to determine Rock phone number." );
            }

            // Response must have a sender person.
            if ( !communicationResponse.FromPersonAliasId.HasValue )
            {
                throw new Exception( "Unable to determine message sender." );
            }

            var messageBag = new ConversationMessageBag
            {
                ConversationKey = CommunicationService.GetSmsConversationKey( rockPhoneNumber.Guid, communicationResponse.FromPerson.Guid ),
                MessageKey = $"R:{communicationResponse.Guid}",
                RockContactKey = rockPhoneNumber.Guid.ToString(),
                MessageDateTime = communicationResponse.CreatedDateTime,
                IsRead = communicationResponse.IsRead,
                Message = communicationResponse.Response,
                IsOutbound = false,
                IsNamelessPerson = namelessRecordValueId == communicationResponse.FromPerson.RecordTypeValueId,
                PersonGuid = communicationResponse.FromPerson.Guid,
                FullName = communicationResponse.FromPerson.FullName,
                ContactKey = communicationResponse.MessageKey,
                Attachments = new List<ConversationAttachmentBag>()
            };

            var photoUrl = communicationResponse?.FromPerson != null
                ? Rock.Model.Person.GetPersonPhotoUrl( communicationResponse.FromPerson, 256, 256 )
                : "/Assets/Images/person-no-photo-unknown.svg?width=256&height=256";

            if ( !Uri.IsWellFormedUriString( photoUrl, UriKind.Absolute ) )
            {
                photoUrl = VirtualPathUtility.ToAbsolute( photoUrl );
            }

            messageBag.PhotoUrl = publicUrl.IsNotNullOrWhiteSpace() ? publicUrl + photoUrl : photoUrl;

            foreach ( var attachment in communicationResponse.Attachments )
            {
                var isImage = attachment.MimeType.StartsWith( "image/", StringComparison.OrdinalIgnoreCase ) == true;

                if ( isImage )
                {
                    messageBag.Attachments.Add( new ConversationAttachmentBag
                    {
                        FileName = attachment.FileName,
                        Url = FileUrlHelper.GetImageUrl( attachment.Guid ),
                        ThumbnailUrl = FileUrlHelper.GetImageUrl( attachment.Guid, new GetImageUrlOptions { MaxWidth = 512, MaxHeight = 512 } )
                    } );
                }
                else
                {
                    messageBag.Attachments.Add( new ConversationAttachmentBag
                    {
                        FileName = attachment.FileName,
                        Url = FileUrlHelper.GetFileUrl( attachment.Guid ),
                        ThumbnailUrl = null
                    } );
                }
            }

            return messageBag;
        }

        /// <summary>
        /// Updates all notification messages in regards to a new response
        /// being received or an existing response being read.
        /// </summary>
        /// <param name="phoneNumber">The phone number that represents Rock's side of the conversation.</param>
        /// <param name="fromPersonId">The identifier of the person that represents the other parties side of the conversation.</param>
        internal static void UpdateResponseNotificationMessagesInBackground( SystemPhoneNumberCache phoneNumber, int fromPersonId )
        {
            Task.Run( () =>
            {
                try
                {
                    Rock.Core.NotificationMessageTypes.SmsConversation.UpdateNotificationMessages( phoneNumber, fromPersonId );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            } );
        }
    }
}
