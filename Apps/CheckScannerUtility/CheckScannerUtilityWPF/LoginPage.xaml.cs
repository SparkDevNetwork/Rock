//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Windows;
using System.Windows.Controls;

namespace CheckScannerUtilityWPF
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
            // todo: auth 
            RockConfig rockConfig = RockConfig.Load();
            rockConfig.RockURL = txtRockUrl.Text;
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
            RockConfig rockConfig = RockConfig.Load();
            txtRockUrl.Text = rockConfig.RockURL;
            txtUsername.Text = rockConfig.Username;
            txtPassword.Password = rockConfig.Password;
        }
    }
}
