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
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;

using RestSharp;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for OptionsPage.xaml
    /// </summary>
    public partial class OptionsPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPage"/> class.
        /// </summary>
        public OptionsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        private void ShowDetail()
        {
            lblAlert.Visibility = Visibility.Collapsed;

            var rockConfig = RockConfig.Load();

            txtRockUrl.Text = rockConfig.RockBaseUrl;
            txtTemporaryDirectory.Text = rockConfig.TemporaryDirectory;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSave_Click( object sender, RoutedEventArgs e )
        {
            RockConfig rockConfig = RockConfig.Load();

            txtRockUrl.Text = txtRockUrl.Text.Trim();
            Uri rockUrl = new Uri( txtRockUrl.Text );
            var validSchemes = new string[] { Uri.UriSchemeHttp, Uri.UriSchemeHttps };
            if ( !validSchemes.Contains( rockUrl.Scheme ) )
            {
                txtRockUrl.Text = "https://" + rockUrl.AbsoluteUri;
            }

            try
            {
                RestClient restClient = new RestClient( txtRockUrl.Text );
                restClient.LoginToRock( rockConfig.Username, rockConfig.Password );
            }
            catch (Exception ex)
            {
                lblAlert.Content = ex.Message;
                lblAlert.Visibility = Visibility.Visible;
                return;
            }

            rockConfig.RockBaseUrl = txtRockUrl.Text;

            if ( txtTemporaryDirectory.Text.IsNotNullOrWhiteSpace() )
            {
                try
                {
                    Directory.CreateDirectory( txtTemporaryDirectory.Text );
                }
                catch ( Exception ex )
                {
                    lblAlert.Content = $"Error creating temporary directory: { ex.Message}";
                    lblAlert.Visibility = Visibility.Visible;
                    return;
                }
            }

            rockConfig.TemporaryDirectory = txtTemporaryDirectory.Text;

            rockConfig.Save();

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
    }
}
