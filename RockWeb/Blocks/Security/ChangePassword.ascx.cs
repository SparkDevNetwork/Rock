//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI;

using Rock.Cms;
using Rock.Web.UI;

namespace RockWeb.Blocks.Security
{
    [BlockProperty( 0, "Invalid UserName", "InvalidUserNameCaption", "Captions", "", false,"The User Name/Password combination is not valid." )]
    [BlockProperty( 1, "Invalid Password", "InvalidPasswordCaption", "Captions", "", false,"The User Name/Password combination is not valid." )]
    [BlockProperty( 2, "Success", "SuccessCaption", "Captions", "", false,"Your password has been changed" )]
    public partial class ChangePassword : Rock.Web.UI.RockBlock
    {

        #region Overridden RockPage Methods

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

        protected void btnChange_Click( object sender, EventArgs e )
        {
            UserService userService = new UserService();
            User user = userService.GetByUserName( tbUserName.Text );
            if ( user != null )
            {
                if ( userService.ChangePassword( user, tbOldPassword.Text, tbPassword.Text ) )
                {
                    userService.Save( user, CurrentPersonId );

                    lSuccess.Text = AttributeValue( "SuccessCaption" );
                    pnlEntry.Visible = false;
                    pnlSuccess.Visible = true;
                }
                else
                    DisplayError( "InvalidPasswordCaption" );
            }
            else
                DisplayError( "InvalidUserNameCaption" );
        }

        private void DisplayError( string message )
        {
            lInvalid.Text = AttributeValue( message );
            pnlInvalid.Visible = true;
        }

        #endregion

    }
}