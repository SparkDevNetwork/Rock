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
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Rock.Model;
using Rock.Net;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for OptionsPage.xaml
    /// </summary>
    public partial class OptionsPage : System.Windows.Controls.Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPage"/> class.
        /// </summary>
        public OptionsPage( BatchPage batchPage )
        {
            InitializeComponent();
            this.BatchPage = batchPage;
        }

        /// <summary>
        /// Gets or sets the batch page.
        /// </summary>
        /// <value>
        /// The batch page.
        /// </value>
        private BatchPage BatchPage { get; set; }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            LoadDropDowns();

            lblAlert.Visibility = Visibility.Collapsed;

            var rockConfig = RockConfig.Load();

            txtRockUrl.Text = rockConfig.RockBaseUrl;

            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 )
            {
                cboScannerInterfaceType.SelectedItem = "MagTek";
                lblMakeModel.Content = "MagTek";

                string version = "-1";
                try
                {
                    this.Cursor = Cursors.Wait;
                    if ( BatchPage.micrImage != null )
                    {
                        version = BatchPage.micrImage.Version();
                    }
                }
                finally
                {
                    this.Cursor = null;
                }

                if ( !version.Equals( "-1" ) )
                {
                    lblInterfaceVersion.Content = version;
                }
                else
                {
                    lblInterfaceVersion.Content = "error";
                }
            }
            else
            {
                cboScannerInterfaceType.SelectedItem = "Ranger";
                if ( BatchPage.rangerScanner != null )
                {
                    lblMakeModel.Content = string.Format( "Scanner Type: {0} {1}", BatchPage.rangerScanner.GetTransportInfo( "General", "Make" ), BatchPage.rangerScanner.GetTransportInfo( "General", "Model" ) );
                    lblInterfaceVersion.Content = string.Format( "Interface Version: {0}", BatchPage.rangerScanner.GetVersion() );
                }
                else
                {
                    lblMakeModel.Content = "Scanner Type: ERROR";
                    lblInterfaceVersion.Content = "Interface Version: ERROR";
                }
            }

            string feederFriendlyNameType = BatchPage.ScannerFeederType.Equals( FeederType.MultipleItems ) ? "Multiple Items" : "Single Item";
            lblFeederType.Content = string.Format( "Feeder Type: {0}", feederFriendlyNameType );

            switch ( (ImageColorType)rockConfig.ImageColorType )
            {
                case ImageColorType.ImageColorTypeGrayscale:
                    cboImageOption.SelectedValue = "Grayscale";
                    break;
                case ImageColorType.ImageColorTypeColor:
                    cboImageOption.SelectedValue = "Color";
                    break;
                default:
                    cboImageOption.SelectedIndex = 0;
                    break;
            }

            if ( cboMagTekCommPort.Items.Count > 0 )
            {
                cboMagTekCommPort.SelectedItem = string.Format( "COM{0}", rockConfig.MICRImageComPort );
            }

            cboTransactionSourceType.SelectedItem = ( cboTransactionSourceType.ItemsSource as List<DefinedValue> ).FirstOrDefault( a => a.Guid == rockConfig.SourceTypeValueGuid.AsGuid() );
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            cboImageOption.Items.Clear();
            cboImageOption.Items.Add( "Black and Whilte" );
            cboImageOption.Items.Add( "Grayscale" );
            cboImageOption.Items.Add( "Color" );

            cboScannerInterfaceType.Items.Clear();
            if ( this.BatchPage.rangerScanner != null )
            {
                cboScannerInterfaceType.Items.Add( "Ranger" );
            }

            if ( this.BatchPage.micrImage != null )
            {
                cboScannerInterfaceType.Items.Add( "MagTek" );
            }

            if ( cboScannerInterfaceType.Items.Count == 0 )
            {
                lblScannerError.Visibility = Visibility.Visible;
                btnSave.IsEnabled = false;
            }
            else
            {
                lblScannerError.Visibility = Visibility.Collapsed;
                btnSave.IsEnabled = true;
            }

            cboMagTekCommPort.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();

            cboTransactionSourceType.Items.Clear();
            cboTransactionSourceType.ItemsSource = this.BatchPage.SourceTypeValueList.OrderBy( a => a.Order ).ThenBy( a => a.Value ).ToList();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSave_Click( object sender, RoutedEventArgs e )
        {
            RockConfig rockConfig = RockConfig.Load();

            try
            {
                txtRockUrl.Text = txtRockUrl.Text.Trim();
                Uri rockUrl = new Uri( txtRockUrl.Text );
                var validSchemes = new string[] { Uri.UriSchemeHttp, Uri.UriSchemeHttps };
                if ( !validSchemes.Contains( rockUrl.Scheme ) )
                {
                    txtRockUrl.Text = "http://" + rockUrl.AbsoluteUri;
                }

                RockRestClient client = new RockRestClient( txtRockUrl.Text );
                client.Login( rockConfig.Username, rockConfig.Password );
                BatchPage.LoggedInPerson = client.GetData<Person>( string.Format( "api/People/GetByUserName/{0}", rockConfig.Username ) );
                BatchPage.LoggedInPerson.Aliases = client.GetData<List<PersonAlias>>( "api/PersonAlias/", "PersonId eq " + BatchPage.LoggedInPerson.Id );
            }
            catch ( WebException wex )
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if ( response != null )
                {
                    if ( response.StatusCode == HttpStatusCode.Unauthorized )
                    {
                        // valid URL but invalid login, so navigate back to the LoginPage
                        rockConfig.RockBaseUrl = txtRockUrl.Text;
                        rockConfig.Save();
                        LoginPage loginPage = new LoginPage( true );
                        this.NavigationService.Navigate( loginPage );
                        return;
                    }
                }

                lblAlert.Content = wex.Message;
                lblAlert.Visibility = Visibility.Visible;
                return;
            }
            catch ( Exception ex )
            {
                App.LogException( ex );
                lblAlert.Content = ex.Message;
                lblAlert.Visibility = Visibility.Visible;
                return;
            }

            rockConfig.RockBaseUrl = txtRockUrl.Text;

            if ( cboScannerInterfaceType.SelectedItem.Equals( "MagTek" ) )
            {
                rockConfig.ScannerInterfaceType = RockConfig.InterfaceType.MICRImageRS232;
            }
            else
            {
                rockConfig.ScannerInterfaceType = RockConfig.InterfaceType.RangerApi;
            }

            string imageOption = cboImageOption.SelectedValue as string;

            switch ( imageOption )
            {
                case "Grayscale":
                    rockConfig.ImageColorType = ImageColorType.ImageColorTypeGrayscale;
                    break;
                case "Color":
                    rockConfig.ImageColorType = ImageColorType.ImageColorTypeColor;
                    break;
                default:
                    rockConfig.ImageColorType = ImageColorType.ImageColorTypeBitonal;
                    break;
            }

            string comPortName = cboMagTekCommPort.SelectedItem as string;

            if ( !string.IsNullOrWhiteSpace( comPortName ) )
            {
                rockConfig.MICRImageComPort = short.Parse( comPortName.Replace( "COM", string.Empty ) );
            }

            rockConfig.SourceTypeValueGuid = ( cboTransactionSourceType.SelectedItem as DefinedValue ).Guid.ToString();

            rockConfig.Save();

            // shutdown the scanner so that options will be reloaded when the batch page loads
            if ( BatchPage.rangerScanner != null )
            {
                BatchPage.rangerScanner.ShutDown();
            }

            BatchPage.ConnectToScanner();

            this.NavigationService.GoBack();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click( object sender, RoutedEventArgs e )
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
            ShowDetail();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cboScannerInterfaceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void cboScannerInterfaceType_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            bool magTekSelected = cboScannerInterfaceType.SelectedItem != null && cboScannerInterfaceType.SelectedItem.Equals( "MagTek" );

            // show COM port option only for Mag Tek
            lblMagTekCommPort.Visibility = magTekSelected ? Visibility.Visible : Visibility.Collapsed;
            cboMagTekCommPort.Visibility = magTekSelected ? Visibility.Visible : Visibility.Collapsed;

            // show Image Option only for Ranger
            lblImageOption.Visibility = magTekSelected ? Visibility.Collapsed : Visibility.Visible;
            cboImageOption.Visibility = magTekSelected ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
