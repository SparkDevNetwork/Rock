//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Web;

using Rock.Data;

namespace Rock.Crm
{
    /// <summary>
    /// Person POCO Entity.
    /// </summary>
    [Table( "crmPerson" )]
    public partial class Person : Model<Person>
    {
        /// <summary>
        /// The Entity Type used for saving user values
        /// </summary>
        public const string USER_VALUE_ENTITY = "Rock.Crm.Person.Value";

        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Record Type Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        public int? RecordTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the Record Status Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        public int? RecordStatusId { get; set; }
        
        /// <summary>
        /// Gets or sets the Record Status Reason Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        public int? RecordStatusReasonId { get; set; }
        
        /// <summary>
        /// Gets or sets the Person Status Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        public int? PersonStatusId { get; set; }
        
        /// <summary>
        /// Gets or sets the Title Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        public int? TitleId { get; set; }
        
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
        [TrackChanges]
        [DataMember]
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the Last Name.
        /// </summary>
        /// <value>
        /// Last Name.
        /// </value>
        [MaxLength( 50 )]
        [TrackChanges]
        [DataMember]
        public string LastName { get; set; }
        
        /// <summary>
        /// Gets or sets the Suffix Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        public int? SuffixId { get; set; }
        
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
        [DataMember]
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the Marital Status Id.
        /// </summary>
        /// <value>
        /// .
        /// </value>
        [DataMember]
        public int? MaritalStatusId { get; set; }
        
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
        [DataMember]
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
        
        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Person Read( int id )
        {
            return Read<Person>( id );
        }
        
        /// <summary>
        /// Gets or sets the Users.
        /// </summary>
        /// <value>
        /// Collection of Users.
        /// </value>
        public virtual ICollection<Cms.User> Users { get; set; }
        
        /// <summary>
        /// Gets or sets the Email Templates.
        /// </summary>
        /// <value>
        /// Collection of Email Templates.
        /// </value>
        public virtual ICollection<EmailTemplate> EmailTemplates { get; set; }
        
        /// <summary>
        /// Gets or sets the Phone Numbers.
        /// </summary>
        /// <value>
        /// Collection of Phone Numbers.
        /// </value>
        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }
        
        /// <summary>
        /// Gets or sets the Members.
        /// </summary>
        /// <value>
        /// Collection of Members.
        /// </value>
        public virtual ICollection<Crm.GroupMember> Members { get; set; }

        /// <summary>
        /// Gets or sets the Pledges.
        /// </summary>
        /// <value>
        /// Collection of Pledges.
        /// </value>
        public virtual ICollection<Financial.Pledge> Pledges { get; set; }

        /// <summary>
        /// Gets or sets the PersonAccountLookups.
        /// </summary>
        /// <value>
        /// Collection of PersonAccountLookups.
        /// </value>
        public virtual ICollection<Financial.PersonAccountLookup> PersonAccountLookups { get; set; }

        /// <summary>
        /// Gets or sets the Marital Status.
        /// </summary>
        /// <value>
        /// A <see cref="Core.DefinedValue"/> object.
        /// </value>
        public virtual Core.DefinedValue MaritalStatus { get; set; }
        
        /// <summary>
        /// Gets or sets the Person Status.
        /// </summary>
        /// <value>
        /// A <see cref="Core.DefinedValue"/> object.
        /// </value>
        public virtual Core.DefinedValue PersonStatus { get; set; }
        
        /// <summary>
        /// Gets or sets the Record Status.
        /// </summary>
        /// <value>
        /// A <see cref="Core.DefinedValue"/> object.
        /// </value>
        public virtual Core.DefinedValue RecordStatus { get; set; }
        
        /// <summary>
        /// Gets or sets the Record Status Reason.
        /// </summary>
        /// <value>
        /// A <see cref="Core.DefinedValue"/> object.
        /// </value>
        public virtual Core.DefinedValue RecordStatusReason { get; set; }
        
        /// <summary>
        /// Gets or sets the Record Type.
        /// </summary>
        /// <value>
        /// A <see cref="Core.DefinedValue"/> object.
        /// </value>
        public virtual Core.DefinedValue RecordType { get; set; }
        
        /// <summary>
        /// Gets or sets the Suffix.
        /// </summary>
        /// <value>
        /// A <see cref="Core.DefinedValue"/> object.
        /// </value>
        public virtual Core.DefinedValue Suffix { get; set; }
        
        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        /// <value>
        /// A <see cref="Core.DefinedValue"/> object.
        /// </value>
        public virtual Core.DefinedValue Title { get; set; }

        /// <summary>
        /// Gets or sets the Photo
        /// </summary>
        public virtual Cms.File Photo { get; set; }

        /// <summary>
        /// Gets NickName if not null, otherwise gets GivenName.
        /// </summary>
        public string FirstName
        {
            get
            {
                return NickName ?? GivenName;
            }
        }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        /// <summary>
        /// Gets the full name (Last, First)
        /// </summary>
        public string FullNameLastFirst
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
        public DateTime? BirthDate
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
        /// Gets the impersonation parameter.
        /// </summary>
        public string ImpersonationParameter
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
        public Rock.Cms.User ImpersonatedUser
        {
            get
            {
                Rock.Cms.User user = new Cms.User();
                user.UserName = this.FullName;
                user.PersonId = this.Id;
                user.Person = this;
                return user;
            }
        }

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
    }

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
            this.HasOptional( p => p.MaritalStatus ).WithMany().HasForeignKey( p => p.MaritalStatusId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.PersonStatus ).WithMany().HasForeignKey( p => p.PersonStatusId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.RecordStatus ).WithMany().HasForeignKey( p => p.RecordStatusId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.RecordStatusReason ).WithMany().HasForeignKey( p => p.RecordStatusReasonId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.RecordType ).WithMany().HasForeignKey( p => p.RecordTypeId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.Suffix ).WithMany().HasForeignKey( p => p.SuffixId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.Title ).WithMany().HasForeignKey( p => p.TitleId ).WillCascadeOnDelete(false);
            this.HasOptional( p => p.Photo ).WithMany().HasForeignKey( p => p.PhotoId ).WillCascadeOnDelete( false );
        }
    }

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

}
