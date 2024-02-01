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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// The person doing the registration. For example, Dad signing his kids up for camp. Dad is the Registration person and the kids would be Registrants
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "Registration" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.REGISTRATION )]
    public partial class Registration : Model<Registration>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        [Required]
        [DataMember]
        [IgnoreCanDelete]
        public int RegistrationInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the confirmation email.
        /// </summary>
        /// <value>
        /// The confirmation email.
        /// </value>
        [MaxLength( 75 )]
        [DataMember]
        public string ConfirmationEmail { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string DiscountCode { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage.
        /// </summary>
        /// <value>
        /// The discount percentage.
        /// </value>
        [DataMember]
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount.
        /// </summary>
        /// <value>
        /// The discount amount.
        /// </value>
        [DataMember]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is temporary.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is temporary; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsTemporary { get; set; }

        /// <summary>
        /// Gets or sets the last payment reminder date time.
        /// </summary>
        /// <value>
        /// The last payment reminder date time.
        /// </value>
        [DataMember]
        public DateTime? LastPaymentReminderDateTime
        {
            get
            {
                return _lastPaymentReminderDateTime.HasValue ? _lastPaymentReminderDateTime : this.CreatedDateTime;
            }

            set
            {
                _lastPaymentReminderDateTime = value;
            }
        }

        private DateTime? _lastPaymentReminderDateTime;

        /// <summary>
        /// Gets the created date key.
        /// </summary>
        /// <value>
        /// The created date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? CreatedDateKey
        {
            get => ( CreatedDateTime == null || CreatedDateTime.Value == default ) ?
                        ( int? ) null :
                        CreatedDateTime.Value.ToString( "yyyyMMdd" ).AsInteger();

            private set { }
        }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Campus"/> the registration will be tied to
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Campus"/> the event occured
        /// </value>
        [DataMember]
        [FieldType(Rock.SystemGuid.FieldType.CAMPUS)]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the payment plan <see cref="Rock.Model.FinancialScheduledTransaction"/> identifier.
        /// </summary>
        /// <value>
        /// The payment plan <see cref="Rock.Model.FinancialScheduledTransaction"/> identifier.
        /// </value>
        [DataMember]
        public int? PaymentPlanFinancialScheduledTransactionId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the registration instance.
        /// </summary>
        /// <value>
        /// The registration instance.
        /// </value>
        [DataMember]
        public virtual RegistrationInstance RegistrationInstance { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/>.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [LavaVisible]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the created source date.
        /// </summary>
        /// <value>
        /// The created source date.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate CreatedSourceDate { get; set; }

        /// <summary>
        /// Gets or sets the registrants.
        /// </summary>
        /// <value>
        /// The registrants.
        /// </value>
        [DataMember]
        public virtual ICollection<RegistrationRegistrant> Registrants
        {
            get { return _registrants ?? ( _registrants = new Collection<RegistrationRegistrant>() ); }
            set { _registrants = value; }
        }

        private ICollection<RegistrationRegistrant> _registrants;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> the registration will be tied to
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> where the <see cref="Rock.Model.Person"/> attended.
        /// </value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the payment plan <see cref="Rock.Model.FinancialScheduledTransaction"/>.
        /// </summary>
        /// <value>
        /// The payment plan <see cref="Rock.Model.FinancialScheduledTransaction"/>.
        /// </value>
        [DataMember]
        public virtual FinancialScheduledTransaction PaymentPlanFinancialScheduledTransaction { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationConfiguration : EntityTypeConfiguration<Registration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationConfiguration"/> class.
        /// </summary>
        public RegistrationConfiguration()
        {
            this.HasRequired( r => r.RegistrationInstance ).WithMany( t => t.Registrations ).HasForeignKey( r => r.RegistrationInstanceId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.Group ).WithMany().HasForeignKey( r => r.GroupId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( true );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier OccurrenceDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasOptional( r => r.CreatedSourceDate ).WithMany().HasForeignKey( r => r.CreatedDateKey ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
