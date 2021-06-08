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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Rock.Client;
using Rock.Client.Enums;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class ReportConfigurationModalWindow : Window
    {
        private DateTime? FinancialStatementReportConfigurationCreateDateTime { get; set; }

        private Guid FinancialStatementReportConfigurationGuid { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfigurationModalWindow" /> class.
        /// </summary>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        public ReportConfigurationModalWindow( FinancialStatementReportConfiguration financialStatementReportConfiguration )
        {
            InitializeComponent();

            if ( financialStatementReportConfiguration == null )
            {
                lblActionTitle.Content = "Add Report";
                financialStatementReportConfiguration = new FinancialStatementReportConfiguration();
                financialStatementReportConfiguration.CreatedDateTime = DateTime.Now;
                financialStatementReportConfiguration.Guid = Guid.NewGuid();
            }
            else
            {
                lblActionTitle.Content = "Edit Report";
            }

            cboPrimarySort.Items.Clear();
            var orderByOptions = Enum.GetValues( typeof( FinancialStatementOrderBy ) ).OfType<FinancialStatementOrderBy>();
            foreach ( var orderByOption in orderByOptions )
            {
                var primarySortItem = new ComboBoxItem { Content = orderByOption.ConvertToString( true ), Tag = orderByOption };
                primarySortItem.IsSelected = financialStatementReportConfiguration.PrimarySortOrder == orderByOption;
                cboPrimarySort.Items.Add( primarySortItem );

                var secondarySortItem = new ComboBoxItem { Content = orderByOption.ConvertToString( true ), Tag = orderByOption };
                secondarySortItem.IsSelected = financialStatementReportConfiguration.SecondarySortOrder == orderByOption;
                cboSecondarySort.Items.Add( secondarySortItem );
            }

            tbDestinationFolder.Text = financialStatementReportConfiguration.DestinationFolder;
            tbFilenamePrefix.Text = financialStatementReportConfiguration.FilenamePrefix;
            cbSplitFilesOnPrimarySortValue.IsChecked = financialStatementReportConfiguration.SplitFilesOnPrimarySortValue;
            tbMaxStatementsInChapter.Text = financialStatementReportConfiguration.MaxStatementsPerChapter.ToString();
            cbPreventSplittingPrimarySortValuesAcrossChapters.IsChecked = financialStatementReportConfiguration.PreventSplittingPrimarySortValuesAcrossChapters;
            tbMinimumContributionAmount.Text = financialStatementReportConfiguration.MinimumContributionAmount.ToString();
            cbIncludeInternationalAddresses.IsChecked = financialStatementReportConfiguration.IncludeInternationalAddresses;
            cbDoNotIncludeIncompleteAddresses.IsChecked = financialStatementReportConfiguration.ExcludeRecipientsThatHaveAnIncompleteAddress;
            cbDoNotIncludeStatementsForThoseWhoHaveOptedOut.IsChecked = financialStatementReportConfiguration.ExcludeOptedOutIndividuals;

            FinancialStatementReportConfigurationCreateDateTime = financialStatementReportConfiguration.CreatedDateTime;
            FinancialStatementReportConfigurationGuid = financialStatementReportConfiguration.Guid;
        }

        /// <summary>
        /// Gets the financial statement report configuration.
        /// </summary>
        /// <returns></returns>
        public FinancialStatementReportConfiguration GetFinancialStatementReportConfiguration()
        {
            FinancialStatementReportConfiguration financialStatementReportConfiguration = new FinancialStatementReportConfiguration();
            financialStatementReportConfiguration.PrimarySortOrder = ( FinancialStatementOrderBy? ) ( cboPrimarySort.SelectedValue as ComboBoxItem ).Tag ?? FinancialStatementOrderBy.PostalCode;
            financialStatementReportConfiguration.SecondarySortOrder = ( FinancialStatementOrderBy? ) ( cboSecondarySort.SelectedValue as ComboBoxItem ).Tag ?? FinancialStatementOrderBy.LastName;
            financialStatementReportConfiguration.DestinationFolder = tbDestinationFolder.Text;
            financialStatementReportConfiguration.FilenamePrefix = tbFilenamePrefix.Text;
            financialStatementReportConfiguration.SplitFilesOnPrimarySortValue = cbSplitFilesOnPrimarySortValue.IsChecked ?? false;
            financialStatementReportConfiguration.MaxStatementsPerChapter = tbMaxStatementsInChapter.Text.AsIntegerOrNull();
            financialStatementReportConfiguration.PreventSplittingPrimarySortValuesAcrossChapters = cbPreventSplittingPrimarySortValuesAcrossChapters.IsChecked ?? true;
            financialStatementReportConfiguration.MinimumContributionAmount = tbMinimumContributionAmount.Text.AsDecimalOrNull();
            financialStatementReportConfiguration.IncludeInternationalAddresses = cbIncludeInternationalAddresses.IsChecked ?? true;
            financialStatementReportConfiguration.ExcludeRecipientsThatHaveAnIncompleteAddress = cbDoNotIncludeIncompleteAddresses.IsChecked ?? true;
            financialStatementReportConfiguration.ExcludeOptedOutIndividuals = cbDoNotIncludeStatementsForThoseWhoHaveOptedOut.IsChecked ?? true;
            financialStatementReportConfiguration.CreatedDateTime = FinancialStatementReportConfigurationCreateDateTime ?? DateTime.Now;
            financialStatementReportConfiguration.Guid = FinancialStatementReportConfigurationGuid;

            return financialStatementReportConfiguration;
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="showWarnings">if set to <c>true</c> [show warnings].</param>
        /// <returns></returns>
        private bool SaveChanges( bool showWarnings )
        {
            if ( showWarnings )
            {
                if ( tbDestinationFolder.Text.Trim() == string.Empty )
                {
                    MessageBoxResult result = MessageBox.Show( "Please select a folder to save contribution statements to.", "Folder Location Required", MessageBoxButton.OK, MessageBoxImage.Warning );
                    return false;
                }

                if ( !Directory.Exists( tbDestinationFolder.Text ) )
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory( tbDestinationFolder.Text );
                    }
                    catch ( Exception )
                    {
                        MessageBoxResult result = MessageBox.Show( "Couldn't create the directory provided. Please double-check that it is a valid path.", "Path Not Valid", MessageBoxButton.OK, MessageBoxImage.Exclamation );
                        return false;
                    }
                }

                if ( tbFilenamePrefix.Text == string.Empty )
                {
                    MessageBoxResult result = MessageBox.Show( "Please provide a filename prefix.", "Required", MessageBoxButton.OK, MessageBoxImage.Warning );
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Handles the Click event of the btnSaveChanges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSaveChanges_Click( object sender, RoutedEventArgs e )
        {
            if ( SaveChanges( true ) )
            {
                DialogResult = true;
                Close();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the btnSelectFolder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectFolder_Click( object sender, RoutedEventArgs e )
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if ( result == System.Windows.Forms.DialogResult.OK )
            {
                tbDestinationFolder.Text = dialog.SelectedPath;
            }
        }
    }
}
