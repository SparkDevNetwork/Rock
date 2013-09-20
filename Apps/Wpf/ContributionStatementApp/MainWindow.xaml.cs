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
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
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
        /// Toggles the fade.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="speed">The speed.</param>
        public void FadeIn( Control control, int speed = 0 )
        {
            // TODO Move this to a shared dll
            control.Opacity = 0;
            control.Visibility = Visibility.Visible;
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan( 0, 0, 0, 0, (int)speed );
            DoubleAnimation fadeInAnimation = new DoubleAnimation { From = 0.0, To = 1.0, Duration = new Duration( duration ) };
            Storyboard.SetTargetName( fadeInAnimation, control.Name );
            Storyboard.SetTargetProperty( fadeInAnimation, new PropertyPath( "Opacity", 1 ) );
            storyboard.Children.Add( fadeInAnimation );
            storyboard.Begin( control );
        }

        /// <summary>
        /// Fades the out.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="speed">The speed.</param>
        public void FadeOut( Control control, int speed = 2000 )
        {
            // TODO Move this to a shared dll
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan( 0, 0, 0, 0, (int)speed );
            DoubleAnimation fadeOutAnimation = new DoubleAnimation { From = 1.0, To = 0.0, Duration = new Duration( duration ) };
            Storyboard.SetTargetName( fadeOutAnimation, control.Name );
            Storyboard.SetTargetProperty( fadeOutAnimation, new PropertyPath( "Opacity", 0 ) );
            storyboard.Children.Add( fadeOutAnimation );

            EventHandler handleCompleted = new EventHandler( ( sender, e ) =>
            {
                control.Visibility = Visibility.Collapsed;
            } );

            storyboard.Completed += handleCompleted;
            storyboard.Begin( control );
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
            FadeIn( lblReportProgress, 2000 );
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
            FadeOut( lblReportProgress );
        }

        /// <summary>
        /// Handles the DoWork event of the bw control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        void bw_DoWork( object sender, DoWorkEventArgs e )
        {
            ContributionForm contributionForm = new ContributionForm();

            // default Initializer is CreateDatabaseIfNotExists, but we don't want that to happen if automigrate is false, so set it to NULL so that nothing happens
            Database.SetInitializer<Rock.Data.RockContext>( null );

            // TODO - Get actual date range

            string fileName = "Statement_" + Guid.NewGuid().ToString( "N" ) + ".pdf";
            contributionForm.StartDate = new DateTime( 2012, 9, 1 );
            contributionForm.EndDate = new DateTime( 2013, 9, 1 );

            FinancialTransactionService financialTransactionService = new FinancialTransactionService();

            var qry = financialTransactionService.Queryable();

            qry = qry
                .Where( a => a.TransactionDateTime >= contributionForm.StartDate )
                .Where( a => a.TransactionDateTime < contributionForm.EndDate );

            // limit to one person 2693 has 81 trans
            //qry = qry.Where( a => a.AuthorizedPersonId == 2693 );

            Stopwatch sw = Stopwatch.StartNew();
            Document document = contributionForm.CreateDocument( qry );
            sw.Stop();
            File.AppendAllText( "stats.log", string.Format( "File {0}, CreateDocument = {1} seconds\n", fileName, sw.Elapsed.TotalSeconds ) );
            sw.Restart();

            //MigraDoc.DocumentObjectModel.IO.DdlWriter.WriteToFile( document, Path.ChangeExtension( fileName, "mdddl" ) );

            PdfDocumentRenderer pdfDocumentRenderer = new PdfDocumentRenderer( false );
            pdfDocumentRenderer.Document = document;
            pdfDocumentRenderer.DocumentRenderer.PrepareDocumentProgress += DocumentRenderer_PrepareDocumentProgress;

            pdfDocumentRenderer.RenderDocument();
            sw.Stop();
            int personCount = qry.Select( a => a.AuthorizedPersonId ).Distinct().Count();
            File.AppendAllText( "stats.log", string.Format( "File {0}\n\tPageCount {1}\n\tRenderDocument() = {2} seconds\n",
                fileName,
                pdfDocumentRenderer.PageCount,
                sw.Elapsed.TotalSeconds ) );
            sw.Restart();
            pdfDocumentRenderer.Save( fileName );

            System.Threading.Thread.Sleep( 250 );
            Process.Start( fileName );
        }

        /// <summary>
        /// The current progress percent
        /// </summary>
        private decimal currentProgressPercent = 0;

        /// <summary>
        /// Handles the PrepareDocumentProgress event of the DocumentRenderer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DocumentRenderer.PrepareDocumentProgressEventArgs"/> instance containing the event data.</param>
        void DocumentRenderer_PrepareDocumentProgress( object sender, DocumentRenderer.PrepareDocumentProgressEventArgs e )
        {
            decimal percentProgress = decimal.Round( decimal.Divide( e.Value, e.Maximum ) * 100, 0 );
            if ( percentProgress != currentProgressPercent )
            {
                currentProgressPercent = percentProgress;
                Dispatcher.Invoke( new Action( UpdateProgressBar ) );
            }
        }

        /// <summary>
        /// Updates the progress bar.
        /// </summary>
        private void UpdateProgressBar()
        {
            if ( currentProgressPercent == 100 )
            {
                lblReportProgress.Content = "Render Complete - Preparing to save document";
            }
            else
            {
                lblReportProgress.Content = "Render Progress " + currentProgressPercent.ToString();
            }
        }
    }
}
