//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Web.Security;

using Rock.Attribute;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Displays currently logged in user's name along with options to Login, Logout, or manage account.
    /// </summary>
    [DisplayName( "Login Status" )]
    [Category( "Security" )]
    [Description( "Displays currently logged in user's name along with options to Login, Logout, or manage account." )]

    [LinkedPage( "My Account Page", "Page for user to manage their account (if blank will use 'MyAccount' page route)" )]
    public partial class LoginStatus : Rock.Web.UI.RockBlock
    {

        #region Fields

        string action = string.Empty;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbLoginLogout control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbLoginLogout_Click( object sender, EventArgs e )
        {
            if ( action == "Login" )
            {
                var site = RockPage.Layout.Site;
                if ( site.LoginPageId.HasValue )
                {
                    site.RedirectToLoginPage( true );
                }
                else
                {
                    FormsAuthentication.RedirectToLoginPage();
                }
            }
            else
            {
                FormsAuthentication.SignOut();

                // After logging out check to see if an anonymous user is allowed to view the current page.  If so
                // redirect back to the current page, otherwise redirect to the site's default page
                if ( RockPage.IsAuthorized( "View", null ) )
                {
                    Response.Redirect( CurrentPageReference.BuildUrl() );
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    RockPage.Layout.Site.RedirectToDefaultPage();
                }

            }
        }

        #endregion

    }
}