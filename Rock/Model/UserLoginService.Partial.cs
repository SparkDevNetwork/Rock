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
    /// User POCO Service class
    /// </summary>
    public partial class UserLoginService 
    {
        /// <summary>
        /// Gets Users by Api Key
        /// </summary>
        /// <param name="apiKey">Api Key.</param>
        /// <returns>An enumerable list of User objects.</returns>
        public IEnumerable<UserLogin> GetByApiKey( string apiKey )
        {
            return Repository.Find( t => ( t.ApiKey == apiKey || ( apiKey == null && t.ApiKey == null ) ) );
        }
        
        /// <summary>
        /// Gets Users by Person Id
        /// </summary>
        /// <param name="personId">Person Id.</param>
        /// <returns>An enumerable list of User objects.</returns>
        public IEnumerable<UserLogin> GetByPersonId( int? personId )
        {
            return Repository.Find( t => ( t.PersonId == personId || ( personId == null && t.PersonId == null ) ) );
        }
        
        /// <summary>
        /// Gets User by User Name
        /// </summary>
        /// <param name="userName">User Name.</param>
        /// <returns>User object.</returns>
        public UserLogin GetByUserName( string userName )
        {
            return Repository
                .AsQueryable( "Person" )
                .Where( u => u.UserName == userName )
                .FirstOrDefault();
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="isConfirmed">if set to <c>true</c> [is confirmed].</param>
        /// <param name="currentPersonId">The current person id.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">username;Username already exists</exception>
        /// <exception cref="System.ArgumentException">serviceName</exception>
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
        /// Changes the password after first validating the existing password
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns></returns>
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
        /// Changes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
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
        /// Unlocks the user.
        /// </summary>
        /// <param name="user">The user.</param>
        public void Unlock( UserLogin user )
        {
            user.IsLockedOut = false;
            this.Save( user, null );
        }

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
        /// Gets the user by the encrypted confirmation code.
        /// </summary>
        /// <param name="code">The encrypted confirmation code.</param>
        /// <returns></returns>
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
        /// Gets the current user.
        /// </summary>
        /// <returns></returns>
        public static UserLogin GetCurrentUser()
        {
            return GetCurrentUser( true );
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <param name="userIsOnline">if set to <c>true</c> [user is online].</param>
        /// <returns></returns>
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
