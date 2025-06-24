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
using System.Web.Hosting;

using Rock.Attribute;
using Rock.Model;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.Slingshot;
using Rock.Utility;
using Rock.ViewModels.Blocks.BulkImport;
using Rock.Web;

namespace Rock.Blocks.BulkImport
{
    /// <summary>
    /// Block to import Slingshot files into Rock using BulkImport
    /// </summary>
    [DisplayName( "Bulk Import" )]
    [Category( "Bulk Import" )]
    [Description( "Block to import Slingshot files into Rock using BulkImport" )]

    [IntegerField(
        "Person Record Import Batch Size",
        Description = "If importing more than this many records, the import will be broken up into smaller batches to optimize memory use. If you run into memory utilization problems while importing a large number of records, consider decreasing this value. (A value less than 1 will result in the default of 25,000 records.)",
        Key = AttributeKey.PersonRecordImportBatchSize,
        DefaultIntegerValue = 25000,
        IsRequired = true,
        Order = 0 )]

    [IntegerField(
        "Financial Record Import Batch Size",
        Description = "If importing more than this many records, the import will be broken up into smaller batches to optimize memory use. If you run into memory utilization problems while importing a large number of records, consider decreasing this value. (A value less than 1 will result in the default of 100,000 records.)",
        Key = AttributeKey.FinancialRecordImportBatchSize,
        DefaultIntegerValue = 100000,
        IsRequired = true,
        Order = 1 )]

    [Rock.SystemGuid.BlockTypeGuid( "66f5882f-163c-4616-9b39-2f063611db22" )]
    [Rock.SystemGuid.EntityTypeGuid( "5B41F45E-2E09-4F97-8BEA-683AFFE0EB62")]
    public class BulkImportTool : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string PersonRecordImportBatchSize = "PersonRecordImportBatchSize";
            public const string FinancialRecordImportBatchSize = "FinancialRecordImportBatchSize";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new BulkImportToolBox();

            box.Options = GetBoxOptions();
            box.RootFolder = Rock.Security.Encryption.EncryptString( GetSlingshotRootFolder() );

            return box;
        }

        /// <summary>
        /// Gets the root folder path for slingshot files.
        /// </summary>
        /// <returns>The virtual path to the slingshot files directory.</returns>
        private string GetSlingshotRootFolder()
        {
            string virtualPath = "~/App_Data/SlingshotFiles";
            string physicalPath = HostingEnvironment.MapPath( virtualPath );

            if ( !Directory.Exists( physicalPath ) )
            {
                Directory.CreateDirectory( physicalPath );
            }

            return virtualPath;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private BulkImportToolOptionsBag GetBoxOptions()
        {
            var options = new BulkImportToolOptionsBag();
            return options;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Starts the import process.
        /// </summary>
        /// <param name="request">The import request containing necessary parameters.</param>
        /// <returns>A result indicating success or failure of the import operation.</returns>
        [BlockAction]
        public BlockActionResult StartImport( BulkImportRequest request )
        {
            if ( request == null )
            {
                return ActionBadRequest( "Import request is required." );
            }

            var physicalSlingshotFile = request.SlingshotFilePath;
            if ( string.IsNullOrWhiteSpace( physicalSlingshotFile ) )
            {
                return ActionBadRequest( "Slingshot file path is required." );
            }

            if ( !File.Exists( physicalSlingshotFile ) )
            {
                return ActionBadRequest( "Slingshot file not found." );
            }

            var importTask = new Task( () =>
            {
                // Wait a little so the browser can render and start listening to events
                Task.Delay( 1000 ).Wait();

                var stopwatch = Stopwatch.StartNew();
                long totalMilliseconds = 0;

                BulkImporter.ImportUpdateType importUpdateType;

                switch ( request.ImportUpdateType )
                {
                    case "AddOnly":
                        importUpdateType = BulkImporter.ImportUpdateType.AddOnly;
                        break;
                    case "MostRecentWins":
                        importUpdateType = BulkImporter.ImportUpdateType.MostRecentWins;
                        break;
                    default:
                        importUpdateType = BulkImporter.ImportUpdateType.AlwaysUpdate;
                        break;
                }

                var progressReporter = RealTimeHelper.GetTopicContext<ITaskActivityProgress>().Clients.All;
                var progress = new TaskActivityProgress( progressReporter, "Bulk Import" );
                progress.StartTask( "Starting import..." );

                var slingshotImporter = new SlingshotImporter( physicalSlingshotFile, request.ForeignSystemKey, importUpdateType, ( sender, e ) =>
                {
                    var importer = sender as SlingshotImporter;
                    string progressMessage = string.Empty;
                    var progressResults = new DescriptionList();

                    if ( e is string )
                    {
                        progressMessage = e.ToString();
                    }

                    var exceptionsCopy = importer.Exceptions.ToArray();
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

                    var resultsCopy = importer.Results.ToArray();
                    foreach ( var result in resultsCopy )
                    {
                        progressResults.Add( result.Key, result.Value );
                    }

                    progress.LogMessage( progressMessage );
                    if ( !string.IsNullOrEmpty( progressResults.Html ) )
                    {
                        progress.LogMessage( progressResults.Html );
                    }
                } );

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

                try
                {
                    switch ( request.ImportType )
                    {
                        case "Photos":
                            slingshotImporter.TEST_UseSampleLocalPhotos = false;
                            slingshotImporter.DoImportPhotos();
                            break;
                        case "All":
                            slingshotImporter.DoImport();
                            slingshotImporter.TEST_UseSampleLocalPhotos = false;
                            slingshotImporter.DoImportPhotos();
                            break;
                        default:
                            slingshotImporter.DoImport();
                            break;
                    }

                    stopwatch.Stop();
                    totalMilliseconds = stopwatch.ElapsedMilliseconds;

                    if ( slingshotImporter.Exceptions.Any() )
                    {
                        slingshotImporter.Results.Add( "ERRORS", string.Join( Environment.NewLine, slingshotImporter.Exceptions.Select( a => a.Message ).ToArray() ) );
                        var errors = slingshotImporter.Exceptions.Select( a => a.Message ).ToList();
                        progress.StopTask( "Import failed with errors", errors );
                        return;
                    }

                    progress.StopTask( $"{request.ImportType} Complete: [{totalMilliseconds}ms]" );
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                    if ( slingshotImporter.Exceptions != null )
                    {
                        slingshotImporter.Exceptions.Add( ex.GetBaseException() );
                    }
                    progress.StopTask( "ERROR: " + ex.Message, new[] { ex.Message } );
                    throw;
                }
                finally
                {
                    progress.Dispose();
                }
            } );

            try
            {
                importTask.Start();
                return ActionOk();
            }
            catch ( Exception ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Checks if a foreign system key has been used before.
        /// </summary>
        /// <param name="foreignSystemKey">The foreign system key to check.</param>
        /// <returns>Information about the foreign system key usage.</returns>
        [BlockAction]
        public BlockActionResult CheckForeignSystemKey( string foreignSystemKey )
        {
            if ( string.IsNullOrWhiteSpace( foreignSystemKey ) )
            {
                return ActionBadRequest( "Foreign system key is required." );
            }

            var tableList = Rock.Slingshot.BulkImporter.TablesThatHaveForeignSystemKey( foreignSystemKey );
            var foreignSystemKeyList = BulkImporter.UsedForeignSystemKeys();

            var result = new
            {
                HasBeenUsed = tableList.Any(),
                Tables = tableList,
                UsedKeys = foreignSystemKeyList
            };

            return ActionOk( result );
        }

        /// <summary>
        /// Handles the Click event of the Download Log Button.
        /// </summary>
        [BlockAction]
        public BlockActionResult DownloadLog()
        {
            string physicalRootFolder = AppDomain.CurrentDomain.BaseDirectory;
            string slingshotFilesDir = Path.Combine( physicalRootFolder, "App_Data", "SlingshotFiles" );
            string filePath = Path.Combine( slingshotFilesDir, "slingshot-errors.log" );

            if ( !Directory.Exists( slingshotFilesDir ) )
            {
                Directory.CreateDirectory( slingshotFilesDir );
            }

            var ms = new MemoryStream();
            if ( File.Exists( filePath ) )
            {
                using ( var fileStream = new FileStream( filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
                {
                    fileStream.CopyTo( ms );
                }
            }
            ms.Seek( 0, SeekOrigin.Begin );

            return new FileBlockActionResult( ms, "text/plain", "slingshot-errors.log" );
        }

        [BlockAction]
        public BlockActionResult GetFileInfo( string filePath )
        {
            if ( string.IsNullOrWhiteSpace( filePath ) )
            {
                return ActionBadRequest( "File path is required." );
            }

            string physicalRootFolder = AppDomain.CurrentDomain.BaseDirectory;
            string slingshotFilesDir = Path.Combine( physicalRootFolder, "App_Data", "SlingshotFiles" );
            string physicalFilePath = Path.Combine( slingshotFilesDir, Path.GetFileName( filePath ) );

            if ( !File.Exists( physicalFilePath ) )
            {
                return ActionBadRequest( "File not found." );
            }

            FileInfo fileInfo = new FileInfo( physicalFilePath );
            var result = new
            {
                fileName = fileInfo.Name,
                size = Math.Round( ( decimal )fileInfo.Length / 1024 / 1024, 2 ),
                createdDateTime = fileInfo.CreationTime.ToString(),
                fullPath = fileInfo.FullName
            };

            return ActionOk( result );
        }

        [BlockAction]
        public BlockActionResult GetImageFiles( string prefix )
        {
            if ( string.IsNullOrWhiteSpace( prefix ) )
            {
                return ActionBadRequest( "File prefix is required." );
            }

            string physicalRootFolder = AppDomain.CurrentDomain.BaseDirectory;
            string slingshotFilesDir = Path.Combine( physicalRootFolder, "App_Data", "SlingshotFiles" );

            if ( !Directory.Exists( slingshotFilesDir ) )
            {
                return ActionOk( new string[] { } );
            }

            var imageFiles = Directory.EnumerateFiles( slingshotFilesDir, prefix + "*.images.slingshot" )
                                    .Select( path => Path.GetFileName( path ) )
                                    .ToList();

            return ActionOk( imageFiles );
        }

        #endregion
    }
}