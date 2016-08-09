// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Web.Security;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Block for user to confirm a newly created login account.
    /// </summary>
    [DisplayName( "Confirm Account" )]
    [Category( "Security" )]
    [Description( "Block for user to confirm a newly created login account, usually from an email that was sent to them." )]

    [CodeEditorField( "Confirmed Caption", "", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "{0}, Your account has been confirmed.  Thank you for creating the account", "Captions", 0 )]
    [CodeEditorField( "Reset Password Caption", "", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "{0}, Enter a new password for your '{1}' account", "Captions", 1 )]
    [CodeEditorField( "Password Reset Caption", "", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "{0}, The password for your '{1}' account has been changed", "Captions", 2 )]
    [CodeEditorField( "Delete Caption", "", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "Are you sure you want to delete the '{0}' account?", "Captions", 3 )]
    [CodeEditorField( "Deleted Caption", "", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "The account has been deleted.", "Captions", 4 )]
    [CodeEditorField( "Invalid Caption", "", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "The confirmation code you've entered is not valid.  Please enter a valid confirmation code or <a href='{0}'>create a new account</a>.", "Captions", 5 )]
    [CodeEditorField( "Password Reset Unavailable Caption", "", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "This type of account does not allow passwords to be changed.  Please contact your system administrator for assistance changing your password.", "Captions", 6 )]
    [LinkedPage( "New Account Page", "Page to navigate to when user selects 'Create New Account' option (if blank will use 'NewAccount' page route)" )]
    public partial class ConfirmAccount : Rock.Web.UI.RockBlock
    {
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

        #region Base Control Methods

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
            pnlResetUnavailable.Visible = false;

            if (!Page.IsPostBack)
            {
                lDeleted.Text = GetAttributeValue( "DeletedCaption" );

                string invalidCaption = GetAttributeValue( "InvalidCaption" );
                if ( invalidCaption.Contains( "{0}" ) )
                {
                    var url = LinkedPageUrl("NewAccountPage");
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        url = ResolveRockUrl("~/NewAccount");
                    }

                    invalidCaption = string.Format( invalidCaption, url );
                }
                
                lInvalid.Text = invalidCaption;
                ConfirmationCode = Request.QueryString["cc"];
                                
                string action = Request.QueryString["action"] ?? string.Empty;

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
            ShowResetPassword();
        }

        /// <summary>
        /// Handles the Click event of the btnResetPassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnResetPassword_Click( object sender, EventArgs e )
        {
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
            ShowDelete();
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            ShowDeleted();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the code.
        /// </summary>
        private void ShowCode()
        {
            pnlCode.Visible = true;
            pnlInvalid.Visible = !string.IsNullOrEmpty( this.ConfirmationCode );
        }

        /// <summary>
        /// Shows the confirmed.
        /// </summary>
        private void ShowConfirmed()
        {
            RockContext rockContext = new RockContext();
            UserLogin user = new UserLoginService( rockContext ).GetByConfirmationCode( this.ConfirmationCode );

            if ( user != null )
            {
                user.IsConfirmed = true;
                rockContext.SaveChanges();

                Rock.Security.Authorization.SetAuthCookie( user.UserName, false, false );

                string caption = GetAttributeValue( "ConfirmedCaption" );
                if ( caption.Contains( "{0}" ) )
                {
                    caption = string.Format( caption, user.Person.FirstName );
                }

                lConfirmed.Text = caption;

                pnlConfirmed.Visible = true;
            }
            else
            {
                ShowCode();
            }
        }

        /// <summary>
        /// Shows the reset password.
        /// </summary>
        private void ShowResetPassword()
        {
            RockContext rockContext = new RockContext();
            UserLogin user = new UserLoginService( rockContext ).GetByConfirmationCode( this.ConfirmationCode );

            if ( user != null )
            {
                var component = AuthenticationContainer.GetComponent( user.EntityType.Name );
                if ( component.SupportsChangePassword )
                {
                    string caption = GetAttributeValue( "ResetPasswordCaption" );
                    if ( caption.Contains( "{1}" ) )
                    {
                        caption = string.Format( caption, user.Person.FirstName, user.UserName );
                    }
                    else if ( caption.Contains( "{0}" ) )
                    {
                        caption = string.Format( caption, user.Person.FirstName );
                    }

                    lResetPassword.Text = caption;

                    pnlResetPassword.Visible = true;
                }
                else
                {
                    pnlResetUnavailable.Visible = true;
                    lResetUnavailable.Text = GetAttributeValue( "PasswordResetUnavailableCaption" );
                }
            }
            else
            {
                ShowCode();
            }
        }

        /// <summary>
        /// Shows the reset success.
        /// </summary>
        private void ShowResetSuccess()
        {
            RockContext rockContext = new RockContext();
            UserLoginService userLoginService = new UserLoginService( rockContext );
            UserLogin user = userLoginService.GetByConfirmationCode( this.ConfirmationCode );

            if ( user != null )
            {
                if ( UserLoginService.IsPasswordValid( tbPassword.Text ) )
                {
                    string caption = GetAttributeValue( "PasswordResetCaption" );
                    if ( caption.Contains( "{1}" ) )
                    {
                        caption = string.Format( caption, user.Person.FirstName, user.UserName );
                    }
                    else if ( caption.Contains( "{0}" ) )
                    {
                        caption = string.Format( caption, user.Person.FirstName );
                    }

                    lResetSuccess.Text = caption;

                    userLoginService.SetPassword( user, tbPassword.Text );
                    user.IsConfirmed = true;
                    rockContext.SaveChanges();

                    pnlResetSuccess.Visible = true;
                }
                else
                {
                    nbMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    nbMessage.Text = UserLoginService.FriendlyPasswordRules();
                    nbMessage.Visible = true;
                    ShowResetPassword();
                }
            }
            else
            {
                ShowCode();
            }
        }

        /// <summary>
        /// Shows the delete.
        /// </summary>
        private void ShowDelete()
        {
            RockContext rockContext = new RockContext();
            UserLoginService userLoginService = new UserLoginService( rockContext );
            UserLogin user = userLoginService.GetByConfirmationCode( this.ConfirmationCode );

            if ( user != null )
            {
                string caption = GetAttributeValue( "DeleteCaption" );
                if ( caption.Contains( "{0}" ) )
                {
                    caption = string.Format( caption, user.UserName );
                }

                lDelete.Text = caption;

                pnlDelete.Visible = true;
            }
            else
            {
                ShowCode();
            }
        }

        /// <summary>
        /// Shows the deleted.
        /// </summary>
        private void ShowDeleted()
        {
            RockContext rockContext = new RockContext();
            UserLoginService userLoginService = new UserLoginService( rockContext );
            UserLogin user = userLoginService.GetByConfirmationCode( this.ConfirmationCode );

            if ( user != null )
            {
                if ( CurrentUser != null && CurrentUser.UserName == user.UserName )
                {
                    var transaction = new Rock.Transactions.UserLastActivityTransaction();
                    transaction.UserId = CurrentUser.Id;
                    transaction.LastActivityDate = RockDateTime.Now;
                    transaction.IsOnLine = false;
                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );

                    FormsAuthentication.SignOut();
                }

                userLoginService.Delete( user );
                rockContext.SaveChanges();

                pnlDeleted.Visible = true;
            }
            else
            {
                ShowCode();
            }
        }

        #endregion
    }
}