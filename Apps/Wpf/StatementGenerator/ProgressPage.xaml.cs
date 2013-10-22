using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ceTe.DynamicPDF.ReportWriter;

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
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            lblReportProgress.Visibility = System.Windows.Visibility.Hidden;
            lblReportProgress.Content = "Progress - Creating Statements";
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
            if ( e.Error != null )
            {
                lblReportProgress.Content = "Error: " + e.Error.Message;
                throw e.Error;
            }
            
            lblReportProgress.Content = "Done";
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

            string fileName = "Statement_" + Guid.NewGuid().ToString( "N" ) + ".pdf";

            ShowProgress( 0, 0, "Generating PDF..." );
            doc.Draw( fileName );

            Process.Start( fileName );
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
        /// Shows the progress.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="progressMessage">The progress message.</param>
        private void ShowProgress( int position, int max, string progressMessage )
        {
            Dispatcher.Invoke( () =>
            {
                if ( max > 0 )
                {
                    lblReportProgress.Content = string.Format( "{0}/{1} - {2}", position, max, progressMessage );
                }
                else
                {
                    lblReportProgress.Content = progressMessage;
                }
            } );
        }
    }
}
