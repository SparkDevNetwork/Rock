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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Rock.Client;

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
        /// Handles the Click event of the toggleCurrency control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void toggleCurrency_Click( object sender, RoutedEventArgs e )
        {
            ToggleButton btnToggleSelected = sender as ToggleButton;

            // ensure only one toggle button is selected at a time
            foreach ( ToggleButton btnToggle in spTenderButtons.Children.OfType<ToggleButton>() )
            {
                btnToggle.IsChecked = btnToggle == btnToggleSelected;
            }

            var scanningChecks = ( Guid ) btnToggleSelected.Tag == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();
            UpdateUI( scanningChecks );
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
            rockConfig.EnableDoubleDocDetection = chkRangerDoubleDocDetection.IsChecked == true;
            rockConfig.EnableSmartScan = chkEnableSmartScan.IsChecked == true;

            if ( cboTransactionSourceType.SelectedItem == null )
            {
                lblTransactionSourceType.Style = this.FindResource( "labelStyleError" ) as Style;
                return;
            }
            else
            {
                lblTransactionSourceType.Style = this.FindResource( "labelStyle" ) as Style;
            }

            rockConfig.SourceTypeValueGuid = ( cboTransactionSourceType.SelectedItem as DefinedValue ).Guid.ToString();

            rockConfig.Save();

            switch (rockConfig.ScannerInterfaceType)
            {
                case RockConfig.InterfaceType.RangerApi:
                    if (this.BatchPage.rangerScanner != null)
                    {
                        this.BatchPage.rangerScanner.ShutDown();
                        this.BatchPage.rangerScanner.StartUp();
                    }
                    else
                    {

                        lblScannerDriverError.Visibility = Visibility.Visible;
                        return;
                    }
                    break;
                case RockConfig.InterfaceType.MICRImageRS232:
                    if (this.BatchPage.micrImage == null)
                    {
                        lblScannerDriverError.Visibility = Visibility.Visible;
                        return;
                    }
                    break;
                case RockConfig.InterfaceType.MagTekImageSafe:
                    //Do Nothing Uses a Callback Function 
                    break;
                default:
                    break;
            }

            this.NavigationService.Navigate( this.BatchPage.ScanningPage);
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
                ToggleButton toggleCurrency = new ToggleButton();
                toggleCurrency.Margin = new Thickness( 0, 12, 0, 0 );
                toggleCurrency.Padding = new Thickness( 0, 12, 0, 8 );
                toggleCurrency.Style = this.FindResource( "toggleButtonStyle" ) as Style;
                toggleCurrency.Content = currency.Value;
                toggleCurrency.Tag = currency.Guid;
                toggleCurrency.IsChecked = rockConfig.TenderTypeValueGuid.AsGuid() == currency.Guid;
                toggleCurrency.Click += toggleCurrency_Click;

                spTenderButtons.Children.Add( toggleCurrency );
            }

            cboTransactionSourceType.DisplayMemberPath = "Value";
            cboTransactionSourceType.ItemsSource = this.BatchPage.SourceTypeValueListSelectable.OrderBy( a => a.Order ).ThenBy( a => a.Value ).ToList();
            cboTransactionSourceType.SelectedItem = ( cboTransactionSourceType.ItemsSource as List<DefinedValue> ).FirstOrDefault( a => a.Guid == rockConfig.SourceTypeValueGuid.AsGuid() );

            var scanningChecks = rockConfig.TenderTypeValueGuid.AsGuid() == Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();

            UpdateUI(scanningChecks);
        }

        /// <summary>
        /// Updates the UI.
        /// </summary>
        /// <param name="scanningChecks">if set to <c>true</c> [scanning checks].</param>
        private void UpdateUI( bool scanningChecks )
        {
            RockConfig rockConfig = RockConfig.Load();
            bool supportsDoubleDocDetection;
            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MagTekImageSafe || rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 )
            {
                this.chkPromptToScanRearImage.Visibility = Visibility.Visible;
                supportsDoubleDocDetection = false;
            }
            else
            {
                this.chkPromptToScanRearImage.Visibility = Visibility.Hidden;
                supportsDoubleDocDetection = true;
            }

            if ( scanningChecks || supportsDoubleDocDetection )
            {
                this.spRangerOrCheckOptions.Visibility = Visibility.Visible;
            }
            else
            {
                this.spRangerOrCheckOptions.Visibility = Visibility.Collapsed;
            }

            if ( supportsDoubleDocDetection )
            {
                chkRangerDoubleDocDetection.Visibility = Visibility.Visible;
            }
            else
            {
                chkRangerDoubleDocDetection.Visibility = Visibility.Collapsed;
            }


            chkRangerDoubleDocDetection.IsChecked = scanningChecks;
            chkEnableSmartScan.Visibility = scanningChecks ? Visibility.Visible : Visibility.Collapsed;

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
