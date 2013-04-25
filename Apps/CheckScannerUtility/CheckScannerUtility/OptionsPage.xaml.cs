//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
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

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// Interaction logic for OptionsPage.xaml
    /// </summary>
    public partial class OptionsPage : Page
    {
        public OptionsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cboImageOption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void cboImageOption_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            RockConfig config = RockConfig.Load();
            string imageOption = cboImageOption.SelectedValue as string;

            switch ( imageOption )
            {
                case "Grayscale":
                    config.ImageColorType = ImageColorType.ImageColorTypeGrayscale;
                    break;
                case "Color":
                    config.ImageColorType = ImageColorType.ImageColorTypeColor;
                    break;
                default:
                    config.ImageColorType = ImageColorType.ImageColorTypeBitonal;
                    break;
            }

            config.Save();

            // restart to get Options to load
            //RangerScanner.ShutDown();
            //RangerScanner.StartUp();
        }

        /// <summary>
        /// Loads the image options.
        /// </summary>
        private void LoadImageOptions()
        {
            RockConfig config = RockConfig.Load();
            ImageColorType colorType = (ImageColorType)config.ImageColorType;

            cboImageOption.Items.Clear();
            cboImageOption.Items.Add( "Bitonal" );
            cboImageOption.Items.Add( "Grayscale" );
            cboImageOption.Items.Add( "Color" );
            cboImageOption.SelectedIndex = 0;

            switch ( colorType )
            {
                case ImageColorType.ImageColorTypeGrayscale:
                    cboImageOption.SelectedValue = "Grayscale";
                    break;
                case ImageColorType.ImageColorTypeColor:
                    cboImageOption.SelectedValue = "Color";
                    break;
                default:
                    cboImageOption.SelectedIndex = 0;
                    break;
            }
        }

        private void ShowDetail()
        {
            lblMakeModel.Content = "MagTek";
            //lblInterfaceVersion.Content = micrImage.Version();

            string feederFriendlyNameType = "Single";
            lblFeederType.Content = string.Format( "Feeder Type: {0}", feederFriendlyNameType );


            //lblMakeModel.Content = string.Format( "Scanner Type: {0} {1}", RangerScanner.GetTransportInfo( "General", "Make" ), RangerScanner.GetTransportInfo( "General", "Model" ) );
            //lblInterfaceVersion.Content = string.Format( "Interface Version: {0}", RangerScanner.GetVersion() );

            lblFeederType.Content = string.Format( "Feeder Type: {0}", feederFriendlyNameType );

            lblImageOption.Visibility = Visibility.Visible;
            cboImageOption.Visibility = Visibility.Visible;
            LoadImageOptions();
        }

        private void Something()
        {
            
        }

        private void btnBack_Click_1( object sender, RoutedEventArgs e )
        {
            this.NavigationService.GoBack();
        }
    }
}
