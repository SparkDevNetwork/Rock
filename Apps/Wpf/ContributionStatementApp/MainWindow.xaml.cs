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
            ContributionForm contributionForm = new ContributionForm();

            DateTime startDate = new DateTime(2013, 8, 1);
            DateTime endDate = new DateTime(2013, 9, 1);

            var qry = new FinancialTransactionService().Queryable( "TransactionDetails" )
                .Where( a => a.TransactionDateTime >= startDate )
                .Where( a => a.TransactionDateTime < endDate );
                
            qry = qry.Take(100);
            

            Document document = contributionForm.CreateDocument( qry.OrderBy( a => a.TransactionDateTime ).ToList());

            PdfDocumentRenderer pdfDocumentRenderer = new PdfDocumentRenderer(false);
            pdfDocumentRenderer.Document = document;
            pdfDocumentRenderer.RenderDocument();

            string fileName = "Statement_" + Guid.NewGuid().ToString("N") + ".pdf";
            pdfDocumentRenderer.Save( fileName );

            using ( MemoryStream ms = new MemoryStream() )
            {
                // doesn't work when pdfDocumentRenderer.Unicode = true
                pdfDocumentRenderer.Save( ms, true );
                byte[] pdfBytes = ms.GetBuffer();
            }

            Process.Start( fileName );
        }
    }
}
