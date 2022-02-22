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
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Rock.Communication;
using Rock.Data;
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

            var communicationRecipientService = new CommunicationRecipientService( rockContext );

            var recipientsQry = GetRecipientsQry( rockContext );

            int? smsMediumEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
            if ( smsMediumEntityTypeId.HasValue )
            {
                IQueryable<CommunicationRecipient> duplicateSMSRecipientsQuery = recipientsQry.Where( a => a.MediumEntityTypeId == smsMediumEntityTypeId.Value )
                    .Where( a => a.PersonAlias.Person.PhoneNumbers.Where( pn => pn.IsMessagingEnabled ).Any() )
                    .GroupBy( a => a.PersonAlias.Person.PhoneNumbers.Where( pn => pn.IsMessagingEnabled ).FirstOrDefault().Number )
                    .Where( a => a.Count() > 1 )
                    .Select( a => a.OrderBy( x => x.Id ).Skip( 1 ).ToList() )
                    .SelectMany( a => a );

                var duplicateSMSRecipients = duplicateSMSRecipientsQuery.ToList();
                communicationRecipientService.DeleteRange( duplicateSMSRecipients );
            }

            int? emailMediumEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
            if ( emailMediumEntityTypeId.HasValue )
            {
                IQueryable<CommunicationRecipient> duplicateEmailRecipientsQry = recipientsQry.Where( a => a.MediumEntityTypeId == emailMediumEntityTypeId.Value )
                    .GroupBy( a => a.PersonAlias.Person.Email )
                    .Where( a => a.Count() > 1 )
                    .Select( a => a.OrderBy( x => x.Id ).Skip( 1 ).ToList() )
                    .SelectMany( a => a );

                var duplicateEmailRecipients = duplicateEmailRecipientsQry.ToList();
                communicationRecipientService.DeleteRange( duplicateEmailRecipients );
            }

            rockContext.SaveChanges();
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

            var segmentDataViewGuids = this.Segments.SplitDelimitedValues().AsGuidList();
            var segmentDataViewIds = new DataViewService( rockContext ).GetByGuids( segmentDataViewGuids ).Select( a => a.Id ).ToList();

            var qryCommunicationListMembers = GetCommunicationListMembers( rockContext, ListGroupId, this.SegmentCriteria, segmentDataViewIds );

            // NOTE: If this is scheduled communication, don't include Members that were added after the scheduled FutureSendDateTime.
            // However, don't exclude if the date added can't be determined or they will never be sent a scheduled communication.
            if ( this.FutureSendDateTime.HasValue )
            {
                var memberAddedCutoffDate = this.FutureSendDateTime;

                qryCommunicationListMembers = qryCommunicationListMembers.Where( a => ( a.DateTimeAdded.HasValue && a.DateTimeAdded.Value < memberAddedCutoffDate )
                                                                                        || ( a.CreatedDateTime.HasValue && a.CreatedDateTime.Value < memberAddedCutoffDate )
                                                                                        || ( !a.DateTimeAdded.HasValue && !a.CreatedDateTime.HasValue ) );
            }

            var communicationRecipientService = new CommunicationRecipientService( rockContext );

            var recipientsQry = GetRecipientsQry( rockContext );

            // Get all the List member which is not part of communication recipients yet
            var newMemberInList = qryCommunicationListMembers
                .Include( c => c.Person )
                .Where( a => !recipientsQry.Any( r => r.PersonAlias.PersonId == a.PersonId ) )
                .AsNoTracking()
                .ToList();

            var emailMediumEntityType = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
            var smsMediumEntityType = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
            var pushMediumEntityType = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() );

            var recipientsToAdd = newMemberInList.Select( m => new CommunicationRecipient
            {
                PersonAliasId = m.Person.PrimaryAliasId.Value,
                Status = CommunicationRecipientStatus.Pending,
                CommunicationId = Id,
                MediumEntityTypeId = DetermineMediumEntityTypeId(
                    emailMediumEntityType.Id,
                    smsMediumEntityType.Id,
                    pushMediumEntityType.Id,
                    CommunicationType,
                    m.CommunicationPreference,
                    m.Person.CommunicationPreference )
            } );
            rockContext.BulkInsert<CommunicationRecipient>( recipientsToAdd );

            // Get all pending communication recipients that are no longer part of the group list member, then delete them from the Recipients
            var missingMemberInList = recipientsQry.Where( a => a.Status == CommunicationRecipientStatus.Pending )
                .Where( a => !qryCommunicationListMembers.Any( r => r.PersonId == a.PersonAlias.PersonId ) );

            rockContext.BulkDelete<CommunicationRecipient>( missingMemberInList );

            rockContext.SaveChanges();
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

        private static object _obj = new object();

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
                using ( var rockContext = new RockContext() )
                {
                    if ( communication.ListGroupId.HasValue )
                    {
                        communication.RefreshCommunicationRecipientList( rockContext );
                    }

                    if ( communication.ExcludeDuplicateRecipientAddress )
                    {
                        communication.RemoveRecipientsWithDuplicateAddress( rockContext );
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
                using ( var rockContext = new RockContext() )
                {
                    if ( communication.ListGroupId.HasValue )
                    {
                        communication.RefreshCommunicationRecipientList( rockContext );
                    }

                    if ( communication.ExcludeDuplicateRecipientAddress )
                    {
                        communication.RemoveRecipientsWithDuplicateAddress( rockContext );
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
        /// Gets the next pending.
        /// </summary>
        /// <param name="communicationId">The communication identifier.</param>
        /// <param name="mediumEntityId">The medium entity identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static Rock.Model.CommunicationRecipient GetNextPending( int communicationId, int mediumEntityId, Rock.Data.RockContext rockContext )
        {
            CommunicationRecipient recipient = null;

            var delayTime = RockDateTime.Now.AddMinutes( -10 );

            lock ( _obj )
            {
                recipient = new CommunicationRecipientService( rockContext ).Queryable().Include( r => r.Communication ).Include( r => r.PersonAlias.Person )
                    .Where( r =>
                        r.CommunicationId == communicationId &&
                        ( r.Status == CommunicationRecipientStatus.Pending ||
                            ( r.Status == CommunicationRecipientStatus.Sending && r.ModifiedDateTime < delayTime )
                        ) &&
                        r.MediumEntityTypeId.HasValue &&
                        r.MediumEntityTypeId.Value == mediumEntityId )
                    .FirstOrDefault();

                if ( recipient != null )
                {
                    recipient.Status = CommunicationRecipientStatus.Sending;
                    rockContext.SaveChanges();
                }
            }

            return recipient;
        }

        #endregion
    }
}
