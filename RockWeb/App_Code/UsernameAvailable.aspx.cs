using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

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