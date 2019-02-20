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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ImageSafeInterop;
using Rock.Apps.CheckScannerUtility.Models;
using Rock.Client;
using Rock.Net;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for OptionsPage.xaml
    /// </summary>
    /// <seealso cref="System.Windows.Controls.Page" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class OptionsPage : System.Windows.Controls.Page
    {
        private RockRestClient _Client;
        private bool _campusChanged = false;
        private List<FinancialAccount> _allAccounts;
        private RockConfig _rockConfig;
        private ObservableCollection<DisplayAccountModel> _displayAccounts = new ObservableCollection<DisplayAccountModel>();
        private RockConfig.InterfaceType _interfaceType;
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPage"/> class.
        /// </summary>
        public OptionsPage( BatchPage batchPage )
        {
            InitializeComponent();
            this.BatchPage = batchPage;
            this._rockConfig = RockConfig.Load();
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
            LoadFinancialAccounts();
            LoadDeviceDropDown();
            LoadBatchCampusDropDown();
            lblAlert.Visibility = Visibility.Collapsed;
            GetConfigValues();

            string feederFriendlyNameType = BatchPage.ScannerFeederType.Equals( FeederType.MultipleItems ) ? "Multiple Items" : "Single Item";
            lblFeederType.Content = string.Format( "Feeder Type: {0}", feederFriendlyNameType );

        }

        private void LoadFinancialAccounts()
        {
            var client = this.RestClient;
            _allAccounts = client.GetData<List<FinancialAccount>>( "api/FinancialAccounts" );
            SetParentChildAccounts( _allAccounts );
            icAccountsForBatches.Items.Clear();
            icAccountsForBatches.ItemsSource = _displayAccounts;

        }

        private HybridDictionary _map = new HybridDictionary();

        public void AddParentChild( DisplayAccountModel displayAccount, int id, int? parentId )
        {
            // keep a map of each id to the node
            _map.Add( id, displayAccount );

            // if no parentId was given then it's a root node, so just add it
            if ( parentId == null )
            {
                _displayAccounts.Add( displayAccount );
            }
            else
            {
                // Find the parent in the map and add node as it's child node
                var parent = ( DisplayAccountModel ) _map[parentId];
                if ( parent.Children == null )
                {
                    parent.Children = new ObservableCollection<DisplayAccountModel>();
                }
                parent.Children.Add( displayAccount );
            }
        }

        private void SetParentChildAccounts( List<FinancialAccount> accounts )
        {
            foreach ( var account in accounts )
            {
                var parentDisplayAccount = new DisplayAccountModel();
                var children = _allAccounts.Where( a => a.ParentAccountId != null && a.ParentAccountId == account.Id ).ToList();
                parentDisplayAccount.AccountDisplayName = account.Name;
                if ( _rockConfig.SelectedAccountForAmountsIds != null )
                {
                    parentDisplayAccount.IsAccountChecked = _rockConfig.SelectedAccountForAmountsIds.Contains( account.Id );
                    parentDisplayAccount.Id = account.Id;
                }

                this.AddParentChild( parentDisplayAccount, account.Id, account.ParentAccountId );
            }
        }

        private void LoadBatchCampusDropDown()
        {
            var client = RestClient;
            var campusList = client.GetData<List<Campus>>( "api/Campuses" );
            cbDefaultCampus.SelectedValue = "Id";
            cbDefaultCampus.DisplayMemberPath = "Name";
            cbDefaultCampus.Items.Clear();
            foreach ( var campus in campusList.OrderBy( a => a.Name ) )
            {
                cbDefaultCampus.Items.Add( campus );
            }

            cbDefaultCampus.SelectedIndex = 0;

        }

        /// <summary>
        /// Gets the configuration values.
        /// These values are saved at
        /// C:\Users\[username]\AppData\Local\Spark_Development_Network
        /// </summary>
        private void GetConfigValues()
        {
            var rockConfig = RockConfig.Load();
            txtRockUrl.Text = rockConfig.RockBaseUrl;
            if ( rockConfig.CaptureAmountOnScan == true )
            {
                chkRequireConrolAmount.IsChecked = rockConfig.RequireControlAmount;
                chkRequireControlItemCount.IsChecked = rockConfig.RequireControlItemCount;
                chkCaptureAmountOnScan.IsChecked = rockConfig.CaptureAmountOnScan;
            }

            if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MICRImageRS232 )
            {
                cboScannerInterfaceType.SelectedItem = "MagTek COM";
                lblMakeModel.Content = "MagTek COM";
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
            else if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.MagTekImageSafe )
            {
                cboScannerInterfaceType.SelectedItem = "MagTek Image Safe";
                lblMakeModel.Content = "MagTek Image Safe";
                string version = "-1";
                try
                {

                    this.Cursor = Cursors.Wait;
                 
                    version = BatchPage.GetImageSafeVersion();
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

            switch ( ( RangerImageColorTypes ) rockConfig.ImageColorType )
            {
                case RangerImageColorTypes.ImageColorTypeGrayscale:
                    cboImageOption.SelectedValue = "Grayscale";
                    break;
                case RangerImageColorTypes.ImageColorTypeColor:
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

            if ( rockConfig.Sensitivity.AsInteger() == 0 )
            {
                txtSensitivity.Text = string.Empty;
            }
            else
            {
                txtSensitivity.Text = rockConfig.Sensitivity;
            }

            if ( rockConfig.Plurality.AsInteger() == 0 )
            {
                txtPlurality.Text = string.Empty;
            }
            else
            {
                txtPlurality.Text = rockConfig.Plurality;
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDeviceDropDown()
        {
            cboImageOption.Items.Clear();
            cboImageOption.Items.Add( "Black and White" );
            cboImageOption.Items.Add( "Grayscale" );
            cboImageOption.Items.Add( "Color" );

            cboScannerInterfaceType.Items.Clear();
            if ( this.BatchPage.rangerScanner != null )
            {
                cboScannerInterfaceType.Items.Add( "Ranger" );
            }

            if ( this.BatchPage.micrImage != null )
            {
                cboScannerInterfaceType.Items.Add( "MagTek COM" );
            }

            if ( ImageSafeHelper.OpenDevice())
            {
                cboScannerInterfaceType.Items.Add( "MagTek Image Safe" );
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
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSave_Click( object sender, RoutedEventArgs e )
        {

            _rockConfig.CaptureAmountOnScan = chkCaptureAmountOnScan.IsChecked == true;
      
            _rockConfig.RequireControlAmount = chkRequireConrolAmount.IsChecked == true;
            _rockConfig.RequireControlItemCount = chkRequireControlItemCount.IsChecked == true;
            AddAccountsForAmountsToSave();
            if ( _rockConfig.CaptureAmountOnScan && _rockConfig.SelectedAccountForAmountsIds.Count() == 0 )
            {
                MessageBox.Show( "Capture Amount selected but no accounts checked" );
                return;
            }
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
                client.Login( _rockConfig.Username, _rockConfig.Password );
                BatchPage.LoggedInPerson = client.GetData<Person>( string.Format( "api/People/GetByUserName/{0}", _rockConfig.Username ) );
            }
            catch ( WebException wex )
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if ( response != null )
                {
                    if ( response.StatusCode == HttpStatusCode.Unauthorized )
                    {
                        // valid URL but invalid login, so navigate back to the LoginPage
                        _rockConfig.RockBaseUrl = txtRockUrl.Text;
                        _rockConfig.Save();
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

            _rockConfig.RockBaseUrl = txtRockUrl.Text;

            switch ( cboScannerInterfaceType.SelectedItem as string )
            {
                case "MagTek COM":
                    _rockConfig.ScannerInterfaceType = RockConfig.InterfaceType.MICRImageRS232;
                    break;
                case "MagTek Image Safe":
                    _rockConfig.ScannerInterfaceType = RockConfig.InterfaceType.MagTekImageSafe;
                    break;
                default:
                    _rockConfig.ScannerInterfaceType = RockConfig.InterfaceType.RangerApi;
                    break;
            }

            string imageOption = cboImageOption.SelectedValue as string;

            _rockConfig.Sensitivity = txtSensitivity.Text.Trim().AsInteger().ToString();
            _rockConfig.Plurality = txtPlurality.Text.Trim().AsInteger().ToString();

            switch ( imageOption )
            {
                case "Grayscale":
                    _rockConfig.ImageColorType = RangerImageColorTypes.ImageColorTypeGrayscale;
                    break;
                case "Color":
                    _rockConfig.ImageColorType = RangerImageColorTypes.ImageColorTypeColor;
                    break;
                default:
                    _rockConfig.ImageColorType = RangerImageColorTypes.ImageColorTypeBitonal;
                    break;
            }

            string comPortName = cboMagTekCommPort.SelectedItem as string;

            if ( !string.IsNullOrWhiteSpace( comPortName ) )
            {
                _rockConfig.MICRImageComPort = short.Parse( comPortName.Replace( "COM", string.Empty ) );
            }

            var defaultCampus = cbDefaultCampus.SelectedValue as Campus;
            if (defaultCampus != null)
            {
                _rockConfig.DefaultCampusId = defaultCampus.Id;
            }

            _rockConfig.Save();
            BatchPage.LoadLookups(_campusChanged);

            // shutdown the scanner so that options will be reloaded when the batch page loads
            if ( BatchPage.rangerScanner != null )
            {
                BatchPage.rangerScanner.ShutDown();
            }

            BatchPage.UnbindAllEvents();
            BatchPage.BindDeviceToPage();
            BatchPage.ConnectToScanner();

            this.NavigationService.Navigate(BatchPage);
        }

        private void AddAccountsForAmountsToSave()
        {
            List<int> selectedAccounts = new List<int>();
            foreach ( var item in icAccountsForBatches.Items )
            {
                var displayAccount = item as DisplayAccountModel;
                SetParentChildIdsToSave( displayAccount, ref selectedAccounts );
            }

            _rockConfig.SelectedAccountForAmountsIds = selectedAccounts.ToArray();

        }

        /// <summary>
        /// Sets the parent child ids to save.
        /// Accounts Are Hierarchical so we have to recursively walk the object
        /// </summary>
        /// <param name="displayAccount">The display account.</param>
        private void SetParentChildIdsToSave( DisplayAccountModel displayAccount, ref List<int> selectedAccounts )
        {
            if ( displayAccount != null && displayAccount.IsAccountChecked == true )
            {
                selectedAccounts.Add( displayAccount.Id );
            }

            if ( displayAccount.Children != null )
            {
                foreach ( var child in displayAccount.Children )
                {
                    SetParentChildIdsToSave( child, ref selectedAccounts );
                }
            }


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
            lblMagTekCommPort.Visibility = Visibility.Collapsed;
            cboMagTekCommPort.Visibility = Visibility.Collapsed;
            ShowDetail();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cboScannerInterfaceType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void cboScannerInterfaceType_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            // show Image Option only for Ranger so default Collapsed for both MagTeks
            lblImageOption.Visibility = Visibility.Hidden;
            cboImageOption.Visibility = Visibility.Hidden;

            // show Image Option only for Ranger so default Collapsed
            lblAdvancedInfo.Visibility = Visibility.Hidden;
            lblSensitivity.Visibility = Visibility.Hidden;
            txtSensitivity.Visibility = Visibility.Hidden;
            lblPlurality.Visibility = Visibility.Hidden;
            txtPlurality.Visibility = Visibility.Hidden;

            if ( cboScannerInterfaceType.SelectedItem != null )
            {
                switch ( cboScannerInterfaceType.SelectedItem.ToString() )
                {

                    case "MagTek COM":
                        // show COM port option only for Mag Tek COM
                        lblMagTekCommPort.Visibility = Visibility.Visible;
                        cboMagTekCommPort.Visibility = Visibility.Visible;
                        lblSensitivity.Visibility = Visibility.Collapsed;
                        txtSensitivity.Visibility = Visibility.Collapsed;
                        lblPlurality.Visibility = Visibility.Collapsed;
                        txtPlurality.Visibility = Visibility.Collapsed;
                        break;
                    case "MagTek Image Safe":
                        //No COM Port for USB
                        lblMagTekCommPort.Visibility = Visibility.Collapsed;
                        cboMagTekCommPort.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        // show Image Option only for Ranger
                        lblImageOption.Visibility = Visibility.Visible;
                        cboImageOption.Visibility = Visibility.Visible;
                        lblMagTekCommPort.Visibility = Visibility.Collapsed;
                        cboMagTekCommPort.Visibility = Visibility.Collapsed;
                        // show Sensitivity/Plurality Option only for Ranger
                        lblAdvancedInfo.Visibility = Visibility.Visible;
                        lblSensitivity.Visibility = Visibility.Visible;
                        txtSensitivity.Visibility = Visibility.Visible;
                        lblPlurality.Visibility = Visibility.Visible;
                        txtPlurality.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private RockRestClient RestClient
        {
            get
            {
                if ( _Client == null )
                {
                    RockConfig rockConfig = RockConfig.Load();
                    _Client = new RockRestClient( rockConfig.RockBaseUrl );
                    _Client.Login( rockConfig.Username, rockConfig.Password );

                }
                return _Client;
            }
        }

        private void ChkCaptureAmountOnScan_Unchecked( object sender, RoutedEventArgs e )
        {
            chkRequireConrolAmount.IsChecked = false;
            chkRequireControlItemCount.IsChecked = false;
        }

        private void Page_SizeChanged( object sender, SizeChangedEventArgs e )
        {

            this.icAccountsForBatches.Height = sp_ScannerSettings.Height - 60;
        }

        private void CbDefaultCampus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._campusChanged = true;
        }
    }
}
