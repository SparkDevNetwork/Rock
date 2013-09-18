using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using Rock.Model;
using System.Linq;
using Rock.Data;
using System.Data.Entity;
using System.ComponentModel;

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

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
        }

        void bw_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            MessageBox.Show( "Done" );
        }

        void bw_DoWork( object sender, DoWorkEventArgs e )
        {
            ContributionForm contributionForm = new ContributionForm();

            // default Initializer is CreateDatabaseIfNotExists, but we don't want that to happen if automigrate is false, so set it to NULL so that nothing happens
            Database.SetInitializer<Rock.Data.RockContext>( null );

            // TODO - Get actual date range
            DateTime startDate = new DateTime( 2013, 1, 1 );
            DateTime endDate = new DateTime( 2013, 12, 1 );

            FinancialTransactionService financialTransactionService = new FinancialTransactionService();

            var qry = financialTransactionService.Queryable();

            qry = qry
                .Where( a => a.TransactionDateTime >= startDate )
                .Where( a => a.TransactionDateTime < endDate );

            Document document = contributionForm.CreateDocument( qry );

            PdfDocumentRenderer pdfDocumentRenderer = new PdfDocumentRenderer( false );
            pdfDocumentRenderer.Document = document;
            pdfDocumentRenderer.DocumentRenderer.PrepareDocumentProgress += DocumentRenderer_PrepareDocumentProgress;

            //pdfDocumentRenderer.DocumentRenderer.WorkingDirectory = "c:\temp";
            //pdfDocumentRenderer.PrepareRenderPages();
            pdfDocumentRenderer.RenderDocument();
            string fileName = "Statement_" + Guid.NewGuid().ToString( "N" ) + ".pdf";
            pdfDocumentRenderer.Save( fileName );

            Process.Start( fileName );
            
        }

        void DocumentRenderer_PrepareDocumentProgress( object sender, DocumentRenderer.PrepareDocumentProgressEventArgs e )
        {
            var progress = e.Value/e.Maximum;
            
        }
    }
}
