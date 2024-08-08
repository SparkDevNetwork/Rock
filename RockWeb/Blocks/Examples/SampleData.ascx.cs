// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;

using Microsoft.AspNet.SignalR;

using Rock;
using Rock.Attribute;
using Rock.Logging;
using Rock.Model;
using Rock.Utility;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// Block that can load sample data into your Rock database.
    /// Dev note: You can set the XML Document URL setting to your local
    /// file when you're testing new data.  Something like C:\Misc\Rock\Documentation\sampledata.xml
    /// </summary>
    [DisplayName( "Rock Solid Church Sample Data" )]
    [Category( "Examples" )]
    [Description( "Loads the Rock Solid Church sample data into your Rock system." )]

    [TextField( "XML Document URL",
        Description = @"The URL for the input sample data XML document. You can also use a local Windows file path (e.g. C:\Rock\Documentation\sampledata_1_14_0.xml) if you want to test locally with your own fake data.  The file format is loosely defined on the <a target='blank' rel='noopener noreferrer' href='https://github.com/SparkDevNetwork/Rock/wiki/z.-Rock-Solid-Demo-Church-Specification-(sample-data)'>Rock Solid Demo Church Specification</a> wiki.",
        Key = AttributeKey.XMLDocumentURL,
        IsRequired = false,
        DefaultValue = "http://storage.rockrms.com/sampledata/sampledata_1_14_1.xml",
        Order = 0 )]

    [BooleanField( "Fabricate Attendance",
        Description = "If true, then fake attendance data will be fabricated (if the right parameters are in the XML)",
        Key = AttributeKey.FabricateAttendance,
        DefaultBooleanValue = true,
        Order = 1 )]

    [BooleanField( "Enable Stopwatch",
        Description = "If true, a stopwatch will be used to time each of the major operations.",
        Key = AttributeKey.EnableStopwatch,
        DefaultBooleanValue = false,
        Order = 2 )]

    [BooleanField( "Enable Giving",
        Description = "If true, the giving data will be loaded otherwise it will be skipped.",
        Key = AttributeKey.EnableGiving,
        DefaultBooleanValue = true,
        Order = 3 )]

    [BooleanField( "Process Only Giving Data",
        Description = "If true, the only giving data will be loaded.",
        Key = AttributeKey.ProcessOnlyGivingData,
        DefaultBooleanValue = false,
        Order = 4 )]

    [BooleanField( "Delete Data First",
        Description = "If true, data will be deleted first.",
        Key = AttributeKey.DeleteDataFirst,
        DefaultBooleanValue = true,
        Order = 5 )]

    [IntegerField( "Random Number Seed",
        Description = "If given, the randomizer used during the creation of attendance and financial transactions will be predictable. Use 0 to use a random seed.",
        Key = AttributeKey.RandomNumberSeed,
        IsRequired = false,
        DefaultIntegerValue = 1,
        Order = 6 )]

    [Rock.SystemGuid.BlockTypeGuid( "A42E0031-B2B9-403A-845B-9C968D7716A6" )]
    public partial class SampleData : Rock.Web.UI.RockBlock
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string XMLDocumentURL = "XMLDocumentURL";
            public const string FabricateAttendance = "FabricateAttendance";
            public const string EnableStopwatch = "EnableStopwatch";
            public const string EnableGiving = "EnableGiving";
            public const string ProcessOnlyGivingData = "ProcessOnlyGivingData";
            public const string DeleteDataFirst = "DeleteDataFirst";
            public const string RandomNumberSeed = "RandomNumberSeed";
        }

        #endregion Attribute Keys

        #region Fields

        private IHubContext _hubContext = null;

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext GetHubContext()
        {
            if ( _hubContext == null )
            {
                _hubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();
            }

            return _hubContext;
        }

        /// <summary>
        /// Stopwatch used to measure time during certain operations.
        /// </summary>
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// StringBuilder is used to build the stopwatch trace for certain operations.
        /// </summary>
        private readonly StringBuilder _sb = new StringBuilder();

        private RockLoggerMemoryBuffer _logger;

        #endregion Fields

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );

            // from https://stackoverflow.com/a/30976223/1755417
            tbPassword.Attributes["autocomplete"] = "new-password";
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            // Set timeout for up to 30 minutes (just like installer)
            Server.ScriptTimeout = 1800;
            ScriptManager.GetCurrent( Page ).AsyncPostBackTimeout = 1800;

            if ( !IsPostBack )
            {
                tbPassword.Focus();
                VerifyXMLDocumentExists();
            }
            else
            {
                if ( GetAttributeValue( AttributeKey.EnableStopwatch ).AsBoolean() )
                {
                    messageContainer.Attributes["style"] = "visibility: visible";
                }
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Events

        private void EventLoggedHandler( object sender, RockLoggerMemoryBuffer.EventLoggedArgs e )
        {
            var message = e.Event.Message;
            if ( GetAttributeValue( AttributeKey.EnableStopwatch ).AsBoolean() )
            {
                _sb.Append( message );
                GetHubContext().Clients.All.receiveNotification( "sampleDataImport", message );
            }
        }

        /// <summary>
        /// This is the entry point for when the user clicks the "load data" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void bbtnLoadData_Click( object sender, EventArgs e )
        {
            string saveFile = Path.Combine( MapPath( "~" ), "sampledata1.xml" );

            try
            {
                string xmlFileUrl = GetAttributeValue( AttributeKey.XMLDocumentURL );
                if ( DownloadFile( xmlFileUrl, saveFile ) )
                {
                    if ( GetAttributeValue( AttributeKey.EnableStopwatch ).AsBoolean() )
                    {
                        GetHubContext().Clients.All.showLog();
                    }

                    var args = GetSampleDataImportArgs();
                    var manager = GetConfiguredSampleDataManager();
                    manager.CreateFromXmlDocumentFile( saveFile, args );

                    nbMessage.Visible = true;
                    nbMessage.Title = "Success";
                    nbMessage.NotificationBoxType = NotificationBoxType.Success;
                    nbMessage.Text = string.Format(
@"<p>Happy tire-kicking! The data is in your database. Hint: try <a href='{0}'>searching for the Decker family</a>.</p>
<p>Here are some of the things you'll find in the sample data:</p>{1}",
                        ResolveRockUrl( "~/Person/Search/name/?SearchTerm=Decker" ),
                        GetStories( saveFile ) );
                    pnlInputForm.Visible = false;

                    RecordSuccess();
                }
            }
            catch ( Exception ex )
            {
                if ( GetAttributeValue( AttributeKey.EnableStopwatch ).AsBoolean() )
                {
                    GetHubContext().Clients.All.showLog();
                }

                nbMessage.Visible = true;
                nbMessage.Title = "Oops!";
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Text = string.Format(
                    "That wasn't supposed to happen. The error was:<br/>{0}<br/>{1}<br/>{2}",
                    ex.Message.ConvertCrLfToHtmlBr(),
                    FlattenInnerExceptions( ex.InnerException ),
                    HttpUtility.HtmlEncode( ex.StackTrace ).ConvertCrLfToHtmlBr() );
            }

            if ( File.Exists( saveFile ) )
            {
                File.Delete( saveFile );
            }
        }

        /// <summary>
        /// Records the current date into the SampleData system setting
        /// so that other blocks (such as the RockUpdate) can know that
        /// sample data has been loaded.
        /// </summary>
        private void RecordSuccess()
        {
            string xmlFileUrl = GetAttributeValue( AttributeKey.XMLDocumentURL );
            if ( xmlFileUrl.StartsWith( "http://storage.rockrms.com/sampledata/" ) )
            {
                Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.SAMPLEDATA_DATE, RockDateTime.Now.ToString() );
            }
        }

        /// <summary>
        /// Extract the stories out of the XML comments and put them on the results page.
        /// </summary>
        /// <param name="saveFile"></param>
        /// <returns></returns>
        protected string GetStories( string saveFile )
        {
            var xdoc = XDocument.Load( saveFile );
            StringBuilder sb = new StringBuilder();
            sb.Append( "<ul>" );
            foreach ( var comment in xdoc.Element( "data" ).DescendantNodes().OfType<XComment>() )
            {
                sb.AppendFormat( "<li>{0}</li>", comment.ToString().Replace( "<!--", string.Empty ).Replace( "-->", string.Empty ) );
            }

            sb.Append( "</ul>" );
            return sb.ToString();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the SampleData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            VerifyXMLDocumentExists();
            nbMessage.Text = string.Empty;
            nbMessage.Visible = false;
            pnlInputForm.Visible = true;
        }

        #endregion Events

        private SampleDataManager GetConfiguredSampleDataManager()
        {
            // Configure a log device for the SampleDataManager.
            var configuration = new RockLoggerInMemoryConfiguration();

            if ( GetAttributeValue( AttributeKey.EnableStopwatch ).AsBoolean() )
            {
                configuration.LogLevel = RockLogLevel.All;
            }
            else
            {
                configuration.LogLevel = RockLogLevel.Info;
            }

            _logger = new RockLoggerMemoryBuffer( configuration );
            _logger.EventLogged += EventLoggedHandler;

            var manager = new SampleDataManager( _logger );
            return manager;
        }

        /// <summary>
        /// Get the SampleDataManager import action arguments from the current block settings.
        /// </summary>
        /// <returns></returns>
        private SampleDataManager.SampleDataImportActionArgs GetSampleDataImportArgs()
        {
            var args = new SampleDataManager.SampleDataImportActionArgs()
            {
                Password = tbPassword.Text,
                RandomizerSeed = GetAttributeValue( AttributeKey.RandomNumberSeed ).AsIntegerOrNull(),
                EnableStopwatch = GetAttributeValue( AttributeKey.EnableStopwatch ).AsBoolean(),
                CreatorPersonAliasId = CurrentPerson.PrimaryAliasId.GetValueOrDefault( 0 ),
                DeleteExistingData = GetAttributeValue( AttributeKey.DeleteDataFirst ).AsBoolean(),
                EnableGiving = GetAttributeValue( AttributeKey.EnableGiving ).AsBoolean(),
                FabricateAttendance = GetAttributeValue( AttributeKey.FabricateAttendance ).AsBoolean(),
                ProcessOnlyGivingData = GetAttributeValue( AttributeKey.ProcessOnlyGivingData ).AsBoolean(),
            };

            // Get the message templates from the current RegistrationTemplateDetail block settings.
            var blockAttributes = System.Attribute.GetCustomAttributes( typeof( RockWeb.Blocks.Event.RegistrationTemplateDetail ), typeof( CodeEditorFieldAttribute ) );
            foreach ( CodeEditorFieldAttribute blockAttribute in blockAttributes )
            {
                switch ( blockAttribute.Name )
                {
                    case "Default Confirmation Email":
                        args.RegistrationConfirmationEmailTemplate = blockAttribute.DefaultValue;
                        break;

                    case "Default Reminder Email":
                        args.RegistrationReminderEmailTemplate = blockAttribute.DefaultValue;
                        break;

                    case "Default Success Text":
                        args.RegistrationSuccessText = blockAttribute.DefaultValue;
                        break;

                    case "Default Payment Reminder Email":
                        args.RegistrationPaymentReminderTemplate = blockAttribute.DefaultValue;
                        break;

                    default:
                        break;
                }
            }

            return args;
        }

        /// <summary>
        /// Verify that the configured XML document exists.
        /// </summary>
        private void VerifyXMLDocumentExists()
        {
            bool fileExists = false;

            try
            {
                Uri fileUri = new Uri( GetAttributeValue( AttributeKey.XMLDocumentURL ) );
                if ( fileUri.IsFile )
                {
                    fileExists = File.Exists( fileUri.LocalPath );
                }
                else
                {
                    var request = ( HttpWebRequest ) WebRequest.Create( GetAttributeValue( AttributeKey.XMLDocumentURL ) );
                    request.Method = "HEAD";
                    var response = ( HttpWebResponse ) request.GetResponse();
                    fileExists = response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch ( Exception ex )
            {
                nbError.Text += string.Format( "<br/>{0} Error trying to check the sample data file. {1}", RockDateTime.Now.ToShortTimeString(), ex.Message );
            }

            if ( !fileExists )
            {
                nbError.Visible = true;
                bbtnLoadData.Enabled = false;
            }
            else
            {
                nbError.Visible = false;
                bbtnLoadData.Enabled = true;
            }
        }

        /// <summary>
        /// Download the given fileUrl and store it at the fileOutput.
        /// </summary>
        /// <param name="fileUrl">The file URL to fetch.</param>
        /// <param name="fileOutput">The full path location to store the file.</param>
        /// <returns></returns>
        private bool DownloadFile( string fileUrl, string fileOutput )
        {
            bool isSuccess = false;
            try
            {
                Uri fileUri = new Uri( fileUrl );
                if ( fileUri.IsFile )
                {
                    File.Copy( fileUrl, fileOutput );
                }
                else
                {
                    using ( WebClient client = new WebClient() )
                    {
                        client.DownloadFile( fileUri, fileOutput );
                    }
                }

                isSuccess = true;
            }
            catch ( Exception ex )
            {
                nbMessage.Text = string.Format( "While trying to fetch {0}, {1} ", fileUrl, ex.Message );
                nbMessage.Visible = true;
            }

            return isSuccess;
        }

        /// <summary>
        /// Flattens exception's inner exceptions and returns an HTML formatted string
        /// useful for debugging.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private string FlattenInnerExceptions( Exception ex )
        {
            StringBuilder sb = new StringBuilder();
            while ( ex != null && ex.InnerException != null )
            {
                sb.AppendLine( ex.InnerException.Message.ConvertCrLfToHtmlBr() );
                ex = ex.InnerException;
            }

            return sb.ToString();
        }
    }
}