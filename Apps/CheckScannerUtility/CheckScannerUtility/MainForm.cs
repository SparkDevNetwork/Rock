//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
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
            frmScanChecks = new ScanChecksForm();
        }

        /// <summary>
        /// The binary file types
        /// </summary>
        private static List<BinaryFileType> binaryFileTypes = null;

        /// <summary>
        /// Gets or sets the rock URL.
        /// </summary>
        /// <value>
        /// The rock URL.
        /// </value>
        public string RockUrl { get; set; }

        /// <summary>
        /// The scan checks form
        /// </summary>
        public ScanChecksForm frmScanChecks;

        /// <summary>
        /// Rangers the new state of the scanner_ transport.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RangerScanner_TransportNewState( object sender, AxRANGERLib._DRangerEvents_TransportNewStateEvent e )
        {
            btnConnect.Visible = true;
            string status = RangerScanner.GetTransportStateString().Replace( "Transport", "" ).SplitCase();

            switch ( (XportStates)e.currentState )
            {
                case XportStates.TransportReadyToFeed:
                    shapeStatus.BackColor = Color.Lime;
                    btnConnect.Text = "Start";
                    break;
                case XportStates.TransportShutDown:
                    shapeStatus.BackColor = Color.Red;
                    btnConnect.Text = "Connect";
                    break;
                case XportStates.TransportFeeding:
                    shapeStatus.BackColor = Color.Blue;
                    btnConnect.Text = "Stop";
                    break;
                case XportStates.TransportStartingUp:
                    shapeStatus.BackColor = Color.Yellow;
                    btnConnect.Visible = false;
                    break;
                default:
                    shapeStatus.BackColor = Color.White;
                    btnConnect.Visible = false;
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
            //
        }

        /// <summary>
        /// Handles the Click event of the btnScanAction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnScanAction_Click( object sender, EventArgs e )
        {
            if ( btnConnect.Text.Equals( "Start" ) )
            {
                const int FeedSourceMainHopper = 0;
                const int FeedContinuously = 0;
                frmScanChecks.Visible = true;
                RangerScanner.StartFeeding( FeedSourceMainHopper, FeedContinuously );
            }
            else if ( btnConnect.Text.Equals( "Stop" ) )
            {
                RangerScanner.StopFeeding();
            }
            else if ( btnConnect.Text.Equals( "Connect" ) )
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
            Bitmap bitImageBack = GetCheckImage( Sides.TransportRear );
            
            frmScanChecks.ShowCheckAccountMicr( checkMicr );
            frmScanChecks.ShowCheckImages( bitImageFront, bitImageBack );
            
            string frontFilePath = Path.Combine( fileDirectory, fileName + "_front.jpg" );
            File.Delete( frontFilePath );
            bitImageFront.Save( frontFilePath, System.Drawing.Imaging.ImageFormat.Jpeg );
            
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
            //
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
            RangerScanner.EnableOptions();
            RangerScanner.StartUp();
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
            if ( this.progressBar1.InvokeRequired )
            {
                ProgressUpdate d = new ProgressUpdate( ShowProgress );
                this.Invoke( d, new object[] { current, max, name } );
            }
            else
            {
                progressBar1.Maximum = max;
                progressBar1.Value = current;
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

        /// <summary>
        /// Handles the DrawItem event of the dataRepeater1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.VisualBasic.PowerPacks.DataRepeaterItemEventArgs"/> instance containing the event data.</param>
        private void dataRepeater1_DrawItem( object sender, Microsoft.VisualBasic.PowerPacks.DataRepeaterItemEventArgs e )
        {
            var a = e;
            //
            //dataRepeater1.DataBindings[
            //e.DataRepeaterItem
        }

        public delegate void AssignDataSource();

        private void button1_Click( object sender, EventArgs e )
        {
            if ( binaryFileTypes == null )
            {

                getBinaryFileTypes( RockUrl ).ContinueWith( a =>
                {
                    binaryFileTypes = a.Result;
                } );

                dataRepeater1.DataSource = binaryFileTypes;
            }
            else
            {
                dataRepeater1.DataSource = binaryFileTypes;
            }
        }
    }
}
