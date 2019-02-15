﻿// <copyright>
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
using Rock.Client;
using Rock.Client.Enums;
using Rock.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml;

namespace Rock.Apps.CheckScannerUtility
{

    /// <summary>
    /// Interaction logic for BatchPage.xaml
    /// </summary>
    public partial class BatchPage : System.Windows.Controls.Page
    {
        private static NavigationService _navigationService;
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchPage" /> class.
        /// </summary>
        /// <param name="loggedInPerson">The logged in person.</param>
        public BatchPage(Person loggedInPerson)
        {


            var rockConfig = RockConfig.Load();
            LoggedInPerson = loggedInPerson;
            InitializeComponent();
            ScanningPage = new ScanningPage(this);
            CaptureAmountScanningPage = new CaptureAmountScanningPage(this);
            ScanningPromptPage = new ScanningPromptPage(this);
            BatchItemDetailPage = new BatchItemDetailPage();
            FirstPageLoad = true;

            try
            {
                var micrImageHostPage = new MicrImageHostPage();
                this.micrImage = micrImageHostPage.micrImage;
            }
            catch
            {
                // intentionally nothing.  means they don't have the MagTek driver
            }
            try
            {
                var rangerScannerHostPage = new RangerScannerHostPage();
                this.rangerScanner = rangerScannerHostPage.rangerScanner;
           
                if ( rockConfig.ScannerInterfaceType == RockConfig.InterfaceType.RangerApi)
                {
                    this.rangerScanner.TransportNewState += rangerScanner_TransportNewState;
                    this.rangerScanner.TransportChangeOptionsState += rangerScanner_TransportChangeOptionsState;

                    // debug output only
                    this.rangerScanner.TransportEnablingOptionsState += rangerScannerHostPage.rangerScanner_TransportEnablingOptionsState;
                    this.rangerScanner.TransportExceptionComplete += rangerScannerHostPage.rangerScanner_TransportExceptionComplete;
                    this.rangerScanner.TransportInExceptionState += rangerScannerHostPage.rangerScanner_TransportInExceptionState;

                    this.rangerScanner.TransportItemInPocket += rangerScannerHostPage.rangerScanner_TransportItemInPocket;
                    this.rangerScanner.TransportItemSuspended += rangerScannerHostPage.rangerScanner_TransportItemSuspended;
                    this.rangerScanner.TransportOverrideOptions += rangerScannerHostPage.rangerScanner_TransportOverrideOptions;
                    this.rangerScanner.TransportPassthroughEvent += rangerScannerHostPage.rangerScanner_TransportPassthroughEvent;
                    this.rangerScanner.TransportReadyToFeedState += rangerScannerHostPage.rangerScanner_TransportReadyToFeedState;
                    this.rangerScanner.TransportReadyToSetEndorsement += rangerScannerHostPage.rangerScanner_TransportReadyToSetEndorsement;
                    this.rangerScanner.TransportShuttingDownState += rangerScannerHostPage.rangerScanner_TransportShuttingDownState;
                    this.rangerScanner.TransportShutDownState += rangerScannerHostPage.rangerScanner_TransportShutDownState;
                    this.rangerScanner.TransportStartingUpState += rangerScannerHostPage.rangerScanner_TransportStartingUpState;
                    this.rangerScanner.TransportTrackIsClear += rangerScannerHostPage.rangerScanner_TransportTrackIsClear;
                }

                UnbindAllEvents();
                BindDeviceToPage();
            }
            catch
            {
                // intentionally nothing.  means they don't have the Ranger driver
            }
        }


        public void BindDeviceToPage()
        {
            var rockConfig = RockConfig.Load();
            if (rockConfig.CaptureAmountOnScan == true)
            {

                this.BindDeviceToCaptureAmountScreen();
            }
            else
            {
                this.BindDeviceToScanningPage();
            }
        }

        public void UnbindAllEvents()
        {

            //Unbind to ScannigScreen
            if (this.micrImage != null)
            {
                this.micrImage.MicrDataReceived -= ScanningPage.micrImage_MicrDataReceived;
            }
            
            if (this.rangerScanner != null)
            {
                this.rangerScanner.TransportFeedingState -= ScanningPage.rangerScanner_TransportFeedingState;
                this.rangerScanner.TransportFeedingStopped -= ScanningPage.rangerScanner_TransportFeedingStopped;
                this.rangerScanner.TransportNewItem -= ScanningPage.rangerScanner_TransportNewItem;
                this.rangerScanner.TransportSetItemOutput -= ScanningPage.rangerScanner_TransportSetItemOutput;
                this.rangerScanner.TransportIsDead -= ScanningPage.rangerScanner_TransportIsDead;
            }

            //Unbind to Capture Screen
            if (this.micrImage != null)
            {

                this.micrImage.MicrDataReceived -= CaptureAmountScanningPage.micrImage_MicrDataReceived;
            }
            if (this.rangerScanner != null)
            {  //  this.ImageSafe.OnDocumentProcessComplete -= CaptureAmountScanningPage.ImageSafe_OnDocumentProcessComplete;
                this.rangerScanner.TransportFeedingState -= CaptureAmountScanningPage.rangerScanner_TransportFeedingState;
                this.rangerScanner.TransportFeedingStopped -= CaptureAmountScanningPage.rangerScanner_TransportFeedingStopped;
                this.rangerScanner.TransportNewItem -= CaptureAmountScanningPage.rangerScanner_TransportNewItem;
                this.rangerScanner.TransportSetItemOutput -= CaptureAmountScanningPage.rangerScanner_TransportSetItemOutput;
                this.rangerScanner.TransportIsDead -= CaptureAmountScanningPage.rangerScanner_TransportIsDead;
            }


        }
        private void BindDeviceToScanningPage()
        {
            //Bind to Scanning Screen
            if (this.micrImage != null)
            {

                this.micrImage.MicrDataReceived += ScanningPage.micrImage_MicrDataReceived;
            }

            if (this.rangerScanner != null)
            {
                this.rangerScanner.TransportFeedingState += ScanningPage.rangerScanner_TransportFeedingState;
                this.rangerScanner.TransportFeedingStopped += ScanningPage.rangerScanner_TransportFeedingStopped;
                this.rangerScanner.TransportNewItem += ScanningPage.rangerScanner_TransportNewItem;
                this.rangerScanner.TransportSetItemOutput += ScanningPage.rangerScanner_TransportSetItemOutput;
                this.rangerScanner.TransportIsDead += ScanningPage.rangerScanner_TransportIsDead;
            }
        }

        private void BindDeviceToCaptureAmountScreen()
        {
            if (this.micrImage != null)
            {
                this.micrImage.MicrDataReceived -= CaptureAmountScanningPage.micrImage_MicrDataReceived;
                this.micrImage.MicrDataReceived += CaptureAmountScanningPage.micrImage_MicrDataReceived;
            }

            if (this.rangerScanner != null)
            {
                this.rangerScanner.TransportFeedingState += CaptureAmountScanningPage.rangerScanner_TransportFeedingState;
                this.rangerScanner.TransportFeedingStopped += CaptureAmountScanningPage.rangerScanner_TransportFeedingStopped;
                this.rangerScanner.TransportNewItem += CaptureAmountScanningPage.rangerScanner_TransportNewItem;
                this.rangerScanner.TransportSetItemOutput += CaptureAmountScanningPage.rangerScanner_TransportSetItemOutput;
                this.rangerScanner.TransportIsDead += CaptureAmountScanningPage.rangerScanner_TransportIsDead;
            }
        }

        /// <summary>
        /// Gets or sets the micr image.
        /// </summary>
        /// <value>
        /// The micr image.
        /// </value>
        public AxMTMicrImage.AxMicrImage micrImage { get; set; }


        /// <summary>
        /// Gets or sets the ranger scanner.
        /// </summary>
        /// <value>
        /// The ranger scanner.
        /// </value>
        public AxRANGERLib.AxRanger rangerScanner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [first page load].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [first page load]; otherwise, <c>false</c>.
        /// </value>
        private bool FirstPageLoad { get; set; }

        /// <summary>
        /// Gets or sets the selected financial batch
        /// </summary>
        /// <value>
        /// The selected financial batch
        /// </value>
        public FinancialBatch SelectedFinancialBatch { get; set; }

        /// <summary>
        /// Gets or sets the logged in person id.
        /// </summary>
        /// <value>
        /// The logged in person id.
        /// </value>
        public Person LoggedInPerson { get; set; }

        /// <summary>
        /// Gets or sets the type of the feeder.
        /// </summary>
        /// <value>
        /// The type of the feeder.
        /// </value>
        public FeederType ScannerFeederType { get; set; }

        /// <summary>
        /// The scanning page
        /// </summary>
        public ScanningPage ScanningPage { get; set; }

        public CaptureAmountScanningPage CaptureAmountScanningPage { get; set; }

        /// <summary>
        /// Gets or sets the scanning prompt page.
        /// </summary>
        /// <value>
        /// The scanning prompt page.
        /// </value>
        public ScanningPromptPage ScanningPromptPage { get; set; }

        /// <summary>
        /// Gets or sets the batch item detail page.
        /// </summary>
        /// <value>
        /// The batch item detail page.
        /// </value>
        public BatchItemDetailPage BatchItemDetailPage { get; set; }

        /// <summary>
        /// The currency value list
        /// </summary>
        public List<DefinedValue> CurrencyValueList { get; set; }

        /// <summary>
        /// Gets the selected currency value.
        /// </summary>
        /// <value>
        /// The selected currency value.
        /// </value>
        public DefinedValue SelectedCurrencyValue
        {
            get
            {
                return CurrencyValueList.FirstOrDefault(a => a.Guid == RockConfig.Load().TenderTypeValueGuid.AsGuid());
            }
        }

        /// <summary>
        /// All the possible Transation Source Type, including ones that can't be selected when scanning
        /// </summary>
        /// <value>
        /// The source type value list.
        /// </value>
        public List<DefinedValue> SourceTypeValueList { get; set; }

        /// <summary>
        /// The Transaction Source Types that can be selected from when scanning
        /// </summary>
        /// <value>
        /// The source type value list selectable.
        /// </value>
        public List<DefinedValue> SourceTypeValueListSelectable
        {
            get
            {
                if (this.SourceTypeValueList != null)
                {
                    return this.SourceTypeValueList.Where(a => a.AttributeValues.ContainsKey("core.ShowInCheckScanner") ? a.AttributeValues["core.ShowInCheckScanner"].Value.AsBoolean() : false).ToList();
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the selected source type value.
        /// </summary>
        /// <value>
        /// The selected source type value.
        /// </value>
        public DefinedValue SelectedSourceTypeValue
        {
            get
            {
                return this.SourceTypeValueList.FirstOrDefault(a => a.Guid == RockConfig.Load().SourceTypeValueGuid.AsGuid());
            }
        }

        public NavigationService BatchNavigationService
        {
            get { return _navigationService; }
        }



        #region Ranger (Canon CR50/80) Scanner Events

        /// <summary>
        /// Rangers the new state of the scanner_ transport.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void rangerScanner_TransportNewState(object sender, AxRANGERLib._DRangerEvents_TransportNewStateEvent e)
        {
            ScanningPage.btnClose.Visibility = Visibility.Visible;

            string status = rangerScanner.GetTransportStateString().Replace("Transport", string.Empty).SplitCase();
            Color statusColor = Colors.Transparent;

            RangerTransportStates xportState = (RangerTransportStates)e.currentState;

            switch (xportState)
            {
                case RangerTransportStates.TransportReadyToFeed:
                    statusColor = Colors.LimeGreen;
                    btnScan.Content = "Scan";
                    break;
                case RangerTransportStates.TransportShutDown:
                    statusColor = Colors.Red;
                    break;
                case RangerTransportStates.TransportFeeding:
                    statusColor = Colors.Blue;
                    btnScan.Content = "Stop";
                    break;
                case RangerTransportStates.TransportStartingUp:
                    statusColor = Colors.Yellow;
                    break;
                case RangerTransportStates.TransportExceptionInProgress:
                    statusColor = Colors.Black;
                    break;
                default:
                    statusColor = Colors.White;
                    break;
            }

            this.shapeStatus.Fill = new SolidColorBrush(statusColor);
            this.shapeStatus.ToolTip = status;

            ScanningPage.ShowScannerStatus(xportState, statusColor, status);
            CaptureAmountScanningPage.ShowScannerStatus(xportState, statusColor, status);
        }

        /// <summary>
        /// Rangers the state of the scanner_ transport change options.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void rangerScanner_TransportChangeOptionsState(object sender, AxRANGERLib._DRangerEvents_TransportChangeOptionsStateEvent e)
        {
          
            if (e.previousState == (int)RangerTransportStates.TransportStartingUp)
            {
                // enable imaging
                rangerScanner.SetGenericOption("OptionalDevices", "NeedImaging", "True");

                // limit splash screen
                rangerScanner.SetGenericOption("Ranger GUI", "DisplaySplashOncePerDay", "true");

                // turn on either color, grayscale, or bitonal(black and white) options depending on selected option
                rangerScanner.SetGenericOption("OptionalDevices", "NeedFrontImage1", "False");
                rangerScanner.SetGenericOption("OptionalDevices", "NeedRearImage1", "False");
                rangerScanner.SetGenericOption("OptionalDevices", "NeedFrontImage2", "False");
                rangerScanner.SetGenericOption("OptionalDevices", "NeedRearImage2", "False");
                rangerScanner.SetGenericOption("OptionalDevices", "NeedFrontImage3", "False");
                rangerScanner.SetGenericOption("OptionalDevices", "NeedRearImage3", "False");
                rangerScanner.SetGenericOption("OptionalDevices", "NeedFrontImage4", "False");
                rangerScanner.SetGenericOption("OptionalDevices", "NeedRearImage4", "False");

                var rockConfig = RockConfig.Load();
                switch (rockConfig.ImageColorType)
                {
                    case RangerImageColorTypes.ImageColorTypeColor:
                        rangerScanner.SetGenericOption("OptionalDevices", "NeedFrontImage3", "True");
                        rangerScanner.SetGenericOption("OptionalDevices", "NeedRearImage3", rockConfig.EnableRearImage.ToTrueFalse());
                        break;
                    case RangerImageColorTypes.ImageColorTypeGrayscale:
                        rangerScanner.SetGenericOption("OptionalDevices", "NeedFrontImage2", "True");
                        rangerScanner.SetGenericOption("OptionalDevices", "NeedRearImage2", rockConfig.EnableRearImage.ToTrueFalse());
                        break;
                    default:
                        rangerScanner.SetGenericOption("OptionalDevices", "NeedFrontImage1", "True");
                        rangerScanner.SetGenericOption("OptionalDevices", "NeedRearImage1", rockConfig.EnableRearImage.ToTrueFalse());
                        break;
                }

                rangerScanner.SetGenericOption("OptionalDevices", "NeedDoubleDocDetection", rockConfig.EnableDoubleDocDetection.ToTrueFalse());

                // Ranger assigns a score of 1-255 on how confident it is that the character was read correctly (1 unsure, 255 very sure)
                // If the score is less than 255, it will assign another score to its next best guess.  
                // For example, if it pretty sure it was a '3', but it thinks it might have been an '8', it might set the score for '3' as 240, but a score of 150 to '8'.
                // If the difference (Plurality) between the scores isn't high enough, it will reject the char. 
                rangerScanner.SetDriverOption("MICR", "Sensitivity", rockConfig.Sensitivity);
                rangerScanner.SetDriverOption("MICR", "Plurality", rockConfig.Plurality);

                rangerScanner.EnableOptions();
            }
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void batchPage_Loaded(object sender, RoutedEventArgs e)
        {
            _navigationService = this.NavigationService;
            spBatchDetailReadOnly.Visibility = Visibility.Visible;
            spBatchDetailEdit.Visibility = Visibility.Collapsed;

            if (this.FirstPageLoad)
            {
                if (!ConnectToScanner())
                {
                    NavigateToOptionsPage();
                }

                this.FirstPageLoad = false;
            }

            if (!string.IsNullOrEmpty(txtControlItemCount.Text))
            {
                if (int.TryParse(txtControlItemCount.Text, out int itemsToProcess))
                {
                    ScanningPageUtility.ItemsToProcess = itemsToProcess;
                }
            }
            CheckBatchCompleted();

        }

        private void SetAmountColVisibilty()
        {
            DataGridTemplateColumn amountCol = this.grdBatchItems.Columns.Where(c => c.Header.ToString() == "Amount") as DataGridTemplateColumn;
            if (amountCol != null)
            {
                amountCol.Visibility = Visibility.Hidden;
            }
        }

        private void CheckBatchCompleted()
        {
            RockConfig rockConfig = RockConfig.Load();

            if (rockConfig.CaptureAmountOnScan && ScanningPageUtility.ItemsToProcess != null && ScanningPageUtility.ItemsUploaded == ScanningPageUtility.ItemsToProcess)
            {
                btnScan.IsEnabled = false;
            }

        }

        /// <summary>
        /// Loads the combo boxes.
        /// </summary>
        public void LoadLookups(bool defaultCampusChanged = false)
        {
            RockConfig rockConfig = RockConfig.Load();
            RockRestClient client = new RockRestClient(rockConfig.RockBaseUrl);
            client.Login(rockConfig.Username, rockConfig.Password);
            List<Campus> campusList = client.GetData<List<Campus>>("api/Campuses");

            cbCampus.SelectedValuePath = "Id";
            cbCampus.DisplayMemberPath = "Name";
            cbCampus.Items.Clear();
            cbCampus.Items.Add(new Campus { Id = 0, Name = string.Empty });
            foreach (var campus in campusList.OrderBy(a => a.Name))
            {
                cbCampus.Items.Add(campus);
            }

            // If the user changes the Default Campus from the Options Screen
            // Then update the Selected Campus
            if (defaultCampusChanged && rockConfig.DefaultCampusId > 0)
            {
                cbCampus.SelectedValue = rockConfig.DefaultCampusId;
                var selectedCampus = cbCampus.SelectedItem as Campus;

                if (this.SelectedFinancialBatch != null)
                {
                    this.SelectedFinancialBatch.Campus = selectedCampus;
                }

            }

            var currencyTypeDefinedType = client.GetDataByGuid<DefinedType>("api/DefinedTypes", Rock.Client.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE.AsGuid());
            this.CurrencyValueList = client.GetData<List<DefinedValue>>("api/DefinedValues", "DefinedTypeId eq " + currencyTypeDefinedType.Id.ToString());

            //// load all the Transaction Source values and fetch the attributes so that we can filter the selectable ones (core.ShowInCheckScanner=true) 
            //// when populating the dropdown on the scanning prompt page.
            //// don't filter them here because we might be viewing transactions that have an unselectable Transaction Source Type
            var sourceTypeDefinedType = client.GetDataByGuid<DefinedType>("api/DefinedTypes", Rock.Client.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE.AsGuid());
            var sourceTypeUri = string.Format("api/DefinedValues?$filter=DefinedTypeId eq {0}&loadAttributes=simple", sourceTypeDefinedType.Id.ToString());
            this.SourceTypeValueList = client.GetData<List<DefinedValue>>(sourceTypeUri);
        }

        /// <summary>
        /// Loads the financial batches grid.
        /// </summary>
        public void LoadFinancialBatchesGrid()
        {
            RockConfig config = RockConfig.Load();
            RockRestClient client = new RockRestClient(config.RockBaseUrl);


            client.Login(config.Username, config.Password);
            List<FinancialBatch> pendingBatches = client.GetDataByEnum<List<FinancialBatch>>("api/FinancialBatches", "Status", BatchStatus.Pending);

            // Order by Batch Id starting with most recent
            grdBatches.DataContext = pendingBatches.OrderByDescending(a => a.Id);
            if (pendingBatches.Count > 0)
            {
                if (SelectedFinancialBatch != null)
                {
                    // try to set the selected batch in the grid to our current batch (if it still exists in the database)
                    grdBatches.SelectedValue = pendingBatches.FirstOrDefault(a => a.Id.Equals(SelectedFinancialBatch.Id));
                    FinancialBatch selectedBatch = grdBatches.SelectedValue as FinancialBatch;

                    ScanningPageUtility.ItemsToProcess = selectedBatch == null ? 0 : selectedBatch.ControlItemCount;

                }

                // if there still isn't a selected batch, set it to the first one
                if (grdBatches.SelectedValue == null)
                {
                    grdBatches.SelectedIndex = 0;
                }
            }
            else
            {
                SelectedFinancialBatch = null;
            }

            bool startWithNewBatch = !pendingBatches.Any();
            if (startWithNewBatch)
            {
                // don't let them start without having at least one batch, so just show the list with the Add button
                HideBatch();
            }
            else
            {
                gBatchDetailList.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Updates the scanner status for magtek.
        /// </summary>
        /// <param name="connected">if set to <c>true</c> [connected].</param>
        private void UpdateScannerStatusForMagtek(bool connected)
        {
            string status;
            Color statusColor;

            if (connected)
            {
                statusColor = Colors.LimeGreen;
                status = "Connected";
            }
            else
            {
                statusColor = Colors.Red;
                status = "Disconnected";
            }

            this.shapeStatus.ToolTip = status;
            this.shapeStatus.Fill = new SolidColorBrush(statusColor);

            ScanningPage.ShowScannerStatus(connected ? RangerTransportStates.TransportReadyToFeed : RangerTransportStates.TransportShutDown, statusColor, status);
            CaptureAmountScanningPage.ShowScannerStatus(connected ? RangerTransportStates.TransportReadyToFeed : RangerTransportStates.TransportShutDown, statusColor, status);
        }

        /// <summary>
        /// Connects to scanner.
        /// </summary>
        /// <returns></returns>
        public bool ConnectToScanner()
        {
            var rockConfig = RockConfig.Load();

            switch (rockConfig.ScannerInterfaceType)
            {
                case RockConfig.InterfaceType.MICRImageRS232:
                    return HandleMICRImageRS232(rockConfig);
                case RockConfig.InterfaceType.MagTekImageSafe:
                    return HandleMagTekImageSafe(rockConfig);
                default:
                    //Ranger
                    return HandleRanger(rockConfig);
            }

        }

        private bool HandleRanger(RockConfig rockConfig)
        {
            try
            {
                if (this.rangerScanner == null)
                {
                    // no ranger driver
                    return false;
                }
            }
            catch
            {
                return false;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                rangerScanner.StartUp();
            }
            finally
            {
                this.Cursor = null;
            }

            string feederTypeName = rangerScanner.GetTransportInfo("MainHopper", "FeederType");
            if (feederTypeName.Equals("MultipleItems"))
            {
                ScannerFeederType = FeederType.MultipleItems;
            }
            else
            {
                ScannerFeederType = FeederType.SingleItem;
            }

            return true;
        }

        private bool HandleMagTekImageSafe(RockConfig rockConfig)
        {
            this.Cursor = Cursors.Wait;
            UpdateScannerStatusForMagtek(false);

            //Port Open Connects
            var firmware = this.GetImageSafeVersion();
            if (firmware.Length > 0)
            {

                UpdateScannerStatusForMagtek(true);
                ScannerFeederType = FeederType.SingleItem;
                return true;
            }
            else
            {
              
                return false;
            }
        }


        public string GetImageSafeVersion()
        {
           
            if (ImageSafeHelper.OpenDevice())
            {
                //Query Device to See its Working
                //option are : DeviceCapabilities,DeviceStatus,DeviceUsage
                //Results are and XML String
                var results = ImageSafeHelper.QueryDevice("DeviceCapabilities");
                if (results.Length > 0)
                {
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(results);
                    var firmware = document.SelectSingleNode("//Firmware").InnerText;
                    //Successfully Connected
                    this.Cursor = null;
                    UpdateScannerStatusForMagtek(true);
                    return firmware;
                }
            }

            return string.Empty;

        }

        private bool HandleMICRImageRS232(RockConfig rockConfig)
        {
            if (micrImage == null)
            {
                // no MagTek driver
                return false;
            }

            micrImage.CommPort = rockConfig.MICRImageComPort;
            micrImage.PortOpen = false;

            UpdateScannerStatusForMagtek(false);

            object dummy = null;

            // converted from VB6 from MagTek's sample app
            if (!micrImage.PortOpen)
            {
                micrImage.PortOpen = true;
                if (micrImage.DSRHolding)
                {
                    // Sets Switch Settings
                    // If you use the MicrImage1.Save command then these do not need to be sent
                    // every time you open the device
                    micrImage.MicrTimeOut = 1;
                    micrImage.MicrCommand("SWA 00100010", ref dummy);
                    micrImage.MicrCommand("SWB 00100010", ref dummy);
                    micrImage.MicrCommand("SWC 00100000", ref dummy);
                    micrImage.MicrCommand("HW 00111100", ref dummy);
                    micrImage.MicrCommand("SWE 00000010", ref dummy);
                    micrImage.MicrCommand("SWI 00000000", ref dummy);

                    // The OCX will work with any Micr Format.  You just need to know which
                    // format is being used to parse it using the FindElement Method
                    micrImage.FormatChange("6200");
                    micrImage.MicrTimeOut = 5;

                    // get Version to test if we have a good connection to the device
                    string version = "-1";
                    try
                    {
                        this.Cursor = Cursors.Wait;
                        version = micrImage.Version();
                    }
                    finally
                    {
                        this.Cursor = null;
                    }

                    if (!version.Equals("-1"))
                    {
                        UpdateScannerStatusForMagtek(true);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                  
                    return false;
                }
            }

            ScannerFeederType = FeederType.SingleItem;

            return true;
        }

        /// <summary>
        /// Handles the Click event of the btnScan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            var rockConfig = RockConfig.Load();
            ScanningPageUtility.CurrentFinacialTransactions = null;
            ScanningPageUtility.ItemsUploaded = 0;
            if (grdBatchItems.DataContext != null)
            {
                ScanningPageUtility.CurrentFinacialTransactions = (grdBatchItems.DataContext as BindingList<FinancialTransaction>).ToList();
            }
            // make sure we can connect (
            // NOTE: If ranger is powered down after the app starts, it might report that is is connected.  We'll catch that later when they actually start scanning
            if (ConnectToScanner())
            {
                _navigationService.Navigate(this.ScanningPromptPage);
            }
            else
            {
                MessageBox.Show(string.Format("Unable to connect to {0} scanner. Verify that the scanner is turned on and plugged in", rockConfig.ScannerInterfaceType.ConvertToString().SplitCase()));
            }
        }

        /// <summary>
        /// Handles the Click event of the btnOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOptions_Click(object sender, RoutedEventArgs e)
        {
            NavigateToOptionsPage();
        }

        /// <summary>
        /// Navigates to options page.
        /// </summary>
        public void NavigateToOptionsPage()
        {
            var optionsPage = new OptionsPage(this);
            _navigationService.Navigate(optionsPage);
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            ShowBatch(true);
        }

        /// <summary>
        /// Shows the batch edit.
        /// </summary>
        private void ShowBatch(bool showInEditMode)
        {
            grdBatchDetailOuterGrid.Visibility = Visibility.Visible;
            gBatchDetailList.Visibility = Visibility.Visible;
            if (showInEditMode)
            {
                spBatchDetailEdit.Visibility = Visibility.Visible;
                spBatchDetailReadOnly.Visibility = Visibility.Collapsed;
            }
            else
            {
                spBatchDetailEdit.Visibility = Visibility.Collapsed;
                spBatchDetailReadOnly.Visibility = Visibility.Visible;
            }

            grdBatches.IsEnabled = !showInEditMode;
            btnAddBatch.IsEnabled = !showInEditMode;
        }

        /// <summary>
        /// Hides the batch.
        /// </summary>
        private void HideBatch()
        {
            gBatchDetailList.Visibility = Visibility.Hidden;
            grdBatchDetailOuterGrid.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RockConfig rockConfig = RockConfig.Load();
                RockRestClient client = new RockRestClient(rockConfig.RockBaseUrl);
                client.Login(rockConfig.Username, rockConfig.Password);

                FinancialBatch financialBatch = null;
                if (SelectedFinancialBatch == null || SelectedFinancialBatch.Id == 0)
                {
                    financialBatch = new FinancialBatch { Id = 0, Guid = Guid.NewGuid(), Status = BatchStatus.Pending, CreatedByPersonAliasId = LoggedInPerson.PrimaryAliasId };
                }
                else
                {
                    financialBatch = client.GetData<FinancialBatch>(string.Format("api/FinancialBatches/{0}", SelectedFinancialBatch.Id));
                }

                if (!IsValid(rockConfig))
                { return; }


                financialBatch.Name = txtBatchName.Text;
                Campus selectedCampus = cbCampus.SelectedItem as Campus;
                if (selectedCampus.Id > 0)
                {
                    financialBatch.CampusId = selectedCampus.Id;
                }
                else
                {
                    financialBatch.CampusId = null;
                }

                financialBatch.BatchStartDateTime = dpBatchDate.SelectedDate;

                if (!string.IsNullOrWhiteSpace(txtControlAmount.Text))
                {
                    financialBatch.ControlAmount = decimal.Parse(txtControlAmount.Text.Replace("$", string.Empty));
                }
                else
                {
                    financialBatch.ControlAmount = 0.00M;
                }

                if (!string.IsNullOrWhiteSpace(txtControlItemCount.Text))
                {
                    financialBatch.ControlItemCount = int.Parse(txtControlItemCount.Text);
                }
                else
                {
                    financialBatch.ControlItemCount = 0;
                }

                txtNote.Text = txtNote.Text.Trim();
                financialBatch.Note = txtNote.Text;

                if (financialBatch.Id == 0)
                {
                    client.PostData<FinancialBatch>("api/FinancialBatches/", financialBatch);
                }
                else
                {
                    client.PutData<FinancialBatch>("api/FinancialBatches/", financialBatch, financialBatch.Id);
                }

                if (SelectedFinancialBatch == null || SelectedFinancialBatch.Id == 0)
                {
                    // refetch the batch to get the Id if it was just Inserted
                    financialBatch = client.GetDataByGuid<FinancialBatch>("api/FinancialBatches", financialBatch.Guid);

                    SelectedFinancialBatch = financialBatch;
                }

                LoadFinancialBatchesGrid();
                UpdateBatchUI(financialBatch);

                ShowBatch(false);
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        /// <summary>
        /// Apply Validation Rules
        /// Returns true if ... is valid.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValid(RockConfig rockConfig)
        {
            bool result = false;

            //Batch Name Required
            txtBatchName.Text = txtBatchName.Text.Trim();
            if (string.IsNullOrWhiteSpace(txtBatchName.Text))
            {
                txtBatchName.Style = this.FindResource("textboxStyleError") as Style;
                result = false;
            }
            else
            {
                txtBatchName.Style = this.FindResource("textboxStyle") as Style;
                result = true;
            }

            if (string.IsNullOrWhiteSpace(dpBatchDate.Text))
            {
                dpBatchDate.Style = this.FindResource("datePickerErrorStyle") as Style;
                result = false;
            }
            else
            {
                dpBatchDate.Style = this.FindResource("datePickerStyle") as Style;
                result = result != false;
            }
            if (cbCampus.SelectedIndex == 0)
            {
                lblCampus.Foreground = Brushes.Red;
                result = false;
            }
            else
            {
                lblCampus.Foreground = new BrushConverter().ConvertFromString("#FF2B2B2B") as SolidColorBrush;
                result = result != false;
            }

            //Capture Amount Required Validation
            if (rockConfig.CaptureAmountOnScan == true)
            {
                if (rockConfig.RequireControlAmount && (string.IsNullOrWhiteSpace(txtControlAmount.Text) || decimal.Parse(txtControlAmount.Text) < 1))
                {
                    txtControlAmount.Style = this.FindResource("textboxStyleError") as Style;
                    result = false;
                }
                else
                {
                    txtControlAmount.Style = this.FindResource("textboxStyle") as Style;
                    result = result != false;
                }

                if (rockConfig.RequireControlItemCount && (string.IsNullOrWhiteSpace(txtControlItemCount.Text) || int.Parse(txtControlItemCount.Text) < 1))
                {
                    txtControlItemCount.Style = this.FindResource("textboxStyleError") as Style;
                    result = false;
                }
                else
                {
                    txtControlItemCount.Style = this.FindResource("textboxStyle") as Style;
                    result = result != false;
                }
            }
            else
            {
                result = result != false;
            }

            return result;
        }

        /// <summary>
        /// Shows the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private static void ShowException(Exception ex)
        {
            App.LogException(ex);
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDeleteBatch_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this batch and all of its transactions?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                try
                {
                    if (this.SelectedFinancialBatch != null)
                    {
                        RockConfig config = RockConfig.Load();
                        RockRestClient client = new RockRestClient(config.RockBaseUrl);
                        client.Login(config.Username, config.Password);

                        var transactions = grdBatchItems.DataContext as BindingList<FinancialTransaction>;
                        if (transactions != null)
                        {
                            foreach (var transaction in transactions)
                            {
                                client.Delete(string.Format("api/FinancialTransactions/{0}", transaction.Id));
                            }
                        }

                        client.Delete(string.Format("api/FinancialBatches/{0}", this.SelectedFinancialBatch.Id));
                    }
                }
                catch (Exception ex)
                {
                    ShowException(ex);
                }

                LoadFinancialBatchesGrid();

            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            UpdateBatchUI(grdBatches.SelectedValue as FinancialBatch);
            btnAddBatch.IsEnabled = true;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the grdBatches control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void grdBatches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FinancialBatch selectedBatch = grdBatches.SelectedValue as FinancialBatch;
            UpdateBatchUI(selectedBatch);
        }

        /// <summary>
        /// Updates the batch UI.
        /// </summary>
        /// <param name="selectedBatch">The selected batch.</param>
        private void UpdateBatchUI(FinancialBatch selectedBatch)
        {
            this.btnScan.IsEnabled = true;
            if (selectedBatch == null)
            {
                grdBatchItems.DataContext = null;
                DisplayTransactionCount();
                HideBatch();
                return;
            }
            else
            {
                ShowBatch(false);
            }

            RockConfig rockConfig = RockConfig.Load();
            RockRestClient client = new RockRestClient(rockConfig.RockBaseUrl);
            client.Login(rockConfig.Username, rockConfig.Password);
            SelectedFinancialBatch = selectedBatch;
            lblBatchNameReadOnly.Content = selectedBatch.Name;
            lblBatchIdReadOnly.Content = string.Format("Batch Id: {0}", selectedBatch.Id);

            lblBatchCampusReadOnly.Content = selectedBatch.CampusId.HasValue ? client.GetData<Campus>(string.Format("api/Campuses/{0}", selectedBatch.CampusId ?? 0)).Name : string.Empty;
            if (selectedBatch.BatchStartDateTime != null)
            {
                lblBatchDateReadOnly.Content = selectedBatch.BatchStartDateTime.Value.ToString("d");
            }

            var createdByPerson = client.GetData<Person>(string.Format("api/People/GetByPersonAliasId/{0}", selectedBatch.CreatedByPersonAliasId ?? 0));
            if (createdByPerson != null)
            {
                lblBatchCreatedByReadOnly.Content = string.Format("{0} {1}", createdByPerson.NickName, createdByPerson.LastName);
            }
            else
            {
                lblBatchCreatedByReadOnly.Content = string.Empty;
            }

            lblBatchControlAmountReadOnly.Content = selectedBatch.ControlAmount.ToString("F");

            txtBatchName.Text = selectedBatch.Name;
            if (selectedBatch.CampusId.HasValue)
            {
                cbCampus.SelectedValue = selectedBatch.CampusId;
            }
            else
            {
                // pull campus from default
                cbCampus.SelectedValue = rockConfig.DefaultCampusId;
                var selectedCampus = cbCampus.SelectedItem as Campus;

                if (this.SelectedFinancialBatch != null)
                {
                    this.SelectedFinancialBatch.Campus = selectedCampus;
                }

            }

            dpBatchDate.SelectedDate = selectedBatch.BatchStartDateTime;
            lblCreatedBy.Content = lblBatchCreatedByReadOnly.Content as string;
            txtControlAmount.Text = selectedBatch.ControlAmount.ToString("F");
            txtControlItemCount.Text = selectedBatch.ControlItemCount.ToString();
            ScanningPageUtility.ItemsToProcess = selectedBatch.ControlItemCount;


            txtNote.Text = selectedBatch.Note;

            // start a background thread to download transactions since this could take a little while and we want a Wait cursor
            BackgroundWorker bw = new BackgroundWorker();
            Rock.Wpf.WpfHelper.FadeOut(lblCount, 0);
            lblCount.Content = "Loading...";
            Rock.Wpf.WpfHelper.FadeIn(lblCount, 300);
            List<FinancialTransaction> transactions = null;
            grdBatchItems.DataContext = null;
            bw.DoWork += delegate (object s, DoWorkEventArgs ee)
            {
                ee.Result = null;

                transactions = client.GetData<List<FinancialTransaction>>("api/FinancialTransactions/", string.Format("BatchId eq {0} &$expand=TransactionDetails", selectedBatch.Id));
            };
            bw.DoWork += (o, s) =>
            {
                var allAccounts = client.GetData<List<FinancialAccount>>("api/FinancialAccounts");
                ScanningPageUtility.Accounts = allAccounts;
            };


            bw.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs ee)
            {
                this.Cursor = null;
                foreach (var transaction in transactions)
                {
                    if (transaction.FinancialPaymentDetailId.HasValue)
                    {
                        transaction.FinancialPaymentDetail = transaction.FinancialPaymentDetail ?? client.GetData<FinancialPaymentDetail>(string.Format("api/FinancialPaymentDetails/{0}", transaction.FinancialPaymentDetailId ?? 0));
                        if (transaction.FinancialPaymentDetail != null)
                        {
                            transaction.FinancialPaymentDetail.CurrencyTypeValue = this.CurrencyValueList.FirstOrDefault(a => a.Id == transaction.FinancialPaymentDetail.CurrencyTypeValueId);
                        }
                    }
                }

                // sort starting with most recent first
                var bindingList = new BindingList<FinancialTransaction>(transactions.OrderByDescending(a => a.CreatedDateTime).ToList());
                bindingList.RaiseListChangedEvents = true;
                bindingList.ListChanged += bindingList_ListChanged;

                grdBatchItems.DataContext = bindingList;
                ScanningPageUtility.CurrentFinacialTransactions = bindingList.ToList();
                DisplayTransactionCount(rockConfig.CaptureAmountOnScan);

            };
            this.Cursor = Cursors.Wait;
            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Handles the ListChanged event of the bindingList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListChangedEventArgs"/> instance containing the event data.</param>
        protected void bindingList_ListChanged(object sender, ListChangedEventArgs e)
        {
            var transactions = grdBatchItems.DataContext as BindingList<FinancialTransaction>;
            if (transactions != null)
            {
                RockConfig rockConfig = RockConfig.Load();
                var client = new RockRestClient(rockConfig.RockBaseUrl);
                client.Login(rockConfig.Username, rockConfig.Password);

                foreach (var transaction in transactions.Where(a => a.FinancialPaymentDetail == null))
                {
                    if (transaction.FinancialPaymentDetailId.HasValue)
                    {
                        transaction.FinancialPaymentDetail = transaction.FinancialPaymentDetail ?? client.GetData<FinancialPaymentDetail>(string.Format("api/FinancialPaymentDetails/{0}", transaction.FinancialPaymentDetailId ?? 0));
                        if (transaction.FinancialPaymentDetail != null)
                        {
                            transaction.FinancialPaymentDetail.CurrencyTypeValue = this.CurrencyValueList.FirstOrDefault(a => a.Id == transaction.FinancialPaymentDetail.CurrencyTypeValueId);
                        }
                    }
                }
            }

            DisplayTransactionCount();
        }

        /// <summary>
        /// Displays the transaction count.
        /// </summary>
        private void DisplayTransactionCount(bool captureBatchAmount = false)
        {
            var list = grdBatchItems.DataContext as BindingList<FinancialTransaction>;
            if (list != null)
            {
                int listCount = list.Count();
                lblCount.Content = string.Format("{0} item{1}", listCount, listCount != 1 ? "s" : string.Empty);
                if (captureBatchAmount && this.SelectedFinancialBatch.ControlItemCount == listCount)
                {
                    this.btnScan.IsEnabled = false;
                }
            }
            else
            {
                lblCount.Content = "none";
            }

            Rock.Wpf.WpfHelper.FadeIn(lblCount);
        }

        /// <summary>
        /// Handles the Click event of the btnAddBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnAddBatch_Click(object sender, RoutedEventArgs e)
        {
            var financialBatch = new FinancialBatch { Id = 0, BatchStartDateTime = DateTime.Now.Date, CreatedByPersonAliasId = LoggedInPerson.PrimaryAliasId };
            UpdateBatchUI(financialBatch);
            ShowBatch(true);
        }

        /// <summary>
        /// Handles the Click event of the btnRefreshBatchList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnRefreshBatchList_Click(object sender, RoutedEventArgs e)
        {
            LoadFinancialBatchesGrid();
            UpdateBatchUI(this.SelectedFinancialBatch);
        }

        /// <summary>
        /// Handles the RowEdit event of the grdBatchItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        protected void grdBatchItems_RowEdit(object sender, MouseButtonEventArgs e)
        {
            ShowTransactionGridItemDetail();
        }

        /// <summary>
        /// Shows the transaction grid item detail.
        /// </summary>
        private void ShowTransactionGridItemDetail()
        {
            try
            {
                FinancialTransaction financialTransaction = grdBatchItems.SelectedValue as FinancialTransaction;

                if (financialTransaction != null)
                {
                    RockConfig config = RockConfig.Load();
                    RockRestClient client = new RockRestClient(config.RockBaseUrl);
                    client.Login(config.Username, config.Password);
                    financialTransaction.Images = client.GetData<List<FinancialTransactionImage>>("api/FinancialTransactionImages", string.Format("TransactionId eq {0}", financialTransaction.Id));
                    BatchItemDetailPage.batchPage = this;
                    BatchItemDetailPage.FinancialTransaction = financialTransaction;
                    if (this.NavigationService != null)
                    {
                        _navigationService.Navigate(BatchItemDetailPage);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException(ex);
            }
        }

        /// <summary>
        /// Handles the Click event of the TransactionGridItemDetail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TransactionGridItemDetail_Click(object sender, RoutedEventArgs e)
        {
            ShowTransactionGridItemDetail();
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            int transactionId = (int)(sender as Button).CommandParameter;
            if (MessageBox.Show("Are you sure you want to delete this transaction?", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                try
                {
                    FinancialTransaction financialTransaction = grdBatchItems.SelectedValue as FinancialTransaction;

                    if (financialTransaction != null)
                    {
                        RockConfig config = RockConfig.Load();
                        RockRestClient client = new RockRestClient(config.RockBaseUrl);
                        client.Login(config.Username, config.Password);
                        client.Delete(string.Format("api/FinancialTransactions/{0}", transactionId));
                        UpdateBatchUI(this.SelectedFinancialBatch);
                    }
                }
                catch (Exception ex)
                {
                    ShowException(ex);
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnViewTransaction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnViewTransaction_Click(object sender, RoutedEventArgs e)
        {
            ShowTransactionGridItemDetail();
        }

        #endregion

        private void BatchPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.grdBatchItems.Height = this.RenderSize.Height - 385;
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            decimal sum = 0;
            var textblock = sender as TextBlock;
            var context = textblock.DataContext as FinancialTransaction;
            if (context != null)
            {
                var transactionDetails = context.TransactionDetails;
                if (transactionDetails != null)
                {
                    foreach (var transaction in transactionDetails)
                    {
                        sum += transaction.Amount;
                    }
                }

            }

            textblock.Text = sum.ToString("C");
        }

        public void BatchPage_Unloaded()
        {


        }
    }
}
