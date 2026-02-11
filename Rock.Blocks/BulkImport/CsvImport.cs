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
using System.Web.Hosting;

using CsvHelper;
using SlingshotCore = global::Slingshot.Core;

using Rock.Attribute;
using Rock.Model;
using Rock.RealTime.Topics;
using Rock.RealTime;
using Rock.Slingshot;
using Rock.ViewModels.Blocks.BulkImport;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Data;

namespace Rock.Blocks.BulkImport
{
    /// <summary>
    /// Block to import CSV files into Rock
    /// </summary>
    [DisplayName( "CSV Import" )]
    [Category( "CSV Import" )]
    [Description( "Block to import data into Rock using CSV files." )]
    [IconCssClass( "ti ti-file-type-csv" )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [Rock.SystemGuid.BlockTypeGuid( "362C679C-9A7F-4A2B-9BB0-8683824BE892" )]
    [Rock.SystemGuid.EntityTypeGuid( "3E6B0AB8-182B-4C16-9E32-BAC0E02F1A43" )]
    public class CsvImport : RockBlockType
    {
        // This matches the private CsvSlingshotImporter.ERROR_CSV_FILENAME because we
        // don't want to allow someone to pass in any file name to the DownloadErrorCsv() BlockAction.
        private const string ERROR_CSV_FILENAME = "errors/errors.csv";

        /// <summary>
        /// The properties that should be mapped to by fields in the csv. Not having one of these fields mapped to a csv column will result in an error
        /// </summary>
        private static readonly string[] RequiredFields = {
            CSVHeaders.FamilyId,
            CSVHeaders.FamilyRole,
            CSVHeaders.FirstName,
            CSVHeaders.Id,
            CSVHeaders.LastName
        };

        /// <summary>
        /// It is optional to map these properties to a column in the csv.
        /// </summary>
        private static readonly string[] OptionalFields = {
            CSVHeaders.AnniversaryDate,
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
            CSVHeaders.Suffix
        };

        private static readonly HashSet<string> AllowedPersonAttributeFieldTypeClassNames = new HashSet<string> { "Rock.Field.Types.TextFieldType",
            "Rock.Field.Types.BooleanFieldType",
            "Rock.Field.Types.IntegerFieldType",
            "Rock.Field.Types.DateFieldType"
        };

        private const string ROCK_ATTRIBUTES_OPTION_NAME = "Attributes";
        private const string FIELD_OPTION_NAME = "Field";

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = GetBoxOptions();

            // delete all the csv files in the root directory on start up to ensure that no residual files are present before the upload
            string directoryPath = GetSlingshotPhysicalRootFolder();
            try
            {
                Directory.EnumerateFiles( directoryPath, "*.csv" ).ToList().ForEach( f => File.Delete( f ) );
            }
            catch ( Exception )
            {
                // Ignore. Not much can be done about it.
            }

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
        /// Gets the root folder physical path for slingshot files.
        /// </summary>
        /// <returns>The physical path to the slingshot files directory.</returns>
        private string GetSlingshotPhysicalRootFolder()
        {
            return HostingEnvironment.MapPath( GetSlingshotRootFolder() );
        }

        /// <summary>
        /// Gets the box options required for the component to render the view.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private CsvImportBox GetBoxOptions()
        {
            var box = new CsvImportBox();

            box.RootFolder = Rock.Security.Encryption.EncryptString( GetSlingshotRootFolder() );
            box.Sources = new PersonService( this.RockContext )
                    .GetForeignKeys()
                    .Select( foreignKey => new ListItemBag { Text = foreignKey, Value = foreignKey } )
                    .ToList();

            // Get literals used in the display for Suffix options, Grade options, etc.
            Guid suffixGUID = Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid();
            box.SuffixOptions = $"({DefinedTypeCache.Get( suffixGUID ).DefinedValues.Take( 50 ).Select( dv => dv.Value ).ToList().AsDelimited( ", " )})";

            Guid connectionStatusGUID = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid();
            box.ConnectionStatusOptions = $"({DefinedTypeCache.Get( connectionStatusGUID ).DefinedValues.Take( 50 ).Select( dv => dv.Value ).ToList().AsDelimited( ", " )})";

            Guid gradeGUID = Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid();
            var gradeAbbreviations = DefinedTypeCache.Get( gradeGUID ).DefinedValues.Take( 50 ).Select( a => a.AttributeValues["Abbreviation"]?.Value )
                .Where( a => !string.IsNullOrWhiteSpace( a ) ).ToList();
            box.GradeOptions = $"({gradeAbbreviations.AsDelimited( ", " )})";

            var emailPreferenceNames = Enum.GetNames( typeof( SlingshotCore.Model.EmailPreference ) ).Take( 50 ).ToList();
            box.EmailPreferenceOptions = $"({emailPreferenceNames.AsDelimited( ", " )})";

            var genderNames = Enum.GetNames( typeof( SlingshotCore.Model.Gender ) ).Take( 50 ).ToList();
            box.GenderOptions = $"({genderNames.AsDelimited( ", " )})";

            var maritalStatusNames = Enum.GetNames( typeof( SlingshotCore.Model.MaritalStatus ) ).Take( 50 ).ToList();
            box.MaritalStatusOptions = $"({maritalStatusNames.AsDelimited( ", " )})";

            var recordStatusNames = Enum.GetNames( typeof( SlingshotCore.Model.RecordStatus ) ).Take( 50 ).ToList();
            box.RecordStatusOptions = $"({recordStatusNames.AsDelimited( ", " )})";

            return box;
        }

        private string GetCsvFilePath( string fileName )
        {
            return GetSlingshotPhysicalRootFolder().TrimEnd( '/' ).TrimEnd( '\\' ) + "/" + fileName.TrimStart( '/' ).TrimStart( '\\' );
        }

        private void DeleteCsvFile( string fileName )
        {
            var fullPath = GetCsvFilePath( fileName );
            try
            {
                if ( File.Exists( fullPath ) )
                {
                    File.Delete( fullPath );
                }
            }
            catch
            {
                // Just Ignore
            }
        }

        private List<ListItemBag> CreateListItemBagsDropDown()
        {
            var rockAttributeArray = AttributeCache.GetPersonAttributes( AllowedPersonAttributeFieldTypeClassNames )
                .OrderBy( a => a.Name )
                .Select( attribute => new ListItemBag { Text = attribute.Name, Value = attribute.Key, Category = ROCK_ATTRIBUTES_OPTION_NAME } ) // attribute key is used by the Slingshot Importer to map the attributes.
                .ToList();

            ListItemBag[] requiredFieldslistItems = RequiredFields
                .OrderBy( name => name )
                .Select( name => new ListItemBag { Text = name, Value = name, Category = "Required Fields" } )
                .ToArray();

            ListItemBag[] optionalFieldslistItems = OptionalFields
                .OrderBy( name => name )
                .Select( name => new ListItemBag { Text = name, Value = name, Category = FIELD_OPTION_NAME } )
                .ToArray();

            return requiredFieldslistItems
                .Concat( optionalFieldslistItems )
                .Concat( rockAttributeArray )
                .ToList();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes a file from the slingshot root folder. Should be called when the "remove" button is clicked on the file uploader
        /// </summary>
        /// <param name="options">Data about the file</param>
        [BlockAction]
        public BlockActionResult DeleteFile( CsvImportDeleteFileOptionsBag options )
        {
            DeleteCsvFile( options.FileName );
            return ActionOk();
        }

        /// <summary>
        /// Get data about the uploaded CSV file, including the column names and number of rows.
        /// </summary>
        /// <param name="options">Data about the file</param>
        [BlockAction]
        public BlockActionResult GetCsvFields( CsvImportGetCsvFieldsOptionsBag options )
        {
            if ( options.FileName.IsNullOrWhiteSpace() || !options.FileName.EndsWith( ".csv" ) )
            {
                return ActionBadRequest( "Please select a valid CSV file." );
            }

            var csvFileName = GetCsvFilePath( options.FileName );

            if ( !File.Exists( csvFileName ) )
            {
                return ActionBadRequest( "CSV file not found." );
            }

            string[] fieldHeaders;
            int recordsCount = 0;

            try
            {
                // get the headers -- this needs to be moved to CSVReader class
                using ( StreamReader csvFileStream = File.OpenText( csvFileName ) )
                {
                    CsvReader csvReader = new CsvReader( csvFileStream );
                    csvReader.Configuration.HasHeaderRecord = true;
                    csvReader.Read();
                    fieldHeaders = csvReader.FieldHeaders;
                    string duplicateHeadersList = fieldHeaders.GroupBy( fh => fh )
                        .Where( g => g.Count() > 1 )
                        .Select( y => y.Key )
                        .ToList()
                        .AsDelimited( ", ", " and " );
                    bool headerContainsDuplicate = !string.IsNullOrEmpty( duplicateHeadersList );
                    if ( headerContainsDuplicate )
                    {
                        return ActionBadRequest( $"The file has duplicated headers: {duplicateHeadersList}. Please fix it and upload again." );
                    }
                }

                // get the number of records in the csv file -- this needs to be moved to CSVReader class
                using ( StreamReader csvFileStream = File.OpenText( csvFileName ) )
                {
                    while ( csvFileStream.ReadLine() != null )
                    {
                        ++recordsCount;
                    }
                    if ( recordsCount > 0 )
                    {
                        recordsCount--;
                    }
                }
            }
            catch ( Exception )
            {
                DeleteCsvFile( options.FileName );
                return ActionBadRequest( "An error occurred while parsing the CSV file. Refresh the page and try again." );
            }

            return ActionOk( new CsvImportGetCsvFieldsResultsBag
            {
                CsvColumns = fieldHeaders.ToList(),
                RecordCount = recordsCount,
                PersonFields = CreateListItemBagsDropDown()
            } );
        }

        /// <summary>
        /// Validates the column mappings to ensure all required fields are mapped.
        /// </summary>
        /// <param name="options">A map of column names (the key) to the name of the Person field (the value) that the column should be mapped to.</param>
        [BlockAction]
        public BlockActionResult ValidateMappings( CsvImportValidateMappingsOptionsBag options )
        {
            bool containsAllRequiredFields = options.ColumnMappings
                .Keys
                .ToHashSet()
                .IsSupersetOf( RequiredFields );

            if ( !containsAllRequiredFields )
            {
                var missingRequiredFields = RequiredFields.Except( options.ColumnMappings.Keys );
                return ActionBadRequest( "Not all required fields have been mapped. Please provide mappings for: \n" + string.Join( ", ", missingRequiredFields ) );
            }

            return ActionOk();
        }

        /// <summary>
        /// Starts the import process.
        /// </summary>
        /// <param name="request">The import request containing necessary parameters.</param>
        /// <returns>A result indicating success or failure of the import operation.</returns>
        [BlockAction]
        public BlockActionResult StartImport( CsvImportStartImportOptionsBag options )
        {
            const string defaultDataType = "People";
            var columnMappings = options.ColumnMappings;
            var bulkImportType = options.AllowUpdatingExisting ? BulkImporter.ImportUpdateType.AlwaysUpdate : BulkImporter.ImportUpdateType.AddOnly;
            var personCSVFileName = GetCsvFilePath( options.FileName );
            string sourceDescription = options.SourceDescription;

            var taskChannelName = $"BulkImport:{personCSVFileName}";
            var topic = RealTimeHelper.GetTopicContext<ICsvImportActivityProgress>();
            var progressReporter = topic.Clients.Channel( taskChannelName );

            var csvSlingshotImporter = new CsvSlingshotImporter( personCSVFileName, sourceDescription, defaultDataType, bulkImportType, ( sender, e ) =>
            {
                var importer = sender as CsvSlingshotImporter;

                bool isPersonImportMessage = e is string && e.ToString().StartsWith( "Bulk Importing Person" );

                if ( !isPersonImportMessage )
                {
                    return;
                }

                string progressMessage = e.ToString();
                DescriptionList progressResults = new DescriptionList();

                var exceptionsCopy = importer.Exceptions.ToArray();
                var errorMessage = "";
                if ( exceptionsCopy.Any() )
                {
                    List<string> exceptionsSummary;
                    if ( exceptionsCopy.Count() > 50 )
                    {
                        exceptionsSummary = exceptionsCopy
                            .GroupBy( a => a.GetBaseException().Message )
                            .Select( a => a.Key + "(" + a.Count().ToString() + ")" )
                            .ToList();
                    }
                    else
                    {
                        exceptionsSummary = exceptionsCopy.Select( a => a.Message ).ToList();
                    }

                    errorMessage = string.Join( "<br>", exceptionsSummary );
                }

                string personImportKey = "Person Import";
                if ( importer.Results.ContainsKey( personImportKey ) )
                {
                    progressResults.Add( personImportKey, importer.Results[personImportKey] );
                }

                // Motive: As of the writing there was no easy way to get the totalCount and the CompletedCount from
                // the server to display the progress bar. Thus, we decided to parse the message string to get the values
                var messageCharArray = progressMessage.Split( ' ' );
                var percentageComplete = 0;
                if ( messageCharArray.Length >= 6 )
                {
                    var completedCount = ( int ) messageCharArray[3].ToIntSafe();
                    // remove any trailing ... from the last number in the total:
                    var totalCount = ( int ) messageCharArray[5].Replace( "...", "" ).ToIntSafe();

                    if ( totalCount > 0 && completedCount > 0 )
                    {
                        percentageComplete = ( int ) Math.Round( ( ( double ) completedCount / totalCount ) * 100 );
                    }
                }

                progressReporter.UpdateTaskLog( new CsvImportActivityProgressStatusBag
                {
                    TaskName = "import",
                    Message = progressMessage,
                    CompletionPercentage = percentageComplete
                } );

            } );

            var task = new Task( async () =>
            {
                await topic.Channels.AddToChannelAsync( options.SessionId, taskChannelName );

                try
                {
                    csvSlingshotImporter.CreateIntermediateCSVFiles( columnMappings, ( sender, readLineCount ) =>
                    {
                        progressReporter.UpdateTaskProgress( new CsvImportActivityProgressStatusBag
                        {
                            TaskName = "preparation",
                            CompletionPercentage = ( decimal ) ( ( int ) readLineCount ) / options.RecordCount * 100,
                        } );
                    } );

                    if ( csvSlingshotImporter.HasErrors )
                    {
                        await progressReporter.UpdateTaskLog( new CsvImportActivityProgressStatusBag
                        {
                            TaskName = "preparation",
                            Error = "Errors Validating Data"
                        } );
                    }

                    csvSlingshotImporter.DoImport();
                    csvSlingshotImporter.AddPersonCSVImportErrorNotes();

                    if ( csvSlingshotImporter.HasErrors )
                    {
                        _ = progressReporter.TaskCompleted( new CsvImportActivityProgressStatusBag
                        {
                            TaskName = "import",
                            Message = "Completed",
                            CompletionPercentage = 100,
                            Error = "Errors Validating Data"
                        } );
                    }
                    else
                    {
                        _ = progressReporter.TaskCompleted( new CsvImportActivityProgressStatusBag
                        {
                            TaskName = "import",
                            Message = "Completed",
                            CompletionPercentage = 100
                        } );
                    }
                }
                catch ( Exception exception )
                {
                    await progressReporter.TaskErrored( new CsvImportActivityProgressStatusBag
                    {
                        TaskName = "import",
                        Error = $"An error occurred while importing the data: {exception.Message}"
                    } );
                }
                finally
                {
                    csvSlingshotImporter.ClearRedundantFilesAfterImport();
                }
            } );

            task.Start();

            var rootFolder = GetSlingshotPhysicalRootFolder();
            var errorCsvFolderName = csvSlingshotImporter.ErrorCSVfilename.Replace( rootFolder, "" ).Replace( ERROR_CSV_FILENAME , "");

            return ActionOk( new { ErrorCsvFolderName = errorCsvFolderName } );
        }

        /// <summary>
        /// Handles the Click event of the Download Log Button fetching the ERROR_CSV_FILENAME
        /// inside the given partial Slingshot folder name.
        /// </summary>
        [BlockAction]
        public BlockActionResult DownloadErrorCsv()
        {
            string folderName = RequestContext.GetPageParameter( "f" );
            string filePath = GetCsvFilePath( folderName ) + ERROR_CSV_FILENAME;

            if ( !File.Exists( filePath ) )
            {
                return ActionNotFound();
            }

            var ms = new MemoryStream();
            using ( var fileStream = new FileStream( filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            {
                fileStream.CopyTo( ms );
            }
            ms.Seek( 0, SeekOrigin.Begin );
            return new FileBlockActionResult( ms, "text/plain", folderName + "_errors.csv" );
        }

        #endregion
    }
}