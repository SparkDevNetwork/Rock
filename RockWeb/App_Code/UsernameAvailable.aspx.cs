//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.Security;

namespace RockWeb.AspxServices
{
    public partial class UsernameAvailable : System.Web.UI.Page
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            Response.ContentType = "application/json";

            string username = Request.QueryString.ToString();
            string output = string.Empty;

            if ( !string.IsNullOrEmpty( username ) )
            {
                MembershipUser usr = Membership.GetUser( username );
                if ( usr == null )
                    output = "{\"available\": true}";
                else
                    output = "{\"available\": false}";
            }

            Response.Write( output );
        }
    }
}