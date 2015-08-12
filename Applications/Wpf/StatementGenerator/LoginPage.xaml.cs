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
using System.ComponentModel;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Rock.Net;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : System.Windows.Controls.Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPage"/> class.
        /// </summary>
        public LoginPage()
            : this( false )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPage"/> class.
        /// </summary>
        public LoginPage( bool forceRockURLVisible )
        {
            InitializeComponent();
            ForceRockURLVisible = forceRockURLVisible;
        }

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnLogin_Click( object sender, RoutedEventArgs e )
        {
            lblLoginWarning.Visibility = Visibility.Hidden;
            txtUsername.Text = txtUsername.Text.Trim();
            txtRockUrl.Text = txtRockUrl.Text.Trim();
            Uri rockUrl = new Uri( txtRockUrl.Text );
            var validSchemes = new string[] { Uri.UriSchemeHttp, Uri.UriSchemeHttps };
            if ( !validSchemes.Contains(rockUrl.Scheme) )
            {
                txtRockUrl.Text = "http://" + rockUrl.AbsoluteUri;
            }

            RockRestClient rockRestClient = new RockRestClient( txtRockUrl.Text );

            string userName = txtUsername.Text;
            string password = txtPassword.Password;

            if ( string.IsNullOrWhiteSpace( userName ) )
            {
                lblLoginWarning.Content = "Username cannot be blank";
                lblLoginWarning.Visibility = Visibility.Visible;
                return;
            }

            // start a background thread to Login since this could take a little while and we want a Wait cursor
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += delegate( object s, DoWorkEventArgs ee )
            {
                ee.Result = null;
                rockRestClient.Login( userName, password );
            };

            // when the Background Worker is done with the Login, run this
            bw.RunWorkerCompleted += delegate( object s, RunWorkerCompletedEventArgs ee )
            {
                this.Cursor = null;
                btnLogin.IsEnabled = true;
                try
                {
                    if ( ee.Error != null )
                    {
                        throw ee.Error;
                    }

                    Rock.Client.Person person = rockRestClient.GetData<Rock.Client.Person>( string.Format( "api/People/GetByUserName/{0}", userName ) );
                    RockConfig rockConfig = RockConfig.Load();
                    rockConfig.RockBaseUrl = txtRockUrl.Text;
                    rockConfig.Username = txtUsername.Text;
                    rockConfig.Password = txtPassword.Password;
                    rockConfig.Save();

                    if ( this.NavigationService.CanGoBack )
                    {
                        // if we got here from some other Page, go back
                        this.NavigationService.GoBack();
                    }
                    else
                    {
                        StartPage startPage = new StartPage();
                        this.NavigationService.Navigate( startPage );
                    }
                }
                catch ( WebException wex )
                {
                    // show WebException on the form, but any others should end up in the ExceptionDialog
                    HttpWebResponse response = wex.Response as HttpWebResponse;
                    if ( response != null )
                    {
                        if ( response.StatusCode.Equals( HttpStatusCode.Unauthorized ) )
                        {
                            lblLoginWarning.Content = "Invalid Login";
                            lblLoginWarning.Visibility = Visibility.Visible;
                            return;
                        }
                    }

                    string message = wex.Message;
                    if ( wex.InnerException != null )
                    {
                        message += "\n" + wex.InnerException.Message;
                    }

                    lblRockUrl.Visibility = Visibility.Visible;
                    txtRockUrl.Visibility = Visibility.Visible;
                    lblLoginWarning.Content = message;
                    lblLoginWarning.Visibility = Visibility.Visible;
                    return;
                }
            };

            // set the cursor to Wait, disable the login button, and start the login background process
            this.Cursor = Cursors.Wait;
            btnLogin.IsEnabled = false;
            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            HideLoginWarning( null, null );
            RockConfig rockConfig = RockConfig.Load();

            bool promptForUrl = string.IsNullOrWhiteSpace( rockConfig.RockBaseUrl ) || ForceRockURLVisible;

            lblRockUrl.Visibility = promptForUrl ? Visibility.Visible : Visibility.Collapsed;
            txtRockUrl.Visibility = promptForUrl ? Visibility.Visible : Visibility.Collapsed;

            txtRockUrl.Text = rockConfig.RockBaseUrl;
            txtUsername.Text = rockConfig.Username;
            txtPassword.Password = rockConfig.Password;

            // set keyboard focus to the first input that needs a value
            if ( string.IsNullOrEmpty( txtRockUrl.Text ) )
            {
                Keyboard.Focus( txtRockUrl );
            }
            else if ( string.IsNullOrEmpty( txtUsername.Text ) )
            {
                Keyboard.Focus( txtUsername );
            }
            else
            {
                Keyboard.Focus( txtPassword );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [force rock URL visible].
        /// </summary>
        /// <value>
        /// <c>true</c> if [force rock URL visible]; otherwise, <c>false</c>.
        /// </value>
        private bool ForceRockURLVisible { get; set; }

        /// <summary>
        /// Hides the login warning.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void HideLoginWarning( object sender, System.Windows.Input.KeyEventArgs e )
        {
            lblLoginWarning.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Handles the Click event of the btnRunReport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnRunReport_Click( object sender, RoutedEventArgs e )
        {
            ProgressPage progressPage = new ProgressPage();
            this.NavigationService.Navigate( progressPage );
        }

        /// <summary>
        /// Handles the KeyDown event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void Page_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Key == Key.Enter )
            {
                btnLogin_Click( null, null );
            }
        }

        /// <summary>
        /// (EasterEgg) Handles the MouseDoubleClick event of the LoginLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void LoginLabel_MouseDoubleClick( object sender, MouseButtonEventArgs e )
        {
            lblRockUrl.Visibility = Visibility.Visible;
            txtRockUrl.Visibility = Visibility.Visible;
        }
    }
}
