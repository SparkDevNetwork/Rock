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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Slingshot;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.BulkImport
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Bulk Import" )]
    [Category( "Bulk Import" )]
    [Description( "Block to import Slingshot files into Rock using BulkImport" )]

    [IntegerField(
        "Person Record Import Batch Size",
        Description = "If importing more than this many records, the import will be broken up into smaller batches to optimize memory use.  If you run into memory utilization problems while importing a large number of records, consider decreasing this value.  (A value less than 1 will result in the default of 25,000 records.)",
        Key = AttributeKey.PersonRecordImportBatchSize,
        DefaultIntegerValue = 25000,
        IsRequired = true,
        Order = 0 )]

    [IntegerField(
        "Financial Record Import Batch Size",
        Description = "If importing more than this many records, the import will be broken up into smaller batches to optimize memory use.  If you run into memory utilization problems while importing a large number of records, consider decreasing this value.  (A value less than 1 will result in the default of 100,000 records.)",
        Key = AttributeKey.FinancialRecordImportBatchSize,
        DefaultIntegerValue = 100000,
        IsRequired = true,
        Order = 1 )]

    [Rock.SystemGuid.BlockTypeGuid( "D9302E4A-C498-4CD7-8D3B-0E9DA9802DD5" )]
    public partial class BulkImportTool : RockBlock
    {
        private static class AttributeKey
        {
            public const string PersonRecordImportBatchSize = "PersonRecordImportBatchSize";
            public const string FinancialRecordImportBatchSize = "FinancialRecordImportBatchSize";
        }

        #region Fields and Properties

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        /// <summary>
        /// Gets the signal r notification key.
        /// </summary>
        /// <value>
        /// The signal r notification key.
        /// </value>
        public string SignalRNotificationKey
        {
            get
            {
                return string.Format( "BulkImport_BlockId:{0}_SessionId:{1}", this.BlockId, Session.SessionID );
            }
        }

        #endregion Fields and Properties

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( this.IsPostBack )
            {
                if ( !string.IsNullOrEmpty( tbForeignSystemKey.Text ) )
                {
                    btnCheckForeignSystemKey_Click( null, null );
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the FileUploaded event of the fupSlingshotFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void fupSlingshotFile_FileUploaded( object sender, EventArgs e )
        {
            pnlActions.Visible = hfMainSlingshotFileName.Value != string.Empty;
            var physicalSlingshotFile = this.Request.MapPath( fupSlingshotFile.UploadedContentFilePath );
            if ( File.Exists( physicalSlingshotFile ) )
            {
                FileInfo fileInfo = new FileInfo( physicalSlingshotFile );
                if ( fileInfo.Extension.Equals( ".slingshot", StringComparison.OrdinalIgnoreCase ) )
                {
                    if ( !fileInfo.FullName.EndsWith( ".images.slingshot", StringComparison.OrdinalIgnoreCase ) )
                    {
                        hfMainSlingshotFileName.Value = fupSlingshotFile.UploadedContentFilePath;
                        lSlingshotFileInfo.Text = new DescriptionList()
                            .Add( "File Name", fileInfo.Name )
                            .Add( "Date/Time", fileInfo.CreationTime )
                            .Add( "Size (MB) ", Math.Round( ( ( decimal ) fileInfo.Length / 1024 / 1024 ), 2 ) )
                            .Html;
                        pnlActions.Visible = true;
                        
                    }

                    if ( hfMainSlingshotFileName.Value != string.Empty )
                    {
                        // list any matching images.slingshot files that either got uploaded or manually placed in the ~/App_Data/SlingshotFiles folder
                        var mainFileNamePrefix = Path.GetFileNameWithoutExtension( hfMainSlingshotFileName.Value );
                        var imageFiles = Directory.EnumerateFiles( Path.GetDirectoryName( fileInfo.FullName ), mainFileNamePrefix + "*.images.slingshot" );
                        if ( imageFiles.Any() )
                        {
                            lAdditionalSlingshotFilesInfo.Text = "<label>Additional Files</label>: " + imageFiles.Select( a => Path.GetFileName( a ) ).ToList().AsDelimited( ", " );
                        }
                        else
                        {
                            lAdditionalSlingshotFilesInfo.Text = string.Empty;
                        }
                    }
                }
                else
                {
                    lSlingshotFileInfo.Text = "<div class='alert alert-warning'>" + fileInfo.Name + " is not a slingshot file. Please select a valid slingshot file.</div>";
                    File.Delete( physicalSlingshotFile );
                }
            }
            else
            {
                lSlingshotFileInfo.Text = "-";
                lAdditionalSlingshotFilesInfo.Text = "";
            }
        }

        /// <summary>
        /// Handles the FileRemoved event of the fupSlingshotFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fupSlingshotFile_FileRemoved( object sender, FileUploaderEventArgs e )
        {
            pnlActions.Visible = false;
            hfMainSlingshotFileName.Value = "";
            lSlingshotFileInfo.Text = "";
            lAdditionalSlingshotFilesInfo.Text = "";
        }

        /// <summary>
        /// 
        /// </summary>
        private enum ImportType
        {
            Import,
            Photos,
            All
        }

        /// <summary>
        /// Handles the Click event of the btnImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImport_Click( object sender, EventArgs e )
        {
            StartImport( ImportType.Import );
        }

        /// <summary>
        /// Handles the Click event of the btnImportPhotos control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnImportPhotos_Click( object sender, EventArgs e )
        {
            StartImport( ImportType.Photos );
        }

        /// <summary>
        /// Handles the Click event of the btnCheckForeignSystemKey control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCheckForeignSystemKey_Click( object sender, EventArgs e )
        {
            nbCheckForeignSystemKey.Visible = false;
            nbCheckForeignSystemKey.Text = string.Empty;
            if ( !string.IsNullOrEmpty( tbForeignSystemKey.Text ) )
            {
                var tableList = Rock.Slingshot.BulkImporter.TablesThatHaveForeignSystemKey( tbForeignSystemKey.Text );

                if ( !tableList.Any() )
                {
                    nbCheckForeignSystemKey.Text = "OK. Foreign System Key <strong>" + tbForeignSystemKey.Text + "</strong> has not been used to import data.";
                    nbCheckForeignSystemKey.NotificationBoxType = NotificationBoxType.Success;
                }
                else
                {
                    nbCheckForeignSystemKey.Text = "Foreign System Key <strong>" + tbForeignSystemKey.Text + "</strong> has already been used. Import again to insert any new records that are detected, and update any person records that have changed.";
                    nbCheckForeignSystemKey.NotificationBoxType = NotificationBoxType.Info;
                }

                nbCheckForeignSystemKey.Details = tableList.AsDelimited( "<br />" );
                nbCheckForeignSystemKey.Visible = true;
            }

            var foreignSystemKeyList = BulkImporter.UsedForeignSystemKeys();

            if ( nbCheckForeignSystemKey.Text == string.Empty )
            {
                nbCheckForeignSystemKey.NotificationBoxType = NotificationBoxType.Default;
            }
            else
            {
                nbCheckForeignSystemKey.Text += "<br /><br />";
            }

            if ( foreignSystemKeyList.Any() )
            {
                nbCheckForeignSystemKey.Text += "The following ForeignSystemKeys have been used from previous imports:<br /><br />" + foreignSystemKeyList.AsDelimited( "<br />" );
            }
            else
            {
                nbCheckForeignSystemKey.Text += "No ForeignSystemKeys have been used from previous imports";
            }

            nbCheckForeignSystemKey.Text += "<br />";

            nbCheckForeignSystemKey.Visible = true;
        }

        /// <summary>
        /// Starts the import.
        /// </summary>
        /// <param name="importType">Type of the import.</param>
        private void StartImport( ImportType importType )
        {
            var physicalSlingshotFile = this.Request.MapPath( hfMainSlingshotFileName.Value );
            long totalMilliseconds = 0;

            SlingshotImporter slingshotImporter = null;

            var importTask = new Task( () =>
            {
                // wait a little so the browser can render and start listening to events
                Task.Delay( 1000 ).Wait();
                _hubContext.Clients.All.showButtons( this.SignalRNotificationKey, false );

                Stopwatch stopwatch = Stopwatch.StartNew();

                BulkImporter.ImportUpdateType importUpdateType;

                if ( rbOnlyAddNewRecords.Checked )
                {
                    importUpdateType = BulkImporter.ImportUpdateType.AddOnly;
                }
                else if ( rbMostRecentWins.Checked )
                {
                    importUpdateType = BulkImporter.ImportUpdateType.MostRecentWins;
                }
                else
                {
                    importUpdateType = BulkImporter.ImportUpdateType.AlwaysUpdate;
                }

                slingshotImporter = new SlingshotImporter( physicalSlingshotFile, tbForeignSystemKey.Text, importUpdateType, slingshotImporter_OnProgress );
                slingshotImporter.PersonChunkSize = 25000;
                slingshotImporter.FinancialTransactionChunkSize = 100000;

                var personChunkSize = GetAttributeValue( AttributeKey.PersonRecordImportBatchSize ).AsInteger();
                var financialTransactionChunkSize = GetAttributeValue( AttributeKey.FinancialRecordImportBatchSize ).AsInteger();

                if ( personChunkSize > 0 )
                {
                    slingshotImporter.PersonChunkSize = personChunkSize;
                }
                if ( financialTransactionChunkSize > 0 )
                {
                    slingshotImporter.FinancialTransactionChunkSize = financialTransactionChunkSize;
                }

                if ( importType == ImportType.Import || importType == ImportType.All )
                {
                    slingshotImporter.DoImport();
                }

                if ( importType == ImportType.Photos || importType == ImportType.All )
                {
                    slingshotImporter.TEST_UseSampleLocalPhotos = false;
                    slingshotImporter.DoImportPhotos();
                }

                stopwatch.Stop();

                if ( slingshotImporter.Exceptions.Any() )
                {
                    slingshotImporter.Results.Add( "ERRORS", string.Join( Environment.NewLine, slingshotImporter.Exceptions.Select( a => a.Message ).ToArray() ) );
                }

                totalMilliseconds = stopwatch.ElapsedMilliseconds;

                _hubContext.Clients.All.showButtons( this.SignalRNotificationKey, true );
            } );

            importTask.ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
                    foreach ( var exception in t.Exception.InnerExceptions )
                    {
                        LogException( exception );
                        if ( slingshotImporter.Exceptions != null )
                        {
                            slingshotImporter.Exceptions.Add( exception.GetBaseException() );
                        }
                    }

                    slingshotImporter_OnProgress( slingshotImporter, "ERROR: " + t.Exception.Message );
                }
                else
                {
                    slingshotImporter_OnProgress( slingshotImporter, string.Format( "{0} Complete: [{1}ms]", importType.ConvertToString(), totalMilliseconds ) );
                }

            } );

            importTask.Start();
        }

        /// <summary>
        /// Handles the ProgressChanged event of the BackgroundWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ProgressChangedEventArgs"/> instance containing the event data.</param>
        private void slingshotImporter_OnProgress( object sender, object e )
        {
            var slingshotImporter = sender as SlingshotImporter;

            string progressMessage = string.Empty;
            DescriptionList progressResults = new DescriptionList();
            if ( e is string )
            {
                progressMessage = e.ToString();
            }

            var exceptionsCopy = slingshotImporter.Exceptions.ToArray();
            if ( exceptionsCopy.Any() )
            {
                if ( exceptionsCopy.Count() > 50 )
                {
                    var exceptionsSummary = exceptionsCopy.GroupBy( a => a.GetBaseException().Message ).Select( a => a.Key + "(" + a.Count().ToString() + ")" );
                    progressResults.Add( "Exceptions", string.Join( Environment.NewLine, exceptionsSummary ) );
                }
                else
                {
                    progressResults.Add( "Exception", string.Join( Environment.NewLine, exceptionsCopy.Select( a => a.Message ).ToArray() ) );
                }
            }

            var resultsCopy = slingshotImporter.Results.ToArray();
            foreach ( var result in resultsCopy )
            {
                progressResults.Add( result.Key, result.Value );
            }

            WriteProgressMessage( progressMessage, progressResults.Html );
        }

        /// <summary>
        /// Writes the progress message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void WriteProgressMessage( string message, string results )
        {
            _hubContext.Clients.All.receiveNotification( this.SignalRNotificationKey, message, results.ConvertCrLfToHtmlBr() );
        }

        /// <summary>
        /// Handles the Click event of the btnDownloadLog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDownloadLog_Click( object sender, EventArgs e )
        {
            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
            response.ClearHeaders();
            response.ClearContent();
            response.Clear();
            response.ContentType = "text/plain";
            response.AddHeader( "content-disposition", "attachment; filename=slingshot-errors.log" );
            response.Charset = "";

            string filePath = Server.MapPath( "~/App_Data/SlingshotFiles/slingshot-errors.log" );
            if ( File.Exists( filePath ) )
            {
                response.TransmitFile( filePath );
            }

            response.Flush();
            response.End();
            response.SuppressContent = true;
            System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        #endregion

    }
}