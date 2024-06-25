using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Plugins.org_lakepointe.CMS
{
    [DisplayName( "PDF Merger V2" )]
    [Category( "LPC > CMS" )]
    [Description( "Merges all of the PDFs from an event into a single printable file." )]

    // SQL Queries
    [LavaField(
        "First SQL Query",
        "The first SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        true,
        "",
        "SQL Queries",
        0,
        AttributeKey.QueryOne )]
    [LavaField(
        "Second SQL Query",
        "The second SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        1,
        AttributeKey.QueryTwo )]
    [LavaField(
        "Third SQL Query",
        "The third SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        2,
        AttributeKey.QueryThree )]
    [LavaField(
        "Fourth SQL Query",
        "The fourth SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        3,
        AttributeKey.QueryFour )]
    [LavaField(
        "Fifth SQL Query",
        "The fifth SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        4,
        AttributeKey.QueryFive )]
    [LavaField(
        "Sixth SQL Query",
        "The sixth SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        5,
        AttributeKey.QuerySix )]
    [LavaField(
        "Seventh SQL Query",
        "The seventh SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        6,
        AttributeKey.QuerySeven )]
    [LavaField(
        "Eighth SQL Query",
        "The eighth SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        7,
        AttributeKey.QueryEight )]
    [LavaField(
        "Ninth SQL Query",
        "The ninth SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        8,
        AttributeKey.QueryNine )]
    [LavaField(
        "Tenth SQL Query",
        "The tenth SQL query that will be selectable by users. Should be a SQL query that returns a single column of BinaryFileIds.",
        false,
        "",
        "SQL Queries",
        9,
        AttributeKey.QueryTen )]

    // API Info
    [TextField(
        "API Initiate URL",
        "The URL to make an HTTP POST request to in order to start the process of creating the PDF.",
        true,
        "",
        "API Info",
        10,
        AttributeKey.ApiInitiateUrl )]
    [TextField(
        "API Status URL",
        "The URL to make an HTTP GET request to in order to check the process of creating the PDF.",
        true,
        "",
        "API Info",
        11,
        AttributeKey.ApiStatusUrl )]

    // Preferences
    [IntegerField(
        "Delete PDF Files Older Than",
        "Delete PDF files in the ouput directory that are older than {x} days whenever a new PDF is generated. 0 means do not delete old PDF files.",
        true,
        7,
        "Preferences",
        12,
        AttributeKey.DeletePdfsOlderThan )]
    [TextField(
        "Output File Path",
        "The filepath that the output file should be placed at. Should match the value defined in the appsettings of the API. Should be a relative path that either starts from '~' or '~~'.",
        true,
        @"~\Plugins\org_lakepointe\Assets\Generated",
        "Preferences",
        13,
        AttributeKey.OutputFilePath )]

    public partial class PdfMergerV2 : RockBlock
    {
        private RockContext _context;

        #region Properties

        protected class AttributeKey
        {
            // SQL Queries
            public const string QueryOne = "QueryOne";
            public const string QueryTwo = "QueryTwo";
            public const string QueryThree = "QueryThree";
            public const string QueryFour = "QueryFour";
            public const string QueryFive = "QueryFive";
            public const string QuerySix = "QuerySix";
            public const string QuerySeven = "QuerySeven";
            public const string QueryEight = "QueryEight";
            public const string QueryNine = "QueryNine";
            public const string QueryTen = "QueryTen";

            // API Info
            public const string ApiInitiateUrl = "ApiInitiateUrl";
            public const string ApiStatusUrl = "ApiStatusUrl";

            // Preferences
            public const string DeletePdfsOlderThan = "DeletePdfsOlderThan";
            public const string OutputFilePath = "OutputFilePath";
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

            if ( ValidateRequiredAttributes() == false )
            {
                lError.Text = "<p>This block does not have all the required attributes.</p>";
                pnlError.Visible = true;
            }

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
                lError.Text = "<p>This block must have at least one valid query.</p>";
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
            if ( string.IsNullOrWhiteSpace( contents ) )
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

        private bool ValidateRequiredAttributes()
        {
            if ( string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.QueryOne ) ) )
            {
                return false;
            }
            if ( string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.ApiInitiateUrl ) ) )
            {
                return false;
            }
            if ( string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.ApiStatusUrl ) ) )
            {
                return false;
            }
            if ( GetAttributeValue( AttributeKey.DeletePdfsOlderThan ).AsIntegerOrNull() == null )
            {
                return false;
            }
            if ( string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.OutputFilePath ) ) )
            {
                return false;
            }

            return true;
        }

        private void GetAndDownloadPdf()
        {
            string selectedValue = ddlQuery.SelectedValue;
            bool success = availableQueries.TryGetValue( selectedValue, out string query );

            if ( success )
            {
                // Get the API Info values
                string apiInitiateUrl = GetAttributeValue( AttributeKey.ApiInitiateUrl );
                string apiStatusUrl = GetAttributeValue( AttributeKey.ApiStatusUrl );

                // Get the Preference values
                bool doubleSided = tglDoubleSidedMode.Checked;
                string fileName = selectedValue;
                foreach ( char c in Path.GetInvalidFileNameChars() )
                {
                    fileName = fileName.Replace( c, '-' );
                }
                string outputPath = GetAttributeValue( AttributeKey.OutputFilePath );
                foreach ( char c in Path.GetInvalidPathChars() )
                {
                    outputPath = outputPath.Replace( c, '-' );
                }

                // Get the ids
                var results = _context.Database.SqlQuery<int>( query ).ToArray();

                if ( results == null || results.Length <= 0 )
                {
                    lError.Text = "<p>The selected query did not return any PDFs.</p>";
                    pnlError.Visible = true;
                    return;
                }

                // Get path info
                var outputFilePath = MapPath( outputPath );
                var outputWebPath = $"{ResolveRockUrl( outputPath ).TrimEnd( '/' )}/{fileName}{( doubleSided == true ? ".ds" : "" )}.pdf";

                // Make sure the directory exists
                Directory.CreateDirectory( outputFilePath );

                // Build the body value
                ApiRequestInfo apiRequestInfo = new ApiRequestInfo
                {
                    FileName = fileName,
                    DoubleSided = doubleSided,
                    Ids = results
                };

                // Send request to localhost API
                Task.Run( () => { SendApiRequest( apiInitiateUrl, apiRequestInfo ); } );

                // Show status to user
                hfOutputWebPath.Value = outputWebPath;
                hfApiStatusUrl.Value = apiStatusUrl;
                pnlStatus.Visible = true;
                pnlEntryForm.Visible = false;

                // Clean up old PDFs to prevent this folder from getting too large
                DeleteOldPdfs( outputFilePath );
            }
            else
            {
                lError.Text = "<p>The selected query is invalid.</p>";
                pnlError.Visible = true;
            }
        }

        private static async void SendApiRequest( string apiInitiateUrl, ApiRequestInfo requestInfo )
        {
            using ( var client = new HttpClient() )
            {
                try
                {
                    _ = await client.PostAsync(
                        apiInitiateUrl,
                        new StringContent(
                            JsonSerializer.Serialize( requestInfo ),
                            Encoding.UTF8,
                            "application/json" ) );
                }
                // If there's an error sending the request, the frontend will handle showing errors to the user
                catch { }
            }
        }

        private void DeleteOldPdfs( string path )
        {
            int days = GetAttributeValue( AttributeKey.DeletePdfsOlderThan ).AsInteger();

            if ( days > 0 )
            {
                DateTime cutoffDate = DateTime.Now.AddDays( ( days * -1 ) );

                var pdfFilesToDelete = Directory.GetFiles( path, "*.pdf" )
                    .Where( filePath => File.GetLastWriteTime( filePath ) < cutoffDate )
                    .ToList();

                foreach ( string filePath in pdfFilesToDelete )
                {
                    File.Delete( filePath );
                }
            }
        }

        #endregion
        #region Classes

        private class ApiRequestInfo
        {
            public string FileName { get; set; }
            public bool DoubleSided { get; set; }
            public int[] Ids { get; set; }
        }

        #endregion
    }
}
