// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Hosting;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a user's Rock login and authentication credentials.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "UserLogin" )]
    [DataContract]
    public partial class UserLogin : Model<UserLogin>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the authentication service that this UserLogin user will use.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int? EntityTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the UserName that is associated with this UserLogin. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the UserName that is associated with this UserLogin.
        /// </value>
        [Required]
        [MaxLength( 255 )]
        [DataMember( IsRequired = true )]
        public string UserName { get; set; }
        
        /// <summary>
        /// Gets or sets the Password.  Stored as a BCrypt hash for Rock Database Auth, but possibly a different hashtype for other ServiceTypes
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the password. 
        /// </value>
        [MaxLength( 128 )]
        [HideFromReporting]
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
        public DateTime? LastActivityDateTime { get; set; }

        /// <summary>
        /// Gets or sets the most recent date and time that a user successfully logged in using this UserLogin.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the most recent date and time that a user successfully logged in with this UserLogin.
        /// </value>
        [NotAudited]
        [DataMember]
        public DateTime? LastLoginDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the password was successfully changed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing when the password was last changed.
        /// </value>
        [NotAudited]
        [DataMember]
        public DateTime? LastPasswordChangedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the user is currently online and logged in to the system.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the user is currently online and logged in with this UserLogin; otherwise <c>false</c>.
        /// </value>
        [NotAudited]
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
        /// Gets or sets the is password change required.
        /// </summary>
        /// <value>
        /// The is password change required.
        /// </value>
        [DataMember]
        public bool? IsPasswordChangeRequired { get; set; }

        /// <summary>
        /// Gets or sets date and time that the UserLogin was last locked out.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the user login was last locked out.
        /// </value>
        [NotAudited]
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
        public DateTime? LastPasswordExpirationWarningDateTime { get; set; }

        /// <summary>
        /// Gets or sets the API key associated with the UserLogin
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the API key that is associated with the UserLogin
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [HideFromReporting]
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
        /// Gets or sets the history changes.
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        private History.HistoryChangeList HistoryChanges { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> that this UserLogin is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> that this UserLogin is associated with.
        /// </value>
        [LavaInclude]
        public virtual Model.Person Person { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> for the authentication service that this UserLogin user.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </value>
        [DataMember]
        [LavaIgnore]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets a flag indicating if the User authenticated with their last interaction with Rock (versus using an impersonation link).
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> value that is <c>true</c> if the user actually authenticated; otherwise <c>false</c>.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual bool IsAuthenticated
        {
            get
            {
                System.Web.Security.FormsIdentity identity = HttpContext.Current.User?.Identity as System.Web.Security.FormsIdentity;
                if ( identity == null )
                    return false;

                if ( identity.Ticket != null &&
                    identity.Ticket.UserData.ToLower() == "true" )
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Gets an encrypted confirmation code for the UserLogin.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the encrypted confirmation code.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string ConfirmationCode
        {
            get
            {
                string identifier = string.Format( "ROCK|{0}|{1}|{2}", this.EncryptedKey.ToString(), this.UserName, RockDateTime.Now.Ticks );
                string encryptedCode;
                if (Rock.Security.Encryption.TryEncryptString(identifier, out encryptedCode))
                {
                    return encryptedCode;
                }
                else
                {
                    return null;
                }
            }
            private set { }
        }

        /// <summary>
        /// Gets a URL encoded  and encrypted confirmation code.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a URL encoded and encrypted confirmation code.
        /// </value>
        [DataMember]
        [NotMapped]
        public virtual string ConfirmationCodeEncoded
        {
            get
            {
                return HttpUtility.UrlEncode( ConfirmationCode );
            }
            private set { }
        }

        #endregion

        #region Public Methods

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
        /// Returns a <see cref="System.String" /> containing the UserName that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the UserName that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.UserName;
        }

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context.  Takes
        /// care of logging any particular change history for user login.
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, DbEntityEntry entry )
        {
            var rockContext = ( RockContext ) dbContext;
            HistoryChanges = new History.HistoryChangeList();

            switch ( entry.State )
            {
                case EntityState.Added:
                    {
                        // Get the authentication provider entity type
                        var entityType = EntityTypeCache.Get( this.EntityTypeId ?? 0 );
                        var change = HistoryChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Authentication Provider" ).SetNewValue( entityType?.FriendlyName );

                        // Don't log Pin Authentication user names.
                        var isUserNameSensitive = ( entityType?.Guid == Rock.SystemGuid.EntityType.AUTHENTICATION_PIN.AsGuid() ) ? true : false;

                        if ( isUserNameSensitive )
                        {
                            change.SetCaption( "User Account" );
                        }

                        History.EvaluateChange( HistoryChanges, "User Login", string.Empty, UserName, isUserNameSensitive );
                        History.EvaluateChange( HistoryChanges, "Is Confirmed", null, IsConfirmed );
                        History.EvaluateChange( HistoryChanges, "Is Password Change Required", null, IsPasswordChangeRequired );
                        History.EvaluateChange( HistoryChanges, "Is Locked Out", null, IsLockedOut );

                        break;
                    }

                case EntityState.Modified:
                    {
                        var entityType = EntityTypeCache.Get( this.EntityTypeId ?? 0 );

                        // Don't log Pin Authentication user names.
                        var isUserNameSensitive = ( entityType?.Guid == Rock.SystemGuid.EntityType.AUTHENTICATION_PIN.AsGuid() ) ? true : false;

                        History.EvaluateChange( HistoryChanges, "User Login", entry.OriginalValues["UserName"].ToStringSafe(), UserName, isUserNameSensitive );
                        History.EvaluateChange( HistoryChanges, "Is Confirmed", entry.OriginalValues["IsConfirmed"].ToStringSafe().AsBooleanOrNull(), IsConfirmed );
                        History.EvaluateChange( HistoryChanges, "Is Password Change Required", entry.OriginalValues["IsPasswordChangeRequired"].ToStringSafe().AsBooleanOrNull(), IsPasswordChangeRequired );
                        History.EvaluateChange( HistoryChanges, "Is Locked Out", entry.OriginalValues["IsLockedOut"].ToStringSafe().AsBooleanOrNull(), IsLockedOut );
                        History.EvaluateChange( HistoryChanges, "Password", entry.OriginalValues["Password"].ToStringSafe(), Password, true );

                        // Did the provider type change?
                        int? origEntityTypeId = entry.OriginalValues["EntityTypeId"].ToStringSafe().AsIntegerOrNull();
                        int? entityTypeId = EntityType != null ? EntityType.Id : EntityTypeId;
                        if ( !entityTypeId.Equals( origEntityTypeId ) )
                        {
                            var origProviderType = EntityTypeCache.Get( origEntityTypeId ?? 0 )?.FriendlyName;
                            var providerType = EntityTypeCache.Get( this.EntityTypeId ?? 0 )?.FriendlyName;
                            History.EvaluateChange( HistoryChanges, "User Login", origProviderType, providerType );
                        }

                        // Change the caption if this is a sensitive user account
                        if ( HistoryChanges.Count > 0 && isUserNameSensitive )
                        {
                                var change = HistoryChanges.FirstOrDefault();
                                change.SetCaption( "User Account" );
                        }

                        break;
                    }

                case EntityState.Deleted:
                    {
                        // By this point EF has stripped out some of the data we need to save history
                        // Reload the data using a new context.
                        RockContext newRockContext = new RockContext();
                        var userLogin = new UserLoginService( newRockContext ).Get( this.Id );
                        if ( userLogin != null && userLogin.PersonId != null )
                        {
                            try
                            {
                                var entityType = EntityTypeCache.Get( userLogin.EntityTypeId ?? 0 );
                                var isUserNameSensitive = ( entityType?.Guid == Rock.SystemGuid.EntityType.AUTHENTICATION_PIN.AsGuid() ) ? true : false;

                                if ( ! isUserNameSensitive )
                                {
                                    HistoryChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "User Login" ).SetOldValue( userLogin.UserName );
                                    HistoryService.SaveChanges( newRockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_ACTIVITY.AsGuid(), userLogin.PersonId.Value, HistoryChanges, UserName, typeof( UserLogin ), this.Id, true, userLogin.ModifiedByPersonAliasId, null );
                                }
                                else
                                {
                                    HistoryChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Authentication Provider" ).SetOldValue( entityType?.FriendlyName ).SetCaption( "User Account" );
                                    HistoryService.SaveChanges( newRockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_ACTIVITY.AsGuid(), userLogin.PersonId.Value, HistoryChanges, entityType?.FriendlyName, typeof( UserLogin ), this.Id, true, userLogin.ModifiedByPersonAliasId, null );
                                }
                            }
                            catch ( Exception ex )
                            {
                                // Just log the problem and move on...
                                ExceptionLogService.LogException( ex );
                            }
                        }

                        HistoryChanges.Clear();
                        return;
                    }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            var rockContext = ( RockContext ) dbContext;

            // It is possible that we have a UserLogin without a PersonId, in these cases we don't want to save a person history record.
            if ( HistoryChanges?.Any() == true && this.PersonId.HasValue )
            {
                try
                {
                    HistoryService.SaveChanges( ( RockContext ) dbContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_ACTIVITY.AsGuid(), this.PersonId.Value, HistoryChanges, this.UserName, typeof( UserLogin ), this.Id, true, this.ModifiedByPersonAliasId, null );
                }
                catch ( Exception ex )
                {
                    // Just log the problem and move on...
                    ExceptionLogService.LogException( ex );
                }
            }

            base.PostSaveChanges( dbContext );
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns the UserName of the user that is currently logged in.
        /// </summary>
        /// <returns>A <see cref="System.String"/> representing the UserName of the user that is currently logged in.</returns>
        public static string GetCurrentUserName()
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
    /// Special Class to use when posting/putting a UserLogin record thru the Rest API.
    /// The Rest Client can't be given access to the DataEncryptionKey, so they'll upload it (using SSL)
    /// with the PlainTextPassword and the Rock server will encrypt prior to saving to database
    /// </summary>
    [DataContract]
    [NotMapped]
    [RockClientInclude("Use Rock.Client.UserLoginWithPlainTextPassword and set PlainTextPassword to set a new password as part of a api/UserLogins POST or PUT")]
    public class UserLoginWithPlainTextPassword : UserLogin
    {
        /// <summary>
        /// Gets or sets the plain text password.
        /// </summary>
        /// <value>
        /// The plain text password.
        /// </value>
        [DataMember]
        public string PlainTextPassword { get; set; }
    }

    #region Entity Configuration

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
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enums

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

    #endregion
}
