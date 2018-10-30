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
    public partial class SelectAdvancedFeaturesPage : Page
    {

        /// <summary>
        /// The rock rest client
        /// </summary>
        private RockRestClient _rockRestClient;

        /// <summary>
        /// The transaction types
        /// </summary>
        private List<Rock.Client.DefinedValue> _transactionTypes = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectAdvancedFeaturesPage"/> class.
        /// </summary>
        public SelectAdvancedFeaturesPage()
        {
            InitializeComponent();

            cbHideRefundedTransactions.IsChecked = ReportOptions.Current.HideRefundedTransactions;
            cbHideCorrectedTransactions.IsChecked = ReportOptions.Current.HideCorrectedTransactions;
            var orderByItems = Enum.GetValues( typeof( Rock.StatementGenerator.OrderBy ) ).OfType<Rock.StatementGenerator.OrderBy>().Select( a => a.ConvertToString( true ) ).ToList();
            ddlOrderBy.ItemsSource = orderByItems;
            ddlOrderBy.SelectedValue = ReportOptions.Current.OrderBy.ConvertToString( true );

            RockConfig rockConfig = RockConfig.Load();

            _rockRestClient = new RockRestClient( rockConfig.RockBaseUrl );
            _rockRestClient.Login( rockConfig.Username, rockConfig.Password );

            var transactionTypeDefinedType = _rockRestClient.GetDataByGuid<Rock.Client.DefinedType>( "api/DefinedTypes", Rock.Client.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE.AsGuid() );
            _transactionTypes = _rockRestClient.GetData<List<Rock.Client.DefinedValue>>( $"api/DefinedValues?$filter=DefinedTypeId eq {transactionTypeDefinedType.Id}" );

            lstTransactionTypes.Items.Clear();

            var defaultTransactionTypeGuids = new Guid[] {
                Rock.Client.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION.AsGuid()
            };

            foreach ( var transactionType in _transactionTypes )
            {
                var transactionTypeCheckbox = new CheckBox { Tag = transactionType.Id, Content = transactionType.Value };
                if ( ReportOptions.Current.TransactionTypeIds != null )
                {
                    transactionTypeCheckbox.IsChecked = ReportOptions.Current.TransactionTypeIds.Contains( transactionType.Id );
                }
                else
                {
                    transactionTypeCheckbox.IsChecked = defaultTransactionTypeGuids.Contains( transactionType.Guid );
                }

                lstTransactionTypes.Items.Add( transactionTypeCheckbox );
            }

            lblTransactionTypesWarning.Visibility = Visibility.Hidden;
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
                var nextPage = new SelectSaveLocationPage();
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
            ReportOptions.Current.HideRefundedTransactions = cbHideRefundedTransactions.IsChecked == true;
            ReportOptions.Current.HideCorrectedTransactions = cbHideCorrectedTransactions.IsChecked == true;
            ReportOptions.Current.OrderBy = ( ddlOrderBy.SelectedValue as string ).ConvertToEnumOrNull<Rock.StatementGenerator.OrderBy>() ?? Rock.StatementGenerator.OrderBy.PostalCode;
            ReportOptions.Current.TransactionTypeIds = lstTransactionTypes.Items.OfType<CheckBox>().Where( a => a.IsChecked == true ).Select( s => ( int ) s.Tag ).ToList();

            if (!ReportOptions.Current.TransactionTypeIds.Any() )
            {
                lblTransactionTypesWarning.Content = "Please select at least one transaction type.";
                lblTransactionTypesWarning.Visibility = Visibility.Visible;
                return false;
            }

            return true;
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
