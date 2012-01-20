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
    [Rock.Attribute.Property( 1, "Confirmed", "ConfirmedCaption", "Captions", "", false,
        "Your account has been confirmed.  Thank you for creating the account" )]
    [Rock.Attribute.Property( 2, "Delete", "DeleteCaption", "Captions", "", false,
        "Are you sure you want to delete the '{0}' account?" )]
    [Rock.Attribute.Property( 3, "Deleted", "DeletedCaption", "Captions", "", false,
        "The account has been deleted." )]
    [Rock.Attribute.Property( 4, "Invalid", "InvalidCaption", "Captions", "", false,
        "Sorry, but the confirmation code you are using is no longer valid.  Please try creating a new account" )]
    public partial class ResetPassword : Rock.Web.UI.Block
    {

        #region Overridden Page Methods

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }
        #endregion

        #region Events

        protected void btnChange_Click( object sender, EventArgs e )
        {
            UserService service = new UserService();
            User user = service.GetByUserName( tbUserName.Text );

        }

        #endregion

    }
}