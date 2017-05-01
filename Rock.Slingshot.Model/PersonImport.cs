using System;
using System.Collections.Generic;

namespace Rock.Slingshot.Model
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay( "{FirstName} {LastName}" )]
    public class PersonImport
    {
        #region Family Fields

        /// <summary>
        /// Gets or sets the person foreign identifier.
        /// </summary>
        /// <value>
        /// The person foreign identifier.
        /// </value>
        public int PersonForeignId { get; set; }

        /// <summary>
        /// Gets or sets the family foreign identifier.
        /// NOTE: If this is set to NULL, autocreate a new family for this person
        /// </summary>
        /// <value>
        /// The family foreign identifier.
        /// </value>
        public int? FamilyForeignId { get; set; }

        /// <summary>
        /// Gets or sets the group role identifier.
        /// This would be Adult,Child, etc for the GroupMember record of the Person in the Family
        /// </summary>
        /// <value>
        /// The group role identifier.
        /// </value>
        public int GroupRoleId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [giving individually] or giving as a family. This will be used to set the GivingGroupId,GivingId of the Person
        /// If this is left Null, Rock will decide based on Family Role
        /// </summary>
        /// <value>
        ///   <c>true</c> if [giving individually]; otherwise, <c>false</c>.
        /// </value>
        public bool? GivingIndividually { get; set; }

        /// <summary>
        /// Gets the name of the family.
        /// </summary>
        /// <value>
        /// The name of the family.
        /// </value>
        public string FamilyName { get; set; }

        #endregion Family Fields

        #region Person Fields that map directly to Rock.Model.Person

        /// <summary>
        /// Gets or sets the Id of the Person Record Type representing what type of Person Record this is.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the identifying the person record type. If no value is selected this can be null.
        /// </value>
        public int? RecordTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Record Status representing the status of this entity
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Record Status representing the status of this entity.
        /// </value>
        public int? RecordStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the record status last modified date time.
        /// </summary>
        /// <value>
        /// The record status last modified date time.
        /// </value>
        public DateTime? RecordStatusLastModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Record Status Reason representing the reason why a person record status would have a set status.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Record Status Reason representing the reason why a person entity would have a set status.
        /// </value>
        public int? RecordStatusReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Defined Value representing the connection status of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the connection status of the Person.
        /// </value>
        public int? ConnectionStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Defined Value representing the reason a record needs to be reviewed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the reason a record needs to be reviewed.
        /// </value>
        public int? ReviewReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Person is deceased.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the Person is deceased; otherwise <c>false</c>.
        /// </value>
        public bool IsDeceased { get; set; }

        /// <summary>
        /// Gets or sets Id of the (Salutation) Tile that is associated with the Person
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Title that is associated with the Person.
        /// </value>
        public int? TitleValueId { get; set; }

        /// <summary>
        /// Gets or sets the first name of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the first name of the Person.
        /// </value>
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
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the middle name of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the middle name of the Person.
        /// </value>
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the last name (Sir Name) of the Person.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Last Name of the Person.
        /// </value>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Person's name Suffix.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Person's name Suffix. If the Person
        /// does not have a suffix as part of their name this value will be null.
        /// </value>
        /// <remarks>
        /// Examples include: Sr., Jr., III, IV, DMD,  MD, PhD, etc.
        /// </remarks>
        public int? SuffixValueId { get; set; }

        /// <summary>
        /// Gets or sets the day of the month portion of the Person's birth date.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the day of the month portion of the Person's birth date. If their birth date is not known
        /// this value will be null.
        /// </value>
        public int? BirthDay { get; set; }

        /// <summary>
        /// Gets or sets the month portion of the Person's birth date.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the month portion of the Person's birth date. If the birth date is not known this value will be null.
        /// </value>
        public int? BirthMonth { get; set; }

        /// <summary>
        /// Gets or sets the year portion of the Person's birth date.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the year portion of the Person's birth date. If the birth date is not known this value will be null.
        /// </value>
        public int? BirthYear { get; set; }

        /// <summary>
        /// Gets or sets the gender of the Person. This property is required.
        /// </summary>
        /// <value>
        /// An enum value representing the Person's gender.  Valid values are <c>Gender.Unknown</c> if the Person's gender is unknown,
        /// <c>Gender.Male</c> if the Person's gender is Male, <c>Gender.Female</c> if the Person's gender is Female.
        /// </value>
        public int Gender { get; set; }

        /// <summary>
        /// Gets or sets Id of the Marital Status representing the Person's martial status.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Marital Status representing the Person's martial status.  This value is nullable.
        /// </value>
        public int? MaritalStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the date of the Person's wedding anniversary.  This property is nullable if the Person is not married or their anniversary date is not known.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the anniversary date of the Person's wedding. If the anniversary date is not known or they are not married this value will be null.
        /// </value>
        public DateTime? AnniversaryDate { get; set; }

        /// <summary>
        /// Gets or sets the date of the Person's projected or actual high school graduation year. This value is used to determine what grade a student is in.
        /// If this is not known, but Grade is, set Grade to have Rock calculate the GraduationYear
        /// </summary>
        /// <value>
        /// The Person's projected or actual high school graduation year
        /// </value>
        public int? GraduationYear { get; set; }

        /// <summary>
        /// Gets or sets the grade, which will be used to determine the GraduationYear
        /// </summary>
        /// <value>
        /// The grade.
        /// </value>
        public string Grade { get; set; }

        /// <summary>
        /// Gets or sets the Person's email address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the Person's email address.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Person's email address is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the email address is active, otherwise <c>false</c>.
        /// </value>
        public bool IsEmailActive { get; set; }

        /// <summary>
        /// Gets or sets a note about the Person's email address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a note about the Person's email address.
        /// </value>
        public string EmailNote { get; set; }

        /// <summary>
        /// Gets or sets the email preference.
        /// </summary>
        /// <value>
        /// The email preference.
        /// </value>
        public int EmailPreference { get; set; }

        /// <summary>
        /// Gets or sets the Inactive Reason Note
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing an Inactive Reason Note.
        /// </value>
        public string InactiveReasonNote { get; set; }

        

        #endregion Person Fields that map directly to Rock.Model.Person

        #region Collections

        /// <summary>
        /// Gets or sets the phone numbers.
        /// </summary>
        /// <value>
        /// The phone numbers.
        /// </value>
        public ICollection<PhoneNumberImport> PhoneNumbers { get; set;}

        /// <summary>
        /// Gets or sets the addresses of person's family
        /// </summary>
        /// <value>
        /// The addresses.
        /// </value>
        public ICollection<PersonAddressImport> Addresses { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public ICollection<AttributeValueImport> AttributeValues { get; set; }

        #endregion

        #region Meta Fields

        /// <summary>
        /// Gets or sets the created date time of when the Person was created in the foreign system
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modified date time of when the Person was modified in the foreign system
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        public string Note { get; set; }

        #endregion
    }
}
