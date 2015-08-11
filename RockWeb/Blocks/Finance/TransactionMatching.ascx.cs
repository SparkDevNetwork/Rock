// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Transaction Matching" )]
    [Category( "Finance" )]
    [Description( "Used to match transactions to an individual and allocate the transaction amount to financial account(s)." )]

    [AccountsField( "Accounts", "Select the accounts that transaction amounts can be allocated to.  Leave blank to show all accounts" )]
    [LinkedPage( "Add Family Link", "Select the page where a new family can be added. If specified, a link will be shown which will open in a new window when clicked", DefaultValue = "6a11a13d-05ab-4982-a4c2-67a8b1950c74,af36e4c2-78c6-4737-a983-e7a78137ddc7" )]
    [LinkedPage( "Add Business Link", "Select the page where a new business can be added. If specified, a link will be shown which will open in a new window when clicked" )]
    public partial class TransactionMatching : RockBlock, IDetailBlock
    {

        #region Properties

        /// <summary>
        /// The _focus control
        /// </summary>
        private Control _focusControl = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
    $('.transaction-image-thumbnail').click( function() {
        var $primaryImg = $('.transaction-image');
        var primarySrc = $primaryImg.attr('src');
        $primaryImg.attr('src', $(this).attr('src'));
        $(this).attr('src', primarySrc);
    });
";
            ScriptManager.RegisterStartupScript( imgPrimary, imgPrimary.GetType(), "imgPrimarySwap", script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // initialize DoFadeIn to "0" so it only gets set to "1" when navigating thru transaction images
            hfDoFadeIn.Value = "0";

            if ( !Page.IsPostBack )
            {
                hfBackNextHistory.Value = string.Empty;
                LoadDropDowns();
                ShowDetail( PageParameter( "BatchId" ).AsInteger() );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            if ( _focusControl != null )
            {
                _focusControl.Focus();
            }

            //btnNext.AccessKey = new string(new char[] { (char)39 });

            base.OnPreRender( e );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        public void LoadDropDowns()
        {
            // get accounts that are both allowed by the BlockSettings and also in the personal AccountList setting
            var rockContext = new RockContext();
            var accountGuidList = GetAttributeValue( "Accounts" ).SplitDelimitedValues().Select( a => a.AsGuid() );

            string keyPrefix = string.Format( "transaction-matching-{0}-", this.BlockId );
            var personalAccountGuidList = ( this.GetUserPreference( keyPrefix + "account-list" ) ?? string.Empty ).SplitDelimitedValues().Select( a => a.AsGuid() ).ToList();

            var accountQry = new FinancialAccountService( rockContext ).Queryable();

            // no accounts specified means "all"
            if ( accountGuidList.Any() )
            {
                accountQry = accountQry.Where( a => accountGuidList.Contains( a.Guid ) );
            }

            // no personal accounts specified means "all(that are allowed in block settings)"
            if ( personalAccountGuidList.Any() )
            {
                accountQry = accountQry.Where( a => personalAccountGuidList.Contains( a.Guid ) );
            }

            rptAccounts.DataSource = accountQry.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            rptAccounts.DataBind();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="batchId">The financial batch identifier.</param>
        public void ShowDetail( int batchId )
        {
            string temp = this.GetAttributeValue( "AddFamilyLink" );
            string addFamilyUrl = this.LinkedPageUrl( "AddFamilyLink" );
            string addBusinessUrl = this.LinkedPageUrl( "AddBusinessLink" );

            rcwAddNewFamily.Visible = !string.IsNullOrWhiteSpace( addFamilyUrl );
            if ( rcwAddNewFamily.Visible )
            {
                // force the link to open a new scrollable,resizable browser window (and make it work in FF, Chrome and IE) http://stackoverflow.com/a/2315916/1755417
                hlAddNewFamily.Attributes["onclick"] = string.Format( "javascript: window.open('{0}', '_blank', 'scrollbars=1,resizable=1,toolbar=1'); return false;", addFamilyUrl );
            }

            rcwAddNewBusiness.Visible = !string.IsNullOrWhiteSpace( addBusinessUrl );
            if ( rcwAddNewBusiness.Visible )
            {
                hlAddNewBusiness.Attributes["onclick"] = string.Format( "javascript: window.open('{0}', '_blank', 'scrollbars=1,resizable=1,toolbar=1'); return false;", addBusinessUrl );
            }

            hfBatchId.Value = batchId.ToString();
            hfTransactionId.Value = string.Empty;

            NavigateToTransaction( Direction.Next );
        }

        /// <summary>
        /// 
        /// </summary>
        private enum Direction
        {
            Prev,
            Next
        }

        /// <summary>
        /// The transaction matching lock object
        /// </summary>
        private static object transactionMatchingLockObject = new object();

        /// <summary>
        /// Navigates to the next (or previous) transaction to edit
        /// </summary>
        private void NavigateToTransaction( Direction direction )
        {
            // put a lock around the entire NavigateToTransaction logic so that the navigation and "other person editing" logic will work consistently even if multiple people are editing the same batch
            lock ( transactionMatchingLockObject )
            {
                hfDoFadeIn.Value = "1";
                nbSaveError.Visible = false;
                int? fromTransactionId = hfTransactionId.Value.AsIntegerOrNull();
                int? toTransactionId = null;
                List<int> historyList = hfBackNextHistory.Value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsIntegerList().Where( a => a > 0 ).ToList();
                int position = hfHistoryPosition.Value.AsIntegerOrNull() ?? -1;

                if ( direction == Direction.Prev )
                {
                    position--;
                }
                else
                {
                    position++;
                }

                if ( historyList.Count > position )
                {
                    if ( position >= 0 )
                    {
                        toTransactionId = historyList[position];
                    }
                    else
                    {
                        // if we trying to go previous when we are already at the start of the list, wrap around to the last item in the list
                        toTransactionId = historyList.Last();
                        position = historyList.Count - 1;
                    }
                }

                hfHistoryPosition.Value = position.ToString();

                int batchId = hfBatchId.Value.AsInteger();
                var rockContext = new RockContext();
                var financialPersonBankAccountService = new FinancialPersonBankAccountService( rockContext );
                var financialTransactionService = new FinancialTransactionService( rockContext );
                var qryTransactionsToMatch = financialTransactionService.Queryable()
                    .Where( a => a.AuthorizedPersonAliasId == null && a.ProcessedByPersonAliasId == null );

                if ( batchId != 0 )
                {
                    qryTransactionsToMatch = qryTransactionsToMatch.Where( a => a.BatchId == batchId );
                }

                // if a specific transactionId was specified (because we are navigating thru history), load that one. Otherwise, if a batch is specified, get the first unmatched transaction in that batch
                if ( toTransactionId.HasValue )
                {
                    qryTransactionsToMatch = financialTransactionService
                        .Queryable( "AuthorizedPersonAlias.Person,ProcessedByPersonAlias.Person" )
                        .Where( a => a.Id == toTransactionId );
                }

                if ( historyList.Any() && !toTransactionId.HasValue )
                {
                    // since we are looking for a transaction we haven't viewed or matched yet, look for the next one in the database that we haven't seen yet
                    qryTransactionsToMatch = qryTransactionsToMatch.Where( a => !historyList.Contains( a.Id ) );
                }

                qryTransactionsToMatch = qryTransactionsToMatch.OrderBy( a => a.CreatedDateTime ).ThenBy( a => a.Id );

                FinancialTransaction transactionToMatch = qryTransactionsToMatch.FirstOrDefault();
                if ( transactionToMatch == null )
                {
                    // we exhausted the transactions that aren't processed and aren't in our history list, so remove those those restrictions and show all transactions that haven't been matched yet
                    var qryRemainingTransactionsToMatch = financialTransactionService
                        .Queryable( "AuthorizedPersonAlias.Person,ProcessedByPersonAlias.Person" )
                        .Where( a => a.AuthorizedPersonAliasId == null );

                    if ( batchId != 0 )
                    {
                        qryRemainingTransactionsToMatch = qryRemainingTransactionsToMatch.Where( a => a.BatchId == batchId );
                    }

                    // get the first transaction that we haven't visited yet, or the next one we have visited after one we are on, or simple the first unmatched one
                    transactionToMatch = qryRemainingTransactionsToMatch.Where( a => a.Id > fromTransactionId && !historyList.Contains( a.Id ) ).FirstOrDefault()
                        ?? qryRemainingTransactionsToMatch.Where( a => a.Id > fromTransactionId ).FirstOrDefault()
                        ?? qryRemainingTransactionsToMatch.FirstOrDefault();
                    if ( transactionToMatch != null )
                    {
                        historyList.Add( transactionToMatch.Id );
                        position = historyList.LastIndexOf( transactionToMatch.Id );
                        hfHistoryPosition.Value = position.ToString();
                    }
                }
                else
                {
                    if ( !toTransactionId.HasValue )
                    {
                        historyList.Add( transactionToMatch.Id );
                    }
                }

                nbNoUnmatchedTransactionsRemaining.Visible = transactionToMatch == null;
                pnlEdit.Visible = transactionToMatch != null;
                nbIsInProcess.Visible = false;
                if ( transactionToMatch != null )
                {
                    if ( transactionToMatch.ProcessedByPersonAlias != null )
                    {
                        if ( transactionToMatch.AuthorizedPersonAliasId.HasValue )
                        {
                            nbIsInProcess.Text = string.Format( "Warning. This transaction was matched by {0} at {1} ({2})", transactionToMatch.ProcessedByPersonAlias.Person, transactionToMatch.ProcessedDateTime.ToString(), transactionToMatch.ProcessedDateTime.ToRelativeDateString() );
                            nbIsInProcess.Visible = true;
                        }
                        else
                        {
                            // display a warning if some other user has this marked as InProcess (and it isn't matched)
                            if ( transactionToMatch.ProcessedByPersonAliasId != CurrentPersonAliasId )
                            {
                                nbIsInProcess.Text = string.Format( "Warning. This transaction is getting processed by {0} as of {1} ({2})", transactionToMatch.ProcessedByPersonAlias.Person, transactionToMatch.ProcessedDateTime.ToString(), transactionToMatch.ProcessedDateTime.ToRelativeDateString() );
                                nbIsInProcess.Visible = true;
                            }
                        }
                    }

                    // Unless somebody else is processing it, immediately mark the transaction as getting processed by the current person so that other potentional transaction matching sessions will know that it is currently getting looked at
                    if ( !transactionToMatch.ProcessedByPersonAliasId.HasValue )
                    {
                        transactionToMatch.ProcessedByPersonAlias = null;
                        transactionToMatch.ProcessedByPersonAliasId = CurrentPersonAliasId;
                        transactionToMatch.ProcessedDateTime = RockDateTime.Now;
                        rockContext.SaveChanges();
                    }

                    hfTransactionId.Value = transactionToMatch.Id.ToString();

                    // get the first 2 images (should be no more than 2, but just in case)
                    var transactionImages = transactionToMatch.Images.OrderBy( a => a.Order ).Take( 2 ).ToList();

                    ddlIndividual.Items.Clear();
                    ddlIndividual.Items.Add( new ListItem( null, null ) );
                    // clear any previously shown badges
                    ddlIndividual.Attributes.Remove( "disabled" );
                    badgeIndividualCount.InnerText = "";

                    // if this transaction has a CheckMicrEncrypted, try to find matching person(s)
                    string checkMicrHashed = null;

                    if ( !string.IsNullOrWhiteSpace( transactionToMatch.CheckMicrEncrypted ) )
                    {
                        try
                        {
                            var checkMicrClearText = Encryption.DecryptString( transactionToMatch.CheckMicrEncrypted );
                            var parts = checkMicrClearText.Split( '_' );
                            if ( parts.Length >= 2 )
                            {
                                checkMicrHashed = FinancialPersonBankAccount.EncodeAccountNumber( parts[0], parts[1] );
                            }
                        }
                        catch
                        {
                            // intentionally ignore exception when decripting CheckMicrEncrypted since we'll be checking for null below
                        }
                    }

                    hfCheckMicrHashed.Value = checkMicrHashed;

                    if ( !string.IsNullOrWhiteSpace( checkMicrHashed ) )
                    {
                        var matchedPersons = financialPersonBankAccountService.Queryable().Where( a => a.AccountNumberSecured == checkMicrHashed ).Select( a => a.PersonAlias.Person ).Distinct();
                        foreach ( var person in matchedPersons.OrderBy( a => a.LastName ).ThenBy( a => a.NickName ) )
                        {
                            ddlIndividual.Items.Add( new ListItem( person.FullNameReversed, person.Id.ToString() ) );
                        }
                    }

                    bool requiresMicr = 
                        transactionToMatch.FinancialPaymentDetail != null &&
                        transactionToMatch.FinancialPaymentDetail.CurrencyTypeValue != null &&
                        transactionToMatch.FinancialPaymentDetail.CurrencyTypeValue.Guid == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();

                    nbNoMicrWarning.Visible = requiresMicr && string.IsNullOrWhiteSpace( checkMicrHashed );

                    if ( ddlIndividual.Items.Count == 2 )
                    {
                        // only one person (and the None selection) are in the list, so init to the person
                        ddlIndividual.SelectedIndex = 1;
                    }
                    else
                    {
                        // either zero or multiple people are in the list, so default to none so they are forced to choose
                        ddlIndividual.SelectedIndex = 0;
                    }

                    if ( transactionToMatch.AuthorizedPersonAlias != null && transactionToMatch.AuthorizedPersonAlias.Person != null )
                    {
                        var person = transactionToMatch.AuthorizedPersonAlias.Person;

                        // if the drop down does not contains the AuthorizedPerson of this transaction, add them to the drop down
                        // note, this can easily happen for non-check transactions
                        if ( !ddlIndividual.Items.OfType<ListItem>().Any( a => a.Value == person.Id.ToString() ) )
                        {
                            ddlIndividual.Items.Add( new ListItem( person.FullNameReversed, person.Id.ToString() ) );
                        }

                        ddlIndividual.SelectedValue = person.Id.ToString();
                    }

                    if ( ddlIndividual.Items.Count != 1 )
                    {
                        badgeIndividualCount.InnerText = ( ddlIndividual.Items.Count - 1 ).ToStringSafe();
                    }
                    else
                    {
                        ddlIndividual.Attributes["disabled"] = "disabled";
                        _focusControl = ppSelectNew;
                    }

                    ddlIndividual_SelectedIndexChanged( null, null );

                    ppSelectNew.SetValue( null );
                    if ( transactionToMatch.TransactionDetails.Any() )
                    {
                        cbTotalAmount.Text = transactionToMatch.TotalAmount.ToString();
                    }
                    else
                    {
                        cbTotalAmount.Text = string.Empty;
                    }

                    // update accountboxes
                    foreach ( var accountBox in rptAccounts.ControlsOfTypeRecursive<CurrencyBox>() )
                    {
                        accountBox.Text = string.Empty;
                    }

                    foreach ( var detail in transactionToMatch.TransactionDetails )
                    {
                        var accountBox = rptAccounts.ControlsOfTypeRecursive<CurrencyBox>().Where( a => a.Attributes["data-account-id"].AsInteger() == detail.AccountId ).FirstOrDefault();
                        if ( accountBox != null )
                        {
                            accountBox.Text = detail.Amount.ToString();
                        }
                    }

                    if ( transactionToMatch.Images.Any() )
                    {
                        var primaryImage = transactionToMatch.Images
                            .OrderBy( i => i.Order )
                            .FirstOrDefault();
                        imgPrimary.ImageUrl = string.Format( "~/GetImage.ashx?id={0}", primaryImage.BinaryFileId );
                        imgPrimary.Visible = true;
                        nbNoTransactionImageWarning.Visible = false;

                        rptrImages.DataSource = transactionToMatch.Images
                            .Where( i => !i.Id.Equals( primaryImage.Id ) )
                            .OrderBy( i => i.Order )
                            .ToList();
                        rptrImages.DataBind();
                    }
                    else
                    {
                        imgPrimary.Visible = false;
                        rptrImages.DataSource = null;
                        rptrImages.DataBind();
                        nbNoTransactionImageWarning.Visible = true;
                    }
                }
                else
                {
                    hfTransactionId.Value = string.Empty;
                }

                // display how many unmatched transactions are remaining
                var qryTransactionCount = financialTransactionService.Queryable();
                if ( batchId != 0 )
                {
                    qryTransactionCount = qryTransactionCount.Where( a => a.BatchId == batchId );
                }

                // get count of transactions that haven't been matched (not including the one we are currently editing)
                int currentTranId = hfTransactionId.Value.AsInteger();
                int matchedRemainingCount = qryTransactionCount.Count( a => a.AuthorizedPersonAliasId != null && a.Id != currentTranId );
                int totalBatchItemCount = qryTransactionCount.Count();

                int percentComplete = (int)Math.Round( (double)(100 * matchedRemainingCount) / totalBatchItemCount );

                lProgressBar.Text = String.Format( @"<div class='progress'>
                    <div class='progress-bar progress-bar-info' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' style='width: {0}%;'>
                        {0}%
                    </div>
                </div>", percentComplete);

                hfBackNextHistory.Value = historyList.AsDelimited( "," );

                if ( _focusControl == null )
                {
                    _focusControl = rptAccounts.ControlsOfTypeRecursive<Rock.Web.UI.Controls.CurrencyBox>().FirstOrDefault();
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SaveClick event of the mdAccountsPersonalFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAccountsPersonalFilter_SaveClick( object sender, EventArgs e )
        {
            var selectedAccountIdList = apPersonalAccounts.SelectedValuesAsInt().ToList();
            var selectedAccountGuidList = new FinancialAccountService( new RockContext() ).GetByIds( selectedAccountIdList ).Select( a => a.Guid ).ToList();

            string keyPrefix = string.Format( "transaction-matching-{0}-", this.BlockId );
            this.SetUserPreference( keyPrefix + "account-list", selectedAccountGuidList.AsDelimited( "," ) );

            mdAccountsPersonalFilter.Hide();
            LoadDropDowns();
        }

        /// <summary>
        /// Handles the Click event of the btnFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            string keyPrefix = string.Format( "transaction-matching-{0}-", this.BlockId );
            var personalAccountGuidList = ( this.GetUserPreference( keyPrefix + "account-list" ) ?? string.Empty ).SplitDelimitedValues().Select( a => a.AsGuid() ).ToList();
            var personalAccountList = new FinancialAccountService( new RockContext() ).GetByGuids( personalAccountGuidList ).ToList();

            apPersonalAccounts.SetValues( personalAccountList );

            mdAccountsPersonalFilter.Show();
        }

        /// <summary>
        /// Marks the transaction as not processed by the current user
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        private void MarkTransactionAsNotProcessedByCurrentUser( int transactionId )
        {
            var rockContext = new RockContext();
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var financialTransaction = financialTransactionService.Get( transactionId );

            if ( financialTransaction != null &&
                financialTransaction.ProcessedByPersonAliasId == CurrentPersonAliasId &&
                financialTransaction.AuthorizedPersonAliasId == null )
            {
                // if the current user marked this as processed, and it wasn't matched, clear out the processedby fields.  Otherwise, assume the other person is still editing it
                financialTransaction.ProcessedByPersonAliasId = null;
                financialTransaction.ProcessedDateTime = null;
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPrevious_Click( object sender, EventArgs e )
        {
            // if the transaction was not matched, clear out the ProcessedBy fields since we didn't match the transaction and are moving on to process another transaction
            MarkTransactionAsNotProcessedByCurrentUser( hfTransactionId.Value.AsInteger() );

            NavigateToTransaction( Direction.Prev );
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            var changes = new List<string>();

            var rockContext = new RockContext();
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );
            var financialPersonBankAccountService = new FinancialPersonBankAccountService( rockContext );
            int txnId = hfTransactionId.Value.AsInteger();
            var financialTransaction = financialTransactionService
                    .Queryable( "AuthorizedPersonAlias.Person,ProcessedByPersonAlias.Person" )
                    .FirstOrDefault( t => t.Id == txnId );

            // set the AuthorizedPersonId (the person who wrote the check, for example) to the if the SelectNew person (if selected) or person selected in the drop down (if there is somebody selected)
            int? authorizedPersonId = ppSelectNew.PersonId ?? ddlIndividual.SelectedValue.AsIntegerOrNull();

            var accountNumberSecured = hfCheckMicrHashed.Value;

            if ( cbTotalAmount.Text.AsDecimalOrNull().HasValue && !authorizedPersonId.HasValue )
            {
                nbSaveError.Text = "Transaction must be matched to a person when the amount is specified.";
                nbSaveError.Visible = true;
                return;
            }

            // if the transaction was previously matched, but user unmatched it, save it as an unmatched transaction and clear out the detail records (we don't want an unmatched transaction to have detail records)
            if ( financialTransaction != null &&
                financialTransaction.AuthorizedPersonAliasId.HasValue &&
                !authorizedPersonId.HasValue )
            {
                financialTransaction.AuthorizedPersonAliasId = null;
                foreach ( var detail in financialTransaction.TransactionDetails )
                {
                    History.EvaluateChange( changes, detail.Account != null ? detail.Account.Name : "Unknown", detail.Amount.ToString( "C2" ), string.Empty );
                    financialTransactionDetailService.Delete( detail );
                }

                changes.Add( "Unmatched transaction" );

                HistoryService.SaveChanges(
                    rockContext,
                    typeof( FinancialBatch ),
                    Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                    financialTransaction.BatchId.Value,
                    changes,
                    string.Format( "Transaction Id: {0}", financialTransaction.Id ),
                    typeof( FinancialTransaction ),
                    financialTransaction.Id,
                    false
                );

                rockContext.SaveChanges();

                // if the transaction was unmatched, clear out the ProcessedBy fields since we didn't match the transaction and are moving on to process another transaction
                MarkTransactionAsNotProcessedByCurrentUser( hfTransactionId.Value.AsInteger() );
            }

            // if the transaction is matched to somebody, attempt to save it.  Otherwise, if the transaction was previously matched, but user unmatched it, save it as an unmatched transaction
            if ( financialTransaction != null && authorizedPersonId.HasValue )
            {
                bool requiresMicr = 
                    financialTransaction.FinancialPaymentDetail != null &&
                    financialTransaction.FinancialPaymentDetail.CurrencyTypeValue != null &&
                    financialTransaction.FinancialPaymentDetail.CurrencyTypeValue.Guid == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
                if ( requiresMicr && string.IsNullOrWhiteSpace( accountNumberSecured ) )
                {
                    // should be showing already, but just in case
                    nbNoMicrWarning.Visible = true;
                    return;
                }

                if ( cbTotalAmount.Text.AsDecimalOrNull() == null )
                {
                    nbSaveError.Text = "Total amount must be allocated to accounts.";
                    nbSaveError.Visible = true;
                    return;
                }

                var personAlias = new PersonAliasService( rockContext ).GetPrimaryAlias( authorizedPersonId.Value );
                int? personAliasId = personAlias != null ? personAlias.Id : (int?)null;

                // if this transaction has an accountnumber associated with it (in other words, it's a scanned check), ensure there is a financialPersonBankAccount record
                if ( !string.IsNullOrWhiteSpace( accountNumberSecured ) )
                {
                    var financialPersonBankAccount = financialPersonBankAccountService.Queryable().Where( a => a.AccountNumberSecured == accountNumberSecured && a.PersonAlias.PersonId == authorizedPersonId.Value ).FirstOrDefault();
                    if ( financialPersonBankAccount == null )
                    {
                        if ( personAliasId.HasValue )
                        {
                            financialPersonBankAccount = new FinancialPersonBankAccount();
                            financialPersonBankAccount.PersonAliasId = personAliasId.Value;
                            financialPersonBankAccount.AccountNumberSecured = accountNumberSecured;

                            var checkMicrClearText = Encryption.DecryptString( financialTransaction.CheckMicrEncrypted );
                            var parts = checkMicrClearText.Split( '_' );
                            if ( parts.Length >= 2 )
                            {
                                financialPersonBankAccount.AccountNumberMasked = parts[1].Masked();
                            }

                            financialPersonBankAccountService.Add( financialPersonBankAccount );
                        }
                    }
                }

                string prevPerson = ( financialTransaction.AuthorizedPersonAlias != null && financialTransaction.AuthorizedPersonAlias.Person != null ) ?
                    financialTransaction.AuthorizedPersonAlias.Person.FullName : string.Empty;
                string newPerson = string.Empty;
                if ( personAliasId.HasValue )
                {
                    newPerson = personAlias.Person.FullName;
                    financialTransaction.AuthorizedPersonAliasId = personAliasId;
                }

                History.EvaluateChange( changes, "Person", prevPerson, newPerson );

                // just in case this transaction is getting re-edited either by the same user, or somebody else, clean out any existing TransactionDetail records
                foreach ( var detail in financialTransaction.TransactionDetails.ToList() )
                {
                    financialTransactionDetailService.Delete( detail );
                    History.EvaluateChange( changes, detail.Account != null ? detail.Account.Name : "Unknown", detail.Amount.ToString( "C2" ), string.Empty );
                }

                foreach ( var accountBox in rptAccounts.ControlsOfTypeRecursive<CurrencyBox>() )
                {
                    var amount = accountBox.Text.AsDecimalOrNull();

                    if ( amount.HasValue && amount.Value >= 0 )
                    {
                        var financialTransactionDetail = new FinancialTransactionDetail();
                        financialTransactionDetail.TransactionId = financialTransaction.Id;
                        financialTransactionDetail.AccountId = accountBox.Attributes["data-account-id"].AsInteger();
                        financialTransactionDetail.Amount = amount.Value;
                        financialTransactionDetailService.Add( financialTransactionDetail );

                        History.EvaluateChange( changes, accountBox.Label, 0.0M.ToString( "C2" ), amount.Value.ToString( "C2" ) );

                    }
                }

                financialTransaction.ProcessedByPersonAliasId = this.CurrentPersonAlias.Id;
                financialTransaction.ProcessedDateTime = RockDateTime.Now;

                changes.Add( "Matched transaction" );

                HistoryService.SaveChanges(
                    rockContext,
                    typeof( FinancialBatch ),
                    Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                    financialTransaction.BatchId.Value,
                    changes,
                    personAlias != null && personAlias.Person != null ? personAlias.Person.FullName : string.Format( "Transaction Id: {0}", financialTransaction.Id ),
                    typeof( FinancialTransaction ),
                    financialTransaction.Id,
                    false
                ); 
                
                rockContext.SaveChanges();
            }
            else
            {
                // if the transaction was not matched, clear out the ProcessedBy fields since we didn't match the transaction and are moving on to process another transaction
                MarkTransactionAsNotProcessedByCurrentUser( hfTransactionId.Value.AsInteger() );
            }

            NavigateToTransaction( Direction.Next );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlIndividual control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlIndividual_SelectedIndexChanged( object sender, EventArgs e )
        {
            var personId = ddlIndividual.SelectedValue.AsIntegerOrNull();

            LoadPersonPreview( personId );

            if (personId.HasValue)
            {
                // if a person was selected using the PersonDropDown, set the PersonPicker to unselected
                ppSelectNew.SetValue( null );
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppSelectNew control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppSelectNew_SelectPerson( object sender, EventArgs e )
        {
            if ( ppSelectNew.PersonId.HasValue )
            {
                // if a person was selected using the PersonPicker, set the PersonDropDown to unselected
                ddlIndividual.SetValue( string.Empty );
                LoadPersonPreview( ppSelectNew.PersonId.Value );
                _focusControl = rptAccounts.ControlsOfTypeRecursive<Rock.Web.UI.Controls.CurrencyBox>().FirstOrDefault();

                nbSaveError.Text = "";
                nbSaveError.Visible = false;
            }
        }

        /// <summary>
        /// Loads the person preview.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        private void LoadPersonPreview( int? personId )
        {
            string previewHtmlDetails = string.Empty;
            var rockContext = new RockContext();
            var person = new PersonService( rockContext ).Get( personId ?? 0 );
            pnlPreview.Visible = person != null;
            if ( person != null )
            {
                lPersonName.Text = person.FullName;
                var spouse = person.GetSpouse( rockContext );
                lSpouseName.Text = spouse != null ? string.Format( "<strong>Spouse: </strong>{0}", spouse.FullName ) : string.Empty;
                rptrAddresses.DataSource = person.GetFamilies().SelectMany( a => a.GroupLocations ).ToList();
                rptrAddresses.DataBind();
            }
        }

        /// <summary>
        /// Images the URL.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        protected string ImageUrl( int binaryFileId, int? maxWidth = null, int? maxHeight = null )
        {
            string width = maxWidth.HasValue ? string.Format( "&maxWidth={0}", maxWidth.Value ) : string.Empty;
            string height = maxHeight.HasValue ? string.Format( "&maxHeight={0}", maxHeight.Value ) : string.Empty;
            return ResolveRockUrl( string.Format( "~/GetImage.ashx?id={0}{1}{2}", binaryFileId, width, height ) );
        }

        #endregion
        
}
}