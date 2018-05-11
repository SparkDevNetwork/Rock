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
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Rock.Wpf;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for ProgressPage.xaml
    /// </summary>
    public partial class ProgressPage : Page
    {
        public ProgressPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The statement count
        /// </summary>
        private int _statementCount;

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            btnPrev.Visibility = Visibility.Hidden;
            lblReportProgress.Visibility = Visibility.Hidden;
            lblReportProgress.Content = "Progress - Creating Statements";
            pgReportProgress.Visibility = Visibility.Hidden;
            WpfHelper.FadeIn( pgReportProgress, 2000 );
            WpfHelper.FadeIn( lblReportProgress, 2000 );
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Handles the RunWorkerCompleted event of the bw control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void bw_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            btnPrev.Visibility = Visibility.Visible;
            pgReportProgress.Visibility = Visibility.Collapsed;
            
            if ( e.Error != null )
            {
                lblReportProgress.Content = "Error: " + e.Error.Message;
                throw e.Error;
            }
            
            if ( _statementCount == 0 )
            {
                lblReportProgress.Content = @"Warning: No records matched your criteria. No statements have been created.";
            }
            else
            {
                lblReportProgress.Style = this.FindResource( "labelStyleAlertSuccess" ) as Style;
                lblReportProgress.Content = string.Format( @"Success:{1}Your statements have been created.{1}( {0} statements created )", _statementCount, Environment.NewLine );
            }
        }

        /// <summary>
        /// Handles the DoWork event of the bw control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void bw_DoWork( object sender, DoWorkEventArgs e )
        {
            ContributionReport contributionReport = new ContributionReport( ReportOptions.Current );
            contributionReport.OnProgress += ContributionReport_OnProgress;

            _statementCount = contributionReport.RunReport();

            e.Result = _statementCount > 0;
        }

        /// <summary>
        /// Handles the OnProgress event of the ContributionReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ProgressEventArgs"/> instance containing the event data.</param>
        private void ContributionReport_OnProgress( object sender, ProgressEventArgs e )
        {
            ShowProgress( e.Position, e.Max, e.ProgressMessage );
        }


        /// <summary>
        /// The _start progress date time
        /// </summary>
        private DateTime _startProgressDateTime = DateTime.MinValue;

        /// <summary>
        /// Shows the progress.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="progressMessage">The progress message.</param>
        private void ShowProgress( int position, int max, string progressMessage )
        {
            Dispatcher.Invoke( () =>
            {
                if ( position <= 1 )
                {
                    _startProgressDateTime = DateTime.Now;
                }

                if ( max > 0 )
                {
                    if ( (string)lblReportProgress.Content != progressMessage )
                    {
                        lblReportProgress.Content = progressMessage;
                    }
                    if ( pgReportProgress.Maximum != max )
                    {
                        pgReportProgress.Maximum = max;
                    }

                    if ( pgReportProgress.Value != position )
                    {
                        pgReportProgress.Value = position;
                    }

                    if ( pgReportProgress.Visibility != Visibility.Visible )
                    {
                        pgReportProgress.Visibility = Visibility.Visible;
                    }
                    
                    // put the current statements/second in the tooltip
                    var duration = DateTime.Now - _startProgressDateTime;
                    if ( duration.TotalSeconds > 1 )
                    {
                        double rate = position / duration.TotalSeconds;
                        string toolTip = string.Format( "{1}/{2} @ {0:F2} per second", rate, position, max );
                        if ( (string)lblReportProgress.ToolTip != toolTip )
                        {
                            lblReportProgress.ToolTip = toolTip;
                        }
                    }
                }
                else
                {
                    lblReportProgress.Content = progressMessage;
                    pgReportProgress.Visibility = Visibility.Collapsed;
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
        /// Handles the MouseDoubleClick event of the lblReportProgress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void lblReportProgress_MouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            if ( ReportOptions.Current.StatementsPerChapter > 1 )
            {
                // open the folder that the pdfs are in
                System.Diagnostics.Process.Start( ReportOptions.Current.SaveDirectory );
            }
            else
            {
                // open the pdf
                string filePath = string.Format( @"{0}\{1}.pdf", ReportOptions.Current.SaveDirectory, ReportOptions.Current.BaseFileName );
                System.Diagnostics.Process.Start( filePath );
            }
        }
    }
}
