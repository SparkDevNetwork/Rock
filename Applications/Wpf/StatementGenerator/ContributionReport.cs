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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

            recipientList = recipientList.Take( 10 ).ToList();

            var recipentResults = new List<Rock.StatementGenerator.StatementGeneratorRecipientResult>();
            this.RecordCount = recipientList.Count;
            this.RecordIndex = 0;

            var tasks = new List<Task>();
            List<Stream> pdfStreams = recipientList.Select( a => ( Stream ) null ).ToList();
            List<double> htmlToPdfTimingsMS = recipientList.Select( a => 0.0 ).ToList();
            List<double> fetchDataTimingsMS = new List<double>();
            

            foreach ( var recipent in recipientList )
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                StringBuilder sbUrl = new StringBuilder();
                sbUrl.Append( $"api/FinancialTransactions/GetStatementGeneratorRecipientResult?GroupId={recipent.GroupId}" );
                if ( recipent.PersonId.HasValue )
                {
                    sbUrl.Append( $"&PersonId={recipent.PersonId.Value}" );
                }

                var recipentResult = _rockRestClient.PostDataWithResult<Rock.StatementGenerator.StatementGeneratorOptions, Rock.StatementGenerator.StatementGeneratorRecipientResult>( sbUrl.ToString(), this.Options );
                double fetchDataMS = stopwatch.Elapsed.TotalMilliseconds;
                fetchDataTimingsMS.Add( fetchDataMS );
                stopwatch.Restart();

                int documentNumber = this.RecordIndex;
                var html = recipentResult.Html;

                var task = Task.Run( () =>
                {
                    var taskStopWatch = Stopwatch.StartNew();
                    var pdfBytes = Pdf.From( html )
                        .WithObjectSetting( "footer.fontSize", "10" )
                        .WithObjectSetting( "footer.right", "Page [page] of [topage]" )
                        .WithoutOutline()
                        .Portrait()
                        .Content();
                    taskStopWatch.Stop();
                    double htmlToPdfMS = taskStopWatch.Elapsed.TotalMilliseconds;
                    var pdfStream = new MemoryStream( pdfBytes );
                    if ( pdfStreams[documentNumber] == null )
                    {
                        pdfStreams[documentNumber] = pdfStream;
                    }
                    else
                    {
                        pdfStreams[documentNumber] = pdfStream;
                    }

                    htmlToPdfTimingsMS[documentNumber] = htmlToPdfMS;
                } );

                tasks.Add( task );

                tasks = tasks.Where( a => a.Status != TaskStatus.RanToCompletion ).ToList();

                recipentResults.Add( recipentResult );
                this.RecordIndex++;
                UpdateProgress( "Processing..." );
            }

            Task.WaitAll( tasks.ToArray() );
            Debug.WriteLine( $"FETCH TIMINGS AVG: {fetchDataTimingsMS.Average()} ms" );
            Debug.WriteLine( $"HTML TIMINGS AVG: {htmlToPdfTimingsMS.Average()} ms" );

            UpdateProgress( "Creating PDF..." );
            this.RecordIndex = 0;

            int maxStatementsPerChapter = RecordCount;

            bool useChapters = this.Options.StatementsPerChapter.HasValue;

            if ( this.Options.StatementsPerChapter.HasValue )
            {
                maxStatementsPerChapter = this.Options.StatementsPerChapter.Value;
            }
            else
            {
                this.Options.StatementsPerChapter = RecordCount;
            }

            if ( maxStatementsPerChapter < 1)
            {
                // just in case they entered 0 or a negative number
                useChapters = false;
                maxStatementsPerChapter = RecordCount;
            }

            int statementsInChapter = 0;
            int chapterIndex = 1;

            PdfDocument resultPdf = new PdfDocument();
            try
            {
                var lastPdfStream = pdfStreams.LastOrDefault();
                foreach ( var pdfStream in pdfStreams )
                {
                    UpdateProgress( "Creating PDF..." );
                    this.RecordIndex++;
                    PdfDocument pdfDocument = PdfReader.Open( pdfStream, PdfDocumentOpenMode.Import );

                    foreach ( var pdfPage in pdfDocument.Pages.OfType<PdfPage>() )
                    {
                        resultPdf.Pages.Add( pdfPage );
                    }

                    statementsInChapter++;
                    if ( useChapters && ( ( statementsInChapter >= maxStatementsPerChapter ) || pdfStream == lastPdfStream ) )
                    {
                        string filePath = string.Format( @"{0}\{1}-chapter{2}.pdf", this.Options.SaveDirectory, this.Options.BaseFileName, chapterIndex );
                        resultPdf.Save( filePath );
                        resultPdf.Dispose();
                        resultPdf = new PdfDocument();
                        statementsInChapter = 0;
                        chapterIndex++;
                    }
                }

                if ( useChapters )
                {
                    // just in case we stil have statements that haven't been written to a pdf
                    if ( statementsInChapter > 0 )
                    {
                        string filePath = string.Format( @"{0}\{1}-chapter{2}.pdf", this.Options.SaveDirectory, this.Options.BaseFileName, chapterIndex );
                        resultPdf.Save( filePath );
                    }
                }
                else
                {
                    string filePath = string.Format( @"{0}\{1}.pdf", this.Options.SaveDirectory, this.Options.BaseFileName );
                    resultPdf.Save( filePath );
                }
            }
            finally
            {
                resultPdf.Dispose();
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
