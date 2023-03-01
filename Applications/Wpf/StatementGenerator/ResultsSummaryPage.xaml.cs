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
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for ResultsSummaryPage.xaml
    /// </summary>
    public partial class ResultsSummaryPage : Page
    {
        public ResultsSummaryPage( ResultsSummary resultsSummary )
        {
            InitializeComponent();

            resultsSummary = resultsSummary ?? new ResultsSummary( new System.Collections.Generic.List<Client.FinancialStatementGeneratorRecipient>(),
                DateTime.Now,
                DateTime.Now,
                DateTime.Now );

            lblReportDates.Content = $"{resultsSummary.ReportStartDate:d} to {resultsSummary.ReportEndDate:d}";

            lblNumberOfGivingUnits.Content = resultsSummary.NumberOfGivingUnits;
            lblTotalGivingAmount.Content = resultsSummary.TotalAmount.ToString("C");

            pnlPaperlessStatements.Visibility = resultsSummary.PaperlessStatementsCount.HasValue ? Visibility.Visible : Visibility.Collapsed;
            lblNumberOfPaperlessStatements.Content = resultsSummary.PaperlessStatementsCount;
            lblPaperlessStatementsTotalAmount.Content = resultsSummary.PaperlessStatementTotalAmount?.ToString( "C" );
            lblPaperlessStatementsNumberOfIndividuals.Content = resultsSummary.PaperlessStatementsIndividualCount;
            if ( resultsSummary.PaperStatementsSummaryList.Any() )
            {
                resultsSummary.PaperStatementsSummaryList.LastOrDefault().BottomSeparatorVisibility = Visibility.Collapsed;
                pnlPaperStatementStatistics.Visibility = Visibility.Visible;
            }
            else
            {
                pnlPaperStatementStatistics.Visibility = Visibility.Collapsed;
            }

            rptReportStatistics.ItemsSource = resultsSummary.PaperStatementsSummaryList;

            // Run Metrics
            lblStartDateTime.Content = resultsSummary.StartDateTime.ToString( "g" );
            lblEndDateTime.Content = resultsSummary.EndDateTime.ToString( "g" );
            lblDuration.Content = string.Format( "{0:%h}h {0:%m}m {0:%s}s", resultsSummary.Duration );

            // Output the Results
            resultsSummary.SaveResultsToFile();
        }

        /// <summary>
        /// Handles the Click event of the btnPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnPrev_Click( object sender, RoutedEventArgs e )
        {
            var reportSettings = new ReportSettings();
            this.NavigationService.Navigate( reportSettings );
        }
    }
}
