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

using Rock.CMS;

namespace RockWeb.Blocks.Security
{
    [Rock.Attribute.Property( 0, "Invalid UserName", "InvalidUserNameCaption", "Captions", "", false,
        "The User Name/Password combination is not valid." )]
    [Rock.Attribute.Property( 1, "Invalid Password", "InvalidPasswordCaption", "Captions", "", false,
        "The User Name/Password combination is not valid." )]
    [Rock.Attribute.Property( 2, "Success", "SuccessCaption", "Captions", "", false,
        "Your password has been changed" )]
    public partial class ChangePassword : Rock.Web.UI.Block
    {

        #region Overridden Page Methods

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            pnlInvalid.Visible = false;

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
                {
                    lInvalid.Text = AttributeValue( "InvalidPasswordCaption" );
                    pnlInvalid.Visible = true;
                }
            }
            else
            {
                lInvalid.Text = AttributeValue( "InvalidUserNameCaption" );
                pnlInvalid.Visible = true;
            }
        }

        #endregion

    }
}