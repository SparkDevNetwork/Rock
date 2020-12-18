using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using RestSharp;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace com.bemaservices.Security.SSO.Authenticators
{
    /// <summary>
    /// Authenticates a user using Office 365
    /// </summary>
    [Description("Vision2 Authentication Provider (Version 1.0)")]
    [Export(typeof(AuthenticationComponent))]
    [ExportMetadata("ComponentName", "Vision2")]

    [UrlLinkField("Authorization URI", "URI for authorization requests. This is your <subdomain>.give2.church/oauth/authorize", true, "", "", 0)]
    [UrlLinkField("Token URI", "URI for token requests. This is your <subdomain>.give2.church/oauth/token", true, "", "", 1)]
    [TextField("Client Id", "This is the Client Id from the api authentication page in the Vision2 administration portal", true, "", "", 3)]
    [TextField("Client Secret", "This is the Client Secret you will obtain from the api authentication page in the Vision2 administration portal", true, "", "", 4)]
    [BooleanField("Enable Debug Mode", "Enabling this will generate exceptions at each point of the authentication process. This is very useful for troubleshooting.", false, "", 5)]
    public class Vision2 : AuthenticationComponent
    {
        private const string EXCEPTION_DEBUG_TEXT = "Vision2 SSO Debug";

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
            get { return true; }
        }

        /// <summary>
        /// Tests the Http Request to determine if authentication should be tested by this
        /// authentication provider.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public override Boolean IsReturningFromAuthentication(HttpRequest request)
        {
            return (!String.IsNullOrWhiteSpace(request.QueryString["code"]));
        }

        /// <summary>
        /// Gets the login button text.
        /// </summary>
        /// <value>
        /// The login button text.
        /// </value>
        public override string LoginButtonText => "Vision2";

        /// <summary>
        /// Generates the login URL.
        /// </summary>
        /// <param name="request">Forming the URL to obtain user consent</param>
        /// <returns></returns>
        public override Uri GenerateLoginUrl(HttpRequest request)
        {
            string authorizationURI = GetAttributeValue("AuthorizationURI");
            string clientId = GetAttributeValue("ClientId");
            string returnUrl = request.QueryString["returnurl"];
            string redirectUri = GetRedirectUrl(request);
            string newUrl = string.Format("{0}?client_id={1}&redirect_uri={2}&response_type=code&scope=openid",
                authorizationURI,
                clientId,
                HttpUtility.UrlEncode(redirectUri)
            );

            return new Uri(newUrl);
        }

        /// <summary>
        /// Authenticates the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="username">The username.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns></returns>
        public override Boolean Authenticate(HttpRequest request, out string username, out string returnUrl)
        {
            username = string.Empty;
            returnUrl = request.QueryString["redirect_uri"];
            string redirectUri = GetRedirectUrl(request);
            string tokenURI = GetAttributeValue("TokenURI");
            bool debugModeEnabled = GetAttributeValue("EnableDebugMode").AsBoolean();


            try
            {
                // Get a new OAuth Access Token for the 'code' that was returned from the Office 365 user consent redirect
                var restClient = new RestClient(tokenURI);
                var restRequest = new RestRequest(Method.POST);
                restRequest.AddParameter("code", request.QueryString["code"]);
                restRequest.AddParameter("client_id", GetAttributeValue("ClientId"));
                restRequest.AddParameter("client_secret", GetAttributeValue("ClientSecret"));

                var restResponse = restClient.Execute(restRequest);

                if (debugModeEnabled)
                {
                    var exceptionText = string.Format("Access Token: {0}", restResponse.Content);
                    ExceptionLogService.LogException(new Exception(exceptionText, new Exception(EXCEPTION_DEBUG_TEXT)));
                }

                if (restResponse.StatusCode == HttpStatusCode.OK)
                {
                    var accesstokenresponse = JsonConvert.DeserializeObject<V2APIResult<V2OAuthClientResponse>>(restResponse.Content);

                    if (debugModeEnabled)
                    {
                        var exceptionText = string.Format("User: {0}", restResponse.Content);
                        ExceptionLogService.LogException(new Exception(exceptionText, new Exception(EXCEPTION_DEBUG_TEXT)));
                    }

                    if (accesstokenresponse.HttpStatus == (int)HttpStatusCode.OK)
                    {
                        string accessToken = accesstokenresponse.Data.access_token;
                        GetVision2User(accesstokenresponse.Data.profile, accesstokenresponse.Data.user_id);
                    }

                }
                else
                {
                    var exceptionText = string.Format("User: {0}", restResponse.Content);
                    ExceptionLogService.LogException(new Exception(exceptionText, new Exception(EXCEPTION_DEBUG_TEXT)));
                }
            }

            catch (Exception ex)
            {
                ExceptionLogService.LogException(ex, HttpContext.Current);
            }

            return !string.IsNullOrWhiteSpace(username);

        }

        /// <summary>
        /// Gets the URL of an image that should be displayed.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override String ImageUrl()
        {
            return string.Empty;
        }

        private string GetRedirectUrl(HttpRequest request)
        {
            Uri uri = new Uri(request.Url.ToString());
            return uri.Scheme + "://" + uri.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped) + uri.LocalPath;
        }

        /// <summary>
        /// Authenticates the specified user name and password
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public override bool Authenticate(UserLogin user, string password)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password"></param>
        /// <returns></returns>
        public override string EncodePassword(UserLogin user, string password)
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
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool ChangePassword(UserLogin user, string oldPassword, string newPassword, out string warningMessage)
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

        /// <summary>
        /// Gets the name of the Vision2 user.
        /// </summary>
        /// <param name="V2Profile">The Vision2 Profile.</param>
        /// <param name="userId">Vision2 userId</param>
        /// <returns></returns>
        public static string GetVision2User(V2Profile vision2Profile, string userId)
        {


            string username = string.Empty;
            string email = vision2Profile?.PrimaryEmail.EmailAddress;


            string userName = "vision2_" + userId;
            UserLogin user = null;

            using (var rockContext = new RockContext())
            {

                // Query for an existing user 
                var userLoginService = new UserLoginService(rockContext);
                user = userLoginService.GetByUserName(userName);

                // If no user was found, see if we can find a match in the person table
                if (user == null)
                {
                    // Get name/email from Office 365 login
                    string lastName = vision2Profile.PrimaryName.FirstName;
                    string firstName = vision2Profile.PrimaryName.LastName;

                    Person person = null;

                    // If person had an email, get the first person with the same name and email address.
                    if (email.IsNotNullOrWhiteSpace())
                    {
                        var personService = new PersonService(rockContext);
                        person = personService.FindPerson(firstName, lastName, email, true);
                    }

                    var personRecordTypeId = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid()).Id;
                    var personStatusPending = DefinedValueCache.Get(Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING.AsGuid()).Id;

                    rockContext.WrapTransaction(() =>
                    {
                        if (person == null)
                        {
                            person = new Person();
                            person.IsSystem = false;
                            person.RecordTypeValueId = personRecordTypeId;
                            person.RecordStatusValueId = personStatusPending;
                            person.FirstName = firstName;
                            person.LastName = lastName;
                            person.Email = email;
                            person.IsEmailActive = true;
                            person.EmailPreference = EmailPreference.EmailAllowed;
                            person.Gender = Gender.Unknown;


                            if (person != null)
                            {
                                PersonService.SaveNewPerson(person, rockContext, null, false);
                            }
                        }

                        if (person != null)
                        {
                            int typeId = EntityTypeCache.Get(typeof(Vision2)).Id;
                            user = UserLoginService.Create(rockContext, person, AuthenticationServiceType.External, typeId, userName, "vision2", true);
                        }

                    });
                }

                if (user != null)
                {
                    return user.UserName;
                }

                return username;
            }
        }

        #region Models


        public class VisionModelValue
        {
            public string AttemptedValue { get; set; }
        }

        public class VisionModelError
        {
            public string ErrorMessage { get; set; }
        }

        public class VisionModelState
        {
            public VisionModelValue Value { get; set; }
            public List<VisionModelError> Errors { get; set; }
        }

        public class V2APIResult<T>
        {
            public String Message { get; set; }
            public String Exception { get; set; }

            public Int32 HttpStatus { get; set; }

            public Dictionary<string, VisionModelState> ModelStateDictionary { get; set; }

            public T Data { get; set; }
        }

        public class V2OAuthClientResponse
        {
            public string user_id { get; set; }

            public string access_token { get; set; }

            public V2Profile profile { get; set; }
        }


        public class V2Profile
        {
            public int Id { get; set; }

            public ProfileName PrimaryName { get; set; }

            public ProfileAddress PrimaryAddress { get; set; }

            public ProfileEmail PrimaryEmail { get; set; }

        }

        public class ProfileName
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string MiddleName { get; set; }
        }

        public class ProfileAddress
        {
            public string Address1 { get; set; }

            public string Address2 { get; set; }

            public string City { get; set; }

            public string Region { get; set; }

            public string PostalCode { get; set; }

        }

        public class ProfileEmail
        {
            public string EmailAddress { get; set; }
        }



        #endregion
    }
}