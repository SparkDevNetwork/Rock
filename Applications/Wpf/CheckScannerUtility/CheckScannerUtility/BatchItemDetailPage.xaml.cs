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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Rock.Apps.CheckScannerUtility.Models;
using Rock.Client;
using Rock.Net;
using Rock;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for BatchItemDetailPage.xaml
    /// </summary>
    public partial class BatchItemDetailPage : System.Windows.Controls.Page
    {
        private bool _disablePredictableIds;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchItemDetailPage"/> class.
        /// </summary>
        public BatchItemDetailPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the financial transaction.
        /// </summary>
        /// <value>
        /// The financial transaction.
        /// </value>
        public FinancialTransaction FinancialTransaction { get; set; }

        /// <summary>
        /// Gets or sets the batch page.
        /// </summary>
        /// <value>
        /// The batch page.
        /// </value>
        public BatchPage batchPage { get; set; }



        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            var financialTransaction = this.FinancialTransaction;
            LoadFinancialTransactionDetails( financialTransaction );

            var images = financialTransaction.Images.OrderBy( a => a.Order ).ToList();

            RockConfig config = RockConfig.Load();
            RockRestClient client = new RockRestClient( config.RockBaseUrl );
            client.Login( config.Username, config.Password );

            _disablePredictableIds = GetDisablePredictableIdsSetting( client );

            if ( config.CaptureAmountOnScan )
            {
                spAccounts.Visibility = Visibility.Visible;
                Grid.SetColumn( spCheckImage, 1 );
                Grid.SetColumnSpan( spCheckImage, 1 );
            }
            else
            {
                spAccounts.Visibility = Visibility.Collapsed;
                Grid.SetColumn( spCheckImage, 0 );
                Grid.SetColumnSpan( spCheckImage, 2 );
            }

            spActionsSaveCancel.Visibility = config.CaptureAmountOnScan == true ? Visibility.Visible : Visibility.Collapsed;
            spActionsReadonly.Visibility = config.CaptureAmountOnScan == false ? Visibility.Visible : Visibility.Collapsed;

            if ( images.Count > 0 )
            {
                imgScannedItemNone.Visibility = Visibility.Collapsed;
                var idOrGuid = GetIdOrGuid( client, images[0].BinaryFileId );
                var imageUrl = $"{config.RockBaseUrl.EnsureTrailingForwardslash()}GetImage.ashx?{( _disablePredictableIds ? "guid" : "id" )}={idOrGuid}";

                var imageBytes = client.DownloadData( imageUrl );

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream( imageBytes );
                bitmapImage.EndInit();
                imgFront.Source = bitmapImage;
                imgFront.Visibility = Visibility.Visible;
            }
            else
            {
                imgFront.Visibility = Visibility.Collapsed;
                imgScannedItemNone.Visibility = Visibility.Visible;
                imgFront.Source = null;
            }

            if ( images.Count > 1 )
            {
                var idOrGuid = GetIdOrGuid( client, images[1].BinaryFileId );
                var imageUrl = $"{config.RockBaseUrl.EnsureTrailingForwardslash()}GetImage.ashx?{( _disablePredictableIds ? "guid" : "id" )}={idOrGuid}";

                var imageBytes = client.DownloadData( imageUrl );

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream( imageBytes );
                bitmapImage.EndInit();
                imgBack.Source = bitmapImage;
            }
            else
            {
                imgBack.Source = null;
            }

            lblFront.Visibility = imgFront.Source != null ? Visibility.Visible : Visibility.Collapsed;
            lblBack.Visibility = imgBack.Source != null ? Visibility.Visible : Visibility.Collapsed;

            lblScannedDateTime.Content = financialTransaction.CreatedDateTime.HasValue ? financialTransaction.CreatedDateTime.Value.ToString( "g" ) : null;
            lblTransactionDateTime.Content = financialTransaction.TransactionDateTime.HasValue ? financialTransaction.TransactionDateTime.Value.ToString( "g" ) : null;
            lblBatch.Content = batchPage.SelectedFinancialBatch.Name;

            financialTransaction.SourceTypeValue = financialTransaction.SourceTypeValue ?? batchPage.SourceTypeValueList.FirstOrDefault( a => a.Id == financialTransaction.SourceTypeValueId );
            lblSource.Content = financialTransaction.SourceTypeValue != null ? financialTransaction.SourceTypeValue.Value : null;

            // only show Transaction Code if it has one
            lblTransactionCodeValue.Content = financialTransaction.TransactionCode;
            bool hasTransactionCode = !string.IsNullOrWhiteSpace( financialTransaction.TransactionCode );
            lblTransactionCodeLabel.Visibility = hasTransactionCode ? Visibility.Visible : Visibility.Collapsed;
            lblTransactionCodeValue.Visibility = hasTransactionCode ? Visibility.Visible : Visibility.Collapsed;

            if ( financialTransaction.FinancialPaymentDetailId.HasValue )
            {
                financialTransaction.FinancialPaymentDetail = financialTransaction.FinancialPaymentDetail ?? client.GetData<FinancialPaymentDetail>( string.Format( "api/FinancialPaymentDetails/{0}", financialTransaction.FinancialPaymentDetailId ?? 0 ) );
            }

            if ( financialTransaction.FinancialPaymentDetail != null )
            {
                financialTransaction.FinancialPaymentDetail.CurrencyTypeValue = financialTransaction.FinancialPaymentDetail.CurrencyTypeValue ?? batchPage.CurrencyValueList.FirstOrDefault( a => a.Id == financialTransaction.FinancialPaymentDetail.CurrencyTypeValueId );
                lblCurrencyType.Content = financialTransaction.FinancialPaymentDetail.CurrencyTypeValue != null ? financialTransaction.FinancialPaymentDetail.CurrencyTypeValue.Value : null;
            }
            else
            {
                lblCurrencyType.Content = string.Empty;
            }
        }

        private void LoadFinancialTransactionDetails( FinancialTransaction financialTransaction )
        {
            decimal sum = 0;
            List<DisplayFinancialTransactionDetailModel> displayFinancialTransactionDetailList = new List<DisplayFinancialTransactionDetailModel>();

            // first, make sure all the accounts that are part of the existing transaction are included, even if they aren't included in the configured selected accounts
            if ( financialTransaction.TransactionDetails != null )
            {
                foreach ( var detail in financialTransaction.TransactionDetails )
                {
                    sum += detail.Amount;
                    detail.Account = ScanningPageUtility.Accounts.FirstOrDefault( a => a.Id == detail.AccountId );
                    displayFinancialTransactionDetailList.Add( new DisplayFinancialTransactionDetailModel( detail ) );
                }
            }

            RockConfig rockConfig = RockConfig.Load();

            List<DisplayAccountValueModel> sortedDisplayedAccountList = ScanningPageUtility.GetVisibleAccountsSortedAndFlattened();

            // now, add accounts that aren't part of the Transaction in case they want to split to different accounts
            foreach ( var displayAccount in sortedDisplayedAccountList )
            {
                if ( !displayFinancialTransactionDetailList.Any( a => a.AccountId == displayAccount.AccountId ) )
                {
                    FinancialTransactionDetail financialTransactionDetail = new FinancialTransactionDetail();
                    financialTransactionDetail.Guid = Guid.NewGuid();
                    financialTransactionDetail.AccountId = displayAccount.AccountId;
                    financialTransactionDetail.Account = displayAccount.Account;
                    displayFinancialTransactionDetailList.Add( new DisplayFinancialTransactionDetailModel( financialTransactionDetail ) );
                }
            }

            // show displayed accounts sorted by its position in sortedDisplayedAccountList
            displayFinancialTransactionDetailList = displayFinancialTransactionDetailList.OrderBy( a => sortedDisplayedAccountList.FirstOrDefault( s => s.AccountId == a.AccountId )?.DisplayIndex ).ToList();

            this.lvAccountDetails.ItemsSource = displayFinancialTransactionDetailList;
            this.lblTotals.Content = sum.ToString( "C" );
        }

        private bool GetDisablePredictableIdsSetting( RockRestClient client )
        {
            try
            {
                var filters = new List<string>
                {
                    "EntityTypeId eq null",
                    "EntityTypeQualifierColumn eq 'SystemSetting'",
                    "EntityTypeQualifierValue eq ''",
                    "Key eq 'core_RockSecuritySettings'"
                };

                var filterString = string.Join( " and ", filters );

                var securitySettings = client.GetData<List<Rock.Client.Attribute>>( $"api/Attributes?$filter={filterString}" );
                var setting = securitySettings.FirstOrDefault();
                if ( setting != null )
                {
                    string defaultValue = setting?.DefaultValue;
                    if ( !string.IsNullOrEmpty( defaultValue ) )
                    {
                        var regex = new System.Text.RegularExpressions.Regex( "\"DisablePredictableIds\"\\s*:\\s*true", System.Text.RegularExpressions.RegexOptions.IgnoreCase );

                        _disablePredictableIds = regex.IsMatch( defaultValue );
                    }
                }
            }
            catch ( Exception ex )
            {
                // Log the exception
                App.LogException( ex );
            }
            return _disablePredictableIds;
        }

        private string GetIdOrGuid( RockRestClient client, int binaryFileId )
        {
            if ( _disablePredictableIds )
            {
                var binaryFile = client.GetData<BinaryFile>( $"api/BinaryFiles/{binaryFileId}" );
                return binaryFile?.Guid.ToString();
            }
            return binaryFileId.ToString();
        }

        /// <summary>
        /// Handles the LostKeyboardFocus event of the TbAccountDetailAmount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyboardFocusChangedEventArgs"/> instance containing the event data.</param>
        private void TbAccountDetailAmount_LostKeyboardFocus( object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e )
        {
            HandleDetailAmountChange( sender );
        }

        /// <summary>
        /// Handles the KeyUp event of the TbAccountDetailAmount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void TbAccountDetailAmount_KeyUp( object sender, System.Windows.Input.KeyEventArgs e )
        {
            if ( e.Key == System.Windows.Input.Key.Decimal )
            {
                return;
            }

            HandleDetailAmountChange( sender );
        }

        /// <summary>
        /// Handles the detail amount change.
        /// </summary>
        /// <param name="sender">The sender.</param>
        private void HandleDetailAmountChange( object sender )
        {
            var tbAccountDetailAmount = sender as TextBox;
            List<DisplayFinancialTransactionDetailModel> displayFinancialTransactionDetails = lvAccountDetails.ItemsSource as List<DisplayFinancialTransactionDetailModel>;
            DisplayFinancialTransactionDetailModel editingDisplayFinancialTransaction = tbAccountDetailAmount.DataContext as DisplayFinancialTransactionDetailModel;
            var displayFinancialTransactionDetail = displayFinancialTransactionDetails.FirstOrDefault( a => a.Guid == editingDisplayFinancialTransaction.Guid );
            var otherDetailTotalAmounts = displayFinancialTransactionDetails.Where( a => a.Guid != editingDisplayFinancialTransaction.Guid && a.Amount.HasValue ).Sum( a => a.Amount.Value );
            var editingAmount = tbAccountDetailAmount.Text.AsDecimalOrNull();
            var totalDetailAmounts = otherDetailTotalAmounts + ( editingAmount ?? 0.00M );

            lblTotals.Content = totalDetailAmounts.ToString( "C" );
        }

        /// <summary>
        /// Handles the Click event of the BtnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnSave_Click( object sender, RoutedEventArgs e )
        {
            RockConfig config = RockConfig.Load();
            RockRestClient client = new RockRestClient( config.RockBaseUrl );
            client.Login( config.Username, config.Password );
            List<FinancialTransactionDetail> databaseFinancialTransactionDetails = client.GetData<List<FinancialTransactionDetail>>( "api/FinancialTransactionDetails", string.Format( "TransactionId eq {0}", this.FinancialTransaction.Id ) );

            bool amountsUpdated = false;

            List<DisplayFinancialTransactionDetailModel> displayFinancialTransactionDetails = lvAccountDetails.ItemsSource as List<DisplayFinancialTransactionDetailModel>;
            foreach ( var displayFinancialTransactionDetail in displayFinancialTransactionDetails )
            {
                var databaseFinancialTransactionDetail = databaseFinancialTransactionDetails.FirstOrDefault( a => a.Guid == displayFinancialTransactionDetail.Guid );
                if ( databaseFinancialTransactionDetail == null )
                {
                    // transaction doesn't have a detail record for this account yet, so add it
                    databaseFinancialTransactionDetail = new FinancialTransactionDetail()
                    {
                        Id = 0,
                        TransactionId = this.FinancialTransaction.Id,
                        AccountId = displayFinancialTransactionDetail.AccountId,
                        Guid = displayFinancialTransactionDetail.Guid,
                        Amount = 0.00M
                    };
                }

                if ( databaseFinancialTransactionDetail?.Amount != displayFinancialTransactionDetail.Amount )
                {

                    databaseFinancialTransactionDetail.Amount = displayFinancialTransactionDetail.Amount ?? 0.00M;

                    if ( databaseFinancialTransactionDetail.Id == 0 )
                    {
                        if ( databaseFinancialTransactionDetail.Amount != 0.00M )
                        {
                            // new detail record, so add it (unless the amount is 0.00)
                            var databaseDetailId = client.PostData<FinancialTransactionDetail>( "api/FinancialTransactionDetails/", databaseFinancialTransactionDetail as FinancialTransactionDetail ).AsIntegerOrNull();
                            if ( databaseDetailId.HasValue )
                            {
                                databaseFinancialTransactionDetail.Id = databaseDetailId.Value;
                            }
                            amountsUpdated = true;
                        }
                    }
                    else
                    {
                        // changed amount on an existing record, so just edit it
                        amountsUpdated = true;
                        client.PutData<FinancialTransactionDetail>( "api/FinancialTransactionDetails/", databaseFinancialTransactionDetail as FinancialTransactionDetail, databaseFinancialTransactionDetail.Id );
                    }
                }
            }

            if ( amountsUpdated )
            {
                // reload the batches since some amounts where changed
                batchPage.LoadFinancialBatchesGrid();
                batchPage.UpdateBatchUI( batchPage.SelectedFinancialBatch );
            }

            this.NavigationService.GoBack();
        }

        /// <summary>
        /// Handles the Click event of the BtnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnCancel_Click( object sender, RoutedEventArgs e )
        {
            this.NavigationService.GoBack();
        }

        /// <summary>
        /// Handles the Click event of the btnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnClose_Click( object sender, RoutedEventArgs e )
        {
            this.NavigationService.GoBack();
        }
    }
}
