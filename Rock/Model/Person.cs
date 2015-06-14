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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a person or a business in Rock.
    /// </summary>
    [Table( "Person" )]
    [DataContract]
    public partial class Person : Model<Person>
    {
        #region Constants

        /// <summary>
        /// The Entity Type used for saving user values
        /// </summary>
        public const string USER_VALUE_ENTITY = "Rock.Model.Person.Value";

        #endregion

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Person is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this Person is part of the Rock core system/framework. This property is required.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

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
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_RECORD_STATUS )]
        public int? RecordStatusValueId { get; set; }

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
        [DataMember]
        public bool? IsDeceased
        {
            get
            {
                return _isDeceased;
            }
            set
            {
                if ( value.HasValue )
                {
                    _isDeceased = value.Value;
                }
                else
                {
                    _isDeceased = false;
                }
            }
        }

        private bool _isDeceased = false;

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
        /// A <see cref="System.Int32"/> representing the Id of the Marital STatus <see cref="Rock.Model.DefinedValue"/> representing the Person's martial status.  This value is nullable.
        /// </value>
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
        [DataMember]
        [HideFromReporting]
        public int? GivingGroupId { get; set; }

        /// <summary>
        /// Gets the giver identifier.
        /// </summary>
        /// <value>
        /// The giver identifier.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        public string GivingId
        {
            get
            {
                // NOTE: This is the In-Memory get, LinqToSql will get the value from the database
                return GivingGroupId.HasValue ?
                    string.Format( "G{0}", GivingGroupId.Value ) :
                    string.Format( "P{0}", Id );
            }

            private set
            {
                // don't do anthing here since EF uses this for loading 
            }
        }

        /// <summary>
        /// Gets or sets the giving leader identifier. This is a computed column and can be used
        /// in LinqToSql queries, but there is no in-memory calculation. Avoid using property outside
        /// a linq query
        /// </summary>
        /// <value>
        /// The giving leader identifier.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        public int GivingLeaderId { get; set; }
   
        /// <summary>
        /// Gets or sets the Person's email address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the Person's email address.
        /// </value>
        [MaxLength( 75 )]
        [DataMember]
        [Previewable]
        [RegularExpression( @"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+", ErrorMessage = "The Email address is invalid" )]
        [Index( "IX_Email" )]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Person's email address is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the email address is active, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsEmailActive { get; set; }

        /// <summary>
        /// Gets or sets a note about the Person's email address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a note about the Person's email address.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string EmailNote { get; set; }

        /// <summary>
        /// Gets or sets the email preference.
        /// </summary>
        /// <value>
        /// The email preference.
        /// </value>
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

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        public Person()
            : base()
        {
            _users = new Collection<UserLogin>();
            _phoneNumbers = new Collection<PhoneNumber>();
            _members = new Collection<GroupMember>();
            _aliases = new Collection<PersonAlias>();
        }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets the primary alias.
        /// </summary>
        /// <value>
        /// The primary alias.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual PersonAlias PrimaryAlias
        {
            get
            {
                return Aliases.FirstOrDefault( a => a.AliasPersonId == Id );
            }
        }

        /// <summary>
        /// Gets the primary alias identifier.
        /// </summary>
        /// <value>
        /// The primary alias identifier.
        /// </value>
        [DataMember]
        [NotMapped]
        [RockClientInclude( "The Primary PersonAliasId of the Person" )]
        public virtual int? PrimaryAliasId
        {
            get
            {
                var primaryAlias = PrimaryAlias;
                if ( primaryAlias != null )
                {
                    return primaryAlias.Id;
                }

                return null;
            }
            private set { }
        }

        /// <summary>
        /// Gets the Full Name of the Person using the NickName LastName Suffix format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Full Name of a Person using the NickName LastName Suffix format.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string FullName
        {
            get
            {
                // Use the SuffixValueId and DefinedValue cache instead of referencing SuffixValue property so 
                // that if FullName is used in datagrid, the SuffixValue is not lazy-loaded for each row
                return FormatFullName( NickName, LastName, SuffixValueId );
            }

            private set { }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is business.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is business; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        private bool IsBusiness
        {
            get
            {
                int recordTypeValueIdBusiness = DefinedValueCache.Read( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
                return this.RecordTypeValueId.HasValue && this.RecordTypeValueId == recordTypeValueIdBusiness;
            }
        }

        /// <summary>
        /// Gets the full name of the Person using the LastName, FirstName format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full name of a Person using the LastName, FirstName format
        /// </value>
        [NotMapped]
        public virtual string FullNameReversed
        {
            get
            {
                if ( this.IsBusiness )
                {
                    return LastName;
                }

                var fullName = new StringBuilder();

                fullName.Append( LastName );

                // Use the SuffixValueId and DefinedValue cache instead of referencing SuffixValue property so 
                // that if FullName is used in datagrid, the SuffixValue is not lazy-loaded for each row
                if ( SuffixValueId.HasValue )
                {
                    var suffix = DefinedValueCache.Read( SuffixValueId.Value );
                    if ( suffix != null )
                    {
                        fullName.AppendFormat( " {0}", suffix.Value );
                    }
                }

                fullName.AppendFormat( ", {0}", NickName );
                return fullName.ToString();
            }
        }

        /// <summary>
        /// Gets the Full Name of the Person using the Title FirstName LastName format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Full Name of a Person using the Title FirstName LastName format.
        /// </value>
        [NotMapped]
        public virtual string FullNameFormal
        {
            get
            {
                if ( this.IsBusiness )
                {
                    return LastName;
                }

                var fullName = new StringBuilder();

                fullName.AppendFormat( "{0} {1}", FirstName, LastName );

                if ( SuffixValue != null && !string.IsNullOrWhiteSpace( SuffixValue.Value ) )
                    fullName.AppendFormat( " {0}", SuffixValue.Value );

                return fullName.ToString();
            }
        }

        /// <summary>
        /// Gets the full name of the Person using the LastName, FirstName format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full name of a Person using the LastName, FirstName format
        /// </value>
        [NotMapped]
        public virtual string FullNameFormalReversed
        {
            get
            {
                if ( this.IsBusiness )
                {
                    return LastName;
                }

                var fullName = new StringBuilder();
                fullName.Append( LastName );

                if ( SuffixValue != null && !string.IsNullOrWhiteSpace( SuffixValue.Value ) )
                    fullName.AppendFormat( " {0}", SuffixValue.Value );

                fullName.AppendFormat( ", {0}", FirstName );
                return fullName.ToString();
            }
        }

        /// <summary>
        /// Gets the day of the week the person's birthday falls on for the current year.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the day of the week the person's birthday falls on for the current year.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string BirthdayDayOfWeek
        {
            get
            {
                string birthdayDayOfWeek = string.Empty;

                if ( BirthMonth.HasValue && BirthDay.HasValue )
                {
                    try
                    {
                        DateTime thisYearsBirthdate = new DateTime( RockDateTime.Now.Year, BirthMonth.Value, BirthDay.Value, 0, 0, 0 );
                        birthdayDayOfWeek = thisYearsBirthdate.ToString( "dddd" );
                    }
                    catch { }
                }

                return birthdayDayOfWeek;
            }
            private set { }
        }

        /// <summary>
        /// Gets the day of the week the person's birthday falls on for the current year as a shortened string (e.g. Wed.)
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the shortened day of the week the person's birthday falls on for the current year.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string BirthdayDayOfWeekShort
        {
            get
            {
                string birthdayDayOfWeek = string.Empty;

                if ( BirthMonth.HasValue && BirthDay.HasValue )
                {
                    try
                    {
                        DateTime thisYearsBirthdate = new DateTime( RockDateTime.Now.Year, BirthMonth.Value, BirthDay.Value, 0, 0, 0 );
                        birthdayDayOfWeek = thisYearsBirthdate.ToString( "ddd" );
                    }
                    catch { }
                }

                return birthdayDayOfWeek;
            }
            private set { }
        }

        /// <summary>
        /// Gets the URL of the person's photo.
        /// </summary>
        /// <value>
        /// URL of the photo
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string PhotoUrl
        {
            get
            {
                if ( this.RecordTypeValue != null )
                {
                    return Person.GetPhotoUrl( this.PhotoId, this.Age, this.Gender, this.RecordTypeValue.Guid );
                }
                else
                {
                    return Person.GetPhotoUrl( this.PhotoId, this.Age, this.Gender );
                }
            }
            private set { }
        }

        /// <summary>
        /// Gets or sets a collection containing the Person's <see cref="Rock.Model.UserLogin">UserLogins</see>.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.UserLogin">UserLogins</see> that belong to the Person.
        /// </value>
        [DataMember]
        public virtual ICollection<UserLogin> Users
        {
            get { return _users; }
            set { _users = value; }
        }

        private ICollection<UserLogin> _users;

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.PhoneNumber">PhoneNumbers</see>
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.PhoneNumber"/> entities representing the phone numbers that are associated with this Person.
        /// </value>
        [DataMember]
        public virtual ICollection<PhoneNumber> PhoneNumbers
        {
            get { return _phoneNumbers; }
            set { _phoneNumbers = value; }
        }

        private ICollection<PhoneNumber> _phoneNumbers;

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.GroupMember">GroupMember</see> entities representing the group memberships that are associated
        /// with this Person.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.GroupMember">GroupMember</see> entities representing the group memberships that are associated with
        /// </value>
        public virtual ICollection<GroupMember> Members
        {
            get { return _members; }
            set { _members = value; }
        }

        private ICollection<GroupMember> _members;

        /// <summary>
        /// Gets or sets the aliases for this person
        /// </summary>
        /// <value>
        /// The aliases.
        /// </value>
        public virtual ICollection<PersonAlias> Aliases
        {
            get { return _aliases; }
            set { _aliases = value; }
        }

        private ICollection<PersonAlias> _aliases;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Person's marital status.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> representing the Person's marital status.
        /// </value>
        [DataMember]
        public virtual DefinedValue MaritalStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Person's connection status
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the Person's connection status.
        /// </value>
        [DataMember]
        public virtual DefinedValue ConnectionStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the review reason value.
        /// </summary>
        /// <value>
        /// The review reason value.
        /// </value>
        [DataMember]
        public virtual DefinedValue ReviewReasonValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the record status.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the record status.
        /// </value>
        [DataMember]
        public virtual DefinedValue RecordStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Record Status Reason.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> that represents the Record Status Reason (disposition)
        /// </value>
        [DataMember]
        public virtual DefinedValue RecordStatusReasonValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the RecordType.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> representing the record type.
        /// </value>
        [DataMember]
        public virtual DefinedValue RecordTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Person's name suffix.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue" /> representing the name suffix.
        /// </value>
        [DataMember]
        public virtual DefinedValue SuffixValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Person's salutation title.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> object representing the Person's salutation title.
        /// </value>
        [DataMember]
        public virtual DefinedValue TitleValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> that contains the Person's photo.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFile"/> that contains the Person's photo.
        /// </value>
        [DataMember]
        public virtual BinaryFile Photo { get; set; }

        /// <summary>
        /// Gets or sets the giving group.
        /// </summary>
        /// <value>
        /// The giving group.
        /// </value>
        public virtual Group GivingGroup { get; set; }

        /// <summary>
        /// Gets the Person's birth date. Note: Use SetBirthDate to set the Birthdate
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the Person's birthdate.  If no birthdate is available, null is returned. If the year is not available then the birthdate is returned with the DateTime.MinValue.Year.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [Column( TypeName = "Date" )]
        public DateTime? BirthDate
        {
            get
            {
                // NOTE: This is the In-Memory get, LinqToSql will get the value from the database
                if ( BirthDay == null || BirthMonth == null )
                {
                    return null;
                }
                else
                {
                    return new DateTime( BirthYear ?? DateTime.MinValue.Year, BirthMonth.Value, BirthDay.Value );
                }
            }

            private set
            {
                // don't do anthing here since EF uses this for loading the Birthdate From the database. Use SetBirthDate to set the birthdate
            }
        }

        /// <summary>
        /// Gets or sets the number of days until their next birthday. This is a computed column and can be used
        /// in LinqToSql queries, but there is no in-memory calculation. Avoid using property outside
        /// a linq query
        /// NOTE: If their birthday is Feb 29, and this isn't a leap year, it'll treat Feb 28th as their birthday when doing this calculation
        /// </summary>
        /// <value>
        /// The the number of days until their next birthday
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        public int? DaysUntilBirthday { get; set; }

        /// <summary>
        /// Sets the birth date, which will set the BirthMonth, BirthDay, and BirthYear values
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetBirthDate( DateTime? value )
        {
            if ( value.HasValue )
            {
                BirthMonth = value.Value.Month;
                BirthDay = value.Value.Day;
                if ( value.Value.Year != DateTime.MinValue.Year )
                {
                    BirthYear = value.Value.Year;
                }
                else
                {
                    BirthYear = null;
                }
            }
            else
            {
                BirthMonth = null;
                BirthDay = null;
                BirthYear = null;
            }
        }

        /// <summary>
        /// Gets the Person's age.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the person's age. Returns null if the birthdate or birthyear is not available.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual int? Age
        {
            get
            {
                if ( BirthYear.HasValue )
                {
                    DateTime? bd = BirthDate;
                    if ( bd.HasValue )
                    {
                        DateTime today = RockDateTime.Today;
                        int age = today.Year - bd.Value.Year;
                        if ( bd.Value > today.AddYears( -age ) ) age--;
                        return age;
                    }
                }
                return null;
            }
            private set { }
        }

        /// <summary>
        /// Gets the number of days until the Person's birthday.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the number of days until the Person's birthday. If the person's birthdate is not available returns Int.MaxValue
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual int DaysToBirthday
        {
            get
            {
                if ( BirthDay.HasValue && BirthMonth.HasValue )
                {
                    if ( BirthDay.Value >= 1 && BirthDay.Value <= 31 && BirthMonth.Value >= 1 && BirthMonth.Value <= 12 )
                    {
                        var today = RockDateTime.Today;

                        int day = BirthDay.Value;
                        int month = BirthMonth.Value;
                        int year = today.Year;
                        if ( month < today.Month || ( month == today.Month && day < today.Day ) )
                        {
                            year++;
                        }

                        DateTime bday = DateTime.MinValue;
                        while ( !DateTime.TryParse( BirthMonth.Value.ToString() + "/" + day.ToString() + "/" + year.ToString(), out bday ) && day > 28 )
                        {
                            day--;
                        }

                        if ( bday != DateTime.MinValue )
                        {
                            return Convert.ToInt32( bday.Subtract( today ).TotalDays );
                        }
                    }
                }
                return int.MaxValue;
            }
            private set { }
        }

        /// <summary>
        /// Gets the Person's precise age (includes the fraction of the year).
        /// </summary>
        /// <value>
        /// A <see cref="System.Double"/> representing the Person's age (including fraction of year)
        /// </value>
        [NotMapped]
        public virtual double? AgePrecise
        {
            get
            {
                DateTime bday;
                if ( DateTime.TryParse( BirthMonth.ToString() + "/" + BirthDay.ToString() + "/" + BirthYear, out bday ) )
                {
                    // Calculate years
                    DateTime today = RockDateTime.Today;
                    int years = today.Year - bday.Year;
                    if ( bday > today.AddYears( -years ) ) years--;

                    // Calculate days between last and next bday (differs on leap years).
                    DateTime lastBday = bday.AddYears( years );
                    DateTime nextBday = lastBday.AddYears( 1 );
                    double daysInYear = nextBday.Subtract( lastBday ).TotalDays;

                    // Calculate days since last bday
                    double days = today.Subtract( lastBday ).TotalDays;

                    return years + ( days / daysInYear );
                }
                return null;
            }
        }

        /// <summary>
        /// Gets or sets the grade offset, which is the number of years until their graduation date.  This is used to determine which Grade (Defined Value) they are in
        /// </summary>
        /// <value>
        /// The grade offset.
        /// </value>
        [NotMapped]
        [DataMember]
        public virtual int? GradeOffset
        {
            get
            {
                if ( !GraduationYear.HasValue )
                {
                    return null;
                }
                else
                {
                    // Use the GradeTransitionDate (aka grade promotion date) to figure out what grade (gradeOffset) they're in
                    var globalAttributes = GlobalAttributesCache.Read();
                    var transitionDate = globalAttributes.GetValue( "GradeTransitionDate" ).AsDateTime();
                    if ( transitionDate.HasValue )
                    {
                        // if the next graduation mm/dd won't happen until next year, refactor the graduationyear to the prior year
                        // Example, if Transition Date is 6/1/YYYY and today is 6/1/YYYY or later, refactor the graduationyear to the prior year
                        // in other words, we are treating the grade transition date as the last day of the "school" year
                        int graduationYearRefactor = ( RockDateTime.Now < transitionDate.Value ) ? this.GraduationYear.Value : ( this.GraduationYear.Value - 1 );

                        int offsetYears = graduationYearRefactor - RockDateTime.Now.Year;
                        return offsetYears;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            set
            {
                if ( value.HasValue && value >= 0 )
                {
                    var globalAttributes = GlobalAttributesCache.Read();
                    var transitionDate = globalAttributes.GetValue( "GradeTransitionDate" ).AsDateTime();
                    if ( transitionDate.HasValue )
                    {
                        int gradeOffsetAdjustment = ( RockDateTime.Now < transitionDate.Value ) ? value.Value : value.Value + 1;
                        GraduationYear = transitionDate.Value.Year + gradeOffsetAdjustment;
                    }
                }
                else
                {
                    GraduationYear = null;
                }
            }
        }

        /// <summary>
        /// Gets the has graduated.
        /// </summary>
        /// <value>
        /// The has graduated.
        /// </value>
        [NotMapped]
        [DataMember]
        public virtual bool? HasGraduated
        {
            get
            {
                if ( GradeOffset.HasValue )
                {
                    return GradeOffset < 0;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the grade string.
        /// </summary>
        /// <value>
        /// The grade string.
        /// </value>
        [NotMapped]
        [DataMember]
        public virtual string GradeFormatted
        {
            get
            {
                int? gradeOffset = GradeOffset;

                if ( gradeOffset.HasValue && gradeOffset >= 0 )
                {
                    var schoolGrades = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );
                    if ( schoolGrades != null )
                    {
                        var sortedGradeValues = schoolGrades.DefinedValues.OrderBy( a => a.Value.AsInteger() );
                        var schoolGradeValue = sortedGradeValues.Where( a => a.Value.AsInteger() >= gradeOffset.Value ).FirstOrDefault();
                        if ( schoolGradeValue != null )
                        {
                            return schoolGradeValue.Description;
                        }
                    }
                }

                return string.Empty;
            }
            private set { }
        }

        /// <summary>
        /// Gets the impersonation parameter.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the impersonation parameter.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual string ImpersonationParameter
        {
            get
            {
                var encryptedKey = this.EncryptedKey;
                return "rckipid=" + HttpUtility.UrlEncode( encryptedKey );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the <see cref="Rock.Model.UserLogin"/> of the user being impersonated.
        /// </summary>
        /// <value>
        /// Th <see cref="Rock.Model.UserLogin"/> of the user being impersonated.
        /// </value>
        public virtual UserLogin GetImpersonatedUser()
        {
            UserLogin user = new UserLogin();
            user.UserName = this.FullName;
            user.PersonId = this.Id;
            user.Person = this;
            return user;
        }

        /// <summary>
        /// Gets an anchor tag for linking to person profile
        /// </summary>
        /// <param name="rockUrlRoot">The rock URL root.</param>
        /// <param name="cssClass">The CSS class.</param>
        /// <returns></returns>
        public string GetAnchorTag( string rockUrlRoot, string cssClass = "" )
        {
            return string.Format( "<a class='{0}' href='{1}Person/{2}'>{3}</a>", cssClass, rockUrlRoot, Id, FullName );
        }

        /// <summary>
        /// Gets an anchor tag to send person a communication
        /// </summary>
        /// <value>
        /// The email tag.
        /// </value>
        public string GetEmailTag( string rockUrlRoot, string cssClass = "", string preText = "", string postText = "" )
        {
            if ( !string.IsNullOrWhiteSpace( Email ) )
            {
                if ( !IsEmailActive.HasValue || IsEmailActive.Value )
                {
                    rockUrlRoot.EnsureTrailingBackslash();

                    switch ( EmailPreference )
                    {
                        case EmailPreference.EmailAllowed:
                            {
                                return string.Format(
                                    "<a class='{0}' href='{1}Communication?person={2}'>{3} {4} {5}</a>",
                                    cssClass, rockUrlRoot, Id, preText, Email, postText );
                            }
                        case EmailPreference.NoMassEmails:
                            {
                                return string.Format(
                                    "<span class='js-email-status email-status no-mass-email' data-toggle='tooltip' data-placement='top' title='Email Preference is set to \"No Mass Emails\"'><a class='{0}' href='{1}Communication?person={2}'>{3} {4} {5} <i class='fa fa-exchange'></i></a> </span>",
                                    cssClass, rockUrlRoot, Id, preText, Email, postText );
                            }
                        case EmailPreference.DoNotEmail:
                            {
                                return string.Format(
                                    "<span class='{0} js-email-status email-status do-not-email' data-toggle='tooltip' data-placement='top' title='Email Preference is set to \"Do Not Email\"'>{1} {2} {3} <i class='fa fa-ban'></i></span>",
                                    cssClass, preText, Email, postText );
                            }
                    }
                }
                else
                {
                    return string.Format(
                        "<span class='js-email-status not-active email-status' data-toggle='tooltip' data-placement='top' title='Email is not active. {0}'>{1} <i class='fa fa-exclamation-triangle'></i></span>",
                        EmailNote, Email );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Creates a <see cref="System.Collections.Generic.Dictionary{String, Object}"/> of the Person object
        /// </summary>
        /// <returns>A <see cref="System.Collections.Generic.Dictionary{String, Object}"/> of the Person object.</returns>
        public override Dictionary<string, object> ToDictionary()
        {
            var dictionary = base.ToDictionary();
            dictionary.Add( "Age", AgePrecise );
            dictionary.Add( "DaysToBirthday", DaysToBirthday );
            dictionary.AddOrIgnore( "FullName", FullName );
            return dictionary;
        }

        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, System.Data.Entity.EntityState state )
        {
            var inactiveStatus = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
            var deceased = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_DECEASED.AsGuid() );

            if ( inactiveStatus != null && deceased != null )
            {
                bool isInactive = ( RecordStatusValueId.HasValue && RecordStatusValueId.Value == inactiveStatus.Id ) ||
                    ( RecordStatusValue != null && RecordStatusValue.Id == inactiveStatus.Id );
                bool isReasonDeceased = ( RecordStatusReasonValueId.HasValue && RecordStatusReasonValueId.Value == deceased.Id ) ||
                    ( RecordStatusReasonValue != null && RecordStatusReasonValue.Id == deceased.Id );

                IsDeceased = isInactive && isReasonDeceased;
            }

            if ( string.IsNullOrWhiteSpace( NickName ) )
            {
                NickName = FirstName;
            }

            if ( PhotoId.HasValue )
            {
                BinaryFileService binaryFileService = new BinaryFileService( (RockContext)dbContext );
                var binaryFile = binaryFileService.Get( PhotoId.Value );
                if ( binaryFile != null && binaryFile.IsTemporary )
                {
                    binaryFile.IsTemporary = false;
                }
            }

            var transaction = new Rock.Transactions.SaveMetaphoneTransaction( this );
            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Person's FullName that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Person's FullName that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.FullName;
        }

        /// <summary>
        /// Determines whether the specified action is authorized.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( person.Guid.Equals( this.Guid ) )
            {
                return true;
            }
            else
            {
                return base.IsAuthorized( action, person );
            }
        }

        #endregion

        #region Static Helper Methods

        /// <summary>
        /// Returns a URL for the person's photo.
        /// </summary>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetPhotoUrl( int? photoId, Gender gender, int? maxWidth = null, int? maxHeight = null )
        {
            return GetPhotoUrl( photoId, null, gender, null, maxWidth, maxHeight );
        }

        /// <summary>
        /// Returns a URL for the person's photo.
        /// </summary>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="age">The age.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetPhotoUrl( int? photoId, int? age, Gender gender, int? maxWidth = null, int? maxHeight = null )
        {
            return GetPhotoUrl( photoId, age, gender, null, maxWidth, maxHeight );
        }

        /// <summary>
        /// Returns a URL for the person's photo.
        /// </summary>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="age">The age.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetPhotoUrl( int? photoId, Gender gender, int? age, int? maxWidth = null, int? maxHeight = null )
        {
            return GetPhotoUrl( photoId, null, gender, null, maxWidth, maxHeight );
        }

        /// <summary>
        /// Returns a URL for the person's photo.
        /// </summary>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="RecordTypeValueGuid">The record type value unique identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetPhotoUrl( int? photoId, Gender gender, Guid? RecordTypeValueGuid, int? maxWidth = null, int? maxHeight = null )
        {
            return GetPhotoUrl( photoId, null, gender, null, maxWidth, maxHeight );
        }

        /// <summary>
        /// Returns a URL for the person's photo.
        /// </summary>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="age">The age.</param>
        /// <param name="RecordTypeValueGuid">The record type value unique identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetPhotoUrl( int? photoId, int? age, Gender gender, Guid? RecordTypeValueGuid, int? maxWidth = null, int? maxHeight = null )
        {
            string virtualPath = String.Empty;
            if ( photoId.HasValue )
            {
                string widthHeightParams = string.Empty;
                if ( maxWidth.HasValue )
                {
                    widthHeightParams += string.Format( "&maxwidth={0}", maxWidth.Value );
                }

                if ( maxHeight.HasValue )
                {
                    widthHeightParams += string.Format( "&maxheight={0}", maxHeight.Value );
                }

                virtualPath = String.Format( "~/GetImage.ashx?id={0}" + widthHeightParams, photoId );
            }
            else
            {
                if ( RecordTypeValueGuid.HasValue && RecordTypeValueGuid.Value == SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
                {
                    virtualPath = "~/Assets/Images/business-no-photo.svg?";
                }
                else if ( age.HasValue && age.Value < 18 )
                {
                    // it's a child
                    if ( gender == Model.Gender.Female )
                    {
                        virtualPath = "~/Assets/Images/person-no-photo-child-female.svg?";
                    }
                    else
                    {
                        virtualPath = "~/Assets/Images/person-no-photo-child-male.svg?";
                    }
                }
                else
                {
                    // it's an adult
                    if ( gender == Model.Gender.Female )
                    {
                        virtualPath = "~/Assets/Images/person-no-photo-female.svg?";
                    }
                    else
                    {
                        virtualPath = "~/Assets/Images/person-no-photo-male.svg?";
                    }
                }
            }

            if ( System.Web.HttpContext.Current == null )
            {
                return virtualPath;
            }
            else
            {
                return VirtualPathUtility.ToAbsolute( virtualPath );
            }
        }

        /// <summary>
        /// Gets the photo image tag.
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public static string GetPhotoImageTag( PersonAlias personAlias, int? maxWidth = null, int? maxHeight = null, string className = "" )
        {
            Person person = personAlias != null ? personAlias.Person : null;
            return GetPhotoImageTag( person, maxWidth, maxHeight, className );
        }

        /// <summary>
        /// Gets the photo image tag.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public static string GetPhotoImageTag( Person person, int? maxWidth = null, int? maxHeight = null, string className = "" )
        {
            int? photoId = null;
            Gender gender = Gender.Male;
            string altText = string.Empty;
            int? age = null;
            Guid? recordTypeValueGuid = null;

            if ( person != null )
            {
                photoId = person.PhotoId;
                gender = person.Gender;
                altText = person.FullName;
                age = person.Age;
                recordTypeValueGuid = person.RecordTypeValueId.HasValue ? DefinedValueCache.Read( person.RecordTypeValueId.Value ).Guid : (Guid?)null;
            }

            return Person.GetPhotoImageTag( photoId, age, gender, recordTypeValueGuid, maxWidth, maxHeight, altText, className );
        }

        /// <summary>
        /// Gets the photo image tag.
        /// </summary>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="gender">The gender to use if the photoId is null.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <param name="altText">The alt text to use on the image.</param>
        /// <param name="className">The css class name to apply to the image.</param>
        /// <returns>An html img tag (string) of the requested photo.</returns>
        public static string GetPhotoImageTag( int? photoId, Gender gender, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            return Person.GetPhotoImageTag( photoId, null, gender, null, maxWidth, maxHeight, altText, className );
        }

        /// <summary>
        /// Gets the photo image tag.
        /// </summary>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="age">The age.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <param name="altText">The alt text.</param>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public static string GetPhotoImageTag( int? photoId, int? age, Gender gender, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            return Person.GetPhotoImageTag( photoId, age, gender, null, maxWidth, maxHeight, altText, className );
        }

        /// <summary>
        /// Gets the photo image tag.
        /// </summary>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="age">The age.</param>
        /// <param name="gender">The gender to use if the photoId is null.</param>
        /// <param name="recordTypeValueGuid">The record type value unique identifier.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <param name="altText">The alt text to use on the image.</param>
        /// <param name="className">The css class name to apply to the image.</param>
        /// <returns>
        /// An html img tag (string) of the requested photo.
        /// </returns>
        public static string GetPhotoImageTag( int? photoId, int? age, Gender gender, Guid? recordTypeValueGuid, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            var photoUrl = new StringBuilder();

            photoUrl.Append( VirtualPathUtility.ToAbsolute( "~/" ) );

            string styleString = string.Empty;

            string altString = string.IsNullOrWhiteSpace( altText ) ? "" :
                string.Format( " alt='{0}'", altText );

            string classString = string.IsNullOrWhiteSpace( className ) ? "" :
                string.Format( " class='{0}'", className );

            if ( photoId.HasValue )
            {
                photoUrl.AppendFormat( "GetImage.ashx?id={0}", photoId );
                if ( maxWidth.HasValue )
                {
                    photoUrl.AppendFormat( "&maxwidth={0}", maxWidth.Value );
                }
                if ( maxHeight.HasValue )
                {
                    photoUrl.AppendFormat( "&maxheight={0}", maxHeight.Value );
                }
            }
            else
            {
                if ( recordTypeValueGuid.HasValue && recordTypeValueGuid.Value == SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
                {
                    photoUrl.Append( "/Assets/Images/business-no-photo.svg?" );
                }
                else if ( age.HasValue && age.Value < 18 )
                {
                    // it's a child
                    if ( gender == Model.Gender.Female )
                    {
                        photoUrl.Append( "Assets/Images/person-no-photo-child-female.svg?" );
                    }
                    else
                    {
                        photoUrl.Append( "Assets/Images/person-no-photo-child-male.svg?" );
                    }
                }
                else
                {
                    // it's an adult
                    if ( gender == Model.Gender.Female )
                    {
                        photoUrl.Append( "Assets/Images/person-no-photo-female.svg?" );
                    }
                    else
                    {
                        photoUrl.Append( "Assets/Images/person-no-photo-male.svg?" );
                    }
                }

                if ( maxWidth.HasValue || maxHeight.HasValue )
                {
                    styleString = string.Format( " style='{0}{1}'",
                        maxWidth.HasValue ? "max-width:" + maxWidth.Value.ToString() + "px; " : "",
                        maxHeight.HasValue ? "max-height:" + maxHeight.Value.ToString() + "px;" : "" );
                }
            }

            return string.Format( "<img src='{0}'{1}{2}{3}/>", photoUrl.ToString(), styleString, altString, classString );
        }

        /// <summary>
        /// Adds the related person to the selected person's known relationships with a role of 'Can check in' which
        /// is typically configured to allow check-in.  If an inverse relationship is configured for 'Can check in'
        /// (i.e. 'Allow check in by'), that relationship will also be created.
        /// </summary>
        /// <param name="personId">A <see cref="System.Int32" /> representing the Id of the Person.</param>
        /// <param name="relatedPersonId">A <see cref="System.Int32" /> representing the Id of the related Person.</param>
        /// <param name="currentPersonAlias">A <see cref="Rock.Model.PersonAlias" /> representing the Person who is logged in.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void CreateCheckinRelationship( int personId, int relatedPersonId, PersonAlias currentPersonAlias, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var knownRelationshipGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
            var ownerRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ) );
            var canCheckInRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN ) ) );

            if ( ownerRole != null && canCheckInRole != null )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var knownRelationshipGroup = groupMemberService.Queryable()
                    .Where( m =>
                        m.PersonId == personId &&
                        m.GroupRole.Guid.Equals( ownerRole.Guid ) )
                    .Select( m => m.Group )
                    .FirstOrDefault();

                // Create known relationship group if doesn't exist
                if ( knownRelationshipGroup == null )
                {
                    var groupMember = new GroupMember();
                    groupMember.PersonId = personId;
                    groupMember.GroupRoleId = ownerRole.Id;

                    knownRelationshipGroup = new Group();
                    knownRelationshipGroup.Name = knownRelationshipGroupType.Name;
                    knownRelationshipGroup.GroupTypeId = knownRelationshipGroupType.Id;
                    knownRelationshipGroup.Members.Add( groupMember );

                    new GroupService( rockContext ).Add( knownRelationshipGroup );
                    rockContext.SaveChanges();
                }

                // Add relationships
                var canCheckInMember = groupMemberService.Queryable()
                    .FirstOrDefault( m =>
                        m.GroupId == knownRelationshipGroup.Id &&
                        m.PersonId == relatedPersonId &&
                        m.GroupRoleId == canCheckInRole.Id );

                if ( canCheckInMember == null )
                {
                    canCheckInMember = new GroupMember();
                    canCheckInMember.GroupId = knownRelationshipGroup.Id;
                    canCheckInMember.PersonId = relatedPersonId;
                    canCheckInMember.GroupRoleId = canCheckInRole.Id;
                    groupMemberService.Add( canCheckInMember );
                    rockContext.SaveChanges();
                }

                var inverseGroupMember = groupMemberService.GetInverseRelationship( canCheckInMember, true, currentPersonAlias );
                if ( inverseGroupMember != null )
                {
                    groupMemberService.Add( inverseGroupMember );
                    rockContext.SaveChanges();
                }
            }
        }


        /// <summary>
        /// Formats the full name.
        /// </summary>
        /// <param name="nickName">The nick name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="suffix">The suffix.</param>
        /// <returns></returns>
        public static string FormatFullName( string nickName, string lastName, string suffix )
        {
            var fullName = new StringBuilder();

            fullName.AppendFormat( "{0} {1}", nickName, lastName );

            
            if ( !string.IsNullOrWhiteSpace(suffix))
            {
                fullName.AppendFormat( " {0}", suffix );
            }

            return fullName.ToString();
        }

        /// <summary>
        /// Formats the full name.
        /// </summary>
        /// <param name="nickName">The nick name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="suffixValueId">The suffix value identifier.</param>
        /// <returns></returns>
        public static string FormatFullName( string nickName, string lastName, int? suffixValueId) {
            
           
            if ( suffixValueId.HasValue )
            {
                var suffix = DefinedValueCache.Read( suffixValueId.Value );
                if ( suffix != null )
                {
                    return FormatFullName( nickName, lastName, suffix.Value );
                }
            }

            return FormatFullName( nickName, lastName, string.Empty );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Person Configuration class.
    /// </summary>
    public partial class PersonConfiguration : EntityTypeConfiguration<Person>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonConfiguration"/> class.
        /// </summary>
        public PersonConfiguration()
        {
            this.HasOptional( p => p.MaritalStatusValue ).WithMany().HasForeignKey( p => p.MaritalStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ConnectionStatusValue ).WithMany().HasForeignKey( p => p.ConnectionStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RecordStatusValue ).WithMany().HasForeignKey( p => p.RecordStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RecordStatusReasonValue ).WithMany().HasForeignKey( p => p.RecordStatusReasonValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RecordTypeValue ).WithMany().HasForeignKey( p => p.RecordTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ReviewReasonValue ).WithMany().HasForeignKey( p => p.ReviewReasonValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.SuffixValue ).WithMany().HasForeignKey( p => p.SuffixValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.TitleValue ).WithMany().HasForeignKey( p => p.TitleValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Photo ).WithMany().HasForeignKey( p => p.PhotoId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.GivingGroup ).WithMany().HasForeignKey( p => p.GivingGroupId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The gender of a person
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Male
        /// </summary>
        Male = 1,

        /// <summary>
        /// Female
        /// </summary>
        Female = 2
    }

    /// <summary>
    /// The person's email preference
    /// </summary>
    public enum EmailPreference
    {
        /// <summary>
        /// Emails can be sent to person
        /// </summary>
        EmailAllowed = 0,

        /// <summary>
        /// No Mass emails should be sent to person
        /// </summary>
        NoMassEmails = 1,

        /// <summary>
        /// No emails should be sent to person
        /// </summary>
        DoNotEmail = 2
    }

    #endregion

    #region Extension Methods

    public static partial class PersonExtensionMethods
    {
        /// <summary>
        /// Gets the families.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<Group> GetFamilies( this Person person, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            return new PersonService( rockContext ).GetFamilies( person != null ? person.Id : 0 );
        }

        /// <summary>
        /// Gets the home location.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static Location GetHomeLocation( this Person person, RockContext rockContext = null )
        {
            Guid homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();
            foreach ( var family in person.GetFamilies( rockContext ) )
            {
                var loc = family.GroupLocations
                    .Where( l =>
                        l.GroupLocationTypeValue.Guid.Equals( homeAddressGuid ) &&
                        l.IsMappedLocation )
                    .Select( l => l.Location )
                    .FirstOrDefault();
                if ( loc != null )
                {
                    return loc;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.Person" /> entities containing the Person's family.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person" /> to retrieve family members for.</param>
        /// <param name="includeSelf">A <see cref="System.Boolean" /> value that is <c>true</c> if the provided person should be returned in the results, otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// Returns a queryable collection of <see cref="Rock.Model.Person" /> entities representing the provided Person's family.
        /// </returns>
        public static IQueryable<GroupMember> GetFamilyMembers( this Person person, bool includeSelf = false, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetFamilyMembers( person != null ? person.Id : 0, includeSelf );
        }

        /// <summary>
        /// Gets any previous last names for this person sorted alphabetically by LastName
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IOrderedQueryable<PersonPreviousName> GetPreviousNames( this Person person, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetPreviousNames( person != null ? person.Id : 0 );
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.Person" /> entity of the provided Person's spouse.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person" /> entity of the Person to retrieve the spouse of.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Person" /> entity containing the provided Person's spouse. If the provided Person's spouse is not found, this value will be null.
        /// </returns>
        public static Person GetSpouse( this Person person, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetSpouse( person );
        }

        /// <summary>
        /// limits the PersonQry to people that have an Age that is between MinAge and MaxAge (inclusive)
        /// </summary>
        /// <param name="personQry">The person qry.</param>
        /// <param name="minAge">The minimum age.</param>
        /// <param name="maxAge">The maximum age.</param>
        /// <param name="includePeopleWithNoAge">if set to <c>true</c> [include people with no age].</param>
        /// <returns></returns>
        public static IQueryable<Person> WhereAgeRange( this IQueryable<Person> personQry, int? minAge, int? maxAge, bool includePeopleWithNoAge = true )
        {
            var currentDate = RockDateTime.Today;
            var qryWithAge = personQry.Select(
                      p => new
                      {
                          Person = p,
                          Age = ( p.BirthDate > SqlFunctions.DateAdd( "year", -SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ), currentDate )
                            ? SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ) - 1
                            : SqlFunctions.DateDiff( "year", p.BirthDate, currentDate ) )
                      } );

            if ( includePeopleWithNoAge )
            {
                if ( minAge.HasValue )
                {
                    qryWithAge = qryWithAge.Where( a => !a.Age.HasValue || a.Age >= minAge );
                }

                if ( maxAge.HasValue )
                {
                    qryWithAge = qryWithAge.Where( a => !a.Age.HasValue || a.Age <= maxAge );
                }
            }
            else
            {
                if ( minAge.HasValue )
                {
                    qryWithAge = qryWithAge.Where( a => a.Age.HasValue && a.Age >= minAge );
                }

                if ( maxAge.HasValue )
                {
                    qryWithAge = qryWithAge.Where( a => a.Age.HasValue && a.Age <= maxAge );
                }
            }

            return qryWithAge.Select( a => a.Person );
        }
    }

    #endregion
}