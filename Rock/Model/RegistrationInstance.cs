// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "RegistrationInstance" )]
    [DataContract]
    public partial class RegistrationInstance : Model<RegistrationInstance>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the registration template identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        [Required]
        [DataMember]
        public int RegistrationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        [DataMember]
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>
        /// The end date time.
        /// </value>
        [DataMember]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the send reminder date time.
        /// </summary>
        /// <value>
        /// The send reminder date time.
        /// </value>
        [DataMember]
        public DateTime? SendReminderDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [reminder sent].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [reminder sent]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ReminderSent { get; set; }

        /// <summary>
        /// Gets or sets the details.
        /// </summary>
        /// <value>
        /// The details.
        /// </value>
        [DataMember]
        public string Details { get; set; }

        /// <summary>
        /// Gets or sets the maximum attendees.
        /// </summary>
        /// <value>
        /// The maximum attendees.
        /// </value>
        [Required]
        [DataMember]
        public int MaxAttendees { get; set; }

        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        [DataMember]
        public int? AccountId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name of the contact.
        /// </summary>
        /// <value>
        /// The name of the contact.
        /// </value>
        [DataMember]
        public int? ContactPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the contact phone.
        /// </summary>
        /// <value>
        /// The contact phone.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string ContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the contact email.
        /// </summary>
        /// <value>
        /// The contact email.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string ContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the additional reminder details.
        /// </summary>
        /// <value>
        /// The additional reminder details.
        /// </value>
        [DataMember]
        public string AdditionalReminderDetails { get; set; }

        /// <summary>
        /// Gets or sets the additional confirmation details.
        /// </summary>
        /// <value>
        /// The additional confirmation details.
        /// </value>
        [DataMember]
        public string AdditionalConfirmationDetails { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the registration template.
        /// </summary>
        /// <value>
        /// The registration template.
        /// </value>
        [DataMember]
        public virtual RegistrationTemplate RegistrationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>
        /// The account.
        /// </value>
        [DataMember]
        public virtual FinancialAccount Account { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> representing the personalias who is the contact person.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PersonAlias"/> representing the personalias who is the contact person.
        /// </value>
        [DataMember]
        public virtual PersonAlias ContactPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the registrations.
        /// </summary>
        /// <value>
        /// The registrations.
        /// </value>
        public virtual ICollection<Registration> Registrations
        {
            get { return _registrations ?? ( _registrations = new Collection<Registration>() ); }
            set { _registrations = value; }
        }
        private ICollection<Registration> _registrations;

        /// <summary>
        /// Gets or sets the linkages.
        /// </summary>
        /// <value>
        /// The linkages.
        /// </value>
        public virtual ICollection<EventItemOccurrenceGroupMap> Linkages
        {
            get { return _linkages ?? ( _linkages = new Collection<EventItemOccurrenceGroupMap>() ); }
            set { _linkages = value; }
        }
        private ICollection<EventItemOccurrenceGroupMap> _linkages;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority
        {
            get
            {
                return RegistrationTemplate != null ? RegistrationTemplate : base.ParentAuthority;
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationInstanceConfiguration : EntityTypeConfiguration<RegistrationInstance>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationInstanceConfiguration"/> class.
        /// </summary>
        public RegistrationInstanceConfiguration()
        {
            this.HasRequired( i => i.RegistrationTemplate ).WithMany( t => t.Instances ).HasForeignKey( i => i.RegistrationTemplateId ).WillCascadeOnDelete( true );
            this.HasOptional( i => i.Account ).WithMany().HasForeignKey( i => i.AccountId ).WillCascadeOnDelete( false );
            this.HasOptional( i => i.ContactPersonAlias ).WithMany().HasForeignKey( i => i.ContactPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
