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

        /// <summary>
        /// Runs the report returning the number of statements that were generated
        /// </summary>
        public int RunReport()
        {
            UpdateProgress( "Connecting..." );

            // Login and setup options for REST calls
            RockConfig rockConfig = RockConfig.Load();

            _rockRestClient = new RockRestClient( rockConfig.RockBaseUrl );
            _rockRestClient.Login( rockConfig.Username, rockConfig.Password );

            var lavaTemplateDefineValues = _rockRestClient.GetData<List<Rock.Client.DefinedValue>>( "api/FinancialTransactions/GetStatementGeneratorTemplates" );
            var lavaTemplateDefineValue = lavaTemplateDefineValues?.FirstOrDefault( a => a.Guid == this.Options.LayoutDefinedValueGuid );

            Dictionary<string, string> pdfObjectSettings = null;

            if ( lavaTemplateDefineValue?.Attributes?.ContainsKey( "PDFObjectSettings" ) == true )
            {
                pdfObjectSettings = lavaTemplateDefineValue.AttributeValues["PDFObjectSettings"].Value.AsDictionaryOrNull();
            }

            pdfObjectSettings = pdfObjectSettings ?? new Dictionary<string, string>();

            UpdateProgress( "Getting Recipients..." );
            var recipientList = _rockRestClient.PostDataWithResult<Rock.StatementGenerator.StatementGeneratorOptions, List<Rock.StatementGenerator.StatementGeneratorRecipient>>( "api/FinancialTransactions/GetStatementGeneratorRecipients", this.Options );

            this.RecordCount = recipientList.Count;
            this.RecordIndex = 0;

            var tasks = new List<Task>();

            // initialize the pdfStreams list for all the recipients so that it can be populated safely in the pdf generation threads
            List<Stream> pdfStreams = recipientList.Select( a => ( Stream ) null ).ToList();

            bool cancel = false;

            UpdateProgress( "Getting Statements..." );
            foreach ( var recipent in recipientList )
            {
                StringBuilder sbUrl = new StringBuilder();
                sbUrl.Append( $"api/FinancialTransactions/GetStatementGeneratorRecipientResult?GroupId={recipent.GroupId}" );
                if ( recipent.PersonId.HasValue )
                {
                    sbUrl.Append( $"&PersonId={recipent.PersonId.Value}" );
                }

                if ( recipent.LocationGuid.HasValue )
                {
                    sbUrl.Append( $"&LocationGuid={recipent.LocationGuid.Value}" );
                }

                var recipentResult = _rockRestClient.PostDataWithResult<Rock.StatementGenerator.StatementGeneratorOptions, Rock.StatementGenerator.StatementGeneratorRecipientResult>( sbUrl.ToString(), this.Options );

                int documentNumber = this.RecordIndex;
                if ( ( this.Options.ExcludeOptedOutIndividuals && recipentResult.OptedOut ) || ( string.IsNullOrWhiteSpace( recipentResult.Html ) ) )
                {
                    // don't generate a statement if opted out or no statement html
                    pdfStreams[documentNumber] = null;
                }
                else
                {
                    var html = recipentResult.Html;
                    var footerHtml = recipentResult.FooterHtml;

                    var task = Task.Run( () =>
                    {
                        var pdfGenerator = Pdf.From( html );

                        string footerHtmlPath = Path.ChangeExtension( Path.GetTempFileName(), "html" );
                        string footerUrl = null;

                        if ( !string.IsNullOrEmpty( footerHtml ) )
                        {
                            File.WriteAllText( footerHtmlPath, footerHtml );
                            footerUrl = "file:///" + footerHtmlPath.Replace( '\\', '/' );
                        }
                        
                        foreach ( var pdfObjectSetting in pdfObjectSettings )
                        {
                            if ( pdfObjectSetting.Key.StartsWith( "margin." ) || pdfObjectSetting.Key.StartsWith( "size." ) )
                            {
                                pdfGenerator = pdfGenerator.WithGlobalSetting( pdfObjectSetting.Key, pdfObjectSetting.Value );
                            }
                            else
                            {
                                pdfGenerator = pdfGenerator.WithObjectSetting( pdfObjectSetting.Key, pdfObjectSetting.Value );
                            }
                        }

                        if ( !pdfObjectSettings.ContainsKey( "footer.fontSize" ) )
                        {
                            pdfGenerator = pdfGenerator.WithObjectSetting( "footer.fontSize", "10" );
                        }

                        if ( footerUrl != null )
                        {
                            pdfGenerator = pdfGenerator.WithObjectSetting( "footer.htmlUrl", footerUrl );
                        }
                        else
                        {
                            if ( !pdfObjectSettings.ContainsKey( "footer.right" ) )
                            {
                                pdfGenerator = pdfGenerator.WithObjectSetting( "footer.right", "Page [page] of [topage]" );
                            }
                        }
                        
                        var pdfBytes = pdfGenerator
                            .WithoutOutline()
                            .Portrait()
                            .Content();

                        var pdfStream = new MemoryStream( pdfBytes );
                        System.Diagnostics.Debug.Assert( pdfStreams[documentNumber] == null, "Threading issue: pdfStream shouldn't already be assigned" );
                        pdfStreams[documentNumber] = pdfStream;

                        if ( File.Exists( footerHtmlPath ) )
                        {
                            File.Delete( footerHtmlPath );
                        }

                    } );

                    tasks.Add( task );

                    tasks = tasks.Where( a => a.Status != TaskStatus.RanToCompletion ).ToList();
                }

                this.RecordIndex++;
                UpdateProgress( "Processing..." );
                if ( cancel )
                {
                    break;
                }
            }

            Task.WaitAll( tasks.ToArray() );

            UpdateProgress( "Creating PDF..." );
            this.RecordIndex = 0;

            // remove any statements that didn't get generated due to OptedOut
            pdfStreams = pdfStreams.Where( a => a != null ).ToList();
            this.RecordCount = pdfStreams.Count();

            int maxStatementsPerChapter = RecordCount;

            bool useChapters = this.Options.StatementsPerChapter.HasValue;

            if ( this.Options.StatementsPerChapter.HasValue )
            {
                maxStatementsPerChapter = this.Options.StatementsPerChapter.Value;
            }

            if ( maxStatementsPerChapter < 1 )
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
                if ( pdfStreams.Any() )
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
                            SavePdfFile( resultPdf, filePath );
                            resultPdf.Dispose();
                            resultPdf = new PdfDocument();
                            statementsInChapter = 0;
                            chapterIndex++;
                        }
                    }

                    if ( useChapters )
                    {
                        // just in case we still have statements that haven't been written to a pdf
                        if ( statementsInChapter > 0 )
                        {
                            string filePath = string.Format( @"{0}\{1}-chapter{2}.pdf", this.Options.SaveDirectory, this.Options.BaseFileName, chapterIndex );
                            SavePdfFile( resultPdf, filePath );
                        }
                    }
                    else
                    {
                        string filePath = string.Format( @"{0}\{1}.pdf", this.Options.SaveDirectory, this.Options.BaseFileName );
                        SavePdfFile( resultPdf, filePath );
                    }
                }
            }
            finally
            {
                resultPdf.Dispose();
            }

            UpdateProgress( "Complete" );

            return this.RecordCount;
        }

        /// <summary>
        /// Saves the PDF file and Prompts if the file seems to be open
        /// </summary>
        /// <param name="resultPdf">The result PDF.</param>
        /// <param name="filePath">The file path.</param>
        private static void SavePdfFile( PdfDocument resultPdf, string filePath )
        {
            if ( File.Exists( filePath ) )
            {
                try
                {
                    File.Delete( filePath );
                }
                catch ( Exception )
                {
                    System.Windows.MessageBox.Show( "Unable to write save PDF File. Make sure you don't have the file open then press OK to try again.", "Warning", System.Windows.MessageBoxButton.OK );
                }
            }

            resultPdf.Save( filePath );
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
