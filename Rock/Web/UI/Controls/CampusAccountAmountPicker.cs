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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="System.Web.UI.INamingContainer" />
    [ToolboxData( "<Rock:CampusAccountAmountPicker runat=\"server\" />" )]
    public class CampusAccountAmountPicker : CompositeControl, INamingContainer
    {
        #region Controls

        #region Controls for SingleAccount Mode

        private Panel _pnlAccountAmountEntrySingle;

        private CurrencyBox _cbAmountAccountSingle;

        private RockDropDownList _ddlSingleAccountCampus;
        private RockDropDownList _ddlAccountSingle;

        #endregion Controls for SingleAccount Mode

        #region Controls for MultiAccount Mode

        private Panel _pnlAccountAmountEntryMulti;
        private Repeater _rptPromptForAccountAmountsMulti;
        private RockDropDownList _ddlMultiAccountCampus;

        #endregion Controls for MultiAccount Mode

        #endregion Controls

        #region Enums

        /// <summary>
        ///
        /// </summary>
        public enum AccountAmountEntryMode
        {
            /// <summary>
            /// Single account can be selected
            /// </summary>
            SingleAccount,

            /// <summary>
            /// Multiple accounts are displayed with an amount input for each
            /// </summary>
            MultipleAccounts
        }

        #endregion Enums

        #region private constants

        private static class RepeaterControlIds
        {
            /// <summary>
            /// The control ID for the hfAccountAmountMultiAccountId hidden field
            /// </summary>
            internal const string ID_hfAccountAmountMultiAccountId = "hfAccountAmountMultiAccountId";

            /// <summary>
            /// The control ID for the cbAccountAmountMulti currency box
            /// </summary>
            internal const string ID_cbAccountAmountMulti = "cbAccountAmountMulti";
        }

        #endregion private constants

        #region Properties

        /// <summary>
        /// Gets or sets the currency code value identifier.
        /// </summary>
        /// <value>
        /// The currency code value identifier.
        /// </value>
        public int CurrencyCodeDefinedValueId
        {
            get
            {
                var storedValue = ViewState["CurrencyCodeDefinedValueId"].ToStringSafe().AsIntegerOrNull();

                if ( storedValue.HasValue )
                {
                    return storedValue.Value;
                }

                return new RockCurrencyCodeInfo().CurrencyCodeDefinedValueId;
            }
            set
            {
                ViewState["CurrencyCodeDefinedValueId"] = value;
                SyncCurrencyBoxesCurrencyCodes();
            }
        }

        /// <summary>
        /// The Lava Template to use as the amount input label for each account.
        /// Default is Account.PublicName.
        /// </summary>
        /// <value>The account header template.</value>
        public string AccountHeaderTemplate
        {
            get => ViewState["AccountHeaderTemplate"] as string;
            set => ViewState["AccountHeaderTemplate"] = value;
        }

        /// <summary>
        /// If enabled, the <seealso cref="SelectedAccountIds"/> will be determined as follows:
        /// <list type="number">
        ///   <item>If the selected account is not associated with a campus, the Selected Account will be the first matching active child account that is associated with the selected campus.</item>
        ///   <item>If the selected account is not associated with a campus, but there are no active child accounts for the selected campus, the parent account (the one the user sees) will be returned.</item>
        ///   <item>If the selected account is associated with a campus, that account will be returned regardless of campus selection (and it won't use the child account logic)</item>
        /// </list>
        /// Default is true.
        /// </summary>
        public bool UseAccountCampusMappingLogic
        {
            get => ViewState["UseAccountCampusMappingLogic"] as bool? ?? true;
            set => ViewState["UseAccountCampusMappingLogic"] = value;
        }

        /// <summary>
        /// Gets or sets the amount entry mode (Defaults to <seealso cref="AccountAmountEntryMode.SingleAccount"/> )
        /// </summary>
        /// <value>
        /// The amount entry mode.
        /// </value>
        public AccountAmountEntryMode AmountEntryMode
        {
            get => ViewState["AmountEntryMode"] as CampusAccountAmountPicker.AccountAmountEntryMode? ?? CampusAccountAmountPicker.AccountAmountEntryMode.SingleAccount;
            set => ViewState["AmountEntryMode"] = value;
        }

        /// <summary>
        /// Gets or sets the accountIds of the selectable accounts (Required).
        /// This will be the accounts that will be displayed, but only if the Account is <seealso cref="FinancialAccount.IsActive"/>, <seealso cref="FinancialAccount.IsPublic"/>, and within the <seealso cref="FinancialAccount.StartDate"/> and <seealso cref="FinancialAccount.EndDate"/> of the Account
        /// Note: This has special logic. See comments on <seealso cref="SelectedAccountIds"/>
        /// </summary>
        /// <value>
        /// The selectable account ids.
        /// </value>
        public int[] SelectableAccountIds
        {
            get => ViewState["SelectableAccountIds"] as int[] ?? new int[0];
            set
            {
                ViewState["SelectableAccountIds"] = value;
                EnsureChildControls();
                BindAccounts();
            }
        }

        /// <summary>
        /// If enabled the Accounts will be ordered by the index of <see cref="SelectableAccountIds"/>,
        /// that is they will be rendered in the order in which the Account Ids were added to the <see cref="SelectableAccountIds"/>
        /// </summary>
        public bool OrderBySelectableAccountsIndex
        {
            get => ViewState["OrderBySelectableAccountsIndex"] as bool? ?? false;
            set => ViewState["OrderBySelectableAccountsIndex"] = value;
        }

        /// <summary>
        /// If enabled private accounts in the  <see cref="SelectableAccountIds"/> will be rendered.
        /// </summary>
        public bool AllowPrivateSelectableAccounts
        {
            get => ViewState["AllowPrivateAccounts"] as bool? ?? false;
            set => ViewState["AllowPrivateAccounts"] = value;
        }

        /// <summary>
        /// Class to specify an amount for a selected AccountId
        /// </summary>
        public class AccountIdAmount
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AccountIdAmount"/> class.
            /// </summary>
            /// <param name="accountId">The account identifier.</param>
            /// <param name="amount">The amount.</param>
            public AccountIdAmount( int accountId, decimal? amount )
            {
                this.AccountId = accountId;
                this.Amount = amount;
            }

            /// <summary>
            /// Gets or sets the account identifier.
            /// </summary>
            /// <value>
            /// The account identifier.
            /// </value>
            public int AccountId { get; set; }

            /// <summary>
            /// Gets or sets the amount.
            /// </summary>
            /// <value>
            /// The amount.
            /// </value>
            public decimal? Amount { get; set; }

            /// <summary>
            /// Set ReadOnly to True to prevent the amount from being changed.
            /// </summary>
            /// <value>
            ///   <c>true</c> if [read only]; otherwise, <c>false</c>.
            /// </value>
            public bool ReadOnly { get; set; } = false;
        }

        /*
        /// <summary>
        /// The financial accounts cache
        /// </summary>
        private Dictionary<int, FinancialAccountInfo> _financialAccountsCache;

        /// <summary>
        /// private class for lightweight FinancialAccountInfo
        /// </summary>
        private class FinancialAccountInfo
        {
            public int Id { get; set; }

            public int? ParentAccountId { get; set; }

            public FinancialAccountInfo ParentAccount { get; set; }

            public int? CampusId { get; set; }

            public bool IsActive { get; set; }

            public DateTime? StartDate { get; set; }

            public DateTime? EndDate { get; set; }

            public List<FinancialAccountInfo> ActiveChildAccounts { get; set; }

            public override string ToString()
            {
                return Id.ToString();
            }
        }
        */

        /// <summary>
        /// Gets or sets the selected account ids (including the ones where an amount is not specified)
        /// <para>
        /// <b>NOTE</b>: This has special logic if <see cref="UseAccountCampusMappingLogic"/> is enabled.
        /// </para>
        /// If so, the account(s) that the user selects <seealso cref="SelectedAccountIds"/> will be determined as follows:
        /// <list type="number">
        ///   <item>If the selected account is not associated with a campus, the Selected Account will be the first matching active child account that is associated with the selected campus.</item>
        ///   <item>If the selected account is not associated with a campus, but there are no active child accounts for the selected campus, the parent account (the one the user sees) will be returned.</item>
        ///   <item>If the selected account is associated with a campus, that account will be returned regardless of campus selection (and it won't use the child account logic)</item>
        /// </list>
        /// </summary>
        public int[] SelectedAccountIds
        {
            get
            {
                EnsureChildControls();
                int? campusId = this.CampusId;
                if ( this.AmountEntryMode == AccountAmountEntryMode.MultipleAccounts )
                {
                    if ( campusId.HasValue )
                    {
                        HashSet<int> selectedAccountIds = new HashSet<int>();
                        foreach ( var displayedAccount in this.SelectableAccountIds.Select( a => FinancialAccountCache.Get( a ) ).ToList() )
                        {
                            var returnedAccountId = GetBestMatchingAccountIdForCampusFromDisplayedAccount( campusId.Value, displayedAccount );
                            selectedAccountIds.Add( returnedAccountId );
                        }

                        return selectedAccountIds.ToArray();
                    }
                    else
                    {
                        return SelectableAccountIds;
                    }
                }
                else
                {
                    int? displayedAccountId = _ddlAccountSingle.SelectedValueAsId();
                    if ( displayedAccountId.HasValue )
                    {
                        var displayedAccount = FinancialAccountCache.Get( displayedAccountId.Value );
                        int selectedAccountId;
                        if ( campusId.HasValue )
                        {
                            selectedAccountId = GetBestMatchingAccountIdForCampusFromDisplayedAccount( campusId.Value, displayedAccount );
                        }
                        else
                        {
                            selectedAccountId = displayedAccountId.Value;
                        }

                        return new int[1] { selectedAccountId };
                    }

                    return new int[0];
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected account identifier for <seealso cref="AccountAmountEntryMode.SingleAccount"/> mode. For <seealso cref="AccountAmountEntryMode.MultipleAccounts"/> mode, use <seealso cref="AccountAmounts"/>
        /// </summary>
        /// <value>
        /// The selected account identifier.
        /// </value>
        public int? SelectedAccountId
        {
            get
            {
                return this.SelectedAccountIds.FirstOrDefault();
            }

            set
            {
                SetCampusAndDisplayedAccountFromSelectedAccount( value );
            }
        }

        /// <summary>
        /// Gets or sets the selected amount for <seealso cref="AccountAmountEntryMode.SingleAccount"/> mode. For <seealso cref="AccountAmountEntryMode.MultipleAccounts"/> mode, use <seealso cref="AccountAmounts"/>
        /// </summary>
        /// <value>
        /// The selected amount.
        /// </value>
        public decimal? SelectedAmount
        {
            get
            {
                EnsureChildControls();
                return _cbAmountAccountSingle.Value;
            }

            set
            {
                EnsureChildControls();
                _cbAmountAccountSingle.Value = value;
            }
        }

        /// <summary>
        /// Determines whether the amount(s) entered is valid
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is valid amount selected]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValidAmountSelected()
        {
            bool isAllPositiveAmount = false;
            decimal? totalAmount;
            if ( this.AmountEntryMode == AccountAmountEntryMode.MultipleAccounts )
            {
                isAllPositiveAmount = !this.AccountAmounts.Any( a => a.Amount < 0 );
                totalAmount = this.AccountAmounts.Sum( a => a.Amount ?? 0.00M );
            }
            else
            {
                isAllPositiveAmount = this.SelectedAmount >= 0;
                totalAmount = this.SelectedAmount;
            }

            // don't allow 0.00 and limit to $21,474,836.47, just in case browser validation doesn't limit it
            const int maxAmountCents = int.MaxValue;
            decimal maxAmountDollars = maxAmountCents / 100;
            return isAllPositiveAmount && totalAmount.HasValue && totalAmount.Value != 0.00M && totalAmount < maxAmountDollars;
        }

        /// <summary>
        /// Sets the campus and displayed account from selected account.
        /// </summary>
        /// <param name="selectedAccountId">The selected account identifier.</param>
        private void SetCampusAndDisplayedAccountFromSelectedAccount( int? selectedAccountId )
        {
            FinancialAccountCache selectedAccount;
            if ( selectedAccountId.HasValue )
            {
                selectedAccount = FinancialAccountCache.Get( selectedAccountId.Value );
            }
            else
            {
                selectedAccount = null;
            }

            int? campusId = selectedAccount?.CampusId;
            var displayedAccount = GetDisplayedAccountFromSelectedAccount( selectedAccount );

            this.CampusId = campusId;

            if ( displayedAccount != null )
            {
                EnsureChildControls();
                BindAccounts();
                _ddlAccountSingle.SetValue( displayedAccount.Id );
            }
        }

        /// <summary>
        /// Gets the displayed account from selected account based on the <seealso cref="UseAccountCampusMappingLogic"/>
        /// setting.
        /// <para>See special logic on <seealso cref="SelectedAccountIds"/></para>
        /// </summary>
        /// <param name="selectedAccount">The selected account.</param>
        /// <returns></returns>
        private FinancialAccountCache GetDisplayedAccountFromSelectedAccount( FinancialAccountCache selectedAccount )
        {
            if ( !UseAccountCampusMappingLogic )
            {
                return selectedAccount;
            }

            int? selectedAccountId = selectedAccount?.Id;

            FinancialAccountCache displayedAccount;
            if ( selectedAccountId.HasValue && this.SelectableAccountIds.Contains( selectedAccountId.Value ) )
            {
                // if the selected account is one of the selectable accounts (displayed accounts) set the displayed account to the selected account (instead of displaying the parent account)
                displayedAccount = selectedAccount;
            }
            else if ( selectedAccount?.ParentAccount != null )
            {
                // selected account has a parent account, so display the parent account
                displayedAccount = selectedAccount.ParentAccount;
            }
            else
            {
                // Selected account doesn't have a parent account and isn't one of the selectable accounts, so just keep the selected account as the displayed account
                // However, it won't up displaying since it isn't one of the selectable accounts
                displayedAccount = selectedAccount;
            }

            return displayedAccount;
        }

        /// <summary>
        /// Gets the best matching AccountId for selected campus from the displayed account.
        /// <para>See special logic on <seealso cref="SelectedAccountIds"/></para>
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="displayedAccount">The displayed account.</param>
        /// <returns></returns>
        private int GetBestMatchingAccountIdForCampusFromDisplayedAccount( int campusId, FinancialAccountCache displayedAccount )
        {
            if ( !UseAccountCampusMappingLogic )
            {
                return displayedAccount.Id;
            }

            if ( displayedAccount.CampusId.HasValue && displayedAccount.CampusId == campusId )
            {
                // displayed account is directly associated with selected campusId, so return it
                return displayedAccount.Id;
            }
            else
            {
                // displayed account doesn't have a campus (or belongs to another campus). Find first active matching child account
                var firstMatchingChildAccount = displayedAccount.ChildAccounts.Where( a => a.IsActive ).FirstOrDefault( a => a.CampusId.HasValue && a.CampusId == campusId );
                if ( firstMatchingChildAccount != null )
                {
                    // one of the child accounts is associated with the campus so, return the child account
                    return firstMatchingChildAccount.Id;
                }
                else
                {
                    // none of the child accounts is associated with the campus so, return the displayed account
                    return displayedAccount.Id;
                }
            }
        }

        /// <summary>
        /// The campusId that was set as the known Campus
        /// </summary>
        private int? knownCampusId = null;

        /// <summary>
        /// When set, sets the CampusId to set the known/default Campus that should be used. If this is set, <seealso cref="AskForCampusIfKnown"/> can optionally be set to false to hide the campus selector and to prevent changing the campus.
        /// When get, gets the selected CampusId
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        public int? CampusId
        {
            get
            {
                EnsureChildControls();
                if ( this.AmountEntryMode == AccountAmountEntryMode.MultipleAccounts )
                {
                    return _ddlMultiAccountCampus.SelectedValueAsId();
                }
                else
                {
                    return _ddlSingleAccountCampus.SelectedValueAsId();
                }
            }

            set
            {
                EnsureChildControls();

                _ddlMultiAccountCampus.SetValue( value );
                _ddlSingleAccountCampus.SetValue( value );

                knownCampusId = value;

                SetCampusVisibility();
            }
        }

        /// <summary>
        /// Sets the campus visibility.
        /// </summary>
        private void SetCampusVisibility()
        {
            bool showCampusPicker = ( ( knownCampusId == null ) || this.AskForCampusIfKnown ) && GetCampusList().Count > 1;

            _ddlMultiAccountCampus.Visible = showCampusPicker;
            _ddlSingleAccountCampus.Visible = showCampusPicker;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Campus Prompt should be shown when <seealso cref="CampusId"/> is set.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [ask for campus if known]; otherwise, <c>false</c>.
        /// </value>
        public bool AskForCampusIfKnown
        {
            get => ViewState["AskForCampusIfKnown"] as bool? ?? true;

            set
            {
                ViewState["AskForCampusIfKnown"] = value;
                EnsureChildControls();

                SetCampusVisibility();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include Inactive Campus (default is false)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include inactive campuses]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactiveCampuses
        {
            get => ViewState["IncludeInactiveCampuses"] as bool? ?? false;
            set
            {
                ViewState["IncludeInactiveCampuses"] = value;
                LoadCampuses();
            }
        }

        /// <summary>
        /// Set this to limit campuses based on campus type
        /// </summary>
        /// <value>
        /// The included campus type ids.
        /// </value>
        public int[] IncludedCampusTypeIds
        {
            get => ViewState["IncludedCampusTypeIds"] as int[];
            set
            {
                ViewState["IncludedCampusTypeIds"] = value;
                LoadCampuses();
            }
        }

        /// <summary>
        /// Set this to limit campuses based on campus status
        /// </summary>
        /// <value>
        /// The included campus status ids.
        /// </value>
        public int[] IncludedCampusStatusIds
        {
            get => ViewState["IncludedCampusStatusIds"] as int[];
            set
            {
                ViewState["IncludedCampusStatusIds"] = value;
                LoadCampuses();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Campus and Account DropDowns will fire a PostBack
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic post back]; otherwise, <c>false</c>.
        /// </value>
        public bool AutoPostBack
        {
            get
            {
                EnsureChildControls();
                return _ddlAccountSingle.AutoPostBack;
            }

            set
            {
                EnsureChildControls();
                _ddlAccountSingle.AutoPostBack = value;
                _ddlSingleAccountCampus.AutoPostBack = value;
                _ddlMultiAccountCampus.AutoPostBack = value;
            }
        }

        /// <summary>
        /// Gets or CSS class that should be applied to the amount input when in SingleAccount mode
        /// </summary>
        /// <value>
        /// The amount entry single CSS class.
        /// </value>
        public string AmountEntrySingleCssClass
        {
            get
            {
                EnsureChildControls();
                return _pnlAccountAmountEntrySingle.CssClass;
            }

            set
            {
                EnsureChildControls();
                _pnlAccountAmountEntrySingle.CssClass = value;
            }
        }

        #endregion Properties

        #region private methods

        /// <summary>
        /// Synchronizes the currency boxes currency codes.
        /// </summary>
        private void SyncCurrencyBoxesCurrencyCodes()
        {
            if ( AmountEntryMode == AccountAmountEntryMode.MultipleAccounts )
            {
                foreach ( var rptItem in _rptPromptForAccountAmountsMulti.Items.OfType<RepeaterItem>() )
                {
                    var cbAccountAmountMulti = rptItem.FindControl( RepeaterControlIds.ID_cbAccountAmountMulti ) as CurrencyBox;
                    cbAccountAmountMulti.CurrencyCodeDefinedValueId = CurrencyCodeDefinedValueId;
                }
            }
            else
            {
                _cbAmountAccountSingle.CurrencyCodeDefinedValueId = CurrencyCodeDefinedValueId;
            }
        }

        /// <summary>
        /// Loads the campuses.
        /// </summary>
        private void LoadCampuses()
        {
            _ddlSingleAccountCampus.Items.Clear();
            _ddlMultiAccountCampus.Items.Clear();
            var campusList = GetCampusList();

            _ddlSingleAccountCampus.Items.Add( new ListItem() );
            _ddlMultiAccountCampus.Items.Add( new ListItem() );

            foreach ( var campus in campusList.OrderBy( a => a.Order ) )
            {
                _ddlSingleAccountCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
                _ddlMultiAccountCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
            }

            if ( CampusCache.All( this.IncludeInactiveCampuses ).Count == 1 )
            {
                _ddlSingleAccountCampus.Visible = false;
                _ddlMultiAccountCampus.Visible = false;
            }

            if ( campusList.Count == 1 && !this.CampusId.HasValue )
            {
                // If we have just one campus after filtering and out CampusId is null, set the only Campus
                // as the CampusId.
                this.CampusId = campusList[0].Id;
            }

            // This will select the value in both campus ddls
            if ( knownCampusId.HasValue )
            {
                this.CampusId = knownCampusId;
            }
        }

        /// <summary>
        /// Gets the campus list after filtering based on the selected Campus Types and Statuses.
        /// </summary>
        /// <returns></returns>
        private List<CampusCache> GetCampusList()
        {
            var campusList = CampusCache.All( this.IncludeInactiveCampuses );

            if ( IncludedCampusTypeIds?.Any() == true )
            {
                campusList = campusList.Where( a => a.CampusTypeValueId.HasValue && IncludedCampusTypeIds.Contains( a.CampusTypeValueId.Value ) ).ToList();
            }

            if ( IncludedCampusStatusIds?.Any() == true )
            {
                campusList = campusList.Where( a => a.CampusStatusValueId.HasValue && IncludedCampusStatusIds.Contains( a.CampusStatusValueId.Value ) ).ToList();
            }

            return campusList;
        }

        /// <summary>
        /// Binds the accounts.
        /// </summary>
        private void BindAccounts()
        {
            var rockContext = new RockContext();
            var selectableAccountIds = this.SelectableAccountIds.ToList();

            IQueryable<FinancialAccount> accountsQry;
            var financialAccountService = new FinancialAccountService( rockContext );

            if ( selectableAccountIds.Any() )
            {
                accountsQry = financialAccountService.GetByIds( selectableAccountIds );
            }
            else
            {
                accountsQry = financialAccountService.Queryable();
            }

            // limit to active, public accounts, and don't include ones that aren't within the date range
            accountsQry = accountsQry.Where( f =>
                    f.IsActive &&
                    ( f.StartDate == null || f.StartDate <= RockDateTime.Today ) &&
                    ( f.EndDate == null || f.EndDate >= RockDateTime.Today ) );

            // Only allow Private accounts from the provided selectableAccountIds.
            if ( !AllowPrivateSelectableAccounts || !selectableAccountIds.Any() )
            {
                accountsQry = accountsQry.Where( f => f.IsPublic == true );
            }

            var accountsList = accountsQry.OrderBy( f => f.Order ).AsNoTracking().ToList();

            _ddlAccountSingle.Items.Clear();

            string accountHeaderTemplate = AccountHeaderTemplate;
            if ( accountHeaderTemplate.IsNullOrWhiteSpace() )
            {
                accountHeaderTemplate = "{{ Account.PublicName }}";
            }

            if ( OrderBySelectableAccountsIndex )
            {
                accountsList = accountsList.OrderBy( x => selectableAccountIds.IndexOf( x.Id ) ).ToList();
            }

            foreach ( var account in accountsList )
            {
                var mergeFields = LavaHelper.GetCommonMergeFields( null, null, new CommonMergeFieldsOptions() );
                mergeFields.Add( "Account", account );
                var accountAmountLabel = accountHeaderTemplate.ResolveMergeFields( mergeFields );
                _ddlAccountSingle.Items.Add( new ListItem( accountAmountLabel, account.Id.ToString() ) );
            }

            _ddlAccountSingle.SetValue( accountsList.FirstOrDefault() );

            _rptPromptForAccountAmountsMulti.DataSource = accountsList;
            _rptPromptForAccountAmountsMulti.DataBind();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets or sets a array of each selected AccountId and its Amount
        /// </summary>
        /// <value>
        /// The account amounts.
        /// </value>
        public AccountIdAmount[] AccountAmounts
        {
            get
            {
                EnsureChildControls();

                var resultAccountAmounts = new List<AccountIdAmount>();
                var selectedCampusId = this.CampusId ?? 0;

                if ( AmountEntryMode == AccountAmountEntryMode.MultipleAccounts )
                {
                    foreach ( var item in _rptPromptForAccountAmountsMulti.Items.OfType<RepeaterItem>() )
                    {
                        var hfAccountAmountMultiAccountId = item.FindControl( RepeaterControlIds.ID_hfAccountAmountMultiAccountId ) as HiddenField;
                        var displayedAccountId = hfAccountAmountMultiAccountId.Value.AsInteger();
                        var displayedAccount = FinancialAccountCache.Get( displayedAccountId );
                        var returnedAccountId = this.GetBestMatchingAccountIdForCampusFromDisplayedAccount( selectedCampusId, displayedAccount );
                        var cbAccountAmountMulti = item.FindControl( RepeaterControlIds.ID_cbAccountAmountMulti ) as CurrencyBox;
                        resultAccountAmounts.Add( new AccountIdAmount( returnedAccountId, cbAccountAmountMulti.Value ) );
                    }
                }
                else
                {
                    var displayedAccountId = _ddlAccountSingle.SelectedValue.AsInteger();
                    var displayedAccount = FinancialAccountCache.Get( displayedAccountId );
                    var returnedAccountId = this.GetBestMatchingAccountIdForCampusFromDisplayedAccount( selectedCampusId, displayedAccount );

                    resultAccountAmounts.Add( new AccountIdAmount( returnedAccountId, _cbAmountAccountSingle.Value ) );
                }

                return resultAccountAmounts.ToArray();
            }

            set
            {
                EnsureChildControls();

                if ( AmountEntryMode == AccountAmountEntryMode.MultipleAccounts )
                {
                    BindAccounts();
                    foreach ( var selectedAccountAmount in value )
                    {
                        // get the best matching accountId for the specified selectedAccountId
                        var displayedAccountId = GetDisplayedAccountFromSelectedAccount( FinancialAccountCache.Get( selectedAccountAmount.AccountId ) )?.Id;
                        decimal? selectedAmount = selectedAccountAmount.Amount;

                        // find the repeater item for the displayedAccountId then set the displayed amount for that account
                        foreach ( var rptItem in _rptPromptForAccountAmountsMulti.Items.OfType<RepeaterItem>() )
                        {
                            var hfAccountAmountMultiAccountId = rptItem.FindControl( RepeaterControlIds.ID_hfAccountAmountMultiAccountId ) as HiddenField;
                            int itemAccountId = hfAccountAmountMultiAccountId.Value.AsInteger();
                            if ( itemAccountId == displayedAccountId )
                            {
                                var cbAccountAmountMulti = rptItem.FindControl( RepeaterControlIds.ID_cbAccountAmountMulti ) as CurrencyBox;
                                cbAccountAmountMulti.Value = selectedAmount;
                                cbAccountAmountMulti.ReadOnly = selectedAccountAmount.ReadOnly;
                                cbAccountAmountMulti.CurrencyCodeDefinedValueId = CurrencyCodeDefinedValueId;
                            }
                        }
                    }
                }
                else
                {
                    var selectedAccountAmount = value.FirstOrDefault();
                    if ( selectedAccountAmount == null )
                    {
                        // an empty dictionary of a selectedAccountAmount was specified so assume they meant to set the selected amount to null
                        _cbAmountAccountSingle.Value = null;
                        return;
                    }

                    var displayedAccountId = GetDisplayedAccountFromSelectedAccount( FinancialAccountCache.Get( selectedAccountAmount.AccountId ) )?.Id;
                    _ddlAccountSingle.SetValue( displayedAccountId );
                    _cbAmountAccountSingle.Value = selectedAccountAmount.Amount;
                    _cbAmountAccountSingle.ReadOnly = selectedAccountAmount.ReadOnly;
                    _cbAmountAccountSingle.CurrencyCodeDefinedValueId = CurrencyCodeDefinedValueId;
                }
            }
        }

        #endregion

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            _pnlAccountAmountEntrySingle.Visible = AmountEntryMode == AccountAmountEntryMode.SingleAccount;
            _pnlAccountAmountEntryMulti.Visible = AmountEntryMode == AccountAmountEntryMode.MultipleAccounts;
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            /* Single Account Mode */

            _pnlAccountAmountEntrySingle = new Panel() { CssClass = "campus-account-amount-picker account-amount-single-entry form-group" };
            _pnlAccountAmountEntrySingle.ID = "_pnlAccountAmountEntrySingle";

            Controls.Add( _pnlAccountAmountEntrySingle );

            _cbAmountAccountSingle = new CurrencyBox();
            _cbAmountAccountSingle.ID = "_cbAmountAccountSingle";
            _cbAmountAccountSingle.CssClass = "js-amount-input amount-input";
            _cbAmountAccountSingle.NumberType = ValidationDataType.Currency;
            _cbAmountAccountSingle.MaximumValue = int.MaxValue.ToString();
            _cbAmountAccountSingle.MinimumValue = "0";
            _cbAmountAccountSingle.CurrencyCodeDefinedValueId = CurrencyCodeDefinedValueId;

            // set max length to prevent input from accepting more than $99,999,999.99 (99 million dollars), this will help prevent an Int32 overflow if amount is stored in cents
            // However, browsers don't seem to enforce this, and we really want to limit to int.MaxValue so we'll also check in validation
            _cbAmountAccountSingle.Attributes["maxlength"] = "14";
            _pnlAccountAmountEntrySingle.Controls.Add( _cbAmountAccountSingle );

            var pnlSingleCampusDiv = new Panel() { CssClass = "campus-dropdown " };
            _pnlAccountAmountEntrySingle.Controls.Add( pnlSingleCampusDiv );

            _ddlSingleAccountCampus = new RockDropDownList();
            _ddlSingleAccountCampus.ID = "_ddlSingleAccountCampus";
            _ddlSingleAccountCampus.Label = "Campus";
            _ddlSingleAccountCampus.CssClass = "single-account-campus";
            _ddlSingleAccountCampus.SelectedIndexChanged += _ddlCampus_SelectedIndexChanged;
            pnlSingleCampusDiv.Controls.Add( _ddlSingleAccountCampus );

            var pnlAccountSingleDiv = new Panel() { CssClass = "account-dropdown" };
            _pnlAccountAmountEntrySingle.Controls.Add( pnlAccountSingleDiv );

            _ddlAccountSingle = new RockDropDownList();
            _ddlAccountSingle.ID = "_ddlAccountSingle";
            _ddlAccountSingle.CssClass = "account-single";
            _ddlAccountSingle.SelectedIndexChanged += _ddlAccountSingle_SelectedIndexChanged;
            pnlAccountSingleDiv.Controls.Add( _ddlAccountSingle );

            /* Multi Account Mode*/

            _pnlAccountAmountEntryMulti = new Panel() { CssClass = "campus-account-amount-picker account-amount-multi-entry form-group" };
            _pnlAccountAmountEntryMulti.ID = "_pnlAccountAmountEntryMulti";
            Controls.Add( _pnlAccountAmountEntryMulti );

            _rptPromptForAccountAmountsMulti = new Repeater();
            _rptPromptForAccountAmountsMulti.ID = "_rptPromptForAccountAmountsMulti";
            _rptPromptForAccountAmountsMulti.ItemDataBound += _rptPromptForAccountAmountsMulti_ItemDataBound;

            _rptPromptForAccountAmountsMulti.ItemTemplate = new PromptForAccountsMultiTemplate();
            _pnlAccountAmountEntryMulti.Controls.Add( _rptPromptForAccountAmountsMulti );

            _ddlMultiAccountCampus = new RockDropDownList();
            _ddlMultiAccountCampus.ID = "_ddlMultiAccountCampus";
            _ddlMultiAccountCampus.CssClass = "multi-account-campus";
            _ddlMultiAccountCampus.SelectedIndexChanged += _ddlCampus_SelectedIndexChanged;
            _ddlMultiAccountCampus.Label = "Campus";
            _pnlAccountAmountEntryMulti.Controls.Add( _ddlMultiAccountCampus );

            LoadCampuses();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _ddlMultiAccountCampus or _ddlSingleAccountCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _ddlCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            CampusChanged?.Invoke( this, new EventArgs() );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _ddlAccountSingle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _ddlAccountSingle_SelectedIndexChanged( object sender, EventArgs e )
        {
            AccountChanged?.Invoke( this, new EventArgs() );
        }

        /// <summary>
        ///
        /// </summary>
        private class PromptForAccountsMultiTemplate : ITemplate
        {
            /// <summary>
            /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
            /// </summary>
            /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
            public void InstantiateIn( Control container )
            {
                var itemTemplateControl = new Panel();
                itemTemplateControl.Controls.Add(
                    new HiddenField
                    {
                        ID = RepeaterControlIds.ID_hfAccountAmountMultiAccountId
                    } );

                var currencyBox = new CurrencyBox
                {
                    ID = RepeaterControlIds.ID_cbAccountAmountMulti,
                    CssClass = "amount-input account-amount-multi js-amount-input",
                    NumberType = ValidationDataType.Currency,
                    MaximumValue = int.MaxValue.ToString(),
                    MinimumValue = "0"
                };

                // set max length to prevent input from accepting more than $99,999,999.99 (99 million dollars), this will help prevent an Int32 overflow if amount is stored in cents
                // However, browsers don't seem to enforce this, and we really want to limit to int.MaxValue so we'll also check in validation
                currencyBox.Attributes["maxlength"] = "14";
                itemTemplateControl.Controls.Add( currencyBox );

                container.Controls.Add( itemTemplateControl );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the _rptPromptForAccountAmountsMulti control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        private void _rptPromptForAccountAmountsMulti_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var financialAccount = e.Item.DataItem as FinancialAccount;
            if ( financialAccount == null )
            {
                return;
            }

            var hfAccountAmountMultiAccountId = e.Item.FindControl( RepeaterControlIds.ID_hfAccountAmountMultiAccountId ) as HiddenField;
            var nbAccountAmountMulti = e.Item.FindControl( RepeaterControlIds.ID_cbAccountAmountMulti ) as CurrencyBox;
            nbAccountAmountMulti.CurrencyCodeDefinedValueId = CurrencyCodeDefinedValueId;

            hfAccountAmountMultiAccountId.Value = financialAccount.Id.ToString();

            string accountHeaderTemplate = AccountHeaderTemplate;
            if ( accountHeaderTemplate.IsNullOrWhiteSpace() )
            {
                accountHeaderTemplate = "{{ Account.PublicName }}";
            }

            var mergeFields = LavaHelper.GetCommonMergeFields( null, null, new CommonMergeFieldsOptions() );
            mergeFields.Add( "Account", financialAccount );
            var accountAmountLabel = accountHeaderTemplate.ResolveMergeFields( mergeFields );

            nbAccountAmountMulti.Label = accountAmountLabel;
        }

        #region Events

        /// <summary>
        /// Occurs when [account changed].
        /// </summary>
        public event EventHandler AccountChanged;

        /// <summary>
        /// Occurs when [campus changed].
        /// </summary>
        public event EventHandler CampusChanged;

        #endregion Events
    }
}
