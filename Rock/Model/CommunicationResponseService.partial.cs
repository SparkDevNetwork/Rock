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
    /// POCO to store the common fields of <see cref="CommunicationRecipient"/> and <see cref="CommunicationResponse"/>
    /// </summary>
    public class CommunicationRecipientResponse
    {
        public int? RecipientPersonAliasId { get; set; }

        public string MessageKey { get; set; }

        public string FullName { get; set; }

        public DateTime? CreatedDateTime { get; set; }

        public string HumanizedCreatedDateTime { get; set; }

        public string SMSMessage { get; set; }

        public bool IsRead { get; set; }
        public int PersonId { get; internal set; }
    }

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

        public List<CommunicationRecipientResponse> GetCommunicationResponseRecipients( int relatedSmsFromDefinedValueId, DateTime startDateTime, bool showReadMessages, int maxCount )
        {
            var smsMediumEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Value;

            IQueryable<CommunicationResponse> communicationResponseQuery = this.Queryable()
                .Where( r => r.RelatedMediumEntityTypeId == smsMediumEntityTypeId && r.RelatedSmsFromDefinedValueId == relatedSmsFromDefinedValueId && r.CreatedDateTime >= startDateTime );

            if ( !showReadMessages )
            {
                communicationResponseQuery = communicationResponseQuery.Where( r => r.IsRead == false );
            }

            IQueryable<CommunicationResponse> mostRecentCommunicationResponseQuery = communicationResponseQuery
                .GroupBy( r => r.FromPersonAlias.PersonId )
                .Select( a => a.OrderByDescending( x => x.CreatedDateTime ).FirstOrDefault() )
                .OrderByDescending( a => a.CreatedDateTime );

            IQueryable<CommunicationRecipient> communicationRecipientQuery = new CommunicationRecipientService( this.Context as RockContext ).Queryable()
                .Where( r => r.MediumEntityTypeId == smsMediumEntityTypeId && r.Communication.SMSFromDefinedValueId == relatedSmsFromDefinedValueId && r.CreatedDateTime >= startDateTime );

            IQueryable<CommunicationRecipient> mostRecentCommunicationRecipientQuery = communicationRecipientQuery
                .GroupBy( r => r.PersonAlias.PersonId )
                .Select( a => a.OrderByDescending( x => x.CreatedDateTime ).FirstOrDefault() )
                .OrderByDescending( a => a.CreatedDateTime );

            var mostRecentCommunicationResponseList = mostRecentCommunicationResponseQuery.AsNoTracking().Take( maxCount ).ToList();

            List<CommunicationRecipientResponse> communicationRecipientResponseList = new List<CommunicationRecipientResponse>();

            foreach (var mostRecentCommunicationResponse in mostRecentCommunicationResponseList )
            {
                var communicationRecipientResponse = new CommunicationRecipientResponse
                {
                    CreatedDateTime = mostRecentCommunicationResponse.CreatedDateTime,
                    PersonId = mostRecentCommunicationResponse.FromPersonAlias.PersonId,
                    FullName = mostRecentCommunicationResponse.FromPersonAlias.Person.FullName,
                    IsRead = mostRecentCommunicationResponse.IsRead,
                    MessageKey = mostRecentCommunicationResponse.MessageKey,
                    RecipientPersonAliasId = mostRecentCommunicationResponse.FromPersonAliasId,
                    SMSMessage = mostRecentCommunicationResponse.Response
                };

                communicationRecipientResponseList.Add( communicationRecipientResponse );
            }

            var mostRecentCommunicationRecipientList = mostRecentCommunicationRecipientQuery.Take( maxCount).Select( a => new
            {
                a.CreatedDateTime,
                a.PersonAlias.Person,
                a.PersonAliasId,
                a.SentMessage
            } ).ToList();

            foreach ( var mostRecentCommunicationRecipient in mostRecentCommunicationRecipientList )
            {
                var communicationRecipientResponse = new CommunicationRecipientResponse
                {
                    CreatedDateTime = mostRecentCommunicationRecipient.CreatedDateTime,
                    PersonId = mostRecentCommunicationRecipient.Person.Id,
                    FullName = mostRecentCommunicationRecipient.Person.FullName,
                    IsRead = true,
                    MessageKey =  "Is this needed?", // mostRecentCommunicationRecipient?.Person.PhoneNumbers.FirstOrDefault(a => a.IsMessagingEnabled)?.Number,
                    RecipientPersonAliasId = mostRecentCommunicationRecipient.PersonAliasId,
                    SMSMessage = mostRecentCommunicationRecipient.SentMessage
                };

                communicationRecipientResponseList.Add( communicationRecipientResponse );
            }

            communicationRecipientResponseList = communicationRecipientResponseList
                .GroupBy( r => r.PersonId )
                .Select( a => a.OrderByDescending( x => x.CreatedDateTime ).FirstOrDefault() )
                .OrderByDescending( a => a.CreatedDateTime ).Take( maxCount). ToList();


            return communicationRecipientResponseList;
        }

        public List<CommunicationRecipientResponse> GetCommunicationConversation( int personAliasId, int relatedSmsFromDefinedValueId )
        {
            List<CommunicationRecipientResponse> communicationRecipientResponseList = new List<CommunicationRecipientResponse>();


            var smsMediumEntityTypeId = EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Value;

            IQueryable<CommunicationResponse> communicationResponseQuery = this.Queryable()
                .Where( r => r.RelatedMediumEntityTypeId == smsMediumEntityTypeId && r.RelatedSmsFromDefinedValueId == relatedSmsFromDefinedValueId && r.FromPersonAliasId == personAliasId );


            foreach ( var communicationResponse in communicationResponseQuery )
            {
                var communicationRecipientResponse = new CommunicationRecipientResponse
                {
                    CreatedDateTime = communicationResponse.CreatedDateTime,
                    PersonId = communicationResponse.FromPersonAlias.PersonId,
                    FullName = communicationResponse.FromPersonAlias.Person.FullName,
                    IsRead = communicationResponse.IsRead,
                    MessageKey = communicationResponse.MessageKey,
                    RecipientPersonAliasId = communicationResponse.FromPersonAliasId,
                    SMSMessage = communicationResponse.Response
                };

                communicationRecipientResponseList.Add( communicationRecipientResponse );
            }


            IQueryable<CommunicationRecipient> communicationRecipientQuery = new CommunicationRecipientService( this.Context as RockContext ).Queryable()
                .Where( r => r.MediumEntityTypeId == smsMediumEntityTypeId && r.Communication.SMSFromDefinedValueId == relatedSmsFromDefinedValueId && r.PersonAliasId == personAliasId );

            var communicationRecipientList = communicationRecipientQuery.Include(a => a.PersonAlias.Person.PhoneNumbers ).Select( a => new
            {
                a.CreatedDateTime,
                a.PersonAlias.Person,
                a.PersonAliasId,
                a.SentMessage
            } ).ToList();

            foreach ( var communicationRecipient in communicationRecipientList )
            {
                var communicationRecipientResponse = new CommunicationRecipientResponse
                {
                    CreatedDateTime = communicationRecipient.CreatedDateTime,
                    PersonId = communicationRecipient.Person.Id,
                    FullName = communicationRecipient.Person.FullName,
                    IsRead = true,
                    MessageKey = communicationRecipient?.Person.PhoneNumbers.FirstOrDefault(a => a.IsMessagingEnabled)?.Number,
                    RecipientPersonAliasId = communicationRecipient.PersonAliasId,
                    SMSMessage = communicationRecipient.SentMessage
                };

                communicationRecipientResponseList.Add( communicationRecipientResponse );
            }

            return communicationRecipientResponseList.OrderBy( a => a.CreatedDateTime ).ToList();
        }

        /// <summary>
        /// Updates the IsRead property of SMS Responses sent from the provided person to the SMSPhone number stored in SmsFromDefinedValue.
        /// </summary>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <param name="relatedSmsFromDefinedValueId">The defined value ID of the from SMS phone number.</param>
        public void UpdateReadPropertyByFromPersonAliasId( int fromPersonAliasId, int relatedSmsFromDefinedValueId )
        {
            var communicationResponsesToUpdate = Queryable().Where( a => a.FromPersonAliasId == fromPersonAliasId && a.RelatedSmsFromDefinedValueId == relatedSmsFromDefinedValueId && a.IsRead == false );

            this.Context.BulkUpdate( communicationResponsesToUpdate, a => new CommunicationResponse { IsRead = true } );
        }

        [Obsolete("ToDo")]
        public IQueryable<CommunicationResponse> GetCommunicationResponseConversation( int personAliasId, int relatedSmsFromDefinedValueId )
        {
            return null;
        }

        #region Obsolete

        /// <summary>
        /// Gets the SMS conversation history for a person alias ID. Inclues the communication sent by Rock that the person may be responding to.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="relatedSmsFromDefinedValueId">The releated SMS from defined value identifier.</param>
        /// <returns></returns>
        [Obsolete( "Probably No longer needed" )]
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
        [Obsolete( "Probably No longer needed" )]
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
        [Obsolete( "Probably No longer needed" )]
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
        [Obsolete( "No longer needed" )]
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

        [RockObsolete( "1.10" )]
        [Obsolete( "No longer needed" )]
        private void UpdateToPersonAliasByMessageKey( int personAliasId, string messageKey )
        {
            //var sqlParams = new Dictionary<string, object>();

            var communicationResponsesToUpdate = Queryable().Where( a => a.MessageKey == messageKey );

            ( this.Context as RockContext ).BulkUpdate( communicationResponsesToUpdate, a => new CommunicationResponse { ToPersonAliasId = personAliasId } );

            /*sqlParams.Add( "@personAliasId", personAliasId );
            sqlParams.Add( "@messageKey", messageKey );

            string sql = @"
                UPDATE [CommunicationResponse]
                SET [ToPersonAliasId] = @personAliasId
                WHERE [MessageKey] = @messageKey";

            Rock.Data.DbService.ExecuteCommand( sql, CommandType.Text, sqlParams );
            */
        }

        [RockObsolete( "1.10" )]
        [Obsolete( "No longer needed" )]
        private void UpdateFromPersonAliasByMessageKey( int personAliasId, string messageKey )
        {
            var communicationResponsesToUpdate = Queryable().Where( a => a.MessageKey == messageKey );

            ( this.Context as RockContext ).BulkUpdate( communicationResponsesToUpdate, a => new CommunicationResponse { FromPersonAliasId = personAliasId } );

            /*
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "personAliasId", personAliasId );
            sqlParams.Add( "messageKey", messageKey );

            string sql = @"
                UPDATE [CommunicationResponse]
                SET [FromPersonAliasId] = @personAliasId
                WHERE [MessageKey] = @messageKey";
            Rock.Data.DbService.ExecuteCommand( sql, CommandType.Text, sqlParams );
            */
        }




        /// <summary>
        /// Gets the communications and response recipients.
        /// </summary>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <param name="conversationAgeInMonths">The conversation age in months.</param>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "No longer needed" )]
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
        [Obsolete( "Probably No longer needed" )]
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
    [Obsolete( "No longer needed" )]
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
