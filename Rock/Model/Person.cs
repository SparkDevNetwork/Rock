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
using System.Runtime.Serialization;
using System.Web;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Person POCO Entity.
    /// </summary>
    [Table( "Person" )]
    [DataContract( IsReference = true )]
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
        public int? RecordTypeValueId { get; set; }
        
        /// <summary>
        /// Gets or sets the Record Status Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        public int? RecordStatusValueId { get; set; }
        
        /// <summary>
        /// Gets or sets the Record Status Reason Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        public int? RecordStatusReasonValueId { get; set; }
        
        /// <summary>
        /// Gets or sets the Person Status Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        public int? PersonStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets whether the person is deceased.
        /// </summary>
        /// <value>
        /// deceased.
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
        /// Gets or sets the Title Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        public int? TitleValueId { get; set; }
        
        /// <summary>
        /// Gets or sets the Given Name.
        /// </summary>
        /// <value>
        /// Given Name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets the Nick Name.
        /// </summary>
        /// <value>
        /// Nick Name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the Last Name.
        /// </summary>
        /// <value>
        /// Last Name.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string LastName { get; set; }
        
        /// <summary>
        /// Gets or sets the Suffix Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
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
        public int? BirthDay { get; set; }
        
        /// <summary>
        /// Gets or sets the Birth Month.
        /// </summary>
        /// <value>
        /// Birth Month.
        /// </value>
        [DataMember]
        public int? BirthMonth { get; set; }
        
        /// <summary>
        /// Gets or sets the Birth Year.
        /// </summary>
        /// <value>
        /// Birth Year.
        /// </value>
        [DataMember]
        public int? BirthYear { get; set; }
        
        /// <summary>
        /// Gets or sets the Gender.
        /// </summary>
        /// <value>
        /// Enum[Gender].
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the Marital Status Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        public int? MaritalStatusValueId { get; set; }
        
        /// <summary>
        /// Gets or sets the Anniversary Date.
        /// </summary>
        /// <value>
        /// Anniversary Date.
        /// </value>
        [DataMember]
        public DateTime? AnniversaryDate { get; set; }
        
        /// <summary>
        /// Gets or sets the Graduation Date.
        /// </summary>
        /// <value>
        /// Graduation Date.
        /// </value>
        [DataMember]
        public DateTime? GraduationDate { get; set; }
        
        /// <summary>
        /// Gets or sets the Email.
        /// </summary>
        /// <value>
        /// Email.
        /// </value>
        [MaxLength( 75 )]
        [DataMember]
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
        public string EmailNote { get; set; }
        
        /// <summary>
        /// Gets or sets the Do Not Email.
        /// </summary>
        /// <value>
        /// Do Not Email.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool DoNotEmail { get; set; }
        
        /// <summary>
        /// Gets or sets the System Note.
        /// </summary>
        /// <value>
        /// System Note.
        /// </value>
        [MaxLength( 1000 )]
        [DataMember]
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
        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }
        
        /// <summary>
        /// Gets or sets the Members.
        /// </summary>
        /// <value>
        /// Collection of Members.
        /// </value>
        [DataMember]
        public virtual ICollection<GroupMember> Members { get; set; }

        /// <summary>
        /// Gets or sets the attendances.
        /// </summary>
        /// <value>
        /// The attendances.
        /// </value>
        [DataMember]
        public virtual ICollection<Attendance> Attendances
        {
            get { return _attendances ?? ( _attendances = new Collection<Attendance>() ); }
            set { _attendances = value; }
        }
        private ICollection<Attendance> _attendances;

        /// <summary>
        /// Gets or sets the Pledges.
        /// </summary>
        /// <value>
        /// Collection of Pledges.
        /// </value>
        [DataMember]
        public virtual ICollection<Pledge> Pledges { get; set; }

        /// <summary>
        /// Gets or sets the PersonAccountLookups.
        /// </summary>
        /// <value>
        /// Collection of PersonAccountLookups.
        /// </value>
        [DataMember]
        public virtual ICollection<PersonAccount> PersonAccountLookups { get; set; }

        /// <summary>
        /// Gets or sets the Marital Status.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        public virtual DefinedValue MaritalStatusValue { get; set; }
        
        /// <summary>
        /// Gets or sets the Person Status.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        public virtual DefinedValue PersonStatusValue { get; set; }
        
        /// <summary>
        /// Gets or sets the Record Status.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        public virtual DefinedValue RecordStatusValue { get; set; }
        
        /// <summary>
        /// Gets or sets the Record Status Reason.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        public virtual DefinedValue RecordStatusReasonValue { get; set; }
        
        /// <summary>
        /// Gets or sets the Record Type.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        public virtual DefinedValue RecordTypeValue { get; set; }
        
        /// <summary>
        /// Gets or sets the Suffix.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        public virtual DefinedValue SuffixValue { get; set; }
        
        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        /// <value>
        /// A <see cref="DefinedValue"/> object.
        /// </value>
        [DataMember]
        public virtual DefinedValue TitleValue { get; set; }

        /// <summary>
        /// Gets or sets the Photo
        /// </summary>
        [DataMember]
        public virtual BinaryFile Photo { get; set; }

        /// <summary>
        /// Gets NickName if not null, otherwise gets GivenName.
        /// </summary>
        public virtual string FirstName
        {
            get
            {
                return NickName ?? GivenName;
            }
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        public virtual string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        /// <summary>
        /// Gets the full name (Last, First)
        /// </summary>
        public virtual string FullNameLastFirst
        {
            get
            {
                return LastName + ", " + FirstName;
            }
        }

        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        /// <value>
        /// The birth date.
        /// </value>
        [NotMapped]
        [DataMember]
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
            this.HasOptional( p => p.MaritalStatusValue ).WithMany().HasForeignKey( p => p.MaritalStatusValueId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.PersonStatusValue ).WithMany().HasForeignKey( p => p.PersonStatusValueId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.RecordStatusValue ).WithMany().HasForeignKey( p => p.RecordStatusValueId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.RecordStatusReasonValue ).WithMany().HasForeignKey( p => p.RecordStatusReasonValueId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.RecordTypeValue ).WithMany().HasForeignKey( p => p.RecordTypeValueId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.SuffixValue ).WithMany().HasForeignKey( p => p.SuffixValueId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.TitleValue ).WithMany().HasForeignKey( p => p.TitleValueId ).WillCascadeOnDelete(false);
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

}
