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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Rock.Net;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for SelectAccountsPage.xaml
    /// </summary>
    public partial class SelectAccountsPage : Page
    {
        /// <summary>
        /// The _rock rest client
        /// </summary>
        private RockRestClient _rockRestClient;

        /// <summary>
        /// All of the Financial Accounts from Rock
        /// </summary>
        private List<Rock.Client.FinancialAccount> _financialAccountList = null;
        private List<Rock.Client.DefinedValue> _currencyTypes = null;

        /// <summary>
        /// The selected cash account ids
        /// </summary>
        private List<int> _selectedAccountIds = null;


        /// <summary>
        /// Initializes a new instance of the <see cref="SelectAccountsPage"/> class.
        /// </summary>
        public SelectAccountsPage()
        {
            InitializeComponent();

            RockConfig rockConfig = RockConfig.Load();

            _rockRestClient = new RockRestClient( rockConfig.RockBaseUrl );
            _rockRestClient.Login( rockConfig.Username, rockConfig.Password );

            _financialAccountList = _rockRestClient.GetData<List<Rock.Client.FinancialAccount>>( "api/FinancialAccounts" );
            var currencyDefinedType = _rockRestClient.GetDataByGuid<Rock.Client.DefinedType>( "api/DefinedTypes", Rock.Client.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid() );
            _currencyTypes = _rockRestClient.GetData<List<Rock.Client.DefinedValue>>( $"api/DefinedValues?$filter=DefinedTypeId eq {currencyDefinedType.Id}" );
            lstCashCurrencyTypes.Items.Clear();
            var defaultUnselectedCashCurrencyGuids = new Guid[] {
                Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_NONCASH.AsGuid()
            };

            var defaultNonCashCurrencyGuids = new Guid[] {
                Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_NONCASH.AsGuid()
            };

            // Default Cash Currency Types to 
            foreach ( var currencyType in _currencyTypes )
            {

                var cashCurrencyCheckbox = new CheckBox { Tag = currencyType.Id, Content = currencyType.Value };
                if ( ReportOptions.Current.CurrencyTypeIdsCash != null )
                {
                    cashCurrencyCheckbox.IsChecked = ReportOptions.Current.CurrencyTypeIdsCash.Contains( currencyType.Id );
                }
                else
                {
                    cashCurrencyCheckbox.IsChecked = !defaultUnselectedCashCurrencyGuids.Contains( currencyType.Guid );
                }

                lstCashCurrencyTypes.Items.Add( cashCurrencyCheckbox );

                var nonCashCurrencyCheckbox = new CheckBox { Tag = currencyType.Id, Content = currencyType.Value };
                if ( ReportOptions.Current.CurrencyTypeIdsNonCash != null )
                {
                    nonCashCurrencyCheckbox.IsChecked = ReportOptions.Current.CurrencyTypeIdsNonCash.Contains( currencyType.Id );
                }
                else
                {
                    nonCashCurrencyCheckbox.IsChecked = defaultNonCashCurrencyGuids.Contains( currencyType.Guid );
                }

                lstNonCashCurrencyTypes.Items.Add( nonCashCurrencyCheckbox );
            }

            // default to the currently configured CashAccountsId (if configured), or default to all
            _selectedAccountIds = ReportOptions.Current.TransactionAccountIds ?? _financialAccountList.Select( a => a.Id ).ToList();

            cbShowInactive.IsChecked = rockConfig.ShowInactiveAccounts;
            cbShowTaxDeductible.IsChecked = rockConfig.ShowTaxDeductibleAccounts;
            cbShowNonTaxDeductible.IsChecked = rockConfig.ShowNonTaxDeductibleAccounts;

            ApplyFilter();

            lblAccountsCurrencyTypesWarning.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Applies the filter.
        /// </summary>
        private void ApplyFilter()
        {
            if ( _financialAccountList == null )
            {
                return;
            }

            IEnumerable<Rock.Client.FinancialAccount> displayedAccounts = _financialAccountList.OrderBy( a => a.Order ).ThenBy( a => a.PublicName );
            if ( cbShowInactive.IsChecked == false )
            {
                displayedAccounts = displayedAccounts.Where( a => a.IsActive == true );
            }

            if ( cbShowTaxDeductible.IsChecked == false && cbShowNonTaxDeductible.IsChecked == false )
            {
                displayedAccounts = displayedAccounts.Where( a => false );
            }
            else if ( cbShowTaxDeductible.IsChecked == true && cbShowNonTaxDeductible.IsChecked == true )
            {
                displayedAccounts = displayedAccounts.Where( a => true );
            }
            else
            {
                displayedAccounts = displayedAccounts.Where( a => a.IsTaxDeductible == cbShowTaxDeductible.IsChecked == true );
            }

            lstAccounts.Items.Clear();
            foreach ( var account in displayedAccounts )
            {
                lstAccounts.Items.Add( new CheckBox { Tag = account.Id, Content = account.PublicName, IsChecked = _selectedAccountIds.Contains( account.Id ) } );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnNext_Click( object sender, RoutedEventArgs e )
        {
            if ( SaveChanges( true ) )
            {
                var nextPage = new SelectPledgeAccountsPage();
                this.NavigationService.Navigate( nextPage );
            }
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="showWarnings">if set to <c>true</c> [show warnings].</param>
        /// <returns></returns>
        private bool SaveChanges( bool showWarnings )
        {
            var rockConfig = RockConfig.Load();
            rockConfig.ShowInactiveAccounts = cbShowInactive.IsChecked == true;
            rockConfig.ShowTaxDeductibleAccounts = cbShowTaxDeductible.IsChecked == true;
            rockConfig.ShowNonTaxDeductibleAccounts = cbShowNonTaxDeductible.IsChecked == true;

            ReportOptions.Current.TransactionAccountIds = lstAccounts.Items.OfType<CheckBox>().Where( a => a.IsChecked == true ).Select( s => ( int ) s.Tag ).ToList();
            ReportOptions.Current.CurrencyTypeIdsCash = lstCashCurrencyTypes.Items.OfType<CheckBox>().Where( a => a.IsChecked == true ).Select( s => ( int ) s.Tag ).ToList();
            ReportOptions.Current.CurrencyTypeIdsNonCash = lstNonCashCurrencyTypes.Items.OfType<CheckBox>().Where( a => a.IsChecked == true ).Select( s => ( int ) s.Tag ).ToList();
            var currencySelected = ReportOptions.Current.CurrencyTypeIdsCash.Any() || ReportOptions.Current.CurrencyTypeIdsNonCash.Any();
            if ( !ReportOptions.Current.TransactionAccountIds.Any() || !currencySelected )
            {
                if ( showWarnings )
                {
                    if ( !ReportOptions.Current.TransactionAccountIds.Any() && !currencySelected )
                    {
                        lblAccountsCurrencyTypesWarning.Content = "Please select at least one account and currency type.";
                    }
                    else if ( !ReportOptions.Current.TransactionAccountIds.Any() )
                    {
                        lblAccountsCurrencyTypesWarning.Content = "Please select at least one account.";
                    }
                    else if ( !currencySelected )
                    {
                        lblAccountsCurrencyTypesWarning.Content = "Please select at least one currency type.";
                    }

                    lblAccountsCurrencyTypesWarning.Visibility = Visibility.Visible;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Handles the Click event of the btnSelectAllForCashGifts and btnSelectAllForNonCashGifts controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectAll_Click( object sender, RoutedEventArgs e )
        {
            lstAccounts.Items.OfType<CheckBox>().ToList().ForEach( a => a.IsChecked = true );
        }

        /// <summary>
        /// Handles the Click event of the btnSelectNoneForCashGifts and btnSelectNoneForNonCashGifts controls
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectNone_Click( object sender, RoutedEventArgs e )
        {
            lstAccounts.Items.OfType<CheckBox>().ToList().ForEach( a => a.IsChecked = false );
        }

        /// <summary>
        /// Handles the Click event of the btnPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnPrev_Click( object sender, RoutedEventArgs e )
        {
            SaveChanges( false );
            this.NavigationService.GoBack();
        }

        /// <summary>
        /// Handles the Checked and UnChecked event of the filter checkboxes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cbFilter_Changed( object sender, RoutedEventArgs e )
        {
            // if the page isn't loaded yet, skip
            if ( this.IsLoaded )
            {
                _selectedAccountIds = lstAccounts.Items.OfType<CheckBox>().Where( a => a.IsChecked == true ).Select( a => ( int ) a.Tag ).ToList();
                ApplyFilter();
            }
        }
    }
}
