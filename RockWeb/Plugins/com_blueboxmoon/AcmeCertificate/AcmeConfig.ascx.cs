using System;
using System.ComponentModel;
using System.Web.UI;

using Rock.Model;
using Rock.Web.UI;

using com.blueboxmoon.AcmeCertificate;

namespace RockWeb.Plugins.com_blueboxmoon.AcmeCertificate
{
    [DisplayName( "Acme Config" )]
    [Category( "Blue Box Moon > Acme Certificate" )]
    [Description( "Configures the Acme certification system." )]
    public partial class AcmeConfig : RockBlock
    {
        /// <summary>
        /// Handles the Load event of the block.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }
        }

        /// <summary>
        /// Configure this block for display.
        /// </summary>
        protected void ShowDetails()
        {
            var account = AcmeHelper.LoadAccountData();

            ltAccountEmail.Text = account.Email;
            ltTestMode.Text = account.TestMode.ToString();
            ltOfflineMode.Text = account.OfflineMode.ToString();

            lbRegister.CssClass = string.IsNullOrWhiteSpace( account.Key ) ? "btn btn-primary" : "btn btn-default";
            lbEdit.Visible = !string.IsNullOrWhiteSpace( account.Key );
        }

        /// <summary>
        /// Handles the Click event of the lbRegister control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRegister_Click( object sender, EventArgs e )
        {
            HideSecondaryBlocks( true );

            pnlAccountDetail.Visible = false;
            pnlAccountRegister.Visible = true;

            var account = AcmeHelper.LoadAccountData();
            tbAccountEmail.Text = account.Email;
            cbTestMode.Checked = account.TestMode;

            nbExistingAccount.Visible = !string.IsNullOrWhiteSpace( account.Email );

            var acme = new AcmeService( false );
            hlTOS.NavigateUrl = hlTOS.Text = acme.TermsOfServiceUrl;
            divTOS.Attributes["class"] = "alert alert-info";
        }

        /// <summary>
        /// Handles the Click event of the lbAccountRegisterSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAccountRegisterSave_Click( object sender, EventArgs e )
        {
            var email = tbAccountEmail.Text;

            if ( !cbTOSAgree.Checked )
            {
                divTOS.Attributes["class"] = "alert alert-danger";
                return;
            }

            var acme = new AcmeService( cbTestMode.Checked );
            var account = acme.Register( email );
            account.TestMode = cbTestMode.Checked;
            account.OfflineMode = cbOfflineMode.Checked;
            account.Key = Convert.ToBase64String( acme.RSA );

            AcmeHelper.SaveAccountData( account );

            ltAccountEmail.Text = email;

            pnlAccountRegister.Visible = false;
            pnlAccountDetail.Visible = true;
            HideSecondaryBlocks( false );

            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the lbAccountRegisterCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAccountRegisterCancel_Click( object sender, EventArgs e )
        {
            pnlAccountRegister.Visible = false;
            pnlAccountDetail.Visible = true;
            HideSecondaryBlocks( false );
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            HideSecondaryBlocks( true );

            pnlAccountDetail.Visible = false;
            pnlAccountEdit.Visible = true;

            var account = AcmeHelper.LoadAccountData();
            cbOfflineMode.Checked = account.OfflineMode;
        }

        /// <summary>
        /// Handles the Click event of the lbAccountEditSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAccountEditSave_Click( object sender, EventArgs e )
        {
            var account = AcmeHelper.LoadAccountData();

            account.OfflineMode = cbOfflineMode.Checked;

            AcmeHelper.SaveAccountData( account );

            pnlAccountEdit.Visible = false;
            pnlAccountDetail.Visible = true;
            HideSecondaryBlocks( false );

            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the lbAccountEditCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAccountEditCancel_Click( object sender, EventArgs e )
        {
            pnlAccountEdit.Visible = false;
            pnlAccountDetail.Visible = true;
            HideSecondaryBlocks( false );
        }
    }
}
