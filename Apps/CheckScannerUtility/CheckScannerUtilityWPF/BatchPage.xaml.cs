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
using Rock.CheckScannerUtility;
using Rock;
using Rock.Model;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Net.Http;

namespace CheckScannerUtilityWPF
{
    /// <summary>
    /// Interaction logic for BatchPage.xaml
    /// </summary>
    public partial class BatchPage : System.Windows.Controls.Page
    {
        public BatchPage()
        {
            InitializeComponent();
        }

        public string RockUrl { get; set; }
        private static List<BinaryFileType> binaryFileTypes = null;

        #region Scanner Events

        /// <summary>
        /// Rangers the new state of the scanner_ transport.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RangerScanner_TransportNewState( object sender, AxRANGERLib._DRangerEvents_TransportNewStateEvent e )
        {
            btnConnect.Visibility = Visibility.Visible;
            //string status = RangerScanner.GetTransportStateString().Replace( "Transport", "" ).SplitCase();

            switch ( (XportStates)e.currentState )
            {
                case XportStates.TransportReadyToFeed:
                    shapeStatus.Fill = new SolidColorBrush( Colors.LimeGreen );
                    btnConnect.Content = "Start";
                    break;
                case XportStates.TransportShutDown:
                    shapeStatus.Fill = new SolidColorBrush( Colors.Red );
                    btnConnect.Content = "Connect";
                    break;
                case XportStates.TransportFeeding:
                    shapeStatus.Fill = new SolidColorBrush( Colors.Blue );
                    btnConnect.Content = "Stop";
                    break;
                case XportStates.TransportStartingUp:
                    shapeStatus.Fill = new SolidColorBrush( Colors.Yellow );
                    btnConnect.Visibility = Visibility.Hidden;
                    break;
                default:
                    shapeStatus.Fill = new SolidColorBrush( Colors.White );
                    btnConnect.Visibility = Visibility.Hidden;
                    break;
            }
        }

        /// <summary>
        /// Rangers the state of the scanner_ transport change options.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RangerScanner_TransportChangeOptionsState( object sender, AxRANGERLib._DRangerEvents_TransportChangeOptionsStateEvent e )
        {

            if ( e.previousState == (int)XportStates.TransportStartingUp )
            {
                //enable imaging
                RangerScanner.SetGenericOption( "OptionalDevices", "NeedImaging", "True" );

                RangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage1", "False" );
                RangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage1", "False" );
                RangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage2", "False" );
                RangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage2", "False" );
                RangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage3", "False" );
                RangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage3", "False" );
                RangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage4", "False" );
                RangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage4", "False" );

                switch ( cboImageOption.SelectedItem.ToString() )
                {
                    case "Color":
                        RangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage3", "True" );
                        RangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage3", "True" );
                        break;
                    case "Grayscale":
                        RangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage2", "True" );
                        RangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage2", "True" );
                        break;
                    default:
                        RangerScanner.SetGenericOption( "OptionalDevices", "NeedFrontImage1", "True" );
                        RangerScanner.SetGenericOption( "OptionalDevices", "NeedRearImage1", "True" );
                        break;
                }

                RangerScanner.EnableOptions();
            }
        }

        /// <summary>
        /// Rangers the scanner_ transport set item output.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RangerScanner_TransportSetItemOutput( object sender, AxRANGERLib._DRangerEvents_TransportSetItemOutputEvent e )
        {
            //
        }

        /// <summary>
        /// Handles the TransportNewItem event of the RangerScanner control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void RangerScanner_TransportNewItem( object sender, EventArgs e )
        {
            //
        }

        /// <summary>
        /// Rangers the scanner_ transport item in pocket.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RangerScanner_TransportItemInPocket( object sender, AxRANGERLib._DRangerEvents_TransportItemInPocketEvent e )
        {
            string checkMicr = RangerScanner.GetMicrText( 1 ).Trim();
            string fileName = checkMicr.Replace( " ", "_" );
            string fileDirectory = GetScannerOutputDirectory();
            if ( !Directory.Exists( fileDirectory ) )
            {
                Directory.CreateDirectory( fileDirectory );
            }

            BitmapImage bitImageFront = GetCheckImage( Sides.TransportFront );
            BitmapImage bitImageBack = GetCheckImage( Sides.TransportRear );

            imgFront.Source = bitImageFront;
            imgBack.Source = bitImageBack;

            string[] micrParts = checkMicr.Split( new char[] { ' ' } );
            lblAccountNumber.Content = string.Format( "Account Number: {0}", checkMicr );

            //todo
            //frmScanChecks.ShowCheckAccountMicr( checkMicr );
            //frmScanChecks.ShowCheckImages( bitImageFront, bitImageBack );

            /*
            string frontFilePath = System.IO.Path.Combine( fileDirectory, fileName + "_front.jpg" );
            File.Delete( frontFilePath );
            bitImageFront. Save( frontFilePath, System.Drawing.Imaging.ImageFormat.Jpeg );

            string backFilePath = System.IO.Path.Combine( fileDirectory, fileName + "_back.jpg" );
            File.Delete( backFilePath );
            bitImageBack.Save( backFilePath, System.Drawing.Imaging.ImageFormat.Jpeg );
             */ 
        }

        /// <summary>
        /// Rangers the scanner_ transport feeding stopped.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RangerScanner_TransportFeedingStopped( object sender, AxRANGERLib._DRangerEvents_TransportFeedingStoppedEvent e )
        {
            //
        }

        #endregion

        /// <summary>
        /// Gets the scanner output directory.
        /// </summary>
        /// <returns></returns>
        private static string GetScannerOutputDirectory()
        {
            string fileDirectory = System.IO.Path.Combine( new FileInfo( System.Reflection.Assembly.GetExecutingAssembly().Location ).DirectoryName, "ScannerOutput" );
            return fileDirectory;
        }

        /// <summary>
        /// Gets the check image.
        /// </summary>
        /// <param name="side">The side.</param>
        /// <returns></returns>
        private BitmapImage GetCheckImage( Sides side )
        {
            ImageColorTypes colorType = ImageColorTypes.ImageColorTypeColor;
            switch ( cboImageOption.SelectedItem.ToString() )
            {
                case "Color":
                    colorType = ImageColorTypes.ImageColorTypeColor;
                    break;
                case "Grayscale":
                    colorType = ImageColorTypes.ImageColorTypeGrayscale;
                    break;
                default:
                    colorType = ImageColorTypes.ImageColorTypeBitonal;
                    break;
            }

            int imageByteCount;
            imageByteCount = RangerScanner.GetImageByteCount( (int)side, (int)colorType );
            byte[] imageBytes = new byte[imageByteCount];

            //create the pointer and assign the Ranger image address to it
            IntPtr imgAddress = new IntPtr( RangerScanner.GetImageAddress( (int)side, (int)colorType ) );

            //Copy the bytes from unmanaged memory to managed memory
            Marshal.Copy( imgAddress, imageBytes, 0, imageByteCount );

            BitmapImage bitImageFront = new BitmapImage();

            bitImageFront.BeginInit();
            //bitImageFront.CacheOption = BitmapCacheOption.OnLoad;
            bitImageFront.StreamSource = new MemoryStream( imageBytes );
            bitImageFront.EndInit();

            return bitImageFront;
        }

        /// <summary>
        /// Handles the Click event of the btnUpload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnUpload_Click( object sender, EventArgs e )
        {
            getBinaryFileTypes( RockUrl ).ContinueWith( a =>
            {
                binaryFileTypes = a.Result;
                UploadScannedChecks( RockUrl, ShowProgress );
            } );
        }

        /// <summary>
        /// Shows the progress.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="max">The max.</param>
        /// <param name="name">The name.</param>
        private void ShowProgress( int current, int max, string name )
        {
            progressBar.Maximum = max;
            progressBar.Value = current;
        }

        /// <summary>
        /// Gets the binary file types.
        /// </summary>
        /// <param name="rockBaseUrl">The rock base URL.</param>
        private static async System.Threading.Tasks.Task<List<BinaryFileType>> getBinaryFileTypes( string rockBaseUrl )
        {
            HttpClient client = new HttpClient();
            HttpContent resultContent;
            string restURL = rockBaseUrl.TrimEnd( new char[] { '/' } ) + "/api/BinaryFileTypes/";
            //bool gotResponse = false;
            await client.GetAsync( restURL ).ContinueWith(
                ( postTask ) =>
                {
                    resultContent = postTask.Result.Content;
                    resultContent.ReadAsAsync<List<BinaryFileType>>().ContinueWith(
                        ( readResult ) =>
                        {
                            binaryFileTypes = readResult.Result;
                        } ).Wait();
                }
                );

            return binaryFileTypes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="max">The max.</param>
        /// <param name="name">The name.</param>
        private delegate void ProgressUpdate( int position, int max, string name );

        /// <summary>
        /// Uploads the scanned checks.
        /// </summary>
        /// <param name="rockBaseUrl">The rock base URL.</param>
        private static async void UploadScannedChecks( string rockBaseUrl, ProgressUpdate progressFeedback )
        {
            string restURL = rockBaseUrl.TrimEnd( new char[] { '/' } ) + "/api/BinaryFiles/";
            var qryParams = new System.Collections.Generic.Dictionary<string, string>();
            restURL += "0?apikey=CcvRockApiKey";
            Guid fileTypeCheckFront = new Guid( "EF9B78C1-57A0-4D18-8275-51EECE0C8A6D" );
            Guid fileTypeCheckBack = new Guid( "DAC10DF2-D57F-45F6-94AD-8E27E3BC4682" );

            DirectoryInfo scannerOutputDirectory = new DirectoryInfo( GetScannerOutputDirectory() );
            var scannedFiles = scannerOutputDirectory.GetFiles( "*.jpg" ).ToList();

            int totalCount = scannedFiles.Count();
            int position = 1;

            foreach ( FileInfo scannedFile in scannedFiles )
            {
                var binaryFile = new BinaryFile();
                binaryFile.Id = 0;
                binaryFile.FileName = scannedFile.Name;
                binaryFile.Data = File.ReadAllBytes( scannedFile.FullName );
                if ( scannedFile.Name.EndsWith( "_front.jpg" ) )
                {
                    binaryFile.BinaryFileTypeId = binaryFileTypes.First( a => a.Guid.Equals( fileTypeCheckFront ) ).Id;

                }
                else if ( scannedFile.Name.EndsWith( "_back.jpg" ) )
                {
                    binaryFile.BinaryFileTypeId = binaryFileTypes.First( a => a.Guid.Equals( fileTypeCheckBack ) ).Id;
                }
                else
                {
                    continue;
                }

                binaryFile.IsSystem = false;
                binaryFile.MimeType = "image/jpeg";

                HttpClient client = new HttpClient();
                try
                {
                    await client.PostAsJsonAsync<BinaryFile>( restURL, binaryFile ).ContinueWith(
                        ( postTask ) =>
                        {
                            progressFeedback( position++, totalCount, scannedFile.Name );
                            postTask.Result.EnsureSuccessStatusCode();
                            scannedFile.Delete();
                        } );
                }
                catch ( Exception ex )
                {
                    MessageBox.Show( ex.Message );
                    break;
                }
            }

            progressFeedback( 0, 0, "Done" );
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded( object sender, RoutedEventArgs e )
        {
            RangerScanner.StartUp();

            cboImageOption.Items.Clear();
            cboImageOption.Items.Add( "Bitonal" );
            cboImageOption.Items.Add( "Grayscale" );
            cboImageOption.Items.Add( "Color" );
            cboImageOption.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles the 1 event of the Page_Unloaded control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Unloaded( object sender, RoutedEventArgs e )
        {
            RangerScanner.ShutDown();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the 1 event of the btnConnect_Click control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnConnect_Click( object sender, RoutedEventArgs e )
        {
            if ( btnConnect.Content.Equals( "Start" ) )
            {
                const int FeedSourceMainHopper = 0;
                const int FeedContinuously = 0;
                //frmScanChecks.Visible = true;
                RangerScanner.StartFeeding( FeedSourceMainHopper, FeedContinuously );
            }
            else if ( btnConnect.Content.Equals( "Stop" ) )
            {
                RangerScanner.StopFeeding();
            }
            else if ( btnConnect.Content.Equals( "Connect" ) )
            {
                RangerScanner.StartUp();
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the cboImageOption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void cboImageOption_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            // restart to get Options to load
            RangerScanner.ShutDown();
            RangerScanner.StartUp();
        }
    }
}
