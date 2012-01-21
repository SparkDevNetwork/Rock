//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace Rock.CMS
{
	public partial class UserService
	{
        private const string VALIDATION_KEY = "D42E08ECDE448643C528C899F90BADC9411AE07F74F9BA00A81BA06FD17E3D6BA22C4AE6947DD9686A35E8538D72B471F14CDB31BD50B9F5B2A1C26E290E5FC2";

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
        public User Create( Rock.CRM.Person person,
            AuthenticationType authenticationType,
            string username,
            string password,
            bool isConfirmed,
            int? currentPersonId )
        {
            User user = this.GetByUserName( username );
            if ( user != null )
                throw new ArgumentOutOfRangeException( "username", "Username already exists" );

            DateTime createDate = DateTime.Now;

            user = new User();
            user.UserName = username;
            user.Password = EncodePassword( password );
            user.IsConfirmed = isConfirmed;
            user.CreationDate = createDate;
            user.LastPasswordChangedDate = createDate;
            if ( person != null )
                user.PersonId = person.Id;
            user.AuthenticationType = authenticationType;

            this.Add( user, currentPersonId );
            this.Save( user, currentPersonId );

            return user;
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns></returns>
        public bool ChangePassword( User user, string oldPassword, string newPassword )
        {
            if ( !Validate( user, oldPassword ) )
                return false;

            user.Password = EncodePassword( newPassword );
            user.LastPasswordChangedDate = DateTime.Now;

            return true;
        }

        /// <summary>
        /// Unlocks the user.
        /// </summary>
        /// <param name="user">The user.</param>
        public void Unlock( User user )
        {
            user.IsLockedOut = false;
            this.Save( user, null );
        }

        /// <summary>
        /// Validates the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public bool Validate( User user, string password )
        {
            if ( EncodePassword( password ) == user.Password )
            {
                if ( user.IsConfirmed ?? false )
                    if ( !user.IsLockedOut.HasValue || !user.IsLockedOut.Value )
                    {
                        user.LastLoginDate = DateTime.Now;
                        this.Save( user, null );
                        return true;
                    }
                return false;
            }
            else
            {
                UpdateFailureCount( user );
                this.Save( user, null );
                return false;
            }
        }

        private void UpdateFailureCount(User user)
        {
            int passwordAttemptWindow = 0;
            int maxInvalidPasswordAttempts = int.MaxValue;

            Rock.Web.Cache.OrganizationAttributes orgAttributes = Rock.Web.Cache.OrganizationAttributes.Read();
            if ( !Int32.TryParse( orgAttributes.AttributeValue( "PasswordAttemptWindow" ), out passwordAttemptWindow ) )
                passwordAttemptWindow = 0;
            if ( !Int32.TryParse( orgAttributes.AttributeValue( "MaxInvalidPasswordAttempts" ), out maxInvalidPasswordAttempts ) )
                maxInvalidPasswordAttempts = int.MaxValue;

            DateTime firstAttempt = user.FailedPasswordAttemptWindowStart ?? DateTime.MinValue;
            int attempts = user.FailedPasswordAttemptCount ?? 0;

            TimeSpan window = new TimeSpan( 0, passwordAttemptWindow, 0 );
            if ( DateTime.Now.CompareTo( firstAttempt.Add( window ) ) < 0 )
            {
                attempts++;
                if ( attempts >= maxInvalidPasswordAttempts )
                {
                    user.IsLockedOut = true;
                    user.LastLockedOutDate = DateTime.Now;
                }

                user.FailedPasswordAttemptCount = attempts;
            }
            else
            {
                user.FailedPasswordAttemptCount = 1;
                user.FailedPasswordAttemptWindowStart = DateTime.Now;
            }
        }

        #region Static Methods

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <returns></returns>
        public static User GetCurrentUser()
        {
            return GetCurrentUser( true );
        }

        /// <summary>
        /// Gets the current user.
        /// </summary>
        /// <param name="userIsOnline">if set to <c>true</c> [user is online].</param>
        /// <returns></returns>
        public static User GetCurrentUser( bool userIsOnline )
        {
            UserService userService = new UserService();
            User user = userService.GetByUserName( User.GetCurrentUserName() );

            if ( user != null && userIsOnline )
            {
                user.LastActivityDate = DateTime.Now;
                userService.Save( user, null );
            }

            return user;
        }

        /// <summary>
        /// Validates the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public static bool Validate( string username, string password )
        {
            UserService service = new UserService();
            User user = service.GetByUserName( username );
            if ( user != null )
                return service.Validate( user, password );

            return false;
        }

        internal static string EncodePassword( string password )
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
}