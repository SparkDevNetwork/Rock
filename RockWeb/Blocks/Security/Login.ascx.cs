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
using Rock.Cms;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Security
{
    [BlockProperty( 0, "Enable Facebook Login", "FacebookEnabled", "", "Enables the user to login using Facebook.  This assumes that the site is configured with both a Facebook App Id and Secret.", false, "True", "Rock", "Rock.Field.Types.Boolean" )]
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
                if ( component.AttributeValues.ContainsKey( "Active" ) && bool.Parse( component.AttributeValues["Active"].Value[0].Value ) )
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
                var userService = new UserService();
                var user = userService.GetByUserName( tbUserName.Text );
                if ( user != null && user.ServiceType == AuthenticationServiceType.Internal )
                {
                    foreach ( var serviceEntry in AuthenticationContainer.Instance.Components )
                    {
                        var component = serviceEntry.Value.Value;
                        string componentName = component.GetType().FullName;

                        if (
                            user.ServiceName == componentName &&
                            component.AttributeValues.ContainsKey( "Active" ) &&
                            bool.Parse( component.AttributeValues["Active"].Value[0].Value )
                        )
                        {
                            if ( component.Authenticate( user, tbPassword.Text ) )
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
                    if ( !component.AttributeValues.ContainsKey( "Active" ) || bool.Parse( component.AttributeValues["Active"].Value[0].Value ) )
                    {
                        string loginTypeName = component.GetType().Name;
                        if ( lb.ID == "lb" + loginTypeName + "Login" )
                        {
                            Uri uri = component.GenerateLoginUrl( Request );
                            Response.Redirect( uri.AbsoluteUri, true );
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
            Response.Redirect( "~/NewAccount", true );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnHelp_Click( object sender, EventArgs e )
        {
            Response.Redirect( "~/ForgotUserName", true );
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

        private void LoginUser( string userName, string returnUrl, bool rememberMe )
        {
            Rock.Security.Authorization.SetAuthCookie( userName, rememberMe, false );

            if ( returnUrl != null )
            {
                returnUrl = returnUrl.ToLower();

                if ( returnUrl.Contains( "changepassword" ) ||
                    returnUrl.Contains( "confirmaccount" ) ||
                    returnUrl.Contains( "forgotusername" ) ||
                    returnUrl.Contains( "newaccount" ) )
                    returnUrl = FormsAuthentication.DefaultUrl;
            }

            Response.Redirect( returnUrl ?? FormsAuthentication.DefaultUrl );
        }
    }

    // helpful links
    //  http://blog.prabir.me/post/Facebook-CSharp-SDK-Writing-your-first-Facebook-Application.aspx
}