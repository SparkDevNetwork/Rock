<%@ WebHandler Language="C#" Class="PowerBiAuth" %>

using System;
using System.Web;
using System.Web.SessionState;
using System.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using Rock.Web.Cache;


public class PowerBiAuth : IHttpHandler, IReadOnlySessionState
{

    /// <summary>
    /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
    /// </summary>
    /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
    public void ProcessRequest( HttpContext context )
    {
        if ( context.Request["code"] != null )
        {

            string clientId = string.Empty;
            string clientSecret = string.Empty;
            string redirectUrl = context.Session["PowerBiRedirectUri"] != null ? context.Session["PowerBiRedirectUri"].ToString() : string.Empty;
            string returnUrl = context.Session["PowerBiRockReturnUrl"] != null ? context.Session["PowerBiRockReturnUrl"].ToString() : string.Empty;

            RockContext rockContext = new RockContext();
            DefinedValueService definedValueService = new DefinedValueService( rockContext );
            DefinedValue biAccountValue = null;

            // get the defined value for the request
            if ( context.Session["PowerBiAccountValueId"] != null )
            {
                if ( context.Session["PowerBiAccountValueId"].ToString() == "0" )
                {
                    // create a new account defined value
                    biAccountValue = new DefinedValue();
                    definedValueService.Add( biAccountValue );

                    var definedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS.AsGuid() ).Id;

                    // get account info from session
                    string accountName = context.Session["PowerBiAccountName"] != null ? context.Session["PowerBiAccountName"].ToString() : string.Empty;
                    string accountDescription = context.Session["PowerBiAccountDescription"] != null ? context.Session["PowerBiAccountDescription"].ToString() : string.Empty;
                    clientId = context.Session["PowerBiClientId"] != null ? context.Session["PowerBiClientId"].ToString() : string.Empty;
                    clientSecret = context.Session["PowerBiClientSecret"] != null ? context.Session["PowerBiClientSecret"].ToString() : string.Empty;


                    biAccountValue.DefinedTypeId = definedTypeId;
                    biAccountValue.Value = accountName;
                    biAccountValue.Description = accountDescription;

                    rockContext.SaveChanges();

                    // add attribute info
                    biAccountValue.LoadAttributes();

                    var clientIdAttribute = biAccountValue.Attributes
                        .Where( a => a.Value.Key == "ClientId" )
                        .Select( a => a.Value )
                        .FirstOrDefault();

                    Helper.SaveAttributeValue( biAccountValue, clientIdAttribute, clientId, rockContext );

                    var clientSecretAttribute = biAccountValue.Attributes
                        .Where( a => a.Value.Key == "ClientSecret" )
                        .Select( a => a.Value )
                        .FirstOrDefault();

                    Helper.SaveAttributeValue( biAccountValue, clientSecretAttribute, clientSecret, rockContext );

                    var redirectUrlAttribute = biAccountValue.Attributes
                        .Where( a => a.Value.Key == "RedirectUrl" )
                        .Select( a => a.Value )
                        .FirstOrDefault();

                    Helper.SaveAttributeValue( biAccountValue, redirectUrlAttribute, redirectUrl, rockContext );
                }
                else
                {
                    // load existing account
                    biAccountValue = definedValueService.Get( context.Session["PowerBiAccountValueId"].ToString().AsInteger() );
                    biAccountValue.LoadAttributes();

                    clientId = biAccountValue.AttributeValues.Where( v => v.Key == "ClientId" ).Select( v => v.Value.Value ).FirstOrDefault();
                    clientSecret = biAccountValue.AttributeValues.Where( v => v.Key == "ClientSecret" ).Select( v => v.Value.Value ).FirstOrDefault();
                }
            }
            else
            {
                context.Response.Write( "Unable to retrieve Power BI account values from earlier. Your Rock session may have been reset." );
                return;
            }

            try
            {
                // Get the auth code
                string code = context.Request.Params.GetValues( 0 )[0];

                // Get auth token from auth code       
                TokenCache TC = new TokenCache();

                AuthenticationContext AC = new AuthenticationContext( Rock.Reporting.PowerBiUtilities.AuthorityUri, TC );
                ClientCredential cc = new ClientCredential( clientId, clientSecret );

                AuthenticationResult AR = AC.AcquireTokenByAuthorizationCode( code, new Uri( redirectUrl ), cc );

                // save the access token
                var accessTokenAttribute = biAccountValue.Attributes
                    .Where( a => a.Value.Key == "AccessToken" )
                    .Select( a => a.Value )
                    .FirstOrDefault();

                Helper.SaveAttributeValue( biAccountValue, accessTokenAttribute, AR.AccessToken, rockContext );

                // save the refresh token
                var refreshTokenAttribute = biAccountValue.Attributes
                    .Where( a => a.Value.Key == "RefreshToken" )
                    .Select( a => a.Value )
                    .FirstOrDefault();

                Helper.SaveAttributeValue( biAccountValue, refreshTokenAttribute, AR.RefreshToken, rockContext );

                context.Response.Redirect( returnUrl + "?Authenticated=True" );
            }
            catch ( Exception ex )
            {
                context.Response.Write( string.Format( "An exception occurred: {0}", ex.Message ) );
            }
        }
        else
        {
                context.Response.Write( "If you were redirected here from Power BI something has gone wrong. Otherwise, this webhook is working." );
        }
    }

    /// <summary>
    /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
    /// </summary>
    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}