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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Rock.Wpf;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for ProgressPage.xaml
    /// </summary>
    public partial class ProgressPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressPage"/> class.
        /// </summary>
        public ProgressPage( bool resume, DateTime? resumeDateTime )
        {
            this.Resume = resume;
            this.ResumeRunDate = resumeDateTime;
            InitializeComponent();
        }

        private readonly bool Resume;
        private readonly DateTime? ResumeRunDate;

        private void NavigationService_Navigating( object sender, System.Windows.Navigation.NavigatingCancelEventArgs e )
        {
            // if the currently running, don't let navigation happen. This fixes an issue where pressing the BackSpace key would go to previous page
            // even though the report was still running
            e.Cancel = _isRunning;
        }

        private ResultsSummary _resultsSummary;

        /// <summary>
        /// The statement count
        /// </summary>
        //private int _statementCount;

        private static bool _wasCancelled = false;
        private static bool _isRunning = false;

        private ContributionReport _contributionReport;

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            var window = Window.GetWindow( this );
            if ( window != null )
            {
                window.KeyDown += Window_KeyDown;
            }

            NavigationService.Navigating += NavigationService_Navigating;
            btnPrev.Visibility = Visibility.Hidden;
            lblRenderStatementsProgress.Visibility = Visibility.Hidden;
            lblRenderStatementsProgress.Content = "Progress - Creating Statements";
            pgRenderStatementsProgress.Visibility = Visibility.Hidden;

            lblSaveMergeDocProgress.Visibility = Visibility.Collapsed;
            pgSaveMergeDocProgress.Visibility = Visibility.Collapsed;

            WpfHelper.FadeIn( pgRenderStatementsProgress, 2000 );
            WpfHelper.FadeIn( lblRenderStatementsProgress, 2000 );
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
        }

        private void Window_KeyDown( object sender, KeyEventArgs e )
        {
            bool isLeftAltDown = Keyboard.IsKeyDown( Key.LeftAlt );
            bool isDeleteDown = Keyboard.IsKeyDown( Key.Delete ) || Keyboard.IsKeyDown( Key.Back );
            if ( isLeftAltDown && isDeleteDown )
            {
                if ( _contributionReport != null )
                {
                    _contributionReport.Cancel();
                }

                var window = Window.GetWindow( this );
                if ( window != null )
                {
                    window.KeyDown -= Window_KeyDown;
                }
            }
        }

        /// <summary>
        /// Handles the RunWorkerCompleted event of the bw control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        protected void bw_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            btnPrev.Visibility = Visibility.Visible;
            pgRenderStatementsProgress.Visibility = Visibility.Collapsed;
            lblSaveMergeDocProgress.Visibility = Visibility.Collapsed;
            pgSaveMergeDocProgress.Visibility = Visibility.Collapsed;

            if ( e.Error != null )
            {
                lblRenderStatementsProgress.Content = "Error: " + e.Error.Message;
                throw e.Error;
            }

            var statementCount = this._resultsSummary?.StatementCount;

            if ( statementCount == 0 )
            {
                lblRenderStatementsProgress.Content = @"Warning: No records matched your criteria. No statements have been created.";
            }
            else if ( _wasCancelled )
            {
                lblRenderStatementsProgress.Style = this.FindResource( "labelStyleAlertWarning" ) as Style;
                lblRenderStatementsProgress.Content = $@"Canceled: {statementCount} statements created.";
            }
            else
            {
                lblRenderStatementsProgress.Style = this.FindResource( "labelStyleAlertSuccess" ) as Style;
                lblRenderStatementsProgress.Content = string.Format( @"Success:{1}Your statements have been created.{1}( {0} statements created )", statementCount, Environment.NewLine );

                ShowResultsSummary( _resultsSummary );
            }
        }

        /// <summary>
        /// Handles the DoWork event of the bw control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        protected void bw_DoWork( object sender, DoWorkEventArgs e )
        {
            using ( _contributionReport = new ContributionReport( ReportOptions.Current, this ) )
            {
                _contributionReport.Resume = this.Resume;
                _contributionReport.ResumeRunDate = this.ResumeRunDate;
                try
                {
                    _wasCancelled = false;
                    _isRunning = true;
                    _resultsSummary = _contributionReport.RunReport();
                }
                catch ( Exception ex )
                {
                    App.LogException( ex );
                    throw;
                }
                finally
                {
                    _isRunning = false;
                    _wasCancelled = _contributionReport.IsCancelled;
                }

                _contributionReport = null;
            }

            e.Result = _resultsSummary?.NumberOfGivingUnits > 0;
        }

        /// <summary>
        /// Shows the results summary.
        /// </summary>
        /// <param name="resultsSummary">The results summary.</param>
        private void ShowResultsSummary( ResultsSummary resultsSummary )
        {
            ResultsSummaryPage resultsSummaryPage = new ResultsSummaryPage( resultsSummary );
            NavigationService.Navigate( resultsSummaryPage );
        }

        /// <summary>
        /// The _start progress date time
        /// </summary>
        private DateTime _lastUpdate = DateTime.MinValue;

        /// <summary>
        /// Shows the save merge document progress.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="progressMessage">The progress message.</param>
        internal void ShowSaveMergeDocProgress( int position, int max, string progressMessage )
        {
            Dispatcher.Invoke( () =>
            {
                lblSaveMergeDocProgress.Content = progressMessage;
                pgSaveMergeDocProgress.Value = position;
                pgSaveMergeDocProgress.Maximum = max;
                lblSaveMergeDocProgress.Visibility = Visibility.Visible;
                pgSaveMergeDocProgress.Visibility = Visibility.Visible;

                lblStats.Content = string.Empty;
            } );
        }

        /// <summary>
        /// Shows the progress.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="progressMessage">The progress message.</param>
        /// <param name="limitUpdates">if set to <c>true</c> [limit updates].</param>
        internal void ShowProgress( int position, int max, string progressMessage, bool limitUpdates )
        {
            var timeSinceLastUpdate = DateTime.Now - _lastUpdate;

            if ( timeSinceLastUpdate.TotalSeconds < 1.0 && limitUpdates )
            {
                return;
            }

            _lastUpdate = DateTime.Now;

            Dispatcher.Invoke( () =>
            {
                if ( max > 0 )
                {
                    if ( lblRenderStatementsProgress.Content.ToString() != progressMessage )
                    {
                        lblRenderStatementsProgress.Content = progressMessage;
                    }

                    if ( pgRenderStatementsProgress.Maximum != max )
                    {
                        pgRenderStatementsProgress.Maximum = max;
                    }

                    if ( pgRenderStatementsProgress.Value != position )
                    {
                        pgRenderStatementsProgress.Value = position;
                    }

                    if ( pgRenderStatementsProgress.Visibility != Visibility.Visible )
                    {
                        pgRenderStatementsProgress.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    lblRenderStatementsProgress.Content = progressMessage;
                    pgRenderStatementsProgress.Visibility = Visibility.Collapsed;
                }

                // put the current statements/second in stats box (easter egg)
                var duration = DateTime.Now - _contributionReport.StartDateTime;
                if ( duration.TotalSeconds > 1 )
                {
                    double ratePerSecond = _contributionReport.RecordsCompletedCount / duration.TotalSeconds;
                    string statsText;
                    if ( max > 0 )
                    {
                        statsText = $"{position}/{max} @ {ratePerSecond:F2} per second";
                    }
                    else
                    {
                        statsText = "";
                    }
                    if ( ( string ) lblStats.Content != statsText )
                    {
                        lblStats.Content = statsText;
                    }
                }
            } );
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

        /// <summary>
        /// Handles the MouseDoubleClick event of the lblRenderStatementsProgress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void lblRenderStatementsProgress_MouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            if ( lblStats.Visibility != Visibility.Visible )
            {
                lblStats.Visibility = Visibility.Visible;
            }
            else
            {
                lblStats.Visibility = Visibility.Hidden;
            }
        }
    }
}
