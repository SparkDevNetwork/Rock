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
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

using Rock;
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
    public partial class BulkImportTool : RockBlock
    {

        #region Fields

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

        #endregion Fields

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", fingerprint: false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( this.IsPostBack )
            {
                if ( !string.IsNullOrEmpty( tbForeignSystemKey.Text ) )
                {
                    btnCheckForeignSystemKey_Click( null, null );
                }
            }
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
            pnlActions.Visible = false;
            var physicalSlingshotFile = this.Request.MapPath( fupSlingshotFile.UploadedContentFilePath );
            if ( File.Exists( physicalSlingshotFile ) )
            {
                FileInfo fileInfo = new FileInfo( physicalSlingshotFile );
                if ( fileInfo.Extension.Equals( ".slingshot", StringComparison.OrdinalIgnoreCase ) )
                {
                    lSlingshotFileInfo.Text = new DescriptionList()
                        .Add( "File Name", fileInfo.Name )
                        .Add( "Date/Time", fileInfo.CreationTime )
                        .Add( "Size (MB) ", Math.Round( ( ( decimal ) fileInfo.Length / 1024 / 1024 ), 2 ) )
                        .Html;
                    pnlActions.Visible = true;
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
            lSlingshotFileInfo.Text = "";
        }

        /// <summary>
        /// 
        /// </summary>
        private enum ImportType
        {
            Import,
            ImportPhotos
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
            StartImport( ImportType.ImportPhotos );
        }

        /// <summary>
        /// Handles the Click event of the btnCheckForeignSystemKey control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCheckForeignSystemKey_Click( object sender, EventArgs e )
        {
            nbCheckForeignSystemKey.Visible = false;
            if ( !string.IsNullOrEmpty( tbForeignSystemKey.Text ) )
            {
                var tableList = Rock.Slingshot.BulkImporter.TablesThatHaveForeignSystemKey( tbForeignSystemKey.Text );

                if ( !tableList.Any() )
                {
                    nbCheckForeignSystemKey.Text = "OK. Foreign System Key <strong>" + tbForeignSystemKey.Text + "</strong> has not be used to import data.";
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
            else
            {
                var foreignSystemKeyList = BulkImporter.UsedForeignSystemKeys();
                if ( foreignSystemKeyList.Any() )
                {
                    nbCheckForeignSystemKey.Text = "The following ForeignSystemKeys have been used from previous imports:<br /><br />" + foreignSystemKeyList.AsDelimited( "<br />" );
                    nbCheckForeignSystemKey.NotificationBoxType = NotificationBoxType.Default;
                    nbCheckForeignSystemKey.Visible = true;
                }
            }
        }

        /// <summary>
        /// Starts the import.
        /// </summary>
        /// <param name="importType">Type of the import.</param>
        private void StartImport( ImportType importType )
        {
            var physicalSlingshotFile = this.Request.MapPath( fupSlingshotFile.UploadedContentFilePath );
            long totalMilliseconds = 0;

            Rock.Slingshot.SlingshotImporter _importer = null;

            var importTask = new Task( () =>
            {
                // wait a little so the browser can render and start listening to events
                System.Threading.Thread.Sleep( 1000 );
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

                _importer = new Rock.Slingshot.SlingshotImporter( physicalSlingshotFile, tbForeignSystemKey.Text, importUpdateType );
                _importer.FinancialTransactionChunkSize = 100000;
                _importer.OnProgress += _importer_OnProgress;

                if ( importType == ImportType.ImportPhotos )
                {
                    _importer.TEST_UseSampleLocalPhotos = false;
                    _importer.DoImportPhotos();
                }
                else
                {
                    _importer.DoImport();
                }

                stopwatch.Stop();

                if ( _importer.Exceptions.Any() )
                {
                    _importer.Results.Add( "ERRORS", string.Join( Environment.NewLine, _importer.Exceptions.Select( a => a.Message ).ToArray() ) );
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
                        _importer.Exceptions.Add( exception.GetBaseException() );
                    }

                    _importer_OnProgress( _importer, "ERROR" );
                }
                else
                {
                    _importer_OnProgress( _importer, string.Format( "{0} Complete: [{1}ms]", importType.ConvertToString(), totalMilliseconds ) );
                }

            } );

            importTask.Start();
        }

        /// <summary>
        /// Handles the ProgressChanged event of the BackgroundWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ProgressChangedEventArgs"/> instance containing the event data.</param>
        private void _importer_OnProgress( object sender, object e )
        {
            Rock.Slingshot.SlingshotImporter _importer = sender as Rock.Slingshot.SlingshotImporter;

            string progressMessage = string.Empty;
            DescriptionList progressResults = new DescriptionList();
            if ( e is string )
            {
                progressMessage = e.ToString();
            }

            var exceptionsCopy = _importer.Exceptions.ToArray();
            if ( exceptionsCopy.Any() )
            {
                progressResults.Add( "Exception", string.Join( Environment.NewLine, exceptionsCopy.Select( a => a.Message ).ToArray() ) );
            }

            var resultsCopy = _importer.Results.ToArray();
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
            System.Diagnostics.Debug.WriteLine( message );
            System.Diagnostics.Debug.WriteLine( results );
            _hubContext.Clients.All.receiveNotification( this.SignalRNotificationKey, message, results.ConvertCrLfToHtmlBr() );
        }

        #endregion


    }
}