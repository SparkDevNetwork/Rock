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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using CsvHelper;
using Microsoft.AspNet.SignalR;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Slingshot;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.BulkImport
{
    [DisplayName( "CSV Import" )]
    [Category( "CSV Import" )]
    [Description( "Block to import data into Rock using the CSV files." )]
    [Rock.SystemGuid.BlockTypeGuid( "362C679C-9A7F-4A2B-9BB0-8683824BE892" )]
    public partial class CsvImport : Rock.Web.UI.RockBlock
    {
        private const string ROCK_ATTRIBUTES_OPTION_NAME = "Attributes";
        private const string FIELD_OPTION_NAME = "Field";

        /// <summary>
        /// The list items the fields in the CSV can be mapped to.
        /// </summary>
        private ListItem[] propertiesDropDownList;

        private Dictionary<string, string> propertiesMapping;

        /// <summary>
        /// The properties that should be mapped to by fields in the csv. Not having one of these fields mapped to a csv column will result in an error
        /// </summary>
        private static readonly string[] requiredFields = { CSVHeaders.FamilyId, CSVHeaders.FamilyRole, CSVHeaders.FirstName, CSVHeaders.Id, CSVHeaders.LastName };

        /// <summary>
        /// It is optional to map these properties to a column in the csv.
        /// </summary>
        private static readonly string[] optionalFields = { CSVHeaders.AnniversaryDate,
           CSVHeaders.Birthdate,
           CSVHeaders.CampusId,
           CSVHeaders.CampusName,
           CSVHeaders.ConnectionStatus,
           CSVHeaders.CreatedDateTime,
           CSVHeaders.Email,
           CSVHeaders.EmailPreference,
           CSVHeaders.Gender,
           CSVHeaders.GiveIndividually,
           CSVHeaders.Grade,
           CSVHeaders.HomeAddressCity,
           CSVHeaders.HomeAddressCountry,
           CSVHeaders.HomeAddressPostalCode,
           CSVHeaders.HomeAddressState,
           CSVHeaders.HomeAddressStreet1,
           CSVHeaders.HomeAddressStreet2,
           CSVHeaders.HomePhone,
           CSVHeaders.InactiveReason,
           CSVHeaders.IsDeceased,
           CSVHeaders.IsSMSEnabled,
           CSVHeaders.MaritalStatus,
           CSVHeaders.MiddleName,
           CSVHeaders.MobilePhone,
           CSVHeaders.ModifiedDateTime,
           CSVHeaders.NickName,
           CSVHeaders.Note,
           CSVHeaders.RecordStatus,
           CSVHeaders.TitleValueId,
           CSVHeaders.Suffix };

        private static readonly HashSet<string> allowedPeronsAttributeFieldTypeClassNames = new HashSet<string> { "Rock.Field.Types.TextFieldType",
            "Rock.Field.Types.BooleanFieldType",
            "Rock.Field.Types.IntegerFieldType",
            "Rock.Field.Types.DateFieldType"
        };

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private readonly IHubContext _hubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

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
                return $"CSVImport_BlockId:{ this.BlockId }_SessionId:{ Session.SessionID }";
            }
        }

        #region ViewState Keys
        private static class ViewStateKey
        {

            public const string PropertiesMapping = "PropertiesMapping";
            public const string RecordCount = "RecordCount";
            public const string CSVImporterErrorsFilePath = "CSVImporterErrorsFilePath";
        }
        #endregion

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( !Page.IsPostBack )
            {
                // delete all the csv files in the root directory on start up to ensure that no residual files are present before the upload
                string directoryPath = Request.MapPath( fupCSVFile.RootFolder );
                try
                {
                    if ( Directory.Exists( directoryPath ) )
                    {
                        Directory.EnumerateFiles( directoryPath, "*.csv" ).ToList()
                            .ForEach( f => File.Delete( f ) );
                    }
                }
                catch ( Exception )
                {
                    nbPageError.Text = "Error Loading Page. Try Closing all files and reload the page.";
                    nbPageError.Visible = true;
                    pnlLandingPage.Visible = false;
                }
            }
            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );
        }

        protected override void OnLoad( EventArgs e )
        {
            propertiesMapping = ( Dictionary<string, string> ) ViewState[ViewStateKey.PropertiesMapping] ?? new Dictionary<string, string>();
            
            if ( !Page.IsPostBack )
            {
                ListItem peopleDataTypeItem = new ListItem( "People" );
                ddlDataType.Items.Add( peopleDataTypeItem );

                RockContext rockContext = new RockContext();

                ListItem[] sourceDescriptionItems = new PersonService( new RockContext() )
                    .GetForeignKeys()
                    .Select( foreignKey => new ListItem( foreignKey ) )
                    .ToArray();
                rblpreviousSourceDescription.Items.AddRange( sourceDescriptionItems );

                bool noPreviousForeignKeyPresent = sourceDescriptionItems.Count() == 0;
                if ( noPreviousForeignKeyPresent )
                {
                    rblpreviousSourceDescription.Required = false;
                    rblpreviousSourceDescription.Visible = false;
                    lbAddSourceDescription.Visible = false;
                    tbpreviousSourceDescription.Visible = true;
                    tbpreviousSourceDescription.Required = true;
                }

                Guid suffixGUID = Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid();
                lsuffixlist.Text = $"({DefinedTypeCache.Get( suffixGUID ).DefinedValues.Take( 50 ).Select( dv => dv.Value ).ToList().AsDelimited( ", " )})";

                Guid connectionStatusGUID = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid();
                lconnectionStatusList.Text = $"({DefinedTypeCache.Get( connectionStatusGUID ).DefinedValues.Take( 50 ).Select( dv => dv.Value ).ToList().AsDelimited( ", " )})";

                Guid gradeGUID = Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid();
                var gradeAbbreviations = DefinedTypeCache.Get( gradeGUID ).DefinedValues.Take( 50 ).Select( a => a.AttributeValues["Abbreviation"]?.Value )
                    .Where( a => !string.IsNullOrWhiteSpace( a ) ).ToList();
                lgrade.Text = $"({ gradeAbbreviations.AsDelimited( ", " ) })";

                var emailPreferenceNames = Enum.GetNames( typeof( Slingshot.Core.Model.EmailPreference ) ).Take( 50 ).ToList();
                lemailPreferenceList.Text = $"({emailPreferenceNames.AsDelimited( ", " )})";

                var genderNames = Enum.GetNames( typeof( Slingshot.Core.Model.Gender ) ).Take( 50 ).ToList();
                lgenderList.Text = $"({genderNames.AsDelimited( ", " )})";

                var maritalStatusNames = Enum.GetNames( typeof( Slingshot.Core.Model.MaritalStatus ) ).Take( 50 ).ToList();
                lmaritalStatusList.Text = $"({maritalStatusNames.AsDelimited( ", " )})";

                var recordStatusNames = Enum.GetNames( typeof( Slingshot.Core.Model.RecordStatus ) ).Take( 50 ).ToList();
                lrecordStatusList.Text = $"({recordStatusNames.AsDelimited( ", " )})";
            }

            base.OnLoad( e );
        }

        protected void fupCSVFile_FileUploaded( object sender, EventArgs e )
        {
            hfCSVFileName.Value = fupCSVFile.UploadedContentFilePath;
            nbErrorsInFile.Visible = false; // hide any error messages from the previous file upload if any.
        }

        protected void fupCSVFile_FileRemoved( object sender, EventArgs e )
        {
            string filePath = Request.MapPath( hfCSVFileName.Value );
            File.Delete( filePath );
            hfCSVFileName.Value = ""; // nullify the file name to be processed.
        }

        protected void rptCSVHeaders_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var ddlCSVHeader = e.Item.FindControl( "ddlCSVHeader" ) as RockDropDownList;
            ddlCSVHeader.Items.AddRange( propertiesDropDownList );
        }

        protected void btnStart_Click( object sender, EventArgs e )
        {
            if ( hfCSVFileName.Value.IsNullOrWhiteSpace() )
            {
                nbErrorsInFile.Text = $"A CSV file must be selected first.";
                nbErrorsInFile.Visible = true;
                return;
            }

            string csvFileName = this.Request.MapPath( hfCSVFileName.Value );

            this.propertiesDropDownList = CreateListItemsDropDown();

            try
            {
                // get the headers -- this needs to be moved to CSVReader class
                using ( StreamReader csvFileStream = File.OpenText( csvFileName ) )
                {
                    CsvReader csvReader = new CsvReader( csvFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;
                    csvReader.Read();
                    string[] fieldHeaders = csvReader.FieldHeaders;
                    string duplicateHeadersList = fieldHeaders.GroupBy( fh => fh )
                        .Where( g => g.Count() > 1 )
                        .Select( y => y.Key )
                        .ToList()
                        .AsDelimited( ", ", " and " );
                    bool headerContainsDuplicate = !string.IsNullOrEmpty( duplicateHeadersList );
                    if ( headerContainsDuplicate )
                    {
                        nbErrorsInFile.Text = $"The file has duplicated headers: {duplicateHeadersList}. Please fix it and upload again.";
                        nbErrorsInFile.Visible = true;
                        return;
                    }
                    rptCSVHeaders.DataSource = fieldHeaders;
                    rptCSVHeaders.DataBind();
                }

                // get the number of records in the csv file -- this needs to be moved to CSVReader class
                using ( StreamReader csvFileStream = File.OpenText( csvFileName ) )
                {
                    int recordsCount = 0;
                    while ( csvFileStream.ReadLine() != null )
                    {
                        ++recordsCount;
                    }
                    if ( recordsCount > 0 )
                    {
                        recordsCount--;
                    }
                    ViewState[ViewStateKey.RecordCount] = recordsCount.ToString();
                    tdRecordCount.Description = recordsCount.ToString();
                }
            }
            catch ( Exception )
            {
                nbPageError.Text = "Error Loading Page. Try Closing all files and reload the page.";
                nbPageError.Visible = true;
                pnlLandingPage.Visible = false;
                pnlFieldMappingPage.Visible = false;
                return;
            }

            pnlFieldMappingPage.Visible = true;
            pnlLandingPage.Visible = false;
        }

        protected void btnImport_Click( object sender, EventArgs e )
        {
            const string defaultDataType = "People";
            bool containsAllRequiredFields = this.propertiesMapping
                .Keys
                .ToHashSet()
                .IsSupersetOf( requiredFields );
            if ( !containsAllRequiredFields )
            {
                var missingRequiredFields = requiredFields.Except( this.propertiesMapping.Keys );
                nbRequiredFieldsNotPresentWarning.Text = "Not all required fields have been mapped. Please provide mappings for: \n" + string.Join( "\n", missingRequiredFields );
                nbRequiredFieldsNotPresentWarning.Visible = true;
                return;
            }

            var bulkImportType = cbAllowUpdatingExisting.Checked ? BulkImporter.ImportUpdateType.AlwaysUpdate : BulkImporter.ImportUpdateType.AddOnly;

            var personCSVFileName = this.Request.MapPath( fupCSVFile.UploadedContentFilePath );
            pnlProgress.Visible = true;
            pnlFieldMappingPage.Visible = false;
            pnlLandingPage.Visible = false;

            string sourceDescription = tbpreviousSourceDescription.Text.IsNullOrWhiteSpace()
                ? rblpreviousSourceDescription.SelectedValue
                : tbpreviousSourceDescription.Text;

            var csvSlingshotImporter = new CsvSlingshotImporter( personCSVFileName, sourceDescription, defaultDataType, bulkImportType, CSVSlingshotImporter_OnProgress );
            ViewState[ViewStateKey.CSVImporterErrorsFilePath] = csvSlingshotImporter.ErrorCSVfilename;

            var task = new Task( () =>
            {
                try
                {
                    csvSlingshotImporter.CreateIntermediateCSVFiles( this.propertiesMapping, UploadedCSVOnLineRead );
                    csvSlingshotImporter.DoImport();
                    csvSlingshotImporter.AddPersonCSVImportErrorNotes();
                }
                catch ( Exception exception )
                {
                    _hubContext.Clients.All.receiveUploadedCSVInvalidException( this.SignalRNotificationKey, exception.Message );
                }
                finally
                {
                    csvSlingshotImporter.ClearRedundantFilesAfterImport();
                }
            } );

            ScriptManager.GetCurrent( Page )
                .RegisterPostBackControl( btnDownloadErrorCSV );

            task.Start();
        }

        protected void ddlCSVHeader_SelectedIndexChanged( object sender, EventArgs e )
        {
            RockDropDownList rockDropDownList = ( RockDropDownList ) sender;
            bool hasNewValue = !rockDropDownList.SelectedValue.IsNullOrWhiteSpace();

            if ( propertiesMapping.ContainsKey( rockDropDownList.SelectedValue ) )
            {
                rockDropDownList.ClearSelection();
                return;
            }

            // remove the stale entry from the dictionary.
            propertiesMapping.Remove( rockDropDownList.LastSelectedValue );

            if ( hasNewValue )
            {
                propertiesMapping.Add( rockDropDownList.SelectedValue, rockDropDownList.Label );
            }
            ViewState[ViewStateKey.PropertiesMapping] = propertiesMapping;
        }

        protected void lbAddSourceDescription_Click( object sender, EventArgs e )
        {
            rblpreviousSourceDescription.Required = false;
            rblpreviousSourceDescription.Visible = false;
            lbAddSourceDescription.Visible = false;
            tbpreviousSourceDescription.Visible = true;
            tbpreviousSourceDescription.Required = true;
            rblpreviousSourceDescription.Required = false;
            lbChooseSourceDescription.Visible = true;
        }

        protected void lbChooseSourceDescription_Click( object sender, EventArgs e )
        {
            rblpreviousSourceDescription.Required = true;
            rblpreviousSourceDescription.Visible = true;
            lbAddSourceDescription.Visible = true;
            tbpreviousSourceDescription.Visible = false;
            tbpreviousSourceDescription.Required = false;
            rblpreviousSourceDescription.Required = true;
            lbChooseSourceDescription.Visible = false;
        }

        private ListItem[] CreateListItemsDropDown()
        {
            ListItem[] rockAttributeArray = AttributeCache.GetPersonAttributes( allowedPeronsAttributeFieldTypeClassNames )
                .Select( attribute => new ListItem( attribute.Name, attribute.Key ) ) // attribute key is used by the Slingshot Importer to map the attributes.
                .ToArray();
            foreach ( ListItem rockAttribute in rockAttributeArray )
            {
                rockAttribute.Attributes["OptionGroup"] = ROCK_ATTRIBUTES_OPTION_NAME;
            }

            ListItem[] requiredFieldslistItems = requiredFields.Select( name => new ListItem( name ) )
                .ToArray();
            foreach ( ListItem listItem in requiredFieldslistItems )
            {
                listItem.Attributes["OptionGroup"] = FIELD_OPTION_NAME;
            }

            ListItem[] optionalFieldslistItems = optionalFields.Select( name => new ListItem( name ) )
                .ToArray();
            foreach ( ListItem listItem in optionalFieldslistItems )
            {
                listItem.Attributes["OptionGroup"] = FIELD_OPTION_NAME;
            }

            ListItem[] emptyDefaultEntry = { new ListItem( "" ) };

            return emptyDefaultEntry
                .Concat( requiredFieldslistItems )
                .Concat( optionalFieldslistItems )
                .Concat( rockAttributeArray )
                .ToArray();
        }

        /// <summary>
        /// Handles the ProgressChanged event of the BackgroundWorker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ProgressChangedEventArgs"/> instance containing the event data.</param>
        private void CSVSlingshotImporter_OnProgress( object sender, object e )
        {
            var csvSlingshotImporter = sender as CsvSlingshotImporter;

            bool isPersonImportMessage = e is string && e.ToString().StartsWith( "Bulk Importing Person" );

            if ( !isPersonImportMessage )
            {
                return;
            }

            string progressMessage = e.ToString();
            DescriptionList progressResults = new DescriptionList();


            var exceptionsCopy = csvSlingshotImporter.Exceptions.ToArray();
            if ( exceptionsCopy.Any() )
            {
                if ( exceptionsCopy.Count() > 50 )
                {
                    var exceptionsSummary = exceptionsCopy
                        .GroupBy( a => a.GetBaseException().Message )
                        .Select( a => a.Key + "(" + a.Count().ToString() + ")" );
                    progressResults.Add( "Exceptions", string.Join( Environment.NewLine, exceptionsSummary ) );
                }
                else
                {
                    progressResults.Add( "Exception", string.Join( Environment.NewLine, exceptionsCopy.Select( a => a.Message ).ToArray() ) );
                }
            }

            string personImportKey = "Person Import";
            if ( csvSlingshotImporter.Results.ContainsKey( personImportKey ) )
                progressResults.Add( personImportKey, csvSlingshotImporter.Results[personImportKey] );

            _hubContext.Clients.All.receiveCSVNotification( this.SignalRNotificationKey, progressMessage, progressResults.Html.ConvertCrLfToHtmlBr(), csvSlingshotImporter.HasErrors );
        }

        private void UploadedCSVOnLineRead( object sender, object readLineCount )
        {
            _hubContext.Clients.All.receiveCSVLineReadNotification( this.SignalRNotificationKey, readLineCount, ViewState[ViewStateKey.RecordCount] );
        }

        protected void btnDownloadErrorCSV_Click( object sender, EventArgs e )
        {
            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
            response.ClearHeaders();
            response.ClearContent();
            response.Clear();
            response.ContentType = "text/csv";
            response.Charset = "";
            response.AddHeader( "content-disposition", "attachment; filename=errors.csv" );

            string filePath = ViewState[ViewStateKey.CSVImporterErrorsFilePath].ToString();
            if ( File.Exists( filePath ) )
            {
                response.TransmitFile( filePath );
            }

            response.Flush();
            response.SuppressContent = true;
            System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }
}
