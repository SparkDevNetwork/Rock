//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Constants;
using Rock.Web;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AccountDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                string itemId = PageParameter( "accountId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "accountId", int.Parse( itemId ) );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }
                
        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            FinancialAccount account;

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var accountService = new Rock.Model.FinancialAccountService();

                int accountId = Int32.Parse( hfAccountId.Value );

                if ( accountId == 0 )
                {
                    account = new Rock.Model.FinancialAccount();
                    accountService.Add( account, CurrentPersonId );
                }
                else
                {
                    account = accountService.Get( accountId );
                }

                account.Name = tbName.Text;
                account.IsActive = cbIsActive.Checked;
                account.Description = tbDescription.Text;

                account.ParentAccountId = apParentAccount.SelectedValueAsInt();
                account.AccountTypeValueId = ddlAccountType.SelectedValueAsInt();
                account.PublicName = tbPublicName.Text;

                account.GlCode = tbGLCode.Text;
                account.StartDate = dtpStartDate.SelectedDate;
                account.EndDate = dtpEndDate.SelectedDate;
                account.IsTaxDeductible = cbIsTaxDeductible.Checked;

                accountService.Save( account, CurrentPersonId );
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Internal Methods

        public void ShowDetail(string itemKey, int itemKeyValue)
        {
            pnlDetails.Visible = false;

            if (!itemKey.Equals("accountId"))
            { 
                return;
            }

            bool editAllowed = true;

            FinancialAccount account = null;

            if (!itemKeyValue.Equals(0))
            {
                account = new FinancialAccountService().Get( itemKeyValue );
                editAllowed = account.IsAuthorized( "Edit", CurrentPerson );
            }
            else
            {
                account = new FinancialAccount { Id = 0, IsActive = true };
            }

            if (account == null)
            {
                return;
            }

            pnlDetails.Visible = true;
            hfAccountId.Value = account.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if (!editAllowed || !IsUserAuthorized("Edit"))
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialAccount.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( account );
            }
            else
            {
                ShowEditDetails( account );
            }

        }

        private void ShowEditDetails( FinancialAccount account )
        {
            if ( account.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( FinancialAccount.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = account.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !account.IsActive;

            SetEditMode( true );

            using ( new UnitOfWorkScope() )
            {
                LoadDropDowns();
            }

            tbName.Text = account.Name;
            cbIsActive.Checked = account.IsActive;
            tbDescription.Text = account.Description;

            apParentAccount.SetValue( account.ParentAccount );
            ddlAccountType.SetValue( account.AccountTypeValueId );
            tbPublicName.Text = account.PublicName;

            tbGLCode.Text = account.GlCode;
            cbIsTaxDeductible.Checked = account.IsTaxDeductible;
            dtpStartDate.SelectedDate = account.StartDate;
            dtpEndDate.SelectedDate = account.EndDate;
        }


        private void ShowReadonlyDetails( FinancialAccount account )
        {
            SetEditMode( false );

            hfAccountId.SetValue( account.Id );
            lActionTitle.Text = account.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !account.IsActive;
            lAccountDescription.Text = account.Description;

            DescriptionList descriptionList = new DescriptionList();
            descriptionList.Add( "", "" );
            lblMainDetails.Text = descriptionList.Html;
        }

        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        private void LoadDropDowns()
        {
            ddlAccountType.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.FINANCIAL_ACCOUNT_TYPE.AsGuid() ) );
        }
        #endregion
    }
}