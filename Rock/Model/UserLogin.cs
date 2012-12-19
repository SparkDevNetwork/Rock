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

namespace Rock.Model
{
    /// <summary>
    /// User POCO Entity.
    /// </summary>
    [Table( "UserLogin" )]
    [DataContract( IsReference = true )]
    public partial class UserLogin : Model<UserLogin>
    {
        /// <summary>
        /// Gets or sets the type of the service.
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        [Required]
        [DataMember]
        public AuthenticationServiceType ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember]
        public string ServiceName { get; set; }

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
        /// Gets or sets the Password.
        /// </summary>
        /// <value>
        /// Password.
        /// </value>
        [MaxLength( 128 )]
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
        /// Gets or sets the Last Activity Date.
        /// </summary>
        /// <value>
        /// Last Activity Date.
        /// </value>
        [NotAudited]
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
        /// Gets a value indicating whether the user has authenticated (vs. used an inpersonation link)
        /// </summary>
        [NotMapped]
        public virtual bool IsAuthenticated
        {
            get
            {
                System.Web.Security.FormsIdentity identity = HttpContext.Current.User.Identity as System.Web.Security.FormsIdentity;
                if ( identity == null )
                    return false;

                if ( identity.Ticket != null &&
                    identity.Ticket.UserData.ToLower() == "true" )
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        [DataMember]
        public virtual Model.Person Person { get; set; }
        
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
        public virtual string ConfirmationCode
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
        public virtual string ConfirmationCodeEncoded
        {

            get
            {
                return HttpUtility.UrlEncode( ConfirmationCode );
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
            return this.UserName;
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
    public partial class UserLoginConfiguration : EntityTypeConfiguration<UserLogin>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoginConfiguration"/> class.
        /// </summary>
        public UserLoginConfiguration()
        {
            this.HasOptional( p => p.Person ).WithMany( p => p.Users ).HasForeignKey( p => p.PersonId ).WillCascadeOnDelete(true);
        }
    }

    /// <summary>
    /// Type of authentication service used to authenticate user
    /// </summary>
    public enum AuthenticationServiceType
    {
        /// <summary>
        /// An internal authentication service (i.e. Database, Active Directory)
        /// </summary>
        Internal = 0,

        /// <summary>
        /// An external authentication service (i.e. Facebook, Twitter, Google, etc.)
        /// </summary>
        External = 1,
    }
}
