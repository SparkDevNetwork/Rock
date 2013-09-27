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
    [DataContract]
    public partial class UserLogin : Model<UserLogin>
    {
        /// <summary>
        /// Gets or sets the type of the service.
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public AuthenticationServiceType ServiceType { get; set; }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        /// <value>
        /// The service.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the User Name.
        /// </summary>
        /// <value>
        /// User Name.
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
        /// Gets or sets the last activity date time.
        /// </summary>
        /// <value>
        /// The last activity date time.
        /// </value>
        [NotAudited]
        [DataMember]
        [MergeField]
        public DateTime? LastActivityDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last login date time.
        /// </summary>
        /// <value>
        /// The last login date time.
        /// </value>
        [DataMember]
        [MergeField]
        public DateTime? LastLoginDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last password changed date time.
        /// </summary>
        /// <value>
        /// The last password changed date time.
        /// </value>
        [DataMember]
        [MergeField]
        public DateTime? LastPasswordChangedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the creation date time.
        /// </summary>
        /// <value>
        /// The creation date time.
        /// </value>
        [DataMember]
        public DateTime? CreationDateTime { get; set; }
        
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
        /// Gets or sets the last locked out date time.
        /// </summary>
        /// <value>
        /// The last locked out date time.
        /// </value>
        [DataMember]
        public DateTime? LastLockedOutDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the Failed Password Attempt Count.
        /// </summary>
        /// <value>
        /// Failed Password Attempt Count.
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
        [MergeField]
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
        /// Gets a urlencoded and encrypted confirmation code.
        /// </summary>
        [MergeField]
        public virtual string ConfirmationCodeEncoded
        {

            get
            {
                return HttpUtility.UrlEncode( ConfirmationCode );
            }
        }

        /// <summary>
        /// To the dictionary.
        /// </summary>
        /// <returns></returns>
        public override System.Collections.Generic.Dictionary<string, object> ToDictionary()
        {
            var dictionary = base.ToDictionary();
            dictionary.Add( "Person", Person );
            dictionary.Add( "ConfirmationCode", ConfirmationCode );
            dictionary.Add( "ConfirmationCodeEncoded", ConfirmationCodeEncoded );
            return dictionary;
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
