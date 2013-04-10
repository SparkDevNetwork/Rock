//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.Security;
using System.Web.UI;
using Rock.Attribute;
using Rock.Model;

namespace RockWeb.Blocks.Security
{
    [TextField( "Confirmed Caption", "", false,"{0}, Your account has been confirmed.  Thank you for creating the account", "Captions", 0  )]
    [TextField( "Reset Password Caption", "", false, "{0}, Enter a new password for your '{1}' account", "Captions", 1 )]
    [TextField( "Password Reset Caption", "", false, "{0}, The password for your '{1}' account has been changed", "Captions", 2 )]
    [TextField( "Delete Caption", "", false, "Are you sure you want to delete the '{0}' account?", "Captions", 3 )]
    [TextField( "Deleted Caption", "", false, "The account has been deleted.", "Captions", 4 )]
    [TextField( "Invalid Caption", "", false, "The confirmation code you've entered is not valid.  Please enter a valid confirmation code or <a href='{0}'>create a new account</a>", "Captions", 5 )]
    public partial class ConfirmAccount : Rock.Web.UI.RockBlock
    {
        private UserLoginService userLoginService = null;
        private UserLogin user = null;

        #region Properties

        /// <summary>
        /// Gets or sets the confirmation code.
        /// </summary>
        /// <value>
        /// The confirmation code.
        /// </value>
        protected string ConfirmationCode
        {
            get
            {
                string confirmationCode = ViewState["ConfirmationCode"] as string;
                return confirmationCode ?? string.Empty;
            }
            set
            {
                ViewState["ConfirmationCode"] = value;
            }
        }

        #endregion

        #region Overridden RockPage Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            pnlCode.Visible = false;
            pnlConfirmed.Visible = false;
            pnlResetPassword.Visible = false;
            pnlResetSuccess.Visible = false;
            pnlDelete.Visible = false;
            pnlDeleted.Visible = false;
            pnlInvalid.Visible = false;

            userLoginService = new UserLoginService();

            if (!Page.IsPostBack)
            {
                lDeleted.Text = GetAttributeValue( "DeletedCaption" );

                string invalidCaption = GetAttributeValue( "InvalidCaption" );
                if ( invalidCaption.Contains( "{0}" ) )
                    invalidCaption = string.Format( invalidCaption, ResolveUrl( "~/NewAccount" ) );
                lInvalid.Text = invalidCaption;

                ConfirmationCode = Request.QueryString["cc"];

                user = userLoginService.GetByConfirmationCode( ConfirmationCode );
                string action = Request.QueryString["action"] ?? "";

                switch ( action.ToLower() )
                {
                    case "delete":
                        ShowDelete();
                        break;
                    case "reset":
                        ShowResetPassword();
                        break;
                    default:
                        ShowConfirmed();
                        break;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnCodeConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnCodeConfirm_Click( object sender, EventArgs e )
        {
            ConfirmationCode = tbConfirmationCode.Text;
            user = userLoginService.GetByConfirmationCode( ConfirmationCode );
            ShowConfirmed();
        }

        /// <summary>
        /// Handles the Click event of the btnCodeReset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnCodeReset_Click( object sender, EventArgs e )
        {
            ConfirmationCode = tbConfirmationCode.Text;
            user = userLoginService.GetByConfirmationCode( ConfirmationCode );
            ShowResetPassword();
        }

        protected void btnResetPassword_Click( object sender, EventArgs e )
        {
            user = userLoginService.GetByConfirmationCode( ConfirmationCode );
            ShowResetSuccess();
        }

        /// <summary>
        /// Handles the Click event of the btnCodeDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnCodeDelete_Click( object sender, EventArgs e )
        {
            ConfirmationCode = tbConfirmationCode.Text;
            user = userLoginService.GetByConfirmationCode( ConfirmationCode );
            ShowDelete();
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            user = userLoginService.GetByConfirmationCode( ConfirmationCode );
            ShowDeleted();
        }

        #endregion

        #region Private Methods

        private void ShowCode()
        {
            pnlCode.Visible = true;
            pnlInvalid.Visible = !string.IsNullOrEmpty(ConfirmationCode);
        }

        private void ShowConfirmed()
        {
            if ( user != null )
            {
                user.IsConfirmed = true;
                userLoginService.Save( user, user.PersonId );

                Rock.Security.Authorization.SetAuthCookie( user.UserName, false, false );

                string caption = GetAttributeValue( "ConfirmedCaption" );
                if ( caption.Contains( "{0}" ) )
                    caption = string.Format( caption, user.Person.FirstName );
                lConfirmed.Text = caption;

                pnlConfirmed.Visible = true;
            }
            else
                ShowCode();
        }

        private void ShowResetPassword()
        {
            if ( user != null )
            {
                string caption = GetAttributeValue( "ResetPasswordCaption" );
                if ( caption.Contains( "{1}" ) )
                    caption = string.Format( caption, user.Person.FirstName, user.UserName );
                else if (caption.Contains( "{0}"))
                    caption = string.Format( caption, user.Person.FirstName );
                lResetPassword.Text = caption;

                pnlResetPassword.Visible = true;
            }
            else
                ShowCode();
        }

        private void ShowResetSuccess()
        {
            if ( user != null )
            {
                string caption = GetAttributeValue( "PasswordResetCaption" );
                if ( caption.Contains( "{1}" ) )
                    caption = string.Format( caption, user.Person.FirstName, user.UserName );
                else if ( caption.Contains( "{0}" ) )
                    caption = string.Format( caption, user.Person.FirstName );
                lResetSuccess.Text = caption;

                userLoginService.ChangePassword( user, tbPassword.Text );
                user.IsConfirmed = true;
                userLoginService.Save( user, user.PersonId );

                pnlResetSuccess.Visible = true;
            }
            else
                ShowCode();
        }

        private void ShowDelete()
        {
            if ( user != null )
            {
                string caption = GetAttributeValue( "DeleteCaption" );
                if ( caption.Contains( "{0}" ) )
                    caption = string.Format( caption, user.UserName );
                lDelete.Text = caption;

                pnlDelete.Visible = true;
            }
            else
                ShowCode();
        }

        private void ShowDeleted()
        {
            if ( user != null )
            {
                if ( CurrentUser != null && CurrentUser.UserName == user.UserName )
                {
                    FormsAuthentication.SignOut();
                }

                userLoginService.Delete( user, user.PersonId );
                userLoginService.Save( user, user.PersonId );

                pnlDeleted.Visible = true;
            }
            else
                ShowCode();
        }

        #endregion
    }
}