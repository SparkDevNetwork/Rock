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

namespace RockWeb.Blocks.CVSImport
{
    [DisplayName( "CSV Import" )]
    [Category( "CSV Import" )]
    [Description( "Block to import data into Rock using the CSV files." )]
    [Rock.SystemGuid.BlockTypeGuid( "EDA8F90D-1201-4AFF-9E6D-A8F6D6F618D9" )]
    public partial class CSVImport : Rock.Web.UI.RockBlock
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
        /// Is there a way to declare this as a global constant? They are in the Rock.Slingshot.PersonCSVMapper
        private static readonly string[] requiredFields = { "Id", "Family Id", "Family Role", "First Name", "Last Name" };

        /// <summary>
        /// It is optional to map these properties to a column in the csv.
        /// </summary>
        /// Is there a way to declare this as a global constant? They are in the Rock.Slingshot.PersonCSVMapper
        private static readonly string[] optionalFields = { "Nick Name",
            "Middle Name",
            "Suffix",
            "TitleValueId",
            "Home Phone",
            "Mobile Phone",
            "Is SMS Enabled",
            "Email",
            "Email Preference",
            "Gender",
            "Marital Status",
            "Birthdate",
            "Anniversary Date",
            "Record Status",
            "Inactive Reason",
            "Is Deceased",
            "Connection Status",
            "Grade",
            "Home Address Street 1",
            "Home Address Street 2",
            "Home Address City",
            "Home Address State",
            "Home Address Postal Code",
            "Home Address Country",
            "Created Date Time",
            "Modified Date Time",
            "Note",
            "Campus Id",
            "Campus Name",
            "Give Individually" };

        private static readonly string[] allowedPeronsAttributeFieldTypes = { "Text", "Boolean", "Integer", "Date" };

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
                return string.Format( "CSVImport_BlockId:{0}_SessionId:{1}", this.BlockId, Session.SessionID );
            }
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );
            Array.Sort( requiredFields );
            Array.Sort( optionalFields );
        }

        protected override void OnLoad( EventArgs e )
        {
            propertiesMapping = ( Dictionary<string, string> ) ViewState["PropertiesMapping"] ?? new Dictionary<string, string>();
            base.OnLoad( e );
            if ( !Page.IsPostBack )
            {
                ListItem peopleDataTypeItem = new ListItem( "People" );
                ddlDataType.Items.Add( peopleDataTypeItem );

                RockContext rockContext = new RockContext();
                ListItem[] sourceDescriptionItems = new PersonService( new RockContext() )
                    .GetForeignKeys()
                    .Select( foreignKey => new ListItem( foreignKey ) )
                    .ToArray();
                if ( sourceDescriptionItems.Count() == 0 )
                {
                    // switch to taking text input if no foreign key is present
                    rblpreviousSourceDescription.Required = true;
                    rblpreviousSourceDescription.Visible = false;
                    lbToggleSourceDescription.Visible = false;
                    tbpreviousSourceDescription.Visible = true;
                    tbpreviousSourceDescription.Required = true;
                }
                else
                {
                    rblpreviousSourceDescription.Items.AddRange( sourceDescriptionItems );
                }

                Guid suffixGUID = Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid();
                lsuffixlist.Text = $"({string.Join( ", ", DefinedTypeCache.GetValues( suffixGUID ) )})";

                Guid connectionStatusGUID = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid();
                lconnectionStatusList.Text = $"({string.Join( ", ", DefinedTypeCache.GetValues( connectionStatusGUID ) )})";

                Guid gradeGUID = Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid();
                lgrade.Text = $"({string.Join( ", ", DefinedValueCache.GetDescriptions( gradeGUID ) )})";

                var emailPreferenceNames = Enum.GetNames( typeof( Slingshot.Core.Model.EmailPreference ) );
                lemailPreferenceList.Text = $"({string.Join( ", ", emailPreferenceNames )})";

                var genderNames = Enum.GetNames( typeof( Slingshot.Core.Model.Gender ) );
                lgenderList.Text = $"({string.Join( ", ", genderNames )})";

                var maritalStatusNames = Enum.GetNames( typeof( Slingshot.Core.Model.MaritalStatus ) );
                lmaritalStatusList.Text = $"({string.Join( ", ", maritalStatusNames )})";

                var recordStatusNames = Enum.GetNames( typeof( Slingshot.Core.Model.RecordStatus ) );
                lrecordStatusList.Text = $"({string.Join( ", ", recordStatusNames )})";
            }
        }

        protected void fupCSVFile_FileUploaded( object sender, EventArgs e )
        {
            hfCSVFileName.Value = fupCSVFile.UploadedContentFilePath;
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
            string csvFileName = this.Request.MapPath( hfCSVFileName.Value );

            this.propertiesDropDownList = CreateListItemsDropDown();

            // get the headers -- this needs to be moved to CSVReader class
            using ( StreamReader csvFileStream = File.OpenText( csvFileName ) )
            {
                CsvReader csvReader = new CsvReader( csvFileStream );
                csvReader.Configuration.HasHeaderRecord = true;
                csvReader.Read();
                string[] fieldHeaders = csvReader.FieldHeaders;
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
                ViewState["RecordCount"] = recordsCount.ToString();
                tdRecordCount.Description = recordsCount.ToString();
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

            var csvSlingshotImporter = new CSVSlingshotImporter( personCSVFileName, sourceDescription, defaultDataType, bulkImportType, CSVSlingshotImporter_OnProgress );
            ViewState["CSVImporterErrorsFilePath"] = csvSlingshotImporter.ErrorCSVfilename;

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
            ViewState["PropertiesMapping"] = propertiesMapping;
        }

        protected void lbToggleSourceDescription_Click( object sender, EventArgs e )
        {
            rblpreviousSourceDescription.Required = true;
            rblpreviousSourceDescription.Visible = false;
            lbToggleSourceDescription.Visible = false;
            tbpreviousSourceDescription.Visible = true;
            tbpreviousSourceDescription.Required = true;
        }

        private ListItem[] CreateListItemsDropDown()
        {
            ListItem[] rockAttributeArray = AttributeService.GetPersonAttributes( allowedPeronsAttributeFieldTypes )
                .Select( attribute => new ListItem( attribute.Name ) )
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
            var csvSlingshotImporter = sender as CSVSlingshotImporter;

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
            _hubContext.Clients.All.receiveCSVLineReadNotification( this.SignalRNotificationKey, readLineCount, ViewState["RecordCount"] );
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

            string filePath = ViewState["CSVImporterErrorsFilePath"].ToString();
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
