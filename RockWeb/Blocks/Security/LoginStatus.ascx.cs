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
        string action = string.Empty;

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            action = hfTest.Value;

            if (CurrentPerson != null)
            {
                phHello.Visible = true;
                lHello.Text = string.Format( "<span>Hello {0}</span>", CurrentPerson.FirstName);

                phMyAccount.Visible = CurrentUser != null && CurrentUser.IsAuthenticated;
                lbLoginLogout.Text = "Logout";
            }
            else
            {
                phHello.Visible = false;
                phMyAccount.Visible = false;
                lbLoginLogout.Text = "Login";
            }

            hfTest.Value = lbLoginLogout.Text;
        }

        protected void lbLoginLogout_Click( object sender, EventArgs e )
        {
            if ( action == "Login" )
                FormsAuthentication.RedirectToLoginPage();
            else
            {
                FormsAuthentication.SignOut();
                Session.Remove( "UserIsAuthenticated" );

                Rock.Web.UI.PageReference pageRef = new Rock.Web.UI.PageReference (PageInstance.Id, PageInstance.RouteId );
                Response.Redirect( PageInstance.BuildUrl( pageRef, null ) );
            }

        }
    }
}