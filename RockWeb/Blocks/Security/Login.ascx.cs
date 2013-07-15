//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Security
{
    [BooleanField( "Enable Facebook Login", "Enables the user to login using Facebook.  This assumes that the site is configured with both a Facebook App Id and Secret.", false )]
    public partial class Login : Rock.Web.UI.RockBlock
    {
        /// <summary>
        /// Handles the Load event of the RockPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            pnlMessage.Visible = false;

            // Look for active external authentication providers
            foreach ( var serviceEntry in ExternalAuthenticationContainer.Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                if ( component.AttributeValues.ContainsKey( "Active" ) && bool.Parse( component.AttributeValues["Active"][0].Value ) )
                {
                    string loginTypeName = component.GetType().Name;

                    // Check if returning from third-party authentication
                    if ( !IsPostBack && component.IsReturningFromAuthentication( Request ) )
                    {
                        string userName = string.Empty;
                        string returnUrl = string.Empty;
                        if ( component.Authenticate( Request, out userName, out returnUrl ) )
                        {
                            LoginUser( userName, returnUrl, false );
                            break;
                        }
                    }

                    var div = new HtmlGenericControl( "div" );
                    phExternalLogins.Controls.Add( div );
                    div.AddCssClass( loginTypeName + "-login" );

                    LinkButton lbLogin = new LinkButton();
                    div.Controls.Add( lbLogin );
                    lbLogin.ID = "lb" + loginTypeName + "Login";
                    lbLogin.Click += lbLogin_Click;
                    lbLogin.CausesValidation = false;

                    if ( !String.IsNullOrWhiteSpace( component.ImageUrl() ) )
                    {
                        HtmlImage img = new HtmlImage();
                        lbLogin.Controls.Add( img );
                        img.Attributes.Add( "style", "border:none" );
                        img.Src = Page.ResolveUrl( component.ImageUrl() );
                    }
                    else
                    {
                        lbLogin.Text = loginTypeName;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnLogin_Click( object sender, EventArgs e )
        {
            bool valid = false;

            if ( Page.IsValid )
            {
                var userLoginService = new UserLoginService();
                var userLogin = userLoginService.GetByUserName( tbUserName.Text );
                if ( userLogin != null && userLogin.ServiceType == AuthenticationServiceType.Internal )
                {
                    foreach ( var serviceEntry in AuthenticationContainer.Instance.Components )
                    {
                        var component = serviceEntry.Value.Value;
                        string componentName = component.GetType().FullName;

                        if (
                            userLogin.ServiceName == componentName &&
                            component.AttributeValues.ContainsKey( "Active" ) &&
                            bool.Parse( component.AttributeValues["Active"][0].Value )
                        )
                        {
                            if ( component.Authenticate( userLogin, tbPassword.Text ) )
                            {
                                valid = true;
                                string returnUrl = Request.QueryString["returnurl"];
                                LoginUser( tbUserName.Text, returnUrl, cbRememberMe.Checked );
                            }
                        }
                    }
                }
            }

            if ( !valid )
            {
                DisplayError( "Invalid Login Information" );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void lbLogin_Click( object sender, EventArgs e )
        {
            if ( sender is LinkButton )
            {
                LinkButton lb = (LinkButton)sender;

                foreach ( var serviceEntry in ExternalAuthenticationContainer.Instance.Components )
                {
                    var component = serviceEntry.Value.Value;
                    if ( !component.AttributeValues.ContainsKey( "Active" ) || bool.Parse( component.AttributeValues["Active"][0].Value ) )
                    {
                        string loginTypeName = component.GetType().Name;
                        if ( lb.ID == "lb" + loginTypeName + "Login" )
                        {
                            Uri uri = component.GenerateLoginUrl( Request );
                            Response.Redirect( uri.AbsoluteUri, false );
                            Context.ApplicationInstance.CompleteRequest();
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnNewAccount_Click( object sender, EventArgs e )
        {
            Response.Redirect( "~/NewAccount", false );
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnHelp_Click( object sender, EventArgs e )
        {
            Response.Redirect( "~/ForgotUserName", false );
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;
        }

        /// <summary>
        /// Logs in the user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="rememberMe">if set to <c>true</c> [remember me].</param>
        private void LoginUser( string userName, string returnUrl, bool rememberMe )
        {
            Rock.Security.Authorization.SetAuthCookie( userName, rememberMe, false );

            if ( returnUrl != null )
            {
                // if the url looks like it contains an account related route, redirect to the DefaultUrl
                string[] accountRoutes = new string[] 
                {
                    "changepassword",
                    "confirmaccount",
                    "forgotusername",
                    "newaccount"
                };
                
                foreach (var accountRoute in accountRoutes)
                {
                    if ( returnUrl.IndexOf( accountRoute, StringComparison.OrdinalIgnoreCase ) >= 0 )
                    {
                        returnUrl = FormsAuthentication.DefaultUrl;
                        break;
                    }
                }
            }

            Response.Redirect( returnUrl ?? FormsAuthentication.DefaultUrl, false );
            Context.ApplicationInstance.CompleteRequest();
        }
    }

    // helpful links
    //  http://blog.prabir.me/post/Facebook-CSharp-SDK-Writing-your-first-Facebook-Application.aspx
}