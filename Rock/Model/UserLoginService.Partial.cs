// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web;
using System.Web.Security;

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
        /// Gets<see cref="Rock.Model.UserLogin"/> by User Name
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
        /// Sets the a <see cref="Rock.Model.UserLogin">UserLogin's</see> password.
        /// </summary>
        /// <param name="user">The <see cref="Rock.Model.UserLogin"/> to change the password for.</param>
        /// <param name="password">A <see cref="System.String"/> representing the new password.</param>
        public void SetPassword( UserLogin user, string password )
        {
            var entityType = EntityTypeCache.Read( user.EntityTypeId ?? 0);

            var authenticationComponent = AuthenticationContainer.GetComponent( entityType.Name );
            if ( authenticationComponent == null || !authenticationComponent.IsActive )
                throw new Exception( string.Format( "'{0}' service does not exist, or is not active", entityType.FriendlyName ) );

            if ( authenticationComponent.ServiceType == AuthenticationServiceType.External )
                throw new Exception( "Cannot change password on external service type" );

            user.Password = authenticationComponent.EncodePassword( user, password );
            user.LastPasswordChangedDateTime = RockDateTime.Now;
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
            if ( RockDateTime.Now.CompareTo( firstAttempt.Add( window ) ) < 0 )
            {
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
                user.FailedPasswordAttemptCount = 1;
                user.FailedPasswordAttemptWindowStartDateTime = RockDateTime.Now;
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
                        if ( RockDateTime.Now.Subtract( dateTime ).Hours > 1 )
                            return null;

                        UserLogin user = this.GetByEncryptedKey( publicKey );
                        if ( user != null && user.UserName == username )
                            return user;
                    }
                }
            }

            return null;
        }

        #region Static Methods

        /// <summary>
        /// Returns the <see cref="Rock.Model.UserLogin"/> of the user who is currently logged in.
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
            var rockContext = new RockContext();

            string userName = UserLogin.GetCurrentUserName();
            if ( userName != string.Empty )
            {
                if ( userName.StartsWith( "rckipid=" ) )
                {
                    Rock.Model.PersonService personService = new Model.PersonService( rockContext );
                    Rock.Model.Person impersonatedPerson = personService.GetByEncryptedKey( userName.Substring( 8 ) );
                    if ( impersonatedPerson != null )
                        return impersonatedPerson.GetImpersonatedUser();
                }
                else
                {
                    var userLoginService = new UserLoginService( rockContext );
                    UserLogin user = userLoginService.GetByUserName( userName );

                    if ( user != null && userIsOnline )
                    {
                        // Save last activity date
                        var transaction = new Rock.Transactions.UserLastActivityTransaction();
                        transaction.UserId = user.Id;
                        transaction.LastActivityDate = RockDateTime.Now;

                        if ( ( user.IsConfirmed ?? true ) && !( user.IsLockedOut ?? false ) )
                        {
                            if ( HttpContext.Current != null && HttpContext.Current.Session != null )
                            {
                                HttpContext.Current.Session["RockUserId"] = user.Id;
                            }

                            // see if there is already a LastActivitytransaction queued for this user, and just update its LastActivityDate instead of adding another to the queue
                            var userLastActivity = Rock.Transactions.RockQueue.TransactionQueue.ToArray().OfType<Rock.Transactions.UserLastActivityTransaction>()
                                .Where( a => a.UserId == transaction.UserId ).FirstOrDefault();

                            if ( userLastActivity != null )
                            {
                                userLastActivity.LastActivityDate = transaction.LastActivityDate;
                            }
                            else
                            {
                                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                            }
                        }
                        else
                        {
                            transaction.IsOnLine = false;
                            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );

                            FormsAuthentication.SignOut();
                            return null;
                        }
                    }

                    return user;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks to see if the given password is valid according to the PasswordRegex (if defined).
        /// </summary>
        /// <param name="password">A password to verify.</param>
        /// <returns>A <see cref="System.Boolean"/> value that indicates if the password is valid. <c>true</c> if valid; otherwise <c>false</c>.</returns>
        public static bool IsPasswordValid( string password )
        {
            var globalAttributes = GlobalAttributesCache.Read();
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
        /// Returns a user friendly description of the password rules.
        /// </summary>
        /// <returns>A user friendly description of the password rules.</returns>
        public static string FriendlyPasswordRules()
        {
            var globalAttributes = GlobalAttributesCache.Read();
            string passwordRegex = globalAttributes.GetValue( "PasswordRegexFriendlyDescription" );
            if ( string.IsNullOrEmpty( passwordRegex ) )
            {
                return "";
            }
            else
            {
                return passwordRegex;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Rock.Model.UserLogin" />
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The <see cref="Rock.Model.Person" /> that this <see cref="UserLogin" /> will be associated with.</param>
        /// <param name="serviceType">The <see cref="Rock.Model.AuthenticationServiceType" /> type of Login</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="username">A <see cref="System.String" /> containing the UserName.</param>
        /// <param name="password">A <see cref="System.String" /> containing the unhashed/unencrypted password.</param>
        /// <param name="isConfirmed">A <see cref="System.Boolean" /> flag indicating if the user has been confirmed.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the Username already exists.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the service does not exist or is not active.</exception>
        public static UserLogin Create( RockContext rockContext,
            Rock.Model.Person person,
            AuthenticationServiceType serviceType,
            int entityTypeId,
            string username,
            string password,
            bool isConfirmed )
        {
            if ( person != null )
            {
                var userLoginService = new UserLoginService( rockContext );

                var entityType = EntityTypeCache.Read( entityTypeId );
                if ( entityType != null )
                {
                    UserLogin user = userLoginService.GetByUserName( username );
                    if ( user != null )
                        throw new ArgumentOutOfRangeException( "username", "Username already exists" );

                    DateTime createDate = RockDateTime.Now;

                    user = new UserLogin();
                    user.Guid = Guid.NewGuid();
                    user.EntityTypeId = entityTypeId;
                    user.UserName = username;
                    user.IsConfirmed = isConfirmed;
                    user.LastPasswordChangedDateTime = createDate;
                    user.PersonId = person.Id;

                    if ( serviceType == AuthenticationServiceType.Internal )
                    {
                        var authenticationComponent = AuthenticationContainer.GetComponent( entityType.Name );
                        if ( authenticationComponent == null || !authenticationComponent.IsActive )
                            throw new ArgumentException( string.Format( "'{0}' service does not exist, or is not active", entityType.FriendlyName ), "entityTypeId" );

                        user.Password = authenticationComponent.EncodePassword( user, password );
                    }

                    userLoginService.Add( user );

                    var historyCategory = CategoryCache.Read( Rock.SystemGuid.Category.HISTORY_PERSON_ACTIVITY.AsGuid(), rockContext );
                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();

                        if ( historyCategory != null )
                        {
                            var changes = new List<string>();
                            History.EvaluateChange( changes, "User Login", string.Empty, username );
                            HistoryService.SaveChanges( rockContext, typeof( Person ), historyCategory.Guid, person.Id, changes );
                        }
                    } );

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
        /// Updates the last login.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        public static void UpdateLastLogin( string userName )
        {
            using ( var rockContext = new RockContext() )
            {
                var userLoginService = new UserLoginService( rockContext );
                var historyService = new HistoryService( rockContext );

                var personEntityTypeId = EntityTypeCache.Read( "Rock.Model.Person" ).Id;
                var activityCategoryId = CategoryCache.Read( Rock.SystemGuid.Category.HISTORY_PERSON_ACTIVITY.AsGuid(), rockContext ).Id;

                if ( !string.IsNullOrWhiteSpace( userName ) && !userName.StartsWith( "rckipid=" ) )
                {
                    var userLogin = userLoginService.GetByUserName( userName );
                    if ( userLogin != null )
                    {
                        userLogin.LastLoginDateTime = RockDateTime.Now;

                        if ( userLogin.PersonId.HasValue )
                        {
                            var summary = new System.Text.StringBuilder();
                            summary.AppendFormat( "User logged in with <span class='field-name'>{0}</span> username", userLogin.UserName );
                            if ( HttpContext.Current != null && HttpContext.Current.Request != null )
                            {
                                summary.AppendFormat( ", to <span class='field-value'>{0}</span>, from <span class='field-value'>{1}</span>",
                                    HttpContext.Current.Request.Url.AbsoluteUri, HttpContext.Current.Request.UserHostAddress );
                            }
                            summary.Append( "." );

                            historyService.Add( new History
                            {
                                EntityTypeId = personEntityTypeId,
                                CategoryId = activityCategoryId,
                                EntityId = userLogin.PersonId.Value,
                                Summary = summary.ToString()
                            } );
                        }
                        rockContext.SaveChanges();
                    }
                }
            }
        }

        #endregion

    }
}
