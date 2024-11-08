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
using Rock.Utility;
using Rock.Web.Cache;
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

    [AccountsField(
        "Accounts",
        Key = AttributeKey.Accounts,
        Description = "Select the accounts that transaction amounts can be allocated to.  Leave blank to show all accounts",
        IsRequired = false,
        Order = 0 )]


    [LinkedPage(
        "Add Family Link",
        Key = AttributeKey.AddFamilyLink,
        Description = "Select the page to be opened in a separate browser window where a new family can be added. If not specified, you can create a new family within this block.",
        IsRequired = false,
        Order = 1 )]

    [LinkedPage(
        "Add Business Link",
        Key = AttributeKey.AddBusinessLink,
        Description = "Select the page to be opened in a separate browser window where a new business can be added. If not specified, you can create a new business within this block.",
        IsRequired = false,
        Order = 2 )]

    [LinkedPage(
        "Batch Detail Page",
        Key = AttributeKey.BatchDetailPage,
        Description = "Select the page for displaying batch details",
        IsRequired = false,
        Order = 3 )]

    [LinkedPage(
        "Transaction Detail Page",
        Key = AttributeKey.TransactionDetailPage,
        Description = "Select the page to return to, if this block was being used to edit a single transaction.",
        IsRequired = false,
        Order = 4 )]

    [BooleanField(
        "Expand Person Search Options",
        Key = AttributeKey.ExpandPersonSearchOptions,
        Description = "When selecting a person, expand the additional search options by default.",
        DefaultBooleanValue = true,
        Order = 5 )]

    [BooleanField(
        "Prompt to Edit Payment Detail Attributes",
        Key = AttributeKey.DisplayPaymentDetailAttributeControls,
        Description = "If Transaction Payment Detail has attributes configured, this will prompt to edit the values for those.",
        DefaultBooleanValue = false,
        Order = 6 )]

    [DefinedValueField(
        "Default Person Connection Status",
        Key = AttributeKey.DefaultPersonConnectionStatus,
        IsRequired = false,
        Category = AttributeCategory.AddPersonOrBusiness,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Description = "The connection status to default the connection status dropdown to.",
        Order = 7 )]

    [DefinedValueField(
        "Person Record Status",
        Key = AttributeKey.PersonRecordStatus,
        Category = AttributeCategory.AddPersonOrBusiness,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE,
        IsRequired = false,
        Description = "The default status to use when adding a person. This is not shown as a selection.",
        Order = 8 )]

    [BooleanField(
        "Show Family Role",
        Key = AttributeKey.ShowFamilyRole,
        Category = AttributeCategory.AddPersonOrBusiness,
        DefaultBooleanValue = true,
        Description = "Determines if the Adult/Child toggle should be shown. If hidden, the role will be set to adult. The default will always be adult.",
        Order = 9 )]

    [BooleanField(
        "Show Email",
        Key = AttributeKey.ShowEmail,
        Category = AttributeCategory.AddPersonOrBusiness,
        DefaultBooleanValue = false,
        Description = "Determines if the email address field should be shown.",
        Order = 9 )]

    [Rock.SystemGuid.BlockTypeGuid( "1A8BEE2A-E5BE-4BA5-AFDB-E9C9278419BA" )]
    public partial class TransactionMatching : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Categories for attributes
        /// </summary>
        private static class AttributeCategory
        {
            /// <summary>
            /// The add person or business section
            /// </summary>
            public const string AddPersonOrBusiness = "Creating a new Person or Business";
        }

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The accounts
            /// </summary>
            public const string Accounts = "Accounts";

            /// <summary>
            /// The add family link
            /// </summary>
            public const string AddFamilyLink = "AddFamilyLink";

            /// <summary>
            /// The add business link
            /// </summary>
            public const string AddBusinessLink = "AddBusinessLink";

            /// <summary>
            /// The batch detail page
            /// </summary>
            public const string BatchDetailPage = "BatchDetailPage";

            /// <summary>
            /// The transaction detail page
            /// </summary>
            public const string TransactionDetailPage = "TransactionDetailPage";

            /// <summary>
            /// The expand person search options
            /// </summary>
            public const string ExpandPersonSearchOptions = "ExpandPersonSearchOptions";

            /// <summary>
            /// The display payment detail attribute controls
            /// </summary>
            public const string DisplayPaymentDetailAttributeControls = "DisplayPaymentDetailAttributeControls";

            /// <summary>
            /// The default person connection status
            /// </summary>
            public const string DefaultPersonConnectionStatus = "DefaultPersonConnectionStatus";

            /// <summary>
            /// The person record status
            /// </summary>
            public const string PersonRecordStatus = "PersonRecordStatus";

            /// <summary>
            /// The person record status
            /// </summary>
            public const string ShowFamilyRole = "ShowFamilyRole";

            /// <summary>
            /// The show email
            /// </summary>
            public const string ShowEmail = "ShowEmail";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        public static class PageParameterKey
        {
            public const string BatchId = "BatchId";
            public const string TransactionId = "TransactionId";
        }

        #endregion PageParameterKeys

        #region Properties

        /// <summary>
        /// The _focus control
        /// </summary>
        private Control _focusControl = null;
        private List<int> _visibleDisplayedAccountIds
        {
            get
            {
                return this.ViewState["_visibleDisplayedAccountIds"] as List<int>;
            }

            set
            {
                this.ViewState["_visibleDisplayedAccountIds"] = value;
            }
        }

        private List<int> _visibleOptionalAccountIds
        {
            get
            {
                return this.ViewState["_visibleOptionalAccountIds"] as List<int>;
            }

            set
            {
                this.ViewState["_visibleOptionalAccountIds"] = value;
            }
        }

        private List<int> _allOptionalAccountIds
        {
            get
            {
                return this.ViewState["_allOptionalAccountIds"] as List<int>;
            }

            set
            {
                this.ViewState["_allOptionalAccountIds"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddCSSLink( "~/Styles/Blocks/Finance/TransactionMatching.css" );
            RockPage.AddScriptLink( "~/Scripts/imagesloaded.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );

            string script = string.Format( @"
    $('.transaction-image-thumbnail').on('click', function() {{
        var $primaryHyperlink = $('.transaction-image a');
        var $primaryImg = $('.transaction-image a img');
        var primarySrc = $primaryHyperlink.attr('href');
        $primaryHyperlink.attr('href', $(this).attr('src'));
        $primaryImg.attr('src', $(this).attr('src'));
        $(this).attr('src', primarySrc);
    }});

    $('.js-add-account').on('change', function (evt, params) {{
        console.log(evt.type);
        console.log(params);
        $('#{0}').attr('disabled', 'disabled');
        window.location = ""javascript: __doPostBack('{1}', '')"";
    }});
", btnNext.ClientID, ddlAddAccount.ClientID );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            ScriptManager.RegisterStartupScript( lImage, lImage.GetType(), "imgPrimarySwap", script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            // initialize DoFadeIn to "0" so it only gets set to "1" when navigating thru transaction images
            hfDoFadeIn.Value = "0";

            if ( !Page.IsPostBack )
            {
                hfBackNextHistory.Value = string.Empty;
                LoadDropDowns();
                RenderState();
            }
            else
            {
                if ( this.Request.Params["__EVENTTARGET"] == ddlAddAccount.ClientID )
                {
                    ddlAddAccount_SelectionChanged( ddlAddAccount, new EventArgs() );
                }
            }

            // Display Payment Detail Attributes
            if ( this.GetAttributeValue( AttributeKey.DisplayPaymentDetailAttributeControls ).AsBoolean() )
            {
                int? transactionId = hfTransactionId.Value.AsIntegerOrNull();
                if ( transactionId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var financialTransactionService = new FinancialTransactionService( rockContext );
                        var txn = financialTransactionService.Queryable().Where( t => t.Id == transactionId ).SingleOrDefault();

                        DisplayPaymentDetailAttributeControls( txn );
                    }
                }
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// If there are any Payment Detail attributes, display their edit controls and populate any existing values
        /// </summary>
        /// <param name="txn">The <see cref="FinancialTransaction"/> to load attributes from </param>

        protected void DisplayPaymentDetailAttributeControls( FinancialTransaction txn )
        {
            txn.FinancialPaymentDetail.LoadAttributes();
            phPaymentAttributeEdits.Controls.Clear();
            Helper.AddEditControls( txn.FinancialPaymentDetail, phPaymentAttributeEdits, true, BlockValidationGroup );
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

            //// btnNext.AccessKey = new string(new char[] { (char)39 });

            base.OnPreRender( e );
        }

        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadDropDowns();
            RenderState();
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
            var blockAccountGuidList = GetAttributeValue( AttributeKey.Accounts ).SplitDelimitedValues().Select( a => a.AsGuid() ).ToList();

            var preferences = GetBlockPersonPreferences();
            var personalAccountGuidList = preferences.GetValue( "account-list" ).SplitDelimitedValues().Select( a => a.AsGuid() ).ToList();
            var optionalAccountGuidList = preferences.GetValue( "optional-account-list" ).SplitDelimitedValues().Select( a => a.AsGuid() ).ToList();

            IEnumerable<FinancialAccountCache> financialAccountList;

            // no accounts specified means "all Active"
            if ( blockAccountGuidList.Any() )
            {
                financialAccountList = FinancialAccountCache.GetByGuids( blockAccountGuidList );
            }
            else
            {
                financialAccountList = FinancialAccountCache.All();
            }

            financialAccountList = financialAccountList.Where( a => a.IsActive );

            if ( !personalAccountGuidList.Any() )
            {
                if ( !optionalAccountGuidList.Any() )
                {
                    // if no personal accounts are selected, and there are no optional accounts either, show all the accounts that are allowed in block settings
                }
                else
                {
                    // if no personal accounts are selected, but there are optional accounts, only show the optional accounts (added manually)
                    financialAccountList = financialAccountList.Where( a => false );
                }
            }
            else
            {
                // if there are person accounts selected, limit accounts to personal accounts
                var selectedAccountList = financialAccountList.Where( a => personalAccountGuidList.Contains( a.Guid ) );

                // If include child accounts is selected, then also select all child accounts of the selected accounts.
                if ( preferences.GetValue( "include-child-accounts" ).AsBoolean() )
                {
                    var selectedParentIds = selectedAccountList.Select( a => a.Id ).ToList();

                    // Now find only those accounts that are descendants of one of the selected (parent) Ids
                    // OR if it is one of the selected Ids.
                    financialAccountList = financialAccountList.Where( a => a.GetAncestorFinancialAccountIds().Any( x => selectedParentIds.Contains( x ) ) || selectedParentIds.Contains( a.Id ) );
                }
                else
                {
                    financialAccountList = selectedAccountList;
                }
            }

            // Show only the accounts that match the batch campus if the corresponding setting is true
            int? batchId = PageParameter( PageParameterKey.BatchId ).AsIntegerOrNull();
            if ( preferences.GetValue( "filter-accounts-batch-campus" ).AsBoolean() && batchId.HasValue )
            {
                // Put a highlight label on this panel that shows the Campus of the Batch being worked on:
                var batchCampusId = new FinancialBatchService( rockContext ).GetSelect( batchId.Value, a => a.CampusId );
                if ( batchCampusId.HasValue )
                {
                    hlCampus.Text = "Batch Campus: " + CampusCache.Get( batchCampusId.Value ).Name;
                    hlCampus.Visible = true;

                    // Filter out anything that does not match the batch's campus.
                    financialAccountList = financialAccountList.Where( a => a.CampusId.HasValue && a.CampusId.Value == batchCampusId );
                }
            }

            int? campusId = preferences.GetValue( "account-campus" ).AsIntegerOrNull();
            if ( campusId.HasValue )
            {
                financialAccountList = financialAccountList.Where( a => !a.CampusId.HasValue || a.CampusId.Value == campusId.Value );
            }

            _visibleDisplayedAccountIds = new List<int>( financialAccountList.OrderBy( a => a.Order ).ThenBy( a => a.Name ).Select( a => a.Id ).ToList() ); // Later on these are assumed to be in order Issue: #5371
            _visibleOptionalAccountIds = new List<int>();

            // make the datasource all accounts, but only show the ones that are in _visibleAccountIds or have a non-zero amount
            var allAccountList = new FinancialAccountService( rockContext ).Queryable().AsNoTracking().OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            rptAccounts.DataSource = allAccountList;
            rptAccounts.DataBind();

            var optionalAccounts = allAccountList.Where( a => optionalAccountGuidList.Contains( a.Guid ) && !_visibleDisplayedAccountIds.Contains( a.Id ) ).ToList();
            if ( blockAccountGuidList.Any() )
            {
                optionalAccounts = optionalAccounts.Where( a => blockAccountGuidList.Contains( a.Guid ) ).ToList();
            }

            _allOptionalAccountIds = optionalAccounts.Select( a => a.Id ).ToList();

            UpdateVisibleAccountBoxes();

            rcwEnvelope.Visible = GlobalAttributesCache.Get().EnableGivingEnvelopeNumber;
        }

        /// <summary>
        /// Gets the campus name from batch id.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="batchId">The batch identifier.</param>
        /// <returns>
        /// The campus name or empty string if the batch has no campus.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        private string GetCampusNameFromBatch( RockContext rockContext, int? batchId )
        {
            if ( !batchId.HasValue )
            {
                return string.Empty;
            }

            var name = new FinancialBatchService( rockContext ).Queryable().Where( b => b.Id == batchId.Value ).Select( a => a.Campus.Name ).FirstOrDefault();
            if ( name != null )
            {
                return name;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the user preference key prefix.
        /// </summary>
        /// <returns></returns>
        private string GetUserPreferenceKeyPrefix()
        {
            return string.Format( "transaction-matching-{0}-", BlockId );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="batchId">The financial batch identifier.</param>
        public void ShowDetail( int batchId )
        {
            btnAddFamily.Visible = true;
            btnAddBusiness.Visible = true;
            btnFilter.Visible = true;
            divAddNewMatch.Visible = false;
            pnlEdit.Visible = true;

            string addFamilyUrl = this.LinkedPageUrl( AttributeKey.AddFamilyLink );
            string addBusinessUrl = this.LinkedPageUrl( AttributeKey.AddBusinessLink );

            ppSelectNew.ExpandSearchOptions = this.GetAttributeValue( AttributeKey.ExpandPersonSearchOptions ).AsBoolean();

            if ( !string.IsNullOrWhiteSpace( addFamilyUrl ) )
            {
                // force the link to open a new scrollable,resizable browser window (and make it work in FF, Chrome and IE) http://stackoverflow.com/a/2315916/1755417
                btnAddFamily.OnClientClick = string.Format( "javascript: window.open('{0}', '_blank', 'scrollbars=1,resizable=1,toolbar=1'); return false;", addFamilyUrl );
            }
            else
            {
                btnAddFamily.OnClientClick = null;
            }

            if ( !string.IsNullOrWhiteSpace( addBusinessUrl ) )
            {
                btnAddBusiness.OnClientClick = string.Format( "javascript: window.open('{0}', '_blank', 'scrollbars=1,resizable=1,toolbar=1'); return false;", addBusinessUrl );
            }
            else
            {
                btnAddBusiness.OnClientClick = null;
            }

            hfBatchId.Value = batchId.ToString();
            hfTransactionId.Value = string.Empty;

            int? specificTransactionId = PageParameter( PageParameterKey.TransactionId ).AsIntegerOrNull();
            if ( specificTransactionId.HasValue )
            {
                hfBackNextHistory.Value = specificTransactionId.Value.ToString();
                btnPrevious.Visible = false;
                btnCancel.Visible = true;
                btnNext.Text = "Save";
            }

            NavigateToTransaction( IsPostBack ? Direction.Current : Direction.Next );
        }

        /// <summary>
        ///
        /// </summary>
        private enum Direction
        {
            Prev,
            Next,
            Current
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

                // reset the visible optional account ids everytime they navigate to a new transaction
                _visibleOptionalAccountIds = new List<int>();

                List<int> historyList = hfBackNextHistory.Value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsIntegerList().Where( a => a > 0 ).ToList();
                int position = hfHistoryPosition.Value.AsIntegerOrNull() ?? -1;

                if ( direction == Direction.Prev )
                {
                    position--;
                }
                else if ( direction == Direction.Next )
                {
                    position++;
                }
                else if ( direction == Direction.Current )
                {
                    // If navigate is set to stay on current, stay on the current transaction ( and don't change history position )
                    toTransactionId = fromTransactionId;
                }

                if ( ( toTransactionId == null ) && ( historyList.Count > position ) )
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
                    .Where( a =>
                        a.AuthorizedPersonAliasId == null
                        &&
                        (
                            a.ProcessedByPersonAliasId == null
                            || a.ProcessedByPersonAliasId == CurrentPersonAliasId
                        )
                    );

                if ( batchId != 0 )
                {
                    qryTransactionsToMatch = qryTransactionsToMatch.Where( a => a.BatchId == batchId );
                }

                // if a specific transactionId was specified (because we are navigating thru history), load that one. Otherwise, if a batch is specified, get the first unmatched transaction in that batch
                if ( toTransactionId.HasValue )
                {
                    qryTransactionsToMatch = financialTransactionService
                        .Queryable()
                        .Include( ft => ft.AuthorizedPersonAlias.Person )
                        .Include( ft => ft.ProcessedByPersonAlias.Person )
                        .Include( ft => ft.Images )
                        .Where( a => a.Id == toTransactionId );
                }

                if ( historyList.Any() && !toTransactionId.HasValue )
                {
                    // since we are looking for a transaction we haven't viewed or matched yet, look for the next one in the database that we haven't seen yet
                    qryTransactionsToMatch = qryTransactionsToMatch.Where( a => !historyList.Contains( a.Id ) );
                }

                // put them in a predictable order
                qryTransactionsToMatch = qryTransactionsToMatch.OrderBy( a => a.CreatedDateTime ).ThenBy( a => a.Id );

                FinancialTransaction transactionToMatch = qryTransactionsToMatch.FirstOrDefault();
                if ( transactionToMatch == null )
                {
                    // we exhausted the transactions that aren't processed and aren't in our history list, so remove those restrictions and show all transactions that haven't been matched yet
                    var qryRemainingTransactionsToMatch = financialTransactionService
                        .Queryable()
                        .Include( ft => ft.AuthorizedPersonAlias.Person )
                        .Include( ft => ft.ProcessedByPersonAlias.Person )
                        .Include( ft => ft.Images )
                        .Where( a => a.AuthorizedPersonAliasId == null );

                    if ( batchId != 0 )
                    {
                        qryRemainingTransactionsToMatch = qryRemainingTransactionsToMatch.Where( a => a.BatchId == batchId );
                    }

                    // put them in a predictable order
                    qryRemainingTransactionsToMatch = qryRemainingTransactionsToMatch.OrderBy( a => a.CreatedDateTime ).ThenBy( a => a.Id );

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

                if ( transactionToMatch == null )
                {
                    nbNoUnmatchedTransactionsRemaining.Visible = true;
                    lbFinish.Visible = true;
                    pnlEdit.Visible = false;

                    btnFilter.Visible = false;
                    btnAddFamily.Visible = false;
                    btnAddBusiness.Visible = false;
                }
                else
                {
                    nbNoUnmatchedTransactionsRemaining.Visible = false;
                    lbFinish.Visible = false;
                    pnlEdit.Visible = true;
                }

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

                    // Unless somebody else is processing it, immediately mark the transaction as getting processed by the current person so that other potential transaction matching sessions will know that it is currently getting looked at
                    if ( !transactionToMatch.ProcessedByPersonAliasId.HasValue )
                    {
                        transactionToMatch.ProcessedByPersonAlias = null;
                        transactionToMatch.ProcessedByPersonAliasId = CurrentPersonAliasId;
                        transactionToMatch.ProcessedDateTime = RockDateTime.Now;
                        rockContext.SaveChanges();
                    }

                    hfTransactionId.Value = transactionToMatch.Id.ToString();

                    // stored the value in cents to avoid javascript floating point math issues
                    hfOriginalTotalAmount.Value = ( transactionToMatch.TotalAmount * 100 ).ToString();
                    hfCurrencySymbol.Value = RockCurrencyCodeInfo.GetCurrencySymbol();

                    // get the first 2 images (should be no more than 2, but just in case)
                    var transactionImages = transactionToMatch.Images.OrderBy( a => a.Order ).Take( 2 ).ToList();

                    ddlIndividual.Items.Clear();
                    ddlIndividual.Items.Add( new ListItem( null, null ) );

                    // clear any previously shown badges
                    ddlIndividual.Attributes.Remove( "disabled" );
                    badgeIndividualCount.InnerText = string.Empty;

                    // if this transaction has a CheckMicrParts, try to find matching person(s)
                    string checkMicrHashed = null;

                    if ( !string.IsNullOrWhiteSpace( transactionToMatch.CheckMicrParts ) )
                    {
                        try
                        {
                            var checkMicrClearText = Encryption.DecryptString( transactionToMatch.CheckMicrParts );
                            var parts = checkMicrClearText.Split( '_' );
                            if ( parts.Length >= 2 )
                            {
                                checkMicrHashed = FinancialPersonBankAccount.EncodeAccountNumber( parts[0], parts[1] );
                            }
                        }
                        catch
                        {
                            // intentionally ignore exception when decripting CheckMicrParts since we'll be checking for null below
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

                    if ( direction != Direction.Current )
                    {
                        ppSelectNew.SetValue( null );
                    }

                    if ( transactionToMatch.TransactionDetails.Any() )
                    {
                        cbTotalAmount.Value = transactionToMatch.TotalAmount;
                    }
                    else
                    {
                        cbTotalAmount.Value = null;
                    }

                    tbTransactionCode.Text = transactionToMatch.TransactionCode;

                    // update accountboxes
                    foreach ( var accountBox in rptAccounts.ControlsOfTypeRecursive<CurrencyBox>() )
                    {
                        accountBox.Value = null;
                    }

                    bool existingAmounts = false;
                    foreach ( var detail in transactionToMatch.TransactionDetails )
                    {
                        var accountBox = rptAccounts.ControlsOfTypeRecursive<CurrencyBox>().Where( a => a.Attributes["data-account-id"].AsInteger() == detail.AccountId ).FirstOrDefault();
                        if ( accountBox != null )
                        {
                            existingAmounts = true;
                            accountBox.Value = detail.Amount;
                        }
                    }

                    if ( existingAmounts )
                    {
                        string keyPrefix = GetUserPreferenceKeyPrefix();
                        bool onlyShowSelectedAccounts = GetBlockPersonPreferences().GetValue( "only-show-selected-accounts" ).AsBoolean();
                        UpdateVisibleAccountBoxes( onlyShowSelectedAccounts );
                    }
                    else
                    {
                        UpdateVisibleAccountBoxes();
                    }

                    tbComments.Text = transactionToMatch.Summary;
                    var primaryImage = GetPrimaryImage( transactionToMatch );

                    if ( primaryImage != null )
                    {
                        lImage.Text = GetCheckImageHtml( primaryImage );
                        lImage.Visible = true;
                        nbNoTransactionImageWarning.Visible = false;

                        rptrImages.DataSource = transactionToMatch.Images
                            .Where( i => !i.Id.Equals( primaryImage.Id ) )
                            .OrderBy( i => i.Order )
                            .ToList();
                        rptrImages.DataBind();
                    }
                    else
                    {
                        lImage.Visible = false;
                        rptrImages.DataSource = null;
                        rptrImages.DataBind();
                        nbNoTransactionImageWarning.Visible = true;
                    }

                    if ( this.GetAttributeValue( AttributeKey.DisplayPaymentDetailAttributeControls ).AsBoolean() )
                    {
                        DisplayPaymentDetailAttributeControls( transactionToMatch );
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

                // get count of transactions that have been matched (not including the one we are currently editing)
                int currentTranId = hfTransactionId.Value.AsInteger();
                int matchedCount = qryTransactionCount.Count( a => a.AuthorizedPersonAliasId != null && a.Id != currentTranId );
                int percentComplete;

                int totalBatchItemCount = qryTransactionCount.Count();
                if ( totalBatchItemCount != 0 )
                {
                    percentComplete = ( int ) Math.Round( ( double ) ( 100 * matchedCount ) / totalBatchItemCount );
                }
                else
                {
                    percentComplete = 100;
                }

                lProgressBar.Text = string.Format(
                        @"<div class='progress'>
                            <div class='progress-bar progress-bar-info' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' style='width: {0}%;'>
                                {0}%
                            </div>
                        </div>",
                       percentComplete );

                hfBackNextHistory.Value = historyList.AsDelimited( "," );

                if ( _focusControl == null )
                {
                    _focusControl = rptAccounts.ControlsOfTypeRecursive<Rock.Web.UI.Controls.CurrencyBox>().Where( a => a.Visible ).FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Gets the primary check image HTML.
        /// </summary>
        private string GetCheckImageHtml( FinancialTransactionImage financialTransactionImage )
        {
            if ( financialTransactionImage == null )
            {
                return string.Empty;
            }

            var imageUrl = FileUrlHelper.GetImageUrl( financialTransactionImage.BinaryFileId );
            return $"<a href='{imageUrl}' target='_blank' rel='noopener noreferrer'><img src='{imageUrl}'/></a>";
        }

        private void UpdateVisibleAccountBoxes( bool onlyShowSelectedAccounts = false )
        {
            List<int> _sortedAccountIds = _visibleDisplayedAccountIds.ToList();
            _sortedAccountIds.AddRange( _visibleOptionalAccountIds );

            List<int> _visibleAccountBoxes = new List<int>();

            foreach ( var accountBox in rptAccounts.ControlsOfTypeRecursive<CurrencyBox>() )
            {
                int accountBoxAccountId = accountBox.Attributes["data-account-id"].AsInteger();
                accountBox.Visible = !onlyShowSelectedAccounts && ( _visibleDisplayedAccountIds.Contains( accountBoxAccountId ) || _visibleOptionalAccountIds.Contains( accountBoxAccountId ) );

                if ( !accountBox.Visible && ( accountBox.Value ?? 0.0M ) != 0 )
                {
                    // if there is a non-zero amount, show the edit box regardless of the account filter settings
                    accountBox.Visible = true;
                }

                if ( accountBox.Visible )
                {
                    _visibleAccountBoxes.Add( accountBoxAccountId );
                }

                accountBox.Attributes["data-sort-order"] = _sortedAccountIds.IndexOf( accountBoxAccountId ).ToString();
            }

            var optionalAccounts = new FinancialAccountService( new RockContext() ).GetByIds( _allOptionalAccountIds ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            ddlAddAccount.Items.Clear();
            ddlAddAccount.Items.Add( new ListItem() );
            foreach ( var account in optionalAccounts )
            {
                if ( !_visibleAccountBoxes.Contains( account.Id ) )
                {
                    ddlAddAccount.Items.Add( new ListItem( account.PublicName, account.Id.ToString() ) );
                }
            }

            pnlAddOptionalAccount.Visible = ddlAddAccount.Items.Count > 1;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the dvpAddPersonMaritalStatus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dvpAddPersonMaritalStatus_SelectedIndexChanged( object sender, EventArgs e )
        {
            SyncFamilyControlsOnRoleAndMaritalStatus();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the bgAddPersonRole control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bgAddPersonRole_SelectedIndexChanged( object sender, EventArgs e )
        {
            SyncFamilyControlsOnRoleAndMaritalStatus();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveNewMatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveNewMatch_Click( object sender, EventArgs e )
        {
            var newPerson = IsAddNewFamilyMode() ?
                SaveNewFamily() :
                SaveNewBusiness();

            if ( newPerson != null )
            {
                hfIsAddNewFamilyMode.Value = "false";
                hfIsAddNewBusinessMode.Value = "false";
                RenderState();

                ppSelectNew.SetValue( newPerson );
                ddlIndividual.SetValue( string.Empty );
                LoadPersonPreview( ppSelectNew.PersonId.Value );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelNewMatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelNewMatch_Click( object sender, EventArgs e )
        {
            hfIsAddNewBusinessMode.Value = "false";
            hfIsAddNewFamilyMode.Value = "false";
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the BtnAddBusiness control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected void BtnAddBusiness_Click( object sender, EventArgs e )
        {
            hfIsAddNewFamilyMode.Value = "false";
            hfIsAddNewBusinessMode.Value = "true";
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the BtnAddFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected void BtnAddFamily_Click( object sender, EventArgs e )
        {
            hfIsAddNewFamilyMode.Value = "true";
            hfIsAddNewBusinessMode.Value = "false";
            RenderState();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAccountsPersonalFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAccountsPersonalFilter_SaveClick( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            var selectedAccountIdList = apDisplayedPersonalAccounts.SelectedValuesAsInt().ToList();
            var selectedAccountGuidList = FinancialAccountCache.GetByIds( selectedAccountIdList ).Select( a => a.Guid ).ToList();
            preferences.SetValue( "account-list", selectedAccountGuidList.AsDelimited( "," ) );

            var optionalAccountIdList = apOptionalPersonalAccounts.SelectedValuesAsInt().ToList();
            var optionalAccountGuidList = FinancialAccountCache.GetByIds( optionalAccountIdList ).Select( a => a.Guid ).ToList();
            preferences.SetValue( "optional-account-list", optionalAccountGuidList.AsDelimited( "," ) );

            preferences.SetValue( "only-show-selected-accounts", cbOnlyShowSelectedAccounts.Checked.ToString() );

            int? campusId = cpAccounts.SelectedCampusId;
            preferences.SetValue( "account-campus", campusId.HasValue ? campusId.Value.ToString() : "" );

            bool includeChildAccounts = cbIncludeChildAccounts.Checked;
            preferences.SetValue( "include-child-accounts", cbIncludeChildAccounts.Checked.ToString() );

            bool filterAccountsByBatchCampus = cbFilterAccountsByBatchsCampus.Checked;
            preferences.SetValue( "filter-accounts-batch-campus", cbFilterAccountsByBatchsCampus.Checked.ToString() );
            hlCampus.Visible = false;

            preferences.Save();

            mdAccountsPersonalFilter.Hide();

            // load the dropdowns again since account filter may have changed
            LoadDropDowns();

            // load the current transaction again to make sure UI shows the accounts based on the updated filter settings
            NavigateToTransaction( Direction.Current );

            // Reload the transaction amounts after changing the displayed accounts.
            int? transactionId = hfTransactionId.Value.AsIntegerOrNull();
            if ( transactionId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var financialTransactionService = new FinancialTransactionService( rockContext );
                    var txn = financialTransactionService.Queryable().Where( t => t.Id == transactionId ).SingleOrDefault();

                    foreach ( var detail in txn.TransactionDetails )
                    {
                        var accountBox = rptAccounts.ControlsOfTypeRecursive<CurrencyBox>().Where( a => a.Attributes["data-account-id"].AsInteger() == detail.AccountId ).FirstOrDefault();
                        if ( accountBox != null )
                        {
                            accountBox.Value = detail.Amount;
                        }
                    }
                }
            }


        }

        /// <summary>
        /// Handles the Click event of the btnFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            var personalAccountGuidList = preferences.GetValue( "account-list" ).SplitDelimitedValues().Select( a => a.AsGuid() ).ToList();
            var personalAccountList = FinancialAccountCache
                .GetByGuids( personalAccountGuidList )
                .Where( a => a.IsActive )
                .ToList();
            apDisplayedPersonalAccounts.SetValuesFromCache( personalAccountList );

            var optionalAccountGuidList = preferences.GetValue( "optional-account-list" ).SplitDelimitedValues().Select( a => a.AsGuid() ).ToList();
            var optionalAccountList = FinancialAccountCache
                .GetByGuids( optionalAccountGuidList )
                .Where( a => a.IsActive )
                .ToList();
            apOptionalPersonalAccounts.SetValuesFromCache( optionalAccountList );

            cbOnlyShowSelectedAccounts.Checked = preferences.GetValue( "only-show-selected-accounts" ).AsBoolean();
            cbIncludeChildAccounts.Checked = preferences.GetValue( "include-child-accounts" ).AsBoolean();
            cbFilterAccountsByBatchsCampus.Checked = preferences.GetValue( "filter-accounts-batch-campus" ).AsBoolean();

            cpAccounts.Campuses = CampusCache.All();
            cpAccounts.SelectedCampusId = preferences.GetValue( "account-campus" ).AsIntegerOrNull();

            mdAccountsPersonalFilter.Show();

            cbFilterAccountsByBatchsCampus.Visible = cpAccounts.Visible;
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
            var changes = new History.HistoryChangeList();

            var rockContext = new RockContext();
            var financialTransactionService = new FinancialTransactionService( rockContext );
            var financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );
            var financialPersonBankAccountService = new FinancialPersonBankAccountService( rockContext );
            int txnId = hfTransactionId.Value.AsInteger();
            var financialTransaction = financialTransactionService
                    .Queryable()
                    .Include( a => a.AuthorizedPersonAlias.Person )
                    .Include( a => a.ProcessedByPersonAlias.Person )
                    .FirstOrDefault( t => t.Id == txnId );

            // set the AuthorizedPersonId (the person who wrote the check, for example) to the if the SelectNew person (if selected) or person selected in the drop down (if there is somebody selected)
            int? authorizedPersonId = ppSelectNew.PersonId ?? ddlIndividual.SelectedValue.AsIntegerOrNull();

            var accountNumberSecured = hfCheckMicrHashed.Value;


            /* 07/24/2014 (added engineer note on 2020-09-23) MDP
             *
             * Note: The logic for this isn't what you might expect!
             *
             * A FinancialTransaction should only have amounts if it is matched to a person, so

             - If individual is not selected, don't save any amounts, even if they entered amounts on the UI. So we will ignore them since an individual wasn't selected.
             - If they 'Unmatched' (the transaction had previously been matched to an individual, but now it isn't) clear out any amounts (even if amounts were specified in the UI)

             - If an individual is selected, then amount is required
               - If amount and individual are both specified, the transaction will updated (and will be considered a Matched transaction)
               - If an individual is selected, but amount isn't, a warning will be shown tell them the amount is required
             */


            // if the transaction was previously matched, but user unmatched it, save it as an unmatched transaction and clear out the detail records (we don't want an unmatched transaction to have detail records)
            if ( financialTransaction != null &&
                financialTransaction.AuthorizedPersonAliasId.HasValue &&
                !authorizedPersonId.HasValue )
            {
                financialTransaction.AuthorizedPersonAliasId = null;
                foreach ( var detail in financialTransaction.TransactionDetails.ToList() )
                {
                    History.EvaluateChange( changes, detail.Account != null ? detail.Account.Name : "Unknown", detail.Amount.FormatAsCurrency(), string.Empty );
                    financialTransactionDetailService.Delete( detail );
                }

                changes.AddChange( History.HistoryVerb.Unmatched, History.HistoryChangeType.Record, "Transaction" );

                HistoryService.SaveChanges(
                    rockContext,
                    typeof( FinancialBatch ),
                    Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                    financialTransaction.BatchId.Value,
                    changes,
                    string.Format( "Transaction Id: {0}", financialTransaction.Id ),
                    typeof( FinancialTransaction ),
                    financialTransaction.Id,
                    false );

                rockContext.SaveChanges();

                // if the transaction was unmatched, clear out the ProcessedBy fields since we didn't match the transaction and are moving on to process another transaction
                MarkTransactionAsNotProcessedByCurrentUser( hfTransactionId.Value.AsInteger() );
            }

            // if the transaction is matched to somebody, attempt to save it.  Otherwise, if the transaction was previously matched, but user unmatched it, save it as an unmatched transaction
            if ( financialTransaction != null && authorizedPersonId.HasValue )
            {
                if ( cbTotalAmount.Value == null )
                {
                    nbSaveError.Text = "Total amount must be allocated to accounts.";
                    nbSaveError.Visible = true;
                    return;
                }

                var personAlias = new PersonAliasService( rockContext ).GetPrimaryAlias( authorizedPersonId.Value );
                int? personAliasId = personAlias != null ? personAlias.Id : ( int? ) null;

                // if this transaction has an accountnumber associated with it (in other words, it's a valid scanned check), ensure there is a financialPersonBankAccount record
                if ( financialTransaction.MICRStatus == MICRStatus.Success && !string.IsNullOrWhiteSpace( accountNumberSecured ) )
                {
                    var financialPersonBankAccount = financialPersonBankAccountService.Queryable().Where( a => a.AccountNumberSecured == accountNumberSecured && a.PersonAlias.PersonId == authorizedPersonId.Value ).FirstOrDefault();
                    if ( financialPersonBankAccount == null )
                    {
                        if ( personAliasId.HasValue )
                        {
                            financialPersonBankAccount = new FinancialPersonBankAccount();
                            financialPersonBankAccount.PersonAliasId = personAliasId.Value;
                            financialPersonBankAccount.AccountNumberSecured = accountNumberSecured;

                            var checkMicrClearText = Encryption.DecryptString( financialTransaction.CheckMicrParts );
                            var parts = checkMicrClearText.Split( '_' );
                            if ( parts.Length >= 2 )
                            {
                                financialPersonBankAccount.AccountNumberMasked = parts[1].Masked();
                            }

                            if ( string.IsNullOrWhiteSpace( financialPersonBankAccount.AccountNumberMasked ) )
                            {
                                financialPersonBankAccount.AccountNumberMasked = "************????";
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
                    History.EvaluateChange( changes, detail.Account != null ? detail.Account.Name : "Unknown", detail.Amount.FormatAsCurrency(), string.Empty );
                }

                foreach ( var accountBox in rptAccounts.ControlsOfTypeRecursive<CurrencyBox>() )
                {
                    var amount = accountBox.Value;

                    if ( amount.HasValue && amount.Value >= 0 )
                    {
                        var financialTransactionDetail = new FinancialTransactionDetail();
                        financialTransactionDetail.TransactionId = financialTransaction.Id;
                        financialTransactionDetail.AccountId = accountBox.Attributes["data-account-id"].AsInteger();
                        financialTransactionDetail.Amount = amount.Value;
                        financialTransactionDetailService.Add( financialTransactionDetail );

                        History.EvaluateChange( changes, accountBox.Label, 0.0M.FormatAsCurrency(), amount.Value.FormatAsCurrency() );
                    }
                }

                financialTransaction.TransactionCode = tbTransactionCode.Text;

                financialTransaction.Summary = tbComments.Text;

                financialTransaction.ProcessedByPersonAliasId = this.CurrentPersonAlias.Id;
                financialTransaction.ProcessedDateTime = RockDateTime.Now;

                if ( this.GetAttributeValue( AttributeKey.DisplayPaymentDetailAttributeControls ).AsBoolean() )
                {
                    // Payment Detail Attributes
                    financialTransaction.FinancialPaymentDetail.LoadAttributes( rockContext );
                    Helper.GetEditValues( phPaymentAttributeEdits, financialTransaction.FinancialPaymentDetail );
                }

                changes.AddChange( History.HistoryVerb.Matched, History.HistoryChangeType.Record, "Transaction" );

                HistoryService.SaveChanges(
                    rockContext,
                    typeof( FinancialBatch ),
                    Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(),
                    financialTransaction.BatchId.Value,
                    changes,
                    personAlias != null && personAlias.Person != null ? personAlias.Person.FullName : string.Format( "Transaction Id: {0}", financialTransaction.Id ),
                    typeof( FinancialTransaction ),
                    financialTransaction.Id,
                    false );

                rockContext.SaveChanges();
                financialTransaction.FinancialPaymentDetail.SaveAttributeValues( rockContext );
            }
            else
            {
                // if the transaction was not matched, clear out the ProcessedBy fields since we didn't match the transaction and are moving on to process another transaction
                MarkTransactionAsNotProcessedByCurrentUser( hfTransactionId.Value.AsInteger() );
            }

            int? specificTransactionId = PageParameter( PageParameterKey.TransactionId ).AsIntegerOrNull();
            if ( specificTransactionId.HasValue )
            {
                var qryParams = new Dictionary<string, string>();
                int? batchId = hfBatchId.Value.AsIntegerOrNull();
                if ( batchId.HasValue )
                {
                    qryParams.Add( PageParameterKey.BatchId, batchId.Value.ToString() );
                }
                qryParams.Add( PageParameterKey.TransactionId, specificTransactionId.Value.ToString() );

                NavigateToLinkedPage( AttributeKey.TransactionDetailPage, qryParams );
            }
            else
            {
                NavigateToTransaction( Direction.Next );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            int? batchId = hfBatchId.Value.AsIntegerOrNull();
            if ( batchId.HasValue )
            {
                qryParams.Add( PageParameterKey.BatchId, batchId.Value.ToString() );
            }
            qryParams.Add( PageParameterKey.TransactionId, PageParameter( PageParameterKey.TransactionId ).AsInteger().ToString() );

            NavigateToLinkedPage( AttributeKey.TransactionDetailPage, qryParams );
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

            if ( personId.HasValue )
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
                _focusControl = rptAccounts.ControlsOfTypeRecursive<Rock.Web.UI.Controls.CurrencyBox>().Where( a => a.Visible ).FirstOrDefault();

                nbSaveError.Text = string.Empty;
                nbSaveError.Visible = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnFindByEnvelopeNumber control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFindByEnvelopeNumber_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personGivingEnvelopeAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.PERSON_GIVING_ENVELOPE_NUMBER.AsGuid() );
            var envelopeNumber = tbEnvelopeNumber.Text;
            if ( !string.IsNullOrEmpty( envelopeNumber ) )
            {
                var personIdsWithEnvelopeNumber = new AttributeValueService( rockContext ).Queryable()
                                        .Where( a => a.AttributeId == personGivingEnvelopeAttribute.Id && a.Value == envelopeNumber )
                                        .Select( a => a.EntityId.Value );
                var count = personIdsWithEnvelopeNumber.Count();
                var personService = new PersonService( rockContext );
                if ( count == 0 )
                {
                    lEnvelopeSearchResults.Text = string.Format( "No individual found with envelope number of {0}.", envelopeNumber );
                    cblEnvelopeSearchPersons.Visible = false;
                    mdEnvelopeSearchResults.SaveButtonText = string.Empty;
                    mdEnvelopeSearchResults.Show();
                }
                else if ( count == 1 )
                {
                    var personId = personIdsWithEnvelopeNumber.First();
                    ppSelectNew.SetValue( personService.Get( personId ) );
                    LoadPersonPreview( personId );
                }
                else
                {
                    lEnvelopeSearchResults.Text = string.Format( "More than one person is assigned envelope number {0}. Please select the individual you wish to use.", envelopeNumber );
                    cblEnvelopeSearchPersons.Visible = true;
                    cblEnvelopeSearchPersons.Items.Clear();
                    var personList = personService.Queryable().Where( a => personIdsWithEnvelopeNumber.Contains( a.Id ) ).AsNoTracking().ToList();
                    foreach ( var person in personList )
                    {
                        cblEnvelopeSearchPersons.Items.Add( new ListItem( person.FullName, person.Id.ToString() ) );
                    }

                    mdEnvelopeSearchResults.SaveButtonText = "Select";
                    mdEnvelopeSearchResults.Show();
                }
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdEnvelopeSearchResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdEnvelopeSearchResults_SaveClick( object sender, EventArgs e )
        {
            var personId = cblEnvelopeSearchPersons.SelectedValue.AsIntegerOrNull();
            if ( personId.HasValue )
            {
                mdEnvelopeSearchResults.Hide();
                ppSelectNew.SetValue( new PersonService( new RockContext() ).Get( personId.Value ) );
                LoadPersonPreview( personId );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbFinish_Click( object sender, EventArgs e )
        {
            int? batchId = hfBatchId.Value.AsIntegerOrNull();
            if ( batchId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var batch = new FinancialBatchService( rockContext ).Get( batchId.Value );
                    if ( batch != null && batch.Status == BatchStatus.Pending )
                    {
                        batch.Status = BatchStatus.Open;
                        rockContext.SaveChanges();
                    }
                }

                NavigateToLinkedPage( AttributeKey.BatchDetailPage, new Dictionary<string, string> { { PageParameterKey.BatchId, batchId.Value.ToString() } } );
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
                // force the link to open a new scrollable,resizable browser window (and make it work in FF, Chrome and IE) http://stackoverflow.com/a/2315916/1755417
                lPersonName.Text = string.Format( "<a href onclick=\"javascript: window.open('/person/{0}', '_blank', 'scrollbars=1,resizable=1,toolbar=1'); return false;\">{1}</a>", person.Id, person.FullName );

                var spouse = person.GetSpouse( rockContext );
                lSpouseName.Text = spouse != null ? string.Format( "<p><strong>Spouse: </strong>{0}</p>", spouse.FullName ) : string.Empty;

                if ( CampusCache.All( false ).Count > 1 )
                {
                    var campus = person.GetCampus();
                    lCampus.Text = campus != null ? string.Format( "<p><strong>Campus: </strong>{0}</p>", campus.Name ) : string.Empty;
                }

                var previousDefinedValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS );
                var addresses = person.GetFamilies().SelectMany( a => a.GroupLocations ).OrderBy( l => l.GroupLocationTypeValue.Order ).ToList();
                if ( addresses.Where( a => a.GroupLocationTypeValueId == previousDefinedValue.Id ).Count() > 1 )
                {
                    var primaryAddresses = addresses.Where( a => a.GroupLocationTypeValueId != previousDefinedValue.Id ).ToList();
                    var previousAddress = addresses.Where( a => a.GroupLocationTypeValueId == previousDefinedValue.Id ).ToList();
                    primaryAddresses.Add( previousAddress.First() );

                    rptrAddresses.DataSource = primaryAddresses;
                    rptrAddresses.DataBind();
                    rptPrevAddresses.DataSource = previousAddress.Skip( 1 );
                    rptPrevAddresses.DataBind();
                    btnMoreAddress.Visible = true;
                }
                else
                {
                    rptrAddresses.DataSource = addresses;
                    rptrAddresses.DataBind();
                    btnMoreAddress.Visible = false;
                }

            }
        }

        /// <summary>
        /// Gets the image URL with optional maximum width and height properties.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns>The URL of the image with specified dimensions.</returns>
        protected string ImageUrl( int binaryFileId, int? maxWidth = null, int? maxHeight = null )
        {
            var options = new GetImageUrlOptions
            {
                MaxWidth = maxWidth,
                MaxHeight = maxHeight
            };

            return FileUrlHelper.GetImageUrl( binaryFileId, options );
        }

        /// <summary>
        /// Handles the SelectionChanged event of the ddlAddAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlAddAccount_SelectionChanged( object sender, EventArgs e )
        {
            // update accountboxes
            var accountId = ddlAddAccount.SelectedValue.AsIntegerOrNull();
            if ( accountId.HasValue && !_visibleOptionalAccountIds.Contains( accountId.Value ) )
            {
                _visibleOptionalAccountIds.Add( accountId.Value );
                var itemToRemove = ddlAddAccount.Items.FindByValue( accountId.Value.ToString() );
                ddlAddAccount.Items.Remove( itemToRemove );
            }

            UpdateVisibleAccountBoxes();

            if ( ddlAddAccount.Items.Count <= 1 )
            {
                pnlAddOptionalAccount.Visible = false;
            }

            foreach ( var accountBox in rptAccounts.ControlsOfTypeRecursive<CurrencyBox>() )
            {
                if ( accountBox.Attributes["data-account-id"].AsInteger() == accountId )
                {
                    accountBox.Value = cbOptionalAccountAmount.Value;
                    cbOptionalAccountAmount.Value = null;
                    _focusControl = accountBox;
                    break;
                }
            }
        }

        #endregion

        #region State Determining

        /// <summary>
        /// Determines whether [is add new family mode].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is add new family mode]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsAddNewFamilyMode()
        {
            return hfIsAddNewFamilyMode.Value.Trim().ToLower() == "true";
        }

        /// <summary>
        /// Determines whether [is add new business mode].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is add new business mode]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsAddNewBusinessMode()
        {
            return hfIsAddNewBusinessMode.Value.Trim().ToLower() == "true";
        }

        /// <summary>
        /// Shows the controls needed
        /// </summary>
        private void RenderState()
        {
            if ( IsAddNewFamilyMode() || IsAddNewBusinessMode() )
            {
                RenderAddNewMatchMode();
            }
            else
            {
                ShowDetail( PageParameter( PageParameterKey.BatchId ).AsInteger() );
            }
        }

        /// <summary>
        /// Renders the add new match mode.
        /// </summary>
        private void RenderAddNewMatchMode()
        {
            btnAddFamily.Visible = false;
            btnAddBusiness.Visible = false;
            btnFilter.Visible = false;
            divAddNewMatch.Visible = true;
            pnlEdit.Visible = false;

            var currentTransaction = GetCurrentTransactionUntracked();
            var primaryImage = GetPrimaryImage( currentTransaction );
            lAddNewMatchImage.Text = GetCheckImageHtml( primaryImage );

            if ( IsAddNewFamilyMode() )
            {
                RenderAddNewFamilyMode();
            }
            else
            {
                RenderAddNewBusinessMode();
            }
        }

        /// <summary>
        /// Shows the add new family mode.
        /// </summary>
        private void RenderAddNewFamilyMode()
        {
            divAddNewFamily.Visible = true;
            divAddNewBusiness.Visible = false;

            BindFamilyGroupRoles( bgAddPersonRole );

            var suffixDefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ).Id;
            dvpAddPersonSuffix.DefinedTypeId = suffixDefinedTypeId;
            dvpAddSpouseSuffix.DefinedTypeId = suffixDefinedTypeId;

            dvpAddPersonMaritalStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS ) ).Id;
            dvpAddPersonConnectionStatus.DefinedTypeId = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS ) ).Id;

            EnforceCampusSettings( cpAddPersonCampus );
            ResetAddPersonForm();
        }

        /// <summary>
        /// Shows the add new business mode.
        /// </summary>
        private void RenderAddNewBusinessMode()
        {
            divAddNewFamily.Visible = false;
            divAddNewBusiness.Visible = true;
            ResetAddBusinessForm();
        }

        #endregion State Determining

        #region Data Interface

        /// <summary>
        /// Saves the new family.
        /// </summary>
        /// <returns></returns>
        private Person SaveNewFamily()
        {
            var isMarried = IsNewPersonMarried();
            var isChild = IsNewPersonRoleChild();
            var gender = GetGender( rblAddPersonGender.SelectedValue );

            var person = new Person
            {
                RecordStatusValueId = GetRecordStatusId(),
                FirstName = tbAddPersonFirstName.Text,
                LastName = tbAddPersonLastName.Text,
                SuffixValueId = dvpAddPersonSuffix.SelectedDefinedValueId,
                Gender = gender,
                MaritalStatusValueId = isChild ? null : dvpAddPersonMaritalStatus.SelectedDefinedValueId,
                ConnectionStatusValueId = dvpAddPersonConnectionStatus.SelectedDefinedValueId,
                Email = ebAddPersonEmail.Text.Trim()
            };

            var homePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ).Id;
            var mobilePhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;

            var homePhone = GetPhoneModel( pnAddPersonHomePhone, homePhoneTypeId, cbAddPersonHomePhoneSms.Checked, cbAddPersonHomePhoneUnlisted.Checked );

            if ( homePhone != null )
            {
                person.PhoneNumbers.Add( homePhone );
            }

            var mobilePhone = GetPhoneModel( pnAddPersonMobilePhone, mobilePhoneTypeId, cbAddPersonMobilePhoneSms.Checked, cbAddPersonMobilePhoneUnlisted.Checked );

            if ( mobilePhone != null )
            {
                person.PhoneNumbers.Add( mobilePhone );
            }

            var groupMember = new GroupMember
            {
                Person = person,
                GroupRoleId = ( isChild ? GetChildRole() : GetAdultRole() ).Id
            };

            var familyMembers = new List<GroupMember> { groupMember };

            // Add spouse if needed
            if ( isMarried && ( !tbAddSpouseFirstName.Text.IsNullOrWhiteSpace() || !tbAddSpouseLastName.Text.IsNullOrWhiteSpace() ) )
            {
                var spouseGender = GetGender( rblAddSpouseGender.SelectedValue );

                var spouse = new Person
                {
                    RecordStatusValueId = GetRecordStatusId(),
                    FirstName = tbAddSpouseFirstName.Text,
                    LastName = tbAddSpouseLastName.Text,
                    SuffixValueId = dvpAddSpouseSuffix.SelectedDefinedValueId,
                    Gender = spouseGender,
                    MaritalStatusValueId = dvpAddPersonMaritalStatus.SelectedDefinedValueId,
                    ConnectionStatusValueId = dvpAddPersonConnectionStatus.SelectedDefinedValueId
                };

                if ( homePhone != null )
                {
                    var spouseHomePhone = GetPhoneModel( pnAddPersonHomePhone, homePhoneTypeId, cbAddPersonHomePhoneSms.Checked, cbAddPersonHomePhoneUnlisted.Checked );
                    spouse.PhoneNumbers.Add( spouseHomePhone );
                }

                var spouseGroupMember = new GroupMember
                {
                    Person = spouse,
                    GroupRoleId = GetAdultRole().Id
                };

                familyMembers.Add( spouseGroupMember );
            }

            var rockContext = new RockContext();
            var campusId = cpAddPersonCampus.SelectedCampusId;
            var addressLocation = GetAddressLocation( rockContext, acAddPersonAddress );
            var family = GroupService.SaveNewFamily( rockContext, familyMembers, cpAddPersonCampus.SelectedCampusId, false );

            if ( addressLocation != null )
            {
                GroupService.AddNewGroupAddress( rockContext, family, Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, addressLocation );
            }

            rockContext.SaveChanges();
            ResetAddPersonForm();
            return person;
        }

        /// <summary>
        /// Saves the new business.
        /// </summary>
        /// <returns></returns>
        private Person SaveNewBusiness()
        {
            var business = new Person
            {
                RecordStatusValueId = GetRecordStatusId(),
                RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id,
                LastName = tbAddBusinessName.Text,
                Email = ebAddBusinessEmail.Text.Trim()
            };

            var workPhoneTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ).Id;
            var workPhone = GetPhoneModel( pnbAddBusinessPhone, workPhoneTypeId, cbAddBusinessSms.Checked, cbAddBusinessUnlisted.Checked );

            if ( workPhone != null )
            {
                business.PhoneNumbers.Add( workPhone );
            }

            var familyMembers = new List<GroupMember> {
                new GroupMember
                {
                    Person = business,
                    GroupRoleId = GetAdultRole().Id
                }
            };

            var rockContext = new RockContext();
            var addressLocation = GetAddressLocation( rockContext, acAddBusinessAddress );
            var campusId = cpAddBusinessCampus.SelectedCampusId;

            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            var familyName = business.LastName + " Business";
            var family = GroupService.SaveNewGroup( rockContext, familyGroupType.Id, null, familyName, familyMembers, campusId, false );

            if ( addressLocation != null )
            {
                GroupService.AddNewGroupAddress( rockContext, family, Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK, addressLocation );
            }

            rockContext.SaveChanges();
            ResetAddBusinessForm();
            return business;
        }

        /// <summary>
        /// Gets the batch identifier.
        /// </summary>
        /// <returns></returns>
        private int GetBatchId()
        {
            if ( _batchId == default( int ) )
            {
                _batchId = PageParameter( PageParameterKey.BatchId ).AsInteger();
            }

            return _batchId;
        }
        private int _batchId;

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        /// <returns></returns>
        private FinancialTransaction GetCurrentTransactionUntracked()
        {
            var transactionId = hfTransactionId.Value.AsInteger();
            var cachedTransactionId = _currentTransactionUntracked == null ? 0 : _currentTransactionUntracked.Id;

            if ( transactionId != cachedTransactionId && transactionId == 0 )
            {
                _currentTransactionUntracked = null;
            }
            else if ( transactionId != cachedTransactionId )
            {
                var rockContext = new RockContext();
                var transactionService = new FinancialTransactionService( rockContext );

                _currentTransactionUntracked = transactionService.Queryable()
                    .AsNoTracking()
                    .Include( ft => ft.AuthorizedPersonAlias.Person )
                    .Include( ft => ft.ProcessedByPersonAlias.Person )
                    .Include( ft => ft.Images )
                    .FirstOrDefault( ft => ft.Id == transactionId );
            }

            return _currentTransactionUntracked;
        }
        private FinancialTransaction _currentTransactionUntracked;

        /// <summary>
        /// Gets the primary image.
        /// </summary>
        /// <param name="financialTransaction">The financial transaction.</param>
        /// <returns></returns>
        private FinancialTransactionImage GetPrimaryImage( FinancialTransaction financialTransaction )
        {
            if ( financialTransaction == null || !financialTransaction.Images.Any() )
            {
                return null;
            }

            return financialTransaction.Images
                .OrderBy( i => i.Order )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the child role.
        /// </summary>
        /// <returns></returns>
        private GroupTypeRoleCache GetChildRole()
        {
            var childRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
            var groupTypeCache = GroupTypeCache.GetFamilyGroupType();
            return groupTypeCache.Roles.FirstOrDefault( gtr => gtr.Guid == childRoleGuid );
        }

        /// <summary>
        /// Gets the adult role.
        /// </summary>
        /// <returns></returns>
        private GroupTypeRoleCache GetAdultRole()
        {
            var adultRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
            var groupTypeCache = GroupTypeCache.GetFamilyGroupType();
            return groupTypeCache.Roles.FirstOrDefault( gtr => gtr.Guid == adultRoleGuid );
        }

        #endregion Data Interface

        #region Control Helpers

        /// <summary>
        /// Binds the family group roles.
        /// </summary>
        /// <param name="buttonGroup">The button group.</param>
        private void BindFamilyGroupRoles( ButtonGroup buttonGroup )
        {
            buttonGroup.DataTextField = "Name";
            buttonGroup.DataValueField = "Id";

            var groupTypeCache = GroupTypeCache.GetFamilyGroupType();
            buttonGroup.DataSource = groupTypeCache.Roles;

            buttonGroup.DataBind();
            buttonGroup.SelectedIndex = 0;
        }

        /// <summary>
        /// Synchronizes the family controls based on family role and marital status.
        /// </summary>
        private void SyncFamilyControlsOnRoleAndMaritalStatus()
        {
            var isChild = IsNewPersonRoleChild();
            var isMarried = IsNewPersonMarried();

            // only prompt for Spouse if the selected marital status is married (and they aren't a child)
            divAddPersonSpouse.Visible = !isChild && ( isMarried );
            dvpAddPersonMaritalStatus.Visible = !isChild;
        }

        /// <summary>
        /// Determines whether [is new person role child].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is new person role child]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsNewPersonRoleChild()
        {
            var childRole = GetChildRole();
            return childRole != null && childRole.Id == bgAddPersonRole.SelectedValue.AsInteger();
        }

        /// <summary>
        /// Determines whether [is new person married].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is new person married]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsNewPersonMarried()
        {
            if ( IsNewPersonRoleChild() )
            {
                return false;
            }

            var marriedStatusGuid = Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid();
            var maritalStatusDefinedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS );
            var marriedStatus = maritalStatusDefinedType.DefinedValues.FirstOrDefault( dv => dv.Guid == marriedStatusGuid );
            return marriedStatus != null && marriedStatus.Id == dvpAddPersonMaritalStatus.SelectedDefinedValueId;
        }

        /// <summary>
        /// Gets the gender.
        /// </summary>
        /// <param name="mOrF">The m or f.</param>
        /// <returns></returns>
        private Gender GetGender( string mOrF )
        {
            if ( mOrF == "M" )
            {
                return Gender.Male;
            }

            if ( mOrF == "F" )
            {
                return Gender.Female;
            }

            return Gender.Unknown;
        }

        /// <summary>
        /// Gets a phone model.
        /// </summary>
        /// <param name="phoneNumberBox">The phone number box.</param>
        /// <param name="numberTypeValueId">The number type value identifier.</param>
        /// <param name="isSms">if set to <c>true</c> [is SMS].</param>
        /// <param name="isUnlisted">if set to <c>true</c> [is unlisted].</param>
        /// <returns></returns>
        private PhoneNumber GetPhoneModel( PhoneNumberBox phoneNumberBox, int numberTypeValueId, bool isSms, bool isUnlisted )
        {
            var cleanedNumber = PhoneNumber.CleanNumber( phoneNumberBox.Number );

            if ( cleanedNumber.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new PhoneNumber
            {
                Number = cleanedNumber,
                NumberTypeValueId = numberTypeValueId,
                IsMessagingEnabled = isSms,
                IsUnlisted = isUnlisted,
                CountryCode = PhoneNumber.CleanNumber( phoneNumberBox.CountryCode )
            };
        }

        /// <summary>
        /// Gets the address location.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="addressControl">The address control.</param>
        /// <returns></returns>
        private Location GetAddressLocation( RockContext rockContext, AddressControl addressControl )
        {
            // Only verify if at least one address field contains a value.
            // Ignore State as it is always prefilled with a value.
            if ( acAddPersonAddress.Street1.IsNullOrWhiteSpace() &&
                acAddPersonAddress.Street2.IsNullOrWhiteSpace() &&
                acAddPersonAddress.City.IsNullOrWhiteSpace() &&
                acAddPersonAddress.PostalCode.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var locationService = new LocationService( rockContext );
            return locationService.Get(
                addressControl.Street1,
                addressControl.Street2,
                addressControl.City,
                addressControl.State,
                addressControl.PostalCode,
                addressControl.Country );
        }

        /// <summary>
        /// Enforces the campus settings.
        /// </summary>
        /// <param name="campusPicker">The campus picker.</param>
        private void EnforceCampusSettings( CampusPicker campusPicker )
        {
            if ( campusPicker == null || campusPicker.Items == null || campusPicker.Items.Count < 1 )
            {
                return;
            }

            if ( campusPicker.Items.Count == 2 && campusPicker.Items[0].Value.IsNullOrWhiteSpace() )
            {
                campusPicker.SelectedIndex = 1;
                campusPicker.Visible = false;
                return;
            }

            var preferences = GetBlockPersonPreferences();
            var campusIdSetting = preferences.GetValue( "account-campus" ).AsIntegerOrNull();

            if ( campusIdSetting.HasValue )
            {
                campusPicker.SelectedValue = campusIdSetting.ToString();
                campusPicker.Visible = false;
            }
            else
            {
                campusPicker.SelectedValue = null;
                campusPicker.Visible = true;
            }
        }

        /// <summary>
        /// Resets the add person.
        /// </summary>
        private void ResetAddPersonForm()
        {
            tbAddPersonFirstName.Text = null;
            tbAddPersonLastName.Text = null;
            dvpAddPersonSuffix.SelectedDefinedValueId = null;
            rblAddPersonGender.SelectedValue = null;
            bgAddPersonRole.SelectedIndex = 0;

            SetDefaultValue( dvpAddPersonConnectionStatus, AttributeKey.DefaultPersonConnectionStatus );
            dvpAddPersonMaritalStatus.SelectedDefinedValueId = null;

            tbAddSpouseFirstName.Text = null;
            tbAddSpouseLastName.Text = null;
            dvpAddSpouseSuffix.SelectedDefinedValueId = null;
            rblAddSpouseGender.SelectedValue = null;

            EnforceCampusSettings( cpAddPersonCampus );
            acAddPersonAddress.SetValues( null );

            pnAddPersonHomePhone.Text = null;
            cbAddPersonHomePhoneSms.Checked = false;
            cbAddPersonHomePhoneUnlisted.Checked = false;

            pnAddPersonMobilePhone.Text = null;
            cbAddPersonMobilePhoneSms.Checked = false;
            cbAddPersonMobilePhoneUnlisted.Checked = false;

            ebAddPersonEmail.Text = null;

            SyncFamilyControlsOnRoleAndMaritalStatus();
            bgAddPersonRole.Visible = ShouldShowFamilyRoleControl();
            ebAddPersonEmail.Visible = ShouldShowEmailControl();
        }

        /// <summary>
        /// Resets the add person.
        /// </summary>
        private void ResetAddBusinessForm()
        {
            tbAddBusinessName.Text = null;

            pnbAddBusinessPhone.Text = null;
            cbAddBusinessSms.Checked = false;
            cbAddBusinessUnlisted.Checked = false;

            ebAddBusinessEmail.Text = null;

            EnforceCampusSettings( cpAddBusinessCampus );

            acAddBusinessAddress.SetValues( null );

            ebAddBusinessEmail.Visible = ShouldShowEmailControl();
        }

        /// <summary>
        /// Sets the default value.
        /// </summary>
        /// <param name="definedValuePicker">The defined value picker.</param>
        /// <param name="attributeKey">The attribute key.</param>
        private void SetDefaultValue( DefinedValuePicker definedValuePicker, string attributeKey )
        {
            var defaultValueGuid = GetAttributeValue( attributeKey ).AsGuidOrNull();

            if ( defaultValueGuid.HasValue )
            {
                var defaultValueId = DefinedValueCache.GetId( defaultValueGuid.Value );

                if ( defaultValueId.HasValue )
                {
                    definedValuePicker.SelectedDefinedValueId = defaultValueId.Value;
                    return;
                }
            }

            definedValuePicker.SelectedDefinedValueId = null;
        }

        /// <summary>
        /// Gets the record status identifier.
        /// </summary>
        /// <returns></returns>
        private int GetRecordStatusId()
        {
            var valueGuid = GetAttributeValue( AttributeKey.PersonRecordStatus ).AsGuidOrNull() ??
                Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid();

            return DefinedValueCache.Get( valueGuid ).Id;
        }

        /// <summary>
        /// Should show the family role control.
        /// </summary>
        /// <returns></returns>
        private bool ShouldShowFamilyRoleControl()
        {
            return GetAttributeValue( AttributeKey.ShowFamilyRole ).AsBooleanOrNull() ?? true;
        }

        /// <summary>
        /// Should show the family role control.
        /// </summary>
        /// <returns></returns>
        private bool ShouldShowEmailControl()
        {
            return GetAttributeValue( AttributeKey.ShowEmail ).AsBooleanOrNull() ?? false;
        }

        #endregion Control Helpers
    }
}