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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Financial Statement Template Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the statement template." )]

    public partial class FinancialStatementTemplateDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string StatementTemplateId = "StatementTemplateId";
        }

        #endregion Page Parameter Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.StatementTemplateId ).AsInteger() );
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? statementTemplateId = PageParameter( pageReference, PageParameterKey.StatementTemplateId ).AsIntegerOrNull();
            if ( statementTemplateId != null )
            {
                var statementTemplate = new FinancialStatementTemplateService( new RockContext() ).Get( statementTemplateId.Value );

                if ( statementTemplate != null )
                {
                    breadCrumbs.Add( new BreadCrumb( statementTemplate.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Template", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            var financialStatementTemplateService = new FinancialStatementTemplateService( new RockContext() );
            var statementTemplate = financialStatementTemplateService.Get( hfStatementTemplateId.ValueAsInt() );
            ShowEditDetails( statementTemplate );
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            var rockContext = new RockContext();
            var financialStatementTemplateService = new FinancialStatementTemplateService( rockContext );
            var statementTemplateId = hfStatementTemplateId.Value.AsIntegerOrNull();
            FinancialStatementTemplate financialStatementTemplate = null;
            if ( statementTemplateId.HasValue )
            {
                financialStatementTemplate = financialStatementTemplateService.Get( statementTemplateId.Value );
            }

            var isNew = financialStatementTemplate == null;

            if ( isNew )
            {
                financialStatementTemplate = new FinancialStatementTemplate();
                financialStatementTemplateService.Add( financialStatementTemplate );
            }

            financialStatementTemplate.Name = tbName.Text;
            financialStatementTemplate.Description = tbDescription.Text;
            financialStatementTemplate.IsActive = cbIsActive.Checked;
            financialStatementTemplate.ReportTemplate = ceReportTemplate.Text;
            financialStatementTemplate.FooterTemplate = ceFooterTemplate.Text;

            financialStatementTemplate.ReportSetting.PDFObjectSettings = kvlPDFObjectSettings.Value.AsDictionary();
            var transactionSetting = new Rock.Finance.ReportSetting.TransactionSetting();
            transactionSetting.CurrencyTypesForCashGiftIds = dvpCurrencyTypesCashGifts.SelectedValuesAsInt;
            transactionSetting.CurrencyTypesForNonCashIds = dvpCurrencyTypesNonCashGifts.SelectedValuesAsInt;
            transactionSetting.TransactionTypeIds = dvpTransactionType.SelectedValuesAsInt;
            transactionSetting.HideRefundedTransaction = cbHideRefundedTransactions.Checked;
            transactionSetting.HideCorrectedTransactionOnSameData = cbHideModifiedTransactions.Checked;
            transactionSetting.AccountIds = apTransactionAccounts.SelectedValuesAsInt().ToList();
            financialStatementTemplate.ReportSetting.TransactionSetting = transactionSetting;

            var pledgeSetting = new Rock.Finance.ReportSetting.PledgeSetting();
            pledgeSetting.IncludeGiftsToChildAccounts = cbIncludeGiftsToChildAccounts.Checked;
            pledgeSetting.IncludeNonCashGifts = cbIncludeNonCashGifts.Checked;
            pledgeSetting.AccountIds = apPledgeAccounts.SelectedValuesAsInt().ToList();
            financialStatementTemplate.ReportSetting.PledgeSetting = pledgeSetting;

            int? existingLogoId = null;
            if ( financialStatementTemplate.LogoBinaryFileId != imgTemplateLogo.BinaryFileId )
            {
                existingLogoId = financialStatementTemplate.LogoBinaryFileId;
                financialStatementTemplate.LogoBinaryFileId = imgTemplateLogo.BinaryFileId;
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                if ( existingLogoId.HasValue )
                {
                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                    var binaryFile = binaryFileService.Get( existingLogoId.Value );
                    if ( binaryFile != null )
                    {
                        // marked the old images as IsTemporary so they will get cleaned up later
                        binaryFile.IsTemporary = true;
                        rockContext.SaveChanges();
                    }
                }
            } );

            var queryParams = new Dictionary<string, string>();
            queryParams.Add( PageParameterKey.StatementTemplateId, financialStatementTemplate.Id.ToStringSafe() );
            NavigateToCurrentPage( queryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            var statementTemplateId = hfStatementTemplateId.Value.AsIntegerOrNull();
            if ( statementTemplateId.HasValue && statementTemplateId > 0 )
            {
                var service = new FinancialStatementTemplateService( new RockContext() );
                var statementTemplate = service.Get( statementTemplateId.Value );
                ShowReadonlyDetails( statementTemplate );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="statementTemplateId">The statement template identifier.</param>
        public void ShowDetail( int statementTemplateId )
        {
            FinancialStatementTemplate financialStatementTemplate = null;

            bool editAllowed = UserCanEdit;

            if ( !statementTemplateId.Equals( 0 ) )
            {
                financialStatementTemplate = new FinancialStatementTemplateService( new RockContext() ).Get( statementTemplateId );
                editAllowed = editAllowed || financialStatementTemplate.IsAuthorized( Authorization.EDIT, CurrentPerson );
                pdAuditDetails.SetEntity( financialStatementTemplate, ResolveRockUrl( "~" ) );
            }

            if ( financialStatementTemplate == null )
            {
                financialStatementTemplate = new FinancialStatementTemplate();

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfStatementTemplateId.Value = financialStatementTemplate.Id.ToString();

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialStatementTemplate.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( financialStatementTemplate );
            }
            else
            {
                if ( financialStatementTemplate.Id > 0 )
                {
                    ShowReadonlyDetails( financialStatementTemplate );
                }
                else
                {
                    ShowEditDetails( financialStatementTemplate );
                }
            }
        }

        /// <summary>
        /// Shows the summary.
        /// </summary>
        /// <param name="statementTemplate">The statement template.</param>
        private void ShowReadonlyDetails( FinancialStatementTemplate financialStatementTemplate )
        {
            SetEditMode( false );

            if ( financialStatementTemplate != null )
            {
                hfStatementTemplateId.SetValue( financialStatementTemplate.Id );
                lTitle.Text = string.Format( "{0} Template", financialStatementTemplate.Name ).FormatAsHtmlTitle();
                hlInactive.Visible = !financialStatementTemplate.IsActive;
                lAccountDescription.Text = financialStatementTemplate.Description;

                var transactionSettings = financialStatementTemplate.ReportSetting.TransactionSetting;
                var detailsDescription = new DescriptionList();
                if ( transactionSettings.AccountIds.Any() )
                {
                    var accountList = new FinancialAccountService( new RockContext() )
                        .GetByIds( transactionSettings.AccountIds )
                        .Where( a => a.IsActive )
                        .ToList();
                    detailsDescription.Add( "Accounts for Transactions", accountList.Select( a => a.Name ).ToList().AsDelimited("<br/>") );
                }

                if ( transactionSettings.TransactionTypeIds.Any() )
                {
                    var transactionTypes = transactionSettings.TransactionTypeIds.Select( a => DefinedValueCache.GetName( a ) ).ToList();
                    detailsDescription.Add( "Transaction Types", transactionTypes.AsDelimited( "<br/>" ) );
                }

                lDetails.Text = detailsDescription.Html;
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="financialStatementTemplate">The financial statement template.</param>
        private void ShowEditDetails( FinancialStatementTemplate financialStatementTemplate )
        {
            if ( financialStatementTemplate.Id == 0 )
            {
                lTitle.Text = ActionTitle.Add( FinancialStatementTemplate.FriendlyTypeName ).FormatAsHtmlTitle();

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }
            else
            {
                lTitle.Text = financialStatementTemplate.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !financialStatementTemplate.IsActive;
            SetEditMode( true );
            LoadDropDowns();

            tbName.Text = financialStatementTemplate.Name;
            cbIsActive.Checked = financialStatementTemplate.IsActive;
            tbDescription.Text = financialStatementTemplate.Description;
            ceReportTemplate.Text = financialStatementTemplate.ReportTemplate;
            ceFooterTemplate.Text = financialStatementTemplate.FooterTemplate;
            imgTemplateLogo.BinaryFileId = financialStatementTemplate.LogoBinaryFileId;
            kvlPDFObjectSettings.Value = financialStatementTemplate.ReportSetting.PDFObjectSettings.Select( a => string.Format( "{0}^{1}", a.Key, a.Value ) ).ToList().AsDelimited( "|" );

            var transactionSetting = financialStatementTemplate.ReportSetting.TransactionSetting;
            cbHideRefundedTransactions.Checked = transactionSetting.HideRefundedTransaction;
            cbHideModifiedTransactions.Checked = transactionSetting.HideCorrectedTransactionOnSameData;
            dvpCurrencyTypesCashGifts.SetValues( transactionSetting.CurrencyTypesForCashGiftIds );
            dvpCurrencyTypesNonCashGifts.SetValues( transactionSetting.CurrencyTypesForNonCashIds );
            dvpTransactionType.SetValues( transactionSetting.TransactionTypeIds );
            if ( transactionSetting.AccountIds.Any() )
            {
                var accountList = new FinancialAccountService( new RockContext() )
                    .GetByIds( transactionSetting.AccountIds )
                    .Where( a => a.IsActive )
                    .ToList();
                apTransactionAccounts.SetValues( accountList );
            }

            var pledgeSetting = financialStatementTemplate.ReportSetting.PledgeSetting;
            cbIncludeGiftsToChildAccounts.Checked = pledgeSetting.IncludeGiftsToChildAccounts;
            cbIncludeNonCashGifts.Checked = pledgeSetting.IncludeNonCashGifts;
            if ( pledgeSetting.AccountIds.Any() )
            {
                var accountList = new FinancialAccountService( new RockContext() )
                    .GetByIds( pledgeSetting.AccountIds )
                    .Where( a => a.IsActive )
                    .ToList();
                apPledgeAccounts.SetValues( accountList );
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            dvpTransactionType.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE.AsGuid() ).Id;
            dvpCurrencyTypesCashGifts.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() ).Id;
            dvpCurrencyTypesNonCashGifts.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() ).Id;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewSummary.Visible = !editable;
            this.HideSecondaryBlocks( editable );
        }

        #endregion Internal Methods
    }
}