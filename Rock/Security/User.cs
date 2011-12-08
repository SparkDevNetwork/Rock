//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Configuration.Provider;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using Rock.CMS;

namespace Rock.Security
{
    /// <summary>
    /// Rock's Membership Provider
    /// </summary>
    public class User : MembershipProvider
    {
        private string applicationName;
        private int maxInvalidPasswordAttempts;
        private int passwordAttemptWindow;
        private int minRequiredNonAlphanumericCharacters;
        private int minRequiredPasswordLength;
        private string passwordStrengthRegularExpression;

        private string validationKey = "D42E08ECDE448643C528C899F90BADC9411AE07F74F9BA00A81BA06FD17E3D6BA22C4AE6947DD9686A35E8538D72B471F14CDB31BD50B9F5B2A1C26E290E5FC2";

        private int newPasswordLength = 8;

        #region Enums

        private enum FailureType
        {
          Password = 1,
          PasswordAnswer = 2
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

        #endregion

        #region Properties

        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <returns>The name of the application using the custom membership provider.</returns>
        public override string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <returns>The number of invalid password or password-answer attempts allowed before the membership user is locked out.</returns>
        public override int MaxInvalidPasswordAttempts
        {
            get { return maxInvalidPasswordAttempts; }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <returns>The minimum number of special characters that must be present in a valid password.</returns>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return minRequiredNonAlphanumericCharacters; }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <returns>The minimum length required for a password. </returns>
        public override int MinRequiredPasswordLength
        {
            get { return minRequiredPasswordLength; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <returns>The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.</returns>
        public override int PasswordAttemptWindow
        {
            get { return passwordAttemptWindow; }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <returns>One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"/> values indicating the format for storing passwords in the data store.</returns>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Hashed; }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <returns>A regular expression used to evaluate a password.</returns>
        public override string PasswordStrengthRegularExpression
        {
            get { return passwordStrengthRegularExpression; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <returns>true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.</returns>
        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <returns>true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.</returns>
        public override bool RequiresUniqueEmail
        {
            get { return false; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <returns>true if the membership provider supports password reset; otherwise, false. The default is true.</returns>
        public override bool EnablePasswordReset
        {
            get { return true; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <returns>true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.</returns>
        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        #endregion

        #region Membership Provider Methods

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        ///   
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        ///   
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.</exception>
        public override void Initialize( string name, System.Collections.Specialized.NameValueCollection config )
        {
            if ( config == null )
                throw new ArgumentNullException( "config" );

            if ( name == null || name.Length == 0 )
            {
                name = "RockChMS";
            }

            if ( String.IsNullOrEmpty( config["description"] ) )
            {
                config.Remove( "description" );
                config.Add( "description", "Rock ChMS" );
            }

            base.Initialize( name, config );

            applicationName = GetConfigValue( config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath );
            maxInvalidPasswordAttempts = Convert.ToInt32( GetConfigValue( config["maxInvalidPasswordAttempts"], "5" ) );
            passwordAttemptWindow = Convert.ToInt32( GetConfigValue( config["passwordAttemptWindow"], "10" ) );
            minRequiredNonAlphanumericCharacters = Convert.ToInt32( GetConfigValue( config["minRequiredAlphaNumericCharacters"], "1" ) );
            minRequiredPasswordLength = Convert.ToInt32( GetConfigValue( config["minRequiredPasswordLength"], "7" ) );
            passwordStrengthRegularExpression = Convert.ToString( GetConfigValue( config["passwordStrengthRegularExpression"], String.Empty ) );

            //Get encryption and decryption key information from the configuration.
            AspNetHostingPermissionLevel currentLevel = GetCurrentTrustLevel();
            if ( currentLevel == AspNetHostingPermissionLevel.High || currentLevel == AspNetHostingPermissionLevel.Unrestricted )
            {
                //System.Configuration.Configuration cfg = WebConfigurationManager.OpenWebConfiguration( System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath );
                //MachineKeySection machineKey = cfg.GetSection( "system.web/machineKey" ) as MachineKeySection;
                //if ( machineKey.ValidationKey.Contains( "AutoGenerate" ) )
                //    throw new ProviderException( "Hashed passwords are not supported with auto-generated keys." );
                //else
                //    validationKey = machineKey.ValidationKey;
            }
        }

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <param name="username">The user to update the password for.</param>
        /// <param name="oldPassword">The current password for the specified user.</param>
        /// <param name="newPassword">The new password for the specified user.</param>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        public override bool ChangePassword( string username, string oldPassword, string newPassword )
        {
            UserService UserService = new UserService();

            Rock.CMS.User user = UserService.GetByApplicationNameAndUsername( applicationName, username );

            if ( user != null )
            {
                if ( !ValidateUser( UserService, user, oldPassword ) )
                    return false;

                ValidatePasswordEventArgs args = new ValidatePasswordEventArgs( username, newPassword, true );

                OnValidatingPassword( args );

                if ( args.Cancel )
                {
                    if ( args.FailureInformation != null )
                        throw args.FailureInformation;
                    else
                        throw new Exception( "Change password canceled due to new password validation failure" );
                }

                user.Password = EncodePassword( newPassword );
                user.LastPasswordChangedDate = DateTime.Now;
                UserService.Save( user, CurrentPersonId() );

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Processes a request to update the password question and answer for a membership user.
        /// </summary>
        /// <param name="username">The user to change the password question and answer for.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <param name="newPasswordQuestion">The new password question for the specified user.</param>
        /// <param name="newPasswordAnswer">The new password answer for the specified user.</param>
        /// <returns>
        /// true if the password question and answer are updated successfully; otherwise, false.
        /// </returns>
        public override bool ChangePasswordQuestionAndAnswer( string username, string password, string newPasswordQuestion, string newPasswordAnswer )
        {
            UserService UserService = new CMS.UserService();

            Rock.CMS.User user = UserService.GetByApplicationNameAndUsername( applicationName, username );

            if ( user != null )
            {
                if ( !ValidateUser( UserService, user, password ) )
                    return false;

                user.PasswordQuestion = newPasswordQuestion;
                user.PasswordAnswer = newPasswordAnswer;
                UserService.Save( user, CurrentPersonId() );
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <param name="username">The user name for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The e-mail address for the new user.</param>
        /// <param name="passwordQuestion">The password question for the new user.</param>
        /// <param name="passwordAnswer">The password answer for the new user</param>
        /// <param name="isApproved">Whether or not the new user is approved to be validated.</param>
        /// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
        /// <param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"/> enumeration value indicating whether the user was created successfully.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the information for the newly created user.
        /// </returns>
        public override MembershipUser CreateUser( string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status )
        {
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs( username, password, true );

            OnValidatingPassword( args );

            if ( args.Cancel )
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            UserService UserService = new CMS.UserService();

            if ( ( RequiresUniqueEmail && ( GetUserNameByEmail( UserService, email ) != String.Empty ) ) )
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            MembershipUser membershipUser = GetUser( UserService, username, false );

            if ( membershipUser == null )
            {
                DateTime createDate = DateTime.Now;

                Rock.CMS.User user = new Rock.CMS.User();

                if ( providerUserKey != null && providerUserKey is int )
                    user.PersonId = ( int )providerUserKey;
                else
                {
                    status = MembershipCreateStatus.InvalidProviderUserKey;
                    return null;
                }

                user.ApplicationName = applicationName;
                user.Username = username;
                user.Password = EncodePassword( password );
                user.PasswordQuestion = passwordQuestion;
                user.PasswordAnswer = passwordAnswer;
                user.IsApproved = isApproved;
                user.Comment = string.Empty;
                user.CreationDate = createDate;
                user.LastPasswordChangedDate = createDate;
                user.LastActivityDate = createDate;
                user.IsLockedOut = false;
                user.LastLockedOutDate = createDate;
                user.FailedPasswordAttemptCount = 0;
                user.FailedPasswordAttemptWindowStart = createDate;
                user.FailedPasswordAnswerAttemptCount = 0;
                user.FailedPasswordAnswerAttemptWindowStart = createDate;
                user.AuthenticationType = (int)AuthenticationType.Database;

                try
                {
                    UserService.Add( user, CurrentPersonId() );
                    UserService.Save( user, CurrentPersonId() );
                    status = MembershipCreateStatus.Success;

                }
                catch 
                {
                    status = MembershipCreateStatus.ProviderError;
                }

                return GetUser( UserService, user, false );

            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }
        }

        /// <summary>
        /// Removes a user from the membership data source.
        /// </summary>
        /// <param name="username">The name of the user to delete.</param>
        /// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        /// <returns>
        /// true if the user was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteUser( string username, bool deleteAllRelatedData )
        {
            UserService UserService = new CMS.UserService();

            Rock.CMS.User user = UserService.GetByApplicationNameAndUsername( applicationName, username );

            if ( user != null )
            {
                try
                {
                    UserService.Delete( user, CurrentPersonId() );
                    UserService.Save( user, CurrentPersonId() );
                    return true;
                }
                catch ( SystemException ex )
                {
                    throw new ProviderException( "Error occurred attempting to delete User", ex );
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <param name="emailToMatch">The e-mail address to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        public override MembershipUserCollection FindUsersByEmail( string emailToMatch, int pageIndex, int pageSize, out int totalRecords )
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();

            UserService UserService = new CMS.UserService();

            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;

            foreach ( Rock.CMS.User user
                in UserService.Queryable().Where( u =>
                    u.ApplicationName == applicationName &&
                    u.Email.ToLower().StartsWith( emailToMatch.ToLower() ) ) )
            {
                if ( counter >= startIndex && counter <= endIndex )
                    membershipUsers.Add( GetUserFromModel( user ) );

                counter++;
            }

            totalRecords = counter;
            return membershipUsers;
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        public override MembershipUserCollection FindUsersByName( string usernameToMatch, int pageIndex, int pageSize, out int totalRecords )
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();

            UserService UserService = new CMS.UserService();

            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;

            foreach ( Rock.CMS.User user
                in UserService.Queryable().Where( u =>
                    u.ApplicationName == applicationName &&
                    u.Username.ToLower().StartsWith( usernameToMatch.ToLower() ) ) )
            {
                if ( counter >= startIndex && counter <= endIndex )
                    membershipUsers.Add( GetUserFromModel( user ) );

                counter++;
            }

            totalRecords = counter;
            return membershipUsers;
        }

        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        public override MembershipUserCollection GetAllUsers( int pageIndex, int pageSize, out int totalRecords )
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();

            UserService UserService = new CMS.UserService();

            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;

            foreach ( Rock.CMS.User user
                in UserService.Queryable().Where( u =>
                    u.ApplicationName == applicationName ) ) 
            {
                if ( counter >= startIndex && counter <= endIndex )
                    membershipUsers.Add( GetUserFromModel( user ) );

                counter++;
            }

            totalRecords = counter;
            return membershipUsers;
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline()
        {
            UserService UserService = new CMS.UserService();

            TimeSpan onlineSpan = new TimeSpan( 0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0 );
            DateTime compareTime = DateTime.Now.Subtract( onlineSpan );

            return UserService.Queryable().Where( u =>
                    u.ApplicationName == applicationName &&
                    u.LastActivityDate > compareTime).Count();
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source.
        /// </summary>
        /// <param name="username">The user to retrieve the password for.</param>
        /// <param name="answer">The password answer for the user.</param>
        /// <returns>
        /// The password for the specified user name.
        /// </returns>
        public override string GetPassword( string username, string answer )
        {
            throw new ProviderException( "Password Retrieval Not Enabled" );
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="username">The name of the user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        public override MembershipUser GetUser( string username, bool userIsOnline )
        {
            if ( username.Trim() == string.Empty )
                return null;

            UserService UserService = new CMS.UserService();
            return GetUser( UserService, username, userIsOnline );
        }

        /// <summary>
        /// Gets user information from the data source based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        public override MembershipUser GetUser( object providerUserKey, bool userIsOnline )
        {
            if ( providerUserKey != null && providerUserKey is int )
            {
                UserService UserService = new CMS.UserService();

                Rock.CMS.User user = UserService.Get( ( int )providerUserKey );

                if ( user != null )
                    return GetUser( UserService, user, userIsOnline );
                else
                    return null;
            }
            else
                throw new ProviderException( "Invalid providerUserKey" );
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <param name="UserService">The user service.</param>
        /// <param name="username">The username.</param>
        /// <param name="userIsOnline">if set to <c>true</c> [user is online].</param>
        /// <returns></returns>
        private MembershipUser GetUser( UserService UserService, string username, bool userIsOnline )
        {
            Rock.CMS.User user = UserService.GetByApplicationNameAndUsername( applicationName, username );

            if ( user != null )
                return GetUser( UserService, user, userIsOnline );
            else
                return null;
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <param name="UserService">The user service.</param>
        /// <param name="user">The user.</param>
        /// <param name="userIsOnline">if set to <c>true</c> [user is online].</param>
        /// <returns></returns>
        private MembershipUser GetUser( UserService UserService, Rock.CMS.User user, bool userIsOnline )
        {

            MembershipUser membershipUser = GetUserFromModel( user );

            if ( userIsOnline )
            {
                user.LastActivityDate = DateTime.Now;
                UserService.Save( user, CurrentPersonId(membershipUser) );
            }

            return membershipUser;
        }

        /// <summary>
        /// Gets the user from model.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private MembershipUser GetUserFromModel( Rock.CMS.User user )
        {
            DateTime now = DateTime.Now;
            
            return new MembershipUser(
                this.Name,
                user.Username,
                user.PersonId,
                user.Email,
                user.PasswordQuestion,
                user.Comment,
                user.IsApproved ?? true,
                user.IsLockedOut ?? false,
                user.CreationDate ?? now,
                user.LastLoginDate ?? now,
                user.LastActivityDate ?? now,
                user.LastPasswordChangedDate ?? now,
                user.LastLockedOutDate ?? now );

        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address to search for.</param>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return null.
        /// </returns>
        public override string GetUserNameByEmail( string email )
        {
            throw new NotImplementedException( "Provider does not support GetUserNameByEmail method" );
        }

        /// <summary>
        /// Gets the user name by email.
        /// </summary>
        /// <param name="UserService">The user service.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        private string GetUserNameByEmail( UserService UserService, string email )
        {
            throw new NotImplementedException( "Provider does not support GetUserNameByEmail method" );
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user.</param>
        /// <returns>
        /// The new password for the specified user.
        /// </returns>
        public override string ResetPassword( string username, string answer )
        {
            if ( !EnablePasswordReset )
                throw new NotSupportedException( "Password Reset is not enabled" );

            UserService UserService = new CMS.UserService();

            Rock.CMS.User user = UserService.GetByApplicationNameAndUsername( applicationName, username );

            if ( user != null )
            {
                if ( string.IsNullOrEmpty( answer ) && RequiresQuestionAndAnswer )
                {
                    UpdateFailureCount( user, FailureType.PasswordAnswer );
                    throw new ProviderException( "Password answer required for password reset" );
                }

                string newPassword = System.Web.Security.Membership.GeneratePassword( newPasswordLength, MinRequiredNonAlphanumericCharacters );

                ValidatePasswordEventArgs args = new ValidatePasswordEventArgs( username, newPassword, true );

                OnValidatingPassword( args );

                if ( args.Cancel )
                {
                    if ( args.FailureInformation != null )
                        throw args.FailureInformation;
                    else
                        throw new MembershipPasswordException( "Reset password canceled due to password validation failure" );
                }

                if ( user.IsLockedOut ?? false )
                    throw new MembershipPasswordException( "The supplied user is locked out" );

                if ( RequiresQuestionAndAnswer && EncodePassword( answer) != user.PasswordAnswer )
                {
                    UpdateFailureCount( user, FailureType.PasswordAnswer );
                    throw new MembershipPasswordException( "Incorrect password answer" );
                }

                user.Password = EncodePassword( newPassword );
                user.LastPasswordChangedDate = DateTime.Now;
                UserService.Save( user, CurrentPersonId() );

                return newPassword;
            }
            else
                throw new MembershipPasswordException( "User does not exist" );

        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <param name="userName">The membership user whose lock status you want to clear.</param>
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        public override bool UnlockUser( string userName )
        {
            UserService UserService = new CMS.UserService();

            Rock.CMS.User user = UserService.GetByApplicationNameAndUsername( applicationName, userName );

            if ( user != null )
            {
                user.IsLockedOut = false;
                user.LastLockedOutDate = DateTime.Now;
                UserService.Save( user, CurrentPersonId() );
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Updates information about a user in the data source.
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"/> object that represents the user to update and the updated information for the user.</param>
        public override void UpdateUser( MembershipUser user )
        {
            UserService UserService = new CMS.UserService();

            Rock.CMS.User userModel = UserService.GetByApplicationNameAndUsername( applicationName, user.UserName );

            if ( userModel != null )
            {
                userModel.Email = user.Email;
                userModel.Comment = user.Comment;
                userModel.IsApproved = user.IsApproved;
                UserService.Save( userModel, CurrentPersonId() );
            }
        }

        /// <summary>
        /// Verifies that the specified user name and password exist in the data source.
        /// </summary>
        /// <param name="username">The name of the user to validate.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <returns>
        /// true if the specified username and password are valid; otherwise, false.
        /// </returns>
        public override bool ValidateUser( string username, string password )
        {
            UserService UserService = new CMS.UserService();

            Rock.CMS.User user = UserService.GetByApplicationNameAndUsername( applicationName, username );

            if ( user != null )
                return ValidateUser( UserService, user, password );
            else
                return false;
        }

        /// <summary>
        /// Validates the user.
        /// </summary>
        /// <param name="UserService">The user service.</param>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private bool ValidateUser( UserService UserService, Rock.CMS.User user, string password )
        {
            if ( EncodePassword( password ) == user.Password )
            {
                if ( user.IsApproved ?? false )
                {
                    user.LastLoginDate = DateTime.Now;
                    UserService.Save( user, CurrentPersonId() );
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                UpdateFailureCount( user, FailureType.Password );
                UserService.Save( user, CurrentPersonId() );
                return false;
            }
        }

        #endregion

        #region Utility Methods

        private string GetConfigValue( string configValue, string defaultValue )
        {
            if ( String.IsNullOrEmpty( configValue ) )
            {
                return defaultValue;
            }

            return configValue;
        }

        private string EncodePassword( string password )
        {
            HMACSHA1 hash = new HMACSHA1();
            hash.Key = HexToByte( validationKey );
            return Convert.ToBase64String( hash.ComputeHash( Encoding.Unicode.GetBytes( password ) ) );
        }

        private byte[] HexToByte( string hexString )
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for ( int i = 0; i < returnBytes.Length; i++ )
                returnBytes[i] = Convert.ToByte( hexString.Substring( i * 2, 2 ), 16 );
            return returnBytes;
        }

        private void UpdateFailureCount( Rock.CMS.User user, FailureType failureType )
        {
            DateTime FirstAttempt = (failureType == FailureType.Password ? 
                user.FailedPasswordAttemptWindowStart :
                user.FailedPasswordAnswerAttemptWindowStart) ?? DateTime.MinValue;

            int attempts = (failureType == FailureType.Password ?
                user.FailedPasswordAttemptCount :
                user.FailedPasswordAnswerAttemptCount) ?? 0;

            TimeSpan window = new TimeSpan(0, passwordAttemptWindow, 0);
            if ( DateTime.Now.CompareTo( FirstAttempt.Add( window ) ) < 0 )
            {
                // Attempt is within window
                attempts++;
                if ( attempts >= MaxInvalidPasswordAttempts )
                {
                    user.IsLockedOut = true;
                    user.LastLockedOutDate = DateTime.Now;
                }

                if ( failureType == FailureType.Password )
                    user.FailedPasswordAttemptCount = attempts;
                else
                    user.FailedPasswordAnswerAttemptCount = attempts;
            }
            else
            {
                // Attempt is outside window
                if ( failureType == FailureType.Password )
                {
                    user.FailedPasswordAttemptCount = 1;
                    user.FailedPasswordAttemptWindowStart = DateTime.Now;
                }
                else
                {
                    user.FailedPasswordAnswerAttemptCount = 1;
                    user.FailedPasswordAnswerAttemptWindowStart = DateTime.Now;
                }
            }
        }

        private int? CurrentPersonId(MembershipUser user = null)
        {
            if (user == null)
                user = Membership.GetUser();

            if ( user != null )
                return user.PersonId();

            return null;
        }

        private AspNetHostingPermissionLevel GetCurrentTrustLevel()
        {
            foreach ( AspNetHostingPermissionLevel trustLevel in
                new AspNetHostingPermissionLevel[] 
                {
                    AspNetHostingPermissionLevel.Unrestricted,
                    AspNetHostingPermissionLevel.High,
                    AspNetHostingPermissionLevel.Medium,
                    AspNetHostingPermissionLevel.Low,
                    AspNetHostingPermissionLevel.Minimal 
                } )
            {
                try
                {
                    new AspNetHostingPermission( trustLevel ).Demand();
                }
                catch ( System.Security.SecurityException )
                {
                    continue;
                }

                return trustLevel;
            }

            return AspNetHostingPermissionLevel.None;
        }

        #endregion

    }
}