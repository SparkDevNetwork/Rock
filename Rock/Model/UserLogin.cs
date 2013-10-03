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
    /// Represents a user's RockChMS login and authentication credentials.
    /// </summary>
    [Table( "UserLogin" )]
    [DataContract]
    public partial class UserLogin : Model<UserLogin>
    {
        /// <summary>
        /// Gets or sets the type of authentication service used by this UserLogin. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.AuthenticationServiceType"/> enum value representing the type of authentication service used by this UserLogin. 
        /// </value>
        /// <remarks>
        /// Options include: 
        /// <c>AuthenticationServiceType.Internal</c> which includes database and Active Directory
        /// <c>AuthenticationServiceType.External</c> which includes Facebook, Google, oAuth, etc.
        /// </remarks>
        [Required]
        [DataMember( IsRequired = true )]
        public AuthenticationServiceType ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the service class/type name of the authentication service that this UserLogin uses. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the service class/type name.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the UserName that is associated with this UserLogin. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the UserName that is associated with this UserLogin.
        /// </value>
        [Required]
        [MaxLength( 255 )]
        [DataMember( IsRequired = true )]
        [MergeField]
        public string UserName { get; set; }
        
        /// <summary>
        /// Gets or sets the Password.  Stored as a SHA1 hash for Rock Database Auth, but possibly a different hashtype for other ServiceTypes
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the password. 
        /// </value>
        [MaxLength( 128 )]
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets a flag indicating if the UserLogin has been confirmed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the UserLogin has been confirmed; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the last activity (login, password change, etc.) performed with this UserLogin.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time of the last activity associated with this UserLogin.
        /// </value>
        [NotAudited]
        [DataMember]
        [MergeField]
        public DateTime? LastActivityDateTime { get; set; }

        /// <summary>
        /// Gets or sets the most recent date and time that a user successfully logged in using this UserLogin.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the most recent date and time that a user successfully logged in with this UserLogin.
        /// </value>
        [DataMember]
        [MergeField]
        public DateTime? LastLoginDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the password was successfully changed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing when the password was last changed.
        /// </value>
        [DataMember]
        [MergeField]
        public DateTime? LastPasswordChangedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the UserLogin was created.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing when the UserLogin was created.
        /// </value>
        [DataMember]
        public DateTime? CreationDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets a flag indicating if the user is currently online and logged in to the system.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the user is currently online and logged in with this UserLogin; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsOnLine { get; set; }
        
        /// <summary>
        /// Gets or sets a flag indicating if the UserLogin is currently locked out.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the UserLogin is currently locked out; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets date and time that the UserLogin was last locked out.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the user login was last locked out.
        /// </value>
        [DataMember]
        public DateTime? LastLockedOutDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the number of failed password attempts within the failed password attempt window.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the failed password attempts during the attempt window.
        /// </value>
        [DataMember]
        public int? FailedPasswordAttemptCount { get; set; }

        /// <summary>
        /// Gets or sets the failed password attempt window start date time.
        /// </summary>
        /// <value>
        /// The failed password attempt window start date time.
        /// </value>
        [DataMember]
        public DateTime? FailedPasswordAttemptWindowStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last time that user was notified about their password expiring.
        /// </summary>
        /// <value>
        /// The last password expiration warning date time.
        /// </value>
        [DataMember]
        [MergeField]
        public DateTime? LastPasswordExpirationWarningDateTime { get; set; }

        /// <summary>
        /// Gets or sets the API key associated with the UserLogin
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the API key that is associated with the UserLogin
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string ApiKey { get; set; }
        
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> who this UserLogin belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> that this UserLogin belongs to.
        /// </value>
        [DataMember]
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets a flag indicating if the User authenticated with their last interaction with RockChMS (versus using an impersonation link).
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if the user actually authenticated; otherwise <c>false</c>.
        /// </value>
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
        /// Gets or sets the <see cref="Rock.Model.Person"/> that this UserLogin is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> that this UserLogin is associated with.
        /// </value>
        [DataMember]
        [MergeField]
        public virtual Model.Person Person { get; set; }

        /// <summary>
        /// Returns a boolean flag indicating if the provided action is allowed by default
        /// </summary>
        /// <param name="action">A <see cref="System.String"/> representing the action.</param>
        /// <returns>A <see cref="System.Boolean"/> flag indicating if the provided action is allowed by default.</returns>
        public override bool IsAllowedByDefault( string action )
        {
            return false;
        }

        /// <summary>
        /// Gets an encrypted confirmation code for the UserLogin.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the encrypted confirmation code.
        /// </value>
        [MergeField]
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
        /// Gets a URL encoded  and encrypted confirmation code.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a URL encoded and encrypted confirmation code.
        /// </value>
        [MergeField]
        public virtual string ConfirmationCodeEncoded
        {

            get
            {
                return HttpUtility.UrlEncode( ConfirmationCode );
            }
        }

        /// <summary>
        /// Creates a <see cref="System.Collections.Generic.Dictionary"/> of this UserLogin object.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Collections.Generic.Dictionary"/> of this UserLogin object.
        /// </returns>
        public override System.Collections.Generic.Dictionary<string, object> ToDictionary()
        {
            var dictionary = base.ToDictionary();
            dictionary.Add( "Person", Person );
            dictionary.Add( "ConfirmationCode", ConfirmationCode );
            dictionary.Add( "ConfirmationCodeEncoded", ConfirmationCodeEncoded );
            return dictionary;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the UserName that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the UserName that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.UserName;
        }

        #region Static Methods

        /// <summary>
        /// Returns the UserName of the user that is currently logged in.
        /// </summary>
        /// <returns>A <see cref="System.String"/> representing the UserName of the user that is currently logged in.</returns>
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
