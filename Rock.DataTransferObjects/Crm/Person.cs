//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.CRM.DTO
{
    /// <summary>
    /// Person Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class Person : Rock.DTO<Person>
    {
		/// <summary>
		/// Gets or sets the System.
		/// </summary>
		/// <value>
		/// System.
		/// </value>
		public bool IsSystem { get; set; }

		/// <summary>
		/// Gets or sets the Record Type Id.
		/// </summary>
		/// <value>
		/// Record Type Id.
		/// </value>
		public int? RecordTypeId { get; set; }

		/// <summary>
		/// Gets or sets the Record Status Id.
		/// </summary>
		/// <value>
		/// Record Status Id.
		/// </value>
		public int? RecordStatusId { get; set; }

		/// <summary>
		/// Gets or sets the Record Status Reason Id.
		/// </summary>
		/// <value>
		/// Record Status Reason Id.
		/// </value>
		public int? RecordStatusReasonId { get; set; }

		/// <summary>
		/// Gets or sets the Person Status Id.
		/// </summary>
		/// <value>
		/// Person Status Id.
		/// </value>
		public int? PersonStatusId { get; set; }

		/// <summary>
		/// Gets or sets the Title Id.
		/// </summary>
		/// <value>
		/// Title Id.
		/// </value>
		public int? TitleId { get; set; }

		/// <summary>
		/// Gets or sets the Given Name.
		/// </summary>
		/// <value>
		/// Given Name.
		/// </value>
		public string GivenName { get; set; }

		/// <summary>
		/// Gets or sets the Nick Name.
		/// </summary>
		/// <value>
		/// Nick Name.
		/// </value>
		public string NickName { get; set; }

		/// <summary>
		/// Gets or sets the Last Name.
		/// </summary>
		/// <value>
		/// Last Name.
		/// </value>
		public string LastName { get; set; }

		/// <summary>
		/// Gets or sets the Suffix Id.
		/// </summary>
		/// <value>
		/// Suffix Id.
		/// </value>
		public int? SuffixId { get; set; }

		/// <summary>
		/// Gets or sets the Photo Id.
		/// </summary>
		/// <value>
		/// Photo Id.
		/// </value>
		public int? PhotoId { get; set; }

		/// <summary>
		/// Gets or sets the Birth Day.
		/// </summary>
		/// <value>
		/// Birth Day.
		/// </value>
		public int? BirthDay { get; set; }

		/// <summary>
		/// Gets or sets the Birth Month.
		/// </summary>
		/// <value>
		/// Birth Month.
		/// </value>
		public int? BirthMonth { get; set; }

		/// <summary>
		/// Gets or sets the Birth Year.
		/// </summary>
		/// <value>
		/// Birth Year.
		/// </value>
		public int? BirthYear { get; set; }

		/// <summary>
		/// Gets or sets the Gender.
		/// </summary>
		/// <value>
		/// Gender.
		/// </value>
		public int? Gender { get; set; }

		/// <summary>
		/// Gets or sets the Marital Status Id.
		/// </summary>
		/// <value>
		/// Marital Status Id.
		/// </value>
		public int? MaritalStatusId { get; set; }

		/// <summary>
		/// Gets or sets the Anniversary Date.
		/// </summary>
		/// <value>
		/// Anniversary Date.
		/// </value>
		public DateTime? AnniversaryDate { get; set; }

		/// <summary>
		/// Gets or sets the Graduation Date.
		/// </summary>
		/// <value>
		/// Graduation Date.
		/// </value>
		public DateTime? GraduationDate { get; set; }

		/// <summary>
		/// Gets or sets the Email.
		/// </summary>
		/// <value>
		/// Email.
		/// </value>
		public string Email { get; set; }

		/// <summary>
		/// Gets or sets the Email Is Active.
		/// </summary>
		/// <value>
		/// Email Is Active.
		/// </value>
		public bool? IsEmailActive { get; set; }

		/// <summary>
		/// Gets or sets the Email Note.
		/// </summary>
		/// <value>
		/// Email Note.
		/// </value>
		public string EmailNote { get; set; }

		/// <summary>
		/// Gets or sets the Do Not Email.
		/// </summary>
		/// <value>
		/// Do Not Email.
		/// </value>
		public bool DoNotEmail { get; set; }

		/// <summary>
		/// Gets or sets the System Note.
		/// </summary>
		/// <value>
		/// System Note.
		/// </value>
		public string SystemNote { get; set; }

		/// <summary>
		/// Gets or sets the Viewed Count.
		/// </summary>
		/// <value>
		/// Viewed Count.
		/// </value>
		public int? ViewedCount { get; set; }
    }
}
