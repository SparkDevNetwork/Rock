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
//

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Configuration;
using Rock.Logging;
using Rock.Model;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Example.SampleData;

namespace Rock.Blocks.Example
{
    /// <summary>
    /// Block that can load sample data into your Rock database.
    /// Dev note: You can set the XML Document URL setting to your local
    /// file when you're testing new data. Something like C:\Misc\Rock\Documentation\sampledata.xml
    /// </summary>
    [DisplayName( "Rock Solid Church Sample Data" )]
    [Category( "Examples" )]
    [Description( "Loads the Rock Solid Church sample data into your Rock system." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [TextField( "XML Document URL",
        Description = @"The URL for the input sample data XML document. You can also use a local Windows file path (e.g. C:\Rock\Documentation\sampledata_1_14_0.xml) if you want to test locally with your own fake data. The file format is loosely defined on the Rock Solid Demo Church Specification wiki.",
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

    [Rock.SystemGuid.EntityTypeGuid( "14573690-56FE-454D-A879-5EA7E88E75CE" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "A3958DEF-802C-489A-BCB9-86543ECE30EC" )]
    [Rock.SystemGuid.BlockTypeGuid( "A42E0031-B2B9-403A-845B-9C968D7716A6" )]
    public class SampleData : RockBlockType
    {
        #region Keys

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

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<SampleDataBag, SampleDataOptionsBag>();

            var isStopwatchEnabled = GetAttributeValue( AttributeKey.EnableStopwatch ).AsBoolean() || RockApp.Current.HostingSettings.IsDevelopmentEnvironment;
            var xmlFileUrl = GetAttributeValue( AttributeKey.XMLDocumentURL );

            box.Bag = new SampleDataBag
            {
                IsXmlFileAvailable = VerifyXmlDocumentExists( xmlFileUrl ),
                IsStopwatchEnabled = isStopwatchEnabled,
                XmlDocumentUrl = xmlFileUrl
            };

            return box;
        }

        /// <summary>
        /// Verifies that the configured XML document exists and is accessible.
        /// </summary>
        /// <param name="xmlFileUrl">The URL or file path of the XML document.</param>
        /// <returns><c>true</c> if the file is accessible; otherwise <c>false</c>.</returns>
        private bool VerifyXmlDocumentExists( string xmlFileUrl )
        {
            try
            {
                var fileUri = new Uri( xmlFileUrl );

                if ( fileUri.IsFile )
                {
                    return File.Exists( fileUri.LocalPath );
                }

                var request = ( HttpWebRequest ) WebRequest.Create( xmlFileUrl );
                request.Method = "HEAD";
                request.Timeout = 10000;

                using ( var response = ( HttpWebResponse ) request.GetResponse() )
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch
            {
                // Intentionally ignored: network or file access errors indicate
                // the file is not available.
                return false;
            }
        }

        /// <summary>
        /// Gets the sample data import action arguments from the current block settings.
        /// </summary>
        /// <param name="password">The password to use for any user logins created in the sample data.</param>
        /// <returns>The configured import arguments.</returns>
        private SampleDataManager.SampleDataImportActionArgs GetSampleDataImportArgs( string password, bool isStopwatchEnabled )
        {
            var args = new SampleDataManager.SampleDataImportActionArgs
            {
                Password = password,
                RandomizerSeed = GetAttributeValue( AttributeKey.RandomNumberSeed ).AsIntegerOrNull(),
                EnableStopwatch = isStopwatchEnabled,
                CreatorPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId.GetValueOrDefault( 0 ) ?? 0,
                DeleteExistingData = GetAttributeValue( AttributeKey.DeleteDataFirst ).AsBoolean(),
                EnableGiving = GetAttributeValue( AttributeKey.EnableGiving ).AsBoolean(),
                FabricateAttendance = GetAttributeValue( AttributeKey.FabricateAttendance ).AsBoolean(),
                ProcessOnlyGivingData = GetAttributeValue( AttributeKey.ProcessOnlyGivingData ).AsBoolean(),
                RegistrationConfirmationEmailTemplate = RegistrationTemplateDefaults.ConfirmationEmail,
                RegistrationReminderEmailTemplate = RegistrationTemplateDefaults.ReminderEmail,
                RegistrationSuccessText = RegistrationTemplateDefaults.SuccessText,
                RegistrationPaymentReminderTemplate = RegistrationTemplateDefaults.PaymentReminderEmail
            };

            return args;
        }

        /// <summary>
        /// Downloads the given file URL and stores it at the specified output path.
        /// </summary>
        /// <param name="fileUrl">The URL or file path to fetch.</param>
        /// <param name="fileOutput">The full path location to store the file.</param>
        /// <param name="errorMessage">When the download fails, contains a description of the error.</param>
        /// <returns><c>true</c> if the download was successful; otherwise <c>false</c>.</returns>
        private bool DownloadFile( string fileUrl, string fileOutput, out string errorMessage )
        {
            errorMessage = null;

            try
            {
                var fileUri = new Uri( fileUrl );

                if ( fileUri.IsFile )
                {
                    File.Copy( fileUrl, fileOutput, true );
                }
                else
                {
                    using ( var client = new WebClient() )
                    {
                        client.DownloadFile( fileUri, fileOutput );
                    }
                }

                return true;
            }
            catch ( Exception ex )
            {
                // Intentionally caught: error details are passed back to the
                // caller via the out parameter.
                errorMessage = string.Format( "While trying to fetch {0}, {1}", fileUrl, ex.Message );
                return false;
            }
        }

        /// <summary>
        /// Extracts the stories from the XML comments and returns them as an HTML list.
        /// </summary>
        /// <param name="saveFile">The path to the XML file.</param>
        /// <returns>An HTML string with the stories, or an empty string if none.</returns>
        private string GetStories( string saveFile )
        {
            try
            {
                var xdoc = XDocument.Load( saveFile );
                var sb = new StringBuilder();
                sb.Append( "<ul>" );

                foreach ( var comment in xdoc.Element( "data" ).DescendantNodes().OfType<XComment>() )
                {
                    sb.AppendFormat( "<li>{0}</li>", comment.ToString().Replace( "<!--", string.Empty ).Replace( "-->", string.Empty ) );
                }

                sb.Append( "</ul>" );
                return sb.ToString();
            }
            catch
            {
                // Intentionally ignored: if the XML cannot be parsed for
                // stories, return empty so the success message still shows.
                return string.Empty;
            }
        }

        /// <summary>
        /// Records the current date into the SampleData system setting
        /// so that other blocks (such as the RockUpdate) can know that
        /// sample data has been loaded.
        /// </summary>
        /// <param name="xmlFileUrl">The XML file URL used for the import.</param>
        private void RecordSuccess( string xmlFileUrl )
        {
            if ( xmlFileUrl.StartsWith( "http://storage.rockrms.com/sampledata/" ) )
            {
                Web.SystemSettings.SetValue( SystemKey.SystemSetting.SAMPLEDATA_DATE, RockDateTime.Now.ToString( "s" ) );
            }
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Loads sample data into the Rock database. This starts the import
        /// as a background task and returns immediately. Progress is reported
        /// via the TaskActivityProgress RealTime topic.
        /// </summary>
        /// <param name="password">The password to use for any user logins created in the sample data.</param>
        /// <param name="sessionId">The RealTime connection identifier for progress reporting.</param>
        /// <returns>A result indicating whether the import was started successfully.</returns>
        [BlockAction]
        public BlockActionResult LoadSampleData( string password, string sessionId )
        {
            // Validate the real-time connection is established before starting.
            if ( string.IsNullOrWhiteSpace( sessionId ) )
            {
                return ActionBadRequest( "A real-time connection is required. Please wait a moment and try again." );
            }

            var xmlFileUrl = GetAttributeValue( AttributeKey.XMLDocumentURL );
            var isStopwatchEnabled = GetAttributeValue( AttributeKey.EnableStopwatch ).AsBoolean() || RockApp.Current.HostingSettings.IsDevelopmentEnvironment;
            var saveFile = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "sampledata1.xml" );
            var virtualRootPath = RockApp.Current.HostingSettings.VirtualRootPath;

            // Validate the XML file is available before starting the import.
            if ( !VerifyXmlDocumentExists( xmlFileUrl ) )
            {
                return ActionBadRequest( "The sample data file is not available. Please check the XML Document URL setting." );
            }

            // Capture the import args on this thread (accesses block settings
            // which require the request context).
            var args = GetSampleDataImportArgs( password, isStopwatchEnabled );

            Task.Run( async () =>
            {
                // Small delay so the browser can render and start listening.
                await Task.Delay( 1000 );

                TaskActivityProgress progress = null;

                try
                {
                    var taskChannelName = $"SampleDataImport:{Guid.NewGuid()}";
                    var topic = RealTimeHelper.GetTopicContext<ITaskActivityProgress>();

                    await topic.Channels.AddToChannelAsync( sessionId, taskChannelName );

                    var progressReporter = topic.Clients.Channel( taskChannelName );
                    progress = new TaskActivityProgress( progressReporter, "Sample Data Import" );
                    progress.StartNotificationDelayMilliseconds = 0;
                    progress.StartTask( "Starting sample data import..." );

                    // Download the XML file.
                    if ( !DownloadFile( xmlFileUrl, saveFile, out var downloadError ) )
                    {
                        progress.StopTask( "Failed to download the sample data file.", new[] { downloadError ?? "The file could not be downloaded from the configured URL." } );
                        return;
                    }

                    // Configure the logger for real-time updates.
                    var logLevel = isStopwatchEnabled ? LogLevel.Trace : LogLevel.Information;
                    var logger = new RockLoggerMemoryBuffer( logLevel );
                    logger.EventLogged += ( sender, e ) =>
                    {
                        if ( isStopwatchEnabled )
                        {
                            var message = e.Event.Message;
                            if ( !string.IsNullOrWhiteSpace( message ) )
                            {
                                progress.LogMessage( message );
                            }
                        }
                    };

                    // Run the import.
                    var manager = new SampleDataManager( logger );
                    manager.IsUnitTest = false;
                    manager.CreateFromXmlDocumentFile( saveFile, args );

                    // Record success and extract stories.
                    RecordSuccess( xmlFileUrl );
                    var storiesHtml = GetStories( saveFile );

                    var personSearchPath = $"{virtualRootPath.TrimEnd( '/' )}/Person/Search/name/?SearchTerm=Decker";
                    var successMessage = string.Format(
                        "<p>Happy tire-kicking! The data is in your database. Hint: try <a href='{0}'>searching for the Decker family</a>.</p><p>Here are some of the things you'll find in the sample data:</p>{1}",
                        personSearchPath,
                        storiesHtml );

                    progress.StopTask( successMessage );
                }
                catch ( Exception ex )
                {
                    var errorMessage = string.Format(
                        "That wasn't supposed to happen. The error was:<br/>{0}<br/>{1}<br/>{2}",
                        System.Net.WebUtility.HtmlEncode( ex.Message ),
                        FlattenInnerExceptions( ex.InnerException ),
                        System.Net.WebUtility.HtmlEncode( ex.StackTrace ).Replace( "\r\n", "<br/>" ).Replace( "\n", "<br/>" ) );

                    progress?.StopTask( errorMessage, new[] { ex.Message } );
                }
                finally
                {
                    progress?.Dispose();

                    if ( File.Exists( saveFile ) )
                    {
                        File.Delete( saveFile );
                    }
                }
            } );

            return ActionOk();
        }

        /// <summary>
        /// Flattens an exception's inner exceptions and returns an HTML-formatted
        /// string useful for debugging.
        /// </summary>
        /// <param name="ex">The inner exception to flatten.</param>
        /// <returns>An HTML string of the inner exception messages.</returns>
        private string FlattenInnerExceptions( Exception ex )
        {
            var sb = new StringBuilder();

            while ( ex != null && ex.InnerException != null )
            {
                sb.Append( System.Net.WebUtility.HtmlEncode( ex.InnerException.Message )
                    .Replace( "\r\n", "<br/>" )
                    .Replace( "\n", "<br/>" ) );
                sb.Append( "<br/>" );
                ex = ex.InnerException;
            }

            return sb.ToString();
        }

        #endregion Block Actions
    }
}
