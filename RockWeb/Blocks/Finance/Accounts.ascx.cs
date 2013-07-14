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

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>
    [ContextAware( typeof( FinancialAccount ) )]
    public partial class Accounts : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rGridAccount.DataKeyNames = new string[] { "id" };
            rAccountFilter.ApplyFilterClick += rAccountFilter_ApplyFilterClick;
            rGridAccount.Actions.AddClick += rGridAccount_Add;
            rGridAccount.GridRebind += rGridAccounts_GridRebind;
            
            bool canAddEditDelete = IsUserAuthorized( "Edit" );
            rGridAccount.Actions.ShowAdd = canAddEditDelete;
            rGridAccount.IsDeleteEnabled = canAddEditDelete;
            btnSaveAccount.Enabled = canAddEditDelete;            
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }
                
        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rAccountFilter control.
        /// </summary>
        /// <param name="Sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rAccountFilter_ApplyFilterClick( object Sender, EventArgs e )
        {
            rAccountFilter.SaveUserPreference( "AccountName", txtAccountName.Text );
            rAccountFilter.SaveUserPreference( "StartFromDate", dtStartDate.SelectedDate.ToString() );
            rAccountFilter.SaveUserPreference( "EndToDate", dtEndDate.SelectedDate.ToString() );
            rAccountFilter.SaveUserPreference( "IsActive", ddlIsActive.SelectedValue );
            rAccountFilter.SaveUserPreference( "IsTaxDeductible", ddlIsTaxDeductible.SelectedValue );
            BindGrid();
        }
        
        /// <summary>
        /// Handles the Add event of the rGridAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridAccount_Add( object sender, EventArgs e )
        {
            BindAccountType();
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the rGridAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridAccount_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( (int)e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Delete event of the rGridAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGridAccount_Delete( object sender, RowEventArgs e )
        {
            RockTransactionScope.WrapTransaction( () =>
            {
                var accountService = new FinancialAccountService();
                var account = accountService.Get( (int)e.RowKeyValue );
                if ( account != null )
                {
                    string errorMessage;
                    if ( !accountService.CanDelete( account, out errorMessage ) )
                    {
                        mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    accountService.Delete( account, CurrentPersonId );
                    accountService.Save( account, CurrentPersonId );
                }
            } );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridAccount control.
        /// </summary>
        /// <param name="sendder">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridAccounts_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveAccount_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var accountService = new Rock.Model.FinancialAccountService();
                Rock.Model.FinancialAccount modifiedAccount;
                int accountId = ( hfAccountId.Value ) != null ? Int32.Parse( hfAccountId.Value ) : 0;

                if ( accountId == 0 )
                {
                    modifiedAccount = new Rock.Model.FinancialAccount();

                    accountService.Add( modifiedAccount, CurrentPersonId );
                }
                else
                {
                    modifiedAccount = accountService.Get( accountId );
                }

                modifiedAccount.Name =tbName.Text;
                modifiedAccount.PublicName = tbPublicName.Text;
                modifiedAccount.Description =tbDescription.Text;
                modifiedAccount.Order = Int32.Parse(tbOrder.Text);
                modifiedAccount.GlCode = tbGLCode.Text;
                modifiedAccount.IsActive = cbIsActive.Checked;
                modifiedAccount.IsTaxDeductible = cbIsTaxDeductible.Checked;
                modifiedAccount.StartDate = dtpStartDate.SelectedDate;
                modifiedAccount.EndDate = dtpEndDate.SelectedDate;
                modifiedAccount.ParentAccountId = apParentAccount.SelectedValueAsInt();
                modifiedAccount.AccountTypeValueId = ddlAccountType.SelectedValueAsInt();
                
                accountService.Save( modifiedAccount, CurrentPersonId );
            }

            //BindCategoryFilter();
            BindGrid();

            pnlAccountDetails.Visible = false;
            pnlAccountList.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnCancelAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlAccountDetails.Visible = false;
            pnlAccountList.Visible = true;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the Account list grid.
        /// </summary>
        private void BindGrid()
        {
            FinancialAccount contextAccount = ContextEntity<FinancialAccount>();
            if ( contextAccount == null )
            {
                var accountService = new FinancialAccountService();
                SortProperty sortProperty = rGridAccount.SortProperty;
                var accountQuery = accountService.Queryable();

                if ( !string.IsNullOrEmpty( txtAccountName.Text ) )
                {
                    accountQuery = accountQuery.Where( account => account.Name.Contains( txtAccountName.Text ) );
                }

                if ( dtStartDate.SelectedDate != null )
                {
                    accountQuery = accountQuery.Where( account => account.StartDate >= dtStartDate.SelectedDate );
                }

                if ( dtEndDate.SelectedDate != null )
                {
                    accountQuery = accountQuery.Where( account => account.EndDate <= dtEndDate.SelectedDate );
                }

                if ( ddlIsActive.SelectedValue != "Any" )
                {
                    accountQuery = accountQuery.Where( account => account.IsActive == ( ddlIsActive.SelectedValue == "Active" ) );
                }

                if ( ddlIsTaxDeductible.SelectedValue != "Any" )
                {
                    accountQuery = accountQuery.Where( account => account.IsTaxDeductible == ( ddlIsTaxDeductible.SelectedValue == "Yes" ) );
                }
                
                if ( sortProperty != null )
                {
                    rGridAccount.DataSource = accountQuery.Sort( sortProperty ).ToList();
                }
                else
                {
                    rGridAccount.DataSource = accountQuery.OrderBy( f => f.Name ).ToList();
                }
            }
            else
            {
                rGridAccount.DataSource = contextAccount;
            }

            rGridAccount.DataBind();
        }

        /// <summary>
        /// Binds the type of the account.
        /// </summary>
        private void BindAccountType()
        {
            var accountTypes = DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_ACCOUNT_TYPE ) );
            if ( accountTypes != null )
            {
                ddlAccountType.BindToDefinedType( accountTypes );
            }                
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="accountId">The account id.</param>
        protected void ShowEdit( int accountId )
        {
            var accountModel = new FinancialAccountService().Get( accountId );
            hfAccountId.Value = accountId.ToString();
            string actionTitle;

            if ( accountModel == null )
            {
                accountModel = new Rock.Model.FinancialAccount();
                actionTitle = ActionTitle.Add( Rock.Model.Attribute.FriendlyTypeName );
                tbName.Text = string.Empty;
                tbPublicName.Text = string.Empty;
                tbDescription.Text = string.Empty;
                tbOrder.Text = string.Empty;
                tbGLCode.Text = string.Empty;
                cbIsActive.Checked = false;
                cbIsTaxDeductible.Checked = false;
            }
            else
            {
                actionTitle = ActionTitle.Edit( Rock.Model.Attribute.FriendlyTypeName );
                tbName.Text = accountModel.Name ?? string.Empty;
                tbPublicName.Text = accountModel.PublicName ?? string.Empty;
                tbDescription.Text = accountModel.Description ?? string.Empty;
                tbOrder.Text = accountModel.Order.ToString();
                tbGLCode.Text = accountModel.GlCode ?? string.Empty;
                cbIsActive.Checked = accountModel.IsActive;
                cbIsTaxDeductible.Checked = accountModel.IsTaxDeductible;
                dtpStartDate.SelectedDate = accountModel.StartDate;
                dtpEndDate.SelectedDate = accountModel.EndDate;
                apParentAccount.SetValue( accountModel.ParentAccount );
            }

            pnlAccountList.Visible = false;
            pnlAccountDetails.Visible = true;
        }

        #endregion
    }
}