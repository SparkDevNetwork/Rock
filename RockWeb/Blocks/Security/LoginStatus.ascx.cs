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

using Rock.Attribute;
using Rock.Web;

namespace RockWeb.Blocks.Security
{
    [LinkedPage( "My Account Page", "Page for user to manage their account (if blank will use 'MyAccount' page route)" )]
    public partial class LoginStatus : Rock.Web.UI.RockBlock
    {
        string action = string.Empty;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var url = LinkedPageUrl( "MyAccountPage" );
            if (string.IsNullOrWhiteSpace(url))
            {
                url = ResolveRockUrl( "~/MyAccount" );
            }
            hlMyAccount.NavigateUrl = url;
        }

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
            {
                var site = CurrentPage.Layout.Site;
                if ( site.LoginPageId.HasValue )
                {
                    var pageReference = new PageReference( site.LoginPageId.Value );
                    if ( site.LoginPageRouteId.HasValue )
                    {
                        pageReference.RouteId = site.LoginPageRouteId.Value;
                    }

                    var parms = new Dictionary<string, string>();

                    // if the QueryString already has a returnUrl, use that, otherwise redirect to RawUrl
                    parms.Add( "returnurl", Request.QueryString["returnUrl"] ?? Server.UrlEncode( Request.RawUrl ) );
                    pageReference.Parameters = parms;

                    Response.Redirect( pageReference.BuildUrl() );
                }
                else
                {
                    FormsAuthentication.RedirectToLoginPage();
                }
            }
            else
            {
                FormsAuthentication.SignOut();

                Response.Redirect( CurrentPageReference.BuildUrl() );
                Context.ApplicationInstance.CompleteRequest();
            }

        }
    }
}