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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Rock.Communication;
using Rock.Data;
using Rock.Security;
using Rock.Utility;
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
        public IQueryable<UserLogin> GetByApiKey( string apiKey )
        {
            return Queryable().Where( t => ( t.ApiKey == apiKey || ( apiKey == null && t.ApiKey == null ) ) );
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.UserLogin"/> entities by a <see cref="Rock.Model.Person">Person's</see> PersonId.
        /// </summary>
        /// <param name="personId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> to search by. This property is nullable
        /// to find <see cref="Rock.Model.UserLogin"/> entities that are not associated with a Person.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.UserLogin"/> entities that are associated with the provided PersonId.</returns>
        public IQueryable<UserLogin> GetByPersonId( int? personId )
        {
            return Queryable().Where( t => ( t.PersonId == personId || ( personId == null && t.PersonId == null ) ) );
        }

        /// <summary>
        /// Gets<see cref="Rock.Model.UserLogin"/> by User Name, with eager loading of the Person and PersonAliases
        /// </summary>
        /// <param name="userName">A <see cref="System.String"/> representing the UserName to search for.</param>
        /// <returns>A <see cref="UserLogin"/> entity where the UserName matches the provided value.</returns>
        public UserLogin GetByUserName( string userName )
        {
            return Queryable( "Person.Aliases" )
                .Where( u => u.UserName == userName )
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns true if there is a UserLogin record matching the specified userName
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        public bool Exists( string userName )
        {
            return Queryable().Where( u => u.UserName == userName ).Any();
        }

        /// <summary>
        /// Sets the a <see cref="Rock.Model.UserLogin">UserLogin's</see> password.
        /// </summary>
        /// <param name="user">The <see cref="Rock.Model.UserLogin"/> to change the password for.</param>
        /// <param name="password">A <see cref="System.String"/> representing the new password.</param>
        public void SetPassword( UserLogin user, string password )
        {
            var entityType = EntityTypeCache.Get( user.EntityTypeId ?? 0 );

            var authenticationComponent = AuthenticationContainer.GetComponent( entityType.Name );
            if ( authenticationComponent == null || !authenticationComponent.IsActive )
            {
                throw new Exception( string.Format( "'{0}' service does not exist, or is not active", entityType.FriendlyName ) );
            }

            if ( authenticationComponent.ServiceType == AuthenticationServiceType.External )
            {
                throw new Exception( "Cannot change password on external service type" );
            }

            authenticationComponent.SetPassword( user, password );
            user.LastPasswordChangedDateTime = RockDateTime.Now;
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
                try
                {
                    identifier = Rock.Security.Encryption.DecryptString( code );
                }
                catch
                {
                }

                if ( identifier.IsNotNullOrWhiteSpace() && identifier.StartsWith( "ROCK|" ) )
                {
                    string[] idParts = identifier.Split( '|' );
                    if ( idParts.Length == 4 )
                    {
                        string publicKey = idParts[1];
                        string username = idParts[2];
                        long ticks = 0;
                        if ( !long.TryParse( idParts[3], out ticks ) )
                        {
                            ticks = 0;
                        }

                        DateTime dateTime = new DateTime( ticks );

                        // Confirmation Code is only valid for an hour
                        if ( RockDateTime.Now.Subtract( dateTime ).Hours > 1 )
                        {
                            return null;
                        }

                        UserLogin user = this.GetByEncryptedKey( publicKey );
                        if ( user != null && user.UserName == username )
                        {
                            return user;
                        }
                    }
                }
            }

            return null;
        }

        #region Static Methods

        /// <inheritdoc cref="GetCurrentUser(bool)"/>
        public static UserLogin GetCurrentUser()
        {
            return GetCurrentUser( true );
        }

        /// <summary>
        /// Checks to see if the given username is valid according to the UsernameRegex (if defined).
        /// </summary>
        /// <param name="username">A username to verify.</param>
        /// <returns>A <see cref="System.Boolean"/> value that indicates if the password is valid. <c>true</c> if valid; otherwise <c>false</c>.</returns>
        public static bool IsUsernameValid( string username )
        {
            var globalAttributes = GlobalAttributesCache.Get();
            string usernameRegex = globalAttributes.GetValue( "core.ValidUsernameRegularExpression" );

            if ( string.IsNullOrEmpty( usernameRegex ) )
            {
                return true;
            }
            else
            {
                var regex = new Regex( usernameRegex );
                return regex.IsMatch( username );
            }
        }

        /// <summary>
        /// Checks to see if the given password is valid according to the PasswordRegex (if defined).
        /// </summary>
        /// <param name="password">A password to verify.</param>
        /// <returns>A <see cref="System.Boolean"/> value that indicates if the password is valid. <c>true</c> if valid; otherwise <c>false</c>.</returns>
        public static bool IsPasswordValid( string password )
        {
            var globalAttributes = GlobalAttributesCache.Get();
            string passwordRegex = globalAttributes.GetValue( "PasswordRegularExpression" );
            if ( string.IsNullOrEmpty( passwordRegex ) )
            {
                return true;
            }
            else
            {
                var regex = new Regex( passwordRegex );
                return regex.IsMatch( password );
            }
        }

        /// <summary>
        /// Determines whether a new UserLogin with the specified parameters would be valid
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="passwordConfirm">The password confirm.</param>
        /// <param name="errorTitle">The error title.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if [is valid new user login] [the specified user name]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidNewUserLogin( string userName, string password, string passwordConfirm, out string errorTitle, out string errorMessage )
        {
            errorTitle = null;
            errorMessage = null;
            if ( string.IsNullOrWhiteSpace( userName ) || string.IsNullOrWhiteSpace( password ) )
            {
                errorTitle = "Missing Information";
                errorMessage = "A username and password are required when saving an account";
                return false;
            }

            if ( new UserLoginService( new RockContext() ).Exists( userName ) )
            {
                errorTitle = "Invalid Username";
                errorMessage = "The selected Username is already being used. Please select a different Username";
                return false;
            }

            if ( !UserLoginService.IsPasswordValid( password ) )
            {
                errorTitle = string.Empty;
                errorMessage = UserLoginService.FriendlyPasswordRules();
                return false;
            }

            if ( passwordConfirm != password )
            {
                errorTitle = "Invalid Password";
                errorMessage = "The password and password confirmation do not match";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a user friendly description of the username rules.
        /// </summary>
        /// <returns></returns>
        public static string FriendlyUsernameRules()
        {
            var globalAttributes = GlobalAttributesCache.Get();
            string validUsernameCaption = globalAttributes.GetValue( "core.ValidUsernameCaption" );

            if ( string.IsNullOrEmpty( validUsernameCaption ) )
            {
                return string.Empty;
            }
            else
            {
                return validUsernameCaption;
            }
        }

        /// <summary>
        /// Returns a user friendly description of the password rules.
        /// </summary>
        /// <returns>A user friendly description of the password rules.</returns>
        public static string FriendlyPasswordRules()
        {
            var globalAttributes = GlobalAttributesCache.Get();
            string validPasswordCaption = globalAttributes.GetValue( "PasswordRegexFriendlyDescription" );

            if ( string.IsNullOrEmpty( validPasswordCaption ) )
            {
                return string.Empty;
            }
            else
            {
                return validPasswordCaption;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Rock.Model.UserLogin" /> and saves it to the database.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="entityTypeId">The EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the authentication service that this UserLogin user will use.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="isConfirmed">if set to <c>true</c> [is confirmed].</param>
        /// <param name="isRequirePasswordChange">if set to <c>true</c> [is require password change].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">username;Username already exists</exception>
        /// <exception cref="System.ArgumentException">
        /// entityTypeId
        /// or
        /// Invalid EntityTypeId, entity does not exist;entityTypeId
        /// or
        /// Invalid Person, person does not exist;person
        /// </exception>
        public static UserLogin Create(
            RockContext rockContext,
            Rock.Model.Person person,
            AuthenticationServiceType serviceType,
            int entityTypeId,
            string username,
            string password,
            bool isConfirmed,
            bool isRequirePasswordChange )
        {
            if ( person != null )
            {
                var userLoginService = new UserLoginService( rockContext );

                var entityType = EntityTypeCache.Get( entityTypeId );
                if ( entityType != null )
                {
                    UserLogin user = userLoginService.GetByUserName( username );
                    if ( user != null )
                    {
                        throw new ArgumentOutOfRangeException( "username", "Username already exists" );
                    }

                    DateTime createDate = RockDateTime.Now;

                    user = new UserLogin();
                    user.Guid = Guid.NewGuid();
                    user.EntityTypeId = entityTypeId;
                    user.UserName = username;
                    user.IsConfirmed = isConfirmed;
                    user.LastPasswordChangedDateTime = createDate;
                    user.PersonId = person.Id;
                    user.IsPasswordChangeRequired = isRequirePasswordChange;

                    if ( serviceType == AuthenticationServiceType.Internal )
                    {
                        var authenticationComponent = AuthenticationContainer.GetComponent( entityType.Name );
                        if ( authenticationComponent == null || !authenticationComponent.IsActive )
                        {
                            throw new ArgumentException( string.Format( "'{0}' service does not exist, or is not active", entityType.FriendlyName ), "entityTypeId" );
                        }

                        user.Password = authenticationComponent.EncodePassword( user, password );
                    }

                    userLoginService.Add( user );
                    rockContext.SaveChanges();

                    var historyCategory = CategoryCache.Get( Rock.SystemGuid.Category.HISTORY_PERSON_ACTIVITY.AsGuid(), rockContext );
                    if ( historyCategory != null )
                    {
                        var changes = new History.HistoryChangeList();
                        History.EvaluateChange( changes, "User Login", string.Empty, username );
                        HistoryService.SaveChanges( rockContext, typeof( Person ), historyCategory.Guid, person.Id, changes );
                    }

                    return user;
                }
                else
                {
                    throw new ArgumentException( "Invalid EntityTypeId, entity does not exist", "entityTypeId" );
                }
            }
            else
            {
                throw new ArgumentException( "Invalid Person, person does not exist", "person" );
            }
        }

        /// <summary>
        /// Creates a new <see cref="Rock.Model.UserLogin" /> and saves it to the database
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The <see cref="Rock.Model.Person" /> that this <see cref="UserLogin" /> will be associated with.</param>
        /// <param name="serviceType">The <see cref="Rock.Model.AuthenticationServiceType" /> type of Login</param>
        /// <param name="entityTypeId">The EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the authentication service that this UserLogin user will use.</param>
        /// <param name="username">A <see cref="System.String" /> containing the UserName.</param>
        /// <param name="password">A <see cref="System.String" /> containing the unhashed/unencrypted password.</param>
        /// <param name="isConfirmed">A <see cref="System.Boolean" /> flag indicating if the user has been confirmed.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the Username already exists.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the service does not exist or is not active.</exception>
        public static UserLogin Create(
            RockContext rockContext,
            Rock.Model.Person person,
            AuthenticationServiceType serviceType,
            int entityTypeId,
            string username,
            string password,
            bool isConfirmed )
        {
            return UserLoginService.Create( rockContext, person, serviceType, entityTypeId, username, password, isConfirmed, false );
        }

        /// <summary>
        /// Call this method if the log in attempt fails.  Updates the
        /// <see cref="Rock.Model.UserLogin"/> failed password attempt count.
        /// </summary>
        /// <param name="user">The <see cref="Rock.Model.UserLogin"/> to update the failure count on.</param>
        public static void UpdateFailureCount( UserLogin user )
        {
            var globalAttributes = GlobalAttributesCache.Get();

            // Get the global attribute that defines what the window in minutes of time
            // are between the first unsuccessful log in and the point in time where those
            // failed log ins will be forgiven
            var passwordAttemptWindow = globalAttributes.GetValue( "PasswordAttemptWindow" ).AsIntegerOrNull() ?? 0;
            var passwordAttemptWindowMinutes = TimeSpan.FromMinutes( passwordAttemptWindow );

            // Get the global attribute that defines the total number of failed attempts that are
            // permitted within the window before the use is locked out
            var maxInvalidPasswordAttempts = globalAttributes.GetValue( "MaxInvalidPasswordAttempts" ).AsIntegerOrNull() ?? int.MaxValue;

            // Get the current state of this user's failed log in attempts
            var firstAttempt = user.FailedPasswordAttemptWindowStartDateTime ?? DateTime.MinValue;
            var attempts = user.FailedPasswordAttemptCount ?? 0;
            var endOfWindow = firstAttempt.Add( passwordAttemptWindowMinutes );

            // Determine if the user is still inside the window where failed log ins have not
            // yet been forgiven
            var inWindow = RockDateTime.Now < endOfWindow;

            if ( inWindow )
            {
                // The user is within the window meaning the failed log ins are accumulating and
                // cannot yet be forgiven
                attempts++;

                if ( attempts >= maxInvalidPasswordAttempts )
                {
                    user.IsLockedOut = true;
                    user.LastLockedOutDateTime = RockDateTime.Now;
                }

                user.FailedPasswordAttemptCount = attempts;
            }
            else
            {
                // The user is outside the window, so failed log ins can be forgiven and the
                // database record tracking fields can be reset to only reflect this single
                // failed log in attempt
                user.FailedPasswordAttemptCount = 1;
                user.FailedPasswordAttemptWindowStartDateTime = RockDateTime.Now;
            }
        }

        /// <summary>
        /// Gets the <see cref="UserLogin"/> that matches the username and password.
        /// This method verifies the password as well all other conditions for the
        /// account to be considered valid.
        /// </summary>
        /// <param name="username">The username to be retrieved.</param>
        /// <param name="password">The password to be verified.</param>
        /// <returns>The result of the authentication attempt and the <see cref="UserLogin"/> that was looked up. Always check the state as the UserLogin is always included except when not found.</returns>
        /// <remarks>
        /// Internal until the concept of a tuple that returns the state and the
        /// login is approved for general use. The intention is that this method
        /// can be called from various places that need to validate a username
        /// and password. We then have a central place to update security logic.
        ///
        /// But we can't just return null because often the caller needs to know
        /// the specific reason authentication failed so it can take appropriate
        /// action or display the correct message. For example, a Login block
        /// needs to know if the failure was because the login wasn't confirmed.
        /// It also needs the UserLogin that was found so it can send a confirmation
        /// e-mail to the individual.
        /// </remarks>
        internal (UserLoginValidationState State, UserLogin UserLogin) GetAuthenticatedUserLogin( string username, string password )
        {
            var userLogin = GetByUserName( username );

            // Check if either login is null or the authentication component is
            // not known.
            if ( userLogin?.EntityType == null )
            {
                return (UserLoginValidationState.InvalidUsername, null);
            }

            var component = AuthenticationContainer.GetComponent( userLogin.EntityType.Name );

            var excludedAuthProviderTypes = new List<Guid>
            {
                SystemGuid.EntityType.AUTHENTICATION_PIN.AsGuid()
            };

            if( component?.EntityType?.Guid != null && excludedAuthProviderTypes.Contains( component.EntityType.Guid ) )
            {
                return ( UserLoginValidationState.InvalidPassword, userLogin );
            }

            // Check if the password is valid for this login.
            if ( component?.IsActive != true || !component.Authenticate( userLogin, password ) )
            {
                return (UserLoginValidationState.InvalidPassword, userLogin);
            }

            // Make sure the login is confirmed, otherwise login is not allowed.
            if ( userLogin.IsConfirmed != true )
            {
                return (UserLoginValidationState.NotConfirmed, userLogin);
            }

            // Make sure the login is not locked out.
            if ( userLogin.IsLockedOut == true )
            {
                return (UserLoginValidationState.LockedOut, userLogin);
            }

            return (UserLoginValidationState.Valid, userLogin);
        }

        /// <summary>
        /// Sends the confirmation e-mail to the person this <see cref="UserLogin"/>
        /// is associated with. The e-mail will contain a link to the page where
        /// the person can confirm they own the account.
        /// </summary>
        /// <param name="userLogin">The user login.</param>
        /// <param name="systemEmailGuid">The system email unique identifier.</param>
        /// <param name="confirmationPage">The confirmation page.</param>
        /// <param name="baseUrl">The base URL to use if known, such as https://www.rockrms.com/. If <c>null</c> the default domain for the page will be used.</param>
        /// <param name="mergeFields">The additional merge fields to provide.</param>
        internal static void SendConfirmationEmail( UserLogin userLogin, Guid systemEmailGuid, PageCache confirmationPage, string baseUrl, Dictionary<string, object> mergeFields )
        {
            string url = null;

            // Check for the required parameters.
            if ( userLogin == null )
            {
                throw new ArgumentNullException( nameof( userLogin ) );
            }

            if ( confirmationPage == null )
            {
                throw new ArgumentNullException( nameof( confirmationPage ) );
            }

            // Get the default route that doesn't require any parameters.
            url = confirmationPage.PageRoutes
                .Where( r => !r.Route.Contains( "{" ) )
                .OrderByDescending( r => r.IsGlobal )
                .Select( r => r.Route )
                .FirstOrDefault();

            // No route, just use legacy page id syntax.
            if ( url.IsNullOrWhiteSpace() )
            {
                url = $"/page/{confirmationPage.Id}";
            }

            // If they didn't provide a base url, then use the one for the page.
            if ( baseUrl.IsNullOrWhiteSpace() )
            {
                baseUrl = confirmationPage.Layout.Site.DefaultDomainUri.AbsoluteUri;
            }

            var confirmAccountUrl = baseUrl.TrimEnd( '/' ) + "/" + url.TrimStart( '/' );

            // Duplicate the merge fields so we don't corrupt the original
            // dictionary.
            mergeFields = mergeFields != null ? new Dictionary<string, object>( mergeFields ) : new Dictionary<string, object>();
            mergeFields.AddOrReplace( "ConfirmAccountUrl", confirmAccountUrl );
            mergeFields.AddOrReplace( "Person", userLogin.Person );
            mergeFields.AddOrReplace( "User", userLogin );

            // Send the e-mail to the on-file address for the person.
            var recipients = new List<RockEmailMessageRecipient>
            {
                new RockEmailMessageRecipient( userLogin.Person, mergeFields )
            };

            // Send it off.
            var message = new RockEmailMessage( systemEmailGuid );
            message.SetRecipients( recipients );
            message.AppRoot = "/";
            message.ThemeRoot = $"/Themes/{confirmationPage.Layout.Site.Theme}";
            message.CreateCommunicationRecord = false;
            message.Send();
        }

        #endregion
    }
}
