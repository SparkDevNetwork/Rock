//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Person POCO Entity.
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
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Record Type Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_RECORD_TYPE )]
        public int? RecordTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the Record Status Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_RECORD_STATUS )]
        public int? RecordStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the Record Status Reason Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_RECORD_STATUS_REASON )]
        public int? RecordStatusReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets the Person Status Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_STATUS )]
        public int? PersonStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets whether the person is deceased.
        /// </summary>
        /// <value>
        /// deceased.
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
        /// Gets or sets the Title Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_TITLE )]
        public int? TitleValueId { get; set; }

        /// <summary>
        /// Gets or sets the Given Name.
        /// </summary>
        /// <value>
        /// Given Name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [Previewable]
        [MergeField]
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets the Nick Name.
        /// </summary>
        /// <value>
        /// Nick Name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [MergeField]
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the Middle Name.
        /// </summary>
        /// <value>
        /// Middle Name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [MergeField]
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the Last Name.
        /// </summary>
        /// <value>
        /// Last Name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [Previewable]
        [MergeField]
        public string LastName { get; set; }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        /// <value>
        /// The full name.
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
        /// Gets NickName if not null, otherwise gets GivenName.
        /// </summary>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
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
        /// Gets the full name (Last, First)
        /// </summary>
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
        /// Gets or sets the Suffix Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_SUFFIX )]
        public int? SuffixValueId { get; set; }

        /// <summary>
        /// Gets or sets the Photo Id.
        /// </summary>
        /// <value>
        /// Photo Id.
        /// </value>
        [DataMember]
        public int? PhotoId { get; set; }

        /// <summary>
        /// Gets or sets the Birth Day.
        /// </summary>
        /// <value>
        /// Birth Day.
        /// </value>
        [DataMember]
        [MergeField]
        public int? BirthDay { get; set; }

        /// <summary>
        /// Gets or sets the Birth Month.
        /// </summary>
        /// <value>
        /// Birth Month.
        /// </value>
        [DataMember]
        [MergeField]
        public int? BirthMonth { get; set; }

        /// <summary>
        /// Gets or sets the Birth Year.
        /// </summary>
        /// <value>
        /// Birth Year.
        /// </value>
        [DataMember]
        [MergeField]
        public int? BirthYear { get; set; }

        /// <summary>
        /// Gets or sets the Gender.
        /// </summary>
        /// <value>
        /// Enum[Gender].
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        [MergeField]
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the Marital Status Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSON_MARITAL_STATUS )]
        public int? MaritalStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the Anniversary Date.
        /// </summary>
        /// <value>
        /// Anniversary Date.
        /// </value>
        [DataMember]
        [MergeField]
        [Column( TypeName = "Date" )]
        public DateTime? AnniversaryDate { get; set; }

        /// <summary>
        /// Gets or sets the Graduation Date.
        /// </summary>
        /// <value>
        /// Graduation Date.
        /// </value>
        [DataMember]
        [MergeField]
        [Column( TypeName = "Date" )]
        public DateTime? GraduationDate { get; set; }

        /// <summary>
        /// Gets or sets the Email.
        /// </summary>
        /// <value>
        /// Email.
        /// </value>
        [MaxLength( 75 )]
        [DataMember]
        [Previewable]
        [MergeField]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the Email Is Active.
        /// </summary>
        /// <value>
        /// Email Is Active.
        /// </value>
        [DataMember]
        public bool? IsEmailActive { get; set; }

        /// <summary>
        /// Gets or sets the Email Note.
        /// </summary>
        /// <value>
        /// Email Note.
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        [MergeField]
        public string EmailNote { get; set; }

        /// <summary>
        /// Gets or sets the Do Not Email.
        /// </summary>
        /// <value>
        /// Do Not Email.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [MergeField]
        public bool DoNotEmail { get; set; }

        /// <summary>
        /// Gets or sets the System Note.
        /// </summary>
        /// <value>
        /// System Note.
        /// </value>
        [MaxLength( 1000 )]
        [DataMember]
        [MergeField]
        public string SystemNote { get; set; }

        /// <summary>
        /// Gets or sets the Viewed Count.
        /// </summary>
        /// <value>
        /// Viewed Count.
        /// </value>
        [DataMember]
        public int? ViewedCount { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Users.
        /// </summary>
        /// <value>
        /// Collection of Users.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual ICollection<UserLogin> Users { get; set; }

        /// <summary>
        /// Gets or sets the Email Templates.
        /// </summary>
        /// <value>
        /// Collection of Email Templates.
        /// </value>
        [DataMember]
        public virtual ICollection<EmailTemplate> EmailTemplates { get; set; }

        /// <summary>
        /// Gets or sets the Phone Numbers.
        /// </summary>
        /// <value>
        /// Collection of Phone Numbers.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the Members.
        /// </summary>
        /// <value>
        /// Collection of Members.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual ICollection<GroupMember> Members { get; set; }

        /// <summary>
        /// Gets or sets the attendances.
        /// </summary>
        /// <value>
        /// The attendances.
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
        /// Gets or sets the Marital Status.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue MaritalStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the Person Status.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue PersonStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the Record Status.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue RecordStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the Record Status Reason.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue RecordStatusReasonValue { get; set; }

        /// <summary>
        /// Gets or sets the Record Type.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue RecordTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the Suffix.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue SuffixValue { get; set; }

        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual DefinedValue TitleValue { get; set; }

        /// <summary>
        /// Gets or sets the Photo
        /// </summary>
        [DataMember]
        [MergeField]
        public virtual BinaryFile Photo { get; set; }

        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        /// <value>
        /// The birth date.
        /// </value>
        [NotMapped]
        [DataMember]
        [MergeField]
        public virtual DateTime? BirthDate
        {
            // notes
            // if no birthday is available then DateTime.MinValue is returned
            // if no birth year is given then the birth year will be DateTime.MinValue.Year
            get
            {
                if ( BirthDay == null || BirthMonth == null )
                {
                    return null;
                }
                else
                {
                    string birthYear = ( BirthYear ?? DateTime.MinValue.Year ).ToString();
                    return Convert.ToDateTime( BirthMonth.ToString() + "/" + BirthDay.ToString() + "/" + birthYear );
                }
            }

            set
            {
                if ( value.HasValue )
                {
                    BirthMonth = value.Value.Month;
                    BirthDay = value.Value.Day;
                    BirthYear = value.Value.Year;
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
        /// Gets the age.
        /// </summary>
        /// <value>
        /// The age.
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
        /// Gets the fractional age
        /// </summary>
        /// <value>
        /// The age as double.
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
        /// The grade level or null if no graduation date.
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
                        GraduationDate = DateTime.Now.AddYears( gradeFactorReactor - value.Value );
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
        public virtual string ImpersonationParameter
        {
            get
            {
                return "rckipid=" + HttpUtility.UrlEncode( this.EncryptedKey );
            }
        }

        /// <summary>
        /// Gets the impersonated user.
        /// </summary>
        /// <value>
        /// The impersonated user.
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
        /// To the dictionary.
        /// </summary>
        /// <returns></returns>
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
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
        /// <param name="personId">The person id.</param>
        /// <param name="relatedPersonId">The related person id.</param>
        /// <param name="currentPersonId">The current person id.</param>
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
                    int? canCheckInRoleId = new GroupRoleService().Queryable()
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
        /// Gets the Family Members.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="includeSelf">if set to <c>true</c> [include self].</param>
        /// <returns></returns>
        public static IQueryable<GroupMember> GetFamilyMembers( this Person person, bool includeSelf = false )
        {
            return new PersonService().GetFamilyMembers( person, includeSelf );
        }

        /// <summary>
        /// Gets the Spouse.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public static Person GetSpouse( this Person person )
        {
            return new PersonService().GetSpouse( person );
        }

    }

    #endregion

}
