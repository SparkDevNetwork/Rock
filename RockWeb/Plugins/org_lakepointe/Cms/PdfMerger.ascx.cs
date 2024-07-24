using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;



namespace RockWeb.Plugins.org_lakepointe.CMS
{
    [DisplayName( "PDF Merger" )]
    [Category( "LPC > CMS" )]
    [Description( "Merges all of the PDFs from an event into a single printable file." )]

    [IntegerField(
        "Delete PDF Files Older Than",
        "Delete PDF files in the ouput directory that are older than x days whenever a new PDF is generated. 0 means do not delete old PDF files.",
        true,
        7,
        "",
        0,
        AttributeKey.DeletePdfsOlderThan )]

    [LavaField(
        "First SQL Query",
        "The first SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        true,
        "",
        "SQL Queries",
        1,
        AttributeKey.QueryOne )]
    [LavaField(
        "Second SQL Query",
        "The second SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        2,
        AttributeKey.QueryTwo )]
    [LavaField(
        "Third SQL Query",
        "The third SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        3,
        AttributeKey.QueryThree )]
    [LavaField(
        "Fourth SQL Query",
        "The fourth SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        4,
        AttributeKey.QueryFour )]
    [LavaField(
        "Fifth SQL Query",
        "The fifth SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        5,
        AttributeKey.QueryFive )]
    [LavaField(
        "Sixth SQL Query",
        "The sixth SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        6,
        AttributeKey.QuerySix )]
    [LavaField(
        "Seventh SQL Query",
        "The seventh SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        7,
        AttributeKey.QuerySeven )]
    [LavaField(
        "Eighth SQL Query",
        "The eighth SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        8,
        AttributeKey.QueryEight )]
    [LavaField(
        "Ninth SQL Query",
        "The ninth SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        9,
        AttributeKey.QueryNine )]
    [LavaField(
        "Tenth SQL Query",
        "The tenth SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        10,
        AttributeKey.QueryTen )]

    public partial class PdfMerger : RockBlock
    {
        private RockContext _context;

        #region Properties

        protected class AttributeKey
        {
            /// <summary>
            /// The first of the SQL queries used to retrieve binary files. Should be a SQL query that returns a single column of BinaryFileIds.
            /// </summary>
            public const string DeletePdfsOlderThan = "DeletePdfsOlderThan";

            /// <summary>
            /// The first of the SQL queries used to retrieve binary files. Should be a SQL query that returns a single column of BinaryFileIds.
            /// </summary>
            public const string QueryOne = "QueryOne";

            /// <summary>
            /// The second of the SQL queries used to retrieve binary files. Should be a SQL query that returns a single column of BinaryFileIds.
            /// </summary>
            public const string QueryTwo = "QueryTwo";

            /// <summary>
            /// The third of the SQL queries used to retrieve binary files. Should be a SQL query that returns a single column of BinaryFileIds.
            /// </summary>
            public const string QueryThree = "QueryThree";

            /// <summary>
            /// The fourth of the SQL queries used to retrieve binary files. Should be a SQL query that returns a single column of BinaryFileIds.
            /// </summary>
            public const string QueryFour = "QueryFour";

            /// <summary>
            /// The fifth of the SQL queries used to retrieve binary files. Should be a SQL query that returns a single column of BinaryFileIds.
            /// </summary>
            public const string QueryFive = "QueryFive";

            /// <summary>
            /// The sixth of the SQL queries used to retrieve binary files. Should be a SQL query that returns a single column of BinaryFileIds.
            /// </summary>
            public const string QuerySix = "QuerySix";

            /// <summary>
            /// The seventh of the SQL queries used to retrieve binary files. Should be a SQL query that returns a single column of BinaryFileIds.
            /// </summary>
            public const string QuerySeven = "QuerySeven";

            /// <summary>
            /// The eighth of the SQL queries used to retrieve binary files. Should be a SQL query that returns a single column of BinaryFileIds.
            /// </summary>
            public const string QueryEight = "QueryEight";

            /// <summary>
            /// The ninth of the SQL queries used to retrieve binary files. Should be a SQL query that returns a single column of BinaryFileIds.
            /// </summary>
            public const string QueryNine = "QueryNine";

            /// <summary>
            /// The tenth of the SQL queries used to retrieve binary files. Should be a SQL query that returns a single column of BinaryFileIds.
            /// </summary>
            public const string QueryTen = "QueryTen";
        }

        private Dictionary<string, string> availableQueries;

        #endregion
        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _context = new RockContext();
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            availableQueries = new Dictionary<string, string>();

            TryAddAvailableQuery( GetAttributeValue( AttributeKey.QueryOne ) );
            TryAddAvailableQuery( GetAttributeValue( AttributeKey.QueryTwo ) );
            TryAddAvailableQuery( GetAttributeValue( AttributeKey.QueryThree ) );
            TryAddAvailableQuery( GetAttributeValue( AttributeKey.QueryFour ) );
            TryAddAvailableQuery( GetAttributeValue( AttributeKey.QueryFive ) );
            TryAddAvailableQuery( GetAttributeValue( AttributeKey.QuerySix ) );
            TryAddAvailableQuery( GetAttributeValue( AttributeKey.QuerySeven ) );
            TryAddAvailableQuery( GetAttributeValue( AttributeKey.QueryEight ) );
            TryAddAvailableQuery( GetAttributeValue( AttributeKey.QueryNine ) );
            TryAddAvailableQuery( GetAttributeValue( AttributeKey.QueryTen ) );

            if ( availableQueries.Any() == false )
            {
                lError.Text = "<p>This block does not have any queries configured.</p>";
                pnlError.Visible = true;
            }

            if ( !Page.IsPostBack )
            {
                ddlQuery.DataSource = availableQueries;
                ddlQuery.DataTextField = "Key";
                ddlQuery.DataValueField = "Key";
                ddlQuery.DataBind();
            }
        }

        #endregion  
        #region Events

        public void btnDownload_Click( object sender, EventArgs e )
        {
            GetAndDownloadPdf();
        }

        #endregion
        #region Methods

        private void TryAddAvailableQuery( string contents )
        {
            if ( contents.IsNullOrWhiteSpace() )
            {
                return;
            }

            // Use the first line as the query's name (removing any comment tags)
            string itemName;
            using ( var reader = new StringReader( contents ) )
            {
                itemName = reader.ReadLine();
            }
            if ( itemName.IsNotNullOrWhiteSpace() )
            {
                itemName = itemName.Replace( "--", "" );
                itemName = itemName.Replace( "/*", "" );
                itemName = itemName.Replace( "*/", "" );
                itemName = itemName.Trim();

                if ( itemName.IsNotNullOrWhiteSpace() )
                {
                    availableQueries.Add( itemName, contents );
                }
            }
        }

        private PdfDocument GetPdfFromId( int id )
        {
            var file = new BinaryFileService( new RockContext() ).Get( id );
            if ( file != null && file.Id > 0 && file.MimeType == "application/pdf" )
            {
                Stream stream = file.ContentStream;

                if ( stream != null )
                {
                    return PdfReader.Open( stream, PdfDocumentOpenMode.Import );
                }
            }
            return null;
        }

        private void GetAndDownloadPdf()
        {
            string selectedValue = ddlQuery.SelectedValue;
            bool success = availableQueries.TryGetValue( selectedValue, out string query );

            if ( success )
            {
                List<PdfDocument> documents = new List<PdfDocument>();

                // Run the selected SQL query to get a list of Binary File Ids
                var results = _context.Database.SqlQuery<int>( query );
                foreach ( var item in results )
                {
                    var pdf = GetPdfFromId( item );
                    if ( pdf != null )
                    {
                        documents.Add( pdf );
                    }
                }

                // Append each PDF found to the output PDF file
                PdfDocument output = new PdfDocument();
                foreach ( PdfDocument document in documents )
                {
                    if ( document != null )
                    {
                        foreach ( var page in document.Pages )
                        {
                            output.AddPage( page );
                        }

                        // If Double Sided Mode is on, add a blank page after any document that has an odd number of pages
                        if ( tglDoubleSidedMode.Checked == true && document.PageCount % 2 != 0 )
                        {
                            output.AddPage();
                        }
                    }
                }

                if ( output.PageCount > 0 )
                {
                    string outputPath = HttpContext.Current.Server.MapPath( "~/Plugins/org_lakepointe/Assets/Generated" );
                    string fileName = selectedValue.MakeValidFileName();

                    // Make sure the output directory exists
                    Directory.CreateDirectory( outputPath );

                    // Clean up old PDFs to prevent this folder from getting too large
                    DeleteOldPdfs( outputPath );

                    // Save the new PDF to disk and redirect the user to it
                    output.Save( Path.Combine( outputPath, $"{fileName}.pdf" ) );
                    Response.Redirect( $"/Plugins/org_lakepointe/Assets/Generated/{fileName}.pdf" );
                }
                else if ( results.Any() == false )
                {
                    lError.Text = "<p>The selected query returned no files.</p>";
                    pnlError.Visible = true;
                }
                else if ( documents.Any() == false )
                {
                    lError.Text = "<p>The selected query returned no valid PDF files.</p>";
                    pnlError.Visible = true;
                }
                else
                {
                    lError.Text = "<p>An unexpected error occurred: The ouput file has no pages.</p>";
                    pnlError.Visible = true;
                }
            }
            else
            {
                lError.Text = "<p>The selected query is invalid.</p>";
                pnlError.Visible = true;
            }
        }

        private void DeleteOldPdfs( string path )
        {
            int days = GetAttributeValue( AttributeKey.DeletePdfsOlderThan ).AsInteger();

            if ( days > 0 )
            {
                DateTime cutoffDate = DateTime.Now.AddDays( ( days * -1 ) );

                var pdfFilesToDelete = Directory.GetFiles( path, "*.pdf" )
                    .Where( filePath => File.GetCreationTime( filePath ) < cutoffDate )
                    .ToList();

                foreach ( string filePath in pdfFilesToDelete )
                {
                    File.Delete( filePath );
                }
            }
        }

        #endregion
    }
}
