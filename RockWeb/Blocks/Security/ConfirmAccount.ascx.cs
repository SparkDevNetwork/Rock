//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Configuration;
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
    public partial class ConfirmAccount : Rock.Web.UI.Block
    {
        private User user = null;

        #region Overridden Page Methods

        protected override void OnLoad( EventArgs e )
        {
            pnlConfirmed.Visible = false;
            pnlDelete.Visible = false;
            pnlDeleted.Visible = false;
            pnlInvalid.Visible = false;

            lConfirmed.Text = AttributeValue("ConfirmedCaption");
            lDeleted.Text = AttributeValue( "DeletedCaption" );
            lInvalid.Text = AttributeValue("InvalidCaption");

            string confirmationCode = Request.QueryString["cc"];
            if ( !string.IsNullOrEmpty( confirmationCode ) )
            {
                string encryptionPhrase = ConfigurationManager.AppSettings["EncryptionPhrase"];
                if ( String.IsNullOrWhiteSpace( encryptionPhrase ) )
                    encryptionPhrase = "Rock Rocks!";

                string identifier = Rock.Security.Encryption.DecryptString( confirmationCode, encryptionPhrase );

                if ( identifier.StartsWith( "ROCK|" ) )
                {
                    string[] idParts = identifier.Split( '|' );
                    if ( idParts.Length == 3)
                    {
                        string guid = idParts[1];
                        string username = idParts[2];

                        try
                        {
                            UserService service = new UserService();
                            user = service.GetByGuid( new Guid( guid ) );

                            if ( !Page.IsPostBack )
                            {
                                if ( user != null && user.Username == username )
                                {
                                    string action = Request.QueryString["action"] ?? "";
                                    switch ( action.ToLower() )
                                    {
                                        case "delete":

                                            string deleteCaption = AttributeValue( "DeleteCaption" );
                                            if ( deleteCaption.Contains( "{0}" ) )
                                                lDelete.Text = string.Format( deleteCaption, username );
                                            else
                                                lDelete.Text = deleteCaption;

                                            pnlDelete.Visible = true;

                                            break;

                                        default:

                                            user.IsApproved = true;
                                            service.Save( user, user.PersonId );
                                                
                                            pnlConfirmed.Visible = true;
                                                
                                            break;
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        protected override void Render( HtmlTextWriter writer )
        {
            pnlInvalid.Visible = !pnlConfirmed.Visible && !pnlDelete.Visible && !pnlDeleted.Visible;

            base.Render( writer );
        }

        #endregion

        #region Events

        protected void btnDelete_Click( object sender, EventArgs e )
        {
            UserService service = new UserService();
            service.Delete( user, user.PersonId );
            service.Save( user, user.PersonId );

            pnlDeleted.Visible = true;
        }

        #endregion

    }
}