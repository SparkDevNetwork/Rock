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
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents an <see cref="Rock.Model.WorkflowActionForm"/> (action or task) that is performed as part of a <see cref="Rock.Model.WorkflowActionForm"/>.
    /// </summary>
    [RockDomain( "Workflow" )]
    [Table( "WorkflowActionForm" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "FDAB9AEB-B2AA-4FB5-A35D-83254A9B014C")]
    public partial class WorkflowActionForm : Model<WorkflowActionForm>, ICacheable, IHasAdditionalSettings
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the notification system communication identifier.
        /// </summary>
        /// <value>
        /// The notification system communication identifier.
        /// </value>
        [DataMember]
        public int? NotificationSystemCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the notification system email identifier.
        /// </summary>
        /// <value>
        /// The notification system email identifier.
        /// </value>
        [DataMember]
        [Obsolete( "Use NotificationSystemCommunicationId instead.", true )]
        [RockObsolete( "1.10" )]
        public int? NotificationSystemEmailId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include actions in notification].
        /// </summary>
        /// <value>
        /// <c>true</c> if [include actions in notification]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IncludeActionsInNotification { get; set; } = true;

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        [DataMember]
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the footer.
        /// </summary>
        /// <value>
        /// The footer.
        /// </value>
        [DataMember]
        public string Footer { get; set; }

        /// <summary>
        /// Gets or sets the delimited list of action buttons and actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        [MaxLength( 2000 )]
        [DataMember]
        public string Actions { get; set; }

        /// <summary>
        /// An optional text attribute that will be updated with the action that was selected
        /// </summary>
        /// <value>
        /// The action attribute unique identifier.
        /// </value>
        [DataMember]
        public Guid? ActionAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets whether Notes can be entered
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow notes entry]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool? AllowNotes { get; set; }

        #region Person entry related Entity Properties

        /// <summary>
        /// Gets or sets a value indicating whether a new person (and spouse) can be added
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow person entry]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowPersonEntry { get; set; } = false;

        /// <summary>
        /// Gets or sets the person entry preHTML.
        /// </summary>
        /// <value>
        /// The person entry preHTML.
        /// </value>
        [DataMember]
        public string PersonEntryPreHtml { get; set; }

        /// <summary>
        /// Gets or sets the person entry post HTML.
        /// </summary>
        /// <value>
        /// The person entry post HTML.
        /// </value>
        [DataMember]
        public string PersonEntryPostHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [person entry show campus].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [person entry show campus]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PersonEntryCampusIsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether Person Entry should auto-fill with the CurrentPerson
        /// </summary>
        /// <value>
        ///   <c>true</c> if [person entry auto-fill current person]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PersonEntryAutofillCurrentPerson { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether Person Entry should be hidden if the CurrentPerson is known
        /// </summary>
        /// <value>
        ///   <c>true</c> if [person entry hide if current person known]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PersonEntryHideIfCurrentPersonKnown { get; set; } = false;

        /// <summary>
        /// Gets or sets the person entry spouse entry option.
        /// </summary>
        /// <value>
        /// The person entry spouse entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntrySpouseEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry gender entry option.
        /// </summary>
        /// <value>
        /// The person entry gender entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryGenderEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Required;

        /// <summary>
        /// Gets or sets the person entry email entry option.
        /// </summary>
        /// <value>
        /// The person entry email entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryEmailEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Required;

        /// <summary>
        /// Gets or sets the person entry mobile phone entry option.
        /// </summary>
        /// <value>
        /// The person entry mobile phone entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryMobilePhoneEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry SMS opt in entry option.
        /// </summary>
        /// <value>
        /// The person entry SMS opt in entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormShowHideOption PersonEntrySmsOptInEntryOption { get; set; } = WorkflowActionFormShowHideOption.Hide;

        /// <summary>
        /// Gets or sets the person entry birthdate entry option.
        /// </summary>
        /// <value>
        /// The person entry birthdate entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryBirthdateEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry address entry option.
        /// </summary>
        /// <value>
        /// The person entry address entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryAddressEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry marital status entry option.
        /// </summary>
        /// <value>
        /// The person entry marital entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryMaritalStatusEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry race entry option.
        /// </summary>
        /// <value>
        /// The person entry marital entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryRaceEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry ethnicity entry option.
        /// </summary>
        /// <value>
        /// The person entry marital entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryEthnicityEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry spouse label.
        /// </summary>
        /// <value>
        /// The person entry spouse label.
        /// </value>
        [MaxLength( 50 )]
        [DataMember( IsRequired = false )]
        public string PersonEntrySpouseLabel { get; set; } = "Spouse";

        /// <summary>
        /// Gets or sets the person entry connection status value identifier.
        /// </summary>
        /// <value>
        /// The person entry connection status value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS )]
        public int? PersonEntryConnectionStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the person entry record status value identifier.
        /// </summary>
        /// <value>
        /// The person entry record status value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS )]
        public int? PersonEntryRecordStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the person entry address type value identifier.
        /// </summary>
        /// <value>
        /// The person entry address type value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE )]
        public int? PersonEntryGroupLocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the person entry campus status value identifier.
        /// This and <seealso cref="PersonEntryCampusTypeValueId"/> will determine which campuses will selectable
        /// </summary>
        /// <value>
        /// The person entry campus status value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( Rock.SystemGuid.DefinedType.CAMPUS_STATUS )]
        public int? PersonEntryCampusStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the person entry campus type value identifier.
        /// This and <seealso cref="PersonEntryCampusStatusValueId"/> will determine which campuses will selectable
        /// </summary>
        /// <value>
        /// The person entry campus type value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( Rock.SystemGuid.DefinedType.CAMPUS_TYPE )]
        public int? PersonEntryCampusTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the person entry person workflow attribute unique identifier. (The one used to set the Added/Edited Person to)
        /// </summary>
        /// <value>
        /// The person entry person attribute unique identifier.
        /// </value>
        [DataMember]
        public Guid? PersonEntryPersonAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the person entry spouse workflow attribute unique identifier.  (The one used to set the Added/Edited Person's Spouse to)
        /// </summary>
        /// <value>
        /// The person entry spouse attribute unique identifier.
        /// </value>
        [DataMember]
        public Guid? PersonEntrySpouseAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the person entry family attribute unique identifier. (The one used to set the Added/Edited Person's Family to)
        /// </summary>
        /// <value>
        /// The person entry family attribute unique identifier.
        /// </value>
        [DataMember]
        public Guid? PersonEntryFamilyAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the <see cref="Rock.Model.DefinedValue"/> that represents the SectionType for the Person Entry Section.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing DefinedValueId of the SectionType's <see cref="Rock.Model.DefinedValue"/> for the Person Entry Section.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.SECTION_TYPE )]
        public int? PersonEntrySectionTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the Title to display at the top the Person Entry Section
        /// </summary>
        /// <value>
        /// The person entry title.
        /// </value>
        [DataMember]
        [MaxLength( 500 )]
        public string PersonEntryTitle { get; set; }

        /// <summary>
        /// Gets or sets the Description to display under the <see cref="PersonEntryTitle"/>
        /// </summary>
        /// <value>
        /// The person entry description.
        /// </value>
        [DataMember]
        public string PersonEntryDescription { get; set; }

        /// <summary>
        /// Gets or sets whether a heading separator should be display under the <see cref="PersonEntryTitle"/> and <see cref="PersonEntryDescription" />
        /// </summary>
        /// <value>
        ///   <c>true</c> if [person entry show heading separator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PersonEntryShowHeadingSeparator { get; set; }

        #endregion Person entry related Entity Properties

        /// <inheritdoc/>
        [RockInternal( "1.16.6" )]
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the form attributes.
        /// </summary>
        /// <value>
        /// The form attributes.
        /// </value>
        [DataMember]
        public virtual ICollection<WorkflowActionFormSection> FormSections
        {
            get { return _formSections ?? ( _formSections = new Collection<WorkflowActionFormSection>() ); }
            set { _formSections = value; }
        }

        private ICollection<WorkflowActionFormSection> _formSections;

        /// <summary>
        /// Gets or sets the form attributes.
        /// </summary>
        /// <value>
        /// The form attributes.
        /// </value>
        [DataMember]
        public virtual ICollection<WorkflowActionFormAttribute> FormAttributes
        {
            get { return _formAttributes ?? ( _formAttributes = new Collection<WorkflowActionFormAttribute>() ); }
            set { _formAttributes = value; }
        }

        private ICollection<WorkflowActionFormAttribute> _formAttributes;

        /// <summary>
        /// Gets or sets the notification system email.
        /// </summary>
        /// <value>
        /// The notification system email.
        /// </value>
        [LavaVisible]
        [Obsolete( "Use NotificationSystemCommunication instead.", true )]
        [RockObsolete( "1.10" )]
        public virtual SystemEmail NotificationSystemEmail { get; set; }

        /// <summary>
        /// Gets or sets the notification system communication.
        /// </summary>
        /// <value>
        /// The notification system communication.
        /// </value>
        [LavaVisible]
        public virtual SystemCommunication NotificationSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the person entry connection status value
        /// </summary>
        /// <value>
        /// The person entry connection status value
        /// </value>
        [DataMember]
        public virtual DefinedValue PersonEntryConnectionStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the person entry record status value identifier.
        /// </summary>
        /// <value>
        /// The person entry record status value identifier.
        /// </value>
        [DataMember]
        public virtual DefinedValue PersonEntryRecordStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the person entry address type value identifier.
        /// </summary>
        /// <value>
        /// The person entry address type value identifier.
        /// </value>
        [DataMember]
        public virtual DefinedValue PersonEntryGroupLocationTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the person entry campus status value.
        /// </summary>
        /// <value>
        /// The person entry campus status value.
        /// </value>
        [DataMember]
        public virtual DefinedValue PersonEntryCampusStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the person entry campus type value.
        /// </summary>
        /// <value>
        /// The person entry campus type value.
        /// </value>
        [DataMember]
        public virtual DefinedValue PersonEntryCampusTypeValue{ get; set; }

        /// <summary>
        /// Gets or sets the person entry section type value.
        /// </summary>
        /// <value>The person entry section type value.</value>
        [DataMember]
        public virtual DefinedValue PersonEntrySectionTypeValue { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Workflow Form Configuration class.
    /// </summary>
    public partial class WorkflowActionFormConfiguration : EntityTypeConfiguration<WorkflowActionForm>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionFormConfiguration"/> class.
        /// </summary>
        public WorkflowActionFormConfiguration()
        {
            this.HasOptional( f => f.NotificationSystemCommunication ).WithMany().HasForeignKey( f => f.NotificationSystemCommunicationId ).WillCascadeOnDelete( false );

            this.HasOptional( f => f.PersonEntryConnectionStatusValue ).WithMany().HasForeignKey( f => f.PersonEntryConnectionStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( f => f.PersonEntryRecordStatusValue ).WithMany().HasForeignKey( f => f.PersonEntryRecordStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( f => f.PersonEntryGroupLocationTypeValue ).WithMany().HasForeignKey( f => f.PersonEntryGroupLocationTypeValueId ).WillCascadeOnDelete( false );

            this.HasOptional( f => f.PersonEntryCampusStatusValue).WithMany().HasForeignKey( f => f.PersonEntryCampusStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( f => f.PersonEntryCampusTypeValue ).WithMany().HasForeignKey( f => f.PersonEntryCampusTypeValueId ).WillCascadeOnDelete( false );

            this.HasOptional( f => f.PersonEntrySectionTypeValue ).WithMany().HasForeignKey( f => f.PersonEntrySectionTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}