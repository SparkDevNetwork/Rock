//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.ComponentModel;
using System.Web.UI;

using Rock.Attribute;
using Rock.Model;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Block for user to change their password.
    /// </summary>
    [DisplayName( "Change Password" )]
    [Category( "Security" )]
    [Description( "Block for user to change their password." )]

    [TextField( "Invalid UserName Caption", "", false, "The User Name/Password combination is not valid.", "Captions", 0 )]
    [TextField( "Invalid Password Caption","", false, "The User Name/Password combination is not valid.", "Captions", 1 )]
    [TextField( "Success Caption","", false, "Your password has been changed", "Captions", 2 )]
    public partial class ChangePassword : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrentUser == null || !CurrentUser.IsAuthenticated )
                DisplayError( "You must login before changing your password" );

            if ( !Page.IsPostBack )
            {
                if ( CurrentUser != null )
                    tbUserName.Text = CurrentUser.UserName;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnChange control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnChange_Click( object sender, EventArgs e )
        {
            var userLoginService = new UserLoginService();
            var userLogin = userLoginService.GetByUserName( tbUserName.Text );
            if ( userLogin != null )
            {
                if ( UserLoginService.IsPasswordValid( tbPassword.Text ) )
                {
                    if ( userLoginService.ChangePassword( userLogin, tbOldPassword.Text, tbPassword.Text ) )
                    {
                        userLoginService.Save( userLogin, CurrentPersonId );

                        lSuccess.Text = GetAttributeValue( "SuccessCaption" );
                        pnlEntry.Visible = false;
                        pnlSuccess.Visible = true;
                    }
                    else
                        DisplayError( "InvalidPasswordCaption" );
                }
                else
                {
                    InvalidPassword();
                }
            }
            else
                DisplayError( "InvalidUserNameCaption" );
        }

        #endregion

        #region Methods

        private void InvalidPassword()
        {
            lInvalid.Text = UserLoginService.FriendlyPasswordRules();
            pnlInvalid.Visible = true;
        }

        private void DisplayError( string message )
        {
            lInvalid.Text = GetAttributeValue( message );
            pnlInvalid.Visible = true;
        }

        #endregion

    }
}