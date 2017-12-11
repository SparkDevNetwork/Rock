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
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.Page" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class SelectPledgeAccountsPage : Page
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
        /// Initializes a new instance of the <see cref="SelectPledgeAccountsPage"/> class.
        /// </summary>
        public SelectPledgeAccountsPage()
        {
            InitializeComponent();

            RockConfig rockConfig = RockConfig.Load();

            _rockRestClient = new RockRestClient( rockConfig.RockBaseUrl );
            _rockRestClient.Login( rockConfig.Username, rockConfig.Password );

            _financialAccountList = _rockRestClient.GetData<List<Rock.Client.FinancialAccount>>( "api/FinancialAccounts" );

            cbIncludeChildAccounts.IsChecked = ReportOptions.Current.PledgesIncludeChildAccounts;
            cbIncludeNonCashGifts.IsChecked = ReportOptions.Current.PledgesIncludeNonCashGifts;
            if ( ReportOptions.Current.PledgesAccountIds == null)
            {
                // default to No Accounts for Pledges
                ReportOptions.Current.PledgesAccountIds = new List<int>();
            }

            foreach ( var account in _financialAccountList.OrderBy( a => a.Order ).ThenBy( a => a.PublicName ) )
            {
                lstPledgeAccounts.Items.Add( new CheckBox { Tag = account.Id, Content = account.PublicName, IsChecked = ReportOptions.Current.PledgesAccountIds.Contains( account.Id ) } );
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
                var nextPage = new SelectAdvancedFeaturesPage();
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

            ReportOptions.Current.PledgesIncludeChildAccounts = cbIncludeChildAccounts.IsChecked == true;
            ReportOptions.Current.PledgesIncludeNonCashGifts = cbIncludeNonCashGifts.IsChecked == true;
            ReportOptions.Current.PledgesAccountIds = lstPledgeAccounts.Items.OfType<CheckBox>().Where( a => a.IsChecked == true ).Select( s => ( int ) s.Tag ).ToList();

            return true;
        }

        /// <summary>
        /// Handles the Click event of the btnSelectAllForCashGifts and btnSelectAllForNonCashGifts controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectAll_Click( object sender, RoutedEventArgs e )
        {
            lstPledgeAccounts.Items.OfType<CheckBox>().ToList().ForEach( a => a.IsChecked = true );
        }

        /// <summary>
        /// Handles the Click event of the btnSelectNoneForCashGifts and btnSelectNoneForNonCashGifts controls
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectNone_Click( object sender, RoutedEventArgs e )
        {
            lstPledgeAccounts.Items.OfType<CheckBox>().ToList().ForEach( a => a.IsChecked = false );
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
    }
}
