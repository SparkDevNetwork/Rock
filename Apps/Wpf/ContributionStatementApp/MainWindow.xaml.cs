using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;

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
            Document document = contributionForm.CreateDocument();

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
