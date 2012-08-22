//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Hosting;

using Rock.Data;

namespace Rock.CMS
{
    /// <summary>
    /// User POCO Entity.
    /// </summary>
    [Table( "cmsUser" )]
    public partial class User : ModelWithAttributes<User>, IAuditable
    {
		/// <summary>
		/// Gets or sets the User Name.
		/// </summary>
		/// <value>
		/// User Name.
		/// </value>
		[Required]
		[MaxLength( 255 )]
		[DataMember]
		public string UserName { get; set; }
		
		/// <summary>
		/// Gets or sets the Authentication Type.
		/// </summary>
		/// <value>
		/// Enum[AuthenticationType]  1=Database, 2= Facebook, 3=Active Directory.
		/// </value>
		[Required]
		[DataMember]
        public AuthenticationType AuthenticationType
        {
            get { return _AuthenticationType; }
            set { _AuthenticationType = value; }
        }
        private AuthenticationType _AuthenticationType = AuthenticationType.Database;

		/// <summary>
		/// Gets or sets the Password.
		/// </summary>
		/// <value>
		/// Password.
		/// </value>
		[Required]
		[MaxLength( 128 )]
		[DataMember]
		public string Password { get; set; }
		
		/// <summary>
		/// Gets or sets the is confirmed.
		/// </summary>
		/// <value>
		/// Is confirmed.
		/// </value>
		[DataMember]
		public bool? IsConfirmed { get; set; }

        /// <summary>
        /// Gets a value indicating whether the user has authenticated (vs. used an inpersonation link)
        /// </summary>
        [NotMapped]
        public bool IsAuthenticated 
        {
            get
            {
                System.Web.Security.FormsIdentity identity = HttpContext.Current.User.Identity as System.Web.Security.FormsIdentity;
                if ( identity == null)
                    return false;

                if (identity.Ticket != null &&
                    identity.Ticket.UserData.ToLower() == "true" )
                    return false;

                return true;
            }
        }

		/// <summary>
		/// Gets or sets the Last Activity Date.
		/// </summary>
		/// <value>
		/// Last Activity Date.
		/// </value>
		[DataMember]
		public DateTime? LastActivityDate { get; set; }
		
		/// <summary>
		/// Gets or sets the Last Login Date.
		/// </summary>
		/// <value>
		/// Last Login Date.
		/// </value>
		[DataMember]
		public DateTime? LastLoginDate { get; set; }
		
		/// <summary>
		/// Gets or sets the Last Password Changed Date.
		/// </summary>
		/// <value>
		/// Last Password Changed Date.
		/// </value>
		[DataMember]
		public DateTime? LastPasswordChangedDate { get; set; }
		
		/// <summary>
		/// Gets or sets the Creation Date.
		/// </summary>
		/// <value>
		/// Creation Date.
		/// </value>
		[DataMember]
		public DateTime? CreationDate { get; set; }
		
		/// <summary>
		/// Gets or sets the Is On Line.
		/// </summary>
		/// <value>
		/// Is On Line.
		/// </value>
		[DataMember]
		public bool? IsOnLine { get; set; }
		
		/// <summary>
		/// Gets or sets the Is Locked Out.
		/// </summary>
		/// <value>
		/// Is Locked Out.
		/// </value>
		[DataMember]
		public bool? IsLockedOut { get; set; }
		
		/// <summary>
		/// Gets or sets the Last Locked Out Date.
		/// </summary>
		/// <value>
		/// Last Locked Out Date.
		/// </value>
		[DataMember]
		public DateTime? LastLockedOutDate { get; set; }
		
		/// <summary>
		/// Gets or sets the Failed Password Attempt Count.
		/// </summary>
		/// <value>
		/// Failed Password Attempt Count.
		/// </value>
		[DataMember]
		public int? FailedPasswordAttemptCount { get; set; }
		
		/// <summary>
		/// Gets or sets the Failed Password Attempt Window Start.
		/// </summary>
		/// <value>
		/// Failed Password Attempt Window Start.
		/// </value>
		[DataMember]
		public DateTime? FailedPasswordAttemptWindowStart { get; set; }
		
		/// <summary>
		/// Gets or sets the Api Key.
		/// </summary>
		/// <value>
		/// Api Key.
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string ApiKey { get; set; }
		
		/// <summary>
		/// Gets or sets the Person Id.
		/// </summary>
		/// <value>
		/// Person Id.
		/// </value>
		[DataMember]
		public int? PersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified Date Time.
		/// </summary>
		/// <value>
		/// Modified Date Time.
		/// </value>
		[DataMember]
		public DateTime? ModifiedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Created By Person Id.
		/// </summary>
		/// <value>
		/// Created By Person Id.
		/// </value>
		[DataMember]
		public int? CreatedByPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified By Person Id.
		/// </summary>
		/// <value>
		/// Modified By Person Id.
		/// </value>
		[DataMember]
		public int? ModifiedByPersonId { get; set; }

        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "CMS.User"; } }
        
		/// <summary>
        /// Gets or sets the Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person Person { get; set; }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person ModifiedByPerson { get; set; }

        /// <summary>
        /// The default authorization for the selected action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public override bool IsAllowedByDefault( string action )
        {
            return false;
        }

        /// <summary>
        /// Gets the encrypted confirmation code.
        /// </summary>
        public string ConfirmationCode
        {
            get
            {
                string identifier = string.Format( "ROCK|{0}|{1}|{2}", this.EncryptedKey.ToString(), this.UserName, DateTime.Now.Ticks );
                string encryptedCode = Rock.Security.Encryption.EncryptString( identifier );
                return encryptedCode;
            }
        }

        /// <summary>
        /// Gets a urlencoded and encrypted confirmation code.
        /// </summary>
        public string ConfirmationCodeEncoded
        {

            get
            {
                return HttpUtility.UrlEncode( ConfirmationCode );
            }
        }

        #region Static Methods

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        /// <returns></returns>
        internal static string GetCurrentUserName()
        {
            if ( HostingEnvironment.IsHosted )
            {
                HttpContext current = HttpContext.Current;
                if ( current != null && current.User != null )
                    return current.User.Identity.Name;
            }
            IPrincipal currentPrincipal = Thread.CurrentPrincipal;
            if ( currentPrincipal == null || currentPrincipal.Identity == null )
                return string.Empty;
            else
                return currentPrincipal.Identity.Name;
        }

        #endregion

    }

    /// <summary>
    /// User Configuration class.
    /// </summary>
    public partial class UserConfiguration : EntityTypeConfiguration<User>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserConfiguration"/> class.
        /// </summary>
        public UserConfiguration()
        {
			this.HasOptional( p => p.Person ).WithMany( p => p.Users ).HasForeignKey( p => p.PersonId ).WillCascadeOnDelete(true);
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class UserDTO : DTO<User>
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

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public UserDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public UserDTO( User user )
        {
            CopyFromModel( user );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="user"></param>
        public override void CopyFromModel( User user )
        {
            this.Id = user.Id;
            this.Guid = user.Guid;
            this.UserName = user.UserName;
            this.AuthenticationType = ( int )user.AuthenticationType;
            this.Password = user.Password;
            this.IsConfirmed = user.IsConfirmed;
            this.LastActivityDate = user.LastActivityDate;
            this.LastLoginDate = user.LastLoginDate;
            this.LastPasswordChangedDate = user.LastPasswordChangedDate;
            this.CreationDate = user.CreationDate;
            this.IsOnLine = user.IsOnLine;
            this.IsLockedOut = user.IsLockedOut;
            this.LastLockedOutDate = user.LastLockedOutDate;
            this.FailedPasswordAttemptCount = user.FailedPasswordAttemptCount;
            this.FailedPasswordAttemptWindowStart = user.FailedPasswordAttemptWindowStart;
            this.ApiKey = user.ApiKey;
            this.PersonId = user.PersonId;
            this.CreatedDateTime = user.CreatedDateTime;
            this.ModifiedDateTime = user.ModifiedDateTime;
            this.CreatedByPersonId = user.CreatedByPersonId;
            this.ModifiedByPersonId = user.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="user"></param>
        public override void CopyToModel( User user )
        {
            user.Id = this.Id;
            user.Guid = this.Guid;
            user.UserName = this.UserName;
            user.AuthenticationType = ( AuthenticationType )this.AuthenticationType;
            user.Password = this.Password;
            user.IsConfirmed = this.IsConfirmed;
            user.LastActivityDate = this.LastActivityDate;
            user.LastLoginDate = this.LastLoginDate;
            user.LastPasswordChangedDate = this.LastPasswordChangedDate;
            user.CreationDate = this.CreationDate;
            user.IsOnLine = this.IsOnLine;
            user.IsLockedOut = this.IsLockedOut;
            user.LastLockedOutDate = this.LastLockedOutDate;
            user.FailedPasswordAttemptCount = this.FailedPasswordAttemptCount;
            user.FailedPasswordAttemptWindowStart = this.FailedPasswordAttemptWindowStart;
            user.ApiKey = this.ApiKey;
            user.PersonId = this.PersonId;
            user.CreatedDateTime = this.CreatedDateTime;
            user.ModifiedDateTime = this.ModifiedDateTime;
            user.CreatedByPersonId = this.CreatedByPersonId;
            user.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }

    /// <summary>
    /// How user is authenticated
    /// </summary>
    public enum AuthenticationType
    {
        /// <summary>
        /// Athenticate login against Rock database
        /// </summary>
        Database = 1,

        /// <summary>
        /// Authenticate using Facebook
        /// </summary>
        Facebook = 2,

        /// <summary>
        /// Authenticate using Active Directory
        /// </summary>
        ActiveDirectory = 3
    }
}
