//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Windows;
using System.Windows.Navigation;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Closing event of the mainWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void mainWindow_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            BatchPage batchPage = null;
            if ( mainWindow.Content is BatchPage )
            {
                batchPage = mainWindow.Content as BatchPage;
            }
            else if ( mainWindow.Content is ScanningPage )
            {
                batchPage = ( mainWindow.Content as ScanningPage ).batchPage;
            }

            if ( batchPage != null )
            {
                batchPage.RangerScanner.ShutDown();
            }

            Application.Current.Shutdown();
        }
    }
}
