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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Rock.Client;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for ReportSettings.xaml
    /// </summary>
    public partial class ReportSettings : System.Windows.Controls.Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportSettings"/> class.
        /// </summary>
        public ReportSettings()
        {
            InitializeComponent();

            cbEnablePageCountPredetermination.IsChecked = RockConfig.Load().EnablePageCountPredetermination;

            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockConfig = RockConfig.Load();

            List<FinancialStatementReportConfiguration> reportConfigurationList = null;

            try
            {
                if ( rockConfig.ReportConfigurationListJson.IsNotNullOrWhiteSpace() )
                {
                    reportConfigurationList = rockConfig.ReportConfigurationListJson.FromJsonOrNull<List<FinancialStatementReportConfiguration>>();
                }
            }
            catch
            {
                // ignore if ReportConfigurationListJson can be de-serialized. We'll just create a new one instead.
            }

            if ( reportConfigurationList == null )
            {
                // if this is the first time the generator has run, create a default.
                // If they delete this default, that is OK. See https://app.asana.com/0/0/1200266899611805/f
                reportConfigurationList = new List<FinancialStatementReportConfiguration>();
                var defaultConfiguration = new FinancialStatementReportConfiguration
                {
                    DestinationFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.Desktop ), "Statements" ),
                    ExcludeOptedOutIndividuals = true,
                    FilenamePrefix = "statement-",
                    IncludeInternationalAddresses = false,
                    MaxStatementsPerChapter = 500,
                    MinimumContributionAmount = 1.00M,
                    PreventSplittingPrimarySortValuesAcrossChapters = true,
                    PrimarySortOrder = Client.Enums.FinancialStatementOrderBy.PostalCode,
                    SecondarySortOrder = Client.Enums.FinancialStatementOrderBy.LastName,
                    SplitFilesOnPrimarySortValue = true,
                    CreatedDateTime = DateTime.Now,
                    Guid = Guid.NewGuid()
                };

                reportConfigurationList.Add( defaultConfiguration );
                rockConfig.ReportConfigurationListJson = reportConfigurationList.ToJson();
                rockConfig.Save();
            }

            ReportOptions.Current.ReportConfigurationList = reportConfigurationList;

            grdReportSettings.DataContext = reportConfigurationList.OrderBy( a => a.CreatedDateTime );
        }

        /// <summary>
        /// Handles the Click event of the btnShowReportSettingsModal control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnShowReportSettingsModal_Click( object sender, RoutedEventArgs e )
        {
            AddEditReportSettings( null );
        }

        /// <summary>
        /// Adds the edit report settings.
        /// </summary>
        /// <param name="selectedSettings">The selected settings.</param>
        private void AddEditReportSettings( FinancialStatementReportConfiguration selectedSettings )
        {
            ReportConfigurationModalWindow reportSettingsModalWindow = new ReportConfigurationModalWindow( selectedSettings );
            reportSettingsModalWindow.Owner = Window.GetWindow( this );
            var showDialogResult = reportSettingsModalWindow.ShowDialog();
            if ( showDialogResult == true )
            {
                FinancialStatementReportConfiguration updatedSettings = reportSettingsModalWindow.GetFinancialStatementReportConfiguration();
                ReportOptions.Current.ReportConfigurationList = ReportOptions.Current.ReportConfigurationList ?? new List<FinancialStatementReportConfiguration>();
                var settingsToUpdate = ReportOptions.Current.ReportConfigurationList.FirstOrDefault( a => a.Guid == updatedSettings.Guid );
                if ( settingsToUpdate != null )
                {
                    // replace the settings with the new ones
                    ReportOptions.Current.ReportConfigurationList.Remove( settingsToUpdate );
                }

                ReportOptions.Current.ReportConfigurationList.Add( updatedSettings );

                var rockConfig = RockConfig.Load();
                rockConfig.ReportConfigurationListJson = ReportOptions.Current.ReportConfigurationList.ToJson();
                rockConfig.Save();

                BindGrid();
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
            rockConfig.EnablePageCountPredetermination = cbEnablePageCountPredetermination.IsChecked == true;
            rockConfig.Save();
            ReportOptions.Current.EnablePageCountPredetermination = rockConfig.EnablePageCountPredetermination;

            return true;
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnGenerateStatements_Click( object sender, RoutedEventArgs e )
        {
            if ( SaveChanges( true ) )
            {
                var nextPage = new ProgressPage( false, null );
                this.NavigationService.Navigate( nextPage );
            }
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
        /// Handles the Click event of the btnDeleteReportOption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDeleteReportOption_Click( object sender, RoutedEventArgs e )
        {
            ReportOptions.Current.ReportConfigurationList = ReportOptions.Current.ReportConfigurationList ?? new List<FinancialStatementReportConfiguration>();
            var seletedReportConfig = ( sender as Button ).DataContext as FinancialStatementReportConfiguration;
            ReportOptions.Current.ReportConfigurationList.Remove( seletedReportConfig );

            var rockConfig = RockConfig.Load();
            rockConfig.ReportConfigurationListJson = ReportOptions.Current.ReportConfigurationList.ToJson();
            rockConfig.Save();

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDoubleClick event of the grdReportSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        protected void grdReportSettings_RowDoubleClick( object sender, MouseButtonEventArgs e )
        {
            var seletedReportConfig = ( sender as DataGridRow ).DataContext as FinancialStatementReportConfiguration;
            AddEditReportSettings( seletedReportConfig );
        }

        /// <summary>
        /// Handles the Click event of the btnEditReportOption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnEditReportOption_Click( object sender, RoutedEventArgs e )
        {
            var seletedReportConfig = ( sender as Button ).DataContext as FinancialStatementReportConfiguration;
            AddEditReportSettings( seletedReportConfig );
        }
    }
}
