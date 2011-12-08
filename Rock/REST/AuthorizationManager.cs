using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web;
using System.Xml.Linq;

namespace Rock.REST
{
    /// <summary>
    /// Custom ServiceAuthorizationManager to verify that either an APIKey was included or that the user making the request is logged in
    /// </summary>
    public class AuthorizationManager : ServiceAuthorizationManager
    {
        const string APIKEY = "APIKey";

        /// <summary>
        /// Checks authorization for the given operation context based on default policy evaluation.
        /// </summary>
        /// <param name="operationContext">The <see cref="T:System.ServiceModel.OperationContext"/> for the current authorization request.</param>
        /// <returns>
        /// true if access is granted; otherwise, false. The default is true.
        /// </returns>
        protected override bool CheckAccessCore( OperationContext operationContext )
        {
            string key = string.Empty;

            // Always allow the Help interface
            var properties = operationContext.RequestContext.RequestMessage.Properties;
            if ( properties["HttpOperationName"].ToString() == "HelpPageInvoke" )
                return true;

            // If the user is currently logged in
            var currentUser = System.Web.Security.Membership.GetUser();
            if ( currentUser != null )
                return true;

            // Get the matched uriTemplate
            UriTemplateMatch template = properties["UriTemplateMatchResults"] as UriTemplateMatch;
            if (template != null && !string.IsNullOrEmpty(template.BoundVariables["apiKey"]))
            {
                // Get the apiKey value from the uriTemplate
                key = template.BoundVariables["apiKey"];

                // Read the user
                Rock.Services.Cms.UserService userService = new Rock.Services.Cms.UserService();
                Rock.Models.Cms.User user = userService.Queryable().
                    Where( u => u.ApiKey == key && u.IsApproved == true && u.IsLockedOut == false ).
                    FirstOrDefault();

                // Verify that the key is valid
                if ( user != null )
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the API key.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public string GetAPIKey( MessageProperties properties )
        {
            // Get the HTTP Request
            var requestProp = ( HttpRequestMessageProperty )properties[HttpRequestMessageProperty.Name];

            // Get the query string
            NameValueCollection queryParams = HttpUtility.ParseQueryString( requestProp.QueryString );

            // Return the API key (if present, null if not)
            return queryParams[APIKEY];
        }
    }
}