//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Rock.Model;
using Rock.Net;

namespace Rock.Apps.CheckScannerUtility
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
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnLogin_Click( object sender, RoutedEventArgs e )
        {
            BatchPage batchPage = new BatchPage();
            try
            {
                RockRestClient rockRestClient = new RockRestClient( txtRockUrl.Text );
                rockRestClient.Login( txtUsername.Text, txtPassword.Password );
                Person person = rockRestClient.GetData<Person>( string.Format( "api/People/GetByUserName/{0}", txtUsername.Text ) );
                batchPage.LoggedInPerson = person;
            }
            catch ( Exception ex )
            {
                if ( ex is WebException )
                {
                    WebException wex = ex as WebException;
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
                }

                lblRockUrl.Visibility = Visibility.Visible;
                txtRockUrl.Visibility = Visibility.Visible;
                lblLoginWarning.Content = ex.Message;
                lblLoginWarning.Visibility = Visibility.Visible;
                return;
            }

            RockConfig rockConfig = RockConfig.Load();
            rockConfig.RockBaseUrl = txtRockUrl.Text;
            rockConfig.Username = txtUsername.Text;
            rockConfig.Password = txtPassword.Password;
            rockConfig.Save();
            
            this.NavigationService.Navigate( batchPage);
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

            bool promptForUrl = ( string.IsNullOrWhiteSpace( rockConfig.RockBaseUrl ) );

            lblRockUrl.Visibility = promptForUrl ? Visibility.Visible : Visibility.Collapsed;
            txtRockUrl.Visibility = promptForUrl ? Visibility.Visible : Visibility.Collapsed;
            
            txtRockUrl.Text = rockConfig.RockBaseUrl;
            txtUsername.Text = rockConfig.Username;
            txtPassword.Password = rockConfig.Password;
        }

        /// <summary>
        /// Hides the login warning.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void HideLoginWarning( object sender, System.Windows.Input.KeyEventArgs e )
        {
            lblLoginWarning.Visibility = Visibility.Hidden;
        }
    }
}
