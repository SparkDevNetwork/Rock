//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Net;
using System.Windows;
using System.Windows.Controls;
using Rock.Net;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
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
            try
            {
                RockRestClient rockRestClient = new RockRestClient(txtRockUrl.Text);
                rockRestClient.Login( txtUsername.Text, txtPassword.Password);
            }
            catch ( WebException wex )
            {
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

                lblLoginWarning.Content = wex.Message;
                lblLoginWarning.Visibility = Visibility.Visible;
                return;
            }

            RockConfig rockConfig = RockConfig.Load();
            rockConfig.RockBaseUrl = txtRockUrl.Text;
            rockConfig.Username = txtUsername.Text;
            rockConfig.Password = txtPassword.Password;
            rockConfig.Save();
            
            BatchPage batchPage = new BatchPage();
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
