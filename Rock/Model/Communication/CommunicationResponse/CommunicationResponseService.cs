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

using Rock.Data;
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
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <returns></returns>
        public IQueryable GetResponsesFromPersonAliasIdForSMSNumber( int fromPersonAliasId, int relatedSmsFromDefinedValueId )
        {
            return Queryable()
                .Where( r => r.FromPersonAliasId == fromPersonAliasId )
                .Where( r => r.RelatedSmsFromDefinedValueId == relatedSmsFromDefinedValueId );
        }

        /// <summary>
        /// Gets the communications and response recipients.
        /// </summary>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="showReadMessages">if set to <c>true</c> [show read messages].</param>
        /// <param name="maxCount">The maximum count.</param>
        /// <returns></returns>
        public List<CommunicationRecipientResponse> GetCommunicationResponseRecipients( int relatedSmsFromDefinedValueId, DateTime startDateTime, bool showReadMessages, int maxCount )
        {
            return GetCommunicationResponseRecipients( relatedSmsFromDefinedValueId, startDateTime, showReadMessages, maxCount, null );
        }

        /// <summary>
        /// Gets the communications and response recipients.
        /// </summary>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="showReadMessages">if set to <c>true</c> [show read messages].</param>
        /// <param name="maxCount">The maximum count.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public List<CommunicationRecipientResponse> GetCommunicationResponseRecipients( int relatedSmsFromDefinedValueId, DateTime startDateTime, bool showReadMessages, int maxCount, int? personId )
        {
            var smsMediumEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Value;

            IQueryable<CommunicationResponse> communicationResponseQuery = this.Queryable()
                .Where( r => r.RelatedMediumEntityTypeId == smsMediumEntityTypeId && r.RelatedSmsFromDefinedValueId == relatedSmsFromDefinedValueId && r.CreatedDateTime >= startDateTime && r.FromPersonAliasId.HasValue );

            IQueryable<CommunicationRecipient> communicationRecipientQuery = new CommunicationRecipientService( this.Context as RockContext ).Queryable()
                .Where( r =>
                r.MediumEntityTypeId == smsMediumEntityTypeId
                && r.Communication.SMSFromDefinedValueId == relatedSmsFromDefinedValueId
                && r.CreatedDateTime >= startDateTime
                && r.Status == CommunicationRecipientStatus.Delivered );

            if ( !showReadMessages )
            {
                communicationResponseQuery = communicationResponseQuery.Where( r => r.IsRead == false );
            }

            return GetCommunicationResponseRecipients( maxCount, personId, communicationResponseQuery, communicationRecipientQuery );
        }

        /// <summary>
        /// Gets the communications and response recipients.
        /// </summary>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="maxCount">The maximum count.</param>
        /// <param name="filter">The message filter.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public List<CommunicationRecipientResponse> GetCommunicationResponseRecipients( int relatedSmsFromDefinedValueId, DateTime startDateTime, int maxCount, CommunicationMessageFilter filter, int? personId )
        {
            var smsMediumEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Value;

            IQueryable<CommunicationResponse> communicationResponseQuery = this.Queryable()
                .Where( r => r.RelatedMediumEntityTypeId == smsMediumEntityTypeId && r.RelatedSmsFromDefinedValueId == relatedSmsFromDefinedValueId && r.CreatedDateTime >= startDateTime && r.FromPersonAliasId.HasValue );

            IQueryable<CommunicationRecipient> communicationRecipientQuery = new CommunicationRecipientService( this.Context as RockContext ).Queryable()
                .Where( r =>
                r.MediumEntityTypeId == smsMediumEntityTypeId
                && r.Communication.SMSFromDefinedValueId == relatedSmsFromDefinedValueId
                && r.CreatedDateTime >= startDateTime
                && r.Status == CommunicationRecipientStatus.Delivered );

            if ( filter == CommunicationMessageFilter.ShowUnreadReplies )
            {
                communicationResponseQuery = communicationResponseQuery.Where( r => r.IsRead == false );
            }

            switch ( filter )
            {
                case CommunicationMessageFilter.ShowUnreadReplies:
                case CommunicationMessageFilter.ShowAllReplies:
                    communicationRecipientQuery = communicationRecipientQuery.Join( communicationResponseQuery,
                        communicationRecipient => communicationRecipient.PersonAliasId,
                        communicationResponse => communicationResponse.ToPersonAliasId,
                        ( communicationRecipient, communicationResponse ) => communicationRecipient );
                    break;
            }

            return GetCommunicationResponseRecipients( maxCount, personId, communicationResponseQuery, communicationRecipientQuery );
        }

        private List<CommunicationRecipientResponse> GetCommunicationResponseRecipients( int maxCount, int? personId, IQueryable<CommunicationResponse> communicationResponseQuery, IQueryable<CommunicationRecipient> communicationRecipientQuery )
        {
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
            var communicationRecipientJoinQuery =
                from cr in communicationRecipientQuery.Where( a => a.PersonAliasId.HasValue )
                join pa in personAliasQuery on cr.PersonAliasId.Value equals pa.Id
                select new
                {
                    PersonId = pa.PersonId,
                    Person = pa.Person,
                    CreatedDateTime = cr.CreatedDateTime,
                    cr.Communication,
                    cr.CommunicationId,
                    CommunicationSMSMessage = cr.Communication.SMSMessage,
                    SentMessage = cr.SentMessage,
                    PersonAliasId = cr.PersonAliasId
                };

            var mostRecentCommunicationRecipientQuery = communicationRecipientJoinQuery
                .GroupBy( r => r.PersonId )
                .Select( a =>
                    a.Select( s => new
                    {
                        s.Person,
                        s.CreatedDateTime,
                        s.CommunicationSMSMessage,
                        s.CommunicationId,
                        s.Communication,
                        s.SentMessage,
                        s.PersonAliasId
                    } ).OrderByDescending( s => s.CreatedDateTime ).FirstOrDefault()
                );

            var mostRecentCommunicationResponseList = mostRecentCommunicationResponseQuery.Include( a => a.FromPersonAlias.Person ).AsNoTracking().Take( maxCount ).ToList();

            List<CommunicationRecipientResponse> communicationRecipientResponseList = new List<CommunicationRecipientResponse>();

            foreach ( var mostRecentCommunicationResponse in mostRecentCommunicationResponseList )
            {
                var communicationRecipientResponse = new CommunicationRecipientResponse
                {
                    CreatedDateTime = mostRecentCommunicationResponse.CreatedDateTime,
                    PersonId = mostRecentCommunicationResponse?.FromPersonAlias.PersonId,
                    RecordTypeValueId = mostRecentCommunicationResponse?.FromPersonAlias.Person.RecordTypeValueId,
                    FullName = mostRecentCommunicationResponse?.FromPersonAlias.Person.FullName,
                    IsRead = mostRecentCommunicationResponse.IsRead,
                    MessageKey = mostRecentCommunicationResponse.MessageKey,
                    IsOutbound = false,
                    RecipientPersonAliasId = mostRecentCommunicationResponse.FromPersonAliasId,
                    SMSMessage = mostRecentCommunicationResponse.Response,
                    CommunicationResponseId = mostRecentCommunicationResponse.Id
                };

                communicationRecipientResponseList.Add( communicationRecipientResponse );
            }

            var mostRecentCommunicationRecipientList = mostRecentCommunicationRecipientQuery.Take( maxCount ).ToList();

            foreach ( var mostRecentCommunicationRecipient in mostRecentCommunicationRecipientList )
            {
                var communicationRecipientResponse = new CommunicationRecipientResponse
                {
                    CreatedDateTime = mostRecentCommunicationRecipient.CreatedDateTime,
                    PersonId = mostRecentCommunicationRecipient.Person.Id,
                    RecordTypeValueId = mostRecentCommunicationRecipient.Person.RecordTypeValueId,
                    FullName = mostRecentCommunicationRecipient.Person.FullName,
                    IsOutbound = true,
                    IsRead = true,
                    MessageKey = null, // communication recipients just need to show their name, not their number
                    RecipientPersonAliasId = mostRecentCommunicationRecipient.PersonAliasId,
                    SMSMessage = mostRecentCommunicationRecipient.SentMessage.IsNullOrWhiteSpace() ? mostRecentCommunicationRecipient.CommunicationSMSMessage : mostRecentCommunicationRecipient.SentMessage,
                    CommunicationId = mostRecentCommunicationRecipient.CommunicationId
                };

                if ( mostRecentCommunicationRecipient?.Person.IsNameless() == true )
                {
                    // if the person is nameless, we'll need to know their number since we don't know their name
                    communicationRecipientResponse.MessageKey = mostRecentCommunicationRecipient.Person.PhoneNumbers.FirstOrDefault()?.Number;
                }
                else
                {
                    // If the Person is not nameless, we just need to show their name, not their number
                    communicationRecipientResponse.MessageKey = null;
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
        /// Gets the SMS conversation history for a person alias ID. Includes the communication sent by Rock that the person may be responding to.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use GetCommunicationConversationForPerson instead." )]
        public List<CommunicationRecipientResponse> GetCommunicationConversation( int personAliasId, int relatedSmsFromDefinedValueId )
        {
            int? personId = new PersonAliasService( this.Context as RockContext ).GetPersonId( personAliasId );
            if ( personId.HasValue )
            {
                return GetCommunicationConversationForPerson( personId.Value, relatedSmsFromDefinedValueId );
            }
            else
            {
                return new List<CommunicationRecipientResponse>();
            }
        }

        /// <summary>
        /// Gets the SMS conversation history for a person alias ID. Includes the communication sent by Rock that the person may be responding to.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <returns>List&lt;CommunicationRecipientResponse&gt;.</returns>
        public List<CommunicationRecipientResponse> GetCommunicationConversationForPerson( int personId, int relatedSmsFromDefinedValueId )
        {
            List<CommunicationRecipientResponse> communicationRecipientResponseList = new List<CommunicationRecipientResponse>();

            var smsMediumEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Value;

            var personAliasIdQuery = new PersonAliasService( this.Context as RockContext ).Queryable().Where( a => a.PersonId == personId ).Select( a => a.Id );

            var communicationResponseQuery = this.Queryable()
                .Where( r => r.RelatedMediumEntityTypeId == smsMediumEntityTypeId
                        && r.RelatedSmsFromDefinedValueId == relatedSmsFromDefinedValueId
                        && r.FromPersonAliasId.HasValue
                        && personAliasIdQuery.Contains( r.FromPersonAliasId.Value )
                         );

            var communicationResponseList = communicationResponseQuery.ToList();

            foreach ( var communicationResponse in communicationResponseList )
            {
                var communicationRecipientResponse = new CommunicationRecipientResponse
                {
                    CreatedDateTime = communicationResponse.CreatedDateTime,
                    PersonId = communicationResponse?.FromPersonAlias?.PersonId,
                    FullName = communicationResponse?.FromPersonAlias?.Person.FullName,
                    IsRead = communicationResponse.IsRead,
                    MessageKey = communicationResponse.MessageKey,
                    IsOutbound = false,
                    RecipientPersonAliasId = communicationResponse.FromPersonAliasId,
                    SMSMessage = communicationResponse.Response,
                    MessageStatus = CommunicationRecipientStatus.Delivered, // We are just going to call these delivered because we have them. Setting this will tell the UI to not display the status.
                    CommunicationResponseId = communicationResponse.Id,
                };

                communicationRecipientResponseList.Add( communicationRecipientResponse );
            }

            var communicationRecipientQuery = new CommunicationRecipientService( this.Context as RockContext )
                .Queryable()
                .Where( r => r.MediumEntityTypeId == smsMediumEntityTypeId )
                .Where( r => r.Communication.SMSFromDefinedValueId == relatedSmsFromDefinedValueId )
                .Where( r => r.PersonAliasId.HasValue )
                .Where( r => personAliasIdQuery.Contains( r.PersonAliasId.Value ) )
                .Where( r => r.Status == CommunicationRecipientStatus.Delivered || r.Status == CommunicationRecipientStatus.Pending );

            var communicationRecipientList = communicationRecipientQuery.Include( a => a.PersonAlias.Person.PhoneNumbers ).Select( a => new
            {
                a.CreatedDateTime,
                a.Communication.SenderPersonAlias.Person,
                a.Communication,
                PersonAliasId = a.Communication.SenderPersonAliasId,
                a.SentMessage,
                a.Status
            } ).ToList();

            foreach ( var communicationRecipient in communicationRecipientList )
            {
                var communicationRecipientResponse = new CommunicationRecipientResponse
                {
                    CreatedDateTime = communicationRecipient.CreatedDateTime,
                    PersonId = communicationRecipient.Person?.Id,
                    FullName = communicationRecipient.Person?.FullName,
                    IsRead = true,
                    IsOutbound = true,
                    RecipientPersonAliasId = communicationRecipient.PersonAliasId,
                    SMSMessage = communicationRecipient.SentMessage,
                    MessageStatus = communicationRecipient.Status,
                    CommunicationId = communicationRecipient.Communication?.Id,
                };

                if ( communicationRecipient.Person?.IsNameless() == true )
                {
                    // if the person is nameless, we'll need to know their number since we don't know their name
                    communicationRecipientResponse.MessageKey = communicationRecipient.Person?.PhoneNumbers.FirstOrDefault()?.Number;
                }
                else
                {
                    // If the Person is not nameless, we just need to show their name, not their number
                    communicationRecipientResponse.MessageKey = null;
                }

                communicationRecipientResponseList.Add( communicationRecipientResponse );
            }

            return communicationRecipientResponseList.OrderBy( a => a.CreatedDateTime ).ToList();
        }

        /// <summary>
        /// Updates the IsRead property of SMS Responses sent from the provided person to the SMSPhone number stored in SmsFromDefinedValue.
        /// </summary>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <param name="relatedSmsFromDefinedValueId">The defined value ID of the from SMS phone number.</param>
        [Obsolete( "Use UpdateReadPropertyByFromPersonId instead." )]
        [RockObsolete( "1.13" )]
        public void UpdateReadPropertyByFromPersonAliasId( int fromPersonAliasId, int relatedSmsFromDefinedValueId )
        {
            int? fromPersonId = new PersonAliasService( this.Context as RockContext ).GetPersonId( fromPersonAliasId );
            if ( fromPersonId != null )
            {
                UpdateReadPropertyByFromPersonId( fromPersonId.Value, relatedSmsFromDefinedValueId );
            }
        }

        /// <summary>
        /// Updates the IsRead property of SMS Responses sent from the provided person to the SMSPhone number stored in SmsFromDefinedValue.
        /// </summary>
        /// <param name="fromPersonId">From person identifier.</param>
        /// <param name="relatedSmsFromDefinedValueId">The defined value ID of the from SMS phone number.</param>
        public void UpdateReadPropertyByFromPersonId( int fromPersonId, int relatedSmsFromDefinedValueId )
        {
            var personAliasIdQuery = new PersonAliasService( this.Context as RockContext ).Queryable().Where( a => a.PersonId == fromPersonId ).Select( a => a.Id );
            var communicationResponsesToUpdate = Queryable().Where( a => a.FromPersonAliasId.HasValue && personAliasIdQuery.Contains( a.FromPersonAliasId.Value ) && a.RelatedSmsFromDefinedValueId == relatedSmsFromDefinedValueId && a.IsRead == false );

            this.Context.BulkUpdate( communicationResponsesToUpdate, a => new CommunicationResponse { IsRead = true } );
        }

        #region Obsolete

        /// <summary>
        /// Gets the SMS conversation history for a person alias ID. Includes the communication sent by Rock that the person may be responding to.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use GetCommunicationConversation instead", true )]
        public DataSet GetConversation( int personAliasId, int relatedSmsFromDefinedValueId )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "@personAliasId", personAliasId );
            sqlParams.Add( "@releatedSmsFromDefinedValueId", relatedSmsFromDefinedValueId );
            sqlParams.Add( "@smsMediumEntityTypeId", EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Id );

            string sql = @"
                SELECT 
	                  COALESCE(cr.[FromPersonAliasId], -1) AS FromPersonAliasId
	                , cr.[MessageKey]
	                , COALESCE( p.[NickName], p.[FirstName] ) + ' ' + p.LastName AS FullName
	                , cr.[CreatedDateTime]
	                , cr.[Response] AS SMSMessage
	                , cr.[IsRead]
                FROM [CommunicationResponse] cr
                LEFT JOIN [PersonAlias] pa ON cr.[FromPersonAliasId] = pa.[Id]
                LEFT JOIN [Person] p ON pa.[PersonId] = p.[Id]
                WHERE cr.[RelatedSmsFromDefinedValueId] = @releatedSmsFromDefinedValueId
                    AND cr.[RelatedMediumEntityTypeId] = @smsMediumEntityTypeId
	                AND cr.[FromPersonAliasId] = @personAliasId
                UNION
                SELECT 
	                  c.[SenderPersonAliasId] AS FromPersonAliasId
	                , '' AS MessageKey
	                , COALESCE(p.[NickName], p.[FirstName]) + ' ' + p.LastName AS FullName
	                , c.[CreatedDateTime]
	                , COALESCE( cr.[SentMessage], c.[SMSMessage])
	                , CONVERT(bit, 1) -- Communications from Rock are always considered read
                FROM [Communication] c
                JOIN [PersonAlias] pa ON c.[SenderPersonAliasId] = pa.[Id]
                JOIN [Person] p ON pa.[PersonId] = p.[Id]
                JOIN [CommunicationRecipient] cr ON cr.[PersonAliasId] = @personAliasId AND cr.CommunicationId = c.Id
                WHERE c.[SMSFromDefinedValueId] = @releatedSmsFromDefinedValueId
                ORDER BY CreatedDateTime ASC";

            var set = Rock.Data.DbService.GetDataSet( sql, CommandType.Text, sqlParams );

            return set;
        }

        /// <summary>
        /// Gets the conversation for a message key. Use this if a person was not able to be determined. This will only
        /// show incoming messages as outgoing messages require a person object with an SMS enabled number.
        /// </summary>
        /// <param name="messageKey">The message key.</param>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use GetCommunicationConversation instead", true )]
        public DataSet GetConversation( string messageKey, int relatedSmsFromDefinedValueId )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "@messageKey", messageKey );
            sqlParams.Add( "@releatedSmsFromDefinedValueId", relatedSmsFromDefinedValueId );
            sqlParams.Add( "@smsMediumEntityTypeId", EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Id );

            string sql = @"
                SELECT 
	                  COALESCE(cr.[FromPersonAliasId], -1) AS FromPersonAliasId
	                , cr.[MessageKey]
	                , COALESCE( p.[NickName], p.[FirstName] ) + ' ' + p.LastName AS FullName
	                , cr.[CreatedDateTime]
	                , cr.[Response] AS SMSMessage
	                , cr.[IsRead]
                FROM [CommunicationResponse] cr
                LEFT JOIN [PersonAlias] pa ON cr.[FromPersonAliasId] = pa.[Id]
                LEFT JOIN [Person] p ON pa.[PersonId] = p.[Id]
                WHERE cr.[RelatedSmsFromDefinedValueId] = @releatedSmsFromDefinedValueId
                    AND cr.[RelatedMediumEntityTypeId] = @smsMediumEntityTypeId
	                AND cr.[MessageKey] = @messageKey
                ORDER BY CreatedDateTime ASC";

            var set = Rock.Data.DbService.GetDataSet( sql, CommandType.Text, sqlParams );

            return set;
        }

        /// <summary>
        /// Updates the IsRead property of SMS Responses sent from the provided MessageKey to the SMSPhone number stored in SmsFromDefinedValue.
        /// The MessageKey is the transport address of the sender, e.g. an SMS enabled phone number or email address.
        /// </summary>
        /// <param name="messageKey">The message key.</param>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "No longer needed (use UpdateReadPropertyByFromPersonAliasId)", true )]
        public void UpdateReadPropertyByMessageKey( string messageKey, int relatedSmsFromDefinedValueId )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "@messageKey", messageKey );
            sqlParams.Add( "@releatedSmsFromDefinedValueId", relatedSmsFromDefinedValueId );
            sqlParams.Add( "@smsMediumEntityTypeId", EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Id );

            string sql = @"
                UPDATE [CommunicationResponse]
                SET IsRead = 1
                WHERE [MessageKey] = @messageKey
                    AND [RelatedSmsFromDefinedValueId] = @releatedSmsFromDefinedValueId
                    AND [RelatedMediumEntityTypeId] = @smsMediumEntityTypeId
                    AND [IsRead] = 0";

            Rock.Data.DbService.ExecuteCommand( sql, CommandType.Text, sqlParams );
        }

        /// <summary>
        /// Updates the person alias by message key.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="messageKey">The message key.</param>
        /// <param name="personAliasType">Type of the person alias.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "No longer needed (SMS Pipeline and RockCleanup will take care of this)", true )]
        public void UpdatePersonAliasByMessageKey( int personAliasId, string messageKey, PersonAliasType personAliasType )
        {
            string sql = string.Empty;

            switch ( personAliasType )
            {
                case PersonAliasType.FromPersonAlias:
                    UpdateFromPersonAliasByMessageKey( personAliasId, messageKey );
                    break;
                case PersonAliasType.ToPersonAlias:
                    UpdateToPersonAliasByMessageKey( personAliasId, messageKey );
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Updates to person alias by message key.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="messageKey">The message key.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use UpdateReadPropertyByFromPersonAliasId instead", true )]
        private void UpdateToPersonAliasByMessageKey( int personAliasId, string messageKey )
        {
            var communicationResponsesToUpdate = Queryable().Where( a => a.MessageKey == messageKey );

            ( this.Context as RockContext ).BulkUpdate( communicationResponsesToUpdate, a => new CommunicationResponse { ToPersonAliasId = personAliasId } );
        }

        /// <summary>
        /// Updates from person alias by message key.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="messageKey">The message key.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "No longer needed (SMS Pipeline and RockCleanup will take care of this)", true )]
        private void UpdateFromPersonAliasByMessageKey( int personAliasId, string messageKey )
        {
            var communicationResponsesToUpdate = Queryable().Where( a => a.MessageKey == messageKey );

            ( this.Context as RockContext ).BulkUpdate( communicationResponsesToUpdate, a => new CommunicationResponse { FromPersonAliasId = personAliasId } );
        }

        /// <summary>
        /// Gets the communications and response recipients.
        /// </summary>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <param name="conversationAgeInMonths">The conversation age in months.</param>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use GetCommunicationResponseRecipients instead", true )]
        public DataSet GetCommunicationsAndResponseRecipients( int relatedSmsFromDefinedValueId, int conversationAgeInMonths = 0 )
        {
            var sqlParams = new Dictionary<string, object>
            {
                { "@releatedSmsFromDefinedValueId", relatedSmsFromDefinedValueId },
                { "@smsMediumEntityTypeId", EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Id }
            };

            string filterDate = conversationAgeInMonths == 0 ? string.Empty : $" AND cr.[CreatedDateTime] > DATEADD(month, -{conversationAgeInMonths}, GETDATE())";

            string sql = $@"
                ;WITH cte AS (
                    SELECT 
	                      COALESCE(cr.[FromPersonAliasId], -1) AS FromPersonAliasId
	                    , cr.[MessageKey]
	                    , COALESCE( p.[NickName], p.[FirstName] ) + ' ' + p.LastName AS FullName
	                    , cr.[CreatedDateTime]
	                    , cr.[Response] AS SMSMessage
	                    , cr.[IsRead]
                    FROM [CommunicationResponse] cr
                    LEFT JOIN [PersonAlias] pa ON cr.[FromPersonAliasId] = pa.[Id]
                    LEFT JOIN [Person] p ON pa.[PersonId] = p.[Id]
                    WHERE cr.[RelatedSmsFromDefinedValueId] = @releatedSmsFromDefinedValueId
                        AND cr.[RelatedMediumEntityTypeId] = @smsMediumEntityTypeId
                        {filterDate}
                    ORDER BY [CreatedDateTime] OFFSET 0 ROWS
                    UNION
                    SELECT 
	                       cr.[PersonAliasId] AS FromPersonAliasId
	                    , COALESCE( pn.[CountryCode], '1' ) + pn.[Number] AS MessageKey
	                    , COALESCE(p.[NickName], p.[FirstName]) + ' ' + p.LastName AS FullName
	                    , c.[CreatedDateTime]
	                    , cr.[SentMessage] 
	                    , CONVERT(bit, 1) -- Communications from Rock are always considered read
                    FROM [Communication] c
                    JOIN [CommunicationRecipient] cr ON c.[Id] = cr.[CommunicationId]
                    JOIN [PersonAlias] pa ON cr.[PersonAliasId] = pa.[Id]
                    JOIN [Person] p ON pa.[PersonId] = p.[Id]
                    JOIN [PhoneNumber] pn on pn.PersonId = p.Id
                    WHERE c.[SMSFromDefinedValueId] = @releatedSmsFromDefinedValueId
	                    AND pn.IsMessagingEnabled = 1
                        {filterDate}
                    ORDER BY [CreatedDateTime] OFFSET 0 ROWS
                    )

                    -- Lets do our grouping here since we are returning a dataset.
                    SELECT *
                    FROM cte cte1
                    WHERE CreatedDateTime = (
	                    SELECT MAX(cte2.CreatedDateTime) 
	                    FROM cte cte2 
	                    WHERE cte2.[MessageKey] = cte1.[MessageKey])
                    ORDER BY CreatedDateTime DESC";

            return Rock.Data.DbService.GetDataSet( sql, CommandType.Text, sqlParams );
        }

        /// <summary>
        /// Gets the response recipients.
        /// </summary>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <param name="showReadMessages">if set to <c>true</c> [show read messages].</param>
        /// <param name="conversationAgeInMonths">The conversation age in months.</param>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "Use GetCommunicationResponseRecipients instead", true )]
        public DataSet GetResponseRecipients( int relatedSmsFromDefinedValueId, bool showReadMessages, int conversationAgeInMonths = 0 )
        {
            var sqlParams = new Dictionary<string, object>
            {
                { "@releatedSmsFromDefinedValueId", relatedSmsFromDefinedValueId },
                { "@smsMediumEntityTypeId", EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Id }
            };
            string showRead = !showReadMessages ? " AND cr.[IsRead] = 0 " : string.Empty;
            string filterDate = conversationAgeInMonths == 0 ? string.Empty : $" AND cr.[CreatedDateTime] > DATEADD(month, -{conversationAgeInMonths}, GETDATE())";

            string sql = $@"
                ;WITH cte AS (
                    SELECT 
	                      COALESCE(cr.[FromPersonAliasId], -1) AS FromPersonAliasId
	                    , cr.[MessageKey]
	                    , COALESCE( p.[NickName], p.[FirstName] ) + ' ' + p.LastName AS FullName
	                    , cr.[CreatedDateTime]
	                    , cr.[Response] AS SMSMessage
	                    , cr.[IsRead]
                    FROM [CommunicationResponse] cr
                    LEFT JOIN [PersonAlias] pa ON cr.[FromPersonAliasId] = pa.[Id]
                    LEFT JOIN [Person] p ON pa.[PersonId] = p.[Id]
                    WHERE cr.[RelatedSmsFromDefinedValueId] = @releatedSmsFromDefinedValueId
                        AND cr.[RelatedMediumEntityTypeId] = @smsMediumEntityTypeId
                        {filterDate}
                        {showRead}
                    )

                    -- Lets do our grouping here since we are returning a dataset.
                    SELECT *
                    FROM cte cte1
                    WHERE CreatedDateTime = (
	                    SELECT MAX(cte2.CreatedDateTime) 
	                    FROM cte cte2 
	                    WHERE cte2.[MessageKey] = cte1.[MessageKey])
                    ORDER BY CreatedDateTime DESC";

            return Rock.Data.DbService.GetDataSet( sql, CommandType.Text, sqlParams );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [RockObsolete( "1.10" )]
    [Obsolete( "No longer needed", true )]
    public enum PersonAliasType
    {
        /// <summary>
        /// From person alias
        /// </summary>
        FromPersonAlias = 0,

        /// <summary>
        /// To person alias
        /// </summary>
        ToPersonAlias = 1
    }

    #endregion Obsolete
}
