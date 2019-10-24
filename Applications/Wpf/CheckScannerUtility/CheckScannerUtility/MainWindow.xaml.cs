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
using ImageSafeInterop;
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
            var rockConfig = RockConfig.Load();
            Width = rockConfig.WindowCurrentWidth;
            Height = rockConfig.WindowCurrentHeight;

        }

        /// <summary>
        /// Handles the Closing event of the mainWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void mainWindow_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            //var rockConfig = RockConfig.Load();
            //var window = sender as NavigationWindow;
            //rockConfig.WindowCurrentHeight = window.ActualHeight;
            //rockConfig.WindowCurrentWidth = window.ActualWidth;
            //rockConfig.Save();
            try
            {
                ImageSafeHelper.CloseDevice();
            }
            catch 
            {
                // if image safe is not loaded then just ignore driver exception.
   
            }

            BatchPage batchPage = null;
            if ( mainWindow.Content is BatchPage )
            {
                batchPage = mainWindow.Content as BatchPage;
            }
            else if ( mainWindow.Content is ScanningPage )
            {
                batchPage = ( mainWindow.Content as ScanningPage )._batchPage;
            }

            if ( batchPage != null && batchPage.rangerScanner != null)
            {
                batchPage.rangerScanner.ShutDown();
              
            }

            Application.Current.Shutdown();
        }
    }
}
