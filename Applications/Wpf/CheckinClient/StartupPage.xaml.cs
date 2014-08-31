// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CheckinClient
{
    /// <summary>
    /// Interaction logic for StartupPage.xaml
    /// </summary>
    public partial class StartupPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartupPage"/> class.
        /// </summary>
        public StartupPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click( object sender, RoutedEventArgs e )
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStart_Click( object sender, RoutedEventArgs e )
        {
            // save settings
            RockConfig rockConfig = RockConfig.Load();
            rockConfig.CheckinAddress = txtCheckinAddress.Text;

            int cacheLabelDuration;
            if (int.TryParse(txtCacheLabelDuration.Text, out cacheLabelDuration))
            {
                rockConfig.CacheLabelDuration = cacheLabelDuration;
            }

            if ( txtPrinterOverrideIp.Text != string.Empty )
            {
                rockConfig.PrinterOverrideIp = txtPrinterOverrideIp.Text;
                rockConfig.PrinterOverrideLocal = string.Empty;
            }
            else
            {
                foreach ( Control control in spUsbPrinterList.Children )
                {
                    if ( control is ToggleButton )
                    {
                        ToggleButton tbControl = control as ToggleButton;
                        if ( tbControl.IsChecked != null && tbControl.IsChecked.Value == true )
                        {
                            rockConfig.PrinterOverrideLocal = tbControl.Content.ToString();
                        }
                    }
                }
            }

            rockConfig.Save();
            
            Uri uriTest;
            if ( !Uri.TryCreate(txtCheckinAddress.Text, UriKind.Absolute, out uriTest ))
            {
                MessageBox.Show( "You must enter a valid URL for the Check-in Address Field.", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation );
            }
            else
            {
                BrowserPage browserPage = new BrowserPage();
                this.NavigationService.Navigate( browserPage );
            }
        }

        /// <summary>
        /// Handles the Loaded event of the pStartupPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void pStartupPage_Loaded( object sender, RoutedEventArgs e )
        {
            int printerCounter = 0;

            RockConfig rockConfig = RockConfig.Load();

            // set save configuration settings
            txtCheckinAddress.Text = rockConfig.CheckinAddress;
            txtPrinterOverrideIp.Text = rockConfig.PrinterOverrideIp;
            txtCacheLabelDuration.Text = rockConfig.CacheLabelDuration.ToString();
            string localPrinterName = rockConfig.PrinterOverrideLocal;
            
            foreach ( string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters )
            {
                // MessageBox.Show( printer );
                ToggleButton btnToggle = new ToggleButton();
                btnToggle.Content = printer;
                btnToggle.Name = "btnPrinter" + printerCounter;
                btnToggle.Margin = new Thickness( 0, 12, 0, 0 );
                btnToggle.Padding = new Thickness( 0, 12, 0, 8 );
                btnToggle.FontSize = 18;
                btnToggle.Style = this.FindResource( "toggleButtonStyle" ) as Style;
                
                if ( printer == localPrinterName )
                {
                    btnToggle.IsChecked = true;
                }

                spUsbPrinterList.Children.Add( btnToggle );
                btnToggle.Click += new RoutedEventHandler( btnToggle_Click );
                printerCounter++;
            }
        }
        
        /// <summary>
        /// Handles the Click event of the btnToggle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnToggle_Click( object sender, RoutedEventArgs e )
        {
            ToggleButton btnClicked = sender as ToggleButton;

            // ensure only one toggle button is selected at a time
            foreach ( Control control in spUsbPrinterList.Children )
            {
                if ( control is ToggleButton && control.Name != btnClicked.Name )
                {
                    ToggleButton tbControl = control as ToggleButton;
                    tbControl.IsChecked = false;
                }
            }
        }
    }
}
