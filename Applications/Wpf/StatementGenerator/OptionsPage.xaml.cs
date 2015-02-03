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
using System.IO;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Rock.Net;

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
            PopulateImageFile( rockConfig.LogoFile );
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
                lblAlert.Content = ex.Message;
                lblAlert.Visibility = Visibility.Visible;
                return;
            }

            rockConfig.RockBaseUrl = txtRockUrl.Text;
            rockConfig.LogoFile = txtLogoFile.Text;
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

        /// <summary>
        /// Handles the Click event of the btnSelectLogoFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectLogoFile_Click( object sender, RoutedEventArgs e )
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Image Files |*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.tif;*.tiff;*.emf;*.wmf";
            openFileDialog.Multiselect = false;
            if ( openFileDialog.ShowDialog() == true )
            {
                PopulateImageFile( openFileDialog.FileName );
            }
        }

        /// <summary>
        /// Populates the image file.
        /// </summary>
        /// <param name="imageFileName">Name of the image file.</param>
        private void PopulateImageFile( string imageFileName )
        {
            string appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string appDirectory = Path.GetDirectoryName( appPath );

            txtLogoFile.Text = imageFileName;
            txtLogoFile.ToolTip = imageFileName;

            Uri imageFileUri;
            if ( string.IsNullOrWhiteSpace( Path.GetDirectoryName( imageFileName ) ) )
            {
                // no directory specified. It's in the App Directory
                imageFileUri = new Uri( Path.Combine( appDirectory, imageFileName ) );
            }
            else if ( Path.GetDirectoryName( imageFileName ).Equals( appDirectory, StringComparison.OrdinalIgnoreCase ) )
            {
                // directory is App Directory, just put the filename without the directory
                imageFileUri = new Uri( Path.GetFileName( imageFileName ) );
            }
            else
            {
                // directory is something besides the App Directory.  Fully qualify it
                imageFileUri = new Uri( imageFileName );
            }

            try
            {
                imgLogo.Source = new BitmapImage( imageFileUri );
                imgLogo.Visibility = Visibility.Visible;
                lblImageError.Visibility = Visibility.Collapsed;
            }
            catch ( Exception ex )
            {
                imgLogo.Visibility = Visibility.Collapsed;
                lblImageError.Visibility = Visibility.Visible;
                lblImageError.ToolTip = ex.Message;
                MessageBox.Show( "Unable to load image: \n\n" + ex.Message, "Image Error", MessageBoxButton.OK, MessageBoxImage.Error );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelectDefaultLogo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectDefaultLogo_Click( object sender, RoutedEventArgs e )
        {
            PopulateImageFile( RockConfig.DefaultLogoFile );
        }
    }
}
