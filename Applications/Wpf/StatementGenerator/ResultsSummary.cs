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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

using Rock.Client;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Class ResultsSummary.
    /// </summary>
    public class ResultsSummary
    {
        private DateTime? _reportEndDate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultsSummary"/> class.
        /// </summary>
        /// <param name="recipientList">The recipient list.</param>
        /// <param name="runStartDateTime">The date/time the execution started.</param>
        public ResultsSummary( List<FinancialStatementGeneratorRecipient> recipientList,
            DateTime? reportStartDate,
            DateTime? reportEndDate,
            DateTime runStartDateTime )
        {
            NumberOfGivingUnits = recipientList.Where( x => x.IsComplete ).Count();
            TotalAmount = recipientList.Where( x => x.IsComplete ).Sum( x => x.ContributionTotal ?? 0.00M );
            if ( recipientList.Any( a => a.PaperlessStatementUploaded.HasValue ) )
            {
                PaperlessStatementsCount = recipientList.Where( a => a.PaperlessStatementUploaded == true ).Count();
                PaperlessStatementsIndividualCount = recipientList.Where( a => a.PaperlessStatementUploaded == true ).Sum( s => s.PaperlessStatementsIndividualCount ?? 0 );
                PaperlessStatementTotalAmount = recipientList.Where( a => a.PaperlessStatementUploaded == true ).Sum( s => s.ContributionTotal ?? 0.00M );
            }

            ReportStartDate = reportStartDate;
            ReportEndDate = reportEndDate;

            // Initialize end the same as start
            StartDateTime = EndDateTime = runStartDateTime;

            // Initialize the output directories
            ReportOutputDirectories = new List<string>();
        }

        /// <summary>
        /// Gets the statement count.
        /// </summary>
        /// <value>
        /// The statement count.
        /// </value>
        public int StatementCount => NumberOfGivingUnits;

        /// <summary>
        /// Gets or sets the number of giving units.
        /// </summary>
        /// <value>
        /// The number of giving units.
        /// </value>
        public int NumberOfGivingUnits { get; private set; }

        /// <summary>
        /// Gets or sets the total amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        public decimal TotalAmount { get; private set; }

        /// <summary>
        /// Gets or sets the paperless statements count.
        /// </summary>
        /// <value>
        /// The paperless statements count.
        /// </value>
        public int? PaperlessStatementsCount { get; private set; }

        /// <summary>
        /// Gets or sets the paperless statement total amount.
        /// </summary>
        /// <value>
        /// The paperless statement total amount.
        /// </value>
        public decimal? PaperlessStatementTotalAmount { get; private set; }

        /// <summary>
        /// Gets or sets the paperless statements individual count.
        /// </summary>
        /// <value>
        /// The paperless statements individual count.
        /// </value>
        public int? PaperlessStatementsIndividualCount { get; private set; }

        /// <summary>
        /// Gets or sets the paper statements summary list.
        /// </summary>
        /// <value>
        /// The paper statements summary list.
        /// </value>
        public List<ReportPaperStatementsSummary> PaperStatementsSummaryList { get; private set; } = new List<ReportPaperStatementsSummary>();

        /// <summary>
        /// Gets the report start date.
        /// </summary>
        /// <value>The report start date.</value>
        public DateTime? ReportStartDate { get; private set; }

        /// <summary>
        /// Gets the report end date.
        /// </summary>
        /// <value>The report end date.</value>
        public DateTime? ReportEndDate
        {
            get => _reportEndDate.Value.AddDays( -1 );
            private set
            {
                _reportEndDate = value;
            }
        }

        /// <summary>
        /// Gets the start date time.
        /// </summary>
        /// <value>The start date time.</value>
        public DateTime StartDateTime { get; private set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>The end date time.</value>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Gets the duration.
        /// </summary>
        /// <value>The duration.</value>
        public TimeSpan Duration => EndDateTime - StartDateTime;

        /// <summary>Saves the results to a file in the current temporary directory.</summary>
        public void SaveResultsToFile()
        {
            if ( ReportOutputDirectories.Count == 0 )
            {
                return;
            }

            var rockConfig = RockConfig.Load();

            foreach ( var outputDir in ReportOutputDirectories )
            {
                var filePath = Path.Combine( outputDir, rockConfig.ResultSummaryFileName );

                // Write the summary out to @Summary of Results.txt
                File.WriteAllText( filePath, this.ToString() );
            }
        }

        /// <summary>
        /// Gets or sets the report output directories.
        /// </summary>
        /// <value>The report output directories.</value>
        public List<string> ReportOutputDirectories { get; set; }

        /// <summary>Returns a <see cref="System.String" /> that represents this results summary.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var output = new StringBuilder();

            // Results Summary
            output.AppendLine( string.Format( @"Results Summary {0:d} to {1:d}
",
                this.ReportStartDate,
                this.ReportEndDate ) );

            // Source Data
            output.AppendLine( string.Format(@"--- Source Data ---
The results below represent the giving data from the configured filters.
This data is provided to each of the report templates for processing. Each template may add additional filters.
Number of Giving Units: {0:n0}
Total Amount: {1:C}
", this.NumberOfGivingUnits, this.TotalAmount ) );

            // Paperless Statements
            if ( this.PaperlessStatementsCount.HasValue )
            {
                output.AppendLine( string.Format( @"--- Paperless Statements ---
Below are statistics for the digital statements that were added as documents to the donors.
Number of Statements: {0:n0}
Total Amount: {1:C}
Number of Individuals: {2:n0}
",
                    this.PaperlessStatementsCount,
                    this.PaperlessStatementTotalAmount,
                    this.PaperlessStatementsIndividualCount ) );
            }

            // Paper Statement Statistics
            if ( this.PaperStatementsSummaryList.Any() )
            {
                output.AppendLine( @"--- Paper Statement Statistics ---
Below are statistics for each report template you defined.
" );

                foreach ( var statement in PaperStatementsSummaryList )
                {
                    output.AppendLine( string.Format( @"  --- {0} ---
  Number of Statements: {1:n0}
  Total Amount: {2:C}",
                        statement.PrimarySortName,
                        statement.NumberOfStatements,
                        statement.TotalAmount) );

                    if ( statement.StatementsExcludedMinAmountVisibility == Visibility.Visible )
                    {
                        output.AppendLine( string.Format( @"  {0} {1}",
                            statement.StatementsExcludedMinAmountLabel,
                            statement.StatementsExcludedMinAmountSummary ) );
                    }

                    if ( statement.StatementsExcludedInternationalVisibility == Visibility.Visible )
                    {
                        output.AppendLine( string.Format( @"  Statements Excluded (International): {0}",
                            statement.StatementsExcludedInternationalSummary ) );
                    }

                    if ( statement.StatementsExcludedIncompleteAddressVisibility == Visibility.Visible )
                    {
                        output.AppendLine( string.Format( @"  Statements Excluded (Incomplete Address): {0}",
                            statement.StatementsExcludedIncompleteAddressSummary ) );
                    }

                    if ( statement.StatementsExcludedOptedOutVisibility == Visibility.Visible )
                    {
                        output.AppendLine( string.Format( @"  Statements Excluded (Opted Out): {0}",
                            statement.StatementExcludedOptedOutSummary ) );
                    }

                    if ( statement.BottomSeparatorVisibility == Visibility.Visible )
                    {
                        output.AppendLine( @"  --------" );
                    }

                    output.AppendLine();
                }
            }

            // Run Metrics
            output.AppendLine( string.Format( @"--- Run Metrics ---
Start Date/Time: {0:g}
End Date/Time: {1:g}
Duration: {2}
",
                this.StartDateTime,
                this.EndDateTime,
                string.Format( "{0:%h}h {0:%m}m {0:%s}s", this.Duration ) ) );

            return output.ToString();
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class ReportPaperStatementsSummary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportPaperStatementsSummary"/> class.
        /// </summary>
        /// <param name="completedRecipients">The completed recipients.</param>
        /// <param name="financialStatementReportConfiguration">The financial statement report configuration.</param>
        public ReportPaperStatementsSummary( List<FinancialStatementGeneratorRecipient> completedRecipients, FinancialStatementReportConfiguration financialStatementReportConfiguration )
        {
            PrimarySortName = financialStatementReportConfiguration.PrimarySortOrder.ConvertToString( true );

            var includedRecipients = completedRecipients.ToList();

            if ( financialStatementReportConfiguration.MinimumContributionAmount.HasValue )
            {
                var excludedRecipients = includedRecipients.Where( a => a.ContributionTotal < financialStatementReportConfiguration.MinimumContributionAmount.Value );
                StatementsExcludedMinAmountSummary = $"{excludedRecipients.Count()} | {excludedRecipients.Sum( x => x.ContributionTotal ?? 0.00M ).ToString( "C" ) }";

                includedRecipients = includedRecipients.Where( a => a.ContributionTotal >= financialStatementReportConfiguration.MinimumContributionAmount.Value ).ToList();
                StatementsExcludedMinAmount = financialStatementReportConfiguration.MinimumContributionAmount;
                StatementsExcludedMinAmountVisibility = Visibility.Visible;
            }

            if ( financialStatementReportConfiguration.IncludeInternationalAddresses == false )
            {
                var excludedRecipients = includedRecipients.Where( a => a.IsInternationalAddress != false );
                StatementsExcludedInternationalSummary = $"{excludedRecipients.Count()} | {excludedRecipients.Sum( x => x.ContributionTotal ?? 0.00M ).ToString( "C" ) }";

                includedRecipients = includedRecipients.Where( a => a.IsInternationalAddress == false ).ToList();
                StatementsExcludedInternationalVisibility = Visibility.Visible;
            }

            if ( financialStatementReportConfiguration.ExcludeRecipientsThatHaveAnIncompleteAddress )
            {
                var excludedRecipients = includedRecipients.Where( a => a.HasValidMailingAddress == false );
                StatementsExcludedIncompleteAddressSummary = $"{excludedRecipients.Count()} | {excludedRecipients.Sum( x => x.ContributionTotal ?? 0.00M ).ToString( "C" ) }";

                includedRecipients = includedRecipients.Where( a => a.HasValidMailingAddress == true ).ToList();
                StatementsExcludedIncompleteAddressVisibility = Visibility.Visible;
            }

            if ( financialStatementReportConfiguration.ExcludeOptedOutIndividuals )
            {
                var excludedRecipients = includedRecipients.Where( a => a.OptedOut != false );
                StatementExcludedOptedOutSummary = $"{excludedRecipients.Count()} | {excludedRecipients.Sum( x => x.ContributionTotal ?? 0.00M ).ToString( "C" ) }";
                StatementsExcludedOptedOutVisibility = Visibility.Visible;
                includedRecipients = includedRecipients.Where( a => a.OptedOut == false ).ToList();
            }

            NumberOfStatements = includedRecipients.Count();
            TotalAmount = includedRecipients.Sum( a => a.ContributionTotal ?? 0.00M );
        }

        /// <summary>
        /// Gets the name of the primary sort.
        /// </summary>
        /// <value>
        /// The name of the primary sort.
        /// </value>
        public string PrimarySortName { get; private set; }

        /// <summary>
        /// Gets the number of statements.
        /// </summary>
        /// <value>
        /// The number of statements.
        /// </value>
        public int NumberOfStatements { get; private set; }

        /// <summary>
        /// Gets the total amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        public decimal TotalAmount { get; private set; }

        /// <summary>
        /// Gets the statements excluded minimum amount visibility.
        /// </summary>
        /// <value>
        /// The statements excluded minimum amount visibility.
        /// </value>
        public Visibility StatementsExcludedMinAmountVisibility { get; private set; } = Visibility.Collapsed;

        /// <summary>
        /// Gets the statements excluded minimum amount.
        /// </summary>
        /// <value>
        /// The statements excluded minimum amount.
        /// </value>
        public decimal? StatementsExcludedMinAmount { get; private set; }

        /// <summary>
        /// Gets the statements excluded minimum amount label.
        /// </summary>
        /// <value>
        /// The statements excluded minimum amount label.
        /// </value>
        public string StatementsExcludedMinAmountLabel => $"Statements Excluded (< {StatementsExcludedMinAmount?.ToString( "C" )}):";

        /// <summary>
        /// Gets the statements excluded minimum amount summary.
        /// </summary>
        /// <value>
        /// The statements excluded minimum amount summary.
        /// </value>
        public string StatementsExcludedMinAmountSummary { get; private set; }

        /// <summary>
        /// Gets the statements excluded international visibility.
        /// </summary>
        /// <value>
        /// The statements excluded international visibility.
        /// </value>
        public Visibility StatementsExcludedInternationalVisibility { get; private set; } = Visibility.Collapsed;

        /// <summary>
        /// Gets the statements excluded international summary.
        /// </summary>
        /// <value>
        /// The statements excluded international summary.
        /// </value>
        public string StatementsExcludedInternationalSummary { get; private set; }

        /// <summary>
        /// Gets the statements excluded incomplete address visibility.
        /// </summary>
        /// <value>
        /// The statements excluded incomplete address visibility.
        /// </value>
        public Visibility StatementsExcludedIncompleteAddressVisibility { get; private set; } = Visibility.Collapsed;

        /// <summary>
        /// Gets the statements excluded incomplete address summary.
        /// </summary>
        /// <value>
        /// The statements excluded incomplete address summary.
        /// </value>
        public string StatementsExcludedIncompleteAddressSummary { get; private set; }

        /// <summary>
        /// Gets the statements excluded opted out visibility.
        /// </summary>
        /// <value>
        /// The statements excluded opted out visibility.
        /// </value>
        public Visibility StatementsExcludedOptedOutVisibility { get; private set; } = Visibility.Collapsed;

        /// <summary>
        /// Gets the statement excluded opted out summary.
        /// </summary>
        /// <value>
        /// The statement excluded opted out summary.
        /// </value>
        public string StatementExcludedOptedOutSummary { get; private set; }

        /// <summary>
        /// Gets the bottom separator visibility.
        /// </summary>
        /// <value>
        /// The bottom separator visibility.
        /// </value>
        public Visibility BottomSeparatorVisibility { get; set; } = Visibility.Visible;
    }
}
