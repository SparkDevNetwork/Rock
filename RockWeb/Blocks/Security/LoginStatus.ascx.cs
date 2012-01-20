using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace RockWeb.Blocks.Security
{
    public partial class LoginStatus : Rock.Web.UI.Block
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            phHello.Controls.Clear();
            phActions.Controls.Clear();

            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                phHello.Controls.Add( new LiteralControl( "<span>Hello " ) );
                phHello.Controls.Add( new LiteralControl( CurrentPerson.NickName ) );
                phHello.Controls.Add( new LiteralControl( "</span>" ) );

                HtmlGenericControl aMyAccount = new HtmlGenericControl("a");
                aMyAccount.Attributes.Add( "href", ResolveUrl( "~/myaccount" ) );
                aMyAccount.InnerText = "My Account";
                phActions.Controls.Add( aMyAccount );

                phActions.Controls.Add( new LiteralControl( " | " ) );

                Rock.Web.UI.PageReference pageRef = new Rock.Web.UI.PageReference( PageInstance.Id, PageInstance.RouteId );

                Dictionary<string, string> parms = new Dictionary<string, string>();
                parms.Add( "logout", "true" );
                
                HtmlGenericControl aLogout = new HtmlGenericControl( "a" );
                aLogout.Attributes.Add( "href", PageInstance.BuildUrl( pageRef, parms ) );
                aLogout.InnerText = "Logout";
                phActions.Controls.Add( aLogout );
            }
            else
            {
                HtmlGenericControl aLogin = new HtmlGenericControl("a");
                aLogin.Attributes.Add( "href", ResolveUrl( "~/login" ) );
                aLogin.InnerText = "Login";
                phActions.Controls.Add( aLogin );
            }
        }
    }
}