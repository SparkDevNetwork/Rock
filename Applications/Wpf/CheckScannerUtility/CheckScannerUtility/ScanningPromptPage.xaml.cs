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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rock.Model;
using Rock.Net;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for ScanningPromptPage.xaml
    /// </summary>
    public partial class ScanningPromptPage : System.Windows.Controls.Page
    {
        /// <summary>
        /// Gets or sets the batch page.
        /// </summary>
        /// <value>
        /// The batch page.
        /// </value>
        public BatchPage BatchPage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanningPromptPage"/> class.
        /// </summary>
        /// <param name="scanningPage">The scanning page.</param>
        public ScanningPromptPage( BatchPage batchPage )
        {
            InitializeComponent();
            this.BatchPage = batchPage;
        }

        /// <summary>
        /// Handles the Click event of the btnToggle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnToggle_Click( object sender, RoutedEventArgs e )
        {
            ToggleButton btnToggleSelected = sender as ToggleButton;

            // ensure only one toggle button is selected at a time
            foreach ( ToggleButton btnToggle in spTenderButtons.Children.OfType<ToggleButton>() )
            {
                btnToggle.IsChecked = btnToggle == btnToggleSelected;
            }

            
            var scanningChecks = (Guid)btnToggleSelected.Tag == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
            chkDoubleDocDetection.IsChecked = scanningChecks;
            chkEnableSmartScan.Visibility = scanningChecks ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnNext_Click( object sender, RoutedEventArgs e )
        {
            var rockConfig = RockConfig.Load();

            var selectedTenderButton = spTenderButtons.Children.OfType<ToggleButton>().Where( a => a.IsChecked == true ).FirstOrDefault();
            if ( selectedTenderButton != null )
            {
                rockConfig.TenderTypeValueGuid = selectedTenderButton.Tag.ToString();
            }

            rockConfig.EnableRearImage = radDoubleSided.IsChecked == true;
            rockConfig.PromptToScanRearImage = chkPromptToScanRearImage.IsChecked == true;
            rockConfig.EnableDoubleDocDetection = chkDoubleDocDetection.IsChecked == true;
            rockConfig.EnableSmartScan = chkEnableSmartScan.IsChecked == true;

            rockConfig.Save();

            // restart the scanner so that options will be reloaded
            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.RangerApi )
            {
                if ( this.BatchPage.rangerScanner != null )
                {
                    this.BatchPage.rangerScanner.ShutDown();
                    this.BatchPage.rangerScanner.StartUp();
                }
                else
                {
                    lblScannerDriverError.Visibility = Visibility.Visible;
                    return;
                }
            }
            else
            {
                if ( this.BatchPage.micrImage == null )
                {
                    lblScannerDriverError.Visibility = Visibility.Visible;
                    return;
                }
            }

            this.NavigationService.Navigate( this.BatchPage.ScanningPage );
        }

        /// <summary>
        /// Handles the Click event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnBack_Click( object sender, RoutedEventArgs e )
        {
            this.NavigationService.GoBack();
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            lblScannerDriverError.Visibility = Visibility.Collapsed;
            RockConfig rockConfig = RockConfig.Load();

            spTenderButtons.Children.Clear();
            foreach ( var currency in this.BatchPage.CurrencyValueList.OrderBy( a => a.Order ).ThenBy( a => a.Value ) )
            {
                ToggleButton btnToggle = new ToggleButton();
                btnToggle.Margin = new Thickness( 0, 12, 0, 0 );
                btnToggle.Padding = new Thickness( 0, 12, 0, 8 );
                btnToggle.Style = this.FindResource( "toggleButtonStyle" ) as Style;
                btnToggle.Content = currency.Value;
                btnToggle.Tag = currency.Guid;
                btnToggle.IsChecked = rockConfig.TenderTypeValueGuid.AsGuid() == currency.Guid;
                btnToggle.Click += btnToggle_Click;

                spTenderButtons.Children.Add( btnToggle );
            }

            chkDoubleDocDetection.IsChecked = rockConfig.TenderTypeValueGuid.AsGuid() == Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
            radDoubleSided.IsChecked = rockConfig.EnableRearImage;
            radSingleSided.IsChecked = !rockConfig.EnableRearImage;
            chkPromptToScanRearImage.IsChecked = rockConfig.PromptToScanRearImage;
            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.RangerApi )
            {
                spRangerScanSettings.Visibility = Visibility.Visible;
                spMagTekScanSettings.Visibility = Visibility.Collapsed;
            }
            else
            {
                spRangerScanSettings.Visibility = Visibility.Collapsed;
                spMagTekScanSettings.Visibility = Visibility.Visible;
            }
        }
    }
}
