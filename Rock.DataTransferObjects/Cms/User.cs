//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.CMS.DTO
{
    /// <summary>
    /// User Data Transfer Object.
    /// </summary>
	/// <remarks>
	/// Data Transfer Objects are a lightweight version of the Entity object that are used
	/// in situations like serializing the object in the REST api
	/// </remarks>
    public partial class User : Rock.DTO<User>
    {
		/// <summary>
		/// Gets or sets the User Name.
		/// </summary>
		/// <value>
		/// User Name.
		/// </value>
		public string UserName { get; set; }

		/// <summary>
		/// Gets or sets the Authentication Type.
		/// </summary>
		/// <value>
		/// Authentication Type.
		/// </value>
		public int AuthenticationType { get; set; }

		/// <summary>
		/// Gets or sets the Password.
		/// </summary>
		/// <value>
		/// Password.
		/// </value>
		public string Password { get; set; }

		/// <summary>
		/// Gets or sets the Is Confirmed.
		/// </summary>
		/// <value>
		/// Is Confirmed.
		/// </value>
		public bool? IsConfirmed { get; set; }

		/// <summary>
		/// Gets or sets the Last Activity Date.
		/// </summary>
		/// <value>
		/// Last Activity Date.
		/// </value>
		public DateTime? LastActivityDate { get; set; }

		/// <summary>
		/// Gets or sets the Last Login Date.
		/// </summary>
		/// <value>
		/// Last Login Date.
		/// </value>
		public DateTime? LastLoginDate { get; set; }

		/// <summary>
		/// Gets or sets the Last Password Changed Date.
		/// </summary>
		/// <value>
		/// Last Password Changed Date.
		/// </value>
		public DateTime? LastPasswordChangedDate { get; set; }

		/// <summary>
		/// Gets or sets the Creation Date.
		/// </summary>
		/// <value>
		/// Creation Date.
		/// </value>
		public DateTime? CreationDate { get; set; }

		/// <summary>
		/// Gets or sets the Is On Line.
		/// </summary>
		/// <value>
		/// Is On Line.
		/// </value>
		public bool? IsOnLine { get; set; }

		/// <summary>
		/// Gets or sets the Is Locked Out.
		/// </summary>
		/// <value>
		/// Is Locked Out.
		/// </value>
		public bool? IsLockedOut { get; set; }

		/// <summary>
		/// Gets or sets the Last Locked Out Date.
		/// </summary>
		/// <value>
		/// Last Locked Out Date.
		/// </value>
		public DateTime? LastLockedOutDate { get; set; }

		/// <summary>
		/// Gets or sets the Failed Password Attempt Count.
		/// </summary>
		/// <value>
		/// Failed Password Attempt Count.
		/// </value>
		public int? FailedPasswordAttemptCount { get; set; }

		/// <summary>
		/// Gets or sets the Failed Password Attempt Window Start.
		/// </summary>
		/// <value>
		/// Failed Password Attempt Window Start.
		/// </value>
		public DateTime? FailedPasswordAttemptWindowStart { get; set; }

		/// <summary>
		/// Gets or sets the Api Key.
		/// </summary>
		/// <value>
		/// Api Key.
		/// </value>
		public string ApiKey { get; set; }

		/// <summary>
		/// Gets or sets the Person Id.
		/// </summary>
		/// <value>
		/// Person Id.
		/// </value>
		public int? PersonId { get; set; }
	}
}
