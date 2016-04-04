﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
        /// The _PDF output
        /// </summary>
        private byte[] _pdfOutput;

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            btnSaveAs.Visibility = Visibility.Hidden;
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

            if ( e.Result != null )
            {
                _pdfOutput = e.Result as byte[];
                lblReportProgress.Content = "Complete";
                btnSaveAs.Visibility = Visibility.Visible;
            }
            else
            {
                _pdfOutput = null;
                lblReportProgress.Content = @"Warning:
No contributions found with the criteria provided.";
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
            contributionReport.OnProgress += contributionReport_OnProgress;

            var doc = contributionReport.RunReport();

            if ( doc != null )
            {
                ShowProgress( 0, 0, "Rendering PDF..." );
                byte[] pdfData = doc.Draw();
                e.Result = pdfData;
            }
            else
            {
                e.Result = null;
            }
        }

        /// <summary>
        /// Handles the OnProgress event of the contributionReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ContributionReport.ProgressEventArgs"/> instance containing the event data.</param>
        protected void contributionReport_OnProgress( object sender, ContributionReport.ProgressEventArgs e )
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
                    lblReportProgress.Content = progressMessage;
                    pgReportProgress.Maximum = max;
                    pgReportProgress.Value = position;
                    if ( pgReportProgress.Visibility != Visibility.Visible )
                    {
                        pgReportProgress.Visibility = Visibility.Visible;
                    }
                    
                    // put the current statements/second in the tooltip
                    var duration = DateTime.Now - _startProgressDateTime;
                    if ( duration.TotalSeconds > 10 )
                    {
                        double rate = position / duration.TotalSeconds;
                        lblReportProgress.ToolTip = string.Format( "{1}/{2} @ {0:F2} per second", rate, position, max );
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
        /// Handles the Click event of the btnSaveAs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSaveAs_Click( object sender, RoutedEventArgs e )
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.OverwritePrompt = true;
            dlg.DefaultExt = ".pdf";
            dlg.Filter = "Pdf Files (.pdf)|*.pdf";

            if ( dlg.ShowDialog() == true )
            {
                File.WriteAllBytes( dlg.FileName, _pdfOutput );
                Process.Start( dlg.FileName );
            }
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
