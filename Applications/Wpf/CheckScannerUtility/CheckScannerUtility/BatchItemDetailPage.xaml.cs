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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Rock.Model;
using Rock.Net;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for BatchItemDetailPage.xaml
    /// </summary>
    public partial class BatchItemDetailPage : System.Windows.Controls.Page
    {
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
        /// Handles the Click event of the btnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnClose_Click( object sender, RoutedEventArgs e )
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
            var financialTransaction = this.FinancialTransaction;
            var images = financialTransaction.Images.OrderBy( a => a.Order ).ToList();

            RockConfig config = RockConfig.Load();
            RockRestClient client = new RockRestClient( config.RockBaseUrl );
            client.Login( config.Username, config.Password );

            if ( images.Count > 0 )
            {
                var imageUrl = string.Format( "{0}GetImage.ashx?Id={1}", config.RockBaseUrl.EnsureTrailingForwardslash(), images[0].BinaryFileId );
                var imageBytes = client.DownloadData( imageUrl );

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream( imageBytes );
                bitmapImage.EndInit();
                imgFront.Source = bitmapImage;
            }
            else
            {
                imgFront.Source = null;
            }

            if ( images.Count > 1 )
            {
                var imageUrl = string.Format( "{0}GetImage.ashx?Id={1}", config.RockBaseUrl.EnsureTrailingForwardslash(), images[1].BinaryFileId );
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

            lblScannedDateTime.Content = financialTransaction.CreatedDateTime.HasValue ? financialTransaction.CreatedDateTime.Value.ToString( "g" ) : null;
            lblTransactionDateTime.Content = financialTransaction.TransactionDateTime.HasValue ? financialTransaction.TransactionDateTime.Value.ToString( "g" ) : null;
            lblBatch.Content = batchPage.SelectedFinancialBatch.Name;

            financialTransaction.SourceTypeValue = financialTransaction.SourceTypeValue ?? batchPage.SourceTypeValueList.FirstOrDefault( a => a.Id == financialTransaction.SourceTypeValueId );
            lblSource.Content = financialTransaction.SourceTypeValue != null ? financialTransaction.SourceTypeValue.Value : null;

            lblTransactionCode.Content = financialTransaction.TransactionCode;

            financialTransaction.CurrencyTypeValue = financialTransaction.CurrencyTypeValue ?? batchPage.CurrencyValueList.FirstOrDefault( a => a.Id == financialTransaction.CurrencyTypeValueId );
            lblCurrencyType.Content = financialTransaction.CurrencyTypeValue != null ? financialTransaction.CurrencyTypeValue.Value : null;
        }
    }
}
