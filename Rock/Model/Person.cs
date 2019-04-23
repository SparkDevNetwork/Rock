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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

using Rock.Data;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a person or a business in Rock.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "Person" )]
    [DataContract]
    [Analytics( true, true )]
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
        public bool IsDeceased
        {
            get {
                return _isDeceased;
            }

            set {
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
        /// A <see cref="System.Int32"/> representing the Id of the Marital Status <see cref="Rock.Model.DefinedValue"/> representing the Person's martial status.  This value is nullable.
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
        /// Gets the computed giver identifier in the format G{GivingGroupId} if they are part of a GivingGroup, or P{Personid} if they give individually
        /// </summary>
        /// <value>
        /// The giver identifier.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        public string GivingId
        {
            get {
                // NOTE: This is the In-Memory get, LinqToSql will get the value from the database
                return GivingGroupId.HasValue ?
                    string.Format( "G{0}", GivingGroupId.Value ) :
                    string.Format( "P{0}", Id );
            }

            private set {
                // don't do anything here since EF uses this for loading
            }
        }

        /// <summary>
        /// Gets or sets the giving leader identifier.
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
        /// Gets or sets the group id for the primary family.
        /// Note: This is computed on save, so any manual changes to this will be ignored.
        /// </summary>
        /// <value>
        /// The primary family id.
        /// </value>
        [DataMember]
        public int? PrimaryFamilyId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Person is locked as child.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the Person is locked as child; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsLockedAsChild
        {
            get {
                return _isLockedAsChild;
            }

            set {
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
            get {
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
            get {
                var primaryAlias = PrimaryAlias;
                if ( primaryAlias != null )
                {
                    return primaryAlias.Id;
                }

                return null;
            }

            private set {
                // intentionally blank
            }
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
            get {
                // Use the SuffixValueId and DefinedValue cache instead of referencing SuffixValue property so
                // that if FullName is used in datagrid, the SuffixValue is not lazy-loaded for each row
                return FormatFullName( NickName, LastName, SuffixValueId );
            }

            private set {
                // intentionally blank
            }
        }

        /// <summary>
        /// Determines whether the <see cref="RecordTypeValue"/> of this Person is Business
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is business; otherwise, <c>false</c>.
        /// </returns>
        public bool IsBusiness()
        {
            return IsBusiness( this.RecordTypeValueId );
        }

        /// <summary>
        /// Gets a value indicating whether this instance is business.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is business; otherwise, <c>false</c>.
        /// </value>
        private static bool IsBusiness( int? recordTypeValueId )
        {
            if ( recordTypeValueId.HasValue )
            {
                int recordTypeValueIdBusiness = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
                return recordTypeValueId.Value == recordTypeValueIdBusiness;
            }
            else
            {
                return false;
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
            get {
                return FormatFullNameReversed( this.LastName, this.NickName, this.SuffixValueId, this.RecordTypeValueId );
            }
        }

        /// <summary>
        /// Gets the full name reversed.
        /// </summary>
        /// <param name="lastName">The last name.</param>
        /// <param name="nickName">Name of the nick.</param>
        /// <param name="suffixValueId">The suffix value identifier.</param>
        /// <param name="recordTypeValueId">The record type value identifier.</param>
        /// <returns></returns>
        public static string FormatFullNameReversed( string lastName, string nickName, int? suffixValueId, int? recordTypeValueId )
        {
            if ( IsBusiness( recordTypeValueId ) )
            {
                return lastName;
            }

            var fullName = new StringBuilder();

            fullName.Append( lastName );

            // Use the SuffixValueId and DefinedValue cache instead of referencing SuffixValue property so
            // that if FullName is used in datagrid, the SuffixValue is not lazy-loaded for each row
            if ( suffixValueId.HasValue )
            {
                var suffix = DefinedValueCache.Get( suffixValueId.Value );
                if ( suffix != null )
                {
                    fullName.AppendFormat( " {0}", suffix.Value );
                }
            }

            fullName.AppendFormat( ", {0}", nickName );
            return fullName.ToString();
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
            get {
                if ( IsBusiness( this.RecordTypeValueId ) )
                {
                    return LastName;
                }

                var fullName = new StringBuilder();

                fullName.AppendFormat( "{0} {1}", FirstName, LastName );

                if ( SuffixValue != null && !string.IsNullOrWhiteSpace( SuffixValue.Value ) )
                {
                    fullName.AppendFormat( " {0}", SuffixValue.Value );
                }

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
            get {
                if ( IsBusiness( this.RecordTypeValueId ) )
                {
                    return LastName;
                }

                var fullName = new StringBuilder();
                fullName.Append( LastName );

                if ( SuffixValue != null && !string.IsNullOrWhiteSpace( SuffixValue.Value ) )
                {
                    fullName.AppendFormat( " {0}", SuffixValue.Value );
                }

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
            get {
                string birthdayDayOfWeek = string.Empty;

                if ( BirthMonth.HasValue && BirthDay.HasValue )
                {
                    try
                    {
                        DateTime thisYearsBirthdate;
                        if ( BirthMonth == 2 && BirthDay == 29 && !DateTime.IsLeapYear( RockDateTime.Now.Year ) )
                        {
                            // if their birthdate is 2/29 and the current year is NOT a leapyear, have their birthday be 2/28
                            thisYearsBirthdate = new DateTime( RockDateTime.Now.Year, BirthMonth.Value, 28, 0, 0, 0 );
                        }
                        else
                        {
                            thisYearsBirthdate = new DateTime( RockDateTime.Now.Year, BirthMonth.Value, BirthDay.Value, 0, 0, 0 );
                        }

                        birthdayDayOfWeek = thisYearsBirthdate.ToString( "dddd" );
                    }
                    catch
                    {
                        // intentionally blank
                    }
                }

                return birthdayDayOfWeek;
            }

            private set {
                // intentionally blank
            }
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
            get {
                string birthdayDayOfWeek = string.Empty;

                if ( BirthMonth.HasValue && BirthDay.HasValue )
                {
                    try
                    {
                        DateTime thisYearsBirthdate;
                        if ( BirthMonth == 2 && BirthDay == 29 && !DateTime.IsLeapYear( RockDateTime.Now.Year ) )
                        {
                            // if their birthdate is 2/29 and the current year is NOT a leapyear, have their birthday be 2/28
                            thisYearsBirthdate = new DateTime( RockDateTime.Now.Year, BirthMonth.Value, 28, 0, 0, 0 );
                        }
                        else
                        {
                            thisYearsBirthdate = new DateTime( RockDateTime.Now.Year, BirthMonth.Value, BirthDay.Value, 0, 0, 0 );
                        }

                        birthdayDayOfWeek = thisYearsBirthdate.ToString( "ddd" );
                    }
                    catch
                    {
                        // intentionally blank
                    }
                }

                return birthdayDayOfWeek;
            }

            private set {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets the URL of the person's photo.
        /// </summary>
        /// <value>
        /// URL of the photo
        /// </value>
        [LavaInclude]
        [NotMapped]
        public virtual string PhotoUrl
        {
            get {
                return Person.GetPersonPhotoUrl( this );
            }

            private set {
                // intentionally blank
            }
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
        [LavaInclude]
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
        [LavaInclude]
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
        [LavaInclude]
        public virtual Group GivingGroup { get; set; }

        /// <summary>
        /// Gets or sets the signals applied to this person.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.PersonSignal">PersonSignal</see> entities representing the signals that are associated with this person.
        /// </value>
        [LavaIgnore]
        public virtual ICollection<PersonSignal> Signals { get; set; }

        /// <summary>
        /// Gets or sets the primary family.
        /// </summary>
        /// <value>
        /// The primary family.
        /// </value>
        [LavaInclude]
        public virtual Group PrimaryFamily { get; set; }

        /// <summary>
        /// Gets the Person's birth date. Note: Use SetBirthDate to set the Birthdate
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the Person's birthdate.  If no birthdate is available, null is returned. If the year is not available then the birthdate is returned with the DateTime.MinValue.Year.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? BirthDate
        {
            get {
                _birthDate = CalculateBirthDate();
                return _birthDate;
            }

            private set {
                _birthDate = value;
            }
        }

        private DateTime? _birthDate ;

        /// <summary>
        /// Calculates the birthdate from the BirthYear, BirthMonth, and BirthDay.
        /// Will return null if BirthMonth or BirthDay is null.
        /// If BirthYear is null then DateTime.MinValue.Year (Year = 1) is used.
        /// </summary>
        /// <returns></returns>
        private DateTime? CalculateBirthDate()
        {
             if ( BirthDay == null || BirthMonth == null )
                {
                    return null;
                }
            else
            {
                if ( BirthMonth <= 12 )
                {
                    if ( BirthDay <= DateTime.DaysInMonth( BirthYear ?? DateTime.MinValue.Year, BirthMonth.Value ) )
                    {
                        return new DateTime( BirthYear ?? DateTime.MinValue.Year, BirthMonth.Value, BirthDay.Value );
                    }
                }

                return null;
            }
        }

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
            get {
                return Person.GetAge( this.BirthDate );
            }

            private set {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets the age.
        /// </summary>
        /// <param name="birthDate">The birth date.</param>
        /// <returns></returns>
        public static int? GetAge( DateTime? birthDate )
        {
            if ( birthDate.HasValue && birthDate.Value.Year != DateTime.MinValue.Year )
            {
                DateTime today = RockDateTime.Today;
                int age = today.Year - birthDate.Value.Year;
                if ( birthDate.Value > today.AddYears( -age ) )
                {
                    // their birthdate is after today's date, so they aren't a year older yet
                    age--;
                }

                return age;
            }

            return null;
        }


        /// <summary>
        /// Formats the age with unit (year, month, day) suffix depending on the age of the individual.
        /// </summary>
        /// <param name="condensed">if set to <c>true</c> age in years is returned without a unit suffix.</param>
        /// <returns></returns>
        public string FormatAge( bool condensed = false )
        {
            var age = Age;
            if ( age != null )
            {
                if ( condensed )
                {
                    return age.ToString();
                }
                if ( age > 0 )
                {
                    return age + ( age == 1 ? " yr" : " yrs" );
                }
                else if ( age < -1 )
                {
                    return string.Empty;
                }
            }

            var today = RockDateTime.Today;
            if ( BirthYear != null && BirthMonth != null )
            {
                int months = today.Month - BirthMonth.Value;
                if ( BirthYear < today.Year )
                {
                    months = months + 12;
                }
                if ( BirthDay > today.Day )
                {
                    months--;
                }
                if ( months > 0 )
                {
                    return months + ( months == 1 ? " mo" : " mos" );
                }
            }

            if ( BirthYear != null && BirthMonth != null && BirthDay != null )
            {
                int days = today.Day - BirthDay.Value;
                if ( days < 0 )
                {
                    // Add the number of days in the birth month
                    var birthMonth = new DateTime( BirthYear.Value, BirthMonth.Value, 1 );
                    days = days + birthMonth.AddMonths( 1 ).AddDays( -1 ).Day;
                }
                return days + ( days == 1 ? " day" : " days" );
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the next birth day.
        /// </summary>
        /// <value>
        /// The next birth day.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual DateTime? NextBirthDay
        {
            get {
                if ( BirthMonth.HasValue && BirthDay.HasValue )
                {
                    var today = RockDateTime.Today;
                    var nextBirthDay = RockDateTime.New( today.Year, BirthMonth.Value, BirthDay.Value );
                    if ( nextBirthDay.HasValue && nextBirthDay.Value.CompareTo( today ) < 0 )
                    {
                        nextBirthDay = RockDateTime.New( today.Year + 1, BirthMonth.Value, BirthDay.Value );
                    }

                    return nextBirthDay;
                }

                return null;
            }
            private set {
                // intentionally blank
            }

        }

        /// <summary>
        /// Gets the number of days until the Person's birthday. This is an in-memory calculation. If needed in a LinqToSql query
        /// use DaysUntilBirthday property instead
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the number of days until the Person's birthday. If the person's birthdate is
        /// not available returns Int.MaxValue
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual int DaysToBirthday
        {
            get {
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

            private set {
                // intentionally blank
            }
        }

        /// <summary>
        /// Gets the Person's precise age (includes the fraction of the year).
        /// </summary>
        /// <value>
        /// A <see cref="System.Double"/> representing the Person's age (including fraction of year)
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual double? AgePrecise
        {
            get {
                DateTime? bday = this.BirthDate;
                if ( this.BirthYear.HasValue && bday.HasValue )
                {
                    // Calculate years
                    DateTime today = RockDateTime.Today;
                    int years = today.Year - bday.Value.Year;
                    if ( bday > today.AddYears( -years ) )
                    {
                        years--;
                    }

                    // Calculate days between last and next bday (differs on leap years).
                    DateTime lastBday = bday.Value.AddYears( years );
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
        /// Gets the number of days until the Person's anniversary. This is an in-memory calculation. If needed in a LinqToSql query
        /// use DaysUntilAnniversary property instead
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the number of days until the Person's anniversary. If the person's anniversary
        /// is not available returns Int.MaxValue
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual int DaysToAnniversary
        {
            get {
                if ( AnniversaryDate.HasValue )
                {
                    var today = RockDateTime.Today;

                    int day = AnniversaryDate.Value.Day;
                    int month = AnniversaryDate.Value.Month;
                    int year = today.Year;
                    if ( month < today.Month || ( month == today.Month && day < today.Day ) )
                    {
                        year++;
                    }

                    DateTime aday = DateTime.MinValue;
                    while ( !DateTime.TryParse( month.ToString() + "/" + day.ToString() + "/" + year.ToString(), out aday ) && day > 28 )
                    {
                        day--;
                    }

                    if ( aday != DateTime.MinValue )
                    {
                        return Convert.ToInt32( aday.Subtract( today ).TotalDays );
                    }
                }

                return int.MaxValue;
            }
            private set {
                // intentionally blank
            }

        }

        /// <summary>
        /// Gets the next anniversary.
        /// </summary>
        /// <value>
        /// The next anniversary.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual DateTime? NextAnniversary
        {
            get {
                if ( AnniversaryDate.HasValue )
                {
                    var today = RockDateTime.Today;
                    var nextAnniversary = RockDateTime.New( today.Year, AnniversaryDate.Value.Month, AnniversaryDate.Value.Day );
                    if ( nextAnniversary.HasValue && nextAnniversary.Value.CompareTo( today ) < 0 )
                    {
                        nextAnniversary = RockDateTime.New( today.Year + 1, AnniversaryDate.Value.Month, AnniversaryDate.Value.Day );
                    }

                    return nextAnniversary;
                }

                return null;
            }
            private set {
                // intentionally blank
            }

        }

        /// <summary>
        /// Gets or sets the number of days until their next anniversary. This is a computed column and can be used
        /// in LinqToSql queries, but there is no in-memory calculation. Avoid using property outside of
        /// a linq query. Use DaysToAnniversary instead.
        /// NOTE: If their anniversary is Feb 29, and this isn't a leap year, it'll treat Feb 28th as their anniversary when doing this calculation
        /// </summary>
        /// <value>
        /// The number of days until their next anniversary
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        public int? DaysUntilAnniversary { get; set; }

        /// <summary>
        /// Gets or sets the grade offset, which is the number of years until their graduation date.  This is used to determine which Grade (Defined Value) they are in
        /// </summary>
        /// <value>
        /// The grade offset.
        /// </value>
        [NotMapped]
        [DataMember]
        [RockClientInclude( "The Grade Offset of the person, which is the number of years until their graduation date. See GradeFormatted to see their current Grade. [Readonly]" )]
        public virtual int? GradeOffset
        {
            get {
                return GradeOffsetFromGraduationYear( GraduationYear );
            }

            set {
                GraduationYear = GraduationYearFromGradeOffset( value );
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
            get {
                return HasGraduatedFromGradeOffset( GradeOffset );
            }

            private set {
                // intentionally blank
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
            get {
                return GradeFormattedFromGradeOffset( GradeOffset );
            }

            private set {
                // intentionally blank
            }
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the default ExpireDateTime and UsageLimit.
        /// Returns the encrypted URLEncoded Token along with the ImpersonationParameter key in the form of "rckipid={ImpersonationParameter}"
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the impersonation parameter.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual string ImpersonationParameter
        {
            get {
                return this.GetImpersonationParameter();
            }
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the default ExpireDateTime and UsageLimit.
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid.
        /// NOTE: Use the GetImpersonationParameter(...) methods to specify an expiration date, usage limit or pageid
        /// </summary>
        /// <value>
        /// A <see cref="T:System.String" /> that represents a URL friendly version of the entity's unique key.
        /// </value>
        [NotMapped]
        public override string EncryptedKey
        {
            get {
                // in the case of Person, use an encrypted PersonToken instead of the base.UrlEncodedKey
                return this.GetImpersonationToken();
            }
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the default ExpireDateTime and UsageLimit.
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid.
        /// NOTE: Use the GetImpersonationParameter(...) methods to specify an expiration date, usage limit or pageid
        /// </summary>
        /// <value>
        /// A <see cref="T:System.String" /> that represents a URL friendly version of the entity's unique key.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public override string UrlEncodedKey
        {
            get {
                // in the case of Person, use an encrypted PersonToken instead of the base.UrlEncodedKey
                return this.GetImpersonationToken();
            }
        }

        /// <summary>
        /// Gets a value indicating whether [allows interactive bulk indexing].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allows interactive bulk indexing]; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        public bool AllowsInteractiveBulkIndexing
        {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the history changes.
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        private History.HistoryChangeList HistoryChanges { get; set; }

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
        /// Creates and stores a new PersonToken for a person using the default ExpireDateTime and UsageLimit.
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid.
        /// </summary>
        /// <returns></returns>
        public virtual string GetImpersonationToken()
        {
            return GetImpersonationToken( null, null, null );
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the default ExpireDateTime and UsageLimit.
        /// Returns the encrypted URLEncoded Token along with the ImpersonationParameter key in the form of "rckipid={ImpersonationParameter}"
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// A <see cref="System.String" /> representing the impersonation parameter.
        /// </value>
        public virtual string GetImpersonationParameter()
        {
            return GetImpersonationParameter( null, null, null );
        }

        /// <summary>
        /// Builds an encrypted action identifier string for the person instance.
        /// Returns the encrypted URLEncoded identifier"
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// A <see cref="System.String" /> representing the person action identifier.
        /// </value>
        public virtual string GetPersonActionIdentifier( string action )
        {
            var encryptedToken = Rock.Security.Encryption.EncryptString( $"{Guid}>{action}" );

            // do a Replace('%', '!') after we UrlEncode it (to make it more safely embeddable in HTML and cross browser compatible)
            return System.Web.HttpUtility.UrlEncode( encryptedToken ).Replace( '%', '!' );
        }

        /// <summary>
        /// Builds an encrypted action identifier stringfor the person instance.
        /// Returns the encrypted URLEncoded Token along with the identifier key in the form of "rckipid={PersonActionIdentifier}"
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// A <see cref="System.String" /> representing the person action identifier.
        /// </value>
        public virtual string GetPersonActionIdentifierParameter( string action )
        {
            return $"rckid={GetPersonActionIdentifier( action )}";
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the specified ExpireDateTime, UsageLimit, and Page
        /// Returns the encrypted URLEncoded Token along with the ImpersonationParameter key in the form of "rckipid={ImpersonationParameter}"
        /// </summary>
        /// <param name="expireDateTime">The expire date time.</param>
        /// <param name="usageLimit">The usage limit.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        /// <value>
        /// A <see cref="System.String" /> representing the impersonation parameter.
        /// </value>
        public virtual string GetImpersonationParameter( DateTime? expireDateTime, int? usageLimit, int? pageId )
        {
            return $"rckipid={GetImpersonationToken( expireDateTime, usageLimit, pageId )}";
        }

        /// <summary>
        /// Creates and stores a new PersonToken for a person using the specified ExpireDateTime, UsageLimit, and Page
        /// Returns the encrypted URLEncoded Token which can be used as a rckipid.
        /// </summary>
        /// <returns></returns>
        public virtual string GetImpersonationToken( DateTime? expireDateTime, int? usageLimit, int? pageId )
        {
            return PersonToken.CreateNew( this.PrimaryAlias, expireDateTime, usageLimit, pageId );
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
        /// <param name="rockUrlRoot">The rock URL root.</param>
        /// <param name="cssClass">The CSS class.</param>
        /// <param name="preText">The pre text.</param>
        /// <param name="postText">The post text.</param>
        /// <param name="styles">The styles.</param>
        /// <returns></returns>
        /// <value>
        /// The email tag.
        /// </value>
        public string GetEmailTag( string rockUrlRoot, string cssClass = "", string preText = "", string postText = "", string styles = "" )
        {
            return GetEmailTag( rockUrlRoot, null, cssClass, preText, postText, styles );
        }

        /// <summary>
        /// Gets an anchor tag to send person a communication
        /// </summary>
        /// <param name="rockUrlRoot">The rock URL root.</param>
        /// <param name="communicationPageReference">The communication page reference.</param>
        /// <param name="cssClass">The CSS class.</param>
        /// <param name="preText">The pre text.</param>
        /// <param name="postText">The post text.</param>
        /// <param name="styles">The styles.</param>
        /// <returns></returns>
        /// <value>
        /// The email tag.
        /// </value>
        public string GetEmailTag( string rockUrlRoot, Rock.Web.PageReference communicationPageReference, string cssClass = "", string preText = "", string postText = "", string styles = "" )
        {
            if ( !string.IsNullOrWhiteSpace( Email ) )
            {
                if ( IsEmailActive )
                {
                    rockUrlRoot.EnsureTrailingBackslash();

                    // get email link preference (new communication/mailto)
                    var globalAttributes = GlobalAttributesCache.Get();
                    string emailLinkPreference = globalAttributes.GetValue( "PreferredEmailLinkType" );

                    string emailLink = string.Empty;

                    // create link
                    if ( string.IsNullOrWhiteSpace( emailLinkPreference ) || emailLinkPreference == "1" )
                    {
                        if ( communicationPageReference != null )
                        {
                            communicationPageReference.QueryString = new System.Collections.Specialized.NameValueCollection( communicationPageReference.QueryString ?? new System.Collections.Specialized.NameValueCollection() );
                            communicationPageReference.QueryString["person"] = this.Id.ToString();
                            emailLink = new Rock.Web.PageReference( communicationPageReference.PageId, communicationPageReference.RouteId, communicationPageReference.Parameters, communicationPageReference.QueryString ).BuildUrl();
                        }
                        else
                        {
                            emailLink = string.Format( "{0}Communication?person={1}", rockUrlRoot, Id );
                        }
                    }
                    else
                    {
                        emailLink = string.Format( "mailto:{0}", Email );
                    }

                    switch ( EmailPreference )
                    {
                        case EmailPreference.EmailAllowed:
                            {
                                return string.Format(
                                    "<a class='{0}' style='{1}' href='{2}'>{3} {4} {5}</a>",
                                    cssClass,
                                    styles,
                                    emailLink,
                                    preText,
                                    Email,
                                    postText );
                            }

                        case EmailPreference.NoMassEmails:
                            {
                                return string.Format(
                                    "<span class='js-email-status email-status no-mass-email' data-toggle='tooltip' data-placement='top' title='Email Preference is set to \"No Mass Emails\"'><a class='{0}' href='{1}'>{2} {3} {4} <i class='fa fa-exchange'></i></a> </span>",
                                    cssClass,
                                    emailLink,
                                    preText,
                                    Email,
                                    postText );
                            }

                        case EmailPreference.DoNotEmail:
                            {
                                return string.Format(
                                    "<span class='{0} js-email-status email-status do-not-email' data-toggle='tooltip' data-placement='top' title='Email Preference is set to \"Do Not Email\"'>{1} {2} {3} <i class='fa fa-ban'></i></span>",
                                    cssClass,
                                    preText,
                                    Email,
                                    postText );
                            }
                    }
                }
                else
                {
                    return string.Format(
                        "<span class='js-email-status not-active email-status' data-toggle='tooltip' data-placement='top' title='Email is not active. {0}'>{1} <i class='fa fa-exclamation-triangle'></i></span>",
                        EmailNote,
                        Email );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Uploads the person's photo from the specified url and sets it as the person's Photo using the default BinaryFileType.
        /// </summary>
        /// <param name="photoUri">The photo URI.</param>
        public void SetPhotoFromUrl( Uri photoUri )
        {
            this.SetPhotoFromUrl( photoUri, Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid() );
        }

        /// <summary>
        /// Uploads the person's photo from the specified url and sets it as the person's Photo using the specified BinaryFileType.
        /// </summary>
        /// <param name="photoUri">The photo URI.</param>
        /// <param name="binaryFileTypeGuid">The binary file type unique identifier.</param>
        public void SetPhotoFromUrl( Uri photoUri, Guid binaryFileTypeGuid )
        {
            try
            {
                HttpWebRequest imageRequest = ( HttpWebRequest ) HttpWebRequest.Create( photoUri );
                HttpWebResponse imageResponse = ( HttpWebResponse ) imageRequest.GetResponse();
                var imageStream = imageResponse.GetResponseStream();
                using ( var rockContext = new RockContext() )
                {
                    var binaryFileType = new BinaryFileTypeService( rockContext ).GetNoTracking( binaryFileTypeGuid );
                    using ( MemoryStream photoData = new MemoryStream() )
                    {
                        imageStream.CopyTo( photoData );
                        var fileName = this.FullName.RemoveSpaces().MakeValidFileName();
                        if ( fileName.IsNullOrWhiteSpace() )
                        {
                            fileName = "PersonPhoto";
                        }

                        var binaryFile = new BinaryFile()
                        {
                            FileName = fileName,
                            MimeType = imageResponse.ContentType,
                            BinaryFileTypeId = binaryFileType.Id,
                            IsTemporary = true
                        };

                        binaryFile.SetStorageEntityTypeId( binaryFileType.StorageEntityTypeId );

                        byte[] photoDataBytes = photoData.ToArray();
                        binaryFile.FileSize = photoDataBytes.Length;
                        binaryFile.ContentStream = new MemoryStream( photoDataBytes );

                        var binaryFileService = new BinaryFileService( rockContext );
                        binaryFileService.Add( binaryFile );
                        rockContext.SaveChanges();

                        this.PhotoId = binaryFile.Id;
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( $"Unable to set photo from {photoUri},", ex );
            }
        }

        /// <summary>
        /// Creates a <see cref="System.Collections.Generic.Dictionary{String, Object}"/> of the Person object
        /// </summary>
        /// <returns>A <see cref="System.Collections.Generic.Dictionary{String, Object}"/> of the Person object.</returns>
        public override Dictionary<string, object> ToDictionary()
        {
            var dictionary = base.ToDictionary();
            dictionary.AddOrIgnore( "Age", AgePrecise );
            dictionary.AddOrIgnore( "DaysToBirthday", DaysToBirthday );
            dictionary.AddOrIgnore( "DaysToAnniversary", DaysToAnniversary );
            dictionary.AddOrIgnore( "FullName", FullName );
            dictionary.AddOrIgnore( "PrimaryAliasId", this.PrimaryAliasId );
            return dictionary;
        }

        /// <summary>
        /// Pres the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, DbEntityEntry entry )
        {
            var rockContext = ( RockContext ) dbContext;

            var inactiveStatus = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );
            var deceased = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_DECEASED.AsGuid() );

            if ( inactiveStatus != null && deceased != null )
            {
                bool isInactive = ( RecordStatusValueId.HasValue && RecordStatusValueId.Value == inactiveStatus.Id ) ||
                    ( RecordStatusValue != null && RecordStatusValue.Id == inactiveStatus.Id );
                bool isReasonDeceased = ( RecordStatusReasonValueId.HasValue && RecordStatusReasonValueId.Value == deceased.Id ) ||
                    ( RecordStatusReasonValue != null && RecordStatusReasonValue.Id == deceased.Id );

                IsDeceased = isInactive && isReasonDeceased;

                if ( isInactive )
                {
                    var dbPropertyEntry = entry.Property( "RecordStatusValueId" );
                    if ( dbPropertyEntry != null && dbPropertyEntry.IsModified )
                    {   
                        // If person was just inactivated, update the group member status for all their group memberships to be inactive
                        foreach ( var groupMember in new GroupMemberService( rockContext )
                            .Queryable()
                            .Where( m =>
                                m.PersonId == Id &&
                                m.GroupMemberStatus != GroupMemberStatus.Inactive &&
                                !m.Group.GroupType.IgnorePersonInactivated ) )
                        {
                            groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
                        }

                        // Also update the person's connection requests
                        int[] aliasIds = Aliases.Select( a => a.Id ).ToArray();
                        foreach ( var connectionRequest in new ConnectionRequestService( rockContext )
                            .Queryable()
                            .Where( c =>
                                 aliasIds.Contains( c.PersonAliasId ) &&
                                 c.ConnectionState != ConnectionState.Inactive &&
                                 c.ConnectionState != ConnectionState.Connected ) )
                        {
                            connectionRequest.ConnectionState = ConnectionState.Inactive;
                        }
                    }
                }
            }

            RecordTypeValueId = RecordTypeValueId ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            RecordStatusValueId = RecordStatusValueId ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            ConnectionStatusValueId = ConnectionStatusValueId ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() ).Id;

            if ( string.IsNullOrWhiteSpace( NickName ) )
            {
                NickName = FirstName;
            }

            if ( PhotoId.HasValue )
            {
                BinaryFileService binaryFileService = new BinaryFileService( ( RockContext ) dbContext );
                var binaryFile = binaryFileService.Get( PhotoId.Value );
                if ( binaryFile != null && binaryFile.IsTemporary )
                {
                    binaryFile.IsTemporary = false;
                }
            }

            // ensure person has a PersonAlias/PrimaryAlias
            if ( !this.Aliases.Any() || !this.Aliases.Any( a => a.AliasPersonId == this.Id ) )
            {
                this.Aliases.Add( new PersonAlias { AliasPerson = this, AliasPersonGuid = this.Guid, Guid = Guid.NewGuid() } );
            }

            if ( this.AnniversaryDate.HasValue )
            {
                if ( this.MaritalStatusValueId != DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED ).Id )
                {
                    this.AnniversaryDate = null;
                }
                else
                {
                    var dbPropertyEntry = entry.Property( "AnniversaryDate" );
                    if ( dbPropertyEntry != null && dbPropertyEntry.IsModified )
                    {
                        var spouse = this.GetSpouse( ( RockContext ) dbContext );
                        if ( spouse != null && spouse.AnniversaryDate != this.AnniversaryDate )
                        {
                            spouse.AnniversaryDate = this.AnniversaryDate;
                        }
                    }
                }
            }

            // Calculates the BirthDate and sets it
            this.BirthDate = this.CalculateBirthDate();

            CalculateSignals();

            if ( this.IsValid )
            {
                var transaction = new Rock.Transactions.SaveMetaphoneTransaction( this );
                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
            }

            HistoryChanges = new History.HistoryChangeList();

            switch ( entry.State )
            {
                case EntityState.Added:
                    {
                        HistoryChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Person" ).SetNewValue( this.FullName );

                        History.EvaluateChange( HistoryChanges, "Record Type", ( int? ) null, RecordTypeValue, RecordTypeValueId );
                        History.EvaluateChange( HistoryChanges, "Record Status", ( int? ) null, RecordStatusValue, RecordStatusValueId );
                        History.EvaluateChange( HistoryChanges, "Record Status Reason", ( int? ) null, RecordStatusReasonValue, RecordStatusReasonValueId );
                        History.EvaluateChange( HistoryChanges, "Inactive Reason Note", string.Empty, InactiveReasonNote );
                        History.EvaluateChange( HistoryChanges, "Connection Status", ( int? ) null, ConnectionStatusValue, ConnectionStatusValueId );
                        History.EvaluateChange( HistoryChanges, "Review Reason", ( int? ) null, ReviewReasonValue, ReviewReasonValueId );
                        History.EvaluateChange( HistoryChanges, "Review Reason Note", string.Empty, ReviewReasonNote );
                        History.EvaluateChange( HistoryChanges, "Deceased", ( bool? ) null, IsDeceased );
                        History.EvaluateChange( HistoryChanges, "Title", ( int? ) null, TitleValue, TitleValueId );
                        History.EvaluateChange( HistoryChanges, "First Name", string.Empty, FirstName );
                        History.EvaluateChange( HistoryChanges, "Nick Name", string.Empty, NickName );
                        History.EvaluateChange( HistoryChanges, "Middle Name", string.Empty, MiddleName );
                        History.EvaluateChange( HistoryChanges, "Last Name", string.Empty, LastName );
                        History.EvaluateChange( HistoryChanges, "Suffix", ( int? ) null, SuffixValue, SuffixValueId );
                        History.EvaluateChange( HistoryChanges, "Birth Date", null, BirthDate );
                        History.EvaluateChange( HistoryChanges, "Gender", null, Gender );
                        History.EvaluateChange( HistoryChanges, "Marital Status", ( int? ) null, MaritalStatusValue, MaritalStatusValueId );
                        History.EvaluateChange( HistoryChanges, "Anniversary Date", null, AnniversaryDate );
                        History.EvaluateChange( HistoryChanges, "Graduation Year", null, GraduationYear );
                        History.EvaluateChange( HistoryChanges, "Giving Id", null, GivingId );
                        History.EvaluateChange( HistoryChanges, "Email", string.Empty, Email );
                        History.EvaluateChange( HistoryChanges, "Email Active", ( bool? ) null, IsEmailActive );
                        History.EvaluateChange( HistoryChanges, "Email Note", string.Empty, EmailNote );
                        History.EvaluateChange( HistoryChanges, "Email Preference", null, EmailPreference );
                        History.EvaluateChange( HistoryChanges, "Communication Preference", null, CommunicationPreference );
                        History.EvaluateChange( HistoryChanges, "System Note", string.Empty, SystemNote );

                        if ( PhotoId.HasValue )
                        {
                            HistoryChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, "Photo" );
                        }

                        // ensure a new person has an Alternate Id
                        int alternateValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() ).Id;
                        var personSearchKeyService = new PersonSearchKeyService( rockContext );
                        PersonSearchKey personSearchKey = new PersonSearchKey()
                        {
                            PersonAlias = this.Aliases.First(),
                            SearchTypeValueId = alternateValueId,
                            SearchValue = PersonSearchKeyService.GenerateRandomAlternateId( true, rockContext )
                        };
                        personSearchKeyService.Add( personSearchKey );

                        break;
                    }

                case EntityState.Modified:
                    {
                        History.EvaluateChange( HistoryChanges, "Record Type", entry.OriginalValues["RecordTypeValueId"].ToStringSafe().AsIntegerOrNull(), RecordTypeValue, RecordTypeValueId );
                        History.EvaluateChange( HistoryChanges, "Record Status", entry.OriginalValues["RecordStatusValueId"].ToStringSafe().AsIntegerOrNull(), RecordStatusValue, RecordStatusValueId );
                        History.EvaluateChange( HistoryChanges, "Record Status Reason", entry.OriginalValues["RecordStatusReasonValueId"].ToStringSafe().AsIntegerOrNull(), RecordStatusReasonValue, RecordStatusReasonValueId );
                        History.EvaluateChange( HistoryChanges, "Inactive Reason Note", entry.OriginalValues["InactiveReasonNote"].ToStringSafe(), InactiveReasonNote );
                        History.EvaluateChange( HistoryChanges, "Connection Status", entry.OriginalValues["ConnectionStatusValueId"].ToStringSafe().AsIntegerOrNull(), ConnectionStatusValue, ConnectionStatusValueId );
                        History.EvaluateChange( HistoryChanges, "Review Reason", entry.OriginalValues["ReviewReasonValueId"].ToStringSafe().AsIntegerOrNull(), ReviewReasonValue, ReviewReasonValueId );
                        History.EvaluateChange( HistoryChanges, "Review Reason Note", entry.OriginalValues["ReviewReasonNote"].ToStringSafe(), ReviewReasonNote );
                        History.EvaluateChange( HistoryChanges, "Deceased", entry.OriginalValues["IsDeceased"].ToStringSafe().AsBoolean(), IsDeceased );
                        History.EvaluateChange( HistoryChanges, "Title", entry.OriginalValues["TitleValueId"].ToStringSafe().AsIntegerOrNull(), TitleValue, TitleValueId );
                        History.EvaluateChange( HistoryChanges, "First Name", entry.OriginalValues["FirstName"].ToStringSafe(), FirstName );
                        History.EvaluateChange( HistoryChanges, "Nick Name", entry.OriginalValues["NickName"].ToStringSafe(), NickName );
                        History.EvaluateChange( HistoryChanges, "Middle Name", entry.OriginalValues["MiddleName"].ToStringSafe(), MiddleName );
                        History.EvaluateChange( HistoryChanges, "Last Name", entry.OriginalValues["LastName"].ToStringSafe(), LastName );
                        History.EvaluateChange( HistoryChanges, "Suffix", entry.OriginalValues["SuffixValueId"].ToStringSafe().AsIntegerOrNull(), SuffixValue, SuffixValueId );
                        History.EvaluateChange( HistoryChanges, "Birth Date", entry.OriginalValues["BirthDate"].ToStringSafe().AsDateTime(), BirthDate );
                        History.EvaluateChange( HistoryChanges, "Gender", entry.OriginalValues["Gender"].ToStringSafe().ConvertToEnum<Gender>(), Gender );
                        History.EvaluateChange( HistoryChanges, "Marital Status", entry.OriginalValues["MaritalStatusValueId"].ToStringSafe().AsIntegerOrNull(), MaritalStatusValue, MaritalStatusValueId );
                        History.EvaluateChange( HistoryChanges, "Anniversary Date", entry.OriginalValues["AnniversaryDate"].ToStringSafe().AsDateTime(), AnniversaryDate );
                        History.EvaluateChange( HistoryChanges, "Graduation Year", entry.OriginalValues["GraduationYear"].ToStringSafe().AsIntegerOrNull(), GraduationYear );
                        History.EvaluateChange( HistoryChanges, "Giving Id", entry.OriginalValues["GivingId"].ToStringSafe(), GivingId );
                        History.EvaluateChange( HistoryChanges, "Email", entry.OriginalValues["Email"].ToStringSafe(), Email );
                        History.EvaluateChange( HistoryChanges, "Email Active", entry.OriginalValues["IsEmailActive"].ToStringSafe().AsBoolean(), IsEmailActive );
                        History.EvaluateChange( HistoryChanges, "Email Note", entry.OriginalValues["EmailNote"].ToStringSafe(), EmailNote );
                        History.EvaluateChange( HistoryChanges, "Email Preference", entry.OriginalValues["EmailPreference"].ToStringSafe().ConvertToEnum<EmailPreference>(), EmailPreference );
                        History.EvaluateChange( HistoryChanges, "Communication Preference", entry.OriginalValues["CommunicationPreference"].ToStringSafe().ConvertToEnum<CommunicationType>(), CommunicationPreference );
                        History.EvaluateChange( HistoryChanges, "System Note", entry.OriginalValues["SystemNote"].ToStringSafe(), SystemNote );

                        int? originalPhotoId = entry.OriginalValues["PhotoId"].ToStringSafe().AsIntegerOrNull();
                        if ( originalPhotoId.HasValue )
                        {
                            if ( PhotoId.HasValue )
                            {
                                if ( PhotoId.Value != originalPhotoId.Value )
                                {
                                    HistoryChanges.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "Photo" );
                                }
                            }
                            else
                            {
                                HistoryChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Property, "Photo" );
                            }
                        }
                        else if ( PhotoId.HasValue )
                        {
                            HistoryChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Property, "Photo" );
                        }

                        if ( entry.OriginalValues["Email"].ToStringSafe() != Email )
                        {
                            var currentEmail = entry.OriginalValues["Email"].ToStringSafe();
                            if ( !string.IsNullOrEmpty( currentEmail ) )
                            {
                                var personSearchKeyService = new PersonSearchKeyService( rockContext );
                                var searchTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_EMAIL.AsGuid() );
                                if ( !personSearchKeyService.Queryable().Any( a => a.PersonAlias.PersonId == Id && a.SearchTypeValueId == searchTypeValue.Id && a.SearchValue.Equals( currentEmail, StringComparison.OrdinalIgnoreCase ) ) )
                                {
                                    PersonSearchKey personSearchKey = new PersonSearchKey()
                                    {
                                        PersonAliasId = PrimaryAliasId.Value,
                                        SearchTypeValueId = searchTypeValue.Id,
                                        SearchValue = currentEmail
                                    };
                                    personSearchKeyService.Add( personSearchKey );

                                }
                            }
                        }

                        if ( entry.OriginalValues["RecordStatusValueId"].ToStringSafe().AsIntegerOrNull() != RecordStatusValueId )
                        {
                            RecordStatusLastModifiedDateTime = RockDateTime.Now;
                        }

                        break;
                    }

                case EntityState.Deleted:
                    {
                        HistoryChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, null );

                        // If PersonRecord is getting deleted, don't do any of the remaining presavechanges
                        return;
                    }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Posts the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            if ( HistoryChanges != null && HistoryChanges.Any() )
            {
                HistoryService.SaveChanges( ( RockContext ) dbContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), this.Id, HistoryChanges, true, this.ModifiedByPersonAliasId );
            }

            base.PostSaveChanges( dbContext );

            // NOTE: This is also done on GroupMember.PostSaveChanges in case Role or family membership changes
            PersonService.UpdatePersonAgeClassification( this.Id, dbContext as RockContext );
            PersonService.UpdatePrimaryFamily( this.Id, dbContext as RockContext );
            PersonService.UpdateGivingLeaderId( this.Id, dbContext as RockContext );
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
            if ( person != null && person.Guid.Equals( this.Guid ) )
            {
                return true;
            }
            else
            {
                return base.IsAuthorized( action, person );
            }
        }

        /// <summary>
        /// Gets the campus.
        /// </summary>
        /// <returns></returns>
        public Campus GetCampus()
        {
            var firstFamily = this.GetFamily();
            return firstFamily != null ? firstFamily.Campus : null;
        }

        /// <summary>
        /// Gets the campus ids for all the families that a person belongs to.
        /// </summary>
        /// <returns></returns>
        public List<int> GetCampusIds()
        {
            return this.GetFamilies()
                .Where( f => f.CampusId.HasValue )
                .Select( f => f.CampusId.Value )
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Calculates the top-most signal and updates the person properties.
        /// </summary>
        public void CalculateSignals()
        {
            if ( Signals != null )
            {
                var rockContext = new RockContext();
                var topSignal = Signals
                    .Select( s => new
                    {
                        Id = s.Id,
                        SignalType = SignalTypeCache.Get( s.SignalTypeId )
                    } )
                    .OrderBy( s => s.SignalType.Order )
                    .ThenBy( s => s.SignalType.Id )
                    .FirstOrDefault();

                TopSignalId = topSignal?.Id;
                TopSignalIconCssClass = topSignal?.SignalType.SignalIconCssClass;
                TopSignalColor = topSignal?.SignalType.SignalColor;
            }
        }

        /// <summary>
        /// Gets the phone number.
        /// </summary>
        /// <param name="phoneType">Type of the phone.</param>
        /// <returns></returns>
        public PhoneNumber GetPhoneNumber( Guid phoneType )
        {
            int numberTypeValueId = DefinedValueCache.GetId( phoneType ) ?? 0;
            return PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == numberTypeValueId );
        }

        #endregion

        #region Static Helper Methods

        /// <summary>
        /// Gets the family salutation.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="includeChildren">if set to <c>true</c> [include children].</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <param name="useFormalNames">if set to <c>true</c> [use formal name].</param>
        /// <param name="finalSeparator">The final separator.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        public static string GetFamilySalutation( Person person, bool includeChildren = false, bool includeInactive = true, bool useFormalNames = false, string finalSeparator = "&", string separator = "," )
        {
            var _familyType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var _adultRole = _familyType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) );
            var _childRole = _familyType.Roles.FirstOrDefault( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) );
            var _deceased = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_REASON_DECEASED ).Id;

            // clean up the separators
            finalSeparator = $" {finalSeparator} "; // add spaces before and after
            if ( separator == "," )
            {
                separator = $"{separator} "; // add space after
            }
            else
            {
                separator = $" {separator} "; // add spaces before and after
            }

            List<string> familyMemberNames = new List<string>();
            string primaryLastName = string.Empty;

            var familyMembers = person.GetFamilyMembers( true ).Where( f => f.Person.RecordStatusReasonValueId != _deceased ).ToList();

            // filter for inactive
            if ( !includeInactive )
            {
                var activeRecordStatusId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE ).Id;
                familyMembers = familyMembers.Where( f => f.Person.RecordStatusValueId == activeRecordStatusId ).ToList();
            }

            // check that a family even existed if not return their name
            if ( familyMembers.Count > 0 )
            {
                // filter out kids if not needed
                if ( !includeChildren )
                {
                    familyMembers = familyMembers.Where( f => f.GroupRoleId == _adultRole.Id ).ToList();
                }

                // determine if more than one last name is at play
                var multipleLastNamesExist = familyMembers.Select( f => f.Person.LastName ).Distinct().Count() > 1;

                // add adults and children separately as adults need to be sorted by gender and children by age

                // adults
                var adults = familyMembers.Where( f => f.GroupRoleId == _adultRole.Id ).OrderBy( f => f.Person.Gender );

                if ( adults.Count() > 0 )
                {
                    primaryLastName = adults.First().Person.LastName;

                    foreach ( var adult in adults.Select( f => f.Person ) )
                    {
                        var firstName = adult.NickName;

                        if ( useFormalNames )
                        {
                            firstName = adult.FirstName;
                        }

                        if ( !multipleLastNamesExist )
                        {
                            familyMemberNames.Add( firstName );
                        }
                        else
                        {
                            familyMemberNames.Add( $"{firstName} {adult.LastName}" );
                        }
                    }
                }

                // children
                if ( includeChildren )
                {
                    var children = familyMembers.Where( f => f.GroupRoleId == _childRole.Id ).OrderByDescending( f => f.Person.Age );

                    if ( primaryLastName.IsNullOrWhiteSpace() )
                    {
                        primaryLastName = children.First().Person.LastName;
                    }

                    if ( children.Count() > 0 )
                    {
                        foreach ( var child in children.Select( f => f.Person ) )
                        {
                            var firstName = child.NickName;

                            if ( useFormalNames )
                            {
                                firstName = child.FirstName;
                            }

                            if ( !multipleLastNamesExist )
                            {
                                familyMemberNames.Add( firstName );
                            }
                            else
                            {
                                familyMemberNames.Add( $"{firstName} {child.LastName}" );
                            }
                        }
                    }
                }

                var familySalutation = string.Join( separator, familyMemberNames ).ReplaceLastOccurrence( separator, finalSeparator );

                if ( !multipleLastNamesExist )
                {
                    familySalutation = familySalutation + " " + primaryLastName;
                }

                return familySalutation;

            }

            return $"{( useFormalNames ? person.FirstName : person.NickName )} {person.LastName}";
        }

        /// <summary>
        /// Gets the person photo URL.
        /// </summary>
        /// <param name="person">The person to get the photo for.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <returns></returns>
        public static string GetPersonPhotoUrl( Person person, int? maxWidth = null, int? maxHeight = null )
        {
            return GetPersonPhotoUrl( person.Id, person.PhotoId, person.Age, person.Gender, person.RecordTypeValueId.HasValue ? DefinedValueCache.Get( person.RecordTypeValueId.Value ).Guid : (Guid?)null, person.AgeClassification, maxWidth, maxHeight );
        }

        /// <summary>
        /// Gets the person photo URL from a person id (warning this will cause a database lookup).
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <returns></returns>
        public static string GetPersonPhotoUrl( int personId, int? maxWidth = null, int? maxHeight = null )
        {
            using ( RockContext rockContext = new RockContext() )
            {
                Person person = new PersonService( rockContext ).Get( personId );
                return GetPersonPhotoUrl( person, maxWidth, maxHeight );
            }
        }

        /// <summary>
        /// Gets the 'NoPictureUrl' for the person based on their Gender, Age, and RecordType (Person or Business)
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetPersonNoPictureUrl( Person person, int? maxWidth = null, int? maxHeight = null )
        {
            return GetPersonPhotoUrl( person.Id, null, person.Age, person.Gender, person.RecordTypeValueId.HasValue ? DefinedValueCache.Get( person.RecordTypeValueId.Value ).Guid : ( Guid? ) null, person.AgeClassification, maxWidth, maxHeight );
        }

        /// <summary>
        /// Gets the person photo URL.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="age">The age.</param>
        /// <param name="gender">The gender to use if the photoId is null.</param>
        /// <param name="recordTypeValueGuid">The record type value unique identifier.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete("Use other GetPersonPhotoUrl")]
        public static string GetPersonPhotoUrl(int? personId, int? photoId, int? age, Gender gender, Guid? recordTypeValueGuid, int? maxWidth = null, int? maxHeight = null )
        {
            return GetPersonPhotoUrl( personId, photoId, age, gender, recordTypeValueGuid, null, maxWidth, maxHeight );
        }

        /// <summary>
        /// Gets the person photo URL.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="age">The age.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="recordTypeValueGuid">The record type value unique identifier.</param>
        /// <param name="ageClassification">The age classification.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetPersonPhotoUrl( int? personId, int? photoId, int? age, Gender gender, Guid? recordTypeValueGuid, AgeClassification? ageClassification, int? maxWidth = null, int? maxHeight = null )
        {
            string virtualPath = string.Empty;
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

                virtualPath = string.Format( "~/GetImage.ashx?id={0}" + widthHeightParams, photoId );
            }
            else
            {
                if ( recordTypeValueGuid.HasValue && recordTypeValueGuid.Value == SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
                {
                    virtualPath = "~/Assets/Images/business-no-photo.svg?";
                }
                else if ( age.HasValue && age.Value < 18 )
                {
                    // it's a child
                    virtualPath = $"~/{GetPhotoPath( gender, false )}";
                }
                else
                {
                    // check age classification
                    if ( ageClassification == null )
                    {
                        if ( personId.HasValue )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                ageClassification = new PersonService( rockContext ).Queryable( true ).Where( a => a.Id == personId ).Select( a => ( AgeClassification? ) a.AgeClassification ).FirstOrDefault();
                            }
                        }
                    }

                    if ( ageClassification.HasValue && ageClassification == AgeClassification.Child )
                    {
                        // it's a child
                        virtualPath = $"~/{GetPhotoPath( gender, false )}";
                    }
                    else
                    {
                        // it's an adult
                        virtualPath = $"~/{GetPhotoPath( gender, true )}";

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
        /// Gets the photo path based on the gender and adult/child status.
        /// </summary>
        /// <param name="gender">The gender.</param>
        /// <param name="isAdult">if set to <c>true</c> is adult.</param>
        /// <returns></returns>
        private static string GetPhotoPath( Gender gender, bool isAdult )
        {
            if ( isAdult )
            {
                switch ( gender )
                {
                    case Gender.Female:
                        {
                            return "Assets/Images/person-no-photo-female.svg?";
                        }
                    case Gender.Male:
                        {
                            return "Assets/Images/person-no-photo-male.svg?";
                        }
                    default:
                        {
                            return "Assets/Images/person-no-photo-unknown.svg?";
                        }
                }
            }

            switch ( gender )
            {
                case Gender.Female:
                    {
                        return "Assets/Images/person-no-photo-child-female.svg?";
                    }
                case Gender.Male:
                    {
                        return "Assets/Images/person-no-photo-child-male.svg?";
                    }
                default:
                    {
                        return "Assets/Images/person-no-photo-child-unknown.svg?";
                    }
            }
        }

        /// <summary>
        /// Gets the person image tag.
        /// </summary>
        /// <param name="person">The person to get the image for.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <param name="altText">The alt text to use on the image.</param>
        /// <param name="className">The css class name to apply to the image.</param>
        /// <returns></returns>
        public static string GetPersonPhotoImageTag( Person person, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            if ( person != null )
            {
                return GetPersonPhotoImageTag( person.Id, person.PhotoId, person.Age, person.Gender, person.RecordTypeValue != null ? ( Guid? ) person.RecordTypeValue.Guid : null, maxWidth, maxHeight, altText, className );
            }
            else
            {
                return GetPersonPhotoImageTag( null, null, null, Gender.Unknown, null, maxWidth, maxHeight, altText, className );
            }
        }

        /// <summary>
        /// Gets the person image tag.
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <param name="altText">The alt text to use on the image.</param>
        /// <param name="className">The css class name to apply to the image.</param>
        /// <returns></returns>
        public static string GetPersonPhotoImageTag( PersonAlias personAlias, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {

            Person person = personAlias != null ? personAlias.Person : null;
            return GetPersonPhotoImageTag( person, maxWidth, maxHeight, altText, className );
        }

        /// <summary>
        /// Gets the person image tag.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="age">The age.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="recordTypeValueGuid">The record type value unique identifier.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <param name="altText">The alt text to use on the image.</param>
        /// <param name="className">The css class name to apply to the image.</param>
        /// <returns></returns>
        public static string GetPersonPhotoImageTag( int? personId, int? photoId, int? age, Gender gender, Guid? recordTypeValueGuid, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            var photoUrl = new StringBuilder();

            photoUrl.Append( VirtualPathUtility.ToAbsolute( "~/" ) );

            string styleString = string.Empty;

            string altString = string.IsNullOrWhiteSpace( altText ) ? string.Empty :
                string.Format( " alt='{0}'", altText );

            string classString = string.IsNullOrWhiteSpace( className ) ? string.Empty :
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
                    photoUrl.Append( "Assets/Images/business-no-photo.svg?" );
                }
                else if ( age.HasValue && age.Value < 18 )
                {
                    // it's a child
                    photoUrl.Append( GetPhotoPath( gender, false ) );
                }
                else
                {
                    // check age classification
                    AgeClassification? ageClassification = null;
                    if ( personId.HasValue )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            ageClassification = new PersonService( rockContext ).GetSelect( personId.Value, s => s.AgeClassification );
                        }
                    }

                    if ( ageClassification.HasValue && ageClassification == AgeClassification.Child )
                    {
                        // it's a child
                        photoUrl.Append( GetPhotoPath( gender, false ) );
                    }
                    else
                    {
                        // it's an adult
                        photoUrl.Append( GetPhotoPath( gender, true ) );
                    }
                }
            }

            return string.Format( "<img src='{0}'{1}{2}{3}/>", photoUrl.ToString(), styleString, altString, classString );
        }
        
        /// <summary>
        /// Gets the HTML markup to use for displaying the top-most signal icon for this person.
        /// </summary>
        /// <returns>A string that represents the Icon to display or an empty string if no signal is active.</returns>
        public string GetSignalMarkup()
        {
            return Person.GetSignalMarkup( TopSignalColor, TopSignalIconCssClass );
        }

        /// <summary>
        /// Gets the HTML markup to use for displaying the given signal color and icon.
        /// </summary>
        /// <returns>A string that represents the Icon to display or an empty string if no signal color was provided.</returns>
        public static string GetSignalMarkup( string signalColor, string signalIconCssClass )
        {
            if ( !string.IsNullOrWhiteSpace( signalColor ) )
            {
                return string.Format( "<i class='{1}' style='color: {0};'></i>",
                    signalColor,
                    !string.IsNullOrWhiteSpace( signalIconCssClass ) ? signalIconCssClass : "fa fa-flag" );
            }

            return string.Empty;
        }

        /// <summary>
        /// Adds the related person to the selected person's known relationships with a role of 'Can check in' which
        /// is typically configured to allow check-in.  If an inverse relationship is configured for 'Can check in'
        /// (i.e. 'Allow check in by'), that relationship will also be created.
        /// </summary>
        /// <param name="personId">A <see cref="System.Int32" /> representing the Id of the Person.</param>
        /// <param name="relatedPersonId">A <see cref="System.Int32" /> representing the Id of the related Person.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void CreateCheckinRelationship( int personId, int relatedPersonId, RockContext rockContext = null )
        {
            var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS );
            var canCheckInRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN ) ) );
            if ( canCheckInRole != null )
            {
                rockContext = rockContext ?? new RockContext();
                var groupMemberService = new GroupMemberService( rockContext );
                groupMemberService.CreateKnownRelationship( personId, relatedPersonId, canCheckInRole.Id );
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

            if ( !string.IsNullOrWhiteSpace( suffix ) )
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
        /// <param name="recordTypeValueId">The record type value identifier.</param>
        /// <returns></returns>
        public static string FormatFullName( string nickName, string lastName, int? suffixValueId, int? recordTypeValueId = null )
        {
            if ( IsBusiness( recordTypeValueId ) )
            {
                return lastName;
            }

            if ( suffixValueId.HasValue )
            {
                var suffix = DefinedValueCache.Get( suffixValueId.Value );
                if ( suffix != null )
                {
                    return FormatFullName( nickName, lastName, suffix.Value );
                }
            }

            return FormatFullName( nickName, lastName, string.Empty );
        }

        /// <summary>
        /// Given a grade offset, returns the graduation year
        /// </summary>
        /// <param name="gradeOffset"></param>
        /// <returns></returns>
        public static int? GraduationYearFromGradeOffset( int? gradeOffset )
        {
            if ( gradeOffset.HasValue && gradeOffset.Value >= 0 )
            {
                return RockDateTime.CurrentGraduationYear + gradeOffset.Value;
            }

            return null;
        }

        /// <summary>
        /// Given a graduation year returns the grade offset
        /// </summary>
        /// <param name="graduationYear">The graduation year.</param>
        /// <returns></returns>
        public static int? GradeOffsetFromGraduationYear( int? graduationYear )
        {
            if ( !graduationYear.HasValue )
            {
                return null;
            }
            else
            {
                return graduationYear.Value - RockDateTime.CurrentGraduationYear;
            }
        }

        /// <summary>
        /// Determines whether person has graduated based on grade offset
        /// </summary>
        /// <param name="gradeOffset">The grade offset.</param>
        /// <returns></returns>
        public static bool? HasGraduatedFromGradeOffset( int? gradeOffset )
        {
            if ( gradeOffset.HasValue )
            {
                return gradeOffset < 0;
            }

            return null;
        }

        /// <summary>
        /// Formats the grade based on graduation year
        /// </summary>
        /// <param name="graduationYear">The graduation year.</param>
        /// <returns></returns>
        public static string GradeFormattedFromGraduationYear( int? graduationYear )
        {
            return GradeFormattedFromGradeOffset( GradeOffsetFromGraduationYear( graduationYear ) );
        }

        /// <summary>
        /// Formats the grade based on grade offset
        /// </summary>
        /// <param name="gradeOffset">The grade offset.</param>
        /// <returns></returns>
        public static string GradeFormattedFromGradeOffset( int? gradeOffset )
        {
            if ( gradeOffset.HasValue && gradeOffset >= 0 )
            {
                var schoolGrades = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() );
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

        /// <summary>
        /// Gets the home locations for all the person id's passed in. If a person is in
        /// more than one family or that family has more than one home address a single
        /// location is provided.
        /// </summary>
        /// <param name="personIds">The person ids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static Dictionary<int, Location> GetHomeLocations( List<int> personIds, RockContext rockContext = null )
        {
            var personHomeAddresses = new Dictionary<int, Location>();

            if ( personIds != null )
            {

                rockContext = rockContext ?? new RockContext();

                Guid? homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuidOrNull();
                Guid? familyGuid = new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );

                if ( homeAddressGuid.HasValue && familyGuid.HasValue )
                {
                    var homeAddressDv = DefinedValueCache.Get( homeAddressGuid.Value );
                    var familyGroupType = GroupTypeCache.Get( familyGuid.Value );
                    if ( homeAddressDv != null && familyGroupType != null )
                    {
                        var personLocations = new GroupMemberService( rockContext ).Queryable()
                                .Where( m =>
                                     personIds.Contains( m.PersonId )
                                     && m.Group.GroupTypeId == familyGroupType.Id )
                                .Select( m => new
                                {
                                    m.PersonId,
                                    Location = m.Group.GroupLocations
                                                                     .Where( gl => gl.GroupLocationTypeValueId == homeAddressDv.Id )
                                                                     .Select( gl => gl.Location )
                                                                     .FirstOrDefault()
                                } ).ToList();

                        foreach ( var personLocation in personLocations )
                        {
                            if ( !personHomeAddresses.ContainsKey( personLocation.PersonId ) )
                            {
                                personHomeAddresses.Add( personLocation.PersonId, personLocation.Location );
                            }
                        }
                    }
                }
            }

            return personHomeAddresses;
        }

        #endregion

        #region Indexing Methods
        /// <summary>
        /// Bulks the index documents.
        /// </summary>
        public void BulkIndexDocuments()
        {
            List<IndexModelBase> indexableItems = new List<IndexModelBase>();

            var recordTypePersonId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            var recordTypeBusinessId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

            RockContext rockContext = new RockContext();

            // return people
            var people = new PersonService( rockContext ).Queryable().AsNoTracking()
                                .Where( p => p.RecordTypeValueId == recordTypePersonId );

            int recordCounter = 0;

            foreach ( var person in people )
            {
                recordCounter++;

                var indexablePerson = PersonIndex.LoadByModel( person );
                indexableItems.Add( indexablePerson );

                if ( recordCounter > 100 )
                {
                    IndexContainer.IndexDocuments( indexableItems );
                    indexableItems = new List<IndexModelBase>();
                    recordCounter = 0;
                }
            }

            // return businesses
            var businesses = new PersonService( rockContext ).Queryable().AsNoTracking()
                                .Where( p =>
                                     p.IsSystem == false
                                     && p.RecordTypeValueId == recordTypeBusinessId );

            foreach ( var business in businesses )
            {
                var indexableBusiness = BusinessIndex.LoadByModel( business );
                indexableItems.Add( indexableBusiness );

                if ( recordCounter > 100 )
                {
                    IndexContainer.IndexDocuments( indexableItems );
                    indexableItems = new List<IndexModelBase>();
                    recordCounter = 0;
                }
            }

            IndexContainer.IndexDocuments( indexableItems );
        }

        /// <summary>
        /// Deletes the indexed documents.
        /// </summary>
        public void DeleteIndexedDocuments()
        {
            IndexContainer.DeleteDocumentsByType<PersonIndex>();
            IndexContainer.DeleteDocumentsByType<BusinessIndex>();
        }

        /// <summary>
        /// Indexes the name of the model.
        /// </summary>
        /// <returns></returns>
        public Type IndexModelType()
        {
            return typeof( PersonIndex );
        }

        /// <summary>
        /// Indexes the document.
        /// </summary>
        /// <param name="id"></param>
        public void IndexDocument( int id )
        {
            var personEntity = new PersonService( new RockContext() ).Get( id );

            if ( personEntity != null )
            {
                if ( personEntity.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() )
                {
                    var indexItem = PersonIndex.LoadByModel( personEntity );
                    IndexContainer.IndexDocument( indexItem );
                }
                else if ( personEntity.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
                {
                    var indexItem = BusinessIndex.LoadByModel( personEntity );
                    IndexContainer.IndexDocument( indexItem );
                }
            }
        }

        /// <summary>
        /// Deletes the indexed document.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteIndexedDocument( int id )
        {
            var personEntity = new PersonService( new RockContext() ).Get( id );

            if ( personEntity != null )
            {
                if ( personEntity.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() )
                {
                    Type indexType = Type.GetType( "Rock.UniversalSearch.IndexModels.PersonIndex" );
                    IndexContainer.DeleteDocumentById( indexType, id );
                }
                else if ( personEntity.RecordTypeValue.Guid == Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
                {
                    Type indexType = Type.GetType( "Rock.UniversalSearch.IndexModels.PersonIndex" );
                    IndexContainer.DeleteDocumentById( indexType, id );
                }
            }
        }

        /// <summary>
        /// Gets the index filter values.
        /// </summary>
        /// <returns></returns>
        public ModelFieldFilterConfig GetIndexFilterConfig()
        {
            return new ModelFieldFilterConfig() { FilterLabel = "", FilterField = "" };
        }

        /// <summary>
        /// Gets the index filter field.
        /// </summary>
        /// <returns></returns>
        public bool SupportsIndexFieldFiltering()
        {
            return false;
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
    /// The age classification of a person
    /// </summary>
    public enum AgeClassification
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Adult
        /// </summary>
        Adult = 1,

        /// <summary>
        /// Child
        /// </summary>
        Child = 2
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
        /// Gets the families sorted by the person's GroupOrder (GroupMember.GroupOrder)
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
        /// Gets the family for the person. If multiple families the first family based on the GroupOrder value of the GroupMember
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static Group GetFamily( this Person person, RockContext rockContext = null )
        {
            // If PrimaryFamily has been calculated, use that. Otherwise, get it from GetFamilies()
            if ( person.PrimaryFamily != null )
            {
                return person.PrimaryFamily;
            }
            else
            {
                return person.GetFamilies( rockContext ).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the home location.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static Location GetHomeLocation( this Person person, RockContext rockContext = null )
        {
            Guid? homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuidOrNull();
            if ( homeAddressGuid.HasValue )
            {
                var homeAddressDv = DefinedValueCache.Get( homeAddressGuid.Value );
                if ( homeAddressDv != null )
                {
                    foreach ( var family in person.GetFamilies( rockContext ) )
                    {
                        var loc = family.GroupLocations
                            .Where( l =>
                                l.GroupLocationTypeValueId == homeAddressDv.Id &&
                                l.IsMappedLocation )
                            .Select( l => l.Location )
                            .FirstOrDefault();
                        if ( loc != null )
                        {
                            return loc;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the most ideal mailing location from among this person's families
        /// </summary>
        /// <param name="person">The person to find a mailing address for</param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        public static Location GetMailingLocation( this Person person, RockContext rockContext = null )
        {
            // Get the mailing address from this person's giving group if there is one
            if ( person.GivingGroup != null )
            {
                var mailingLocation = person.GivingGroup.GetBestMailingLocation();
                if ( mailingLocation != null )
                {
                    return mailingLocation;
                }
            }

            return person.GetFamilies( rockContext ).GetBestMailingLocation();
        }

        /// <summary>
        /// Returns the most ideal mailing location from a single family
        /// </summary>
        /// <param name="group">The family to find addresses on</param>
        /// <returns></returns>
        private static Location GetBestMailingLocation( this Group group )
        {
            return GetBestMailingLocation( new List<Group> { group } );
        }

        /// <summary>
        /// Returns the most ideal mailing location from among the selected family groups
        /// </summary>
        /// <param name="groups">The families to find addresses on</param>
        /// <returns></returns>
        private static Location GetBestMailingLocation( this IEnumerable<Group> groups )
        {
            if ( groups.Any() )
            {
                var homeAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuidOrNull();
                var workAddressGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuidOrNull();
                if ( homeAddressGuid.HasValue && workAddressGuid.HasValue )
                {
                    var homeAddressDv = DefinedValueCache.Get( homeAddressGuid.Value );
                    var workAddressDv = DefinedValueCache.Get( workAddressGuid.Value );
                    if ( homeAddressDv != null && workAddressDv != null )
                    {
                        // Get all available mailing locations, prioritizing mapped locations then home locations
                        var mailingLocations = groups.SelectMany( x => x.GroupLocations )
                            .Where( l => l.IsMailingLocation )
                            .Where( l => l.GroupLocationTypeValueId == homeAddressDv.Id || l.GroupLocationTypeValueId == workAddressDv.Id )
                            .OrderBy( l => l.IsMappedLocation ? 0 : 1 )
                            .ThenBy( l => l.GroupLocationTypeValueId == homeAddressDv.Id ? 0 : 1 );

                        return mailingLocations.Select( l => l.Location ).FirstOrDefault();
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Updates, adds or removes a PhoneNumber of the given type.
        /// </summary>
        public static void UpdatePhoneNumber( this Person person, int numberTypeValueId, string phoneCountryCode, string phoneNumber, bool? isMessagingEnabled, bool? isUnlisted, RockContext rockContext )
        {
            // try to find the phone number based on the typeGuid.
            var phoneObject = person.PhoneNumbers
                .Where( p =>
                    p.NumberTypeValueId.HasValue &&
                    p.NumberTypeValueId.Value == numberTypeValueId )
                .FirstOrDefault();

            // Since only one number can be used for SMS, before anything else, if isMessagingEnabled is true, turn it off on ALL
            // numbers, so we only enable it for this one.
            if ( isMessagingEnabled.HasValue && isMessagingEnabled.Value == true )
            {
                foreach ( PhoneNumber currPhoneNumber in person.PhoneNumbers )
                {
                    currPhoneNumber.IsMessagingEnabled = false;
                }
            }

            // do they currently have this type of number?
            if ( phoneObject != null )
            {
                // if the text field is blank, we'll delete this phone number type from their list.
                if ( string.IsNullOrWhiteSpace( phoneNumber ) )
                {
                    person.PhoneNumbers.Remove( phoneObject );

                    var phoneNumberService = new PhoneNumberService( rockContext );
                    phoneNumberService.Delete( phoneObject );
                }
                else
                {
                    // otherwise update it with the new info
                    phoneObject.CountryCode = PhoneNumber.CleanNumber( phoneCountryCode );
                    phoneObject.Number = PhoneNumber.CleanNumber( phoneNumber );

                    // for an existing number, if they don't provide messaging / unlisted, use the current values.
                    phoneObject.IsMessagingEnabled = isMessagingEnabled ?? phoneObject.IsMessagingEnabled;
                    phoneObject.IsUnlisted = isUnlisted ?? phoneObject.IsUnlisted;
                }
            }
            // they don't have a number of this type. If one is being added, we'll add it.
            // (otherwise we'll just do nothing, leaving it as it)
            else if ( !string.IsNullOrWhiteSpace( phoneNumber ) )
            {
                // create a new phone number and add it to their list.
                phoneObject = new PhoneNumber();
                person.PhoneNumbers.Add( phoneObject );

                var phoneNumberService = new PhoneNumberService( rockContext );
                phoneNumberService.Add( phoneObject );

                // get the typeId for this phone number so we set it correctly
                //var numberType = DefinedValueCache.Get( phoneTypeGuid );
                phoneObject.NumberTypeValueId = numberTypeValueId;

                phoneObject.CountryCode = PhoneNumber.CleanNumber( phoneCountryCode );
                phoneObject.Number = PhoneNumber.CleanNumber( phoneNumber );

                // for a new number, if they don't specify messaging / unlisted, assume no texting and not unlisted.
                phoneObject.IsMessagingEnabled = isMessagingEnabled ?? false;
                phoneObject.IsUnlisted = isUnlisted ?? false;
            }
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
        /// Gets the group members.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<GroupMember> GetGroupMembers( this Person person, int groupTypeId, bool includeSelf = false, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetGroupMembers( groupTypeId, person != null ? person.Id : 0, includeSelf );
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
        /// Gets any search keys for this person
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<PersonSearchKey> GetPersonSearchKeys( this Person person, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetPersonSearchKeys( person != null ? person.Id : 0 );
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
        /// Gets the <see cref="Rock.Model.Person" /> entity of the provided Person's head of household.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person" /> entity of the Person to retrieve the head of household of.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Person" /> entity containing the provided Person's head of household. If the provided Person's head of household is not found, this value will be null.
        /// </returns>
        public static Person GetHeadOfHousehold( this Person person, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetHeadOfHousehold( person );
        }

        /// <summary>
        /// Gets the family role (adult or child).
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static GroupTypeRole GetFamilyRole( this Person person, RockContext rockContext = null )
        {
            if ( rockContext == null )
            {
                rockContext = new RockContext();
            }

            return new PersonService( rockContext ).GetFamilyRole( person, rockContext );
        }

        /// <summary>
        /// Gets a Person's spouse with a selector that lets you only fetch the properties that you need
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="person">The <see cref="Rock.Model.Person" /> entity of the Person to retrieve the spouse of.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>
        /// The <see cref="Rock.Model.Person" /> entity containing the provided Person's spouse. If the provided Person's spouse is not found, this value will be null.
        /// </returns>
        public static TResult GetSpouse<TResult>( this Person person, System.Linq.Expressions.Expression<Func<GroupMember, TResult>> selector, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetSpouse( person, selector );
        }

        /// <summary>
        /// Gets the businesses.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<Person> GetBusinesses( this Person person, RockContext rockContext = null )
        {
            return new PersonService( rockContext ?? new RockContext() ).GetBusinesses( person.Id );
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

        /// <summary>
        /// Limits the PersonQry to people that have an Grade Offset that is between MinGradeOffset and MaxGradeOffset (inclusive)
        /// </summary>
        /// <param name="personQry">The person qry.</param>
        /// <param name="minGradeOffset">The minimum grade offset.</param>
        /// <param name="maxGradeOffset">The maximum grade offset.</param>
        /// <param name="includePeopleWithNoGrade">if set to <c>true</c> [include people with no Grade].</param>
        /// <returns></returns>
        public static IQueryable<Person> WhereGradeOffsetRange( this IQueryable<Person> personQry, int? minGradeOffset, int? maxGradeOffset, bool includePeopleWithNoGrade = true )
        {
            var currentGradYear = RockDateTime.CurrentGraduationYear;

            var qryWithGradeOffset = personQry.Select(
                      p => new
                      {
                          Person = p,
                          GradeOffset = p.GraduationYear.HasValue ? p.GraduationYear.Value - currentGradYear : ( int? ) null
                      } );

            if ( includePeopleWithNoGrade )
            {
                if ( minGradeOffset.HasValue )
                {
                    qryWithGradeOffset = qryWithGradeOffset.Where( a => !a.GradeOffset.HasValue || a.GradeOffset >= minGradeOffset );
                }

                if ( maxGradeOffset.HasValue )
                {
                    qryWithGradeOffset = qryWithGradeOffset.Where( a => !a.GradeOffset.HasValue || a.GradeOffset <= maxGradeOffset );
                }
            }
            else
            {
                if ( minGradeOffset.HasValue )
                {
                    qryWithGradeOffset = qryWithGradeOffset.Where( a => a.GradeOffset.HasValue && a.GradeOffset >= minGradeOffset );
                }

                if ( maxGradeOffset.HasValue )
                {
                    qryWithGradeOffset = qryWithGradeOffset.Where( a => a.GradeOffset.HasValue && a.GradeOffset <= maxGradeOffset );
                }
            }

            return qryWithGradeOffset.Select( a => a.Person );
        }
    }

    #endregion
}
