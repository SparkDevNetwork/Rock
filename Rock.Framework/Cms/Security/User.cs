using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Security.Cryptography;

using Rock.Services.Cms;

namespace Rock.Cms.Security
{
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

        private enum FailureType
        {
          Password = 1,
          PasswordAnswer = 2
        }

        public enum AuthenticationType
        {
            Database = 1,
            Facebook = 2, 
            ActiveDirectory = 3
        }

        #region Properties

        public override string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return maxInvalidPasswordAttempts; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return minRequiredNonAlphanumericCharacters; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return minRequiredPasswordLength; }
        }

        public override int PasswordAttemptWindow
        {
            get { return passwordAttemptWindow; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Hashed; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return passwordStrengthRegularExpression; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return false; }
        }

        public override bool EnablePasswordReset
        {
            get { return true; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        #endregion

        #region Membership Provider Methods

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

        public override bool ChangePassword( string username, string oldPassword, string newPassword )
        {
            UserService UserService = new UserService();

            Rock.Models.Cms.User user = UserService.GetUserByApplicationNameAndUsername( applicationName, username );

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

        public override bool ChangePasswordQuestionAndAnswer( string username, string password, string newPasswordQuestion, string newPasswordAnswer )
        {
            UserService UserService = new Services.Cms.UserService();

            Rock.Models.Cms.User user = UserService.GetUserByApplicationNameAndUsername( applicationName, username );

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

        public override MembershipUser CreateUser( string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status )
        {
            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs( username, password, true );

            OnValidatingPassword( args );

            if ( args.Cancel )
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            UserService UserService = new Services.Cms.UserService();

            if ( ( RequiresUniqueEmail && ( GetUserNameByEmail( UserService, email ) != String.Empty ) ) )
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            MembershipUser membershipUser = GetUser( UserService, username, false );

            if ( membershipUser == null )
            {
                DateTime createDate = DateTime.Now;

                Rock.Models.Cms.User user = new Rock.Models.Cms.User();

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
                    UserService.AddUser( user );
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

        public override bool DeleteUser( string username, bool deleteAllRelatedData )
        {
            UserService UserService = new Services.Cms.UserService();

            Rock.Models.Cms.User user = UserService.GetUserByApplicationNameAndUsername( applicationName, username );

            if ( user != null )
            {
                try
                {
                    UserService.DeleteUser( user );
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

        public override MembershipUserCollection FindUsersByEmail( string emailToMatch, int pageIndex, int pageSize, out int totalRecords )
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();

            UserService UserService = new Services.Cms.UserService();

            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;

            foreach ( Rock.Models.Cms.User user
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

        public override MembershipUserCollection FindUsersByName( string usernameToMatch, int pageIndex, int pageSize, out int totalRecords )
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();

            UserService UserService = new Services.Cms.UserService();

            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;

            foreach ( Rock.Models.Cms.User user
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

        public override MembershipUserCollection GetAllUsers( int pageIndex, int pageSize, out int totalRecords )
        {
            MembershipUserCollection membershipUsers = new MembershipUserCollection();

            UserService UserService = new Services.Cms.UserService();

            int counter = 0;
            int startIndex = pageSize * pageIndex;
            int endIndex = startIndex + pageSize - 1;

            foreach ( Rock.Models.Cms.User user
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

        public override int GetNumberOfUsersOnline()
        {
            UserService UserService = new Services.Cms.UserService();

            TimeSpan onlineSpan = new TimeSpan( 0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0 );
            DateTime compareTime = DateTime.Now.Subtract( onlineSpan );

            return UserService.Queryable().Where( u =>
                    u.ApplicationName == applicationName &&
                    u.LastActivityDate > compareTime).Count();
        }

        public override string GetPassword( string username, string answer )
        {
            throw new ProviderException( "Password Retrieval Not Enabled" );
        }

        public override MembershipUser GetUser( string username, bool userIsOnline )
        {
            UserService UserService = new Services.Cms.UserService();
            return GetUser( UserService, username, userIsOnline );
        }

        public override MembershipUser GetUser( object providerUserKey, bool userIsOnline )
        {
            if ( providerUserKey != null && providerUserKey is int )
            {
                UserService UserService = new Services.Cms.UserService();

                Rock.Models.Cms.User user = UserService.GetUser( ( int )providerUserKey );

                if ( user != null )
                    return GetUser( UserService, user, userIsOnline );
                else
                    return null;
            }
            else
                throw new ProviderException( "Invalid providerUserKey" );
        }

        private MembershipUser GetUser( UserService UserService, string username, bool userIsOnline )
        {
            Rock.Models.Cms.User user = UserService.GetUserByApplicationNameAndUsername( applicationName, username );

            if ( user != null )
                return GetUser( UserService, user, userIsOnline );
            else
                return null;
        }

        private MembershipUser GetUser( UserService UserService, Rock.Models.Cms.User user, bool userIsOnline )
        {

            MembershipUser membershipUser = GetUserFromModel( user );

            if ( userIsOnline )
            {
                user.LastActivityDate = DateTime.Now;
                UserService.Save( user, CurrentPersonId(membershipUser) );
            }

            return membershipUser;
        }

        private MembershipUser GetUserFromModel( Rock.Models.Cms.User user )
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

        public override string GetUserNameByEmail( string email )
        {
            throw new NotImplementedException( "Provider does not support GetUserNameByEmail method" );
        }

        private string GetUserNameByEmail( UserService UserService, string email )
        {
            throw new NotImplementedException( "Provider does not support GetUserNameByEmail method" );
        }

        public override string ResetPassword( string username, string answer )
        {
            if ( !EnablePasswordReset )
                throw new NotSupportedException( "Password Reset is not enabled" );

            UserService UserService = new Services.Cms.UserService();

            Rock.Models.Cms.User user = UserService.GetUserByApplicationNameAndUsername( applicationName, username );

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

        public override bool UnlockUser( string userName )
        {
            UserService UserService = new Services.Cms.UserService();

            Rock.Models.Cms.User user = UserService.GetUserByApplicationNameAndUsername( applicationName, userName );

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

        public override void UpdateUser( MembershipUser user )
        {
            UserService UserService = new Services.Cms.UserService();

            Rock.Models.Cms.User userModel = UserService.GetUserByApplicationNameAndUsername( applicationName, user.UserName );

            if ( userModel != null )
            {
                userModel.Email = user.Email;
                userModel.Comment = user.Comment;
                userModel.IsApproved = user.IsApproved;
                UserService.Save( userModel, CurrentPersonId() );
            }
        }

        public override bool ValidateUser( string username, string password )
        {
            UserService UserService = new Services.Cms.UserService();

            Rock.Models.Cms.User user = UserService.GetUserByApplicationNameAndUsername( applicationName, username );

            if ( user != null )
                return ValidateUser( UserService, user, password );
            else
                return false;
        }

        private bool ValidateUser( UserService UserService, Rock.Models.Cms.User user, string password )
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

        private void UpdateFailureCount( Rock.Models.Cms.User user, FailureType failureType )
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
            {
                if ( user.ProviderUserKey != null )
                    return ( int )user.ProviderUserKey;
                else
                    return null;
            }
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