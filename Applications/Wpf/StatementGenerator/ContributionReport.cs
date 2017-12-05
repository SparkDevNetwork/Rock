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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenHtmlToPdf;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Rock.Net;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public class ContributionReport
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContributionReport"/> class.
        /// </summary>
        public ContributionReport( Rock.StatementGenerator.StatementGeneratorOptions options )
        {
            this.Options = options;
        }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public Rock.StatementGenerator.StatementGeneratorOptions Options { get; set; }

        /// <summary>
        /// The _rock rest client
        /// </summary>
        private RockRestClient _rockRestClient = null;

        /// <summary>
        /// Gets or sets the record count.
        /// </summary>
        /// <value>
        /// The record count.
        /// </value>
        private int RecordCount { get; set; }

        /// <summary>
        /// Gets or sets the index of the record.
        /// </summary>
        /// <value>
        /// The index of the record.
        /// </value>
        private int RecordIndex { get; set; }

        public void RunReport()
        {
            UpdateProgress( "Connecting..." );

            // Login and setup options for REST calls
            RockConfig rockConfig = RockConfig.Load();

            _rockRestClient = new RockRestClient( rockConfig.RockBaseUrl );
            _rockRestClient.Login( rockConfig.Username, rockConfig.Password );

            var recipientList = _rockRestClient.PostDataWithResult<Rock.StatementGenerator.StatementGeneratorOptions, List<Rock.StatementGenerator.StatementGeneratorRecipient>>( "api/FinancialTransactions/GetStatementGeneratorRecipients", this.Options );

            recipientList = recipientList.Take( 100 ).ToList();

            var recipentResults = new List<Rock.StatementGenerator.StatementGeneratorRecipientResult>();
            this.RecordCount = recipientList.Count;
            this.RecordIndex = 0;

            var tasks = new List<Task>();
            ConcurrentDictionary<int, Stream> pdfResults = new ConcurrentDictionary<int, Stream>();

            foreach ( var recipent in recipientList )
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                var url = $"api/FinancialTransactions/GetStatementGeneratorRecipientResult?GroupId={recipent.GroupId}";
                if ( recipent.PersonId.HasValue )
                {
                    url += $"&PersonId={recipent.PersonId.Value}";
                }
                var recipentResult = _rockRestClient.PostDataWithResult<Rock.StatementGenerator.StatementGeneratorOptions, Rock.StatementGenerator.StatementGeneratorRecipientResult>( url, this.Options );
                double fetchDataMS = stopwatch.Elapsed.TotalMilliseconds;
                stopwatch.Restart();

                var task = Task.Run( () =>
                {
                    int documentNumber = this.RecordIndex;
                    var html = recipentResult.Html;
                    var taskStopWatch = Stopwatch.StartNew();
                    var pdfBytes = Pdf.From( html ).WithoutOutline().Portrait().Content();
                    taskStopWatch.Stop();
                    double htmlToPdfMS = taskStopWatch.Elapsed.TotalMilliseconds;
                    var pdfStream = new MemoryStream( pdfBytes );
                    pdfResults.TryAdd( documentNumber, pdfStream );
                    Debug.WriteLine( $"fetchDataMS:{fetchDataMS} ms, htmlToPdfMS:{htmlToPdfMS} ms" );
                } );

                tasks.Add( task );
                //task.Wait();

                tasks = tasks.Where( a => a.Status != TaskStatus.RanToCompletion ).ToList();
                Debug.WriteLine( $"taskCount:{tasks.Count}" );
                
                recipentResults.Add( recipentResult );
                this.RecordIndex++;
                UpdateProgress( "Processing..." );
            }

            Task.WaitAll( tasks.ToArray() );

            UpdateProgress( "Creating PDF..." );
            this.RecordIndex = 0;

            var pdfResultList = pdfResults.ToList().OrderBy( a => a.Key ).ToList();
            using ( PdfDocument outPdf = new PdfDocument() )
            {
                foreach ( var pdfResult in pdfResultList )
                {
                    UpdateProgress( "Creating PDF..." );
                    this.RecordIndex++;
                    pdfResult.Value.Seek( 0, SeekOrigin.Begin );
                    PdfDocument pdfDocument = PdfReader.Open( pdfResult.Value, PdfDocumentOpenMode.Import );

                    foreach ( var pdfPage in pdfDocument.Pages.OfType<PdfPage>() )
                    {
                        outPdf.Pages.Add( pdfPage );
                    }

                    pdfResult.Value.Dispose();
                }

                outPdf.Save( $@"c:\\temp\\combined.pdf" );
            }

        }

        /// <summary>
        /// Updates the progress.
        /// </summary>
        /// <param name="progressMessage">The message.</param>
        private void UpdateProgress( string progressMessage )
        {
            OnProgress?.Invoke( this, new ProgressEventArgs { ProgressMessage = progressMessage, Position = RecordIndex, Max = RecordCount } );
        }

        /// <summary>
        /// Occurs when [configuration progress].
        /// </summary>
        public event EventHandler<ProgressEventArgs> OnProgress;


    }

    /// <summary>
    /// 
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>
        /// The position.
        /// </value>
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the maximum.
        /// </summary>
        /// <value>
        /// The maximum.
        /// </value>
        public int Max { get; set; }

        /// <summary>
        /// Gets or sets the progress message.
        /// </summary>
        /// <value>
        /// The progress message.
        /// </value>
        public string ProgressMessage { get; set; }
    }
}
