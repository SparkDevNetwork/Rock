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

        /// <summary>
        /// The selected cash account ids
        /// </summary>
        private List<int> _selectedCashAccountIds = null;

        /// <summary>
        /// The selected non cash account ids
        /// </summary>
        private List<int> _selectedNonCashAccountIds = null;

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

            // default to the currently configured CashAccountsId (if configured), or default to all
            _selectedCashAccountIds = ReportOptions.Current.CashAccountIds ?? _financialAccountList.Select( a => a.Id ).ToList();

            // default to the currently configured NonCashAccountsId (if configured), or default to none
            _selectedNonCashAccountIds = ReportOptions.Current.NonCashAccountIds ?? new List<int>();

            cbShowInactive.IsChecked = rockConfig.ShowInactiveAccounts;
            cbShowTaxDeductible.IsChecked = rockConfig.ShowTaxDeductibleAccounts;
            cbShowNonTaxDeductible.IsChecked = rockConfig.ShowNonTaxDeductibleAccounts;

            ApplyFilter();

            lblWarning.Visibility = Visibility.Hidden;
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

            lstCashAccounts.Items.Clear();
            lstNonCashAccounts.Items.Clear();
            foreach ( var account in displayedAccounts )
            {
                lstCashAccounts.Items.Add( new CheckBox { Tag = account.Id, Content = account.PublicName, IsChecked = _selectedCashAccountIds.Contains( account.Id ) } );
                lstNonCashAccounts.Items.Add( new CheckBox { Tag = account.Id, Content = account.PublicName, IsChecked = _selectedNonCashAccountIds.Contains( account.Id ) } );
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

            ReportOptions.Current.CashAccountIds = lstCashAccounts.Items.OfType<CheckBox>().Where( a => a.IsChecked == true ).Select( s => ( int ) s.Tag ).ToList();
            ReportOptions.Current.NonCashAccountIds = lstNonCashAccounts.Items.OfType<CheckBox>().Where( a => a.IsChecked == true ).Select( s => ( int ) s.Tag ).ToList();
            if ( !ReportOptions.Current.CashAccountIds.Any() && !ReportOptions.Current.NonCashAccountIds.Any() )
            {
                if ( showWarnings )
                {
                    lblWarning.Visibility = Visibility.Visible;
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
            var listBox = sender == btnSelectAllForCashGifts ? lstCashAccounts : lstNonCashAccounts;
            listBox.Items.OfType<CheckBox>().ToList().ForEach( a => a.IsChecked = true );
        }

        /// <summary>
        /// Handles the Click event of the btnSelectNoneForCashGifts and btnSelectNoneForNonCashGifts controls
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectNone_Click( object sender, RoutedEventArgs e )
        {
            var listBox = sender == btnSelectNoneForCashGifts ? lstCashAccounts : lstNonCashAccounts;
            listBox.Items.OfType<CheckBox>().ToList().ForEach( a => a.IsChecked = false );
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
                _selectedCashAccountIds = lstCashAccounts.Items.OfType<CheckBox>().Where( a => a.IsChecked == true ).Select( a => ( int ) a.Tag ).ToList();
                _selectedNonCashAccountIds = lstNonCashAccounts.Items.OfType<CheckBox>().Where( a => a.IsChecked == true ).Select( a => ( int ) a.Tag ).ToList();
                ApplyFilter();
            }
        }
    }
}
