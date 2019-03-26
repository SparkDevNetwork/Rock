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
using Rock.Model;
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

        // NOTE: we want a stock asp.net TextBox because this will have special styling
        private TextBox _nbAmountAccountSingle;

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
            /// The control ID for the nbAccountAmountMulti currency box
            /// </summary>
            internal const string ID_nbAccountAmountMulti = "nbAccountAmountMulti";
        }

        #endregion private constants

        #region Properties

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
        /// Gets the financial accounts lookup.
        /// </summary>
        /// <value>
        /// The financial accounts lookup.
        /// </value>
        private Dictionary<int, FinancialAccountInfo> FinancialAccountsLookup
        {
            get
            {
                if ( _financialAccountsCache == null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        _financialAccountsCache = new FinancialAccountService( rockContext ).Queryable().AsNoTracking()
                            .Select(a => new
                            {
                                a.Id,
                                a.ParentAccountId,
                                a.CampusId,
                                a.IsActive,
                                a.StartDate,
                                a.EndDate
                            } )
                            .ToDictionary(
                                k => k.Id,
                                v => new FinancialAccountInfo
                                {
                                    Id = v.Id,
                                    ParentAccountId = v.ParentAccountId,
                                    CampusId = v.CampusId,
                                    IsActive = v.IsActive,
                                    StartDate = v.StartDate,
                                    EndDate = v.EndDate
                                } );

                        foreach ( var account in _financialAccountsCache.Values )
                        {
                            account.ActiveChildAccounts = _financialAccountsCache.Values
                                .Where( a =>
                                    a.ParentAccountId == account.Id
                                    && a.IsActive
                                    && ( a.StartDate == null || a.StartDate <= RockDateTime.Today )
                                    && ( a.EndDate == null || a.EndDate >= RockDateTime.Today ) 
                                )
                                .ToList();
                            if ( account.ParentAccountId.HasValue )
                            {
                                account.ParentAccount = _financialAccountsCache.GetValueOrNull( account.ParentAccountId.Value );
                            }
                        }
                    }
                }

                return _financialAccountsCache;
            }
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

        /// <summary>
        /// Gets or sets the selected account ids (including the ones where an amount is not specified)
        /// Note: This has special logic. The account(s) that the user selects <seealso cref="SelectedAccountIds"/> will be determined as follows:
        ///   1) If the selected account is not associated with a campus, the Selected Account will be the first matching active child account that is associated with the selected campus.
        ///   2) If the selected account is not associated with a campus, but there are no active child accounts for the selected campus, the parent account (the one the user sees) will be returned.
        ///   3) If the selected account is associated with a campus, that account will be returned regardless of campus selection (and it won't use the child account logic)
        /// </summary>
        /// <value>
        /// The selected account ids.
        /// </value>
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
                        foreach ( var displayedAccount in this.SelectableAccountIds.Select( a => FinancialAccountsLookup[a] ).ToList() )
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
                        var displayedAccount = FinancialAccountsLookup[displayedAccountId.Value];
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
                return _nbAmountAccountSingle.Text.AsDecimalOrNull();
            }

            set
            {
                EnsureChildControls();
                _nbAmountAccountSingle.Text = value?.ToString( "N" );
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
            decimal? totalAmount;
            if ( this.AmountEntryMode == AccountAmountEntryMode.MultipleAccounts )
            {
                totalAmount = this.AccountAmounts.Sum( a => a.Amount ?? 0.00M );
            }
            else
            {
                totalAmount = this.SelectedAmount;
            }

            return totalAmount.HasValue && totalAmount.Value != 0.00M;
        }

        /// <summary>
        /// Sets the campus and displayed account from selected account.
        /// </summary>
        /// <param name="selectedAccountId">The selected account identifier.</param>
        private void SetCampusAndDisplayedAccountFromSelectedAccount( int? selectedAccountId )
        {
            FinancialAccountInfo selectedAccount;
            if ( selectedAccountId.HasValue )
            {
                selectedAccount = this.FinancialAccountsLookup.GetValueOrNull( selectedAccountId.Value );
            }
            else
            {
                selectedAccount = null;
            }

            int? campusId = selectedAccount?.CampusId;
            FinancialAccountInfo displayedAccount = GetDisplayedAccountFromSelectedAccount( selectedAccount );

            this.CampusId = campusId;

            if ( displayedAccount != null )
            {
                EnsureChildControls();
                BindAccounts();
                _ddlAccountSingle.SetValue( displayedAccount.Id );
            }
        }

        /// <summary>
        /// Gets the displayed account from selected account.
        /// </summary>
        /// <param name="selectedAccount">The selected account.</param>
        /// <returns></returns>
        private FinancialAccountInfo GetDisplayedAccountFromSelectedAccount( FinancialAccountInfo selectedAccount )
        {
            int? selectedAccountId = selectedAccount?.Id;

            FinancialAccountInfo displayedAccount;
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
        /// Gets the best matching AccountId for selected campus from the displayed account (see logic on <seealso cref="SelectedAccountIds"/>
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="displayedAccount">The displayed account.</param>
        /// <returns></returns>
        private int GetBestMatchingAccountIdForCampusFromDisplayedAccount( int campusId, FinancialAccountInfo displayedAccount )
        {
            if ( displayedAccount.CampusId.HasValue && displayedAccount.CampusId == campusId )
            {
                // displayed account is directly associated with selected campusId, so return it
                return displayedAccount.Id;
            }
            else
            {
                // displayed account doesn't have a campus (or belongs to another campus). Find first active matching child account
                var firstMatchingChildAccount = displayedAccount.ActiveChildAccounts.FirstOrDefault( a => a.CampusId.HasValue && a.CampusId == campusId );
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
            bool showCampusPicker = ( knownCampusId == null ) || this.AskForCampusIfKnown;

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
        /// Loads the campuses.
        /// </summary>
        private void LoadCampuses()
        {
            _ddlSingleAccountCampus.Items.Clear();
            _ddlMultiAccountCampus.Items.Clear();
            foreach ( var campus in CampusCache.All().OrderBy( a => a.Order ) )
            {
                _ddlSingleAccountCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
                _ddlMultiAccountCampus.Items.Add( new ListItem( campus.Name, campus.Id.ToString() ) );
            }
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
                    f.IsPublic.HasValue &&
                    f.IsPublic.Value &&
                    ( f.StartDate == null || f.StartDate <= RockDateTime.Today ) &&
                    ( f.EndDate == null || f.EndDate >= RockDateTime.Today ) )
                .OrderBy( f => f.Order );

            var accountsList = accountsQry.AsNoTracking().ToList();

            _ddlAccountSingle.Items.Clear();

            foreach ( var account in accountsList )
            {
                _ddlAccountSingle.Items.Add( new ListItem( account.PublicName, account.Id.ToString() ) );
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

                if ( AmountEntryMode == AccountAmountEntryMode.MultipleAccounts )
                {
                    foreach ( var item in _rptPromptForAccountAmountsMulti.Items.OfType<RepeaterItem>() )
                    {
                        var hfAccountAmountMultiAccountId = item.FindControl( RepeaterControlIds.ID_hfAccountAmountMultiAccountId ) as HiddenField;
                        var displayedAccountId = hfAccountAmountMultiAccountId.Value.AsInteger();
                        var displayedAccount = FinancialAccountsLookup.GetValueOrNull( displayedAccountId );
                        var returnedAccountId = this.GetBestMatchingAccountIdForCampusFromDisplayedAccount( _ddlMultiAccountCampus.SelectedValue.AsInteger(), displayedAccount );
                        var nbAccountAmountMulti = item.FindControl( RepeaterControlIds.ID_nbAccountAmountMulti ) as CurrencyBox;
                        resultAccountAmounts.Add( new AccountIdAmount( returnedAccountId, nbAccountAmountMulti.Text.AsDecimalOrNull() ) );
                    }
                }
                else
                {
                    var displayedAccountId = _ddlAccountSingle.SelectedValue.AsInteger();
                    var displayedAccount = FinancialAccountsLookup.GetValueOrNull( displayedAccountId );
                    var returnedAccountId = this.GetBestMatchingAccountIdForCampusFromDisplayedAccount( _ddlMultiAccountCampus.SelectedValue.AsInteger(), displayedAccount );

                    resultAccountAmounts.Add( new AccountIdAmount( returnedAccountId, _nbAmountAccountSingle.Text.AsDecimalOrNull() ) );
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
                        var displayedAccountId = GetDisplayedAccountFromSelectedAccount( FinancialAccountsLookup.GetValueOrNull( selectedAccountAmount.AccountId ) )?.Id;
                        decimal? selectedAmount = selectedAccountAmount.Amount;

                        // find the repeater item for the displayedAccountId then set the displayed amount for that account
                        foreach ( var rptItem in _rptPromptForAccountAmountsMulti.Items.OfType<RepeaterItem>() )
                        {
                            var hfAccountAmountMultiAccountId = rptItem.FindControl( RepeaterControlIds.ID_hfAccountAmountMultiAccountId ) as HiddenField;
                            int itemAccountId = hfAccountAmountMultiAccountId.Value.AsInteger();
                            if ( itemAccountId == displayedAccountId )
                            {
                                var nbAccountAmountMulti = rptItem.FindControl( RepeaterControlIds.ID_nbAccountAmountMulti ) as CurrencyBox;
                                nbAccountAmountMulti.Value = selectedAmount;
                                nbAccountAmountMulti.ReadOnly = selectedAccountAmount.ReadOnly;
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
                        _nbAmountAccountSingle.Text = string.Empty;
                        return;
                    }

                    var displayedAccountId = GetDisplayedAccountFromSelectedAccount( FinancialAccountsLookup.GetValueOrNull( selectedAccountAmount.AccountId ) )?.Id;
                    _ddlAccountSingle.SetValue( displayedAccountId );
                    _nbAmountAccountSingle.Text = selectedAccountAmount.Amount?.ToString( "N2" );
                    _nbAmountAccountSingle.ReadOnly = selectedAccountAmount.ReadOnly;
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

            // Special big entry for entering a single dollar amount
            _nbAmountAccountSingle = new TextBox();
            _nbAmountAccountSingle.ID = "_nbAmountAccountSingle";
            _nbAmountAccountSingle.Attributes["placeholder"] = "Enter Amount";
            _nbAmountAccountSingle.Attributes["type"] = "number";
            _nbAmountAccountSingle.CssClass = "amount-input form-control";
            _nbAmountAccountSingle.Attributes["min"] = "0";
            _nbAmountAccountSingle.Attributes["step"] = "0.01";
            _pnlAccountAmountEntrySingle.Controls.Add( _nbAmountAccountSingle );

            var pnlSingleCampusDiv = new Panel() { CssClass = "campus-dropdown " };
            _pnlAccountAmountEntrySingle.Controls.Add( pnlSingleCampusDiv );

            _ddlSingleAccountCampus = new RockDropDownList();
            _ddlSingleAccountCampus.ID = "_ddlSingleAccountCampus";
            _ddlSingleAccountCampus.SelectedIndexChanged += _ddlCampus_SelectedIndexChanged;
            pnlSingleCampusDiv.Controls.Add( _ddlSingleAccountCampus );

            var pnlAccountSingleDiv = new Panel() { CssClass = "account-dropdown" };
            _pnlAccountAmountEntrySingle.Controls.Add( pnlAccountSingleDiv );

            _ddlAccountSingle = new RockDropDownList();
            _ddlAccountSingle.ID = "_ddlAccountSingle";
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
            _ddlMultiAccountCampus.SelectedIndexChanged += _ddlCampus_SelectedIndexChanged;
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

                itemTemplateControl.Controls.Add(
                    new CurrencyBox
                    {
                        ID = RepeaterControlIds.ID_nbAccountAmountMulti,
                        CssClass = "amount-input",
                        NumberType = ValidationDataType.Currency,
                        MinimumValue = "0"
                    } );

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
            var nbAccountAmountMulti = e.Item.FindControl( RepeaterControlIds.ID_nbAccountAmountMulti ) as CurrencyBox;

            hfAccountAmountMultiAccountId.Value = financialAccount.Id.ToString();
            nbAccountAmountMulti.Label = financialAccount.PublicName;
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
