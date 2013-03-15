using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Rock.Model;

namespace Rock.CheckScannerUtility
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The binary file types
        /// </summary>
        private static List<BinaryFileType> binaryFileTypes = null;

        /// <summary>
        /// Rangers the new state of the scanner_ transport.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RangerScanner_TransportNewState( object sender, AxRANGERLib._DRangerEvents_TransportNewStateEvent e )
        {
            btnScanAction.Visible = true;
            string status = RangerScanner.GetTransportStateString().Replace( "Transport", "" ).SplitCase();

            txtStatus.Text = status;

            switch ( (XportStates)e.currentState )
            {
                case XportStates.TransportReadyToFeed:
                    shapeStatus.BackColor = Color.Lime;
                    btnScanAction.Text = "Start";
                    break;
                case XportStates.TransportShutDown:
                    shapeStatus.BackColor = Color.Red;
                    btnScanAction.Text = "Connect";
                    break;
                case XportStates.TransportFeeding:
                    shapeStatus.BackColor = Color.Blue;
                    btnScanAction.Text = "Stop";
                    break;
                case XportStates.TransportStartingUp:
                    shapeStatus.BackColor = Color.Yellow;
                    btnScanAction.Visible = false;
                    break;
                default:
                    shapeStatus.BackColor = Color.White;
                    btnScanAction.Visible = false;
                    break;
            }
        }

        /// <summary>
        /// Handles the FormClosing event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
        private void MainForm_FormClosing( object sender, FormClosingEventArgs e )
        {
            RangerScanner.ShutDown();
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
            string itemInfo = string.Format( "item_id: {0}, micr: {1}", e.itemId, RangerScanner.GetMicrText( 1 ) );
            lstLog.Items.Add( itemInfo );
        }

        /// <summary>
        /// Handles the Click event of the btnScanAction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnScanAction_Click( object sender, EventArgs e )
        {
            if ( btnScanAction.Text.Equals( "Start" ) )
            {
                const int FeedSourceMainHopper = 0;
                const int FeedContinuously = 0;
                RangerScanner.StartFeeding( FeedSourceMainHopper, FeedContinuously );
            }
            else if ( btnScanAction.Text.Equals( "Stop" ) )
            {
                RangerScanner.StopFeeding();
            }
            else if ( btnScanAction.Text.Equals( "Connect" ) )
            {
                RangerScanner.StartUp();
            }
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

            Bitmap bitImageFront = GetCheckImage( Sides.TransportFront );
            pbxFront.Image = bitImageFront;
            string frontFilePath = Path.Combine( fileDirectory, fileName + "_front.jpg" );
            File.Delete( frontFilePath );
            bitImageFront.Save( frontFilePath, System.Drawing.Imaging.ImageFormat.Jpeg );

            Bitmap bitImageBack = GetCheckImage( Sides.TransportRear );
            pbxBack.Image = bitImageBack;
            string backFilePath = Path.Combine( fileDirectory, fileName + "_back.jpg" );
            File.Delete( backFilePath );
            bitImageBack.Save( backFilePath, System.Drawing.Imaging.ImageFormat.Jpeg );
        }

        /// <summary>
        /// Gets the scanner output directory.
        /// </summary>
        /// <returns></returns>
        private static string GetScannerOutputDirectory()
        {
            string fileDirectory = Path.Combine( new FileInfo( Application.ExecutablePath ).DirectoryName, "ScannerOutput" );
            return fileDirectory;
        }

        /// <summary>
        /// Gets the check image.
        /// </summary>
        /// <param name="side">The side.</param>
        /// <returns></returns>
        private Bitmap GetCheckImage( Sides side )
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

            //Create an image stream and a bitmap object to hold the image 
            System.IO.MemoryStream streamBitmap = new MemoryStream( imageBytes );
            Bitmap bitImageFront = new Bitmap( Image.FromStream( streamBitmap ) );
            return bitImageFront;
        }

        /// <summary>
        /// Rangers the scanner_ transport feeding stopped.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RangerScanner_TransportFeedingStopped( object sender, AxRANGERLib._DRangerEvents_TransportFeedingStoppedEvent e )
        {
            lstLog.Items.Add( ( (FeedingStoppedReasons)e.reason ).ToString() );
        }

        /// <summary>
        /// Handles the Tick event of the timer1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void timer1_Tick( object sender, EventArgs e )
        {
            // startup the RangerScanner in a timer so that form display gets a chance to draw
            timer1.Enabled = false;
            RangerScanner.SetGenericOption( "Ranger GUI", "DisplaySplashOncePerDay", "true" );
            RangerScanner.StartUp();
            lstLog.Items.Add( "ImagesAvailableDuringOutputEvent: " + RangerScanner.GetTransportInfo( "General", "ImagesAvailableDuringOutputEvent" ) );
            lstLog.Items.Add( "ImagesAvailableDuringPocketEvent: " + RangerScanner.GetTransportInfo( "General", "ImagesAvailableDuringPocketEvent" ) );
            cboImageOption.SelectedItem = "Grayscale";
        }

        /// <summary>
        /// Handles the 1 event of the MainForm_Load control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void MainForm_Load( object sender, EventArgs e )
        {
            cboImageOption.Items.Clear();
            cboImageOption.Items.Add( "Bitonal" );
            cboImageOption.Items.Add( "Grayscale" );
            cboImageOption.Items.Add( "Color" );

            timer1.Enabled = true;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cboImageOption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void cboImageOption_SelectedIndexChanged( object sender, EventArgs e )
        {
            // restart to get Options to load
            RangerScanner.ShutDown();
            RangerScanner.StartUp();
        }

        /// <summary>
        /// Handles the Click event of the btnUpload control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnUpload_Click( object sender, EventArgs e )
        {
            getBinaryFileTypes( txtRockURL.Text ).ContinueWith( a =>
            {
                binaryFileTypes = a.Result;
                UploadScannedChecks( txtRockURL.Text, ShowProgress );
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
            if ( this.lstLog.InvokeRequired )
            {
                ProgressUpdate d = new ProgressUpdate( ShowProgress );
                this.Invoke( d, new object[] { current, max, name } );
            }
            else
            {
                lstLog.Items.Add( string.Format( "{0} {1} {2}", current, max, name ) );
            }
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
    }
}
