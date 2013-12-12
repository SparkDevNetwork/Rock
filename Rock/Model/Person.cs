//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a person or a business in RockChMS.  
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
        /// Gets or sets a flag indicating if this Person is part of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this Person is part of the RockChMS core system/framework. This property is required.
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
        /// Gets or sets the Id of the Record Status Reason <see cref="Rock.Model.DefinedValue"/> representing the reason why a person status would have a set status.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Record Status Reason <see cref="Rock.Model.DefinedValue"/> representing the reason why a person entity would have a set status.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON )]
        public int? RecordStatusReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Person Status <see cref="Rock.Model.DefinedValue"/> representing the status of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Person Status <see cref="Rock.Model.DefinedValue"/> representing the status of the Person.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_STATUS )]
        public int? PersonStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Person is deceased.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the Person is deceased; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        [MergeField]
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
        /// Gets or sets the given name of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the given name of the Person.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [MergeField]
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets the nick name of the Person. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the nick name of the Person.
        /// </value>
        /// <remarks>
        /// The name that the person goes by.
        /// </remarks>
        [MaxLength( 50 )]
        [DataMember]
        [MergeField]
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the middle name of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the middle name of the Person.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [MergeField]
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
        [MergeField]
        public string LastName { get; set; }

        /// <summary>
        /// Gets the Full Name of the Person using the Title FirstName LastName format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Full Name of a Person using the Title FirstName LastName format.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [MergeField]
        public string FullName
        {
            get
            {
                var fullName = new StringBuilder();

                if ( TitleValue != null && !string.IsNullOrWhiteSpace( TitleValue.Name ) )
                    fullName.AppendFormat( "{0} ", TitleValue.Name );

                fullName.AppendFormat( "{0} {1}", FirstName, LastName );

                if ( SuffixValue != null && !string.IsNullOrWhiteSpace( SuffixValue.Name ) )
                    fullName.AppendFormat( " {0}", SuffixValue.Name );

                return fullName.ToString();
            }

            // TODO: Private set currently not needed, but not possible to remove without a migration. 
            // Consider removing during migration flattening.
            private set { }
        }

        /// <summary>
        /// Gets the FirstName of the Person. It will use the NickName if one is provided, otherwise the GivenName.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the FirstName of the Person. If the NickName is not null, it will be returned; otherwise returns the GivenName.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [Previewable]
        [MergeField]
        public string FirstName
        {
            get
            {
                return string.IsNullOrEmpty( NickName ) ? GivenName : NickName;
            }

            // TODO: Private set currently not needed, but not possible to remove without a migration. 
            // Consider removing during migration flattening.
            private set { }
        }

        /// <summary>
        /// Gets the full name of the Person using the LastName, FirstName format.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full name of a Person using the LastName, FirstName format
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [MergeField]
        public string FullNameLastFirst
        {
            get
            {
                var fullName = new StringBuilder();
                fullName.Append( LastName );

                if ( SuffixValue != null && !string.IsNullOrWhiteSpace( SuffixValue.Name ) )
                    fullName.AppendFormat( " {0}", SuffixValue.Name );

                fullName.AppendFormat( ", {0}", FirstName );
                return fullName.ToString();
            }

            // TODO: Private set currently not needed, but not possible to remove without a migration. 
            // Consider removing during migration flattening.
            private set { }
        }

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
        [MergeField]
        public int? BirthDay { get; set; }

        /// <summary>
        /// Gets or sets the month portion of the Person's birth date.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the month portion of the Person's birth date. If the birth date is not known this value will be null.
        /// </value>
        [DataMember]
        [MergeField]
        public int? BirthMonth { get; set; }

        /// <summary>
        /// Gets or sets the year portion of the Person's birth date.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the year portion of the Person's birth date. If the birth date is not known this value will be null.
        /// </value>
        [DataMember]
        [MergeField]
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
        [MergeField]
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
        [MergeField]
        [Column( TypeName = "Date" )]
        public DateTime? AnniversaryDate { get; set; }

        /// <summary>
        /// Gets or sets the date of the Person's projected or actual high school graduation date. The month and date will match the "Grade Transition Date" global attribute. This value is used to determine what grade a student is in. 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the Person's projected or actual high school graduation date.  This value will be null if a Graduation Date is an adult, not known, not applicable or the 
        /// Person has not entered school.
        /// </value>
        [DataMember]
        [MergeField]
        [Column( TypeName = "Date" )]
        public DateTime? GraduationDate { get; set; }


        /// <summary>
        /// Gets or sets the giving group id.  If an individual would like their giving to be grouped with the rest of their family,
        /// this will be the id of their family group.  If they elect to contribute on their own, this value will be null.
        /// </summary>
        /// <value>
        /// The giving group id.
        /// </value>
        [DataMember]
        [MergeField]
        public int? GivingGroupId { get; set; }

        /// <summary>
        /// Gets or sets the Person's email address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the Person's email address.
        /// </value>
        [MaxLength( 75 )]
        [DataMember]
        [Previewable]
        [MergeField]
        [RegularExpression(@"[\w\.\'_%-]+(\+[\w-]*)?@([\w-]+\.)+[\w-]+", ErrorMessage= "The Email address is invalid")]
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
        [MergeField]
        public string EmailNote { get; set; }

        /// <summary>
        /// Gets or sets a flag that indicates that the Person does not want to receive email.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the Person does not wish to receive email; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [MergeField]
        public bool DoNotEmail { get; set; }

        /// <summary>
        /// Gets or sets the System Note
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a System Note.
        /// </value>
        [MaxLength( 1000 )]
        [DataMember]
        [MergeField]
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

        #region Virtual Properties

        /// <summary>
        /// Gets the first name of the last.
        /// </summary>
        /// <value>
        /// The first name of the last.
        /// </value>
        public virtual string FirstLastName
        {
            get { return string.Format( "{0} {1}", FirstName, LastName ); }
        }

        /// <summary>
        /// Gets the last name of the first.
        /// </summary>
        /// <value>
        /// The last name of the first.
        /// </value>
        public virtual string LastFirstName
        {
            get { return string.Format( "{0}, {1}", LastName, FirstName ); }
        }

        /// <summary>
        /// Gets or sets a collection containing the Person's <see cref="Rock.Model.UserLogin">UserLogins</see>.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.UserLogin">UserLogins</see> that belong to the Person.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual ICollection<UserLogin> Users
        {
            get { return _users ?? ( _users = new Collection<UserLogin>() ); }
            set { _users = value; }
        }
        private ICollection<UserLogin> _users;

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.EmailTemplate">EmailTemplates</see> that were created by this Person.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.EmailTemplate">EmailTemplates</see> that were created by this Person.
        /// </value>
        [DataMember]
        public virtual ICollection<EmailTemplate> EmailTemplates
        {
            get { return _emailTemplates ?? ( _emailTemplates = new Collection<EmailTemplate>() ); }
            set { _emailTemplates = value; }
        }
        private ICollection<EmailTemplate> _emailTemplates;

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.PhoneNumber">PhoneNumbers</see> 
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.PhoneNumber"/> entities representing the phone numbers that are associated with this Person.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual ICollection<PhoneNumber> PhoneNumbers
        {
            get { return _phoneNumbers ?? ( _phoneNumbers = new Collection<PhoneNumber>() ); }
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
        [DataMember]
        [MergeField]
        public virtual ICollection<GroupMember> Members
        {
            get { return _members ?? ( _members = new Collection<GroupMember>() ); }
            set { _members = value; }
        }
        private ICollection<GroupMember> _members;

        /// <summary>
        /// Gets or set a collection containing the Person's <see cref="Rock.Model.Attendance"/> history.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.Attendance"/> entities representing the Person's attendance history.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual ICollection<Attendance> Attendances
        {
            get { return _attendances ?? ( _attendances = new Collection<Attendance>() ); }
            set { _attendances = value; }
        }
        private ICollection<Attendance> _attendances;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Person's marital status.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> representing the Person's marital status.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue MaritalStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Person's membership status
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the Person's membership status. 
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue PersonStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the record status.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object representing the record status.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue RecordStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Record Status Reason.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> that represents the Record Status Reason (disposition)
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue RecordStatusReasonValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the RecordType.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> representing the record type.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue RecordTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Person's name suffix.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue" /> representing the name suffix.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue SuffixValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the Person's salutation title.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> object representing the Person's salutation title.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue TitleValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> that contains the Person's photo.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.BinaryFile"/> that contains the Person's photo.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual BinaryFile Photo { get; set; }

        /// <summary>
        /// Gets or sets the giving group.  The 
        /// </summary>
        /// <value>
        /// The giving group.
        /// </value>
        [DataMember]
        public virtual Group GivingGroup { get; set; }

        /// <summary>
        /// Gets or sets the Person's birth date.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the Person's birthdate.  If no birthdate is available, null is returned. If the year is not available then the birthdate is returned with the DateTime.MinValue.Year.
        /// </value>
        [NotMapped]
        [DataMember]
        [MergeField]
        public virtual DateTime? BirthDate
        {
            get
            {
                if ( BirthDay == null || BirthMonth == null )
                {
                    return null;
                }
                else
                {
                    return new DateTime( BirthYear ?? DateTime.MinValue.Year, BirthMonth.Value, BirthDay.Value );
                }
            }

            set
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
        }

        /// <summary>
        /// Gets the Person's age.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the person's age.  If the birthdate and age is not available then returns null.
        /// </value>
        [MergeField]
        public virtual int? Age
        {
            get
            {
                DateTime bday;
                if ( DateTime.TryParse( BirthMonth.ToString() + "/" + BirthDay.ToString() + "/" + BirthYear, out bday ) )
                {
                    DateTime today = DateTime.Today;
                    int age = today.Year - bday.Year;
                    if ( bday > today.AddYears( -age ) ) age--;
                    return age;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the number of days until the Person's birthday.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the number of days until the Person's birthday. If the person's birthdate is not available returns Int.MaxValue
        /// </value>
        [MergeField]
        public virtual int DaysToBirthday
        {
            get
            {
                if ( BirthDay == null || BirthMonth == null )
                {
                    return int.MaxValue;
                }
                else
                {
                    var today = DateTime.Today;
                    var birthdate = Convert.ToDateTime( BirthMonth.ToString() + "/" + BirthDay.ToString() + "/" + today.Year.ToString() );
                    if ( birthdate.CompareTo( today ) < 0 )
                    {
                        birthdate = birthdate.AddYears( 1 );
                    }

                    return Convert.ToInt32( birthdate.Subtract( today ).TotalDays );
                }
            }
        }

        /// <summary>
        /// Gets the Person's precise age (includes the fraction of the year).
        /// </summary>
        /// <value>
        /// A <see cref="System.Double"/> representing the Person's age (including fraction of year) 
        /// </value>
        public virtual double? AgePrecise
        {
            get
            {
                DateTime bday;
                if ( DateTime.TryParse( BirthMonth.ToString() + "/" + BirthDay.ToString() + "/" + BirthYear, out bday ) )
                {
                    // Calculate years
                    DateTime today = DateTime.Today;
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
        /// Gets the grade level of the person based on their high school graduation date.  Grade levels are -1 for prekindergarten, 0 for kindergarten, 1 for first grade, etc. or null if they have no graduation date or if no 'GradeTransitionDate' is configured.
        /// </summary>
        /// <value>
        /// The Person's grade level based on their Graduation Date. If no graduation date is provided or the GradeTransitionDate is not provided, returns null.
        /// </value>
        [NotMapped]
        [DataMember]
        [MergeField]
        public virtual int? Grade
        {
            get
            {
                if ( !GraduationDate.HasValue )
                {
                    return null;
                }
                else
                {
                    // Use the GradeTransitionDate (aka grade promotion date) to figure out what grade their in
                    DateTime transitionDate;
                    var globalAttributes = GlobalAttributesCache.Read();
                    if ( !DateTime.TryParse( globalAttributes.GetValue( "GradeTransitionDate" ), out transitionDate ) )
                    {
                        return null;
                    }

                    int gradeMaxFactorReactor = ( DateTime.Now < transitionDate ) ? 12 : 13;
                    return gradeMaxFactorReactor - ( GraduationDate.Value.Year - DateTime.Now.Year );
                }
            }

            set
            {
                if ( value.HasValue && value.Value <= 12 )
                {
                    DateTime transitionDate;
                    var globalAttributes = GlobalAttributesCache.Read();
                    if ( DateTime.TryParse( globalAttributes.GetValue( "GradeTransitionDate" ), out transitionDate ) )
                    {
                        int gradeFactorReactor = ( DateTime.Now < transitionDate ) ? 12 : 13;
                        GraduationDate = transitionDate.AddYears( gradeFactorReactor - value.Value );
                    }
                }
                else
                {
                    GraduationDate = null;
                }
            }
        }

        /// <summary>
        /// Gets the impersonation parameter.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the impersonation parameter.
        /// </value>
        public virtual string ImpersonationParameter
        {
            get
            {
                return "rckipid=" + HttpUtility.UrlEncode( this.EncryptedKey );
            }
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.UserLogin"/> of the user being impersonated.
        /// </summary>
        /// <value>
        /// Th <see cref="Rock.Model.UserLogin"/> of the user being impersonated.
        /// </value>
        public virtual UserLogin ImpersonatedUser
        {
            get
            {
                UserLogin user = new UserLogin();
                user.UserName = this.FullName;
                user.PersonId = this.Id;
                user.Person = this;
                return user;
            }
        }

        /// <summary>
        /// Creates a <see cref="System.Collections.Generic.Dictionary{String, Object}"/> of the Person object
        /// </summary>
        /// <returns>A <see cref="System.Collections.Generic.Dictionary{String, Object}"/> of the Person object.</returns>
        public override Dictionary<string, object> ToDictionary()
        {
            var dictionary = base.ToDictionary();
            dictionary.Add( "BirthDate", BirthDate );
            dictionary.Add( "Age", AgePrecise );
            dictionary.Add( "DaysToBirthday", DaysToBirthday );
            return dictionary;
        }

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

        #region Static Helper Methods

        /// <summary>
        /// Adds the related person to the selected person's known relationships with a role of 'Can check in' which
        /// is typically configured to allow check-in.  If an inverse relationship is configured for 'Can check in' 
        /// (i.e. 'Allow check in by'), that relationship will also be created.
        /// </summary>
        /// <param name="personId">A <see cref="System.Int32"/> representing the Id of the Person.</param>
        /// <param name="relatedPersonId">A <see cref="System.Int32"/> representing the Id of the related Person.</param>
        /// <param name="currentPersonId">A <see cref="System.Int32"/> representing the Id of the Person who is logged in.</param>
        public static void CreateCheckinRelationship( int personId, int relatedPersonId, int? currentPersonId )
        {
            using ( new UnitOfWorkScope() )
            {
                var groupMemberService = new GroupMemberService();
                var knownRelationshipGroup = groupMemberService.Queryable()
                    .Where( m =>
                        m.PersonId == personId &&
                        m.GroupRole.Guid.Equals( new Guid( SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ) )
                    .Select( m => m.Group )
                    .FirstOrDefault();

                if ( knownRelationshipGroup != null )
                {
                    int? canCheckInRoleId = new GroupTypeRoleService().Queryable()
                        .Where( r =>
                            r.Guid.Equals( new Guid( SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN ) ) )
                        .Select( r => r.Id )
                        .FirstOrDefault();
                    if ( canCheckInRoleId.HasValue )
                    {
                        var canCheckInMember = groupMemberService.Queryable()
                            .FirstOrDefault( m =>
                                m.GroupId == knownRelationshipGroup.Id &&
                                m.PersonId == relatedPersonId &&
                                m.GroupRoleId == canCheckInRoleId.Value );

                        if ( canCheckInMember == null )
                        {
                            canCheckInMember = new GroupMember();
                            canCheckInMember.GroupId = knownRelationshipGroup.Id;
                            canCheckInMember.PersonId = relatedPersonId;
                            canCheckInMember.GroupRoleId = canCheckInRoleId.Value;
                            groupMemberService.Add( canCheckInMember, currentPersonId );
                            groupMemberService.Save( canCheckInMember, currentPersonId );
                        }

                        var inverseGroupMember = groupMemberService.GetInverseRelationship( canCheckInMember, true, currentPersonId );
                        if ( inverseGroupMember != null )
                        {
                            groupMemberService.Save( inverseGroupMember, currentPersonId );
                        }
                    }
                }
            }
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
            this.HasOptional( p => p.PersonStatusValue ).WithMany().HasForeignKey( p => p.PersonStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RecordStatusValue ).WithMany().HasForeignKey( p => p.RecordStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RecordStatusReasonValue ).WithMany().HasForeignKey( p => p.RecordStatusReasonValueId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.RecordTypeValue ).WithMany().HasForeignKey( p => p.RecordTypeValueId ).WillCascadeOnDelete( false );
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
    /// A person's possible grade levels
    /// </summary>
    public enum GradeLevel
    {
        /// <summary>
        /// Kindergarten
        /// </summary>
        [Description( "Pre-K" )]
        PreK = -1,

        /// <summary>
        /// Kindergarten
        /// </summary>
        [Description( "Kindergarten" )]
        Kindergarten = 0,

        /// <summary>
        /// 1st Grade
        /// </summary>
        [Description( "1st Grade" )]
        First = 1,

        /// <summary>
        /// 2nd Grade
        /// </summary>
        [Description( "2nd Grade" )]
        Second = 2,

        /// <summary>
        /// 3rd Grade
        /// </summary>
        [Description( "3rd Grade" )]
        Third = 3,

        /// <summary>
        /// 4th Grade
        /// </summary>
        [Description( "4th Grade" )]
        Fourth = 4,

        /// <summary>
        /// 5th Grade
        /// </summary>
        [Description( "5th Grade" )]
        Fifth = 5,

        /// <summary>
        /// 6th Grade
        /// </summary>
        [Description( "6th Grade" )]
        Sixth = 6,

        /// <summary>
        /// 7th Grade
        /// </summary>
        [Description( "7th Grade" )]
        Seventh = 7,

        /// <summary>
        /// 8th Grade
        /// </summary>
        [Description( "8th Grade" )]
        Eighth = 8,

        /// <summary>
        /// 9th Grade
        /// </summary>
        [Description( "9th Grade" )]
        Ninth = 9,

        /// <summary>
        /// 10th Grade
        /// </summary>
        [Description( "10th Grade" )]
        Tenth = 10,

        /// <summary>
        /// 11th Grade
        /// </summary>
        [Description( "11th Grade" )]
        Eleventh = 11,

        /// <summary>
        /// 12th Grade
        /// </summary>
        [Description( "12th Grade" )]
        Twelfth = 12
    }

    #endregion

    #region Extension Methods

    public static partial class PersonExtensionMethods
    {
        /// <summary>
        /// Gets the families.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static IQueryable<Group> GetFamilies( this Person person )
        {
            return new PersonService().GetFamilies( person );
        }

        /// <summary>
        /// Gets a queryable collection of <see cref="Rock.Model.Person"/> entities containing the Person's family.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> to retrieve family members for.</param>
        /// <param name="includeSelf">A <see cref="System.Boolean"/> value that is <c>true</c> if the provided person should be returned in the results, otherwise <c>false</c>.</param>
        /// <returns>Returns a queryable collection of <see cref="Rock.Model.Person"/> entities representing the provided Person's family.</returns>
        public static IQueryable<GroupMember> GetFamilyMembers( this Person person, bool includeSelf = false )
        {
            return new PersonService().GetFamilyMembers( person, includeSelf );
        }

        /// <summary>
        /// Gets the <see cref="Rock.Model.Person"/> entity of the provided Person's spouse.
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> entity of the Person to retrieve the spouse of.</param>
        /// <returns>The <see cref="Rock.Model.Person"/> entity containing the provided Person's spouse. If the provided Person's spouse is not found, this value will be null.</returns>
        public static Person GetSpouse( this Person person )
        {
            return new PersonService().GetSpouse( person );
        }

    }

    #endregion

}
