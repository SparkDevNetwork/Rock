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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Communication;
using Rock.Web.Cache;
using System.Data.Entity;
using System.Linq.Expressions;
using Rock.Web;

namespace Rock.Model
{
    /// <summary>
    /// Represents a communication in Rock (i.e. email, SMS message, etc.).
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "Communication" )]
    [DataContract]
    public partial class Communication : Model<Communication>, ICommunicationDetails
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the communication.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the communication type value identifier.
        /// </summary>
        /// <value>
        /// The communication type value identifier.
        /// </value>
        [Required]
        [DataMember]
        public CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// Gets or sets the URL from where this communication was created (grid)
        /// </summary>
        /// <value>
        /// The URL referrer.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string UrlReferrer { get; set; }

        /// <summary>
        /// Gets or sets the list that email is being sent to.
        /// </summary>
        /// <value>
        /// The list group identifier.
        /// </value>
        [DataMember]
        public int? ListGroupId { get; set; }

        /// <summary>
        /// Gets or sets the segments that list is being filtered to (comma-delimited list of dataview guids).
        /// </summary>
        /// <value>
        /// The segments.
        /// </value>
        [DataMember]
        public string Segments { get; set; }

        /// <summary>
        /// Gets or sets if communication is targeted to people in all selected segments or any selected segments.
        /// </summary>
        /// <value>
        /// The segment criteria.
        /// </value>
        [DataMember]
        public SegmentCriteria SegmentCriteria { get; set; }

        /// <summary>
        /// Gets or sets the Communication Template that was used to compose this communication
        /// </summary>
        /// <value>
        /// The communication template identifier.
        /// </value>
        /// <remarks>
        /// [IgnoreCanDelete] since there is a ON DELETE SET NULL cascade on this
        /// </remarks>
        [IgnoreCanDelete]
        public int? CommunicationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the sender person alias identifier.
        /// </summary>
        /// <value>
        /// The sender person alias identifier.
        /// </value>
        [DataMember]
        public int? SenderPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the is bulk communication.
        /// </summary>
        /// <value>
        /// The is bulk communication.
        /// </value>
        [DataMember]
        public bool IsBulkCommunication { get; set; }

        /// <summary>
        /// Gets or sets the datetime that communication was sent. This also indicates that communication shouldn't attempt to send again.
        /// </summary>
        /// <value>
        /// The send date time.
        /// </value>
        [DataMember]
        public DateTime? SendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the future send date for the communication. This allows a user to schedule when a communication is sent 
        /// and the communication will not be sent until that date and time.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> value that represents the FutureSendDate for the communication.  If no future send date is provided, this value will be null.
        /// </value>
        [DataMember]
        public DateTime? FutureSendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the status of the Communication.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.CommunicationStatus"/> enum value that represents the status of the Communication.
        /// </value>
        [DataMember]
        public CommunicationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the reviewer person alias identifier.
        /// </summary>
        /// <value>
        /// The reviewer person alias identifier.
        /// </value>
        [DataMember]
        public int? ReviewerPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the date and time stamp of when the Communication was reviewed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the Communication was reviewed.
        /// </value>
        [DataMember]
        public DateTime? ReviewedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the note that was entered by the reviewer.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a note that was entered by the reviewer.
        /// </value>
        [DataMember]
        public string ReviewerNote { get; set; }

        /// <summary>
        /// Gets or sets a Json formatted string containing the Medium specific data.
        /// </summary>
        /// <value>
        /// A Json formatted <see cref="System.String"/> that contains any Medium specific data.
        /// </value>
        [DataMember]
        [RockObsolete( "1.7" )]
        [Obsolete( "MediumDataJson is no longer used.", true )]
        public string MediumDataJson { get; set; }

        /// <summary>
        /// Gets or sets a Json string containing any additional merge fields for the Communication.
        /// </summary>
        /// <value>
        /// A Json formatted <see cref="System.String"/> that contains any additional merge fields for the Communication.
        /// </value>
        [DataMember]
        public string AdditionalMergeFieldsJson
        {
            get
            {
                return AdditionalMergeFields.ToJson();
            }

            set
            {
                AdditionalMergeFields = value.FromJsonOrNull<List<string>>() ?? new List<string>();
            }
        }

        /// <summary>
        /// Gets or sets the enabled lava commands.
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        [DataMember]
        public string EnabledLavaCommands { get; set; }

        #region Email Fields

        /// <summary>
        /// Gets or sets the name of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the communication.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>
        /// From name.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets from email address.
        /// </summary>
        /// <value>
        /// From email address.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the reply to email address.
        /// </summary>
        /// <value>
        /// The reply to email address.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets a comma separated list of CC'ed email addresses.
        /// </summary>
        /// <value>
        /// A comma separated list of CC'ed email addresses.
        /// </value>
        [DataMember]
        public string CCEmails { get; set; }

        /// <summary>
        /// Gets or sets a comma separated list of BCC'ed email addresses.
        /// </summary>
        /// <value>
        /// A comma separated list of BCC'ed email addresses.
        /// </value>
        [DataMember]
        public string BCCEmails { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the message meta data.
        /// </summary>
        /// <value>
        /// The message meta data.
        /// </value>
        [DataMember]
        public string MessageMetaData { get; set; }

        #endregion

        #region SMS Properties

        /// <summary>
        /// Gets or sets the SMS from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        [DataMember]
        public int? SMSFromDefinedValueId { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string SMSMessage { get; set; }

        #endregion

        #region Push Notification Properties

        /// <summary>
        /// Gets or sets from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string PushTitle { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string PushSound { get; set; }

        #endregion

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the list group.
        /// </summary>
        /// <value>
        /// The list group.
        /// </value>
        [DataMember]
        public virtual Group ListGroup { get; set; }

        /// <summary>
        /// Gets or sets the sender person alias.
        /// </summary>
        /// <value>
        /// The sender person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias SenderPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the reviewer person alias.
        /// </summary>
        /// <value>
        /// The reviewer person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ReviewerPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> for the Communication.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> of the Communication.
        /// </value>
        [DataMember]
        public virtual ICollection<CommunicationRecipient> Recipients
        {
            get { return _recipients ?? ( _recipients = new Collection<CommunicationRecipient>() ); }
            set { _recipients = value; }
        }
        private ICollection<CommunicationRecipient> _recipients;

        /// <summary>
        /// Gets or sets the attachments.
        /// NOTE: In most cases, you should use GetAttachments( CommunicationType ) instead.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        [DataMember]
        public virtual ICollection<CommunicationAttachment> Attachments
        {
            get { return _attachments ?? ( _attachments = new Collection<CommunicationAttachment>() ); }
            set { _attachments = value; }
        }
        private ICollection<CommunicationAttachment> _attachments;

        /// <summary>
        /// Gets or sets the data used by the selected communication medium.
        /// </summary>
        /// <value>
        /// A <see cref="System.Collections.Generic.Dictionary{String,String}"/> of key value pairs that contain medium specific data.
        /// </value>
        [DataMember]
        [RockObsolete( "1.7" )]
        [Obsolete( "MediumData is no longer used. Communication now has specific properties for medium data.", true )]
        public virtual Dictionary<string, string> MediumData
        {
            get
            {
                // Get the MediumData from the new property values. This is provided due to the fact that there may be Lava that is 
                // referencing the "MediumData" property of a communication.

                var mediumData = new Dictionary<string, string>();

                switch ( CommunicationType )
                {
                    case CommunicationType.SMS:
                        {
                            mediumData.AddIfNotBlank( "FromValue", SMSFromDefinedValueId.Value.ToString() );
                            mediumData.AddIfNotBlank( "Subject", Subject );
                            mediumData.AddIfNotBlank( "Message", SMSMessage );
                            break;
                        }

                    case CommunicationType.PushNotification:
                        {
                            mediumData.AddIfNotBlank( "Title", PushTitle );
                            mediumData.AddIfNotBlank( "Message", PushMessage );
                            mediumData.AddIfNotBlank( "Sound", PushSound );
                            break;
                        }

                    default:
                        {
                            mediumData.AddIfNotBlank( "FromName", FromName );
                            mediumData.AddIfNotBlank( "FromAddress", FromEmail );
                            mediumData.AddIfNotBlank( "ReplyTo", ReplyToEmail );
                            mediumData.AddIfNotBlank( "CC", CCEmails );
                            mediumData.AddIfNotBlank( "BCC", BCCEmails );
                            mediumData.AddIfNotBlank( "Subject", Subject );
                            mediumData.AddIfNotBlank( "HtmlMessage", Message );
                            mediumData.AddIfNotBlank( "Attachments", GetAttachmentBinaryFileIds( CommunicationType.Email ).AsDelimited( "," ) );
                            break;
                        }
                }

                return mediumData;
            }

            set { }
        }

        /// <summary>
        /// Gets or sets the additional merge field list. When a communication is created
        /// from a grid, the grid may add additional merge fields that will be available
        /// for the communication.
        /// </summary>
        /// <value>
        /// A <see cref="System.Collections.Generic.List{String}"/> of values containing the additional merge field list.
        /// </value>
        [DataMember]
        public virtual List<string> AdditionalMergeFields
        {
            get { return _additionalMergeFields; }
            set { _additionalMergeFields = value; }
        }
        private List<string> _additionalMergeFields = new List<string>();

        /// <summary>
        /// Gets or sets the SMS from defined value.
        /// </summary>
        /// <value>
        /// The SMS from defined value.
        /// </value>
        [DataMember]
        public virtual DefinedValue SMSFromDefinedValue { get; set; }

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

        /// <summary>
        /// /// Gets or sets the Communication Template that was used to compose this communication
        /// </summary>
        /// <value>
        /// The communication template.
        /// </value>
        [DataMember]
        public virtual CommunicationTemplate CommunicationTemplate { get; set; }

        #endregion

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
                return this.CommunicationTemplate ?? base.ParentAuthority;
            }
        }

        #endregion 

        #region Public Methods

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
        /// Gets the attachment binary file ids.
        /// Specify CommunicationType.Email to get the attachments for Email and CommunicationType.SMS to get the Attachment(s) for SMS
        /// </summary>
        /// <param name="communicationType">Type of the communication.</param>
        /// <returns></returns>
        public List<int> GetAttachmentBinaryFileIds( CommunicationType communicationType )
        {
            return this.GetAttachments( communicationType ).Select( a => a.BinaryFileId ).ToList();
        }

        /// <summary>
        /// Returns a medium data value.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> containing the key associated with the value to retrieve. </param>
        /// <returns>A <see cref="System.String"/> representing the value that is linked with the specified key.</returns>
        [RockObsolete( "1.7" )]
        [Obsolete( "MediumData is no longer used", true )]
        public string GetMediumDataValue( string key )
        {
            if ( MediumData.ContainsKey( key ) )
            {
                return MediumData[key];
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Sets a medium data value. If the key exists, the value will be replaced with the new value, otherwise a new key value pair will be added to dictionary.
        /// </summary>
        /// <param name="key">A <see cref="System.String"/> representing the key.</param>
        /// <param name="value">A <see cref="System.String"/> representing the value.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "MediumData is no longer used", true )]
        public void SetMediumDataValue( string key, string value )
        {
            if ( MediumData.ContainsKey( key ) )
            {
                MediumData[key] = value;
            }
            else
            {
                MediumData.Add( key, value );
            }
        }

        /// <summary>
        /// Gets the recipient count.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        [RockObsolete( "1.7.4" )]
        [Obsolete( "This can return incorrect results if Recipients has been modified and not saved to the database. So don't use this.", true )]
        public int GetRecipientCount( RockContext rockContext )
        {
            var count = new CommunicationRecipientService( rockContext ).Queryable().Where( a => a.CommunicationId == this.Id ).Count();

            return count;
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
        public static IQueryable<GroupMember> GetCommunicationListMembers( RockContext rockContext, int? listGroupId, SegmentCriteria segmentCriteria, List<int> segmentDataViewIds)
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
                    List<string> errorMessages;

                    var exp = segmentDataView.GetExpression( personService, paramExpression, out errorMessages );
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

            var emailMediumEntityType = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
            var smsMediumEntityType = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
            var preferredCommunicationTypeAttribute = AttributeCache.Get( SystemGuid.Attribute.GROUPMEMBER_COMMUNICATION_LIST_PREFERRED_COMMUNICATION_MEDIUM.AsGuid() );
            var segmentDataViewGuids = this.Segments.SplitDelimitedValues().AsGuidList();
            var segmentDataViewIds =  new DataViewService( rockContext ).GetByGuids( segmentDataViewGuids ).Select( a => a.Id ).ToList();

            var qryCommunicationListMembers = GetCommunicationListMembers( rockContext, ListGroupId, this.SegmentCriteria, segmentDataViewIds );

            // NOTE: If this is scheduled communication, don't include Members that were added after the scheduled FutureSendDateTime
            if ( this.FutureSendDateTime.HasValue )
            {
                var memberAddedCutoffDate = this.FutureSendDateTime;
                qryCommunicationListMembers = qryCommunicationListMembers.Where( a => ( a.DateTimeAdded.HasValue && a.DateTimeAdded.Value < memberAddedCutoffDate ) || ( a.CreatedDateTime.HasValue && a.CreatedDateTime.Value < memberAddedCutoffDate ) );
            }

            var communicationRecipientService = new CommunicationRecipientService( rockContext );

            var recipientsQry = GetRecipientsQry( rockContext );

            // Get all the List member which is not part of communication recipients yet
            var newMemberInList = qryCommunicationListMembers
                .Where( a => !recipientsQry.Any( r => r.PersonAlias.PersonId == a.PersonId ) )
                .AsNoTracking()
                .ToList();

            foreach ( var newMember in newMemberInList )
            {
                var communicationRecipient = new CommunicationRecipient
                {
                    PersonAliasId = newMember.Person.PrimaryAliasId.Value,
                    Status = CommunicationRecipientStatus.Pending,
                    CommunicationId = Id
                };

                switch ( CommunicationType )
                {
                    case CommunicationType.Email:
                        communicationRecipient.MediumEntityTypeId = emailMediumEntityType.Id;
                        break;
                    case CommunicationType.SMS:
                        communicationRecipient.MediumEntityTypeId = smsMediumEntityType.Id;
                        break;
                    case CommunicationType.RecipientPreference:
                        newMember.LoadAttributes();

                        if ( preferredCommunicationTypeAttribute != null )
                        {
                            var recipientPreference = ( CommunicationType? ) newMember
                                .GetAttributeValue( preferredCommunicationTypeAttribute.Key ).AsIntegerOrNull();

                            switch ( recipientPreference )
                            {
                                case CommunicationType.SMS:
                                    communicationRecipient.MediumEntityTypeId = smsMediumEntityType.Id;
                                    break;
                                case CommunicationType.Email:
                                    communicationRecipient.MediumEntityTypeId = emailMediumEntityType.Id;
                                    break;
                                default:
                                    communicationRecipient.MediumEntityTypeId = emailMediumEntityType.Id;
                                    break;
                            }
                        }

                        break;

                    default:
                        throw new Exception( "Unexpected CommunicationType: " + CommunicationType.ConvertToString() );
                }

                communicationRecipientService.Add( communicationRecipient );
            }

            // Get all pending communication recipents that are no longer part of the group list member, then delete them from the Recipients
            var missingMemberInList = recipientsQry.Where( a => a.Status == CommunicationRecipientStatus.Pending )
                .Where( a => !qryCommunicationListMembers.Any( r => r.PersonId == a.PersonAlias.PersonId ) )
                .ToList();

            foreach ( var missingMember in missingMemberInList )
            {
                communicationRecipientService.Delete( missingMember );
            }

            rockContext.SaveChanges();
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

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Rock.Data.DbContext dbContext )
        {
            // ensure any attachments have the binaryFile.IsTemporary set to False
            var attachmentBinaryFilesIds = this.Attachments.Select( a => a.BinaryFileId ).ToList();
            if ( attachmentBinaryFilesIds.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var temporaryBinaryFiles = new BinaryFileService( rockContext ).GetByIds( attachmentBinaryFilesIds ).Where( a => a.IsTemporary == true ).ToList();
                    {
                        foreach ( var binaryFile in temporaryBinaryFiles )
                        {
                            binaryFile.IsTemporary = false;
                        }
                    }

                    rockContext.SaveChanges();
                }
            }

            base.PostSaveChanges( dbContext );
        }

        #endregion

        #region Private Methods

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

            if ( communication.ListGroupId.HasValue && !communication.SendDateTime.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    communication.RefreshCommunicationRecipientList( rockContext );
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
                recipient = new CommunicationRecipientService( rockContext ).Queryable( "Communication,PersonAlias.Person" )
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

        /// <summary>
        /// Gets the next pending.
        /// </summary>
        /// <param name="communicationId">The communication identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use GetNextPending( int communicationId, int mediumEntityId, Rock.Data.RockContext rockContext ) instead.", true )]
        public static Rock.Model.CommunicationRecipient GetNextPending( int communicationId, Rock.Data.RockContext rockContext )
        {
            CommunicationRecipient recipient = null;

            var delayTime = RockDateTime.Now.AddMinutes( -10 );

            lock ( _obj )
            {
                recipient = new CommunicationRecipientService( rockContext ).Queryable( "Communication,PersonAlias.Person" )
                    .Where( r =>
                        r.CommunicationId == communicationId &&
                        ( r.PersonAlias.Person.IsDeceased == false ) &&
                        ( r.Status == CommunicationRecipientStatus.Pending ||
                            ( r.Status == CommunicationRecipientStatus.Sending && r.ModifiedDateTime < delayTime ) ) )
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


    #region Entity Configuration

    /// <summary>
    /// Communication Configuration class.
    /// </summary>
    public partial class CommunicationConfiguration : EntityTypeConfiguration<Communication>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationConfiguration"/> class.
        /// </summary>
        public CommunicationConfiguration()
        {
            this.HasOptional( c => c.SenderPersonAlias ).WithMany().HasForeignKey( c => c.SenderPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.ReviewerPersonAlias ).WithMany().HasForeignKey( c => c.ReviewerPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.SMSFromDefinedValue ).WithMany().HasForeignKey( c => c.SMSFromDefinedValueId ).WillCascadeOnDelete( false );

            // the Migration will manually add a ON DELETE SET NULL for ListGroupId
            this.HasOptional( c => c.ListGroup ).WithMany().HasForeignKey( c => c.ListGroupId ).WillCascadeOnDelete( false );

            // the Migration will manually add a ON DELETE SET NULL for CommunicationTemplateId
            this.HasOptional( c => c.CommunicationTemplate ).WithMany().HasForeignKey( c => c.CommunicationTemplateId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The status of a communication
    /// </summary>
    public enum CommunicationStatus
    {
        /// <summary>
        /// Communication was created, but not yet edited by a user. (i.e. from data grid or report)
        /// Transient communications more than a few hours old may be deleted by clean-up job.
        /// </summary>
        Transient = 0,

        /// <summary>
        /// Communication is currently being drafted
        /// </summary>
        Draft = 1,

        /// <summary>
        /// Communication has been submitted but not yet approved or denied
        /// </summary>
        PendingApproval = 2,

        /// <summary>
        /// Communication has been approved for sending
        /// </summary>
        Approved = 3,

        /// <summary>
        /// Communication has been denied
        /// </summary>
        Denied = 4,

    }

    /// <summary>
    /// Type of communication
    /// </summary>
    public enum CommunicationType
    {
        /// <summary>
        /// RecipientPreference
        /// </summary>
        RecipientPreference = 0,

        /// <summary>
        /// Email
        /// </summary>
        Email = 1,

        /// <summary>
        /// SMS
        /// </summary>
        SMS = 2,

        /// <summary>
        /// Push notification
        /// </summary>
        PushNotification = 3,

        /// <summary>
        /// Some other communication type
        /// </summary>
        [RockObsolete( "1.7" )]
        [Obsolete( "Not Supported" )]
        Other = 4
    }

    /// <summary>
    /// Flag indicating if communication is for all selected segments or any segments
    /// </summary>
    public enum SegmentCriteria
    {
        /// <summary>
        /// All
        /// </summary>
        All = 0,

        /// <summary>
        /// Any
        /// </summary>
        Any = 1,
    }

    #endregion
}

