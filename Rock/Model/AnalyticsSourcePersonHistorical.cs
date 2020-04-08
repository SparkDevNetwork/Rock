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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents the source record for the AnalyticsDimPersonHistorical and AnalyticsDimPersonCurrent views.
    /// NOTE: Rock.Jobs.ProcessBIAnalytics dynamically adds additional columns to this table for any Attribute that is marked for Analytics
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsSourcePersonHistorical" )]
    [DataContract]
    [HideFromReporting]
    public class AnalyticsSourcePersonHistorical : AnalyticsSourcePersonBase<AnalyticsSourcePersonHistorical>
    {
        // intentionally blank
    }

    /// <summary>
    /// AnalyticsSourcePersonHistorical is a real table, and AnalyticsDimPersonHistorical and AnalyticsDimPersonCurrent are VIEWs off of AnalyticsSourcePersonHistorical, so they share lots of columns
    /// </summary>
    [RockDomain( "Reporting" )]
    public abstract class AnalyticsSourcePersonBase<T> : Entity<T>
        where T : AnalyticsSourcePersonBase<T>, new()
    {
        #region Entity Properties specific to Analytics

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        [DataMember]
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [current row indicator].
        /// This will be True if this represents the same values as the current Rock.Model.Person record for this person
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current row indicator]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CurrentRowIndicator { get; set; }

        /// <summary>
        /// Gets or sets the effective date.
        /// This is the starting date that the person record had the values reflected in this record
        /// </summary>
        /// <value>
        /// The effective date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets the expire date.
        /// This is the last date that the person record had the values reflected in this record
        /// For example, if a person's LastName changed on '2016-07-14', the ExpireDate of the previously current record will be '2016-07-13', and the EffectiveDate of the current record will be '2016-07-14'
        /// If this is most current record, the ExpireDate will be '9999-01-01'
        /// </summary>
        /// <value>
        /// The expire date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// Gets or sets the primary family identifier.
        /// </summary>
        /// <value>
        /// The primary family identifier.
        /// </value>
        [AnalyticHistoryField]
        [DataMember]
        public int? PrimaryFamilyId { get; set; }

        /// <summary>
        /// Gets or sets the count.
        /// NOTE: this always has a hardcoded value of 1. It is stored in the table because it is supposed to help do certain types of things in analytics
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        [DataMember]
        public int Count { get; set; } = 1;

        #endregion

        #region Entity Properties from Rock.Model.Person

        /// <summary>
        /// Gets or sets the Id of the Person Record Type <see cref="Rock.Model.DefinedValue" /> representing what type of Person Record this is.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.DefinedValue"/> identifying the person record type. If no value is selected this can be null.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_RECORD_TYPE )]
        public int? RecordTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Record Status <see cref="Rock.Model.DefinedValue"/> representing the status of this entity
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Record Status <see cref="Rock.Model.DefinedValue"/> representing the status of this entity.
        /// </value>
        [AnalyticHistoryField]
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_RECORD_STATUS )]
        public int? RecordStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the record status last modified date time.
        /// </summary>
        /// <value>
        /// The record status last modified date time.
        /// </value>
        [DataMember]
        public DateTime? RecordStatusLastModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Record Status Reason <see cref="Rock.Model.DefinedValue"/> representing the reason why a person record status would have a set status.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Record Status Reason <see cref="Rock.Model.DefinedValue"/> representing the reason why a person entity would have a set status.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON )]
        public int? RecordStatusReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Defined Value <see cref="Rock.Model.DefinedValue"/> representing the connection status of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the connection status of the Person.
        /// </value>
        [AnalyticHistoryField]
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS )]
        public int? ConnectionStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Defined Value <see cref="Rock.Model.DefinedValue"/> representing the reason a record needs to be reviewed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the reason a record needs to be reviewed.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_REVIEW_REASON )]
        public int? ReviewReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Person is deceased.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the Person is deceased; otherwise <c>false</c>.
        /// </value>
        [AnalyticHistoryField]
        [DataMember]
        public bool IsDeceased { get; set; }

        /// <summary>
        /// Gets or sets Id of the (Salutation) Tile <see cref="Rock.Model.DefinedValue"/> that is associated with the Person
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Title <see cref="Rock.Model.DefinedValue"/> that is associated with the Person.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_TITLE )]
        public int? TitleValueId { get; set; }

        /// <summary>
        /// Gets or sets the first name of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the first name of the Person.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the nick name of the Person.  If a nickname was not entered, the first name is used.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the nick name of the Person.
        /// </value>
        /// <remarks>
        /// The name that the person goes by.
        /// </remarks>
        [MaxLength( 50 )]
        [DataMember]
        [Previewable]
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the middle name of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the middle name of the Person.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the last name (Sir Name) of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Last Name of the Person.
        /// </value>
        [AnalyticHistoryField]
        [MaxLength( 50 )]
        [DataMember]
        [Previewable]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Person's name Suffix <see cref="Rock.Model.DefinedValue"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Person's name Suffix <see cref="Rock.Model.DefinedValue"/>. If the Person
        /// does not have a suffix as part of their name this value will be null.
        /// </value>
        /// <remarks>
        /// Examples include: Sr., Jr., III, IV, DMD,  MD, PhD, etc.
        /// </remarks>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_SUFFIX )]
        public int? SuffixValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.BinaryFile"/> that contains the photo of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.BinaryFile"/> containing the photo of the Person.
        /// </value>
        [DataMember]
        public int? PhotoId { get; set; }

        /// <summary>
        /// Gets or sets the day of the month portion of the Person's birth date.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the day of the month portion of the Person's birth date. If their birth date is not known
        /// this value will be null.
        /// </value>
        [DataMember]
        public int? BirthDay { get; set; }

        /// <summary>
        /// Gets or sets the month portion of the Person's birth date.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the month portion of the Person's birth date. If the birth date is not known this value will be null.
        /// </value>
        [DataMember]
        public int? BirthMonth { get; set; }

        /// <summary>
        /// Gets or sets the year portion of the Person's birth date.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the year portion of the Person's birth date. If the birth date is not known this value will be null.
        /// </value>
        [DataMember]
        public int? BirthYear { get; set; }

        /// <summary>
        /// Gets or sets the Birth date key which is the form YYYYMMDD
        /// </summary>
        /// <value>
        /// The birth date key.
        /// </value>
        [DataMember]
        public int? BirthDateKey { get; set; }

        /// <summary>
        /// Gets the Person's age.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the person's age. Returns null if the birthdate or birthyear is not available.
        /// </value>
        [DataMember]
        public int? Age { get; set; }

        /// <summary>
        /// Gets or sets the gender of the Person. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.Gender"/> enum value representing the Person's gender.  Valid values are <c>Gender.Unknown</c> if the Person's gender is unknown,
        /// <c>Gender.Male</c> if the Person's gender is Male, <c>Gender.Female</c> if the Person's gender is Female.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets Id of the Marital Status <see cref="Rock.Model.DefinedValue"/> representing the Person's martial status.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Marital Status <see cref="Rock.Model.DefinedValue"/> representing the Person's martial status.  This value is nullable.
        /// </value>
        [AnalyticHistoryField]
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_MARITAL_STATUS )]
        public int? MaritalStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the date of the Person's wedding anniversary.  This property is nullable if the Person is not married or their anniversary date is not known.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the anniversary date of the Person's wedding. If the anniversary date is not known or they are not married this value will be null.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? AnniversaryDate { get; set; }

        /// <summary>
        /// Gets or sets the date of the Person's projected or actual high school graduation year. This value is used to determine what grade a student is in.
        /// </summary>
        /// <value>
        /// The Person's projected or actual high school graduation year
        /// </value>
        [DataMember]
        public int? GraduationYear { get; set; }

        /// <summary>
        /// Gets or sets the giving group id.  If an individual would like their giving to be grouped with the rest of their family,
        /// this will be the id of their family group.  If they elect to contribute on their own, this value will be null.
        /// </summary>
        /// <value>
        /// The giving group id.
        /// </value>
        [AnalyticHistoryField]
        [DataMember]
        [HideFromReporting]
        public int? GivingGroupId { get; set; }

        /// <summary>
        /// Gets the giver identifier in the format G{GivingGroupId} if they are part of a GivingGroup, or P{Personid} if they give individually
        /// </summary>
        /// <value>
        /// The giver identifier.
        /// </value>
        [DataMember]
        public string GivingId { get; set; }

        /// <summary>
        /// Gets or sets the giving leader identifier. 
        /// </summary>
        /// <value>
        /// The giving leader identifier.
        /// </value>
        [DataMember]
        public int? GivingLeaderId { get; set; }

        /// <summary>
        /// Gets or sets the Person's email address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the Person's email address.
        /// </value>
        [AnalyticHistoryField]
        [MaxLength( 75 )]
        [DataMember]
        [Previewable]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the email preference.
        /// </summary>
        /// <value>
        /// The email preference.
        /// </value>
        [AnalyticHistoryField]
        [DataMember]
        public EmailPreference EmailPreference { get; set; }

        /// <summary>
        /// Gets or sets notes about why a person profile needs to be reviewed
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing an Review Reason Note.
        /// </value>
        [MaxLength( 1000 )]
        [DataMember]
        public string ReviewReasonNote { get; set; }

        /// <summary>
        /// Gets or sets the Inactive Reason Note
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing an Inactive Reason Note.
        /// </value>
        [MaxLength( 1000 )]
        [DataMember]
        public string InactiveReasonNote { get; set; }

        /// <summary>
        /// Gets or sets the System Note
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a System Note.
        /// </value>
        [MaxLength( 1000 )]
        [DataMember]
        public string SystemNote { get; set; }

        /// <summary>
        /// Gets or sets the count of the number of times that the Person has been viewed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the number of times that the Person has been viewed.
        /// </value>
        [DataMember]
        public int? ViewedCount { get; set; }

        #endregion
        
        #region Virtual
        
        /// <summary>
        /// Gets or sets the birth date dim.
        /// </summary>
        /// <value>
        /// The birth date dim.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate BirthDateDim { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AnalyticsSourcePersonHistoricalConfiguration : EntityTypeConfiguration<AnalyticsSourcePersonHistorical>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsSourcePersonHistoricalConfiguration"/> class.
        /// </summary>
        public AnalyticsSourcePersonHistoricalConfiguration()
        {
            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier birthdates 
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasOptional( t => t.BirthDateDim ).WithMany().HasForeignKey( t => t.BirthDateKey ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
