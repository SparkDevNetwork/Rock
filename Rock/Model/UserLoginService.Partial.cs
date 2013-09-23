//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.UserLogin"/> entities.
    /// </summary>
    public partial class UserLoginService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.UserLogin"/> entities by their API Key.
        /// </summary>
        /// <param name="apiKey">A <see cref="System.String"/> representing the API key to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.UserLogin"/> entities where the API key matches the provided value..</returns>
        public IEnumerable<UserLogin> GetByApiKey( string apiKey )
        {
            return Repository.Find( t => ( t.ApiKey == apiKey || ( apiKey == null && t.ApiKey == null ) ) );
        }
        
        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.UserLogin"/> entities by a <see cref="Rock.Model.Person">Person's</see> PersonId.
        /// </summary>
        /// <param name="personId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> to search by. This property is nullable
        /// to find <see cref="Rock.Model.UserLogin"/> entities that are not associated with a Person.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.UserLogin"/> entities that are associated with the provided PersonId.</returns>
        public IEnumerable<UserLogin> GetByPersonId( int? personId )
        {
            return Repository.Find( t => ( t.PersonId == personId || ( personId == null && t.PersonId == null ) ) );
        }
        
        /// <summary>
        /// Gets<see cref="Rock.Model.UserLogin"/> by User Name
        /// </summary>
        /// <param name="userName">A <see cref="System.String"/> representing the UserName to search for.</param>
        /// <returns>A <see cref="UserLogin"/> entity where the UserName matches the provided value.</returns>
        public UserLogin GetByUserName( string userName )
        {
            return Repository
                .AsQueryable( "Person" )
                .Where( u => u.UserName == userName )
                .FirstOrDefault();
        }

        /// <summary>
        /// Creates a new <see cref="Rock.Model.UserLogin"/>
        /// </summary>
        /// <param name="person">The <see cref="Rock.Model.Person"/> that this <see cref="UserLogin"/> will be associated with.</param>
        /// <param name="serviceType">The <see cref="Rock.Model.AuthenticationServiceType"/> type of Login</param>
        /// <param name="serviceName">A <see cref="System.String"/> representing the service class/type name of the authentication service</param>
        /// <param name="username">A <see cref="System.String"/> containing the UserName.</param>
        /// <param name="password">A <see cref="System.String"/> containing the unhashed/unencrypted password.</param>
        /// <param name="isConfirmed">A <see cref="System.Boolean"/> flag indicating if the user has been confirmed.</param>
        /// <param name="currentPersonId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> creating the <see cref="Rock.Model.UserLogin"/></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the Username already exists.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the service does not exist or is not active.</exception>
        public UserLogin Create( Rock.Model.Person person,
            AuthenticationServiceType serviceType,
            string serviceName,
            string username,
            string password,
            bool isConfirmed,
            int? currentPersonId )
        {
            UserLogin user = this.GetByUserName( username );
            if ( user != null )
                throw new ArgumentOutOfRangeException( "username", "Username already exists" );

            DateTime createDate = DateTime.Now;

            user = new UserLogin();
            user.ServiceType = serviceType;
            user.ServiceName = serviceName;
            user.UserName = username;
            user.IsConfirmed = isConfirmed;
            user.CreationDateTime = createDate;
            user.LastPasswordChangedDateTime = createDate;
            if ( person != null )
                user.PersonId = person.Id;

            if ( serviceType == AuthenticationServiceType.Internal )
            {
                AuthenticationComponent authenticationComponent = GetComponent( serviceName );
                if ( authenticationComponent == null )
                    throw new ArgumentException( string.Format( "'{0}' service does not exist, or is not active", serviceName), "serviceName" );

                user.Password = authenticationComponent.EncodePassword( user, password );
            }

            this.Add( user, currentPersonId );
            this.Save( user, currentPersonId );

            return user;
        }

        /// <summary>
        /// Changes a <see cref="Rock.Model.UserLogin">UserLogin's</see> password after first validating the current password.
        /// </summary>
        /// <param name="user">The <see cref="Rock.Model.UserLogin"/> to change the password on..</param>
        /// <param name="oldPassword">A <see cref="System.String"/> representing an unhashed instance of the current/old password.</param>
        /// <param name="newPassword">A <see cref="System.String"/> representing an unhashed instance of the new password..</param>
        /// <returns>A <see cref="System.Boolean"/> value that indicates if the password change was successful. <c>true</c> if successful; otherwise <c>false</c>.</returns>
        public bool ChangePassword( UserLogin user, string oldPassword, string newPassword )
        {
            if ( user.ServiceType == AuthenticationServiceType.External )
                throw new Exception( "Cannot change password on external service type" );

            AuthenticationComponent authenticationComponent = GetComponent( user.ServiceName );
            if ( authenticationComponent == null )
                throw new Exception( string.Format( "'{0}' service does not exist, or is not active", user.ServiceName ) );

            if ( !authenticationComponent.Authenticate( user, oldPassword ) )
                return false;

            user.Password = authenticationComponent.EncodePassword( user, newPassword );
            user.LastPasswordChangedDateTime = DateTime.Now;

            return true;
        }

        /// <summary>
        /// Changes the a <see cref="Rock.Model.UserLogin">UserLogin's</see> password.
        /// </summary>
        /// <param name="user">The <see cref="Rock.Model.UserLogin"/> to change the password for.</param>
        /// <param name="password">A <see cref="System.String"/> representing the new password.</param>
        public void ChangePassword( UserLogin user, string password )
        {
            if ( user.ServiceType == AuthenticationServiceType.External )
                throw new Exception( "Cannot change password on external service type" );

            AuthenticationComponent authenticationComponent = GetComponent( user.ServiceName );
            if ( authenticationComponent == null )
                throw new Exception( string.Format( "'{0}' service does not exist, or is not active", user.ServiceName ) );

            user.Password = authenticationComponent.EncodePassword( user, password );
            user.LastPasswordChangedDateTime = DateTime.Now;
        }

        /// <summary>
        /// Unlocks a <see cref="Rock.Model.UserLogin"/>
        /// </summary>
        /// <param name="user">The <see cref="Rock.Model.UserLogin"/> to unlock.</param>
        public void Unlock( UserLogin user )
        {
            user.IsLockedOut = false;
            this.Save( user, null );
        }

        /// <summary>
        /// Updates the <see cref="Rock.Model.UserLogin"/> failed password attempt count.
        /// </summary>
        /// <param name="user">The <see cref="Rock.Model.UserLogin"/> to update the failure count on.</param>
        private void UpdateFailureCount( UserLogin user )
        {
            int passwordAttemptWindow = 0;
            int maxInvalidPasswordAttempts = int.MaxValue;

            var globalAttributes = GlobalAttributesCache.Read();
            if ( !Int32.TryParse( globalAttributes.GetValue( "PasswordAttemptWindow" ), out passwordAttemptWindow ) )
                passwordAttemptWindow = 0;
            if ( !Int32.TryParse( globalAttributes.GetValue( "MaxInvalidPasswordAttempts" ), out maxInvalidPasswordAttempts ) )
                maxInvalidPasswordAttempts = int.MaxValue;

            DateTime firstAttempt = user.FailedPasswordAttemptWindowStartDateTime ?? DateTime.MinValue;
            int attempts = user.FailedPasswordAttemptCount ?? 0;

            TimeSpan window = new TimeSpan( 0, passwordAttemptWindow, 0 );
            if ( DateTime.Now.CompareTo( firstAttempt.Add( window ) ) < 0 )
            {
                attempts++;
                if ( attempts >= maxInvalidPasswordAttempts )
                {
                    user.IsLockedOut = true;
                    user.LastLockedOutDateTime = DateTime.Now;
                }

                user.FailedPasswordAttemptCount = attempts;
            }
            else
            {
                user.FailedPasswordAttemptCount = 1;
                user.FailedPasswordAttemptWindowStartDateTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Returns a <see cref="Rock.Model.UserLogin"/> by an encrypted confirmation code.
        /// </summary>
        /// <param name="code">A <see cref="System.String"/> containing the encrypted confirmation code to search for.</param>
        /// <returns>The <see cref="Rock.Model.UserLogin"/> associated with the encrypted confirmation code.</returns>
        public UserLogin GetByConfirmationCode( string code )
        {
            if ( !string.IsNullOrEmpty( code ) )
            {
                string identifier = string.Empty;
                try { identifier = Rock.Security.Encryption.DecryptString( code ); }
                catch { }

                if ( identifier.StartsWith( "ROCK|" ) )
                {
                    string[] idParts = identifier.Split( '|' );
                    if ( idParts.Length == 4 )
                    {
                        string publicKey = idParts[1];
                        string username = idParts[2];
                        long ticks = 0;
                        if ( !long.TryParse( idParts[3], out ticks ) )
                            ticks = 0;
                        DateTime dateTime = new DateTime( ticks );

                        // Confirmation Code is only valid for an hour
                        if ( DateTime.Now.Subtract( dateTime ).Hours > 1 )
                            return null;

                        UserLogin user = this.GetByEncryptedKey( publicKey );
                        if ( user.UserName == username )
                            return user;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a <see cref="Rock.Security.AuthenticationComponent"/> by the name of the authentication service.
        /// </summary>
        /// <param name="serviceName">A <see cref="System.String"/> containing the service name.</param>
        /// <returns>The <see cref="Rock.Security.AuthenticationCompeont"/> associated with the service.</returns>
        private AuthenticationComponent GetComponent( string serviceName )
        {
            foreach ( var serviceEntry in AuthenticationContainer.Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                string componentName = component.GetType().FullName;
                if (
                    componentName == serviceName &&
                    component.AttributeValues.ContainsKey( "Active" ) &&
                    bool.Parse( component.AttributeValues["Active"][0].Value )
                )
                {
                    return component;
                }
            }
            return null;
        }

        #region Static Methods

        /// <summary>
        /// Returns the <see cref="Rcok.Model.UserLogin"/> of the user who is currently logged in.
        /// </summary>
        /// <returns>The <see cref="Rock.Model.UserLogin"/> of the user who is currently logged in</returns>
        public static UserLogin GetCurrentUser()
        {
            return GetCurrentUser( true );
        }

        /// <summary>
        /// Returns the <see cref="Rock.Model.UserLogin"/>
        /// </summary>
        /// <param name="userIsOnline">A <see cref="System.Boolean"/> value that returns the logged in user if <c>true</c>; otherwise can return the impersonated user</param>
        /// <returns>The current <see cref="Rock.Model.UserLogin"/></returns>
        public static UserLogin GetCurrentUser( bool userIsOnline )
        {
            string userName = UserLogin.GetCurrentUserName();
            if ( userName != string.Empty )
            {
                if ( userName.StartsWith( "rckipid=" ) )
                {
                    Rock.Model.PersonService personService = new Model.PersonService();
                    Rock.Model.Person impersonatedPerson = personService.GetByEncryptedKey( userName.Substring( 8 ) );
                    if ( impersonatedPerson != null )
                        return impersonatedPerson.ImpersonatedUser;
                }
                else
                {
                    var userLoginService = new UserLoginService();
                    UserLogin user = userLoginService.GetByUserName( userName );

                    if ( user != null && userIsOnline )
                    {
                        // Save last activity date
                        var transaction = new Rock.Transactions.UserLastActivityTransaction();
                        transaction.UserId = user.Id;
                        transaction.LastActivityDate = DateTime.Now;
                        Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                    }

                    return user;
                }
            }

            return null;
        }

        #endregion

    }
}
