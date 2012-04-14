//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Configuration;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace Rock.CMS
{
    public partial class User
    {
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
        /// Gets the encrypted confirmation code.
        /// </summary>
        public string ConfirmationCode
        {
            get
            {
                string identifier = string.Format( "ROCK|{0}|{1}|{2}", this.PublicKey.ToString(), this.UserName, DateTime.Now.Ticks );
                string encryptionPhrase = ConfigurationManager.AppSettings["EncryptionPhrase"];
                if ( String.IsNullOrWhiteSpace( encryptionPhrase ) )
                    encryptionPhrase = "Rock Rocks!";
                string encryptedCode = Rock.Security.Encryption.EncryptString( identifier, encryptionPhrase );
                return encryptedCode;
            }
        }

        /// <summary>
        /// Gets a urlencoded and encrypted confirmation code.
        /// </summary>
        public string ConfirmationCodeEncoded
        {
            get
            {
                return HttpUtility.UrlEncode( ConfirmationCode );
            }
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
