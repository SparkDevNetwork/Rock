using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;

namespace church.ccv.Authentication.Azure
{
    /// <summary>
    /// Authenticates a username/password using Azure Active Directory. 
    /// NOTE: This is not a supported authentication scenario by Microsoft and is advised against using... 
    /// Microsoft recommends/requires redirect to the Azure Login Page for authentication.
    /// This authentication provider was created to mimic the Active Directory on premise provider using Azure Active Directory.
    /// You have been warned, use at your own risk!
    /// </summary>
    [Description("Azure Active Directory Authentication Provider")]
    [Export(typeof(AuthenticationComponent))]
    [ExportMetadata("ComponentName", "Azure Active Directory")]
    [TextField( "Azure Active Directory Tenant", "The Azure Active Directory Tenant Name", true, "", "Azure", 1 )]
    [TextField( "Azure Active Directory Client Application Id", "The Azure Active Directory Client Application Id", true, "", "Azure", 2 )]
    [TextField( "Azure Active Directory Service App Id URI", "The Azure Active Directory Service App Id URI", true, "", "Azure", 3 )]
    public class AzureActiveDirectory : AuthenticationComponent
    {
        /// <summary>
        /// Gets the type of the service.
        /// </summary>
        /// <value>
        /// The type of the service.
        /// </value>
        public override AuthenticationServiceType ServiceType
        {
            get { return AuthenticationServiceType.External; }
        }

        /// <summary>
        /// Determines if user is directed to another site (i.e. Facebook, Gmail, Twitter, etc) to confirm approval of using
        /// that site's credentials for authentication.
        /// </summary>
        /// <value>
        /// The requires remote authentication.
        /// </value>
        public override bool RequiresRemoteAuthentication
        {
            get { return false; }
        }

        /// <summary>
        /// Authenticates the specified user name and password
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public override bool Authenticate(UserLogin user, string password)
        {
            // Get attribute values
            string aadTenant = GetAttributeValue( "AzureActiveDirectoryTenant" );
            string aadClientApplicationId = GetAttributeValue( "AzureActiveDirectoryClientApplicationId" );
            string aadServiceAppIdURI = GetAttributeValue( "AzureActiveDirectoryServiceAppIdURI" );

           // Check for azure configuration settings
            if ( !String.IsNullOrWhiteSpace( aadTenant ) || !String.IsNullOrWhiteSpace( aadClientApplicationId ) || !String.IsNullOrWhiteSpace( aadServiceAppIdURI ) )
            {
                // Create UserCredential
                UserCredential userCredentials = new UserPasswordCredential( user.UserName, password );
                
                // Create AuthentcationContext with a null TokenCache to prevent client from using cached token for authentication.
                // Cached tokens allow authentication without re-entering of password, which means anyone can log in once the account is cached
                string authority = "https://login.microsoftonline.com/" + aadTenant;
                AuthenticationContext authContext = new AuthenticationContext( authority, null );

                // try to acquire a token using provided credentials
                // if successful then provided credentials are valid, return true authentication
                try
                {
                    AuthenticationResult result = authContext.AcquireTokenAsync( aadServiceAppIdURI, aadClientApplicationId, userCredentials ).Result;
                    return true;
                }
                catch ( Exception )
                {
                    // Any results other than successful mean provided credentials are not valid, return false authentication
                    return false;
                }
            }
            else
            {
                // Missing azure configuration settings, return false authentiation
                return false;
            }
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public override string EncodePassword(UserLogin user, string password)
        {
            return null;
        }

        /// <summary>
        /// Authenticates the user based on a request from a third-party provider.  Will set the username and returnUrl values.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Authenticate(System.Web.HttpRequest request, out string userName, out string returnUrl)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generates the login URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Uri GenerateLoginUrl(System.Web.HttpRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Tests the Http Request to determine if authentication should be tested by this
        /// authentication provider.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool IsReturningFromAuthentication(System.Web.HttpRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the URL of an image that should be displayed.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string ImageUrl()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether [supports change password].
        /// </summary>
        /// <value>
        /// <c>true</c> if [supports change password]; otherwise, <c>false</c>.
        /// </value>
        public override bool SupportsChangePassword
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="warningMessage">The warning message.</param>
        /// <returns></returns>
        public override bool ChangePassword( UserLogin user, string oldPassword, string newPassword, out string warningMessage )
        {
            warningMessage = "not supported";
            return false;
        }

        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void SetPassword(UserLogin user, string password)
        {
            throw new NotImplementedException();
        }
    }
}