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
using System.Linq;
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

            if ( batchPage != null && batchPage.rangerScanner != null)
            {
                batchPage.rangerScanner.ShutDown();
            }

            Application.Current.Shutdown();
        }
    }
}
