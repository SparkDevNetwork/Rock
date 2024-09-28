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

using Rock.Communication;
using Rock.Data;
using Rock.Observability;
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

        #endregion
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
            return new CommunicationRecipientService( rockContext ).Queryable().Where( a => a.CommunicationId == this.Id && a.Status == Model.CommunicationRecipientStatus.Pending ).Any();
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
            IQueryable<GroupMember> groupMemberQuery = null;
            if ( listGroupId.HasValue )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var personService = new PersonService( rockContext );
                var dataViewService = new DataViewService( rockContext );

                groupMemberQuery = groupMemberService.Queryable().Where( a => a.GroupId == listGroupId.Value && a.GroupMemberStatus == GroupMemberStatus.Active );

                Expression segmentExpression = null;
                ParameterExpression paramExpression = personService.ParameterExpression;
                var segmentDataViewList = dataViewService.GetByIds( segmentDataViewIds ).AsNoTracking().ToList();
                foreach ( var segmentDataView in segmentDataViewList )
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
                    groupMemberQuery = groupMemberQuery.Where( a => personQry.Any( p => p.Id == a.PersonId ) );
                }
            }

            return groupMemberQuery;
        }

        /// <summary>
        /// if <see cref="ExcludeDuplicateRecipientAddress" /> is set to true, removes <see cref="CommunicationRecipient"></see>s that have the same SMS/Email address as another recipient
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        public void RemoveRecipientsWithDuplicateAddress( RockContext rockContext )
        {
            if ( !ExcludeDuplicateRecipientAddress )
            {
                return;
            }

            /*
                6/25/2024 - JPH

                These delete operations were previously accomplished with EF-generated queries that made use of CROSS APPLY
                and complex, nested SELECTs, occasionally resulting in timeouts. The alternative, custom queries below that
                make use of table variables are much more performant - and prevent timeouts - in local testing.

                Note that the goal is leave the first recipient for each duplicate contact number/email while deleting
                the remaining duplicates for each.

                Here's what the EF queries previously were (so we're not tempted to replace the custom SQL with
                similarly-problematic EF queries in the future):

                Delete Duplicate SMS Recipients:
                --------------------------------
                IQueryable<CommunicationRecipient> duplicateSMSRecipientsQuery = recipientsQry.Where( a => a.MediumEntityTypeId == smsMediumEntityTypeId.Value )
                        .Where( a => a.PersonAlias.Person.PhoneNumbers.Where( pn => pn.IsMessagingEnabled ).Any() )
                        .GroupBy( a => a.PersonAlias.Person.PhoneNumbers.Where( pn => pn.IsMessagingEnabled ).FirstOrDefault().Number )
                        .Where( a => a.Count() > 1 )
                        .Select( a => a.OrderBy( x => x.Id ).Skip( 1 ).ToList() )
                        .SelectMany( a => a );

                rockContext.BulkDelete<CommunicationRecipient>( duplicateSMSRecipientsQuery );

                Delete Duplicate Email Recipients:
                ----------------------------------
                IQueryable<CommunicationRecipient> duplicateEmailRecipientsQry = recipientsQry.Where( a => a.MediumEntityTypeId == emailMediumEntityTypeId.Value )
                        .GroupBy( a => a.PersonAlias.Person.Email )
                        .Where( a => a.Count() > 1 )
                        .Select( a => a.OrderBy( x => x.Id ).Skip( 1 ).ToList() )
                        .SelectMany( a => a );

                rockContext.BulkDelete<CommunicationRecipient>( duplicateEmailRecipientsQry );

                Reason: Communications with a large number of recipients time out and don't send.
                https://github.com/SparkDevNetwork/Rock/issues/5651
             */

            using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Prepare Recipient List > Remove Recipients With Duplicate Address" ) )
            {
                int? smsMediumEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
                if ( smsMediumEntityTypeId.HasValue )
                {
                    var deleteDuplicateSmsRecipientsSql = @"
/******************************************************************************
* 1. Get the first SMS-enabled phone number for each recipient of the
*    specified communication.
*/

DECLARE @SmsNumbers TABLE
(
    [CommunicationRecipientId] [int] NOT NULL
    , [Number] [nvarchar](100) NULL
);

INSERT INTO @SmsNumbers
SELECT cr.[Id]
    , (
        SELECT TOP 1 [Number]
        FROM [PhoneNumber]
        WHERE [PersonId] = p.[Id]
            AND [IsMessagingEnabled] = 1
    )
FROM [CommunicationRecipient] cr
INNER JOIN [PersonAlias] pa
    ON pa.[Id] = cr.[PersonAliasId]
INNER JOIN [Person] p
    ON p.[Id] = pa.[PersonId]
WHERE cr.[CommunicationId] = @CommunicationId
    AND cr.[MediumEntityTypeId] = @MediumEntityTypeId;

/******************************************************************************
* 2. Get duplicate SMS numbers for the specified communication.
*/

DECLARE @DuplicateNumbers TABLE
(
    [Number] [varchar](100) NOT NULL
);

INSERT INTO @DuplicateNumbers
SELECT [Number]
FROM @SmsNumbers
WHERE [Number] IS NOT NULL
    AND [Number] <> ''
GROUP BY [Number]
HAVING COUNT(1) > 1;

/******************************************************************************
* 3. Get recipients of these duplicate SMS numbers.
*/

DECLARE @DuplicateRecipients TABLE
(
    [Number] [varchar](100) NOT NULL
    , [CommunicationRecipientId] [int] NOT NULL
);

INSERT INTO @DuplicateRecipients
SELECT sn.[Number]
    , sn.[CommunicationRecipientId]
FROM @SmsNumbers sn
INNER JOIN @DuplicateNumbers dn
    ON dn.[Number] = sn.[Number];

/******************************************************************************
* 4. Get the first recipient for each duplicate SMS number.
*/

DECLARE @FirstRecipients TABLE
(
    [CommunicationRecipientId] [int] NOT NULL
);

INSERT INTO @FirstRecipients
SELECT MIN([CommunicationRecipientId])
FROM @DuplicateRecipients
GROUP BY [Number];

/******************************************************************************
* 5. Delete the first recipient for each duplicate SMS number from the
*    @DuplicateRecipients table. These are the recipients we'll end up
*    KEEPING in the final recipients list.
*/

DELETE dr
FROM @DuplicateRecipients dr
INNER JOIN @FirstRecipients fr
    ON fr.[CommunicationRecipientId] = dr.[CommunicationRecipientId];

/******************************************************************************
* 6. Finally, delete the duplicate [CommunicationRecipient] records that are
*    joined to those IDs that remain in the @DuplicateRecipients table.
*/

DELETE cr
FROM [CommunicationRecipient] cr
INNER JOIN @DuplicateRecipients dr
    ON dr.[CommunicationRecipientId] = cr.[Id];";

                    var parameters = new List<object>
                    {
                        new SqlParameter( "@CommunicationId", this.Id ),
                        new SqlParameter( "@MediumEntityTypeId", smsMediumEntityTypeId.Value )
                    };

                    rockContext.Database.ExecuteSqlCommand( deleteDuplicateSmsRecipientsSql, parameters.ToArray() );
                }

                int? emailMediumEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
                if ( emailMediumEntityTypeId.HasValue )
                {
                    var deleteDuplicateEmailRecipientsSql = @"
/******************************************************************************
* 1. Get duplicate email addresses for the specified communication.
*/

DECLARE @DuplicateEmails TABLE
(
    [Email] [varchar](100) NOT NULL
);

INSERT INTO @DuplicateEmails
SELECT p.[Email]
FROM [CommunicationRecipient] cr
INNER JOIN [PersonAlias] pa
    ON pa.[Id] = cr.[PersonAliasId]
INNER JOIN [Person] p
    ON p.[Id] = pa.[PersonId]
WHERE cr.[CommunicationId] = @CommunicationId
    AND cr.[MediumEntityTypeId] = @MediumEntityTypeId
    AND p.[Email] IS NOT NULL
    AND p.[Email] <> ''
GROUP BY p.[Email]
HAVING COUNT(1) > 1;

/******************************************************************************
* 2. Get recipients of these duplicate email addresses.
*/

DECLARE @DuplicateRecipients TABLE
(
    [Email] [varchar](100) NOT NULL
    , [CommunicationRecipientId] [int] NOT NULL
);

INSERT INTO @DuplicateRecipients
SELECT p.[Email]
    , cr.[Id]
FROM [CommunicationRecipient] cr
INNER JOIN [PersonAlias] pa
    ON pa.[Id] = cr.[PersonAliasId]
INNER JOIN [Person] p
    ON p.[Id] = pa.[PersonId]
INNER JOIN @DuplicateEmails de
    ON de.[Email] = p.[Email]
WHERE cr.[CommunicationId] = @CommunicationId
    AND cr.[MediumEntityTypeId] = @MediumEntityTypeId;

/******************************************************************************
* 3. Get the first recipient for each duplicate email address.
*/

DECLARE @FirstRecipients TABLE
(
    [CommunicationRecipientId] [int] NOT NULL
);

INSERT INTO @FirstRecipients
SELECT MIN([CommunicationRecipientId])
FROM @DuplicateRecipients
GROUP BY [Email];

/******************************************************************************
* 4. Delete the first recipient for each duplicate email address from the
*    @DuplicateRecipients table. These are the recipients we'll end up
*    KEEPING in the final recipients list.
*/

DELETE dr
FROM @DuplicateRecipients dr
INNER JOIN @FirstRecipients fr
    ON fr.[CommunicationRecipientId] = dr.[CommunicationRecipientId];

/******************************************************************************
* 5. Finally, delete the duplicate [CommunicationRecipient] records that are
*    joined to those IDs that remain in the @DuplicateRecipients table.
*/

DELETE cr
FROM [CommunicationRecipient] cr
INNER JOIN @DuplicateRecipients dr
    ON dr.[CommunicationRecipientId] = cr.[Id];";

                    var parameters = new List<object>
                    {
                        new SqlParameter( "@CommunicationId", this.Id ),
                        new SqlParameter( "@MediumEntityTypeId", emailMediumEntityTypeId.Value )
                    };

                    rockContext.Database.ExecuteSqlCommand( deleteDuplicateEmailRecipientsSql, parameters.ToArray() );
                }
            }
        }

        /// <summary>
        /// Removes the non-primary person alias recipients.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void RemoveNonPrimaryPersonAliasRecipients( RockContext rockContext )
        {
            /*
                5/4/2022 - DMV

                In tracking down alleged duplicate communications we discovered
                that duplicates could be sent to the same person if they are in the
                recipient list more that once with multiple Person Alias IDs.
                This could have occurred through a person merge or other data changes
                in Rock. This method removes those duplicates from the list before
                sending the communication.
            */

            /*
                1/2/2024 - JPH

                We were previously loading these entities into memory and calling DeleteRange() on the
                collection, which was causing a separate DELETE statement to be run for each entity.
                By instead calling BulkDelete(), we can run the delete operation outside of EF context,
                bypassing quite a bit of unnecessary overhead.

                Reason: Communications with a large number of recipients time out and don't send.
                https://github.com/SparkDevNetwork/Rock/issues/5651
            */

            using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Prepare Recipient List > Remove Non-Primary Person Alias Recipients" ) )
            {
                var communicationRecipientService = new CommunicationRecipientService( rockContext );

                var recipientsQry = GetRecipientsQry( rockContext );

                int? smsMediumEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
                if ( smsMediumEntityTypeId.HasValue )
                {
                    var duplicateSMSRecipientsQuery = recipientsQry
                        .Where( a =>
                            a.MediumEntityTypeId == smsMediumEntityTypeId.Value
                            && a.PersonAlias.PersonId != a.PersonAlias.AliasPersonId
                        );

                    rockContext.BulkDelete<CommunicationRecipient>( duplicateSMSRecipientsQuery );
                }

                int? emailMediumEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
                if ( emailMediumEntityTypeId.HasValue )
                {
                    var duplicateEmailRecipientsQry = recipientsQry
                        .Where( a =>
                            a.MediumEntityTypeId == emailMediumEntityTypeId.Value
                            && a.PersonAlias.PersonId != a.PersonAlias.AliasPersonId
                        );

                    rockContext.BulkDelete<CommunicationRecipient>( duplicateEmailRecipientsQry );
                }
            }
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

            using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Prepare Recipient List > Refresh Communication Recipient List" ) )
            {
                var segmentDataViewGuids = this.Segments.SplitDelimitedValues().AsGuidList();
                var segmentDataViewIds = new DataViewService( rockContext ).GetByGuids( segmentDataViewGuids ).Select( a => a.Id ).ToList();

                var qryCommunicationListMembers = GetCommunicationListMembers( rockContext, ListGroupId, this.SegmentCriteria, segmentDataViewIds );

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
                    // Get all pending communication recipients that are no longer in the list of group members and delete them from the recipients.
                    var missingMemberInList = recipientsQry
                        .Where( a =>
                            a.Status == CommunicationRecipientStatus.Pending
                            && !qryCommunicationListMembers.Any( r => r.PersonId == a.PersonAlias.PersonId )
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
        }

        /// <summary>
        /// Determines the medium entity type identifier.
        /// Given the email, SMS medium, and Push entity type ids, along with the available communication preferences,
        /// this method will determine which medium entity type id should be used and return that id.
        /// </summary>
        /// <remarks>
        ///  NOTE: For the given communicationTypePreferences parameters array, in the event that CommunicationType.RecipientPreference is given,
        ///  the logic below will use the *next* given CommunicationType to determine which medium/type is selected/returned. If none is available,
        ///  it will return the email medium entity type id.  Typically is expected that the ordered params list eventually has either
        ///  CommunicationType.Email, CommunicationType.SMS or CommunicationType.PushNotification.
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
        /// Determines the medium entity type identifier.
        /// Given the email and sms medium entity type ids and the available communication preferences
        /// this method will determine which medium entity type id should be used and return that id.
        /// If a preference could not be determined the email medium entity type id will be returned.
        /// </summary>
        /// <param name="emailMediumEntityTypeId">The email medium entity type identifier.</param>
        /// <param name="smsMediumEntityTypeId">The SMS medium entity type identifier.</param>
        /// <param name="recipientPreference">The recipient preference.</param>
        /// <returns></returns>
        [Obsolete( "Use the override that includes 'pushMediumEntityTypeId' instead." )]
        [RockObsolete( "1.11" )]
        public static int DetermineMediumEntityTypeId( int emailMediumEntityTypeId, int smsMediumEntityTypeId, params CommunicationType[] recipientPreference )
        {
            return DetermineMediumEntityTypeId( emailMediumEntityTypeId, smsMediumEntityTypeId, 0, recipientPreference );
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

        #endregion

        #region Static Methods

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public static void Send( Rock.Model.Communication communication )
        {
            if ( communication == null || communication.Status != CommunicationStatus.Approved )
            {
                return;
            }

            // only alter the Recipient list if it the communication hasn't sent a message to any recipients yet
            if ( communication.SendDateTime.HasValue == false )
            {
                using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Send > Prepare Recipient List" ) )
                {
                    activity?.AddTag( "rock.communication.id", communication.Id );
                    activity?.AddTag( "rock.communication.name", communication.Name );

                    using ( var rockContext = new RockContext() )
                    {
                        /*
                            1/2/2024 - JPH

                            We're increasing this timeout from the default of 30 seconds to give the following
                            pre-send tasks more time to complete, as the sending of communications with a large
                            number of recipients is most often done as a background task, and shouldn't risk
                            tying up the UI.

                            Reason: Communications with a large number of recipients time out and don't send.
                            https://github.com/SparkDevNetwork/Rock/issues/5651
                        */
                        rockContext.Database.CommandTimeout = 90;

                        if ( communication.ListGroupId.HasValue )
                        {
                            communication.RefreshCommunicationRecipientList( rockContext );
                        }

                        if ( communication.ExcludeDuplicateRecipientAddress )
                        {
                            communication.RemoveRecipientsWithDuplicateAddress( rockContext );
                        }

                        communication.RemoveNonPrimaryPersonAliasRecipients( rockContext );
                    }
                }
            }

            foreach ( var medium in communication.GetMediums() )
            {
                medium.Send( communication );
            }

            using ( var rockContext = new RockContext() )
            {
                var dbCommunication = new CommunicationService( rockContext ).Get( communication.Id );

                // Set the SendDateTime of the Communication
                dbCommunication.SendDateTime = RockDateTime.Now;
                rockContext.SaveChanges();
            }
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

            // only alter the Recipient list if it the communication hasn't sent a message to any recipients yet
            if ( communication.SendDateTime.HasValue == false )
            {
                using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Send Async > Prepare Recipient List" ) )
                {
                    activity?.AddTag( "rock.communication.id", communication.Id );
                    activity?.AddTag( "rock.communication.name", communication.Name );

                    using ( var rockContext = new RockContext() )
                    {
                        /*
                            1/2/2024 - JPH

                            We're increasing this timeout from the default of 30 seconds to give the following
                            pre-send tasks more time to complete, as the sending of communications with a large
                            number of recipients is most often done as a background task, and shouldn't risk
                            tying up the UI.

                            Reason: Communications with a large number of recipients time out and don't send.
                            https://github.com/SparkDevNetwork/Rock/issues/5651
                        */
                        rockContext.Database.CommandTimeout = 90;

                        if ( communication.ListGroupId.HasValue )
                        {
                            communication.RefreshCommunicationRecipientList( rockContext );
                        }

                        if ( communication.ExcludeDuplicateRecipientAddress )
                        {
                            communication.RemoveRecipientsWithDuplicateAddress( rockContext );
                        }

                        communication.RemoveNonPrimaryPersonAliasRecipients( rockContext );
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

                // Set the SendDateTime of the Communication
                dbCommunication.SendDateTime = RockDateTime.Now;
                rockContext.SaveChanges();
            }
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

        #endregion
    }
}
