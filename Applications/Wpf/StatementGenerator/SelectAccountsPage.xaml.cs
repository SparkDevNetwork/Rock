﻿// <copyright>
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
        /// 
        /// </summary>
        private class NameIdIsChecked
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>
            /// The unique identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [is checked].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [is checked]; otherwise, <c>false</c>.
            /// </value>
            public bool IsChecked { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectAccountsPage"/> class.
        /// </summary>
        public SelectAccountsPage()
        {
            InitializeComponent();

            RockConfig rockConfig = RockConfig.Load();

            _rockRestClient = new RockRestClient( rockConfig.RockBaseUrl );
            _rockRestClient.Login( rockConfig.Username, rockConfig.Password );

            var accounts = _rockRestClient.GetData<List<Rock.Client.FinancialAccount>>( "api/FinancialAccounts" );

            lstAccounts.ItemsSource = accounts.OrderBy( a => a.Order ).ThenBy( a => a.Name ).Select( a => new NameIdIsChecked { Id = a.Id, Name = a.PublicName, IsChecked = true } );

            lblWarning.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnNext_Click( object sender, RoutedEventArgs e )
        {
            var accountIds = lstAccounts.Items.OfType<NameIdIsChecked>().Where( a => a.IsChecked == true ).Select( s => s.Id ).ToList();
            if ( accountIds.Count > 0 )
            {
                ReportOptions.Current.AccountIds = accountIds;
                SelectDateRangePage nextPage = new SelectDateRangePage();
                this.NavigationService.Navigate( nextPage );
            }
            else
            {
                lblWarning.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelectAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectAll_Click( object sender, RoutedEventArgs e )
        {
            var list = lstAccounts.Items.OfType<NameIdIsChecked>().ToList();
            list.ForEach( a => a.IsChecked = true );
            lstAccounts.ItemsSource = list;
        }

        /// <summary>
        /// Handles the Click event of the btnSelectNone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectNone_Click( object sender, RoutedEventArgs e )
        {
            var list = lstAccounts.Items.OfType<NameIdIsChecked>().ToList();
            list.ForEach( a => a.IsChecked = false );
            lstAccounts.ItemsSource = list;
        }

        /// <summary>
        /// Handles the Click event of the btnPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnPrev_Click( object sender, RoutedEventArgs e )
        {
            this.NavigationService.GoBack();
        }
    }
}
