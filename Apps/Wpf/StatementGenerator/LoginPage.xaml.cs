//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ComponentModel;
using System.Net;
using System.Windows;
using System.Windows.Input;
using Rock.Model;
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
            txtUsername.Text = txtUsername.Text.Trim();
            txtRockUrl.Text = txtRockUrl.Text.Trim();
            RockRestClient rockRestClient = new RockRestClient( txtRockUrl.Text );

            string userName = txtUsername.Text;
            string password = txtPassword.Password;

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

                    Person person = rockRestClient.GetData<Person>( string.Format( "api/People/GetByUserName/{0}", userName ) );
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
    }
}
