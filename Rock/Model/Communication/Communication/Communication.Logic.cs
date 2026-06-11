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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Rock.Communication;
using Rock.Data;
using Rock.Observability;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Communication
    {
        #region Properties

        /// <summary>
        /// Gets or sets a list of email binary file ids
        /// </summary>
        /// <value>
        /// The attachment binary file ids
        /// </value>
        [NotMapped]
        public virtual IEnumerable<int> EmailAttachmentBinaryFileIds
        {
            get
            {
                return this.Attachments.Where( a => a.CommunicationType == CommunicationType.Email ).Select( a => a.BinaryFileId ).ToList();
            }
        }

        /// <summary>
        /// Gets or sets a list of sms binary file ids
        /// </summary>
        /// <value>
        /// The attachment binary file ids
        /// </value>
        [NotMapped]
        public virtual IEnumerable<int> SMSAttachmentBinaryFileIds
        {
            get
            {
                return this.Attachments.Where( a => a.CommunicationType == CommunicationType.SMS ).Select( a => a.BinaryFileId ).ToList();
            }
        }

        #endregion Properties

        #region ISecured

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.CommunicationTemplate != null )
                {
                    return this.CommunicationTemplate;
                }

                if ( this.SystemCommunication != null )
                {
                    return this.SystemCommunication;
                }

                return base.ParentAuthority;
            }
        }

        #endregion ISecured

        #region Methods

        /// <summary>
        /// Gets the <see cref="Rock.Communication.MediumComponent" /> for the communication medium that is being used.
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// The <see cref="Rock.Communication.MediumComponent" /> for the communication medium that is being used.
        /// </value>
        public virtual List<MediumComponent> GetMediums()
        {
            var mediums = new List<MediumComponent>();

            foreach ( var serviceEntry in MediumContainer.Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                if ( component.IsActive &&
                    ( this.CommunicationType == component.CommunicationType ||
                        this.CommunicationType == CommunicationType.RecipientPreference ) )
                {
                    mediums.Add( component );
                }
            }

            return mediums;
        }

        /// <summary>
        /// Adds the attachment.
        /// </summary>
        /// <param name="communicationAttachment">The communication attachment.</param>
        /// <param name="communicationType">Type of the communication.</param>
        public void AddAttachment( CommunicationAttachment communicationAttachment, CommunicationType communicationType )
        {
            communicationAttachment.CommunicationType = communicationType;
            this.Attachments.Add( communicationAttachment );
        }

        /// <summary>
        /// Gets the attachments.
        /// Specify CommunicationType.Email to get the attachments for Email and CommunicationType.SMS to get the Attachment(s) for SMS
        /// </summary>
        /// <param name="communicationType">Type of the communication.</param>
        /// <returns></returns>
        public IEnumerable<CommunicationAttachment> GetAttachments( CommunicationType communicationType )
        {
            return this.Attachments.Where( a => a.CommunicationType == communicationType );
        }

        /// <summary>
        /// Gets the attachment <see cref="Rock.Model.BinaryFile" /> ids.
        /// Specify CommunicationType.Email to get the attachments for Email and CommunicationType.SMS to get the Attachment(s) for SMS
        /// </summary>
        /// <param name="communicationType">Type of the communication.</param>
        /// <returns></returns>
        public List<int> GetAttachmentBinaryFileIds( CommunicationType communicationType )
        {
            return this.GetAttachments( communicationType ).Select( a => a.BinaryFileId ).ToList();
        }

        /// <summary>
        /// Returns true if this communication has any pending recipients
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public bool HasPendingRecipients( RockContext rockContext )
        {
            return GetRecipientsQry( rockContext ).Where( a => a.Status == CommunicationRecipientStatus.Pending ).Any();
        }

        /// <summary>
        /// Updates CommunicationRecipients who are stuck in the "Sending" status, setting the status to failed if they have been there for 2 days or more, and setting the status back to Pending otherwise.
        /// </summary>
        public void UpdateSendingRecipients()
        {
            var expirationDate = RockDateTime.Now.AddDays( -2 );
            using ( var rockContext = new RockContext() )
            {
                // If any recipients have been in "Sending" status (or reset to "Pending" status from "Sending") for 2 days, set the status to failed, instead.
                var expiredSendingRecipients = GetRecipientsQry( rockContext ).Where( a => ( a.Status == CommunicationRecipientStatus.Sending || a.Status == CommunicationRecipientStatus.Pending ) && a.FirstSendAttemptDateTime <= expirationDate ).ToList();
                foreach ( var expiredSendingRecipient in expiredSendingRecipients )
                {
                    expiredSendingRecipient.Status = CommunicationRecipientStatus.Failed;
                    expiredSendingRecipient.StatusNote = "Recipient locked in Sending status.";
                }

                // Any recipients stuck in "Sending" for less than two days get set back to "Pending".
                var sendingRecipients = GetRecipientsQry( rockContext ).Where( a => a.Status == CommunicationRecipientStatus.Sending && ( !a.FirstSendAttemptDateTime.HasValue || a.FirstSendAttemptDateTime > expirationDate ) ).ToList();
                foreach ( var sendingRecipient in sendingRecipients )
                {
                    sendingRecipient.Status = CommunicationRecipientStatus.Pending;
                    sendingRecipient.StatusNote = "Recipient reverted to Pending status after initial attempt.";

                    // This should already be set when the recipient was set to "Sending", but let's be certain the clock has started.
                    sendingRecipient.FirstSendAttemptDateTime = sendingRecipient.FirstSendAttemptDateTime ?? RockDateTime.Now;
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Returns a queryable of the Recipients for this communication. Note that this will return the recipients that have been saved to the database. Any pending changes in the Recipients property are not included.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IQueryable<CommunicationRecipient> GetRecipientsQry( RockContext rockContext )
        {
            return new CommunicationRecipientService( rockContext ).Queryable().Where( a => a.CommunicationId == this.Id );
        }

        /// <summary>
        /// Gets the communication list members.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="listGroupId">The list group identifier.</param>
        /// <param name="segmentCriteria">The segment criteria.</param>
        /// <param name="segmentDataViewIds">The segment data view ids.</param>
        /// <returns></returns>
        public static IQueryable<GroupMember> GetCommunicationListMembers( RockContext rockContext, int? listGroupId, SegmentCriteria segmentCriteria, List<int> segmentDataViewIds )
        {
            if ( listGroupId.HasValue )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var groupMemberQuery = groupMemberService.Queryable()
                    .Where( a => a.GroupId == listGroupId.Value && a.GroupMemberStatus == GroupMemberStatus.Active );

                var dataViewService = new DataViewService( rockContext );
                var segmentDataViews = dataViewService.GetByIds( segmentDataViewIds ).AsNoTracking();

                return GetCommunicationListMembersInternal( rockContext, groupMemberQuery, segmentCriteria, segmentDataViews );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the communication list members.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="listGroupGuid">The group unique identifier.</param>
        /// <param name="segmentCriteria">The segment criteria.</param>
        /// <param name="segmentDataViewGuids">The segment data view unique identifiers.</param>
        /// <returns></returns>
        public static IQueryable<GroupMember> GetCommunicationListMembers( RockContext rockContext, Guid? listGroupGuid, SegmentCriteria segmentCriteria, List<Guid> segmentDataViewGuids )
        {
            if ( listGroupGuid.HasValue )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var groupMemberQuery = groupMemberService.Queryable()
                    .Where( a => a.Group.Guid == listGroupGuid.Value && a.GroupMemberStatus == GroupMemberStatus.Active );

                var dataViewService = new DataViewService( rockContext );
                var segmentDataViews = dataViewService.GetByGuids( segmentDataViewGuids ).AsNoTracking();

                return GetCommunicationListMembersInternal( rockContext, groupMemberQuery, segmentCriteria, segmentDataViews );
            }
            else
            {
                return Enumerable.Empty<GroupMember>().AsQueryable();
            }
        }

        /// <summary>
        /// Gets the communication list members.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupMemberQuery">The group member query.</param>
        /// <param name="segmentCriteria">The segment criteria.</param>
        /// <param name="segmentDataViews">The segment data views.</param>
        /// <returns></returns>
        private static IQueryable<GroupMember> GetCommunicationListMembersInternal( RockContext rockContext, IQueryable<GroupMember> groupMemberQuery, SegmentCriteria segmentCriteria, IQueryable<DataView> segmentDataViews )
        {
            var personService = new PersonService( rockContext );

            Expression segmentExpression = null;
            ParameterExpression paramExpression = personService.ParameterExpression;
            foreach ( var segmentDataView in segmentDataViews )
            {
                var exp = segmentDataView.GetExpression( personService, paramExpression );
                if ( exp != null )
                {
                    if ( segmentExpression == null )
                    {
                        segmentExpression = exp;
                    }
                    else
                    {
                        if ( segmentCriteria == SegmentCriteria.All )
                        {
                            segmentExpression = Expression.AndAlso( segmentExpression, exp );
                        }
                        else
                        {
                            segmentExpression = Expression.OrElse( segmentExpression, exp );
                        }
                    }
                }
            }

            if ( segmentExpression != null )
            {
                var personQry = personService.Get( paramExpression, segmentExpression );
                groupMemberQuery = groupMemberQuery.Join( personQry, g => g.PersonId, p => p.Id, ( g, p ) => g );
            }

            return groupMemberQuery;
        }

        /// <summary>
        /// If <see cref="ExcludeDuplicateRecipientAddress" /> is set to true, removes <see cref="CommunicationRecipient"></see>s
        /// that have the same SMS/Email address as another recipient.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        public void RemoveRecipientsWithDuplicateAddress( RockContext rockContext )
        {
            /*
                10/10/2025 - JPH

                This method has undergone several refactors over time to improve performance; the previous version
                (along with comments explaining the version before that) can be seen here:
                https://github.com/SparkDevNetwork/Rock/blob/383dd5f675aad1a6a8e8df999e433b081860dc31/Rock/Model/Communication/Communication/Communication.Logic.cs#L315

                Reason: Refactor method to improve performance while preserving explanations of previous changes.
             */

            if ( !ExcludeDuplicateRecipientAddress )
            {
                return;
            }

            if ( CommunicationType == CommunicationType.SMS || CommunicationType == CommunicationType.RecipientPreference )
            {
                using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Prepare Recipient List > Remove Recipients With Duplicate SMS Phone Numbers" ) )
                {
                    var smsMediumEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ) ?? 0;

                    var sql = @"
;WITH Recipients AS (
    SELECT
        cr.[Id] AS [CommunicationRecipientId]
        , ROW_NUMBER() OVER (
            PARTITION BY pn.[Number]
            ORDER BY cr.[Id]
        ) AS [RowNumber]
    FROM [CommunicationRecipient] cr
    INNER JOIN [PersonAlias] pa
        ON pa.[Id] = cr.[PersonAliasId]
    INNER JOIN [PhoneNumber] pn
        ON pn.[PersonId] = pa.[PersonId]
    WHERE cr.[CommunicationId] = @CommunicationId
        AND cr.[MediumEntityTypeId] = @SmsMediumEntityTypeId
        AND pn.[IsMessagingEnabled] = 1
        AND pn.[IsMessagingOptedOut] = 0
)
DELETE cr
FROM [CommunicationRecipient] cr
INNER JOIN [Recipients] r
    ON r.[CommunicationRecipientId] = cr.[Id]
WHERE r.[RowNumber] > 1;";

                    var rowsDeletedCount = rockContext.Database.ExecuteSqlCommand(
                        sql,
                        new SqlParameter( "@CommunicationId", Id ),
                        new SqlParameter( "@SmsMediumEntityTypeId", smsMediumEntityTypeId )
                    );

                    activity?.AddTag( "rock.communication.recipients_removed_with_duplicate_sms", rowsDeletedCount );
                }
            }

            if ( CommunicationType == CommunicationType.Email || CommunicationType == CommunicationType.RecipientPreference )
            {
                using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Prepare Recipient List > Remove Recipients With Duplicate Email Addresses" ) )
                {
                    var emailMediumEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ) ?? 0;

                    var sql = @"
;WITH Recipients AS (
    SELECT
        cr.[Id] AS [CommunicationRecipientId]
        , ROW_NUMBER() OVER (
            PARTITION BY p.[Email]
            ORDER BY cr.[Id]
        ) AS [RowNumber]
    FROM [CommunicationRecipient] cr
    INNER JOIN [PersonAlias] pa
        ON pa.[Id] = cr.[PersonAliasId]
    INNER JOIN [Person] p
        ON p.[Id] = pa.[PersonId]
    WHERE cr.[CommunicationId] = @CommunicationId
        AND cr.[MediumEntityTypeId] = @EmailMediumEntityTypeId
        AND p.[Email] IS NOT NULL
        AND p.[Email] <> ''
        AND p.[IsEmailActive] = 1
)
DELETE cr
FROM [CommunicationRecipient] cr
INNER JOIN [Recipients] r
    ON r.[CommunicationRecipientId] = cr.[Id]
WHERE r.[RowNumber] > 1;";

                    var rowsDeletedCount = rockContext.Database.ExecuteSqlCommand(
                        sql,
                        new SqlParameter( "@CommunicationId", Id ),
                        new SqlParameter( "@EmailMediumEntityTypeId", emailMediumEntityTypeId )
                    );

                    activity?.AddTag( "rock.communication.recipients_removed_with_duplicate_email", rowsDeletedCount );
                }
            }
        }

        /// <summary>
        /// Removes duplicate person recipients, when a given <see cref="Person"/> is represented within the list of
        /// <see cref="CommunicationRecipient"/>s more than once.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <remarks>
        /// The first <see cref="CommunicationRecipient"/> that was added for a given <see cref="Person"/> is the one
        /// that will be preserved. This means that the corresponding <see cref="PersonAlias"/> that remains might not
        /// be the person's current "primary" alias. Since all we need is a pointer to the person, any alias record -
        /// primary or not - will serve this purpose. It is more performant to NOT try to preserve the primary alias
        /// reference here.
        /// </remarks>
        private void RemoveDuplicatePersonRecipients( RockContext rockContext )
        {
            /*
                8/28/2025 - JPH

                This method used to be called `RemoveNonPrimaryPersonAliasRecipients()` and was related to multiple past issues:

                ----------

                1. Communications with a large number of recipients time out and don't send.
                https://github.com/SparkDevNetwork/Rock/issues/5651

                The fix for this issue involved replacing the previous EF query with a bulk delete process, and was
                ultimately solved by introducing a precision index to greatly improve the delete performance.

                EF Rework: https://github.com/SparkDevNetwork/Rock/commit/567a51652d1fe7fd09d894fa0474e152750c4d54
                New Index: https://github.com/SparkDevNetwork/Rock/commit/f3b6f435d5425ef37e27bccfe60d676a4c398af7

                ----------

                2. Merged recipients incorrectly deleted from communication record.
                https://github.com/SparkDevNetwork/Rock/issues/6255

                The fix for this issue did solve the problem of no longer completely removing recipients from a
                communication, but introduced a new SQL timeout because of the complexity of the EF-generated queries.

                EF Queries Before Fix: https://github.com/SparkDevNetwork/Rock/blob/8bd4aabd56c31d88353c65635b84fc2c7e835984/Rock/Model/Communication/Communication/Communication.Logic.cs#L593-L622
                EF Queries After Fix: https://github.com/SparkDevNetwork/Rock/blob/2a1c7d3df3fd1a597c81ac5d04ff32398d56b18a/Rock/Model/Communication/Communication/Communication.Logic.cs#L604-L635

                ----------

                3. SQL Timeout with large communication lists.
                https://github.com/SparkDevNetwork/Rock/issues/6415

                When this performance issue appeared again, we decided to abandon EF-generated queries altogether, in
                favor of inline SQL that does what the original, poorly-named `RemoveNonPrimaryPersonAliasRecipients()`
                method set out to do, as performantly as possible: delete duplicate people from a communication, when
                they have multiple recipient records corresponding to multiple person alias records. It was also
                determined that it's more performant to NOT try and preserve the "primary" alias record for a given
                person, and instead simply delete all but the first recipient record that was added for that person.
                Since a person alias is simply a pointer to a person, it ultimately doesn't matter which one we preserve
                here, so we settled on performance over attempting to preserve the primary alias.

                ----------

                Reason: Rename method to reflect the work being performed and improve performance.
            */

            var sql = @"
;WITH Recipients AS (
    SELECT
        cr.[Id] AS [CommunicationRecipientId]
        , ROW_NUMBER() OVER (
            PARTITION BY pa.[PersonId]
            ORDER BY cr.[Id]
        ) AS [RowNumber]
    FROM [CommunicationRecipient] cr
    INNER JOIN [PersonAlias] pa
        ON pa.[Id] = cr.[PersonAliasId]
    WHERE cr.[CommunicationId] = @CommunicationId
)
DELETE cr
FROM [CommunicationRecipient] cr
INNER JOIN [Recipients] r
    ON r.[CommunicationRecipientId] = cr.[Id]
WHERE r.[RowNumber] > 1;";

            using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Prepare Recipient List > Remove Duplicate Person Recipients" ) )
            {
                rockContext.Database.ExecuteSqlCommand( sql, new SqlParameter( "@CommunicationId", Id ) );
            }
        }

        /// <summary>
        /// Retrieves an <see cref="IQueryable{GroupMember}"/> of communication list members
        /// who match the specified personalization segment filters.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <param name="communicationListGroupId">The ID of the communication list (group).</param>
        /// <param name="segmentCriteria">
        /// The matching criteria:
        /// <list type="bullet">
        /// <item><description><see cref="SegmentCriteria.Any"/> - Matches members with at least one of the specified segments.</description></item>
        /// <item><description><see cref="SegmentCriteria.All"/> - Matches members with all specified segments.</description></item>
        /// </list>
        /// </param>
        /// <param name="personalizationSegmentIds">A list of personalization segment IDs to filter by.</param>
        /// <returns>
        /// An <see cref="IQueryable{GroupMember}"/> containing group members who meet the specified criteria.
        /// </returns>
        private static IQueryable<GroupMember> GetPersonalizedCommunicationListMembersQuery( RockContext rockContext, int communicationListGroupId, SegmentCriteria segmentCriteria, List<int> personalizationSegmentIds )
        {
            var groupMemberQuery = new GroupMemberService( rockContext ).Queryable();
            var personAliasQuery = new PersonAliasService( rockContext ).Queryable();
            var personAliasPersonalizationQuery = new PersonalizationSegmentService( rockContext ).GetPersonAliasPersonalizationSegmentQuery();

            return groupMemberQuery
                .Where( gm => gm.GroupId == communicationListGroupId && gm.GroupMemberStatus == GroupMemberStatus.Active && gm.Person.PrimaryAliasId.HasValue )
                .Where( gm =>
                    !personalizationSegmentIds.Any()
                    || (
                        segmentCriteria == SegmentCriteria.Any
                        && personAliasQuery.Any( pa =>
                            pa.PersonId == gm.PersonId
                            && personAliasPersonalizationQuery.Any( pap =>
                                pa.Id == pap.PersonAliasId
                                && personalizationSegmentIds.Contains( pap.PersonalizationEntityId )
                            )
                        )
                    )
                    || (
                        segmentCriteria == SegmentCriteria.All
                        && personAliasQuery.Where( pa =>
                            pa.PersonId == gm.PersonId
                        ).SelectMany( pa =>
                            personAliasPersonalizationQuery.Where( pap =>
                                pa.Id == pap.PersonAliasId
                                && personalizationSegmentIds.Contains( pap.PersonalizationEntityId )
                            )
                            .Select( pap => pap.PersonalizationEntityId )
                        )
                        .Distinct()
                        .Count() == personalizationSegmentIds.Count
                    )
                );
        }

        /// <summary>
        /// Refresh the recipients list.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public void RefreshCommunicationRecipientList( RockContext rockContext )
        {
            if ( !ListGroupId.HasValue )
            {
                return;
            }

            if ( ( rockContext.Database.CommandTimeout ?? 0 ) < 90 )
            {
                /*
                    12/1/2025 - JPH

                    We're increasing this timeout from the default of 30 seconds to give the following
                    pre-send task more time to complete.

                    Reason: Communications with a large number of recipients time out and don't send.
                    https://github.com/SparkDevNetwork/Rock/issues/5651
                */
                rockContext.Database.SetCommandTimeout( 90 );
            }

            using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Prepare Recipient List > Refresh Communication Recipient List" ) )
            {
                /*
                    10/16/2025 - JPH

                    For communications created by the Legacy Communication Entry Wizard block, we must continue supporting
                    the slower, legacy method of refreshing the recipient list, since it supports data view segments with
                    queries that are built using EF LINQ expressions. The newer, faster stored procedure-based approach
                    only supports personalization segments (or legacy communications that don't have any segments at all).

                    Reason: Improve refresh communication recipient list performance when possible.
                */

                if ( Segments.IsNotNullOrWhiteSpace() )
                {
                    RefreshCommunicationRecipientListLegacy( rockContext );
                    return;
                }

                rockContext.Database.ExecuteSqlCommand( "EXEC [dbo].[spCommunication_SynchronizeListRecipients] @CommunicationId", new SqlParameter( "@CommunicationId", Id ) );
            }
        }

        /// <summary>
        /// Refreshes the communication recipient list using the legacy method that supports segment data views.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void RefreshCommunicationRecipientListLegacy( RockContext rockContext )
        {
            IQueryable<GroupMember> qryCommunicationListMembers;

            var personalizationSegmentIds = this.PersonalizationSegments.SplitDelimitedValues().AsIntegerList();

            if ( personalizationSegmentIds.Any() )
            {
                qryCommunicationListMembers = GetPersonalizedCommunicationListMembersQuery( rockContext, this.ListGroupId.Value, this.SegmentCriteria, personalizationSegmentIds );
            }
            else
            {
                var segmentDataViewGuids = this.Segments.SplitDelimitedValues().AsGuidList();
                var segmentDataViewIds = new DataViewService( rockContext ).GetByGuids( segmentDataViewGuids ).Select( a => a.Id ).ToList();

                qryCommunicationListMembers = GetCommunicationListMembers( rockContext, ListGroupId, this.SegmentCriteria, segmentDataViewIds );
            }

            // NOTE: If this is a scheduled communication, don't include Members that were added after the scheduled FutureSendDateTime.
            // However, don't exclude if the date added can't be determined or they will never be sent a scheduled communication.
            if ( this.FutureSendDateTime.HasValue )
            {
                var memberAddedCutoffDate = this.FutureSendDateTime;

                qryCommunicationListMembers = qryCommunicationListMembers.Where( a =>
                    ( a.DateTimeAdded.HasValue && a.DateTimeAdded.Value < memberAddedCutoffDate )
                    || ( a.CreatedDateTime.HasValue && a.CreatedDateTime.Value < memberAddedCutoffDate )
                    || ( !a.DateTimeAdded.HasValue && !a.CreatedDateTime.HasValue )
                );
            }

            var recipientsQry = GetRecipientsQry( rockContext );

            using ( var bulkInsertActivity = ObservabilityHelper.StartActivity( "COMMUNICATION: Prepare Recipient List > Refresh Communication Recipient List > Bulk Insert New Members" ) )
            {
                /*
                    6/25/2024 - JPH

                    Using LINQ query syntax for the following query allows us to easily force
                    LEFT OUTER JOINs and purposefully handle NULL join scenarios. We're also
                    cherry-picking the specific entity fields we need instead of materializing
                    entire entities.

                    Reason: Communications with a large number of recipients time out and don't send.
                    https://github.com/SparkDevNetwork/Rock/issues/5651
                 */

                // Note that we're not actually getting all person alias records here.
                // We're simply creating this query to be used when joining against
                // primary aliases below.
                var personAliases = new PersonAliasService( rockContext ).Queryable();

                var listMembersToAdd =
                    (
                        // Start with all current communication list members.
                        from listMember in qryCommunicationListMembers

                            // Get list members who don't yet have a communication recipient record.
                        join recipient in recipientsQry on listMember.PersonId equals recipient.PersonAlias.PersonId into existingRecipientsLeftJoin
                        from existingRecipient in existingRecipientsLeftJoin.DefaultIfEmpty()
                        where existingRecipient == null

                        // For those list members who need recipient records to be added, get each person's primary alias.
                        join personAlias in personAliases on listMember.PersonId equals personAlias.AliasPersonId into primaryAliasesLeftJoin
                        from primaryAlias in primaryAliasesLeftJoin.DefaultIfEmpty()
                        where primaryAlias != null

                        // Cherry-pick the following info for each new recipient record to be added below.
                        select new
                        {
                            PrimaryAliasId = primaryAlias.Id,
                            MemberCommunicationPreference = listMember.CommunicationPreference,
                            PersonCommunicationPreference = listMember.Person.CommunicationPreference
                        }
                    )
                    .ToList();

                bulkInsertActivity?.AddTag( "rock.communication.recipients_to_add_count", listMembersToAdd.Count );

                var emailMediumEntityType = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
                var smsMediumEntityType = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
                var pushMediumEntityType = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() );

                // Create and add the new communication recipient records.
                var recipientsToAdd = listMembersToAdd.Select( a => new CommunicationRecipient
                {
                    PersonAliasId = a.PrimaryAliasId,
                    Status = CommunicationRecipientStatus.Pending,
                    CommunicationId = Id,
                    MediumEntityTypeId = DetermineMediumEntityTypeId(
                        emailMediumEntityType.Id,
                        smsMediumEntityType.Id,
                        pushMediumEntityType.Id,
                        CommunicationType,
                        a.MemberCommunicationPreference,
                        a.PersonCommunicationPreference )
                } );

                rockContext.BulkInsert<CommunicationRecipient>( recipientsToAdd );
            }

            using ( var bulkDeleteActivity = ObservabilityHelper.StartActivity( "COMMUNICATION: Prepare Recipient List > Refresh Communication Recipient List > Bulk Delete Old Members" ) )
            {
                // Get all pending communication recipients that are no longer
                // in the list of group members and delete them from the recipients.
                // Do not remove nameless recipients that may have been added by the
                // Communication Entry block's Additional Email Recipients feature.
                var namelessPersonRecordTypeId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() );
                var missingMemberInList = recipientsQry
                    .Where( a =>
                        a.Status == CommunicationRecipientStatus.Pending
                        && !qryCommunicationListMembers.Any( r => r.PersonId == a.PersonAlias.PersonId )
                        && a.PersonAlias.Person.RecordTypeValueId != namelessPersonRecordTypeId
                    );

                /*
                    1/2/2024 - JPH

                    This BulkDelete() call introduces a measurable delay of several seconds before actually executing
                    the SQL queries to perform the bulk delete operation; the queries themselves run pretty fast once
                    finally executed. We'll want to circle back here and dig deeper when time allows.

                    While testing alternative approaches, one interesting observation was: if we don't call BulkDelete()
                    here, it seems this delay is simply deferred until the first time BulkDelete() is called - i.e. within
                    the RemoveRecipientsWithDuplicateAddress() method - with subsequent calls to this same method
                    performing much better, even when a different source query is provided as the argument.

                    Reason: Communications with a large number of recipients time out and don't send.
                    https://github.com/SparkDevNetwork/Rock/issues/5651
                */
                rockContext.BulkDelete<CommunicationRecipient>( missingMemberInList );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Determines the medium entity type identifier.
        /// Given the email, SMS medium, and Push entity type ids, along with the available communication preferences,
        /// this method will determine which medium entity type id should be used and return that id.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///  <item>NOTE 1: If you have a SystemCommunication, we recommend using the DetermineMediumEntityTypeId overload that
        ///          accepts a SystemCommunication parameter because it performs more checks to ensure that the SMS
        ///          and/or Push mediums are valid for the given communication.</item>
        ///          
        ///  <item>NOTE 2: For the given communicationTypePreferences parameters array, in the event that CommunicationType.RecipientPreference is given,
        ///          the logic below will use the *next* given CommunicationType to determine which medium/type is selected/returned.
        ///          If none is available, it will return the email medium entity type id.  Typically is expected that the ordered
        ///          params list eventually has either CommunicationType.Email, CommunicationType.SMS or CommunicationType.PushNotification.</item>
        /// </list>
        /// </remarks>
        /// <param name="emailMediumEntityTypeId">The email medium entity type identifier.</param>
        /// <param name="smsMediumEntityTypeId">The SMS medium entity type identifier.</param>
        /// <param name="pushMediumEntityTypeId">The push medium entity type identifier.</param>
        /// <param name="communicationTypePreference">An array of ordered communication type preferences.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Unexpected CommunicationType: {currentCommunicationPreference.ConvertToString()} - communicationTypePreference</exception>
        /// <exception cref="Exception">Unexpected CommunicationType: " + currentCommunicationPreference.ConvertToString()</exception>
        public static int DetermineMediumEntityTypeId( int emailMediumEntityTypeId, int smsMediumEntityTypeId, int pushMediumEntityTypeId, params CommunicationType[] communicationTypePreference )
        {
            for ( var i = 0; i < communicationTypePreference.Length; i++ )
            {
                var currentCommunicationPreference = communicationTypePreference[i];
                var hasNextCommunicationPreference = ( i + 1 ) < communicationTypePreference.Length;

                switch ( currentCommunicationPreference )
                {
                    case CommunicationType.Email:
                        return emailMediumEntityTypeId;
                    case CommunicationType.SMS:
                        return smsMediumEntityTypeId;
                    case CommunicationType.PushNotification:
                        return pushMediumEntityTypeId;
                    case CommunicationType.RecipientPreference:
                        if ( hasNextCommunicationPreference )
                        {
                            break;
                        }

                        return emailMediumEntityTypeId;
                    default:
                        throw new ArgumentException( $"Unexpected CommunicationType: {currentCommunicationPreference.ConvertToString()}", "communicationTypePreference" );
                }
            }

            return emailMediumEntityTypeId;
        }

        /// <summary>
        /// Determines the medium entity type identifier taking into account whether the Medium (SMS and Push) is active and
        /// whether the communication has the required values set for that type.  For example, if the SMS Medium is active,
        /// but the communication does not have an SMS From System Phone Number set, then it will not be returned as the
        /// medium entity type id.
        /// 
        /// Given the email, SMS medium, and Push entity type ids, along with the available communication preferences,
        /// this method will determine which medium entity type id should be used and return that id.
        /// </summary>
        /// 
        /// <remarks>
        /// NOTES:
        ///     <list type="bullet">
        ///     <item>If the person does not have an SMS number then SMS is not available for sending.</item>
        ///     <item>If a medium is not active, then it is not available for sending.</item>
        ///     <item>For the given communicationTypePreferences parameters array, in the event that CommunicationType.RecipientPreference is given,
        ///       the logic below will use the *next* given CommunicationType to determine which medium/type is selected/returned.
        ///     </item>
        ///     <item>If no suitable medium entity type could be selected, it will fall back to Email.</item>
        ///     </list>
        /// </remarks>
        /// <param name="emailMediumEntityTypeId">The email medium entity type identifier.</param>
        /// <param name="smsMediumEntityTypeId">The SMS medium entity type identifier.</param>
        /// <param name="pushMediumEntityTypeId">The push medium entity type identifier.</param>
        /// <param name="communication">The <see cref="Rock.Model.SystemCommunication"/> that is intended to be sent.</param>
        /// <param name="person">The <see cref="Rock.Model.Person"/> that the communication is being sent to.</param>
        /// <param name="communicationTypePreference">An array of ordered communication type preferences.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Unexpected CommunicationType: {currentCommunicationPreference.ConvertToString()} - communicationTypePreference</exception>
        /// <exception cref="Exception">Unexpected CommunicationType: " + currentCommunicationPreference.ConvertToString()</exception>
        public static int DetermineMediumEntityTypeId( int emailMediumEntityTypeId, int smsMediumEntityTypeId, int pushMediumEntityTypeId, SystemCommunication communication, Person person, params CommunicationType[] communicationTypePreference )
        {
            var isSmsActive = MediumContainer.HasActiveSmsTransport();
            var isPushActive = MediumContainer.HasActivePushTransport();

            // Only check for the person's SMS number if SMS is one of the possible communications types being considered.
            string personSmsNumber = string.Empty;
            if ( communicationTypePreference.Contains( CommunicationType.SMS ) )
            {
                personSmsNumber = person?.PhoneNumbers != null
                    ? person.PhoneNumbers.GetFirstSmsNumber()
                    : null;
            }

            var isSmsAvailableForCommunication = communication.SmsFromSystemPhoneNumberId.HasValue && !string.IsNullOrWhiteSpace( personSmsNumber );
            var isPushAvailableForCommunication = !( string.IsNullOrWhiteSpace( communication.PushMessage ) && string.IsNullOrWhiteSpace( communication.PushTitle ));

            for ( var i = 0; i < communicationTypePreference.Length; i++ )
            {
                var currentCommunicationPreference = communicationTypePreference[i];
                var hasNextCommunicationPreference = ( i + 1 ) < communicationTypePreference.Length;

                switch ( currentCommunicationPreference )
                {
                    case CommunicationType.Email:
                        return emailMediumEntityTypeId;

                    case CommunicationType.SMS:
                        if ( isSmsActive && isSmsAvailableForCommunication )
                        {
                            return smsMediumEntityTypeId;
                        }
                        if ( hasNextCommunicationPreference )
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }

                    case CommunicationType.PushNotification:
                        if ( isPushActive && isPushAvailableForCommunication )
                        {
                            return pushMediumEntityTypeId;
                        }
                        if ( hasNextCommunicationPreference )
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }

                    case CommunicationType.RecipientPreference:
                        if ( hasNextCommunicationPreference )
                        {
                            break;
                        }

                        return emailMediumEntityTypeId;

                    default:
                        throw new ArgumentException( $"Unexpected CommunicationType: {currentCommunicationPreference.ConvertToString()}", "communicationTypePreference" );
                }
            }

            return emailMediumEntityTypeId;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name ?? this.Subject ?? base.ToString();
        }

        #endregion Methods

        #region Static Methods

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public static void Send( Rock.Model.Communication communication )
        {
            AsyncHelper.RunSync( () => SendAsync( communication ) );
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public async static Task SendAsync( Rock.Model.Communication communication )
        {
            if ( communication == null || communication.Status != CommunicationStatus.Approved )
            {
                return;
            }

            // Only alter the recipient list if Rock hasn't already begun sending to recipients.
            using ( var rockContext = new RockContext() )
            {
                var hasSendingBegun = GetOrSetHasSendingBegun( communication.Id, rockContext );

                if ( !communication.SendDateTime.HasValue && !hasSendingBegun )
                {
                    using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Send Async > Prepare Recipient List" ) )
                    {
                        activity?.AddTag( "rock.communication.id", communication.Id );
                        activity?.AddTag( "rock.communication.name", communication.Name );

                        /*
                            1/2/2024 - JPH

                            We're increasing this timeout from the default of 30 seconds to give the following
                            pre-send tasks more time to complete, as the sending of communications with a large
                            number of recipients is most often done as a background task, and shouldn't risk
                            tying up the UI.

                            Reason: Communications with a large number of recipients time out and don't send.
                            https://github.com/SparkDevNetwork/Rock/issues/5651
                        */
                        rockContext.Database.SetCommandTimeout( 90 );

                        if ( communication.ListGroupId.HasValue )
                        {
                            communication.RefreshCommunicationRecipientList( rockContext );
                        }

                        if ( communication.ExcludeDuplicateRecipientAddress )
                        {
                            communication.RemoveRecipientsWithDuplicateAddress( rockContext );
                        }

                        communication.RemoveDuplicatePersonRecipients( rockContext );
                    }
                }
            }

            var sendTasks = new List<Task>();
            foreach ( var medium in communication.GetMediums() )
            {
                var asyncMedium = medium as IAsyncMediumComponent;

                if ( asyncMedium == null )
                {
                    sendTasks.Add( Task.Run( () => medium.Send( communication ) ) );
                }
                else
                {
                    sendTasks.Add( asyncMedium.SendAsync( communication ) );
                }
            }

            var aggregateExceptions = new List<Exception>();
            while ( sendTasks.Count > 0 )
            {
                var completedTask = await Task.WhenAny( sendTasks ).ConfigureAwait( false );
                if ( completedTask.Exception != null )
                {
                    aggregateExceptions.AddRange( completedTask.Exception.InnerExceptions );
                }

                sendTasks.Remove( completedTask );
            }

            if ( aggregateExceptions.Count > 0 )
            {
                throw new AggregateException( aggregateExceptions );
            }

            using ( var rockContext = new RockContext() )
            {
                var dbCommunication = new CommunicationService( rockContext ).Get( communication.Id );

                dbCommunication.UpdateSendingRecipients();

                if ( !dbCommunication.HasPendingRecipients( rockContext ) )
                {
                    // Set the SendDateTime of the Communication
                    dbCommunication.SendDateTime = RockDateTime.Now;
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets whether Rock has already begun sending this communication to any of its recipients.
        /// </summary>
        /// <param name="communicationId">The communication identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>Whether Rock has already begun sending this communication to any of its recipients.</returns>
        /// <remarks>
        /// If sending hasn't already begun, one of the recipient's <see cref="CommunicationRecipient.FirstSendAttemptDateTime"/>
        /// will be set to <see cref="RockDateTime.Now"/> to indicate that sending has begun.
        /// </remarks>
        public static bool GetOrSetHasSendingBegun( int communicationId, RockContext rockContext )
        {
            var communicationRecipient = new CommunicationRecipientService( rockContext )
                .Queryable()
                .Where( cr =>
                    cr.CommunicationId == communicationId
                )
                .OrderByDescending( cr => cr.FirstSendAttemptDateTime.HasValue )
                .FirstOrDefault();

            if ( communicationRecipient?.FirstSendAttemptDateTime.HasValue == true )
            {
                return true;
            }

            if ( communicationRecipient != null )
            {
                communicationRecipient.FirstSendAttemptDateTime = RockDateTime.Now;
                rockContext.SaveChanges();
            }

            return false;
        }

        /// <summary>
        /// Gets the next pending communication recipient for the specified communication and medium entity type.
        /// </summary>
        /// <param name="communicationId">The communication identifier.</param>
        /// <param name="mediumEntityId">The medium entity type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The next pending communication recipient or <see langword="null"/> if there are no more non-expired,
        /// pending recipients.</returns>
        public static CommunicationRecipient GetNextPending( int communicationId, int mediumEntityId, RockContext rockContext )
        {
            CommunicationRecipient recipient = null;

            var previousSendLockExpiredDateTime = RockDateTime.Now.AddMinutes( CommunicationService.PreviousSendLockExpiredMinutes );

            /*
                9/27/2024 - JPH

                By wrapping the following in a transaction and using table hints within our query, we instruct SQL Server
                to lock the next pending communication recipient row, knowing that multiple Rock instances (in a web farm
                environment) + multiple threads and tasks (within each Rock instance) can simultaneously access this block
                of code.

                XLOCK: This hint places an exclusive lock on the rows read by the SELECT. The exclusive lock prevents
                       other transactions from reading or modifying those rows until the current transaction completes.
                       This is key in preventing simultaneous updates to the same rows.

                ROWLOCK: This ensures that locks are applied at the row level, which is efficient when working with
                         smaller sets of data (as we are in this case: seeking only one row at a time).

                READPAST: This hint skips rows that are locked by other transactions. It prevents the current transaction
                          from blocking or waiting on locked rows, but it will skip over them and continue processing
                          other rows.

                Reason: Ensure each recipient receives only a singly copy of each communication.
             */

            rockContext.WrapTransaction( () =>
            {
                var recipientId = rockContext.Database.SqlQuery<int?>( @"
UPDATE cr
SET cr.[ModifiedDateTime] = @Now
    , cr.[Status] = @SendingStatus
    , cr.[FirstSendAttemptDateTime] = CASE
        WHEN cr.[FirstSendAttemptDateTime] IS NOT NULL
            THEN cr.[FirstSendAttemptDateTime]
            ELSE @FirstSendAttemptDateTime
        END
OUTPUT INSERTED.[Id]
FROM [CommunicationRecipient] cr
WHERE cr.[Id] IN (
    SELECT TOP 1 next.[Id]
    FROM [CommunicationRecipient] next WITH (XLOCK, ROWLOCK, READPAST)
    WHERE next.[CommunicationId] = @CommunicationId
        AND next.[MediumEntityTypeId] = @MediumEntityTypeId
        AND (
            next.[Status] = @PendingStatus
            OR (
                next.[Status] = @SendingStatus
                AND next.[ModifiedDateTime] < @PreviousSendLockExpiredDateTime
            )
        )
);",
                        new SqlParameter( "@CommunicationId", communicationId ),
                        new SqlParameter( "@MediumEntityTypeId", mediumEntityId ),
                        new SqlParameter( "@PendingStatus", CommunicationRecipientStatus.Pending ),
                        new SqlParameter( "@SendingStatus", CommunicationRecipientStatus.Sending ),
                        new SqlParameter( "@FirstSendAttemptDateTime", RockDateTime.Now ),
                        new SqlParameter( "@PreviousSendLockExpiredDateTime", previousSendLockExpiredDateTime ),
                        new SqlParameter( "@Now", RockDateTime.Now )
                    ).FirstOrDefault();

                if ( recipientId.HasValue )
                {
                    recipient = new CommunicationRecipientService( rockContext )
                        .Queryable()
                        .Include( r => r.Communication )
                        .Include( r => r.PersonAlias.Person )
                        .FirstOrDefault( r => r.Id == recipientId.Value );
                }
            } );

            return recipient;
        }

        #endregion Static Methods
    }
}
