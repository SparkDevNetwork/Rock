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

using Rock.Data;
using Rock.Enums.Crm;
using Rock.Lava;
using Rock.UniversalSearch;
using Rock.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents a person or a business in Rock.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "Person" )]
    [DataContract]
    [Analytics( true, true )]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.PERSON )]
    public partial class Person : Model<Person>, IRockIndexable
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
        [Index( "IX_IsDeceased_FirstName_LastName", IsUnique = false, Order = 1 )]
        [Index( "IX_IsDeceased_LastName_FirstName", IsUnique = false, Order = 1 )]
        public bool IsDeceased
        {
            get
            {
                return _isDeceased;
            }

            set
            {
                _isDeceased = value;
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
        [Index( "IX_IsDeceased_FirstName_LastName", IsUnique = false, Order = 2 )]
        [Index( "IX_IsDeceased_LastName_FirstName", IsUnique = false, Order = 3 )]
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
        [Index( "IX_IsDeceased_FirstName_LastName", IsUnique = false, Order = 3 )]
        [Index( "IX_IsDeceased_LastName_FirstName", IsUnique = false, Order = 2 )]
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

        #region Birthday Fields

        private int? _birthday;
        private int? _birthmonth;
        private int? _birthyear;

        /// <summary>
        /// Gets or sets the day of the month portion of the Person's birth date.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the day of the month portion of the Person's birth date. If their birth date is not known
        /// this value will be null.
        /// </value>
        [DataMember]
        public int? BirthDay
        {
            get => _birthday;
            set
            {
                _birthday = value;
                // updating the Age so as to keep it consistent with the birthyear
                // the BirthDate field is updated automatically unlike the Age.
                Age = Person.GetAge( BirthDate, null );
            }
        }

        /// <summary>
        /// Gets or sets the month portion of the Person's birth date.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the month portion of the Person's birth date. If the birth date is not known this value will be null.
        /// </value>
        [DataMember]
        public int? BirthMonth
        {
            get => _birthmonth;
            set
            {
                _birthmonth = value;
                // updating the Age so as to keep it consistent with the birthyear
                // the BirthDate field is updated automatically unlike the Age.
                Age = Person.GetAge( BirthDate, null );
            }
        }

        /// <summary>
        /// Gets or sets the year portion of the Person's birth date.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the year portion of the Person's birth date. If the birth date is not known this value will be null.
        /// </value>
        [DataMember]
        public int? BirthYear
        {
            get => _birthyear;
            set
            {
                _birthyear = value;
                // updating the Age so as to keep it consistent with the birthyear
                // the BirthDate field is updated automatically unlike the Age.
                Age = Person.GetAge( BirthDate, null );
            }
        }

        /// <summary>
        /// Gets the Person's age.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the person's age. Returns null if the birthdate or birthyear is not available.
        /// </value>
        [DataMember]
        public int? Age
        {
            get;
            // The setter has been explicitly set to private as the value of this property is dependent on the birthday fields
            // and should not be manipulated otherwise.
            private set;
        }

        #endregion

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
        /// Gets or sets Id of the Marital Status <see cref="Rock.Model.DefinedValue"/> representing the Person's marital status.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Marital Status <see cref="Rock.Model.DefinedValue"/> representing the Person's marital status.  This value is nullable.
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
        /// Gets the computed giver identifier in the format G{GivingGroupId} if they are part of a GivingGroup, or P{Personid} if they give individually
        /// </summary>
        /// <value>
        /// The giver identifier.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [Index( "IX_GivingId" )]
        public string GivingId { get; private set; }

        /// <summary>
        /// Gets or sets the giving leader's Person Id.
        /// Note: This is computed on save, so any manual changes to this will be ignored.
        /// </summary>
        /// <value>
        /// The giving leader identifier.
        /// </value>
        [DataMember]
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
        // DV See also: Rock.Communication.EmailAddressFieldValidator _emailAddressRegex, make sure the two stay in sync. #4829, #4867
        [RegularExpression( @"\s*(?:[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[A-Za-z0-9-]*[A-Za-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])\s*", ErrorMessage = "The Email address is invalid" )]
        [Index( "IX_Email" )]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Person's email address is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the email address is active, otherwise <c>false</c>.
        /// </value>
        [DataMember]
        [Newtonsoft.Json.JsonConverter( typeof( Rock.Utility.NotNullJsonConverter<bool> ), true )]
        [DefaultValue( true )]
        public bool IsEmailActive
        {
            get { return _isEmailActive; }
            set { _isEmailActive = value; }
        }

        private bool _isEmailActive = true;

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
        /// Gets or sets the communication preference.
        /// </summary>
        /// <value>
        /// The communication preference.
        /// </value>
        [DataMember]
        public CommunicationType CommunicationPreference { get; set; }

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

        /// <summary>
        /// Gets or sets the name of the top signal color. This property is used to indicate the icon color
        /// on a person if they have a related signal.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the CSS color.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string TopSignalColor { get; set; }

        /// <summary>
        /// Gets or sets the name of the top signal CSS class. This property is used to indicate which icon to display
        /// on a person if they have a related signal.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the signal CSS class.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string TopSignalIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the highest priority PersonSignal associated with this person.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing a PersonSignal Id of the <see cref="Rock.Model.PersonSignal"/>.
        /// </value>
        [DataMember]
        public int? TopSignalId { get; set; }

        /// <summary>
        /// Gets or sets the age classification of the Person.
        /// Note: This is computed on save, so any manual changes to this will be ignored.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.AgeClassification"/> enum value representing the Person's age classification.  Valid values are <c>AgeClassification.Unknown</c> if the Person's age is unknown,
        /// <c>AgeClassification.Adult</c> if the Person's age falls under Adult Range, <c>AgeClassification.Child</c> if the Person is under the age of 18
        /// </value>
        [DataMember]
        [Previewable]
        public AgeClassification AgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the group id for the <see cref="Person.PrimaryFamily" />.
        /// Note: This is computed on save, so any manual changes to this will be ignored.
        /// </summary>
        /// <value>
        /// The primary family id.
        /// </value>
        [DataMember]
        public int? PrimaryFamilyId { get; set; }

        /// <summary>
        /// Gets or sets the campus id for the primary family.
        /// Note: This is computed on save, so any manual changes to this will be ignored.
        /// </summary>
        /// <value>
        /// The campus id of the primary family.
        /// </value>
        [DataMember]
        public int? PrimaryCampusId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Person is locked as child.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the Person is locked as child; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsLockedAsChild
        {
            get
            {
                return _isLockedAsChild;
            }

            set
            {
                _isLockedAsChild = value;
            }
        }

        private bool _isLockedAsChild = false;

        /// <summary>
        /// Gets or sets the deceased date.
        /// </summary>
        /// <value>
        /// The deceased date.
        /// </value>
        [DataMember]
        public DateTime? DeceasedDate { get; set; }

        /// <summary>
        /// Gets or sets the person's default financial account gift designation.
        /// </summary>
        /// <value>
        /// The financial account id.
        /// </value>
        [DataMember]
        public int? ContributionFinancialAccountId { get; set; }

        /// <summary>
        /// Gets or sets the person's account protection profile, which is used by the duplication detection and merge processes.
        /// </summary>
        /// <value>
        /// The account protection profile.
        /// </value>
        [DataMember]
        public AccountProtectionProfile AccountProtectionProfile { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the <see cref="Rock.Model.DefinedValue"/> that represents the Preferred Language for this person.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing DefinedValueId of the Preferred Language <see cref="Rock.Model.DefinedValue"/> for this person.
        /// </value>
        [DataMember]
        public int? PreferredLanguageValueId { get; set; }

        /// <summary>
        /// Gets or sets the reminder count associated with the Person.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the reminder count that is associated with the Person.
        /// </value>
        [DataMember]
        public int? ReminderCount { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Race <see cref="Rock.Model.DefinedValue"/> representing the race of this person
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Race <see cref="Rock.Model.DefinedValue"/> representing the race of this person.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_RACE )]
        public int? RaceValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Ethnicity <see cref="Rock.Model.DefinedValue"/> representing the ethnicity of this person
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Ethnicity <see cref="Rock.Model.DefinedValue"/> representing the ethnicity of this person.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_ETHNICITY )]
        public int? EthnicityValueId { get; set; }

        /// <summary>
        /// Gets or sets the birth date key.
        /// </summary>
        /// <value>
        /// The birth date key.
        /// </value>
        [DataMember]
        public int? BirthDateKey { get; set; }

        /// <summary>
        /// Gets or sets the age bracket.
        /// </summary>
        /// <value>
        /// The age range.
        /// </value>
        [DataMember]
        public AgeBracket AgeBracket { get; private set; }

        /// <summary>
        /// Gets or sets the First Name pronunciation override.
        /// </summary>
        /// <value>
        /// A string with a pronunciation of the first name.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string FirstNamePronunciationOverride { get; set; }

        /// <summary>
        /// Gets or sets the nick Name pronunciation override.
        /// </summary>
        /// <value>
        /// A string with a pronunciation of the nick name.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string NickNamePronunciationOverride { get; set; }

        /// <summary>
        /// Gets or sets the last Name pronunciation override.
        /// </summary>
        /// <value>
        /// A string with a pronunciation of the last name.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string LastNamePronunciationOverride { get; set; }

        /// <summary>
        /// Gets or sets the notes for the pronunciation.
        /// </summary>
        /// <value>
        /// A string with notes on the pronunciation.
        /// </value>
        [MaxLength( 1000 )]
        [DataMember]
        public string PronunciationNote { get; set; }


        /**
            6/29/2023 - KA

            The PrimaryAliasId is not configured as a foreign key with a navigation property to the PersonAlias
            table because Person.Id is already referenced as a foreign key on PersonAlias. Introducing a foreign
            key here back to that table makes delete operations tricky because either one must be deleted first
            before the other can be deleted and at the same time neither can be deleted because it is referenced
            by the other table.
        */

        /// <summary>
        /// Gets the <see cref="Rock.Model.PersonAlias">primary alias</see> identifier.
        /// </summary>
        /// <value>
        /// The primary alias identifier.
        /// </value>
        [DataMember]
        public int? PrimaryAliasId
        {
            get
            {
                var id = _primaryAliasId ?? PrimaryAlias?.Id;
                if ( id == 0 )
                {
                    // This is not a valid reference to a persisted PersonAlias.
                    // In this case, most verification code will expect a null value to be returned.
                    return null;
                }
                return id;
            }

            set => _primaryAliasId = value;
        }

        private int? _primaryAliasId;

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
            CommunicationPreference = CommunicationType.Email;
        }

        #endregion

        #region Navigation Properties

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
        [LavaVisible]
        public virtual ICollection<GroupMember> Members
        {
            get { return _members; }
            set { _members = value; }
        }

        private ICollection<GroupMember> _members;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias">aliases</see> for this person.
        /// </summary>
        /// <value>
        /// The aliases.
        /// </value>
        [LavaVisible]
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
        [LavaVisible]
        public virtual Group GivingGroup { get; set; }

        /// <summary>
        /// Gets or sets the signals applied to this person.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.PersonSignal">PersonSignal</see> entities representing the signals that are associated with this person.
        /// </value>
        [LavaHidden]
        public virtual ICollection<PersonSignal> Signals { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group">primary family</see>.
        /// </summary>
        /// <value>
        /// The primary family.
        /// </value>
        [LavaVisible]
        public virtual Group PrimaryFamily { get; set; }

        /// <summary>
        /// Gets or sets the person's <see cref="Rock.Model.Campus">primary campus</see>.
        /// </summary>
        /// <value>
        /// The primary campus.
        /// </value>
        [LavaVisible]
        public virtual Campus PrimaryCampus { get; set; }

        /// <summary>
        /// Gets or sets the person's default <see cref="Rock.Model.FinancialAccount" /> gift designation.
        /// </summary>
        /// <value>
        /// The financial account.
        /// </value>
        [LavaHidden]
        public virtual FinancialAccount ContributionFinancialAccount { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Person's preferred language.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> object representing the Person's preferred language.
        /// </value>
        [DataMember]
        public virtual DefinedValue PreferredLanguageValue { get; set; }

        /// <summary>
        /// Gets or sets the number of days until their next birthday. This is a computed column and can be used
        /// in LinqToSql queries, but there is no in-memory calculation. Avoid using this property outside of
        /// a linq query. Use DaysToBirthday property instead
        /// NOTE: If their birthday is Feb 29, and this isn't a leap year, it'll treat Feb 28th as their birthday when doing this calculation
        /// </summary>
        /// <value>
        /// The number of days until their next birthday
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        public int? DaysUntilBirthday { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Person's Race
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the Person's race.
        /// </value>
        /// 
        [DataMember]
        public virtual DefinedValue RaceValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Person's Ethnicity
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the Person's ethnicity.
        /// </value>
        [DataMember]
        public virtual DefinedValue EthnicityValue { get; set; }

        #endregion

        #region Methods

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
            this.HasOptional( p => p.PrimaryFamily ).WithMany().HasForeignKey( p => p.PrimaryFamilyId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.PrimaryCampus ).WithMany().HasForeignKey( p => p.PrimaryCampusId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ContributionFinancialAccount ).WithMany().HasForeignKey( p => p.ContributionFinancialAccountId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.PreferredLanguageValue ).WithMany().HasForeignKey( a => a.PreferredLanguageValueId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.RaceValue ).WithMany().HasForeignKey( a => a.RaceValueId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.EthnicityValue ).WithMany().HasForeignKey( a => a.EthnicityValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
