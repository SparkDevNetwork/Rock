using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using ceTe.DynamicPDF.ReportWriter;
using Rock.Data;
using Rock.Model;

namespace ContributionStatementApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the btnShowStatement control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnShowStatement_Click( object sender, RoutedEventArgs e )
        {
            btnShowStatement.IsEnabled = false;
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
        void bw_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            btnShowStatement.IsEnabled = true;

            lblReportProgress.Content = "Done";
            WpfHelper.FadeOut( lblReportProgress );
        }

        /// <summary>
        /// Handles the DoWork event of the bw control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        void bw_DoWork( object sender, DoWorkEventArgs e )
        {
            ContributionReport contributionReport = new ContributionReport();
            contributionReport.OnProgress += contributionReport_OnProgress;

            // default Initializer is CreateDatabaseIfNotExists, but we don't want that to happen if automigrate is false, so set it to NULL so that nothing happens
            Database.SetInitializer<Rock.Data.RockContext>( null );

            // TODO - Get actual date range
            contributionReport.Options.StartDate = new DateTime( 2012, 10, 1 );
            contributionReport.Options.EndDate = new DateTime( 2013, 10, 1 );
            contributionReport.Options.LayoutFile = new DplxFile( "rock-report.dplx" );

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

        /// <summary>
        /// Handles the Initialized event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Window_Initialized( object sender, EventArgs e )
        {
            // default Initializer is CreateDatabaseIfNotExists, but we don't want that to happen if automigrate is false, so set it to NULL so that nothing happens
            Database.SetInitializer<Rock.Data.RockContext>( null );

            new RockContext().Database.Connection.Open();
        }
    }
}
