//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
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
    public partial class LoginStatus : Rock.Web.UI.RockBlock
    {
        string action = string.Empty;

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            action = hfTest.Value;

            var currentPerson = CurrentPerson;
            if ( currentPerson != null )
            {
                phHello.Visible = true;
                lHello.Text = string.Format( "<span>Hello {0}</span>", currentPerson.FirstName );

                var currentUser = CurrentUser;
                phMyAccount.Visible = currentUser != null && currentUser.IsAuthenticated;
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

                Rock.Web.UI.PageReference pageRef = new Rock.Web.UI.PageReference( CurrentPage.Id, CurrentPage.RouteId );
                Response.Redirect( CurrentPage.BuildUrl( pageRef, null ), false );
                Context.ApplicationInstance.CompleteRequest();
            }

        }
    }
}