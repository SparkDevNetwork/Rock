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
using System.Data;
using System.Linq;

using Rock.Web.Cache;

namespace Rock.Model
{
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
        /// Gets the SMS conversation history for a person alias ID. Inclues the communication sent by Rock that the person may be responding to.
        /// </summary>
        /// <param name="personAliasId">The person alias identifier.</param>
        /// <param name="relatedSmsFromDefinedValueId">The releated SMS from defined value identifier.</param>
        /// <returns></returns>
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
        /// Updates the IsRead property of SMS Responses sent from the provided person to the SMSPhone number stored in SmsFromDefinedValue.
        /// </summary>
        /// <param name="fromPersonAliasId">From person alias identifier.</param>
        /// <param name="relatedSmsFromDefinedValueId">The defined value ID of the from SMS phone number.</param>
        public void UpdateReadPropertyByFromPersonAliasId( int fromPersonAliasId, int relatedSmsFromDefinedValueId )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "@fromPersonAliasId", fromPersonAliasId );
            sqlParams.Add( "@releatedSmsFromDefinedValueId", relatedSmsFromDefinedValueId );
            sqlParams.Add( "@smsMediumEntityTypeId", EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS ).Id );

            string sql = @"
                UPDATE [CommunicationResponse]
                SET IsRead = 1
                WHERE [FromPersonAliasId] = @fromPersonAliasId
                    AND [RelatedSmsFromDefinedValueId] = @releatedSmsFromDefinedValueId
                    AND [RelatedMediumEntityTypeId] = @smsMediumEntityTypeId
                    AND [IsRead] = 0";

            Rock.Data.DbService.ExecuteCommand( sql, CommandType.Text, sqlParams );
        }

        /// <summary>
        /// Updates the IsRead property of SMS Responses sent from the provided MessageKey to the SMSPhone number stored in SmsFromDefinedValue.
        /// The MessageKey is the transport address of the sender, e.g. an SMS enabled phone number or email address.
        /// </summary>
        /// <param name="messageKey">The message key.</param>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
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

        private void UpdateToPersonAliasByMessageKey( int personAliasId, string messageKey )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "@personAliasId", personAliasId );
            sqlParams.Add( "@messageKey", messageKey );

            string sql = @"
                UPDATE [CommunicationResponse]
                SET [ToPersonAliasId] = @personAliasId
                WHERE [MessageKey] = @messageKey";

            Rock.Data.DbService.ExecuteCommand( sql, CommandType.Text, sqlParams );
        }

        private void UpdateFromPersonAliasByMessageKey( int personAliasId, string messageKey )
        {
            var sqlParams = new Dictionary<string, object>();
            sqlParams.Add( "personAliasId", personAliasId );
            sqlParams.Add( "messageKey", messageKey );

            string sql = @"
                UPDATE [CommunicationResponse]
                SET [FromPersonAliasId] = @personAliasId
                WHERE [MessageKey] = @messageKey";
            Rock.Data.DbService.ExecuteCommand( sql, CommandType.Text, sqlParams );
        }

        /// <summary>
        /// Gets the communications and response recipients.
        /// </summary>
        /// <param name="relatedSmsFromDefinedValueId">The related SMS from defined value identifier.</param>
        /// <param name="conversationAgeInMonths">The conversation age in months.</param>
        /// <returns></returns>
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
}
