using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Configuration;

namespace CheckinClient
{
    /// <summary>
    /// Interaction logic for StartupPage.xaml
    /// </summary>
    public partial class StartupPage : Page
    {
        
        public StartupPage()
        {
            InitializeComponent();
        }

        private void btnCancel_Click( object sender, RoutedEventArgs e )
        {
            Application.Current.Shutdown();
        }

        private void btnStart_Click( object sender, RoutedEventArgs e )
        {
            // save settings
            Configuration configuration = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );
            configuration.AppSettings.Settings["CheckinAddress"].Value = txtCheckinAddress.Text;
            
            configuration.AppSettings.Settings["CacheLabelDuration"].Value = txtCacheLabelDuration.Text;

            if ( txtPrinterOverrideIp.Text != string.Empty )
            {
                configuration.AppSettings.Settings["PrinterOverrideIp"].Value = txtPrinterOverrideIp.Text;
                configuration.AppSettings.Settings["PrinterOverrideLocal"].Value = "";
            }
            else
            {
                foreach ( Control control in spPrinterOverride.Children )
                {
                    if ( control is ToggleButton )
                    {
                        ToggleButton tbControl = control as ToggleButton;
                        if ( tbControl.IsChecked != null && tbControl.IsChecked.Value == true )
                        {
                            configuration.AppSettings.Settings["PrinterOverrideLocal"].Value = tbControl.Content.ToString();
                        }
                    }
                }
            }

            configuration.Save();

            ConfigurationManager.RefreshSection( "appSettings" );
            Uri uriTest;
            if ( !Uri.TryCreate(txtCheckinAddress.Text, UriKind.Absolute, out uriTest ))
            {
                MessageBox.Show( "You must enter a valid URL for the Check-in Address Field.", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation );
            }
            else
            {
                BrowserPage browserPage = new BrowserPage();
                this.NavigationService.Navigate( browserPage );
            }
            
        }

        private void pStartupPage_Loaded( object sender, RoutedEventArgs e )
        {
            int printerCounter = 0;

            // set save configuration settings
            txtCheckinAddress.Text = ConfigurationManager.AppSettings["CheckinAddress"];
            txtPrinterOverrideIp.Text = ConfigurationManager.AppSettings["PrinterOverrideIp"];
            txtCacheLabelDuration.Text = ConfigurationManager.AppSettings["CacheLabelDuration"];
            string localPrinterName = ConfigurationManager.AppSettings["PrinterOverrideLocal"];
            
            foreach ( string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters )
            {
                //MessageBox.Show( printer );
                ToggleButton btnToggle = new ToggleButton();
                btnToggle.Content = printer;
                btnToggle.Name = "btnPrinter" + printerCounter;
                btnToggle.Margin = new Thickness( 0, 12, 0, 0 );
                btnToggle.Padding = new Thickness( 0, 12, 0, 8 );
                btnToggle.FontSize = 18;

                if ( printer == localPrinterName )
                {
                    btnToggle.IsChecked = true;
                }
                
                spPrinterOverride.Children.Add( btnToggle );
                btnToggle.Click += new RoutedEventHandler( btnToggle_Click );
                printerCounter++;
            }
        }

        // ensure only one toggle button is selected at a time
        private void btnToggle_Click( object sender, RoutedEventArgs e )
        {
            ToggleButton btnClicked = sender as ToggleButton;

            foreach ( Control control in spPrinterOverride.Children )
            {
                if ( control is ToggleButton && control.Name != btnClicked.Name )
                {
                    ToggleButton tbControl = control as ToggleButton;
                    tbControl.IsChecked = false;
                }
            }
        }
    }
}
