using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Security;

namespace RockWeb.Blocks.Security
{
    public partial class LoginStatus : Rock.Web.UI.Block
    {
        protected void Page_Load( object sender, EventArgs e )
        {
            phHello.Controls.Clear();

            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                phHello.Controls.Add( new LiteralControl( "<span>Hello " ) );
                phHello.Controls.Add( new LiteralControl( CurrentPerson.FirstName ) );
                phHello.Controls.Add( new LiteralControl( "</span>" ) );

                phMyAccount.Visible = true;
                lbLoginLogout.Text = "Logout";
            }
            else
            {
                phMyAccount.Visible = false;
                lbLoginLogout.Text = "Login";
            }
        }
        protected void lbLoginLogout_Click( object sender, EventArgs e )
        {
            if ( lbLoginLogout.Text == "Login" )
                FormsAuthentication.RedirectToLoginPage();
            else
            {
                FormsAuthentication.SignOut();

                Rock.Web.UI.PageReference pageRef = new Rock.Web.UI.PageReference (PageInstance.Id, PageInstance.RouteId );
                Response.Redirect( PageInstance.BuildUrl( pageRef, null ) );
            }

        }
    }
}