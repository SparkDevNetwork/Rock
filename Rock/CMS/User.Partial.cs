//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;

using Rock.CRM;

namespace Rock.CMS
{
    public partial class User
    {
        private const string VALIDATION_KEY = "D42E08ECDE448643C528C899F90BADC9411AE07F74F9BA00A81BA06FD17E3D6BA22C4AE6947DD9686A35E8538D72B471F14CDB31BD50B9F5B2A1C26E290E5FC2";

 
        /// <summary>
        /// The default authorization for the selected action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public override bool DefaultAuthorization( string action )
        {
            return false;
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns></returns>
        public bool ChangePassword( string oldPassword, string newPassword )
        {
            UserService service = new UserService();
            if ( !ValidateUser( service, oldPassword ) )
                return false;

            this.Password = EncodePassword( newPassword );
            this.LastPasswordChangedDate = DateTime.Now;
            service.Save( this, null );

            return true;
        }

        /// <summary>
        /// Unlocks the user.
        /// </summary>
        public void UnlockUser()
        {
            this.IsLockedOut = false;
            new UserService().Save( this, null );
        }

        /// <summary>
        /// Validates the user.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public bool ValidateUser( UserService service, string password )
        {
            if ( EncodePassword( password ) == this.Password )
            {
                if ( this.IsConfirmed ?? false )
                    if ( !this.IsLockedOut.HasValue || !this.IsLockedOut.Value )
                    {
                        LastLoginDate = DateTime.Now;
                        service.Save( this, null );
                        return true;
                    }
                return false;
            }
            else
            {
                UpdateFailureCount();
                service.Save( this, null );
                return false;
            }
        }

        private void UpdateFailureCount()
        {
            int passwordAttemptWindow = 0;
            int maxInvalidPasswordAttempts = int.MaxValue;

            Rock.Web.Cache.OrganizationAttributes orgAttributes = Rock.Web.Cache.OrganizationAttributes.Read();
            if (!Int32.TryParse(orgAttributes.Value("PasswordAttemptWindow"), out passwordAttemptWindow))
                passwordAttemptWindow = 0;
            if (!Int32.TryParse(orgAttributes.Value("MaxInvalidPasswordAttempts"), out maxInvalidPasswordAttempts))
                maxInvalidPasswordAttempts = int.MaxValue;

            DateTime firstAttempt = this.FailedPasswordAttemptWindowStart ?? DateTime.MinValue;
            int attempts = this.FailedPasswordAttemptCount ?? 0;

            TimeSpan window = new TimeSpan(0, passwordAttemptWindow, 0);
            if ( DateTime.Now.CompareTo( firstAttempt.Add( window ) ) < 0 )
            {
                attempts++;
                if ( attempts >= maxInvalidPasswordAttempts )
                {
                    this.IsLockedOut = true;
                    this.LastLockedOutDate = DateTime.Now;
                }

                this.FailedPasswordAttemptCount = attempts;
            }
            else
            {
                this.FailedPasswordAttemptCount = 1;
                this.FailedPasswordAttemptWindowStart = DateTime.Now;
            }
        }

        #region Static Methods

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="authenticationType">Type of the authentication.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="isConfirmed">if set to <c>true</c> [is confirmed].</param>
        /// <param name="currentPersonId">The current person id.</param>
        /// <returns></returns>
        public static User CreateUser( Person person, 
            AuthenticationType authenticationType,
            string username, 
            string password,
            bool isConfirmed,
            int? currentPersonId)
        {

            UserService service = new UserService();
            User user = service.GetByUserName( username );
            if ( user != null )
                throw new ArgumentOutOfRangeException( "username", "Username already exists" );

            DateTime createDate = DateTime.Now;

            user = new User();
            user.UserName = username;
            user.Password = EncodePassword( password );
            user.IsConfirmed = isConfirmed;
            user.CreationDate = createDate;
            user.LastPasswordChangedDate = createDate;
            if (person != null)
                user.PersonId = person.Id;
            user.AuthenticationType = authenticationType;

            service.Add( user, currentPersonId );
            service.Save( user, currentPersonId );

            return user;
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns></returns>
        public static User GetCurrentUser()
        {
            return GetCurrentUser(true);
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <param name="userIsOnline">if set to <c>true</c> [user is online].</param>
        /// <returns></returns>
        public static User GetCurrentUser(bool userIsOnline)
        {
            UserService service = new UserService();
            User user = service.GetByUserName(GetCurrentUserName());

            if ( user != null && userIsOnline )
            {
                user.LastActivityDate = DateTime.Now;
                service.Save(user, null);
            }

            return user;
        }

        /// <summary>
        /// Validates the user.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public static bool ValidateUser( string username, string password )
        {
            UserService service = new UserService();
            User user = service.GetByUserName( username );
            if ( user != null )
                return user.ValidateUser( service, password );

            return false;
        }

        private static string GetCurrentUserName()
        {
          if (HostingEnvironment.IsHosted)
          {
            HttpContext current = HttpContext.Current;
            if (current != null)
              return current.User.Identity.Name;
          }
          IPrincipal currentPrincipal = Thread.CurrentPrincipal;
          if (currentPrincipal == null || currentPrincipal.Identity == null)
            return string.Empty;
          else
            return currentPrincipal.Identity.Name;
        }

        private static string EncodePassword( string password )
        {
            HMACSHA1 hash = new HMACSHA1();
            hash.Key = HexToByte( VALIDATION_KEY );
            return Convert.ToBase64String( hash.ComputeHash( Encoding.Unicode.GetBytes( password ) ) );
        }

        private static byte[] HexToByte( string hexString )
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for ( int i = 0; i < returnBytes.Length; i++ )
                returnBytes[i] = Convert.ToByte( hexString.Substring( i * 2, 2 ), 16 );
            return returnBytes;
        }

        #endregion
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
}
